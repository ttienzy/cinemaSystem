using Cinema.Shared.Extensions;
using Cinema.Shared.Models;
using Movie.API.Application.DTOs;

namespace Movie.API.Api.Endpoints;

public static class GenreEndpoints
{
    public static void MapGenreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/genres")
            .WithTags("Genres");

        group.MapGet("/", GetAllGenres)
            .WithName("GetAllGenres")
            .WithOpenApi();

        group.MapGet("/{id}", GetGenreById)
            .WithName("GetGenreById")
            .WithOpenApi();

        group.MapPost("/", CreateGenre)
            .WithName("CreateGenre")
            .WithOpenApi();

        group.MapPut("/{id}", UpdateGenre)
            .WithName("UpdateGenre")
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteGenre)
            .WithName("DeleteGenre")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllGenres(IGenreService service, HttpContext context)
    {
        var response = await service.GetAllAsync();
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetGenreById(Guid id, IGenreService service, HttpContext context)
    {
        var response = await service.GetByIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> CreateGenre(
        CreateGenreRequest request,
        IGenreService service,
        HttpContext context)
    {
        var response = await service.CreateAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> UpdateGenre(
        Guid id,
        CreateGenreRequest request,
        IGenreService service,
        HttpContext context)
    {
        var response = await service.UpdateAsync(id, request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> DeleteGenre(Guid id, IGenreService service, HttpContext context)
    {
        var response = await service.DeleteAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }
}


