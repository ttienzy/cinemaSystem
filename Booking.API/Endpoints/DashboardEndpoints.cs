using Booking.API.Client;
using Booking.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace Booking.API.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings/dashboard")
            .WithTags("Dashboard");

        group.MapGet("/summary", GetSummary)
            .WithName("GetDashboardSummary")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<ApiResponse<DashboardSummaryDto>>(200)
            .Produces(422);

        group.MapGet("/kpi-snapshot", GetKpiSnapshot)
            .WithName("GetDashboardKpiSnapshot")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<ApiResponse<DashboardKpiSnapshotDto>>(200)
            .Produces(422);
    }

    private static async Task<IResult> GetSummary(
        IDashboardService dashboardService,
        HttpContext context,
        int utcOffsetMinutes = 420)
    {
        var response = await dashboardService.GetSummaryAsync(utcOffsetMinutes);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetKpiSnapshot(
        IDashboardService dashboardService,
        HttpContext context,
        int utcOffsetMinutes = 420)
    {
        var response = await dashboardService.GetKpiSnapshotAsync(utcOffsetMinutes);
        response.SetTraceId(context);
        return response.ToResult();
    }
}
