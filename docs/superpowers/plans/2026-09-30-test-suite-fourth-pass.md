# Test Suite Value Cleanup — Fourth Pass Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task by task.

**Goal:** Aggressively remove tests whose failure-detection capability is already fully supplied by approved Gallery snapshots or package/API validation.

**Architecture:** Treat an approved example snapshot as the single complete guard for that page's example markup. Delete subset assertions over the same markup, but retain page-shell scroll/deferred-loading contracts, Semantic Part hosts, runtime interaction tests, code-behind behavior, AOT/trim, lifecycle and data correctness.

**Tech Stack:** .NET 10, xUnit v3, Avalonia.Headless, repository verification CLI.

**Spec:** [Test value and lifecycle policy](../../engineering/development/test-value-and-lifecycle.md).

## Constraints

- Work directly on `release/6.0`.
- Preserve the user's unstaged rendering-test change.
- Do not commit or run full regression without explicit instruction.

## Tasks

- [x] Remove example-markup subset assertions already covered by approved snapshots.
- [x] Remove compiler/API-compat duplicate checks while retaining observable defaults and behavior.
- [x] Run audit, `git diff --check`, focused Desktop/Gallery verification, and record the new count.

## Result

- Removed 23 discoverable cases: 16 Gallery example subset assertions already covered by approved snapshots, plus seven Desktop API/reflection cases duplicated by package validation and runtime behavior suites.
- Deleted the now-empty `ListViewSelectionContractTests.cs`; retained ListView selection model/state tests. NavMenu command/composition/spacing behavior remains covered by container, command and layout suites. Upload input properties and SourceKind remain covered by file-selection, drop-zone and input-pipeline tests.
- Focused iterate passed 4,210 Desktop tests and 21 selected Gallery tests in 669.287 seconds. Repository audit found 15 projects, 756 test files, 754 classes and no mapping gaps; `git diff --check` passed.
- Current discoverable total is 6,765 tests, down 553 from the original 7,318. Full regression was not run, so its wall-time improvement remains unmeasured.
