# PopupConfirm Changelog

本文档记录 PopupConfirm 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-21

- Fix：原生弹窗模式下弹层右侧在边框与阴影之间有 1 物理像素的透明缝，白色页面上看起来像边框外多了一圈白底。原因是阴影裁切按 `PART_ContentDecorator` 的位置计算，而它可能在外层容器不重新排版的情况下单独重排（例如宽 259.5 DIP 时），阴影的裁切矩形停在旧的 260 DIP。`ShadowsAwareContainer` 现在在可见框体的 Bounds 变化时重新排版，阴影紧贴边框。InfoFlyout、ToolTip 等带箭头的弹层同样受益。

## 2026-11-19

- 内置主题默认启用 `IsPointAtCenter`，让箭头指向触发控件中心。显式设置为 `False` 可使用严格的边缘对齐。

## 2026-11-15

- Theme / Token：与 anchor 的间距改读 `FlyoutHostToken.MarginToAnchor`（原 `PopupToken.MarginToAnchor`），与阴影同属箭头 flyout 的 Token；两者默认值相同。带边框时箭头随框体整体描边显示（见 InfoFlyout Token 设计）。

## 2026-11-08

- Theme / Token
  - PopupConfirm reads its motion Tokens through `PopupConfirmTokenResource`; the confirm container reads the PopupConfirm TokenResource (title colour and `FontWeightStrong`, icon size, status colours).
  - `PopupConfirm.PopupMinWidth` and `PopupMinHeight` are removed (copied from the menu popup Tokens and never consumed).

## 2026-11-04

- Changed
  - 在独立主题显式补齐 Popover OpenMotion/CloseMotion 默认值，确保默认配置传到实际 Flyout/Popup。
  - 默认 OpenMotion/CloseMotion 改为按样式实例创建的直接主题值，移除默认值回传与几何绑定链；播放准备保留原方向、原点和覆盖优先级。定义不持有控件或运行资源。

## 2026-11-03

- Changed
  - 从 FlyoutHost 继承 OpenMotion/CloseMotion，在 PopupConfirm 实例设置；内部 PopupConfirmFlyout 只承载和转发。
  - 明确 Motion 纯定义、单向配置和旧宿主释放边界；实际 AXAML 与共享定义的可回收性作为交付门禁。实现与验证状态见共享 Motion 验证页。

## 2026-11-02

- Changed
  - 明确 Popover 对应的 ZoomBig、Mid 时长、alignment 原点及公开配置 owner。见[控件架构](overview.md)与共享动效契约；已完成对应默认规格接入。

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
