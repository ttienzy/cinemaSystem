using Cinema.API.Infrastructure.Persistence;
using Cinema.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Infrastructure.Persistence.Repositories;

public class CinemaHallRepository : ICinemaHallRepository
{
    private readonly CinemaDbContext _context;

    public CinemaHallRepository(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<List<CinemaHall>> GetByCinemaIdAsync(Guid cinemaId)
    {
        return await _context.CinemaHalls
            .Where(ch => ch.CinemaId == cinemaId)
            .Include(ch => ch.Seats)
            .ToListAsync();
    }

    public async Task<CinemaHall?> GetByIdAsync(Guid id)
    {
        return await _context.CinemaHalls
            .Include(ch => ch.Cinema)
            .Include(ch => ch.Seats)
            .FirstOrDefaultAsync(ch => ch.Id == id);
    }

    public async Task<List<CinemaHall>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var hallIds = ids
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (hallIds.Count == 0)
        {
            return [];
        }

        return await _context.CinemaHalls
            .Include(ch => ch.Cinema)
            .Include(ch => ch.Seats)
            .Where(ch => hallIds.Contains(ch.Id))
            .ToListAsync();
    }

    public async Task<List<Seat>> GetSeatsByHallIdAsync(Guid hallId)
    {
        return await _context.Seats
            .Where(s => s.CinemaHallId == hallId)
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Number)
            .ToListAsync();
    }

    public async Task AddAsync(CinemaHall hall)
    {
        await _context.CinemaHalls.AddAsync(hall);
    }

    public void Update(CinemaHall hall)
    {
        _context.CinemaHalls.Update(hall);
    }

    public void Delete(CinemaHall hall)
    {
        _context.CinemaHalls.Remove(hall);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


