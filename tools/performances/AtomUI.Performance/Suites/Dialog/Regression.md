# Dialog Regression Matrix

## Functional matrix

- [x] Overlay modal anchored opening uses compositor opacity/scale and keeps mask synchronized.
- [x] Overlay modal anchored closing keeps Surface, content layer and mask synchronized until teardown.
- [x] Overlay modal unanchored opening/closing uses compositor fade without UI-thread opacity frames.
- [x] Overlay modeless opening/closing runs without a mask and preserves background input.
- [x] `IsMotionEnabled=false` skips visual motion but still completes attach, close and teardown.
- [x] Window host continues to use native `Opened` / `Closed` lifecycle rather than Overlay choreography.
- [ ] Missing optional `PART_SurfaceContentLayer` degrades to Surface/mask motion without blocking close.
- [ ] Missing required Composition visual causes an all-or-nothing fallback to existing transitions.

## Multi-step user flows

- [x] Modal Gallery basic Overlay: open from the original trigger, observe continuous mask + anchored Surface motion, close with OK, repeat at least three times.
- [ ] Modal Gallery fallback-placement static API: open without an explicit placement target, verify fade-only opening/closing, repeat at least three times.
- [x] Open a Dialog and immediately request close during opening; verify no stale actor transform or retained presenter (automated headless coverage).
- [x] Open nested Dialogs; close the top Dialog and verify focus returns to the lower Session before final teardown (automated lifecycle coverage).

## Lifecycle matrix

- [x] Opening cancellation stops server animations and restores actor base values before closing motion.
- [x] Normal close waits for the completion boundary before composition disconnect, Surface dispose and layer removal.
- [ ] Re-template releases old mask/content part references and the next open uses the current visuals.
- [ ] Presenter dispose leaves no active timer, event handler, binding, composition parent or Dialog layer entry.

## Platform evidence

- [ ] Windows: masked anchored open/close frame pacing recorded under issue #486 conditions.
- [x] macOS: masked anchored open/close visually checked with native screen recording.
- [ ] Linux Wayland: Overlay open/close and content popup layering checked.
- [ ] Linux X11: Overlay open/close and content popup layering checked.

## Notes

Checked automated items were covered by the affected Dialog test classes, including `OverlayDialogPresenterCompositorMotionTests`, `OverlayDialogPresenterTests`, `DialogMotionAnchorTests`, and `DialogLifecycleTests`. The macOS manual item is backed by the 2026-09-26 screen recording analysis in `docs/superpowers/progress/2026-09-26-dialog-compositor-motion-performance.md`. Unchecked platform and custom-template fallback items remain required before claiming those environments or branches are verified.
