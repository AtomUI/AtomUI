# Performance tool build repair

> Execute inline with superpowers:executing-plans. User requested fixing the reported build errors.

**Goal:** Restore compilation and the affected Input/TabControl verification entry points.
**Architecture:** Import the existing registration/theme namespaces and migrate stale Tab overflow verification consumers to the existing typed TabScrollViewer/Popup/context contract. No production control behavior or dependency changes.
**Tech Stack:** .NET 10, AtomUI.Performance, Avalonia headless.
**Spec:** User request to repair the seven errors reported in the [presenter retirement record](../progress/2026-11-05-retire-input-text-presenter.md).

## Global constraints

Current feature checkout reused. Preserve all prior changes. No commit, new dependency, compiler exclusion, warning suppression, compatibility alias or new permanent test. Compiler/build and existing state-verification entry points are the red/green checks. Remove temporary logs and snapshots. No full regression or product NativeAOT rebuild for tool-only changes.

## Review focus

Registration extensions must resolve from their real namespace; stale private-field strings must not survive a mere type rename; overflow checks must exercise actual opening/closing and detach/reopen; ordinary close keeps reusable template resources while detached context actions become inert; all existing checks stay enabled.

## Task: restore the tool and affected checks

- [x] Reproduce the seven compiler errors and inspect the current source contracts.
- [x] Fix imports and typed consumers; reproduce stale runtime probes before migrating them.
- [x] Validate actual Input/TabControl entry points and the existing focused Tab overflow contract tests.
- [x] One fresh final review, clean temporary artifacts, verify final build and diff.
