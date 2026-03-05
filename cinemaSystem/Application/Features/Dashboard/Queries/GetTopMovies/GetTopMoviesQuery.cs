using MediatR;
using Shared.Models.DataModels.DashboardDtos;

namespace Application.Features.Dashboard.Queries.GetTopMovies
{
    /// <summary>
    /// Truy vấn top phim ăn khách — sắp xếp theo doanh thu hoặc lượng vé bán.
    /// </summary>
    public record GetTopMoviesQuery(
        int Limit = 10,
        DateTime? From = null,
        DateTime? To = null,
        Guid? CinemaId = null
    ) : IRequest<List<TopMovieDto>>;
}
