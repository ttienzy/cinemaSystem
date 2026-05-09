using Microsoft.Extensions.DependencyInjection;

namespace Cinema.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICinemaService, CinemaService>();
        services.AddScoped<ICinemaHallService, CinemaHallService>();
        services.AddScoped<ISeatService, SeatService>();

        return services;
    }
}

