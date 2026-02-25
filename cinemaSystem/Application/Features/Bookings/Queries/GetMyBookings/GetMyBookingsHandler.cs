using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Bookings.Queries.GetMyBookings
{
    public class GetMyBookingsHandler(
        IBookingRepository bookingRepo) : IRequestHandler<GetMyBookingsQuery, MyBookingsResult>
    {
        public async Task<MyBookingsResult> Handle(GetMyBookingsQuery query, CancellationToken ct)
        {
            var bookings = await bookingRepo.GetByCustomerAsync(
                query.CustomerId, query.Page, query.PageSize, ct);

            var items = bookings.Select(b => new BookingSummaryDto(
                b.Id, b.BookingCode, b.ShowtimeId, b.BookingTime,
                b.FinalAmount, b.Status.ToString(), b.TotalTickets, b.IsCheckedIn))
                .ToList();

            return new MyBookingsResult(items, items.Count, query.Page, query.PageSize);
        }
    }
}
