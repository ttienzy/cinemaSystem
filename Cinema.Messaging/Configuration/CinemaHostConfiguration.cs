using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Cinema.Messaging.Configuration;

/// <summary>
/// Centralized RabbitMQ host configuration for the Cinema system.
/// Supports both connection-string format (amqp://) and discrete settings.
/// Replaces the 3x duplicated ConfigureRabbitMqHost() private methods.
/// </summary>
public static class CinemaHostConfiguration
{
    /// <summary>
    /// Configures the RabbitMQ host connection from IConfiguration.
    /// Reads "ConnectionStrings:rabbitmq" first, falls back to "RabbitMQ:*" section.
    /// </summary>
    public static void ApplyCinemaHost(
        this IRabbitMqBusFactoryConfigurator cfg,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rabbitmq");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            ConfigureFromConnectionString(cfg, connectionString);
            return;
        }

        ConfigureFromSection(cfg, configuration);
    }

    private static void ConfigureFromConnectionString(
        IRabbitMqBusFactoryConfigurator cfg,
        string connectionString)
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
    }

    private static void ConfigureFromSection(
        IRabbitMqBusFactoryConfigurator cfg,
        IConfiguration configuration)
    {
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
