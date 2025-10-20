using Ardalis.Specification;
using Domain.Entities.StaffAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.StaffSpec
{
    public class StaffByIdSpecification : Specification<Staff>
    {
        public StaffByIdSpecification(Guid cinemaId)
        {
            Query.Where(s => s.CinemaId == cinemaId)
                .AsNoTracking();
        }
    }
}
