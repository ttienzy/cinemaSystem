using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class MovieWithCastCrewSpeccification : Specification<Movie>
    {
        public MovieWithCastCrewSpeccification(Guid movieId)
        {
            Query.Where(mc => mc.Id == movieId)
                .Include(mcc => mcc.CastCrew);
        }
        public MovieWithCastCrewSpeccification(Guid movieId, Guid castCrewId)
        {
            Query.Where(mc => mc.Id == movieId && mc.CastCrew.Any(cc => cc.Id == castCrewId))
                .Include(mcc => mcc.CastCrew);
        }
        public MovieWithCastCrewSpeccification(Guid movieId, List<Guid> castCrewIds)
        {
            Query.Where(mc => mc.Id == movieId && mc.CastCrew.Any(cc => castCrewIds.Contains(cc.Id)))
                .Include(mcc => mcc.CastCrew);
        }
    }
}
