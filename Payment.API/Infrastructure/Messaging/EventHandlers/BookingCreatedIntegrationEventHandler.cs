using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Payment.API.Application.DTOs.Requests;

namespace Payment.API.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Handles BookingCreatedIntegrationEvent from Booking service
/// Creates a payment record when a booking is created
/// </summary>
public class BookingCreatedIntegrationEventHandler
    : IIntegrationEventHandler<BookingCreatedIntegrationEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<BookingCreatedIntegrationEventHandler> _logger;
    private readonly IConfiguration _configuration;

    public BookingCreatedIntegrationEventHandler(
        IPaymentService paymentService,
        ILogger<BookingCreatedIntegrationEventHandler> logger,
        IConfiguration configuration)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task Handle(BookingCreatedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "Handling BookingCreatedIntegrationEvent for booking {BookingId}",
            @event.BookingId);

        try
        {
            // Get return URLs from configuration
            var frontendUrl = (_configuration["Frontend:BaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
            var paymentCallbackBaseUrl = ResolvePaymentCallbackBaseUrl(_configuration);

            // Create payment record for this booking
            var request = new CreatePaymentRequest
            {
                BookingId = @event.BookingId,
                Amount = (long)@event.TotalPrice, // Convert decimal to long (VND has no decimals)
                OrderDescription = $"Payment for booking {@event.BookingId}",
                CustomerEmail = @event.CustomerEmail,
                CustomerPhone = @event.CustomerPhone,
                CustomerName = @event.CustomerName,

                // Route failed/cancelled browser returns through Payment.API so seats are released immediately.
                SuccessUrl = $"{frontendUrl}/payment/success?bookingId={@event.BookingId}",
                ErrorUrl = $"{paymentCallbackBaseUrl}/api/payments/sepay/error?bookingId={@event.BookingId}",
                CancelUrl = $"{paymentCallbackBaseUrl}/api/payments/sepay/cancel?bookingId={@event.BookingId}"
            };

            var result = await _paymentService.CreatePaymentAsync(request);

            if (result.Success && result.Data != null)
            {
                _logger.LogInformation(
                    "Successfully created payment {PaymentId} for booking {BookingId} with invoice {InvoiceNumber}",
                    result.Data.Id,
                    @event.BookingId,
                    result.Data.OrderInvoiceNumber);
            }
            else
            {
                _logger.LogError(
                    "Failed to create payment for booking {BookingId}: {Message}",
                    @event.BookingId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling BookingCreatedIntegrationEvent for booking {BookingId}",
                @event.BookingId);

            // Re-throw to trigger retry mechanism (if configured)
            throw;
        }
    }

    private static string ResolvePaymentCallbackBaseUrl(IConfiguration configuration)
    {
        var configuredBaseUrl = configuration["Payment:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            return configuredBaseUrl.TrimEnd('/');
        }

        return "https://localhost:7252";
    }
}
