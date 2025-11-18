# ===============================================================================
# ALTERNATIVT NUCLEAR RESET SCRIPT
# ===============================================================================

Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host "  NUCLEAR RESET v2 - Alternative Method" -ForegroundColor Cyan
Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host ""

# Sjekk at vi er i riktig mappe
if (-not (Test-Path "FirstWebApplication.csproj")) {
    Write-Host "ERROR: Kan ikke finne FirstWebApplication.csproj" -ForegroundColor Red
    Write-Host "Kjor skriptet fra FirstWebApplication-mappen!" -ForegroundColor Yellow
    Read-Host "Trykk Enter for aa avslutte"
    exit
}

Write-Host "OK: Fant prosjektfil" -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------------------------------
# STEG 1: Slett Migrations-mappen helt
# -------------------------------------------------------------------------------

Write-Host "STEG 1: Sletter Migrations-mappen..." -ForegroundColor Yellow

if (Test-Path "Migrations") {
    Remove-Item -Path "Migrations" -Recurse -Force
    Write-Host "   OK: Migrations-mappen slettet" -ForegroundColor Green
} else {
    Write-Host "   INFO: Migrations-mappen finnes ikke" -ForegroundColor Cyan
}

Write-Host ""

# -------------------------------------------------------------------------------
# STEG 2: Drop database via Docker
# -------------------------------------------------------------------------------

Write-Host "STEG 2: Dropper database via Docker..." -ForegroundColor Yellow

try {
    docker exec mariadbcontainer mysql -u root -p1234 -e "DROP DATABASE IF EXISTS ObstacleDB;" 2>&1 | Out-Null
    Write-Host "   OK: Database droppet" -ForegroundColor Green
} catch {
    Write-Host "   WARNING: Kunne ikke droppe database via Docker" -ForegroundColor Yellow
}

Write-Host ""

# -------------------------------------------------------------------------------
# STEG 3: Opprett database via Docker
# -------------------------------------------------------------------------------

Write-Host "STEG 3: Oppretter database via Docker..." -ForegroundColor Yellow

try {
    docker exec mariadbcontainer mysql -u root -p1234 -e "CREATE DATABASE ObstacleDB;" 2>&1 | Out-Null
    Write-Host "   OK: Database opprettet" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Kunne ikke opprette database" -ForegroundColor Red
    Read-Host "Trykk Enter for aa avslutte"
    exit
}

Write-Host ""

# -------------------------------------------------------------------------------
# STEG 4: Opprett ny initial migration
# -------------------------------------------------------------------------------

Write-Host "STEG 4: Oppretter ny InitialCreate migration..." -ForegroundColor Yellow

$migrationOutput = dotnet ef migrations add InitialCreate 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   OK: InitialCreate migration opprettet" -ForegroundColor Green
} else {
    Write-Host "   ERROR ved opprettelse av migration:" -ForegroundColor Red
    Write-Host "   $migrationOutput" -ForegroundColor Red
    Read-Host "Trykk Enter for aa avslutte"
    exit
}

Write-Host ""

# -------------------------------------------------------------------------------
# STEG 5: Kjor migrations
# -------------------------------------------------------------------------------

Write-Host "STEG 5: Kjorer migrations..." -ForegroundColor Yellow

$updateOutput = dotnet ef database update 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   OK: Migrations kjort" -ForegroundColor Green
} else {
    Write-Host "   ERROR ved database update:" -ForegroundColor Red
    Write-Host "   $updateOutput" -ForegroundColor Red
    Read-Host "Trykk Enter for aa avslutte"
    exit
}

Write-Host ""

# -------------------------------------------------------------------------------
# FERDIG!
# -------------------------------------------------------------------------------

Write-Host "===============================================================================" -ForegroundColor Green
Write-Host "  OK: FERDIG! Database er blank og klar!" -ForegroundColor Green
Write-Host "===============================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "NESTE STEG:" -ForegroundColor Cyan
Write-Host "1. Erstatt Program.cs med Program_FIXED_v2.cs" -ForegroundColor White
Write-Host "2. Start applikasjonen (F5)" -ForegroundColor White
Write-Host "3. Logg inn som Admin: admin@test.com / Admin123" -ForegroundColor White
Write-Host "4. Gaa til /Seed og kjor seeder" -ForegroundColor White
Write-Host ""

Read-Host "Trykk Enter for aa avslutte"
