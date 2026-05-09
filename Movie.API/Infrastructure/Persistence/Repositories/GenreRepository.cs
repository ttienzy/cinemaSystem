using Microsoft.EntityFrameworkCore;
using Movie.API.Infrastructure.Persistence;
using Movie.API.Domain.Entities;

namespace Movie.API.Infrastructure.Persistence.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly MovieDbContext _context;

    public GenreRepository(MovieDbContext context)
    {
        _context = context;
    }

    public async Task<List<Genre>> GetAllAsync()
    {
        return await _context.Genres
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<Genre?> GetByIdAsync(Guid id)
    {
        return await _context.Genres
            .Include(g => g.MovieGenres)
            .ThenInclude(mg => mg.Movie)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Genre> CreateAsync(Genre genre)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
        return genre;
    }

    public async Task<Genre?> UpdateAsync(Guid id, Genre genre)
    {
        var existing = await _context.Genres.FindAsync(id);
        if (existing == null) return null;

        existing.Name = genre.Name;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null) return false;

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
        return true;
    }
}



