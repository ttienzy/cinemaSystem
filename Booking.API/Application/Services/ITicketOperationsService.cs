using Booking.API.Application.DTOs.Responses;
using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

public interface ITicketOperationsService
{
    Task<ApiResponse<PaginatedResponse<TicketOperationResponse>>> SearchTicketsAsync(string? query, int pageNumber, int pageSize);
    Task<ApiResponse<TicketOperationResponse>> CheckInAsync(Guid bookingId, string staffUserId);
}
