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

1. Map the package family to its source repository. Locate the local reference before any upstream
   source or release search. Check `.referenceprojects/<RepoName>` first, then follow the parent
   `ReferenceProjects` lookup order in [Reference Project Source Lookup](../../../docs/engineering/contributing/reference-project-source-guidelines.md).
   For Avalonia, the first location is `.referenceprojects/Avalonia`.
2. Use the reference repository's local Git objects for the comparison. Fetch missing refs from
   its configured upstream. If local lookup finds no repository, obtain the official source under
   `.referenceprojects/<RepoName>`.
3. Resolve the published current and target versions to exact commits. Record the reference path,
   original refs, and commit IDs. Use NuGet package metadata to verify package versions and
   dependency constraints, and use its source commit when available to corroborate the mapping.
   If either version cannot be resolved, report the exact blocker.
4. Inventory all changed files, then inspect production source changes for every area that could
   reach AtomUI. Inspect companion upstream tests to interpret source-backed behavior changes.

```bash
git diff <old-ref>..<new-ref> --name-status --find-renames
git diff <old-ref>..<new-ref> -- <relevant paths>
```

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

## Compatibility Code Review

Trace relevant upstream fixes to existing AtomUI compatibility code. Check reflection hooks and
private-member bridges, overrides, event and lifecycle workarounds, and native adaptations such as
P/Invoke and window-message hooks in `AtomUI.Native`. Use introducing commits and existing acceptance
evidence to recover the original trigger and every responsibility of the affected code. Check both
version snapshots for backported fixes. Record cleanup enabled by an older fix as an existing
opportunity with its original version evidence.

Give each affected adaptation a disposition in the upgrade evaluation and plan:

- **Retire.** Upstream covers the original responsibilities and verification of the original trigger
  supports removal. Identify implementation, callers, and trimming roots used only by the old hook.
- **Scope a candidate.** Upstream overlaps part of the behavior or equivalent behavior still needs
  verification. Identify the exact code that could be removed and its remaining responsibilities.
  Plan a focused A/B check on the target version that bypasses only the candidate. Preserve the
  original layout, input, and lifecycle conditions. Use the relevant native backend for behavior
  owned by that backend.
- **Retain or adapt.** State the remaining responsibility or separate execution path and identify
  any required signature, lifecycle, or trimming adaptation.

Record the upstream source location, AtomUI call site, original trigger, and verification status for
each disposition. Use compilation for interface compatibility and scenario evidence for behavioral
equivalence. Include proposed removals and conditional candidates in the existing plan review.

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

For Avalonia upgrades, put the old-version baseline before the version change and add the
post-upgrade checks from [Avalonia Upgrade Acceptance](#avalonia-upgrade-acceptance).

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

## Avalonia Upgrade Acceptance

Every Avalonia upgrade requires measured before-and-after evidence that AtomUI behavior remains
correct, performance meets the agreed acceptance limits, resources are released as expected, and
every control passes scripted visual acceptance with no unresolved noticeable visual differences.

### Before the upgrade

- Map every control in the repository's current control inventory, including optional control packages,
  to a scripted scenario. Reuse actual Gallery scenarios and use a temporary host for missing coverage.
  Cover at least one representative state and the key interaction for each control. Add scenarios for
  affected states and platform paths identified in the upstream source comparison. Use the
  [AtomUI visual acceptance workflow](../atomui-visual-acceptance/SKILL.md) to drive controls and
  capture results. Apply this section's cleanup after the upgrade comparison.
- On the current Avalonia version, run the scripts and capture behavior assertions, screenshots, and
  key geometry measurements. Record the platform, renderer, scale, fonts, theme, window size, data,
  and capture timing so the same scenarios can be rerun after the upgrade.
- Select performance and resource scenarios from the upstream impact trace. Record repeated samples
  for relevant measures such as operation time, frame time, and allocations. Establish baseline
  evidence for collectibility, event and timer cleanup, and native resource release after close or
  detach. Put the sampling method, allowed variation, and pass criteria in the upgrade plan.

### After the upgrade

- Rerun the same scripts on the target version. Compare behavior assertions, geometry, and screenshot
  differences per control, then review the visible differences flagged by the scripts. Investigate
  and fix noticeable differences in style, layout, clipping, states, or motion, and rerun those scenarios.
- Repeat the performance and resource measurements under the baseline conditions. Confirm the
  agreed performance criteria and verify that objects and native resources are released according to
  their contracts. Check for sustained growth across repeated open, close, reattach, or disposal
  cycles. Run platform-owned behavior on its corresponding backend.
- Remove the upgrade's temporary tests, scripts, probes, screenshots, and dedicated artifacts after
  acceptance. Keep one Markdown evidence record with both versions, control coverage counts, key
  before-and-after measurements, findings, and verification results. Run the final focused checks on
  the changed modules after cleanup.

Report the upgrade as successful only after every control passes scripted visual acceptance, the
functional, performance, and resource-release results meet the plan's criteria, and all noticeable
visual differences are resolved. Keep acceptance pending for any failed required scenario, missing
measurement, or unverified required platform.
