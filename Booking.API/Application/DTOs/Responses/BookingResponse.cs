using Booking.API.Domain.Entities;

namespace Booking.API.Application.DTOs.Responses;

/// <summary>
/// Response containing booking details
/// </summary>
public class BookingResponse
{
    public Guid BookingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<BookingSeatDto> Seats { get; set; } = new();
    public ShowtimeDetailsDto? ShowtimeDetails { get; set; }

    /// <summary>
    /// Payment ID for this booking (if payment has been created)
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// Payment checkout URL (if available)
    /// </summary>
    public string? CheckoutUrl { get; set; }
}

public class BookingSeatDto
{
    public Guid SeatId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public string SeatType { get; set; } = string.Empty;
    public decimal Price { get; set; }
}


