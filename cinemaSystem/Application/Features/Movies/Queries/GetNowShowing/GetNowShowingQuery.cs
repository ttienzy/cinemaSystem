using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.MovieDtos;

namespace Application.Features.Movies.Queries.GetNowShowing
{
    public record GetNowShowingQuery(DateTime? Date = null) : IRequest<List<MovieSummaryResponse>>;

    public class GetNowShowingHandler(IMovieRepository movieRepo)
        : IRequestHandler<GetNowShowingQuery, List<MovieSummaryResponse>>
    {
        public async Task<List<MovieSummaryResponse>> Handle(GetNowShowingQuery request, CancellationToken ct)
        {
            var date = request.Date ?? DateTime.UtcNow;
            var movies = await movieRepo.GetNowShowingAsync(date, ct);

            return movies.Select(m => new MovieSummaryResponse
            {
                Id = m.Id,
                Title = m.Title,
                PosterUrl = m.PosterUrl,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate,
                Description = m.Description,
                Genres = m.MovieGenres.Select(mg => mg.Genre?.GenreName ?? "").ToList(),
                Trailer = m.Trailer,
                AgeRating = m.Rating
            }).ToList();
        }
    }
}
