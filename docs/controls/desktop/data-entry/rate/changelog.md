# Rate Changelog

本文档记录 Rate 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-04

- Fixed
  - 指针在区域外释放、禁用、卸载及模板重建时清除按下状态，避免后续外部开始的手势误提交评分；输入同时遵循祖先禁用状态。

## 2026-10-03

- Fixed
  - Rate 输入按事件所属视觉根、最终 Bounds 和实际命中判断归属；修复 Margin、缩放、跨窗口与遮罩下的误判。RateCharacter 按实际排列盒绘制。

- Implementation
  - Scope rating pointer input to its live presentation source, convert positions before hit testing, and release temporary press/hover state on exit and detach; foreign-window clicks cannot write back Value.

## 2026-07-06

- Behavior
  - Make `Rate.Value` default to TwoWay binding and enable Avalonia data validation.
  - Keep user rating, Form value and ViewModel state on the same `Value` source of truth.
- Gallery
  - Add a `v6.0.8` Rate binding example showing user selection and ViewModel updates staying synchronized.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Rate`.
  - Align generated output paths with `controls/rate/index-cn.md` and `controls/rate/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Rate desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Rate desktop architecture documentation under `docs/controls/desktop/data-entry/rate/overview.md`.
  - Add Rate implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Rate control-level changelog.
  - Add Rate Token documentation covering RateToken.
