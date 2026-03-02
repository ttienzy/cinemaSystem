using Application.Features.Showtimes.Commands.CreateShowtime;
using FluentValidation;

namespace Application.Features.Showtimes.Commands.CreateShowtime
{
    public class CreateShowtimeValidator : AbstractValidator<CreateShowtimeCommand>
    {
        public CreateShowtimeValidator()
        {
            RuleFor(x => x.Request.CinemaId)
                .NotEmpty().WithMessage("Cinema ID is required.");

            RuleFor(x => x.Request.MovieId)
                .NotEmpty().WithMessage("Movie ID is required.");

            RuleFor(x => x.Request.ScreenId)
                .NotEmpty().WithMessage("Screen ID is required.");

            RuleFor(x => x.Request.SlotId)
                .NotEmpty().WithMessage("Time slot ID is required.");

            RuleFor(x => x.Request.PricingTierId)
                .NotEmpty().WithMessage("Pricing tier ID is required.");

            RuleFor(x => x.Request.ShowDate)
                .NotEmpty().WithMessage("Show date is required.")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Show date cannot be in the past.");

            RuleFor(x => x.Request.ActualStartTime)
                .NotEmpty().WithMessage("Start time is required.");

            RuleFor(x => x.Request.ActualEndTime)
                .NotEmpty().WithMessage("End time is required.")
                .GreaterThan(x => x.Request.ActualStartTime).WithMessage("End time must be after start time.");

            RuleFor(x => x.Request.ShowtimePricings)
                .NotEmpty().WithMessage("At least one pricing is required.");
        }
    }
}
