namespace Payment.API.Infrastructure.Configuration;

/// <summary>
/// SePay configuration options (from official docs)
/// </summary>
public class SePayOptions
{
    public const string SectionName = "SePay";

    public string MerchantId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Environment { get; set; } = "sandbox"; // "sandbox" | "production"

    private bool IsSandbox =>
        string.Equals(Environment, "sandbox", StringComparison.OrdinalIgnoreCase);

    // Official URLs from SePay docs
    // ✅ CRITICAL: Forms must submit to pay-sandbox, NOT pgapi-sandbox
    // - pay-sandbox.sepay.vn = Payment page (accepts form POST from browser)
    // - pgapi-sandbox.sepay.vn = API server (for backend REST calls only)
    public string CheckoutFormActionUrl => IsSandbox
        ? "https://pay-sandbox.sepay.vn/v1/checkout/init"
        : "https://pay.sepay.vn/v1/checkout/init";

    public string SuccessUrl { get; set; } = string.Empty;
    public string ErrorUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
}


