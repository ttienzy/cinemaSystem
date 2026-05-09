using Cinema.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace Cinema.Shared.Extensions;

public static class HttpContextExtensions
{
    public static void SetTraceId<T>(this ApiResponse<T> response, HttpContext context)
    {
        response.TraceId = context.TraceIdentifier;
    }

    public static void SetTraceId(this ApiResponse response, HttpContext context)
    {
        response.TraceId = context.TraceIdentifier;
    }

    public static IResult ToResult<T>(this ApiResponse<T> response)
    {
        return response.StatusCode switch
        {
            200 => Results.Ok(response),
            201 => Results.Created(string.Empty, response),
            204 => Results.NoContent(),
            400 => Results.BadRequest(response),
            401 => Results.Unauthorized(),
            403 => Results.StatusCode(403),
            404 => Results.NotFound(response),
            422 => Results.UnprocessableEntity(response),
            _ => Results.StatusCode(response.StatusCode)
        };
    }

    public static IResult ToResult(this ApiResponse response)
    {
        return response.StatusCode switch
        {
            200 => Results.Ok(response),
            201 => Results.Created(string.Empty, response),
            204 => Results.NoContent(),
            400 => Results.BadRequest(response),
            401 => Results.Unauthorized(),
            403 => Results.StatusCode(403),
            404 => Results.NotFound(response),
            422 => Results.UnprocessableEntity(response),
            _ => Results.StatusCode(response.StatusCode)
        };
    }
}
