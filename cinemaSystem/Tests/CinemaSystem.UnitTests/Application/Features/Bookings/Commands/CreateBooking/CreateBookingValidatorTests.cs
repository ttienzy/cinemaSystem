using Application.Features.Bookings.Commands.CreateBooking;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CinemaSystem.UnitTests.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingValidatorTests
    {
        private readonly CreateBookingValidator _validator;

        public CreateBookingValidatorTests()
        {
            _validator = new CreateBookingValidator();
        }

        [Fact]
        public void Validate_ValidCommand_PassesValidation()
        {
            // Arrange
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new List<SeatSelection> { new SeatSelection(Guid.NewGuid(), 100000m) },
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public void Validate_EmptyCustomerId_FailsValidation(string customerId)
        {
            // Arrange
            var command = new CreateBookingCommand(
                Guid.Parse(customerId),
                Guid.NewGuid(),
                new List<SeatSelection> { new SeatSelection(Guid.NewGuid(), 100000m) },
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public void Validate_EmptyShowtimeId_FailsValidation(string showtimeId)
        {
            // Arrange
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.Parse(showtimeId),
                new List<SeatSelection> { new SeatSelection(Guid.NewGuid(), 100000m) },
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ShowtimeId);
        }

        [Fact]
        public void Validate_EmptySeats_FailsValidation()
        {
            // Arrange
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new List<SeatSelection>(),
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Seats);
        }

        [Fact]
        public void Validate_MoreThan8Seats_FailsValidation()
        {
            // Arrange
            var seats = Enumerable.Range(1, 9)
                .Select(i => new SeatSelection(Guid.NewGuid(), 100000m))
                .ToList();

            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                seats,
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Seats);
        }

        [Fact]
        public void Validate_EmptySeatId_FailsValidation()
        {
            // Arrange
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new List<SeatSelection> { new SeatSelection(Guid.Empty, 100000m) },
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor("Seats[0].SeatId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Validate_NegativeOrZeroPrice_FailsValidation(decimal price)
        {
            // Arrange
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new List<SeatSelection> { new SeatSelection(Guid.NewGuid(), price) },
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor("Seats[0].Price");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000)]
        public void Validate_ValidPrice_PassesValidation(decimal price)
        {
            // Arrange
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new List<SeatSelection> { new SeatSelection(Guid.NewGuid(), price) },
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor("Seats[0].Price");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        public void Validate_ValidSeatCount_PassesValidation(int seatCount)
        {
            // Arrange
            var seats = Enumerable.Range(1, seatCount)
                .Select(i => new SeatSelection(Guid.NewGuid(), 100000m))
                .ToList();

            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                seats,
                null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Seats);
        }
    }
}
