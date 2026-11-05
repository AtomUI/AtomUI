# Spin Changelog

## 2026-11-04

- SpinIndicator 的 Custom 外框/点径支持普通 Style 接管并投射到真实图元，保留原动效与三档默认。

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


## 未发布：custom-theme CT15

- 七个 Own 默认迁至 DependencyDefaultsV2 revision 1，保留四算法28默认及原资源身份。
- Spin/独立 SpinIndicator 接入可选字体和有效 Global；组合 Tip/圆点/Mask/Spacing 使用实际 Spin owner。
- 自定义模板单独存在也选择自定义路径；caller PathIcon 尺寸投射可释放，保留原生尺寸与普通 Style。
- 原生内容重模板恢复焦点，保留后续焦点优先和原内容/滚动合同。
- 复用既有持续动画、有效显隐及退休机制；限定视觉、平台和完整家族结论按 CT15 结果记录，不据此关闭家族。


## 未发布：CT15 精确循环补充

- 修正内置/自定义指示器整周期沿用上一旋转角度。
- 恢复内置首点初始0.3及各点循环初值，保留原Alternate曲线、错峰延时、时长与有效显隐/退出机制。
- 正常曲线、周期与有意边界变化分别取证；原46对静态批准及其他未通过边界保持，完整家族/CT15不据局部验证关闭。
