using Application.Features.Cinemas.Commands.CreateCinema;
using FluentValidation;

namespace Application.Features.Cinemas.Commands.CreateCinema
{
    public class CreateCinemaValidator : AbstractValidator<CreateCinemaCommand>
    {
        public CreateCinemaValidator()
        {
            RuleFor(x => x.Request.CinemaName)
                .NotEmpty().WithMessage("Cinema name is required.")
                .MaximumLength(200).WithMessage("Cinema name cannot exceed 200 characters.");

            RuleFor(x => x.Request.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

            RuleFor(x => x.Request.ManagerName)
                .NotEmpty().WithMessage("Manager name is required.");

            RuleFor(x => x.Request.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.Email))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.Request.Phone)
                .Matches(@"^\+?[0-9]{10,15}$").When(x => !string.IsNullOrEmpty(x.Request.Phone))
                .WithMessage("Invalid phone number format.");
        }
    }
}
