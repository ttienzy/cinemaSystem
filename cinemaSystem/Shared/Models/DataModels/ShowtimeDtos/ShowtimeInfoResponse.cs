using Domain.Entities.ShowtimeAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeInfoResponse
    {
        public Guid Id { get; set; }
        public DateTime ShowDate { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string ScreenName { get; set; }
    }
}
