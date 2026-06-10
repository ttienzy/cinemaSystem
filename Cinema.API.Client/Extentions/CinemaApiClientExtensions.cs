using Cinema.API.Client.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cinema.API.Client.Extentions;

public static class CinemaApiClientExtensions
{
    /// <summary>
    /// Registers a typed <see cref="ICinemaApiClient"/> resolved through Aspire service discovery.
    /// The <paramref name="serviceName"/> must match the name used in AppHost.cs (default: "cinema").
    /// </summary>
    public static IHttpClientBuilder AddCinemaApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "cinema")
    {
        return builder.Services.AddHttpClient<ICinemaApiClient, CinemaApiClient>(client =>
        {
            client.BaseAddress = ResolveBaseAddress(builder, serviceName);
        });
    }

    private static Uri ResolveBaseAddress(IHostApplicationBuilder builder, string serviceName)
    {
        var configKey = char.ToUpperInvariant(serviceName[0]) + serviceName[1..];
        var configuredUrl = builder.Configuration[$"ServiceUrls:{configKey}"]
            ?? builder.Configuration[$"ServiceUrls:{serviceName}"];

        return new Uri(string.IsNullOrWhiteSpace(configuredUrl)
            ? $"https+http://{serviceName}"
            : configuredUrl.TrimEnd('/') + "/");
    }
}
