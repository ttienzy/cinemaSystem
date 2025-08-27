using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface IPricingTierService
    {
        Task<BaseResponse<IEnumerable<PricingTier>>> GetPricingTiersAsync();
        Task<BaseResponse<PricingTier>> GetPricingTierByIdAsync(Guid pricingTierId);
        Task<BaseResponse<PricingTier>> CreatePricingTierAsync(PricingTierRequest request);
        Task<BaseResponse<PricingTier>> UpdatePricingTierAsync(Guid pricingTierId, PricingTierRequest request);
        Task<BaseResponse<object>> DeletePricingTierAsync(Guid pricingTierId);
    }
}
