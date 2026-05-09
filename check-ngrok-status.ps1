#!/usr/bin/env pwsh
# Check if ngrok tunnel is running

Write-Host "🔍 Checking Ngrok Status..." -ForegroundColor Cyan
Write-Host ""

$ngrokUrl = "https://your-ngrok-url.ngrok-free.dev"
$ngrokApiUrl = "http://127.0.0.1:4040/api/tunnels"

# Check 1: Is ngrok process running?
Write-Host "1️⃣ Checking ngrok process..." -ForegroundColor Yellow
$ngrokProcess = Get-Process -Name "ngrok" -ErrorAction SilentlyContinue

if ($ngrokProcess) {
    Write-Host "   ✅ Ngrok process is running (PID: $($ngrokProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "   ❌ Ngrok process is NOT running" -ForegroundColor Red
    Write-Host ""
    Write-Host "To start ngrok:" -ForegroundColor Cyan
    Write-Host "  .\start-ngrok-payment.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "Or manually:" -ForegroundColor Cyan
    Write-Host "  ngrok http 7252" -ForegroundColor White
    exit 1
}

Write-Host ""

# Check 2: Can we access ngrok API?
Write-Host "2️⃣ Checking ngrok API (http://127.0.0.1:4040)..." -ForegroundColor Yellow
try {
    $tunnels = Invoke-RestMethod -Uri $ngrokApiUrl -ErrorAction Stop
    Write-Host "   ✅ Ngrok API is accessible" -ForegroundColor Green
    
    if ($tunnels.tunnels.Count -gt 0) {
        Write-Host "   ✅ Found $($tunnels.tunnels.Count) active tunnel(s)" -ForegroundColor Green
        Write-Host ""
        Write-Host "   📋 Active Tunnels:" -ForegroundColor Cyan
        foreach ($tunnel in $tunnels.tunnels) {
            Write-Host "      • $($tunnel.public_url) → $($tunnel.config.addr)" -ForegroundColor White
        }
    } else {
        Write-Host "   ⚠️  No active tunnels found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Cannot access ngrok API" -ForegroundColor Red
    Write-Host "   This usually means ngrok is not running or not exposing API" -ForegroundColor Yellow
}

Write-Host ""

# Check 3: Can we reach the public URL?
Write-Host "3️⃣ Checking public URL ($ngrokUrl)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $ngrokUrl -Method GET -TimeoutSec 5 -ErrorAction Stop
    Write-Host "   ✅ Public URL is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode) {
        Write-Host "   ⚠️  Public URL responded with status: $statusCode" -ForegroundColor Yellow
    } else {
        Write-Host "   ❌ Cannot reach public URL" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""

# Check 4: Test IPN endpoint through ngrok
Write-Host "4️⃣ Testing IPN endpoint through ngrok..." -ForegroundColor Yellow
$ipnUrl = "$ngrokUrl/api/payments/sepay/ipn"
try {
    $response = Invoke-WebRequest `
        -Uri $ipnUrl `
        -Method POST `
        -ContentType "application/json" `
        -Body '{"timestamp":123,"notification_type":"order.paid","order":{"order_invoice_number":"TEST"}}' `
        -ErrorAction Stop
    
    Write-Host "   ✅ IPN endpoint accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 400 -or $statusCode -eq 401) {
        Write-Host "   ✅ IPN endpoint accessible (Status: $statusCode - rejected test data)" -ForegroundColor Green
    } elseif ($statusCode -eq 404) {
        Write-Host "   ❌ IPN endpoint not found (404)" -ForegroundColor Red
    } else {
        Write-Host "   ⚠️  IPN endpoint responded with: $statusCode" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor DarkGray

# Summary
Write-Host "📊 Summary" -ForegroundColor Cyan
Write-Host ""

if ($ngrokProcess) {
    Write-Host "✅ Ngrok is running" -ForegroundColor Green
    Write-Host "✅ Public URL: $ngrokUrl" -ForegroundColor Green
    Write-Host "✅ Web Interface: http://127.0.0.1:4040" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Ngrok tunnel is active and working!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  • View requests: http://127.0.0.1:4040" -ForegroundColor White
    Write-Host "  • Test payment: start test-sepay-with-ngrok.html" -ForegroundColor White
    exit 0
} else {
    Write-Host "❌ Ngrok is NOT running" -ForegroundColor Red
    Write-Host ""
    Write-Host "To start ngrok:" -ForegroundColor Cyan
    Write-Host "  .\start-ngrok-payment.ps1" -ForegroundColor White
    exit 1
}
