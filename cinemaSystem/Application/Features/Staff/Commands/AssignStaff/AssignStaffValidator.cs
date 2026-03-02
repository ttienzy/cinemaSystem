using Application.Features.Staff.Commands.AssignStaff;
using FluentValidation;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Staff.Commands.AssignStaff
{
    public class AssignStaffValidator : AbstractValidator<AssignStaffCommand>
    {
        public AssignStaffValidator()
        {
            RuleFor(x => x.Request.CinemaId)
                .NotEmpty().WithMessage("Cinema ID is required.");

            RuleFor(x => x.Request.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Request.Phone)
                .Matches(@"^\+?[0-9]{10,15}$").When(x => !string.IsNullOrEmpty(x.Request.Phone))
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.Request.Position)
                .NotEmpty().WithMessage("Position is required.");
        }
    }
}
