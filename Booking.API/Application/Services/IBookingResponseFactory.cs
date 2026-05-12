using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

public interface IBookingResponseFactory
{
    Task<BookingResponse> CreateAsync(BookingEntity booking, PaymentCheckoutDto? paymentCheckout = null);
}
