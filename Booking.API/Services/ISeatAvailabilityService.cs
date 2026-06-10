using Booking.API.Infrastructure.Caching.Models;

namespace Booking.API.Services;

public interface ISeatAvailabilityService
{
    Task<SeatAvailabilityResponse> GetSeatAvailabilityAsync(Guid showtimeId, CancellationToken cancellationToken = default);
}
