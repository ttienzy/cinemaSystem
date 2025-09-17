using Application.Interfaces.Persistences.Repo;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.MovieAggregate;
using Domain.Entities.ShowtimeAggregate;
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
    }
}
