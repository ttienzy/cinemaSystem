using System.Net;
using System.Text.Json;
using Booking.API.Application.DTOs.External;
using Cinema.Shared.Models;

namespace Booking.API.Infrastructure.Integrations.Clients;

public class PaymentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PaymentApiClient(HttpClient httpClient, ILogger<PaymentApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Wait for payment to be created by Payment.API (after BookingCreatedIntegrationEvent)
    /// Uses polling with exponential backoff
    /// </summary>
    public async Task<PaymentCheckoutDto?> WaitForPaymentCreationAsync(Guid bookingId, int maxRetries = 10, int initialDelayMs = 100)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var payment = await GetPaymentByBookingIdAsync(bookingId);

            if (payment != null)
            {
                // Payment found, now get checkout details
                return await GetPaymentCheckoutAsync(payment.PaymentId);
            }

            // Exponential backoff: 100ms, 200ms, 400ms, 800ms, 1600ms...
            var delay = initialDelayMs * (int)Math.Pow(2, attempt);
            await Task.Delay(Math.Min(delay, 3000)); // Cap at 3 seconds
        }

        _logger.LogWarning("Payment not created after {MaxRetries} attempts for booking {BookingId}", maxRetries, bookingId);
        return null;
    }

    /// <summary>
    /// Get payment checkout details (form action and fields for SePay)
    /// </summary>
    public async Task<PaymentCheckoutDto?> GetPaymentCheckoutAsync(Guid paymentId)
    {
        var url = $"/api/payments/{paymentId}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var paymentEntity = ApiResponseJsonHelper.DeserializeApiResponse<PaymentEntityDto>(content, JsonOptions);

            if (paymentEntity == null)
            {
                return null;
            }

            // Build checkout URL (relative path that frontend will use)
            var checkoutUrl = $"/api/payments/{paymentId}/checkout";

            return new PaymentCheckoutDto
            {
                PaymentId = paymentEntity.Id,
                BookingId = paymentEntity.BookingId,
                CheckoutUrl = checkoutUrl,
                CheckoutFormAction = string.Empty, // Will be populated by frontend when accessing checkout page
                CheckoutFormFields = new Dictionary<string, string>(),
                OrderInvoiceNumber = paymentEntity.OrderInvoiceNumber,
                Amount = paymentEntity.Amount,
                ExpiresAt = paymentEntity.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Payment.API to get checkout for payment {PaymentId}", paymentId);
            throw new ExternalServiceException($"Failed to get payment checkout from Payment.API: {ex.Message}", ex);
        }
    }

    public async Task<PaymentLookupDto?> GetPaymentByBookingIdAsync(Guid bookingId)
    {
        var url = $"/api/payments/booking/{bookingId}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var payment = ApiResponseJsonHelper.DeserializeApiResponse<PaymentLookupDto>(content, JsonOptions);
            if (payment != null && payment.PaymentId == Guid.Empty)
            {
                payment.PaymentId = payment.Id;
            }

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Payment.API to get payment for booking {BookingId}", bookingId);
            throw new ExternalServiceException($"Failed to get payment from Payment.API: {ex.Message}", ex);
        }
    }

    public async Task<PaginatedResponse<PaymentLookupDto>> SearchPaymentsAsync(string? query, int pageNumber, int pageSize)
    {
        var parameters = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            parameters.Add($"q={Uri.EscapeDataString(query)}");
        }

        var url = $"/api/payments/search?{string.Join("&", parameters)}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return ApiResponseJsonHelper.DeserializeApiResponse<PaginatedResponse<PaymentLookupDto>>(content, JsonOptions)
                ?? PaginatedResponse<PaymentLookupDto>.Create([], 0, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Payment.API to search payments with query {Query}", query);
            throw new ExternalServiceException($"Failed to search payments from Payment.API: {ex.Message}", ex);
        }
    }
}
