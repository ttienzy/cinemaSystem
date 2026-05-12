namespace Movie.API.Application.Mappers
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