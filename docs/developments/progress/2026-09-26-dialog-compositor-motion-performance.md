# Dialog Overlay Composition Motion Performance Investigation

## Scope

- Report: [AtomUI issue #486](https://github.com/AtomUI/AtomUI/issues/486), masked Overlay Dialog opening feels unsmooth.
- Control: `Dialog` / `MessageBox`, `DialogHostType.Overlay` only.
- Public API, ControlTheme keys, Template Parts, Semantic Parts and Token values remain unchanged.
- Gallery qualification: `ModalShowCase.axaml` contains 11 direct `Dialog` and 9 direct `MessageBox` instances; opening and closing are repeatable interactions.

## Reproduction and evidence

The original Overlay path used `AbstractMotion` transitions on `MotionActor.Opacity` and `MotionActor.MotionTransform`; modal mask and close content opacity used additional Avalonia UI-thread animation channels. Avalonia Composition can animate `CompositionVisual.Opacity` and `CompositionVisual.Scale` on the render/compositor side, so these channels do not require UI-thread property interpolation.

Evidence collected on 2026-09-26:

- A Debug macOS probe observed UI-transition ticks at 6.6-9.7ms average with no frame above 25ms. This is a local smoke result, not proof for other platforms; it showed that frame production still followed the UI/compositor commit cadence.
- Source inspection of the resolved Avalonia dependency confirmed that `Opacity` and `Scale` are Composition visual properties, while AtomUI's `MotionTransform` transition is an Avalonia property path updated on the UI thread.
- A native screen recording of the first compositor prototype without an initial commit showed the first opening as one changed source frame at about 1.133s: the Dialog jumped directly to the visible state.
- A minimal A/B probe established the cause: an existing visual animated; a freshly attached visual started immediately did not; the same freshly attached visual animated after `Compositor.RequestCommitAsync()`.
- A second native screen recording after adding the initial commit showed 12 changing source frames from about 0.953s to 1.170s for the first opening (about 217ms), instead of a one-frame jump.

The recording analysis used native decoded frames, a fixed Dialog-window crop, downsampling to 96x64 grayscale and mean absolute difference between consecutive frames. It is smoke evidence for visual continuity, not a cross-platform frame-time benchmark.

## Attempts and rulings

| Attempt | Result | Ruling |
| --- | --- | --- |
| Keep `AbstractMotion` transitions and only profile them | macOS looked smooth, but every surface transform/opacity frame still crossed the UI property/commit path. | Insufficient: it did not remove the platform-sensitive dependency. |
| Start Composition animation immediately after adding the presenter | `GetElementVisual` returned a visual, but the first animation batch was silently ineffective and the Dialog jumped visible. | Rejected. Visual availability is not proof that its server counterpart has processed the attach batch. |
| Wait one initial `RequestCommitAsync`, then start Composition animation | Native recording contained a continuous multi-frame opening. | Adopted for opening only; closing uses an already attached visual and does not need another activation commit. |
| Wait a second `RequestCommitAsync` after starting animations | Headless compositor runs could leave this second processed-batch task pending. | Rejected. It did not improve the visual contract and made completion dependent on another render-loop round trip. |
| Use a captured `Task.Delay` continuation as the presenter boundary | Manual headless `RunJobs()` probes were nondeterministic because queue draining does not drive dispatcher timers/run loops. | Replaced with a one-shot Avalonia Dispatcher timer and tests that run the real controlled dispatcher loop. |
| Mix Composition for available actors and UI transitions for missing actors | Surface, mask and content could run on different clocks and diverge visually. | Rejected. Any missing required Composition visual causes an all-or-nothing fallback for that choreography. |

## Adopted design

`OverlayDialogPresenter` now:

1. resolves Surface, optional modal mask and optional close-content Composition visuals;
2. waits one compositor commit before the opening animation so freshly attached server visuals are active;
3. starts server-side `Opacity` and, for anchored motion, `Scale` key-frame animations with the existing easing and duration;
4. uses one disposable Dispatcher timer for `duration + 32ms` as the async teardown boundary; the timer does not drive frames;
5. restores actor/visual base state on opening cancellation before close begins;
6. falls back as a whole to the existing `AbstractMotion` / Avalonia `Animation` path when a required Composition visual is unavailable.

`AtomUI.Core` owns the `ZoomBig` endpoint pairs as internal `ZoomBigMotionDefinition.State` values (`OpeningStart`, `Visible`, `ClosingEnd`). The generic Transition motions and Dialog Composition path consume those same states, so opacity and scale cannot drift between the two execution paths; this uses the existing Core-to-Desktop `InternalsVisibleTo` boundary and adds no Public API or Token.

The Window host remains unchanged and continues to use native `Opened` / `Closed` lifecycle boundaries.

## Benefit

The table counts UI-thread animated property channels for a modal Dialog with the standard content layer. It is a structural result; no cross-platform timing percentage is claimed.

| Metric | Baseline | Optimized | Formula | Improvement | Conclusion |
| --- | ---: | ---: | --- | ---: | --- |
| Anchored opening UI-thread animation channels per Dialog | 3 | 0 | `(3-0)/3` | 100% removed | Surface opacity/transform and mask opacity run on the compositor. |
| Anchored closing UI-thread animation channels per Dialog | 4 | 0 | `(4-0)/4` | 100% removed | Surface opacity/transform, mask opacity and content opacity run on the compositor. |
| Unanchored opening UI-thread animation channels per Dialog | 2 | 0 | `(2-0)/2` | 100% removed | Surface and mask fades no longer write UI properties per frame. |
| Unanchored closing UI-thread animation channels per Dialog | 3 | 0 | `(3-0)/3` | 100% removed | Surface, mask and content fades no longer write UI properties per frame. |
| First-opening changed source frames in the macOS recording smoke | 1 frame | 12 frames over about 217ms | observed native frames | qualitative continuity restored | Confirms the activation commit prevents the one-frame jump in the recorded scenario. |

Complexity burden: two presenter execution paths (Composition primary and existing fallback), seven focused Composition helper methods, one bounded completion timer, 287 production lines added / 36 removed across two files, no new public API, no new persistent field, no subscription, no visual node and no Theme-to-C# visual migration.

## Regression coverage

`OverlayDialogPresenterCompositorMotionTests` proves:

- anchored opening and closing do not write UI-thread `MotionTransform` frames;
- unanchored opening and closing do not write intermediate UI-thread `Opacity` frames;
- closing during opening restores actor opacity/transform before teardown;
- each test uses the real headless Composition visual and controlled dispatcher main loop;
- the tests share the non-parallel Dialog lifecycle collection.

The permanent control matrix is `tools/performances/AtomUI.Performance/Suites/Dialog/Regression.md`.

## Remaining validation boundary

- The final code path has automated headless coverage and macOS recording evidence from the equivalent initial-commit prototype.
- No reliable before/after Windows frame-time sample was collected in this workspace, so no Windows timing or percentage improvement is claimed.
- Gallery page-load timing is not used as evidence for this interaction-only change; it does not measure Dialog open/close frame pacing.
- Repository affected verification covers the changed Core MotionScene path plus its Desktop, Gallery, localization, rendering, DataGrid and LLMS consumers; the authoritative current result is the final `.artifacts/verification/latest.json` receipt. The runner keeps the separate performance obligation pending because its comparable evidence is recorded in this document.
- `--verify-dialog-states` could not build because the existing TabControl performance verification source references missing `TabControlScrollViewer` / `TabStripScrollViewer` types; this unrelated harness baseline was not changed.
- Modal-only LLMS generation and verification passed for 1 control / 5 files. Full-repository LLMS verify still reports pre-existing Button/DropdownButton semantic output drift; Modal generated outputs are synchronized.
- Before closing issue #486, run the Modal Gallery on Windows with a masked anchored Dialog and record opening/closing frame pacing under the original trigger conditions.
