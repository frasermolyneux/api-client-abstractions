[CmdletBinding()]
param(
    [string]$SolutionPath = "src/MX.Api.Abstractions.sln",
    [ValidateSet("Major", "Minor", "None")]
    [string]$VersionLock = "None",
    [switch]$IncludePrerelease,
    [switch]$IncludeTransitive,
    [switch]$SkipVerification
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$solutionFullPath = Join-Path $repoRoot $SolutionPath

if (-not (Test-Path $solutionFullPath)) {
    throw "Solution file '$SolutionPath' was not found at '$solutionFullPath'."
}

Push-Location $repoRoot
try {
    Write-Host "Restoring local dotnet tools (dotnet-outdated)" -ForegroundColor Cyan
    dotnet tool restore | Out-Null

    $outdatedArgs = @(
        "tool", "run", "dotnet-outdated", $SolutionPath,
        "--upgrade",
        "--version-lock", $VersionLock
    )

    if ($IncludeTransitive) {
        $outdatedArgs += "--include-transitive"
    }

    if ($IncludePrerelease) {
        $outdatedArgs += "--pre-release"
    }

    Write-Host "Upgrading NuGet packages while respecting multi-target frameworks (net9.0/net10.0)" -ForegroundColor Cyan
    & dotnet @outdatedArgs

    if (-not $SkipVerification) {
        Write-Host "Running dotnet build to validate all target frameworks" -ForegroundColor Cyan
        & dotnet build $SolutionPath --configuration Release

        Write-Host "Running dotnet test (excluding IntegrationTests)" -ForegroundColor Cyan
        & dotnet test $SolutionPath --configuration Release --filter "FullyQualifiedName!~IntegrationTests"
    }
}
finally {
    Pop-Location
}
