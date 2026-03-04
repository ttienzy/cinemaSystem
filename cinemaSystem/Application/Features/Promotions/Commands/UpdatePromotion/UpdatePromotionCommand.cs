using MediatR;
using Shared.Models.DataModels.PromotionDtos;

namespace Application.Features.Promotions.Commands.UpdatePromotion
{
    public record UpdatePromotionCommand(
        Guid Id,
        PromotionUpsertRequest Request) : IRequest<PromotionResponse>;
}
