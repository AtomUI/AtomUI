# Spin Changelog

本文档记录 Spin 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-14

- Performance
  - Spin indicators use `MotionGate`, which now watches visual ancestors only while the indicator itself is visible; start and stop behavior is unchanged.

## 2026-11-11

- Fix
  - 指示器动画启动期间（含派生类 `StartCustomAnimation` 回调内修改 `MotionDuration` 等属性）发生的重启或停止请求延后到启动状态登记之后执行，`StartCustomAnimation` 与 `StopCustomAnimation` 保持一一配对。
- Design
  - 确认运动 Token 范围：Spin 的 `IsMotionEnabled` 跟随 `EnableMotion`，只控制遮罩过渡；内部 `SpinIndicator` 不接收该开关，关闭全局运动时仍旋转以表达加载中（与 Ant Design 的旋转动画不依赖 motion Token 一致）。

## 2026-11-10

- Theme
  - 控件主题读取自身有效全局 Token，局部覆盖与嵌套已注册控件保持隔离；消费范围见 [Token 设计](token.md#7-有效-token-与作用域边界)。

## 2026-11-07

- API
  - 新增公开枚举 `IndicatorAnimationMode { Rotate, Custom }` 与 `AbstractSpinIndicator.AnimationModeProperty` / `AnimationMode`，默认 `Rotate`。
  - 新增 `protected virtual bool StartCustomAnimation(Visual target)`（返回是否已启动，默认 `false`）与 `protected virtual void StopCustomAnimation(Visual target)`；`RestartIndicatorAnimation()` 由 private 改为 `protected`，供子类在几何变化后重新开始。
  - `Custom` 模式下，基类仍在 `MotionGate` 放行且已加载时启动动画，以模板部件 `PART_IndicatorLayout`（找不到时退回内置或自定义指示器目标）调用 `StartCustomAnimation`，不启动旋转与点透明度动画；`StartCustomAnimation` 返回 `false` 时视为未运行。每次成功启动恰好对应一次 `StopCustomAnimation`（门控关闭、脱离视觉树、模板重应用、模式切换、重启）。
  - `IsIndicatorAnimationRunning` 与 `IndicatorAnimationStarted` 在两种模式下语义相同；`AnimationMode` 运行中改变时先停当前动画再按新模式启动。
- Desktop 影响
  - 默认 `Rotate`，行为不变：内置旋转与圆点透明度动画、门控启停、中心点更新均沿用原实现。
- Tests
  - 新增 `SpinCustomAnimationTests`：默认模式不变、自定义模式门控放行时启动一次、门控关闭时停止、不触发旋转路径、运行标志跟随自定义动画、运行中切换模式、重启先停后启。

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
