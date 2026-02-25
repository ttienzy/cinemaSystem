using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdHandler(
        IBookingRepository bookingRepo) : IRequestHandler<GetBookingByIdQuery, BookingDetailDto?>
    {
        public async Task<BookingDetailDto?> Handle(GetBookingByIdQuery query, CancellationToken ct)
        {
            var booking = await bookingRepo.GetByIdWithDetailsAsync(query.BookingId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.BookingAggregate.Booking), query.BookingId);

            return new BookingDetailDto(
                booking.Id,
                booking.BookingCode,
                booking.CustomerId,
                booking.ShowtimeId,
                booking.BookingTime,
                booking.ExpiresAt,
                booking.TotalTickets,
                booking.TotalAmount,
                booking.DiscountAmount,
                booking.FinalAmount,
                booking.Status.ToString(),
                booking.IsCheckedIn,
                booking.BookingTickets.Select(t => new BookingTicketDto(
                    t.SeatId, t.TicketPrice, null)).ToList(),
                booking.Payments.Select(p => new PaymentDto(
                    p.Id, p.Amount, p.Status.ToString(), p.CreatedAt, p.TransactionId)).ToList());
        }
    }
}
