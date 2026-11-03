# Statistic Changelog

本文档记录 Statistic 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-01

- 将两 Own 迁移至 DependencyDefaultsV2 revision 1，保留四算法默认、资源身份、key 和 slot；接入实际 owner Global 字体、原颜色和 spacing 角色。
- CountUp 明确自身 Global 字号时生效，未配置保留原共享 Own 字号；原生值和六专用 Semantic Style 继续按优先级覆盖。
- 修复 Statistic/Timer 原生输入换模板后焦点丢失，弱引用与 generation 取消旧回调，保留后来用户焦点和原生内容。
- 修复已结束倒计时重挂或更改刷新间隔后变成正计时；只在 Value 变化时重新分类，复用原 timer 退休与显隐机制。
- 保留原 public Formatter 字段和数据/计时 API；成员整理保持原声明语义与字段初始化顺序。验证、清理和限定视觉结果逐项记入 CT15 分包，完整 CT15 不据此关闭。

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
