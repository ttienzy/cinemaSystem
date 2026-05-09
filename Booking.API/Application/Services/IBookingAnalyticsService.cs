using Booking.API.Application.DTOs.Responses;
using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

public interface IBookingAnalyticsService
{
    Task<ApiResponse<ShowtimeOccupancyResponse>> GetShowtimeOccupancyAsync(List<Guid> showtimeIds);
}
