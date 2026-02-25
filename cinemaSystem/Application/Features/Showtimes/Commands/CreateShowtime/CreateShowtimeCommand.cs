using MediatR;

namespace Application.Features.Showtimes.Commands.CreateShowtime
{
    public record CreateShowtimeCommand(
        Guid CinemaId,
        Guid MovieId,
        Guid ScreenId,
        Guid SlotId,
        Guid PricingTierId,
        DateTime ShowDate,
        DateTime StartTime,
        DateTime EndTime,
        int TotalSeats,
        List<ShowtimePricingInput> Pricings) : IRequest<CreateShowtimeResult>;

    public record ShowtimePricingInput(
        Guid SeatTypeId,
        decimal BasePrice,
        decimal SeatTypeMultiplier,
        decimal PricingTierMultiplier,
        decimal ScreenSurcharge);

    public record CreateShowtimeResult(
        Guid ShowtimeId,
        DateTime ShowDate,
        DateTime StartTime,
        DateTime EndTime,
        int TotalSeats);
}
