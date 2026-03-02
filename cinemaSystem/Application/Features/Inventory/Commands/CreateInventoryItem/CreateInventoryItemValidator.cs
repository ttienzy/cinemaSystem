using Application.Features.Inventory.Commands.CreateInventoryItem;
using FluentValidation;

namespace Application.Features.Inventory.Commands.CreateInventoryItem
{
    public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemCommand>
    {
        public CreateInventoryItemValidator()
        {
            RuleFor(x => x.Request.CinemaId)
                .NotEmpty().WithMessage("Cinema ID is required.");

            RuleFor(x => x.Request.ItemName)
                .NotEmpty().WithMessage("Item name is required.")
                .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters.");

            RuleFor(x => x.Request.InitialStock)
                .GreaterThanOrEqualTo(0).WithMessage("Initial stock cannot be negative.");

            RuleFor(x => x.Request.MinimumStock)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative.");

            RuleFor(x => x.Request.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than 0.");

            RuleFor(x => x.Request.CostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative.");
        }
    }
}
