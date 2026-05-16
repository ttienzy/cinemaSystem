using Booking.API.Infrastructure.BackgroundServices;
using Booking.API.Infrastructure.Caching.Services;
using Booking.API.Infrastructure.Configuration;
using Booking.API.Infrastructure.Hubs;
using Booking.API.Infrastructure.Hubs.Services;
using Booking.API.Infrastructure.Integrations.Clients;
using Booking.API.Infrastructure.Messaging.Consumers;

using Booking.API.Infrastructure.Notifications.Services;
using Booking.API.Infrastructure.Persistence.Repositories;
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
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

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
        services.AddScoped<IEmailService, EmailService>();

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

        services.AddBookingMassTransit(configuration);

        if (configuration.GetValue("BackgroundServices:EnableCleanupService", true))
        {
            services.AddHostedService<ExpiredBookingCleanupService>();
        }

        return services;
    }

    private static IServiceCollection AddBookingMassTransit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(busRegistration =>
        {
            busRegistration.AddConsumer<PaymentCompletedConsumer>();
            busRegistration.AddConsumer<PaymentFailedConsumer>();

            busRegistration.UsingRabbitMq((context, cfg) =>
            {
                ConfigureRabbitMqHost(cfg, configuration);

                cfg.ReceiveEndpoint(
                    configuration["MassTransit:EndpointName"] ?? "booking-api-masstransit",
                    endpoint =>
                    {
                        ConfigureEndpoint(endpoint, configuration);
                        endpoint.ConfigureConsumer<PaymentCompletedConsumer>(context);
                        endpoint.ConfigureConsumer<PaymentFailedConsumer>(context);
                    });
            });
        });

        return services;
    }

    private static void ConfigureRabbitMqHost(
        IRabbitMqBusFactoryConfigurator cfg,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rabbitmq");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var uri = new Uri(connectionString);
            var virtualHost = uri.AbsolutePath.Trim('/');

            cfg.Host(
                uri.Host,
                (ushort)(uri.IsDefaultPort ? 5672 : uri.Port),
                string.IsNullOrWhiteSpace(virtualHost) ? "/" : virtualHost,
                host =>
                {
                    if (!string.IsNullOrWhiteSpace(uri.UserInfo))
                    {
                        var userInfo = uri.UserInfo.Split(':', 2);
                        host.Username(Uri.UnescapeDataString(userInfo[0]));

                        if (userInfo.Length > 1)
                        {
                            host.Password(Uri.UnescapeDataString(userInfo[1]));
                        }
                    }
                });

            return;
        }

        var eventBusConfig = configuration.GetSection("EventBus");
        cfg.Host(
            eventBusConfig["Connection"] ?? "localhost",
            "/",
            host =>
            {
                host.Username(eventBusConfig["UserName"] ?? "guest");
                host.Password(eventBusConfig["Password"] ?? "guest");
            });
    }

    private static void ConfigureEndpoint(
        IRabbitMqReceiveEndpointConfigurator endpoint,
        IConfiguration configuration)
    {
        endpoint.PrefetchCount = configuration.GetValue<ushort>("MassTransit:PrefetchCount", 16);

        endpoint.UseMessageRetry(retry =>
        {
            retry.Interval(
                configuration.GetValue("MassTransit:RetryLimit", 3),
                TimeSpan.FromSeconds(configuration.GetValue("MassTransit:RetryIntervalSeconds", 5)));
        });
    }
}
