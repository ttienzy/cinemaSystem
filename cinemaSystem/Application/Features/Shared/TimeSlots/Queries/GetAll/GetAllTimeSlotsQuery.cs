using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.TimeSlots.Queries.GetAll
{
    public record GetAllTimeSlotsQuery(bool ActiveOnly = false) : IRequest<List<TimeSlotDto>>;

    public class GetAllTimeSlotsHandler(ITimeSlotRepository repo) : IRequestHandler<GetAllTimeSlotsQuery, List<TimeSlotDto>>
    {
        public async Task<List<TimeSlotDto>> Handle(GetAllTimeSlotsQuery request, CancellationToken ct)
        {
            var items = await repo.GetAllAsync(ct);

            if (request.ActiveOnly)
            {
                items = items.Where(t => t.IsActive).ToList();
            }

            return items.Select(x => new TimeSlotDto
            {
                Id = x.Id,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DayType = x.DayType,
                IsActive = x.IsActive
            }).ToList();
        }
    }
}
