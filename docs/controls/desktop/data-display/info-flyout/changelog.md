# InfoFlyout Changelog

本文档记录 InfoFlyout 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-04

- Changed
  - 默认 OpenMotion/CloseMotion 改为按样式实例创建的直接主题值，移除默认值回传与几何绑定链；播放准备保留原方向、原点和覆盖优先级。定义不持有控件或运行资源。

## 2026-11-03

- Changed
  - FlyoutHost 提供 OpenMotion/CloseMotion；托管组合在 Host 配置，独立 Flyout/MenuFlyout/TreeViewFlyout 使用 Flyout 继承的入口。
  - 明确 Motion 纯定义、单向配置和旧宿主释放边界；实际 AXAML 与共享定义的可回收性作为交付门禁。实现与验证状态见共享 Motion 验证页。

## 2026-11-02

- Changed
  - 明确 Popover 对应的 ZoomBig、Mid 时长、alignment 原点及公开配置 owner。见[控件架构](overview.md)与共享动效契约；已完成对应默认规格接入。

## 2026-11-01

- Theme
  - Migrate two Own definitions to V2 revision 1, preserving 11 fields and all four-algorithm defaults.
  - Consume optional Global fonts on actual owners and owner background on the ordinary popup presenter, preserving native and dedicated Semantic Part Style priority.
  - Resolve Pointer offsets from the current anchor scope and retire the old lease before retargeting.
- Fix
  - Replay hot Own/native offsets to the actual open Popup and reconcile a pinned Host after template replacement without replacing its content or scroll position.
  - Connect TreeViewFlyoutPresenter background through its root TemplateBinding; the effective null-to-Own correction requires bounded visual approval.
  - Keep every original tree row reachable through a real ScrollViewer inside the existing height limits.
  - Relay actual TreeViewItem clicks through the published event without per-item recursive MenuItem subscriptions.
- Docs
  - Correct the source-derived TreeViewFlyoutPresenterToken name and document resource retirement and scope boundaries. CT15 remains open; platform, full lifecycle and visual exclusions remain subject to their evidence records.


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
  - Add InfoFlyout Token documentation covering FlyoutHostToken, TreeViewFlyoutPresenterToken.
