using System.ComponentModel.DataAnnotations;

namespace Booking.API.Application.DTOs.Requests;

public class GetShowtimeOccupancyRequest
{
    [Required]
    [MinLength(1)]
    public List<Guid> ShowtimeIds { get; set; } = new();
}
