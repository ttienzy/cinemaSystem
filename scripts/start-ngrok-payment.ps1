# ========================================
# Start ngrok tunnel for Payment.API IPN
# Payment.API runs on https://localhost:7252
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Starting ngrok for Payment.API IPN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if ngrok is installed
$ngrokInstalled = Get-Command ngrok -ErrorAction SilentlyContinue

if (-not $ngrokInstalled) {
    Write-Host "ERROR: ngrok is not installed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Install ngrok using one of these methods:" -ForegroundColor Yellow
    Write-Host "  1. winget install ngrok"
    Write-Host "  2. choco install ngrok"
    Write-Host "  3. Download from https://ngrok.com/download"
    Write-Host ""
    Write-Host "After installation, run this script again." -ForegroundColor Yellow
    exit 1
}

Write-Host "ngrok is installed" -ForegroundColor Green
Write-Host ""

# Payment.API port
$paymentApiPort = 7252

Write-Host "Starting ngrok tunnel..." -ForegroundColor Yellow
Write-Host ("Local: https://localhost:{0}" -f $paymentApiPort)
Write-Host ""

Write-Host "IMPORTANT NOTES:" -ForegroundColor Yellow
Write-Host "1. Copy the ngrok URL (example: https://abc123.ngrok-free.app)"
Write-Host "2. Update appsettings.Development.json with:"
Write-Host ("   IpnUrl: https://YOUR_NGROK_URL.ngrok-free.app/api/payments/sepay/ipn")
Write-Host "3. Configure this URL in SePay Dashboard"
Write-Host "4. Keep this terminal running during testing"
Write-Host ""

Write-Host "TIP: For static domain (free tier):" -ForegroundColor Magenta
Write-Host "1. Sign up at https://ngrok.com"
Write-Host "2. Get your authtoken:"
Write-Host "   ngrok config add-authtoken YOUR_TOKEN"
Write-Host "3. Use command:"
Write-Host ("   ngrok http {0} --domain=your-name.ngrok-free.app" -f $paymentApiPort)
Write-Host ""

Write-Host "Press Ctrl+C to stop ngrok" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Start ngrok with HTTPS upstream
ngrok http https://localhost:$paymentApiPort