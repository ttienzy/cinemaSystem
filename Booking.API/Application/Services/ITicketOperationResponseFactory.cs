using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

public interface ITicketOperationResponseFactory
{
    Task<TicketOperationResponse> CreateAsync(BookingEntity booking, PaymentLookupDto payment);
}
