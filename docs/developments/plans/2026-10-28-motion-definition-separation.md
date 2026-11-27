# Motion Definition Separation Implementation Plan

**Goal:** Implement the user-approved AbstractMotion/Motion separation on existing feature/motion worktree, baseline2de819b6a.
**Spec:** ../specs/2026-10-28-motion-definition-separation-design.md
**Execution:** Inline; one fresh final reviewer. Explicit user direction to start overrides another design/plan approval round. No new branch/worktree, commit, push or full regression.

## Files and interfaces

AbstractMotion.cs owns instance definition/preparation. Motion.cs owns unchanged attached-property registration and static target operations; Motion.Visibility.cs is merged. BuiltIn, MotionBuilder and Playback accept AbstractMotion definitions. Badge, Popup/Flyout/ContextMenu, Drawer, TreeView and Dialog carry that definition type; static Motion calls stay unchanged. Tests and current API docs follow the same division. No AXAML product edits are expected.

## Task1 — Characterize and verify the new extension boundary

- [x] Run existing Core animation/initialization/retention6 cases before edits.
- [x] Add temporary Core consumer with a real compiled AXAML using production https://atomui.net Motion.Enter/Leave/IsVisible and custom definition; cover initially true and false, binding-driven roundtrip, replacement and detach.
- [x] Run the new boundary contract before implementation; require a valid failure for the unsplit model, then the same contract green after separation. Do not treat an incidental harness error as RED.

## Task2 — Separate responsibilities and synchronize all callers

- [x] Add AbstractMotion instance definition; retain sealed Motion attached owner/private constructor, static callbacks and target operations. Preserve registration order/defaults/visibility lifecycle.
- [x] Migrate all actual definition types and inheritance, including composition, runtime, consumer APIs and existing test subclasses. Do not rename namespace Motion or static Motion references.
- [x] Update current architecture/control docs and examples; mark historical stage evidence accurately rather than recertifying it by a mechanical rename.
- [x] Verify the temporary consumer and relevant existing Core/desktop/Gallery contracts; remove temporary tests and dedicated resources; verify the cleaned inputs.

## Task3 — Final verification and review

- [x] Production net8 Desktop.Controls build; Gallery osx-arm64 NativeAOT output validation, report existing third-party warnings separately.
- [x] Structural check touched controls; Markdown links and git diff --check.
- [x] One fresh read-only final review of current change against2de819b6a; grade findings and fix important ones once with RED→GREEN. No implementation delegation.
- [x] Clean temporary fixtures/artifacts, hand over uncommitted worktree for review.

## Review focus

The separate Motion type must not acquire instance properties or derive from AbstractMotion. Callback registration/defaults, visibility CWT and static entry timing must survive; no lost duration/enable/layout options during definition preparation. All control property value types and builder/composition shapes must accept custom AbstractMotion. AXAML owner, namespace and existing syntax stay stable. Verify static MotionApi test aliases are not renamed as definition types. No extra per-frame work or compatibility wrapper.

## Ledger

Pre-flight: shared interfaces are AbstractMotion definition type and unchanged Motion target facade; all consumers migrate together. Current source baseline clean, branch feature/motion. The user explicitly authorized this split and start; preserve existing worktree and deliver for review without a commit.

Final: clean Core6, related Desktop918, Gallery24 passed. NativeAOT and net8 passed at their stated scope. Structural41 baseline findings/checker limit explicitly recorded; all12 Desktop source files are baseline-identical after type normalization. One fresh review clean, no new permanent cases. No commit/push/merge or new worktree. Final artifact/log cleanup and handoff follow.
