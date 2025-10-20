using Ardalis.Specification;
using Domain.Entities.StaffAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.StaffSpec
{
    public class StaffsByCinemaIdSpecification : Specification<Staff>
    {
        public StaffsByCinemaIdSpecification(Guid cinemaId)
        {
            Query.AsNoTracking()
                .Where(s => s.CinemaId == cinemaId);
        }
    }
}
