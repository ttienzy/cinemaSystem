using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository lịch làm — truy xuất lịch làm nhân viên theo staff/cinema/tuần.
    /// </summary>
    public class WorkScheduleRepository(BookingContext context) : IWorkScheduleRepository
    {
        public async Task<WorkSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Set<WorkSchedule>()
                .Include(ws => ws.Staff)
                .Include(ws => ws.Shift)
                .FirstOrDefaultAsync(ws => ws.Id == id, ct);

        public async Task<List<WorkSchedule>> GetByStaffAsync(
            Guid staffId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
        {
            var query = context.Set<WorkSchedule>()
                .Include(ws => ws.Shift)
                .Where(ws => ws.StaffId == staffId);

            if (fromDate.HasValue) query = query.Where(ws => ws.WorkDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(ws => ws.WorkDate <= toDate.Value);

            return await query.OrderBy(ws => ws.WorkDate).ToListAsync(ct);
        }

        /// <summary>
        /// Lấy lịch làm 1 tuần tại rạp — join Staff và Shift để hiển thị bảng lịch.
        /// </summary>
        public async Task<List<WorkSchedule>> GetByCinemaAndWeekAsync(
            Guid cinemaId, DateTime weekStart, CancellationToken ct = default)
        {
            var weekEnd = weekStart.AddDays(7);
            return await context.Set<WorkSchedule>()
                .Include(ws => ws.Staff)
                .Include(ws => ws.Shift)
                .Where(ws => ws.Shift.CinemaId == cinemaId
                    && ws.WorkDate >= weekStart && ws.WorkDate < weekEnd)
                .OrderBy(ws => ws.WorkDate)
                .ThenBy(ws => ws.Shift.DefaultStartTime)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Kiểm tra xung đột — 1 nhân viên không thể làm 2 ca cùng ngày.
        /// </summary>
        public async Task<bool> HasConflictAsync(
            Guid staffId, DateTime workDate, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = context.Set<WorkSchedule>()
                .Where(ws => ws.StaffId == staffId && ws.WorkDate.Date == workDate.Date);

            if (excludeId.HasValue)
                query = query.Where(ws => ws.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(WorkSchedule schedule, CancellationToken ct = default)
            => await context.Set<WorkSchedule>().AddAsync(schedule, ct);

        public async Task AddRangeAsync(List<WorkSchedule> schedules, CancellationToken ct = default)
            => await context.Set<WorkSchedule>().AddRangeAsync(schedules, ct);

        public void Update(WorkSchedule schedule)
            => context.Set<WorkSchedule>().Update(schedule);

        public void Delete(WorkSchedule schedule)
            => context.Set<WorkSchedule>().Remove(schedule);

        public IQueryable<WorkSchedule> GetQueryable()
            => context.Set<WorkSchedule>().AsQueryable();
    }
}
