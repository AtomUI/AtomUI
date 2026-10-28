# Collapse Semantic Part 契约

本文档定义 `Collapse` 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Collapse 的整体设计见
[Collapse 桌面版架构设计](overview.md)，真实模板与生命周期见 [Collapse 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`Collapse` 主控件公开 `root`、`header`、`icon`、`title` 与 `body` 五个职责区域，与上游稳定 Semantic DOM 对齐。上游基线为
6.6.0 稳定发布的 `CollapseSemanticType` 与 Semantic DOM 演示：

- `header`、`body` 自上游 5.21.0 公开；
- `root`、`icon`、`title` 自上游 6.0.0 公开。

上游 Semantic DOM 以 `itemsAPI="items"` 组织：`header`、`icon`、`title`、`body` 按 item 出现（每个面板各一个），面板容器
本身（`.ant-collapse-item`）不是 Semantic key。AtomUI 五个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since`
统一为 `6.0`。

`CollapseItem` 不持有独立 Semantic descriptor：

- 上游 `Collapse` 只提供一个 owner 的 Semantic DOM；`Collapse.Panel` 没有独立公开 Semantic DOM Props。
- `CollapseItem` 是 Collapse 的公开子控件与运行时容器，其职责通过 `Collapse` 的 `header`、`icon`、`title`、`body` Part
  对外公开；容器本身与 item shell 边框不属于任何 Part。

### 1.1 `Collapse`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Collapse` |
| Part | `root` |
| Selector | Collapse 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Collapse` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Collapse owner（表面投影到 `PART_Frame`） |
| 职责 | Collapse root 是面板集合状态、视觉模式与根表面样式（背景、边框、圆角、内边距）的统一 owner。 |
| 相关 API | `IsAccordion`、`IsBorderless`、`IsGhostStyle`、`TriggerType`、`ExpandIconPosition`、`SizeType`、`IsMotionEnabled`、`ItemHeaderPadding`、`ItemContentPadding`、`Items`、`SelectedItems` |
| 相关 Token | CollapseToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner | `Collapse` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `> .semantic-scope-item /template/ .semantic-header` |
| Style Type | `CollapseHeaderStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `CollapseItem` 模板中的 `PixelAlignedBorder#PART_HeaderDecorator` |
| 职责 | 统一表示每个面板头部的背景、内边距、字体/行高、光标与交互视觉；对应上游 `.ant-collapse-header` 的 flex 布局、内边距、颜色、行高、光标与过渡动画职责。 |
| 相关 API | `SizeType`、`ItemHeaderPadding`、`TriggerType`、`IsGhostStyle`、`IsEnabled` |
| 相关 Token | `HeaderBg`、`HeaderPadding`、`CollapseHeaderPaddingSM`、`CollapseHeaderPaddingLG`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `Collapse` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `> .semantic-scope-item /template/ .semantic-icon` |
| Style Type | `CollapseIconStyle` |
| ContractType | `IconButton` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `CollapseItem` 模板中的 `IconButton#PART_ExpandButton` |
| 职责 | 统一表示展开/收起箭头的大小、对齐、边距与动效视觉；对应上游 `.ant-collapse-expand-icon` 的字体大小、过渡动画与旋转变换职责。 |
| 相关 API | `ExpandIcon`、`ExpandIconPosition`、`IsShowExpandIcon`、`IsSelected` |
| 相关 Token | `IconSizeSM`、`LeftExpandButtonMargin*`、`RightExpandButtonMargin*`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `Collapse` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `> .semantic-scope-item /template/ .semantic-title` |
| Style Type | `CollapseTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `CollapseItem` 模板中的 `ContentPresenter#PART_HeaderPresenter` |
| 职责 | 统一表示每个面板标题文字的布局、颜色、字体与对齐；对应上游 `.ant-collapse-title` 的 flex 自适应布局与边距职责。 |
| 相关 API | `Header`、`HeaderTemplate` |
| 相关 Token | `ColorTextHeading`、`ColorTextDisabled`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `Collapse` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `> .semantic-scope-item /template/ .semantic-body` |
| Style Type | `CollapseBodyStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `CollapseItem` 模板中的 `PixelAlignedBorder#PART_ContentFrame` |
| 职责 | 统一表示每个面板内容区域的内边距、颜色、背景与内容顶部分隔线；对应上游 `.ant-collapse-body` 的内边距、颜色与背景职责。 |
| 相关 API | `Content`、`ContentTemplate`、`ItemContentPadding`、`IsBorderless`、`IsGhostStyle` |
| 相关 Token | `ContentPadding`、`ContentBg`、`HeaderBg`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。`header` 与 `body` 的承载节点是
公开的 `PixelAlignedBorder`，`icon` 是公开的 `IconButton`，`title` 是公开的 `ContentPresenter`，均取节点真实 public 类型
作为最低依赖类型。

`header`、`icon`、`title`、`body` 的节点位于 `CollapseItem` 自己的模板内，而 `CollapseItem` 容器由 `Collapse` 的
ItemsControl 生命周期运行时创建（`TemplatedParent` 为 null），因此这四个 Part 声明 `RuntimeCreated=true` 并显式携带
SelectorRoute。Avalonia 的 `>` 步骤沿逻辑树（`LogicalParent`）行走，而 ItemsControl 生成的容器逻辑父级是 Collapse
owner 本身（并非运行时 ItemsPanel），所以路由从 owner 出发经一步 `>` 直达 `.semantic-scope-item` 容器，再以
`/template/` 进入容器模板到达 Part 节点。三个 scope marker（`.semantic-scope-items` / `.semantic-scope-panel` /
`.semantic-scope-item`）中只有 `.semantic-scope-item` 参与路由，前两者标识 items host 链、不单独发布为 Part，详见
[§3 Selector 用法](#3-selector-用法)。

## 2. Part 说明

### 2.1 root

`root` 是 `Collapse` owner 本身，在实例整个生命周期内始终存在，每个实例恰好一个。

它负责：

- 承载 `IsAccordion`、`IsBorderless`、`IsGhostStyle`、`TriggerType`、`ExpandIconPosition`、`SizeType`、`IsMotionEnabled`、
  `ItemHeaderPadding`、`ItemContentPadding` 等公共状态与面板集合。
- 承载 selection model 的 owner 职责；普通/手风琴模式的展开状态都由 owner 表达。
- 提供背景、边框、圆角、整体内边距与整体前景色，并作为全部 Part 的 owner-scoped Selector 作用域边界。

root 的表面样式经主题模板投影到根视觉 `PART_Frame`：`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 与
`Padding` 都作用于该表面。当前默认主题不设置根背景（默认透明），保持与既有外观一致；上游 antd 根背景默认为
`headerBg`（AtomUI `HeaderBg`），需要根背景默认值时通过 Semantic Setter、应用级样式或主题分支提供。适合通过 root 定制
整体背景、边框、圆角、对齐与整体内边距；需要按状态分支改变根样式时，在 owner Selector 上组合公开属性或伪类
（例如 `atom|Collapse[IsBorderless=True]`、`atom|Collapse[IsGhostStyle=True]`）。

root 不表示模板中的 `PART_Frame`、`PART_ItemsPresenter` 等任何内部节点；这些节点的名称、数量和层级不属于 root 契约。
Item 之间的分隔线由 item shell 的 `ItemBorderThickness` 承载，也不属于 root 契约。

### 2.2 header

`header` 表示每个面板的头部区域，每个 `CollapseItem` 模板恰好一个 `PixelAlignedBorder#PART_HeaderDecorator`，cardinality
为 `Multiple`（每个面板各一个）。节点始终存在于模板稳定结构中，`TriggerType`、选中状态与主题视觉分支都不增删节点。

它负责：

- 承载头部区域的背景、内边距、字体/行高与光标视觉；`TriggerType=Header` 时主题把光标设为 `Hand`。
- 把 `SizeType` 三档的 `DefaultHeaderPadding`（`CollapseHeaderPaddingSM` / `HeaderPadding` /
  `CollapseHeaderPaddingLG`）与 `OwnerHeaderPadding`（`ItemHeaderPadding`）经 `EffectiveHeaderPadding` 投影到 `Padding`。
- 把 `SizeType` 三档的字体与行高（`FontSizeLG`/`FontHeightLG`、`FontSize`/`FontHeight`）施加在头部文本上。

适合定制头部区域的 `Background`、`Padding`、`Cursor`、`TextElement.FontSize` / `TextBlock.LineHeight` 与 `BorderBrush`。
Semantic `Padding` / 字体 Setter 的优先级高于主题尺寸档 Setter，可以统一覆盖三档基线（上游 style-class 演示的
`padding: 12px 16px` 即通过该路径生效）。头部文字的颜色不在 header 上：`PixelAlignedBorder` 没有 `Foreground`，标题文字
颜色属于 `title` Part；header 的禁用态前景色由主题 selector 拥有。头部区域的命中测试与 `TriggerType` 语义由
`CollapseItem` 的输入逻辑拥有，不属于 header 的样式契约。

### 2.3 icon

`icon` 表示每个面板的展开/收起箭头，即 `IconButton#PART_ExpandButton`，cardinality 为 `Multiple`（每个面板各一个）。
`IsShowExpandIcon=False` 时节点通过 `IsVisible=false` 隐藏，但节点与 marker 仍属于模板稳定结构，Part 身份与数量不变。
这一行为与上游一致：antd 在 `showExpandIcon=false` 时不渲染箭头元素。

它负责：

- 承载箭头图标的尺寸（`IconSizeSM`）、对齐与边距；`ExpandIconPosition` Start/End 切换 `Grid.Column` 0/3 与对应尺寸档
  边距 Token。
- 承载 `ExpandIcon` 替换图标与展开/收起动效的视觉入口。

适合定制图标的 `IconWidth` / `IconHeight`、`Foreground`、`Margin` 与 `RenderTransform`。选中态的 `rotate(90deg)` 旋转由
主题 `^[IsSelected=True]` selector 拥有；Semantic `RenderTransform` 与选中态主题 Setter 按 Avalonia 原生优先级竞争（用户
样式优先级更高），但箭头旋转语义仍由 `IsSelected` 驱动，不建议用 Semantic `RenderTransform` 替代状态表达。
`ExpandIcon` 替换路径（用户自定义图标）只改变图标内容，不改变节点、marker 与 Part 身份。

### 2.4 title

`title` 表示每个面板的标题文字区域，即 `ContentPresenter#PART_HeaderPresenter`，cardinality 为 `Multiple`（每个面板各
一个）。`HeaderTemplate` 只替换内容呈现，不改变节点与 marker。

它负责：

- 承载 `Header` / `HeaderTemplate` 生成的内容，并提供标题文字的布局、颜色、字体与对齐。
- 默认前景色来自 `ColorTextHeading`，禁用态由主题 selector 投影 `ColorTextDisabled`。

适合定制标题的 `Foreground`、`FontSize`、`FontWeight`、`Margin` 与水平/垂直内容对齐。标题与 `AddOnContent` 的列排布、
Grid 结构不属于 title 契约。

### 2.5 body

`body` 表示每个面板的内容区域，即 `PixelAlignedBorder#PART_ContentFrame`，cardinality 为 `Multiple`（每个面板各一个）。
节点属于模板稳定结构，不随展开/收起增删；主题声明的 `ExpandMotion` / `CollapseMotion` 通过布局能力驱动
`ContentViewport#PART_ContentViewport`，收起稳定态隐藏内容视口（`IsVisible=false`）并释放属性覆盖，
body 节点自身的 Part 身份与数量不变。

它负责：

- 承载 `Content` / `ContentTemplate` 生成的面板内容，并提供内容区域的内边距、颜色、背景与文字样式。
- 把 `SizeType` 三档的 `DefaultContentPadding`（`CollapseContentPaddingSM` / `ContentPadding` /
  `CollapseContentPaddingLG`）与 `OwnerContentPadding`（`ItemContentPadding`）经 `EffectiveContentPadding` 投影到
  `Padding`。
- 默认 bordered 模式绘制内容顶部分隔线（`ContentBorderThickness`），Borderless 模式把背景设为 `HeaderBg`，Ghost 模式
  不绘制分隔线。

适合定制内容区域的 `Background`、`Padding`、`BorderBrush` / `BorderThickness` 与 `TextElement.*`。展开/收起动效的高度
动画由 motion actor 拥有，不属于 body 的样式契约；固定 `Height` / `Width` / Min/Max 类布局 Setter 不作为公共定制路径，
见 [§5 尺寸基线](#5-尺寸基线)。

## 3. Selector 用法

应用级样式先限定 Collapse owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护与
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Collapse">
        <atom:CollapseHeaderStyle x:SetterTargetType="atom:PixelAlignedBorder">
            <Setter Property="Background" Value="#F0F0F0" />
            <Setter Property="Padding" Value="12,16" />
        </atom:CollapseHeaderStyle>

        <atom:CollapseTitleStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#141414" />
        </atom:CollapseTitleStyle>

        <atom:CollapseIconStyle x:SetterTargetType="atom:IconButton">
            <Setter Property="IconWidth" Value="16" />
            <Setter Property="IconHeight" Value="16" />
        </atom:CollapseIconStyle>

        <atom:CollapseBodyStyle x:SetterTargetType="atom:PixelAlignedBorder">
            <Setter Property="Background" Value="#FFFFFF" />
        </atom:CollapseBodyStyle>
    </Style>
</Application.Styles>
```

对特定 Collapse class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Collapse.semantic-card[SizeType=Large]">
    <atom:CollapseHeaderStyle x:SetterTargetType="atom:PixelAlignedBorder">
        <Setter Property="Background" Value="#F5EFFF" />
    </atom:CollapseHeaderStyle>
</Style>
```

`header`、`icon`、`title`、`body` 是运行时 Part：它们位于运行时 `CollapseItem` 容器（`TemplatedParent` 为 null）的模板
内。Avalonia 的 `>` 步骤沿逻辑树（`LogicalParent`）行走，ItemsControl 生成的容器逻辑父级是 Collapse owner 本身而非
运行时 ItemsPanel，因此 SelectorRoute 从 owner 出发经一步 `>` 直达 `.semantic-scope-item`（运行时容器），再以
`/template/` 进入容器模板到达 Part 节点。`.semantic-scope-items`（`PART_ItemsPresenter`，owner 模板静态节点）与
`.semantic-scope-panel`（运行时 ItemsPanel）是 items host 链的标识 marker，用于测试与工具定位，不参与 Part 路由；
三个 scope marker 均不单独发布为 Part。生成 Style 已封装完整路由，用户样式不得复制这些 route，也不得依赖 `PART_*`
名称或内部节点层级。

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `PixelAlignedBorder.semantic-header` 或 `:is(PixelAlignedBorder).semantic-header`。
- `ContentPresenter.semantic-title` 或 `IconButton.semantic-icon`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 连续穿过子控件模板的多个 `/template/`。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。

## 4. 状态与数量语义

marker 身份不随任何运行时状态切换增删；Part 节点数量只随面板集合（items）增减：

| 状态 | root | header | icon | title | body | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| 默认（N 个面板） | 1 | N | N | N | N | 每个面板各携带四个 Part 节点。 |
| 全部收起 / 部分展开 | 1 | N | N | N | N | `IsSelected` 只改变 content motion 与箭头旋转，不增删节点。 |
| `IsShowExpandIcon=False` | 1 | N | N | N | N | `PART_ExpandButton` 隐藏但节点与 marker 仍存在。 |
| `TriggerType` Icon/Header | 1 | N | N | N | N | 只改变命中区域与光标，不增删节点。 |
| `ExpandIconPosition` Start/End | 1 | N | N | N | N | 只切换 `Grid.Column` 与边距，marker 不变。 |
| `SizeType` Large/Middle/Small/Custom | 1 | N | N | N | N | 只改变 Padding/字体/边距 Token 档位。 |
| `IsAccordion` 切换 | 1 | N | N | N | N | 只改变 selection mode，不增删节点。 |
| `IsBorderless` / `IsGhostStyle` | 1 | N | N | N | N | 只改变分隔线与背景的主题视觉。 |
| Item disabled | 1 | N | N | N | N | 只改变禁用态前景色，节点不变。 |
| 空集合 | 1 | 0 | 0 | 0 | 0 | 没有容器就没有 Part 节点，`Multiple` 的 0 是合法运行时状态。 |
| items reset/replace/clear | 1 | 随集合 | 随集合 | 随集合 | 随集合 | 节点数量跟随容器增删；容器复用/回收不增删 marker。 |
| 模板重应用 | 1 | N | N | N | N | 静态 marker 随模板重建；容器上的 scope marker 一次性建立后保留。 |

容器由 `SelectingItemsControl` 的 ItemsControl 生命周期创建。`.semantic-scope-item` 在容器创建路径与
`PrepareContainerForItemOverride` 幂等添加（覆盖 item 本身即为容器、不经过 `CreateContainerForItemOverride` 的路径），
`.semantic-scope-panel` 在默认 ItemsPanel 创建时一次性建立；prepare、clear、recycle、items 集合变化与模板重应用都不
增删这些 marker，保证复用容器重新进入面板时 Part 身份不变。

## 5. 尺寸基线

Collapse 的尺寸链与 Semantic Setter 的关系：

| 项目 | 内容 |
| --- | --- |
| 完整尺寸分支 | `Large`、`Middle`、`Small`、`Custom` 四档（`CustomizableSizeType`），默认 `Middle`；`Collapse.SizeType` 是 public 属性（`ICustomizableSizeTypeAware`），经 `PrepareContainerForItemOverride` 投影到每个 `CollapseItem`。 |
| 布局 owner | 头部 `PART_HeaderDecorator`（`Padding=EffectiveHeaderPadding` + `TextElement.FontSize`/`TextBlock.LineHeight`）、内容 `PART_ContentFrame`（`Padding=EffectiveContentPadding`）、箭头 `PART_ExpandButton`（`IconSizeSM` 图标 + 按位置/尺寸档的边距）。面板总高度由内容自然测量 + `PART_ContentViewport` 的动效（Core `Motion` 的尺寸/透明度进度）拥有。 |
| Token 映射 | Middle/Custom → `HeaderPadding`、`ContentPadding`、`FontSize`/`FontHeight`、`Left/RightExpandButtonMargin`；Large → `CollapseHeaderPaddingLG`、`CollapseContentPaddingLG`、`FontSizeLG`/`FontHeightLG`、`*MarginLG`；Small → `CollapseHeaderPaddingSM`、`CollapseContentPaddingSM`、`FontSize`/`FontHeight`（Small 与 Middle 同字号）、`*MarginSM`。 |
| 状态矩阵 | 展开/收起、disabled、ghost、borderless、TriggerType、ExpandIconPosition 只切换主题 selector 视觉，不改变 marker 与节点数量，见 [§4](#4-状态与数量语义)。 |
| 外部映射 | antd `size="small"` → `Small`；antd 默认（middle）→ `Middle`（及 `Custom`）；antd `size="large"` → `Large`。依据：antd Collapse 的 `size` 三档驱动 `headerPadding*`、`contentPadding*`、`fontSize*` 与图标尺寸；AtomUI 用同构三档 Token 表达，`Custom` 复用 Middle 基线并把完整控制权交给用户。 |
| 失败回归 | 给 `header` 设置固定 `Height`/`MinHeight` 会绕过 Padding + 字体自然测量，多行标题与三档尺寸基线失效。展开/收起动效由 Core 共享的 `Motion` 拥有：动效期间以尺寸/透明度进度驱动内容视口，固定 `body` Height 会与该进度竞争，因为 actor 的视口尺寸覆盖内容测量。Semantic Setter 中不应设置固定 `Height`/`Width`/Min/Max 布局值；改变头部/内容间距应通过 `ItemHeaderPadding`/`ItemContentPadding`、Token 或主题分支实现。 |

`header` / `body` 的 `Background`、`Padding`、字体与边框 Setter 是公共定制路径（Semantic Setter 优先级高于主题尺寸档
TemplateBinding）；固定 `Height` / `Width` / Min/Max 类布局 Setter 会与尺寸档链或 content motion 的高度动画竞争 Avalonia
属性优先级，因此不作为公共定制路径。需要改变头部/内容间距时应通过 `ItemHeaderPadding` / `ItemContentPadding`、Token 或
主题分支实现。

## 6. 定制边界

以下区域明确不属于 Collapse Semantic Part：

- `CollapseItem` 容器本身与 item shell 边框（面板之间的底部分隔线）；上游不公开 `.ant-collapse-item`，容器上的
  `.semantic-scope-item` 只是路由用 scope marker。
- `PART_MainLayout`、`PART_AddOnContentPresenter`（`AddOnContent` 附加内容区域）与 `PART_ContentViewport`（展开/收起
  动效 actor）；`item`、`motion`、`content` 的旧分类名不属于 Semantic Part，节点映射以本文档为准。
- 展开/收起动效由 Motion 播放协议与内容视口的布局租约拥有（真实占位、裁剪与透明度），不属于 `body` Part
  的样式契约。
- 用户自定义 `ItemsPanel` 不影响 Part 路由（容器逻辑父级始终是 Collapse owner）；建议自定义面板根节点声明
  `Classes.semantic-scope-panel="True"` 以保持 items host 链标识一致。
- 用户 `HeaderTemplate` / `ContentTemplate` 模板生成的子树。
- `PART_*` 名称、internal 类型、状态转换器与 motion phase。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间
节点的固定测量、主题尺寸绑定或动效高度动画约束，应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`header`、`icon`、`title`、`body`，字段值与本文表格一致（后四者为 `Multiple`、
  `RuntimeCreated=true` 且携带显式 SelectorRoute，`ContractType` 为公开类型 `PixelAlignedBorder` / `IconButton` /
  `ContentPresenter`）。
- `CollapseItemTheme.axaml` 的 `semantic-header`、`semantic-icon`、`semantic-title`、`semantic-body` 静态 marker 各一个；
  `CollapseTheme.axaml` 的 `semantic-scope-items` 静态存在；`.semantic-scope-panel` 在默认 ItemsPanel 创建时建立，
  `.semantic-scope-item` 在容器创建与 prepare 路径幂等建立。
- N 个面板时四个 Part 各 N 个节点；空集合为 0；items reset/replace/clear 后数量跟随容器；容器复用/回收与模板重应用
  不增删 marker。
- `IsShowExpandIcon=False` 时 `semantic-icon` 节点隐藏但存在；`TriggerType`、`ExpandIconPosition`、`SizeType`、
  `IsAccordion`、`IsBorderless`、`IsGhostStyle`、disabled 与展开/收起切换不改变 marker 数量。
- root 表面投影生效：`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding` 作用于 `PART_Frame`。
- `header` / `body` 的背景与 Padding、`title` 的前景色、`icon` 的图标尺寸在 Light/Dark 下保持有效；Semantic Style 覆盖
  不与默认主题（不消费 `.semantic-*`）产生 selector activator 开销。
- 默认主题不消费 `.semantic-*`；未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描；scope 与 Part marker 通过静态 AXAML class 与生成常量在
  既有创建路径一次性添加，不引入 VisualTree 搜索、动态 marker 绑定或运行时 AXAML 解析。
