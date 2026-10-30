---
name: atomui-upgrade-dependencies
description: Use when upgrading AtomUI NuGet dependencies, source dependency projects, ReactiveUI, Avalonia, Splat, or any third-party version where compatibility must be evaluated before implementation.
---

# AtomUI Dependency Upgrade

## Core Principle

Dependency upgrades in AtomUI are evidence-first work. Establish what changed only by comparing
the dependency project's source code at the exact current and target versions. Account for the
complete changed-file inventory before narrowing inspection to code that can affect AtomUI.
Upstream tests can clarify intended behavior, but a test or its description alone does not prove a
production change. Changelogs, release notes, issues, and docs may suggest where to inspect; never
infer or report a dependency change from them without confirming it in the version-to-version source diff.
Compilation, semantic versioning, and a green existing suite do not establish behavioral compatibility.

## Hard Rules

- Do not modify dependency versions, AtomUI source, or generated files during evaluation.
- Do not implement an upgrade until the user has reviewed and approved the upgrade task plan.
- Do not commit unless the user explicitly asks for a commit.
- Upgrade one dependency family at a time unless the user explicitly approves a grouped upgrade.
- Preserve AtomUI behavior. Compatibility fixes are allowed; unrelated refactors and behavior
  improvements are not part of dependency upgrade work.
- Check the dirty worktree first and do not revert unrelated user changes.
- If planned work changes after discovery, stop and update the plan for user review before
  continuing.

## Version Sources

Check these before evaluating an upgrade:

- `build/Versions.props`
- `Directory.Packages.props`
- Relevant `*.csproj` package references
- Existing lock files, restore output, or generated props when relevant

Record the current version, requested target version, and the file that owns each version.

## Reference Source Workflow

AtomUI keeps dependency source projects under `.referenceprojects`.

1. Map the package family to its upstream source repository.
2. Ensure `.referenceprojects/<RepoName>` exists.
3. If it is missing, clone the upstream repository directly under `.referenceprojects`.
4. Run `git fetch --tags --prune` in the reference repository.
5. Resolve both the current version and target version to tags, branches, or commits.
6. If a version cannot be resolved, report the exact blocker; do not guess.
7. Inventory all changed files, then inspect the production source changes for every area that could
   reach AtomUI. Inspect companion upstream tests to interpret source-backed behavior changes:

```bash
git diff <old-ref>..<new-ref> --name-status --find-renames
git diff <old-ref>..<new-ref> -- <relevant paths>
```

For Avalonia, use `.referenceprojects/Avalonia` when present.

## Impact Trace

For each changed upstream area, record either an AtomUI impact or a source-backed reason it cannot
affect AtomUI. Use upstream test changes to clarify the intended behavior of a corresponding source
change, including one that keeps the same API shape. Trace plausible impacts through direct calls
and indirect consumers: inheritance and overrides, events, bindings and XAML, reflection, generated
code, build tasks, package dependencies, AOT/trimming, and platform-specific paths. Search for
both changed symbols and the behavior they implement; a symbol search alone can miss a change in
notification timing or lifecycle.

For each plausible impact, connect the old and new upstream behavior to the exact AtomUI consumer,
its observable contract, and a risk-matched verification scenario. Choose the observable dimensions
from the change: values, errors, state, notifications, ordering, lifecycle, rendering, input,
accessibility, threading, performance, or platform behavior. Record uncertainty and unverified
environments explicitly; do not turn an untested path into a "no impact" conclusion.

## Evaluation Checklist

Produce an evaluation before proposing implementation. Include:

- Current version and target version.
- Exact source refs compared.
- The old and new source paths or diff hunks supporting each claimed dependency change; a claim found
  only in a changelog, release note, issue, documentation page, or test is unverified.
- Disposition of the complete changed-file inventory, including the upstream tests used to interpret
  behavior and source-backed reasons for areas ruled out.
- Source diff summary:
  - public API changes
  - internal API changes AtomUI depends on
  - behavior changes
  - removed or renamed symbols
  - dependency constraint changes
  - target framework, runtime, platform, or tooling changes
- The impact trace for affected AtomUI consumers and their observable contracts, including indirect
  paths; use `rg` to locate usage but inspect the surrounding data and control flow.
- Risk level with concrete reasons.
- Required AtomUI changes, if any.
- Verification scenarios derived from each plausible impact, plus any residual unverified risk.

## Upgrade Plan Gate

After evaluation, create a complete trackable task plan and wait for user approval. The plan must
be ordered and concrete enough to execute item by item.

Use this shape:

```markdown
## Upgrade Plan: <Dependency> <current-version> -> <target-version>

- [ ] 1. Update version declarations in <files>.
- [ ] 2. Apply compatibility fix for <specific API or behavior>.
- [ ] 3. Add focused tests, probes, or Gallery scenarios for the contracts in the impact trace.
- [ ] 4. Run restore/build verification.
- [ ] 5. Run targeted verification for <specific control or feature>.
- [ ] 6. Report residual risks and pre-existing failures.
```

The user must approve the plan before implementation. Approval must be explicit, such as
"approved", "continue", "execute this plan", or equivalent wording.

## Execution Rules

Once approved:

- Execute the task list strictly from top to bottom.
- Update task status incrementally as items complete.
- Keep edits scoped to the approved plan.
- If a new incompatibility appears, pause and revise the plan for user review.
- If verification fails, distinguish upgrade-caused failures from pre-existing failures.

## Verification

Choose verification based on the affected dependency. At minimum, use:

```bash
dotnet restore
dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Debug --framework net10.0 /p:BuildInParallel=false /p:UseSharedCompilation=false
```

Add targeted tests, sample apps, Gallery scenarios, or reference-project inspections when the source
diff shows a plausible AtomUI impact. Reproduce the relevant trigger and assert the observable
contract, not only the final value or a successful build. When behavior remains uncertain, compare
old and new versions with a focused disposable probe. Follow the repository's temporary-test
lifecycle. Existing full-suite, build, package, and publish checks complement these scenarios; they
do not substitute for a missing impact-specific check. Before reporting completion, ensure the
verification evidence corresponds to the final dependency version and AtomUI working tree, and list
any platform or scenario that remains unverified.
