using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using Movie.API.Infrastructure.Storage;

namespace Movie.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration);
        services.Configure<SchedulingOptions>(configuration.GetSection(SchedulingOptions.SectionName));
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));

        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
        services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.CloudName) ||
                string.IsNullOrWhiteSpace(options.ApiKey) ||
                string.IsNullOrWhiteSpace(options.ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing or incomplete.");
            }

            var account = new Account(options.CloudName, options.ApiKey, options.ApiSecret);
            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;

            return cloudinary;
        });

        var cinemaApiUrl = configuration["ServiceUrls:CinemaApi"] ?? "https://localhost:7251";
        var bookingApiUrl = configuration["ServiceUrls:BookingApi"] ?? "https://localhost:7043";
        services.AddHttpClient<ICinemaApiClient, CinemaApiClient>(client =>
        {
            client.BaseAddress = new Uri(cinemaApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IBookingApiClient, BookingApiClient>(client =>
        {
            client.BaseAddress = new Uri(bookingApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

}

