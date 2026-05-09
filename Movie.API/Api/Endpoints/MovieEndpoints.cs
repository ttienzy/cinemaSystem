using Cinema.Shared.Extensions;
using Cinema.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Movie.API.Application.DTOs;

namespace Movie.API.Api.Endpoints;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movies")
            .WithTags("Movies");

        group.MapGet("/", GetAllMovies)
            .WithName("GetAllMovies")
            .WithOpenApi();

        group.MapGet("/admin/list", GetAdminMovies)
            .WithName("GetAdminMovies")
            .WithOpenApi();

        group.MapGet("/admin/summary", GetAdminMovieSummary)
            .WithName("GetAdminMovieSummary")
            .WithOpenApi();

        group.MapGet("/{id}", GetMovieById)
            .WithName("GetMovieById")
            .WithOpenApi();

        group.MapPost("/", CreateMovie)
            .WithName("CreateMovie")
            .Accepts<CreateMovieRequest>("multipart/form-data")
            .DisableAntiforgery()
            .WithOpenApi();

        group.MapPut("/{id}", UpdateMovie)
            .WithName("UpdateMovie")
            .Accepts<UpdateMovieRequest>("multipart/form-data")
            .DisableAntiforgery()
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteMovie)
            .WithName("DeleteMovie")
            .WithOpenApi();

        group.MapGet("/genre/{genreId}", GetMoviesByGenre)
            .WithName("GetMoviesByGenre")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllMovies(
        IMovieService service,
        HttpContext context,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var response = await service.GetAllAsync(pageNumber, pageSize);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetMovieById(Guid id, IMovieService service, HttpContext context)
    {
        var response = await service.GetByIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetAdminMovies(
        IMovieService service,
        HttpContext context,
        string? search = null,
        string? status = null,
        Guid? genreId = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var response = await service.GetAdminListAsync(search, status, genreId, pageNumber, pageSize);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetAdminMovieSummary(
        IMovieService service,
        HttpContext context)
    {
        var response = await service.GetAdminSummaryAsync();
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> CreateMovie(
        [FromForm] CreateMovieRequest request,
        IMovieService service,
        HttpContext context)
    {
        var response = await service.CreateAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> UpdateMovie(
        Guid id,
        [FromForm] UpdateMovieRequest request,
        IMovieService service,
        HttpContext context)
    {
        var response = await service.UpdateAsync(id, request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> DeleteMovie(Guid id, IMovieService service, HttpContext context)
    {
        var response = await service.DeleteAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetMoviesByGenre(Guid genreId, IMovieService service, HttpContext context)
    {
        var response = await service.GetByGenreAsync(genreId);
        response.SetTraceId(context);
        return response.ToResult();
    }
}


