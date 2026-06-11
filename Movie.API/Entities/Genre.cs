
namespace Movie.API.Entities;

public class Genre
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

}


