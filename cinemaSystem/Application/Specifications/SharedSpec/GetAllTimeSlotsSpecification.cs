using Ardalis.Specification;
using Domain.Entities.SharedAggregates;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.SharedSpec
{
    public class GetAllTimeSlotsSpecification : Specification<TimeSlot,SlotInfoDto>
    {
        public GetAllTimeSlotsSpecification()
        {
            Query.AsNoTracking().Select(slot => new SlotInfoDto
            {
                SlotId = slot.Id,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                SlotName = slot.DayType,
                IsPeakTime = slot.IsActive,
            });
        }
    }
}
