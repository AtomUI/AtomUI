# SearchEdit Changelog

本文档记录 SearchEdit 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-15

- Documentation
  - Replace the placeholder semantic contract in `overview.md` with the real five Selector Parts published by
    `SearchEdit.SemanticParts.cs`: `root`、`prefix`、`input`、`suffix`、`clear`、`button`. The previous table listed
    generic placeholder entries (`input` / `trigger` / `popup` / `validation`) that did not correspond to any declared
    descriptor, and the LLMS generator projected that table verbatim into `controls/search-edit/semantic-cn.md`.
  - Record the routing anchors (`semantic-scope-input-frame`、`semantic-scope-prefix`、`semantic-scope-suffix`) and
    clarify that `semantic-content` / `semantic-placeholder` are non-Part routing/visual helper markers, not public
    contract.
- API documentation
  - Correct two public-surface descriptions that did not match the source: `SearchButtonText` is typed `object?`
    (not `string`), and the inherited inner-content entry points are `InnerLeftContentTemplate` /
    `InnerRightContentTemplate` (there are no `InnerLeftContent` / `InnerRightContent` properties on the
    `LineEdit` base).

## 2026-09-01

- API
  - Rename the public enum `SearchEditButtonStyle` to `SearchButtonType`; values (`Default`, `Primary`) and the `SearchButtonStyle` property are unchanged. The old name collided with the generated `AtomUI.Theme.Styling.SearchEditButtonStyle` Semantic Part style class under the shared `atom:` XML namespace, which crashed the XAML compiler (AVLN2000) whenever both were referenced from XAML.

## 2026-08-27

- API
  - Follow the shared input family change: the standard root `Background` / `BorderBrush` now reach `InputControlFrame` through the `AbstractTextInput` relay with inline style semantics, and the SearchEdit theme no longer carries dead owner-level defaults. The full family contract is recorded in the LineEdit family changelog.

## 2026-08-23

- Architecture
  - Align SearchEdit with the shared `AbstractTextInput` logic layer and `InputControlFrame` surface layer.
  - Make `SearchEditDecoratedBox` a search-button layout extension rather than an independent input-surface status owner.
- Theme
  - Route SearchEdit border, background, variant, effective status, CompactSpace and motion through the shared frame theme.
  - Keep the public Button search part as a consumer of the frame effective status, not a validation owner.

记录原则：

- 只记录会影响 SearchEdit 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-08-11

- API
  - Replace `HandleEnterAsSearch` with `IsSearchOnEnterEnabled` and enable Enter-triggered search by default.
  - Replace `SearchButtonClick` with `SearchRequested` and add `SearchRequestedEventArgs` plus `SearchTriggerSource`.
- Behavior
  - Route search-button clicks and unhandled Enter `KeyUp` events through `RaiseSearchRequested()`.
  - Include the query snapshot and `Button` / `EnterKey` trigger source in every search request.
  - Suppress repeated requests while `IsOperating=true` and leave Enter unconsumed when Enter search is disabled.
- Tests
  - Add SearchEdit behavior coverage for defaults, query snapshots, trigger sources and repeated-request suppression.
- Docs
  - Synchronize SearchEdit, LineEdit and AutoComplete contracts with the unified search-request model.

## 2026-08-01

- Theme
  - Give SearchEdit an independent Control identity while keeping it free of Own Tokens.
  - Replace the former LineEdit/Button Token ownership overlap with explicit `SearchEditTokenResource` and `ButtonTokenResource` references.
  - Use public `Button` as `PART_RightAddOn`; keep `SearchButtonTheme` as the typed Semantic Part Theme customization entry.
  - Register `SearchEditTheme.axaml`, `SearchEditDecoratedBoxTheme.axaml` and `SearchButtonTheme.axaml` as independent generated theme assets.
- Docs
  - Align the architecture and implementation documents with exact Control identity, Effective Global Token and generated asset registration.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `SearchEdit`.
  - Align generated output paths with `controls/search-edit/index-cn.md` and `controls/search-edit/semantic-cn.md`.

## 2026-06-21

- Docs
  - Establish SearchEdit desktop architecture documentation under `docs/controls/desktop/data-entry/search-edit/overview.md`.
  - Add SearchEdit implementation documentation covering search button event flow, SearchEditDecoratedBox, SearchButton, SearchEditPanel layout and maintenance invariants.
  - Document the former shared Token ownership model, superseded by the independent SearchEdit identity design on 2026-08-01.
  - Add SearchEdit changelog and link the document set from the Data Entry category entry.
