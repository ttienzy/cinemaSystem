using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Base
{
    public class Error
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        public Error(HttpStatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
        /// <summary>
        /// Creates an <see cref="Error"/> instance representing a "Bad Request" (HTTP 400) error.
        /// </summary>
        /// <param name="message">The error message describing the reason for the bad request. This value cannot be null or empty.</param>
        /// <returns>An <see cref="Error"/> object with a status code of <see cref="HttpStatusCode.BadRequest"/> and the
        /// specified error message.</returns>
        public static Error BadRequest(string message) => new Error(HttpStatusCode.BadRequest, message);
        /// <summary>
        /// Creates an <see cref="Error"/> instance representing an HTTP 401 Unauthorized error.
        /// </summary>
        /// <param name="message">The error message describing the reason for the unauthorized status.</param>
        /// <returns>An <see cref="Error"/> object with a status code of <see cref="HttpStatusCode.Unauthorized"/> and the
        /// specified error message.</returns>
        public static Error Unauthorized(string message) => new Error(HttpStatusCode.Unauthorized, message);
        /// <summary>
        /// Creates an <see cref="Error"/> instance representing a "Forbidden" HTTP status.
        /// </summary>
        /// <param name="message">The error message describing the reason for the forbidden status.</param>
        /// <returns>An <see cref="Error"/> object with a status code of <see cref="HttpStatusCode.Forbidden"/> and the specified
        /// message.</returns>
        public static Error Forbidden(string message) => new Error(HttpStatusCode.Forbidden, message);
        /// <summary>
        /// Creates an <see cref="Error"/> instance representing a "Not Found" (404) HTTP status code.
        /// </summary>
        /// <param name="message">The error message describing the reason for the "Not Found" status.</param>
        /// <returns>An <see cref="Error"/> object with a 404 status code and the specified error message.</returns>
        public static Error NotFound(string message) => new Error(HttpStatusCode.NotFound, message);
        /// <summary>
        /// Creates an <see cref="Error"/> instance representing a conflict error (HTTP 409).
        /// </summary>
        /// <param name="message">The error message describing the conflict.</param>
        /// <returns>An <see cref="Error"/> object with a status code of <see cref="HttpStatusCode.Conflict"/> and the specified
        /// message.</returns>
        public static Error Conflict(string message) => new Error(HttpStatusCode.Conflict, message);
        /// <summary>
        /// Creates an <see cref="Error"/> instance representing an internal server error (HTTP 500).
        /// </summary>
        /// <param name="message">The error message describing the internal server error.</param>
        /// <returns>An <see cref="Error"/> object with a status code of <see cref="HttpStatusCode.InternalServerError"/> and the
        /// specified error message.</returns>
        public static Error InternalServerError(string message) => new Error(HttpStatusCode.InternalServerError, message);
        
    }
}
