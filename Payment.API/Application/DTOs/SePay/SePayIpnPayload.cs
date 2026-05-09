using System.Text.Json;
using System.Text.Json.Serialization;

namespace Payment.API.Application.DTOs.SePay;

/// <summary>
/// IPN Payload from SePay (official docs structure)
/// </summary>
public class SePayIpnPayload
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("notification_type")]
    public string NotificationType { get; set; } = string.Empty; // "ORDER_PAID" | "TRANSACTION_VOID"

    [JsonPropertyName("order")]
    public SePayIpnOrder? Order { get; set; }

    [JsonPropertyName("transaction")]
    public SePayIpnTransaction? Transaction { get; set; }

    [JsonPropertyName("customer")]
    public SePayIpnCustomer? Customer { get; set; }
}

public class SePayIpnOrder
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("order_status")]
    public string OrderStatus { get; set; } = string.Empty; // "CAPTURED"

    [JsonPropertyName("order_currency")]
    public string OrderCurrency { get; set; } = string.Empty;

    [JsonPropertyName("order_amount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? OrderAmount { get; set; }

    [JsonPropertyName("order_invoice_number")]
    public string OrderInvoiceNumber { get; set; } = string.Empty; // Lookup key

    [JsonPropertyName("custom_data")]
    public JsonElement? CustomData { get; set; }

    [JsonPropertyName("user_agent")]
    public JsonElement? UserAgent { get; set; }

    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; set; }

    [JsonPropertyName("order_description")]
    public string OrderDescription { get; set; } = string.Empty;

    public bool HasCustomDataFlag(string key)
    {
        if (CustomData is not { } customData || customData.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!customData.TryGetProperty(key, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(property.GetString(), out var parsed) && parsed,
            JsonValueKind.Number => property.TryGetInt32(out var number) && number != 0,
            _ => false
        };
    }
}

public class SePayIpnTransaction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty; // "BANK_TRANSFER" | "CARD"

    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("transaction_type")]
    public string TransactionType { get; set; } = string.Empty;

    [JsonPropertyName("transaction_date")]
    public string TransactionDate { get; set; } = string.Empty; // "2025-09-01 00:00:15"

    [JsonPropertyName("transaction_status")]
    public string TransactionStatus { get; set; } = string.Empty; // "APPROVED"

    [JsonPropertyName("transaction_amount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TransactionAmount { get; set; }

    [JsonPropertyName("transaction_currency")]
    public string? TransactionCurrency { get; set; }

    [JsonPropertyName("authentication_status")]
    public string? AuthenticationStatus { get; set; }

    // Card-specific (null if bank transfer)
    [JsonPropertyName("card_number")]
    public string? CardNumber { get; set; }

    [JsonPropertyName("card_holder_name")]
    public string? CardHolderName { get; set; }

    [JsonPropertyName("card_expiry")]
    public string? CardExpiry { get; set; }

    [JsonPropertyName("card_funding_method")]
    public string? CardFundingMethod { get; set; }

    [JsonPropertyName("card_brand")]
    public string? CardBrand { get; set; } // "VISA", "MASTERCARD"
}

public class SePayIpnCustomer
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; } = string.Empty;
}


