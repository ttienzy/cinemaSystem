using Cinema.Shared.Entities;

namespace Booking.API.Domain.Entities;

public class Booking : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Paid = 1,
    Cancelled = 2,
    Expired = 3,
    CheckedIn = 4
}


