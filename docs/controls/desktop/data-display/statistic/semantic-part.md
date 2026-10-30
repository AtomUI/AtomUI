# Statistic Semantic Part 契约

本文档定义 Statistic 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Statistic 的整体设计见
[Statistic 桌面版架构设计](overview.md)，真实模板节点与状态流见 [Statistic 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Statistic 公开 `root`、`header`、`title`、`content`、`value`、`prefix` 和 `suffix`。该契约只属于 `Statistic`；
`AbstractStatistic`、`TimerStatistic` 和 `StatisticCountUp` 不继承或发布这组 descriptor。

| Part | Selector | Style Type | ContractType | Cardinality | AtomUI 节点 | 稳定性 |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | Statistic 本身 | 不适用 | `Statistic` | `Single` | owner | stable since 6.2.0 |
| `header` | `.semantic-header` | `StatisticHeaderStyle` | `Border` | `Single` | `Border#HeaderLayout` | stable since 6.2.0 |
| `title` | `.semantic-title` | `StatisticTitleStyle` | `ContentPresenter` | `Single` | `ContentPresenter#HeaderPresenter` | stable since 6.2.0 |
| `content` | `.semantic-content` | `StatisticContentStyle` | `StackPanel` | `Single` | `StackPanel#ContentLayout` | stable since 6.2.0 |
| `value` | `.semantic-value` | `StatisticValueStyle` | `ContentPresenter` | `Single` | 数值 `ContentPresenter` | stable since 6.2.0 |
| `prefix` | `.semantic-prefix` | `StatisticPrefixStyle` | `ContentPresenter` | `Single` | 前缀 `ContentPresenter` | stable since 6.2.0 |
| `suffix` | `.semantic-suffix` | `StatisticSuffixStyle` | `ContentPresenter` | `Single` | 后缀 `ContentPresenter` | stable since 6.2.0 |

六个 selector Part 的 `SelectorRoute` 均为 `/template/ .semantic-<name>`，`Customization` 为 `Selector`，
`CrossVisualRoot=false`，`RuntimeCreated=false`。root 的 `Customization` 为 `Root`，不生成 `.semantic-root` 或 Style Type。

## 2. Part 说明

### 2.1 `root`

root 是 `Statistic` 控件实例本身，适合定制 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、
`StrokeDashArray`、Margin、对齐、透明度和整体可见性。叶子模板通过 `DashedBorder` 投影这些表面属性；该内部 frame 不是独立 Part。

### 2.2 `header` 与 `title`

`header` 是标题区域容器，负责标题布局和可见性；`title` 是实际承载 `Header` 与 `HeaderTemplate` 的 `ContentPresenter`。Header 为 null
时 header 隐藏，但两个静态 target 均保留。应用可以在 header 上调整 Padding、Margin、Background 或对齐，在 title 上调整字体、前景色和文本布局。

### 2.3 `content`

`content` 是横向排列 prefix、value、suffix 的 `StackPanel`。它适合定制 spacing、对齐，以及继承型
`TextElement.Foreground`、`TextElement.FontSize`。默认主题也通过该继承路径把 `ContentForeground` 和 `ContentFontSize` 投影到三个内容节点；
prefix 中的 AtomUI Icon 同步消费这组颜色和字号。

### 2.4 `value`

`value` 承载格式化 `Value` 或调用方提供的自定义 `Content`。它适合定制 `Foreground`、`FontSize`、`FontWeight`、`Background`、
`CornerRadius`、`Padding` 和对齐，不负责改变数值格式化规则。

### 2.5 `prefix` 与 `suffix`

`prefix`、`suffix` 分别承载 `ValuePrefixAddOn` 和 `ValueSuffixAddOn`。AddOn 为 null 时对应节点隐藏，重新赋值时复用同一 target。
应用可以定制前后缀的颜色、字号、间距、背景、Padding 和对齐，但不得假定用户内容子树属于 Statistic 的 Semantic Part。

## 3. Selector 用法

生成的 Style Type 封装 owner-relative route，应用不直接复制 `/template/` selector：

```xml
<Style Selector="atom|Statistic.semantic-demo">
    <Setter Property="BorderBrush" Value="#CCCCCC" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="16" />
    <Setter Property="StrokeDashArray" Value="4,2" />

    <atom:StatisticTitleStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#1890FF" />
        <Setter Property="FontWeight" Value="SemiBold" />
    </atom:StatisticTitleStyle>
    <atom:StatisticContentStyle x:SetterTargetType="StackPanel">
        <Setter Property="TextElement.FontSize" Value="24" />
    </atom:StatisticContentStyle>
    <atom:StatisticValueStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Background" Value="#E6F4FF" />
        <Setter Property="Foreground" Value="#0958D9" />
        <Setter Property="CornerRadius" Value="4" />
        <Setter Property="Padding" Value="6,0" />
    </atom:StatisticValueStyle>
</Style>
```

不得使用以下写法：

- `.semantic-root`、`DashedBorder#Frame`、`Border#HeaderLayout` 或 Name selector 作为应用主题契约。
- `Border.semantic-header`、`ContentPresenter.semantic-value` 等类型前缀。
- 复制 `/template/ .semantic-*` 完整 route 作为主要用户写法。
- 穿过 prefix、value、suffix 的用户内容继续匹配内部 Visual。
- 用 Semantic Style 改写 `IsVisible`、`Content` 或格式化状态 ownership。

## 4. 状态与数量语义

| 状态 | root | header/title | content/value | prefix | suffix |
| --- | --- | --- | --- | --- | --- |
| 默认 Header 与 AddOn | 1 | 1 / 1，可见 | 1 / 1 | 1，可见 | 1，可见 |
| Header 为 null | 1 | 1 / 1，header 隐藏 | 1 / 1 | 不变 | 不变 |
| Prefix 为 null | 1 | 不变 | 1 / 1 | 1，隐藏 | 不变 |
| Suffix 为 null | 1 | 不变 | 1 / 1 | 不变 | 1，隐藏 |
| `IsLoading=true` | 1 | 不变 | 1 / 1，由 Skeleton 覆盖 loading 视觉 | 1 | 1 |
| 自定义 HeaderTemplate 或 Content | 1 | target 不变 | target 不变 | 不变 | 不变 |
| 模板重套用 | 1 个新 owner | 各 1 个新 target | 各 1 个新 target | 1 个新 target | 1 个新 target |

Static marker 表达稳定模板职责。Header、Prefix、Suffix 的存在性通过 `IsVisible` 表达，不在属性变化时创建、删除或重新标记 target。

## 5. 定制边界

以下区域不属于 Statistic Semantic Part：

- root 内部 `DashedBorder`、Skeleton、loading 占位结构和 `StackPanel#RootLayout`。
- 格式化数值字符串内部字符、`StatisticCountUp` 内部文本和 TimerStatistic 的剩余时间节点。
- Prefix、Value、Suffix 中由调用方提供的 DataTemplate 或 Control 子树。
- TokenResourceBinder、格式化委托、`EffectiveValue` 和内部数据同步状态。

默认 ControlTheme 不得消费 `.semantic-*` 作为自身样式机制。Statistic 不提供 Semantic Part Theme 替换，不跨 VisualRoot，也不在运行时发现 target。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、把静态 marker 移出 `Statistic` 叶子模板，或让默认主题依赖
`.semantic-*`，均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有七个已批准 Part，字段值与本文一致。
- 六个生成 Style Type 可以在 AXAML 中编译并命中真实节点。
- 叶子模板只存在六个静态 `Classes.semantic-*="True"`，不出现 `.semantic-root`。
- root 的 Background、Border、CornerRadius、Padding 和 `StrokeDashArray` 投影到真实 `DashedBorder`。
- Header、Prefix、Suffix 的 null/非 null 切换不改变 target 身份。
- 默认主题不消费 `.semantic-*`，未声明用户 Style 时不增加反射、VisualTree 搜索或运行时 selector 组装。
- Gallery Semantic Preview 只在首次选择 Semantic Parts Tab 后创建。
- Gallery 示例保持对应公开上游 6.6.0 的双 Statistic 布局、数值、标题、颜色、字号、图标、圆角、Padding、虚线边框和 16px 间距。
