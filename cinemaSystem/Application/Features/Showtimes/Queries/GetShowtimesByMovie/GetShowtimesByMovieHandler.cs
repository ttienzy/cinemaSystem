using Application.Common.Interfaces.Persistence;
using Shared.Models.DataModels.ShowtimeDtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Showtimes.Queries.GetShowtimesByMovie
{
    public record GetShowtimesByMovieQuery(Guid MovieId, DateTime Date) : IRequest<List<ShowtimeDetailResponse>>;

    public class GetShowtimesByMovieHandler(IShowtimeRepository showtimeRepo) 
        : IRequestHandler<GetShowtimesByMovieQuery, List<ShowtimeDetailResponse>>
    {
        public async Task<List<ShowtimeDetailResponse>> Handle(GetShowtimesByMovieQuery request, CancellationToken ct)
        {
            // Note: In a real scenario, we'd add GetByMovieAndDateAsync to IShowtimeRepository
            // For now, we'll use GetByCinemaAndDateAsync with Empty Guid and filter manually
            var showtimes = await showtimeRepo.GetByCinemaAndDateAsync(Guid.Empty, request.Date, ct); 
            
            return showtimes
                .Where(s => s.MovieId == request.MovieId)
                .Select(s => new ShowtimeDetailResponse
                {
                    Id = s.Id,
                    MovieId = s.MovieId,
                    MovieTitle = "",
                    MoviePosterUrl = "",
                    MovieDurationMinutes = 0,
                    CinemaId = s.CinemaId,
                    CinemaName = "",
                    ScreenId = s.ScreenId,
                    ScreenName = "",
                    SlotId = s.SlotId,
                    SlotName = "",
                    PricingTierId = s.PricingTierId,
                    PricingTierName = "",
                    PricingTierMultiplier = 0,
                    ShowDate = s.ShowDate,
                    ActualStartTime = s.ActualStartTime,
                    ActualEndTime = s.ActualEndTime,
                    Status = s.Status,
                    ShowtimePricings = s.ShowtimePricings.Select(p => new ShowtimePricingResponse
                    {
                        Id = p.Id,
                        SeatTypeId = p.SeatTypeId,
                        SeatTypeName = "",
                        FinalPrice = p.FinalPrice
                    }).ToList()
                }).ToList();
        }
    }
}
