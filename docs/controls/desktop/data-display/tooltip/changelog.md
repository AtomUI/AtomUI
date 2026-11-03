# Tooltip Changelog

## 2026-10-31

- 保留六Own原值/key/type/slot并迁有效默认；MotionDuration记为历史保留，继续复用原Global动效入口。
- 接通真实文字的ToolTip Global/原生字体与前景、跨根scope资源发布、打开期间宽度/颜色变化和清除恢复。
- 重套模板退役旧part绑定，关闭后不反向保留旧宿主；保留原内容、箭头、关闭和滚动合同。

## 2026-10-24

- OverflowTip 按文本实际可见矩形判断祖先裁剪，修复 presenter 宽于裁剪输入区域时漏掉提示；动态裁剪与变换会重新计算，卸载时释放祖先订阅。

## 2026-10-23

- `arrow`语义部件的共享`ArrowIndicator`支持`TipRadius`，通过`ToolTipArrowStyle`声明尖角或圆角，默认保持1.5 DIP。

本文档记录 Tooltip 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-04

- Changed
  - 默认 OpenMotion/CloseMotion 改为按样式实例创建的直接主题值，移除默认值回传与几何绑定链；播放准备保留原方向、原点和覆盖优先级。定义不持有控件或运行资源。

## 2026-11-03

- Changed
  - ToolTip 实例公开 OpenMotion/CloseMotion；字符串 Tip 创建的实例由主题提供默认。OverflowTip 复用实际 ToolTip，不重复注册，也不新增宿主 Motion 附加属性。
  - 明确 Motion 纯定义、单向配置和旧宿主释放边界；实际 AXAML 与共享定义的可回收性作为交付门禁。实现与验证状态见共享 Motion 验证页。

## 2026-11-02

- Changed
  - 明确 ZoomBig 使用 Fast 时长、alignment/箭头 Token 原点与实例/宿主来源规则。见[控件架构](overview.md)与共享动效契约；已完成对应默认规格接入。

## 2026-10-03

- Implementation
  - Compare input-root identities in `ToolTipService`, allowing a window to close synchronously during routed pointer dispatch before the service receives the same raw event.
  - Distinguish native Tooltip input roots from overlays sharing the owner root, preserving same-window host switching and same-timestamp native-root transitions.
  - Release tracked hosts and pending show timers on visual-tree detach, reject stale hits, and preserve desired `IsOpen` state for reconciliation after reattachment.
- Tests
  - Replace three visual-root helper assertions with real synchronous-close, Overlay host-switching and detached pending-host lifetime coverage; keep the existing native-root timestamp regression without increasing the permanent test count.

## 2026-09-17

- Implementation
  - Make `IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()` nullable and degrade every ToolTip provider method to arrowless fallbacks when the template has not been applied or the theme does not provide `PART_ArrowDecorator`, removing the `Debug.Assert`-masked contract violation that crashed popup shadow configuration and custom placement with a `NullReferenceException` (issue #477).
  - Keep `DeferSetupArrowDecoratedBox` subscribed while the applied template still lacks the arrow part, so arrow setup completes once a later template application provides the part.
  - Mirror the same null tolerance in `MenuFlyoutPresenter`, matching its already null-safe `GetMask*` methods.
- Tests
  - Add `ToolTipArrowDecoratedBoxContractTests` covering unstyled-provider degradation, opening with a template missing `PART_ArrowDecorator`, arrow-binding recovery when a later template provides the part, and the default-theme arrow wiring.

## 2026-09-16

- Implementation
  - Move `ToolTipService` back to `Tooltip/ToolTipService.cs` and declare its Package Core registration ownership with `[AotTrimUnit(AotTrimGeneralUnits.Core)]`, replacing the retired `PackageCore/` directory boundary; the service no longer depends on a dedicated shared directory for unit attribution. This records the historical registration implementation; the adopted replacement contract is [TypeMap registration](../../../../architecture/foundations/aot-typemap-registration.md).
- Docs
  - Update the implementation source index to the new `ToolTipService` location and record the annotation as a maintenance invariant.

## 2026-09-07

- API
  - Expose `root` / `container` / `arrow` Semantic Parts on `ToolTip`, aligned with the upstream antd Tooltip semantic DOM.
  - Declare `container` and `arrow` with `CrossNestedOwners=true`, routing through the `.semantic-scope-arrow-decorated-box` scope anchor into the shared `ArrowDecoratedBox` template; `arrow` is `Optional`.
- Theme
  - Add the `.semantic-scope-arrow-decorated-box` scope anchor to the `ArrowDecoratedBox#PART_ArrowDecorator` node in `ToolTipTheme.axaml`.
  - Add `.semantic-container` / `.semantic-arrow` markers to `Border#PART_ContentDecorator` / `ArrowIndicator#PART_ArrowIndicator` in the shared `ArrowDecoratedBoxTheme.axaml`.
- Docs
  - Add `semantic-part.md` as the single source for the Tooltip Semantic Part contract.
  - Re-scope overview's semantic region summary to `root` / `container` / `arrow` and link the new contract; document the descriptor↔marker mapping in implementation.
- Gallery
  - Add a Semantic Parts tab to the Tooltip showcase with a `SemanticPartPreview` and localized part descriptions.
  - Center the Semantic Parts preview `ToolTip` (`HorizontalAlignment="Center"`) so the pill hugs its content instead of stretching to `ToolTipMaxWidth`.
  - Add a `Custom Semantic Part styling` example (`SemanticPartStyleTitle` / `SemanticPartStyleDescription`) that customizes `container` via the generated `ToolTipContainerStyle` and hides `arrow` via `ToolTip.IsArrowVisible=False` on the trigger buttons, aligned with the antd `style-class` demo.
- Fix
  - The popup shadow now follows the visible container `Border` radius instead of the `ArrowDecoratedBox`'s own `CornerRadius`: overriding only `ToolTipContainerStyle`'s `CornerRadius` previously left the shadow mask at `ToolTipCornerRadius`, exposing white page corners behind the rounded pill.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record ToolTip as the semantic owner for Tooltip.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-19

- Architecture
  - Define `ToolTip.IsOpen` as a declarative desired-open state reconciled against `Tip` readiness and host visual-tree attachment, replacing edge-triggered open-on-set semantics.
  - Establish a single idempotent reconciliation entry as the only path that changes the physical popup state; `ToolTipService` and other callers only write `IsOpen`.
  - `Tip` not being ready no longer resets `IsOpen`; only `ToolTipOpening` cancellation and external popup close write `IsOpen` back to `false`.
  - Host detach closes the physical popup while preserving `IsOpen`; re-attachment reopens through reconciliation.
- Implementation
  - Implement `ReconcileOpenState` in `ToolTip` as the single idempotent entry, driven by `IsOpen`/`Tip` changes and a self-limited one-shot host `AttachedToVisualTree` subscription.
  - Defer popup-closed convergence to after the current detach/attach pass, distinguishing lifecycle closes from external closes by actual host attachment state.
- API
  - Add `TextWrapping` (default `Wrap`) and `TextTrimming` (default `None`) attached properties controlling tip text layout within the maximum width constraint; they propagate to `PART_ContentPresenter` through live bindings while the popup is open.
  - Define the tip-instance customization model: when `Tip` is a `ToolTip` instance, presentation attached properties explicitly set on the instance take precedence over host values, unset ones fall back to the host; lifecycle attached properties stay host-owned.
- Theme
  - `PART_ContentPresenter` declares `TextBlock.TextWrapping="Wrap"` so long tip text wraps within `ToolTipMaxWidth` instead of being clipped.
- Docs
  - Document the ToolTip attached-property contract surface and the `ToolTipOpening`/`ToolTipClosing` routed events in overview.
  - Correct the `ToolTipService` source location in the implementation source index.
  - Document the tip text layout contract in overview and clarify the wrap semantics of `ToolTipMaxWidth` in token.md.

## 2026-08-03

- Architecture
  - Replace OverflowTip's dependency on TextBox visual descendants with an internal text viewport metric published by the input owner.
  - Define reactive viewport updates, template reapply cleanup and the compatibility fallback for third-party Avalonia TextBox templates.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Tooltip`.
  - Align generated output paths with `controls/tooltip/index-cn.md` and `controls/tooltip/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Tooltip desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Tooltip desktop architecture documentation under `docs/controls/desktop/data-display/tooltip/overview.md`.
  - Add Tooltip implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Tooltip control-level changelog.
  - Add Tooltip Token documentation covering ToolTipToken.
