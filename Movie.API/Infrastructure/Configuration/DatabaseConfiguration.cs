using Microsoft.EntityFrameworkCore;
using Movie.API.Infrastructure.Persistence;

namespace Movie.API.Infrastructure.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("movie-db");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<MovieDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}


