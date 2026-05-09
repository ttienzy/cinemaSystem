using Cinema.API.Application.DTOs;
using Cinema.Shared.Extensions;
using Cinema.Shared.Models;

namespace Cinema.API.Api.Endpoints;

public static class CinemaHallEndpoints
{
    public static void MapCinemaHallEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cinema-halls")
            .WithTags("Cinema Halls");

        // Read operations
        group.MapGet("/cinema/{cinemaId}", GetHallsByCinemaId)
            .WithName("GetHallsByCinemaId")
            .WithOpenApi();

        group.MapGet("/{id}", GetHallById)
            .WithName("GetHallById")
            .WithOpenApi();

        group.MapPost("/lookup", LookupHalls)
            .WithName("LookupCinemaHalls")
            .WithOpenApi();

        group.MapGet("/{id}/seats", GetSeatsByHallId)
            .WithName("GetHallSeats")
            .WithOpenApi();

        // CRUD operations
        group.MapPost("/", CreateHall)
            .WithName("CreateCinemaHall")
            .WithOpenApi();

        group.MapPut("/{id}", UpdateHall)
            .WithName("UpdateCinemaHall")
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteHall)
            .WithName("DeleteCinemaHall")
            .WithOpenApi();
    }

    private static async Task<IResult> GetHallsByCinemaId(
        Guid cinemaId,
        ICinemaHallService service,
        HttpContext context)
    {
        var response = await service.GetByCinemaIdAsync(cinemaId);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetHallById(Guid id, ICinemaHallService service, HttpContext context)
    {
        var response = await service.GetByIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> LookupHalls(
        CinemaHallLookupRequest request,
        ICinemaHallService service,
        HttpContext context)
    {
        var response = await service.GetByIdsAsync(request.CinemaHallIds);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetSeatsByHallId(
        Guid id,
        ICinemaHallService service,
        HttpContext context)
    {
        var response = await service.GetSeatsByHallIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }




    private static async Task<IResult> CreateHall(
        CreateCinemaHallRequest request,
        ICinemaHallService service,
        HttpContext context)
    {
        var response = await service.CreateAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> UpdateHall(
        Guid id,
        UpdateCinemaHallRequest request,
        ICinemaHallService service,
        HttpContext context)
    {
        var response = await service.UpdateAsync(id, request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> DeleteHall(
        Guid id,
        ICinemaHallService service,
        HttpContext context)
    {
        var response = await service.DeleteAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }
}
