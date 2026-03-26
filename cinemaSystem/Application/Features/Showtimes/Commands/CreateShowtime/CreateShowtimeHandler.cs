using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.ShowtimeAggregate;
using Domain.Entities.CinemaAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Showtimes.Commands.CreateShowtime
{
    public class CreateShowtimeHandler(
        IShowtimeRepository showtimeRepo,
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<CreateShowtimeCommand, CreateShowtimeResult>
    {
        private const int CleaningOffsetMinutes = 20;

        public async Task<CreateShowtimeResult> Handle(
            CreateShowtimeCommand cmd, CancellationToken ct)
        {
            var req = cmd.Request;

            // 0. Location-based RBAC: Ensure Manager manages this Cinema
            //if (!currentUser.IsInRole("Admin"))
            //{
            //    var staff = await staffRepo.GetByEmailAsync(currentUser.Email ?? string.Empty, ct)
            //        ?? throw new UnauthorizedException("Authenticated user is not registered as staff.");

            //    if (staff.CinemaId != req.CinemaId)
            //        throw new ForbiddenException($"Access denied. You can only manage showtimes for your assigned cinema (Branch ID: {staff.CinemaId}).");
            //}

            // 1. Conflict Detection with Cleaning Offset
            var startOnly = TimeOnly.FromDateTime(req.ActualStartTime);
            var endOnly = TimeOnly.FromDateTime(req.ActualEndTime);

            var hasOverlap = await showtimeRepo.HasOverlappingAsync(
                req.ScreenId, req.ShowDate, startOnly, endOnly, CleaningOffsetMinutes, ct);

            if (hasOverlap)
                throw new ConflictException(
                    $"Screen conflict detected. Please allow at least {CleaningOffsetMinutes} minutes for cleaning between sessions.");

            // 2. Fetch Screen for Total Seats
            var cinema = await cinemaRepo.GetByIdWithScreensAsync(req.CinemaId, ct)
                ?? throw new NotFoundException(nameof(Cinema), req.CinemaId);

            var screen = cinema.GetScreenById(req.ScreenId)
                ?? throw new NotFoundException(nameof(Screen), req.ScreenId);

            // 3. Resolve Pricing Multipliers
            var pricingTier = await showtimeRepo.GetPricingTierAsync(req.PricingTierId, ct)
                ?? throw new NotFoundException("PricingTier", req.PricingTierId);

            var seatTypes = await showtimeRepo.GetSeatTypesAsync(ct);

            // 4. Create Showtime via Factory
            // Note: TotalSeats from screen, BookedSeats starts at 0
            var showtime = Showtime.Schedule(
                req.CinemaId, req.MovieId, req.ScreenId,
                req.SlotId, req.PricingTierId,
                req.ShowDate, req.ActualStartTime, req.ActualEndTime,
                screen.Seats.Count); // Actual seat count from the screen

            // 5. Build Showtime Pricings
            foreach (var spReq in req.ShowtimePricings)
            {
                var seatType = seatTypes.FirstOrDefault(st => st.Id == spReq.SeatTypeId)
                    ?? throw new NotFoundException("SeatType", spReq.SeatTypeId);

                var pricing = ShowtimePricing.Calculate(
                    spReq.SeatTypeId,
                    spReq.BasePrice,
                    seatType.PriceMultiplier,
                    pricingTier.Multiplier,
                    0 // Surcharge can be added later if needed
                );
                showtime.AddShowtimePricing(pricing);
            }

            // 6. Persist
            await showtimeRepo.AddAsync(showtime, ct);
            await uow.SaveChangesAsync(ct);

            return new CreateShowtimeResult(
                showtime.Id, showtime.ShowDate,
                showtime.ActualStartTime, showtime.ActualEndTime,
                showtime.TotalSeats);
        }
    }
}
