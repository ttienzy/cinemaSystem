using Domain.Entities.ShowtimeAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public Guid CinemaId { get; set; }
        public Guid ScreendId { get; set; }
        public Guid SlotId { get; set; }
        public Guid PricingTierId { get; set; }
        public DateTime ShowDate { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }
        public string MovieTitle { get; set; }
        public string MoviePosterUrl { get; set; }
        public int MovieDurationMinutes { get; set; }
        public string ScreenName { get; set; }
    }
}
