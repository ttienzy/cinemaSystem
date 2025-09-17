using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class MovieWithMGenreSpecification : Specification<Movie>
    {
        public MovieWithMGenreSpecification()
        {
            Query
                .Skip(0)
                .Take(3)
                .Include(mg => mg.MovieGenres)
                .OrderByDescending(m => m.CreatedAt)
                .AsNoTracking();
        }
        public MovieWithMGenreSpecification(Guid movieId)
        {
            Query.Where(m => m.Id == movieId)
                .Include(mg => mg.MovieGenres);
        }
        public MovieWithMGenreSpecification(List<Guid> movieIds)
        {
            Query.Where(m => movieIds.Contains(m.Id))
                .Include(mg => mg.MovieGenres)
                .Skip(0)
                .Take(6)
                .AsNoTracking();
        }
        public MovieWithMGenreSpecification(Guid movieId, Guid genreId)
        {
            Query.Where(m => m.Id == movieId && m.MovieGenres.Any(mg => mg.GenreId == genreId))
                .Include(mg => mg.MovieGenres);
        }
        public MovieWithMGenreSpecification(Guid movieId, List<Guid> genreIds)
        {
            Query.Where(m => m.Id == movieId && m.MovieGenres.Any(mg => genreIds.Contains(mg.GenreId)))
                .Include(mg => mg.MovieGenres);
        }
    }
}
