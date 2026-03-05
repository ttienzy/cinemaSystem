using Domain.Entities.EquipmentAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface IEquipmentRepository
    {
        Task<Equipment?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Equipment>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken ct = default);
        Task<IReadOnlyList<Equipment>> GetByCinemaIdAndStatusAsync(Guid cinemaId, string status, CancellationToken ct = default);
        Task AddAsync(Equipment equipment, CancellationToken ct = default);
        void Update(Equipment equipment);
        void Delete(Equipment equipment);
        IQueryable<Equipment> GetQueryable();
    }

    public interface IMaintenanceLogRepository
    {
        Task<MaintenanceLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<MaintenanceLog>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken ct = default);
        Task AddAsync(MaintenanceLog log, CancellationToken ct = default);
        void Update(MaintenanceLog log);
        IQueryable<MaintenanceLog> GetQueryable();
    }
}
