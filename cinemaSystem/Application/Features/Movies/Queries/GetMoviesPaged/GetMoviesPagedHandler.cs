using Application.Common.Interfaces.Persistence;
using Shared.Models.DataModels.MovieDtos;
using MediatR;

namespace Application.Features.Movies.Queries.GetMoviesPaged
{
    public record GetMoviesPagedQuery(
        string? Search, 
        Guid? GenreId, 
        Guid? CinemaId,
        string? Status, 
        int Page = 1, 
        int PageSize = 10) : IRequest<PagedMovieResponse>;

    public class PagedMovieResponse
    {
        public List<MovieSummaryResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class GetMoviesPagedHandler(IMovieRepository movieRepo) 
        : IRequestHandler<GetMoviesPagedQuery, PagedMovieResponse>
    {
        public async Task<PagedMovieResponse> Handle(GetMoviesPagedQuery request, CancellationToken ct)
        {
            var (items, total) = await movieRepo.GetPagedAsync(
                request.Search, request.GenreId, request.CinemaId, request.Status, request.Page, request.PageSize, ct);

            return new PagedMovieResponse
            {
                Items = items.Select(m => new MovieSummaryResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterUrl = m.PosterUrl,
                    DurationMinutes = m.DurationMinutes,
                    ReleaseDate = m.ReleaseDate,
                    Description = m.Description,
                    Genres = new List<string>(), // TODO: Map genres from entity if available
                    Trailer = m.Trailer ?? string.Empty,
                    AgeRating = m.Rating
                }).ToList(),
                TotalCount = total,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
