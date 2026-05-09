using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Movie.API.Application.DTOs;

public class CreateMovieRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(1, 500)]
    public int Duration { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required]
    public DateTime ReleaseDate { get; set; }

    public IFormFile? PosterFile { get; set; }

    public List<Guid> GenreIds { get; set; } = new();
}


