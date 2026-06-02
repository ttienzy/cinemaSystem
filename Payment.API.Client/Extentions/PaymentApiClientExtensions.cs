using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.API.Client.Client;

namespace Payment.API.Client.Extentions;

public static class PaymentApiClientExtensions
{
    public static IHttpClientBuilder AddPaymentApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "gateway")
    {
        return builder.Services.AddHttpClient<IPaymentApiClient, PaymentApiClient>(client =>
        {
            client.BaseAddress = new Uri($"https+http://{serviceName}");
        });
    }
}
