# Separator Changelog

本文档记录 Separator 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-08-16

- Semantic Parts
  - Add Ant Design-aligned Semantic Part descriptors for `Separator`：`root`（owner）、`rail`（`SeparatorRail`，`.semantic-rail`，`Multiple`）与 `content`（`TextBlock#PART_Title`，`.semantic-content`）。
  - Introduce `SeparatorRail` line-render control and move connection-line drawing from `AbstractSeparator.Render` into two template rail nodes (`PART_RailStart` / `PART_RailEnd`) so `rail` is a resolvable template target aligned with Ant Design's `rail` semantic.
  - Add static template markers and generated owner-scoped `SeparatorRailStyle` / `SeparatorContentStyle` without runtime discovery.
- Gallery
  - Add the Semantic Parts Preview reproducing Ant Design's `_semantic.tsx` demo (lorem ipsum, plain/`Solid`/`Dotted`/`Dashed`, vertical dividers) and the custom Semantic Part styling example.

## 2026-07-29

- Breaking
  - Change `SizeType` from `SizeType` to `CustomizableSizeType` and implement `ICustomizableSizeTypeAware`, adding `Custom` as the explicit caller-owned spacing mode.
- Fix
  - Restore horizontal `Small`、`Middle`、`Large` block margins from SharedToken values and apply them consistently to titled and untitled Separator layouts.
- Theme
  - Keep a horizontal Middle margin baseline, omit a `Custom` preset selector, and let instance values or owner-scoped styles override spacing.
  - Keep Drawer structural separators compact through `SizeType=Custom` and a Drawer-owned template selector.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Separator`.
  - Align generated output paths with `controls/separator/index-cn.md` and `controls/separator/semantic-cn.md`.

## 2026-06-24

- Fix
  - Reduce untitled horizontal Separator block margin tokens to `1px` so plain divider lines do not create large vertical gaps in compact surfaces such as Drawer headers.

- Docs
  - Complete Separator desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Separator desktop architecture documentation under `docs/controls/desktop/general/separator/overview.md`.
  - Add Separator implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Separator control-level changelog.
  - Add Separator Token documentation covering SeparatorToken.
