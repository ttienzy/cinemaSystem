using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.BookingAggregate;
using Domain.Entities.BookingAggregate.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Bookings.Commands.CompleteBooking
{
    public class CompleteBookingHandler(
        IBookingRepository bookingRepo,
        ISeatLockService seatLock,
        ICacheService cacheService,
        IUnitOfWork uow,
        ILogger<CompleteBookingHandler> logger) : IRequestHandler<CompleteBookingCommand, Unit>
    {
        private const string ProcessedTransactionKeyPrefix = "payment:processed:";

        public async Task<Unit> Handle(CompleteBookingCommand cmd, CancellationToken ct)
        {
            logger.LogInformation("Completing booking {BookingId} with Transaction {TransactionId}", cmd.BookingId, cmd.TransactionId);

            // Idempotency check: Prevent duplicate payment processing
            var transactionKey = $"{ProcessedTransactionKeyPrefix}{cmd.TransactionId}";
            var alreadyProcessed = await cacheService.GetAsync<bool>(transactionKey);

            if (alreadyProcessed)
            {
                logger.LogWarning("Transaction {TransactionId} already processed. Skipping duplicate callback.", cmd.TransactionId);
                return Unit.Value;
            }

            var booking = await bookingRepo.GetByIdWithDetailsAsync(cmd.BookingId, ct)
                ?? throw new NotFoundException(nameof(Booking), cmd.BookingId);

            // Check if booking is already completed
            if (booking.Status == BookingStatus.Completed)
            {
                logger.LogWarning("Booking {BookingId} is already completed. Skipping duplicate completion.", cmd.BookingId);
                // Mark transaction as processed to prevent future retries
                await cacheService.SetAsync(transactionKey, true, TimeSpan.FromDays(7));
                return Unit.Value;
            }

            // Check if booking has expired
            if (booking.IsExpired)
            {
                logger.LogWarning("Booking {BookingId} has expired at {ExpiresAt}. Cannot complete.", cmd.BookingId, booking.ExpiresAt);
                throw new ConflictException($"Booking has expired. Please create a new booking.");
            }

            // Create payment and complete booking (raises BookingCompletedEvent)
            var payment = Payment.CreatePending(booking.Id, booking.FinalAmount);
            payment.Complete(cmd.TransactionId, cmd.ReferenceCode);
            booking.Complete(payment);

            // Release Redis locks (seats are now officially booked in DB)
            var seatIds = booking.BookingTickets.Select(t => t.SeatId).ToList();
            await seatLock.ReleaseSeatsAsync(booking.ShowtimeId, seatIds, ct);

            // Mark transaction as processed (7 days TTL)
            await cacheService.SetAsync(transactionKey, true, TimeSpan.FromDays(7));

            await uow.SaveChangesAsync(ct);

            logger.LogInformation("Booking {BookingId} completed successfully.", booking.Id);

            return Unit.Value;
        }
    }
}
