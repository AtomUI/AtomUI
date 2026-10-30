# AutoComplete Changelog

本文档记录 AutoComplete 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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
