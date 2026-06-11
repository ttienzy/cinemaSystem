using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movie.API.Client;
using Movie.API.Services;

namespace Movie.API.Endpoints;

public static class MovieEndpoints
{
    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api");

        MapMovieRoutes(group);
        MapGenreRoutes(group);
        MapShowtimeRoutes(group);

        return app;
    }

    private static void MapMovieRoutes(RouteGroupBuilder group)
    {
        var movies = group.MapGroup("/movies").WithTags("Movies");
        var admin = movies.MapGroup("/admin").RequireAuthorization(AdminOnly());

        movies.MapGet("", GetAllMovies);
        movies.MapGet("/{id}", GetMovieById);
        movies.MapGet("/genre/{genreId}", GetMoviesByGenre);
        movies.MapPost("", CreateMovie)
            .RequireAuthorization(AdminOnly())
            .Accepts<CreateMovieRequest>("multipart/form-data")
            .DisableAntiforgery();
        movies.MapPut("/{id}", UpdateMovie)
            .RequireAuthorization(AdminOnly())
            .Accepts<UpdateMovieRequest>("multipart/form-data")
            .DisableAntiforgery();
        movies.MapDelete("/{id}", DeleteMovie).RequireAuthorization(AdminOnly());

        admin.MapGet("/list", GetAdminMovies);
        admin.MapGet("/summary", GetAdminMovieSummary);
    }

    private static void MapGenreRoutes(RouteGroupBuilder group)
    {
        var genres = group.MapGroup("/genres").WithTags("Genres");

        genres.MapGet("", GetAllGenres);
        genres.MapGet("/{id}", GetGenreById);
        genres.MapPost("", CreateGenre).RequireAuthorization(AdminOnly());
        genres.MapPut("/{id}", UpdateGenre).RequireAuthorization(AdminOnly());
        genres.MapDelete("/{id}", DeleteGenre).RequireAuthorization(AdminOnly());
    }

    private static void MapShowtimeRoutes(RouteGroupBuilder group)
    {
        var showtimes = group.MapGroup("/showtimes").WithTags("Showtimes");

        showtimes.MapGet("/{id}", GetShowtimeById);
        showtimes.MapGet("/movie/{movieId}", GetShowtimesByMovieId);
        showtimes.MapGet("/cinemahall/{cinemaHallId}", GetShowtimesByCinemaHallId);
        showtimes.MapGet("/upcoming", GetUpcomingShowtimes);
        showtimes.MapGet("/range", GetShowtimesByRange);
        showtimes.MapPost("/lookup", LookupShowtimes);

        showtimes.MapPost("", CreateShowtime).RequireAuthorization(AdminOnly());
        showtimes.MapPut("/{id}", UpdateShowtime).RequireAuthorization(AdminOnly());
        showtimes.MapDelete("/{id}", DeleteShowtime).RequireAuthorization(AdminOnly());
    }

    private static AuthorizeAttribute AdminOnly()
    {
        return new AuthorizeAttribute { Roles = "Admin" };
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

    private static async Task<IResult> GetAdminMovieSummary(IMovieService service, HttpContext context)
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

    private static async Task<IResult> GetShowtimeById(Guid id, IShowtimeService service, HttpContext context)
    {
        var response = await service.GetByIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetShowtimesByMovieId(Guid movieId, IShowtimeService service, HttpContext context)
    {
        var response = await service.GetByMovieIdAsync(movieId);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetShowtimesByCinemaHallId(Guid cinemaHallId, IShowtimeService service, HttpContext context)
    {
        var response = await service.GetByCinemaHallIdAsync(cinemaHallId);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetUpcomingShowtimes(
        IShowtimeService service,
        HttpContext context,
        int count = 20)
    {
        var response = await service.GetUpcomingShowtimesAsync(count);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetShowtimesByRange(
        DateTime from,
        DateTime to,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.GetByRangeAsync(from, to);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> LookupShowtimes(
        ShowtimeLookupRequest request,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.GetLookupByIdsAsync(request.ShowtimeIds);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> CreateShowtime(
        CreateShowtimeRequest request,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.CreateAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> UpdateShowtime(
        Guid id,
        UpdateShowtimeRequest request,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.UpdateAsync(id, request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> DeleteShowtime(Guid id, IShowtimeService service, HttpContext context)
    {
        var response = await service.DeleteAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }
}
