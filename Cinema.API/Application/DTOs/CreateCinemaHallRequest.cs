using System.ComponentModel.DataAnnotations;

namespace Cinema.API.Application.DTOs;

public class CreateCinemaHallRequest
{
    [Required]
    public Guid CinemaId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateCinemaHallRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
