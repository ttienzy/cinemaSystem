using Application.Features.Concessions.Commands.CreateSale;
using FluentValidation;

namespace Application.Features.Concessions.Commands.CreateSale
{
    public class CreateConcessionSaleValidator : AbstractValidator<CreateConcessionSaleCommand>
    {
        public CreateConcessionSaleValidator()
        {
            RuleFor(x => x.CinemaId)
                .NotEmpty().WithMessage("Cinema ID is required.");

            RuleFor(x => x.StaffId)
                .NotEmpty().WithMessage("Staff ID is required.");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.InventoryItemId)
                    .NotEmpty().WithMessage("Inventory item ID is required.");
                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            });
        }
    }
}
