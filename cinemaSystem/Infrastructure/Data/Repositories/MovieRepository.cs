using Application.Interfaces.Persistences.Repo;
using Domain.Entities.MovieAggregate;
using Domain.Entities.MovieAggregate.Enum;
using Shared.Models.DataModels.MovieDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly BookingContext _context;
        public MovieRepository(BookingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IQueryable<MovieResponse> GetMovies(string? title, MovieStatus movieStatus)
        {
            var query = _context.Movies.Where(m => m.Status == movieStatus);
            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }
            var result =  query
            .OrderByDescending(m => m.ReleaseDate)
            .Select(e => new MovieResponse
            {
                Id = e.Id,
                Title = e.Title,
                DurationMinutes = e.DurationMinutes,
                ReleaseDate = e.ReleaseDate,
                PosterUrl = e.PosterUrl,
                Description = e.Description,
            }).AsQueryable();
            return result;
        }
    }
}
