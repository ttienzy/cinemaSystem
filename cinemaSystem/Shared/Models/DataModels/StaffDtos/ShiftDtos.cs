namespace Shared.Models.DataModels.StaffDtos
{
    /// <summary>
    /// Shift DTO — used for shift list and response.
    /// </summary>
    public class ShiftDto
    {
        public Guid Id { get; set; }
        public Guid CinemaId { get; set; }
        public string? Name { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to create or update a shift.
    /// </summary>
    public class ShiftUpsertRequest
    {
        public Guid CinemaId { get; set; }
        public string? Name { get; set; }
        public TimeSpan DefaultStartTime { get; set; }
        public TimeSpan DefaultEndTime { get; set; }
    }

    /// <summary>
    /// Staff work schedule DTO.
    /// </summary>
    public class WorkScheduleDto
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public string? StaffName { get; set; }
        public Guid ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public string ShiftTime { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public DateTime? ActualCheckInTime { get; set; }
    }

    /// <summary>
    /// Request to create a work schedule for a staff member.
    /// </summary>
    public class ScheduleCreateRequest
    {
        public Guid StaffId { get; set; }
        public Guid ShiftId { get; set; }
        public DateTime WorkDate { get; set; }
    }

    /// <summary>
    /// Request to create schedules in bulk (for a whole week).
    /// </summary>
    public class BulkScheduleRequest
    {
        public Guid CinemaId { get; set; }
        public List<ScheduleCreateRequest> Schedules { get; set; } = new();
    }
}
