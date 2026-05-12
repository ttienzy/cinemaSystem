# Test complete auth flow: Login → Get /me

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Complete Auth Flow" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Skip certificate validation
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

# Step 1: Login
Write-Host "[1] Logging in..." -ForegroundColor Yellow
$loginBody = @{
    email = "Nguyendientien01062005@gmail.com"
    password = "YourPassword123!"  # Replace with actual password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "https://localhost:7012/api/auth/login" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    Write-Host "✅ Login successful" -ForegroundColor Green
    
    if ($loginResponse.success -and $loginResponse.data.accessToken) {
        $token = $loginResponse.data.accessToken
        Write-Host "Access Token: $($token.Substring(0, 50))..." -ForegroundColor Gray
        Write-Host ""
        
        # Step 2: Test /me endpoint
        Write-Host "[2] Testing /api/auth/me with new token..." -ForegroundColor Yellow
        
        try {
            $meResponse = Invoke-RestMethod -Uri "https://localhost:7012/api/auth/me" `
                -Method GET `
                -Headers @{ "Authorization" = "Bearer $token" } `
                -ErrorAction Stop
            
            Write-Host "✅ /api/auth/me successful" -ForegroundColor Green
            Write-Host "User Info:" -ForegroundColor Green
            $meResponse | ConvertTo-Json -Depth 5
        } catch {
            Write-Host "❌ /api/auth/me failed" -ForegroundColor Red
            Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
            if ($_.Exception.Response) {
                Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "❌ Login response missing token" -ForegroundColor Red
        $loginResponse | ConvertTo-Json -Depth 5
    }
} catch {
    Write-Host "❌ Login failed" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
