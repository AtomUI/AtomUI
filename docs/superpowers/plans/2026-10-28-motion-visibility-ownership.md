# Motion Visibility Ownership Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans; inline implementation, one fresh final reviewer.

**Goal:** Fix all confirmed preparation/reentry/cleanup defects within MotionVisibility against79af86f4e.
**Spec:** ../specs/2026-10-28-motion-visibility-ownership-design.md
**Architecture:** Preserve request versions and existing playback. Fence notification boundaries, retain zero/current occupancy until playback takes ownership, independently release preparation handles and settle only the latest request.
**Tech Stack:** Avalonia12.1.4, C#, existing headless/native Gallery fixtures.

## Task1 — RED and lifecycle characterization
- [x] Add temporary desktop tests using real ContentViewport, existing app/manual clock; reproduce opacity callback reentry before Loaded.
- [x] Cover actual-visible notification reentry, preparation failure plus cleanup failure, cleanup callback latest intent and detach/remount. Distinguish new failures from already correct behavior.
- [x] Record independent expected geometry0/120 and latest binding values; no source mirrors or permanent new cases.

## Task2 — Implement and integrate
- [x] Fence opacity/definition/layout/visibility callback boundaries with current request checks.
- [x] Preserve current occupancy/opacity when an already-visible layout target is replaced while not arranged; do not reset reversal to opacity0.
- [x] Use deterministic preparation resource ownership; release all handles despite errors, retire before callbacks, and attempt stable projection independently.
- [x] Verify temporary tests green and related existing contracts; merge essential assertions into existing permanent tests, delete temporary tests/helpers/resources.
- [x] Update formal ownership/layout docs and NavMenu changelog, with scoped verification evidence.

## Task3 — Verify clean inputs and review
- [x] Exact Core/ContentExpansion/NavMenuInline/close/dialog and Gallery filters with TRX; no full/change-wide run.
- [x] Native exact Menu sample replay with verified current DLL hashes and original selected Option2; no pre-layout peaks/backtracking and real intermediate values.
- [x] net8 build and Gallery NativeAOT output validation; third-party warnings/platform gaps separate.
- [x] One fresh read-only review of current diff plus any untracked source; fix important findings once with valid RED→GREEN. No re-review or implementation delegation.
- [x] Clean temporary artifacts, run diff/link checks, deliver uncommitted current worktree.

## Review focus
Alpha and actual-visible notifications, disposal and resource lookup can synchronously change request identity. No stale request may create a layout lease or clear the new preparation. Pending visible reversals must freeze current presentation until handoff. Cleanup failure must not leave a zero lease or skip stable projection. Preserve pure Build contract and scoped latest bindings; no new per-frame work or public API. Test failure/cleanup paths at their exact original boundary, not only endpoints.

## Ledger
Pre-flight: preparation identity links Request, PlayAsync, SnapCurrent, Fail and Detached. PublicIMotionLayout and Motion API unchanged; callers share the single owner. Existing approved worktree, HEAD79af86f4e clean. User explicitly requested all fixes after concrete review; no extra approval round, branch/worktree creation, commit or full regression authorized.
