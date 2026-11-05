# Caption Button Wayland Compositor Transition Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move generic caption-button background interpolation from Wayland-throttled UI-thread transitions to a premultiplied-alpha compositor animation.

**Architecture:** `CaptionButton.Background` remains the selector-owned target. The generic theme projects it into an internal `CaptionButtonBackground` border whose `CompositionCustomVisual` interpolates color and brush opacity from the current compositor-side value. A theme-only duration property opts Linux/macOS into this path while Windows remains unchanged.

**Tech Stack:** C#/.NET 10, Avalonia composition animations, AtomUI ControlTheme AXAML, xUnit/Shouldly.

**Spec:** `docs/superpowers/specs/2026-09-11-caption-button-wayland-compositor-transition-design.md`

## Global Constraints

- Do not change public/protected API, Design Tokens, resource keys, pseudo-classes, or template-part names.
- Preserve `IsMotionEnabled=False` as an immediate state change.
- Preserve premultiplied-alpha interpolation and separate brush-opacity interpolation.
- Do not add timers, dispatcher delays, reflection, runtime-created controls, or long-lived subscriptions.
- Keep Windows caption-button behavior on its existing theme path.
- Do not create a commit unless the user explicitly requests one.

---

### Task 1: Prove the execution-path regression

**Files:**
- Modify: `tests/AtomUI.Desktop.Controls.Tests/Window/WindowTitleBarTokenTests.cs`

**Interfaces:**
- Consumes: existing `CaptionButtonGroup`, `CaptionButton`, and `PART_Frame` runtime template.
- Produces: regression coverage for compositor ownership, state retargeting, configured duration, and motion-disabled fallback.

- [ ] **Step 1: Replace the manual UI-thread clock test with a compositor-path test**

  Create a Linux `CaptionButtonGroup`, show it in a headless Window, locate the minimize button and `PART_Frame`, and assert that motion-enabled rendering owns a `CompositionCustomVisual` and no UI-thread transition. Assert pointer hover/press changes retain the same custom visual. Add a motion-disabled case asserting no compositor child visual is installed.

- [ ] **Step 2: Run the focused test and verify RED**

  Run:

  ```bash
  dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~WindowTitleBarTokenTests'
  ```

  Expected: FAIL because `PART_Frame` has no compositor child visual and `CaptionButton.Transitions` still owns the UI-thread animation.

### Task 2: Move generic caption backgrounds to the compositor

**Files:**
- Create: `src/AtomUI.Desktop.Controls/WindowTitleBar/CaptionButtonBackground.cs`
- Modify: `src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonTheme.axaml`

**Interfaces:**
- Consumes: `CaptionButton.Background`, `PART_Frame`, `InterpolateUtils` premultiplied-alpha semantics, and Avalonia `CompositionCustomVisual`.
- Produces: internal `CaptionButtonBackground`, compositor-side retargeting, and a template-owned duration setting.

- [ ] **Step 1: Add the internal compositor-backed background border**

  Add `CaptionButtonBackground : Border` with internal `TargetBrush` and `TransitionDuration` styled properties. It creates and attaches one `CompositionCustomVisual` only while attached, motion-enabled, and rendering a supported solid brush; detach and fallback remove the child visual symmetrically.

- [ ] **Step 2: Implement compositor brush synchronization**

  Send immutable state messages containing color, brush opacity, duration, and radius. The handler snapshots its compositor-side current value before each retarget, requests frame callbacks only while active, invalidates its content, and draws the rounded background. For zero duration, null, non-solid, or transformed brushes, use normal `Border.Background` rendering.

- [ ] **Step 3: Preserve AtomUI transparent-color interpolation**

  On each compositor frame call `InterpolateUtils.ColorInterpolate` for color and `DoubleInterpolate` for brush opacity. A new target starts from the value computed at the current `CompositionNow`, so hover/pressed reversal cannot snap to the previous UI-thread target.

- [ ] **Step 4: Remove the UI-thread transition from the generic theme**

  Replace the generic frame with `CaptionButtonBackground`, binding `TargetBrush` to `CaptionButton.Background`. Replace the generic motion-enabled `Transitions` setter with a scoped `PART_Frame.TransitionDuration` setter using `MotionDurationMid`. Leave `CaptionButton` and `WindowsCaptionButtonTheme` unchanged.

- [ ] **Step 5: Run the focused test and verify GREEN**

  Run the Task 1 command. Expected: all `WindowTitleBarTokenTests` pass.

### Task 3: Synchronize maintained documentation and validate

**Files:**
- Modify: `docs/controls/desktop/window/window-title-bar/implementation.md`
- Modify: `docs/controls/desktop/window/window-title-bar/changelog.md`
- Regenerate: `docs/AI/generated/llms/controls/window-title-bar/semantic-cn.md`
- Regenerate: `docs/AI/generated/llms/llms-semantic-cn.md`

**Interfaces:**
- Consumes: the verified implementation behavior from Task 2.
- Produces: current design documentation and generated LLMS mirrors without the discarded Fast-duration explanation.

- [ ] **Step 1: Update maintained control documentation**

  Document selector-owned target state, compositor-owned interpolation, premultiplied-alpha keyframes, motion-disabled fallback, and the Wayland scheduling reason without pinning long-term docs to an external source path or commit.

- [ ] **Step 2: Regenerate and verify LLMS output**

  Run the repository's LLMS generation and verification entry points discovered from the documentation skill.

- [ ] **Step 3: Run scoped verification**

  Run the focused WindowTitleBar tests, the directly affected Desktop Controls tests, Gallery build, and `git diff --check`. Do not run unrelated full-solution tests.

- [ ] **Step 4: Launch the exact native Wayland Gallery binary**

  Run `.agents/skills/atomui-gallery-local-debug/scripts/launch-gallery-debug.sh`, verify the printed PID resolves to the printed binary, and ask the user to inspect normal → hover → pressed → hover reversal. Do not claim visual success before that confirmation.
