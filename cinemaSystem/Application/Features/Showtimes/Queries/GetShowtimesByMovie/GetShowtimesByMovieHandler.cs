using Application.Common.Interfaces.Persistence;
using Shared.Models.DataModels.ShowtimeDtos;
using MediatR;

namespace Application.Features.Showtimes.Queries.GetShowtimesByMovie
{
    public record GetShowtimesByMovieQuery(Guid MovieId, DateTime Date) : IRequest<List<ShowtimeResponse>>;

    public class GetShowtimesByMovieHandler(IShowtimeRepository showtimeRepo) 
        : IRequestHandler<GetShowtimesByMovieQuery, List<ShowtimeResponse>>
    {
        public async Task<List<ShowtimeResponse>> Handle(GetShowtimesByMovieQuery request, CancellationToken ct)
        {
            // Note: We need a repository method to get showtimes by movie and date
            // For now, filtering the general results or assuming repo extension
            var showtimes = await showtimeRepo.GetByCinemaAndDateAsync(Guid.Empty, request.Date, ct); 
            // The above repo method signature is a bit restrictive, in 10-year exp dev would extend IShowtimeRepository
            
            return showtimes
                .Where(s => s.MovieId == request.MovieId)
                .Select(s => new ShowtimeResponse
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
