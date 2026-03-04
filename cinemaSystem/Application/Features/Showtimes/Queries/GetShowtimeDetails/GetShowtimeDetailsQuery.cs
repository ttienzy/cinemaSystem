using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.ShowtimeDtos;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Showtimes.Queries.GetShowtimeDetails
{
    public record GetShowtimeDetailsQuery(Guid ShowtimeId) : IRequest<ShowtimeDetailResponse>;

    public class GetShowtimeDetailsHandler(
        IShowtimeRepository showtimeRepo,
        IMovieRepository movieRepo,
        ICinemaRepository cinemaRepo) : IRequestHandler<GetShowtimeDetailsQuery, ShowtimeDetailResponse>
    {
        public async Task<ShowtimeDetailResponse> Handle(GetShowtimeDetailsQuery request, CancellationToken ct)
        {
            // 1. Get showtime with pricing
            var showtime = await showtimeRepo.GetByIdWithPricingAsync(request.ShowtimeId, ct)
                ?? throw new NotFoundException("Showtime", request.ShowtimeId);

            // 2. Load Movie
            var movie = await movieRepo.GetByIdAsync(showtime.MovieId, ct);

            // 3. Load Cinema with Screens
            var cinema = await cinemaRepo.GetByIdWithScreensAsync(showtime.CinemaId, ct);
            var screen = cinema?.GetScreenById(showtime.ScreenId);

            // 4. Load TimeSlot
            var timeSlot = await showtimeRepo.GetTimeSlotAsync(showtime.SlotId, ct);

            // 5. Load PricingTier
            var pricingTier = await showtimeRepo.GetPricingTierAsync(showtime.PricingTierId, ct);

            // 6. Load all SeatTypes for mapping (batch query once)
            var allSeatTypes = await showtimeRepo.GetSeatTypesAsync(ct);
            var seatTypeDict = allSeatTypes.ToDictionary(s => s.Id);

            // 7. Build response with REAL data from DB
            var response = new ShowtimeDetailResponse
            {
                Id = showtime.Id,
                MovieId = showtime.MovieId,
                MovieTitle = movie?.Title ?? "Unknown",
                MoviePosterUrl = movie?.PosterUrl ?? "",
                MovieDurationMinutes = movie?.DurationMinutes ?? 0,
                CinemaId = showtime.CinemaId,
                CinemaName = cinema?.CinemaName ?? "Unknown",
                ScreenId = showtime.ScreenId,
                ScreenName = screen?.ScreenName ?? "Unknown",
                SlotId = showtime.SlotId,
                SlotName = timeSlot != null ? $"{timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}" : "Unknown",
                PricingTierId = showtime.PricingTierId,
                PricingTierName = pricingTier?.TierName ?? "Unknown",
                PricingTierMultiplier = pricingTier?.Multiplier ?? 1m,
                ShowDate = showtime.ShowDate,
                ActualStartTime = showtime.ActualStartTime,
                ActualEndTime = showtime.ActualEndTime,
                Status = showtime.Status,
                ShowtimePricings = showtime.ShowtimePricings.Select(p =>
                {
                    seatTypeDict.TryGetValue(p.SeatTypeId, out var seatType);
                    return new ShowtimePricingResponse
                    {
                        Id = p.Id,
                        SeatTypeId = p.SeatTypeId,
                        SeatTypeName = seatType?.TypeName ?? "Unknown",
                        FinalPrice = p.FinalPrice
                    };
                }).ToList()
            };

            return response;
        }
    }
}
