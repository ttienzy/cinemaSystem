using Booking.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Booking.API.Infrastructure.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("booking-db");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}


