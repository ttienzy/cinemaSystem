using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.BookingAggregate;
using Domain.Entities.ShowtimeAggregate;
using Domain.Common;
using MediatR;

namespace Application.Features.Bookings.Commands.ApproveRefund
{
    public record ApproveRefundCommand(Guid BookingId) : IRequest<Unit>;

    public class ApproveRefundHandler(
        IBookingRepository bookingRepo,
        IShowtimeRepository showtimeRepo,
        IUnitOfWork uow) : IRequestHandler<ApproveRefundCommand, Unit>
    {
        public async Task<Unit> Handle(ApproveRefundCommand request, CancellationToken ct)
        {
            var booking = await bookingRepo.GetByIdAsync(request.BookingId, ct)
                ?? throw new NotFoundException(nameof(Booking), request.BookingId);

            var showtime = await showtimeRepo.GetByIdAsync(booking.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Showtime), booking.ShowtimeId);

            // 1. Domain logic to approve refund
            booking.ApproveRefund();

            // 2. Business side-effect: Free up the slots in showtime capacity
            showtime.DecrementBookedSeats(booking.TotalTickets);

            // Note: Real-time seat release in Redis/SignalR should ideally be handled 
            // by a Domain Event Handler for BookingRefundedEvent to keep the command clean.
            // For now, we persist the domain state.

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
