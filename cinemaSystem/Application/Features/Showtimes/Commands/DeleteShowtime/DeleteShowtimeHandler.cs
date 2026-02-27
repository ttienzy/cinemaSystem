using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.ShowtimeAggregate.Enum;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Showtimes.Commands.DeleteShowtime
{
    public record DeleteShowtimeCommand(Guid ShowtimeId) : IRequest;

    public class DeleteShowtimeHandler(
        IShowtimeRepository showtimeRepo,
        IStaffRepository staffRepo,
        ICurrentUserService currentUser,
        IUnitOfWork uow) : IRequestHandler<DeleteShowtimeCommand>
    {
        public async Task Handle(DeleteShowtimeCommand cmd, CancellationToken ct)
        {
            // 1. Load showtime
            var showtime = await showtimeRepo.GetByIdAsync(cmd.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.ShowtimeAggregate.Showtime), cmd.ShowtimeId);

            // 2. Location-based RBAC (non-Admin must own the cinema)
            if (!currentUser.IsInRole("Admin"))
            {
                var staff = await staffRepo.GetByEmailAsync(currentUser.Email ?? string.Empty, ct)
                    ?? throw new UnauthorizedException("Authenticated user is not registered as staff.");

                if (staff.CinemaId != showtime.CinemaId)
                    throw new ForbiddenException("Access denied. You can only delete showtimes for your assigned cinema.");
            }

            // 3. Safety guard: cannot delete a showtime that has sold tickets
            if (showtime.BookedSeats > 0)
                throw new ConflictException(
                    "Cannot delete a showtime with active bookings. Cancel all bookings first.");

            // 4. Soft-cancel: mark as Cancelled instead of hard delete to preserve audit trail
            showtime.Cancel("Cancelled by manager via admin portal.");
            showtimeRepo.Update(showtime);
            await uow.SaveChangesAsync(ct);
        }
    }
}
