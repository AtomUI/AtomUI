# ButtonSpinner Changelog

本文档记录 ButtonSpinner 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-03

- Implementation
  - Scope floating-handle hover to the live owner input root and local bounds; reset hover on window leave and detach without changing inline or floating handle geometry.

## 2026-09-16

- Behavior
  - Relay the owner `BorderBrush` onto the shared input frame as a local value, mirroring `NumericUpDown` and the shared `AbstractTextInput` behavior (antd `styles.root.borderColor`). The visible outer border is drawn by the frame, whose state machine owns `BorderBrush`, so before this change an owner-scoped root `BorderBrush` setter (the customization the Gallery Semantic Part example demonstrates) had no effect on the control outline. The relay carries an ownership flag and only clears the frame slot when this control wrote it, so a nested owner that holds its own relay (NumericUpDown, whose spinner is a ButtonSpinner) is not wiped by the inner control.
  - Bind `IsMotionEnabled` onto the `ButtonSpinnerDecoratedBox` frame (`TemplateBinding`); it was previously only propagated to the inner `ButtonSpinnerHandle`. The frame theme defaults this property to the shared motion token, so the frame kept its `BorderBrush` / `Background` transitions running even with motion disabled. This was not only a motion defect: while a `SolidColorBrush` transition is in flight its value outranks the relayed local value, so a root `BorderBrush` assigned on an already-templated control (a running app, or an owner-scoped Style change) appeared not to apply at all. The first version of the relay only appeared to work because the tests assigned `BorderBrush` before template application, where no transition baseline exists.
- Semantic Part
  - Add Semantic Part descriptors for `ButtonSpinner`: `root`（owner）、`content`（帧模板主内容 presenter，`.semantic-content`，`ContractType` 为 `ContentPresenter`）、`innerLeftContent` / `innerRightContent`（帧内容左/右槽 presenter，`.semantic-inner-left-content` / `.semantic-inner-right-content`，`ContractType` 为 `ContentPresenter`）、`actions`（帧内步进手柄 `ButtonSpinnerHandle`，`.semantic-actions`，`ContractType` 为 `TemplatedControl`）、`increaseButton` / `decreaseButton`（手柄模板内 `IconButton`，`.semantic-increase-button` / `.semantic-decrease-button`）。七个 Part 均为 `Single`。
  - 准入依据：上游 `antd@6.6.4` 的 `InputNumberSemanticType` 公开并实际消费 `root` / `prefix` / `suffix` / `input` / `actions` 五个分区键，其中 `actions` 由 `@rc-component/input-number` 应用于包裹上、下步进按钮的容器，对应 AtomUI 的 `ButtonSpinnerHandle`；原排除判定「`InputNumber` 的 handle 属内部区域」的事实前提不再成立。
  - 帧内容槽**不复用** `semantic-prefix` / `semantic-suffix`：`NumericUpDown` 已发布的 `prefix` route 是宽松后代（`/template/ .semantic-scope-spinner >> .semantic-prefix`），而 ButtonSpinner 的帧节点位于 `NumericUpDownSpinner` 子树内；复用会让该 route 命中两个节点，破坏 `NumericUpDown` 的 `Single` 契约。因此改用与自身公开 API 同名的 `innerLeftContent` / `innerRightContent`，`NumericUpDownSemanticPartTests` 作为强制回归护栏。
  - 新增 `ButtonSpinner.SemanticParts.cs` 声明文件，并把 `ButtonSpinner` 声明为 `partial`；标记使用静态 `Classes.semantic-*="True"`，`content` / `innerLeftContent` / `innerRightContent` / `increaseButton` / `decreaseButton` 声明 `CrossNestedOwners`（分别落在 `ButtonSpinnerDecoratedBoxTheme.axaml` 与 `ButtonSpinnerHandleTheme.axaml`），`actions` 按宿主模板本地校验。
  - 单个上、下按钮上游没有独立语义键，属显式能力补充（与 `SplitButton` 的 `primary` / `secondary` 同理）：二者的宿主是模板内部件，不发布则完全不可定制。
- Gallery
  - Migrate the ButtonSpinner ShowCase from `GalleryStickyTabsHost` to `GalleryShowCaseHost` with a lazy Semantic Parts Preview and add a custom Semantic Part styling example using the generated `ButtonSpinner*Style` types.
  - Consolidate the Semantic Parts tab to a **single** preview. The first version shipped two previews (`ButtonSpinner` and `Inline handle`) intended as the floating and inline handle variants, but `IsButtonSpinnerFloatable` defaults to `False` and the first preview never set it, so both rendered the identical default inline handle and duplicated all seven part rows. `ButtonSpinner` has one built-in template, so one preview covers the contract; the page test now asserts exactly one preview so the duplicate cannot return.
  - Add the root border color to both styling example groups (`#1677FF` / `#722ED1`), matching the NumericUpDown example, so the root `BorderBrush` relay is visible in the Gallery.
- Docs
  - Add `semantic-part.md` and rewrite the stale `root/trigger/item/popup/motion` LLMS semantic table to `root/content/innerLeftContent/innerRightContent/actions/increaseButton/decreaseButton`.
  - Record the scope revocation in the rollout design (§2.3 / §2.4) and plan (new Batch 6 task), including the two honest boundaries: upstream still has no standalone spinner owner, and this control publishes `content` / `innerLeftContent` / `innerRightContent` responsibilities rather than copying the upstream key names.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `ButtonSpinner`.
  - Align generated output paths with `controls/button-spinner/index-cn.md` and `controls/button-spinner/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete ButtonSpinner desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish ButtonSpinner desktop architecture documentation under `docs/controls/desktop/navigation/button-spinner/overview.md`.
  - Add ButtonSpinner implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add ButtonSpinner control-level changelog.
  - Add ButtonSpinner Token documentation covering ButtonSpinnerToken.
