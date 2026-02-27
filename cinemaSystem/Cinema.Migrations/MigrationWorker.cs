using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cinema.Migrations;

public class MigrationWorker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostLifetime,
    ILogger<MigrationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("═══ Starting database migrations ═══");

            using var scope = serviceProvider.CreateScope();

            // ── 1. Migrate BookingContext ─────────────────────
            var bookingContext = scope.ServiceProvider
                .GetRequiredService<BookingContext>();
            
            var pendingBooking = await bookingContext.Database
                .GetPendingMigrationsAsync(stoppingToken);
            
            if (pendingBooking.Any())
            {
                logger.LogInformation(
                    "Applying {Count} pending migrations for BookingContext...",
                    pendingBooking.Count());
                await bookingContext.Database.MigrateAsync(stoppingToken);
            }
            else
            {
                logger.LogInformation("BookingContext is up to date.");
            }

            // ── 2. Migrate AppIdentityContext ────────────────
            var identityContext = scope.ServiceProvider
                .GetRequiredService<AppIdentityContext>();
            
            var pendingIdentity = await identityContext.Database
                .GetPendingMigrationsAsync(stoppingToken);
            
            if (pendingIdentity.Any())
            {
                logger.LogInformation(
                    "Applying {Count} pending migrations for AppIdentityContext...",
                    pendingIdentity.Count());
                await identityContext.Database.MigrateAsync(stoppingToken);
            }
            else
            {
                logger.LogInformation("AppIdentityContext is up to date.");
            }

            // ── 3. Seed Data ─────────────────────────────────
            logger.LogInformation("🚀 Seeding initial data...");
            await SeedDataAsync(scope.ServiceProvider, bookingContext);

            logger.LogInformation("✅ Database migrations and seeding completed successfully.");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Migration operation was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "❌ CRITICAL: Migration failed! The application will not start until infra is ready.");
            Environment.ExitCode = 1; // Explicitly set exit code for Aspire
            throw;
        }
        finally
        {
            logger.LogInformation("👋 Migration worker finished. Stopping application...");
            hostLifetime.StopApplication();
        }
    }

    private static async Task SeedDataAsync(
        IServiceProvider services, BookingContext context)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        logger.LogDebug("Seeding Roles...");
        string[] roles = ["Admin", "Manager", "Staff", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (result.Succeeded) logger.LogInformation("Role '{Role}' created.", role);
            }
        }

        logger.LogDebug("Seeding Admin User...");
        if (await userManager.FindByEmailAsync("admin@cinema.local") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@cinema.local",
                Email = "admin@cinema.local",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Admin user 'admin@cinema.local' created and assigned to 'Admin' role.");
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
