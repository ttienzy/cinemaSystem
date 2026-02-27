using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.BookingAggregate;
using Domain.Entities.BookingAggregate.Enums;
using Domain.Entities.ShowtimeAggregate;
using Domain.Common;
using MediatR;
using Shared.Models.DataModels.BookingDtos;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Bookings.Commands.CreateCounterBooking
{
    public class CreateCounterBookingHandler(
        IShowtimeRepository showtimeRepo,
        IBookingRepository bookingRepo,
        IStaffRepository staffRepo,
        IPromotionRepository promotionRepo,
        ISeatLockService seatLock,
        ICurrentUserService currentUser,
        IUnitOfWork uow) : IRequestHandler<CreateCounterBookingCommand, CounterBookingResponse>
    {
        public async Task<CounterBookingResponse> Handle(
            CreateCounterBookingCommand cmd, CancellationToken ct)
        {
            var req = cmd.Request;

            // 1. Identification: Lookup Staff via Current User Email
            var staff = await staffRepo.GetByEmailAsync(currentUser.Email ?? string.Empty, ct)
                ?? throw new UnauthorizedException("Authenticated user is not registered as staff.");

            // 2. Load Showtime
            var showtime = await showtimeRepo.GetByIdWithPricingAsync(req.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Showtime), req.ShowtimeId);

            // 3. Prevent booking for past showtimes (professional rule)
            if (showtime.ActualStartTime < DateTime.UtcNow.AddMinutes(-10))
                throw new DomainException("Cannot book seats for a showtime that has already started or ended.");

            // 4. Validate Seats and Build Tickets
            var seatIds = req.Seats.Select(s => s.SeatId).ToList();
            var seatStatuses = await seatLock.GetSeatStatusesAsync(req.ShowtimeId, seatIds, ct);
            var unavailable = seatStatuses.Where(kv => kv.Value != SeatLockStatus.Available).ToList();
            if (unavailable.Any())
                throw new ConflictException($"Seats already occupied: {string.Join(", ", unavailable.Select(kv => kv.Key))}");

            var tickets = req.Seats
                .Select(s => new BookingTicket(s.SeatId, s.Price))
                .ToList();

            var totalAmount = tickets.Sum(t => t.TicketPrice);

            // 5. Create Booking (Staff Context)
            var booking = Booking.CreateAtCounter(
                req.CustomerId,
                staff.Id,
                req.ShowtimeId,
                tickets.Count,
                totalAmount,
                tickets);

            // 6. Rapid Confirmation logic
            // In POS, we don't wait for webhook. We mark as paid immediately.
            var payment = Payment.CreatePending(booking.Id, totalAmount, req.PaymentMethod);
            payment.Complete("POS-CASH-" + Guid.NewGuid().ToString("N")[..8], "COUNTER");
            
            booking.Complete(payment); // Marks status as Completed
            booking.CheckIn(); // Usually POS sales are ready for entry

            // 7. Persist and Unlock
            await bookingRepo.AddAsync(booking, ct);
            showtime.IncrementBookedSeats(tickets.Count);
            
            await uow.SaveChangesAsync(ct);
            // POS bookings don't need Redis locks to persist, but we should clear any temp locks
            await seatLock.ReleaseSeatsAsync(req.ShowtimeId, seatIds, ct);

            return new CounterBookingResponse
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                TotalAmount = booking.TotalAmount,
                FinalAmount = booking.FinalAmount,
                BookingTime = booking.BookingTime
            };
        }
    }
}
