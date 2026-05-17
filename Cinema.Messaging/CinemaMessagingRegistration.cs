using Cinema.Messaging.Abstractions;
using Cinema.Messaging.Configuration;
using Cinema.Messaging.Infrastructure;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cinema.Messaging;

/// <summary>
/// Main entry point for Cinema messaging infrastructure.
/// Provides a single extension method that configures MassTransit + RabbitMQ
/// with Cinema-standard topology, host, endpoint defaults, and IEventBus registration.
/// </summary>
public static class CinemaMessagingRegistration
{
    /// <summary>
    /// Registers MassTransit with RabbitMQ using Cinema-standard configuration.
    /// Each service provides its own queue name, consumer registrations, and endpoint bindings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (for RabbitMQ connection + MassTransit settings).</param>
    /// <param name="queueName">The unique queue name for this service (from CinemaQueues constants).</param>
    /// <param name="configureConsumers">Action to register service-specific consumers on the bus.</param>
    /// <param name="configureEndpoint">Action to bind consumers to the receive endpoint.</param>
    public static IServiceCollection AddCinemaMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string queueName,
        Action<IBusRegistrationConfigurator> configureConsumers,
        Action<IRabbitMqReceiveEndpointConfigurator, IBusRegistrationContext> configureEndpoint)
    {
        services.AddMassTransit(bus =>
        {
            configureConsumers(bus);

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.ApplyCinemaHost(configuration);
                cfg.ApplyCinemaTopology();

                cfg.ReceiveEndpoint(queueName, endpoint =>
                {
                    endpoint.ApplyCinemaDefaults(configuration);
                    configureEndpoint(endpoint, context);
                });
            });
        });

        // Register the IEventBus abstraction for Application layer usage
        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
