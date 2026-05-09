using Microsoft.Extensions.DependencyInjection;

namespace Booking.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingAnalyticsService, BookingAnalyticsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITicketOperationsService, TicketOperationsService>();

        return services;
    }
}

