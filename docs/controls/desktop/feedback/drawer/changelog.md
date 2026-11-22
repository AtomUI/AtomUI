# Drawer Changelog

本文档记录 Drawer 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-22

- Changed
  - 作用域 Drawer（`OpenOn` 为 null 或指向局部元素）由 `ScopeAwareAdornerLayer`（T3）迁到 `ContentPresentationLayer`（T5）：它位于同宿主的 Watermark、Gallery 吸顶 Tab 和 FloatButton 之上。
  - `OpenOn is TopLevel` 的 Drawer（含 Window Drawer）迁到 `TopLevelPresentationLayer`（T7）：在 Window 中位于全屏标题栏之上，在所有宿主中位于作用域 Dialog 之上、TopLevel Dialog 与 Tour 之下。PopupRoot、EmbeddableControlRoot、浏览器 shell 的 TopLevel Drawer 与 Window 一致。
  - 嵌套 Drawer 的 `OpenOn` 指向父 Drawer 之外时，现在始终复用父 Drawer 所在的层。
  - `OpenOn` 在 T5 与 T7 之间动态迁移时，一次状态迁移中移动 container、交接 chrome suppression lease、释放旧层。
  - Drawer 内控件的键盘焦点描边与 Badge 画在 Drawer 表面之上（`DrawerContainer` 自带表面宿主），不再被 Drawer 自己的表面盖住；Drawer 内的 Watermark、FloatButton、作用域 Dialog 留在 Drawer 表面之内。
- API
  - Drawer 的 public / protected API 不变。
- Docs
  - 层归属改按全局层级阶梯描述，见 [视觉层规范](../../../../architecture/systems/rendering/visual-layers.md)。
- Known limits
  - Windows 10 实机（Gallery，全屏，150% 缩放）只验证了 Window Drawer：Drawer 表面内关闭、取消、提交按钮的键盘焦点描边可见。作用域 Drawer、嵌套 Drawer 与 `OpenOn` 迁移在 Windows 未实测；macOS 与 Linux 未实测，相关证据为 Headless 测试。

## 2026-11-16

- Token
  - 新增 Own Token `EdgeBorderColor` / `EdgeBorderThickness`：贴边一侧的抽屉边框（左侧抽屉画右边框，类 shadcn sheet）；默认透明且厚度 0，Ant Design 渲染不变。厚度按方位组合成单边 Thickness，由 `DrawerContainer` 以 `SetCurrentValue` 写入表面。

## 2026-11-10

- Fixed
  - 运行时覆盖层桥接 Drawer 的局部资源上下文，局部发布即时刷新，关闭/脱离时释放桥接。
- Theme
  - 控件主题读取自身有效全局 Token，局部覆盖与嵌套已注册控件保持隔离；消费范围见 [Token 设计](token.md#7-有效-token-与作用域边界)。

## 2026-10-29

- Performance
  - Hold the Window content cache lease for the whole open and close motion (with or without mask), so the sliding panel and mask composite cached window content instead of re-rendering the page beneath; render-thread median drops from about 8.5 ms to 2.5 ms per frame on the measured Wayland Gallery. Drawers whose layer lies inside the cached content (local scroll or content-control hosts) keep direct rendering.

## 2026-09-10

- API
  - Add the public `IsPinnedOpen` styled property: a pinned drawer ignores mask clicks and the close button so `IsOpen` stays true (semantic preview parity with the upstream controlled-open demo); external `IsOpen` assignments keep closing normally.
- Semantic Parts
  - Publish eight antd-aligned semantic parts (`mask`, `section`, `header`, `title`, `extra`, `body`, `footer`, `close`) plus the implicit `root`; markers are declared statically on the two internal container themes with `CrossVisualRoot` + `RuntimeCreated`.
  - Implement `ISemanticPartCrossRootProvider` on `Drawer`: the live `DrawerContainer` is reported as the cross root and `CrossRootsChanged` fires on container attach, detach and release.
- Infrastructure
  - Parent the `DrawerContainer` logically to the owning `Drawer` while attached to its scope layer (`ISetLogicalParent`, cleared on detach) so owner-scoped generated semantic styles reach container targets; relay `ThemeVariantScope.ActualThemeVariant` from the owner.
- Gallery
  - Replace the sticky host with `GalleryShowCaseHost` and add the Semantic Parts tab: an inline always-open stage (local `ScrollContentPresenter` layer host, the `getContainer={false}` equivalent) with `IsOpen`/`IsPinnedOpen` set declaratively, plus a Semantic Part styling example exercising all eight generated part styles.
- Bugfix
  - Fix semantic-stage clipping under the height-bounded preview mode: the stage used a fixed `Height` while the declared `PreviewStageMinHeight` floor (360) did not cover the stage content's real desired height (320 + 48 margins), so the stage — and the mask/panel inside its scope layer — were cut by the stage viewport while the highlight adorner kept drawing at the unclipped bounds. The stage now uses `MinHeight` + stretch (grows with room, never fights the viewport) and the floor is raised to 434 (viewport = floor − 62 panel overhead must cover 368).
  - Fix a pre-existing tracking-size defect: the adorned-element snapshot took `Math.Max(Bounds, DesiredSize)` while `DesiredSize` includes margin, inflating the container to the host's margin box so the mask and panel bled past the visible host bounds (the semantic preview stage footer was clipped below the stage border). The snapshot now takes the arranged `Bounds` and only falls back to `DesiredSize - Margin` before the first arrange.
  - Fix a pre-existing first-open defect surfaced by scrolled local hosts: `ScopeAwareAdornerLayer` captured the adorned-element tracking snapshot during the layer-injection transient (host `ScrollContentPresenter` content rewrap, extent collapse, offset coerced to zero before recovery), leaving the container translated without scroll compensation until the next open/close cycle. The snapshot now translates directly into layer space (scroll-invariant on the shared content subtree) and re-syncs when the layer itself gets arranged after injection.
- Validation
  - Add `DrawerSemanticPartTests` (descriptor contract, static markers, materialization, logical-parent invariant, generated style hits, pinning, cross-root reporting) and `DrawerSemanticPartHighlightTests` (page-level end-to-end highlighting for all nine cards).
  - Add `DrawerScopeLayerTrackingTests`: first open must track the scrolled host bounds immediately and keep tracking across subsequent scrolling.
  - Add `DrawerSemanticPreviewStageGeometryTests` (theory across window heights): the container, mask and panel must stay inside the visible stage box (margin must not inflate the container; the stage content must not overflow its viewport under the height-bounded mode).

## 2026-08-20

- Architecture
  - Keep Drawer containers in the owning Window `TopLevel` and its `ScopeAwareAdornerLayer`; reserve `WindowDrawnDecorations` overlay for chrome visuals.
  - Share the Window-owned reference-counted drawn chrome suppression lease with Overlay Dialog without sharing container, motion or close state.
- Behavior
  - Keep ComboBox, Select, DatePicker, Tooltip and Flyout content popups visible, interactive and light-dismissible through the owning TopLevel popup layers.
- Lifecycle
  - Remove the Drawer container directly from its scope layer and release host geometry/chrome leases on close or disposal, preserving reopen behavior without retaining the closed visual subtree.
  - Migrate an open Drawer atomically when `OpenOn` changes: move its container to the new scope layer, rebind visible-frame geometry, transfer the Window chrome lease and propagate the effective target to open nested Drawers.
- Validation
  - Reuse the canonical popup entry inventory, primitive matrix and control-family matrix instead of limiting Drawer content coverage to named selector examples.
  - Cover live `OpenOn` migration from Window to local target, between Windows and across nested open Drawers, including old-layer and suppression-lease cleanup.
  - Record Windows and macOS as tested for the final shared layering scheme; Linux X11/Wayland remains untested.

## 2026-07-20

- Architecture
  - Select the drawn decorations Drawer host by actual host capability instead of an OS or CSD gate.
  - Share the Window visible-frame geometry used by frame clipping and Overlay Dialog while preserving Drawer-owned container, motion and nested push lifecycle.
- Behavior
  - Use managed/drawn title-bar space on every platform and in both CSD and non-CSD Window paths while excluding transparent frame-shadow buffers.
  - Keep nested Drawer open/close on the containing scope layer when a drawn decorations overlay has no discoverable TopLevel visual ancestor.
- Docs
  - Document drawn-host fallback, visible-frame sizing and cross-platform maintenance invariants.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Drawer`.
  - Align generated output paths with `controls/drawer/index-cn.md` and `controls/drawer/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Drawer desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Drawer desktop architecture documentation under `docs/controls/desktop/feedback/drawer/overview.md`.
  - Add Drawer implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Drawer control-level changelog.
  - Add Drawer Token documentation covering DrawerToken.
