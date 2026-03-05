using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.EquipmentDtos;

namespace Application.Features.Equipment.Queries
{
    /// <summary>Lấy danh sách thiết bị theo cinema.</summary>
    public record GetEquipmentListQuery(Guid? CinemaId = null, string? Status = null) : IRequest<List<EquipmentResponse>>;

    /// <summary>Lấy chi tiết thiết bị.</summary>
    public record GetEquipmentByIdQuery(Guid Id) : IRequest<EquipmentResponse?>;

    /// <summary>Lấy lịch sử bảo trì theo thiết bị.</summary>
    public record GetMaintenanceLogsQuery(Guid EquipmentId) : IRequest<List<MaintenanceLogResponse>>;

    // === Handlers ===

    public class GetEquipmentListHandler(IEquipmentRepository repo) : IRequestHandler<GetEquipmentListQuery, List<EquipmentResponse>>
    {
        public async Task<List<EquipmentResponse>> Handle(GetEquipmentListQuery request, CancellationToken ct)
        {
            var list = request.CinemaId.HasValue
                ? await repo.GetByCinemaIdAsync(request.CinemaId.Value, ct)
                : await repo.GetByCinemaIdAndStatusAsync(request.CinemaId!.Value, request.Status!, ct);

            return list.Select(e => new EquipmentResponse(
                e.Id, e.CinemaId, e.ScreenId, e.Screen?.ScreenName, e.EquipmentType,
                e.PurchaseDate, e.Status
            )).ToList();
        }
    }

    public class GetEquipmentByIdHandler(IEquipmentRepository repo) : IRequestHandler<GetEquipmentByIdQuery, EquipmentResponse?>
    {
        public async Task<EquipmentResponse?> Handle(GetEquipmentByIdQuery request, CancellationToken ct)
        {
            var e = await repo.GetByIdAsync(request.Id, ct);
            if (e == null) return null;

            return new EquipmentResponse(
                e.Id, e.CinemaId, e.ScreenId, e.Screen?.ScreenName, e.EquipmentType,
                e.PurchaseDate, e.Status
            );
        }
    }

    public class GetMaintenanceLogsHandler(IMaintenanceLogRepository repo) : IRequestHandler<GetMaintenanceLogsQuery, List<MaintenanceLogResponse>>
    {
        public async Task<List<MaintenanceLogResponse>> Handle(GetMaintenanceLogsQuery request, CancellationToken ct)
        {
            var list = await repo.GetByEquipmentIdAsync(request.EquipmentId, ct);
            return list.Select(m => new MaintenanceLogResponse(
                m.Id, m.EquipmentId, m.MaintenanceDate, m.Cost, m.IssuesFound, m.IsCompleted
            )).ToList();
        }
    }
}
