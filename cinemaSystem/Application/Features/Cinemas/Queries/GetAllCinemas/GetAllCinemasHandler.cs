using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Common.Paging;
using Shared.Models.DataModels.CinemaDtos;

namespace Application.Features.Cinemas.Queries.GetAllCinemas
{
    public record GetAllCinemasQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<CinemaSummaryResponse>>;

    public class GetAllCinemasHandler(ICinemaRepository cinemaRepo)
        : IRequestHandler<GetAllCinemasQuery, PaginatedList<CinemaSummaryResponse>>
    {
        public async Task<PaginatedList<CinemaSummaryResponse>> Handle(GetAllCinemasQuery request, CancellationToken ct)
        {
            var query = cinemaRepo.GetQueryable()
                .Select(c => new CinemaSummaryResponse
                {
                    CinemaId = c.Id,
                    CinemaName = c.CinemaName,
                    Address = c.Address,
                    Phone = c.Phone ?? "",
                    Image = c.Image ?? "",
                    Screens = c.Screens.Count
                });

            return await PaginatedList<CinemaSummaryResponse>.CreateAsync(query, request.PageNumber, request.PageSize);
        }
    }
}
