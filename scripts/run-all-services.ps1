# ========================================
# Cinema System - Run Microservices Only
# ========================================

Write-Host '========================================' -ForegroundColor Cyan
Write-Host 'Cinema System - Start Microservices' -ForegroundColor Cyan
Write-Host '========================================' -ForegroundColor Cyan
Write-Host ''

Write-Host 'Starting microservices...' -ForegroundColor Yellow
Write-Host 'Press Ctrl+C to stop all services' -ForegroundColor Yellow
Write-Host ''

# Function start service
function Start-ServiceWindow {
    param (
        [string]$Path,
        [string]$Name,
        [string]$Port
    )

    $fullPath = Join-Path $PWD $Path

    Start-Process powershell -ArgumentList "-NoExit -Command cd `"$fullPath`"; Write-Host '$Name - Port $Port' -ForegroundColor Green; dotnet run --launch-profile https"
    Start-Sleep -Seconds 2
}

# Start Gateway first
Start-ServiceWindow 'Gateway.API'  'Gateway.API'  '7100'

# Start services
Start-ServiceWindow 'Identity.API' 'Identity.API' '7012'
Start-ServiceWindow 'Cinema.API'   'Cinema.API'   '7251'
Start-ServiceWindow 'Movie.API'    'Movie.API'    '7295'
Start-ServiceWindow 'Booking.API'  'Booking.API'  '7043'
Start-ServiceWindow 'Payment.API'  'Payment.API'  '7252'
Start-ServiceWindow 'Notification.API' 'Notification.API' '7147'

# Summary
Write-Host ''
Write-Host '========================================' -ForegroundColor Green
Write-Host 'All services started!' -ForegroundColor Green
Write-Host '========================================' -ForegroundColor Green
Write-Host ''

Write-Host 'Service URLs:' -ForegroundColor Cyan
Write-Host '  Gateway.API:   https://localhost:7100 (API Gateway)'
Write-Host '  Identity.API:  https://localhost:7012/swagger'
Write-Host '  Cinema.API:    https://localhost:7251/swagger'
Write-Host '  Movie.API:     https://localhost:7295/swagger'
Write-Host '  Booking.API:   https://localhost:7043/swagger'
Write-Host '  Payment.API:   https://localhost:7252/swagger'
Write-Host '  Notification.API: https://localhost:7147/swagger'

Write-Host ''
Write-Host 'Close all PowerShell windows to stop services' -ForegroundColor Yellow
