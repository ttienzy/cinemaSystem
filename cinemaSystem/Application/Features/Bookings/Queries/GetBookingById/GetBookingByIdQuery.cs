using MediatR;

namespace Application.Features.Bookings.Queries.GetBookingById
{
    public record GetBookingByIdQuery(Guid BookingId) : IRequest<BookingDetailDto?>;

    public record BookingDetailDto(
        Guid Id,
        string BookingCode,
        Guid? CustomerId,
        Guid ShowtimeId,
        DateTime BookingTime,
        DateTime ExpiresAt,
        int TotalTickets,
        decimal TotalAmount,
        decimal DiscountAmount,
        decimal FinalAmount,
        string Status,
        bool IsCheckedIn,
        List<BookingTicketDto> Tickets,
        List<PaymentDto> Payments);

    public record BookingTicketDto(Guid SeatId, decimal TicketPrice, string? SeatLabel);
    public record PaymentDto(Guid Id, decimal Amount, string Status, DateTime CreatedAt, string? TransactionId);
}
