using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Subscriptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Cinema.EventBusRabbitMQ.Extensions;

/// <summary>
/// Extension methods for registering EventBus services
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Add RabbitMQ EventBus to the service collection.
    /// Supports Aspire connection string (AMQP URI) via GetConnectionString("rabbitmq"),
    /// with fallback to manual EventBus config section for non-Aspire scenarios.
    /// </summary>
    public static IServiceCollection AddRabbitMQEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "EventBus")
    {
        var eventBusConfig = configuration.GetSection(configSectionName);

        // Register subscription manager
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        // Register RabbitMQ persistent connection
        services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
            var config = sp.GetRequiredService<IConfiguration>();

            var factory = new ConnectionFactory
            {
                DispatchConsumersAsync = true
            };

            // Priority 1: Aspire injects ConnectionStrings:rabbitmq as AMQP URI
            var connectionString = config.GetConnectionString("rabbitmq");
            if (!string.IsNullOrEmpty(connectionString))
            {
                factory.Uri = new Uri(connectionString);
            }
            else
            {
                // Priority 2: Fallback to manual config (non-Aspire / local dev)
                factory.HostName = eventBusConfig["Connection"] ?? "localhost";
                factory.UserName = eventBusConfig["UserName"] ?? "guest";
                factory.Password = eventBusConfig["Password"] ?? "guest";

                if (!string.IsNullOrEmpty(eventBusConfig["Port"]))
                {
                    factory.Port = int.Parse(eventBusConfig["Port"]!);
                }
            }

            var retryCount = 5;
            if (!string.IsNullOrEmpty(eventBusConfig["RetryCount"]))
            {
                retryCount = int.Parse(eventBusConfig["RetryCount"]!);
            }

            return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
        });

        // Register EventBus
        services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        {
            var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
            var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

            var retryCount = 5;
            if (!string.IsNullOrEmpty(eventBusConfig["RetryCount"]))
            {
                retryCount = int.Parse(eventBusConfig["RetryCount"]!);
            }

            var brokerName = eventBusConfig["BrokerName"] ?? "cinema_event_bus";
            var queueName = eventBusConfig["QueueName"];

            return new EventBusRabbitMQ(
                rabbitMQPersistentConnection,
                logger,
                eventBusSubcriptionsManager,
                sp,
                brokerName,
                queueName,
                retryCount);
        });

        return services;
    }

    /// <summary>
    /// Configure event subscriptions for the application
    /// </summary>
    public static IServiceProvider ConfigureEventBus(
        this IServiceProvider serviceProvider,
        Action<IEventBus> configure)
    {
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();
        configure(eventBus);
        return serviceProvider;
    }
}
