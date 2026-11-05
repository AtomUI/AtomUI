# Retire the InputTextPresenter compatibility layer

> Execute inline with the executing-plans skill. User approved implementation on 2026-11-05.

**Goal:** Remove the obsolete internal selection-cache workaround while preserving input behavior and public contracts.
**Architecture:** Six AXAML templates use Avalonia TextPresenter directly. OTP uses the existing TextPresenter contract. No replacement hook, binding or subscription.
**Tech stack:** Avalonia 12.1.5, .NET 10, compiled AXAML, generated Semantic Part Styles, Skia headless rendering.
**Spec:** [Approved source audit](../progress/2026-11-05-avalonia-1215-hook-retirement-audit.md).

## Global constraints

- Reuse the current feature/upgrade-12.1.5 checkout, preserve all pre-existing changes. No commit.
- Scope: InputTextPresenter, six templates, OTP presenter typing, existing Semantic Part assertions, input verification tooling and current control documentation.
- Keep PART names, bindings, classes, selection/caret/preedit/password behavior and public APIs unchanged.
- Do not remove other hooks, change clocks, or introduce resource bindings or runtime template construction.
- Temporary tests first; remove them and their artifacts before final module verification. No full regression.
- This is compatibility retirement, not a speculative performance project: report duplicate callback removal, with no wall-clock speedup claim or invented page timings.

## Tasks

- [x] Establish existing selector migration failures and before/after real-template rendering/state evidence.
- [x] Replace six template nodes, narrow OTP types and remove the compatibility class.
- [x] Make input verification assert selected foreground output rather than implementation callbacks; remove obsolete helper.
- [x] Synchronize current control docs and dated audit status.
- [x] Validate affected input consumers, generated Styles, Gallery/tool builds and trimming/AOT as appropriate.
- [x] One fresh final implementation review; resolve findings, clean temporary artifacts and run final focused module checks.

## Review focus

Native null-selection-brush semantics; all six template paths; dedicated AXAML Semantic Styles; OTP caret ownership and template lifecycle; stale concrete-type assumptions; AOT-visible type references; separation from prior menu/clock work.
