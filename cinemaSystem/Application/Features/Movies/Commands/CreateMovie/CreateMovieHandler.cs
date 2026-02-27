using Application.Common.Interfaces.Persistence;
using Domain.Entities.MovieAggregate;
using Domain.Entities.MovieAggregate.Enum;
using MediatR;
using Shared.Models.DataModels.MovieDtos;

namespace Application.Features.Movies.Commands.CreateMovie
{
    public record CreateMovieCommand(MovieUpsertRequest Request) : IRequest<Guid>;

    public class CreateMovieHandler(IMovieRepository movieRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<CreateMovieCommand, Guid>
    {
        public async Task<Guid> Handle(CreateMovieCommand request, CancellationToken ct)
        {
            var movie = new Movie(
                request.Request.Title,
                request.Request.DurationMinutes,
                request.Request.ReleaseDate,
                request.Request.Status,
                request.Request.Description ?? string.Empty,
                request.Request.RatingStatus,
                request.Request.PosterUrl,
                request.Request.Trailer
            );

            await movieRepo.AddAsync(movie, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return movie.Id;
        }
    }
}
