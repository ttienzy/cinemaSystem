using FluentValidation;

namespace Application.Features.Showtimes.Commands.CreateShowtime
{
    public class CreateShowtimeValidator : AbstractValidator<CreateShowtimeCommand>
    {
        public CreateShowtimeValidator()
        {
            RuleFor(x => x.CinemaId).NotEmpty();
            RuleFor(x => x.MovieId).NotEmpty();
            RuleFor(x => x.ScreenId).NotEmpty();
            RuleFor(x => x.SlotId).NotEmpty();
            RuleFor(x => x.PricingTierId).NotEmpty();
            RuleFor(x => x.ShowDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Show date cannot be in the past.");
            RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time.");
            RuleFor(x => x.TotalSeats).GreaterThan(0)
                .WithMessage("Total seats must be positive.");
            RuleFor(x => x.Pricings).NotEmpty()
                .WithMessage("At least one pricing entry is required.");
        }
    }
}
