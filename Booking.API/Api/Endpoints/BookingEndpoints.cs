using Booking.API.Application.DTOs.Requests;
using Booking.API.Application.DTOs.Responses;
using Cinema.Shared.Extensions;
using Cinema.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking.API.Api.Endpoints;

/// <summary>
/// Endpoints for booking operations
/// </summary>
public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings")
            .RequireAuthorization() // ✅ Require authentication
            .WithOpenApi();

        // POST /api/bookings
        group.MapPost("/", CreateBooking)
            .WithName("CreateBooking")
            .WithSummary("Create a new booking")
            .WithDescription("Creates a booking, locks seats, and publishes integration event")
            .Produces<BookingResponse>(201)
            .Produces(400)
            .Produces(500);

        // GET /api/bookings/{id}
        group.MapGet("/{id:guid}", GetBookingById)
            .WithName("GetBookingById")
            .WithSummary("Get booking by ID")
            .WithDescription("Returns booking details with seat and showtime information")
            .Produces<BookingResponse>(200)
            .Produces(404)
            .Produces(500);

        // GET /api/bookings/user/{userId}
        group.MapGet("/user/{userId}", GetUserBookings)
            .WithName("GetUserBookings")
            .WithSummary("Get all bookings for a user")
            .WithDescription("Returns list of bookings with details")
            .Produces<List<BookingResponse>>(200)
            .Produces(500);

        // PUT /api/bookings/{id}/cancel
        group.MapPut("/{id:guid}/cancel", CancelBooking)
            .WithName("CancelBooking")
            .WithSummary("Cancel a booking")
            .WithDescription("Cancels booking and releases seats")
            .Produces<bool>(200)
            .Produces(400)
            .Produces(401)
            .Produces(404)
            .Produces(500);
    }

    private static async Task<IResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        [FromServices] IBookingService bookingService,
        [FromServices] ILogger<Program> logger,
        HttpContext httpContext)
    {
        // Extract userId from JWT token
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unauthorized create booking attempt - no userId in token");
            var unauthorizedResponse = ApiResponse<BookingResponse>.FailureResponse(
                "Unauthorized - User ID not found in token",
                401,
                new List<ErrorDetail>
                {
                    new("UNAUTHORIZED", "User authentication required")
                }
            );
            return unauthorizedResponse.ToResult();
        }



        // Override request userId with authenticated userId from token
        request.UserId = userId;

        logger.LogInformation("Creating booking for authenticated user {UserId}, showtime {ShowtimeId}",
            userId, request.ShowtimeId);

        var result = await bookingService.CreateBookingAsync(request);

        return result.ToResult();
    }

    private static async Task<IResult> GetBookingById(
        Guid id,
        [FromServices] IBookingService bookingService,
        [FromServices] ILogger<Program> logger)
    {
        logger.LogInformation("Getting booking {BookingId}", id);

        var result = await bookingService.GetBookingByIdAsync(id);

        return result.ToResult();
    }

    private static async Task<IResult> GetUserBookings(
        string userId,
        [FromServices] IBookingService bookingService,
        [FromServices] ILogger<Program> logger)
    {
        logger.LogInformation("Getting bookings for user {UserId}", userId);

        var result = await bookingService.GetUserBookingsAsync(userId);

        return result.ToResult();
    }

    private static async Task<IResult> CancelBooking(
        Guid id,
        [FromBody] CancelBookingRequest request,
        [FromServices] IBookingService bookingService,
        [FromServices] ILogger<Program> logger,
        HttpContext httpContext)
    {
        // Extract userId from JWT token
        var userId = httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unauthorized cancel booking attempt - no userId in token");
            var unauthorizedResponse = ApiResponse<bool>.FailureResponse(
                "Unauthorized - User ID not found in token",
                401,
                new List<ErrorDetail>
                {
                    new("UNAUTHORIZED", "User authentication required")
                }
            );
            return unauthorizedResponse.ToResult();
        }

        // Validate userId from token matches request (if provided)
        if (!string.IsNullOrEmpty(request.UserId) && request.UserId != userId)
        {
            logger.LogWarning("UserId mismatch - Token: {TokenUserId}, Request: {RequestUserId}",
                userId, request.UserId);
            var errorResponse = ApiResponse<bool>.FailureResponse(
                "UserId mismatch",
                403,
                new List<ErrorDetail>
                {
                    new("USERID_MISMATCH", "User ID in request does not match authenticated user")
                }
            );
            return errorResponse.ToResult();
        }

        // Override request userId with authenticated userId from token
        request.UserId = userId;

        logger.LogInformation("Cancelling booking {BookingId} by authenticated user {UserId}",
            id, userId);

        var result = await bookingService.CancelBookingAsync(id, request);

        return result.ToResult();
    }
}


