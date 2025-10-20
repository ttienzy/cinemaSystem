using Ardalis.Specification;
using Domain.Entities.StaffAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.StaffSpec
{
    public class GetShiftByCinemaIdSpecifcation : Specification<Shift>
    {
        public GetShiftByCinemaIdSpecifcation(Guid cinemaId)
        {
            Query.AsNoTracking()
                .Where(s => s.CinemaId == cinemaId);
        }
    }
}
