# Splitter Changelog

本文档记录 Splitter 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-14

- API
  - Add `DraggerDoubleClicked` and `SplitterDraggerDoubleClickedEventArgs`, exposing the zero-based handle index without applying an automatic reset policy.
- Runtime
  - Detect platform double-clicks from pointer click count, suppress the second drag start, dispatch after the matching left-button release, and cancel pending notification on pointer capture loss.
  - Keep dragger double-click available when resizing is disabled and isolate nested Splitter notifications to their direct owner.
  - Pair double-click subscriptions with drag bar template replacement and generated handle rebuild cleanup.
- Gallery
  - Add the stable `splitter-double-click-reset` ShowCase item, restoring three panels to their 30% / 40% / 30% default sizes.
- Docs
  - Document double-click event timing, ownership, reset responsibility, lifecycle and validation boundaries.

## 2026-08-21

- Runtime
  - Publish `root` / `panel` / `dragger` Semantic Parts for `Splitter` (`Splitter.SemanticParts.cs`), generated Semantic Style types `SplitterPanelStyle` / `SplitterDraggerStyle`.
  - Add runtime `semantic-panel` markers on tracked user panels in `SplitterPanel`, with ownership tracking so user-declared classes survive panel removal.
  - Add runtime `semantic-scope-handle` markers on generated `SplitterHandle` instances.
  - Add `BorderDashArray` / `BorderDashOffset` public root frame dash APIs on `Splitter` for dashed root borders.
- Theme
  - Declare static `semantic-scope-panel` on `PART_SplitterPanel` (`SplitterTheme.axaml`) and `semantic-dragger` on `PART_DragBar` (`SplitterHandleTheme.axaml`).
  - Bind `StrokeDashArray` / `StrokeDaskOffset` on the root `Frame` to `BorderDashArray` / `BorderDashOffset` (`SplitterTheme.axaml`).
- Gallery
  - Switch Splitter ShowCase to `GalleryShowCaseHost` with a Semantic Parts tab previewing `root` / `panel` / `dragger`.
  - Add Splitter semantic part descriptions to all Gallery language catalogs.
  - Add a trailing Semantic Part styling ShowCase item (`splitter-semantic-part`) aligned with the Ant Design style-class demo: root background + dragger styling, then a dashed root border with secondary text.
- Tests
  - Add `SplitterSemanticPartTests` covering descriptor fields, static theme markers, runtime marker counts, children sync, user-owned class preservation and generated Semantic Style application.
  - Add `SplitterBorderDashTests` covering root frame dash propagation through the theme template.
  - Extend `SplitterShowCasePageTests` with Semantic Parts tab materialization, semantic demo declaration, official style value application and localization copy checks.
- Docs
  - Add [Splitter Semantic Part 契约](semantic-part.md).
  - Update `overview.md` and `implementation.md` with the Semantic Part model, marker ownership, root frame dash APIs and verification boundaries.

## 2026-07-02

- Runtime
  - Add `LineThickness` and `LineCornerRadius` public style APIs on `Splitter`, propagate them through `SplitterPanel`, `SplitterHandle` and `SplitterDragBar`, and bind them to `PART_HandleLine` / `PART_Grip`.
  - Bind Splitter root frame appearance to `Background`, `BorderBrush`, `BorderThickness` and `CornerRadius`.
- Gallery
  - Add Splitter API rows and a line style ShowCase item for root frame and visible divider customization.
- Docs
  - Clarify Splitter public contract, internal collaboration types and resize event model.
  - Document the style boundary for root frame appearance, visible split line thickness, split line corner radius and user-owned panel content.
  - Define the maintenance rule that line style APIs must be exposed from `Splitter` and propagated to internal handle parts instead of exposing `SplitterHandle` or `SplitterDragBar`.
  - Clarify the semantic difference between `HandleSize`, `SplitBarHandleSize`, `SplitBarSize` and `HandleLineThickness`.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Splitter`.
  - Align generated output paths with `controls/splitter/index-cn.md` and `controls/splitter/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Splitter desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Splitter desktop architecture documentation under `docs/controls/desktop/layout/splitter/overview.md`.
  - Add Splitter implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Splitter control-level changelog.
  - Add Splitter Token documentation covering SplitterToken.
