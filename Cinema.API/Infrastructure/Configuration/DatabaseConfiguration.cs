using Cinema.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Infrastructure.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("cinema-db");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<CinemaDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}


