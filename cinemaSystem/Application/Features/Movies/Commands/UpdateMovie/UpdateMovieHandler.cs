using Application.Common.Interfaces.Persistence;
using Application.Common.Exceptions;
using Domain.Entities.MovieAggregate;
using MediatR;
using Shared.Models.DataModels.MovieDtos;

namespace Application.Features.Movies.Commands.UpdateMovie
{
    public record UpdateMovieCommand(Guid Id, MovieUpsertRequest Request) : IRequest;

    public class UpdateMovieHandler(IMovieRepository movieRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<UpdateMovieCommand>
    {
        public async Task Handle(UpdateMovieCommand request, CancellationToken ct)
        {
            var movie = await movieRepo.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Movie), request.Id);

            movie.UpdateDetail(
                request.Request.Title,
                request.Request.DurationMinutes,
                request.Request.ReleaseDate,
                request.Request.Description ?? string.Empty,
                request.Request.PosterUrl,
                request.Request.RatingStatus,
                request.Request.Trailer
            );

            movieRepo.Update(movie);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
