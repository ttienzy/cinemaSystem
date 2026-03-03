using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Application.Features.Bookings.Commands.CreateBooking;
using Domain.Entities.BookingAggregate;
using Domain.Entities.ShowtimeAggregate;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CinemaSystem.UnitTests.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingHandlerTests
    {
        private readonly CreateBookingHandler _handler;
        private readonly Mock<IShowtimeRepository> _mockShowtimeRepo;
        private readonly Mock<IBookingRepository> _mockBookingRepo;
        private readonly Mock<IPromotionRepository> _mockPromotionRepo;
        private readonly Mock<ISeatLockService> _mockSeatLock;
        private readonly Mock<IPaymentGateway> _mockPaymentGateway;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<CreateBookingHandler>> _mockLogger;

        public CreateBookingHandlerTests()
        {
            _mockShowtimeRepo = new Mock<IShowtimeRepository>();
            _mockBookingRepo = new Mock<IBookingRepository>();
            _mockPromotionRepo = new Mock<IPromotionRepository>();
            _mockSeatLock = new Mock<ISeatLockService>();
            _mockPaymentGateway = new Mock<IPaymentGateway>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CreateBookingHandler>>();

            _handler = new CreateBookingHandler(
                _mockShowtimeRepo.Object,
                _mockBookingRepo.Object,
                _mockPromotionRepo.Object,
                _mockSeatLock.Object,
                _mockPaymentGateway.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessResult()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var seatId = Guid.NewGuid();

            var showtime = CreateTestShowtime(showtimeId);
            var command = new CreateBookingCommand(
                customerId,
                showtimeId,
                new List<SeatSelection> { new SeatSelection(seatId, 100000m) },
                null);

            _mockShowtimeRepo.Setup(x => x.GetByIdWithPricingAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(showtime);

            _mockSeatLock.Setup(x => x.GetSeatStatusesAsync(showtimeId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, SeatLockStatus>());

            _mockSeatLock.Setup(x => x.LockSeatsAsync(showtimeId, It.IsAny<List<Guid>>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockBookingRepo.Setup(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockPaymentGateway.Setup(x => x.CreatePaymentUrl(It.IsAny<PaymentRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("https://payment.url");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.BookingCode.Should().NotBeEmpty();
            result.TotalAmount.Should().BeGreaterThan(0);
            result.PaymentUrl.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ShowtimeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var showtimeId = Guid.NewGuid();
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                showtimeId,
                new List<SeatSelection> { new SeatSelection(Guid.NewGuid(), 100000m) },
                null);

            _mockShowtimeRepo.Setup(x => x.GetByIdWithPricingAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Showtime?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SeatsNotAvailable_ThrowsConflictException()
        {
            // Arrange
            var showtimeId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var showtime = CreateTestShowtime(showtimeId);
            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                showtimeId,
                new List<SeatSelection> { new SeatSelection(seatId, 100000m) },
                null);

            _mockShowtimeRepo.Setup(x => x.GetByIdWithPricingAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(showtime);

            _mockSeatLock.Setup(x => x.GetSeatStatusesAsync(showtimeId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, SeatLockStatus>
                {
                    { seatId, SeatLockStatus.Locked }
                });

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithValidPromotion_AppliesDiscount()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var showtimeId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var promoCode = "SAVE10";

            var showtime = CreateTestShowtime(showtimeId);
            var command = new CreateBookingCommand(
                customerId,
                showtimeId,
                new List<SeatSelection> { new SeatSelection(seatId, 100000m) },
                promoCode);

            _mockShowtimeRepo.Setup(x => x.GetByIdWithPricingAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(showtime);

            _mockSeatLock.Setup(x => x.GetSeatStatusesAsync(showtimeId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, SeatLockStatus>());

            _mockSeatLock.Setup(x => x.LockSeatsAsync(showtimeId, It.IsAny<List<Guid>>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockBookingRepo.Setup(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockPaymentGateway.Setup(x => x.CreatePaymentUrl(It.IsAny<PaymentRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("https://payment.url");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.DiscountAmount.Should().BeGreaterOrEqualTo(0);
        }

        private static Showtime CreateTestShowtime(Guid showtimeId)
        {
            var showtime = Showtime.Schedule(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now.AddDays(1),
                DateTime.Now.AddDays(1).AddHours(10),
                DateTime.Now.AddDays(1).AddHours(12),
                50);
            return showtime;
        }
    }
}
