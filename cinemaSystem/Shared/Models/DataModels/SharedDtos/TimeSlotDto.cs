using System;

namespace Shared.Models.DataModels.SharedDtos
{
    public class TimeSlotDto
    {
        public Guid Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DayType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
