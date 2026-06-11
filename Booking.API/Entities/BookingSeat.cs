namespace Booking.API.Entities;

public class BookingSeat : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public Guid SeatId { get; set; }
    public decimal Price { get; set; }
}


