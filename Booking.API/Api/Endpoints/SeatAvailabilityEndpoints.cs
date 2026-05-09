using Booking.API.Application.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Cinema.Shared.Models;
using Cinema.Shared.Extensions;
using System.Security.Claims;

namespace Booking.API.Api.Endpoints;

/// <summary>
/// Endpoints for seat availability and locking
/// </summary>
public static class SeatAvailabilityEndpoints
{
    public static void MapSeatAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/showtimes")
            .WithTags("Seat Availability")
            .WithOpenApi();

        // GET /api/showtimes/{showtimeId}/seats
        group.MapGet("/{showtimeId:guid}/seats", GetSeatAvailability)
            .WithName("GetSeatAvailability")
            .WithSummary("Get seat availability for a showtime")
            .WithDescription("Returns seat map with real-time status from Redis")
            .Produces<SeatAvailabilityResponse>(200)
            .Produces(404)
            .Produces(500);

        // POST /api/showtimes/{showtimeId}/seats/lock
        group.MapPost("/{showtimeId:guid}/seats/lock", LockSeats)
            .WithName("LockSeats")
            .RequireAuthorization()
            .WithSummary("Lock seats temporarily for a user")
            .WithDescription("Locks seats for 10 minutes while user completes booking")
            .Produces<SeatLockResult>(200)
            .Produces(400)
            .Produces(500);

        // POST /api/showtimes/{showtimeId}/seats/unlock
        group.MapPost("/{showtimeId:guid}/seats/unlock", UnlockSeats)
            .WithName("UnlockSeats")
            .WithSummary("Unlock previously locked seats")
            .WithDescription("Releases seat locks when user deselects or cancels")
            .Produces<bool>(200)
            .Produces(400)
            .Produces(500);
    }

    private static async Task<IResult> GetSeatAvailability(
        Guid showtimeId,
        [FromServices] ISeatStatusService seatStatusService,
        [FromServices] ILogger<Program> logger)
    {
        logger.LogInformation("Getting seat availability for showtime {ShowtimeId}", showtimeId);

        try
        {
            var availability = await seatStatusService.GetSeatAvailabilityAsync(showtimeId);

            var response = ApiResponse<SeatAvailabilityResponse>.SuccessResponse(
                availability,
                "Seat availability retrieved successfully"
            );

            return response.ToResult();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Showtime {ShowtimeId} not found", showtimeId);

            var response = ApiResponse<SeatAvailabilityResponse>.NotFoundResponse(
                ex.Message
            );

            return response.ToResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting seat availability for showtime {ShowtimeId}", showtimeId);

            var response = ApiResponse<SeatAvailabilityResponse>.FailureResponse(
                "Error getting seat availability",
                500,
                new List<ErrorDetail>
                {
                    new("SYSTEM_ERROR", ex.Message)
                }
            );

            return response.ToResult();
        }
    }

    private static async Task<IResult> LockSeats(
        Guid showtimeId,
        [FromBody] LockSeatsRequest request,
        [FromServices] ISeatStatusService seatStatusService,
        [FromServices] ILogger<Program> logger,
        HttpContext httpContext)
    {
        // Extract userId from JWT token
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unauthorized lock seats attempt - no userId in token");
            var unauthorizedResponse = ApiResponse<SeatLockResult>.FailureResponse(
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
        // Validate request
        if (request.ShowtimeId != showtimeId)
        {
            var errorResponse = ApiResponse<SeatLockResult>.FailureResponse(
                "Showtime ID mismatch",
                400,
                new List<ErrorDetail>
                {
                    new("SHOWTIME_MISMATCH", "Showtime ID in URL and body do not match")
                }
            );
            return errorResponse.ToResult();
        }

        if (!request.SeatIds.Any())
        {
            var errorResponse = ApiResponse<SeatLockResult>.FailureResponse(
                "No seats selected",
                400,
                new List<ErrorDetail>
                {
                    new("SEATS_REQUIRED", "At least one seat must be selected")
                }
            );
            return errorResponse.ToResult();
        }

        logger.LogInformation("Locking {Count} seats for user {UserId} in showtime {ShowtimeId}",
            request.SeatIds.Count, request.UserId, showtimeId);

        try
        {
            var result = await seatStatusService.LockSeatsAsync(
                showtimeId,
                request.SeatIds,
                request.UserId
            );

            var response = result.Success
                ? ApiResponse<SeatLockResult>.SuccessResponse(result, "Seats locked successfully")
                : ApiResponse<SeatLockResult>.FailureResponse(result.Message ?? "Failed to lock seats", 409);

            return response.ToResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error locking seats for showtime {ShowtimeId}", showtimeId);

            var response = ApiResponse<SeatLockResult>.FailureResponse(
                "Error locking seats",
                500,
                new List<ErrorDetail>
                {
                    new("SYSTEM_ERROR", ex.Message)
                }
            );

            return response.ToResult();
        }
    }

    private static async Task<IResult> UnlockSeats(
        Guid showtimeId,
        [FromBody] UnlockSeatsRequest request,
        [FromServices] ISeatStatusService seatStatusService,
        [FromServices] ILogger<Program> logger,
        HttpContext httpContext)
    {
        // Extract userId from JWT token
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unauthorized unlock seats attempt - no userId in token");
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
        // Validate request
        if (request.ShowtimeId != showtimeId)
        {
            var errorResponse = ApiResponse<bool>.FailureResponse(
                "Showtime ID mismatch",
                400,
                new List<ErrorDetail>
                {
                    new("SHOWTIME_MISMATCH", "Showtime ID in URL and body do not match")
                }
            );
            return errorResponse.ToResult();
        }

        if (!request.SeatIds.Any())
        {
            var errorResponse = ApiResponse<bool>.FailureResponse(
                "No seats to unlock",
                400,
                new List<ErrorDetail>
                {
                    new("SEATS_REQUIRED", "At least one seat must be specified")
                }
            );
            return errorResponse.ToResult();
        }

        logger.LogInformation("Unlocking {Count} seats for user {UserId} in showtime {ShowtimeId}",
            request.SeatIds.Count, request.UserId, showtimeId);

        try
        {
            var success = await seatStatusService.UnlockSeatsAsync(
                showtimeId,
                request.SeatIds,
                request.UserId
            );

            var response = success
                ? ApiResponse<bool>.SuccessResponse(true, "Seats unlocked successfully")
                : ApiResponse<bool>.FailureResponse("Some seats could not be unlocked", 400);

            return response.ToResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unlocking seats for showtime {ShowtimeId}", showtimeId);

            var response = ApiResponse<bool>.FailureResponse(
                "Error unlocking seats",
                500,
                new List<ErrorDetail>
                {
                    new("SYSTEM_ERROR", ex.Message)
                }
            );

            return response.ToResult();
        }
    }
}


