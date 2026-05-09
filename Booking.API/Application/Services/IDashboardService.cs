using Booking.API.Application.DTOs.Responses;
using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(int utcOffsetMinutes);
    Task<ApiResponse<DashboardKpiSnapshotDto>> GetKpiSnapshotAsync(int utcOffsetMinutes);
}
