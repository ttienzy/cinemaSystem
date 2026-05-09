using System.ComponentModel.DataAnnotations;

namespace Cinema.API.Application.DTOs;

public class CreateCinemaRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }
}


