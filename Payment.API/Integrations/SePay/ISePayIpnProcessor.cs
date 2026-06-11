using Payment.API.Client;

namespace Payment.API.Integrations.SePay;

public interface ISePayIpnProcessor
{
    Task ProcessAsync(SePayIpnPayload payload, string? receivedSecretKey);
}


