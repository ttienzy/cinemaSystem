using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Shifts.Queries.GetShiftsByCinema
{
    /// <summary>
    /// Lấy danh sách ca làm theo rạp — dùng cho dropdown xếp lịch.
    /// </summary>
    public record GetShiftsByCinemaQuery(Guid CinemaId) : IRequest<List<ShiftDto>>;

    public class GetShiftsByCinemaHandler(IShiftRepository shiftRepo)
        : IRequestHandler<GetShiftsByCinemaQuery, List<ShiftDto>>
    {
        public async Task<List<ShiftDto>> Handle(GetShiftsByCinemaQuery request, CancellationToken ct)
        {
            var shifts = await shiftRepo.GetByCinemaAsync(request.CinemaId, ct);

            return shifts.Select(s => new ShiftDto
            {
                Id = s.Id,
                CinemaId = s.CinemaId,
                Name = s.Name,
                StartTime = s.DefaultStartTime.ToString(@"hh\:mm"),
                EndTime = s.DefaultEndTime.ToString(@"hh\:mm")
            }).ToList();
        }
    }
}
