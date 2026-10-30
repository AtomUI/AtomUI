# Expander Semantic Part 契约

本文档定义 `Expander` 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Expander 的整体设计见
[Expander 桌面版架构设计](overview.md)，真实模板与生命周期见 [Expander 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`Expander` 公开 `root`、`header`、`icon`、`title` 与 `body` 五个职责区域，与上游稳定 Semantic DOM 对齐。上游基线为
6.6.0 稳定发布的 `CollapseSemanticType` 与 Semantic DOM 演示：

- `header`、`body` 自上游 5.21.0 公开；
- `root`、`icon`、`title` 自上游 6.0.0 公开。

Expander 是单面板折叠容器，与上游 `Collapse` 的单个面板承担同一产品职责：Header 表达内容主题，展开图标表达折叠状态
和方向，Content 承载可延迟阅读的内容。上游 `Collapse` 的 Semantic DOM 按 item 组织，而 AtomUI `Expander` 恰好是
“一个面板”的独立 owner，因此两者按职责一一对应，不需要新的上游 owner。

准入依据按[全量改造设计 §2.1](../../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md)第 4 条
“AtomUI 控件与该公开 owner 的产品职责直接对应”判定：上游 owner 数量不是必要条件，职责对应关系才是。Expander 直接消费
上游 `Collapse` 已公开的五个语义键，Part 名称与 Collapse 保持同构。

范围变更（2026-09-15）：Expander 的原排除判定经用户指令撤销。原判定以“上游只有 `Collapse` 一个 owner、`Collapse.Panel`
没有独立 API”为由拒绝映射，但该理由检验的是 owner 数量而非职责对应关系，与 §2.1 第 4 条不符。Expander 以自身 public
owner 独立通过准入 Gate，移入第二批计划执行（见
[第二批任务清单](../../../../superpowers/plans/2026-08-12-semantic-part-batch-2-collections-containers.md)任务 16）。

AtomUI 五个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since` 统一为 `6.2.0`。

Expander 不引入额外 owner：它是单面板控件，没有 item 容器或独立子控件 owner，五个 Part 全部属于 `Expander` 自身。

### 1.1 `Expander`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Expander` |
| Part | `root` |
| Selector | Expander 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Expander` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Expander owner（根边框投影到 `PART_Frame`） |
| 职责 | Expander root 是单面板展开状态、展开方向、视觉模式与根边框样式的统一 owner。 |
| 相关 API | `IsExpanded`、`ExpandDirection`、`IsBorderless`、`IsGhostStyle`、`BorderThickness`、`TriggerType`、`ExpandIconPosition`、`SizeType`、`IsMotionEnabled`、`HeaderPadding`、`ContentPadding`、`Header`、`Content`、`AddOnContent` |
| 相关 Token | ExpanderToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner | `Expander` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `/template/ .semantic-header` |
| Style Type | `ExpanderHeaderStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `PixelAlignedBorder#PART_HeaderDecorator` |
| 职责 | 统一表示头部区域的背景、内边距、字体/行高与命中光标；对应上游 `.ant-collapse-header` 的内边距、颜色、行高、光标与过渡动画职责。 |
| 相关 API | `SizeType`、`HeaderPadding`、`TriggerType`、`IsGhostStyle`、`IsEnabled`、`ExpandDirection` |
| 相关 Token | `HeaderBg`、`HeaderPadding`、`HeaderPaddingSM`、`HeaderPaddingLG`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `Expander` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `ExpanderIconStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `IconButton#PART_ExpandButton` |
| 职责 | 统一表示展开/收起箭头的大小、位置、边距与旋转视觉；对应上游 `.ant-collapse-expand-icon` 的字体大小、过渡动画与旋转变换职责。 |
| 相关 API | `ExpandIcon`、`ExpandIconPosition`、`IsShowExpandIcon`、`IsExpanded`、`ExpandDirection`、`HeaderPadding`、`TriggerType`、`IsEnabled` |
| 相关 Token | `IconSizeSM`、`LeftExpandButtonHMargin`、`RightExpandButtonHMargin`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `Expander` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `ExpanderTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#PART_HeaderPresenter` |
| 职责 | 统一表示标题文字的布局、颜色、字体与对齐；对应上游 `.ant-collapse-title` 的自适应布局与边距职责。 |
| 相关 API | `Header`、`HeaderTemplate`、`SizeType`、`IsEnabled`、`HeaderPadding` |
| 相关 Token | `ColorTextHeading`、`ColorTextDisabled`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `Expander` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `ExpanderBodyStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#PART_ContentPresenter` |
| 职责 | 统一表示内容区域的内边距、背景与内容呈现；对应上游 `.ant-collapse-body` 的内边距、颜色与背景职责。 |
| 相关 API | `Content`、`ContentTemplate`、`ContentPadding`、`SizeType`、`IsBorderless`、`IsGhostStyle` |
| 相关 Token | `ContentPadding`、`ContentPaddingSM`、`ContentPaddingLG`、`ContentBg`、`HeaderBg`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。`header` 的承载节点是公开的
`PixelAlignedBorder`，`icon` 是公开的 `IconButton`，`title` 与 `body` 是公开的 `ContentPresenter`，均取节点真实 public
类型作为最低依赖类型。

`header`、`icon`、`title`、`body` 全部是 `ExpanderTheme.axaml` 单一 `ControlTemplate` 内的静态节点，`TemplatedParent` 为
Expander owner 本身，因此四个 Part 声明 `RuntimeCreated=false` 且不携带显式 `SelectorRoute`；生成器按
[Semantic Part Generator §2.3](../../../../modules/generator/semantic-part-generator.md)把静态根模板 Part 的 route 规范化为
`/template/ .<SelectorClass>`。`header` 位于 `LayoutTransformControl#PART_HeaderLayoutTransform` 内部，`body` 位于
`ContentViewport#PART_ContentViewport` 内部，但两者都不跨越第二层模板边界，`TemplatedParent` 仍是 Expander
owner，因此单一 `/template/` 路由已经足够，不需要为它们声明更宽的 logical descendant 路由。Expander 没有任何由 C# 创建
并注入 marker 的 Part，也不存在跨视觉根 Part。

`body` 的数量语义是 `Optional` 而不是 `Single`：折叠稳定态下 `PART_ContentViewport` 的 `IsVisible=false`，其内部
`ContentPresenter` 只保留逻辑子级、不挂接视觉子级，因此 `body` 节点从视觉树中**完全缺席**（不是隐藏）；展开后才物化。
`header` / `icon` / `title` 是模板常驻节点，折叠与展开都存在于视觉树，因此声明 `Single`。该结论由 Gate B 的 selector 命中
测试实测得出，见 [§7 兼容性与验证](#7-兼容性与验证)。

## 2. Part 说明

### 2.1 root

`root` 是 `Expander` owner 本身，在实例整个生命周期内始终存在，每个实例恰好一个。

它负责：

- 承载 `IsExpanded`、`ExpandDirection`、`IsBorderless`、`IsGhostStyle`、`TriggerType`、`ExpandIconPosition`、`SizeType`、
  `IsMotionEnabled`、`HeaderPadding`、`ContentPadding` 等公共状态。
- 承载 `Header` / `Content` 与 `AddOnContent` 的 owner 职责。
- 作为全部 Part 的 owner-scoped Selector 作用域边界。

root 的根边框经主题模板投影到 `PART_Frame`：`BorderThickness` 由 owner 属性经内部 `EffectiveBorderThickness` 投影，
`IsBorderless` / `IsGhostStyle` 会把有效边框厚度归零。`PART_Frame` 的 `BorderBrush` 与 `CornerRadius` 当前由默认主题的
`ColorBorder` / `ExpanderBorderRadius` 提供，`Background` 与 `Padding` 未在根表面设置，也**不从 owner 属性投影**——这是
Expander 与 Collapse 的一处真实差异，见 [§6 定制边界](#6-定制边界)。

`PART_Frame` 同时开启 `ClipToBounds` 与 `ClipContentToCornerRadius`。后者不是可选优化：`PART_Frame` 自己绘制圆角，
而 header/body 是它的子节点且各自带背景，`ClipToBounds` 只做矩形裁剪，缺少圆角裁剪时子节点的不透明背景会平铺到方形边界、
覆盖掉父节点画出的圆角。默认主题的 `HeaderBg` 是 alpha≈2% 的 `ColorFillAlter`，因此该缺陷在默认外观下几乎不可见；一旦
通过 Semantic Part 或 `IsGhostStyle` 设置不透明背景就会显形。裁剪由 `PART_Frame` 统一施加于整棵子树，与展开状态、
`ExpandDirection`、`IsBorderless` / `IsGhostStyle` 均无关，也不改变任何 Part 的命中数量。

适合通过 root 定制整体边框厚度、视觉模式和全部状态型行为；需要按状态分支改变 root 时，在 owner Selector 上组合公开属性或
伪类（例如 `atom|Expander[IsBorderless=True]`、`atom|Expander:expanded`）。需要改变根表面背景、圆角或整体内边距时，应替换
owner `ControlTheme`（整控件结构替换），不能依赖 root Semantic Setter。

root 不表示模板中的 `PART_Frame`、`PART_MainLayout` 等任何内部节点；这些节点的名称、数量和层级不属于 root 契约。

### 2.2 header

`header` 表示头部区域，即 `PixelAlignedBorder#PART_HeaderDecorator`，cardinality 为 `Single`。节点位于模板稳定结构中，
`TriggerType`、`SizeType`、视觉模式与展开状态都不增删节点。

它负责：

- 承载头部区域的背景、内边距、字体/行高与命中光标；`TriggerType=Header` 时主题把光标设为 `Hand`。
- 把 `SizeType` 三档的默认头部内边距（`HeaderPaddingSM` / `HeaderPadding` / `HeaderPaddingLG`）投影到 `Padding`；显式
  `HeaderPadding` 经 `:custom-header-padding` 伪类覆盖三档基线。
- 把 `SizeType` 三档的字体与行高（`FontSizeLG`/`FontHeightLG`、`FontSize`/`FontHeight`）施加在头部文本上。
- 承载横向展开方向（`ExpandDirection=Left`/`Right`）下经 `LayoutTransformControl` 旋转后的头部区域。

适合定制头部区域的 `Background`、`Padding`、`Cursor`、`TextElement.FontSize` / `TextBlock.LineHeight` 与 `BorderBrush`。
Semantic `Padding` / 字体 Setter 的优先级高于主题尺寸档 Setter，可以统一覆盖三档基线。头部文字的颜色不在 header 上：
`PixelAlignedBorder` 没有 `Foreground`，标题文字颜色属于 `title` Part；header 的禁用态前景色由主题 selector 拥有。头部区域的
命中测试与 `TriggerType` 语义由 `Expander` 的输入逻辑拥有，不属于 header 的样式契约。

### 2.3 icon

`icon` 表示展开/收起箭头，即 `IconButton#PART_ExpandButton`，cardinality 为 `Single`。`IsShowExpandIcon=False` 时节点通过
`IsVisible=false` 隐藏，但节点与 marker 仍属于模板稳定结构，Part 身份与数量不变。这一行为与上游一致：antd 在
`showExpandIcon=false` 时不渲染箭头元素。

它负责：

- 承载箭头图标的尺寸（`IconSizeSM`）、对齐与边距；`ExpandIconPosition` 的 Start/End 切换 `Grid.Column` 0/3 与对应
  `LeftExpandButtonHMargin` / `RightExpandButtonHMargin`。
- 承载 `ExpandIcon` 替换图标与展开/收起旋转动效的视觉入口。
- 承载 `TriggerType=Icon` 下的命中光标与点击入口。

适合定制图标的 `IconWidth` / `IconHeight`、`Foreground`、`Margin` 与 `RenderTransform`。`:expanded` 组合四个方向伪类的
`rotate(±90deg)` 旋转由主题拥有；Semantic `RenderTransform` 与状态主题 Setter 按 Avalonia 原生优先级竞争（用户样式优先级
更高），但箭头旋转语义仍由 `IsExpanded` 与 `ExpandDirection` 驱动，不建议用 Semantic `RenderTransform` 替代状态表达。
显式 `HeaderPadding` 下箭头间距由 `EffectiveExpandButtonMargin` 从 `HeaderPadding` 对应方向推导，语义 Setter 覆盖该值会
破坏自定义 padding 的视觉对称。`ExpandIcon` 替换路径（用户自定义图标）只改变图标内容，不改变节点、marker 与 Part 身份。

### 2.4 title

`title` 表示标题文字区域，即 `ContentPresenter#PART_HeaderPresenter`，cardinality 为 `Single`。`HeaderTemplate` 只替换内容
呈现，不改变节点与 marker。

它负责：

- 承载 `Header` / `HeaderTemplate` 生成的内容，并提供标题文字的布局、颜色、字体与对齐。
- 默认前景色来自 `ColorTextHeading`，禁用态由主题 selector 投影 `ColorTextDisabled`。

适合定制标题的 `Foreground`、`FontSize`、`FontWeight`、`Margin` 与水平/垂直内容对齐。标题、展开图标与 `AddOnContent` 的
Grid 列排布（`PART_HeaderLayout` 的 `Auto, *, Auto, Auto` 四列）不属于 title 契约；`:custom-header-padding` 下主题把
`PART_HeaderPresenter.Margin` 归零以保持自定义紧凑布局，语义 Setter 覆盖该 margin 会破坏该关系。

### 2.5 body

`body` 表示内容区域，即 `ContentPresenter#PART_ContentPresenter`，cardinality 为 `Optional`。
主题声明的 `ExpandMotion` / `CollapseMotion` 通过 `IMotionLayout` 驱动 `ContentViewport#PART_ContentViewport`。

`body` 的节点存在性由内容区域的呈现历史决定：

- 从未展开的实例：actor 的 `IsVisible=false`，其内部 `ContentPresenter` 尚未挂接视觉子级，`body` 节点只保留逻辑子级、
  从视觉树缺席，命中 0 个节点。
- 首次展开：节点挂接进视觉树并携带 owner 的 `TemplatedParent`，命中 1 个节点。
- 再次收起：actor 回到 `IsVisible=false`，但 Avalonia 的 `ContentPresenter` 只在自身可见时挂接子级、转不可见时不解除
  挂接，因此节点保持存在，仍命中 1 个节点。

`body` 是 Expander 唯一的 `Optional` Part；`header` / `icon` / `title` 是模板常驻节点，任何状态下都恰好命中一个。

`body` 缺席不影响 owner 级 Semantic Style 的稳定性：`ExpanderBodyStyle` 的路由是 `/template/ .semantic-body`，节点首次
物化时样式照常生效，不需要重新应用模板。

它负责：

- 承载 `Content` / `ContentTemplate` 生成的内容，并提供内容区域的内边距、背景与文字样式。
- 把 `SizeType` 三档的默认内容内边距（`ContentPaddingSM` / `ContentPadding` / `ContentPaddingLG`）投影到 `Padding`；显式
  `ContentPadding` 经 `:custom-content-padding` 伪类覆盖三档基线。
- Borderless 模式下接收 `HeaderBg` 作为内容背景；Ghost 模式下头部改用 `ContentBg`。

适合定制内容区域的 `Background`、`Padding` 与 `TextElement.*`。展开/收起动效的尺寸动画由 motion actor 拥有，不属于
`body` 的样式契约；固定 `Height` / `Width` / Min/Max 类布局 Setter 不作为公共定制路径，见 [§5 尺寸基线](#5-尺寸基线)。

`body` 不包含 Header/Content 分隔线。分隔线由 `PART_ContentViewport` 内部一个未命名 `PixelAlignedBorder` 拥有，该节点
不作为 Part 或模板 part 契约暴露，见 [§6 定制边界](#6-定制边界)。

## 3. Selector 用法

应用级样式先限定 Expander owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护与 `SelectorRoute`，
用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Expander">
        <atom:ExpanderHeaderStyle x:SetterTargetType="atom:PixelAlignedBorder">
            <Setter Property="Background" Value="#F0F0F0" />
            <Setter Property="Padding" Value="12,16" />
        </atom:ExpanderHeaderStyle>

        <atom:ExpanderTitleStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#141414" />
        </atom:ExpanderTitleStyle>

        <atom:ExpanderIconStyle x:SetterTargetType="atom:IconButton">
            <Setter Property="IconWidth" Value="16" />
            <Setter Property="IconHeight" Value="16" />
        </atom:ExpanderIconStyle>

        <atom:ExpanderBodyStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Background" Value="#FFFFFF" />
        </atom:ExpanderBodyStyle>
    </Style>
</Application.Styles>
```

对特定 Expander class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Expander.semantic-compact[SizeType=Small]">
    <atom:ExpanderHeaderStyle x:SetterTargetType="atom:PixelAlignedBorder">
        <Setter Property="Background" Value="#F5EFFF" />
    </atom:ExpanderHeaderStyle>
</Style>

<Style Selector="atom|Expander:expanded">
    <atom:ExpanderIconStyle x:SetterTargetType="atom:IconButton">
        <Setter Property="Foreground" Value="#1677FF" />
    </atom:ExpanderIconStyle>
</Style>
```

`header`、`icon`、`title`、`body` 都是 owner 自身模板内的静态 Part，生成 Style 的路由是单一的 `/template/` 边界，不需要
`.semantic-scope-*` 中间锚点，也不存在跨视觉根或容器回收路径。生成 Style 已封装完整路由，用户样式不得复制这些 route，
也不得依赖 `PART_*` 名称或内部节点层级。

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `PixelAlignedBorder.semantic-header` 或 `:is(PixelAlignedBorder).semantic-header`。
- `ContentPresenter.semantic-title` 或 `IconButton.semantic-icon`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 连续穿过子控件模板的多个 `/template/`（例如借 `PART_HeaderLayoutTransform` 的 `LayoutTransformControl` 模板继续下钻）。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。

## 4. 状态与数量语义

Expander 是单面板控件。`root`、`header`、`icon`、`title` 在每个已实例化模板中恒为 1；`body` 是 `Optional`，其命中数量取决于
内容区域是否已经被呈现过（而不是仅取决于当前展开状态）：

| 状态 | root | header | icon | title | body | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| 从未展开 | 1 | 1 | 1 | 1 | 0 | actor `IsVisible=false`，其内部 `ContentPresenter` 尚未挂接视觉子级，`body` 节点缺席。 |
| 首次展开 | 1 | 1 | 1 | 1 | 1 | `body` 节点挂接进视觉树，`TemplatedParent` 为 Expander owner。 |
| 首次展开后再次收起 | 1 | 1 | 1 | 1 | 1 | actor 回到 `IsVisible=false`，但 `ContentPresenter` 不解除挂接，节点保持存在。 |
| 展开 / 收起切换 | 1 | 1 | 1 | 1 | 0→1 | 只影响 motion actor 的尺寸/透明度与箭头旋转，以及首次挂接时机。 |
| `IsShowExpandIcon=False` | 1 | 1 | 1 | 1 | 不变 | `PART_ExpandButton` 隐藏但节点与 marker 仍存在（`IsVisible=false`，非缺席）。 |
| `TriggerType` Header/Icon | 1 | 1 | 1 | 1 | 不变 | 只改变命中区域与光标，不增删节点。 |
| `ExpandIconPosition` Start/End | 1 | 1 | 1 | 1 | 不变 | 只切换 `Grid.Column` 与边距 Token，marker 不变。 |
| `ExpandDirection` Up/Down/Left/Right | 1 | 1 | 1 | 1 | 不变 | 只切换 Dock、Header 旋转、图标旋转与分隔线边，marker 不变。 |
| `SizeType` Large/Middle/Small/Custom | 1 | 1 | 1 | 1 | 不变 | 只改变 Padding/字体/边距 Token 档位。 |
| `IsBorderless` / `IsGhostStyle` | 1 | 1 | 1 | 1 | 不变 | 只改变根边框、分隔线与背景的主题视觉。 |
| `IsMotionEnabled=False` | 1 | 1 | 1 | 1 | 不变 | 直接归一稳定态，不增删节点。 |
| 自定义 HeaderPadding / ContentPadding | 1 | 1 | 1 | 1 | 不变 | 只切换伪类与 padding/margin 值。 |
| disabled | 1 | 1 | 1 | 1 | 不变 | 只改变禁用态前景色，节点不变。 |
| 模板重应用 | 1 | 1 | 1 | 1 | 不变 | 静态 marker 随模板重建，数量按重建时的呈现状态恢复。 |

表中“不变”表示该状态切换本身不改变 `body` 的命中数量；`body` 的实际值仍由“是否已呈现过”决定（0 或 1）。

Expander 没有 `Multiple` Part，也没有由 C# 创建 marker 的 Part：不存在容器 prepare/clear/recycle 路径，也不存在
`RuntimeCreated=true` 的 marker 注入点。`body` 的 0/1 差异来自 motion actor 内部 `ContentPresenter` 的视觉子级挂接时机，
而不是容器生命周期或 marker 注入。这使 Expander 的 Part 生命周期比 Collapse 简单——Part 身份完全由模板决定。

`body` 的 `Optional` 语义不改变生成 Style：`ExpanderBodyStyle` 在节点物化时照常命中；尚未物化时没有目标节点，样式自然
无可施加对象。这与 Avalonia 对未挂接内容的原生行为一致。

## 5. 尺寸基线

Expander 的尺寸链与 Semantic Setter 的关系：

| 项目 | 内容 |
| --- | --- |
| 完整尺寸分支 | `Large`、`Middle`、`Small`、`Custom` 四档（`CustomizableSizeType`），默认 `Middle`；`SizeType` 是 public 属性。 |
| 布局 owner | 头部 `PART_HeaderDecorator`（`Padding` + `TextElement.FontSize` / `TextBlock.LineHeight`）、内容 `PART_ContentPresenter`（`Padding`）、箭头 `PART_ExpandButton`（`IconSizeSM` 图标 + 按位置与尺寸档的 margin）。整体高度由内容自然测量 + `PART_ContentViewport` 的动效进度拥有。 |
| Token 映射 | Middle/Custom → `HeaderPadding`、`ContentPadding`、`FontSize`/`FontHeight`、`Left/RightExpandButtonHMargin`；Large → `HeaderPaddingLG`、`ContentPaddingLG`、`FontSizeLG`/`FontHeightLG`；Small → `HeaderPaddingSM`、`ContentPaddingSM`、`FontSize`/`FontHeight`（Small 与 Middle 同字号）。 |
| 显式覆盖 | `HeaderPadding` / `ContentPadding` 非空时置 `:custom-header-padding` / `:custom-content-padding`，主题在 `:not(...)` 分支外覆盖三档默认 Padding；显式 HeaderPadding 同时把箭头 margin 改为从 `HeaderPadding` 对应方向推导。 |
| 状态矩阵 | 展开/收起、方向、disabled、Ghost、Borderless、TriggerType、ExpandIconPosition、SizeType 只切换主题 selector 视觉，不改变 marker 与节点数量，见 [§4](#4-状态与数量语义)。 |
| 外部映射 | antd `size="small"` → `Small`；antd 默认（middle）→ `Middle`（及 `Custom`）；antd `size="large"` → `Large`。依据：antd Collapse 的 `size` 三档驱动 header/content padding、字号与图标尺寸；AtomUI 用同构三档 Token 表达，`Custom` 复用 Middle 基线并把完整控制权交给用户。 |
| 失败回归 | 给 `header` 或 `body` 设置固定 `Height`/`MinHeight` 会绕过 Padding + 字体自然测量，多行标题与三档尺寸基线失效。展开/收起动效由 Core 共享的 `Motion` 拥有：动效期间以尺寸/透明度进度驱动内容视口，固定 `body` Height 会与该进度竞争，因为 actor 的视口尺寸覆盖内容测量。Semantic Setter 中不应设置固定 `Height`/`Width`/Min/Max 布局值；改变头部/内容间距应通过 `HeaderPadding`/`ContentPadding`、Token 或主题分支实现。 |

`header` 的 `Background`、`Padding`、字体与边框 Setter，以及 `body` 的 `Background`、`Padding` Setter 是公共定制路径
（Semantic Setter 优先级高于主题尺寸档 TemplateBinding）；固定 `Height` / `Width` / Min/Max 类布局 Setter 会与尺寸档链或
content motion 的尺寸动画竞争 Avalonia 属性优先级，因此不作为公共定制路径。

## 6. 定制边界

以下区域明确不属于 Expander Semantic Part：

- `PART_Frame`、`PART_MainLayout`、`PART_HeaderLayout`、`PART_HeaderLayoutTransform`、`PART_ContentViewport`：模板结构
  节点，其名称、类型与层级不是公共契约。
- Header/Content 分隔线：位于 `PART_ContentViewport` 内部的未命名 `PixelAlignedBorder`，只由 `BorderThickness`、
  `IsBorderless`、`IsGhostStyle` 与 `ExpandDirection` 决定。它没有 Part 身份，也不作为新的 template part 暴露；需要改变
  分隔线时通过公开的 `BorderThickness` 与视觉模式属性实现，见 [§8.5 结构化分隔线模型](overview.md#85-结构化分隔线模型)。
- `PART_AddOnContentPresenter`（`AddOnContent` / `AddOnContentTemplate` 承载节点）：上游 `Collapse` 没有对应的公开语义
  区域，因此不发布为 Part。AddOnContent 是轻量辅助入口，其视觉通过 `AddOnContent` 自身类型或整控件 ControlTheme 定制。
- 展开/收起动效：由 `PART_ContentViewport` 与 Core 共享的 `Motion` 拥有（尺寸/透明度进度与内容视口
  测量），不属于 `body` 的样式契约；`IsMotionEnabled` 与 `MotionDuration` 是公共控制入口。
- 根表面背景、圆角与整体内边距：`PART_Frame` 的 `Background`、`CornerRadius`、`Padding` 由默认主题拥有，且**不从 owner
  属性投影**。因此这三项不能通过 root Semantic Setter 定制，只能通过替换 owner `ControlTheme` 实现。`BorderThickness` 与
  视觉模式（`IsBorderless` / `IsGhostStyle`）是 owner API，属于 root 契约。
- 圆角裁剪的可见前提：header/body 的背景色被裁剪到 `PART_Frame` 的圆角内框，因此**不透明的 Part 背景不会溢出圆角**，但
  也意味着用户在 `header` / `body` 上设置的不透明背景在四角会被切掉一部分（这是预期行为，与容器圆角一致）。需要圆角处
  完整显示某个背景时，应连同 `PART_Frame` 的圆角一并考虑，而不是移除裁剪——移除裁剪会让子节点背景重新覆盖圆角。
- 嵌套 Expander 或用户 `HeaderTemplate` / `ContentTemplate` 生成的子树：其内部控件由用户拥有；内层 Expander 以自身
  owner 作用域独立匹配，不继承外层 Part。
- `PART_*` 名称、internal 类型、方向 selector 与 motion phase。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间节点的
固定测量、主题尺寸绑定或动效尺寸动画约束，应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让内置模板缺少 marker，均属于公共主题
契约变更。把 `body` 从 `Optional` 改成 `Single`（或反之）属于 cardinality 语义变更，需要兼容性评估。

验证至少覆盖：

- descriptor 中只有 `root`、`header`、`icon`、`title`、`body`，字段值与本文表格一致（`header` / `icon` / `title` 为
  `Single`，`body` 为 `Optional`；四者均为 `Customization=Selector`、`CrossVisualRoot=false`、`RuntimeCreated=false` 且不
  携带显式 `SelectorRoute`，`ContractType` 为公开类型 `PixelAlignedBorder` / `IconButton` / `ContentPresenter`）。
- `ExpanderTheme.axaml` 的 `semantic-header`、`semantic-icon`、`semantic-title`、`semantic-body` 静态 marker 各一个；模板
  没有第二个 `ControlTemplate` 变体，因此不存在 marker 覆盖缺口。
- `header` / `icon` / `title` 在任意状态下恰好命中一个节点：`IsExpanded` 切换、`IsShowExpandIcon=False`、`TriggerType`、
  `ExpandIconPosition`、`SizeType`、`ExpandDirection`、`IsBorderless` / `IsGhostStyle`、`IsMotionEnabled=False`、disabled 与
  自定义 padding 伪类都不改变命中数量。
- `body` 的数量严格遵循 `Optional` 语义：从未展开的实例命中 0 个节点；展开后命中 1 个节点且该节点的 `TemplatedParent`
  必须是 Expander owner（证明单一 `/template/` 路由成立、没有退化为宽泛 logical descendant）；首次展开后再次收起，节点
  保持存在（命中 1），同时 `PART_ContentViewport.IsVisible` 转为 `false`。
- 生成的 `ExpanderHeaderStyle` / `ExpanderIconStyle` / `ExpanderTitleStyle` / `ExpanderBodyStyle` 在展开状态下各精确命中
  一个节点（不是 0 个、也不是多个）。
- root 边框投影生效：`BorderThickness` 与 `IsBorderless` / `IsGhostStyle` 驱动 `PART_Frame` 的有效边框厚度；未声明用户
  Semantic Style 时不增加 selector activator。
- 圆角裁剪生效：`PART_Frame.ClipContentToCornerRadius` 为 `true` 且 `CornerRadius` 非零；header 节点位于 `PART_Frame` 的
  左上角圆角方形区域内（即该裁剪确实承担了遮挡职责）。注意 headless 测试平台的几何包含性无法表示圆角图形，
  `DashedBorder` 会按设计把裁剪降级为不应用，因此该平台只能断言模板契约与几何前置条件，**实际裁剪效果必须在真实 Skia
  后端（Gallery 桌面宿主）做视觉验证**。
- `header` 的背景与 Padding、`title` 的前景色、`icon` 的图标尺寸、`body` 的背景与 Padding 在 Light/Dark 下保持有效；
  Semantic Style 覆盖不与默认主题（不消费 `.semantic-*`）产生额外开销。
- 显式 `HeaderPadding` 下箭头 margin 仍由 `EffectiveExpandButtonMargin` 推导，不被 `icon` 的 Semantic `Margin` Setter 破坏；
  `:custom-header-padding` 下 `PART_HeaderPresenter.Margin` 的归零关系保持。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描；四个 Part marker 通过静态 AXAML class 在既有模板路径一次性
  添加，不引入 VisualTree 搜索、动态 marker 绑定或运行时 AXAML 解析。
