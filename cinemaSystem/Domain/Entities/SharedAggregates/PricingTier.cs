using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.SharedAggregates
{
    public class PricingTier : BaseEntity, IAggregateRoot
    {
        public string? TierName { get; private set; } // e.g., Standard, Peak, Holiday, Student
        public decimal Multiplier { get; private set; }
        public string? ValidDays { get; private set; }
        public PricingTier()
        {
            Id = Guid.NewGuid();
        }
        public PricingTier(string tierName, decimal multiplier, string? validDays = null)
        {
            Id = Guid.NewGuid();
            TierName = tierName;
            Multiplier = multiplier;
            ValidDays = validDays;
        }
        public void UpdatePricingTier(string tierName, decimal multiplier, string? validDays = null)
        {
            TierName = tierName;
            Multiplier = multiplier;
            ValidDays = validDays;
        }

    }
}
