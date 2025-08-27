using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ClassificationDtos
{
    public class PricingTierRequest
    {
        public string? TierName { get; set; } 
        public decimal Multiplier { get; set; }
        public string? ValidDays { get; set; }
    }
}
