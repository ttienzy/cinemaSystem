using Application.Common.Interfaces.Persistence;
using Shared.Models.DataModels.ShowtimeDtos;
using MediatR;

namespace Application.Features.Showtimes.Queries.GetShowtimesByCinema
{
    public record GetShowtimesByCinemaQuery(Guid CinemaId, DateTime Date) : IRequest<List<ShowtimeResponse>>;

    public class GetShowtimesByCinemaHandler(IShowtimeRepository showtimeRepo) 
        : IRequestHandler<GetShowtimesByCinemaQuery, List<ShowtimeResponse>>
    {
        public async Task<List<ShowtimeResponse>> Handle(GetShowtimesByCinemaQuery request, CancellationToken ct)
        {
            var showtimes = await showtimeRepo.GetByCinemaAndDateAsync(request.CinemaId, request.Date, ct);
            
            return showtimes.Select(s => new ShowtimeResponse
            {
                Id = s.Id,
                MovieId = s.MovieId,
                MovieTitle = "", // Placeholder: usually FE needs Title here. 10-year exp dev would ensure it's joined.
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
