# Startup configuration platform boundary implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Prevent application configuration registered by `UseAtomUI` from creating the first Dispatcher before platform initialization.

**Architecture:** `UseAtomUI` registers its delegate and hooks without invoking application code. After platform services are initialized, verify UI-thread access, invoke configuration, snapshot eligible preparation inputs, then launch background Prepare before Application construction. Keep synchronous Attach fallback for design mode, Browser/WASI and subsequent instances.

**Tech Stack:** C#, .NET 10, Avalonia 12, macOS native backend, existing AtomUI startup and Headless infrastructure.

**Spec:** User bug report in this chat; runtime contracts belong to `docs/architecture/systems/startup/lifecycle.md`.

## Global constraints

- Do not reset Dispatcher clocks, change dispatcher implementations, or compensate timer deadlines.
- Preserve Prepare overlap with Application construction and commit before `Application.Initialize()`.
- Configuration executes once per Application instance; duplicate `UseAtomUI` registration still fails immediately.
- Configuration is allowed to construct platform-backed data objects; Application and AtomUI resources remain unavailable until their normal lifecycle phases.
- New diagnostic probes belong in the system temporary directory and are deleted after validation; prefer extending the existing startup contract test.
- Run only module-focused verification; do not commit without explicit user instruction.
- The working checkout currently uses Avalonia 12.1.4; reproduce the report separately with NuGet 6.2.6 / Avalonia 12.1.3, and record both version boundaries.

## Review focus

- Configuration exceptions propagate unchanged during platform setup, rather than from the `UseAtomUI` call.
- Headless reuses an AppBuilder across isolated Application instances without reusing a consumed builder or running configuration twice.
- Ineligible background theme resolvers retain their UI-thread fallback.
- Design mode and Browser/WASI do not launch background work or wait for it.
- Original Gallery dimensions, icon content, transitions, scrolling and hover behavior stay intact during visual acceptance.

### Task 1: Move configuration across the platform boundary

**Files:**
- Modify: `src/AtomUI.Core/Startup/StartupConfiguration.cs`
- Modify: `src/AtomUI.Core/AppBuilderExtensions.cs`
- Extend: `tests/AtomUI.Core.Tests/Startup/StartupContractTests.cs`
- Modify: `tools/performances/AtomUI.StartupPerformance/Program.cs`
- Add: `tools/performances/AtomUI.StartupPerformance/StartupConfigurationAcceptance.cs` (on-demand native process acceptance)
- Temporary native probe: `/private/tmp/atomui-test-startup-callback/`

**Interfaces:**
- Consumes: Avalonia `AppBuilder.AfterPlatformServicesSetup(Action<AppBuilder>)`.
- Produces: same public `UseAtomUI` signature and the same builder/Prepare consumption contract for `StartupRun`.

- [x] Extend the existing startup test: track platform readiness before `UseAtomUI`; assert readiness inside the application configuration callback, construct an AvaloniaObject there, and retain Initialize/Ready assertions.
- [x] Run `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --filter FullyQualifiedName~StartupContractTests`; confirm failure is caused by configuration preceding platform readiness.
- [x] Remove configuration invocation from `StartupConfiguration.Register`; invoke it in the platform-ready hook after `Dispatcher.UIThread.VerifyAccess()`, then capture immutable inputs and start eligible Prepare. Keep existing fallback semantics; use names/comments that describe the new timing.
- [x] Run the same test and temporary native probe against the candidate. Verify registration invokes no callback, callback precedes Application construction, callback creates an AvaloniaObject safely, and steady timer intervals match the pure-data control.
- [x] Use temporary focused assertions for duplicate registration, original exception propagation and multiple Application instances; remove these temporary assertions after validation unless merged into the existing contract without duplicate cases.

### Task 2: Synchronize the contract and perform acceptance

**Files:**
- Modify: `docs/architecture/systems/startup/overview.md`
- Modify: `docs/architecture/systems/startup/lifecycle.md`
- Modify: `docs/architecture/systems/startup/verification.md`
- Modify: `docs/architecture/foundations/startup-and-registration.md`
- Modify: `docs/guides/startup/overview.md`
- Modify: `docs/modules/core/overview.md`
- Modify: `tools/performances/AtomUI.StartupPerformance/README.md`
- Modify: `scripts/verification/test-policy.json` (ownership of this tool only; preserve unrelated edits)

**Interfaces:**
- Consumes: Task 1's verified callback execution boundary.
- Produces: documentation that distinguishes platform availability from Application/AtomUI resource availability.

- [x] Update timing diagrams, exception timing, host differences and the Configure event description; remove the obsolete pre-platform prohibition as the explanation for ordinary object construction.
- [x] Inspect Material Gallery without changing its source: reproduce Av hover with the original layout and compare a temporary candidate deployment that changes only the AtomUI Core implementation. Record timer measurements separately from visual conclusions.
- [x] Decide test disposition: retain the extended existing contract, delete temporary probes and their dedicated artifacts/processes; no net-new global test cases by default.
- [x] Run `python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Core/Startup --path src/AtomUI.Core/AppBuilderExtensions.cs --path tests/AtomUI.Core.Tests/Startup` on the cleaned working tree, then `git diff --check`.
- [x] Review the final diff and report red/green evidence, versions, test disposition, focused results and any original Gallery or platform acceptance gaps. Leave all changes uncommitted for user review.

## Diagnostic evidence before implementation

- Local Avalonia reference: local `ReferenceProjects/Avalonia`, tag `12.1.3`. The checkout-local `.referenceprojects/Avalonia` has no source files.
- NuGet 6.2.6 / Avalonia 12.1.3 native minimal process: configuration ran during registration in both arms. With an equal 200 ms pre-platform gap and a 16 ms timer, pure data produced 146 measured intervals (median 16.30 ms); creating an AvaloniaObject produced 1 interval (1734.76 ms).
- This establishes the minimal trigger mechanism, not yet the original Material Gallery's full visual/performance A/B.
