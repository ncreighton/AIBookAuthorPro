# =============================================================================
# Setup Database for AI Book Author Pro
# =============================================================================

param(
    [string]$Provider = "SQLite",
    [string]$ConnectionString = ""
)

Write-Host "Setting up database for AI Book Author Pro..." -ForegroundColor Cyan

# Navigate to Infrastructure project
Push-Location "src/AIBookAuthorPro.Infrastructure"

try {
    # Install EF Core tools if not present
    Write-Host "Checking EF Core tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef 2>$null
    
    # Add initial migration
    Write-Host "Creating initial migration..." -ForegroundColor Yellow
    dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Data/Migrations
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration created successfully!" -ForegroundColor Green
        
        # Apply migration
        Write-Host "Applying migration..." -ForegroundColor Yellow
        dotnet ef database update --context AppDbContext
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Database created successfully!" -ForegroundColor Green
        } else {
            Write-Host "Failed to apply migration" -ForegroundColor Red
        }
    } else {
        Write-Host "Failed to create migration" -ForegroundColor Red
    }
}
finally {
    Pop-Location
}

Write-Host "`nSetup complete!" -ForegroundColor Cyan
