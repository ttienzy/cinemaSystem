using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Application.Common.Exceptions;
using Shared.Models.DataModels.ShowtimeDtos;
using MediatR;
using Domain.Entities.ShowtimeAggregate;

namespace Application.Features.Showtimes.Queries.GetSeatingPlan
{
    public record GetSeatingPlanQuery(Guid ShowtimeId) : IRequest<ShowtimeSeatingPlanResponse>;

    public class GetSeatingPlanHandler(
        IShowtimeRepository showtimeRepo,
        ICinemaRepository cinemaRepo,
        ISeatLockService seatLock) : IRequestHandler<GetSeatingPlanQuery, ShowtimeSeatingPlanResponse>
    {
        public async Task<ShowtimeSeatingPlanResponse> Handle(GetSeatingPlanQuery request, CancellationToken ct)
        {
            // 1. Get showtime with pricing
            var showtime = await showtimeRepo.GetByIdWithPricingAsync(request.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Showtime), request.ShowtimeId);

            // 2. Get screen with all seats
            var screen = await cinemaRepo.GetScreenWithSeatsAsync(showtime.ScreenId, ct)
                ?? throw new NotFoundException("Screen", showtime.ScreenId);

            // 3. Get transient locks from Redis
            var seatIds = screen.Seats.Select(s => s.Id).ToList();
            var redisStatuses = await seatLock.GetSeatStatusesAsync(showtime.Id, seatIds, ct);

            // 4. Map to response
            return new ShowtimeSeatingPlanResponse
            {
                ShowtimeInfo = new ShowtimeInfoResponse
                {
                    Id = showtime.Id,
                    ShowDate = showtime.ShowDate,
                    ActualStartTime = showtime.ActualStartTime,
                    ActualEndTime = showtime.ActualEndTime,
                    ScreenName = screen.ScreenName,
                    MovieTitle = "", // Should be hydrated if needed, for now placeholder
                    CinemaName = ""  // Placeholder
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
                    Status = s.IsBlocked ? Domain.Entities.CinemaAggregate.Enum.SeatStatus.Unavailable :
                             (redisStatuses.TryGetValue(s.Id, out var lockStatus) && lockStatus == SeatLockStatus.Locked 
                                ? Domain.Entities.CinemaAggregate.Enum.SeatStatus.Booked : Domain.Entities.CinemaAggregate.Enum.SeatStatus.Available),
                    LastUpdated = DateTime.UtcNow
                }).ToList()
            };
        }
    }
}
