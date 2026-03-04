using MediatR;

namespace Application.Features.Promotions.Commands.DeletePromotion
{
    public record DeletePromotionCommand(Guid Id) : IRequest<bool>;
}
