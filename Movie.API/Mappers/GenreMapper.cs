using Movie.API.Client;
using Movie.API.Entities;

namespace Movie.API.Mappers
{
    public static class GenreMapper
    {
        public static GenreDto GenreMapToDto(this Genre genre)
        {
            return new GenreDto
            {
                Id = genre.Id,
                Name = genre.Name
            };
        }
    }
}