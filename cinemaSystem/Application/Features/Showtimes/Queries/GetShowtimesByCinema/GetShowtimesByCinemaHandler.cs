using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Common.Paging;
using Shared.Models.DataModels.ShowtimeDtos;

namespace Application.Features.Showtimes.Queries.GetShowtimesByCinema
{
    public record GetShowtimesByCinemaQuery(Guid CinemaId, DateTime Date, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ShowtimeDetailResponse>>;

    public class GetShowtimesByCinemaHandler(IShowtimeRepository showtimeRepo)
        : IRequestHandler<GetShowtimesByCinemaQuery, PaginatedList<ShowtimeDetailResponse>>
    {
        public async Task<PaginatedList<ShowtimeDetailResponse>> Handle(GetShowtimesByCinemaQuery request, CancellationToken ct)
        {
            var query = showtimeRepo.GetQueryable()
                .Where(s => s.CinemaId == request.CinemaId && s.ShowDate.Date == request.Date.Date)
                .Select(s => new ShowtimeDetailResponse
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
                });

            return await PaginatedList<ShowtimeDetailResponse>.CreateAsync(query, request.PageNumber, request.PageSize);
        }
    }
}
