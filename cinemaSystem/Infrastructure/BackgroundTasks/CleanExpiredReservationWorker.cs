using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.BookingSpec;
using Infrastructure.Data.Repositories;
using Infrastructure.Hubs;
using Infrastructure.Hubs.Constants;
using Infrastructure.Payments.Constants;
using Infrastructure.Redis.Constants;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Models.DataModels.BookingDtos;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundTasks
{
    public class CleanExpiredReservationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanExpiredReservationWorker> _logger;
        public CleanExpiredReservationWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CleanExpiredReservationWorker> logger)
        {
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired Reservation Worker started at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                using(var scope = _scopeFactory.CreateScope())
                {
                    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    var hubService = scope.ServiceProvider.GetRequiredService<IHubContext<SeatHub>>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    try
                    {
                        var bookingSpec = new BookingByShowtimeIdSpecification();
                        var bookings = await unitOfWork.Bookings.ListAsync(bookingSpec);
                        if (bookings.Count == 0)
                            return;

                        var grouped = bookings.GroupBy(b => b.ShowtimeId)
                            .Select(g => new CleanBookingModel
                            {
                                Key = g.Key,
                                Values = g.SelectMany(bt => bt.BookingTickets).Select(t => t.SeatId).ToHashSet()
                            });
                        foreach (var item in grouped)
                        {
                            var data = await cacheService.GetAsync<ShowtimeSeatingPlanResponse>(item.Key.ToString());
                            if (data is null)
                                continue;
                            data.Seats.ForEach(s =>
                            {
                                if (item.Values.Contains(s.Id) && DateTime.UtcNow >= s.LastUpdated.AddMinutes(PaymentConstants.ExpireInMinutes))
                                {
                                    s.Status = Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Available;
                                }
                            });
                            await hubService.Clients.Group(item.Key.ToString()).SendAsync(SignalMethodConstants.OnSeatsReleased, item.Values.ToList());
                            await cacheService.UpdateAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(item.Key), data);
                        }
                                            }
                    catch (Exception ex)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "An error occurred while creating a scope in Expired Reservation Worker.");
                        continue;
                    }
                    finally
                    {
                        unitOfWork.Dispose();
                    }
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(PaymentConstants.ExpireInMinutes), stoppingToken);
        }
    }
}
