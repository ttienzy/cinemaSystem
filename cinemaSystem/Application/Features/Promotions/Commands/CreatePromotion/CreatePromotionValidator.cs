using FluentValidation;

namespace Application.Features.Promotions.Commands.CreatePromotion
{
    public class CreatePromotionValidator : AbstractValidator<CreatePromotionCommand>
    {
        public CreatePromotionValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Type)
                .Must(t => t is "Percentage" or "FixedAmount")
                .WithMessage("Type must be 'Percentage' or 'FixedAmount'.");
            RuleFor(x => x.Value).GreaterThan(0);
            RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after start date.");
        }
    }
}
