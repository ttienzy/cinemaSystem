using FluentValidation;

namespace Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(x => x.ShowtimeId)
                .NotEmpty().WithMessage("Showtime ID is required.");

            RuleFor(x => x.Seats)
                .NotEmpty().WithMessage("At least one seat must be selected.")
                .Must(s => s.Count <= 8).WithMessage("Cannot book more than 8 seats at once.");

            RuleForEach(x => x.Seats).ChildRules(seat =>
            {
                seat.RuleFor(s => s.SeatId).NotEmpty().WithMessage("Seat ID is required.");
                seat.RuleFor(s => s.Price).GreaterThan(0).WithMessage("Seat price must be positive.");
            });
        }
    }
}
