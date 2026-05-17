using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cinema.Contracts.Messaging;
using Cinema.Messaging;
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

        services.AddCinemaMessaging(
            configuration,
            CinemaQueues.Payment,
            bus =>
            {
                bus.AddConsumer<BookingCreatedConsumer>();
                bus.AddConsumer<BookingCancelledConsumer>();
                bus.AddConsumer<BookingExpiredConsumer>();
            },
            (endpoint, context) =>
            {
                endpoint.ConfigureConsumer<BookingCreatedConsumer>(context);
                endpoint.ConfigureConsumer<BookingCancelledConsumer>(context);
                endpoint.ConfigureConsumer<BookingExpiredConsumer>(context);
            });

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
}
