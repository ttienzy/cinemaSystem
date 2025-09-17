using Shared.Models.ExtenalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Integrations
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailRequest request);
    }
}
