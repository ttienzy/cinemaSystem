using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.Genres.Commands.Create
{
    public record CreateGenreCommand(CreateGenreRequest Request) : IRequest<Guid>;

    public class CreateGenreHandler(IGenreRepository genreRepo, IUnitOfWork unitOfWork) : IRequestHandler<CreateGenreCommand, Guid>
    {
        public async Task<Guid> Handle(CreateGenreCommand request, CancellationToken ct)
        {
            var genre = new Genre(request.Request.GenreName, request.Request.Description);
            
            await genreRepo.AddAsync(genre, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return genre.Id;
        }
    }
}
