# Development Workflows

NuGet-focused repository for shared API client abstractions and web extensions. CI/CD flows below.

## Branch Strategy & Triggers

### Feature Development (feature/*, bugfix/*, hotfix/*)
- **build-and-test.yml**: Runs on push; executes dotnet-ci and the runtime integration suite across net9.0/net10.0. No publishing.

### Pull Requests → main
- **pr-verify.yml**: Runs on PR open/update/reopen/ready for review; executes dotnet-ci and the runtime integration suite for the solution. No NuGet publishing or tagging.

### Main Branch (on merge)
- **release-version-and-tag.yml**: Triggered by push to `main` affecting `src/**` or manual dispatch. Computes Nerdbank.GitVersioning output, reruns dotnet-ci with `BUILD_VERSION_OVERRIDE`, and creates `v<SemVer>` tag only for public releases.

### Release Publishing
- **release-publish-nuget.yml**: Triggered when the version/tag workflow succeeds and the commit carries a `v*` tag. Publishes that run's `nuget-packages` artifact to NuGet.org and creates a GitHub release with attached `.nupkg` files. NuGet publishing uses the `NuGet` GitHub environment, job-scoped `id-token: write`, and the required `vars.NUGET_USER` value for the pinned OIDC-only v3.0.0 publisher; no NuGet API-key secret is passed. Its `NuGet/login` step requires a GitHub Actions runner version 2.327.1 or later.

### Release Acceptance
For a real production package and symbol acceptance after a workflow-only change reaches `main`, manually dispatch **Release - Version and Tag** against the latest untagged `main` commit. Confirm the calculated Nerdbank.GitVersioning version is public and is the intended new package version before proceeding. The producer runs the normal CI, builds packages and `.snupkg` symbols, and uploads them together as `nuget-packages`; only a successful run that leaves a `v*` tag on its commit triggers the publisher, which downloads that exact run's artifact. Do not dispatch the publisher directly or dispatch the producer for a commit that already has a `v*` tag.

### Quality & Automation
- **codequality.yml**: Runs weekly (Mon 03:00 UTC), on push to `main`, and on PRs to `main` for SonarCloud + CodeQL using the reusable workflow.
- **dependabot-automerge.yml**: Auto-merges approved Dependabot updates when checks pass.
- **copilot-setup-steps.yml**: Documents Copilot usage guidance for contributors.

## Standard Developer Flow

```bash
dotnet build src/MX.Api.Abstractions.slnx
dotnet test src/MX.Api.Abstractions.slnx --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.Api.Abstractions.slnx --filter "FullyQualifiedName~IntegrationTests"
```

## Quick Reference

| Scenario                | Workflow                | Trigger                           | Notes                        |
| ----------------------- | ----------------------- | --------------------------------- | ---------------------------- |
| Feature commit          | build-and-test          | Push to feature/bugfix/hotfix     | Build/test only              |
| PR validation           | pr-verify               | PR to main                        | Build/test only              |
| Version + tag           | release-version-and-tag | Push to main (src/**) / manual    | Tags only on public releases |
| Publish to NuGet        | release-publish-nuget   | workflow_run (version+tag)        | Requires v* tag + NuGet env + OIDC username |
| Code quality & scanning | codequality             | Schedule / push main / PR to main | SonarCloud + CodeQL checks   |
