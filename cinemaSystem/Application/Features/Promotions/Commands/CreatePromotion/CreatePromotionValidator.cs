using FluentValidation;

namespace Application.Features.Promotions.Commands.CreatePromotion
{
    public class CreatePromotionValidator : AbstractValidator<CreatePromotionCommand>
    {
        public CreatePromotionValidator()
        {
            RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.Type)
                .Must(t => t is "Percentage" or "FixedAmount")
                .WithMessage("Type must be 'Percentage' or 'FixedAmount'.");
            RuleFor(x => x.Request.Value).GreaterThan(0);
            RuleFor(x => x.Request.EndDate).GreaterThan(x => x.Request.StartDate)
                .WithMessage("End date must be after start date.");
        }
    }
}
