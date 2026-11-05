# Overlay Dialog mask performance investigation plan

Current status (2026-10-29): no accepted mask fix. Cache candidates were removed;
custom Avalonia builds are prohibited by the user. The remaining items below
belong to historical experiments and do not authorize implementation. Current
evidence and acceptance gaps: ../progress/2026-10-29-overlay-dialog-mask-performance-investigation.md.

> Execute inline with superpowers:executing-plans. The user has authorized continuing the existing investigation; do not commit or push.

**Goal:** Find and repair the measured cause of missed presentations during the real Gallery Overlay Dialog mask fade.

**Architecture:** Keep the existing Native frame-pacing adapter and all UX contracts. A temporary host loads current-workspace Gallery assemblies and its original AppBuilder; diagnostics observe the same Wayland connection and real submissions. Production changes follow evidence, never precede it.

**Tech Stack:** .NET 10 Debug, Avalonia 12.1.4, NWayland 0.11.0, EGL, wp_presentation.

**Spec:** ../specs/2026-10-29-overlay-dialog-background-retention-design.md (approved by the user on 2026-10-29).

Execution ruling: continue inline in the current dirty checkout as requested. No commits,
worktree replacement or full-suite runs. The user approved implementation of the design;
the following steps make that design concrete without another workflow approval round.

## Constraints and review focus

- Preserve the current dirty worktree, durations, content, background, clipping, hit testing and interactions.
- Verify loaded assembly identities and actual adapted targets, including the explicit false baseline.
- Keep static-mask diagnostics temporary; preserve the Dialog animation in every primary comparison.
- Separate UI property/layout/render, GPU submission, callback and presentation timestamps; keep long frames.
- Cover cancellation/reversal, detach, resize, multiple dialogs and resource release for any changed lifecycle.
- Temporary tests and hosts are deleted before final focused module verification. No full regression.
- NativeAOT and actual-window UX remain explicit acceptance obligations; unavailable evidence is reported as missing.

## Task 1: Reproduce and localize

- [x] Build/identify current Gallery; record source and loaded assembly hashes, actual render backend and adapted targets.
- [x] Create temporary diagnostics in /tmp, using the real Gallery Modal page and BasicDialog.
- [x] Measure adapter on/off × static/fading mask, opening and closing separately, warmup fixed, 20 repetitions per direction.
- [x] Record actual wp_presentation feedback and a separate detailed timeline; retain failing evidence.
- [x] Isolate mask rendering, measure asynchronous GPU elapsed time, and verify the background retention hypothesis. Record pixel comparison.

## Task 2: Evidence-led repair

- [x] Add temporary tests under tests/AtomUI.Desktop.Controls.Tests/Temporary/OverlayBackgroundRetention. Verify that a real animated modal presenter currently leaves ContentFrame uncached.
- [x] Add Window.OverlayBackground.cs: internal reference-counted IDisposable RetainOverlayBackground(), current-template target observation, and cleanup on owner close. Keep CacheMode declarations in WindowTheme.axaml with ClearType enabled and additional snapping disabled. Preserve user cache configuration and invalidate the effective target on mode changes for Avalonia 12.1.4.
- [x] Wire OverlayDialogPresenter acquisition for modal animated default-window hosts and release in RemoveFromDialogLayer. Cover nested sessions, IsModal/IsMotionEnabled changes, early cancellation/disposal, owner close, template replacement and user-owned cache values.
- [ ] Repeat native presentation measurements with actual server mask opacity attached to submitted/presented frames. Separate fade-active intervals from stable tails. Verify live text changes and fractional-scale resize via framebuffer readback.
- [ ] Repeat the same test and real presentation comparisons, inspect correctness and lifecycle, verify applicable AOT behavior.
- [ ] Revert only this investigation's ineffective performance changes; preserve all incoming work.

Task 2 ruling: a Window-wide selector also matched retained, detached templates.
The temporary template-reapplication test failed on that stale cache. Use the
`overlay-background-cache` class on the current template part instead of adding a
Window DirectProperty. Removing the class restores the old template while the
new part can retain independently; the theme still owns CacheMode. The corrected
temporary tests ran RED (6 failed/5 passed with retention disabled), then GREEN
(11/11), including custom local/style caches and cancellation.

## Task 3: Clean and verify

- [x] Stop diagnostic processes, delete temporary tests, hosts and dedicated binaries; preserve necessary raw evidence only.
- [x] Run exact filters or iterate with explicit changed module paths, plus git diff --check.
- [x] Update the existing investigation log with evidence, formulas, remaining acceptance gaps and test disposition. No commit/push.

Review ruling: the initial current-part class setter masked ordinary application
styles added while a modal was open. A temporary test reproduced it. Replace the
conditional setter with a normal theme-priority compiled binding to an internal
attached state on the current part. Native fractional resize also exposed
Avalonia 12.1.4 cache extent truncation; a sub-float-precision RenderAtScale guard
restores integral extents without changing layout. Repeat pixels and timing after
these corrections; keep the failed evidence.

## Acceptance ruling after broader native coverage

The BitmapCache candidate is rejected. Sparse/fractional content, transparent LCD
text and an opaque parent stamp expose rendering differences. The tiny extent
scale guard is not a general repair. Removed only this investigation's cache
wiring/helpers and its four canonical control-document additions; the preexisting
Wayland adapter and all incoming changes remain. Temporary tests reached 14/14
before removal; they establish lifecycle behavior, not native pixel equivalence.
Final cleaned Window/Dialog filter: 497 pass / 4 previously reproduced failures.

Next evidence step: compare original UI mask sampling with an isolated temporary
composition mask in the same Gallery scene. Disable per-property string logging
in both runs; continue reading actual server opacity on every submitted frame.
The host must report completion-clock differences rather than hiding them.
Diagnostic success alone is not acceptance for product Motion/clock changes.

Status before renderer experiment: no product retention or composition prototype
remains. Native mask acceptance is incomplete. The renderer-backdrop source-build experiment was later withdrawn by the user.
Earlier source test/readback evidence is retained and temporary hosts were removed.


## Task 4: Renderer-source experiment — stopped by user (2026-10-29)

- [x] Stop the temporary source-build route before host launch or retention implementation.
- [x] Delete /tmp/atomui-renderer-retention-20261029, including exported source, assemblies and the diagnostic host.
- [x] Record that no workspace dependency, installed DLL or NuGet cache was replaced.
- [x] Retain existing product changes and record the user's explicit implementation boundary.

User correction overrides earlier feasibility approval: do not compile or maintain
Avalonia packages. No custom renderer build, fork or framework assembly replacement
is authorized. Continue investigation within AtomUI and existing Avalonia packages;
reference source may be read only. Do not revive the failed BitmapCache candidate.
No commit, push or full regression.
