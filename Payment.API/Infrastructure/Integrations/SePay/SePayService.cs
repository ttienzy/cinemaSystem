using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Payment.API.Infrastructure.Configuration;
using Payment.API.Application.DTOs.SePay;
using Payment.API.Domain.Exceptions;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Integrations.SePay;

/// <summary>
/// Anti-corruption layer for SePay contract generation and auth checks.
/// Checkout is form-based per current SePay docs, not a server-to-server JSON API call.
/// </summary>
public class SePayService : ISePayService
{
    // ✅ Field order MUST match SePay documentation exactly
    // Reference: https://developer.sepay.vn/en/cong-thanh-toan/sdk/nodejs
    private static readonly string[] SignedFieldOrder =
    {
        "order_amount",
        "merchant",
        "currency",
        "operation",
        "order_description",
        "order_invoice_number",
        "customer_id",
        "payment_method",  // ✅ CRITICAL: Required by SePay
        "success_url",
        "error_url",
        "cancel_url"
    };

    private readonly SePayOptions _options;
    private readonly ILogger<SePayService> _logger;

    public SePayService(
        IOptions<SePayOptions> options,
        ILogger<SePayService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public SePayCheckoutResult BuildCheckout(PaymentEntity payment)
    {
        if (string.IsNullOrWhiteSpace(_options.MerchantId))
        {
            throw new SePayException("SePay MerchantId is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new SePayException("SePay SecretKey is not configured.");
        }

        var fields = new Dictionary<string, string>
        {
            ["order_amount"] = payment.Amount.ToString(CultureInfo.InvariantCulture),
            ["merchant"] = _options.MerchantId,
            ["currency"] = payment.Currency,
            ["operation"] = "PURCHASE",
            ["order_description"] = payment.OrderDescription,
            ["order_invoice_number"] = payment.OrderInvoiceNumber,
            ["customer_id"] = payment.CustomerEmail ?? payment.CustomerPhone ?? "guest",  // ✅ Use customer info
            ["payment_method"] = payment.PaymentMethod ?? "BANK_TRANSFER",  // ✅ CRITICAL: Required field
            ["success_url"] = payment.SuccessUrl ?? _options.SuccessUrl,
            ["error_url"] = payment.ErrorUrl ?? _options.ErrorUrl,
            ["cancel_url"] = payment.CancelUrl ?? _options.CancelUrl
        };

        fields["signature"] = GenerateSignature(fields, _options.SecretKey);

        // ✅ DEBUG: Log all fields for troubleshooting
        _logger.LogInformation(
            "SePay checkout form fields: {@Fields}",
            fields.Select(f => new { f.Key, Value = f.Key == "signature" ? "***" : f.Value }));

        _logger.LogInformation(
            "Built SePay checkout form for invoice {InvoiceNumber}",
            payment.OrderInvoiceNumber);

        return new SePayCheckoutResult
        {
            CheckoutFormAction = _options.CheckoutFormActionUrl,
            CheckoutFormFields = fields,
            OrderInvoiceNumber = payment.OrderInvoiceNumber
        };
    }

    public bool ValidateIpnSecretKey(string? receivedSecretKey)
        => !string.IsNullOrWhiteSpace(receivedSecretKey)
           && string.Equals(receivedSecretKey, _options.SecretKey, StringComparison.Ordinal);

    private static string GenerateSignature(
        IReadOnlyDictionary<string, string> fields,
        string secretKey)
    {
        var signingParts = new List<string>();

        foreach (var field in SignedFieldOrder)
        {
            if (!fields.TryGetValue(field, out var value) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            signingParts.Add($"{field}={value}");
        }

        var signingString = string.Join(",", signingParts);
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(signingString);

        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToBase64String(hmac.ComputeHash(dataBytes));
    }
}


