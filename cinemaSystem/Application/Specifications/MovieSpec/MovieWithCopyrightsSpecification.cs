using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class MovieWithCopyrightsSpecification : Specification<Movie>
    {
        public MovieWithCopyrightsSpecification(Guid movieId)
        {
            Query.Where(mc => mc.Id == movieId)
                .Include(mcr => mcr.Copyrights);
        }
        public MovieWithCopyrightsSpecification(Guid movieId, Guid copyrightId)
        {
            Query.Where(mc => mc.Id == movieId && mc.Copyrights.Any(c => c.Id == copyrightId))
                .Include(mcr => mcr.Copyrights);
        }
        public MovieWithCopyrightsSpecification(Guid movieId, List<Guid> copyrightIds)
        {
            Query.Where(mc => mc.Id == movieId && mc.Copyrights.Any(c => copyrightIds.Contains(c.Id)))
                .Include(mcr => mcr.Copyrights);
        }
    }
}
