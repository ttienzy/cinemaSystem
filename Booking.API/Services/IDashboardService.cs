using Booking.API.Client;

namespace Booking.API.Services;

public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(int utcOffsetMinutes);
    Task<ApiResponse<DashboardKpiSnapshotDto>> GetKpiSnapshotAsync(int utcOffsetMinutes);
}
