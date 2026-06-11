using Booking.API.Client;

namespace Booking.API.Services;

public interface ITicketOperationsService
{
    Task<ApiResponse<PaginatedResponse<TicketOperationResponse>>> SearchTicketsAsync(string? query, int pageNumber, int pageSize);
    Task<ApiResponse<TicketOperationResponse>> CheckInAsync(Guid bookingId, string adminUserId);
}
