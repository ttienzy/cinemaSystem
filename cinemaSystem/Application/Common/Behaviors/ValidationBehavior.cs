using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that auto-validates commands/queries
    /// using registered FluentValidation validators before the handler runs.
    /// Throws ValidationException with all errors if invalid.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, ct)));

            var failures = results
                .Where(r => r.Errors.Count != 0)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count != 0)
                throw new Application.Common.Exceptions.ValidationException(failures);

            return await next();
        }
    }
}
