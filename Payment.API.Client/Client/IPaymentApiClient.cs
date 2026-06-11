namespace Payment.API.Client.Client;

public interface IPaymentApiClient
{
    Task<ApiResponse<PaymentCheckoutResponse>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentDto>> GetPaymentByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaginatedResponse<PaymentSearchItemResponse>>> SearchPaymentsAsync(string? query = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
