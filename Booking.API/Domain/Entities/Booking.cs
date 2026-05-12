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

    public static Booking CreatePending(
        Guid bookingId,
        string userId,
        Guid showtimeId,
        IEnumerable<Guid> seatIds,
        decimal seatPrice,
        DateTime bookingDateUtc,
        DateTime expiresAtUtc)
    {
        var booking = new Booking
        {
            Id = bookingId,
            UserId = userId,
            ShowtimeId = showtimeId,
            Status = BookingStatus.Pending,
            BookingDate = bookingDateUtc,
            ExpiresAt = expiresAtUtc,
            BookingSeats = seatIds.Select(seatId => new BookingSeat
            {
                Id = Guid.NewGuid(),
                SeatId = seatId,
                Price = seatPrice
            }).ToList()
        };

        booking.TotalPrice = booking.BookingSeats.Sum(seat => seat.Price);
        return booking;
    }

    public bool IsOwnedBy(string userId)
    {
        return string.Equals(UserId, userId, StringComparison.Ordinal);
    }

    public bool IsPending()
    {
        return Status == BookingStatus.Pending;
    }

    public bool IsConfirmed()
    {
        return Status == BookingStatus.Confirmed;
    }

    public bool IsCancelled()
    {
        return Status == BookingStatus.Cancelled;
    }

    public bool IsExpired()
    {
        return Status == BookingStatus.Expired;
    }

    public bool IsCheckedIn()
    {
        return Status == BookingStatus.CheckedIn;
    }

    public bool NeedsRefundOnCancellation()
    {
        return Status == BookingStatus.Confirmed;
    }

    public List<Guid> GetSeatIds()
    {
        return BookingSeats.Select(seat => seat.SeatId).ToList();
    }

    public void MarkCancelled(DateTime updatedAtUtc)
    {
        Status = BookingStatus.Cancelled;
        UpdatedAt = updatedAtUtc;
    }

    public void MarkConfirmed(DateTime updatedAtUtc)
    {
        Status = BookingStatus.Confirmed;
        ExpiresAt = null;
        UpdatedAt = updatedAtUtc;
    }

    public void MarkExpired(DateTime updatedAtUtc)
    {
        Status = BookingStatus.Expired;
        UpdatedAt = updatedAtUtc;
    }

    public void MarkCheckedIn(DateTime updatedAtUtc)
    {
        Status = BookingStatus.CheckedIn;
        UpdatedAt = updatedAtUtc;
    }
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


