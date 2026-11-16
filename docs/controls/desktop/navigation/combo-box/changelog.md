# ComboBox Changelog

本文档记录 ComboBox 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-15

- Motion
  - 选中项文字（`Foreground`）与选中背景同时过渡（`MotionDurationMid`，仅 `IsMotionEnabled` 时），实色选中底色下不再先变色。

## 2026-11-14

- Fixed
  - 直接添加 `ComboBoxItem` 子项且下拉项超过视口时，键盘候选向上走回已滚出的区域会跳到列表末尾，向下循环无法回到开头。默认虚拟化面板会隐藏它回收的 `ComboBoxItem` 自身容器，而 `ContainerFromIndex` 仍返回这些项，候选可用性判断把面板隐藏误当作不可见。现在 `IsVisible` 只对面板已实现（有索引）的容器生效，过滤仍按数据项判断。

## 2026-11-11

- Theme
  - 可编辑模式的内部 `ComboBoxTextBox` 设置空 `FocusAdorner`：键盘焦点进入编辑框时不在 Select 外框内部叠画焦点框，焦点仍由外框 `:focus-within` 表达。
- Behavior
  - 清除按钮成为键盘停靠点（Ant Design Select 的清除元素是带本地化名称的按钮）：
    - Tab 顺序为“输入（宿主或可编辑文本框）→ 清除按钮 → 离开”；
    - Enter / Space 清除后，焦点回到宿主或文本框；键盘触发时保持键盘焦点可见状态。
  - 外框 `PART_AddOnDecoratedBox` 与下拉箭头 `PART_OpenIndicatorButton` 不再是停靠点。此前不可编辑模式按一次 Tab 会落在看不见的外框上。
  - 清除按钮只在悬停或焦点在控件内时显示，因此从控件后方 Shift+Tab 先回到输入，再按 Tab 到达清除按钮。
  - 附加内容（`LeftAddOn` / `RightAddOn` / `ContentLeftAddOn` / `ContentRightAddOn`）中的可聚焦控件按视觉顺序参与 Tab，不再被困住：
    - 此前可编辑模式下焦点落到附加内容的按钮后，`OnGotFocus` 又把它转回文本框，Tab 在“文本框 → 清除按钮 → 附加按钮 → 文本框”之间循环，离不开控件；不可编辑模式的顺序是“宿主 → 清除 → 左附加 → 右附加 → 内左 → 内右”。
    - 现在可编辑模式为“左附加 → 内左 → 文本框 → 内右 → 清除 → 右附加 → 离开”。不可编辑模式的输入是宿主本身，排在其内部停靠点之前，为“宿主 → 左附加 → 内左 → 内右 → 清除 → 右附加”。
  - 焦点在清除按钮或附加内容上时用 Alt+Down / F4 打开下拉，焦点移到输入（宿主或文本框）。此前当选中项无法获得焦点（例如禁用，或真实窗口中弹层打开时容器尚未布局）时焦点留在清除按钮上，随后 Enter / Space 先被按钮处理，值被清除而下拉仍开着。
  - 钉住（`IsPopupPinnedOpen`）时每次选择只在查询变化时过滤一次。此前选择提交、文本回填和钉住重开各自重新过滤，一次选择对全部条目调用过滤器多达五遍。
- Implementation
  - `OnGotFocus` 在焦点落到清除按钮时不调用基类：Avalonia 可编辑 ComboBox 会把控件内的任何焦点转给输入文本框。
  - 清除按钮在外框模板中排在文本框之前（DockPanel 顺序），外框设为 `TabNavigation=Local` 的分组（本身不作停靠点），文本框与清除按钮分别为 `TabIndex` 0 和 1。
  - 可编辑模式下宿主 `IsTabStop=False`，避免 Shift+Tab 回到宿主后又被转回文本框。
  - 关闭弹层时只在离场期间有被推迟的刷新、或过滤条件与已呈现的不同时才重新过滤，不再每次关闭都对全部条目调用一次过滤器。
  - 同一判断（`RefreshStaleFilteredItemVisibility`）也用于选择提交结束、`Text` 回填和打开下拉；条目集合、过滤器及过滤值本身的变化仍直接刷新。
  - 外框分组内的 `TabIndex`：内左附加 0、文本框 1、内右附加 2、清除按钮 3，两个内附加的 presenter 各自是 `TabNavigation=Local` 分组；外框为 `Local` 分组时，`AddOnDecoratedBoxTheme` 让左附加 presenter 成为最小 `TabIndex` 的分组、右附加为最大值的分组。
  - `OnGotFocus` 对清除按钮及四个附加 presenter 内的焦点都不调用基类，也不转交文本框。
- Tests
  - `ComboBoxKeyboardNavigationTests` 新增：清除按钮的停靠点、名称与 Enter / Space 清除，两种模式的 Tab 顺序，以及无待应用查询时关闭不调用过滤器。
  - `ComboBoxKeyboardNavigationTests` 新增两种模式下附加内容的 Tab / Shift+Tab 顺序（不困住焦点）。

## 2026-11-10

- Interaction
  - 选中回填与候选过滤投影分离，离场期间保持当前列表自然高度及滚动；物理关闭后应用最新查询，正常输入、钉住和重开保持实时语义。
  - 验证范围和临时测试处置见[过滤离场修复记录](../../../../superpowers/plans/2026-11-10-combobox-filter-close.md)。

- Behavior
  - 实现已有 `IsAllowClear` 的清除交互，覆盖选中值、null 项和未匹配编辑文本；保留双向绑定、过滤复位、输入焦点及弹层状态。
- Theme
  - 后缀区复用 InputClearIconButton，显隐由 ControlTheme 声明；保持原下拉 handle 可用。
  - Small 规格的占位符与选中内容统一纵向 margin，清除后保持输入框高度。
  - 清除与箭头组成独立操作组，内部使用 SpacingXXS；用户后缀和 Form 反馈保留 SpacingXS。
  - 清除入口预留布局槽，Auto Width 在 hover / focus 时保持宽度与箭头位置；关闭清除时不占位。
  - 主题为 `PlaceholderForeground` 提供 `ColorTextPlaceholder` 默认值，修复编辑态占位文字因画刷为空而不绘制的问题。#534
  - 非编辑态占位符改为同样消费 `PlaceholderForeground`，不再以 `Opacity=0.3` 弱化；两种模式的占位颜色与 Select 家族一致，设置 `PlaceholderForeground` 同时作用于两种模式。
- Semantic Part
  - 增加 `clear` 与生成的 `ComboBoxClearStyle`，同步 Gallery 声明式预览与契约文档；既有部件 route 和类型不变。
- Validation
  - 根因、修复前后证据、验证范围及清理见[修复记录](../../../../superpowers/plans/2026-11-10-combobox-clear.md)。

## 2026-11-08

- 主题契约
  - 主题改读 ComboBox 的有效 Global Token；`ComboBoxItem` 以自身 identity 读取 Global Token，候选行颜色与尺寸跟随 ComboBox Own Token；`ItemDisabledColor` 接入禁用候选行前景。见 [ComboBox Token](token.md) §7。
  - 根 `BorderBrush` / `Background` 经输入帧 `HostBorderBrush` / `HostBackground` 投射，移除代码侧 LocalValue 中继。
- Fix
  - 占位文字改用 `PlaceholderForeground`（默认 `ColorTextPlaceholder`），替代 `Opacity="0.3"`；聚焦阴影不再被根裁剪。

## 2026-11-05

- 实现结构
  - 编辑输入模板直接使用 Avalonia `TextPresenter`；保留文本、选区、密码、caret 和编辑绑定契约。

## 2026-11-04

- Changed
  - 默认 OpenMotion/CloseMotion 改为按样式实例创建的直接主题值，移除默认值回传与几何绑定链；播放准备保留原方向、原点和覆盖优先级。定义不持有控件或运行资源。

## 2026-11-03

- Changed
  - ComboBox 自身通过 AddOwner 复用 Popup 的两项注册，不改变 AvaloniaComboBox 继承关系。
  - 明确 Motion 纯定义、单向配置和旧宿主释放边界；实际 AXAML 与共享定义的可回收性作为交付门禁。实现与验证状态见共享 Motion 验证页。

## 2026-11-02

- Changed
  - 明确 Select/Picker 对应的上下方 Slide、Mid 时长及实际 Popup 的主题接入责任。见[控件架构](overview.md)与共享动效契约；已完成对应默认规格接入。

- Design
  - 定义[统一候选空态模型](overview.md#41-候选与空态)：集合为空或有效过滤无可见匹配项时，根据空态开关显示反馈；过滤清理只恢复容器可见性，最终空态由候选刷新流程统一推导。
  - 定义[空态 API](overview.md#31-空态-api)：`IsShowEmptyIndicator`、`EmptyIndicator`、`EmptyIndicatorTemplate`、`EmptyIndicatorPadding`，覆盖开关、默认与自定义内容、内容模板和内边距。
  - 明确现有 `popup.empty` 外层节点、marker、route 和数量语义，以及候选行、弹层布局与交互契约的兼容边界；空态内边距复用 SharedToken `Padding`。
- Behavior
  - 空数据判定进入统一候选刷新流程；清空过滤、退出编辑态、集合清空或替换后均根据当前数据更新空态，空态开关不恢复被过滤的候选行。
- Theme
  - 在既有空态语义外框内声明自定义内容 presenter 与默认 Empty；通过模板绑定消费空态内容、内容模板与内边距，保持列表和空态互斥。
- Gallery
  - 增加默认空态、自定义内容、仅内容模板与关闭空态的示例；自定义空态外框使用 `ComboBoxPopupEmptyStyle` 声明式定制，更新四种语言的语义说明。
- Docs
  - 同步架构、实现、Semantic Part 与 Token 文档，公开 API 由源码和控件文档维护；Gallery 按现行规范只展示示例与 Semantic Parts。
- Validation
  - 修复前后证据、实际验证范围与清理结果统一见[空态修复执行记录](../../../../superpowers/progress/2026-11-02-combobox-empty-state.md)，不将 Headless 或 Skia 结果扩大为原生平台验收结论。

## 2026-10-20

- Fixed
  - `ComboBoxItem` 在构造阶段建立共享初始化会话，初始候选及选中背景直接建立；加载后恢复过渡，回收清理前复位。弹层打开、焦点和后续候选变化仍遵循各自的正常动效语义。

## 2026-09-16

- Docs
  - Publish the ComboBox Semantic Part contract: 12 parts (`root` plus 11 non-root parts — `prefix`, `frame`, `content`, `placeholder`, `input`, `suffix`, `indicator`, `popup.root`, `popup.list`, `popup.listItem`, `popup.empty`), with the generated style types `ComboBoxPrefixStyle`, `ComboBoxFrameStyle`, `ComboBoxContentStyle`, `ComboBoxPlaceholderStyle`, `ComboBoxInputStyle`, `ComboBoxSuffixStyle`, `ComboBoxIndicatorStyle`, `ComboBoxPopupRootStyle`, `ComboBoxPopupListStyle`, `ComboBoxPopupListItemStyle` and `ComboBoxPopupEmptyStyle`.
  - Add `semantic-part.md` as the single source of truth for the ComboBox part contract, node mapping, selector usage and customization boundaries; replace the generator fallback placeholder table in `overview.md` (`root` / `trigger` / `item` / `popup` / `motion`) with the real part table.
  - Document the scope revocation: ComboBox was removed from the exclusion list in the rollout design and the master rollout plan, and added to the third batch. Record that this admission is **not** an Ant Design §2.1 gate pass — upstream 6.6.0 has no public ComboBox owner — and rests on the explicit user instruction plus ComboBox being a self-contained public owner that derives directly from Avalonia `ComboBox`.
  - Record the parts that are deliberately **not** published: the clear affordance (`IsAllowClear` is re-owned from `TextBox` but never implemented in ComboBox — no clear node and no clear logic), the non-editable selected-content node, the Form feedback node, the built-in AddOn regions, `ComboBoxItem` as an independent owner, the `atom:Empty` internals, and the popup host/placement concerns.
- Architecture
  - Document that the marker for `popup.listItem` is injected on the `ComboBoxItem` container instance rather than inside `ComboBoxItemTheme.axaml`, and that it is injected idempotently on both container paths (`CreateContainerForItemOverride` and `PrepareContainerForItemOverride`) because `NeedsContainerOverride` returns `false` for consumer-supplied `ComboBoxItem` instances.
  - Document that `indicator` crosses the nested `ComboBoxHandle` template boundary (`CrossNestedOwners`) and that the popup parts are reached through the template Popup's `TemplatedParent` propagation (`CrossVisualRoot`).
  - Record that `popup.empty` is only visible under an effective filter (`IsEditable` + `IsFilterEnabled` + a filter value + no match): without filtering enabled `IsEffectiveEmptyVisible` is forced to `false`, so an empty candidate set alone does not surface the empty state.
- Feature
  - Publish the `frame` part (`ComboBoxFrameStyle`) for the input frame border box so application code can customize the input border color, width, corner radius and background — the most common styling need for an input control. Its marker is carried by the shared `AddOnDecoratedBoxTheme` on the existing `PART_ContentFrame` node (same shape as ToolTip's `container` / `arrow` in `ArrowDecoratedBoxTheme`), and the generated selector stays owner-scoped because it starts from `Nesting()`.
- Theme
  - Mark `PART_ContentFrame` in the shared `AddOnDecoratedBoxTheme.axaml` with `Classes.semantic-frame="True"`; no node is added or restructured.
  - Project `ContentLeftAddOn` as an `AddOnContentPresenter` element instead of an attribute `TemplateBinding` so the `prefix` part has a marker-bearing node, matching `Select` / `LineEdit` / `InfoPickerInput`.
  - Add the `.semantic-scope-input` and `.semantic-scope-handle` routing anchors to the ComboBox template (both were previously absent); `prefix` / `suffix` reuse the shared `AddOnDecoratedBoxTheme` anchors.
- Gallery
  - Migrate the ComboBox showcase from `GalleryStickyTabsHost` to `GalleryShowCaseHost` and add the Semantic Parts preview plus a "Custom Semantic Part styling" example using the generated `ComboBox*Style` types including the frame border color (11 showcase items, 14 new localization keys in four languages; each xlf grows from 39 to 53 units).
  - Fix the Semantic Parts preview showing a raw type name (`AtomUIGallery.ShowCases.ComboBox.ComboBoxItemData`) in the editable input instead of the committed candidate text. The preview enables `IsEditable` so the `input` part is visible, which makes the input display `ComboBox.Text`; Avalonia derives that from the selected item via `TextSearch.GetEffectiveText`, whose chain is `TextSearch.Text` → `TextSearch.TextBinding` → `DisplayMemberBinding` → `IContentControl.Content.ToString()` → `object.ToString()`. The demo model declared no text representation and is not read through a binding, so every earlier step missed and the fallback rendered the type full name. Declare `ComboBoxItemData.ToString() => Text` on the demo model — the layer where the missing representation belongs — rather than working around it in the preview markup.
  - The preview list previously showed correct row text because `ItemTemplate` drives the dropdown rows while `Text` drives the input; the two presentation paths are independent, which is why only the input was wrong.
- Behavior
  - Fix the pinned popup blocking the whole page: with `IsPopupPinnedOpen` set, a visible `LightDismissOverlayLayer` stayed on top of the window and absorbed every pointer hit except the input frame, so the rest of the page was not interactive. Root cause: Avalonia reads `IsLightDismissEnabled` **only at the moment the popup opens** to decide whether to create the dismiss mask, and the property has no change handler; the ComboBox template opened the popup through `IsOpen="{TemplateBinding IsDropDownOpen, Mode=TwoWay}"`, so a preview that declares `IsDropDownOpen="True"` opened it **during template inflation**, before the control could suppress light-dismiss. Suppressing afterwards cannot remove a mask that is already created.
  - Stop declaring `IsOpen` on `PART_Popup` in `ComboBoxTheme.axaml` and drive the popup opening from code instead, matching the shared `AbstractSelect` convention that `Select` / `Cascader` / `TreeSelect` already use (their templates declare no `IsOpen` binding for exactly this reason).
  - Open after suppressing, not before: `OnApplyTemplate` now relays the pinned state, suppresses light-dismiss, and only then opens the popup if `IsDropDownOpen` is already true. `IsDropDownOpen` changes open and close the popup through dedicated `OpenPopup` / `ClosePopup` helpers.
  - Write popup-initiated closes back explicitly. The previous close write-back was carried by the template's TwoWay `IsOpen` binding, so removing it required a `Popup.Closed` handler that sets `IsDropDownOpen` back to `false`. While pinned the shared `Popup` already suppresses physical closing and re-converges, so that handler never observes a close in this state (verified: `Closed` fires 0 times while pinned, 1 time after unpinning); the pinned branch is a defensive guard that leaves the business state untouched rather than writing it.
  - This closes the ComboBox row of the popup first-open case study, which had already recorded "pinned relay 已存在，但首次打开前没有抑制 light-dismiss" as an open gap and listed a ComboBox suppression test that only asserted the property, never the mask layer.
  - Fix root surface customization for ComboBox after rebasing onto the input-family fix: the visible input frame is drawn by the shared `AddOnDecoratedBox` frame, whose state machine owns `BorderBrush`, so an application-set `ComboBox.BorderBrush` / `Background` previously could not reach the frame at all. Relay both root surface brushes to the decorated frame as `BindingPriority.LocalValue`, with the same takeover flags used by `AbstractSelect.RelayRootSurfaceBrush`, so application-level customization wins over the frame state machine and clearing the property hands it back.
  - This closes the gap the input-family fix left for ComboBox: that commit covered `ButtonSpinner`, `NumericUpDown`, `Select`, `TreeSelect`, `Cascader` and `OtpLineEdit`, but ComboBox derives directly from Avalonia `ComboBox` and is not on the `AbstractTextInput` / `AbstractSelect` inheritance chain, so it was not included.
  - Forward `IsMotionEnabled` from the owner to the `AddOnDecoratedBox` frame in the ComboBox template. Without this wiring the frame's motion transition stays on, and an in-progress `SolidColorBrush` transition outranks the relayed `LocalValue`, silently defeating a live root border customization.
- API
  - Promote `ComboBox.IsPopupPinnedOpen` (and its Avalonia property field) from internal to public, matching `Select` / `AbstractSelect`, `AutoComplete`, `Mentions`, `ColorPicker`, `InfoPickerInput`, `Menu`, `NavMenu`, `DropdownButton`, `SplitButton`, `Tour` and `FlyoutHost`, including the standard XML documentation. This lets the Gallery pin the template popup the standard way.
  - Note for maintainers: `PopupPinnedOpenContractTests.Direct_Popup_Pinned_Open_Contracts_Are_Internal` lists ComboBox and asserts the contract must be internal, but that theory and `Leaf_Controls_Inherit_The_Contract_From_Their_Semantic_Owner` execute zero cases (their `MemberData` yields no data), so the assertion was never enforced; several other types in the same list are already public. The test was left untouched.
- Tests
  - Add `ComboBoxSemanticPartTests` (descriptor, static markers, generated style hits, container marker stability across recycle, consumer-supplied container, editable switching, template reapplication, and a custom frame border brush overriding the themed warning-state border), `ComboBoxRootSurfaceRelayTests` (root `BorderBrush` / `Background` customization wins over the frame hover / rest state machine, clearing restores the state machine, a value set before template application is still relayed, and an uncustomized ComboBox keeps the themed rest state), `ComboBoxPinnedPopupOverlayTests` (pinned open with `IsDropDownOpen` already true before template apply leaves no visible dismiss mask and a distant point still hits the page, unpinned keeps the mask, popup self-close writes back `IsDropDownOpen`, pinning after open keeps the popup open, unpin restores the template default, and pin-then-unpin-then-reopen stays consistent) and `ComboBoxSemanticPartHighlightTests` (12 preview cards, every part marker present, popup pinned with light-dismiss suppressed, a committed candidate rendering its text — not its type name — in the editable input, and real card-hover highlighting for trigger-side and popup-side parts).
  - Both new assertion layers were verified to fail against the unfixed implementation, not merely to pass against the fixed one: restoring the template `IsOpen` binding makes the mask assertion fail, and additionally disabling that assertion makes the hover-count assertion fail too.
  - Supersede the earlier Gallery observation. While the popup is pinned the dismiss mask used to absorb card hover — the control now suppresses light-dismiss before the popup opens, so preview cards highlight on real hover again and the degraded "assert by marker presence only" workaround is retired. `OverlayInputPassThroughElement` still points at the input frame, which is what keeps the input clickable while the popup is open.

## 2026-09-04

- Behavior
  - Apply pinned light-dismiss suppression before the first physical Popup open and restore the template default after unpinning.
- Tests
  - Cover a pin request made before template materialization, including physical open state and unpin restoration.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record ComboBox as the semantic owner for ComboBox.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-23

- Behavior
  - Route Form validation through `FormStatus` and the shared input-frame effective-status pipeline without overwriting explicit `Status`.
- Architecture
  - Pair the external Form feedback status subscription with logical attach/detach so a reused ComboBox reconnects without requiring template reapplication.
- Theme
  - Forward `FormStatus`, focus-within and native validation from `ComboBox` to its decorated input frame.
- Docs
  - Link ComboBox to the shared input-control architecture and document the validation-state ownership boundary.

## 2026-08-19

- Behavior
  - Make pointer movement and keyboard navigation update the same internal active candidate without committing `SelectedItem`.
  - Keep pointer migration non-scrolling and make `Enter` commit the current visual candidate.
- Theme
  - Neutralize inherited pointer-over candidate styling and preserve selected-item visual precedence.
- Tests
  - Add mixed pointer/keyboard navigation and pointer-to-`Enter` commit regression coverage.

## 2026-08-18

- Behavior
  - Keep keyboard input on a non-editable ComboBox when reopening its popup so consecutive `Down` and `Enter` selections continue to update the selected item.

## 2026-08-17

- Theme
  - `AddOnDecoratedBox` 的 outline 变体默认背景改为 `ColorBgContainer`，与 Ant Design Select/Input outlined 变体的默认容器背景一致（此前为透明，落在彩色背景上会透出底色）；filled、borderless、disabled 等其余变体背景不变。Calendar Header 的年/月选择器由此获得白色容器背景，直接使用标准 `ComboBox`，无需派生控件或使用点样式。

## 2026-07-05

- Behavior
  - Use `SelectedItem` as the ComboBox Form value for `SetFormValue`, `GetFormValue`, and `ClearFormValue`.
  - Add non-editable selected content overflow tooltip support through `IsShowOverflowTip`, `OverflowTipDelay` and `OverflowTipPlacement`.
  - Align non-editable selected content overflow tooltip against the outer `AddOnDecoratedBox` instead of the padded inner presenter.
- Gallery
  - Add a `v6.0.8` `SelectedItem` binding example showing the ViewModel/Form value contract.
- Docs
  - Document the `SelectedItem` Form value owner in ComboBox overview and implementation notes.
  - Document the non-editable selected content overflow tooltip boundary.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ComboBox`.
  - Align generated output paths with `controls/combo-box/index-cn.md` and `controls/combo-box/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete ComboBox desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish ComboBox desktop architecture documentation under `docs/controls/desktop/navigation/combo-box/overview.md`.
  - Add ComboBox implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add ComboBox control-level changelog.
  - Add ComboBox Token documentation covering ComboBoxToken.
