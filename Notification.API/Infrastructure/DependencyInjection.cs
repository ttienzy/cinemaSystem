using MassTransit;
using Cinema.Contracts.Messaging;
using Cinema.Messaging;
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

        services.AddCinemaMessaging(
            configuration,
            CinemaQueues.Notification,
            bus =>
            {
                bus.AddConsumer<PaymentCompletedConsumer>();
            },
            (endpoint, context) =>
            {
                endpoint.ConfigureConsumer<PaymentCompletedConsumer>(context);
            });

        return services;
    }
}
