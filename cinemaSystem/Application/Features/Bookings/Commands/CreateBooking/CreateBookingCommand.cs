using MediatR;

namespace Application.Features.Bookings.Commands.CreateBooking
{
    public record CreateBookingCommand(
        Guid CustomerId,
        Guid ShowtimeId,
        List<SeatSelection> Seats,
        string? PromotionCode) : IRequest<CreateBookingResult>;

    public record SeatSelection(Guid SeatId, decimal Price);

    public record CreateBookingResult(
        Guid BookingId,
        string BookingCode,
        decimal TotalAmount,
        decimal DiscountAmount,
        decimal FinalAmount,
        DateTime ExpiresAt,
        string? PaymentUrl);
}
