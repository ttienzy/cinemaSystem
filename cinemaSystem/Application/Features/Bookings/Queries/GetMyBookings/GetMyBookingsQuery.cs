using MediatR;

namespace Application.Features.Bookings.Queries.GetMyBookings
{
    public record GetMyBookingsQuery(
        Guid CustomerId,
        int Page = 1,
        int PageSize = 10) : IRequest<MyBookingsResult>;

    public record MyBookingsResult(
        List<BookingSummaryDto> Items,
        int Total,
        int Page,
        int PageSize);

    public record BookingSummaryDto(
        Guid Id,
        string BookingCode,
        Guid ShowtimeId,
        DateTime BookingTime,
        decimal FinalAmount,
        string Status,
        int TotalTickets,
        bool IsCheckedIn);
}
