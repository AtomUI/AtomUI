# Statistic Changelog

本文档记录 Statistic 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-08

- Token
  - 主题改读控件有效 Token：Statistic、TimerStatistic、StatisticCountUp 以各自 identity 读取全局 Token；共同基础主题带 `owner:` 标记。
    数值外层的 Skeleton 已是 Token owner 透明控件，数值内容保持 Statistic 为 owner。默认渲染不变。

## 2026-08-25

- Fixed
  - Pause TimerStatistic refresh while effectively invisible and recompute from absolute time before resuming.
- Docs
  - Document hidden-page timer behavior and time-drift prevention.

## 2026-08-14

- Semantic Part
  - Publish `root`, `header`, `title`, `content`, `value`, `prefix` and `suffix` for `Statistic`.
  - Keep all six selector targets as static leaf-template nodes and keep the default theme independent from `.semantic-*` selectors.
- API and Theme
  - Add `StrokeDashArray` to `AbstractStatistic` and project the standard root surface through `DashedBorder`.
  - Move the `Statistic` template to its leaf ControlTheme while retaining shared family selectors in `AbstractStatisticTheme`.
  - Let content typography and foreground inherit through the semantic content region so value, prefix, suffix and icons stay aligned.
- Gallery
  - Add the deferred Semantic Part preview and align the semantic styling example with the corresponding public upstream 6.6.0 Statistic demo.
  - Replace React-specific semantic DOM wording with AtomUI owner-scoped Semantic Part terminology.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Statistic`.
  - Align generated output paths with `controls/statistic/index-cn.md` and `controls/statistic/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Statistic desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Statistic desktop architecture documentation under `docs/controls/desktop/data-display/statistic/overview.md`.
  - Add Statistic implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Statistic control-level changelog.
  - Add Statistic Token documentation covering StatisticToken.
