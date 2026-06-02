using Booking.API.Client;

namespace Booking.API.Endpoints;

public static class ApiResponseResultExtensions
{
    public static IResult ToResult<T>(this ApiResponse<T> response)
    {
        return Results.Json(response, statusCode: response.StatusCode);
    }

    public static IResult ToResult(this ApiResponse response)
    {
        return Results.Json(response, statusCode: response.StatusCode);
    }

    public static void SetTraceId<T>(this ApiResponse<T> response, HttpContext context)
    {
        response.TraceId = context.TraceIdentifier;
    }

    public static void SetTraceId(this ApiResponse response, HttpContext context)
    {
        response.TraceId = context.TraceIdentifier;
    }
}
