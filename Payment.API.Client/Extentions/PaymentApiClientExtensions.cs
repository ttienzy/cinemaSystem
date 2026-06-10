using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.API.Client.Client;

namespace Payment.API.Client.Extentions;

public static class PaymentApiClientExtensions
{
    public static IHttpClientBuilder AddPaymentApiClient(
        this IHostApplicationBuilder builder,
        string serviceName = "payment")
    {
        return builder.Services.AddHttpClient<IPaymentApiClient, PaymentApiClient>(client =>
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
