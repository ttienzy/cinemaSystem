using Microsoft.AspNetCore.Mvc;
using Payment.API.Application.DTOs.Requests;
using Payment.API.Application.DTOs.Responses;
using Payment.API.Domain.Entities;
using Payment.API.Infrastructure.Integrations.SePay;

namespace Payment.API.Api.Endpoints;

/// <summary>
/// Test endpoints for SePay payment gateway integration
/// These endpoints allow testing SePay without creating real bookings
/// </summary>
public static class SePayTestEndpoints
{
    public static void MapSePayTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments/test")
            .WithTags("SePay Test")
            .WithOpenApi();

        // POST /api/payments/test/sepay/json - Create test payment and return JSON
        group.MapPost("/sepay/json", CreateTestPaymentJson)
            .WithName("CreateTestSePayPaymentJson")
            .WithSummary("Create a test payment and return JSON with checkout URL")
            .WithDescription(@"
This endpoint creates a test payment and returns JSON with checkout URL and form fields.
Use this when you want to handle the redirect manually.

**Response includes:**
- checkoutUrl: SePay form action URL
- formFields: Dictionary of fields to submit
- paymentId: Test payment ID
- invoiceNumber: Test invoice number

**Usage:**
1. Call this endpoint to get checkout URL and form fields
2. Create an HTML form with the fields
3. Submit the form to checkoutUrl (POST)
4. Browser will redirect to SePay payment page
")
            .Produces<SePayTestResponse>(200)
            .Produces<ProblemDetails>(400);

        // POST /api/payments/test/sepay - Create test payment and redirect to SePay (HTML)
        group.MapPost("/sepay", CreateTestPayment)
            .WithName("CreateTestSePayPayment")
            .WithSummary("Create a test payment and return HTML form (auto-submit)")
            .WithDescription(@"
This endpoint creates a test payment record and generates a SePay checkout form.
Returns HTML that auto-submits to SePay.

**Use /sepay/json instead if you want JSON response.**
")
            .Produces<string>(200, "text/html")
            .Produces<ProblemDetails>(400);

        // GET /api/payments/test/sepay/form - Show test form UI
        group.MapGet("/sepay/form", ShowTestForm)
            .WithName("ShowSePayTestForm")
            .WithSummary("Show interactive test form for SePay")
            .WithDescription("Displays an HTML form to easily test SePay integration with custom parameters")
            .Produces<string>(200, "text/html");
    }

    private static IResult CreateTestPaymentJson(
        [FromBody] SePayTestRequest request,
        [FromServices] ISePayService sePayService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Generate unique invoice number
            var invoiceNumber = $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100000, 999999)}";

            // Create test payment entity
            var payment = new PaymentEntity
            {
                Id = Guid.NewGuid(),
                BookingId = Guid.Empty, // Test payment - no real booking
                Amount = request.Amount,
                Currency = "VND",
                OrderDescription = request.OrderDescription,
                OrderInvoiceNumber = invoiceNumber,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CustomerName = request.CustomerName,
                PaymentMethod = request.PaymentMethod,
                PaymentGateway = "SePay",
                Status = PaymentStatus.Pending,
                SuccessUrl = request.SuccessUrl ?? "https://your-ngrok-url.ngrok-free.dev/api/payments/callback/success",
                ErrorUrl = request.ErrorUrl ?? "https://your-ngrok-url.ngrok-free.dev/api/payments/callback/error",
                CancelUrl = request.CancelUrl ?? "https://your-ngrok-url.ngrok-free.dev/api/payments/callback/cancel",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            logger.LogInformation(
                "Creating test payment (JSON): Invoice={Invoice}, Amount={Amount} VND, Method={Method}",
                invoiceNumber, request.Amount, request.PaymentMethod);

            // Build SePay checkout form
            var checkoutResult = sePayService.BuildCheckout(payment);

            logger.LogInformation(
                "Test payment created successfully (JSON): Invoice={Invoice}, PaymentId={PaymentId}",
                invoiceNumber, payment.Id);

            // Return JSON response
            var response = new SePayTestResponse
            {
                PaymentId = payment.Id,
                InvoiceNumber = invoiceNumber,
                CheckoutUrl = checkoutResult.CheckoutFormAction,
                FormFields = new Dictionary<string, string>(checkoutResult.CheckoutFormFields),
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Instructions = "Create an HTML form with these fields and submit to checkoutUrl (POST method)"
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating test payment (JSON)");
            return Results.Problem(
                title: "Test Payment Creation Failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static IResult CreateTestPayment(
        [FromBody] SePayTestRequest request,
        [FromServices] ISePayService sePayService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Generate unique invoice number
            var invoiceNumber = $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100000, 999999)}";

            // Create test payment entity
            var payment = new PaymentEntity
            {
                Id = Guid.NewGuid(),
                BookingId = Guid.Empty, // Test payment - no real booking
                Amount = request.Amount,
                Currency = "VND",
                OrderDescription = request.OrderDescription,
                OrderInvoiceNumber = invoiceNumber,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CustomerName = request.CustomerName,
                PaymentMethod = request.PaymentMethod,
                PaymentGateway = "SePay",
                Status = PaymentStatus.Pending,
                SuccessUrl = request.SuccessUrl ?? "https://your-ngrok-url.ngrok-free.dev/api/payments/callback/success",
                ErrorUrl = request.ErrorUrl ?? "https://your-ngrok-url.ngrok-free.dev/api/payments/callback/error",
                CancelUrl = request.CancelUrl ?? "https://your-ngrok-url.ngrok-free.dev/api/payments/callback/cancel",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            logger.LogInformation(
                "Creating test payment: Invoice={Invoice}, Amount={Amount} VND, Method={Method}",
                invoiceNumber, request.Amount, request.PaymentMethod);

            // Build SePay checkout form
            var checkoutResult = sePayService.BuildCheckout(payment);

            // Generate HTML form that auto-submits to SePay
            var html = GenerateAutoSubmitForm(checkoutResult);

            logger.LogInformation(
                "Test payment created successfully: Invoice={Invoice}, PaymentId={PaymentId}",
                invoiceNumber, payment.Id);

            return Results.Content(html, "text/html");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating test payment");
            return Results.Problem(
                title: "Test Payment Creation Failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static IResult ShowTestForm()
    {
        var html = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>SePay Test Form - Cinema Booking System</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .container {
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            max-width: 600px;
            width: 100%;
            padding: 40px;
        }
        h1 {
            color: #333;
            margin-bottom: 10px;
            font-size: 28px;
        }
        .subtitle {
            color: #666;
            margin-bottom: 30px;
            font-size: 14px;
        }
        .form-group {
            margin-bottom: 20px;
        }
        label {
            display: block;
            margin-bottom: 8px;
            color: #333;
            font-weight: 500;
            font-size: 14px;
        }
        input, select, textarea {
            width: 100%;
            padding: 12px;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            font-size: 14px;
            transition: border-color 0.3s;
        }
        input:focus, select:focus, textarea:focus {
            outline: none;
            border-color: #667eea;
        }
        .row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
        }
        button {
            width: 100%;
            padding: 14px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        button:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.4);
        }
        button:active {
            transform: translateY(0);
        }
        button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
        .info-box {
            background: #f0f4ff;
            border-left: 4px solid #667eea;
            padding: 15px;
            margin-bottom: 25px;
            border-radius: 4px;
        }
        .info-box h3 {
            color: #667eea;
            font-size: 14px;
            margin-bottom: 8px;
        }
        .info-box ul {
            margin-left: 20px;
            color: #555;
            font-size: 13px;
        }
        .info-box li {
            margin-bottom: 4px;
        }
        .badge {
            display: inline-block;
            padding: 4px 8px;
            background: #667eea;
            color: white;
            border-radius: 4px;
            font-size: 11px;
            font-weight: 600;
            margin-left: 8px;
        }
        #loading {
            display: none;
            text-align: center;
            padding: 20px;
        }
        .spinner {
            border: 3px solid #f3f3f3;
            border-top: 3px solid #667eea;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 0 auto 15px;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        .success-message {
            background: #d4edda;
            border: 1px solid #c3e6cb;
            color: #155724;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: none;
        }
        .error-message {
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: none;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🧪 SePay Test Form</h1>
        <p class='subtitle'>Test SePay payment gateway integration without creating real bookings</p>

        <div class='info-box'>
            <h3>📋 Test Information</h3>
            <ul>
                <li><strong>Minimum Amount:</strong> 10,000 VND (~$0.40 USD)</li>
                <li><strong>Environment:</strong> Sandbox Mode</li>
                <li><strong>Payment Methods:</strong> Bank Transfer, Card, NAPAS</li>
            </ul>
        </div>

        <div class='success-message' id='successMessage'></div>
        <div class='error-message' id='errorMessage'></div>

        <form id='testForm'>
            <div class='form-group'>
                <label>Amount (VND) <span class='badge'>Required</span></label>
                <input type='number' name='amount' value='100000' min='10000' max='100000000' required>
            </div>

            <div class='form-group'>
                <label>Order Description</label>
                <textarea name='orderDescription' rows='2'>Test payment for Cinema Booking System</textarea>
            </div>

            <div class='row'>
                <div class='form-group'>
                    <label>Customer Name</label>
                    <input type='text' name='customerName' value='Nguyen Van Test' required>
                </div>
                <div class='form-group'>
                    <label>Payment Method</label>
                    <select name='paymentMethod'>
                        <option value='BANK_TRANSFER'>Bank Transfer</option>
                        <option value='CARD'>Card</option>
                        <option value='NAPAS_BANK_TRANSFER'>NAPAS Bank Transfer</option>
                    </select>
                </div>
            </div>

            <div class='row'>
                <div class='form-group'>
                    <label>Customer Email</label>
                    <input type='email' name='customerEmail' value='test@example.com' required>
                </div>
                <div class='form-group'>
                    <label>Customer Phone</label>
                    <input type='tel' name='customerPhone' value='0123456789' required>
                </div>
            </div>

            <div class='form-group'>
                <label>Success URL (Optional)</label>
                <input type='url' name='successUrl' placeholder='http://localhost:3000/payment/success'>
            </div>

            <div class='form-group'>
                <label>Error URL (Optional)</label>
                <input type='url' name='errorUrl' placeholder='http://localhost:3000/payment/error'>
            </div>

            <div class='form-group'>
                <label>Cancel URL (Optional)</label>
                <input type='url' name='cancelUrl' placeholder='http://localhost:3000/payment/cancel'>
            </div>

            <button type='submit' id='submitBtn'>🚀 Test SePay Payment</button>
        </form>

        <div id='loading'>
            <div class='spinner'></div>
            <p>Creating payment and redirecting to SePay...</p>
        </div>
    </div>

    <script>
        document.getElementById('testForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const formData = new FormData(e.target);
            const data = {
                amount: parseInt(formData.get('amount')),
                orderDescription: formData.get('orderDescription'),
                customerName: formData.get('customerName'),
                customerEmail: formData.get('customerEmail'),
                customerPhone: formData.get('customerPhone'),
                paymentMethod: formData.get('paymentMethod'),
                successUrl: formData.get('successUrl') || null,
                errorUrl: formData.get('errorUrl') || null,
                cancelUrl: formData.get('cancelUrl') || null
            };

            // Show loading
            document.getElementById('testForm').style.display = 'none';
            document.getElementById('loading').style.display = 'block';
            document.getElementById('successMessage').style.display = 'none';
            document.getElementById('errorMessage').style.display = 'none';

            try {
                const response = await fetch('/api/payments/test/sepay/json', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    const result = await response.json();
                    
                    // Show success message
                    document.getElementById('successMessage').innerHTML = 
                        `✅ Payment created! Invoice: <strong>${result.invoiceNumber}</strong><br>Redirecting to SePay...`;
                    document.getElementById('successMessage').style.display = 'block';
                    
                    // Create form and submit to SePay
                    const form = document.createElement('form');
                    form.method = 'POST';
                    form.action = result.checkoutUrl;
                    
                    // Add all form fields
                    for (const [key, value] of Object.entries(result.formFields)) {
                        const input = document.createElement('input');
                        input.type = 'hidden';
                        input.name = key;
                        input.value = value;
                        form.appendChild(input);
                    }
                    
                    document.body.appendChild(form);
                    
                    // Submit after 1 second
                    setTimeout(() => {
                        form.submit();
                    }, 1000);
                } else {
                    const error = await response.json();
                    document.getElementById('errorMessage').innerHTML = 
                        `❌ Error: ${error.detail || error.title || 'Failed to create test payment'}`;
                    document.getElementById('errorMessage').style.display = 'block';
                    document.getElementById('testForm').style.display = 'block';
                    document.getElementById('loading').style.display = 'none';
                }
            } catch (error) {
                document.getElementById('errorMessage').innerHTML = 
                    `❌ Error: ${error.message}`;
                document.getElementById('errorMessage').style.display = 'block';
                document.getElementById('testForm').style.display = 'block';
                document.getElementById('loading').style.display = 'none';
            }
        });
    </script>
</body>
</html>";

        return Results.Content(html, "text/html");
    }

    private static string GenerateAutoSubmitForm(SePayCheckoutResult checkoutResult)
    {
        var formFields = string.Join("\n",
            checkoutResult.CheckoutFormFields.Select(f =>
                $"    <input type='hidden' name='{f.Key}' value='{System.Web.HttpUtility.HtmlEncode(f.Value)}' />"));

        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Redirecting to SePay...</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0;
        }}
        .container {{
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            padding: 60px 40px;
            text-align: center;
            max-width: 400px;
        }}
        .spinner {{
            border: 4px solid #f3f3f3;
            border-top: 4px solid #667eea;
            border-radius: 50%;
            width: 60px;
            height: 60px;
            animation: spin 1s linear infinite;
            margin: 0 auto 30px;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        h2 {{
            color: #333;
            margin-bottom: 15px;
            font-size: 24px;
        }}
        p {{
            color: #666;
            font-size: 14px;
            margin-bottom: 10px;
        }}
        .invoice {{
            background: #f0f4ff;
            padding: 10px;
            border-radius: 8px;
            color: #667eea;
            font-weight: 600;
            font-size: 13px;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='spinner'></div>
        <h2>🚀 Redirecting to SePay</h2>
        <p>Please wait while we redirect you to the payment gateway...</p>
        <p class='invoice'>Invoice: {checkoutResult.OrderInvoiceNumber}</p>
    </div>

    <form id='sePayForm' method='POST' action='{checkoutResult.CheckoutFormAction}'>
{formFields}
    </form>

    <script>
        // Auto-submit form after 1 second
        setTimeout(() => {{
            document.getElementById('sePayForm').submit();
        }}, 1000);
    </script>
</body>
</html>";
    }
}
