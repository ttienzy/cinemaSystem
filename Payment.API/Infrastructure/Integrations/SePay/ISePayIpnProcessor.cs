using Payment.API.Application.DTOs.SePay;

namespace Payment.API.Infrastructure.Integrations.SePay;

public interface ISePayIpnProcessor
{
    Task ProcessAsync(SePayIpnPayload payload, string? receivedSecretKey);
}


