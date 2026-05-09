# Test Token Validation
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImQ4Yjc2MjBlLTE0ZTAtNDc5Ny1iYTljLTIwY2ViOWY0N2FlNyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNpbmVtYS5jb20iLCJqdGkiOiJlMjQyYjE5Mi0xMTM2LTRmZDAtODMyYi1hZTg2ODE2ZTdhMzAiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc3ODA4OTI5OSwiaXNzIjoiQ2luZW1hU3lzdGVtLklkZW50aXR5IiwiYXVkIjoiQ2luZW1hU3lzdGVtLkNsaWVudHMifQ.jnnWhS5OOpSTcJ4CiE-q4U1tzvHpYKJLBfo5SccxO1k"

Write-Host "Testing token validation..." -ForegroundColor Yellow
Write-Host ""

# Test with correct Authorization header format
Write-Host "Test 1: With 'Bearer ' prefix" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5216/test" `
        -Headers @{"Authorization"="Bearer $token"} `
        -Method GET `
        -UseBasicParsing
    Write-Host "SUCCESS! Response: $($response.Content)" -ForegroundColor Green
} catch {
    Write-Host "FAILED! Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $responseBody = $reader.ReadToEnd()
    if ($responseBody) {
        Write-Host "Response Body: $responseBody"
    }
}

Write-Host ""

# Test without Bearer prefix
Write-Host "Test 2: Without 'Bearer ' prefix" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5216/test" `
        -Headers @{"Authorization"=$token} `
        -Method GET `
        -UseBasicParsing
    Write-Host "SUCCESS! Response: $($response.Content)" -ForegroundColor Green
} catch {
    Write-Host "FAILED! Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

Write-Host ""

# Test the /api/auth/test endpoint
Write-Host "Test 3: /api/auth/test endpoint" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5216/api/auth/test" `
        -Headers @{"Authorization"="Bearer $token"} `
        -Method GET `
        -UseBasicParsing
    Write-Host "SUCCESS! Response: $($response.Content)" -ForegroundColor Green
} catch {
    Write-Host "FAILED! Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}
