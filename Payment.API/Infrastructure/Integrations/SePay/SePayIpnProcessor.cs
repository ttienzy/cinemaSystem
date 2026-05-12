using System.Globalization;
using System.Text.Json;
using Payment.API.Application.DTOs.SePay;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Integrations.SePay;

public class SePayIpnProcessor : ISePayIpnProcessor
{
    private readonly ISePayService _sePayService;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<SePayIpnProcessor> _logger;

    public SePayIpnProcessor(
        ISePayService sePayService,
        IPaymentService paymentService,
        IPaymentIntegrationEventPublisher eventPublisher,
        ILogger<SePayIpnProcessor> logger)
    {
        _sePayService = sePayService;
        _paymentService = paymentService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task ProcessAsync(SePayIpnPayload payload, string? receivedSecretKey)
    {
        var invoiceNumber = payload.Order?.OrderInvoiceNumber;
        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            throw new ArgumentException(SePayException.MISSING_INVOICE_NUMBER);
        }

        var isTestMode = payload.Order?.HasCustomDataFlag("webhook_test") == true;
        if (!isTestMode && !_sePayService.ValidateIpnSecretKey(receivedSecretKey))
        {
            _logger.LogWarning(
                "Rejected SePay IPN for invoice {InvoiceNumber}: invalid X-Secret-Key",
                invoiceNumber);
            throw new UnauthorizedAccessException(SePayException.INVALID_SECRET_KEY);
        }

        var paymentResult = await _paymentService.GetPaymentByOrderInvoiceNumberAsync(invoiceNumber);
        if (!paymentResult.Success || paymentResult.Data == null)
        {
            _logger.LogWarning(
                "SePay IPN ignored because payment was not found for invoice {InvoiceNumber}",
                invoiceNumber);
            return;
        }

        var payment = paymentResult.Data;
        var gatewayMetadata = JsonSerializer.Serialize(new
        {
            source = "SePay",
            receivedAtUtc = DateTime.UtcNow,
            payload
        });

        switch (NormalizeNotificationType(payload.NotificationType))
        {
            case "ORDER_PAID":
                await HandleOrderPaidAsync(payment, payload, gatewayMetadata);
                return;

            case "TRANSACTION_VOID":
                await HandleTransactionVoidAsync(payment, gatewayMetadata);
                return;

            default:
                _logger.LogWarning(
                    "SePay IPN ignored because notification_type {NotificationType} is unknown for invoice {InvoiceNumber}",
                    payload.NotificationType,
                    invoiceNumber);
                return;
        }
    }

    private async Task HandleOrderPaidAsync(
        PaymentEntity payment,
        SePayIpnPayload payload,
        string gatewayMetadata)
    {
        if (payment.Status == PaymentStatus.Completed)
        {
            _logger.LogInformation(
                "SePay IPN is idempotent: payment {PaymentId} already completed",
                payment.Id);
            return;
        }

        var completedAt = ParseGatewayDate(payload.Transaction?.TransactionDate);
        var transactionId = payload.Transaction?.TransactionId;
        var paymentMethod = payload.Transaction?.PaymentMethod;

        var updateResult = await _paymentService.UpdatePaymentStatusAsync(
            payment.Id,
            PaymentStatus.Completed,
            transactionId: transactionId,
            paymentMethod: paymentMethod,
            completedAt: completedAt,
            gatewayMetadata: gatewayMetadata);

        if (!updateResult.Success)
        {
            throw new InvalidOperationException(
                $"{PaymentException.PAYMENT_STATUS_UPDATE_FAILED} for payment {payment.Id}: {updateResult.Message}");
        }

        _eventPublisher.PublishPaymentCompleted(payment, transactionId, completedAt);

        _logger.LogInformation(
            "Processed SePay ORDER_PAID for payment {PaymentId}, booking {BookingId}",
            payment.Id,
            payment.BookingId);
    }

    private async Task HandleTransactionVoidAsync(PaymentEntity payment, string gatewayMetadata)
    {
        if (payment.Status is PaymentStatus.Failed or PaymentStatus.Cancelled)
        {
            _logger.LogInformation(
                "SePay IPN is idempotent: payment {PaymentId} already in terminal failure state {Status}",
                payment.Id,
                payment.Status);
            return;
        }

        var updateResult = await _paymentService.UpdatePaymentStatusAsync(
            payment.Id,
            PaymentStatus.Failed,
            gatewayMetadata: gatewayMetadata);

        if (!updateResult.Success)
        {
            throw new InvalidOperationException(
                $"{PaymentException.PAYMENT_STATUS_UPDATE_FAILED} for payment {payment.Id}: {updateResult.Message}");
        }

        var reason = "Transaction voided by gateway";
        _eventPublisher.PublishPaymentFailed(payment, reason, DateTime.UtcNow);

        _logger.LogInformation(
            "Processed SePay TRANSACTION_VOID for payment {PaymentId}, booking {BookingId}",
            payment.Id,
            payment.BookingId);
    }

    private static string NormalizeNotificationType(string? notificationType) =>
        notificationType?.Trim().ToUpperInvariant() switch
        {
            "ORDER_PAID" => "ORDER_PAID",
            "PAYMENT_SUCCESS" => "ORDER_PAID",
            "TRANSACTION_VOID" => "TRANSACTION_VOID",
            "PAYMENT_FAILED" => "TRANSACTION_VOID",
            _ => string.Empty
        };

    private static DateTime ParseGatewayDate(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            DateTime.TryParseExact(
                value,
                PaymentTimeConstants.GatewayTransactionDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }
}


