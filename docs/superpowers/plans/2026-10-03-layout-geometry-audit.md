# Layout Geometry Audit Implementation Plan

> **For agentic workers:** Use `superpowers:executing-plans` for the remaining work. Keep changes uncommitted for user review.

**Goal:** Repair the shared layout contract behind disappearing zoomed Watermarks and verified related geometry defects.

**Architecture:** Keep measurement requests separate from final layout boxes. Give content one arrangement owner, and derive rendering/input geometry from actual bounds and the event's visual root. Preserve existing control APIs and presentation behavior unless the user authorizes a specific change.

**Tech Stack:** Avalonia, C#, compiled AXAML, .NET 10, xUnit, Skia Headless.

**Spec:** The user's current chat request, `docs/engineering/development/control-development-guidelines.md`, and the existing ShowCase zoom design.

## Global Constraints

- Preserve unrelated DateViewer, Tooltip and documentation work in the primary checkout.
- Worktree baseline: `85bd9dffafd5088133fe64ca58e38ec995853104`.
- No commits; no full regression. Use exact filters or iterate scope with explicit module paths.
- Temporary tests belong in each owning project's `Temporary/LayoutSlotAudit`; delete them and their dedicated artifacts after validation.
- Do not infer layout intent from whether an unchanged effective alignment has an explicitly assigned value.

## Review Focus

- Natural-size centering versus default Stretch requires a user decision for the zoom presentation contract.
- External Switch templates using the existing Canvas PART must retain their positioning contract.
- Content-size changes smaller than the Switch knob must still update content offsets.
- Popup shadow, arrow, Padding and child Margin must share the same coordinate basis.
- Rate must reject other-window and occluded input while accepting transformed-row input.

### Task 1: Audit and reproduce geometry errors

- [x] Scan custom arrangement, rendering and raw-input code; classify intentional natural-size uses separately.
- [x] Reproduce Stage, Segmented, SkeletonLine, Switch, native Popup, Rate and ColorBlock failures in temporary tests.
- [x] Record original-size Watermark rendering and repeated zoom/restore with unchanged target height and scroll offset.

### Task 2: Repair verified control defects

- [x] Use final cross-axis slots and background bounds for Segmented.
- [x] Resolve SkeletonLine dimensions against final content boxes.
- [x] Let the Switch content canvas own presenter arrangement, with AXAML offset bindings and explicit measure/arrange dependency.
- [x] Preserve the existing external Canvas-template contract.
- [x] Allocate native Popup content inside shadow/Padding and preserve arrow-body origins.
- [x] Use final Rate/ColorBlock drawing geometry; map Rate raw input from its own root and check actual input hits.
- [x] Synchronize component implementation docs, component changelogs and the shared engineering geometry rules.

### Task 3: Finalize the zoom contract

- [x] User approved preserving natural-size centering with an optional strategy; implemented Natural (default) / Stretch.
- [x] Remove the candidate's `IsSet`-based policy and implement the selected observable contract.
- [x] Run temporary Stage and original Watermark rendering cases: 13/13 passed, including Skia text, multi-line and image rendering.

### Task 4: Review, cleanup and final verification

- [x] Independent review: six control families and the final zoom strategy received clean verdicts; review-derived fixes have before/after evidence.
- [x] Remove temporary tests and restore the temporary raster fixture; all new tests remain temporary and no net-new permanent tests are retained.
- [x] Cleaned module verification: related controls 203 passed; zoom/Gallery 566 passed; `git diff --check` passed. macOS ARM64 NativeAOT output validation passed.
- [x] Delivery: changes remain uncommitted in the attached worktree; no temporary or net-new permanent tests retained. Skia verified text/multiline/image zoom and restore. NativeAOT publish has ReactiveUI trim/AOT and Homebrew deployment-target warnings; no native desktop UI acceptance is claimed.

## 2026-10-04 global reassessment

- Scope: review every changed control and Gallery ownership path; preserve public contracts, Natural default, explicit Stretch, original heights and custom Glyph rendering.
- Reproduced additional failures: resource callback clear/shared attach leaked ownership; adorner replacement callback left duplicate watermarks; zero tile steps did not terminate; large built-in offsets drew invisible tiles; overlay retemplate retained old visual parent; Rate outside release left a stale press.
- Fix resource and visual ownership ordering with current-owner identity and single-assignment lifetime slots; do not use delays or suppression flags.
- Built-in culling uses real text ink bounds, mirror/normal union and periodic phase. Custom Glyphs keep their original drawing contract; extremely dense legitimate overlapping tiles remain a performance boundary without a newly approved density contract.
- Temporary evidence: Watermark 16 boundary/phase/resource tests plus four exact Skia pixel comparisons; Rate five cancellation/valid click cases plus existing bindings; overlay one new retemplate case plus existing host cases. All temporary test sources/fixtures removed before final focused validation.
- Final code review clean; independent phase comparison covered 20,000 exact rational samples. Final focused verification: 205 passed in 83.657s (48 Desktop, 155 GalleryBase, 2 Gallery); macOS ARM64 NativeAOT output validation passed. ReactiveUI trim/AOT and Homebrew deployment-target warnings persist. Native Popup input remains a platform verification boundary.
