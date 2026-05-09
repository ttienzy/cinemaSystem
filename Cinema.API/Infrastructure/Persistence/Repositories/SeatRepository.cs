using Cinema.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Infrastructure.Persistence.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly CinemaDbContext _context;

    public SeatRepository(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Seat>> GetByHallIdAsync(Guid hallId)
    {
        return await _context.Seats
            .Where(s => s.CinemaHallId == hallId)
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Number)
            .ToListAsync();
    }

    public async Task<Seat?> GetByIdAsync(Guid id)
    {
        return await _context.Seats
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Seat>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var seatIds = ids.Where(id => id != Guid.Empty).Distinct().ToList();
        if (!seatIds.Any())
        {
            return [];
        }

        return await _context.Seats
            .Where(s => seatIds.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<Seat?> GetByRowAndNumberAsync(Guid hallId, string row, int number)
    {
        return await _context.Seats
            .FirstOrDefaultAsync(s => s.CinemaHallId == hallId && s.Row == row && s.Number == number);
    }

    public async Task AddAsync(Seat seat)
    {
        await _context.Seats.AddAsync(seat);
    }

    public async Task AddRangeAsync(IEnumerable<Seat> seats)
    {
        await _context.Seats.AddRangeAsync(seats);
    }

    public void Update(Seat seat)
    {
        _context.Seats.Update(seat);
    }

    public void Delete(Seat seat)
    {
        _context.Seats.Remove(seat);
    }

    public void DeleteRange(IEnumerable<Seat> seats)
    {
        _context.Seats.RemoveRange(seats);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
