# Mentions Changelog

本文档记录 Mentions 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 Mentions 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-11-14

- Fixed
  - `IMentionOption.IsEnabled=false` 的候选项此前按启用项渲染，键盘导航（视口内外）会落到它上面，打开时的预选也可能选中它并被 Enter 提交。现在禁用项呈现禁用视觉，预选取第一个启用项，键盘、指针悬停与点击都跳过禁用项。Mentions 通过 internal `CandidateList.ItemEnabledResolver` 把选项启用状态交给共享 CandidateList，AutoComplete / Select 行为不变。
  - 候选列表超过视口、虚拟化回收容器后，键盘活动候选的高亮会落到错误的行上，滚回后活动候选不高亮。修复位于共享的 CandidateList，详见 [ListBox Changelog](../../data-display/list-box/changelog.md)。

## 2026-11-11

- Behavior
  - `Mentions` 宿主 `IsTabStop` 默认 `False`（与 AutoComplete 一致），键盘停靠点只有内部文本域，不再多出一个没有视觉的停靠点。
- Fixed
  - 清除按钮（内部 TextArea 的停靠点）按 Enter / Space 或被点击清除后，焦点回到文本域；此前按钮随文本清空隐藏，焦点丢失。

## 2026-11-10

- Theme：终端 Own 与实际 owner 对齐，共享计算不传播其他控件的覆盖；字段迁移与保留范围见 [Token 设计](token.md)。


- Theme
  - 候选列表和条目透传 Mentions owner；内部输入编辑器跟随 Mentions，独立 TextArea 保持自身 identity；候选部件读取宿主 globals，不借用 ListBox Own。

## 2026-11-05

- 实现结构
  - `input` Semantic Part 的实际节点与已公开的 Avalonia `TextPresenter` 契约统一，选择器路由和专用 Style 类型保持一致。

## 2026-10-04

- Fix
  - Remove inherited ListBox item dividers from the shared Mentions candidate dropdown.

## 2026-09-05

- API
  - Promote `IsPopupPinnedOpen` from internal to public, aligning with Select and AutoComplete so the Gallery Semantic Parts preview can pin the candidate Popup open and expose the `popup.*` parts.
- Architecture
  - Publish the Mentions Semantic Part contract with nine parts (`root`, `prefix`, `content`, `placeholder`, `input`, `clear`, `popup.root`, `popup.list`, `popup.listItem`) aligned with the Ant Design Mentions semantic structure; see `Mentions.SemanticParts.cs` and `semantic-part.md`.
  - Reuse the AutoCompleteTextArea cross-nested owner routing for the input region: `MentionTextArea` carries the `.semantic-scope-input` anchor, and `prefix` / `content` / `placeholder` / `input` / `clear` markers live in the shared `TextAreaTheme` and `TextAreaDecoratedBoxTheme`.
  - Add the missing `.semantic-scope-prefix` anchor to the shared `TextAreaDecoratedBoxTheme` so the TextArea-based prefix part can route through the shared scope chain, matching `AddOnDecoratedBoxTheme` and `LineEditTheme`.
  - Keep the popup triple-key structure (`popup.root` static `PopupFrame`, `popup.list` static `CandidateList`, `popup.listItem` runtime-injected at `CandidateList` container creation) identical to AutoComplete.
- Tests
  - Add `MentionsSemanticPartTests` covering descriptor shape, template marker inventory, default-theme non-consumption, generated style hits and popup part marker exposure.
- Fix
  - The shared `CandidateList` now maintains `IsDefaultEmptyIndicatorVisible`, so a Mentions dropdown with no matching options shows the built-in default `Empty` indicator instead of a blank panel, matching Ant Design Mentions' default `notFoundContent` rendering. The `EmptyIndicator` / `EmptyIndicatorTemplate` / `IsShowEmptyIndicator` / `EmptyIndicatorPadding` public properties remain unwired for Mentions itself (see overview limitation note).

## 2026-09-04

- Behavior
  - Apply pinned light-dismiss suppression before the first candidate Popup open and restore the template default after unpinning.
- Tests
  - Cover a pin request made before template materialization, including physical open state and unpin restoration.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record Mentions as the semantic owner for Mentions.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-23

- Architecture
  - Align `MentionTextArea` with the shared `AbstractTextInput` logic layer and `InputControlFrame` input-surface layer.
  - Keep trigger, candidate, popup and async state in Mentions while reusing the shared validation and Form status pipeline.

## 2026-08-19

- Behavior
  - Unify pointer and keyboard candidate navigation through the shared `CandidateList` active candidate owner.
  - Keep the current mention trigger context intact while `Enter` inserts the single visual active candidate.
- Theme
  - Stop inherited `:pointerover` styling from rendering a second candidate highlight; committed selection remains visually dominant.
- Tests
  - Add trigger-popup integration coverage for pointer migration followed by mention insertion.

## 2026-07-06

- API
  - Make `Value` a default `TwoWay` Form value and keep validation integrated with Avalonia `DataValidationErrors`.
- Gallery
  - Add a `v6.0.8` `Value` binding example.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Mentions`.
  - Align generated output paths with `controls/mentions/index-cn.md` and `controls/mentions/semantic-cn.md`.

## 2026-06-21

- Docs
  - Establish Mentions desktop architecture documentation under `docs/controls/desktop/data-entry/mentions/overview.md`.
  - Add Mentions implementation documentation covering trigger detection, candidate popup, filtering, async loading, option insertion, Form integration and lifecycle boundaries.
  - Add Mentions Token documentation for `PopupContentPadding`, `OptionHeight` and `MinPopupWidth`.
  - Add Mentions changelog and link the document set from the Data Entry category entry.
