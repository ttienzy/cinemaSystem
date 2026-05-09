# ========================================
# Test email functionality for Booking.API
# Usage:
# .\test-email.ps1 -EmailType basic|booking|all -Email your@email.com
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("basic", "booking", "all")]
    [string]$EmailType = "basic",

    [Parameter(Mandatory=$false)]
    [string]$Email = "tester@example.com",

    [Parameter(Mandatory=$false)]
    [string]$BookingApiUrl = "https://localhost:7043"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Testing Email Functionality" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host ("Target Email: {0}" -f $Email) -ForegroundColor Yellow
Write-Host ("Email Type: {0}" -f $EmailType) -ForegroundColor Yellow
Write-Host ("API URL: {0}" -f $BookingApiUrl) -ForegroundColor Yellow
Write-Host ""

# ==============================
# Check Booking.API
# ==============================
Write-Host "Checking Booking.API..." -ForegroundColor Yellow

try {
    $healthUrl = "$BookingApiUrl/health"
    $healthCheck = Invoke-RestMethod -Uri $healthUrl -SkipCertificateCheck -TimeoutSec 5
    Write-Host "Booking.API is running" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Booking.API is not running!" -ForegroundColor Red
    Write-Host "Start it with:"
    Write-Host "cd Booking.API"
    Write-Host "dotnet run"
    exit 1
}

Write-Host ""

# ==============================
# Resolve endpoint
# ==============================
$endpoint = switch ($EmailType) {
    "basic"   { "/api/email-test/send-basic" }
    "booking" { "/api/email-test/send-booking-confirmation" }
    "all"     { "/api/email-test/send-all-types" }
}

$fullUrl = "{0}{1}?email={2}" -f $BookingApiUrl, $endpoint, $Email

Write-Host "Sending request..." -ForegroundColor Yellow
Write-Host ("Endpoint: {0}" -f $endpoint)
Write-Host ("URL: {0}" -f $fullUrl)
Write-Host ""

# ==============================
# Call API
# ==============================
try {
    $response = Invoke-RestMethod `
        -Uri $fullUrl `
        -Method POST `
        -SkipCertificateCheck `
        -TimeoutSec 10

    Write-Host "SUCCESS" -ForegroundColor Green
    Write-Host ""
    Write-Host "Response:" -ForegroundColor Cyan
    Write-Host ($response | ConvertTo-Json -Depth 10)
    Write-Host ""

    if ($response.success -eq $true) {
        Write-Host "Email sent successfully!" -ForegroundColor Green
        Write-Host ("Check inbox: {0}" -f $Email) -ForegroundColor Yellow

        if ($EmailType -eq "all" -and $response.results) {
            Write-Host ""
            Write-Host "Details:" -ForegroundColor Cyan
            foreach ($result in $response.results) {
                $status = if ($result.success) { "[OK]" } else { "[FAIL]" }
                Write-Host ("{0} {1}: {2}" -f $status, $result.type, $result.message)
            }
        }
    }
    else {
        Write-Host ("FAILED: {0}" -f $response.message) -ForegroundColor Red
    }

} catch {
    Write-Host "ERROR calling API!" -ForegroundColor Red
    Write-Host $_.Exception.Message

    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $body = $reader.ReadToEnd()
            Write-Host ("Response: {0}" -f $body)
        } catch {
            Write-Host "Cannot read error response"
        }
    }

    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "- Check SMTP config (appsettings.json)"
    Write-Host "- Verify Gmail App Password"
    Write-Host "- Check Booking.API logs"
    Write-Host ("- Test Swagger: {0}/swagger" -f $BookingApiUrl)
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Usage examples:" -ForegroundColor Magenta
Write-Host ".\test-email.ps1 -EmailType basic"
Write-Host ".\test-email.ps1 -EmailType booking"
Write-Host ".\test-email.ps1 -EmailType all"
Write-Host ".\test-email.ps1 -Email your@email.com"
Write-Host "========================================" -ForegroundColor Cyan
