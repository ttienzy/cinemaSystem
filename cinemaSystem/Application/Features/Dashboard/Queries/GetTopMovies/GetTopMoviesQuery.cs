using MediatR;
using Shared.Models.DataModels.DashboardDtos;

namespace Application.Features.Dashboard.Queries.GetTopMovies
{
    /// <summary>
    /// Query for top-grossing movies — sorted by revenue or ticket volume.
    /// </summary>
    public record GetTopMoviesQuery(
        int Limit = 10,
        DateTime? From = null,
        DateTime? To = null,
        Guid? CinemaId = null
    ) : IRequest<List<TopMovieDto>>;
}
