using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.PricingTiers.Queries.GetAll
{
    public record GetAllPricingTiersQuery() : IRequest<List<PricingTierDto>>;

    public class GetAllPricingTiersHandler(IPricingTierRepository repo) : IRequestHandler<GetAllPricingTiersQuery, List<PricingTierDto>>
    {
        public async Task<List<PricingTierDto>> Handle(GetAllPricingTiersQuery request, CancellationToken ct)
        {
            var items = await repo.GetAllAsync(ct);

            return items.Select(x => new PricingTierDto
            {
                Id = x.Id,
                TierName = x.TierName,
                Multiplier = x.Multiplier,
                ValidDays = x.ValidDays
            }).ToList();
        }
    }
}
