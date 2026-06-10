using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Movie.API.Client.Client;

namespace Movie.API.Client.Extentions;

public static class MovieApiClientExtensions
{
    public static IHttpClientBuilder AddMovieApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "movie")
    {
        return builder.Services.AddHttpClient<IMovieApiClient, MovieApiClient>(client =>
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
