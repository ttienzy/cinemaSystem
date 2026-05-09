using Booking.API.Application.DTOs.Responses;
using Booking.API.Infrastructure.Persistence.Repositories;
using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

public class BookingAnalyticsService : IBookingAnalyticsService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingAnalyticsService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
    }

    public async Task<ApiResponse<ShowtimeOccupancyResponse>> GetShowtimeOccupancyAsync(List<Guid> showtimeIds)
    {
        var normalizedIds = showtimeIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedIds.Count == 0)
        {
            return ApiResponse<ShowtimeOccupancyResponse>.ValidationErrorResponse(
                "ShowtimeIds is required",
                [new ErrorDetail("SHOWTIME_IDS_REQUIRED", "At least one showtime id is required", "ShowtimeIds")]);
        }

        var counts = await _bookingRepository.GetBookedSeatCountsByShowtimeIdsAsync(normalizedIds);
        var response = new ShowtimeOccupancyResponse
        {
            Items = normalizedIds.Select(id => new ShowtimeOccupancyItemResponse
            {
                ShowtimeId = id,
                BookedSeats = counts.GetValueOrDefault(id)
            }).ToList()
        };

        return ApiResponse<ShowtimeOccupancyResponse>.SuccessResponse(response);
    }
}
