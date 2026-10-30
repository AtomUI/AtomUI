# PopupConfirm Changelog

本文档记录 PopupConfirm 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-11

- Docs
  - Publish the PopupConfirm Semantic Part contract: 9 parts (`root` implicit + 8 `popup.*`) aligned with the upstream Popconfirm semantic DOM; upstream `content` maps to `popup.description`, and the unexposed button row is published as `popup.actions`.
  - Add `semantic-part.md` and link it from `overview.md` / `implementation.md`; record the descriptor/marker injection mapping.
- Theme
  - Declare `semantic-popup-icon` / `semantic-popup-title` / `semantic-popup-description` / `semantic-popup-actions` statically on `PopupConfirmContainerTheme.axaml`; the four popup frame parts reuse the shared `Flyout` / `FlyoutPresenter` code-injected markers.
  - Default themes still do not consume any `.semantic-*` selector; default rendering is unchanged.
- Gallery
  - Make the Placement example occupy an entire row and let the semantic style-class example switch the title foreground via `PopupConfirmPopupTitleStyle`, matching the upstream title/content color contract.
  - Fix
    - The semantic style-class demo kept the heading color on the dark popup because `PopupConfirmContainerTheme` sets `ColorTextHeading` directly on `PART_Title`; the title now needs an explicit `PopupConfirmPopupTitleStyle` foreground.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record PopupConfirm (inherited from FlyoutHost) as the semantic owner, with PopupConfirmFlyout used only as the relay adapter.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `PopupConfirm`.
  - Align generated output paths with `controls/popup-confirm/index-cn.md` and `controls/popup-confirm/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete PopupConfirm desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish PopupConfirm desktop architecture documentation under `docs/controls/desktop/feedback/popup-confirm/overview.md`.
  - Add PopupConfirm implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add PopupConfirm control-level changelog.
  - Add PopupConfirm Token documentation covering PopupConfirmToken.
