using Payment.API.Client;

namespace Payment.API.Endpoints;

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
}
