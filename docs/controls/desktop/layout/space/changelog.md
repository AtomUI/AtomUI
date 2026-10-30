# Space Changelog

本文档记录 Space 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-08-21

- Code
  - Add root frame appearance properties (`Background`, `BorderBrush`, `BorderThickness`, `CornerRadius`, `Padding`, `BorderDashArray`, `BorderDashOffset`) to `Space`, rendered through `BorderRenderHelper`, so Ant Design style-class root decoration (dashed border, background and padding) can be expressed directly on the control.
  - Make `Padding + BorderThickness` participate in the measure/arrange frame inset so children stay inside the root border.
- Gallery
  - Migrate `SpaceShowCase` to `GalleryShowCaseHost` with a Semantic Parts preview for `root` / `item` / `separator`.
  - Add a `Custom Semantic Part styling` example aligned with the Ant Design `style-class` demo: a dashed root border with grey item background and red bold separator, plus a `size=large` root with light blue background.
  - Add localization units for the semantic part descriptions, the style demo title/description and the demo button labels (en-US / zh-CN / zh-TW / pt-BR).
- Tests
  - Add `SpaceRootFrameTests` covering layout inset, legacy default layout and the appearance API surface.
  - Extend `SpaceShowCasePageTests` with document layout, semantic preview materialization, official style values and approved localization copy checks.
- Docs
  - Document the root customization boundary in the Space Semantic Part contract and update the Space overview and implementation docs.

## 2026-08-20

- Code
  - Add `Space` semantic parts for `root` / `item` / `separator` and propagate runtime markers from direct children and split separators.
  - Generate `SpaceItemStyle` and `SpaceSeparatorStyle` contract types through the semantic-part generator.
- Tests
  - Add regression coverage for the `Space` descriptor, runtime marker counts, separator rebuild behavior and generated style routing.
- Docs
  - Add the dedicated `Space Semantic Part` contract document and link it from the Space architecture and implementation docs.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Space`.
  - Align generated output paths with `controls/space/index-cn.md` and `controls/space/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Space desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Space desktop architecture documentation under `docs/controls/desktop/layout/space/overview.md`.
  - Add Space implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Space control-level changelog.
  - Add Space Token documentation covering SpaceToken.
