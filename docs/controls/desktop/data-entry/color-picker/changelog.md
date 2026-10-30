# ColorPicker Changelog

本文档记录 ColorPicker 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-06

- Design
  - 定义纯色与渐变选择器共享的 `IsNeedConfirm=false` 契约、显式确认优先级，以及宿主提交值与面板候选值的隔离模型。
  - 定义清除可取消、值事件与选择事件时机、渐变对象隔离、外部值重置、配置切换、输入校验及 pinned/lifecycle 边界。
- Docs
  - 增加 [确认与取消设计](confirmation-design.md)，同步总览、实现职责和 Token 边界。
- API
  - 在 `AbstractColorPicker` 实现默认关闭的 `IsNeedConfirm`，纯色和渐变共同继承；开启后显式确认优先于纯色同步策略。
- Behavior
  - 面板草稿与提交值分离，清除可取消；确认只写回一次，普通关闭丢弃草稿，pinned 操作保持打开。
  - 输入确认读取当前有效文本，重置清理无效文本；明确选择编辑默认白色形成非空候选，渐变候选隔离并保留画刷元数据。
  - 外部写入先同步候选再通知观察者，确认操作拥有独立生命周期，回调修改模式不会重复发出选择事件。
- Theme
  - 在面板主题中声明本地化“取消 / 确定”操作区，默认模式不占布局空间；Popup pinned 绑定迁移到宿主 ControlTheme。
- Gallery
  - 增加纯色和渐变的确认示例及绑定值展示，同步中英文、繁体中文和葡萄牙语资源、示例快照及葡萄牙语控件语言包。

## 2026-10-03

- Fixed
  - ColorBlock 的空颜色斜线按最终内容盒定位，排除外部 Margin。

## 2026-09-04

- Behavior
  - Keep the original `CoerceIsPickerOpen` contract: ordinary close requests do not publish a transient closed business state while pinned, while lifecycle closes remain explicitly allowed through `_popupLifecycleCloseDepth`.
  - Keep physical Popup opening code-driven (no `IsOpen` template binding) and retain the `OnApplyTemplate` tail open, so placement, pinned state and dismiss behavior are ready before the host opens.
  - Fix the shared Popup late-actor race: when a lazy popup host raises `Opened` before `PopupMotionActor` attaches, actor readiness now enters the same open-motion path instead of leaving the physically open popup transparent until its second open.
  - Preserve the configured Click/Hover/Focus light-dismiss behavior after unpinning instead of always restoring `true`.
- Docs
  - Record the incident, repository-wide audit, final ownership boundaries and prevention checklist in the long-lived engineering case study `docs/engineering/case-studies/semantic-part-popup-first-open-lifecycle-case-study.md`.
- Tests
  - Add late-motion-actor coverage, keep the no-transient-close assertion, cover Click/Hover/Focus light-dismiss restoration, and add the Gallery first-tab-selection plus hide/show pinned-popup regressions.

## 2026-09-03

- Architecture
  - Publish the ColorPicker and GradientColorPicker Semantic Part contract with five parts (`root`, `body`, `content`, `description`, `popup.root`) aligned one-to-one with the upstream ColorPicker semantic API; see `ColorPicker.SemanticParts.cs`, `GradientColorPicker.SemanticParts.cs` and `semantic-part.md`.
  - Anchor trigger markers in the host templates: `semantic-body` on the trigger `ColorBlock` (`PART_ColorIndicator`) and `semantic-description` on the trigger text (`PART_ColorText`, `PART_ColorTextPanel`); the shared `ColorBlockTheme.axaml` carries the `semantic-content` marker inside the ColorBlock template, so the `content` part declares `CrossNestedOwners=true` and routes through the `semantic-body` anchor.
  - Expose `popup.root` through `ColorPickerPopupRootFrame` (a `Border` subclass) placed as the direct child of `PART_Popup` in the owner template, forwarding `IArrowAwareShadowMaskInfoProvider` to the content-layer `ArrowDecoratedBox` so the semantic border hugs the popup outer edge (matching the upstream popover root): the picker views are created lazily by `CreatePresenter()`, so their template nodes break the owner's `TemplatedParent` chain and neither `/template/` routes nor `>>` descendant steps can reach dynamic popup content under the overlay host, while the shared Popup arrow/shadow-mask machinery only recognizes a direct `Child` implementing `IArrowAwareShadowMaskInfoProvider`, which the forwarding frame satisfies; the frame adds no default visuals.
  - Resolve the `description` contract through an `AvaloniaTextBlock` alias: the `AtomUI.Desktop.Controls` namespace declares its own `TextBlock` that shadows the Avalonia type at generator contract validation.
  - Remove the `IsOpen="{TemplateBinding IsPickerOpen, Mode=TwoWay}"` binding from `PART_Popup` in both picker themes: the binding was evaluated during template inflation (while the Gallery preview had already pinned `IsPickerOpen` to true before the template applied), opening the popup before the anchor was laid out and before the pinned light-dismiss suppression ran, which flipped the placement upward and left a residual interaction-blocking mask. The popup is now driven purely by code in `AbstractColorPicker` — opened after the presenter and suppressions are ready, closed on `IsPickerOpen=false`, with `PopupClosed` writing `IsPickerOpen=false` back on external closes and the tail of `OnApplyTemplate` re-opening a "host open but popup closed" state — aligning with the `AbstractAutoComplete` code-driven dropdown precedent.
- API
  - Promote `AbstractColorPicker.IsPopupPinnedOpen` from internal to public (mirroring `AbstractSelect`) so pinned-open pickers can be declared from XAML, matching the upstream `open` style API; `IsPickerOpen` stays internal.
- Behavior
  - Suppress the light-dismiss mask while the popup is pinned open (aligned with the Select/AutoComplete pinned precedent) so a pinned-open preview no longer intercepts clicks across the whole window. `OnAttachedToVisualTree` restores the pinned business state after reattach, while the end of `OnApplyTemplate` performs the physical open only after relay bindings, event subscriptions and placement inputs are ready.
- Gallery
  - Add the ColorPicker Semantic Parts tab with a pinned-open preview and localized part descriptions (en-US / zh-CN / zh-TW / pt-BR); migrate the showcase host to `GalleryShowCaseHost` per the standard Semantic Part page model.
  - Wrap the Semantic Parts pinned-open preview in a `MinHeight="600"` panel so the pinned popup expands downward instead of flipping up over the Examples/Semantic Parts tabs.
  - Add the "Custom Semantic Part styling" example replicating the upstream `style-class` demo: two pickers (`#1677ff` middle, `#722ed1` large, arrow hidden) styled through `ColorPickerPopupRootStyle` with white and purple popup borders plus the shared border-radius root style.
- Tests
  - Add `ColorPickerSemanticPartTests` covering the descriptor shape for both controls, the template marker inventory, the guard against semantic selectors in default themes, the public pinned-open API surface, and generated-style hits across trigger and overlay popup targets.
  - Migrate `ColorPickerShowCaseExamples.snapshot` to the shared count + SHA256 format used by the other showcase snapshots.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record AbstractColorPicker as the semantic owner for ColorPicker.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-07-06

- API
  - Make `ColorPicker.Value` and `GradientColorPicker.Value` public controlled values with default `TwoWay` binding and Avalonia data validation.
- Behavior
  - Align clear/Form clear behavior so `Value` becomes `null` instead of only clearing trigger visuals.
- Gallery
  - Add a `v6.0.8` value binding example for solid and gradient pickers.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ColorPicker`.
  - Align generated output paths with `controls/color-picker/index-cn.md` and `controls/color-picker/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete ColorPicker desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish ColorPicker desktop architecture documentation under `docs/controls/desktop/data-entry/color-picker/overview.md`.
  - Add ColorPicker implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add ColorPicker control-level changelog.
  - Add ColorPicker Token documentation covering ColorPickerToken.
