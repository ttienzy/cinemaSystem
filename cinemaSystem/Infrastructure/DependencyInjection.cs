using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Security;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Identity;
using Infrastructure.Identity.Security;
using Infrastructure.Payments.Services;
using Infrastructure.Redis;
using Infrastructure.ExternalServices;
using Infrastructure.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure
{
    /// <summary>
    /// Clean Architecture DI — Infrastructure layer.
    /// Called from Program.cs: builder.AddInfrastructure()
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Database ─────────────────────────────────────────────
            services.AddScoped<DomainEventDispatcherInterceptor>();

            services.AddDbContext<BookingContext>((sp, options) =>
            {
                var connStr = configuration.GetConnectionString("BookingConnection");
                options.UseSqlServer(connStr);
                options.AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            });

            services.AddDbContext<AppIdentityContext>(options =>
            {
                var connStr = configuration.GetConnectionString("IdentityConnection");
                options.UseSqlServer(connStr);
            });

            // ── Redis ─────────────────────────────────────────────────
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<ISeatLockService, SeatLockService>();

            // ── Repositories (1 per aggregate root) ───────────────────
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
            services.AddScoped<ICinemaRepository, CinemaRepository>();
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<IConcessionSaleRepository, ConcessionSaleRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();

            // Shared Aggregates
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<ISeatTypeRepository, SeatTypeRepository>();
            services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
            services.AddScoped<IPricingTierRepository, PricingTierRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── External Services ─────────────────────────────────────
            services.AddScoped<IPaymentGateway, VnPayGateway>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<ISeatNotificationService, SeatNotificationService>();

            // ── Identity ──────────────────────────────────────────────
            services.AddScoped<IIdentityUserService, IdentityService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ITokenClaimService, TokenClaimService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IUserManagementService, Infrastructure.Identity.Services.UserManagementService>();

            // ── Staff Management (Shifts & Schedules) ────────────────
            services.AddScoped<IShiftRepository, ShiftRepository>();
            services.AddScoped<IWorkScheduleRepository, WorkScheduleRepository>();

            // ── Equipment Management ─────────────────────────────────
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();

            // ── SignalR ───────────────────────────────────────────────
            var redisConnStr = configuration.GetConnectionString("RedisConnection");
            if (!string.IsNullOrWhiteSpace(redisConnStr))
            {
                services.AddSignalR(o => o.EnableDetailedErrors = true)
                        .AddStackExchangeRedis(redisConnStr);
            }
            else
            {
                services.AddSignalR(o => o.EnableDetailedErrors = true);
            }

            return services;
        }
    }
}
