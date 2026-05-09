using System.ComponentModel.DataAnnotations;

namespace Cinema.API.Application.DTOs;

public class CreateSeatRequest
{
    [Required]
    public Guid CinemaHallId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Row { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Number { get; set; }
}

public class UpdateSeatRequest
{
    [Required]
    [MaxLength(10)]
    public string Row { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Number { get; set; }
}

public class BulkCreateSeatsRequest
{
    [Required]
    public Guid CinemaHallId { get; set; }

    [Required]
    [MinLength(1)]
    public List<SeatInput> Seats { get; set; } = new();
}

public class SeatInput
{
    [Required]
    [MaxLength(10)]
    public string Row { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Number { get; set; }
}
