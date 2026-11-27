# Motion Definition Consolidation Design

## 1. Intent

Consolidate duplicated Motion endpoint values, key-frame choreography, easing definitions, completion timing rules, and floating-point comparisons without changing public Motion types, constructor signatures, theme contracts, or documented animation semantics. Confirmed directional inconsistencies are repaired from tests rather than normalized mechanically.

This design covers:

- `AtomUI.Core/MotionScene`: `AbstractMotion`, Zoom, Slide, Collapse/Expand, Move and layout/ghost helpers.
- `AtomUI.Controls/Badge`: Badge in/out states and adorner preparation/reset paths.
- `AtomUI.Desktop.Controls/Primitives/FeedbackStack`: the easing curve shared with content expansion.
- Existing Dialog Composition work, which remains a consumer of the Core `ZoomBig` definition.

It does not create public animation-amplitude properties or Design Tokens.

## 2. Invariants

1. Public Motion class names, inheritance from `AbstractMotion`, constructors, `Offset`, `Direction`, `Duration`, `Easing`, `FillMode`, `SpiritType`, and scene-size overrides remain source and binary compatible.
2. Pure consolidation preserves current endpoints, origin, easing, duration, key-frame cues, offset ratios, cleanup state, and visual lifecycle.
3. A numeric pair or key-frame profile that represents one semantic state has one internal owner.
4. Different Motion families do not share a definition merely because their current numbers match; shared definitions require shared product semantics.
5. Confirmed directional bugs receive failing behavior tests before implementation and are separate from mechanical consolidation.
6. No runtime reflection, type scanning, new binding, subscription, timer, visual node, or trimming root is introduced.

## 3. Internal Definition Model

### 3.1 Opacity and scale states

Each semantic family owns an internal immutable state definition near its implementation:

- `ZoomMotionDefinition`: ordinary Zoom opening, visible and closing states.
- `ZoomBigMotionDefinition`: existing `OpeningStart`, `Visible`, `ClosingEnd` states.
- `DirectionalZoomMotionDefinition`: hidden and visible states shared by Up/Left/Right/Down Zoom.
- `SlideMotionDefinition`: hidden and visible states; axis and origin remain direction-specific.
- `CollapseMotionDefinition`: collapsed and expanded states plus one direction-to-origin resolver.
- `BadgeMotionDefinition`: hidden and visible opacity/transform states shared by motions and adorner convergence paths.

State objects group values that must change together. They remain `internal`; existing `InternalsVisibleTo` relationships allow Desktop and Controls consumers without expanding Public API.

### 3.2 Easing definitions

The identical `(0.645, 0.045, 0.355, 1)` spline is represented by an internal factory in Core. The factory returns a new `SplineEasing` per use so consumers do not share mutable easing state. `ContentExpansionAnimator` and `FeedbackCardMotion` consume the factory.

### 3.3 Completion timing

`AbstractMotion` owns one private completion-timeout calculation:

```text
transition maximum duration
  + min(maximum duration * 10%, 100ms)
```

Synchronous and asynchronous transition execution call the same helper.

## 4. Move Motion Model

`MoveMotions.cs` currently duplicates eight three-key-frame animations. Introduce one internal `MoveMotionDefinition` that produces a profile from:

- direction: Up / Down / Left / Right;
- phase: Entering / Exiting;
- caller-provided offset;
- the existing duration, easing and fill mode.

The eight public Motion classes remain thin `AbstractMotion` wrappers. They consume the profile for animation construction, transform origin, scene size and scene position.

### 4.1 Key-frame contract

| Phase | Start opacity | Middle opacity | End opacity |
| --- | ---: | ---: | ---: |
| Entering | 0 | 0.1 | 1 |
| Exiting | 1 | 0.1 | 0 |

- Cues remain `0`, `0.8`, `1`.
- Move is translate/fade; scale stays identity for every direction.
- Existing vertical middle offset ratio remains `1/4`.
- Existing horizontal middle offset ratio remains `1/2` until a separate visual-design decision changes it.

This fixes the isolated `MoveUpIn` Y-scale `0.01` and `MoveRightOut` middle opacity `1.0` differences as copy inconsistencies.

### 4.2 Direction geometry matrix

| Direction | Start/end offset vector | Scene size | Entering scene position |
| --- | --- | --- | --- |
| Down | `(0, +Offset)` | height × 2 | `Y + actor height` |
| Up | `(0, -Offset)` | height × 2 | `Y - actor height` |
| Left | `(-Offset, 0)` | width × 2 | `X - actor width` |
| Right | `(+Offset, 0)` | width × 2 | `X + actor width` |

The matrix fixes the horizontal `WithHeight(...)` calls and `MoveRightIn` X/Y coordinate mix-up.

## 5. Collapse/Expand Direction Contract

Collapse and Expand use the same origin for a given direction:

| Direction | Origin |
| --- | --- |
| Left | `(1, 0.5)` |
| Right | `(0, 0.5)` |
| Top | `(0.5, 1)` |
| Bottom | `(0.5, 0)` |

`ExpandMotion` Bottom currently differs from the corresponding Collapse origin and is corrected through a failing direction-matrix test.

Collapsed state is `(opacity=0, active-axis scale=0.01)`; expanded state is `(opacity=1, active-axis scale=1)`.

## 6. Floating-Point Comparison Cleanup

- `MotionGhostControl` removes its local `AreClose` implementation and uses `MathUtils.AreClose`.
- `BaseLayoutAwareMotionActor.IsSizeSmaller` uses the shared `MathUtils` ordering semantics instead of an independent `0.0001` constant.
- Tests pin the intended near-equality behavior before changing the comparison path.

## 7. Test Strategy

### 7.1 Characterization before refactor

Add Core tests for:

- ordinary, directional and big Zoom start/end actor states;
- Slide horizontal/vertical start/end states;
- Collapse/Expand state and direction origins;
- Move key-frame cue, opacity, scale, translation, scene size and scene position matrices;
- `AbstractMotion` completion slack through both sync and async paths where observable;
- near-equality behavior used by Motion layout/ghost helpers.

Add Controls tests for Badge preparation, in/out and convergence states. Existing Feedback and Dialog tests remain consumers.

### 7.2 RED requirements

The following tests must fail against the current implementation for the expected value mismatch:

- Expand Bottom origin.
- MoveUpIn starting scale.
- MoveRightOut middle opacity.
- MoveLeftOut, MoveRightIn and MoveRightOut scene width.
- MoveRightIn scene X position.

Pure single-source refactors begin from green characterization tests and remain green; no source-text or reflection-shape assertions are added.

## 8. Phasing

1. Add behavior matrices and prove the confirmed inconsistencies RED.
2. Fix Move and Collapse behavior through their shared definitions; verify GREEN.
3. Consolidate Zoom, Slide, Badge and AbstractMotion values without behavior change.
4. Consolidate the shared easing factory and floating-point comparison helpers.
5. Run focused Motion tests, affected verification, `git diff --check`, and manual Gallery checks for consumers that visibly use the changed motions.

Each phase remains reviewable and may be reverted independently in the working tree. No commit is created without explicit user instruction.

## 9. Contract and Risk Summary

- Public/API contract change: none.
- Theme/Token contract change: none.
- Intended visual behavior changes: only the explicitly identified Move and Expand inconsistencies.
- AOT impact: none; all definitions are static typed code.
- Primary risk: correcting legacy differences that may have become relied upon accidentally.
- Risk control: literal behavior matrices, RED evidence, affected consumers and manual Gallery verification.
