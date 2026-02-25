using Application.Common.Interfaces.Persistence;
using Application.Common.Exceptions;
using MediatR;
using Domain.Entities.PromotionAggregate;

namespace Application.Features.Promotions.Queries.ValidatePromotion
{
    public record ValidatePromotionQuery(string Code, decimal OrderTotal) : IRequest<ValidationResult>;

    public record ValidationResult(
        bool IsValid, 
        decimal DiscountAmount, 
        string Message);

    public class ValidatePromotionHandler(IPromotionRepository promotionRepo) 
        : IRequestHandler<ValidatePromotionQuery, ValidationResult>
    {
        public async Task<ValidationResult> Handle(ValidatePromotionQuery request, CancellationToken ct)
        {
            var promo = await promotionRepo.GetByCodeAsync(request.Code, ct);
            
            if (promo == null)
                return new ValidationResult(false, 0, "Promotion code not found.");

            var (canApply, discount, reason) = promo.Evaluate(request.OrderTotal);
            
            return new ValidationResult(canApply, discount, reason);
        }
    }
}
