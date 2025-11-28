# =============================================================================
# Fix Task<r> typos in the codebase
# Run this script from the repository root after pulling
# =============================================================================

$filesToFix = @(
    "src/AIBookAuthorPro.Application/Services/GuidedCreation/IBookGenerationOrchestrator.cs",
    "src/AIBookAuthorPro.Application/Services/GuidedCreation/IGuidedCreationWizardService.cs",
    "src/AIBookAuthorPro.Infrastructure/Services/GuidedCreation/ChapterGenerationPipeline.cs",
    "src/AIBookAuthorPro.Infrastructure/Services/GuidedCreation/GuidedCreationWizardService.cs",
    "src/AIBookAuthorPro.Infrastructure/Data/Repositories/IUserRepository.cs",
    "src/AIBookAuthorPro.Infrastructure/Data/Repositories/UserRepository.cs",
    "src/AIBookAuthorPro.Infrastructure/Data/Repositories/BookProjectRepository.cs",
    "src/AIBookAuthorPro.Infrastructure/Data/Repositories/ChapterRepository.cs",
    "src/AIBookAuthorPro.Infrastructure/Data/Repositories/WizardSessionRepository.cs"
)

Write-Host "Fixing Task<r> typos in codebase..." -ForegroundColor Cyan

foreach ($file in $filesToFix) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        # Replace Task<r> with Task<Result>
        $newContent = $content -replace 'Task<r>', 'Task<Result>'
        
        if ($content -ne $newContent) {
            Set-Content $file -Value $newContent -NoNewline
            Write-Host "  Fixed: $file" -ForegroundColor Green
        } else {
            Write-Host "  No changes needed: $file" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  File not found: $file" -ForegroundColor Red
    }
}

Write-Host "`nDone! Now run 'dotnet build' to verify." -ForegroundColor Cyan
