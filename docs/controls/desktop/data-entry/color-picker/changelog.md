# ColorPicker Changelog

本文档记录 ColorPicker 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-25（待完整家族验收）

- Fix：历史GradientColorPicker.SetPresetColor的Func委托通过强类型适配写入既有Action附加属性，修复非空formatter赋值时的ArgumentException；保持公开签名、null与样式优先级。支持重复更新和复用输出控件，清除后恢复默认文字。

- Theme
  - 空值斜线和色谱选择环消费有效Global线宽，明确0不绘制；斜线按明确ColorError消费有效颜色，未明确提供和撤回配置时保持历史红色2 DIP。宿主与弹层View身份隔离，Own字段、资源编号及Semantic Part不变。
  - 渐变节点文字颜色改由声明式样式更新，修复同一选择下前景、禁用及作用域变化仍保留旧画刷；正常文字跟随 owner 或 description 样式，活动节点沿用弱化语义。
  - 新增节点与文字恢复时重放当前选择；formatter 本地颜色遵循 Avalonia 优先级。公开 API、Token、Part 名称和尺寸不变。
  - 24个Own字段改用依赖默认计划，修复小号handler、预设色块及轨道覆盖未传到下游；名称、类型、slot、资源编号及六类配置的144条默认记录保持一致，明确下游和0值优先级保留。
  - 接通Alpha输入宽度与数字输入按钮宽度，Alpha未明确配置时保留历史48 DIP，明确0与删除配置均有独立语义；默认按钮16、通道宽48及默认输入容器262不变；明确加宽的Alpha通过扩展输入行容纳，避免通道重叠。
- Fixed
  - ColorSlider后台位图遵循最新请求，修复较早颜色或尺寸任务晚完成覆盖当前结果；同一实例离树改色后重新挂载时重新计算位图。
  - ColorSpectrum后台图及同步fast路径遵循同一请求顺序，避免旧像素覆盖新尺寸／范围；使用同一分量与归一范围提交map元数据，修复倒置／等值范围的异常或NaN选择环及单像素映射。共享增量helper保留min为唯一值的范围约定，不改变公开Components默认值。
  - 输入区回放离树期间的外部颜色／格式变化；没有外部变化时保留草稿、校验与光标，外部A→B→A仍覆盖旧草稿。格式选择改为ControlTheme中的原生编译绑定，保持公开Format双向更新和旧模板解绑。
  - 输入重挂切片20项临时检查通过并删除，清理后23项、NET8零警告及成员审计通过；Mac managed／NativeAOT各308项。预期文字／确认状态及审查表内59对限定Headless合成差异已获批准，原生最终像素及完整家族继续验收。
  - 恢复两个View的基类属性回调，使Alpha隐藏／禁用作用到实际输入与滑块；隐藏格式选择器保持当前格式。
  - 色谱绘制子层不再将旧视口尺寸反馈到自然测量，修复加宽配置撤回后面板只增不减。
  - 格式选择保留双向模板绑定，修复首次切换后公开Format无法继续切到HSV或恢复HEX。
- Verification
  - C7切片26项临时功能、有效亮暗514项及Mac managed／NativeAOT各184项通过；140帧及28组1DIP、独立直接Skia／公开输入对照完成，所示修复与表内41对限定合成差异获用户批准。临时宿主／源码／媒体已清理，最终Release聚焦23项、NET8零警告／错误及成员审计通过；原生最终像素、既有键盘可达性及完整家族仍开放。
  - CT19 B08 的定向检查、真实 Mac managed／NativeAOT 两类弹层各66项通过；临时源已删除，清理后20项相邻检查和NET8零警告通过。所示颜色变化已获用户明确视觉批准，不代表完整 Own、输入、状态或原生像素验收。
  - Own默认切片的14项定向检查、六配置144条兼容记录、Mac managed648／AOT676检查及清理后20项／NET8通过。指定配置的布局变化和审查表内限定Headless合成差异已获用户明确批准，B08结果不代替本项验收。
  - 输入与状态恢复切片22项临时验证通过，原版宽度、格式、Alpha及尺寸残留反例已重现；Mac managed／NativeAOT各1844项（各36次框架原始指针点击）及清理后20项／NET8通过。预期布局／状态变化和审查表内15个限定合成场景已获用户明确批准；原生最终像素及完整家族未验收。

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
