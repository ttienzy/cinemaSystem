using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

public class BookingCreationPreparationService : IBookingCreationPreparationService
{
    private readonly IExternalServiceClient _externalClient;
    private readonly IConfiguration _configuration;

    public BookingCreationPreparationService(
        IExternalServiceClient externalClient,
        IConfiguration configuration)
    {
        _externalClient = externalClient ?? throw new ArgumentNullException(nameof(externalClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<BookingCreationPreparationResult> PrepareAsync(CreateBookingRequest request)
    {
        var validationErrors = ValidateSeatSelection(request.SeatIds);
        if (validationErrors.Count > 0)
        {
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.ValidationErrorResponse(
                    BookingException.VALIDATION_FAILED,
                    validationErrors)
            };
        }

        var showtime = await _externalClient.GetShowtimeByIdAsync(request.ShowtimeId);
        if (showtime == null)
        {
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.NotFoundResponse(
                    BookingException.SHOWTIME_NOT_FOUND(request.ShowtimeId))
            };
        }

        if (!showtime.IsActive || showtime.StartTime < DateTime.UtcNow)
        {
            var value = BookingException.SHOWTIME_INACTIVE;
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.FailureResponse(
                    BookingException.SHOWTIME_NOT_AVAILABLE,
                    400,
                    [new ErrorDetail(value.Code, value.Message, value.Field)])
            };
        }

        var timingErrors = ValidateShowtimeTiming(showtime);
        if (timingErrors.Count > 0)
        {
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.ValidationErrorResponse(
                    BookingException.VALIDATION_FAILED,
                    timingErrors)
            };
        }

        var seats = await _externalClient.GetSeatsByCinemaHallIdAsync(showtime.CinemaHallId);
        var selectedSeats = seats
            .Where(seat => request.SeatIds.Contains(seat.Id))
            .ToList();

        if (selectedSeats.Count != request.SeatIds.Count)
        {
            var value = BookingException.INVALID_SEATS;
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.FailureResponse(
                    BookingException.INVALID_SEATS_MESSAGE,
                    400,
                    [new ErrorDetail(value.Code, value.Message, value.Field)])
            };
        }

        return new BookingCreationPreparationResult
        {
            Showtime = showtime,
            SelectedSeats = selectedSeats
        };
    }

    private List<ErrorDetail> ValidateSeatSelection(List<Guid> seatIds)
    {
        var errors = new List<ErrorDetail>();
        var maxSeats = _configuration.GetValue<int>("Booking:MaxSeatsPerBooking", 10);

        if (seatIds.Count > maxSeats)
        {
            var value = BookingException.MAX_SEATS_EXCEEDED(maxSeats);
            errors.Add(new ErrorDetail(value.Code, value.Message, value.Field));
        }

        if (seatIds.Count == 0)
        {
            var value = BookingException.SEATS_REQUIRED;
            errors.Add(new ErrorDetail(value.Code, value.Message, value.Field));
        }

        return errors;
    }

    private List<ErrorDetail> ValidateShowtimeTiming(ShowtimeDto showtime)
    {
        var errors = new List<ErrorDetail>();
        var minMinutes = _configuration.GetValue<int>("Booking:MinutesBeforeShowtimeToBook", 30);
        var minBookingTime = showtime.StartTime.AddMinutes(-minMinutes);

        if (DateTime.UtcNow > minBookingTime)
        {
            var value = BookingException.TOO_LATE_TO_BOOK(minMinutes);
            errors.Add(new ErrorDetail(value.Code, value.Message, value.Field));
        }

        return errors;
    }
}
