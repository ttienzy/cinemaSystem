using Application.Interfaces.Persistences.Repo;
using Domain.Entities.ShowtimeAggregate;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<List<ShowtimeDetailsResponse>> GetShowtimeByQuerryAsync(Guid? cinemaId, Guid? movieId, DateTime showDate)
        {
            var query = from st in _context.Showtimes
                        join m in _context.Movies on st.MovieId equals m.Id
                        join s in _context.Screens on st.ScreenId equals s.Id
                        where (cinemaId == null || st.CinemaId == cinemaId) &&
                              (movieId == null || st.MovieId == movieId) &&
                              st.ShowDate.Date == showDate.Date
                        select new ShowtimeDetailsResponse
                        {
                            Id = st.Id,
                            MovieId = st.MovieId,
                            CinemaId = st.CinemaId,
                            ScreendId = st.ScreenId,
                            SlotId = st.SlotId,
                            PricingTierId = st.PricingTierId,
                            ShowDate = st.ShowDate,
                            ActualStartTime = st.ActualStartTime,
                            ActualEndTime = st.ActualEndTime,
                            MovieTitle = m.Title,
                            MoviePosterUrl = m.PosterUrl,
                            MovieDurationMinutes = m.DurationMinutes,
                            ScreenName = s.ScreenName
                        };
            return await query.OrderByDescending(x => x.ShowDate).ToListAsync();
        }

        
    }
}
