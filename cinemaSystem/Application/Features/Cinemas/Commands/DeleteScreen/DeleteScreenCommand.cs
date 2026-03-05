using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using Domain.Entities.CinemaAggregate.Enum;
using MediatR;

namespace Application.Features.Cinemas.Commands.DeleteScreen
{
    public record DeleteScreenCommand(Guid CinemaId, Guid ScreenId) : IRequest;

    public class DeleteScreenHandler(
        IUnitOfWork uow,
        ICinemaRepository cinemaRepo)
        : IRequestHandler<DeleteScreenCommand>
    {
        public async Task Handle(DeleteScreenCommand request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdAsync(request.CinemaId, ct)
                ?? throw new KeyNotFoundException("Cinema not found");

            var screen = cinema.Screens.FirstOrDefault(s => s.Id == request.ScreenId)
                ?? throw new KeyNotFoundException("Screen not found");

            // Soft delete - mark as inactive
            screen.UpdateDetails(screen.ScreenName, screen.Type, ScreenStatus.Closed);
            cinemaRepo.Update(cinema);
            await uow.SaveChangesAsync(ct);
        }
    }
}
