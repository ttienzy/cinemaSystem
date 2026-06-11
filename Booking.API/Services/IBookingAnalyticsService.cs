using Booking.API.Client;

namespace Booking.API.Services;

public interface IBookingAnalyticsService
{
    Task<ApiResponse<ShowtimeOccupancyResponse>> GetShowtimeOccupancyAsync(List<Guid> showtimeIds);
}
