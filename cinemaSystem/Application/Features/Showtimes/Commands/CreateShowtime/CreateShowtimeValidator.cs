using FluentValidation;
using System;

namespace Application.Features.Showtimes.Commands.CreateShowtime
{
    public class CreateShowtimeValidator : AbstractValidator<CreateShowtimeCommand>
    {
        public CreateShowtimeValidator()
        {
            RuleFor(x => x.Request.CinemaId).NotEmpty();
            RuleFor(x => x.Request.MovieId).NotEmpty();
            RuleFor(x => x.Request.ScreenId).NotEmpty();
            RuleFor(x => x.Request.SlotId).NotEmpty();
            RuleFor(x => x.Request.PricingTierId).NotEmpty();
            
            RuleFor(x => x.Request.ShowDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Show date cannot be in the past.");

            RuleFor(x => x.Request.ActualEndTime).GreaterThan(x => x.Request.ActualStartTime)
                .WithMessage("Actual end time must be after start time.");

            RuleFor(x => x.Request.ShowtimePricings).NotEmpty()
                .WithMessage("At least one pricing entry is required.");
        }
    }
}
