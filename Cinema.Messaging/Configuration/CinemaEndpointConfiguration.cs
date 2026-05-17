using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Cinema.Messaging.Configuration;

/// <summary>
/// Centralized receive endpoint defaults for the Cinema system.
/// Applies standard PrefetchCount and retry policies.
/// Replaces the duplicated ConfigureEndpoint() private methods.
/// </summary>
public static class CinemaEndpointConfiguration
{
    /// <summary>
    /// Applies Cinema-standard defaults to a receive endpoint:
    /// - PrefetchCount from config (default: 16)
    /// - Message retry with configurable interval (default: 3 retries, 5s interval)
    /// </summary>
    public static void ApplyCinemaDefaults(
        this IRabbitMqReceiveEndpointConfigurator endpoint,
        IConfiguration configuration)
    {
        var prefetchCount = configuration.GetSection("MassTransit:PrefetchCount").Value;
        endpoint.PrefetchCount = ushort.TryParse(prefetchCount, out var parsed) ? parsed : (ushort)16;

        var retryLimitStr = configuration.GetSection("MassTransit:RetryLimit").Value;
        var retryLimit = int.TryParse(retryLimitStr, out var rl) ? rl : 3;

        var retryIntervalStr = configuration.GetSection("MassTransit:RetryIntervalSeconds").Value;
        var retryInterval = int.TryParse(retryIntervalStr, out var ri) ? ri : 5;

        endpoint.UseMessageRetry(retry =>
        {
            retry.Interval(retryLimit, TimeSpan.FromSeconds(retryInterval));
        });
    }
}
