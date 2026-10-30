# SplitButton Changelog

本文档记录 SplitButton 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-15

- Docs
  - Add the `semantic-part.md` contract with 7 Semantic Parts: popup-side `popup.root` / `itemTitle` / `item` / `itemContent` / `itemIcon` aligned with the upstream antd Dropdown semantic DOM, plus trigger-side `primary` / `secondary` as an explicit AtomUI capability supplement (upstream consumers own the trigger buttons, AtomUI consumers get template-internal buttons).
  - Record the marker ownership: trigger-side static markers on `PART_PrimaryButton` / `PART_SecondaryButton`, popup-side runtime markers through the shared MenuFlyout / MenuItem injection paths established by DropdownButton.
- Semantic Parts
  - Register the 7-part descriptor via `SplitButton.SemanticParts.cs` and add `semantic-primary` / `semantic-secondary` static markers in `SplitButtonTheme.axaml` (generator-validated).
  - Align the primary-shape seam separator with the upstream solid compact rule: `SplitSeparatorBrush` resolves to `ColorPrimaryHover` / `ColorErrorHover` (by `IsDanger`) instead of `ColorBorder`, and the separator hides while the line-owning secondary button is hovered (emerges from color: the strip equals the secondary hover background token, no geometry change).
- Bugfix
  - Fix a severe hover regression introduced by the first separator-hide implementation: extending the secondary button 1px left on hover visibly shifted the button and amplified edge jitter into rapid flicker (`IsPrimaryButtonType = true` only). Hover no longer touches layout; a regression test locks the secondary button's identical Bounds while pointed over.
- API
  - Promote `IsPopupPinnedOpen` from internal to public (mirrors `DropdownButton.IsPopupPinnedOpen`) so Gallery semantic previews and diagnostics can pin the cross-visual-root flyout popup.
- Gallery
  - Migrate the SplitButton showcase to `GalleryShowCaseHost` with a Semantic Parts tab (pinned primary-shape preview with 7 part cards) and add a semantic styles example driven by the generated `SplitButton*Style` classes.
  - Fix the semantic preview popup-root registration to follow the resilient MenuShowCase pattern (`bf00b29b9`): re-register on `LayoutUpdated` instead of a one-shot `Loaded` pass (the pinned flyout opens asynchronously after `Loaded`, so the one-shot pass silently missed the code-created popup and left all popup-side parts unresolvable), subscribe to popup `Closed` with a deferred rescan, and keep tracking not-yet-opened nested submenu popups so late-opening submenus stay resolvable.
- Pinned popup lifecycle
  - Reopen the pinned flyout after lifecycle closes: the upstream `HideCore` tears down `Flyout.Target` / `Popup.PlacementTarget` and with them the popup-side pinned reconcile subscriptions, so a tab switch (placement target becomes invisible) closed the flyout permanently. SplitButton now arms a `LayoutUpdated`-driven retry (silent while the tree is hidden) and re-`ShowAt`s the anchor once the tree becomes visible again.
  - Propagate `IsPopupPinnedOpen` from MenuFlyout down through MenuFlyoutPresenter and MenuItem containers (the NavMenu `item.IsPopupPinnedOpen` pattern): pinned MenuItems now keep their declarative `IsSubMenuOpen` across the host popup's lifecycle close (rebound by the existing pinned guard) and restore the submenu popup presentation on re-attach.
  - Fix an orphan submenu popup introduced by the pinned rebound: while the host popup is closed (tab switched away), the rebound `IsSubMenuOpen=true` sync used to reopen the nested popup immediately and leave a blank popup shadow hanging beside the restored submenu. `SyncSubMenuPopupOpenState` now skips the open write while the hosting popup is closed (plain menus without a popup host are unaffected); presentation is restored by the re-attach deferred sync instead.
  - Center the semantic preview anchor horizontally (`HorizontalAlignment="Center"`, same as the MenuShowCase popup preview): the control's default `Left` alignment pushed the button and its cascaded menu to the left edge of the stage.
  - Place the semantic preview anchor at the top of the stage with `PreviewStageMinHeight=420` (same pattern as DatePicker / TimePicker / ImagePreviewer): the centered anchor left too little room below, so the flyout and its cascaded submenu overflowed the stage bottom (7px at 1280 wide, 36/73px in the compact layout) and covered the parts cards. Measured with rendered frames; both layouts now report the popups fully inside the stage.
  - Fix the blank popup shell rendering directly under a menu group title (root cause): the pinned flag was relayed to every `MenuItem`'s `PART_Popup`, but a pinned popup force-opens itself — leaf items have no submenu content, so each pinned leaf opened an empty popup that stayed in the overlay layer as a rounded white card with a shadow. Pin relay is preserved end to end (Flyout → Presenter → MenuItem containers), but `MenuItem` now applies it only to popups that actually carry a submenu (`UpdateSubMenuPopupPinnedOpen`, re-synced on pin changes and item-count changes). Covered by `SplitButtonPinnedSubMenuPopupTests`.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record SplitButton as the semantic owner, with Flyout used only as the relay adapter.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `SplitButton`.
  - Align generated output paths with `controls/split-button/index-cn.md` and `controls/split-button/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete SplitButton desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish SplitButton desktop architecture documentation under `docs/controls/desktop/general/split-button/overview.md`.
  - Add SplitButton implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add SplitButton control-level changelog.
  - Document that SplitButton does not require a dedicated Token document and records its theme dependencies in the overview.
