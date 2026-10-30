# Separator Semantic Part 契约

本文档定义 Separator 家族公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Separator 的整体设计见
[Separator 桌面版架构设计](overview.md)，真实模板与生命周期见 [Separator 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Separator 主控件公开 `root`、`rail` 与 `content` 三个职责区域，与上游稳定 Semantic DOM（`root` / `rail` /
`content`）对齐。`rail` 对应连接线（背景条）区域，由模板中的 `SeparatorRail` 节点承载；`content` 对应带标题分隔线中的文本区域。

`VerticalSeparator` 是 `Separator` 的便捷子类，仅覆盖 `Orientation` 默认值并通过 `StyleKeyOverride` 复用
`SeparatorTheme`，不持有独立 descriptor，其 Semantic Part 契约与 `Separator` 完全一致。

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `Separator` | `Single` | `Root` | `false` | `false` |
| `rail` | `.semantic-rail` | `SeparatorRail` | `Multiple` | `Selector` | `false` | `false` |
| `content` | `.semantic-content` | `TextBlock` | `Single` | `Selector` | `false` | `false` |

`root` 是控件自身，承载方向、尺寸、线型、标题位置等 public API、主题入口和状态归一，不声明 `.semantic-root`
marker。`rail` 是模板中的连接线节点 `PART_RailStart` / `PART_RailEnd`，以 `Multiple` 基数表示带标题水平分隔线的
左右两段；无标题水平分隔线只保留一段可见 rail，垂直分隔线只保留一段可见 rail，另一段被归零宽/高而不参与
Semantic Part 命中。`content` 是模板文本节点 `PART_Title`，承载 `Title` 文本内容，对应公共 API
`Title` / `TitleColor` / `TitlePosition` / `IsPlain`。

## 2. Selector 用法

应用侧只使用 owner-scoped selector 和生成的 Style 类型，不复制 `/template/` 路径：

```xml
<Style Selector="atom|Separator.semantic-style-demo">
    <atom:SeparatorRailStyle x:SetterTargetType="atomc:SeparatorRail">
        <Setter Property="LineColor" Value="#7cb305" />
    </atom:SeparatorRailStyle>
    <atom:SeparatorContentStyle x:SetterTargetType="atom:TextBlock">
        <Setter Property="FontStyle" Value="Italic" />
    </atom:SeparatorContentStyle>
</Style>
```

其中 `atom` 映射 `https://atomui.net`，`atomc` 映射 `https://atomui.net/common-controls`（`SeparatorRail` 所在
命名空间 `AtomUI.Controls.Commons`）。

Semantic Style 只覆盖 `rail` 连接线节点与 `content` 文本节点的视觉属性，不改变 `Orientation`、`Variant`、
`SizeType` 分支、标题 position 计算或分隔线绘制路径。连接线的颜色、宽度和线型既可通过 `SeparatorRailStyle`
以 Semantic Style 定制，也仍可通过 `LineColor`、`LineWidth`、`Variant` 公共 API 设置。

## 3. Template 与状态边界

- `rail` marker 位于 `SeparatorTheme.axaml` 模板的 `atom.commons:SeparatorRail` 节点 `PART_RailStart` /
  `PART_RailEnd` 上，`content` marker 位于 `atom:TextBlock` 节点 `PART_Title` 上，均使用静态
  `Classes.semantic-*="True"` 标记，不使用 Binding 或运行时赋值。
- 模板始终静态包含两段 rail 节点；`AbstractSeparator.ArrangeOverride` 按标题状态归位 rail 布局：
  - 带标题水平：`PART_RailStart` 占标题左侧、`PART_RailEnd` 占标题右侧，`TitlePosition` 决定哪一侧受
    `OrientationMargin` 影响。
  - 无标题水平：`PART_RailStart` 占满宽度，`PART_RailEnd` 归零宽。
  - 垂直：`PART_RailStart` 占满高度（上下各留 20% 内缩），`PART_RailEnd` 归零。
- `PART_Title` 在模板中静态存在；`Title` 为空或 `Orientation=Vertical` 时仅切换其 `IsVisible`，不改变
  descriptor、marker 身份或数量。
- `SeparatorRail.Render` 使用 `Pen` 绘制连接线，`Variant`（`Solid` / `Dotted` / `Dashed`）只影响该 `Pen` 的
  dash style；连接线绘制已从 root 的 `Render` 迁移到 rail 节点，因此 rail 是可命中的模板节点。
- 模板重套用后由新模板重新提供同一 `rail` / `content` marker 契约；隐藏 `PART_Title` 或归零 rail 尺寸不改变
  Part 数量。

## 4. 尺寸与布局基线

`content` 的文本字号由 `IsPlain` 与 `SeparatorToken` 控制：`IsPlain=True` 时使用正文 `FontSize` / 常规字重，
`IsPlain=False` 时使用 `FontSizeLG` / 字重 `500`。Semantic Style 覆盖 `content` 的字号、字重、字体样式或前景色时
作用于同一节点，不创建平行样式入口，也不应改写 `IsPlain` 状态分支。

`rail` 的设计线宽来自 `LineWidth`，实际绘制厚度经 render scale 归一；`SeparatorRailStyle` 覆盖 `LineColor`、
`LineWidth` 或 `Variant` 时作用于连接线节点。水平 Separator 的上下外间距由 `SizeType`（`Small` / `Middle` /
`Large`）映射到 `SeparatorToken` 的 `HorizontalMarginBlockSM` / `HorizontalMarginBlock` / `HorizontalMarginBlockLG`；
`SizeType=Custom` 由调用方接管间距。垂直 Separator 不应用这组间距，`content` 始终隐藏。Semantic Style 不替代这些
尺寸分支，需要定制间距时应设置公开尺寸 API。

## 5. 验证不变量

- `Separator` descriptor 包含 `root/rail/content`；`rail` 为 `Multiple` 静态 marker target，`content` 为单个
  静态 marker target。
- `VerticalSeparator` 不持有独立 descriptor，复用 `Separator` 的 `SeparatorTheme` 与契约。
- Desktop 叶子 Separator 主题（`SeparatorTheme.axaml`）声明静态 `Classes.semantic-rail="True"` 与
  `Classes.semantic-content="True"`，不使用 Binding 或 `False` marker。
- 默认主题不消费 `.semantic-*` selector；Semantic Part 只增加静态 class、descriptor 和生成 Style。
- `rail` 与上游 `rail` 语义对齐；连接线定制可走 Semantic Style 或
  `LineColor` / `LineWidth` / `Variant` 公共 API。
- Gallery Semantic Parts Tab 延迟创建 Separator Preview；Examples Tab 的既有示例树保持不变。
