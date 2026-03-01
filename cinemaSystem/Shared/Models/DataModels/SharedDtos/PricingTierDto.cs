using System;

namespace Shared.Models.DataModels.SharedDtos
{
    public class PricingTierDto
    {
        public Guid Id { get; set; }
        public string? TierName { get; set; }
        public decimal Multiplier { get; set; }
        public string? ValidDays { get; set; }
    }
}
