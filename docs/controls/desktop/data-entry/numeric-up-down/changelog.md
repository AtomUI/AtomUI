# NumericUpDown Changelog

本文档记录 NumericUpDown 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 NumericUpDown 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-10-18

- Implementation
  - Route the existing floating-handle side-shift animation into a shared input layout panel. The suffix and editor now occupy adjacent layout regions throughout the animation; remove the duplicate suffix transform and hover subscriptions.
  - Derive floating clearance from the handle width and side padding, including Small size, and release side shifts when the handle is hidden.
  - Keep Spinner action buttons square using their actual height, with centered icons and separate divider space.
- Tests
  - Extend existing pointer/layout/state tests to cover clear-button hit areas, editor clearance, handle visibility changes, size changes and square Spinner actions without adding permanent test cases.

## 2026-09-28

- 注册生成器通过真实 type-key 对应的默认主题 TypeMap 条件证明 `NumericUpDownSpinnerTheme.BasedOn` 的保留关系；保持 Application 级 ButtonSpinner 主题替换、content padding、派生模板及局部属性覆盖优先级。

## 2026-09-16

- Behavior
  - Propagate `IsMotionEnabled` from `NumericUpDown` onto `NumericUpDownSpinner` in both mode templates. The spinner control never received the owner motion setting, so the `ButtonSpinnerDecoratedBox` frame fell back to the frame theme's shared-token default and kept its `BorderBrush` / `Background` transitions running even with motion disabled. Besides the motion defect itself, an in-flight `SolidColorBrush` transition outranks the root `BorderBrush` relay that the frame customization relies on, so a root border set on an already-templated control (the documented `styles.root.borderColor` path, and the Gallery semantic styling example) appeared not to apply.

## 2026-08-30

- Behavior
  - Restore the floating-handle hover shift for the suffix group: since the suffix group moved into the spinner content segment (outside the decorated box `ContentRightAddOn` slot), the animated `ButtonSpinnerDecoratedBox.ContentRightShift` no longer reached it through the template. The transform (`translateX`) and its `TransformOperationsTransition` are declared in the ControlTheme behind the `IsMotionEnabled` gate; `NumericUpDown` code only reacts to the decorated box hover state and replaces the `RenderTransform` value, which arms the transition. Re-mirroring the animated property frame by frame was rejected: the per-frame relay fights the render clock and shows up as a one-frame jump ("flicker") when the pointer re-enters quickly. Note: `Transitions` cannot be declared directly inside ControlTemplate content — the template build has no clock attached and throws NRE — so the transition lives in a style setter.
  - Fix `InnerLeftContent` never rendering: the prefix presenter lives in a `ButtonSpinner.InnerLeftContent` property-element subtree where `TemplatedParent` never propagates, so its `TemplateBinding`s stayed dead. The presenter is now named `PART_InnerLeftContentPresenter` and relays `InnerLeftContent` / `InnerLeftContentTemplate` in code, matching the suffix presenter pattern.
  - Consolidate part bindings into the ControlTheme where the template can express them: the clear button's `Icon` / `IsMotionEnabled`, the text box's `IsCustomFontSize`, and the inner-right presenter's `Content` / `ContentTemplate` (already template-bound since the suffix group moved into direct template content) no longer go through code relays. The remaining code relays are exactly the ones the ControlTheme cannot express: the prefix presenter's property-element subtree (dead `TemplatedParent`), and the clear button's visibility from the internal direct property `IsEffectiveShowClearButton`.
  - Move the `clear` button from the suffix addon row into the input segment (right edge of the text area, before the spinner action buttons) in both mode templates; its semantic route simplifies to `/template/ .semantic-clear`.
  - Keep the inner-right content (`suffix` group) attached after the clear button inside the input segment in both mode templates, so `clear`, the user suffix content and the spinner action buttons keep consistent spacing; the `suffix` semantic route simplifies to `/template/ .semantic-suffix`. The Spinner mode adds an 8px inset between the suffix group and the action-button divider, whose content frame has no padding of its own.
  - Relay the owner `BorderBrush` onto the input frame as a local value (mirroring the shared `AbstractTextInput` behavior and antd `styles.root.borderColor`), so application root border customization wins over the frame state machine; clearing the owner value restores the themed border.
  - Fix the Spinner-mode input text riding the top of the frame: `EmbeddedTextBoxTheme` now stretches `PART_InputControlFrame` so the frame follows the text box whenever a host stretches it beyond its natural line height (the frame theme defaults to top alignment); the Input mode is unaffected because its text box already matches its natural height.
- Semantic Part
  - Add Semantic Part descriptors for `NumericUpDown`: `root`（owner）、`prefix`（`AddOnContentPresenter`，`.semantic-prefix`，`ContractType` 为 `ContentPresenter`）、`input`（`EmbeddedTextBox#PART_TextBox`，`.semantic-input`，`ContractType` 为 AtomUI `TextBox`）、`suffix`（内部 `StackPanel`，`.semantic-suffix`）与 `clear`（`InputClearIconButton#PART_ClearButton`，`.semantic-clear`，`ContractType` 为 `Avalonia.Controls.Button`）。所有 Part 均为 `Single`。
  - Fix the `clear` button riding the top of the suffix row: the button template node now pins `VerticalAlignment="Center"` in both mode templates (matching LineEdit) instead of inheriting the `IconButtonTheme` `Top` default, which made the button overshoot the frame top whenever suffix content was taller than the icon.
  - Add static `Classes.semantic-*="True"` markers to both the Input-mode and Spinner-mode templates; `Mode` 切换重建模板子树后仍提供相同 Part 集合。
  - Route `prefix` / `suffix` / `clear` through the spinner scope (`.semantic-scope-spinner`), decorated-box scope (`.semantic-scope-frame`, annotated in `ButtonSpinnerTheme` and `NumericUpDownSpinnerTheme`) and the shared content add-on slots (`.semantic-scope-prefix` / `.semantic-scope-suffix`, annotated in `AddOnDecoratedBoxTheme` and `ButtonSpinnerDecoratedBoxTheme`).
  - Align the Spinner-mode `NumericUpDownSpinner` template to present `InnerLeftContent` / `InnerRightContent` through the decorated box `ContentLeftAddOn` / `ContentRightAddOn` slots, matching the Input-mode structure so both variants share one `SelectorRoute` per part.
  - Keep the spinner action buttons (`PART_IncreaseButton` / `PART_DecreaseButton`) and the floating handle outside the public contract; they are candidates for future compatible part additions.
  - Restore the theme for `SizeTypeAwareIconPresenter` by moving its ControlTheme dictionary under a `Themes/` folder inside `ButtonSpinner` so the theme asset manifest picks it up again; addon `PathIcon`s (for example the `SettingOutlined` right add-on) had been rendering at zero size since the compiled-theme refactor dropped the old `PrimitiveThemes.axaml` include. The hover left-shift behavior of the suffix group is unchanged.
- Gallery
  - Migrate the NumberUpDown ShowCase from `GalleryStickyTabsHost` to `GalleryShowCaseHost` with a lazy Semantic Parts Preview (Input and Spinner mode) and add a custom Semantic Part styling example using the generated `NumericUpDown*Style` types.
- Docs
  - Add `semantic-part.md` and rewrite the stale `root/input/trigger/popup/validation` LLMS semantic table to `root/prefix/input/suffix/clear`.

## 2026-08-23

- Architecture
  - Align NumericUpDown and ButtonSpinner with the shared `InputControlFrame` surface owner.
  - Keep numeric, string-mode and spinner state in NumericUpDown while routing validation, variant, CompactSpace and motion through the shared frame.

## 2026-08-03

- Implementation
  - Extract `NumericUpDownSpinner` as an internal `ButtonSpinner` child control and move the spinner-mode template and action-button visuals into `NumericUpDownSpinnerTheme`.
  - Keep NumericUpDown template parts, `ShowButtonSpinner` visibility, spin handling and rendered behavior unchanged while removing parent selectors that crossed the ButtonSpinner template boundary.

## 2026-07-02

- Implementation
  - Relay inherited `ShowButtonSpinner` into both NumericUpDown templates so input mode floating handles and spinner mode inline action segments respect the user setting.
  - Add regression coverage for `ShowButtonSpinner=false` in input and spinner modes, including runtime visibility changes.
- Docs
  - Document `ShowButtonSpinner` as an inherited NumericUpDown contract consumed by both AtomUI display modes.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `NumericUpDown`.
  - Align generated output paths with `controls/numeric-up-down/index-cn.md` and `controls/numeric-up-down/semantic-cn.md`.

## 2026-06-21

- Docs
  - Align NumericUpDown documentation with `ICustomizableSizeTypeAware`, `CustomizableSizeType.Custom` and `IsCustomFontSize` contracts.
  - Clarify template ownership for spinner mode action buttons and the lifecycle of NumericUpDown relay bindings.
  - Document custom size validation points for `ButtonSpinner`, the inner `TextBox`, floating Handle and spinner actions.
- Implementation
  - Reorder `ICompactSpaceAware` and `IFormItemAware` contract regions after constructors without changing public API.
  - Consolidate template part event subscriptions and relay bindings into paired part setters and `SetupTemplatePartBindings`.
  - Centralize string mode synchronization flag enter/exit paths with paired helpers.

## 2026-06-19

- Docs
  - Add `implementation.md` for NumericUpDown value synchronization, string mode, on-demand template switching, ButtonSpinner integration and maintenance invariants.
  - Refactor `overview.md` to focus on data-entry semantics, public contracts, behavior state, visual theme model and validation entry points.
  - Add NumericUpDown implementation documentation to the Data Entry category entry.

## 2026-06-18

- Docs
  - Establish NumericUpDown desktop architecture documentation under `docs/controls/desktop/data-entry/numeric-up-down/overview.md`.
  - Add dedicated NumericUpDown Token design documentation under `docs/controls/desktop/data-entry/numeric-up-down/token.md`.
  - Introduce per-control changelog under `docs/controls/desktop/data-entry/numeric-up-down/changelog.md`.
  - Document the NumericUpDown composition model based on Avalonia NumericUpDown, ButtonSpinner, TextBox, clear button and AddOnDecoratedBox.
  - Document the string mode, floating Handle and Form integration models.
  - Document NumericUpDown compatibility invariants, template parts, Token scope and validation strategy.
  - Document `NumericUpDownMode` / `Mode=Spinner` design, on-demand template switching, zero-cost default path and spinner mode Token reuse.
- Implementation
  - Add public `NumericUpDownMode` and `Mode` API with `Input` as the default value.
  - Implement `Mode=Spinner` with an independent NumericUpDown template containing inline decrease and increase buttons.
  - Reuse `ButtonSpinner` spin handling for the spinner template so min/max, `AllowSpin`, keyboard and wheel semantics remain shared.
  - Add Gallery API metadata and a NumberUpDown spinner mode showcase.
