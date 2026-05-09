using Payment.API.Application.DTOs.SePay;
using PaymentModel = Payment.API.Domain.Entities.PaymentEntity;

namespace Payment.API.Infrastructure.Integrations.SePay;

public interface ISePayService
{
    SePayCheckoutResult BuildCheckout(PaymentModel payment);
    bool ValidateIpnSecretKey(string? receivedSecretKey);
}


