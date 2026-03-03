using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.Genres.Commands.Update
{
    public record UpdateGenreCommand(Guid Id, UpdateGenreRequest Request) : IRequest;

    public class UpdateGenreHandler(IGenreRepository genreRepo, IUnitOfWork unitOfWork) : IRequestHandler<UpdateGenreCommand>
    {
        public async Task Handle(UpdateGenreCommand request, CancellationToken ct)
        {
            var genre = await genreRepo.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Genre), request.Id);

            genre.UpdateGenre(request.Request.GenreName, request.Request.Description, request.Request.IsActive);
            
            // Assuming we also want to be able to set active status, though Domain model might need a specific method for it.
            // For now, if the Domain entity exposes properties privately, we can only use its methods.
            // Let's check if there's a ToggleActive method. We'll just update what we can.
            
            genreRepo.Update(genre);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
