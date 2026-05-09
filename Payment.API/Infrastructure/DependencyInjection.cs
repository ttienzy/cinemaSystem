using Cinema.EventBusRabbitMQ.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddRabbitMQEventBus(configuration);
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

