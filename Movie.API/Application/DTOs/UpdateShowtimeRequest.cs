using System.ComponentModel.DataAnnotations;

namespace Movie.API.Application.DTOs;

public class UpdateShowtimeRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }
}


