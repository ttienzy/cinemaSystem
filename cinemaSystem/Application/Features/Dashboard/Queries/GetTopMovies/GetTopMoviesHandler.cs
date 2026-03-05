using Application.Common.Interfaces.Persistence;
using Domain.Entities.BookingAggregate.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.DashboardDtos;

namespace Application.Features.Dashboard.Queries.GetTopMovies
{
    /// <summary>
    /// Handler lấy top phim ăn khách — join Booking → Showtime → Movie
    /// để tổng hợp doanh thu và lượng vé theo từng phim.
    /// </summary>
    public class GetTopMoviesHandler(
        IBookingRepository bookingRepo,
        IShowtimeRepository showtimeRepo,
        IMovieRepository movieRepo)
        : IRequestHandler<GetTopMoviesQuery, List<TopMovieDto>>
    {
        public async Task<List<TopMovieDto>> Handle(GetTopMoviesQuery request, CancellationToken ct)
        {
            var from = request.From?.Date ?? DateTime.UtcNow.AddMonths(-1);
            var to = (request.To?.Date ?? DateTime.UtcNow).AddDays(1);

            // Lấy bookings đã hoàn thành trong khoảng thời gian
            var bookingsQuery = bookingRepo.GetQueryable()
                .Where(b => b.Status == BookingStatus.Completed
                    && b.BookingTime >= from && b.BookingTime < to);

            if (request.CinemaId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.CinemaId == request.CinemaId.Value);
            }

            // Join với Showtime để lấy MovieId, MovieTitle
            var topMovies = await bookingsQuery
                .Join(
                    showtimeRepo.GetQueryable(),
                    b => b.ShowtimeId,
                    s => s.Id,
                    (b, s) => new { b.TotalAmount, s.MovieId, b.TotalTickets })
                .Join(
                    movieRepo.GetQueryable(),
                    bs => bs.MovieId,
                    m => m.Id,
                    (bs, m) => new { bs.TotalAmount, bs.MovieId, MovieTitle = m.Title, Tickets = bs.TotalTickets }
                    )
                .GroupBy(x => new { x.MovieId, x.MovieTitle })
                .Select(g => new TopMovieDto
                {
                    MovieId = g.Key.MovieId,
                    Title = g.Key.MovieTitle ?? "N/A",
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalTicketsSold = g.Sum(x => x.Tickets),
                    ShowtimeCount = g.Select(x => x.MovieId).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(request.Limit)
                .ToListAsync(ct);

            return topMovies;
        }
    }
}
