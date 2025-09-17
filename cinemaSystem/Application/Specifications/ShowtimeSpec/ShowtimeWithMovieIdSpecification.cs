using Ardalis.Specification;
using Domain.Entities.ShowtimeAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.ShowtimeSpec
{
    public class ShowtimeWithMovieIdSpecification : Specification<Showtime, Guid>
    {
        public ShowtimeWithMovieIdSpecification()
        {
            Query.Where(s => s.ShowDate >= DateTime.UtcNow.Date).AsNoTracking();
            Query.Select(s => s.MovieId);
        }
    }
}
