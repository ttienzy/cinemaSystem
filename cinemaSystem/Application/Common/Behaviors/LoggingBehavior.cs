using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that logs each request name, elapsed time,
    /// and warns on slow requests (> 500ms).
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse>(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogInformation("Handling {RequestName}", requestName);

            var startTime = DateTimeOffset.UtcNow;
            var response = await next();
            var elapsed = DateTimeOffset.UtcNow - startTime;

            if (elapsed.TotalMilliseconds > 500)
            {
                logger.LogWarning(
                    "Slow request: {RequestName} took {ElapsedMs}ms",
                    requestName, elapsed.TotalMilliseconds);
            }
            else
            {
                logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMs}ms",
                    requestName, elapsed.TotalMilliseconds);
            }

            return response;
        }
    }
}
