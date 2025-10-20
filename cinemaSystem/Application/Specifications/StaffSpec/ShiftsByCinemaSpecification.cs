using Ardalis.Specification;
using Domain.Entities.StaffAggregate;
using Shared.Models.DataModels.StaffDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.StaffSpec
{
    public class ShiftsByCinemaSpecification : Specification<Shift, ShiftInfoResponse>
    {
        public ShiftsByCinemaSpecification(Guid cinemeId)
        {
            Query.Where(x => x.CinemaId == cinemeId)
                .AsNoTracking()
                .Select(e => new ShiftInfoResponse
                {
                    ShiftId = e.Id,
                    StartTime = e.DefaultStartTime,
                    EndTime = e.DefaultEndTime,
                });
        }
    }
}
