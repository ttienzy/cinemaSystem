using Ardalis.Specification;
using Domain.Entities.SharedAggregates;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.SharedSpec
{
    public class GetAllPricingTiersSpecification : Specification<PricingTier, PricingTierInfoDto>
    {
        public GetAllPricingTiersSpecification()
        {
            Query.AsNoTracking().Select(tier => new PricingTierInfoDto
            {
                PricingTierId = tier.Id,
                TierName = tier.TierName,
                Multiplier = tier.Multiplier,
                Description = tier.ValidDays,
            });
        }
    }
}
