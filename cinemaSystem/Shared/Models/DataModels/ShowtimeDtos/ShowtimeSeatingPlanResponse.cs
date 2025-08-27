using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeSeatingPlanResponse
    {
        public ShowtimeInfoResponse ShowtimeInfo { get; set; }
        public List<ShowtimePricingInfoResponse> Pricings { get; set; }
        public List<SeatInfoResponse> Seats { get; set; }
    }
}
