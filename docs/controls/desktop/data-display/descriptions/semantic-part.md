# Descriptions Semantic Part 契约

本文档定义 Descriptions 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。Descriptions 的整体设计见
[Descriptions 桌面版架构设计](overview.md)，真实模板节点、生成视觉与生命周期见
[Descriptions 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Descriptions 只有一个 public descriptor owner：`Descriptions`。`DescriptionItem` 是非视觉数据对象，内部生成的
`DescriptionDefaultItem`、`DescriptionBorderedItemLabel` 和 `DescriptionBorderedItemContent` 只实现视觉承载，不建立独立
descriptor。

### 1.1 `Descriptions`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Descriptions` |
| Part | `root` |
| Selector | Descriptions 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Descriptions` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Descriptions owner |
| 职责 | Descriptions root 是数据、布局、尺寸、边框和响应式状态的统一 owner。 |
| 相关 API | 全部 Descriptions public API |
| 相关 Token | DescriptionsToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner | `Descriptions` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `/template/ .semantic-header` |
| Style Type | `DescriptionsHeaderStyle` |
| ContractType | `DockPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `HeaderLayout` |
| 职责 | 承载标题与辅助内容的完整头部布局区域。 |
| 相关 API | `Header`、`HeaderTemplate`、`Extra`、`ExtraTemplate` |
| 相关 Token | `HeaderMargin` |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `Descriptions` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `DescriptionsTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `HeaderPresenter` |
| 职责 | 展示 Header 内容及其模板结果。 |
| 相关 API | `Header`、`HeaderTemplate` |
| 相关 Token | `TitleColor`、SharedToken typography |
| 稳定性 | stable since 6.2.0 |

#### `extra`

| 字段 | 值 |
| --- | --- |
| Owner | `Descriptions` |
| Part | `extra` |
| Selector | `.semantic-extra` |
| SelectorRoute | `/template/ .semantic-extra` |
| Style Type | `DescriptionsExtraStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ExtraPresenter` |
| 职责 | 展示头部辅助内容及其模板结果。 |
| 相关 API | `Extra`、`ExtraTemplate` |
| 相关 Token | `ExtraColor`、SharedToken typography |
| 稳定性 | stable since 6.2.0 |

#### `label`

| 字段 | 值 |
| --- | --- |
| Owner | `Descriptions` |
| Part | `label` |
| Selector | `.semantic-label` |
| SelectorRoute | `/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-label` |
| Style Type | `DescriptionsLabelStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个已物化 DescriptionItem 的 label presenter |
| 职责 | 展示描述项标签，并提供重复标签区域的统一局部样式入口。 |
| 相关 API | `Items`、`ItemsSource`、`DescriptionItem.Label` |
| 相关 Token | `LabelBg`、`LabelColor`、`ItemPadding*`、`ColonMargin` |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `Descriptions` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content` |
| Style Type | `DescriptionsContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个已物化 DescriptionItem 的 content presenter |
| 职责 | 展示描述项内容，并提供重复内容区域的统一局部样式入口。 |
| 相关 API | `Items`、`ItemsSource`、`DescriptionItem.Content` |
| 相关 Token | `ContentColor`、`ItemPadding*`、SharedToken typography |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`header`、`title` 和 `extra` 是根 ControlTemplate 的静态节点；`label` 和
`content` 随 `DescriptionItem` 生成视觉物化，并由内部 item control 的模板接入路径把生成的 selector class 添加到目标
presenter，因此使用 `RuntimeCreated=true`。

## 2. Part 说明

### 2.1 root

root 是 Descriptions owner 本身，在控件实例生命周期内始终存在且恰好一个。它负责 `Items`、响应式列数、布局方向、边框、
尺寸和 Header/Extra 显隐状态，也是其余 Part 的 owner scope。

适合在 root 上定制整体 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、Margin、Opacity、Effect、
对齐和基于 public 属性的状态组合。默认模板通过内部 `RootFrame` 把标准表面属性投影到包含 Header 和内容区的整体表面，
因此 root 的边框、圆角和 Padding 会同时约束完整 Descriptions，而不是只作用于 `ContentFrame`。

root 不表示 `RootFrame`、`ContentFrame`、`PART_GridLayout`、内部生成控件或用户内容子树；这些节点仍是可替换的模板实现，
应用必须在 Descriptions owner 上设置 root 属性，不能直接选择内部 frame。

### 2.2 header

header 是完整头部布局区域。内置模板始终创建该节点；当 `Header` 与 `Extra` 都为空时，节点通过
`IsHeaderLayoutVisible=false` 隐藏而不是销毁，因此 cardinality 保持 `Single`。

适合定制 Margin、Background、Opacity、对齐和子项间距。header 只承诺头部布局边界，不公开 Dock 顺序、内部 Name 或
title/extra 之外的辅助容器。

### 2.3 title

title 是唯一 Header presenter。`Header=null` 时 presenter 仍存在，只是没有用户内容。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`LineHeight`、`Opacity`、Padding 和对齐。`HeaderTemplate` 创建的用户子树
不属于 title Part，应用不能依赖该子树的类型或层级。

### 2.4 extra

extra 是唯一辅助内容 presenter。`Extra=null` 时 presenter 仍存在，只是没有用户内容。

适合定制 `Foreground`、字体、Opacity、Padding、Margin 和对齐。`ExtraTemplate` 创建的用户子树不属于 extra Part。

### 2.5 label

每个已物化 `DescriptionItem` 恰好对应一个 label target。普通水平、纵向非边框和纵向边框模式由
`DescriptionDefaultItem` 的 label presenter 实现；水平边框模式由 label cell 内的 presenter 实现。`Layout` 或
`IsBordered` 改变时可以替换目标节点，但 Part 名称、`ContractType` 和每 item 一个 label 的语义不变。

适合定制 `Foreground`、`Background`、字体、Opacity、Padding、Margin、MinWidth 和对齐。对 Padding、Margin、Width 等
布局型属性进行覆盖时，必须同时验证水平、水平边框、纵向和纵向边框四种组合，以及 Large、Middle、Small 三档尺寸。

水平 bordered 模式的默认 Padding、Background、Foreground 和文本排版基线必须落在 label presenter 上，外层 label cell
只负责边框几何。冒号、label cell 外框、内部 separator、生成控件类型和 `DescriptionItem` 数据对象不属于 label Part。

### 2.6 content

每个已物化 `DescriptionItem` 恰好对应一个 content target。普通水平、纵向非边框和纵向边框模式由
`DescriptionDefaultItem` 的 content presenter 实现；水平边框模式由 content cell 内的 presenter 实现。布局模式重建视觉时
可以替换 target，但每 item 一个 content 的公共语义不变。

适合定制 `Foreground`、`Background`、字体、Opacity、Padding、Margin、MinWidth 和对齐。`DescriptionItem.Content` 创建的
用户控件树不属于 content Part。水平 bordered 模式的默认 Padding、Background、Foreground 和文本排版基线必须落在
content presenter 上，外层 content cell 只负责边框几何。

## 3. Selector 用法

应用级样式先限定 Descriptions owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Descriptions">
        <atom:DescriptionsHeaderStyle x:SetterTargetType="DockPanel">
            <Setter Property="Margin" Value="0,0,0,12" />
        </atom:DescriptionsHeaderStyle>

        <atom:DescriptionsTitleStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:DescriptionsTitleStyle>

        <atom:DescriptionsExtraStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Opacity" Value="0.8" />
        </atom:DescriptionsExtraStyle>
    </Style>
</Application.Styles>
```

label/content 位于运行时生成的 item control 模板中。生成的 Style 内部保留根模板 items scope、直接 item 和 item 模板的
完整 route：

```xml
<Application.Styles>
    <Style Selector="atom|Descriptions">
        <atom:DescriptionsLabelStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="DarkSlateBlue" />
        </atom:DescriptionsLabelStyle>

        <atom:DescriptionsContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:DescriptionsContentStyle>
    </Style>
</Application.Styles>
```

对特定业务 class 或状态定制时，把限定条件放在 owner 一侧：

```xml
<Style Selector="atom|Descriptions.compact[IsBordered=True]">
    <atom:DescriptionsLabelStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Padding" Value="8,4" />
    </atom:DescriptionsLabelStyle>
</Style>
```

不得使用以下写法：

- `ContentPresenter.semantic-label`、`:is(ContentPresenter).semantic-label` 等类型前缀。
- `atom|Descriptions /template/ .semantic-label`；该 selector 要求 label 的直接 `TemplatedParent` 是 Descriptions，与真实结构不符。
- `atom|Descriptions .semantic-label` 或 `atom|Descriptions .semantic-content`；Avalonia logical descendant 会继续穿过嵌套
  Button、Descriptions 和其他 Semantic owner，无法表达最近 owner 边界。
- internal item 类型、`PART_GridLayout` 或节点 Name；完整 route 只使用公开 owner 和稳定 `.semantic-scope-*` marker。
- 绕过 descriptor `SelectorRoute`，自行穿透未建模的内部模板或拼接未发布的中间节点。
- 直接复制完整 route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `DescriptionDefaultItem`、`DescriptionBorderedItemLabel`、`DescriptionBorderedItemContent`、`PART_*`、Name 或视觉层级。

## 4. 状态与数量语义

| 状态 | root | header | title | extra | label | content |
| --- | --- | --- | --- | --- | --- | --- |
| 空 Items | 1 | 1 | 1 | 1 | 0 | 0 |
| N 个已物化 item | 1 | 1 | 1 | 1 | N | N |
| Header/Extra 均为空 | 1 | 1，隐藏 | 1 | 1 | N | N |
| 水平非边框 | 1 | 1 | 1 | 1 | N | N |
| 水平边框 | 1 | 1 | 1 | 1 | N | N |
| 纵向非边框 | 1 | 1 | 1 | 1 | N | N |
| 纵向边框 | 1 | 1 | 1 | 1 | N | N |
| Layout/IsBordered 切换 | 1 | 1 | 1 | 1 | N 个新 target | N 个新 target |

`Multiple` 表示同一 owner 下该职责可以重复出现。空集合没有 item-scoped target；非空集合中 label/content 的 target 数量
始终与当前已物化 `Items.Count` 相等。`ItemsSource` 中不是 `DescriptionItem` 的值不会产生 target。

## 5. 定制边界

以下区域明确不属于 Descriptions Semantic Part：

- `RootFrame`、`ContentFrame`、`PART_GridLayout`、Grid row/column definition 和响应式布局算法。
- `DescriptionDefaultItem`、`DescriptionBorderedItemLabel`、`DescriptionBorderedItemContent` 和 `DescriptionBorderedCell` 类型。
- 冒号、separator、cell 外框、有效边框厚度和 last-row/last-column 状态。
- `DescriptionItem` 非视觉数据对象及其 binding expression。
- Header、Extra、Label 或 Content 的用户模板/内容创建的子树。

Descriptions 的 `SizeType` 只支持 `Large`、`Middle`、`Small`，不提供 `Custom` 分支。预设尺寸负责默认 RowSpacing 和边框模式
Padding 基线；Semantic Style 对 label/content 同一属性的合法 Setter 按 Avalonia 原生优先级生效。控件内部不得使用固定
Height 或额外本地值压制这些 Setter。

四种布局的 label/content target 均为 `ContentPresenter`。水平 bordered 模式不得把默认 Padding 或 Background 留在外层
cell 后再允许用户定制 presenter；默认基线必须通过 `TemplateBinding` 投影到 presenter，确保 Semantic Padding 是替换而非
嵌套叠加，Semantic Background 覆盖完整 cell 内容区域。

布局型 Setter 生效后，最终尺寸仍由 presenter、cell 外框、Grid 和 root 共同 Measure/Arrange。排查时必须先读取目标
presenter 的有效属性值，再检查父级 Min/Max、Padding、Margin、裁剪和列宽约束，不能把跨节点布局约束误判为 Semantic
Style 优先级失效。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一适用布局缺少 marker，均属于公共
主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`header`、`title`、`extra`、`label`、`content`，字段值与本文一致。
- root 的标准 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 和 `Padding` 必须投影到包含 Header 与内容区的
  整体表面，默认值不得改变未定制 Descriptions 的视觉和布局。
- 根模板中 header/title/extra marker 各一个，空内容只改变可见性，不改变 cardinality。
- 四种 Layout/IsBordered 组合中，每个 item 都产生一个 label 和一个 content target。
- `Items` 增删、reset、替换与 Layout/IsBordered 重建后，旧 target 释放且新 target 保持 marker。
- 静态 Part 使用单段 `/template/` route；运行时 item Part 使用 descriptor 中的多段 template/child route，并以
  `x:SetterTargetType` 编译和命中。
- label/content route 不命中 Extra 内嵌 Button 的 `.semantic-content`，也不命中 content 中嵌套 Descriptions 的同名 Part。
- Large、Middle、Small 三档在所有布局中把完整尺寸基线传递到生成视觉；水平 bordered presenter 只有一层默认 Padding，
  Semantic Padding/Background/Foreground 覆盖保持有效。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加运行时扫描、VisualTree 遍历或反射发现。
- 生成 descriptor、静态 class marker 和运行时 item marker 在 NativeAOT 路径中保持静态可达。
