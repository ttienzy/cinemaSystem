using System.Security.Claims;
using Booking.API.Client;
using Booking.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings")
            .RequireAuthorization("CustomerOrAdmin");

        group.MapPost("/", CreateBooking)
            .WithName("CreateBooking")
            .WithSummary("Create a new booking")
            .Produces<BookingResponse>(201)
            .Produces(400)
            .Produces(500);

        group.MapGet("/{id:guid}", GetBookingById)
            .WithName("GetBookingById")
            .WithSummary("Get booking by ID")
            .Produces<BookingResponse>(200)
            .Produces(404)
            .Produces(500);

        group.MapGet("/user/{userId}", GetUserBookings)
            .WithName("GetUserBookings")
            .WithSummary("Get all bookings for a user")
            .Produces<List<BookingResponse>>(200)
            .Produces(500);

        group.MapPut("/{id:guid}/cancel", CancelBooking)
            .WithName("CancelBooking")
            .WithSummary("Cancel a booking")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404)
            .Produces(500);
    }

    private static async Task<IResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        ClaimsPrincipal user,
        [FromServices] IBookingService bookingService,
        [FromServices] ILogger<Program> logger)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unauthorized create booking attempt - no userId in token");
            return ApiResponse<BookingResponse>.UnauthorizedResponse("User authentication required").ToResult();
        }

        request.UserId = userId;
        var result = await bookingService.CreateBookingAsync(request);
        return result.ToResult();
    }

    private static async Task<IResult> GetBookingById(
        Guid id,
        ClaimsPrincipal user,
        [FromServices] IBookingService bookingService)
    {
        var result = await bookingService.GetBookingByIdAsync(id);
        if (result.Success &&
            result.Data != null &&
            !CanAccessUserResource(user, result.Data.UserId))
        {
            return ApiResponse<BookingResponse>
                .ForbiddenResponse("You are not allowed to access this booking.")
                .ToResult();
        }

        return result.ToResult();
    }

    private static async Task<IResult> GetUserBookings(
        string userId,
        ClaimsPrincipal user,
        [FromServices] IBookingService bookingService)
    {
        if (!CanAccessUserResource(user, userId))
        {
            return ApiResponse<List<BookingResponse>>
                .ForbiddenResponse("You are not allowed to access bookings for this user.")
                .ToResult();
        }

        var result = await bookingService.GetUserBookingsAsync(userId);
        return result.ToResult();
    }

    private static async Task<IResult> CancelBooking(
        Guid id,
        [FromBody] CancelBookingRequest request,
        ClaimsPrincipal user,
        [FromServices] IBookingService bookingService,
        [FromServices] ILogger<Program> logger)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unauthorized cancel booking attempt - no userId in token");
            return ApiResponse.UnauthorizedResponse("User authentication required").ToResult();
        }

        if (!string.IsNullOrEmpty(request.UserId) && request.UserId != userId)
        {
            return ApiResponse.ForbiddenResponse("User ID in request does not match authenticated user").ToResult();
        }

        request.UserId = userId;
        var result = await bookingService.CancelBookingAsync(id, request);
        return result.ToResult();
    }

    private static bool CanAccessUserResource(ClaimsPrincipal user, string ownerUserId)
    {
        var userId = GetUserId(user);

        return user.Identity?.IsAuthenticated == true &&
               (string.Equals(userId, ownerUserId, StringComparison.Ordinal) ||
                user.IsInRole("Admin"));
    }

    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue("sub") ??
               user.FindFirstValue(ClaimTypes.NameIdentifier) ??
               user.FindFirstValue("nameid");
    }
}
