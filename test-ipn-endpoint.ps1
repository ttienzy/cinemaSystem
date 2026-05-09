#!/usr/bin/env pwsh
# Test SePay IPN endpoint

Write-Host "🔍 Testing SePay IPN Endpoint..." -ForegroundColor Cyan
Write-Host ""

$ngrokUrl = "https://your-ngrok-url.ngrok-free.dev"
$localUrl = "http://localhost:7252"
$ipnPath = "/api/payments/sepay/ipn"
$secretKey = "YOUR_SEPAY_SECRET_KEY"

# Test data (mock IPN payload from SePay)
$testPayload = @{
    timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    notification_type = "order.paid"
    order = @{
        id = "SEPAY-TEST-123456"
        order_invoice_number = "TEST-20260506120000-999999"
        order_amount = 100000
        currency = "VND"
        order_description = "Test IPN"
        status = "PAID"
        custom_data = @{
            webhook_test = $true
        }
    }
    transaction = @{
        id = "TXN-123456"
        amount = 100000
        currency = "VND"
        status = "SUCCESS"
        payment_method = "BANK_TRANSFER"
    }
    customer = @{
        id = "test@example.com"
        name = "Test User"
        email = "test@example.com"
    }
} | ConvertTo-Json -Depth 10

Write-Host "📋 Test Payload:" -ForegroundColor Yellow
Write-Host $testPayload -ForegroundColor Gray
Write-Host ""

# Function to test endpoint
function Test-IpnEndpoint {
    param(
        [string]$BaseUrl,
        [string]$Label
    )
    
    $url = "$BaseUrl$ipnPath"
    Write-Host "🧪 Testing $Label" -ForegroundColor Cyan
    Write-Host "   URL: $url" -ForegroundColor Gray
    
    try {
        $response = Invoke-WebRequest `
            -Uri $url `
            -Method POST `
            -ContentType "application/json" `
            -Body $testPayload `
            -Headers @{
                "X-Secret-Key" = $secretKey
            } `
            -ErrorAction Stop `
            -TimeoutSec 10
        
        if ($response.StatusCode -eq 200) {
            Write-Host "   ✅ Status: $($response.StatusCode) OK" -ForegroundColor Green
            Write-Host "   ✅ Endpoint exists and responds correctly" -ForegroundColor Green
            
            if ($response.Content) {
                Write-Host "   📄 Response:" -ForegroundColor Yellow
                Write-Host "   $($response.Content)" -ForegroundColor Gray
            }
            
            return $true
        }
        else {
            Write-Host "   ⚠️  Status: $($response.StatusCode)" -ForegroundColor Yellow
            Write-Host "   ⚠️  Unexpected status code" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        
        if ($statusCode -eq 404) {
            Write-Host "   ❌ Status: 404 Not Found" -ForegroundColor Red
            Write-Host "   ❌ Endpoint does not exist" -ForegroundColor Red
            return $false
        }
        elseif ($statusCode -eq 401) {
            Write-Host "   ⚠️  Status: 401 Unauthorized" -ForegroundColor Yellow
            Write-Host "   ✅ Endpoint exists but rejected secret key" -ForegroundColor Green
            Write-Host "   💡 This is expected if payment doesn't exist" -ForegroundColor Cyan
            return $true
        }
        elseif ($statusCode -eq 400) {
            Write-Host "   ⚠️  Status: 400 Bad Request" -ForegroundColor Yellow
            Write-Host "   ✅ Endpoint exists but rejected payload" -ForegroundColor Green
            Write-Host "   💡 This is expected if payment doesn't exist" -ForegroundColor Cyan
            return $true
        }
        else {
            Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
            
            if ($_.Exception.Response) {
                Write-Host "   Status: $statusCode" -ForegroundColor Red
            }
            
            return $false
        }
    }
    
    Write-Host ""
}

# Test localhost
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor DarkGray
$localResult = Test-IpnEndpoint -BaseUrl $localUrl -Label "Localhost"
Write-Host ""

# Test ngrok
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor DarkGray
$ngrokResult = Test-IpnEndpoint -BaseUrl $ngrokUrl -Label "Ngrok"
Write-Host ""

# Summary
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "📊 Summary" -ForegroundColor Cyan
Write-Host ""

if ($localResult) {
    Write-Host "✅ Localhost endpoint: WORKING" -ForegroundColor Green
}
else {
    Write-Host "❌ Localhost endpoint: NOT WORKING" -ForegroundColor Red
}

if ($ngrokResult) {
    Write-Host "✅ Ngrok endpoint: WORKING" -ForegroundColor Green
}
else {
    Write-Host "❌ Ngrok endpoint: NOT WORKING" -ForegroundColor Red
}

Write-Host ""

if ($localResult -and $ngrokResult) {
    Write-Host "🎉 SUCCESS! Both endpoints are working!" -ForegroundColor Green
    Write-Host ""
    Write-Host "SePay can reach your IPN endpoint at:" -ForegroundColor Cyan
    Write-Host "$ngrokUrl$ipnPath" -ForegroundColor White
    Write-Host ""
    Write-Host "✅ Ready for production testing!" -ForegroundColor Green
    exit 0
}
elseif ($localResult -and -not $ngrokResult) {
    Write-Host "⚠️  Localhost works but ngrok doesn't" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Possible causes:" -ForegroundColor Yellow
    Write-Host "  1. Ngrok is not running" -ForegroundColor White
    Write-Host "  2. Ngrok URL has changed" -ForegroundColor White
    Write-Host "  3. Ngrok tunnel expired" -ForegroundColor White
    Write-Host ""
    Write-Host "Solution:" -ForegroundColor Cyan
    Write-Host "  1. Check ngrok status: curl $ngrokUrl" -ForegroundColor White
    Write-Host "  2. Restart ngrok: .\start-ngrok-payment.ps1" -ForegroundColor White
    Write-Host "  3. Update ngrok URL in code if changed" -ForegroundColor White
    exit 1
}
elseif (-not $localResult) {
    Write-Host "❌ Localhost endpoint not working" -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible causes:" -ForegroundColor Yellow
    Write-Host "  1. Payment.API is not running" -ForegroundColor White
    Write-Host "  2. Running on wrong port (should be 7252)" -ForegroundColor White
    Write-Host "  3. Endpoint not registered" -ForegroundColor White
    Write-Host ""
    Write-Host "Solution:" -ForegroundColor Cyan
    Write-Host "  1. Start Payment.API: .\restart-payment-api.ps1" -ForegroundColor White
    Write-Host "  2. Check Swagger: https://localhost:7252/swagger" -ForegroundColor White
    Write-Host "  3. Look for 'SePay IPN' tag in Swagger" -ForegroundColor White
    exit 1
}
else {
    Write-Host "❌ Both endpoints not working" -ForegroundColor Red
    Write-Host ""
    Write-Host "Solution:" -ForegroundColor Cyan
    Write-Host "  1. Start Payment.API: .\restart-payment-api.ps1" -ForegroundColor White
    Write-Host "  2. Start ngrok: .\start-ngrok-payment.ps1" -ForegroundColor White
    Write-Host "  3. Run this script again" -ForegroundColor White
    exit 1
}
