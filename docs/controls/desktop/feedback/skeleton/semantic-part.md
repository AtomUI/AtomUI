# Skeleton Semantic Part 契约

本文档定义 Skeleton 家族公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Skeleton 的整体设计见
[Skeleton 桌面版架构设计](overview.md)，真实模板与生命周期见 [Skeleton 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Skeleton 主控件公开 `root`、`header`、`section`、`avatar`、`title`、`paragraph` 六个职责区域；AtomUI Skeleton
保持相同的语义名称。`SkeletonAvatar`、`SkeletonButton`、`SkeletonInput`、`SkeletonImage` 和 `SkeletonNode`
是独立的公开子控件，各自公开 `root` 与 `content`。

### 1.1 `Skeleton`

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `Skeleton` | `Single` | `Root` | `false` | `false` |
| `header` | `.semantic-header` | `DockPanel` | `Single` | `Selector` | `false` | `true` |
| `section` | `.semantic-section` | `StackPanel` | `Single` | `Selector` | `false` | `true` |
| `avatar` | `.semantic-avatar` | `SkeletonAvatar` | `Single` | `Selector` | `false` | `true` |
| `title` | `.semantic-title` | `SkeletonTitle` | `Single` | `Selector` | `false` | `true` |
| `paragraph` | `.semantic-paragraph` | `SkeletonParagraph` | `Single` | `Selector` | `false` | `true` |

`header` 只承载头像占位区域，作为根布局的左侧 cell；`section` 承载标题和段落，作为右侧、填充剩余宽度的 cell。
`avatar`、`title` 和 `paragraph` 是主控件占位中的公开 Skeleton 子控件。这五个 Part 都位于 `PlaceholderTemplate` 构建的占位中，
只在 `IsLoading` 为真时存在，因此声明为 `RuntimeCreated`，选择器路径为 `/template/ .semantic-*`（占位的模板父级是 `Skeleton`）。
`root` 不声明 `.semantic-root` marker。

### 1.2 Skeleton 元素 owner

| Owner | Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SkeletonAvatar` | `root` | owner | `SkeletonAvatar` | `Single` | `Root` | `false` | `false` |
| `SkeletonAvatar` | `content` | `.semantic-content` | `Border` | `Multiple` | `Selector` | `false` | `false` |
| `SkeletonButton` | `root` | owner | `SkeletonButton` | `Single` | `Root` | `false` | `false` |
| `SkeletonButton` | `content` | `.semantic-content` | `Border` | `Multiple` | `Selector` | `false` | `false` |
| `SkeletonInput` | `root` | owner | `SkeletonInput` | `Single` | `Root` | `false` | `false` |
| `SkeletonInput` | `content` | `.semantic-content` | `Border` | `Multiple` | `Selector` | `false` | `false` |
| `SkeletonImage` | `root` | owner | `SkeletonImage` | `Single` | `Root` | `false` | `false` |
| `SkeletonImage` | `content` | `.semantic-content` | `Border` | `Multiple` | `Selector` | `false` | `false` |
| `SkeletonNode` | `root` | owner | `SkeletonNode` | `Single` | `Root` | `false` | `false` |
| `SkeletonNode` | `content` | `.semantic-content` | `Border` | `Multiple` | `Selector` | `false` | `false` |

当前模板中每个元素 owner 只有一个 `content` target：`PART_ContentLayer`，普通与激活状态共用，始终可见；激活时微光在它之上
绘制。`content` 的 Cardinality 保持 `Multiple`，descriptor 契约不变。

## 2. Selector 用法

应用侧只使用 owner-scoped selector 和生成的 Style 类型，不复制 `/template/` 路径：

```xml
<Style Selector="atom|Skeleton.semantic-style-demo">
    <Setter Property="Padding" Value="12" />
    <Setter Property="CornerRadius" Value="10" />
    <atom:SkeletonHeaderStyle x:SetterTargetType="DockPanel">
        <Setter Property="Margin" Value="0,0,0,12" />
    </atom:SkeletonHeaderStyle>
    <atom:SkeletonAvatarStyle x:SetterTargetType="atom:SkeletonAvatar">
        <Setter Property="BorderBrush" Value="#AAAAAA" />
        <Setter Property="BorderThickness" Value="1" />
    </atom:SkeletonAvatarStyle>
</Style>
```

`SkeletonTitleStyle`、`SkeletonParagraphStyle` 以及五个元素 owner 的 `*ContentStyle` 使用同一规则。Semantic Style 只覆盖
视觉属性和布局属性，不改变 `IsLoading`、`IsActive`、尺寸分支、内容模板或动画状态。

## 3. Template 与状态边界

- 主控件的 `header`、`section`、`avatar`、`title` 和 `paragraph` marker 静态写在 Skeleton 的 `PlaceholderTemplate` 中，
  只在加载时存在；`header` 和 `section` 是同一两列布局的 sibling，分别对应左侧头像 cell 和右侧内容 cell。
- 元素 owner 的 `content` marker 位于唯一的静态内容层；不扫描 `SkeletonLine` 运行时集合，也不公开 `PART_*` 名称。
- `SkeletonParagraph` 是运行时 `SkeletonLine` 背景的唯一状态 owner：创建行时写入当前 Background，
  Paragraph Background 变化时直接同步已创建的行。该路径不使用字符串绑定，不把行节点提升为独立的 Semantic Part owner。
- `SkeletonImage` 的内部图标、`SkeletonNode` 的用户 ContentPresenter、标题/段落的内部 line 和微光的合成层视觉均不属于
  `content` Part。
- 激活与否不改变 descriptor、marker 身份或数量；模板重套用后由新模板重新提供同一 marker 契约。

## 4. 尺寸与布局基线

Skeleton 没有统一的 `SizeType`；公开元素 owner 的 Large/Middle/Small 分支由 `SkeletonElement.SizeType`、
`SkeletonAvatar.SizeType` 和对应 SharedToken 控制，`Size` 非 `NaN` 时进入 Avatar 自定义尺寸分支。Semantic Style 不以固定
Height 替代这些完整分支。

主控件的根 Padding 通过 `SkeletonTheme` 的根 Border 参与测量；根 Border 默认透明，不重复绘制子占位层的 Background，显式设置
根 Background 时仍可作为 root surface 定制。header Margin、avatar Margin、title/paragraph 的自然尺寸和 paragraph 行间距继续由
现有 Token 与模板负责。Paragraph 的 Background 由 Paragraph owner 投影到运行时行，避免 Gallery 示例通过像素偏移或 VisualTree
扫描伪造段落定制效果。

Gallery 的五个元素预览会把带有 `semantic-preview-owner` 标记的 owner root 铺满预览 stage；`content` 层仍按元素自身尺寸
测量并从 root 左侧对齐。该预览专用布局会解除图片元素的 `MaxWidth` 约束，但不改变控件直接使用时的尺寸 Token 或调用方显式尺寸。

## 5. 验证不变量

- `Skeleton` descriptor 只包含 `root/header/section/avatar/title/paragraph`。
- 五个元素 owner descriptor 只包含 `root/content`，`content` 为唯一的静态内容层。
- 主控件的 Part 只在 `IsLoading` 为真时存在；Gallery Semantic Preview 的 owner 设置 `IsLoading="True"`。
- Desktop 所有叶子 Skeleton 主题均声明静态 `Classes.semantic-*="True"`，不使用 Binding 或 `False` marker。
- 默认主题不消费 `.semantic-*` selector；Semantic Part 只增加静态 class、descriptor 和生成 Style。
- Gallery Semantic Parts Tab 延迟创建主控件和元素 owner Preview；Examples Tab 的既有示例树保持不变。
- Semantic 示例标题使用 `Custom Semantic Part styling`，文案只描述 AtomUI owner-scoped Style；Tag 使用当前版本
  `GalleryVersionInfo.DisplayVersion`（`v6.1.3`）。
