using Microsoft.AspNetCore.Mvc;
using Payment.API.Application.DTOs.SePay;

namespace Payment.API.Api.Endpoints;

public static class SePayIpnEndpoints
{
    public static void MapSePayIpnEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/sepay/ipn", HandleIpn)
           .WithTags("SePay IPN")
           .WithName("SePayIPN")
           .WithSummary("Receive IPN from SePay")
           .AllowAnonymous();
    }

    private static async Task<IResult> HandleIpn(
        HttpRequest httpRequest,
        [FromBody] SePayIpnPayload payload,
        [FromServices] ISePayIpnProcessor processor,
        [FromServices] ILogger<Program> logger)
    {
        logger.LogInformation(
            "SePay IPN received for invoice {InvoiceNumber} with notification {NotificationType}",
            payload.Order?.OrderInvoiceNumber,
            payload.NotificationType);

        var receivedKey = httpRequest.Headers["X-Secret-Key"].FirstOrDefault();

        try
        {
            await processor.ProcessAsync(payload, receivedKey);
            return Results.Ok(new { success = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "SePay IPN rejected due to authentication failure");
            return Results.Unauthorized();
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "SePay IPN rejected due to malformed payload");
            return Results.BadRequest(new { success = false, error = ex.Message });
        }
    }
}


