using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.BookingAggregate;
using Domain.Entities.ShowtimeAggregate;
using Domain.Services;
using Domain.Common;
using MediatR;

namespace Application.Features.Bookings.Commands.RequestRefund
{
    public record RequestRefundCommand(Guid BookingId, string Reason) : IRequest<Unit>;

    public class RequestRefundHandler(
        IBookingRepository bookingRepo,
        IShowtimeRepository showtimeRepo,
        IUnitOfWork uow) : IRequestHandler<RequestRefundCommand, Unit>
    {
        public async Task<Unit> Handle(RequestRefundCommand request, CancellationToken ct)
        {
            var booking = await bookingRepo.GetByIdAsync(request.BookingId, ct)
                ?? throw new NotFoundException(nameof(Booking), request.BookingId);

            var showtime = await showtimeRepo.GetByIdAsync(booking.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Showtime), booking.ShowtimeId);

            var (canRefund, percentage, reason) = RefundPolicy.Evaluate(booking, showtime.ActualStartTime);

            if (!canRefund)
                throw new DomainException(reason);

            decimal refundAmount = booking.FinalAmount * (percentage / 100m);
            
            booking.RequestRefund(percentage, refundAmount, request.Reason);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
