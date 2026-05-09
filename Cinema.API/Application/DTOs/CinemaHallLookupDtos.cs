using System.ComponentModel.DataAnnotations;

namespace Cinema.API.Application.DTOs;

public class CinemaHallLookupRequest
{
    [Required]
    public List<Guid> CinemaHallIds { get; set; } = new();
}
