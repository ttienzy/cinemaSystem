using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Base
{
    public class ErrorReponse<T> : IActionResult where T : class
    {
        public Error Error { get; set; }
        public ErrorReponse(Error error)
        {
            Error = error;
        }
        /// <summary>
        /// Creates an <see cref="ErrorReponse{T}"/> instance using the error information from the specified <see
        /// cref="BaseResponse{T}"/>.
        /// </summary>
        /// <param name="baseResponse">The base response containing the error details to be used for creating the error response. Must not be null.</param>
        /// <returns>An <see cref="ErrorReponse{T}"/> initialized with the error details from the provided <paramref
        /// name="baseResponse"/>.</returns>
        public static ErrorReponse<T> WithError(BaseResponse<T> baseResponse) => new ErrorReponse<T>(baseResponse.Error);
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)Error.StatusCode;
            response.ContentType = "application/json";
            await response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = (int)Error.StatusCode,
                message = Error.Message
            }));
        }
    }
}
