using Microsoft.AspNetCore.Http;
using Shared.Models.PaymentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Integrations
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInfomationRequest request, HttpContext context);
        PaymentResponse PaymentExecute(IQueryCollection collections);
    }
}
