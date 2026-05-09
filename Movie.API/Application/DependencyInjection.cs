using Microsoft.Extensions.DependencyInjection;

namespace Movie.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IShowtimeService, ShowtimeService>();

        return services;
    }
}

