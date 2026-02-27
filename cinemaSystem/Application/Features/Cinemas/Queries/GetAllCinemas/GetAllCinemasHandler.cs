using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.CinemaDtos;

namespace Application.Features.Cinemas.Queries.GetAllCinemas
{
    public record GetAllCinemasQuery() : IRequest<List<CinemaSummaryResponse>>;

    public class GetAllCinemasHandler(ICinemaRepository cinemaRepo) 
        : IRequestHandler<GetAllCinemasQuery, List<CinemaSummaryResponse>>
    {
        public async Task<List<CinemaSummaryResponse>> Handle(GetAllCinemasQuery request, CancellationToken ct)
        {
            var cinemas = await cinemaRepo.GetAllAsync(ct);
            return cinemas.Select(c => new CinemaSummaryResponse
            {
                CinemaId = c.Id,
                CinemaName = c.CinemaName,
                Address = c.Address,
                Phone = c.Phone ?? "",
                Image = c.Image ?? "",
                Screens = c.Screens.Count
            }).ToList();
        }
    }
}
