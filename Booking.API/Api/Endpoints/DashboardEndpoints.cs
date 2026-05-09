using Booking.API.Application.DTOs.Responses;
using Booking.API.Application.Services;
using Cinema.Shared.Extensions;
using Cinema.Shared.Models;

namespace Booking.API.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings/dashboard")
            .WithTags("Dashboard")
            .WithOpenApi();

        group.MapGet("/summary", GetSummary)
            .WithName("GetDashboardSummary")
            .Produces<ApiResponse<DashboardSummaryDto>>(200)
            .Produces(422);

        group.MapGet("/kpi-snapshot", GetKpiSnapshot)
            .WithName("GetDashboardKpiSnapshot")
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
