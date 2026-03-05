using Application.Common.Interfaces.Persistence;
using Domain.Entities.EquipmentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly BookingContext _context;

        public EquipmentRepository(BookingContext context)
        {
            _context = context;
        }

        public async Task<Equipment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Equipments
                .Include(e => e.Screen)
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<IReadOnlyList<Equipment>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken ct = default)
        {
            return await _context.Equipments
                .Include(e => e.Screen)
                .Where(e => e.CinemaId == cinemaId)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Equipment>> GetByCinemaIdAndStatusAsync(Guid cinemaId, string status, CancellationToken ct = default)
        {
            return await _context.Equipments
                .Include(e => e.Screen)
                .Where(e => e.CinemaId == cinemaId && e.Status == status)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Equipment equipment, CancellationToken ct = default)
        {
            await _context.Equipments.AddAsync(equipment, ct);
        }

        public void Update(Equipment equipment)
        {
            _context.Equipments.Update(equipment);
        }

        public void Delete(Equipment equipment)
        {
            _context.Equipments.Remove(equipment);
        }

        public IQueryable<Equipment> GetQueryable()
        {
            return _context.Equipments.Include(e => e.Screen);
        }
    }

    public class MaintenanceLogRepository : IMaintenanceLogRepository
    {
        private readonly BookingContext _context;

        public MaintenanceLogRepository(BookingContext context)
        {
            _context = context;
        }

        public async Task<MaintenanceLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.MaintenanceLogs.FindAsync(new object[] { id }, ct);
        }

        public async Task<IReadOnlyList<MaintenanceLog>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken ct = default)
        {
            return await _context.MaintenanceLogs
                .Where(m => m.EquipmentId == equipmentId)
                .OrderByDescending(m => m.MaintenanceDate)
                .ToListAsync(ct);
        }

        public async Task AddAsync(MaintenanceLog log, CancellationToken ct = default)
        {
            await _context.MaintenanceLogs.AddAsync(log, ct);
        }

        public void Update(MaintenanceLog log)
        {
            _context.MaintenanceLogs.Update(log);
        }

        public IQueryable<MaintenanceLog> GetQueryable()
        {
            return _context.MaintenanceLogs;
        }
    }
}
