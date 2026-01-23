# NuGet Package Maintenance

This repository targets both `net9.0` and `net10.0`. For the overall .NET support strategy and framework policy, see [dotnet-support-strategy.md](dotnet-support-strategy.md).

This page captures the automation that already exists and the non-standard manual process that can be used when Dependabot cannot unblock an update quickly enough.

## Default Path: Dependabot

- `.github/dependabot.yml` schedules daily `nuget` scans over `src/`, so each library receives upgrade PRs automatically.
- Accept Dependabot PRs whenever possible. They already run the full CI matrix, ensuring `net9.0`/`net10.0` builds remain healthy.
- If a dependency requires coordination across multiple repositories or needs additional context (e.g., SDK preview alignment), leave the manual flow below as a last resort and document the rationale in the PR.

## Non-Standard Manual Script

Only run the script when you need an expedited update outside Dependabot’s cadence (e.g., urgent security patch or coordinating a batch upgrade).

- **VS Code task (recommended):** `Terminal → Run Task… → update-nuget-packages`. This task shells into PowerShell, invokes the script with execution policy bypassed, and streams the build/test output in the integrated terminal.
- **Direct PowerShell:** `pwsh ./scripts/update-nuget-packages.ps1`

What the script does:

1. Restores the local `.config/dotnet-tools.json` manifest and installs `dotnet-outdated-tool`.
2. Executes `dotnet-outdated --upgrade` against `src/MX.Api.Abstractions.sln`, ensuring both target frameworks receive the same versions (the tool does not touch lock files; this repo relies on PackageReference flow only). Any tool failure stops the script immediately so you can resolve TFM-specific issues before proceeding.
3. Builds and tests the solution (skipping `IntegrationTests`) so regressions in either `net9.0` or `net10.0` are caught immediately.

### Optional Parameters

- `-VersionLock <None|Major|Minor>` – default `Major`; stay within the current major line for shared dependencies unless you intentionally want cross-major upgrades (`None`).
- `-IncludePrerelease` – allow preview packages (useful when the SDK is on a preview track).
- `-IncludeTransitive` – also upgrade transitive dependencies when the solution depends on them indirectly.
- `-SkipVerification` – avoid the build/test phase (only when another pipeline will run immediately). When using the VS Code task, pass extra switches by editing `.vscode/tasks.json` or running the script directly.

### Manual Command Output

The script updates project files in-place; review the diffs locally and open a single PR that summarizes the context (why Dependabot was bypassed, verification performed, etc.). Always follow up with the standard `dotnet build` / `dotnet test` tasks if you skipped verification.

## Framework-specific packages

Some dependencies publish different major versions per TFM (e.g., ASP.NET test host packages). Use conditional `ItemGroup` blocks to pin the correct line for each target framework:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
	<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.11" />
</ItemGroup>
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
	<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
</ItemGroup>
```

- Dependabot and `dotnet-outdated` evaluate each block independently, so they still surface upgrades within the respective major line.
- Keep shared dependencies (xUnit, Moq, etc.) in the unconditional `ItemGroup`; only split packages that genuinely need divergent versions.
- When onboarding a new TFM, duplicate the conditional block and set the appropriate version before running the script or Dependabot.
