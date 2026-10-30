# Tag Changelog

本文档记录 Tag 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-08-19

- Design
  - 公开 Semantic Part：`Tag` owner 发布 `root` / `icon` / `content` / `close` 四个 Part，对齐上游
    `TagSemanticType`（`classNames` / `styles` 均为 `{ root?, icon?, content?, close? }`）。
  - `CheckableTagGroup` owner 发布 `root` / `item` 两个 Part，对齐上游 `CheckableTagGroupSemanticType`
    （`classNames` / `styles` 均为 `{ root?, item? }`）；`CheckableTag` 不持有独立 descriptor，容器职责由
    `item` Part 表达。
  - Tag 颜色算法（`TagColor` × `Variant`）与颜色伪类是行为状态，不是 Part；它以 `Template` 优先级写回
    `Background` / `Foreground` / `BorderBrush`，低于用户 selector Style 的 `StyleTrigger` 与本地值优先级，
    用户 root 样式 setter 统一覆盖全部颜色分支，移除后状态机恢复。
- Theme/Token
  - 修正 `DefaultBg` 推导：由 `ColorFillQuaternary` 改为 `ColorFillTertiary` 在 `ColorBgContainer` 上合成，对齐
    antd v6 `prepareComponentToken` 的 `defaultBg = colorFillTertiary.onBackground(colorBgContainer)`
    （Filled 变体同样消费该值）。此前浅一档，视觉上比 antd 默认 Tag 背景过浅。
- Implementation
  - `TagTheme.axaml` 的 `IconPresenter#IconPresenter` / `TextBlock#TagTextLabel` / `IconButton#PART_CloseButton`
    模板节点声明 `Classes.semantic-icon` / `Classes.semantic-content` / `Classes.semantic-close` 静态 marker。
  - `CheckableTagGroupTheme.axaml` 的 `CheckableTagItemsControl#PART_CheckableTagItems` 节点声明
    `.semantic-scope-items` route 跳点；`CheckableTagItemsControl` 容器创建路径用生成常量
    `CheckableTagGroupSemanticParts.ItemClass` 幂等建立 `semantic-item` marker，prepare 补齐。
  - 生成 `TagIconStyle` / `TagContentStyle` / `TagCloseStyle` / `CheckableTagGroupItemStyle` owner-scoped
    语义样式类。
- Gallery
  - ShowCase 迁移到 `GalleryShowCaseHost`，新增两个 Semantic Part 预览（`TagSemanticPreview` 覆盖
    `root` / `icon` / `content` / `close`，`CheckableTagGroupSemanticPreview` 覆盖 `root` / `item`）与自定义
    Semantic Part 样式示例（`tag-semantic-part`）。
- Docs
  - 新增 [Tag Semantic Part 契约](semantic-part.md)，定义六个 Part 的 Selector、类型、数量语义、尺寸基线与
    验证契约。

## 2026-07-29

- Design/API
  - Define `TagVariant` with `Filled`, `Solid` and `Outlined`; the default is `Filled`.
  - Remove `IsBordered` from the Tag contract. The old borderless and inverse compatibility behavior is not part of the new design.
  - Keep Tag color input orthogonal to Variant and define the Ant Design v6 `Default`, `Preset`, `Status` and `Custom` color matrix.
  - Define `CheckableTag` as a ToggleButton-based binary selection tag with Content, Icon, command, motion and Form semantics, without ordinary Tag color, Variant or close APIs.
  - Define `CheckableTagGroup` with primitive/structured Options, cancellable single selection, multiple selection, TwoWay `CheckedItem(s)`, one-time defaults, mode conversion and unified change events.
- Architecture
  - Keep Group business values separate from the internal SelectionModel, option wrappers and CheckableTag containers through an internal `SelectingItemsControl` host without a public `ListBox` fallback.
  - Define collection replacement, in-place collection notifications, template reapplication, container recycle and detach release invariants.
- Theme/Token
  - Define preset palette mapping, semantic status mapping, custom HSL light-background calculation and default Solid text contrast.
  - Keep `info` as the AtomUI input alias for `processing` and use the same Info semantic tokens.
  - Do not add global Tag configuration or put Variant into `ThemeConfig`.
  - Reuse Tag family and SharedToken visual semantics for CheckableTag and keep CheckableTagGroup free of component-specific Tokens.
- Docs
  - Add the CheckableTag and CheckableTagGroup selection-model design and synchronize the Tag overview, implementation, Token and LLMS source map.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Tag`.
  - Align generated output paths with `controls/tag/index-cn.md` and `controls/tag/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Tag desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Tag desktop architecture documentation under `docs/controls/desktop/data-display/tag/overview.md`.
  - Add Tag implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Tag control-level changelog.
  - Add Tag Token documentation covering TagToken.
