# .NET Support Strategy

## Overview

This library targets **.NET 9** and **.NET 10** exclusively. All projects multi-target `net9.0` and `net10.0`, ensuring consumers can adopt either runtime without needing separate NuGet packages.

## Target Framework Policy

### Supported Runtimes
- **.NET 9.0**: Current stable release (standard support track, 18-month lifecycle)
- **.NET 10.0**: Next major release (currently in preview/development)

### Rationale
1. **Aligned Release Cadence**: By supporting the current stable release plus the upcoming release, consumers have a smooth upgrade path without waiting for library updates.
2. **Single Package Distribution**: Multi-targeting allows a single NuGet package to serve both runtimes, reducing maintenance overhead and version fragmentation.
3. **Forward Compatibility**: Code written against .NET 9 APIs remains compatible with .NET 10, provided we avoid APIs removed or breaking-changed in the newer runtime.

### Framework Lifecycle
- When .NET 11 releases, we drop .NET 9 support and add .NET 11, maintaining the current + next pattern.
- Each TFM change requires updating all `.csproj` files, CI workflows, and conditional package references.

## Dependency Management

### Universal Dependencies
Dependencies that work across both .NET 9 and .NET 10 are declared once without conditional logic:

```xml
<ItemGroup>
  <PackageReference Include="Azure.Identity" Version="1.17.1" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
  <PackageReference Include="RestSharp" Version="113.1.0" />
</ItemGroup>
```

**Key Requirements:**
- Use stable releases compatible with both target frameworks
- Avoid preview or beta packages in production code unless coordinated across all projects
- Microsoft.Extensions.* packages typically align to the highest TFM (10.0.x works with both net9.0 and net10.0)

### Framework-Specific Dependencies
Some packages are TFM-specific (notably ASP.NET Core hosting/testing packages). These require conditional references:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.12" />
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.2" />
</ItemGroup>
```

**When to Use Conditional References:**
- ASP.NET Core packages versioned to match the runtime (e.g., Microsoft.AspNetCore.Mvc.Testing)
- Platform-specific tooling that ships per-TFM versions
- Intentional divergence needed to work around a runtime-specific issue

**Exception for ASP.NET Core in Class Libraries:**
For non-test projects that need ASP.NET Core types (e.g., `IActionResult`), use framework references instead of explicit packages:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

This eliminates version management entirely—the framework provides the correct version for each TFM.

### Automated Updates via Dependabot

Dependabot configuration (`.github/dependabot.yml`) enables automatic minor and patch updates:

```yaml
updates:
  - package-ecosystem: "nuget"
    directory: "/src"
    schedule:
      interval: "daily"
    ignore:
      - dependency-name: "*"
        update-types: ["version-update:semver-major"]
```

**Behavior:**
- ✅ **Automatic**: Patch updates (e.g., 1.17.1 → 1.17.2) and minor updates (e.g., 1.17.x → 1.18.0)
- ❌ **Manual Only**: Major updates (e.g., 1.x → 2.0) require explicit team review for breaking changes

**Review Process:**
1. Dependabot opens PRs daily for eligible updates
2. CI runs the full build/test matrix against both .NET 9 and 10
3. Auto-merge workflow handles successful patches/minors if enabled
4. Major updates stay open until manually reviewed and merged

## CI/CD Configuration

### Build Matrix
All GitHub Actions workflows install both SDK versions:

```yaml
- uses: frasermolyneux/actions/dotnet-ci@dotnet-ci/v1.1
  with:
    dotnet-version: |
      9.0.x
      10.0.x-preview
    src-folder: "src"
```

### Verification Steps
1. **Restore**: `dotnet restore` validates package references for both TFMs
2. **Build**: Each project compiles against `net9.0` and `net10.0` in parallel
3. **Test**: Unit and integration tests execute for both runtimes
4. **Pack**: NuGet packages include binaries for both frameworks

### Workflow Coverage
- **pr-validation.yml**: PR checks run the full matrix
- **main-branch-build-and-tag.yml**: Main branch builds/tests both runtimes before tagging
- **code-quality.yml**: Static analysis covers multi-targeted code

## Adding a New Target Framework

When adopting .NET 11 (or dropping .NET 9):

1. **Update all `.csproj` files:**
   ```xml
   <TargetFrameworks>net10.0;net11.0</TargetFrameworks>
   ```

2. **Update conditional dependencies:**
   - Add new `ItemGroup Condition` blocks for TFM-specific packages
   - Remove blocks for the retired TFM

3. **Update CI workflows:**
   - Modify `dotnet-version` inputs in all workflow files
   - Update any hardcoded version references in scripts

4. **Update documentation:**
   - Revise this document to reflect current + next TFMs
   - Update README.md and package descriptions

5. **Test & verify:**
   - Run full build/test suite locally
   - Monitor first CI run to catch TFM-specific issues

## Common Pitfalls

### Conditional Reference Sprawl
**Problem**: Adding conditional `ItemGroup` blocks for every package "just in case"  
**Solution**: Only conditionalize when a package truly differs per TFM. Most dependencies work universally.

### Preview Package Drift
**Problem**: Mixing stable (.NET 9) packages with preview (.NET 10) packages in the same build  
**Solution**: Use the highest stable version that both TFMs support. Only use previews when coordinated across all projects.

### Framework Reference Omissions
**Problem**: Referencing old ASP.NET Core NuGet packages (e.g., Microsoft.AspNetCore.Mvc.Core 2.x) instead of framework references  
**Solution**: Use `<FrameworkReference Include="Microsoft.AspNetCore.App" />` for .NET 9/10 projects.

### Lock File Conflicts
**Problem**: Merge conflicts in obj/ or bin/ directories  
**Solution**: Ensure `.gitignore` excludes build artifacts. Never commit lock files or intermediate outputs.

## Updating Dependencies

### Routine Updates (Patch/Minor)
1. **Automatic via Dependabot** (preferred)
2. **Manual via script**: Run `scripts/update-nuget-packages.ps1` for expedited updates
3. Verify build/test success for both TFMs before merging

### Major Updates
1. Research breaking changes in package release notes
2. Update one package at a time, running full test suite
3. Update both TFM-conditional blocks if applicable
4. Document migration steps in PR description

For detailed dependency management workflows, see [package-maintenance.md](package-maintenance.md).

## References

- [.NET Release Cadence](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [Multi-targeting in .NET](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [Dependabot configuration](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)
