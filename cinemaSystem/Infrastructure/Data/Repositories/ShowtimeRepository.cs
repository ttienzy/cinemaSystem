using Application.Interfaces.Persistences.Repo;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.MovieAggregate;
using Domain.Entities.ShowtimeAggregate;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    internal class ShowtimeRepository : IShowtimeRepository
    {
        private readonly BookingContext _context;
        public ShowtimeRepository(BookingContext context)
        {
            _context = context;
        }
        public async Task<List<ShowtimeFeaturedResponse>> GetShowtimeByQuerryAsync(Guid? cinemaId, DateTime showDate)
        {
            var query = _context.Showtimes
                .Where(st => st.CinemaId == cinemaId &&
                             st.ShowDate.Date == showDate.Date)
                .Join(_context.Movies, st => st.MovieId, m => m.Id, (st, m) => new { st, m })
                .Join(_context.Cinemas, x => x.st.CinemaId, c => c.Id, (x, c) => new { x.st, x.m, c })
                .GroupBy(x => new
                {
                    x.m.Title,
                    x.m.DurationMinutes,
                    x.m.Rating,
                    x.m.PosterUrl,
                    x.m.Trailer,
                    x.m.Description,
                    x.c.CinemaName,
                    x.st.ShowDate.Date,
                })
                .Select(grouped => new ShowtimeFeaturedResponse
                {
                    Title = grouped.Key.Title,
                    DurationMinutes = grouped.Key.DurationMinutes,
                    AgeRating = grouped.Key.Rating,
                    PostUrl = grouped.Key.PosterUrl,
                    Trailer = grouped.Key.Trailer,
                    Description = grouped.Key.Description,
                    CinemaName = grouped.Key.CinemaName,
                    ReleaseDate = grouped.Key.Date,
                    ScreeningSlots = grouped.Select(x => new ScreeningSlot
                    {
                        ShowtimeId = x.st.Id,
                        ActualStartTime = x.st.ActualStartTime,
                        ActualEndTime = x.st.ActualEndTime
                    }).ToList()
                });
            return await query.ToListAsync();
        }

        public async Task<ShowtimeFeaturedResponse> GetShowtimeFeaturedAsync(ShowtimeQueryParameters parameters)
        {        
            var query = _context.Showtimes
                .Where(st => st.CinemaId == parameters.CinemaId &&
                             st.MovieId == parameters.MovieId &&
                             st.ShowDate.Date == parameters.ShowDate.Date)
                .Join(_context.Movies, st => st.MovieId, m => m.Id, (st, m) => new { st, m })
                .Join(_context.Cinemas, x => x.st.CinemaId, c => c.Id, (x, c) => new { x.st, x.m, c })
                .GroupBy(x => new
                {
                    x.m.Title,
                    x.m.DurationMinutes,
                    x.m.Rating,
                    x.m.PosterUrl,
                    x.m.Trailer,
                    x.m.Description,
                    x.c.CinemaName,
                    x.st.ShowDate.Date,
                })
                .Select(grouped => new ShowtimeFeaturedResponse
                {
                    Title = grouped.Key.Title,
                    DurationMinutes = grouped.Key.DurationMinutes,
                    AgeRating = grouped.Key.Rating,
                    PostUrl = grouped.Key.PosterUrl,
                    Trailer = grouped.Key.Trailer,
                    Description = grouped.Key.Description,
                    CinemaName = grouped.Key.CinemaName,
                    ReleaseDate = grouped.Key.Date,
                    ScreeningSlots = grouped.Select(x => new ScreeningSlot
                    {
                        ShowtimeId = x.st.Id,
                        ActualStartTime = x.st.ActualStartTime,
                        ActualEndTime = x.st.ActualEndTime
                    }).ToList()
                });

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ShowtimePerformanceDto>> GetShowtimePerformanceAsync(Guid cinemaId)
        {
            var sql = @"
    SELECT 
        s.Id AS ShowtimeId,
        m.Title,
        scr.ScreenName,
        scr.Type AS ScreenType,
        s.ShowDate,
        ts.StartTime AS SlotStartTime,
        ts.EndTime AS SlotEndTime,
        s.ActualStartTime,
        s.ActualEndTime,
        s.Status,      
        pt.TierName AS PricingTier,
        pt.Multiplier,
        COUNT_BIG(b.Id) AS TotalBookings,
        AVG(sp.FinalPrice) AS AvgTicketPrice
    FROM 
        Showtimes s
        INNER JOIN Movies m ON s.MovieId = m.Id
        INNER JOIN Screens scr ON s.ScreenId = scr.Id
        INNER JOIN TimeSlots ts ON s.SlotId = ts.Id
        INNER JOIN PricingTiers pt ON s.PricingTierId = pt.Id
        LEFT JOIN ShowtimePricings sp ON s.Id = sp.ShowtimeId
        LEFT JOIN Bookings b ON s.Id = b.ShowtimeId
    WHERE 
        s.CinemaId = @CinemaId
        AND s.Status = 'Scheduled'
        AND s.ShowDate >= CAST(GETDATE() AS DATE)
    GROUP BY 
        s.Id,
        m.Title,
        scr.ScreenName,
        scr.Type,
        s.ShowDate,
        ts.StartTime,
        ts.EndTime,
        s.ActualStartTime,
        s.ActualEndTime,
        s.Status, 
        pt.TierName,
        pt.Multiplier
    ORDER BY 
        s.ActualStartTime";

            // ✅ Tạo tham số đúng cách
            var param = new SqlParameter("@CinemaId", cinemaId);

            var result = await _context.Database
                .SqlQueryRaw<ShowtimePerformanceDto>(sql, param)
                .ToListAsync();

            return result;
        }
    }
}
