using Identity.API.Client.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.API.Client.Extentions
{
    public static class IdentityApiClientExtensions
    {
        /// <summary>
        /// Registers a typed <see cref="IIdentityApiClient"/> resolved through Aspire service discovery.
        /// The <paramref name="serviceName"/> must match the name used in AppHost.cs (default: "identity").
        /// </summary>
        public static IHttpClientBuilder AddIdentityApiClient(
            this IHostApplicationBuilder builder,
            string serviceName = "identity")
        {
            return builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
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

}
