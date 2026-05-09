using Booking.API.Domain.Entities;

namespace Booking.API.Application.DTOs;

/// <summary>
/// DTO containing all information needed for booking emails
/// </summary>
public class BookingEmailDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;

    // Customer Information
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Movie Information
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePoster { get; set; } = string.Empty;

    // Cinema Information
    public string CinemaName { get; set; } = string.Empty;
    public string CinemaAddress { get; set; } = string.Empty;
    public string CinemaHallName { get; set; } = string.Empty;

    // Showtime Information
    public DateTime ShowtimeDate { get; set; }

    // Booking Information
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Seats Information
    public List<BookingSeatDto> BookingSeats { get; set; } = new();
}

public class BookingSeatDto
{
    public Guid Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal SeatPrice { get; set; }
}

