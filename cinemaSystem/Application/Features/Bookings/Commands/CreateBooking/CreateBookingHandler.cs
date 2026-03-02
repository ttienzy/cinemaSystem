using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.BookingAggregate;
using Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingHandler(
        IShowtimeRepository showtimeRepo,
        IBookingRepository bookingRepo,
        IPromotionRepository promotionRepo,
        ISeatLockService seatLock,
        IPaymentGateway paymentGateway,
        IUnitOfWork uow,
        ILogger<CreateBookingHandler> logger) : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        public async Task<CreateBookingResult> Handle(
            CreateBookingCommand cmd, CancellationToken ct)
        {
            // 1. Load showtime with pricing
            var showtime = await showtimeRepo.GetByIdWithPricingAsync(cmd.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.ShowtimeAggregate.Showtime), cmd.ShowtimeId);

            logger.LogInformation("Creating booking for customer {CustomerId} at showtime {ShowtimeId}", cmd.CustomerId, cmd.ShowtimeId);

            // 2. Domain rules validation
            var seatIds = cmd.Seats.Select(s => s.SeatId).ToList();
            BookingRules.ValidateBookingTime(showtime.ActualStartTime);
            BookingRules.ValidateTicketCount(cmd.Seats.Count);

            // 3. Check seat lock availability (Redis)
            var seatStatuses = await seatLock.GetSeatStatusesAsync(cmd.ShowtimeId, seatIds, ct);
            var unavailable = seatStatuses.Where(kv => kv.Value != SeatLockStatus.Available).ToList();
            if (unavailable.Count != 0)
                throw new ConflictException($"Seats are not available: {string.Join(", ", unavailable.Select(kv => kv.Key))}");

            // 4. Build tickets
            var tickets = cmd.Seats
                .Select(s => new BookingTicket(s.SeatId, s.Price))
                .ToList();
            var totalAmount = tickets.Sum(t => t.TicketPrice);

            // 5. Create booking aggregate (raises BookingCreatedEvent internally)
            var booking = Booking.Create(
                cmd.CustomerId, cmd.ShowtimeId,
                cmd.Seats.Count, totalAmount, tickets);

            // 6. Apply promotion if provided
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(cmd.PromotionCode))
            {
                var promo = await promotionRepo.GetByCodeAsync(cmd.PromotionCode, ct);
                if (promo is not null)
                {
                    var (canApply, discount, _) = promo.Evaluate(totalAmount);
                    if (canApply)
                    {
                        booking.ApplyPromotion(promo.Id, discount);
                        promo.IncrementUsage();
                        discountAmount = discount;
                    }
                }
            }

            // 7. Lock seats in Redis (15min TTL)
            try
            {
                await seatLock.LockSeatsAsync(
                    cmd.ShowtimeId, seatIds, booking.Id,
                    TimeSpan.FromMinutes(15), ct);
            }
            catch (InvalidOperationException)
            {
                throw new ConflictException("One or more seats have just been booked by another user. Please refresh and try again.");
            }

            // 8. Persist
            await bookingRepo.AddAsync(booking, ct);
            showtime.IncrementBookedSeats(cmd.Seats.Count);
            await uow.SaveChangesAsync(ct);

            // 9. Generate payment URL
            var paymentUrl = paymentGateway.CreatePaymentUrl(
                new PaymentRequest(booking.Id, booking.BookingCode, booking.FinalAmount, "Cinema booking"),
                cmd.ClientIpAddress ?? "127.0.0.1",
                "");

            logger.LogInformation("Booking {BookingId} created successfully with code {BookingCode}. Total: {Amount}",
                booking.Id, booking.BookingCode, booking.FinalAmount);

            return new CreateBookingResult(
                booking.Id, booking.BookingCode,
                totalAmount, discountAmount, booking.FinalAmount,
                booking.ExpiresAt, paymentUrl);
        }
    }
}
