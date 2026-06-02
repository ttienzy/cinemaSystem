using Payment.API.Client;
using PaymentModel = Payment.API.Entities.PaymentEntity;

namespace Payment.API.Integrations.SePay;

public interface ISePayService
{
    SePayCheckoutResult BuildCheckout(PaymentModel payment);
    bool ValidateIpnSecretKey(string? receivedSecretKey);
}
