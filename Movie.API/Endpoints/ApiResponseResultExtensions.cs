using Movie.API.Client;

namespace Movie.API.Endpoints;

public static class ApiResponseResultExtensions
{
    public static void SetTraceId<T>(this ApiResponse<T> response, HttpContext context)
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
            403 => Results.StatusCode(StatusCodes.Status403Forbidden),
            404 => Results.NotFound(response),
            409 => Results.Conflict(response),
            422 => Results.UnprocessableEntity(response),
            _ => Results.StatusCode(response.StatusCode)
        };
    }
}
