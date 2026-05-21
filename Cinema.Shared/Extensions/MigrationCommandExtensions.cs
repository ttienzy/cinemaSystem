using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cinema.Shared.Extensions;

public static class MigrationCommandExtensions
{
    private const string MigrateOnlyArgument = "--migrate-only";

    public static bool IsMigrationOnlyCommand(this string[] args)
    {
        return args.Any(arg => string.Equals(arg, MigrateOnlyArgument, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task MigrateAndStopAsync<TContext>(
        this WebApplication app,
        int maxAttempts = 12,
        int delaySeconds = 5)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigrator");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation(
                    "Applying migrations for {DbContext}. Attempt {Attempt}/{MaxAttempts}.",
                    typeof(TContext).Name,
                    attempt,
                    maxAttempts);

                var strategy = dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(() => dbContext.Database.MigrateAsync());

                logger.LogInformation("Migrations for {DbContext} completed successfully.", typeof(TContext).Name);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Migration for {DbContext} failed. Retrying in {DelaySeconds} seconds.",
                    typeof(TContext).Name,
                    delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        await dbContext.Database.MigrateAsync();
    }
}
