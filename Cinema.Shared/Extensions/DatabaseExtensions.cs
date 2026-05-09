using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cinema.Shared.Extensions;

/// <summary>
/// Extension methods for database operations with retry logic
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Add DbContext with retry logic for SQL Server
    /// </summary>
    public static IServiceCollection AddDbContextWithRetry<TContext>(
        this IServiceCollection services,
        Func<IServiceProvider, string> connectionStringFactory,
        Action<DbContextOptionsBuilder>? additionalOptions = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            var connectionString = connectionStringFactory(serviceProvider);

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Enable retry on failure (for transient errors)
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                // Command timeout
                sqlOptions.CommandTimeout(60);
            });

            // Apply additional options if provided
            additionalOptions?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Run database migrations with retry logic
    /// </summary>
    public static async Task MigrateDatabaseAsync<TContext>(
        this IHost host,
        int maxRetries = 5,
        int delaySeconds = 5)
        where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var dbContext = services.GetRequiredService<TContext>();

        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            try
            {
                logger.LogInformation(
                    "Attempting database migration for {DbContext} (Attempt {Attempt}/{MaxRetries})",
                    typeof(TContext).Name,
                    retryCount + 1,
                    maxRetries);

                // Check if database can be connected
                var canConnect = await dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    throw new InvalidOperationException("Cannot connect to database");
                }

                // Run migrations
                await dbContext.Database.MigrateAsync();

                logger.LogInformation(
                    "Database migration completed successfully for {DbContext}",
                    typeof(TContext).Name);

                return; // Success!
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount >= maxRetries)
                {
                    logger.LogError(ex,
                        "Failed to migrate database for {DbContext} after {MaxRetries} attempts",
                        typeof(TContext).Name,
                        maxRetries);
                    throw;
                }

                logger.LogWarning(ex,
                    "Migration attempt {Attempt} failed for {DbContext}. Retrying in {Delay} seconds...",
                    retryCount,
                    typeof(TContext).Name,
                    delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }

    /// <summary>
    /// Wait for database to be ready
    /// </summary>
    public static async Task WaitForDatabaseAsync<TContext>(
        this IHost host,
        int maxRetries = 10,
        int delaySeconds = 3)
        where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var dbContext = services.GetRequiredService<TContext>();

        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            try
            {
                logger.LogInformation(
                    "Checking if database is ready for {DbContext} (Attempt {Attempt}/{MaxRetries})",
                    typeof(TContext).Name,
                    retryCount + 1,
                    maxRetries);

                var canConnect = await dbContext.Database.CanConnectAsync();
                if (canConnect)
                {
                    logger.LogInformation(
                        "Database is ready for {DbContext}",
                        typeof(TContext).Name);
                    return;
                }

                throw new InvalidOperationException("Cannot connect to database");
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount >= maxRetries)
                {
                    logger.LogError(ex,
                        "Database not ready for {DbContext} after {MaxRetries} attempts",
                        typeof(TContext).Name,
                        maxRetries);
                    throw;
                }

                logger.LogWarning(
                    "Database not ready for {DbContext}. Retrying in {Delay} seconds... (Attempt {Attempt}/{MaxRetries})",
                    typeof(TContext).Name,
                    delaySeconds,
                    retryCount,
                    maxRetries);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}
