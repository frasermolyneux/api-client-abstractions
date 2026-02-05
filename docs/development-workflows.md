# Development Workflows

NuGet-focused repository for shared API client abstractions and web extensions. CI/CD flows below.

## Branch Strategy & Triggers

### Feature Development (feature/*, bugfix/*, hotfix/*)
- **build-and-test.yml**: Runs on push; executes dotnet-ci across net9.0/net10.0. No publishing.

### Pull Requests → main
- **pr-verify.yml**: Runs on PR open/update/reopen/ready for review; executes dotnet-ci for the solution. No NuGet packaging or tagging.

### Main Branch (on merge)
- **release-version-and-tag.yml**: Triggered by push to `main` affecting `src/**` or manual dispatch. Computes Nerdbank.GitVersioning output, reruns dotnet-ci with `BUILD_VERSION_OVERRIDE`, and creates `v<SemVer>` tag only for public releases.

### Release Publishing
- **release-publish-nuget.yml**: Triggered when the version/tag workflow succeeds and the commit carries a `v*` tag. Publishes `nuget-packages` artifact to NuGet.org (NuGet environment, `NUGET_API_KEY` secret) and creates a GitHub release with attached `.nupkg` files.

### Quality & Automation
- **codequality.yml**: Runs weekly (Mon 03:00 UTC), on push to `main`, and on PRs to `main` for SonarCloud + CodeQL using the reusable workflow.
- **dependabot-automerge.yml**: Auto-merges approved Dependabot updates when checks pass.
- **copilot-setup-steps.yml**: Documents Copilot usage guidance for contributors.

## Standard Developer Flow

```bash
dotnet build src/MX.Api.Abstractions.sln
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName~IntegrationTests"
```

## Quick Reference

| Scenario                | Workflow                     | Trigger                          | Notes                        |
| ----------------------- | ---------------------------- | -------------------------------- | ---------------------------- |
| Feature commit          | build-and-test               | Push to feature/bugfix/hotfix    | Build/test only              |
| PR validation           | pr-verify                    | PR to main                       | Build/test only              |
| Version + tag           | release-version-and-tag      | Push to main (src/**) / manual   | Tags only on public releases |
| Publish to NuGet        | release-publish-nuget        | workflow_run (version+tag)       | Requires v* tag + NuGet env  |
| Code quality & scanning | codequality                  | Schedule / push main / PR to main| SonarCloud + CodeQL checks   |
