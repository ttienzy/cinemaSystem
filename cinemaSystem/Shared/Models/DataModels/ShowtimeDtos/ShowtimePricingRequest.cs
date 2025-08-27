using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimePricingRequest
    {
        public Guid SeatTypeId { get; set; }
        public decimal BasePrice { get; set; }
    }
}
