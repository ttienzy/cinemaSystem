using Ardalis.Specification;
using Domain.Entities.CinemaAggreagte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.CinemaSpec
{
    public  class CinemaScreenAndSeatsSpecification : Specification<Cinema>
    {
        public CinemaScreenAndSeatsSpecification(Guid cinemaId, Guid screenId)
        {
            Query.Where(c => c.Id == cinemaId && c.Screens.Any(sc => sc.Id == screenId))
                .Include(c => c.Screens.Where(sc => sc.Id == screenId))
                    .ThenInclude(s => s.Seats)
                    .AsSplitQuery();
        }
    }
}
