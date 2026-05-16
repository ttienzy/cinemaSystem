using System.Net;
using System.Text;
using System.Text.Json;
using Cinema.Shared.Extensions;
using Cinema.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Application.DTOs.Requests;
using Payment.API.Application.DTOs.Responses;

namespace Payment.API.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .WithOpenApi();

        group.MapPost("/", CreatePayment)
            .WithName("CreatePayment")
            .WithSummary("Create payment and prepare SePay checkout")
            .Produces<PaymentCheckoutResponse>(201)
            .Produces(400)
            .Produces(500);

        group.MapGet("/{id:guid}", GetPaymentById)
            .WithName("GetPaymentById")
            .WithSummary("Get payment by ID")
            .Produces<PaymentEntity>(200)
            .Produces(404);

        group.MapGet("/booking/{bookingId:guid}", GetPaymentByBookingId)
            .WithName("GetPaymentByBookingId")
            .WithSummary("Get payment by booking ID")
            .Produces<PaymentEntity>(200)
            .Produces(404);

        group.MapGet("/search", SearchPayments)
            .WithName("SearchPayments")
            .WithSummary("Search payments by invoice number, customer email, or phone")
            .Produces<PaginatedResponse<PaymentSearchItemResponse>>(200);

        group.MapGet("/sepay/cancel", HandleSePayCancelReturn)
            .WithName("HandleSePayCancelReturn")
            .WithSummary("Handle SePay browser cancel return")
            .AllowAnonymous();

        group.MapGet("/sepay/error", HandleSePayErrorReturn)
            .WithName("HandleSePayErrorReturn")
            .WithSummary("Handle SePay browser error return")
            .AllowAnonymous();

        group.MapGet("/{id:guid}/checkout", GetCheckoutPage)
            .WithName("GetPaymentCheckoutPage")
            .WithSummary("Render auto-submitting checkout form for SePay")
            .AllowAnonymous();
    }

    private static async Task<IResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        [FromServices] IPaymentService paymentService,
        [FromServices] ISePayService sePayService,
        [FromServices] LinkGenerator linkGenerator,
        HttpContext httpContext,
        [FromServices] ILogger<Program> logger)
    {
        logger.LogInformation("Creating payment for booking {BookingId}", request.BookingId);

        var paymentResult = await paymentService.CreatePaymentAsync(request);
        if (!paymentResult.Success || paymentResult.Data == null)
        {
            return Results.BadRequest(paymentResult);
        }

        var payment = paymentResult.Data;

        try
        {
            var checkout = sePayService.BuildCheckout(payment);
            var checkoutPath = linkGenerator.GetPathByName(
                httpContext,
                "GetPaymentCheckoutPage",
                new { id = payment.Id }) ?? $"/api/payments/{payment.Id}/checkout";

            var response = new PaymentCheckoutResponse
            {
                PaymentId = payment.Id,
                BookingId = payment.BookingId,
                CheckoutUrl = checkoutPath,
                CheckoutFormAction = checkout.CheckoutFormAction,
                CheckoutFormFields = checkout.CheckoutFormFields,
                OrderInvoiceNumber = payment.OrderInvoiceNumber,
                Amount = payment.Amount,
                ExpiresAt = payment.ExpiresAt
            };

            return ApiResponse<PaymentCheckoutResponse>
                .SuccessResponse(response, PaymentException.PAYMENT_CREATED_SUCCESSFULLY, 201)
                .ToResult();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid SePay configuration while preparing checkout for payment {PaymentId}", payment.Id);
            return Results.Problem(
                title: SePayException.CHECKOUT_PREPARATION_FAILED,
                detail: ex.Message,
                statusCode: 500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error preparing SePay checkout for payment {PaymentId}", payment.Id);
            return Results.Problem(
                title: SePayException.CHECKOUT_PREPARATION_FAILED,
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetPaymentById(
        Guid id,
        [FromServices] IPaymentService paymentService)
    {
        var result = await paymentService.GetPaymentByIdAsync(id);
        return result.ToResult();
    }

    private static async Task<IResult> GetPaymentByBookingId(
        Guid bookingId,
        [FromServices] IPaymentService paymentService)
    {
        var result = await paymentService.GetPaymentByBookingIdAsync(bookingId);
        return result.ToResult();
    }

    private static async Task<IResult> SearchPayments(
        [FromQuery] string? q,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromServices] IPaymentService paymentService)
    {
        var result = await paymentService.SearchPaymentsAsync(q, pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 20 : pageSize);
        return result.ToResult();
    }

    private static Task<IResult> HandleSePayCancelReturn(
        [FromQuery] Guid bookingId,
        [FromServices] IPaymentService paymentService,
        [FromServices] IPaymentIntegrationEventPublisher eventPublisher,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<Program> logger)
    {
        return HandleFailedSePayReturnAsync(
            bookingId,
            PaymentStatus.Cancelled,
            "Payment cancelled by customer",
            "cancel",
            paymentService,
            eventPublisher,
            configuration,
            logger);
    }

    private static Task<IResult> HandleSePayErrorReturn(
        [FromQuery] Guid bookingId,
        [FromServices] IPaymentService paymentService,
        [FromServices] IPaymentIntegrationEventPublisher eventPublisher,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<Program> logger)
    {
        return HandleFailedSePayReturnAsync(
            bookingId,
            PaymentStatus.Failed,
            "Payment gateway returned error",
            "error",
            paymentService,
            eventPublisher,
            configuration,
            logger);
    }

    private static async Task<IResult> HandleFailedSePayReturnAsync(
        Guid bookingId,
        PaymentStatus failureStatus,
        string reason,
        string returnType,
        IPaymentService paymentService,
        IPaymentIntegrationEventPublisher eventPublisher,
        IConfiguration configuration,
        ILogger logger)
    {
        if (bookingId == Guid.Empty)
        {
            return Results.Redirect(BuildFrontendPaymentUrl(configuration, "error", bookingId));
        }

        var paymentResult = await paymentService.GetPaymentByBookingIdAsync(bookingId);
        if (!paymentResult.Success || paymentResult.Data is null)
        {
            logger.LogWarning(
                "SePay {ReturnType} return received for booking {BookingId}, but payment was not found.",
                returnType,
                bookingId);

            return Results.Redirect(BuildFrontendPaymentUrl(configuration, "error", bookingId));
        }

        var payment = paymentResult.Data;
        if (payment.Status == PaymentStatus.Completed)
        {
            logger.LogInformation(
                "Ignoring SePay {ReturnType} return for completed payment {PaymentId}.",
                returnType,
                payment.Id);

            return Results.Redirect(BuildFrontendPaymentUrl(configuration, "success", bookingId));
        }

        if (payment.Status is not PaymentStatus.Failed and not PaymentStatus.Cancelled)
        {
            var metadata = JsonSerializer.Serialize(new
            {
                source = "SePayBrowserReturn",
                returnType,
                receivedAtUtc = DateTime.UtcNow
            });

            var updateResult = await paymentService.UpdatePaymentStatusAsync(
                payment.Id,
                failureStatus,
                gatewayMetadata: metadata);

            if (updateResult.Success)
            {
                await eventPublisher.PublishPaymentFailedAsync(payment, reason, DateTime.UtcNow);
            }
            else
            {
                logger.LogWarning(
                    "Failed to mark payment {PaymentId} as {Status} from SePay {ReturnType} return: {Message}",
                    payment.Id,
                    failureStatus,
                    returnType,
                    updateResult.Message);
            }
        }

        return Results.Redirect(BuildFrontendPaymentUrl(
            configuration,
            failureStatus == PaymentStatus.Cancelled ? "cancel" : "error",
            bookingId));
    }

    private static string BuildFrontendPaymentUrl(
        IConfiguration configuration,
        string resultPath,
        Guid bookingId)
    {
        var frontendUrl = (configuration["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        return $"{frontendUrl}/payment/{resultPath}?bookingId={Uri.EscapeDataString(bookingId.ToString())}";
    }

    private static async Task<IResult> GetCheckoutPage(
        Guid id,
        [FromServices] IPaymentService paymentService,
        [FromServices] ISePayService sePayService)
    {
        var paymentResult = await paymentService.GetPaymentByIdAsync(id);
        if (!paymentResult.Success || paymentResult.Data == null)
        {
            return paymentResult.ToResult();
        }

        var checkout = sePayService.BuildCheckout(paymentResult.Data);
        var html = BuildCheckoutHtml(checkout.CheckoutFormAction, checkout.CheckoutFormFields);
        return Results.Content(html, "text/html", Encoding.UTF8);
    }

    private static string BuildCheckoutHtml(
        string action,
        IReadOnlyDictionary<string, string> fields)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\" />");
        builder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        builder.AppendLine("  <title>Redirecting to SePay</title>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("  <p>Redirecting to SePay...</p>");
        builder.AppendLine($"  <form id=\"sepay-checkout\" action=\"{WebUtility.HtmlEncode(action)}\" method=\"post\">");

        foreach (var field in fields)
        {
            builder.AppendLine(
                $"    <input type=\"hidden\" name=\"{WebUtility.HtmlEncode(field.Key)}\" value=\"{WebUtility.HtmlEncode(field.Value)}\" />");
        }

        builder.AppendLine("    <noscript><button type=\"submit\">Continue to SePay</button></noscript>");
        builder.AppendLine("  </form>");
        builder.AppendLine("  <script>document.getElementById('sepay-checkout').submit();</script>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }
}



