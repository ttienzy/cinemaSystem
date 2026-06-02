using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Payment.API.Client.Client;

public class PaymentApiClient : IPaymentApiClient
{
    private const string ApiPrefix = "api/v1";
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PaymentApiClient(HttpClient http) => _http = http;

    public Task<ApiResponse<PaymentCheckoutResponse>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<PaymentCheckoutResponse>(HttpMethod.Post, $"{ApiPrefix}/payments", request, cancellationToken);
    }

    public Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetAsync<PaymentDto>($"{ApiPrefix}/payments/{id}", cancellationToken);
    }

    public Task<ApiResponse<PaymentDto>> GetPaymentByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return GetAsync<PaymentDto>($"{ApiPrefix}/payments/booking/{bookingId}", cancellationToken);
    }

    public Task<ApiResponse<PaginatedResponse<PaymentSearchItemResponse>>> SearchPaymentsAsync(
        string? query = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var queryString = new QueryStringBuilder()
            .Add("q", query)
            .Add("pageNumber", pageNumber.ToString(CultureInfo.InvariantCulture))
            .Add("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))
            .ToString();

        return GetAsync<PaginatedResponse<PaymentSearchItemResponse>>($"{ApiPrefix}/payments/search{queryString}", cancellationToken);
    }

    private Task<ApiResponse<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        return SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
    }

    private Task<ApiResponse<T>> SendJsonAsync<T>(HttpMethod method, string requestUri, object value, CancellationToken cancellationToken)
    {
        return SendAsync<T>(new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(value)
        }, cancellationToken);
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _http.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(content))
        {
            var body = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
            if (body is not null)
            {
                return body;
            }
        }

        return ApiResponse<T>.FailureResponse($"Payment API returned {(int)response.StatusCode}.", (int)response.StatusCode);
    }

    private sealed class QueryStringBuilder
    {
        private readonly StringBuilder _builder = new();

        public QueryStringBuilder Add(string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return this;
            }

            _builder.Append(_builder.Length == 0 ? '?' : '&');
            _builder.Append(Uri.EscapeDataString(name));
            _builder.Append('=');
            _builder.Append(Uri.EscapeDataString(value));
            return this;
        }

        public override string ToString() => _builder.ToString();
    }
}
