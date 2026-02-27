using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.ShowtimeAggregate.Enum;
using MediatR;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Showtimes.Commands.UpdateShowtime
{
    public record UpdateShowtimeCommand(Guid ShowtimeId, ShowtimeUpsertRequest Request) : IRequest<UpdateShowtimeResult>;

    public record UpdateShowtimeResult(Guid ShowtimeId, string Message);

    public class UpdateShowtimeHandler(
        IShowtimeRepository showtimeRepo,
        IStaffRepository staffRepo,
        ICurrentUserService currentUser,
        IUnitOfWork uow) : IRequestHandler<UpdateShowtimeCommand, UpdateShowtimeResult>
    {
        private const int CleaningOffsetMinutes = 20;

        public async Task<UpdateShowtimeResult> Handle(UpdateShowtimeCommand cmd, CancellationToken ct)
        {
            var req = cmd.Request;

            // 0. Location-based RBAC
            if (!currentUser.IsInRole("Admin"))
            {
                var staff = await staffRepo.GetByEmailAsync(currentUser.Email ?? string.Empty, ct)
                    ?? throw new UnauthorizedException("Authenticated user is not registered as staff.");

                if (staff.CinemaId != req.CinemaId)
                    throw new ForbiddenException($"Access denied. You can only manage showtimes for your assigned cinema.");
            }

            // 1. Load existing showtime
            var showtime = await showtimeRepo.GetByIdAsync(cmd.ShowtimeId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.ShowtimeAggregate.Showtime), cmd.ShowtimeId);

            // 2. Cannot reschedule if tickets already sold
            if (showtime.BookedSeats > 0)
                throw new ConflictException("Cannot reschedule a showtime that already has active bookings. Cancel all bookings first.");

            // 3. Conflict detection (exclude self)
            var startOnly = TimeOnly.FromDateTime(req.ActualStartTime);
            var endOnly = TimeOnly.FromDateTime(req.ActualEndTime);

            var hasOverlap = await showtimeRepo.HasOverlappingExcludingAsync(
                req.ScreenId, req.ShowDate, startOnly, endOnly, cmd.ShowtimeId, ct);

            if (hasOverlap)
                throw new ConflictException(
                    $"Screen conflict detected. Please allow at least {CleaningOffsetMinutes} minutes for cleaning between sessions.");

            // 4. Apply update via domain method
            showtime.Reschedule(req.ShowDate, req.ActualStartTime, req.ActualEndTime);

            showtimeRepo.Update(showtime);
            await uow.SaveChangesAsync(ct);

            return new UpdateShowtimeResult(showtime.Id, "Showtime rescheduled successfully.");
        }
    }
}
