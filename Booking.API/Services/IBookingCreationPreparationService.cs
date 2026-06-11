using Booking.API.Client;
using Booking.API.Models;

namespace Booking.API.Services;

public interface IBookingCreationPreparationService
{
    Task<BookingCreationPreparationResult> PrepareAsync(CreateBookingRequest request);
}
