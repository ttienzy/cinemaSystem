using System.ComponentModel.DataAnnotations;

namespace Movie.API.Client;

public class CreateGenreRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}


