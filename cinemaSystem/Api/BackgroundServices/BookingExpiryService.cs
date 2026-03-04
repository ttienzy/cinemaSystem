using Application.Features.Bookings.Commands.CancelExpiredBookings;
using MediatR;

namespace Api.BackgroundServices
{
    /// <summary>
    /// Background service that runs periodically to cancel expired bookings.
    /// Runs every minute to check and cancel unpaid bookings.
    /// </summary>
    public class BookingExpiryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingExpiryService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public BookingExpiryService(IServiceProvider serviceProvider, ILogger<BookingExpiryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var cancelledCount = await mediator.Send(new CancelExpiredBookingsCommand(), stoppingToken);

                    if (cancelledCount > 0)
                    {
                        _logger.LogInformation("Expired bookings cleanup: {Count} bookings cancelled", cancelledCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in booking expiry background service");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
