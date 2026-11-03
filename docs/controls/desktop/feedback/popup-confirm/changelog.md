# PopupConfirm Changelog

## 2026-10-31

- 七Own保留原值/key/type/slot并迁有效默认；PopupMinWidth/PopupMinHeight保留历史资源，不新增自然尺寸下限。
- 确认容器复用scope发布桥和真实终端owner，字体/标题/状态glyph Global及Own间距支持打开期间热更新，原生内容和专用Part保持。
- 保留原动态图标、确认/取消事件、键盘与pinned关闭合同。

本文档记录 PopupConfirm 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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
