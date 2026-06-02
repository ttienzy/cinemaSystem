using Microsoft.EntityFrameworkCore;
using Movie.API.Entities;
using MovieEntity = Movie.API.Entities.Movie;

namespace Movie.API.Data;

public static class MovieDbSeeder
{
    public static async Task SeedAsync(MovieDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Movies.AnyAsync(cancellationToken) ||
            await context.Genres.AnyAsync(cancellationToken))
        {
            return;
        }

        var genreMap = await EnsureGenresAsync(context, cancellationToken);
        await EnsureMoviesAsync(context, genreMap, cancellationToken);
    }

    private static async Task<Dictionary<string, Guid>> EnsureGenresAsync(
        MovieDbContext context,
        CancellationToken cancellationToken)
    {
        var names = new[]
        {
            "Action",
            "Adventure",
            "Animation",
            "Comedy",
            "Drama",
            "Horror",
            "Science Fiction",
            "Thriller"
        };

        var existingGenres = await context.Genres.ToListAsync(cancellationToken);

        foreach (var name in names)
        {
            if (existingGenres.Any(genre => genre.Name == name))
            {
                continue;
            }

            var genre = new Genre { Name = name };
            context.Genres.Add(genre);
            existingGenres.Add(genre);
        }

        await context.SaveChangesAsync(cancellationToken);

        return existingGenres.ToDictionary(genre => genre.Name, genre => genre.Id);
    }

    private static async Task EnsureMoviesAsync(
        MovieDbContext context,
        IReadOnlyDictionary<string, Guid> genreMap,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.Date;
        var movies = new[]
        {
            new SeedMovie(
                "The Last Horizon",
                "A rescue crew crosses deep space to recover a lost research vessel.",
                128,
                "English",
                now.AddDays(-30),
                ["Action", "Science Fiction", "Thriller"]),
            new SeedMovie(
                "Saigon Summer",
                "A gentle comedy about old friends rebuilding a neighborhood cinema.",
                105,
                "Vietnamese",
                now.AddDays(-10),
                ["Comedy", "Drama"]),
            new SeedMovie(
                "Moonlit Arcade",
                "A group of teenagers discover that an abandoned arcade hides a strange portal.",
                112,
                "English",
                now.AddDays(20),
                ["Adventure", "Science Fiction"]),
            new SeedMovie(
                "The Quiet Room",
                "A chamber drama where one interview changes the course of a family.",
                96,
                "Vietnamese",
                now.AddDays(-60),
                ["Drama", "Thriller"])
        };

        foreach (var seed in movies)
        {
            var exists = await context.Movies.AnyAsync(movie => movie.Title == seed.Title, cancellationToken);
            if (exists)
            {
                continue;
            }

            var genreIds = seed.Genres
                .Where(genreMap.ContainsKey)
                .Select(genre => genreMap[genre])
                .ToList();

            var movie = MovieEntity.Create(
                seed.Title,
                seed.Description,
                seed.Duration,
                seed.Language,
                seed.ReleaseDate,
                posterUrl: null,
                genreIds);

            context.Movies.Add(movie);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed record SeedMovie(
        string Title,
        string Description,
        int Duration,
        string Language,
        DateTime ReleaseDate,
        IReadOnlyCollection<string> Genres);
}
