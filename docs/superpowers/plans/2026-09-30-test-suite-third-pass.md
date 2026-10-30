# Test Suite Value Cleanup — Third Pass Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task by task.

**Goal:** Continue deleting permanent tests that only preserve completed migrations, retired APIs, or private Gallery implementation details.

**Architecture:** Work directly on `release/6.0` as requested. Delete only tests with no independent behavioral detection after naming the surviving runtime, packaging, or architecture safeguard; retain UI input, scrolling, resource lifecycle, AOT/trim, Semantic Part, and data correctness tests.

**Tech Stack:** .NET 10, xUnit v3, Avalonia.Headless, repository verification CLI.

**Spec:** [Test value and lifecycle policy](../../engineering/development/test-value-and-lifecycle.md).

## Global Constraints

- Preserve the user's unstaged `tests/AtomUI.Desktop.Controls.Rendering.Tests/GalleryTabOverflowShadowRenderingTests.cs` change.
- Do not run full regression unless explicitly requested; verify changed owners with explicit iterate paths.
- Do not commit without a new explicit commit request.

### Task 1: Delete completed migration and retired API guards

- [x] Remove the seven remaining Desktop token-scope migration suites and their private helper.
- [x] Remove Theme and registration reflection tests that freeze retired or exact public API shape already governed by compilation, behavior tests, and package validation.
- [x] Remove Splash source-absence checks for retired implementation types.

### Task 2: Prune Gallery private-shape checks

- [x] Remove workspace tests that freeze removed search UI, separator implementation, old menu styling, minimum-width literal, or exact submenu structure.
- [x] Preserve pointer-hit-test, command behavior, theme switching, and caption capability contracts.

### Task 3: Verify and record

- [x] Run repository audit, `git diff --check`, and focused owner tests. Verification policy was unchanged, so the existing 72/72 self-test receipt remains applicable.
- [x] Count discoverable cases and report the delta without claiming unmeasured full-suite speedup.

## Result

- Deleted 10 test files: seven completed Desktop theme-scope migration suites, their dedicated helper, and the remaining Theme/registration retired-API reflection suites.
- Removed completed Splash implementation-absence checks, obsolete DataGrid/Upload public-shape checks, and Gallery assertions already fully covered by approved example snapshots.
- Kept the ListView group-header height contract but removed its diagnostic console output. Kept Gallery per-page scroll ownership, deferred loading, Semantic Part, pointer input, window command and theme-switching tests.
- Focused iterate passed 4,693 tests across Core, DataGrid, Desktop Controls and Gallery in 665.238 seconds. Repository audit found 15 projects, 757 test files, 755 classes and no mapping gaps; `git diff --check` passed.
- Current discoverable total is 6,781 tests. This pass removed 52 cases from the post-merge 6,833 total; cumulative reduction from the original 7,318 is 537 cases. Full regression was not run, so full-suite wall-time improvement remains unmeasured.
