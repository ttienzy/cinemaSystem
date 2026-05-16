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
