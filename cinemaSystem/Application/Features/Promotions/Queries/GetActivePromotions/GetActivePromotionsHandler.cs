using Application.Common.Interfaces.Persistence;
using MediatR;
using Domain.Entities.PromotionAggregate;

namespace Application.Features.Promotions.Queries.GetActivePromotions
{
    public record GetActivePromotionsQuery() : IRequest<List<PromotionDto>>;

    public record PromotionDto(
        Guid Id,
        string Code,
        string Name,
        string? Description,
        PromotionType Type,
        decimal Value,
        decimal? MaxDiscountAmount,
        decimal? MinOrderValue,
        DateTime StartDate,
        DateTime EndDate);

    public class GetActivePromotionsHandler(IPromotionRepository promotionRepo) 
        : IRequestHandler<GetActivePromotionsQuery, List<PromotionDto>>
    {
        public async Task<List<PromotionDto>> Handle(GetActivePromotionsQuery request, CancellationToken ct)
        {
            var promos = await promotionRepo.GetActiveAsync(ct);
            return promos.Select(p => new PromotionDto(
                p.Id,
                p.Code,
                p.Name,
                p.Description,
                p.Type,
                p.Value,
                p.MaxDiscountAmount,
                p.MinOrderValue,
                p.StartDate,
                p.EndDate)).ToList();
        }
    }
}
