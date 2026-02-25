using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Showtimes.Commands.CancelShowtime
{
    public class CancelShowtimeHandler(
        IShowtimeRepository showtimeRepo,
        IUnitOfWork uow) : IRequestHandler<CancelShowtimeCommand, Unit>
    {
        public async Task<Unit> Handle(CancelShowtimeCommand cmd, CancellationToken ct)
        {
            var showtime = await showtimeRepo.GetByIdAsync(cmd.ShowtimeId, ct)
                ?? throw new NotFoundException(
                    nameof(Domain.Entities.ShowtimeAggregate.Showtime), cmd.ShowtimeId);

            // Raises ShowtimeCancelledEvent inside aggregate
            showtime.Cancel(cmd.MovieTitle);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
