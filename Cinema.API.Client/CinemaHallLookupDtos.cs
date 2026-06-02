using System.ComponentModel.DataAnnotations;

namespace Cinema.API.Client;

public class CinemaHallLookupRequest
{
    [Required]
    public List<Guid> CinemaHallIds { get; set; } = new();
}
