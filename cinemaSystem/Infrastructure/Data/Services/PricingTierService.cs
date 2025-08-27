using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;

namespace Infrastructure.Data.Services
{
    public class PricingTierService : IPricingTierService
    {
        private readonly IRepository<PricingTier> _pricingTierRepository;
        public PricingTierService(IRepository<PricingTier> pricingTierRepository)
        {
            _pricingTierRepository = pricingTierRepository ?? throw new ArgumentNullException(nameof(pricingTierRepository));
        }
        public async Task<BaseResponse<PricingTier>> CreatePricingTierAsync(PricingTierRequest request)
        {
            try
            {
                var pricingTier = new PricingTier(request.TierName, request.Multiplier, request.ValidDays);
                await _pricingTierRepository.AddAsync(pricingTier);
                return BaseResponse<PricingTier>.Success(pricingTier);
            }
            catch (Exception ex)
            {
                return BaseResponse<PricingTier>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeletePricingTierAsync(Guid pricingTierId)
        {
            try
            {
                var pricingTier = await _pricingTierRepository.GetByIdAsync(pricingTierId);
                if (pricingTier == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound("Pricing tier not found."));
                }
                await _pricingTierRepository.DeleteAsync(pricingTier);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<PricingTier>> GetPricingTierByIdAsync(Guid pricingTierId)
        {
            try
            {
                var pricingTier = await _pricingTierRepository.GetByIdAsync(pricingTierId);
                return BaseResponse<PricingTier>.Success(pricingTier);
            }
            catch (Exception ex)
            {
                return BaseResponse<PricingTier>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<PricingTier>>> GetPricingTiersAsync()
        {
            try
            {
                var pricingTiers = await _pricingTierRepository.ListAsync();
                return BaseResponse<IEnumerable<PricingTier>>.Success(pricingTiers);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<PricingTier>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<PricingTier>> UpdatePricingTierAsync(Guid pricingTierId, PricingTierRequest request)
        {
            try
            {
                var pricingTier = await _pricingTierRepository.GetByIdAsync(pricingTierId);
                if (pricingTier == null)
                {
                    return BaseResponse<PricingTier>.Failure(Error.NotFound("Pricing tier not found."));
                }
                pricingTier.UpdatePricingTier(request.TierName, request.Multiplier, request.ValidDays);
                await _pricingTierRepository.UpdateAsync(pricingTier);
                return BaseResponse<PricingTier>.Success(pricingTier);
            }
            catch (Exception ex)
            {
                return BaseResponse<PricingTier>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
