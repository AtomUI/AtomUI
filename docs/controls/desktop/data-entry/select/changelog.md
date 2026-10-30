# Select Changelog

本文档记录 Select 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-10-04

- Fix
  - Remove inherited ListView item dividers from Select dropdown options while preserving the standard ListView divider contract.

## 2026-09-16

- Behavior
  - Relay the owner root `BorderBrush` / `Background` onto the `SelectAddOnDecoratedBox` frame as local values, so an owner-scoped root setter reaches the visible outline instead of being silently ignored. The frame state machine owns those two property slots, and the frame theme's `:focus-within` / `[IsInnerBoxHover]` setters outrank a `TemplateBinding`, so the relay must use `BindingPriority.LocalValue` from code. The relay carries an ownership flag and only clears a slot when this control wrote it.
  - Bind `IsMotionEnabled` onto the `SelectAddOnDecoratedBox` frame (`TemplateBinding`). It was previously only propagated to the inner `SelectHandle`, so the frame kept its `BorderBrush` / `Background` transitions on even with motion disabled; a `SolidColorBrush` transition then keeps its value ahead of the relay's local value and the root customization appeared not to apply.

## 2026-09-05

- Fix
  - Restore the built-in empty indicator for the dropdown: `SelectCandidateList.ConfigureEmptyIndicator()` now maintains `IsDefaultEmptyIndicatorVisible` like the `ListView` base, so an empty or fully filtered candidate list shows the default `Empty` (`PresetImage=Simple`, localized No data) instead of a blank panel, matching Ant Design's default `notFoundContent`.
  - Wire `AbstractSelect.IsShowEmptyIndicator`, `EmptyIndicator` and `EmptyIndicatorTemplate` to the candidate list in `EnsurePopupContent()`; a custom empty indicator is honored and hides the default one. `EmptyIndicatorPadding` stays unwired (same as TreeSelect) so the theme per-size padding remains effective.
- Theme
  - Add `MultiModePrefixIndent` / `MultiModePrefixIndentSM` / `MultiModePrefixIndentLG` tokens and apply them as the extra left `Margin` of `PART_ContentLeftAddOn` in `SelectAddOnDecoratedBoxTheme` for Multiple / Tags mode with a non-empty selection.
  - The content frame keeps the small multi-mode left padding for the tag area while the prefix (ContentLeftAddOn) aligns with the single-mode horizontal padding, matching Ant Design's selector-padding + prefix-margin model.
- Tests
  - Add headless tests for the empty dropdown indicator (default shown, custom honored, `IsShowEmptyIndicator=false` respected).
  - Add headless layout tests asserting Multiple / Tags prefix left inset equals the Single mode inset, and a SelectToken unit test asserting `MultiModePadding*.Left + MultiModePrefixIndent*.Left == SingleModePadding*.Left` per size.

## 2026-09-04

- Architecture
  - Publish the Select Semantic Part contract with thirteen parts (`root`, `prefix`, `content`, `placeholder`, `input`, `suffix`, `clear`, `item`, `itemContent`, `itemRemove`, `popup.root`, `popup.list`, `popup.listItem`) aligned with the Ant Design Select semantic structure; see `Select.SemanticParts.cs` and `semantic-part.md`.
  - Anchor trigger-part markers in the host template (`SelectTheme.axaml`) projected into `SelectAddOnDecoratedBox`, reusing the shared `AddOnDecoratedBoxTheme` scope anchors for `prefix` / `suffix`.
  - Resolve multiple-selection tag parts through the shared tag mechanism: `item` markers are injected when `SelectResultOptionsBox` creates tag containers, and `itemContent` / `itemRemove` markers live in `TagTheme.axaml` (`SelectTag : Tag`); the generator's cross-nested validation chains through the declared RuntimeCreated `item` sibling part's ContractType.
  - Keep the popup root static while injecting the candidate list at runtime: `popup.root` is the static `PopupFrame` border child of `PART_Popup`, `popup.list` markers are injected in `EnsurePopupContent`, and `popup.listItem` markers are injected at `SelectCandidateList` container creation.
  - Resolve `PopupFrame` from `Popup.Child` at point of use instead of caching it in a field: template re-application in a pinned-open state inflates a fresh `Popup` whose static frame becomes the new `Popup.Child`, so a cached reference previously pointed at the stale frame and rejected the candidate list as "already has a parent".
- Gallery
  - Add the Select Semantic Parts tab with a pinned-open preview and localized part descriptions; migrate the showcase host to `GalleryShowCaseHost` per the standard Semantic Part page model.
  - Add the "Custom Semantic Part styling" example (aligned with the upstream semantic styling demo): object-style and variant-conditioned Select part styles covering prefix / placeholder / suffix / popup root / popup list item.
- Tests
  - Add `SelectSemanticPartTests` covering descriptor shape, template marker inventory, default-theme non-consumption, generated style hits, tag part style hits, inline prefix presentation and popup part marker exposure.
  - Update `SelectShowCasePageTests` and `SelectShowCaseExamples.snapshot` for the migrated host, added example and semantic preview template.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record AbstractSelect as the semantic owner for Select.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-23

- Architecture
  - Align Select input surfaces with the shared `InputControlFrame` / `AddOnDecoratedBox` composition.
  - Keep selection, candidate, popup and result state in Select while using the shared EffectiveStatus and Form/native validation pipeline.
  - Pair `SelectHandle` Form feedback observation with logical attach/detach so reused Select input surfaces reconnect deterministically.

## 2026-08-19

- Design
  - Generalize `candidate-interaction-design.md` from the Select-specific implementation into the shared contract for Select, AutoComplete, Mentions, ComboBox, and Cascader candidate lists.
  - Record the explicit boundary for TreeSelect, DatePicker/Calendar, and TimePicker state models so their tree selection, range preview, focus, and current-value semantics are not replaced by list hover suppression.
- Tests
  - Align family-level verification with pointer-to-keyboard, keyboard-to-pointer, `Enter`, disabled candidate, lifecycle, and projection invariants.

## 2026-08-18

- Design
  - Define a single active candidate shared by pointer movement and keyboard navigation across `Single`, `Multiple` and `Tags` modes.
  - Separate active candidate, committed selection and Avalonia pointer hit state so the visual target and `Enter` commit target remain identical.
  - Define pointer hot-path, keyboard scrolling, virtualization projection, lifecycle invalidation and selected-visual precedence invariants.
- Docs
  - Add `candidate-interaction-design.md` and synchronize the Select overview and implementation maintenance boundaries.

## 2026-07-05

- API
  - Add `IsShowOverflowTip`, `OverflowTipDelay` and `OverflowTipPlacement` to the Select family input surface for selected result overflow tooltips.
- Theme
  - Wire overflow tooltip behavior and placement to single selected text and multiple selected tags through the shared `OverflowTip` attached behavior.
  - Align single selected text overflow tooltip against the outer `SelectAddOnDecoratedBox` instead of the padded inner text node.
- Docs
  - Document the overflow tooltip API, template boundary and lifecycle ownership.

## 2026-07-03

- Docs
  - Document the root ownership model for Tags dynamic options: user option sources, runtime dynamic options and effective candidate options are separate state layers.
  - Clarify that Tags runtime options must not be written into user `OptionsSource` or XAML child `Options`, preserving the ItemsSource contract.
  - Add verification expectations for `Mode=Tags` with `OptionsSource` and custom tag creation.
- Implementation
  - Move Tags runtime options into Select-owned internal state and bind the candidate list to effective options built from user options plus runtime options.
  - Preserve selected runtime tags across `OptionsSource` replacement and remap them to formal user options when an identity match appears.
- Tests
  - Add regression coverage for `Mode=Tags` with `OptionsSource`, custom tag creation and source replacement remapping.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Select`.
  - Align generated output paths with `controls/select/index-cn.md` and `controls/select/semantic-cn.md`.

## 2026-06-21

- Docs
  - Add Select control documentation set with architecture overview, implementation notes, Token design and changelog.
  - Document Select mode, option source, selection, filtering, async loading, popup lifecycle, Form / CompactSpace integration and compatibility invariants.
  - Add Select to the Data Entry control documentation index.
- Implementation
  - Reorder `Select` and `AbstractSelect` so control-owned public/protected API members stay before explicit interface regions, with private implementation methods after those contract regions.
  - Consolidate internal drop-down state restoration and candidate selection sync suppression into paired helper paths.
  - Move stable Select template part state binding to AXAML compiled ancestor binding; keep C# `BindUtils.RelayBind` only for AddOnDecoratedBox to SelectHandle hover / pressed sibling part coordination.
- Tests
  - Add Select behavior coverage for option replacement selection preservation, default value selection, Form value mapping and candidate sync suppression recovery.
  - Add Select template binding coverage for AXAML-owned right add-on / count / handle state and the remaining AddOnDecoratedBox hover / pressed relay binding.
