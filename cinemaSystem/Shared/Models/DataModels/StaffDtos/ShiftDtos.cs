namespace Shared.Models.DataModels.StaffDtos
{
    /// <summary>
    /// DTO ca làm — dùng cho danh sách ca và response.
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
    /// Request tạo/cập nhật ca làm.
    /// </summary>
    public class ShiftUpsertRequest
    {
        public Guid CinemaId { get; set; }
        public string? Name { get; set; }
        public TimeSpan DefaultStartTime { get; set; }
        public TimeSpan DefaultEndTime { get; set; }
    }

    /// <summary>
    /// DTO lịch làm nhân viên.
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
    /// Request tạo lịch làm cho nhân viên.
    /// </summary>
    public class ScheduleCreateRequest
    {
        public Guid StaffId { get; set; }
        public Guid ShiftId { get; set; }
        public DateTime WorkDate { get; set; }
    }

    /// <summary>
    /// Request tạo lịch làm hàng loạt (cả tuần).
    /// </summary>
    public class BulkScheduleRequest
    {
        public Guid CinemaId { get; set; }
        public List<ScheduleCreateRequest> Schedules { get; set; } = new();
    }
}
