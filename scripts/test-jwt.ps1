# Test JWT Authentication
$originalToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImQ4Yjc2MjBlLTE0ZTAtNDc5Ny1iYTljLTIwY2ViOWY0N2FlNyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNpbmVtYS5jb20iLCJqdGkiOiJiYWQyZjZiMi1mNzdmLTQ4YmMtYTAxMS1mOGJlMDUxYjA4ZTQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc3ODA4NjY1OCwiaXNzIjoiQ2luZW1hU3lzdGVtLklkZW50aXR5IiwiYXVkIjoiQ2luZW1hU3lzdGVtLkNsaWVudHMifQ.9J1ZAgF6JT3WWczRiHWz1gEgKHBr_dP4HYlE24v-mK4"

Write-Host "Testing with original token..." -ForegroundColor Yellow
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5216/test" -Headers @{"Authorization"="Bearer $originalToken"} -Method GET -UseBasicParsing
    
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Status Code: $($response.StatusCode)"
    Write-Host "Response: $($response.Content)"
} catch {
    Write-Host "FAILED!" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Error: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "Token Details:" -ForegroundColor Cyan
$parts = $originalToken.Split('.')
$payload = $parts[1]
$payload = $payload.Replace('-', '+').Replace('_', '/')
while ($payload.Length % 4 -ne 0) { $payload += '=' }
$decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
$json = $decoded | ConvertFrom-Json
Write-Host "Issuer: $($json.iss)"
Write-Host "Audience: $($json.aud)"
$expiryDate = ([DateTimeOffset]::FromUnixTimeSeconds($json.exp)).DateTime
Write-Host "Expiry: $($json.exp) ($expiryDate)"
$userId = $json.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
Write-Host "User ID: $userId"
$email = $json.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
Write-Host "Email: $email"
$role = $json.'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
Write-Host "Role: $role"
