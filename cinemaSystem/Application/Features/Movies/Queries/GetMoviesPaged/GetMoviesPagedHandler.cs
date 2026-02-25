using Application.Common.Interfaces.Persistence;
using Shared.Models.DataModels.MovieDtos;
using MediatR;

namespace Application.Features.Movies.Queries.GetMoviesPaged
{
    public record GetMoviesPagedQuery(
        string? Search, 
        Guid? GenreId, 
        string? Status, 
        int Page = 1, 
        int PageSize = 10) : IRequest<PagedMovieResponse>;

    public class PagedMovieResponse
    {
        public List<MovieResponse> Items { get; set; } = new();
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
                request.Search, request.GenreId, request.Status, request.Page, request.PageSize, ct);

            return new PagedMovieResponse
            {
                Items = items.Select(m => new MovieResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterUrl = m.PosterUrl,
                    DurationMinutes = m.DurationMinutes,
                    ReleaseDate = m.ReleaseDate,
                    Description = m.Description,
                    Genres = new List<string>(),
                    Trailer = "",
                    AgeRating = Domain.Entities.MovieAggregate.Enum.RatingStatus.P
                }).ToList(),
                TotalCount = total,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
