using Booking.API.Domain.Entities;
using Booking.API.Application.DTOs.External;

namespace Booking.API.Application.DTOs.Responses;

public class TicketOperationResponse
{
    public Guid BookingId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public BookingStatus BookingStatus { get; set; }
    public PaymentLookupStatus? PaymentStatus { get; set; }
    public string OperationalStatus { get; set; } = string.Empty;
    public bool CanCheckIn { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public List<BookingSeatDto> Seats { get; set; } = new();
    public ShowtimeDetailsDto? ShowtimeDetails { get; set; }
}
