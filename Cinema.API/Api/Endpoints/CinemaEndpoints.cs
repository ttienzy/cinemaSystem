using Cinema.API.Application.DTOs;
using Cinema.Shared.Extensions;
using Cinema.Shared.Models;

namespace Cinema.API.Api.Endpoints;

public static class CinemaEndpoints
{
    public static void MapCinemaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cinemas")
            .WithTags("Cinemas");

        group.MapGet("/", GetAllCinemas)
            .WithName("GetAllCinemas")
            .WithOpenApi();

        group.MapGet("/admin/overview", GetCinemaAdminOverview)
            .WithName("GetCinemaAdminOverview")
            .WithOpenApi();

        group.MapGet("/admin/summary", GetCinemaAdminSummary)
            .WithName("GetCinemaAdminSummary")
            .WithOpenApi();

        group.MapGet("/{id}", GetCinemaById)
            .WithName("GetCinemaById")
            .WithOpenApi();

        group.MapPost("/", CreateCinema)
            .WithName("CreateCinema")
            .WithOpenApi();

        group.MapPut("/{id}", UpdateCinema)
            .WithName("UpdateCinema")
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteCinema)
            .WithName("DeleteCinema")
            .WithOpenApi();
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
}


