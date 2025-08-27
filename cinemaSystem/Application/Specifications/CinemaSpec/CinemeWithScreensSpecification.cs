using Ardalis.Specification;
using Domain.Entities.CinemaAggreagte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.CinemaSpec
{
    public class CinemeWithScreensSpecification : Specification<Cinema>
    {
        public CinemeWithScreensSpecification(Guid cinemaId)
        {
            Query.Where(c => c.Id == cinemaId)
                .Include(sc => sc.Screens);
        }
        public CinemeWithScreensSpecification(Guid cinemaId, Guid screenId)
        {
            Query.Where(c => c.Id == cinemaId && c.Screens.Any(sc => sc.Id == screenId))
                .Include(sc => sc.Screens);
        }

    }
}
