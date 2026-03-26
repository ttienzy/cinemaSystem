using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Application.Common.Exceptions;
using Shared.Models.DataModels.ShowtimeDtos;
using MediatR;
using Domain.Entities.ShowtimeAggregate;
using Application.Common.Constants;
using Domain.Entities.CinemaAggregate.Enum;

namespace Application.Features.Showtimes.Queries.GetSeatingPlan
{
    public record GetSeatingPlanQuery(Guid ShowtimeId) : IRequest<ShowtimeSeatingPlanResponse>;

    public class GetSeatingPlanHandler(
        IShowtimeRepository showtimeRepo,
        ICinemaRepository cinemaRepo,
        IMovieRepository movieRepo,
        ICacheService redisCache,
        ISeatLockService seatLock) : IRequestHandler<GetSeatingPlanQuery, ShowtimeSeatingPlanResponse>
    {
        public async Task<ShowtimeSeatingPlanResponse> Handle(GetSeatingPlanQuery request, CancellationToken ct)
        {
            // Get seatingplan on redis, if not exist get from db and set to redis with expiration time of 15 minutes
            var cacheKey = RedisKey.SeatingPlan(request.ShowtimeId);
            var cachedResponse = await redisCache.GetAsync<ShowtimeSeatingPlanResponse>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            // 1. Get showtime with pricing
            var showtime = await showtimeRepo.GetByIdWithPricingAsync(request.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Showtime), request.ShowtimeId);

            // 2. Get cinema and screen with all seats
            var cinema = await cinemaRepo.GetByIdAsync(showtime.CinemaId, ct)
                ?? throw new NotFoundException("Cinema", showtime.CinemaId);

            var screen = await cinemaRepo.GetScreenWithSeatsAsync(showtime.ScreenId, ct)
                ?? throw new NotFoundException("Screen", showtime.ScreenId);

            var movie = await movieRepo.GetByIdAsync(showtime.MovieId, ct)
                ?? throw new NotFoundException("Movie", showtime.MovieId);


            // 3. Get transient locks from Redis
            //var seatIds = screen.Seats.Select(s => s.Id).ToList();
            //var redisStatuses = await seatLock.GetSeatStatusesAsync(showtime.Id, seatIds, ct);

            // 4. Map to response
            var result = new ShowtimeSeatingPlanResponse
            {
                ShowtimeInfo = new ShowtimeInfoResponse
                {
                    Id = showtime.Id,
                    ShowDate = showtime.ShowDate,
                    ActualStartTime = showtime.ActualStartTime,
                    ActualEndTime = showtime.ActualEndTime,
                    ScreenName = screen.ScreenName,
                    MovieTitle = movie.Title,
                    CinemaName = cinema.CinemaName
                },
                Pricings = showtime.ShowtimePricings.Select(p => new ShowtimePricingInfoResponse
                {
                    SeatTypeId = p.SeatTypeId,
                    FinalPrice = p.FinalPrice
                }).ToList(),
                Seats = screen.Seats.Select(s => new SeatInfoResponse
                {
                    Id = s.Id,
                    RowName = s.RowName,
                    Number = s.Number,
                    SeatTypeId = s.SeatTypeId,
                    // Status priority: Blocked (DB) > Locked (Redis) > Available
                    Status = s.IsBlocked ? SeatStatus.Unavailable :
                             SeatStatus.Available,
                    LastUpdated = DateTime.UtcNow
                }).ToList()
            };

            // 5. Cache the result in Redis for 15 minutes
            await redisCache.SetAsync<ShowtimeSeatingPlanResponse>(cacheKey, result, TimeSpan.FromMinutes(RedisTTL.TTLInMinutes));

            return result;
        }
    }
}
