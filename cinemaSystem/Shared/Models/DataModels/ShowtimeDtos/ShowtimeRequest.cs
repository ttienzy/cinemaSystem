using Domain.Entities.ShowtimeAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeRequest
    {
        public Guid MovieId { get; set; }
        public Guid CinemaId { get; set; }
        public Guid ScreenId { get; set; }
        public Guid SlotId { get; set; }
        public Guid PricingTierId { get; set; }
        public DateTime ShowDate { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }
        public ShowtimeStatus Status { get; set; }
    }
}
