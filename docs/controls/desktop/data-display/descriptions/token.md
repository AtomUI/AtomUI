# Descriptions Token 设计

本文档定义 `AtomUI.Desktop.Controls.DescriptionsToken` 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Descriptions 整体架构见 [Descriptions 桌面版架构设计](overview.md)，内部实现原理见 [Descriptions 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Descriptions Changelog](changelog.md)。

## 1. 定位

DescriptionsToken 是 Descriptions 的控件级 Token scope，描述描述列表的 label 背景、文本颜色、标题颜色、Header 间距、item padding、冒号间距、内容颜色和 Extra 颜色。

DescriptionsToken 不承载以下状态：

- `Items`、`ItemsSource`、`DescriptionItem.Content` 等数据状态。
- `ColumnInfo`、当前断点、有效列数、row/column/column-span 等布局状态。
- `IsBordered`、`Layout`、`IsShowColon`、`IsFilled`、`Span` 等实例行为状态。
- `Header`、`Extra` 的实际内容或模板。
- `IsLastRow`、`IsLastColumn`、`EffectiveBorderThickness` 等生成视觉派生状态。

## 2. Token 分类

`DescriptionsToken` 使用 V2 revision 1，10字段及四算法40条默认/type/key/slot保持。Global字体和Shared角色进入真实Root与生成cell：Root读取实际InheritanceParent；内部cell主题通过现有OwnerTokenResource的SourceControlType=Descriptions消费声明公开scope，不新增独立identity。运行注册表中只有Descriptions独立注册Own10，其余模型/内部视觉无独立fragment。

保留原角色边界：普通label/colon使用LabelColor，边框label使用ColorTextSecondary；ItemPadding三档只在边框布局消费，普通模式保持原自然布局。此API使用SizeType三档且默认Large，不新增Custom枚举；原生内容自身字体/颜色及专用Semantic Style仍按原生优先级覆盖。

### 2.1 文本颜色 Token

- `LabelColor`
- `TitleColor`
- `ContentColor`
- `ExtraColor`

这些 Token 控制普通模式 label、标题、内容和 Extra 的文本颜色。边框模式 label cell 的 foreground 当前使用 SharedToken `ColorTextSecondary`，label 背景使用 `LabelBg`。

### 2.2 Label 背景 Token

- `LabelBg`

`LabelBg` 用于边框模式下 label 区域背景，包括水平边框 label cell 和纵向边框模式 label 区域。

### 2.3 Header 间距 Token

- `HeaderMargin`

`HeaderMargin` 应用于根模板 `HeaderLayout`，控制 Header/Extra 行和内容区域之间的间距。

### 2.4 Item padding Token

- `ItemPaddingLG`
- `ItemPadding`
- `ItemPaddingSM`

这些 Token 控制边框模式 item cell 的内边距，对应 `SizeType=Large/Middle/Small`。普通非边框模式主要依赖 Grid row spacing 和内容自然尺寸。

### 2.5 冒号间距 Token

- `ColonMargin`

`ColonMargin` 用于普通 horizontal 和 vertical 非边框模板中的冒号 TextBlock。`IsShowColon=false` 时冒号节点隐藏，但 Token 语义不改变。

## 3. 控件专项模型中的 Token 使用

DescriptionsToken 使用路径：

```text
SharedToken
   ↓
DescriptionsToken
   ↓
DescriptionsTheme / DescriptionDefaultItemTheme / bordered cell themes
   ↓
Header + Extra + default items + bordered label/content cells
```

重要映射：

| Token | 主要使用点 |
| --- | --- |
| `LabelBg` | 边框 label cell 背景、纵向边框 label 背景。 |
| `LabelColor` | 普通模式 label 和冒号颜色。 |
| `TitleColor` | HeaderPresenter foreground。 |
| `HeaderMargin` | HeaderLayout margin。 |
| `ItemPaddingLG` / `ItemPadding` / `ItemPaddingSM` | 边框 item padding。 |
| `ColonMargin` | 冒号 TextBlock margin。 |
| `ContentColor` | content foreground。 |
| `ExtraColor` | ExtraPresenter foreground。 |

实例状态派生：

```text
SizeType
  → choose ItemPaddingLG / ItemPadding / ItemPaddingSM

IsBordered + Layout
  → choose root frame, default item template, or bordered cell theme

ColumnInfo + current break point
  → effective grid columns
```

`effective grid columns`、`rowCount`、`columnSpan`、`EffectiveBorderThickness` 都是运行时派生状态，不是 Token。

## 4. 控件家族影响

DescriptionsToken 只直接影响 Descriptions 及其内部生成视觉：

- `DescriptionsTheme.axaml`
- `DescriptionDefaultItemTheme.axaml`
- `DescriptionBorderedItemLabelTheme.axaml`
- `DescriptionBorderedItemContentTheme.axaml`
- Descriptions token.md 语义说明

其他 Data Display 控件不复用 DescriptionsToken。若多个控件需要共享同一语义，应评估是否上升为 SharedToken，而不是跨控件引用 DescriptionsToken。

## 5. 兼容性要求

DescriptionsToken 属于 Descriptions 主题契约。即使 `DescriptionsToken` 是 internal 类型，生成的 token kind、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不把 item 数据、响应式列数、span、当前断点或生成子控件状态迁移为 Token。
- 不在 DescriptionsToken 中复制根容器边框、圆角、分割线粗细等 SharedToken 语义。
- 不把 `SizeType`、`IsBordered`、`Layout` 或 `IsShowColon` 变成 Token；它们是实例行为属性。
- 修改 item padding 时必须同时验证 horizontal bordered 和 vertical bordered 模式。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改文本颜色 Token | 验证普通、边框、纵向和 Header/Extra 示例颜色。 |
| 修改 `LabelBg` | 验证水平边框 label cell 和纵向边框 label 区域背景。 |
| 修改 `HeaderMargin` | 验证 Header/Extra 与内容区域间距。 |
| 修改 item padding Token | 验证 Large / Middle / Small 下水平边框和纵向边框 item 高度、内容居中和边框对齐。 |
| 修改 `ColonMargin` | 验证普通 horizontal 和 vertical 非边框冒号间距，以及 `IsShowColon=false` 隐藏状态。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
