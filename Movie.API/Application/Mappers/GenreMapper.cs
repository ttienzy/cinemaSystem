namespace Movie.API.Application.Mappers
{
    public static class GenreMapper
    {
        public static GenreDto MapToDto(this Genre genre)
        {
            return new GenreDto
            {
                Id = genre.Id,
                Name = genre.Name
            };
        }
    }
}