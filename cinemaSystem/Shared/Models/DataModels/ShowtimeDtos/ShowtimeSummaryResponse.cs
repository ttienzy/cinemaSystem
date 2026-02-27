using Domain.Entities.ShowtimeAggregate.Enum;
using System;
using System.Collections.Generic;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeSummaryResponse
    {
        public Guid Id { get; set; }
        public string MovieTitle { get; set; }
        public string ScreenName { get; set; }
        public DateTime ShowDate { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }
        public ShowtimeStatus Status { get; set; }
    }
}
