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
using StackExchange.Redis;

namespace Infrastructure
{
    /// <summary>
    /// Clean Architecture DI — Infrastructure layer.
    /// All old service-layer registrations removed. Controllers now use MediatR.
    /// Called from Program.cs: builder.Services.AddInfrastructure(config)
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration config)
        {
            // ── Database ─────────────────────────────────────────────
            services.AddScoped<DomainEventDispatcherInterceptor>();

            services.AddDbContext<BookingContext>((sp, options) =>
                options.UseSqlServer(config.GetConnectionString("BookingConnection"))
                       .AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>()));

            services.AddDbContext<AppIdentityContext>(options =>
                options.UseSqlServer(config.GetConnectionString("IdentityConnection")));

            // ── Redis ─────────────────────────────────────────────────
            var redisConn = config.GetConnectionString("RedisConnection");
            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(redisConn!));
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
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── External Services ─────────────────────────────────────
            services.AddScoped<IPaymentGateway, VnPayGateway>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<ISeatNotificationService, SeatNotificationService>();

            // ── Identity ──────────────────────────────────────────────
            services.AddScoped<IIdentityUserService, IdentityService>();
            services.AddScoped<ITokenClaimService, TokenClaimService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IOtpService, OtpService>();

            // ── SignalR ───────────────────────────────────────────────
            services.AddSignalR(o => o.EnableDetailedErrors = true)
                    .AddStackExchangeRedis(redisConn!);

            return services;
        }
    }
}
