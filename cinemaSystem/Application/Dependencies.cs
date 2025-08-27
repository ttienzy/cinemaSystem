using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class Dependencies
    {
        public static void AddApplicationDependencies(IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssemblyContaining<AssemblyReferenceMarker>();
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
        }
    }
}
