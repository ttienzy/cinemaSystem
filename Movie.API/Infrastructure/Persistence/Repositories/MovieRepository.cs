using Microsoft.EntityFrameworkCore;
using Movie.API.Infrastructure.Persistence;
using Movie.API.Domain.Entities;
using MovieEntity = Movie.API.Domain.Entities.Movie;

namespace Movie.API.Infrastructure.Persistence.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MovieDbContext _context;

    public MovieRepository(MovieDbContext context)
    {
        _context = context;
    }

    public async Task<List<MovieEntity>> GetAllAsync()
    {
        return await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.Showtimes)
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();
    }

    public async Task<MovieEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.Showtimes)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MovieEntity> CreateAsync(MovieEntity movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(movie.Id)
            ?? throw new InvalidOperationException("Created movie could not be reloaded.");
    }

    public async Task<MovieEntity?> UpdateAsync(Guid id, MovieEntity movie, IEnumerable<Guid> genreIds)
    {
        var existing = await _context.Movies
            .Include(m => m.MovieGenres)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (existing == null) return null;

        existing.Title = movie.Title;
        existing.Description = movie.Description;
        existing.Duration = movie.Duration;
        existing.Language = movie.Language;
        existing.ReleaseDate = movie.ReleaseDate;
        existing.PosterUrl = movie.PosterUrl;
        existing.UpdatedAt = DateTime.UtcNow;

        var requestedGenreIds = genreIds
            .Distinct()
            .ToHashSet();

        var movieGenresToRemove = existing.MovieGenres
            .Where(movieGenre => !requestedGenreIds.Contains(movieGenre.GenreId))
            .ToList();

        if (movieGenresToRemove.Count > 0)
        {
            _context.MovieGenres.RemoveRange(movieGenresToRemove);
        }

        var currentGenreIds = existing.MovieGenres
            .Select(movieGenre => movieGenre.GenreId)
            .ToHashSet();

        var movieGenresToAdd = requestedGenreIds
            .Except(currentGenreIds)
            .Select(genreId => new MovieGenre
            {
                MovieId = existing.Id,
                GenreId = genreId
            });

        foreach (var movieGenre in movieGenresToAdd)
        {
            existing.MovieGenres.Add(movieGenre);
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null) return false;

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<MovieEntity>> GetByGenreAsync(Guid genreId)
    {
        return await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.Showtimes)
            .Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId))
            .ToListAsync();
    }
}




