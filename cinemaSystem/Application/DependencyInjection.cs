using Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    /// <summary>
    /// Registers all Application layer services.
    /// Called from Api/Program.cs: builder.Services.AddApplication()
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR: auto-register all Commands, Queries, and Event Handlers
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            // FluentValidation: auto-register all validators in Application assembly
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // MediatR Pipeline Behaviors (order matters — validation runs first)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            return services;
        }
    }
}
