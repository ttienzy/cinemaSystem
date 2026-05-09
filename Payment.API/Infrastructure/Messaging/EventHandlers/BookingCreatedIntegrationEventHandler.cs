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
            // Get frontend base URL from configuration
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";

            // Create payment record for this booking
            var request = new CreatePaymentRequest
            {
                BookingId = @event.BookingId,
                Amount = (long)@event.TotalPrice, // Convert decimal to long (VND has no decimals)
                OrderDescription = $"Payment for booking {@event.BookingId}",
                CustomerEmail = @event.CustomerEmail,
                CustomerPhone = @event.CustomerPhone,
                CustomerName = @event.CustomerName,

                // Pass return URLs with bookingId for frontend routing
                SuccessUrl = $"{frontendUrl}/payment/success?bookingId={@event.BookingId}",
                ErrorUrl = $"{frontendUrl}/payment/error?bookingId={@event.BookingId}",
                CancelUrl = $"{frontendUrl}/payment/cancel?bookingId={@event.BookingId}"
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
}
