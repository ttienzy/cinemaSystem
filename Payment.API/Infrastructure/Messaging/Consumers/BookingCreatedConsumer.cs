using Cinema.Contracts.Events;
using Payment.API.Application.DTOs.Requests;
using MassTransit;

namespace Payment.API.Infrastructure.Messaging.Consumers;

public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<BookingCreatedConsumer> _logger;
    private readonly IConfiguration _configuration;

    public BookingCreatedConsumer(
        IPaymentService paymentService,
        ILogger<BookingCreatedConsumer> logger,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed BookingCreatedEvent for booking {BookingId}",
            message.BookingId);

        try
        {
            // Get return URLs from configuration
            var frontendUrl = (_configuration["Frontend:BaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
            var paymentCallbackBaseUrl = ResolvePaymentCallbackBaseUrl(_configuration);

            // Create payment record for this booking
            var request = new CreatePaymentRequest
            {
                BookingId = message.BookingId,
                Amount = (long)message.TotalPrice,
                OrderDescription = $"Payment for booking {message.BookingId}",
                CustomerEmail = message.CustomerEmail,
                CustomerPhone = message.CustomerPhone,
                CustomerName = message.CustomerName,

                // Route failed/cancelled browser returns through Payment.API so seats are released immediately.
                SuccessUrl = $"{frontendUrl}/payment/success?bookingId={message.BookingId}",
                ErrorUrl = $"{paymentCallbackBaseUrl}/api/payments/sepay/error?bookingId={message.BookingId}",
                CancelUrl = $"{paymentCallbackBaseUrl}/api/payments/sepay/cancel?bookingId={message.BookingId}"
            };

            var result = await _paymentService.CreatePaymentAsync(request);

            if (result.Success && result.Data != null)
            {
                _logger.LogInformation(
                    "Successfully created payment {PaymentId} for booking {BookingId} with invoice {InvoiceNumber}",
                    result.Data.Id,
                    message.BookingId,
                    result.Data.OrderInvoiceNumber);
            }
            else
            {
                _logger.LogError(
                    "Failed to create payment for booking {BookingId}: {Message}",
                    message.BookingId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling BookingCreatedEvent for booking {BookingId}",
                message.BookingId);

            throw; // Re-throw to trigger retry
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
