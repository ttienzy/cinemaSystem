using Application.Common.Exceptions;
using Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Middleware
{
    public class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, type) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict Occurred", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Access Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3"),
                DomainException => (StatusCodes.Status400BadRequest, "Business Rule Violation", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                _ => (0, string.Empty, string.Empty)
            };

            if (statusCode == 0)
            {
                return false;
            }

            logger.LogWarning(exception, "Domain exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
