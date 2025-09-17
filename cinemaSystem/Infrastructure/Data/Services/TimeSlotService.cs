using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        public TimeSlotService(IRepository<TimeSlot> timeSlotRepository)
        {
            _timeSlotRepository = timeSlotRepository ?? throw new ArgumentNullException(nameof(timeSlotRepository));
        }
        public async Task<BaseResponse<TimeSlot>> CreateTimeSlotAsync(TimeSlotRequest request)
        {
            try
            {
                var tSlot = new TimeSlot(request.StartTime, request.EndTime, request.DayType, request.IsActive);
                await _timeSlotRepository.AddAsync(tSlot);
                return BaseResponse<TimeSlot>.Success(tSlot);
            }
            catch (Exception ex)
            {
                return BaseResponse<TimeSlot>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteTimeSlotAsync(Guid timeSlotId)
        {
            try
            {
                var tSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
                if (tSlot == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound("Time slot not found."));
                }
                await _timeSlotRepository.DeleteAsync(tSlot);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<TimeSlot>> GetTimeSlotByIdAsync(Guid timeSlotId)
        {
            try
            {
                var tSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
                if (tSlot == null)
                {
                    return BaseResponse<TimeSlot>.Failure(Error.NotFound("Time slot not found."));
                }
                return BaseResponse<TimeSlot>.Success(tSlot);
            }
            catch (Exception ex)
            {
                return BaseResponse<TimeSlot>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<TimeSlot>>> GetTimeSlotsAsync()
        {
            try
            {
                var tSlots = await _timeSlotRepository.ListAsync();
                return BaseResponse<IEnumerable<TimeSlot>>.Success(tSlots);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<TimeSlot>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<TimeSlot>> UpdateTimeSlotAsync(Guid timeSlotId, TimeSlotRequest request)
        {
            try
            {
                var tSlot = await _timeSlotRepository.GetByIdAsync(timeSlotId);
                if (tSlot == null)
                {
                    return BaseResponse<TimeSlot>.Failure(Error.NotFound("Time slot not found."));
                }
                tSlot.UpdateTimeSlot(request.StartTime, request.EndTime, request.DayType, request.IsActive);
                await _timeSlotRepository.UpdateAsync(tSlot);
                return BaseResponse<TimeSlot>.Success(tSlot);
            }
            catch (Exception ex)
            {
                return BaseResponse<TimeSlot>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
