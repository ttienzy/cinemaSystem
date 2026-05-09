using Identity.API.Infrastructure.Persistence;
using Identity.API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("identity-db");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        // Prevent ASP.NET Core Identity from returning 302 Redirect to /Account/Login on unauthorized API requests
        // services.ConfigureApplicationCookie(options =>
        // {
        //     options.Events.OnRedirectToLogin = context =>
        //     {
        //         context.Response.StatusCode = 401;
        //         return Task.CompletedTask;
        //     };
        //     options.Events.OnRedirectToAccessDenied = context =>
        //     {
        //         context.Response.StatusCode = 403;
        //         return Task.CompletedTask;
        //     };
        // });

        return services;
    }
}


