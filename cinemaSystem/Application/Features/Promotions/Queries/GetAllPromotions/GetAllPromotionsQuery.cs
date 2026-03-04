using Application.Common.Interfaces.Persistence;
using MediatR;
using Domain.Entities.PromotionAggregate;
using Shared.Models.DataModels.PromotionDtos;

namespace Application.Features.Promotions.Queries.GetAllPromotions
{
    public record GetAllPromotionsQuery(bool IncludeInactive = false) : IRequest<List<PromotionResponse>>;

    public class GetAllPromotionsHandler(IPromotionRepository promotionRepo)
        : IRequestHandler<GetAllPromotionsQuery, List<PromotionResponse>>
    {
        public async Task<List<PromotionResponse>> Handle(GetAllPromotionsQuery request, CancellationToken ct)
        {
            var promos = await promotionRepo.GetAllAsync(request.IncludeInactive, ct);
            return promos.Select(MapToResponse).ToList();
        }

        private static PromotionResponse MapToResponse(Promotion p)
        {
            return new PromotionResponse
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Type = p.Type == PromotionType.Percentage ? "Percentage" : "FixedAmount",
                Value = p.Value,
                MaxDiscountAmount = p.MaxDiscountAmount,
                MinOrderValue = p.MinOrderValue,
                MaxUsageCount = p.MaxUsageCount,
                CurrentUsageCount = p.CurrentUsageCount,
                MaxUsagePerUser = p.MaxUsagePerUser,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                SpecificMovieId = p.SpecificMovieId,
                SpecificCinemaId = p.SpecificCinemaId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }
}
