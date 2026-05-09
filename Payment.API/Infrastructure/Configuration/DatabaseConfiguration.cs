using Microsoft.EntityFrameworkCore;
using Payment.API.Infrastructure.Persistence;

namespace Payment.API.Infrastructure.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}


