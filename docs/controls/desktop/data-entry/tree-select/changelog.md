# TreeSelect Changelog

本文档记录 TreeSelect 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-11-04

- Changed
  - 默认 OpenMotion/CloseMotion 改为按样式实例创建的直接主题值，移除默认值回传与几何绑定链；播放准备保留原方向、原点和覆盖优先级。定义不持有控件或运行资源。

## 2026-11-03

- Changed
  - 从 AbstractSelect 继承 OpenMotion/CloseMotion，控制根弹层，不改变 TreeView 内容展开。
  - 明确 Motion 纯定义、单向配置和旧宿主释放边界；实际 AXAML 与共享定义的可回收性作为交付门禁。实现与验证状态见共享 Motion 验证页。

## 2026-11-02

- Changed
  - 明确 Select/Picker 对应的上下方 Slide、Mid 时长及实际 Popup 的主题接入责任。见[控件架构](overview.md)与共享动效契约；已完成对应默认规格接入。

## 2026-10-28

- MinPopupWidth 默认求值迁移到 DependencyDefaultsV2、声明修订 1；保留默认 300、类型、资源身份与显式覆盖/撤回合同。

- 共享手柄箭头尺寸与反馈区间距分别消费TreeSelect有效Global.IconSize/SpacingXS，保留默认14/8、原颜色/清除流程及专用Suffix Style优先级。

- 共享内部标签布局与数量文字读取 TreeSelect 实际 owner 的有效 Global.SpacingXXS/ColorTextDescription，保留默认值及原选择、删除流程。


## 2026-10-23

- 根表面几何接入 owner 有效主题值，保留默认渲染和原生覆盖优先级。
- 根几何进入实际输入 frame；多选 Custom 的默认紧凑 inset 与显式 inset 分开，标签内在高度保持。现有多选尺寸/前缀参数继续引用 Select 家族资源。


## 2026-10-19

- Behavior
  - Keep TreeSelect Form value notifications tied to the final selected nodes when the popup TreeView replaces its `ItemsSource`; temporary deselection and replay no longer produce duplicate notifications. Compare selected and checked nodes by instance identity so an equal-valued source replacement updates the owner to the new node. Ignore selection and checked events from a closed popup tree, then restore the owner selection when it reopens.

## 2026-09-16

- Behavior
  - Relay the owner root `BorderBrush` / `Background` onto the `TreeSelectAddOnDecoratedBox` frame as local values, so an owner-scoped root setter reaches the visible outline instead of being silently ignored. The frame theme's state setters outrank a `TemplateBinding`, so the relay must use `BindingPriority.LocalValue` from code; it carries an ownership flag and only clears a slot when this control wrote it.
  - Bind `IsMotionEnabled` onto the `TreeSelectAddOnDecoratedBox` frame (`TemplateBinding`); it was previously only propagated to the inner `SelectHandle`.

## 2026-09-05

- Architecture
  - Publish the TreeSelect Semantic Part contract with thirteen parts (`root`, `prefix`, `content`, `placeholder`, `input`, `suffix`, `clear`, `item`, `itemContent`, `itemRemove`, `popup.root`, `popup.list`, `popup.listItem`) aligned with the Ant Design TreeSelect semantic structure (popup tree node internals are covered by the `TreeViewItem` contract); see `TreeSelect.SemanticParts.cs` and `semantic-part.md`.
  - Anchor trigger-part markers in the host template (`TreeSelectTheme.axaml`) projected into `TreeSelectAddOnDecoratedBox`, reusing the shared `AddOnDecoratedBoxTheme` scope anchors for `prefix` / `suffix`, and make `PopupFrame` a static template child of `PART_Popup` instead of a runtime-created border.
  - Resolve multiple-selection tag parts through the shared tag mechanism: `item` markers are injected when `SelectTagAwareTextBox` creates tag containers, the multiple-mode search input carries the `semantic-input` marker, and `itemContent` / `itemRemove` markers live in `TagTheme.axaml` (`SelectTag : Tag`).
  - Inject `popup.list` markers when `EnsurePopupContent` creates the candidate tree and `popup.listItem` markers in the `TreeViewSelectTreeViewItem` constructor, covering every nesting level and container recycle.
- Theme
  - Apply the reused SelectToken `MultiModePrefixIndent*` values as the extra left `Margin` of `PART_ContentLeftAddOn` in `TreeSelectAddOnDecoratedBoxTheme` for `IsMultiple=True` with a non-empty selection, aligning the TreeSelect multiple-mode prefix with its single-mode horizontal padding.
  - Fix the multiple-selection non-filter cursor style that targeted the nonexistent `SelectAddOnDecoratedBox` type inside the TreeSelect template, so `IsMultiple=True` with `IsFilterEnabled=False` now shows the hand cursor as the single-selection variant already does.
- Gallery
  - Add the TreeSelect Semantic Parts tab with a pinned-open multiple-selection preview and localized part descriptions; migrate the showcase host to `GalleryShowCaseHost` per the standard Semantic Part page model.
  - Add the "Custom Semantic Part styling" example (aligned with the upstream semantic styling demo): object-style and variant-conditioned TreeSelect part styles covering prefix / suffix / popup root.
- Tests
  - Add `TreeSelectSemanticPartTests` covering descriptor shape, template marker inventory, default-theme non-consumption, generated style hits, tag part style hits, multiple-mode search input marker, inline prefix presentation and popup part marker exposure.
  - Update `TreeSelectShowCasePageTests` and `TreeSelectShowCaseExamples.snapshot` for the migrated host, added example and semantic preview template.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record AbstractSelect as the semantic owner for TreeSelect.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-23

- Architecture
  - Align TreeSelect and `TreeSelectAddOnDecoratedBox` with the shared `InputControlFrame` surface owner.
  - Keep tree selection, filtering and popup state in TreeSelect while reusing shared input status and Form/native validation semantics.

## 2026-07-05

- API
  - Make `SelectedItem` and `SelectedItems` default to `BindingMode.TwoWay` and enable Avalonia data validation.
- Implementation
  - Subscribe to `SelectedItems` `INotifyCollectionChanged` sources while attached and refresh tags, count, Form value notifications, max-count state, and popup TreeView selection / checked items on collection mutation.
- Gallery
  - Add a `v6.0.8` binding example that demonstrates single and multiple TreeSelect selection binding.
- Tests
  - Add TreeSelect selection binding regression coverage for default binding mode, data validation metadata, collection mutation, and checkable mode.

- API
  - Add inherited `IsShowOverflowTip`, `OverflowTipDelay` and `OverflowTipPlacement` support for selected result overflow tooltips.
- Theme
  - Wire single selected text and multiple selected tags to the shared `OverflowTip` attached behavior, including tooltip placement.
  - Align single selected text overflow tooltip against the outer `TreeSelectAddOnDecoratedBox` instead of the padded inner text node.
- Docs
  - Document the overflow tooltip API and template ownership boundary.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `TreeSelect`.
  - Align generated output paths with `controls/tree-select/index-cn.md` and `controls/tree-select/semantic-cn.md`.

## 2026-06-21

- Docs
  - Add TreeSelect control documentation set with architecture overview, implementation notes, Token design and changelog.
  - Document TreeSelect tree data, selection, checkable mode, filter strategy, popup lifecycle, Form / CompactSpace integration and compatibility invariants.
  - Add TreeSelect to the Data Entry control documentation index.
- Implementation
  - Reorder `TreeSelect` so control-owned public/protected API members stay before the Form interface region, with private implementation methods after the contract region.
  - Move stable TreeSelect right add-on, count and handle state binding to AXAML compiled ancestor binding.
  - Keep C# `BindUtils.RelayBind` only for AddOnDecoratedBox to SelectHandle hover / pressed sibling part coordination.
- Tests
  - Add TreeSelect template binding coverage for AXAML-owned right add-on / count / handle state and the remaining AddOnDecoratedBox hover / pressed relay binding.
