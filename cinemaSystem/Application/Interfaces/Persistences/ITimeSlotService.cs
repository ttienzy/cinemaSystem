using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface ITimeSlotService
    {
        Task<BaseResponse<IEnumerable<TimeSlot>>> GetTimeSlotsAsync();
        Task<BaseResponse<TimeSlot>> GetTimeSlotByIdAsync(Guid timeSlotId);
        Task<BaseResponse<TimeSlot>> CreateTimeSlotAsync(TimeSlotRequest request);
        Task<BaseResponse<TimeSlot>> UpdateTimeSlotAsync(Guid timeSlotId, TimeSlotRequest request);
        Task<BaseResponse<object>> DeleteTimeSlotAsync(Guid timeSlotId);
    }
}
