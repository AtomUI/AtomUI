# AutoComplete Changelog

本文档记录 AutoComplete 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-04

- Changed
  - 默认 OpenMotion/CloseMotion 改为按样式实例创建的直接主题值，移除默认值回传与几何绑定链；播放准备保留原方向、原点和覆盖优先级。定义不持有控件或运行资源。

## 2026-11-03

- Changed
  - AbstractAutoComplete 声明 OpenMotion/CloseMotion，AutoComplete、AutoCompleteSearchEdit、AutoCompleteTextArea 继承；不在 CompactSpace 子类重复声明。
  - 明确 Motion 纯定义、单向配置和旧宿主释放边界；实际 AXAML 与共享定义的可回收性作为交付门禁。实现与验证状态见共享 Motion 验证页。

## 2026-11-02

- Changed
  - 明确 Select/Picker 对应的上下方 Slide、Mid 时长及实际 Popup 的主题接入责任。见[控件架构](overview.md)与共享动效契约；已完成对应默认规格接入。

## 2026-10-30

- 私有复用输入的Global由实际公开变体提供，正文、光标、选区与状态沿原主题消费；Own及公开子Button保持身份。
- 标准Foreground进入实际正文，Dark根getter对齐原可见颜色；私有owner绑定保留原生Resources覆盖，不设置实例Token字典或私有主题作用域。

- 接通公开owner占位文字与弹层Global颜色，保留标准属性及专用Style优先级。

- 输入本体消费公开owner有效Global字体，保留未配置时的原可见字体模型、普通Style/Local优先级及内部输入身份隔离。
- 多行root标准FontSize与原可见三档预设对齐；默认行框随实际字体自然行距增长，显式FontHeight仍保持精确值。

- 既有Own默认迁移DependencyDefaultsV2 rev1，保留公式、名称/类型和资源ID/slot，无新增Own依赖边。
- 候选行最小高度读取OptionHeight，padding变化参与原有页数预算重算，保留数据、滚动、状态与专用Style优先级。
- 控件成员按契约分区整理，初始化字段顺序与非修复成员语法保持；没有拆分文件或改变API签名。


## 2026-10-04

- Fix
  - Remove inherited ListBox item dividers from the shared candidate dropdown used by all AutoComplete variants.

## 2026-09-05

- Fix
  - The shared `CandidateList` now maintains `IsDefaultEmptyIndicatorVisible`, so an AutoComplete dropdown with an empty or fully filtered option set shows the built-in default `Empty` indicator instead of a blank panel. Note: Ant Design's combobox-mode AutoComplete renders no default empty content; if combobox-parity is preferred, set `IsShowEmptyIndicator=False` on the candidate list theme.

## 2026-09-02

- Architecture
  - Add 9 semantic parts (`root`, `prefix`, `content`, `placeholder`, `input`, `clear`, `popup.root`, `popup.list`, `popup.listItem`) to `AutoComplete`, `AutoCompleteSearchEdit` and `AutoCompleteTextArea`, with cross-nested-owner routes for host input parts and runtime-created popup list items.
- API
  - Promote `IsPopupPinnedOpen` on `AbstractAutoComplete` (and its subclasses) from internal to public, exposing the shared Popup pinned-open mechanism; the semantic parts preview pins the candidate popup open so `popup.*` parts stay highlightable.
  - A pinned popup now suppresses the light-dismiss overlay: `IsLightDismissEnabled` is turned off on the template popup before it opens (and restored on unpin), because a pinned popup ignores dismiss anyway and Avalonia only creates the dismiss overlay at open time.
- Docs
  - Add the [AutoComplete Semantic Part contract](semantic-part.md) covering part routes, contract types, cardinality and customization boundaries.
- Gallery
  - Align the 10 AutoComplete showcase examples with the Ant Design AutoComplete documentation, including a semantic structure style/class example and a Semantic Parts preview tab.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record AbstractAutoComplete as the semantic owner for AutoComplete.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-23

- Architecture
  - Align `AutoCompleteLineEditBox`, `AutoCompleteSearchEditBox` and `AutoCompleteTextAreaBox` with the shared `AbstractTextInput` and `InputControlFrame` layers.
  - Keep candidate, filter, popup and async state in AutoComplete while delegating input-surface status to the shared frame.

## 2026-08-19

- Behavior
  - Unify pointer and keyboard candidate navigation through the shared `CandidateList` active candidate owner.
  - Keep pointer movement non-committing and non-scrolling while ensuring `Enter` commits the single visual active candidate.
- Theme
  - Stop inherited `:pointerover` styling from rendering a second candidate highlight; committed selection remains visually dominant.
- Tests
  - Add popup integration coverage for pointer migration followed by `Enter` commit.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `AutoComplete`.
  - Align generated output paths with `controls/auto-complete/index-cn.md` and `controls/auto-complete/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete AutoComplete desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish AutoComplete desktop architecture documentation under `docs/controls/desktop/data-entry/auto-complete/overview.md`.
  - Add AutoComplete implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add AutoComplete control-level changelog.
  - Add AutoComplete Token documentation covering AutoCompleteToken.
