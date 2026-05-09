using Cinema.API.Application.DTOs;
using Cinema.Shared.Extensions;
using Cinema.Shared.Models;

namespace Cinema.API.Api.Endpoints;

public static class SeatEndpoints
{
    public static void MapSeatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/seats")
            .WithTags("Seats");

        // Read operations
        group.MapGet("/hall/{hallId}", GetSeatsByHallId)
            .WithName("GetSeatsByHallId")
            .WithOpenApi();

        group.MapGet("/{id}", GetSeatById)
            .WithName("GetSeatById")
            .WithOpenApi();

        // CRUD operations
        group.MapPost("/", CreateSeat)
            .WithName("CreateSeat")
            .WithOpenApi();

        group.MapPost("/bulk", BulkCreateSeats)
            .WithName("BulkCreateSeats")
            .WithOpenApi();

        group.MapPut("/{id}", UpdateSeat)
            .WithName("UpdateSeat")
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteSeat)
            .WithName("DeleteSeat")
            .WithOpenApi();

        group.MapPost("/bulk-delete", BulkDeleteSeats)
            .WithName("BulkDeleteSeats")
            .WithOpenApi();
    }

    private static async Task<IResult> GetSeatsByHallId(
        Guid hallId,
        ISeatService service,
        HttpContext context)
    {
        var response = await service.GetByHallIdAsync(hallId);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetSeatById(
        Guid id,
        ISeatService service,
        HttpContext context)
    {
        var response = await service.GetByIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> CreateSeat(
        CreateSeatRequest request,
        ISeatService service,
        HttpContext context)
    {
        var response = await service.CreateAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> BulkCreateSeats(
        BulkCreateSeatsRequest request,
        ISeatService service,
        HttpContext context)
    {
        var response = await service.BulkCreateAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> UpdateSeat(
        Guid id,
        UpdateSeatRequest request,
        ISeatService service,
        HttpContext context)
    {
        var response = await service.UpdateAsync(id, request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> DeleteSeat(
        Guid id,
        ISeatService service,
        HttpContext context)
    {
        var response = await service.DeleteAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> BulkDeleteSeats(
        List<Guid> seatIds,
        ISeatService service,
        HttpContext context)
    {
        var response = await service.BulkDeleteAsync(seatIds);
        response.SetTraceId(context);
        return response.ToResult();
    }
}
