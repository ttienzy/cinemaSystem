using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.ShowtimeAggregate;
using Domain.Entities.SharedAggregates;
using MediatR;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Showtimes.Commands.BulkCreateShowtimes
{
    public class BulkCreateShowtimesHandler(
        IShowtimeRepository showtimeRepo,
        IMovieRepository movieRepo,
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<BulkCreateShowtimesCommand, BulkShowtimeResult>
    {
        private const int CleaningOffsetMinutes = 20;

        public async Task<BulkShowtimeResult> Handle(
            BulkCreateShowtimesCommand cmd,
            CancellationToken ct)
        {
            var req = cmd.Request;
            var result = new BulkShowtimeResult();

            // 1. Validate Movie exists
            var movie = await movieRepo.GetByIdAsync(req.MovieId, ct)
                ?? throw new NotFoundException("Movie", req.MovieId);

            // 2. Validate Cinema and Screen
            var cinema = await cinemaRepo.GetByIdWithScreensAsync(req.CinemaId, ct)
                ?? throw new NotFoundException("Cinema", req.CinemaId);

            var screen = cinema.GetScreenById(req.ScreenId)
                ?? throw new NotFoundException("Screen", req.ScreenId);

            // 3. Validate PricingTier
            var pricingTier = await showtimeRepo.GetPricingTierAsync(req.PricingTierId, ct)
                ?? throw new NotFoundException("PricingTier", req.PricingTierId);

            // 4. BATCH QUERY: Get all TimeSlots ONCE
            var allTimeSlots = await showtimeRepo.GetTimeSlotsAsync(ct);
            var slotDict = allTimeSlots.ToDictionary(s => s.Id);

            // 5. BATCH QUERY: Get all SeatTypes ONCE
            var allSeatTypes = await showtimeRepo.GetSeatTypesAsync(ct);
            var seatTypeDict = allSeatTypes.ToDictionary(s => s.Id);

            // 6. Prepare exclude dates
            var excludeDatesSet = req.ExcludeDates.Select(d => d.Date).ToHashSet();
            var createdShowtimes = new List<Showtime>();

            // 7. Generate showtimes for each day
            for (var date = req.StartDate.Date; date <= req.EndDate.Date; date = date.AddDays(1))
            {
                // Skip excluded dates
                if (excludeDatesSet.Contains(date))
                {
                    result.TotalSkipped++;
                    continue;
                }

                var dayOfWeek = (int)date.DayOfWeek;
                if (dayOfWeek == 0) dayOfWeek = 7; // Convert Sunday=0 to 7

                // Process each time slot
                foreach (var slotReq in req.TimeSlots)
                {
                    // Check day of week filter
                    if (slotReq.DaysOfWeek != null && slotReq.DaysOfWeek.Any()
                        && !slotReq.DaysOfWeek.Contains(dayOfWeek))
                    {
                        continue;
                    }

                    // Get TimeSlot from pre-loaded dictionary (O(1))
                    if (!slotDict.TryGetValue(slotReq.SlotId, out var timeSlot))
                    {
                        result.Errors.Add($"TimeSlot {slotReq.SlotId} not found. Skipping.");
                        continue;
                    }

                    // Calculate times
                    var startTime = !string.IsNullOrEmpty(slotReq.TimeOverride)
                        ? DateTime.Parse(slotReq.TimeOverride)
                        : date.Add(timeSlot.StartTime);

                    var endTime = date.Add(timeSlot.EndTime);

                    // Check for conflicts
                    var startOnly = TimeOnly.FromDateTime(startTime);
                    var endOnly = TimeOnly.FromDateTime(endTime);

                    var hasOverlap = await showtimeRepo.HasOverlappingAsync(
                        req.ScreenId, date, startOnly, endOnly, CleaningOffsetMinutes, ct);

                    if (hasOverlap)
                    {
                        result.Errors.Add($"Conflict: {date:yyyy-MM-dd} {startTime:HH:mm} - Screen in use.");
                        result.TotalSkipped++;
                        continue;
                    }

                    // Create showtime
                    var showtime = Showtime.Schedule(
                        req.CinemaId,
                        req.MovieId,
                        req.ScreenId,
                        slotReq.SlotId,
                        req.PricingTierId,
                        date,
                        startTime,
                        endTime,
                        screen.Seats.Count);

                    // Add pricing using pre-loaded SeatType dictionary (O(1))
                    foreach (var spReq in req.ShowtimePricings)
                    {
                        if (!seatTypeDict.TryGetValue(spReq.SeatTypeId, out var seatType))
                        {
                            result.Errors.Add($"SeatType {spReq.SeatTypeId} not found. Skipping pricing.");
                            continue;
                        }

                        var pricing = ShowtimePricing.Calculate(
                            spReq.SeatTypeId,
                            spReq.BasePrice,
                            seatType.PriceMultiplier,
                            pricingTier.Multiplier,
                            0);

                        showtime.AddShowtimePricing(pricing);
                    }

                    createdShowtimes.Add(showtime);

                    result.CreatedShowtimes.Add(new ShowtimeCreatedResult
                    {
                        ShowtimeId = showtime.Id,
                        ShowDate = date,
                        StartTime = startTime.ToString("HH:mm")
                    });
                }
            }

            // 8. Save all showtimes
            foreach (var showtime in createdShowtimes)
            {
                await showtimeRepo.AddAsync(showtime, ct);
            }

            await uow.SaveChangesAsync(ct);

            result.TotalCreated = createdShowtimes.Count;
            return result;
        }
    }
}
