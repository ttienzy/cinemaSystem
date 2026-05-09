using Booking.API.Application.DTOs.Responses;
using Booking.API.Application.Services;
using Cinema.Shared.Constants;
using Cinema.Shared.Extensions;
using Cinema.Shared.Helpers;
using Cinema.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking.API.Api.Endpoints;

public static class BookingOperationsEndpoints
{
    public static void MapBookingOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings/operations")
            .WithTags("Booking Operations")
            .WithOpenApi()
            .RequireAuthorization(new AuthorizeAttribute
            {
                Roles = $"{AppConstants.Roles.Staff},{AppConstants.Roles.Admin}"
            });

        group.MapGet("/tickets", SearchTickets)
            .WithName("SearchTickets")
            .WithSummary("Search booked tickets by ticket code, email, or phone")
            .Produces<PaginatedResponse<TicketOperationResponse>>(200)
            .Produces(401)
            .Produces(403);

        group.MapPut("/tickets/{bookingId:guid}/check-in", CheckInTicket)
            .WithName("CheckInTicket")
            .WithSummary("Check in a paid ticket")
            .Produces<TicketOperationResponse>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);
    }

    private static async Task<IResult> SearchTickets(
        [FromQuery] string? q,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromServices] ITicketOperationsService ticketOperationsService)
    {
        var result = await ticketOperationsService.SearchTicketsAsync(
            q,
            pageNumber <= 0 ? 1 : pageNumber,
            pageSize <= 0 ? 20 : pageSize);

        return result.ToResult();
    }

    private static async Task<IResult> CheckInTicket(
        Guid bookingId,
        ClaimsPrincipal user,
        [FromServices] ITicketOperationsService ticketOperationsService)
    {
        var staffUserId = JwtHelper.GetUserId(user);
        if (string.IsNullOrWhiteSpace(staffUserId))
        {
            return ApiResponse<TicketOperationResponse>.UnauthorizedResponse("Invalid user context").ToResult();
        }

        var result = await ticketOperationsService.CheckInAsync(bookingId, staffUserId);
        return result.ToResult();
    }
}
