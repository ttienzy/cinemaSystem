using Cinema.Shared.Models;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cinema.Shared.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseConsulRegistration(
        this WebApplication app,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(ConsulOptions.SectionName).Get<ConsulOptions>();
        if (options is null || !options.Enabled || string.IsNullOrWhiteSpace(options.ServiceName))
        {
            return app;
        }

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            RegisterServiceAsync(app, options).GetAwaiter().GetResult();
        });

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            DeregisterServiceAsync(app, options).GetAwaiter().GetResult();
        });

        return app;
    }

    private static async Task RegisterServiceAsync(
        WebApplication app,
        ConsulOptions options)
    {
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ConsulRegistration");

        var serviceUri = ResolveServiceUri(app, options);
        if (serviceUri is null)
        {
            logger.LogWarning(
                "Skipping Consul registration for {ServiceName}: no server address was resolved.",
                options.ServiceName);
            return;
        }

        var serviceId = ResolveServiceId(options, serviceUri);
        options.ServiceId = serviceId;

        using var client = new ConsulClient(cfg => cfg.Address = new Uri(options.AgentAddress));

        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = options.ServiceName,
            Address = serviceUri.Host,
            Port = serviceUri.Port,
            Tags = options.Tags,
            Check = new AgentServiceCheck
            {
                HTTP = $"{serviceUri.Scheme}://{serviceUri.Host}:{serviceUri.Port}{options.HealthCheckPath}",
                Method = "GET",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        await client.Agent.ServiceDeregister(serviceId);
        await client.Agent.ServiceRegister(registration);

        logger.LogInformation(
            "Registered {ServiceName} with Consul at {Address}:{Port} ({Scheme})",
            options.ServiceName,
            serviceUri.Host,
            serviceUri.Port,
            serviceUri.Scheme);
    }

    private static async Task DeregisterServiceAsync(
        WebApplication app,
        ConsulOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceId))
        {
            return;
        }

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ConsulRegistration");

        try
        {
            using var client = new ConsulClient(cfg => cfg.Address = new Uri(options.AgentAddress));
            await client.Agent.ServiceDeregister(options.ServiceId);

            logger.LogInformation(
                "Deregistered {ServiceName} from Consul.",
                options.ServiceName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to deregister {ServiceName} from Consul.",
                options.ServiceName);
        }
    }

    private static Uri? ResolveServiceUri(
        WebApplication app,
        ConsulOptions options)
    {
        var addresses = app.Urls.Count > 0
            ? app.Urls
            : app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()?
                .Addresses
              ?? [];

        var candidates = addresses
            .Select(address => Uri.TryCreate(address, UriKind.Absolute, out var uri) ? uri : null)
            .OfType<Uri>()
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        var preferredScheme = options.PreferHttps ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        var selected = candidates.FirstOrDefault(uri => uri.Scheme.Equals(preferredScheme, StringComparison.OrdinalIgnoreCase))
            ?? candidates.FirstOrDefault(uri => uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            ?? candidates.First();

        var host = ResolvePublicHost(selected.Host, options.PublicHost);
        return new UriBuilder(selected.Scheme, host, selected.Port).Uri;
    }

    private static string ResolveServiceId(
        ConsulOptions options,
        Uri serviceUri)
    {
        if (!string.IsNullOrWhiteSpace(options.ServiceId))
        {
            return options.ServiceId;
        }

        return $"{options.ServiceName}-{serviceUri.Host}-{serviceUri.Port}";
    }

    private static string ResolvePublicHost(
        string host,
        string? configuredPublicHost)
    {
        if (!string.IsNullOrWhiteSpace(configuredPublicHost))
        {
            return configuredPublicHost;
        }

        return host switch
        {
            "0.0.0.0" => "localhost",
            "::" => "localhost",
            "[::]" => "localhost",
            "+" => "localhost",
            "*" => "localhost",
            _ => host
        };
    }
}
