using Application.Common.Exceptions;
using Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security;
using Microsoft.IdentityModel.Tokens;

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
                ConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict Occurred", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Access Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1"),
                DomainException => (StatusCodes.Status400BadRequest, "Business Rule Violation", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),

                // JWT/Security Token Exceptions
                SecurityTokenException => (StatusCodes.Status401Unauthorized, GetSecurityTokenErrorTitle(exception), "https://tools.ietf.org/html/rfc7235#section-3.1"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Access Denied", "https://tools.ietf.org/html/rfc7235#section-3.1"),

                // Argument Exceptions
                ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid Argument", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Argument", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),

                // Operation Exceptions
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid Operation", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Key Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),

                // Format Exceptions
                FormatException => (StatusCodes.Status400BadRequest, "Invalid Format", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                OverflowException => (StatusCodes.Status400BadRequest, "Value Overflow", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),

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

        private static string GetSecurityTokenErrorTitle(Exception exception)
        {
            // Use exception type name for more specific error messages
            return exception.GetType().Name switch
            {
                "SecurityTokenExpiredException" => "Token Expired",
                "SecurityTokenInvalidSignatureException" => "Invalid Token Signature",
                "SecurityTokenInvalidLifetimeException" => "Invalid Token Lifetime",
                "SecurityTokenReplayAttackException" => "Token Replay Detected",
                "SecurityTokenException" => "Invalid Token",
                _ => "Invalid Token"
            };
        }
    }
}
