using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Showtimes.Commands.ConfirmShowtime
{
    public record ConfirmShowtimeCommand(Guid ShowtimeId) : IRequest<bool>;

    public class ConfirmShowtimeHandler(
        IShowtimeRepository showtimeRepo,
        IUnitOfWork uow) : IRequestHandler<ConfirmShowtimeCommand, bool>
    {
        public async Task<bool> Handle(ConfirmShowtimeCommand request, CancellationToken ct)
        {
            // 1. Get showtime
            var showtime = await showtimeRepo.GetByIdAsync(request.ShowtimeId, ct)
                ?? throw new NotFoundException("Showtime", request.ShowtimeId);

            // 2. Confirm showtime (domain method handles validation)
            showtime.Confirm();
            showtimeRepo.Update(showtime);
            await uow.SaveChangesAsync(ct);

            return true;
        }
    }
}
