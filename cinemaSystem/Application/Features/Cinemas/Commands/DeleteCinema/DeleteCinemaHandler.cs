using Application.Common.Interfaces.Persistence;
using Application.Common.Exceptions;
using Domain.Entities.CinemaAggregate;
using MediatR;

namespace Application.Features.Cinemas.Commands.DeleteCinema
{
    public record DeleteCinemaCommand(Guid Id) : IRequest;

    public class DeleteCinemaHandler(ICinemaRepository cinemaRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<DeleteCinemaCommand>
    {
        public async Task Handle(DeleteCinemaCommand request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Cinema), request.Id);

            cinemaRepo.Delete(cinema);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
