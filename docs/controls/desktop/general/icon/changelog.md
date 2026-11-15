# Icon Changelog

本文档记录 Icon 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-14

- Performance
  - Remove the Icon control template. Its only node, a `Border`, repeated what `Icon.Render`, `Width`/`Height` and `ICustomHitTest` already provide; each visible Icon now has one visual instead of two. `Icon.Render` paints `Background` over the glyph in local bounds as the Border did, so rendering and layout are pixel-identical, including non-transparent backgrounds.
  - Track ancestor visibility for the loading animation only while the Icon is loaded, visible and animated, so hidden loading icons in templates no longer subscribe to every ancestor; skip the composition reset for icons that never started rotating.
- Tests
  - Cover loading animation start/stop across own visibility, ancestor visibility, animation changes and detach, plus the template-free layout, hit-test and unchanged background output.

## 2026-10-13

- Fixed
  - Initialize the primary stroke brush and Pen in their own slot, preserving metadata defaults independently of the fallback brush.

## 2026-08-25

- Fixed
  - Stop loading Icon Compositor animation when the Icon becomes effectively invisible and resume it when visible again.
- Docs
  - Document loading animation visibility tracking and unload/detach cleanup.

## 2026-08-09

- Fixed
  - Make `IconPresenter` and `IconTemplatePresenter` project `IconBrush` to all AtomUI `Icon` brush slots so host foreground states stay synchronized for multi-brush icons.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Icon`.
  - Align generated output paths with `controls/icon/index-cn.md` and `controls/icon/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Icon desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Icon desktop architecture documentation under `docs/controls/desktop/general/icon/overview.md`.
  - Add Icon implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Icon control-level changelog.
  - Add Icon Token documentation covering IconToken.
