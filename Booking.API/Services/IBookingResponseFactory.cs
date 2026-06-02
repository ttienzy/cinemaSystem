using Booking.API.Client;
using BookingEntity = Booking.API.Entities.Booking;

namespace Booking.API.Services;

public interface IBookingResponseFactory
{
    Task<BookingResponse> CreateAsync(BookingEntity booking, PaymentCheckoutDto? paymentCheckout = null);
}
