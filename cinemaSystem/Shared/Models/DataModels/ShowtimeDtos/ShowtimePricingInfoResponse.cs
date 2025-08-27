using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimePricingInfoResponse
    {
        public Guid SeatTypeId { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
