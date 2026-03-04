using MediatR;
using Shared.Models.DataModels.PromotionDtos;

namespace Application.Features.Promotions.Commands.CreatePromotion
{
    public record CreatePromotionCommand(PromotionUpsertRequest Request) : IRequest<Guid>;
}
