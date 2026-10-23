# Cascader Changelog

本文档记录 Cascader 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-10-23

- 根表面几何接入 owner 有效主题值，保留默认渲染和原生覆盖优先级。
- 根几何进入实际输入 frame；多选 Custom 的默认紧凑 inset 与显式 inset 分开，标签内在高度保持。现有多选尺寸/前缀参数继续引用 Select 家族资源。

## 2026-10-18

- Behavior
  - Use data-owned active paths and same-column sibling navigation; activate branches without entering their child column, support RTL and root-level return/close.
  - Open with Down/Enter/Space, submit only an explicit valid candidate, close after permitted parent selection, and toggle subtree checks for multi-select without closing.
  - Preserve valid search candidates by full path, include selectable parent paths, and keep IME/caret editing separate from candidate commands.
- Implementation
  - Complete Enter down/up cycles and retain the inline search input while rebuilding selected tags, preserving focus and preventing repeated commits.
- Gallery
  - Connect both external search examples to the shared keyboard implementation and coordinate search versus confirmation.

## 2026-10-04

- Fix
  - Remove inherited ListBox item dividers from Cascader search-result dropdown options.

## 2026-09-16

- Behavior
  - Relay the owner root `BorderBrush` / `Background` onto the `CascaderAddOnDecoratedBox` frame as local values, so an owner-scoped root setter reaches the visible outline instead of being silently ignored. The frame theme's state setters outrank a `TemplateBinding`, so the relay must use `BindingPriority.LocalValue` from code; it carries an ownership flag and only clears a slot when this control wrote it.
  - Bind `IsMotionEnabled` onto the `CascaderAddOnDecoratedBox` frame (`TemplateBinding`); it was previously only propagated to the inner `SelectHandle`.

## 2026-09-05

- Theme
  - Apply the reused SelectToken `MultiModePrefixIndent*` values as the extra left `Margin` of `PART_ContentLeftAddOn` in `CascaderAddOnDecoratedBoxTheme` for `IsMultiple=True` with a non-empty selection, aligning the multiple-mode prefix with the single-mode horizontal padding (same fix as Select / TreeSelect).

## 2026-09-03

- Architecture
  - Publish the Cascader Semantic Part contract with thirteen parts (`root`, `prefix`, `content`, `placeholder`, `input`, `suffix`, `clear`, `item`, `itemContent`, `itemRemove`, `popup.root`, `popup.list`, `popup.listItem`) aligned with the Ant Design Cascader semantic structure; see `Cascader.SemanticParts.cs` and `semantic-part.md`.
  - Resolve multiple-selection tag parts through the shared tag mechanism: `item` markers are injected when `SelectTagAwareTextBox` creates tag containers, and `itemContent` / `itemRemove` markers live in `TagTheme.axaml` (`SelectTag : Tag`); the generator's cross-nested validation now chains through a declared RuntimeCreated sibling part's ContractType.
  - Anchor trigger-part markers in the host template (`CascaderTheme.axaml`) projected into `CascaderAddOnDecoratedBox`, reusing the shared `AddOnDecoratedBoxTheme` scope anchors for `prefix` / `suffix`.
  - Publish the inline prefix presenter against the `ContentLeftAddOn` public API via a `$parent[atom:Cascader]` compiled binding: template-inflated property-value subtree nodes carry no `TemplatedParent`, so a `TemplateBinding` there resolves to nothing and the prefix falls back to the boxed outer add-on rendering.
  - Validate the `clear` part across the nested owner boundary (`CrossNestedOwners=true`): the physical clear button marker lives in the shared `SelectHandleTheme.axaml`.
  - Keep the cascade menu columns and items runtime-created: `CascaderView` injects `popup.list` markers on child level lists, and `CascaderViewLevelList` / `CascaderViewFilterList` inject `popup.listItem` markers at container creation.
- API
  - Promote `AbstractSelect.IsPopupPinnedOpen` from internal to public (mirroring `AbstractAutoComplete`) so pinned-open popups can be declared from AXAML.
  - Align the pinned-popup light-dismiss mechanism with `AbstractAutoComplete` at the `AbstractSelect` level: suppress the mask in `OnPropertyChanged` and `OnApplyTemplate` before any open, drive template-applied opens through `OpeningDropDown`, sync `IsDropDownOpen=false` on `PopupClosed` (covering light-dismiss closes), and drop the Cascader template's `IsOpen` template binding so the popup is opened purely by code after the suppression.
- Behavior
  - Make the dropdown arrow indicator color customizable through the `suffix` semantic part style: `SelectHandleTheme` now derives the indicator brush from the inherited `TextElement.Foreground`, and the shared `AddOnDecoratedBoxTheme` supplies `ColorTextQuaternary` as the suffix area default, so setting `TextElement.Foreground` on `CascaderSuffixStyle` recolors the arrow while unstyled controls keep the previous quaternary appearance. The mechanism applies to every `SelectHandle` host (Cascader, Select, TreeSelect).
  - Paint the `Filled` variant background across the full border box (`AddOnDecoratedBoxTheme.axaml` switches the content frame to `OuterBorderEdge` for `StyleVariant=Filled`): the variant keeps a 1px transparent border as height compensation, so the previous `InnerBorderEdge` fill rendered 2px shorter than the `Outlined` frame (30px instead of 32px at Middle). The fix applies to every control sharing `AddOnDecoratedBox` (Cascader, Select, LineEdit, AutoComplete, date/time pickers).
- Gallery
  - Add the Cascader Semantic Parts tab with a pinned-open preview and localized part descriptions; migrate the showcase host to `GalleryShowCaseHost` per the standard Semantic Part page model.
  - Add the "Custom Semantic Part styling" example (aligned with the upstream semantic styling demo): object-style and variant-conditioned Cascader part styles covering prefix / placeholder / popup root / popup list item.
- Tests
  - Add `CascaderSemanticPartTests` and `CascaderPinnedPopupTests` covering descriptor shape, template marker inventory, generated style hits, popup part resolution and marker stability across reopen and source reset.
  - Add `AddOnDecoratedBoxVariantTests` asserting the `Filled` frame paints `OuterBorderEdge` while `Outlined` keeps `InnerBorderEdge`, and both variants stay 32px tall at Middle.
  - Add `SelectHandleIndicatorColorTests` covering the arrow indicator default (`ColorTextQuaternary` on Cascader and Select) and the `suffix` semantic style override reaching the presented icon.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record AbstractSelect as the semantic owner for Cascader.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-23

- Architecture
  - Align Cascader and `CascaderAddOnDecoratedBox` with the shared `InputControlFrame` surface owner.
  - Keep cascade, filter, popup and option state in Cascader while reusing shared input status and Form/native validation semantics.

## 2026-08-19

- Behavior
  - Make pointer movement and keyboard navigation share one active candidate in both ordinary tree columns and filtered path results.
  - Preserve hover-triggered expansion while excluding disabled, hidden, and loading nodes from active candidate ownership.
  - Make `Enter` commit the filtered or tree candidate represented by the single active visual state.
- Theme
  - Remove independent pointer-over candidate highlighting while preserving expanded and selected visual precedence.
- Tests
  - Add mixed pointer/keyboard and pointer-to-`Enter` coverage for tree and filtered modes.

## 2026-07-05

- API
  - `SelectedOption` 和 `SelectedOptions` 默认 binding mode 调整为 `TwoWay`，并启用 Avalonia data validation。
  - 继承 `IsShowOverflowTip`、`OverflowTipDelay` 和 `OverflowTipPlacement`，用于单选路径和多选 tag 溢出提示。
- Implementation
  - `SelectedOptions` 支持 `INotifyCollectionChanged` 原地集合变化，同步刷新 tag 展示、选中计数、空状态、Form value 和内部 CascaderView 勾选状态。
  - 内部 CascaderView 选择同步改为差量勾选 / 取消，避免外部集合变化时先清空再重选。
  - 单选路径文本、过滤态路径显示和多选 tag 接入共享 `OverflowTip` behavior，只在视觉溢出时使用 `ToolTip`，并支持配置提示位置。
  - 单选路径和过滤输入的溢出 tooltip 以外层 `CascaderAddOnDecoratedBox` 为定位基准，避免按内部文本 padding 对齐。
- Gallery
  - 新增 `SelectedOption / SelectedOptions` 双向绑定示例，标记 `v6.0.8`。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Cascader`.
  - Align generated output paths with `controls/cascader/index-cn.md` and `controls/cascader/semantic-cn.md`.

## 2026-06-23

- Docs
  - 新增 Cascader 控件文档集，记录控件定位、数据节点模型、级联展开、选择 / 勾选、异步加载、过滤、Token 和兼容性不变量。
  - 记录绑定型选项模型边界，明确 `CascaderOption` 保持轻量数据对象定位。
  - 补齐 Cascader 设计文档中的外层 public API、内部 CascaderView 事件、template part、伪类、单选 / 多选状态流、过滤、默认路径、Form、Token 使用和分层验证策略。
  - 在 Data Entry 分类入口登记 Cascader 文档。
- API
  - 新增 `BindableCascaderOption`，用于选项属性需要作为 Avalonia binding target、`DynamicResource` 或 scoped resource host 的场景。
- Implementation
  - `CascaderViewItem` 接入绑定型选项的 owner resource host attach/release、属性同步和 checked / expanded 反写。
  - `CascaderViewLevelList` 在容器准备和清理路径管理绑定型选项生命周期，回收容器时先释放订阅再清空容器值。
- Tests
  - 新增绑定型选项 parent 维护、容器同步、资源宿主优先级和 WeakReference 生命周期覆盖。
