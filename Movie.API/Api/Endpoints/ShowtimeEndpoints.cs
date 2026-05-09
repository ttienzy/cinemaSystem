using Cinema.Shared.Extensions;
using Movie.API.Application.DTOs;

namespace Movie.API.Api.Endpoints;

public static class ShowtimeEndpoints
{
    public static void MapShowtimeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/showtimes")
            .WithTags("Showtimes");

        group.MapGet("/{id}", GetShowtimeById)
            .WithName("GetShowtimeById")
            .WithOpenApi();

        group.MapGet("/movie/{movieId}", GetShowtimesByMovieId)
            .WithName("GetShowtimesByMovieId")
            .WithOpenApi();

        group.MapGet("/cinemahall/{cinemaHallId}", GetShowtimesByCinemaHallId)
            .WithName("GetShowtimesByCinemaHallId")
            .WithOpenApi();

        group.MapGet("/upcoming", GetUpcomingShowtimes)
            .WithName("GetUpcomingShowtimes")
            .WithOpenApi();

        group.MapGet("/range", GetShowtimesByRange)
            .WithName("GetShowtimesByRange")
            .WithOpenApi();

        group.MapGet("/timeline", GetTimeline)
            .WithName("GetShowtimeTimeline")
            .WithOpenApi();

        group.MapPost("/lookup", LookupShowtimes)
            .WithName("LookupShowtimes")
            .WithOpenApi();

        group.MapPost("/validate-slot", ValidateShowtimeSlot)
            .WithName("ValidateShowtimeSlot")
            .WithOpenApi();

        group.MapPost("/", CreateShowtime)
            .WithName("CreateShowtime")
            .WithOpenApi();

        group.MapPut("/{id}", UpdateShowtime)
            .WithName("UpdateShowtime")
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteShowtime)
            .WithName("DeleteShowtime")
            .WithOpenApi();
    }

    private static async Task<IResult> GetShowtimeById(
        Guid id,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.GetByIdAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetShowtimesByMovieId(
        Guid movieId,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.GetByMovieIdAsync(movieId);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetShowtimesByCinemaHallId(
        Guid cinemaHallId,
        IShowtimeService service,
        HttpContext context)
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

    private static async Task<IResult> GetTimeline(
        Guid cinemaId,
        DateTime date,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.GetTimelineAsync(cinemaId, date);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> ValidateShowtimeSlot(
        ValidateShowtimeSlotRequest request,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.ValidateSlotAsync(request);
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

    private static async Task<IResult> DeleteShowtime(
        Guid id,
        IShowtimeService service,
        HttpContext context)
    {
        var response = await service.DeleteAsync(id);
        response.SetTraceId(context);
        return response.ToResult();
    }
}


