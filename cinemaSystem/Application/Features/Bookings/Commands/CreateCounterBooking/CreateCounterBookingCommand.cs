using Application.Common.Interfaces.Security;
using Shared.Models.DataModels.BookingDtos;
using MediatR;

namespace Application.Features.Bookings.Commands.CreateCounterBooking
{
    public record CreateCounterBookingCommand(CounterBookingRequest Request) : IRequest<CounterBookingResponse>;
}
