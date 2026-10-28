# Popup Changelog

本文档记录 Popup 控件级 API、Theme、Token、实现结构和设计契约变化，不替代仓库根目录 `CHANGELOG.md`。

## 2026-10-27

- Motion
  - `OpenMotion` / `CloseMotion` 使用新的 `Motion` 定义，时长通过播放请求覆盖，不再修改定义对象。
  - 实际表面改为 `PopupSurface : Panel`，去除 Actor 包装；显示前准备使用可释放的属性覆盖。
  - 入场替代离场保持当前值，关闭 completion 与排队 Close 均核对请求版本和表面身份。
  - Direct Popup 同时支持 visual attach 就绪，不依赖 rooted logical owner。

## 2026-10-03

- Fixed
  - native Popup 使用统一阴影内槽测量/排列，尊重 Padding；上/左箭头 frame 保留 Child 原点偏移。

## 2026-09-28

- native/overlay 翻转预测和 CSD 可见区域约束统一使用扣除 Child Margin 后的有效定位尺寸。
- 补充非对称 Margin、零 Margin、水平/垂直边缘、CSD 和 native 屏幕坐标预测回归，保留原有 shadow/motion 宿主结构。
- 删除无调用的 open-state flag 和 closing 解绑反射封装，保留实际使用的生命周期桥接。

## 2026-09-18

- 将 pin 对 light-dismiss 的有效值计算收敛到共享 Popup，保留原始配置与绑定。
- 运行中同步当前 Popup 会话的遮罩 registration，不重建 host，不通过隐藏共享遮罩影响其他弹层。

## 2026-09-17

- Design
  - Document `IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()` as a nullable contract: providers may transiently or permanently lack the arrow part, and consumers must degrade to arrowless behavior instead of assuming a non-null result (issue #477).
- Implementation
  - `ShadowsAwareContainer` degrades to `IsArrowVisible=false` and default corner radius when the provider cannot supply the arrow part, subscribes the provider's `TemplateApplied` while degraded, and re-probes on the next template application so arrow relay bindings are established once the part becomes available.
  - `Popup.ApplyCustomPlacement` treats an unavailable arrow part like a hidden arrow, reusing the existing shadow-thickness compensation path instead of dereferencing the part.

## 2026-09-07

- Fix
  - `ShadowsAwareContainer` and `AbstractArrowDecoratedBox.GetMaskCornerRadius` now derive the shadow mask radius from the visible content decorator (`PART_ContentDecorator`) instead of the `ArrowDecoratedBox`'s own `CornerRadius`; a semantic-part style that only overrides the container `Border` radius (for example `ToolTipContainerStyle`) no longer leaves white corner gaps between the surface and the popup shadow.
- Tests
  - Add regressions for the mask-radius contract and for `ShadowsAwareContainer` corner-radius tracking of arrow-decorated children.
## 2026-09-04

- Motion
  - Reconcile both `Opened -> PopupMotionActor ready` and `actor ready -> Opened` orderings so a lazily materialized popup cannot remain physically open but transparent until its second open.
- Pinned open
  - Suppress light-dismiss before first pinned open in shared Flyout, ComboBox, Mentions and InfoPickerInput owners; Flyout restores its configured Click/Hover/Focus value through `configured && !pinned` rather than a fixed default.
- Tests
  - Add a late-actor visual-state regression and first-open light-dismiss regressions for each newly covered direct owner.
- Docs
  - Record the incident and repository-wide audit in the long-lived [Semantic Part Popup first-open lifecycle case study](../../../../engineering/case-studies/semantic-part-popup-first-open-lifecycle-case-study.md).

## 2026-08-25

- Design
  - Define the internal-only pinned-open contract across semantic owners, Flyout adapters, and the physical Popup.
- Implementation
  - Gate pinned opening on content, anchor, attachment, effective visibility/enabled state, TopLevel, and placement validity.
  - Reject ordinary close sources without starting close motion, while lifecycle teardown clears hosts, pending requests, bindings, subscriptions, trackers, and timers.
  - Keep an already open Popup open after unpinning, but cancel and clean a not-yet-open pending request without transient business-state close notifications.

## 2026-08-24

- Lifecycle
  - Separate ordinary animated close from placement-target and logical-owner teardown; invalid popup sessions now close synchronously instead of retaining an Avalonia host without a valid anchor.
  - Require an animated close to preserve every owner established for the current open session: an existing logical owner must remain attached, while the effective PlacementTarget must remain attached, transformable and owned by the captured TopLevel. Direct Popup sessions that started without a logical owner continue to animate against an explicit valid PlacementTarget.
- Motion
  - Replace independent close-motion flags with the shared internal `AtomUI.MotionScene.MotionExecutionState` flow (`Idle` / `Pending` / `Playing` / `Completing`), coalesce repeated close requests and clear motion/session state from the `Closed` boundary.
- Verification
  - Add lifecycle regressions for placement-target detach, Popup logical detach, cross-TopLevel target replacement and ordinary animated close behavior.

## 2026-08-21

- API
  - Add optional `Popup.SurfaceBackground` with a `null` default; a non-null brush explicitly selects a host-owned surface.
- Architecture
  - Extend the shared `ShadowsAwareContainer` frame renderer to draw an optional surface and the existing shadow for both native `PopupRoot` and `OverlayPopupHost`.
  - Keep `PopupRoot.Background=null` so native transparent window composition and shadow buffers remain unchanged.
- Compatibility
  - Keep Direct Popup, Flyout, ToolTip, ContextMenu, menu, selector, Picker, Tour and ColorPicker surfaces content-owned through the shared primitive default.
  - Remove redundant per-consumer `SurfaceBackground=null` declarations, keep the Direct Popup host transparent, and make its regression-demo Child own the visible surface.
- Verification
  - Add primitive, runtime-family and source-inventory contracts; record Windows/macOS as tested and Linux X11/Wayland as untested.
