# Test /api/auth/me endpoint with Bearer token

param(
    [string]$Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImE1M2E0YmMzLThmYTAtNGU4ZS05MDQ4LWEyYzk5ZTZhNDhjZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6Ik5ndXllbmRpZW50aWVuMDEwNjIwMDVAZ21haWwuY29tIiwianRpIjoiYmYwNDI1ODYtOWNiMi00NzJmLThkNDUtZTk3YTRmNjJiN2MzIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ3VzdG9tZXIiLCJleHAiOjE3NzgwNjEwNjcsImlzcyI6IkNpbmVtYVN5c3RlbS5JZGVudGl0eSIsImF1ZCI6IkNpbmVtYVN5c3RlbS5DbGllbnRzIn0.xeBHHscUjfLzu5pZG1MfvEAdVOeIFXyGr364bJ1Iv28"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing /api/auth/me Endpoint" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Via Gateway (port 5200)
Write-Host "[1] Testing via Gateway (http://localhost:5200)..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5200/api/auth/me" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $Token" } `
        -ErrorAction Stop
    
    Write-Host "✅ SUCCESS via Gateway" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "❌ FAILED via Gateway" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "----------------------------------------" -ForegroundColor Gray
Write-Host ""

# Test 2: Direct to Identity.API (port 7012)
Write-Host "[2] Testing direct to Identity.API (https://localhost:7012)..." -ForegroundColor Yellow
try {
    # Skip certificate validation for localhost HTTPS
    add-type @"
        using System.Net;
        using System.Security.Cryptography.X509Certificates;
        public class TrustAllCertsPolicy : ICertificatePolicy {
            public bool CheckValidationResult(
                ServicePoint srvPoint, X509Certificate certificate,
                WebRequest request, int certificateProblem) {
                return true;
            }
        }
"@
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

    $response = Invoke-RestMethod -Uri "https://localhost:7012/api/auth/me" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $Token" } `
        -ErrorAction Stop
    
    Write-Host "✅ SUCCESS direct to Identity.API" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "❌ FAILED direct to Identity.API" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
