# Wayland Native Frame Pacing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Steps use checkbox syntax for tracking.

**Goal:** Implement and measure a bounded prepared-frame adapter for the current native Wayland EGL backend.

**Architecture:** Desktop owns version-checked reflection and render-interface registration. Native owns EGL sessions, callback relay and one prepared frame per surface. Existing Avalonia targets retain native resource ownership.

**Tech Stack:** C#, Avalonia 12.1.4, NWayland 0.11.0, EGL, .NET 10 Debug / linux-x64 NativeAOT.

**Spec:** [Approved design](../specs/2026-10-28-wayland-native-frame-pacing-design.md); user instructed implementation on 2026-10-28.

## Global Constraints

- Continue in the requested `feature/motion` checkout; preserve existing investigation changes. No commit/push.
- No NavMenu/Motion sampling change in phase one. No dependency DLL patching.
- At most one prepared frame and one pending callback per surface; all native access on the Wayland worker.
- Initial validation used opt-in installation. User amendment on 2026-10-29: enable compatible Wayland EGL by default; explicit false retains the original path. Performance acceptance remains open.
- Preserve exact known reflection metadata; actual NativeAOT execution is required before any compatibility claim.
- Temporary tests/harnesses are removed after validation. Final verification is module-focused only.

## Review Focus

- An EGL failure after callback staging must not kill the protocol dispatch loop or strand readiness.
- Context/target recreation while an old callback is pending must not orphan a prepared frame.
- Configure/scale changes must invalidate prepared content and request redraw.
- Disconnect/hide/close must invalidate callbacks and release GL owners exactly once.
- Unrelated surfaces/features, initialization order and non-Wayland paths must remain unchanged.

### Task 1: Reproduction and native state machine

**Files:** temporary `/tmp/atomui-wayland-pacing-*`; `tests/AtomUI.Desktop.Controls.Tests/Temporary/WaylandFramePacing/`; new `src/AtomUI.Native/Linux/Rendering/WaylandFramePacer.cs` and `WaylandFrameCallbackRelay.cs`.

**Interfaces:** Native `WaylandFramePacer` takes callback-pending and wakeup/fault delegates; `CompleteFrame(IWaylandPreparedFrame)`, `OnFrameReady()`, `Cancel(owner)`, `Dispose()`. Prepared frames expose owner, validity, submit and invalidate operations.

- [x] Reuse original actual-presentation baseline evidence at unchanged `a1f63a29f`; establish a fresh real Gallery baseline with a temporary harness before enabling the adapter.
- [x] Temporary behavior tests: one prepared slot, pending callback defers submit, original listener before next commit, stale owner invalidation, disposal, and commit exceptions. Run exact `--filter FullyQualifiedName~WaylandFramePacing` and observe a behavior failure against the missing/old path.
- [x] Implement bounded state machine and static listener relay; verify tests pass.
- [x] Implement checked EGL swap via the active EGL interface's proc address, preserving EGLBoolean; validate failure classification without creating another connection/context.

### Task 2: Version-bound bridge and EGL session

**Files:** new `src/AtomUI.Desktop.Controls/Platform/Wayland/WaylandFramePacingCompatibility.cs`; new Native `Linux/Rendering/WaylandSurfaceBridge.cs`, `WaylandPreparedFrameSurface.cs`, `WaylandEglSubmission.cs`.

**Interfaces:** `WaylandSurfaceBridge` exposes bound readiness/snapshot/callback/registration operations. Static `IGlPlatformSurface` wrapper consumes the bridge and original surface; original target owns EnsureWindow/EglSurface/Dispose.

- [ ] Add temporary integration assertions for exact ABI rejection, configured readiness, target replacement, failed commit cleanup and stale configure discard.
- [x] Bind only known members with precise preservation; retain original protocol before-attach call and cleanup.
- [x] Implement drawing session with default framebuffer/GL_BACK initialization, flush/context restoration and delayed swap.
- [x] Test failure before staging and after staging; no callback exceptions escape. If recovery cannot be proved, keep adapter opt-in and record the exact blocking evidence.
- [x] Build Native and Desktop, then run exact temporary test filter.

### Task 3: Render-interface installation and real measurement

**Files:** new `src/AtomUI.Desktop.Controls/Platform/Wayland/WaylandFramePacingRenderInterface.cs`; modify `DesktopAppBuilderExtensions.cs`; temporary Gallery harness only.

**Interfaces:** internal `WaylandFramePacingCompatibility.TryCreate` plus decorator registration run before the original Wayland initializer. Static decorators forward all features and translate the same surfaces in readiness and creation.

- [ ] Add temporary test for non-EGL surfaces and unsupported versions remaining original, and stable surface identities.
- [x] Install via an internal diagnostic switch; initially default off, then changed to default on for compatible Wayland at explicit user request (2026-10-29). No public API or platform-selection change.
- [x] Run real Debug Gallery baseline / adapter / restored-baseline comparisons, each direction at least 20 cycles for sidebar and three expandable submenu levels.
- [ ] Record wp_presentation timestamps/discarded and actual output refresh; retain all runs. Verify rapid reversal, resize, scaling, hide/show, Popup, close and multi-window lifetimes.
- [x] Validate visual fidelity where available; report native UI automation limitations distinctly from scripted behavior.

### Task 4: AOT, review and cleanup

**Files:** affected native/desktop docs, current investigation and design status only; no release notes.

- [x] Publish and execute a focused linux-x64 NativeAOT host that exercises the adapter; run a real Gallery publish if required to prove its final integration.
- [x] Request one fresh whole-change review. Resolve important findings using failing temporary checks and focused verification.
- [x] Decide acceptance using the design criteria; do not report 60FPS if any named scene misses them. Do not silently broaden into clock changes.
- [x] Remove temporary test/harness source and their processes, retain compact evidence, update implementation/progress docs.
- [x] Run cleaned module verification with explicit Native/Desktop paths or exact existing filters; `git diff --check`. Leave changes uncommitted for review.

## Execution rulings

- Existing user instruction to begin implementation is execution authorization; proceed inline without another plan approval round.
- Existing checkout is the selected branch and contains task-owned evidence; no new worktree is needed for this continuation.
- Repository temporary-test and focused-verification rules override generic skill instructions to keep tests, run the whole suite or commit each task.

## Evidence and remaining acceptance

- Eleven temporary checks passed, including a failing EGL context-loss classification case before its fix; the real original presentation baseline failed the FPS criterion. No permanent tests are planned.
- Exact current ABI and unrelated surfaces exercised; unsupported-version rejection has source review but no alternate-assembly runtime test. Configure, owner and disconnect unit cases use controlled frame state; do not substitute them for native integration evidence.
- Real Gallery exercised post-staging failure, context loss, reversal, resize, hide/show, native and overlay Popup, two windows and modal mask. Output scale changes and real compositor disconnect/reconnect remain untested.
- Focused linux-x64 NativeAOT executable exercised actual adapter targets, resizing and hide/show. Full Gallery AOT, net8 AOT, other drivers and 120 Hz remain untested.
- Framework-rendered snapshots were inspected but contain composition/layout differences; physical flicker, clipping, hit testing and scrolling acceptance remains open because native UI automation is unavailable.
- Single fresh reviewer found no Critical/Important defect; enabled diagnostic logging was added for its minor finding. Runtime evidence gaps above are accepted only for the default-OFF experimental path.
- All-scene stable-refresh acceptance is NOT met. Keep phase one experimental and phase two unimplemented; see investigation for every retained run.

- Final cleaned module run: 497 passed / 4 failed out of 501 (Window/Dialog filters). The identical four fail in separately exported a1f63a29f source; retained as baseline defects, no unrelated fixes. Net8 Release build zero warnings/errors; final focused NativeAOT targets2/draws106/errors0. All temporary source/hosts and baseline export removed, compact evidence retained. No commit/push.

## Default policy amendment (2026-10-29)

User explicitly requested default-on Wayland adapter installation. An absent switch now behaves like true, explicit false opts out before initialization, and ABI rejection/error fallback remain unchanged. This supersedes the earlier default-OFF delivery policy and does not close mask/60/120FPS acceptance gaps. Temporary real startup probe checks default/true/false renderer installation, followed by cleanup and exact WindowingPlatformDetectionTests.
