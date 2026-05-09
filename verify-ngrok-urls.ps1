#!/usr/bin/env pwsh
# Verify that Payment.API returns ngrok URLs

Write-Host "🔍 Verifying Payment.API returns ngrok URLs..." -ForegroundColor Cyan
Write-Host ""

# Test data
$testData = @{
    amount = 100000
    orderDescription = "Verification test"
    customerEmail = "test@example.com"
    customerPhone = "0901234567"
    customerName = "Test User"
    paymentMethod = "BANK_TRANSFER"
} | ConvertTo-Json

Write-Host "📞 Calling API..." -ForegroundColor Yellow

try {
    # Call API
    $response = Invoke-RestMethod `
        -Uri "http://localhost:7252/api/payments/test/sepay/json" `
        -Method POST `
        -ContentType "application/json" `
        -Body $testData `
        -ErrorAction Stop

    Write-Host "✅ API call successful" -ForegroundColor Green
    Write-Host ""

    # Check URLs
    $checkoutUrl = $response.checkoutUrl
    $successUrl = $response.formFields.success_url
    $errorUrl = $response.formFields.error_url
    $cancelUrl = $response.formFields.cancel_url

    Write-Host "📋 Response URLs:" -ForegroundColor Cyan
    Write-Host "  Checkout: $checkoutUrl" -ForegroundColor White
    Write-Host "  Success:  $successUrl" -ForegroundColor White
    Write-Host "  Error:    $errorUrl" -ForegroundColor White
    Write-Host "  Cancel:   $cancelUrl" -ForegroundColor White
    Write-Host ""

    # Verify ngrok URLs
    $hasNgrok = $successUrl -like "*ngrok*" -and $errorUrl -like "*ngrok*" -and $cancelUrl -like "*ngrok*"
    $hasLocalhost = $successUrl -like "*localhost*" -or $errorUrl -like "*localhost*" -or $cancelUrl -like "*localhost*"
    
    # Verify correct SePay URL (pay-sandbox, not pgapi-sandbox)
    $hasCorrectSePayUrl = $checkoutUrl -like "*pay-sandbox.sepay.vn*" -or $checkoutUrl -like "*pay.sepay.vn*"
    $hasWrongSePayUrl = $checkoutUrl -like "*pgapi-sandbox.sepay.vn*" -or $checkoutUrl -like "*pgapi.sepay.vn*"

    if ($hasNgrok -and -not $hasLocalhost -and $hasCorrectSePayUrl -and -not $hasWrongSePayUrl) {
        Write-Host "✅ SUCCESS! All URLs are correct" -ForegroundColor Green
        Write-Host "✅ Callback URLs use ngrok" -ForegroundColor Green
        Write-Host "✅ Checkout URL uses pay-sandbox (not pgapi-sandbox)" -ForegroundColor Green
        Write-Host ""
        Write-Host "🎉 Payment.API is correctly configured!" -ForegroundColor Green
        Write-Host "🧪 You can now test with SePay" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "  1. Open: test-sepay-with-ngrok.html" -ForegroundColor White
        Write-Host "  2. Click any test button" -ForegroundColor White
        Write-Host "  3. Should redirect to SePay payment page (not 404)" -ForegroundColor White
        exit 0
    }
    elseif ($hasWrongSePayUrl) {
        Write-Host "❌ FAILED! Using wrong SePay URL" -ForegroundColor Red
        Write-Host ""
        Write-Host "Current checkout URL: $checkoutUrl" -ForegroundColor Yellow
        Write-Host "Expected: https://pay-sandbox.sepay.vn/v1/checkout/init" -ForegroundColor Green
        Write-Host ""
        Write-Host "Problem:" -ForegroundColor Yellow
        Write-Host "  - pgapi-sandbox is the API server (backend REST calls)" -ForegroundColor White
        Write-Host "  - pay-sandbox is the payment page (browser form POST)" -ForegroundColor White
        Write-Host "  - Forms must submit to pay-sandbox, not pgapi-sandbox" -ForegroundColor White
        Write-Host ""
        Write-Host "Solution:" -ForegroundColor Cyan
        Write-Host "  1. Check: Payment.API/Infrastructure/Configuration/SePayOptions.cs" -ForegroundColor White
        Write-Host "  2. Change: pgapi-sandbox → pay-sandbox" -ForegroundColor White
        Write-Host "  3. Restart: .\restart-payment-api.ps1" -ForegroundColor White
        Write-Host "  4. Verify: .\verify-ngrok-urls.ps1" -ForegroundColor White
        exit 1
    }
    elseif ($hasLocalhost) {
        Write-Host "❌ FAILED! Still using localhost URLs" -ForegroundColor Red
        Write-Host ""
        Write-Host "Possible causes:" -ForegroundColor Yellow
        Write-Host "  1. Payment.API not restarted after code changes" -ForegroundColor White
        Write-Host "  2. Running old compiled code" -ForegroundColor White
        Write-Host ""
        Write-Host "Solution:" -ForegroundColor Cyan
        Write-Host "  1. Stop Payment.API (Ctrl+C)" -ForegroundColor White
        Write-Host "  2. Run: .\restart-payment-api.ps1" -ForegroundColor White
        Write-Host "  3. Run this script again to verify" -ForegroundColor White
        exit 1
    }
    else {
        Write-Host "⚠️  WARNING! URLs don't match expected patterns" -ForegroundColor Yellow
        Write-Host "This is unexpected. Check the response manually." -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "❌ API call failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible causes:" -ForegroundColor Yellow
    Write-Host "  1. Payment.API is not running" -ForegroundColor White
    Write-Host "  2. Wrong port (should be 7252)" -ForegroundColor White
    Write-Host "  3. SSL certificate issue" -ForegroundColor White
    Write-Host ""
    Write-Host "Solution:" -ForegroundColor Cyan
    Write-Host "  1. Start Payment.API: .\restart-payment-api.ps1" -ForegroundColor White
    Write-Host "  2. Wait for 'Now listening on: https://localhost:7252'" -ForegroundColor White
    Write-Host "  3. Run this script again" -ForegroundColor White
    exit 1
}
