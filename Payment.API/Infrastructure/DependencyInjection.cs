using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cinema.Contracts.Events;
using Cinema.Contracts.Messaging;
using Payment.API.Infrastructure.Messaging.Consumers;

namespace Payment.API.Infrastructure;

public static class DependencyInjection
{
    public const string CorsPolicyName = "PaymentApiCors";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration);
        services.Configure<SePayOptions>(configuration.GetSection(SePayOptions.SectionName));

        services.AddScoped<ISePayService, SePayService>();
        services.AddScoped<ISePayIpnProcessor, SePayIpnProcessor>();
        services.AddScoped<IPaymentIntegrationEventPublisher, PaymentIntegrationEventPublisher>();
        services.AddPaymentMassTransit(configuration);
        services.AddHealthChecks();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    private static IServiceCollection AddPaymentMassTransit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(busRegistration =>
        {
            busRegistration.AddConsumer<BookingCreatedConsumer>();
            busRegistration.AddConsumer<BookingCancelledConsumer>();
            busRegistration.AddConsumer<BookingExpiredConsumer>();

            busRegistration.UsingRabbitMq((context, cfg) =>
            {
                ConfigureRabbitMqHost(cfg, configuration);
                ConfigureEventTopology(cfg);

                cfg.ReceiveEndpoint(
                    CinemaQueues.Payment,
                    endpoint =>
                    {
                        ConfigureEndpoint(endpoint, configuration);
                        endpoint.ConfigureConsumer<BookingCreatedConsumer>(context);
                        endpoint.ConfigureConsumer<BookingCancelledConsumer>(context);
                        endpoint.ConfigureConsumer<BookingExpiredConsumer>(context);
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

        cfg.Host(
            configuration["RabbitMQ:Connection"] ?? "localhost",
            "/",
            host =>
            {
                host.Username(configuration["RabbitMQ:UserName"] ?? "guest");
                host.Password(configuration["RabbitMQ:Password"] ?? "guest");
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

    private static void ConfigureEventTopology(IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<BookingCreatedEvent>(x => x.SetEntityName(CinemaEventNames.BookingCreated));
        cfg.Message<BookingCancelledEvent>(x => x.SetEntityName(CinemaEventNames.BookingCancelled));
        cfg.Message<BookingExpiredEvent>(x => x.SetEntityName(CinemaEventNames.BookingExpired));
        cfg.Message<PaymentCompletedEvent>(x => x.SetEntityName(CinemaEventNames.PaymentCompleted));
        cfg.Message<PaymentFailedEvent>(x => x.SetEntityName(CinemaEventNames.PaymentFailed));
    }
}
