# Spin Changelog

本文档记录 Spin 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-08-25

- Fixed
  - Stop and restart Spin Compositor rotation and dot-opacity animations across effective visibility changes.
- Docs
  - Document ancestor visibility tracking and Compositor cleanup ownership.

## 2026-08-16

- Semantic Parts
  - Add Ant Design-aligned Semantic Part descriptors for `Spin` and the public `SpinIndicator` owner.
  - Add static template markers and generated owner-scoped style contracts without runtime discovery.
  - Document the dual static content targets used by built-in and custom SpinIndicator implementations.
- API
  - Promote `AbstractSpinIndicator.DotBgBrush` to public so the built-in indicator color is customizable through the `indicator` Semantic Part.
- Gallery
  - Add the Semantic Parts Preview and Ant Design-aligned custom styling example using AtomUI `v6.1.3` version metadata.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Spin`.
  - Align generated output paths with `controls/spin/index-cn.md` and `controls/spin/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Spin desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Spin desktop architecture documentation under `docs/controls/desktop/feedback/spin/overview.md`.
  - Add Spin implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Spin control-level changelog.
  - Add Spin Token documentation covering SpinToken.
