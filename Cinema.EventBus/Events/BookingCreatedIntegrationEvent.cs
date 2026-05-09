using Cinema.EventBus.Abstractions;

namespace Cinema.EventBus.Events;

/// <summary>
/// Event published when a booking is created
/// </summary>
public class BookingCreatedIntegrationEvent : IntegrationEvent
{
    public Guid BookingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public List<Guid> SeatIds { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public BookingCreatedIntegrationEvent()
    {
    }

    public BookingCreatedIntegrationEvent(
        Guid bookingId,
        string userId,
        Guid showtimeId,
        List<Guid> seatIds,
        decimal totalPrice,
        DateTime bookingDate,
        string customerEmail,
        string customerPhone,
        string customerName)
    {
        BookingId = bookingId;
        UserId = userId;
        ShowtimeId = showtimeId;
        SeatIds = seatIds;
        TotalPrice = totalPrice;
        BookingDate = bookingDate;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        CustomerName = customerName;
    }
}
