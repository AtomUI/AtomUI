# Card Token 设计

本文档定义 `AtomUI.Desktop.Controls.CardToken` 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Card 整体架构见 [Card 桌面版架构设计](overview.md)，内部实现原理见 [Card 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Card Changelog](changelog.md)。

## 1. 定位

CardToken 是 Card 的控件级 Token scope，描述卡片 Header、Body、Actions、Tabs、Extra、阴影、Grid item 和 action icon 的组件语义值。

CardToken 不承载以下状态：

- `Content`、`Header`、`Extra`、`Cover`、`Actions` 等实际内容。
- `ContentType`、`IsLoading`、`IsHoverable`、`IsInnerMode`、`StyleVariant` 等实例行为状态。
- `HeaderBorderThickness`、`EffectiveBorderThickness`、`EffectiveCornerRadius`、`IsActionsPanelVisible` 等内部派生状态。
- `CardGridItem.Row`、`Column`、`RowSpan`、`ColumnSpan` 等布局位置。
- `TabControl` 的 selected item、current content 或 tab collection。

## 2. Token 分类

### 2.1 Header Token

- `HeaderBg`
- `HeaderFontSizeLG`
- `HeaderFontSize`
- `HeaderFontSizeSM`
- `HeaderHeightLG`
- `HeaderHeight`
- `HeaderHeightSM`
- `HeaderPaddingLG`
- `HeaderPadding`
- `HeaderPaddingSM`
- `CardHeadPadding`（保留）

这些 Token 控制 Card Header 区域的背景、字体、最小高度和水平 padding。`CardHeadPadding` 是保留 Token（`[ReservedToken]`），对应
antd `cardHeadPadding`（页签放在 head 内时标题的上内边距）；AtomUI 的 `CardTabsContent` 把页签栏放在 Header 下方的 body 中，Header
保持常规高度与 padding。`SizeType` 负责选择 Large、Middle 或 Small 分支。

### 2.2 Body padding Token

- `BodyPaddingLG`
- `BodyPadding`
- `BodyPaddingSM`
- `CardPaddingBase`（保留）

这些 Token 控制普通内容、Meta 内容、Tabs 内容和 Grid item 的内容 padding。Grid 内容本身不使用 Card body padding，padding 由 `CardGridItem` 按尺寸
消费 `BodyPaddingLG` / `BodyPadding` / `BodyPaddingSM`。`CardPaddingBase` 是保留 Token，对应 antd `cardPaddingBase`（grid item padding）。

### 2.3 Actions Token

- `ActionsBg`
- `ActionsSpacing`（保留）
- `CardActionsIconSize`

这些 Token 控制底部操作区背景和 action icon 尺寸。操作区由 `CardActionPanel` 在 Render 中绘制背景（`ActionsBg`，只取 Card 的下圆角）、
顶部分隔线与操作项之间的竖线，竖线不会被背景遮住；分隔线颜色与粗细读 Card 的 `ColorBorderSecondary` / `BorderThickness`，操作项之间留
`LineWidth` 宽的间隙。`ActionsSpacing` 是保留 Token，对应 antd `actionsLiMargin`（每个操作项的上下外边距）；AtomUI 的操作区是
`ControlHeightLG` 高的一行，操作项拉伸占满整格，没有项外边距可设。

### 2.4 Tabs Token

- `TabsMarginBottom`（保留）

`TabsMarginBottom` 是保留 Token，对应 antd `tabsMarginBottom`（把 head 内的页签拉到 head 底边线上）；`CardTabsContent` 的页签栏位于 Header
下方的 body，没有可重叠的 head。`CardTabsContent` 的页签栏边距与内容 padding 读取 Card 的 Token（它是 Token owner 透明部件），内部的
`TabControl` 保留自己的 identity。

### 2.5 文本和阴影 Token

- `ExtraColor`
- `CardShadows`
- `CardGridItemShadows`

`ExtraColor` 控制 Header Extra 区域文字色（`HeaderExtra` 的 `Foreground`）。`CardShadows` 是 hoverable Card 和 hoverable grid item 的
悬停阴影（antd `cardShadow`）；静止的 Borderless Card 阴影是 `BoxShadowsTertiary`，经 `Controls[Card].BoxShadowsTertiary` 设置。`CardGridItemShadows` 使用多层阴影表达 grid item 之间的边线。

## 3. 控件专项模型中的 Token 使用

CardToken 使用路径：

```text
SharedToken
   ↓
CardToken
   ↓
CardTheme / CardActionPanelTheme / CardActionButtonTheme / CardGridItemTheme / CardTabsContentTheme / CardMetaContentTheme
   ↓
Header + Body + Cover + Actions + Grid + Tabs + Meta
```

重要映射：

| Token | 主要使用点 |
| --- | --- |
| `HeaderBg` | 根 Card HeaderFrame 背景。 |
| `HeaderFontSizeLG` / `HeaderFontSize` / `HeaderFontSizeSM` | HeaderFrame 文本尺寸。 |
| `HeaderHeightLG` / `HeaderHeight` / `HeaderHeightSM` | HeaderFrame 最小高度。 |
| `HeaderPaddingLG` / `HeaderPadding` / `HeaderPaddingSM` | HeaderFrame 水平 padding。 |
| `BodyPaddingLG` / `BodyPadding` / `BodyPaddingSM` | Card body、CardGridItem 和 CardTabsContent content padding。 |
| `ActionsBg` | CardActionPanel 背景（Render 中绘制，只取下圆角）。 |
| `TabsMarginBottom` | 保留，无视觉效果。 |
| `ExtraColor` | Extra 区域文字颜色。 |
| `CardShadows` | Card hover 和 grid item hover 阴影。 |
| `CardActionsIconSize` | CardActionButton icon 宽高。 |
| `CardGridItemShadows` | CardGridItem 默认边线阴影。 |

实例状态派生：

```text
SizeType
  -> choose Header / Body size branch

StyleVariant
  -> choose bordered or borderless surface

ContentType
  -> choose body padding and corner/border behavior

IsHoverable
  -> choose normal or hover shadow state
```

这些实例状态本身不是 Token。

全局 Token 经 `CardTokenResource` 读取，`Controls[Card]` 覆盖全部生效：卡片圆角是 `BorderRadiusLG`（设置 Card 圆角用
`Controls[Card].BorderRadiusLG`），外框与分隔线是 `ColorBorderSecondary` / `BorderThickness`，背景是 `ColorBgContainer`，Inner 模式头部是
`ColorFillAlter`。`CardGridItem`、`CardTabsContent` 是 Token owner 透明部件（`[TokenOwnerTransparent]`），主题以 `CardTokenResource` 读取；
`CardActionPanel` 无 identity，同样读 Card。`CardActionButton`、`CardMetaContent` 保留自己的 identity，以各自 TokenResource 读取全局 Token
（`CardActionsIconSize` 仍是 Card 的 Own Token）。Card body 位于注册控件 `Skeleton` 中，透明部件当前继承的 owner 是 Skeleton，因此它们的
TokenTransitions 时长跟随 Skeleton（Task 10.9 处理 Skeleton 的透明化）。

## 4. 控件家族影响

CardToken 直接影响 Card 家族控件：

- `Card`
- `CardActionPanel`
- `CardActionButton`
- `CardGridContent`
- `CardGridItem`
- `CardTabsContent`
- `CardMetaContent`
- Card token.md 语义说明

CardToken 不应被其他 Data Display 控件直接复用。若多个控件需要共享同一语义值，应评估是否上升为 SharedToken，而不是跨控件引用 CardToken。

## 5. 兼容性要求

CardToken 属于 Card 主题契约。即使 `CardToken` 是 internal 类型，生成的 token kind、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不改变 Header、Body、Actions、Tabs、Extra、Shadow 或 Grid item Token 的语义范围。
- 不把 `ContentType`、`IsLoading`、`IsHoverable`、`StyleVariant` 或 Grid item 位置状态迁移为 Token。
- 不在 CardToken 中复制 SharedToken 已表达的根边框、圆角、基础背景或通用文字颜色语义。
- 修改 size 相关 Token 时必须同时验证 Large / Middle / Small 下 Header、Body、Grid item 和 Tabs 内容。
- 修改 shadow Token 时必须验证 Card hover、borderless 和 grid item hover。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 Header Token | 验证 Large / Middle / Small Card Header 高度、padding、字体和 Headerless 状态。 |
| 修改 Body padding Token | 验证普通内容、Meta 内容、Tabs 内容和 Grid item padding。 |
| 修改 Actions Token | 验证底部操作区背景、action icon 尺寸、分隔线对齐和 hover 颜色。 |
| 修改 Tabs Token | 验证 CardTabsContent 与 Header/Body 边界。 |
| 修改 `ExtraColor` | 验证 Header Extra 和 HyperLinkButton 等内容视觉。 |
| 修改 shadow Token | 验证 hoverable Card、borderless Card 和 hoverable CardGridItem。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
