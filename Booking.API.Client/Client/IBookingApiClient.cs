namespace Booking.API.Client.Client;

public interface IBookingApiClient
{
    Task<ApiResponse<SeatAvailabilityResponse>> GetSeatAvailabilityAsync(Guid showtimeId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SeatLockResult>> LockSeatsAsync(Guid showtimeId, LockSeatsRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> UnlockSeatsAsync(Guid showtimeId, UnlockSeatsRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<BookingResponse>>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> CancelBookingAsync(Guid bookingId, CancelBookingRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<ShowtimeOccupancyResponse>> GetShowtimeOccupancyAsync(GetShowtimeOccupancyRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaginatedResponse<TicketOperationResponse>>> SearchTicketsAsync(string? query = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<TicketOperationResponse>> CheckInTicketAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync(int utcOffsetMinutes = 420, CancellationToken cancellationToken = default);
    Task<ApiResponse<DashboardKpiSnapshotDto>> GetDashboardKpiSnapshotAsync(int utcOffsetMinutes = 420, CancellationToken cancellationToken = default);
}
