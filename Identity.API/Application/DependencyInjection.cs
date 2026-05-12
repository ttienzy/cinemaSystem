using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}

