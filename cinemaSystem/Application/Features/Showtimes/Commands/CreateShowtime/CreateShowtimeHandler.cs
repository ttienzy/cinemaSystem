using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.ShowtimeAggregate;
using MediatR;

namespace Application.Features.Showtimes.Commands.CreateShowtime
{
    public class CreateShowtimeHandler(
        IShowtimeRepository showtimeRepo,
        IUnitOfWork uow) : IRequestHandler<CreateShowtimeCommand, CreateShowtimeResult>
    {
        public async Task<CreateShowtimeResult> Handle(
            CreateShowtimeCommand cmd, CancellationToken ct)
        {
            // 1. Check overlap
            var startOnly = TimeOnly.FromDateTime(cmd.StartTime);
            var endOnly = TimeOnly.FromDateTime(cmd.EndTime);

            var hasOverlap = await showtimeRepo.HasOverlappingAsync(
                cmd.ScreenId, cmd.ShowDate, startOnly, endOnly, ct);

            if (hasOverlap)
                throw new ConflictException(
                    "This screen already has a showtime during the requested time slot.");

            // 2. Create showtime via factory
            var showtime = Showtime.Schedule(
                cmd.CinemaId, cmd.MovieId, cmd.ScreenId,
                cmd.SlotId, cmd.PricingTierId,
                cmd.ShowDate, cmd.StartTime, cmd.EndTime,
                cmd.TotalSeats);

            // 3. Attach pricing
            foreach (var p in cmd.Pricings)
            {
                var pricing = ShowtimePricing.Calculate(
                    p.SeatTypeId, p.BasePrice,
                    p.SeatTypeMultiplier, p.PricingTierMultiplier,
                    p.ScreenSurcharge);
                showtime.AddShowtimePricing(pricing);
            }

            // 4. Persist
            await showtimeRepo.AddAsync(showtime, ct);
            await uow.SaveChangesAsync(ct);

            return new CreateShowtimeResult(
                showtime.Id, showtime.ShowDate,
                showtime.ActualStartTime, showtime.ActualEndTime,
                showtime.TotalSeats);
        }
    }
}
