# Icon Changelog

本文档记录 Icon 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-23

- 修复模板图标遗留宿主绑定及显式 child 值被覆盖；两类 Presenter 的画刷/尺寸采用原生 Template 优先级，detach/替换释放并在 attach 恢复；IconPresenter 默认画刷消费自己的 Effective Global。

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
