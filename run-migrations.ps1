# ========================================
# Cinema System - Database Migration Script
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cinema System - Database Migration (Auto-detecting AppSettings)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Define services and their connection string keys
$services = @(
    @{ Name = "Cinema.API";   Key = "cinema-db" },
    @{ Name = "Movie.API";    Key = "movie-db" },
    @{ Name = "Booking.API";  Key = "booking-db" },
    @{ Name = "Payment.API";  Key = "DefaultConnection" }
)

# ========================================
# Function to get connection string from appsettings.json
# ========================================
function Get-ConnectionString {
    param(
        [string]$ProjectName,
        [string]$Key
    )

    $configFile = Join-Path $PWD "$ProjectName/appsettings.json"
    if (-not (Test-Path $configFile)) {
        # Try appsettings.Development.json if appsettings.json not found or missing key
        $configFile = Join-Path $PWD "$ProjectName/appsettings.Development.json"
    }

    if (-not (Test-Path $configFile)) {
        Write-Host "[WARN] No appsettings.json found in $ProjectName" -ForegroundColor Yellow
        return $null
    }

    try {
        $config = Get-Content $configFile | ConvertFrom-Json
        $connStr = $config.ConnectionStrings.$Key
        
        # If not found in primary config, try Development one specifically
        if ([string]::IsNullOrWhiteSpace($connStr)) {
            $devConfigFile = Join-Path $PWD "$ProjectName/appsettings.Development.json"
            if (Test-Path $devConfigFile) {
                $devConfig = Get-Content $devConfigFile | ConvertFrom-Json
                $connStr = $devConfig.ConnectionStrings.$Key
            }
        }
        
        return $connStr
    }
    catch {
        Write-Host "[ERROR] Failed to parse config for $ProjectName" -ForegroundColor Red
        return $null
    }
}

# ========================================
# Function to test connection (Verify server & credentials)
# ========================================
function Test-Connection {
    param([string]$ConnectionString)

    if ([string]::IsNullOrWhiteSpace($ConnectionString)) { return $false }

    try {
        # Replace the specific database with 'master' to check if server is up and credentials work
        # even if the target DB hasn't been created by EF yet.
        $testConnString = $ConnectionString -replace "Database=[^;]+", "Database=master"
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($testConnString)
        $connection.Open()
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# ========================================
# Function run migration
# ========================================
function Run-Migration {
    param(
        [string]$Name,
        [string]$ConnectionString
    )

    Write-Host ""
    Write-Host "----------------------------------------" -ForegroundColor Gray
    Write-Host "Processing $Name..." -ForegroundColor Yellow

    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        Write-Host "[FAIL] Connection string for $Name not found in appsettings.json" -ForegroundColor Red
        return $false
    }

    # Test if DB server is reachable with these credentials
    if (-not (Test-Connection $ConnectionString)) {
        Write-Host "[FAIL] Cannot connect to database for $Name" -ForegroundColor Red
        Write-Host "Using: $ConnectionString" -ForegroundColor Gray
        return $false
    }

    Write-Host "[OK] Database connection verified" -ForegroundColor Green

    $fullPath = Join-Path $PWD $Name
    
    Push-Location $fullPath
    
    # Check if migrations exist
    $migrationsPath = "Infrastructure/Persistence/Migrations"
    $hasMigrations = $false
    
    if (Test-Path $migrationsPath) {
        $migrationFiles = Get-ChildItem -Path $migrationsPath -Filter "*.cs" -File
        if ($migrationFiles.Count -gt 0) {
            $hasMigrations = $true
            Write-Host "[INFO] Found $($migrationFiles.Count) existing migration file(s)" -ForegroundColor Cyan
        }
    }
    
    # Create migration if none exist
    if (-not $hasMigrations) {
        Write-Host "[INFO] No migrations found. Creating initial migration..." -ForegroundColor Cyan
        
        $migrationName = "InitialCreate"
        Write-Host "[CMD] dotnet ef migrations add $migrationName --output-dir $migrationsPath" -ForegroundColor Gray
        
        # Capture output to display it
        $output = dotnet ef migrations add $migrationName --output-dir $migrationsPath 2>&1
        Write-Host $output -ForegroundColor Gray
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[FAIL] Failed to create migration for $Name" -ForegroundColor Red
            Pop-Location
            return $false
        }
        
        # Verify migration files were actually created
        Start-Sleep -Milliseconds 500  # Give filesystem time to sync
        $migrationFiles = Get-ChildItem -Path $migrationsPath -Filter "*.cs" -File -ErrorAction SilentlyContinue
        
        if ($migrationFiles.Count -eq 0) {
            Write-Host "[FAIL] Migration command succeeded but no files were created in $migrationsPath" -ForegroundColor Red
            Pop-Location
            return $false
        }
        
        Write-Host "[OK] Migration created successfully ($($migrationFiles.Count) files)" -ForegroundColor Green
    }
    
    # Run dotnet ef database update
    Write-Host "[INFO] Applying migrations to database..." -ForegroundColor Cyan
    Write-Host "[CMD] dotnet ef database update" -ForegroundColor Gray
    
    $output = dotnet ef database update 2>&1
    Write-Host $output -ForegroundColor Gray
    
    $exitCode = $LASTEXITCODE
    Pop-Location

    if ($exitCode -ne 0) {
        Write-Host "[FAIL] $Name migration command failed" -ForegroundColor Red
        return $false
    }

    Write-Host "[SUCCESS] $Name migration completed" -ForegroundColor Green
    return $true
}

# ========================================
# Execution
# ========================================

$success = $true

foreach ($service in $services) {
    # Access hashtable values using square brackets or .Key syntax correctly
    $serviceName = $service["Name"]
    $serviceKey = $service["Key"]
    
    $connString = Get-ConnectionString $serviceName $serviceKey
    if (-not (Run-Migration $serviceName $connString)) {
        $success = $false
    }
}

if (-not $success) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Some migrations failed. Please check errors above." -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}

# ========================================
# Done
# ========================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All migrations completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Start Services: ./run-all-services.ps1" -ForegroundColor White
Write-Host "  2. Start UI: cd Cinema.UI; npm run dev" -ForegroundColor White