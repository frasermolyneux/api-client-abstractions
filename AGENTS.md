# AGENTS.md — api-client-abstractions

Shared .NET library repository for MX.Api response envelopes, client runtime abstractions, and ASP.NET API-result mapping helpers.

## Required reading (read these first)

1. `.github/copilot-instructions.md` — repo-specific orientation
2. `.github-copilot/.github/instructions/personal.working-preferences.instructions.md` — Fraser's always-on rules (git hands-off, default to `main`, `code-review` gate)
3. `.github-copilot/.github/copilot-instructions.md` — org-wide context catalog
4. Stack-specific instruction files for the work area (see Stack guardrails below)

## Org conventions via MCP (when available)

If a `frasermolyneux-copilot` MCP server is configured in your client (`~/.copilot/mcp-config.json`, VS Code user `mcp.json`, or an equivalent stdio MCP wire-up), **prefer its catalog tools** over your own assumptions when answering questions about org standards, branching, workflows, Terraform, .NET projects, Azure patterns, or shared library / platform consumption contracts. The catalog source-of-truth lives in `frasermolyneux/.github-copilot` — see `mcp-server/README.md` there for the tool contract.

This is **complementary** to the file-load model: if `./.github-copilot/` is checked out in the runner (per `copilot-setup-steps.yml`), continue to read those files directly. If both are available, prefer MCP for freshness. If no MCP server is configured in your client, treat this section as a no-op and fall back to the file paths above.

## Stack guardrails

### Enforceable standards
- `.github-copilot/.github/instructions/standards.dotnet-project.instructions.md`
- `.github-copilot/.github/instructions/standards.editorconfig.instructions.md`

### Library conventions
- `.github-copilot/.github/instructions/dotnet-nuget-library.instructions.md`
- `.github-copilot/.github/instructions/dotnet-api-client-libraries.instructions.md`
- `.github-copilot/.github/instructions/patterns.api-client.instructions.md`
- `.github-copilot/.github/instructions/patterns.nbgv-versioning.instructions.md`

### Shared contracts
- `.github-copilot/.github/instructions/shared.api-client-abstractions.instructions.md`

## Build, test, and format

```pwsh
# Build
dotnet build src/MX.Api.Abstractions.sln

# Tests (exclude integration tests to match CI defaults)
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName!~IntegrationTests"

# Run one test
dotnet test src/MX.Api.Abstractions.sln --filter "FullyQualifiedName~MyTestClass.MyTestMethod"

# Format check
dotnet format src/MX.Api.Abstractions.sln --verify-no-changes
```

## .NET completion gate (tasks first)

For .NET-related edits (`.cs`, `.csproj`, `.sln`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.vscode/tasks.json`), validate before reporting completion:

1. Prefer VS Code tasks when available:
	- `dotnet: build`
	- `dotnet: format` (must run with `--verify-no-changes`)
2. Fallback commands when tasks are unavailable:
	- `dotnet build src/MX.Api.Abstractions.sln`
	- `dotnet format src/MX.Api.Abstractions.sln --verify-no-changes`

If build or format fails, stop and report the blocker.

## Do NOT

- Do not `git commit`, `git push`, force-push, rebase, `reset --hard`, or create/delete branches.
- Do not introduce secrets, tokens, connection strings, or ad hoc credentials in code or docs.
- Do not bypass build/test/format validation gates.
- Do not pull context from sibling workspace folders; only use this repo and `./.github-copilot/`.
- Do not edit `.github/workflows/`, `version.json`, or cross-repo contracts unless explicitly requested.

## Opening the PR

You MUST use `.github/PULL_REQUEST_TEMPLATE.md` as your PR body — do **not** write a freeform body. The org template is inherited from `frasermolyneux/.github` and GitHub pre-populates it when you open the PR. Concretely:

1. Fill `## Summary` (one line) and `Closes #<issue>`.
2. Tick the relevant `## Type of change` box.
3. Paste the **actual command output** from your Build, Tests, and Format check runs into `## Validation evidence`. Show the real summary line, not "tests passed".
4. Fill `## Risk and rollout` — blast radius, auto-deploy?, manual steps post-merge, rollback plan.
5. Tick **every** box in `## Agent attestation`.
6. Delete `## Consumer impact` only if no published contract (Abstractions / Client NuGet / Service Bus DTO / Terraform output) changed.

Complete the `## Agent attestation` section before requesting review; reviewers use it as a readiness checklist.

## Pre-PR checks (run before you open the PR)

- [ ] Build succeeds.
- [ ] Tests pass (excluding integration tests unless explicitly required).
- [ ] Format check passes (`dotnet format --verify-no-changes`).
- [ ] No new secrets / GUIDs / connection strings introduced.
- [ ] Changes align with stack guardrails.
- [ ] `code-review` sub-agent run; High/Medium findings resolved or justified in the PR body.

## Escalation

If you hit any of the conditions below, **open the PR as draft** and **apply the `needs-decision` label** instead of pushing forward to ready-for-review. Post a comment on the originating issue summarising what's blocking you and what decision is needed.

This protects against the agent silently expanding scope, bypassing a contract change, or merging a half-resolved review finding.

Stop and escalate when:

- A required reading file is missing, conflicting, or ambiguous.
- The requested change expands into workflow/versioning/platform-contract edits outside scope.
- A High `code-review` finding cannot be resolved without changing scope.
- Required SDK/tooling is unavailable and setup changes would be substantial.
- Acceptance criteria conflict with instruction files or package contract boundaries.


