using Application.Features.Showtimes.Commands.CreateShowtime;
using MediatR;
using Shared.Models.DataModels.ShowtimeDtos;

namespace Application.Features.Showtimes.Commands.CreateShowtime
{
    public record CreateShowtimeCommand(ShowtimeUpsertRequest Request) : IRequest<CreateShowtimeResult>;

    public record CreateShowtimeResult(
        Guid ShowtimeId,
        DateTime ShowDate,
        DateTime StartTime,
        DateTime EndTime,
        int TotalSeats);
}
