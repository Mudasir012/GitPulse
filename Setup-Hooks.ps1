<#
.SYNOPSIS
    Configures Git to use the custom hooks directory (.githooks).
.DESCRIPTION
    Sets core.hooksPath to .githooks for the current repository,
    enabling the auto-config pre-commit hook to run before every commit.
#>
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -LiteralPath $repoRoot

git config core.hooksPath .githooks

if ($LASTEXITCODE -eq 0) {
    Write-Host "Git hooks configured. .githooks/pre-commit will run before every commit." -ForegroundColor Green
} else {
    Write-Host "Failed to configure git hooks." -ForegroundColor Red
}
