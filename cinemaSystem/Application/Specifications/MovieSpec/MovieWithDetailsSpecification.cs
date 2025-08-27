using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class MovieWithDetailsSpecification : Specification<Movie>
    {
        public MovieWithDetailsSpecification(Guid movieId)
        {
            Query
                .Where(movie => movie.Id == movieId)
                .Include(movie => movie.Copyrights)
                .Include(movie => movie.CastCrew)
                .Include(movie => movie.Certifications)
                .Include(movie => movie.MovieGenres)
                .AsNoTracking();
        }
    }
}
