# .NET Support Strategy

## Target Frameworks

This library targets **.NET 9** and **.NET 10**. All projects multi-target `net9.0` and `net10.0`:

```xml
<TargetFrameworks>net9.0;net10.0</TargetFrameworks>
```

This allows a single NuGet package to serve both runtimes without separate distributions.

## Dependency Management

### Universal Dependencies

Most dependencies work across both frameworks without conditional logic:

```xml
<ItemGroup>
  <PackageReference Include="Azure.Identity" Version="1.17.1" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
  <PackageReference Include="RestSharp" Version="113.1.0" />
</ItemGroup>
```

Use stable releases compatible with both `net9.0` and `net10.0`.

### Conditional References

Only use conditional references when a package requires it (e.g., ASP.NET Core test packages that version-match the framework):

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.12" />
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.2" />
</ItemGroup>
```

For ASP.NET Core types in class libraries, use framework references instead of packages:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

## Automated Dependency Updates

Dependabot is configured to handle minor and patch updates automatically:

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

- ✅ Patch updates (e.g., 1.17.1 → 1.17.2) are automatic
- ✅ Minor updates (e.g., 1.17.x → 1.18.0) are automatic  
- ❌ Major updates (e.g., 1.x → 2.0) require manual review

## CI/CD

GitHub Actions workflows install both SDK versions and run builds/tests for both frameworks:

```yaml
- uses: frasermolyneux/actions/dotnet-ci@dotnet-ci/v1.1
  with:
    dotnet-version: |
      9.0.x
      10.0.x-preview
    src-folder: "src"
```

For detailed dependency maintenance procedures, see [package-maintenance.md](package-maintenance.md).
