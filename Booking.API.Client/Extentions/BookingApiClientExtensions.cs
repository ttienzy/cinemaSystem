using Booking.API.Client.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Booking.API.Client.Extentions;

public static class BookingApiClientExtensions
{
    public static IHttpClientBuilder AddBookingApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "booking")
    {
        return builder.Services.AddHttpClient<IBookingApiClient, BookingApiClient>(client =>
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
