using Application.Common.Interfaces.Persistence;
using Domain.Entities.EquipmentAggregate;
using MediatR;
using Shared.Models.DataModels.EquipmentDtos;

namespace Application.Features.Equipment.Commands
{
    // === Command Records ===

    /// <summary>Tạo thiết bị mới.</summary>
    public record CreateEquipmentCommand(
        Guid CinemaId,
        Guid? ScreenId,
        string EquipmentType,
        DateTime PurchaseDate,
        string Status
    ) : IRequest<Guid>;

    /// <summary>Cập nhật thiết bị.</summary>
    public record UpdateEquipmentCommand(
        Guid Id,
        Guid CinemaId,
        Guid? ScreenId,
        string EquipmentType,
        string Status
    ) : IRequest;

    /// <summary>Xóa thiết bị.</summary>
    public record DeleteEquipmentCommand(Guid Id) : IRequest;

    /// <summary>Tạo log bảo trì.</summary>
    public record CreateMaintenanceLogCommand(
        Guid EquipmentId,
        DateTime MaintenanceDate,
        decimal? Cost,
        string IssuesFound,
        bool IsCompleted
    ) : IRequest<Guid>;

    // === Handlers ===

    public class CreateEquipmentHandler(
        IEquipmentRepository repo, IUnitOfWork uow)
        : IRequestHandler<CreateEquipmentCommand, Guid>
    {
        public async Task<Guid> Handle(CreateEquipmentCommand request, CancellationToken ct)
        {
            var equipment = Domain.Entities.EquipmentAggregate.Equipment.Create(
                request.CinemaId,
                request.ScreenId,
                request.EquipmentType,
                request.PurchaseDate,
                request.Status
            );
            await repo.AddAsync(equipment, ct);
            await uow.SaveChangesAsync(ct);
            return equipment.Id;
        }
    }

    public class UpdateEquipmentHandler(
        IEquipmentRepository repo, IUnitOfWork uow)
        : IRequestHandler<UpdateEquipmentCommand>
    {
        public async Task Handle(UpdateEquipmentCommand request, CancellationToken ct)
        {
            var equipment = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy thiết bị ID: {request.Id}");

            equipment.Update(request.CinemaId, request.ScreenId, request.EquipmentType, request.Status);

            repo.Update(equipment);
            await uow.SaveChangesAsync(ct);
        }
    }

    public class DeleteEquipmentHandler(
        IEquipmentRepository repo, IUnitOfWork uow)
        : IRequestHandler<DeleteEquipmentCommand>
    {
        public async Task Handle(DeleteEquipmentCommand request, CancellationToken ct)
        {
            var equipment = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy thiết bị ID: {request.Id}");

            repo.Delete(equipment);
            await uow.SaveChangesAsync(ct);
        }
    }

    public class CreateMaintenanceLogHandler(
        IMaintenanceLogRepository logRepo,
        IEquipmentRepository equipRepo,
        IUnitOfWork uow)
        : IRequestHandler<CreateMaintenanceLogCommand, Guid>
    {
        public async Task<Guid> Handle(CreateMaintenanceLogCommand request, CancellationToken ct)
        {
            var equipment = await equipRepo.GetByIdAsync(request.EquipmentId, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy thiết bị ID: {request.EquipmentId}");

            var log = MaintenanceLog.Create(
                request.EquipmentId,
                request.MaintenanceDate,
                request.Cost,
                request.IssuesFound,
                request.IsCompleted
            );

            await logRepo.AddAsync(log, ct);
            await uow.SaveChangesAsync(ct);
            return log.Id;
        }
    }
}
