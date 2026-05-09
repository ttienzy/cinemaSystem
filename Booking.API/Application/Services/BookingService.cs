using Booking.API.Application.DTOs.Requests;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Domain.Entities;
using Booking.API.Infrastructure.Persistence.Repositories;
using BookingSeatResponseDto = Booking.API.Application.DTOs.Responses.BookingSeatDto;
using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

/// <summary>
/// Orchestration service that coordinates booking operations
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISeatStatusService _seatStatusService;
    private readonly IExternalServiceClient _externalClient;
    private readonly PaymentApiClient _paymentApiClient;
    private readonly IEventBus _eventBus;
    private readonly ILogger<BookingService> _logger;
    private readonly IConfiguration _configuration;

    public BookingService(
        IBookingRepository bookingRepository,
        ISeatStatusService seatStatusService,
        IExternalServiceClient externalClient,
        PaymentApiClient paymentApiClient,
        IEventBus eventBus,
        ILogger<BookingService> logger,
        IConfiguration configuration)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _seatStatusService = seatStatusService ?? throw new ArgumentNullException(nameof(seatStatusService));
        _externalClient = externalClient ?? throw new ArgumentNullException(nameof(externalClient));
        _paymentApiClient = paymentApiClient ?? throw new ArgumentNullException(nameof(paymentApiClient));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request)
    {
        _logger.LogInformation("Creating booking for user {UserId}, showtime {ShowtimeId}, seats: {SeatCount}",
            request.UserId, request.ShowtimeId, request.SeatIds.Count);

        // 1. Validate business rules
        var validationResult = await ValidateBookingRequestAsync(request);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        // 2. Get showtime info from Movie.API
        var showtime = await _externalClient.GetShowtimeByIdAsync(request.ShowtimeId);
        if (showtime == null)
        {
            return ApiResponse<BookingResponse>.NotFoundResponse(
                $"Showtime {request.ShowtimeId} not found"
            );
        }

        // Check if showtime is still active
        if (!showtime.IsActive || showtime.StartTime < DateTime.UtcNow)
        {
            return ApiResponse<BookingResponse>.FailureResponse(
                "Showtime is no longer available for booking",
                400,
                new List<ErrorDetail>
                {
                    new("SHOWTIME_INACTIVE", "Showtime is no longer available")
                }
            );
        }

        // 3. Get seat details from Cinema.API
        var seats = await _externalClient.GetSeatsByCinemaHallIdAsync(showtime.CinemaHallId);
        var selectedSeats = seats.Where(s => request.SeatIds.Contains(s.Id)).ToList();

        if (selectedSeats.Count != request.SeatIds.Count)
        {
            return ApiResponse<BookingResponse>.FailureResponse(
                "Some selected seats do not exist",
                400,
                new List<ErrorDetail>
                {
                    new("INVALID_SEATS", "Some selected seats do not exist")
                }
            );
        }

        // 4. Verify locked seats and mark as booked atomically
        // Note: Seats should already be locked by the user from the seat selection step
        // This verifies the lock is still valid and marks them as booked
        var bookingId = Guid.NewGuid();
        var bookingResult = await _seatStatusService.VerifyAndMarkAsBookedAsync(
            request.ShowtimeId,
            request.SeatIds,
            request.UserId,
            bookingId
        );

        if (!bookingResult.Success)
        {
            var errorCode = bookingResult.FailureReason switch
            {
                SeatBookingFailureReason.NotLocked => "SEATS_NOT_LOCKED",
                SeatBookingFailureReason.LockExpired => "LOCK_EXPIRED",
                SeatBookingFailureReason.WrongUser => "SEATS_LOCKED_BY_OTHER_USER",
                SeatBookingFailureReason.AlreadyBooked => "SEATS_ALREADY_BOOKED",
                _ => "SEATS_UNAVAILABLE"
            };

            var statusCode = bookingResult.FailureReason == SeatBookingFailureReason.LockExpired ? 410 : 409;

            return ApiResponse<BookingResponse>.FailureResponse(
                bookingResult.Message ?? "Cannot book seats",
                statusCode,
                new List<ErrorDetail>
                {
                    new(errorCode, bookingResult.Message ?? "Seats are not available for booking")
                }
            );
        }

        // 5. Calculate total price
        //var totalPrice = selectedSeats.Sum(s => s.);

        // 6. Create booking in database
        var expirationMinutes = _configuration.GetValue<int>("Booking:ExpirationMinutes", 10);
        var booking = new Booking.API.Domain.Entities.Booking
        {
            Id = bookingId, // Use the same ID from verification step
            UserId = request.UserId,
            ShowtimeId = request.ShowtimeId,
            TotalPrice = 0,
            Status = BookingStatus.Pending,
            BookingDate = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            BookingSeats = selectedSeats.Select(s => new BookingSeat
            {
                Id = Guid.NewGuid(),
                SeatId = s.Id,
                Price = showtime.Price
            }).ToList()
        };
        var totalPrice = booking.BookingSeats.Sum(bs => bs.Price);
        booking.TotalPrice = totalPrice;

        try
        {
            await _bookingRepository.CreateAsync(booking);

            // 7. Seats are already marked as booked in step 4 (VerifyAndMarkAsBookedAsync)
            // No need to call MarkSeatsAsBookedAsync again

            // 8. Publish integration event
            var integrationEvent = new BookingCreatedIntegrationEvent(
                booking.Id,
                booking.UserId,
                booking.ShowtimeId,
                request.SeatIds,
                totalPrice,
                booking.BookingDate,
                request.ContactEmail,
                request.ContactPhone,
                request.ContactName
            );

            _eventBus.Publish(integrationEvent);

            _logger.LogInformation("Booking {BookingId} created successfully", booking.Id);

            // 9. Wait for Payment.API to create payment record (via event handler)
            // Reduced retries to prevent gateway timeout (5s → 35s gateway timeout)
            _logger.LogInformation("Waiting for payment creation for booking {BookingId}", booking.Id);

            var paymentCheckout = await _paymentApiClient.WaitForPaymentCreationAsync(
                booking.Id,
                maxRetries: 8,  // ✅ Reduced from 15 to 8 (max ~6 seconds total)
                initialDelayMs: 200  // ✅ Increased from 100ms to 200ms for better event processing
            );

            // 10. Return response with payment checkout info
            var response = await MapToBookingResponseAsync(booking);

            if (paymentCheckout != null)
            {
                response.PaymentId = paymentCheckout.PaymentId;
                response.CheckoutUrl = paymentCheckout.CheckoutUrl;

                _logger.LogInformation(
                    "Booking {BookingId} created with payment {PaymentId}, checkout URL: {CheckoutUrl}",
                    booking.Id, paymentCheckout.PaymentId, paymentCheckout.CheckoutUrl);
            }
            else
            {
                _logger.LogWarning(
                    "Booking {BookingId} created but payment was not ready in time. Client should poll for payment.",
                    booking.Id);
            }

            return ApiResponse<BookingResponse>.SuccessResponse(
                response,
                "Booking created successfully",
                201
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking, rolling back seats to available");

            // Rollback: release booked seats back to available
            // Note: Seats were already marked as booked in step 4, so we need to release them
            await _seatStatusService.ReleaseBookedSeatsAsync(
                request.ShowtimeId,
                request.SeatIds
            );

            return ApiResponse<BookingResponse>.FailureResponse(
                "Failed to create booking due to system error",
                500,
                new List<ErrorDetail>
                {
                    new("DATABASE_ERROR", ex.Message)
                }
            );
        }
    }

    public async Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(Guid bookingId)
    {
        _logger.LogInformation("Getting booking {BookingId}", bookingId);

        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse<BookingResponse>.NotFoundResponse(
                $"Booking {bookingId} not found"
            );
        }

        var response = await MapToBookingResponseAsync(booking);
        return ApiResponse<BookingResponse>.SuccessResponse(response);
    }

    public async Task<ApiResponse<List<BookingResponse>>> GetUserBookingsAsync(string userId)
    {
        _logger.LogInformation("Getting bookings for user {UserId}", userId);

        var bookings = await _bookingRepository.GetByUserIdAsync(userId);

        var responses = new List<BookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToBookingResponseAsync(booking));
        }

        return ApiResponse<List<BookingResponse>>.SuccessResponse(
            responses,
            $"Found {responses.Count} bookings"
        );
    }

    public async Task<ApiResponse> CancelBookingAsync(Guid bookingId, CancelBookingRequest request)
    {
        _logger.LogInformation("Cancelling booking {BookingId} by user {UserId}",
            bookingId, request.UserId);

        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse.NotFoundResponse($"Booking {bookingId} not found");
        }

        // Verify ownership
        if (booking.UserId != request.UserId)
        {
            return ApiResponse.UnauthorizedResponse("You can only cancel your own bookings");
        }

        // Check if can be cancelled
        if (booking.Status == BookingStatus.Cancelled)
        {
            return ApiResponse.FailureResponse(
                "Booking is already cancelled",
                400,
                new List<ErrorDetail>
                {
                    new("ALREADY_CANCELLED", "Booking is already cancelled")
                }
            );
        }

        if (booking.Status == BookingStatus.Expired)
        {
            return ApiResponse.FailureResponse(
                "Booking has expired",
                400,
                new List<ErrorDetail>
                {
                    new("BOOKING_EXPIRED", "Cannot cancel expired booking")
                }
            );
        }

        // Update booking status
        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking);

        // Release seats in Redis
        var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
        await _seatStatusService.ReleaseBookedSeatsAsync(booking.ShowtimeId, seatIds);

        // Publish cancellation event (for refund processing if confirmed)
        var cancellationEvent = new BookingCancelledIntegrationEvent(
            booking.Id,
            booking.UserId,
            booking.ShowtimeId,
            seatIds,
            booking.Status == BookingStatus.Confirmed, // needsRefund
            request.CancellationReason ?? "User cancelled"
        );

        _eventBus.Publish(cancellationEvent);

        _logger.LogInformation("Booking {BookingId} cancelled successfully", bookingId);

        return ApiResponse.SuccessResponse("Booking cancelled successfully");
    }

    public async Task<ApiResponse> ConfirmBookingAsync(Guid bookingId, string transactionId)
    {
        _logger.LogInformation("Confirming booking {BookingId} with transaction {TransactionId}",
            bookingId, transactionId);

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse.NotFoundResponse($"Booking {bookingId} not found");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return ApiResponse.FailureResponse(
                $"Booking is not in Pending status (current: {booking.Status})",
                400,
                new List<ErrorDetail>
                {
                    new("INVALID_STATUS", $"Cannot confirm booking with status {booking.Status}")
                }
            );
        }

        // Update booking status
        booking.Status = BookingStatus.Confirmed;
        booking.ExpiresAt = null; // No longer expires
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking);

        _logger.LogInformation("Booking {BookingId} confirmed successfully", bookingId);

        return ApiResponse.SuccessResponse("Booking confirmed successfully");
    }

    public async Task<ApiResponse> ExpireBookingAsync(Guid bookingId)
    {
        _logger.LogInformation("Expiring booking {BookingId}", bookingId);

        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse.NotFoundResponse($"Booking {bookingId} not found");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            _logger.LogWarning("Cannot expire booking {BookingId} - status is {Status}",
                bookingId, booking.Status);

            return ApiResponse.FailureResponse(
                $"Cannot expire booking with status {booking.Status}",
                400,
                new List<ErrorDetail>
                {
                    new("INVALID_STATUS", "Only pending bookings can be expired")
                }
            );
        }

        // Update booking status
        booking.Status = BookingStatus.Expired;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking);

        // Release seats in Redis
        var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
        await _seatStatusService.ReleaseBookedSeatsAsync(booking.ShowtimeId, seatIds);

        // Publish expiration event
        var expirationEvent = new BookingExpiredIntegrationEvent(
            booking.Id,
            booking.ShowtimeId,
            seatIds,
            DateTime.UtcNow
        );

        _eventBus.Publish(expirationEvent);

        _logger.LogInformation("Booking {BookingId} expired successfully", bookingId);

        return ApiResponse.SuccessResponse("Booking expired successfully");
    }

    // Private helper methods
    private async Task<ApiResponse<BookingResponse>> ValidateBookingRequestAsync(CreateBookingRequest request)
    {
        var errors = new List<ErrorDetail>();

        // Check max seats per booking
        var maxSeats = _configuration.GetValue<int>("Booking:MaxSeatsPerBooking", 10);
        if (request.SeatIds.Count > maxSeats)
        {
            errors.Add(new ErrorDetail(
                "MAX_SEATS_EXCEEDED",
                $"Cannot book more than {maxSeats} seats at once",
                "SeatIds"
            ));
        }

        if (request.SeatIds.Count == 0)
        {
            errors.Add(new ErrorDetail(
                "SEATS_REQUIRED",
                "At least one seat must be selected",
                "SeatIds"
            ));
        }

        // Check minimum time before showtime
        var showtime = await _externalClient.GetShowtimeByIdAsync(request.ShowtimeId);
        if (showtime != null)
        {
            var minMinutes = _configuration.GetValue<int>("Booking:MinutesBeforeShowtimeToBook", 30);
            var minBookingTime = showtime.StartTime.AddMinutes(-minMinutes);

            if (DateTime.UtcNow > minBookingTime)
            {
                errors.Add(new ErrorDetail(
                    "TOO_LATE_TO_BOOK",
                    $"Cannot book seats less than {minMinutes} minutes before showtime",
                    "ShowtimeId"
                ));
            }
        }

        if (errors.Any())
        {
            return ApiResponse<BookingResponse>.ValidationErrorResponse("Validation failed", errors);
        }

        return ApiResponse<BookingResponse>.SuccessResponse(null!, "Validation passed");
    }

    private async Task<BookingResponse> MapToBookingResponseAsync(Booking.API.Domain.Entities.Booking booking)
    {
        // Get showtime details
        var showtime = await _externalClient.GetShowtimeByIdAsync(booking.ShowtimeId);
        ShowtimeDetailsDto? showtimeDetails = null;

        if (showtime != null)
        {
            var movie = await _externalClient.GetMovieByIdAsync(showtime.MovieId);
            var cinemaHall = await _externalClient.GetCinemaHallByIdAsync(showtime.CinemaHallId);

            showtimeDetails = new ShowtimeDetailsDto
            {
                ShowtimeId = showtime.Id,
                MovieTitle = movie?.Title ?? "Unknown",
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                CinemaHallName = cinemaHall?.Name ?? "Unknown"
            };
        }

        // Get seat details
        var seatDtos = new List<BookingSeatResponseDto>();
        if (showtime != null)
        {
            var seats = await _externalClient.GetSeatsByCinemaHallIdAsync(showtime.CinemaHallId);
            foreach (var bookingSeat in booking.BookingSeats)
            {
                var seat = seats.FirstOrDefault(s => s.Id == bookingSeat.SeatId);
                if (seat != null)
                {
                    seatDtos.Add(new BookingSeatResponseDto
                    {
                        SeatId = seat.Id,
                        Row = seat.Row,
                        Number = seat.Number,
                        Price = bookingSeat.Price
                    });
                }
            }
        }

        return new BookingResponse
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            ShowtimeId = booking.ShowtimeId,
            Status = booking.Status,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            ExpiresAt = booking.ExpiresAt,
            Seats = seatDtos,
            ShowtimeDetails = showtimeDetails
        };
    }
}


