using System.Net;

namespace Cinema.Shared.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public List<ErrorDetail>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = true,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> FailureResponse(string message, int statusCode = 400, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse<T>
        {
            Data = default,
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
    }

    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found")
    {
        return FailureResponse(message, 404);
    }

    public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized")
    {
        return FailureResponse(message, 401);
    }

    public static ApiResponse<T> ForbiddenResponse(string message = "Forbidden")
    {
        return FailureResponse(message, 403);
    }

    public static ApiResponse<T> ValidationErrorResponse(string message, List<ErrorDetail> errors)
    {
        return FailureResponse(message, 422, errors);
    }
    public static ApiResponse<T> InternalServerErrorResponse(string message = "Internal server error")
    {
        return FailureResponse(message, 500);
    }
    public static ApiResponse<T> ConflictResponse(string message = "Conflict")
    {
        return FailureResponse(message, 409);
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public List<ErrorDetail>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ApiResponse SuccessResponse(string message = "Success", int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ApiResponse FailureResponse(string message, int statusCode = 400, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
    }

    public static ApiResponse NotFoundResponse(string message = "Resource not found")
    {
        return FailureResponse(message, 404);
    }

    public static ApiResponse UnauthorizedResponse(string message = "Unauthorized")
    {
        return FailureResponse(message, 401);
    }

    public static ApiResponse ForbiddenResponse(string message = "Forbidden")
    {
        return FailureResponse(message, 403);
    }
    public static ApiResponse InternalServerErrorResponse(string message = "Internal server error")
    {
        return FailureResponse(message, 500);
    }
    public static ApiResponse ConflictResponse(string message = "Conflict")
    {
        return FailureResponse(message, 409);
    }
}

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }

    public ErrorDetail() { }

    public ErrorDetail(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }
}
