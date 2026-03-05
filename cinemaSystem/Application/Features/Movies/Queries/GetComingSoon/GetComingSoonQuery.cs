using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.MovieDtos;

namespace Application.Features.Movies.Queries.GetComingSoon
{
    public record GetComingSoonQuery(DateTime? Date = null) : IRequest<List<MovieComingSoonResponse>>;

    public class GetComingSoonHandler(IMovieRepository movieRepo)
        : IRequestHandler<GetComingSoonQuery, List<MovieComingSoonResponse>>
    {
        public async Task<List<MovieComingSoonResponse>> Handle(GetComingSoonQuery request, CancellationToken ct)
        {
            var date = request.Date ?? DateTime.UtcNow;
            var movies = await movieRepo.GetComingSoonAsync(date, ct);

            return movies.Select(m => new MovieComingSoonResponse
            {
                MovieId = m.Id,
                Title = m.Title,
                Description = m.Description,
                PostUrl = m.PosterUrl,
                Genres = m.MovieGenres.Select(mg => mg.Genre?.GenreName ?? "").ToList(),
                Duration = m.DurationMinutes,
                Trailer = m.Trailer
            }).ToList();
        }
    }
}
