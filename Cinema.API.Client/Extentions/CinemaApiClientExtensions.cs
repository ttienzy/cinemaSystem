using Cinema.API.Client.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cinema.API.Client.Extentions;

public static class CinemaApiClientExtensions
{
    /// <summary>
    /// Registers a typed <see cref="ICinemaApiClient"/> resolved through Aspire service discovery.
    /// The <paramref name="serviceName"/> must match the name used in AppHost.cs (default: "gateway").
    /// </summary>
    public static IHttpClientBuilder AddCinemaApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "gateway")
    {
        return builder.Services.AddHttpClient<ICinemaApiClient, CinemaApiClient>(client =>
        {
            client.BaseAddress = new Uri($"https+http://{serviceName}");
        });
    }
}
