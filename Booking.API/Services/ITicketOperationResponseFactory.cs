using Booking.API.Client;
using BookingEntity = Booking.API.Entities.Booking;

namespace Booking.API.Services;

public interface ITicketOperationResponseFactory
{
    Task<TicketOperationResponse> CreateAsync(BookingEntity booking, PaymentLookupDto payment);
}
