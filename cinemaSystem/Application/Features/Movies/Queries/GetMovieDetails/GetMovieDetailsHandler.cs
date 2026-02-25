using Application.Common.Interfaces.Persistence;
using Application.Common.Exceptions;
using Shared.Models.DataModels.MovieDtos;
using MediatR;
using Domain.Entities.MovieAggregate;

namespace Application.Features.Movies.Queries.GetMovieDetails
{
    public record GetMovieDetailsQuery(Guid MovieId) : IRequest<MovieDetailsResponse>;

    public class GetMovieDetailsHandler(IMovieRepository movieRepo) 
        : IRequestHandler<GetMovieDetailsQuery, MovieDetailsResponse>
    {
        public async Task<MovieDetailsResponse> Handle(GetMovieDetailsQuery request, CancellationToken ct)
        {
            var movie = await movieRepo.GetByIdWithDetailsAsync(request.MovieId, ct)
                ?? throw new NotFoundException(nameof(Movie), request.MovieId);

            return new MovieDetailsResponse
            {
                Movie = new MovieDetail
                {
                    Title = movie.Title,
                    DurationMinutes = movie.DurationMinutes,
                    ReleaseDate = movie.ReleaseDate,
                    PosterUrl = movie.PosterUrl,
                    Description = movie.Description,
                    CreatedAt = movie.CreatedAt
                },
                Genres = movie.MovieGenres.Select(mg => mg.Genre).ToList(),
                Certifications = new(),
                Copyrights = new(),
                CastCrews = new()
            };
        }
    }
}
