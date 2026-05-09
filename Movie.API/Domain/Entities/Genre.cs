using Cinema.Shared.Entities;

namespace Movie.API.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}


