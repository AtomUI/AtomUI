# Tag Changelog

本文档记录 Tag 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-07

- API
  - `TagVariant` 末尾新增 `Outline`（值 3；`Filled`、`Solid`、`Outlined` 仍为 0、1、2）。有 `TagColor` 时背景透明，边框和文字取主色（预设色取调色板第 6 色，状态色取语义色，自定义色取原色）。
  - `TagColor` 接受 `primary`（`ColorPrimary`、`ColorPrimaryBg`、`ColorPrimaryBorder`）与 `danger`（等同 `error`），行为与其他状态名一致，设置 `:status-color` 伪类。
  - `AbstractTag` 新增 `protected virtual AbstractIconButton? CreateCloseButton()`，以及受保护常量 `CloseButtonPartName`、`CloseButtonHostPartName`。
  - 模板契约：模板不含 `PART_CloseButton` 但含名为 `PART_CloseButtonHost` 的 `Decorator` 时，基类在 `IsClosable=true` 期间通过 `CreateCloseButton()` 按需创建关闭按钮并放入该宿主，`IsClosable=false` 时移除。按钮命名为 `PART_CloseButton`，设置 `TemplatedParent`，`Icon` 绑定 `CloseIcon`，`Click` 触发 `Closed`。
- Theme
  - Desktop `TagTheme.axaml` 为无 `TagColor` 的 `Variant=Outline` 增加样式：透明背景，主色边框与文字。
- Performance
  - 新增 `TagColorTable`，按 `ThemeSnapshot` 构建一次并缓存，持有预设色、五个状态色和 Solid 文字色，取代每个实例各自持有的字典。1000 个空颜色 Tag 的创建加布局分配约 17.9 MB 降至 14.9 MB。
  - `TagColor` 为空、空白或 `default` 时不订阅主题、不解析快照；设置颜色时才订阅，清除颜色时释放。`OnApplyTemplate` 不再重复构建快照。
  - 重新挂树时按当前主题重新解析颜色表，离树期间的主题切换在挂回后生效。
- Desktop 影响
  - Desktop 模板声明 `PART_CloseButton`，沿用静态关闭按钮，行为不变；按需创建只对声明 `PART_CloseButtonHost` 的模板生效。
  - 现有三种 Variant、预设色与状态色的取值不变；`primary`、`danger` 与 `Outline` 为新增能力。
  - 已知边界：在已应用模板且已离树的 Tag 上把 `IsClosable` 设为 `true` 时，按需创建的关闭按钮此刻没有 `CloseIcon`，因为默认关闭图标只在挂树期间设置。
- Tests
  - 新增 `TagExtensionTests`：枚举值与既有变体颜色、Outline 颜色、`primary`/`danger`、空颜色不解析快照、颜色表按快照共享、挂树与离树期间的主题切换、按需创建关闭按钮、静态关闭按钮保持可用。
  - `TagColorVariantTests` 新增无颜色 Outline 使用主色的用例。

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
