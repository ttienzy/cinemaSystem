using Domain.Entities.StaffAggregate;

namespace Application.Common.Interfaces.Persistence
{
    /// <summary>
    /// Repository lịch làm nhân viên (WorkSchedule).
    /// Quản lý phân công nhân viên vào ca làm theo ngày.
    /// </summary>
    public interface IWorkScheduleRepository
    {
        Task<WorkSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<WorkSchedule>> GetByStaffAsync(Guid staffId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
        Task<List<WorkSchedule>> GetByCinemaAndWeekAsync(Guid cinemaId, DateTime weekStart, CancellationToken ct = default);
        Task<bool> HasConflictAsync(Guid staffId, DateTime workDate, Guid? excludeId = null, CancellationToken ct = default);
        Task AddAsync(WorkSchedule schedule, CancellationToken ct = default);
        Task AddRangeAsync(List<WorkSchedule> schedules, CancellationToken ct = default);
        void Update(WorkSchedule schedule);
        void Delete(WorkSchedule schedule);
        IQueryable<WorkSchedule> GetQueryable();
    }
}
