using Application.Interfaces.Persistences.Repo;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.BookingDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingContext _bookingContext;
        public BookingRepository(BookingContext bookingContext)
        {
            _bookingContext = bookingContext;
        }

        public async Task<BookingCheckedInResponse> CheckInBookingAsync(Guid bookingId)
        {
            var query = from b in _bookingContext.Bookings
                        .Where(b => b.Id == bookingId)
                        join s in _bookingContext.Showtimes on b.ShowtimeId equals s.Id
                        join m in _bookingContext.Movies on s.MovieId equals m.Id
                        join c in _bookingContext.Cinemas on s.CinemaId equals c.Id
                        select new BookingCheckedInResponse
                        {
                            TotalAmount = b.TotalAmount,
                            TotalTickets = b.TotalTickets,
                            BookingTime = b.BookingTime,
                            MovieTitle = m.Title,
                            CinemaName = c.CinemaName,
                            Status = b.Status,
                            ActualEndTime = s.ActualEndTime,
                            ActualStartTime = s.ActualStartTime,
                            IsCheckedIn = b.IsCheckedIn,
                            ScreenName = (from scr in _bookingContext.Screens
                                          where scr.Id == s.ScreenId
                                          select scr.ScreenName).FirstOrDefault() ?? "Unknown",
                            SeatsList = (from bt in _bookingContext.BookingTickets
                                         join seat in _bookingContext.Seats on bt.SeatId equals seat.Id
                                         where bt.BookingId == b.Id
                                         select $"{seat.RowName}{seat.Number}").ToList()
                        };
            return await query.FirstOrDefaultAsync() ?? new BookingCheckedInResponse();
        }

        public async Task<IEnumerable<PurchaseResponse>> PurchaseHistoryAsync(Guid userId)
        {
            var query = from b in _bookingContext.Bookings
                        .Where(b => b.CustomerId == userId)
                        join s in _bookingContext.Showtimes on b.ShowtimeId equals s.Id
                        join m in _bookingContext.Movies on s.MovieId equals m.Id
                        join c in _bookingContext.Cinemas on s.CinemaId equals c.Id
                        orderby b.BookingTime descending
                        select new PurchaseResponse
                        {
                            BookingId = b.Id,
                            TotalAmount = b.TotalAmount,
                            TotalTickets = b.TotalTickets,
                            BookingTime = b.BookingTime,
                            ShowTime = s.ActualStartTime,
                            MovieTitle = m.Title,
                            CinemaName = c.CinemaName,
                            Status = b.Status,
                        };
                        
            return await query.ToListAsync();
        }
    }
}
