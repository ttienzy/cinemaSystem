using System.Security.Claims;
using Booking.API.Client;
using Booking.API.Infrastructure.Caching.Services;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Endpoints;

public static class SeatAvailabilityEndpoints
{
    public static void MapSeatAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/showtimes")
            .WithTags("Seat Availability");

        group.MapGet("/{showtimeId:guid}/seats", GetSeatAvailability)
            .WithName("GetSeatAvailability")
            .WithSummary("Get seat availability for a showtime")
            .Produces<SeatAvailabilityResponse>(200)
            .Produces(404)
            .Produces(500);

        group.MapPost("/{showtimeId:guid}/seats/lock", LockSeats)
            .WithName("LockSeats")
            .RequireAuthorization("CustomerOrAdmin")
            .WithSummary("Lock seats temporarily for a user")
            .Produces<SeatLockResult>(200)
            .Produces(400)
            .Produces(500);

        group.MapPost("/{showtimeId:guid}/seats/unlock", UnlockSeats)
            .WithName("UnlockSeats")
            .RequireAuthorization("CustomerOrAdmin")
            .WithSummary("Unlock previously locked seats")
            .Produces<bool>(200)
            .Produces(400)
            .Produces(500);
    }

    private static async Task<IResult> GetSeatAvailability(
        Guid showtimeId,
        [FromServices] ISeatStatusService seatStatusService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            var availability = await seatStatusService.GetSeatAvailabilityAsync(showtimeId);

            return ApiResponse<SeatAvailabilityResponse>
                .SuccessResponse(availability, "Seat availability retrieved successfully")
                .ToResult();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Showtime {ShowtimeId} not found", showtimeId);
            return ApiResponse<SeatAvailabilityResponse>.NotFoundResponse(ex.Message).ToResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting seat availability for showtime {ShowtimeId}", showtimeId);
            return ApiResponse<SeatAvailabilityResponse>
                .FailureResponse("Error getting seat availability", 500, [new ErrorDetail("SYSTEM_ERROR", ex.Message)])
                .ToResult();
        }
    }

    private static async Task<IResult> LockSeats(
        Guid showtimeId,
        [FromBody] LockSeatsRequest request,
        ClaimsPrincipal user,
        [FromServices] ISeatStatusService seatStatusService,
        [FromServices] ILogger<Program> logger)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<SeatLockResult>.UnauthorizedResponse("User authentication required").ToResult();
        }

        request.UserId = userId;
        var validation = ValidateSeatRequest(showtimeId, request.ShowtimeId, request.SeatIds);
        if (validation != null)
        {
            return validation.ToResult();
        }

        try
        {
            var result = await seatStatusService.LockSeatsAsync(showtimeId, request.SeatIds, request.UserId);

            return result.Success
                ? ApiResponse<SeatLockResult>.SuccessResponse(result, "Seats locked successfully").ToResult()
                : ApiResponse<SeatLockResult>.FailureResponse(result.Message ?? "Failed to lock seats", 409).ToResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error locking seats for showtime {ShowtimeId}", showtimeId);
            return ApiResponse<SeatLockResult>
                .FailureResponse("Error locking seats", 500, [new ErrorDetail("SYSTEM_ERROR", ex.Message)])
                .ToResult();
        }
    }

    private static async Task<IResult> UnlockSeats(
        Guid showtimeId,
        [FromBody] UnlockSeatsRequest request,
        ClaimsPrincipal user,
        [FromServices] ISeatStatusService seatStatusService,
        [FromServices] ILogger<Program> logger)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<bool>.UnauthorizedResponse("User authentication required").ToResult();
        }

        if (!string.IsNullOrEmpty(request.UserId) && request.UserId != userId)
        {
            return ApiResponse<bool>.ForbiddenResponse("User ID in request does not match authenticated user").ToResult();
        }

        request.UserId = userId;
        var validation = ValidateSeatRequest(showtimeId, request.ShowtimeId, request.SeatIds);
        if (validation != null)
        {
            return validation.ToResult();
        }

        try
        {
            var success = await seatStatusService.UnlockSeatsAsync(showtimeId, request.SeatIds, request.UserId);

            return success
                ? ApiResponse<bool>.SuccessResponse(true, "Seats unlocked successfully").ToResult()
                : ApiResponse<bool>.FailureResponse("Some seats could not be unlocked", 400).ToResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unlocking seats for showtime {ShowtimeId}", showtimeId);
            return ApiResponse<bool>
                .FailureResponse("Error unlocking seats", 500, [new ErrorDetail("SYSTEM_ERROR", ex.Message)])
                .ToResult();
        }
    }

    private static ApiResponse? ValidateSeatRequest(Guid routeShowtimeId, Guid bodyShowtimeId, List<Guid> seatIds)
    {
        if (routeShowtimeId != bodyShowtimeId)
        {
            return ApiResponse.FailureResponse(
                "Showtime ID mismatch",
                400,
                [new ErrorDetail("SHOWTIME_MISMATCH", "Showtime ID in URL and body do not match")]);
        }

        if (seatIds.Count == 0)
        {
            return ApiResponse.FailureResponse(
                "At least one seat must be selected",
                400,
                [new ErrorDetail("SEATS_REQUIRED", "At least one seat must be selected")]);
        }

        return null;
    }

    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue("sub") ??
               user.FindFirstValue(ClaimTypes.NameIdentifier) ??
               user.FindFirstValue("nameid");
    }
}
