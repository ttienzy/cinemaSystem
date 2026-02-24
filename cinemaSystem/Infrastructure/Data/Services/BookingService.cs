using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.BookingSpec;
using Domain.Entities.BookingAggregate;
using Domain.Entities.ShowtimeAggregate;
using Infrastructure.Hubs;
using Infrastructure.Hubs.Constants;
using Infrastructure.Identity;
using Infrastructure.Redis.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Common.Base;
using Shared.Models.DataModels.BookingDtos;
using Shared.Models.DataModels.ShowtimeDtos;
using Shared.Models.ExtenalModels;
using Shared.Models.PaymentModels;
using Shared.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vnPayService;
        private readonly ICacheService _cacheService;
        private readonly IBookingRepository _bookingCustomeRepository;
        private readonly IHubContext<SeatHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IUnitOfWork unitOfWork,
            IVnPayService vnPayService,
            ICacheService cacheService,
            IBookingRepository bookingRepository,
            IHubContext<SeatHub> hubContext,
            IEmailService emailService,
            ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _vnPayService = vnPayService;
            _cacheService = cacheService;
            _bookingCustomeRepository = bookingRepository;
            _hubContext = hubContext;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<BaseResponse<string>> CancelPaymentAsync(PaymentResponse response)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var bookingSpec = new BookingWithPaymentSpec(Guid.Parse(response.OrderDescription));
                var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(bookingSpec);
                if (booking == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Booking not found"));
                }
                var seatIds = booking.BookingTickets.Select(_ => _.SeatId).ToHashSet();
                
                var results = await _cacheService.GetAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(booking.ShowtimeId));
                if (results == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Seats not found"));
                }
                results.Seats.ForEach(s =>
                {
                    if (seatIds.Contains(s.Id))
                    {
                        s.Status = Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Available;
                        s.LastUpdated = DateTime.UtcNow;
                    }
                });
                await _cacheService.UpdateAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(booking.ShowtimeId), results);
                await _unitOfWork.Bookings.DeleteAsync(booking);
                await _unitOfWork.CommitTransactionAsync();
                await _hubContext.Clients.Group(booking.ShowtimeId.ToString()).SendAsync(SignalMethodConstants.OnSeatsReleased, booking.BookingTickets.Select(e => e.SeatId).ToList());
                await _hubContext.Clients.User(booking.CustomerId.ToString() ?? "#").SendAsync(SignalMethodConstants.OnPaymentCanceled, booking.BookingTickets.Select(e => e.SeatId).ToList());
                return BaseResponse<string>.Success(booking.ShowtimeId.ToString());
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<BaseResponse<BookingCheckedInResponse>> CheckInBookingAsync(Guid bookingId)
        {
            try
            {
                _logger.LogInformation("aaaaaaaaaa");

                var chekedInInfo = await _bookingCustomeRepository.CheckInBookingAsync(bookingId);
                if (chekedInInfo == null || chekedInInfo.BookingTime == default)
                {
                    return BaseResponse<BookingCheckedInResponse>.Failure(Error.NotFound("Booking not found"));
                }
                return BaseResponse<BookingCheckedInResponse>.Success(chekedInInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BaseResponse<BookingCheckedInResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> ConfirmCheckedIn(Guid bookingId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Booking not found"));
                }
                booking.MarkAsCheckedIn();
                await _unitOfWork.Bookings.UpdateAsync(booking);
                return BaseResponse<string>.Success("Check-in confirmed");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> ConfirmPaymentAsync(PaymentResponse response)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Get booking
                var bookingSpec = new BookingWithPaymentSpec(Guid.Parse(response.OrderDescription.ToString()));
                var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(bookingSpec);

                if (booking == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BaseResponse<string>.Failure(Error.NotFound("Booking not found"));
                }

                // 2. Validate payment
                var payment = booking.Payments.FirstOrDefault();
                if (payment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BaseResponse<string>.Failure(Error.NotFound("Payment not found"));
                }

                // 3. Get seating plan
                var seatingPlan = await _cacheService.GetAsync<ShowtimeSeatingPlanResponse>(
                    CacheKey.SeatingPlan(booking.ShowtimeId));

                if (seatingPlan == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BaseResponse<string>.Failure(Error.NotFound("Seats not found"));
                }

                // 4. Update booking status
                booking.MarkAsCompleted();

                // 5. Update seats in cache
                var seatIds = booking.BookingTickets.Select(bt => bt.SeatId).ToHashSet();
                var currentTime = DateTime.UtcNow;
                var bookedSeats = new List<string>();

                foreach (var seat in seatingPlan.Seats.Where(s => seatIds.Contains(s.Id)))
                {
                    seat.Status = Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Booked;
                    seat.LastUpdated = currentTime;
                    bookedSeats.Add($"{seat.RowName}{seat.Number}");
                }

                await _cacheService.UpdateAsync(
                    CacheKey.SeatingPlan(booking.ShowtimeId), seatingPlan);

                // 6. Update payment info
                payment.UpdatePayment(
                    response.PaymentMethod,
                    response.TransactionId,
                    response.VnPayResponseCode);

                // 7. Save to database
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 8. Send real-time notifications (after successful commit)
                var bookedSeatIds = booking.BookingTickets.Select(e => e.SeatId).ToList();

                await Task.WhenAll(
                    _hubContext.Clients
                        .Group(booking.ShowtimeId.ToString())
                        .SendAsync(SignalMethodConstants.OnSeatsBooked, bookedSeatIds),

                    _hubContext.Clients
                        .User(booking.CustomerId?.ToString() ?? "#")
                        .SendAsync(SignalMethodConstants.OnPaymentSuccessful, bookedSeatIds)
                );

                // 9. Send confirmation email (fire and forget - don't block the response)
                if (booking.CustomerId != null)
                {
                    var emailResponse = new EmailConfirmBookingResponse
                    {
                        BookingCode = booking.Id,
                        CinemaName = seatingPlan.ShowtimeInfo.CinemaName,
                        MovieTitle = seatingPlan.ShowtimeInfo.MovieTitle,
                        ScreenName = seatingPlan.ShowtimeInfo.ScreenName,
                        Showtime = seatingPlan.ShowtimeInfo.ShowDate.Date,
                        TimeSlot = $"{seatingPlan.ShowtimeInfo.ActualStartTime:HH:mm} - {seatingPlan.ShowtimeInfo.ActualEndTime:HH:mm}",
                        TotalTickets = booking.TotalTickets,
                        TotalAmount = booking.TotalAmount,
                        SeatsList = bookedSeats
                    };

                    _ = SendEmailAsync(emailResponse);
                }

                return BaseResponse<string>.Success(booking.ShowtimeId.ToString());
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                // TODO: Add logging
                // _logger.LogError(ex, "Error confirming payment for order: {OrderId}", response.OrderDescription);
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        private async Task SendEmailAsync(EmailConfirmBookingResponse emailResponse)
        {
            try
            {
                await _emailService.SendBookingConfirmationEmailAsync(
                    toEmail: "nguyendientien01062005@gmail.com", // TODO: Get actual customer email
                    bookingInfo: emailResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Log but don't throw - email failure shouldn't affect payment confirmation
                // _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", emailResponse.BookingCode);
            }
        }

        public async Task<BaseResponse<string>> CreateBookingAsync(PaymentInfomationRequest request, HttpContext httpContext)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cachedSeatingPlan = await _cacheService.GetAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(request.ShowtimeId));
                if (cachedSeatingPlan == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Seating plan not found"));
                }

                var selectedSeatIds = request.SelectedSeats.Select(seat => seat.SeatId).ToHashSet();
                if (selectedSeatIds.Count != request.SelectedSeats.Count())
                {
                    return BaseResponse<string>.Failure(Error.BadRequest("Duplicate seat selection detected"));
                }

                //var selectedSeats = cachedSeatingPlan.Seats.Where(seat => selectedSeatIds.Contains(seat.Id) && seat.Status == Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Available).ToList();
                //if (selectedSeats.Count != request.SelectedSeats.Count())
                //{
                //    return BaseResponse<string>.Failure(Error.BadRequest("One or more selected seats are invalid or already booked"));
                //}

                var selectedPrices = cachedSeatingPlan.Pricings.Select(pricing => pricing.FinalPrice).ToHashSet();
                var invalidPriceSeats = request.SelectedSeats.Where(seat => !selectedPrices.Contains(seat.Price)).ToList();
                if (invalidPriceSeats.Any())
                {
                    return BaseResponse<string>.Failure(Error.BadRequest("Some selected seats have invalid prices"));
                }


                var newBooking = new Booking(request.UserId, request.ShowtimeId, request.SelectedSeats.Count(), request.SelectedSeats.Sum(s => s.Price));
                newBooking.AddTickets(request.SelectedSeats.Select(seat => new BookingTicket(seat.SeatId, seat.Price)).ToList());
                newBooking.AddPayment(new Payment(newBooking.TotalAmount));


                await _unitOfWork.Bookings.AddAsync(newBooking);
                request.BookingId = newBooking.Id;
                var paymentUrl = _vnPayService.CreatePaymentUrl(request, httpContext);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return BaseResponse<string>.Success(paymentUrl);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<BaseResponse<IEnumerable<PurchaseResponse>>> PurchaseHistoryAsync(Guid userId)
        {
            try
            {
                var purchases = await _bookingCustomeRepository.PurchaseHistoryAsync(userId);
                return BaseResponse<IEnumerable<PurchaseResponse>>.Success(purchases);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<PurchaseResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
