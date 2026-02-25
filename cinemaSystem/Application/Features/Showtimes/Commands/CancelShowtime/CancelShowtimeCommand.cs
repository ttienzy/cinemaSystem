using MediatR;

namespace Application.Features.Showtimes.Commands.CancelShowtime
{
    public record CancelShowtimeCommand(Guid ShowtimeId, string MovieTitle) : IRequest<Unit>;
}
