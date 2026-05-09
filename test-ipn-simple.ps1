#!/usr/bin/env pwsh
# Simple IPN endpoint test - just check if it exists

Write-Host "Checking IPN Endpoints..." -ForegroundColor Cyan
Write-Host ""

$ngrokUrl = "https://your-ngrok-url.ngrok-free.dev/api/payments/sepay/ipn"
$localUrl = "http://localhost:7252/api/payments/sepay/ipn"

# Simple test payload
$payload = '{"timestamp":1234567890,"notification_type":"order.paid","order":{"order_invoice_number":"TEST-123"}}'

Write-Host "Testing Localhost..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $localUrl -Method POST -ContentType "application/json" -Body $payload -ErrorAction Stop
    Write-Host "OK - Localhost: $($response.StatusCode)" -ForegroundColor Green
}
catch {
    if ($_.Exception.Response) {
        $status = [int]$_.Exception.Response.StatusCode
        if ($status -eq 404) {
            Write-Host "Error - Localhost: 404 Not Found - Endpoint doesn't exist" -ForegroundColor Red
        }
        elseif ($status -eq 400 -or $status -eq 401) {
            Write-Host "OK - Localhost: $status - Endpoint exists (rejected payload as expected)" -ForegroundColor Green
        }
        else {
            Write-Host "Warning - Localhost: $status - $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Error - Localhost: Could not connect to server. Is your app running?" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Testing Ngrok..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $ngrokUrl -Method POST -ContentType "application/json" -Body $payload -ErrorAction Stop
    Write-Host "OK - Ngrok: $($response.StatusCode)" -ForegroundColor Green
}
catch {
    if ($_.Exception.Response) {
        $status = [int]$_.Exception.Response.StatusCode
        if ($status -eq 404) {
            Write-Host "Error - Ngrok: 404 Not Found - Endpoint doesn't exist" -ForegroundColor Red
        }
        elseif ($status -eq 400 -or $status -eq 401) {
            Write-Host "OK - Ngrok: $status - Endpoint exists (rejected payload as expected)" -ForegroundColor Green
        }
        else {
            Write-Host "Warning - Ngrok: $status - $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Error - Ngrok: Could not connect to Ngrok. Is the tunnel open?" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Note: 400/401 responses mean endpoint exists but rejected test data (this is OK)" -ForegroundColor Cyan
