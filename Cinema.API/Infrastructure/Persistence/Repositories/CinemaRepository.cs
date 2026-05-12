using Cinema.API.Infrastructure.Persistence;
using Cinema.API.Domain.Entities;
using CinemaEntity = Cinema.API.Domain.Entities.Cinema;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Infrastructure.Persistence.Repositories;

public class CinemaRepository : ICinemaRepository
{
    private readonly CinemaDbContext _context;

    public CinemaRepository(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<List<CinemaEntity>> GetAllAsync()
    {
        return await _context.Cinemas
            .Include(c => c.CinemaHalls)
            .ThenInclude(hall => hall.Seats)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<CinemaEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Cinemas
            .Include(c => c.CinemaHalls)
            .ThenInclude(ch => ch.Seats)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CinemaEntity> CreateAsync(CinemaEntity cinema)
    {
        _context.Cinemas.Add(cinema);
        await _context.SaveChangesAsync();
        return cinema;
    }

    public async Task<CinemaEntity?> UpdateAsync(Guid id, CinemaEntity cinema)
    {
        var existing = await _context.Cinemas.FindAsync(id);
        if (existing == null) return null;

        existing.UpdateDetails(cinema.Name, cinema.Address, cinema.City);

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var cinema = await _context.Cinemas.FindAsync(id);
        if (cinema == null) return false;

        _context.Cinemas.Remove(cinema);
        await _context.SaveChangesAsync();
        return true;
    }
}



