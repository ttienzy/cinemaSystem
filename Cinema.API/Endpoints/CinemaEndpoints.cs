using Cinema.API.Client;
using Cinema.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace Cinema.API.Endpoints;

public static class CinemaEndpoints
{
    public static IEndpointRouteBuilder MapCinemaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api");

        MapCinemaRoutes(group);
        MapCinemaHallRoutes(group);
        MapSeatRoutes(group);

        return app;
    }

    private static void MapCinemaRoutes(RouteGroupBuilder group)
    {
        var cinemas = group.MapGroup("/cinemas").WithTags("Cinemas");
        var admin = cinemas.MapGroup("/admin").RequireAuthorization(AdminOnly());

        cinemas.MapGet("", GetAllCinemas);
        cinemas.MapGet("/{id}", GetCinemaById);
        cinemas.MapPost("", CreateCinema).RequireAuthorization(AdminOnly());
        cinemas.MapPut("/{id}", UpdateCinema).RequireAuthorization(AdminOnly());
        cinemas.MapDelete("/{id}", DeleteCinema).RequireAuthorization(AdminOnly());

        admin.MapGet("/overview", GetCinemaAdminOverview);
        admin.MapGet("/summary", GetCinemaAdminSummary);
    }

    private static void MapCinemaHallRoutes(RouteGroupBuilder group)
    {
        var halls = group.MapGroup("/cinema-halls").WithTags("Cinema Halls");

        halls.MapGet("/cinema/{cinemaId}", GetHallsByCinemaId);
        halls.MapGet("/{id}", GetHallById);
        halls.MapPost("/lookup", LookupHalls);
        halls.MapGet("/{id}/seats", GetHallSeats);

        halls.MapPost("", CreateHall).RequireAuthorization(AdminOnly());
        halls.MapPut("/{id}", UpdateHall).RequireAuthorization(AdminOnly());
        halls.MapDelete("/{id}", DeleteHall).RequireAuthorization(AdminOnly());
    }

    private static void MapSeatRoutes(RouteGroupBuilder group)
    {
        var seats = group.MapGroup("/seats").WithTags("Seats");

        seats.MapGet("/hall/{hallId}", GetSeatsByCinemaHallId);
        seats.MapGet("/{id}", GetSeatById);

        seats.MapPost("", CreateSeat).RequireAuthorization(AdminOnly());
        seats.MapPost("/bulk", BulkCreateSeats).RequireAuthorization(AdminOnly());
        seats.MapPut("/{id}", UpdateSeat).RequireAuthorization(AdminOnly());
        seats.MapDelete("/{id}", DeleteSeat).RequireAuthorization(AdminOnly());
        seats.MapPost("/bulk-delete", BulkDeleteSeats).RequireAuthorization(AdminOnly());
    }

    private static AuthorizeAttribute AdminOnly()
    {
        return new AuthorizeAttribute { Roles = CinemaConstants.AdminRole };
    }

    private static async Task<IResult> GetAllCinemas(
        ICinemaService service,
        HttpContext context,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var response = await service.GetAllAsync(pageNumber, pageSize);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetCinemaById(Guid id, ICinemaService service, HttpContext context)
    {
        var response = await service.GetByIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetCinemaAdminOverview(
        ICinemaService service,
        HttpContext context,
        string? search = null,
        string? city = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var response = await service.GetAdminOverviewAsync(search, city, status, pageNumber, pageSize);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetCinemaAdminSummary(
        ICinemaService service,
        HttpContext context)
    {
        var response = await service.GetAdminSummaryAsync();
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> CreateCinema(
        CreateCinemaRequest request,
        ICinemaService service,
        HttpContext context)
    {
        var response = await service.CreateAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> UpdateCinema(
        Guid id,
        CreateCinemaRequest request,
        ICinemaService service,
        HttpContext context)
    {
        var response = await service.UpdateAsync(id, request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> DeleteCinema(Guid id, ICinemaService service, HttpContext context)
    {
        var response = await service.DeleteAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
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

    private static async Task<IResult> GetHallSeats(
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

    private static async Task<IResult> GetSeatsByCinemaHallId(
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
