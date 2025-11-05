# Nuclear Reset Script for Windows PowerShell
# This script completely resets the database and project

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Nuclear Reset Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "WARNING: This will delete ALL data!" -ForegroundColor Red
Write-Host "- All user accounts" -ForegroundColor Yellow
Write-Host "- All obstacle registrations" -ForegroundColor Yellow
Write-Host "- All database content" -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "Are you sure you want to continue? (yes/no)"

if ($confirmation -ne "yes") {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit
}

Write-Host ""
Write-Host "Step 1: Stopping Docker containers..." -ForegroundColor Green
docker-compose down -v

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error stopping Docker. Make sure Docker Desktop is running." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

Write-Host ""
Write-Host "Step 2: Starting Docker containers..." -ForegroundColor Green
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error starting Docker." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

Write-Host ""
Write-Host "Step 3: Waiting for MariaDB to start (15 seconds)..." -ForegroundColor Green
Start-Sleep -Seconds 15

Write-Host ""
Write-Host "Step 4: Navigating to project folder..." -ForegroundColor Green
Set-Location -Path "FirstWebApplication"

Write-Host ""
Write-Host "Step 5: Cleaning project..." -ForegroundColor Green
dotnet clean

Write-Host ""
Write-Host "Step 6: Removing bin folder..." -ForegroundColor Green
if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin"
}

Write-Host ""
Write-Host "Step 7: Removing obj folder..." -ForegroundColor Green
if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj"
}

Write-Host ""
Write-Host "Step 8: Restoring packages..." -ForegroundColor Green
dotnet restore

Write-Host ""
Write-Host "Step 9: Building project..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Please check errors above." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

Write-Host ""
Write-Host "Step 10: Applying database migrations..." -ForegroundColor Green
dotnet ef database update

if ($LASTEXITCODE -ne 0) {
    Write-Host "Database update failed. Please check errors above." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Reset Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test accounts created:" -ForegroundColor Green
Write-Host "  Pilot:        pilot@test.com / Pilot123" -ForegroundColor White
Write-Host "  Registerforer: registerforer@test.com / Register123" -ForegroundColor White
Write-Host "  Admin:        admin@test.com / Admin123" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run: dotnet run" -ForegroundColor White
Write-Host "2. Open browser (preferably Incognito/Private mode)" -ForegroundColor White
Write-Host "3. Navigate to your app" -ForegroundColor White
Write-Host "4. Login with one of the test accounts" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to exit"
