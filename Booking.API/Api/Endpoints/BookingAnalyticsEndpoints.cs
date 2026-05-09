using Booking.API.Application.DTOs.Requests;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Application.Services;
using Cinema.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Api.Endpoints;

public static class BookingAnalyticsEndpoints
{
    public static void MapBookingAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings/analytics")
            .WithTags("Booking Analytics")
            .WithOpenApi();

        group.MapPost("/showtime-occupancy", GetShowtimeOccupancy)
            .WithName("GetShowtimeOccupancy")
            .WithSummary("Get booked seat counts for multiple showtimes")
            .Produces<ShowtimeOccupancyResponse>(200)
            .Produces(422);
    }

    private static async Task<IResult> GetShowtimeOccupancy(
        [FromBody] GetShowtimeOccupancyRequest request,
        [FromServices] IBookingAnalyticsService bookingAnalyticsService)
    {
        var result = await bookingAnalyticsService.GetShowtimeOccupancyAsync(request.ShowtimeIds);
        return result.ToResult();
    }
}
