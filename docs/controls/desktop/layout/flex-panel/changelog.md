# FlexPanel Changelog

本文档记录 FlexPanel 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-27

- API
  - Use `Avalonia.Controls.FlexDirection` for `Direction` and `DirectionProperty`. Update explicit references to `AtomUI.Controls.FlexDirection` and rebuild affected assemblies. AXAML values such as `Direction="Row"` remain valid.
  - Use `Avalonia.Controls.Flex` for child layout settings. Replace explicit references to AtomUI's `Flex`, `FlexBasis`, `FlexBasisKind` and `AlignItems` with Avalonia's `Flex`, `FlexBasis`, `FlexBasisKind` and `FlexAlignItems`. In AXAML with the default Avalonia namespace, use `Flex.Grow` and other `Flex.*` properties on children of `atom:FlexPanel`.

## 2026-11-21

- Fix
  - Relayout when a child's `Width`, `Height`, `MinWidth`, `MinHeight`, `MaxWidth`, `MaxHeight` or `Margin` changes; these are now registered with `AffectsParentMeasure` because a child measured against its flexed length keeps the same clamped `DesiredSize` ([AtomUI/AtomUI#544](https://github.com/AtomUI/AtomUI/pull/544)).
  - Skip the post-flex remeasure for items whose resolved length equals their base length, so nested content growth still propagates to the panel ([AtomUI/AtomUI#544](https://github.com/AtomUI/AtomUI/pull/544)).
- Tests
  - Add `FlexPanelRelayoutTests` covering child size, margin, min-width and nested content changes.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `FlexPanel`.
  - Align generated output paths with `controls/flex-panel/index-cn.md` and `controls/flex-panel/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete FlexPanel desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish FlexPanel desktop architecture documentation under `docs/controls/desktop/layout/flex-panel/overview.md`.
  - Add FlexPanel implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add FlexPanel control-level changelog.
  - Document that FlexPanel does not require a dedicated Token document and records its theme dependencies in the overview.
