# InfoFlyout Changelog

本文档记录 InfoFlyout 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-07

- Feature
  - Add Semantic Part contract for InfoFlyout: `root` + `popup.root` / `popup.container` / `popup.content` /
    `popup.arrow`, aligned with Ant Design Popover's semantic DOM (`root` / `container` / `title` /
    `content` / `arrow`); the upstream `title` slot is omitted because InfoFlyout has no title node.
  - Introduce `FlyoutHost.SemanticParts.cs`; make `FlyoutHost.IsPopupPinnedOpen` public (precedent:
    `AbstractColorPicker`) so Gallery semantic preview can pin the flyout open.
  - Inject `popup.root` marker in `Flyout.CreatePresenter()` and `popup.container` / `popup.content` /
    `popup.arrow` markers in `FlyoutPresenter.OnApplyTemplate` (runtime-created cross-visual-root popup).
  - Use a `>>` descendant `SelectorRoute` (instead of `/template/`) for the four `popup.*` parts: the
    code-created popup presenter is a Visual-tree foreign popup root but a logical descendant of
    `FlyoutHost`, so the generated `FlyoutHostPopupXxxStyle` selectors reach it across the visual root.
  - Gallery StyleClass example reproduces the upstream Popover `style-class` demo: Object Style / Function
    Style hover popovers customized declaratively through dedicated generated
    `FlyoutHostPopupRootStyle` styles in AXAML (`Background` / `Foreground` / `Padding` /
    `CornerRadius`).
- Docs
  - Add standalone [InfoFlyout Semantic Part 契约](semantic-part.md) documenting the antd Popover alignment,
    `title` omission, the `>>` descendant route for the code-created cross-visual-root popup, and validation.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record FlyoutHost/Flyout as the semantic owner for InfoFlyout.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `InfoFlyout`.
  - Align generated output paths with `controls/info-flyout/index-cn.md` and `controls/info-flyout/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete InfoFlyout desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish InfoFlyout desktop architecture documentation under `docs/controls/desktop/data-display/info-flyout/overview.md`.
  - Add InfoFlyout implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add InfoFlyout control-level changelog.
  - Add InfoFlyout Token documentation covering FlyoutHostToken, TreeFlyoutToken.
