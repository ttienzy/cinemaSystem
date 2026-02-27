using Application.Common.Interfaces.Persistence;
using Application.Common.Exceptions;
using Domain.Entities.MovieAggregate;
using MediatR;

namespace Application.Features.Movies.Commands.DeleteMovie
{
    public record DeleteMovieCommand(Guid Id) : IRequest;

    public class DeleteMovieHandler(IMovieRepository movieRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<DeleteMovieCommand>
    {
        public async Task Handle(DeleteMovieCommand request, CancellationToken ct)
        {
            var movie = await movieRepo.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Movie), request.Id);

            movieRepo.Delete(movie);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
