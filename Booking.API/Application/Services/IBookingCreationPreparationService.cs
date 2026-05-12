namespace Booking.API.Application.Services;

public interface IBookingCreationPreparationService
{
    Task<BookingCreationPreparationResult> PrepareAsync(CreateBookingRequest request);
}
