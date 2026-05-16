using MassTransit;
using Notification.API.Infrastructure.Configuration;
using Notification.API.Infrastructure.Messaging.Consumers;
using Notification.API.Infrastructure.Notifications;

namespace Notification.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailService, EmailService>();
        services.AddNotificationMassTransit(configuration);

        return services;
    }

    private static IServiceCollection AddNotificationMassTransit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(busRegistration =>
        {
            busRegistration.AddConsumer<PaymentCompletedConsumer>();

            busRegistration.UsingRabbitMq((context, cfg) =>
            {
                ConfigureRabbitMqHost(cfg, configuration);

                cfg.ReceiveEndpoint(
                    configuration["MassTransit:EndpointName"] ?? "notification-api-masstransit",
                    endpoint =>
                    {
                        endpoint.PrefetchCount = configuration.GetValue<ushort>("MassTransit:PrefetchCount", 16);
                        endpoint.UseMessageRetry(retry =>
                        {
                            retry.Interval(
                                configuration.GetValue("MassTransit:RetryLimit", 3),
                                TimeSpan.FromSeconds(configuration.GetValue("MassTransit:RetryIntervalSeconds", 5)));
                        });

                        endpoint.ConfigureConsumer<PaymentCompletedConsumer>(context);
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
}
