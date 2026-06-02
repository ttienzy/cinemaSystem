using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Movie.API.Client.Client;

namespace Movie.API.Client.Extentions;

public static class MovieApiClientExtensions
{
    public static IHttpClientBuilder AddMovieApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "gateway")
    {
        return builder.Services.AddHttpClient<IMovieApiClient, MovieApiClient>(client =>
        {
            client.BaseAddress = new Uri($"https+http://{serviceName}");
        });
    }
}
