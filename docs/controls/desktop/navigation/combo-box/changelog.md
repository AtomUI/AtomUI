# ComboBox Changelog

本文档记录 ComboBox 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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
