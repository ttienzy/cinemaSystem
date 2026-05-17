using Booking.API.Infrastructure.BackgroundServices;
using Booking.API.Infrastructure.Caching.Services;
using Booking.API.Infrastructure.Configuration;
using Booking.API.Infrastructure.Hubs;
using Booking.API.Infrastructure.Hubs.Services;
using Booking.API.Infrastructure.Integrations.Clients;
using Booking.API.Infrastructure.Messaging.Consumers;
using Booking.API.Infrastructure.Persistence.Repositories;
using Cinema.Contracts.Messaging;
using Cinema.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Booking.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration);

        var redisConnection = configuration.GetConnectionString("redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            throw new InvalidOperationException("Connection string 'redis' is not configured.");
        }

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "CinemaBooking:";
        });



        var cinemaApiUrl = configuration["ServiceUrls:CinemaApi"] ?? "https://localhost:7251";
        var movieApiUrl = configuration["ServiceUrls:MovieApi"] ?? "https://localhost:7295";
        var paymentApiUrl = configuration["ServiceUrls:PaymentApi"] ?? "https://localhost:7252";

        services.AddHttpClient<CinemaApiClient>(client =>
        {
            client.BaseAddress = new Uri(cinemaApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<MovieApiClient>(client =>
        {
            client.BaseAddress = new Uri(movieApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<PaymentApiClient>(client =>
        {
            client.BaseAddress = new Uri(paymentApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IExternalServiceClient, ExternalServiceClient>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISeatLockService, SeatLockService>();
        services.AddScoped<ISeatStatusService, SeatStatusService>();

        // SignalR services
        services.AddSingleton<IConnectionTracker, RedisConnectionTracker>();
        services.AddScoped<ISeatNotificationService, SeatNotificationService>();
        services.AddScoped<IAdminDashboardNotificationService, AdminDashboardNotificationService>();

        // SignalR with Redis backplane
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = configuration.GetValue<bool>("SignalR:EnableDetailedErrors", false);
            options.KeepAliveInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("SignalR:KeepAliveIntervalSeconds", 15));
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("SignalR:ClientTimeoutIntervalSeconds", 30));
            options.HandshakeTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("SignalR:HandshakeTimeoutSeconds", 15));
            options.MaximumReceiveMessageSize = configuration.GetValue<long?>("SignalR:MaximumReceiveMessageSize", 32 * 1024);
        })
        .AddJsonProtocol(options =>
        {
            // Vô hiệu hóa việc tự động đổi tên thuộc tính và method name
            // Giữ nguyên PascalCase từ C# để đảm bảo consistency giữa Backend và Frontend
            options.PayloadSerializerOptions.PropertyNamingPolicy = null;
        })
        .AddStackExchangeRedis(redisConnection, options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal(
                configuration.GetValue<string>("Redis:SignalRChannelPrefix") ?? "cinema:signalr");
        });

        services.AddCinemaMessaging(
            configuration,
            CinemaQueues.Booking,
            bus =>
            {
                bus.AddConsumer<PaymentCompletedConsumer>();
                bus.AddConsumer<PaymentFailedConsumer>();
            },
            (endpoint, context) =>
            {
                endpoint.ConfigureConsumer<PaymentCompletedConsumer>(context);
                endpoint.ConfigureConsumer<PaymentFailedConsumer>(context);
            });

        if (configuration.GetValue("BackgroundServices:EnableCleanupService", true))
        {
            services.AddHostedService<ExpiredBookingCleanupService>();
        }

        return services;
    }
}
