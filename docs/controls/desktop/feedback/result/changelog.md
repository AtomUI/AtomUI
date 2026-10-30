# Result Changelog

本文档记录 Result 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-08-15

- Semantic Part
  - Publish `root`, `icon`, `title`, `subTitle`, `extra` and `body` for `Result` with five generated owner-scoped Style types.
  - Mark the two icon alternatives and four content presenters statically while keeping the default theme independent from `.semantic-*` selectors.
- API and Theme
  - Add `StrokeDashArray` to `AbstractResult` and project the standard root surface through `DashedBorder`.
  - Preserve all seven statuses, custom icons, optional content visibility and icon-presenter re-template subscription ownership.
- Fix
  - Recompute title and subtitle line heights when `HeaderFontSize` or `SubHeaderFontSize` changes after template application.
  - Stretch the `extra` Semantic Part across the Result content area while keeping its content centered, so area styles do not collapse around the action control.
- Gallery
  - Add the deferred Result Semantic Part preview with the six Ant Design 6.1.3 descriptions and the AtomUI `v6.1.3` Tag.
  - Reproduce the Ant Design `classNames Object` and `classNames Function` demo content and style values with owner-scoped Semantic Part styles.
- Tests
  - Cover descriptor metadata, six static marker targets, generated Style hits, root surface projection, status/content stability, re-template cleanup and lazy Gallery materialization.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Result`.
  - Align generated output paths with `controls/result/index-cn.md` and `controls/result/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Result desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Result desktop architecture documentation under `docs/controls/desktop/feedback/result/overview.md`.
  - Add Result implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Result control-level changelog.
  - Add Result Token documentation covering ResultToken.
