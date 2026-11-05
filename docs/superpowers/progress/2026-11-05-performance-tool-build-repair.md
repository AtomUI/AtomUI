# Performance tool build repair execution

Plan: [performance-tool-build-repair.md](../plans/2026-11-05-performance-tool-build-repair.md). User authorized the previously identified compile error repair; no commit.

## Root cause and red evidence

Real Debug/net10.0 build fails with 7 errors, 0 warnings: Program lacks AtomUI.Theme and AtomUI.Desktop.Controls imports (IThemeManager and the three package registration extensions); Tab verification references removed TabControlScrollViewer/TabStripScrollViewer types. Source now has one TabScrollViewer owning a template Popup and TabOverflowPopupContext, no MenuFlyout or binding disposable fields. Compiler is the minimal sufficient check; no redundant permanent compile-shape test added.

## Verification

- Import + three concrete-type corrections restore Debug/net10.0 compilation: 0 warnings, 0 errors.
- Actual TabControl entry point then fails 19 assertions: 9 obsolete MenuFlyout/reflection checks and 10 unrelated existing lazy icon/close-button checks. Input entry point also exposes old lazy/static-slot and accessory/count assertions. These were not compile failures and no product behavior is changed to satisfy them.
- Migrated the causally affected overflow verification to typed Popup/context APIs, actual host-open checks, ordinary close snapshot/action cleanup, full detach and fresh-context reopen for TabControl and TabStrip. No missing-host exception is accepted as successful opening. Removed obsolete disposable injection; preserved the shared IsMissingPopupHostException helper because ColorPicker still uses it.
- Typed overflow probes exposed a real verification-host problem: a bare headless Avalonia Window plus an unconfigured VisualLayerManager cannot create a PopupOverlayLayer. The old code swallowed the missing-host exception. The new focused realization uses the styled AtomUI Window, which supplies its real popup host, and requires Popup.IsOpen=true. No internal-property reflection or accepted host failure added.
- Temporary external probe invokes the actual compiled tool methods (ordinary close cleanup; TabControl and TabStrip detach/reopen). All three paths pass. No permanent probe/helper/CLI switch added.
- Existing exact focused tests: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~TabOverflowMenuTemplateTests|FullyQualifiedName~TabOverflowMenuReuseTests|FullyQualifiedName~TabOverflowSessionReviewTests'`: **114/114 passed**, zero failures/skips.
- Full `--verify-tabcontrol-states` still returns 1 with **10** old lazy-icon/close-button assertion mismatches. Compared its exact failure list against the minimal compiler-fix checkpoint: identical to the initial 10 non-overflow failures; all 9 obsolete overflow failures resolved. All assertions remain enabled.
- Full `--verify-input-states` executes and returns 1 with **26** old accessory/count/lazy-slot assertion mismatches. Input source is unchanged in this task. Neither full flag is reported green; these runtime contract updates are not part of fixing the compiler errors.
- Temporary external probe used reflection only to invoke the actual compiled private verification entry points; no production reflection added. Probe source/output, backup source, diff package and logs are removed. No permanent test added.

## Final review and rulings

One read-only gpt-6-astra/high review: zero actionable Critical/Important/Minor findings. Imports, typed APIs, popup hosting, detach/reopen, normal-close reuse and cleanup verified against current source and existing tests. No second review pass.

Final: Ruling: Preserve and report the ten pre-existing Tab lazy-slot assertions without product changes — unrelated to the compiler repair and reproduced at the compiler-only checkpoint.

Final: Ruling: Preserve and report the twenty-six Input runtime assertions and prior presenter cleanup — no Input source changes in this task; earlier work remains intact.

Final: Ruling: Broad timing benchmarks, product NativeAOT and unrelated suites remain outside this tool-only change — production/dependencies/registration code unchanged; real tool build and scoped behavior checks are sufficient.

All pre-existing tracked diffs compared byte-for-byte with the task-start diff and remain intact. No commit. No full regression.

## Final cleaned-tree build

`dotnet build tools/performances/AtomUI.Performance/AtomUI.Performance.csproj --framework net10.0 --no-restore --nologo` after temporary cleanup: **succeeded, 0 warnings, 0 errors**, 3.22 seconds. `git diff --check` passed. No new permanent tests or task processes; no commit.
