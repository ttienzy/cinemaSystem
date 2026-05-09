using System.ComponentModel.DataAnnotations;

namespace Movie.API.Application.DTOs;

public class CreateShowtimeRequest
{
    [Required]
    public Guid MovieId { get; set; }

    [Required]
    public Guid CinemaHallId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    [Range(10000, 10000000)] // Minimum 10,000 VND (~$0.40 USD) to meet payment gateway requirements
    public decimal Price { get; set; }
}


