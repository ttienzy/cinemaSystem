using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.CinemaDtos;

namespace Application.Features.Cinemas.Queries.GetScreens
{
    public record GetScreensQuery(Guid CinemaId) : IRequest<List<ScreenResponse>>;

    public class GetScreensHandler(
        ICinemaRepository cinemaRepo)
        : IRequestHandler<GetScreensQuery, List<ScreenResponse>>
    {
        public async Task<List<ScreenResponse>> Handle(GetScreensQuery request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdWithScreensAsync(request.CinemaId, ct)
                ?? throw new KeyNotFoundException("Cinema not found");

            return cinema.Screens.Select(s => new ScreenResponse
            {
                Id = s.Id,
                ScreenName = s.ScreenName,
                Type = s.Type,
                Status = s.Status
            }).ToList();
        }
    }
}
