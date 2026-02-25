using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Cinemas.Queries.GetAllCinemas
{
    public record GetAllCinemasQuery() : IRequest<List<CinemaDto>>;

    public record CinemaDto(
        Guid Id,
        string Name,
        string Address,
        string Phone,
        string Email,
        string Description);

    public class GetAllCinemasHandler(ICinemaRepository cinemaRepo) 
        : IRequestHandler<GetAllCinemasQuery, List<CinemaDto>>
    {
        public async Task<List<CinemaDto>> Handle(GetAllCinemasQuery request, CancellationToken ct)
        {
            var cinemas = await cinemaRepo.GetAllAsync(ct);
            return cinemas.Select(c => new CinemaDto(
                c.Id,
                c.CinemaName,
                c.Address,
                c.Phone ?? "",
                c.Email ?? "",
                c.Image ?? "")).ToList();
        }
    }
}
