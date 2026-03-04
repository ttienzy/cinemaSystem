using MediatR;
using Shared.Models.DataModels.ShowtimeDtos;

namespace Application.Features.Showtimes.Commands.BulkCreateShowtimes
{
    public record BulkCreateShowtimesCommand(BulkShowtimeRequest Request) : IRequest<BulkShowtimeResult>;
}
