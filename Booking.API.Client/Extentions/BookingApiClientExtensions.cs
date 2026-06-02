using Booking.API.Client.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Booking.API.Client.Extentions;

public static class BookingApiClientExtensions
{
    public static IHttpClientBuilder AddBookingApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "gateway")
    {
        return builder.Services.AddHttpClient<IBookingApiClient, BookingApiClient>(client =>
        {
            client.BaseAddress = new Uri($"https+http://{serviceName}");
        });
    }
}
