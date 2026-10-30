# Steps Semantic Part 契约

本文档定义 `Steps` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。整体设计见
[Steps 桌面版架构设计](overview.md)，真实模板与生命周期见 [Steps 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

Steps 的公开语义结构与 Ant Design 6 的 Semantic DOM 对齐：`StepsSemanticType = { root, item, itemWrapper,
itemIcon, itemTitle, itemSubtitle, itemSection, itemContent, itemRail }`（since 6.2.0）。`root` 是隐式
owner，`item` 覆盖 `StepsItem` 步骤容器，七个 item 子 Part 覆盖 item 模板内的包裹层、图标、标题、副标题、
内容区、详情和连接线。上游把 `styles.item` 应用到整个步骤项，把 `styles.itemWrapper` / `styles.itemIcon` /
`styles.itemTitle` / `styles.itemSubtitle` / `styles.itemSection` / `styles.itemContent` /
`styles.itemRail` 应用到 item 内部的对应节点。

`StepsItem` 不持有独立 Semantic descriptor；它是 `Steps` 的 public item 容器。`item` Part 的 marker 由
`Steps` 在容器准备时写入容器，七个子 Part 的 marker 是 `StepsItemTheme.axaml` 内的静态模板声明。因为
ItemsControl 容器不是 owner 的模板子节点，子 Part 路由必须先经逻辑 `>` 到达容器、再经 `/template/` 进入
item 模板，形如 `> .semantic-item /template/ .semantic-item-x`。

## 1. Semantic Parts

| Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- |
| `root` | owner | `Steps` | `Single` | `Root` | `false` | `false` |
| `item` | `.semantic-item` | `StepsItem` | `Multiple` | `Selector` | `false` | `true` |
| `itemWrapper` | `.semantic-item-wrapper` | `Border` | `Multiple` | `Selector` | `false` | `true` |
| `itemIcon` | `.semantic-item-icon` | `TemplatedControl` | `Multiple` | `Selector` | `false` | `true` |
| `itemTitle` | `.semantic-item-title` | `ContentPresenter` | `Multiple` | `Selector` | `false` | `true` |
| `itemSubtitle` | `.semantic-item-subtitle` | `ContentPresenter` | `Multiple` | `Selector` | `false` | `true` |
| `itemSection` | `.semantic-item-section` | `Panel` | `Multiple` | `Selector` | `false` | `true` |
| `itemContent` | `.semantic-item-content` | `ContentPresenter` | `Multiple` | `Selector` | `false` | `true` |
| `itemRail` | `.semantic-item-rail` | `DashedBorder` | `Multiple` | `Selector` | `false` | `true` |

`root` 是控件自身，承载 `Current`、`Initial`、`Status`、`Percent`、`Type`、`Orientation`、
`TitlePlacement`、`SizeType`、`IsItemClickable` 等 public API、主题入口和状态投影，不声明 `.semantic-root`
marker。

`item` 的 marker 挂在 `StepsItem` 实例上，是运行时创建的语义标记：`Steps` 在容器准备时对每个容器写入
`semantic-item`。直接声明的 `StepsItem`、普通数据项生成的容器以及回收复用后重新准备的容器遵循同一规则。

七个子 Part 是 item 模板内的静态标记，声明在 `StepsItemTheme.axaml` 的对应节点上。它们的
`RuntimeCreated = true` 表示这些 Part 只随 item 容器的存在而存在：item 被创建时 marker 随模板出现，
item 被移除时随模板销毁；`Steps` 自身不包含任何子 Part marker。

## 2. Part 说明

### 2.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Steps` |
| Part | `root` |
| Selector | 不适用（owner 自身） |
| SelectorRoute | 不适用 |
| ContractType | `Steps` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Steps` owner |
| 职责 | 步骤条根语义区域，承载流程状态、布局入口和主题视觉。 |
| 相关 API | `Current`、`Initial`、`Status`、`Percent`、`Type`、`Orientation`、`TitlePlacement`、`SizeType`、`IsItemClickable` |
| 相关 Token | Steps Token + SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是 `Steps` owner 本身，在控件实例的整个生命周期内始终存在，每个实例恰好一个。它负责：

- 承载流程状态输入 `Current` / `Initial` / `Status` 与 `Percent`。
- 承载 `Type`、`Orientation`、`TitlePlacement`、`SizeType` 等布局与视觉入口。
- 承载 `IsItemClickable` 等交互状态。
- 作为 `item` 和七个子 Part owner-scoped Selector 的作用域边界。

`StepsTheme.axaml` 的根模板在 `PART_ItemsPresenter` 外包裹 `atom:DashedBorder`，并 TemplateBind
`Background`、`BackgroundSizing`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 以及
`BorderDashArray` / `BorderDashOffset`（映射到 `DashedBorder` 的 `StrokeDashArray` / `StrokeDaskOffset`）。
root 视觉定制直接作用于 owner 自身的这些属性，对应 Ant Design `styles.root` 的边框（含虚线）、背景与内边距
定制。默认值全部为空/零，不改变既有默认外观。

root 不表示模板中的 `PART_ItemsPresenter`、`StepsPanel` 或各 item 内部节点；这些节点不属于 root 契约。

### 2.2 `item`

| 字段 | 值 |
| --- | --- |
| Owner | `Steps` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| ContractType | `StepsItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `StepsItem` 容器 |
| 职责 | 承载单个步骤项的状态、内容与交互语义。 |
| 相关 API | `Header`、`SubHeader`、`Content`、`Icon`、`Status`、`IsEnabled` |
| 相关 Token | `ItemHeaderForeground`、`ItemSubHeaderForeground`、`ItemRailBackground` 实例语义覆盖之外的 Steps Token |
| 稳定性 | stable since 6.2.0 |

`item` 覆盖步骤条内的全部 `StepsItem` 容器：直接声明的 `StepsItem` 与普通数据项生成的容器完全等价。
marker 由 `Steps.PrepareContainerForItemOverride` 在容器准备时写入，容器回收复用后重新准备时再次写入；
移除后的容器不再属于该 owner 的 `item` 范围。

`SelectorRoute` 是 `> .semantic-item`：`StepsItem` 是 ItemsControl 的逻辑子节点，不是 owner 的模板子节点，
路由不经过 `/template/`，直接以逻辑 `>` 到达容器。

`ContractType` 为 `StepsItem`：它是 public 容器类型，同时提供 `Background`、`BorderBrush`、
`CornerRadius`、`Padding` 等条目样式能力，以及 `Header`、`SubHeader`、`Content` 内容契约。

### 2.3 item 子 Part

| Part | SelectorRoute | ContractType | AtomUI 节点 | 职责 |
| --- | --- | --- | --- | --- |
| `itemWrapper` | `> .semantic-item /template/ .semantic-item-wrapper` | `Border` | `ItemWrapper`（`StepsPanelItemFrame`） | item 包裹层，承载单项整体背景、边框、圆角与内边距。 |
| `itemIcon` | `> .semantic-item /template/ .semantic-item-icon` | `TemplatedControl` | `PART_Indicator`（`StepsItemIndicator`） | 数字、状态图标、Dot、自定义 Icon、Progress 和 Wave 目标。 |
| `itemTitle` | `> .semantic-item /template/ .semantic-item-title` | `ContentPresenter` | `HeaderPresenter` | 标题文本。 |
| `itemSubtitle` | `> .semantic-item /template/ .semantic-item-subtitle` | `ContentPresenter` | `SubHeaderPresenter` | 副标题文本。 |
| `itemSection` | `> .semantic-item /template/ .semantic-item-section` | `Panel` | `Section`（`StepsItemSectionPanel`） | item 内容区，承载标题行与详情内容的分组布局容器。 |
| `itemContent` | `> .semantic-item /template/ .semantic-item-content` | `ContentPresenter` | `ContentPresenter` | 步骤详情内容。 |
| `itemRail` | `> .semantic-item /template/ .semantic-item-rail` | `DashedBorder` | `Connector`（`PixelAlignedBorder`） | 当前 item 与下一个 item 的连接线。 |

七个 item 子 Part 的 marker 全部静态声明在 `StepsItemTheme.axaml` 的对应节点上，每个 item 模板恰好各一个。
路由先经逻辑 `>` 到达 `StepsItem` 容器，再经 `/template/` 进入 item 模板：

- `itemWrapper` 覆盖 item 包裹层。它是 item 模板的最外层容器，背景、边框与圆角类定制作用于此。
- `itemIcon` 覆盖 `PART_Indicator`。`StepsItemIndicator` 是 internal 类型，不能作为公共 Setter 依赖的最低
  类型，`TemplatedControl` 同时提供 `Background`、`BorderBrush`、`CornerRadius`、`Padding` 等图标容器
  样式能力。Indicator 的默认全圆来自 `StepsItemIndicatorTheme.axaml` 以 style 优先级设置的
  `StepsToken.IconContainerCornerRadius`；Semantic Style 的 Setter 以更高优先级直接覆盖，无需应用清除
  默认值。
- `itemTitle` / `itemSubtitle` / `itemContent` 覆盖三个 `ContentPresenter` 文本区域，适合字体、颜色、
  对齐等文本样式；`SubHeaderPresenter` 在 `SubHeader` 为 `null` 时隐藏，但 marker 仍保留在模板节点上。
- `itemSection` 覆盖 `Section`（`StepsItemSectionPanel`）。它是 internal 布局面板，ContractType 承诺其
  public 基类 `Panel`，承载 `HeaderPresenter`、`SubHeaderPresenter` 与 `ContentPresenter` 三个正文节点的
  分组与对齐，适合背景与布局型定制；不得依赖 `StepsItemSectionPanel` 类型。
- `itemRail` 覆盖 `Connector`。Connector 节点是 `PixelAlignedBorder`，ContractType 承诺其 public 基类
  `DashedBorder`，提供 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 等连接线样式能力；
  最后一个 item 的 Connector 隐藏，marker 仍保留。

`itemHeader` 没有对应的模板节点，不进入 Steps 的 Semantic Part 集合；标题行的整体分组与对齐由
`itemSection` 承载。

## 3. Selector 用法

应用侧使用 owner-scoped selector 和生成的 Style 类型，不手写 `/template/` 路径：

```xml
<Style Selector="atom|Steps.semantic-style-demo-root">
    <Setter Property="BorderBrush" Value="{atom:SharedTokenResource ColorBorder}" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="BorderDashArray" Value="4,2" />
    <Setter Property="CornerRadius" Value="{atom:SharedTokenResource BorderRadius}" />
    <Setter Property="Padding" Value="{atom:SharedTokenResource Padding}" />
</Style>

<Style Selector="atom|Steps.semantic-style-demo-object">
    <atom:StepsItemIconStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="CornerRadius" Value="10" />
    </atom:StepsItemIconStyle>
    <atom:StepsItemContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontStyle" Value="Italic" />
    </atom:StepsItemContentStyle>
</Style>

<Style Selector="atom|Steps[Type=Navigation].semantic-style-demo-root">
    <Setter Property="BorderBrush" Value="#1890FF" />
</Style>
```

`StepsItemStyle`、`StepsItemWrapperStyle`、`StepsItemIconStyle`、`StepsItemTitleStyle`、
`StepsItemSubtitleStyle`、`StepsItemSectionStyle`、`StepsItemContentStyle` 与 `StepsItemRailStyle` 位于
`AtomUI.Theme.Styling` 命名空间，由语义生成器根据 `Steps.SemanticParts.cs` 生成。生成类型已封装 owner 类型
保护和 `SelectorRoute`，`StepsItemIconStyle` 通过 `> .semantic-item /template/ .semantic-item-icon` 路由到
item 模板内的 Indicator 节点。owner-scoped selector 可以是状态 selector（如
`atom|Steps[Type=Navigation]`），对应上游把 `styles` 传成函数、按 props 决定返回值的用法。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML
编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。item 状态（Wait、Process、Finish、Error 与
当前项）由 `StepsItemTheme` / `StepsItemIndicatorTheme` 的主题 selector 表达；Semantic Style 的 Setter
以 trigger 优先级同时覆盖主题基础值与状态值，如果希望保留主题的某个状态视觉，应在生成的 Style 内用嵌套
selector 重新声明该状态的值。

## 4. 状态与数量语义

- `root` 始终恰好一个，不随状态、模板重套或 Items 变化增删。
- `item` 的 marker 数量跟随已实现 `StepsItem` 容器数。直接声明的 `StepsItem`、普通数据项生成的容器和
  回收复用后的容器都携带 marker；容器移除后 marker 随容器离开 owner 范围。
- 七个子 Part 每个 item 模板各声明一个静态 marker，数量跟随已实现 item 数；`SubHeaderPresenter` 在
  `SubHeader` 为 `null` 时隐藏、`Connector` 在最后一个 item 隐藏，但 marker 不随状态增删，隐藏节点不参与
  Gallery 预览的视觉解析。
- Items 变化只改变 marker 落在哪些容器上，不要求应用重建样式或重新匹配 selector。
- `EffectiveStatus`、`IsCurrent`、`IsLast` 或 `SubHeader` 值不参与 marker 判定。

## 5. 定制边界

- `root` 的定制直接作用于 owner 自身。可稳定依赖的根视觉属性为 `TemplatedControl` 的 `Background`、
  `BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`，以及 `Steps` 的 `BorderDashArray`、
  `BorderDashOffset`；根模板 `atom:DashedBorder` 对这些属性 TemplateBind（虚线属性映射到
  `StrokeDashArray` / `StrokeDaskOffset`），默认值不改变既有外观。
- `item` 的定制通过生成的 `StepsItemStyle` 表达，`x:SetterTargetType` 必须兼容 `StepsItem`。
- `itemWrapper` / `itemRail` 的定制分别通过 `StepsItemWrapperStyle` / `StepsItemRailStyle` 表达，
  `x:SetterTargetType` 必须兼容 `Border` / `DashedBorder`；不得依赖 `PixelAlignedBorder` 类型。
- `itemIcon` 的定制通过 `StepsItemIconStyle` 表达，`x:SetterTargetType` 必须兼容 `TemplatedControl`；
  不得依赖 `StepsItemIndicator` 类型。默认全圆由 `IconContainerCornerRadius` Token 以 style 优先级提供，
  Semantic Style Setter 直接覆盖。
- `itemTitle` / `itemSubtitle` / `itemContent` 的定制分别通过 `StepsItemTitleStyle` /
  `StepsItemSubtitleStyle` / `StepsItemContentStyle` 表达，`x:SetterTargetType` 必须兼容
  `ContentPresenter`；适合字体、颜色、对齐等文本样式，不得依赖 `HeaderPresenter` / `SubHeaderPresenter`
  节点身份以外的模板结构。
- `itemSection` 的定制通过 `StepsItemSectionStyle` 表达，`x:SetterTargetType` 必须兼容 `Panel`；不得依赖
  `StepsItemSectionPanel` 类型。
- 布局型 Setter（`Margin`、`Padding`、`Width`）作用于 item 的 Measure/Arrange，需按 StepsPanel /
  StepsItemLayoutPanel / StepsItemSectionPanel 的布局语义验证 owner 尺寸与对齐边界；`itemRail` 的几何主要由
  布局 Panel 决定，不适合表达 rail 的长度与方向。
- `PART_ItemsPresenter`、`StepsPanel`、`NavigationArrow` 与 `NavigationActiveIndicator` 明确不属于任何
  Semantic Part，不能通过 Semantic Style 承诺样式；实例级 Header、SubHeader 与 Connector 覆盖由
  `ItemHeaderForeground`、`ItemSubHeaderForeground`、`ItemRailBackground` 三项公开语义 API 表达，二者
  互不取代。
- 应用样式不得手写 `/template/` selector 穿透 `StepsItem`；需要子节点定制时使用生成的子 Part Style。

## 6. 兼容性与验证

- `Steps` 的 descriptor 公开 `root` + `item` + 七个 item 子 Part，与 Ant Design 6 的 `StepsSemanticType`
  对齐；`item` 与七个子 Part 均为 `Multiple`。
- `root` 是隐式 owner，不存在 `.semantic-root` marker。
- `StepsItemTheme.axaml` 含七个静态 `semantic-item-*` marker；`StepsTheme.axaml` 不含语义 marker。
  `item` 的 marker 由 `Steps` 在容器准备时写入。
- `item` 的 `SelectorRoute` 为 `> .semantic-item`，七个子 Part 为
  `> .semantic-item /template/ .semantic-item-x`；容器边界路由形状是稳定契约，删除或重命名 marker、
  改变 marker 类型或 cardinality 时必须同步生成 descriptor、主题 marker、运行时代码与回归测试。
- 运行时 marker 行为回归测试见
  `tests/AtomUI.Desktop.Controls.Tests/Steps/StepsSemanticPartTests.cs` 与
  `tests/AtomUI.Desktop.Controls.Tests/Steps/StepsRootFrameTests.cs`；
  Gallery 预览与样式示例验证见 `tests/AtomUIGallery.Tests/ShowCases/StepsShowCasePageTests.cs`。
