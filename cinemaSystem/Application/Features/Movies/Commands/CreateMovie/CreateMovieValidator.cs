using Application.Features.Movies.Commands.CreateMovie;
using FluentValidation;

namespace Application.Features.Movies.Commands.CreateMovie
{
    public class CreateMovieValidator : AbstractValidator<CreateMovieCommand>
    {
        public CreateMovieValidator()
        {
            RuleFor(x => x.Request.Title)
                .NotEmpty().WithMessage("Movie title is required.")
                .MaximumLength(300).WithMessage("Title cannot exceed 300 characters.");

            RuleFor(x => x.Request.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0.")
                .LessThanOrEqualTo(600).WithMessage("Duration cannot exceed 600 minutes.");

            RuleFor(x => x.Request.ReleaseDate)
                .NotEmpty().WithMessage("Release date is required.");

            RuleFor(x => x.Request.Description)
                .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters.");

            RuleFor(x => x.Request.PosterUrl)
                .MaximumLength(500).WithMessage("Poster URL cannot exceed 500 characters.");

            RuleFor(x => x.Request.Trailer)
                .MaximumLength(500).WithMessage("Trailer URL cannot exceed 500 characters.");
        }
    }
}
