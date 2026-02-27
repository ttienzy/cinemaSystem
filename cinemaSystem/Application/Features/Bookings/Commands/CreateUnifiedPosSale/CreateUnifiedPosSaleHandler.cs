using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.BookingAggregate;
using Domain.Entities.BookingAggregate.Enums;
using Domain.Entities.ConcessionAggregate;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.ShowtimeAggregate;
using Domain.Common;
using MediatR;
using Shared.Models.DataModels.BookingDtos;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Bookings.Commands.CreateUnifiedPosSale
{
    public record CreateUnifiedPosSaleCommand(UnifiedPosRequest Request) : IRequest<UnifiedPosResponse>;

    public class CreateUnifiedPosSaleHandler(
        IShowtimeRepository showtimeRepo,
        IBookingRepository bookingRepo,
        IConcessionSaleRepository concessionSaleRepo,
        IInventoryRepository inventoryRepo,
        IStaffRepository staffRepo,
        ISeatLockService seatLock,
        ICurrentUserService currentUser,
        IUnitOfWork uow) : IRequestHandler<CreateUnifiedPosSaleCommand, UnifiedPosResponse>
    {
        public async Task<UnifiedPosResponse> Handle(CreateUnifiedPosSaleCommand cmd, CancellationToken ct)
        {
            var req = cmd.Request;

            // 1. Identify staff
            var staff = await staffRepo.GetByEmailAsync(currentUser.Email ?? string.Empty, ct)
                ?? throw new UnauthorizedException("Authenticated user is not registered as staff.");

            // 2. Load showtime
            var showtime = await showtimeRepo.GetByIdWithPricingAsync(req.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Showtime), req.ShowtimeId);

            if (showtime.ActualStartTime < DateTime.UtcNow.AddMinutes(-10))
                throw new DomainException("Cannot book seats for a showtime that has already started.");

            // 3. Validate seat availability
            var seatIds = req.Seats.Select(s => s.SeatId).ToList();
            var seatStatuses = await seatLock.GetSeatStatusesAsync(req.ShowtimeId, seatIds, ct);
            var unavailable = seatStatuses.Where(kv => kv.Value != SeatLockStatus.Available).ToList();
            if (unavailable.Any())
                throw new ConflictException($"Seats already occupied: {string.Join(", ", unavailable.Select(kv => kv.Key))}");

            // 4. Build ticket booking
            var tickets = req.Seats.Select(s => new BookingTicket(s.SeatId, s.Price)).ToList();
            var ticketAmount = tickets.Sum(t => t.TicketPrice);

            var booking = Booking.CreateAtCounter(
                req.CustomerId, staff.Id, req.ShowtimeId,
                tickets.Count, ticketAmount, tickets);

            var payment = Payment.CreatePending(booking.Id, ticketAmount, req.PaymentMethod);
            payment.Complete("POS-" + Guid.NewGuid().ToString("N")[..8], "COUNTER");
            booking.Complete(payment);
            booking.CheckIn();

            await bookingRepo.AddAsync(booking, ct);
            showtime.IncrementBookedSeats(tickets.Count);

            // 5. Process concessions (if any)
            Guid? concessionSaleId = null;
            decimal concessionAmount = 0;

            if (req.Concessions.Any())
            {
                var concessionSale = ConcessionSale.Create(
                    showtime.CinemaId, staff.Id, req.PaymentMethod, booking.Id);

                foreach (var itemReq in req.Concessions)
                {
                    var item = await inventoryRepo.GetByIdAsync(itemReq.InventoryItemId, ct)
                        ?? throw new NotFoundException(nameof(InventoryItem), itemReq.InventoryItemId);

                    concessionSale.AddItem(new ConcessionSaleItem(item.Id, itemReq.Quantity, item.UnitPrice));
                    item.Deduct(itemReq.Quantity, concessionSale.Id, $"POS-bundle: {booking.BookingCode}");
                    inventoryRepo.Update(item);
                }

                concessionAmount = concessionSale.TotalAmount;
                concessionSaleId = concessionSale.Id;
                await concessionSaleRepo.AddAsync(concessionSale, ct);
            }

            // 6. Single commit — atomicity
            await uow.SaveChangesAsync(ct);

            // 7. Release seat locks
            await seatLock.ReleaseSeatsAsync(req.ShowtimeId, seatIds, ct);

            return new UnifiedPosResponse
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                ConcessionSaleId = concessionSaleId,
                TicketAmount = ticketAmount,
                ConcessionAmount = concessionAmount,
                TotalAmount = ticketAmount + concessionAmount,
                BookingTime = booking.BookingTime,
                PaymentMethod = req.PaymentMethod
            };
        }
    }
}
