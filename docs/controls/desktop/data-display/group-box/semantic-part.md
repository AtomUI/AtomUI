# GroupBox Semantic Part 契约

本文档定义 `GroupBox` 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。GroupBox 的整体设计见
[GroupBox 桌面版架构设计](overview.md)，真实模板与生命周期见 [GroupBox 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`GroupBox` 公开 `root`、`header`、`icon`、`title`、`content` 五个职责区域：

| Part | Selector | ContractType | Cardinality | Customization | AtomUI 节点 |
| --- | --- | --- | --- | --- | --- |
| `root` | 控件本身 | `GroupBox` | `Single` | `Root` | `GroupBox` owner |
| `header` | `.semantic-header` | `Border` | `Single` | `Selector` | `Border#PART_HeaderContent` |
| `icon` | `.semantic-icon` | `IconPresenter` | `Single` | `Selector` | `IconPresenter#PART_HeaderIconPresenter` |
| `title` | `.semantic-title` | `TextBlock` | `Single` | `Selector` | `TextBlock#PART_HeaderPresenter` |
| `content` | `.semantic-content` | `ContentPresenter` | `Single` | `Selector` | `ContentPresenter#PART_ContentPresenter` |

五个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since` 统一为 `6.2.0`，与其余已改造控件一致。
GroupBox 是独立控件，不存在派生控件家族、public 子 Control owner 或 item container，五个 Part 全部属于 `GroupBox` 自身。

### 1.1 准入依据（2026-09-16 用户指令）

GroupBox 的纳入与既有三个追认项（`Expander`、`TabStrip`、`Menu`）依据不同，必须单独说明：

- 上游设计体系当前稳定发布源码中**不存在**与 GroupBox 职责对应的公开 Semantic DOM owner：其组件目录下没有 fieldset、
  group 或 group-box 类组件，`GroupBox` 标识在组件源码中零命中。因此本次纳入不存在可映射的上游 owner。
- 原排除判定见[全量改造设计 §2.4](../../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md)，其正向
  触发条件是“新稳定版出现职责直接对应的公开 owner”。该条件**并未发生**；纳入由用户直接指令撤销，判据是 AtomUI 需要
  让 GroupBox 支持 Semantic Part，而不是上游新增 owner。因此沿用 §2.4 的三次撤销记录方式（日期化范围变更），而不是
  声明一次新的上游 Gate 通过。
- 支持该决定的既有先例是 `SplitButton` 的触发侧按键：上游没有对应键时，AtomUI 可以按自身模板结构**显式能力补充**发布
  Part，而不是因为“上游没有”就拒绝定制。
- 产品职责的最近参照是上游 `Card`：它公开 `root` / `header` / `title` / `body` / `extra` / `cover` / `actions` 分区键，是
  “带标题的边框容器”这一职责的分区式参照。GroupBox 的 `header` / `title` 与之一一对应，`icon` / `content` 按 GroupBox
  自身的 Header API 与 `ContentControl.Content` 命名。`Card` 的 owner 资格已由 AtomUI `Card` 持有，GroupBox **不借用**
  Card 的准入资格。

GroupBox 的 Part 名称、`ContractType` 与 cardinality 自本文件发布起构成公共主题契约。

### 1.2 `GroupBox`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `root` |
| Selector | GroupBox 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `GroupBox` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `GroupBox` owner（可见边框与背景由 owner `Render` 自绘，无模板节点承载） |
| 职责 | GroupBox root 是分组边框、背景、圆角、内容内边距与标题位置的统一 owner。 |
| 相关 API | `HeaderTitle`、`HeaderTitleColor`、`HeaderIcon`、`HeaderTitlePosition`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight`、`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、`Content`、`ContentTemplate` |
| 相关 Token | GroupBoxToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `/template/ .semantic-header` |
| Style Type | `GroupBoxHeaderStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Border#PART_HeaderContent` |
| 职责 | 统一表示 Header 内容区域（图标与标题的承载区域）的背景、内边距与水平对齐；该区域的 bounds 同时是边框缺口的几何来源。 |
| 相关 API | `HeaderTitle`、`HeaderIcon`、`HeaderTitlePosition`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight` |
| 相关 Token | `HeaderContentPadding`、`HeaderContainerMargin`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `GroupBoxIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `IconPresenter#PART_HeaderIconPresenter` |
| 职责 | 统一表示 Header 图标的尺寸、颜色与间距。 |
| 相关 API | `HeaderIcon` |
| 相关 Token | `HeaderIconMargin`、`IconSizeLG`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `GroupBoxTitleStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#PART_HeaderPresenter` |
| 职责 | 统一表示标题文字的布局、颜色、字体与对齐。 |
| 相关 API | `HeaderTitle`、`HeaderTitleColor`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight` |
| 相关 Token | SharedToken（`ColorText`、`FontSize`） |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `GroupBoxContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#PART_ContentPresenter` |
| 职责 | 统一表示分组内容区域的内边距、背景与内容呈现。 |
| 相关 API | `Content`、`ContentTemplate`、`Padding`、`Background` |
| 相关 Token | `ContentPadding`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。`header` 的承载节点是公开的
`Border`，`icon` 是公开的 `IconPresenter`，`content` 是公开的 `ContentPresenter`；`title` 的 `ContractType` 取
`Avalonia.Controls.TextBlock` 而不是节点自身的派生类型 `AtomUI.Desktop.Controls.TextBlock`——派生类型同样满足该契约，
取基类可以让后续把节点替换为普通 `TextBlock` 保持兼容；收窄到派生类型属于破坏性变更。

`header`、`icon`、`title`、`content` 全部是 `GroupBoxTheme.axaml` 单一 `ControlTemplate` 内的静态节点，`TemplatedParent`
为 GroupBox owner 本身，因此四个 Part 声明 `RuntimeCreated=false` 且不携带显式 `SelectorRoute`；生成器按
[Semantic Part Generator §2.3](../../../../modules/generator/semantic-part-generator.md)把静态根模板 Part 的 route 规范化为
`/template/ .<SelectorClass>`。`icon` 与 `title` 在模板中位于 `header` 节点内部（`Decorator`/`Border` → `StackPanel`），
但都不跨越第二层模板边界，`TemplatedParent` 仍是 GroupBox owner，因此单一 `/template/` 路由已经足够，不需要为它们声明
更宽的 logical descendant 路由。GroupBox 没有任何由 C# 创建并注入 marker 的 Part，也不存在跨视觉根 Part。

GroupBox 只有一个叶子主题资产（`src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml`），没有第二个
`ControlTemplate` 变体、没有 Browser 变体、没有派生模板，也不存在用于聚合的 `*Themes.axaml` 参与校验；因此不存在
marker 覆盖缺口。

## 2. Part 说明

### 2.1 root

`root` 是 `GroupBox` owner 本身，在实例整个生命周期内始终存在，每个实例恰好一个。

它负责：

- 承载 `HeaderTitle`、`HeaderTitleColor`、`HeaderIcon`、`HeaderTitlePosition`、`HeaderFontSize`、`HeaderFontStyle`、
  `HeaderFontWeight` 等 Header 状态。
- 承载 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 与 `Padding` 等分组边框与内容内边距状态。
- 作为全部 Part 的 owner-scoped Selector 作用域边界。

**root 是唯一可以定制分组边框与背景的入口。** GroupBox 的可见边框和背景由 `GroupBox.Render` 用几何绘制，模板中不存在
承载它们的节点：`PART_Frame` 既不设置 `Background` 也不设置 `BorderBrush`，只提供布局根与 bounds 参考。因此
`BorderBrush`、`Background`、`BorderThickness`、`CornerRadius` 只能通过 root 的公开属性（或整体替换 owner
`ControlTheme`）定制，任何 Part 上的 `Background` / `BorderBrush` Setter 都只会作用于该 Part 自己的节点，不会改变分组
边框。这一关系见 [§6 定制边界](#6-定制边界)。

适合通过 root 定制边框厚度、圆角、背景、内容内边距与标题位置；需要按状态分支改变 root 时，在 owner Selector 上组合公开
属性（例如 `atom|GroupBox[HeaderTitlePosition=Center]`）。

root 不表示模板中的 `PART_Frame`、`PART_HeaderContainer` 等任何内部节点；这些节点的名称、数量和层级不属于 root 契约。

### 2.2 header

`header` 表示 Header 内容区域，即 `Border#PART_HeaderContent`，cardinality 为 `Single`。节点位于模板稳定结构中，
`HeaderTitle`、`HeaderIcon`、`HeaderTitlePosition`、内容尺寸与视觉模式都不增删节点。

**节点类型提升（2026-09-16 设计决定）：** `PART_HeaderContent` 由 `Decorator` 提升为 `Border`。Avalonia `Decorator` 只提供
`Child` 与 `Padding`，**没有 `Background`**，保留 `Decorator` 会让 `header` 只能定制内边距与布局类属性，无法完成
“给分组标题区域一个背景”这一最常见的定制诉求；同族 header（`Expander.header` 为 `PixelAlignedBorder`、`Card.header` 为
`DashedBorder`）都支持 `Background`。`Border` 继承自 `Decorator`，因此 `GroupBox.OnApplyTemplate` 的
`Find<Decorator>("PART_HeaderContent")` 与 `CalculateHeaderGapBounds` 的 `Bounds` 语义完全不变，缺口几何不受影响；唯一
需要同步的是主题中以节点类型开头的 selector token（`Decorator#PART_HeaderContent` → `Border#PART_HeaderContent`）。
该提升与 marker 添加同属本次改造，不改变缺口语义。

它负责：

- 承载 Header 内容区域（图标与标题）的背景、内边距与水平对齐。
- 作为边框缺口的几何来源：缺口宽度取该节点的实际 bounds，因此 `Padding`、图标尺寸与标题宽度变化会同步改变缺口宽度。
- 把 `HeaderTitlePosition` 的 Left / Center / Right 投影为自身的 `HorizontalAlignment`。

适合定制 `Background`、`Padding`、`BorderBrush` / `BorderThickness` / `CornerRadius`、`Opacity` 与 `RenderTransform`。
Semantic `Padding` Setter 的优先级高于主题 `HeaderContentPadding` 档基线，可以整体覆盖默认留白。

两点需要明确：

- 缺口是**几何排除**而不是背景遮挡（这一条是 GroupBox 的既有渲染契约，见 [§8 专项模型](overview.md#8-专项模型)）。因此给
  `header` 设置不透明或半透明 `Background` 既不会在标题下方还原出边框短线，也不会破坏 `Background="Transparent"` 的
  视觉契约；`header` 的背景与 root 的边框是两条互不干扰的绘制路径。
- 缺口宽度恒等于该节点 bounds。因此 `header` 上的固定 `Width` / `Height` / Min/Max 类布局 Setter 会同时改变缺口形状，
  属于需要与缺口语义一起评估的改动，见 [§5 尺寸基线](#5-尺寸基线)。

`HeaderTitlePosition` 的水平对齐由该 Part 承载，但标题位置是 owner API：改变位置应设置该属性，而不是用 Semantic Setter
覆盖 `HorizontalAlignment` 来绕过它。

### 2.3 icon

`icon` 表示 Header 图标，即 `IconPresenter#PART_HeaderIconPresenter`，cardinality 为 `Single`。`HeaderIcon` 为 `null` 时该
节点通过 `IsVisible=false` 隐藏，但节点与 marker 仍属于模板稳定结构，Part 身份与数量不变（与 `Expander.icon` 在
`IsShowExpandIcon=False` 下的处理一致）。

它负责：

- 承载 Header 图标的尺寸（`IconSizeLG`）、颜色（`ColorText`）与右向间距（`HeaderIconMargin`）。
- 承载 `HeaderIcon` 传入的 `PathIcon` 的呈现。

适合定制图标的 `Width` / `Height`、`IconBrush`、`Margin`、`Opacity` 与对齐。这三项在 owner API 上**没有**对应属性——
`HeaderIcon` 只接受图标实例本身，图标尺寸、颜色与间距此前只能通过替换整个 `ControlTheme` 或改 Token 调整，因此 `icon`
是本次改造新增能力最明确的一个 Part。

`HeaderIcon=null` 时不保留额外占位宽度：节点虽然仍存在（`Single`），但 `IsVisible=false` 使其不参与布局，这一点是
GroupBox 的既有不变量，Semantic Setter 不应通过强制 `IsVisible=true` 去恢复占位。

`icon` 的 Semantic Setter 只作用于图标承载节点本身；`HeaderIcon` 传入的 `PathIcon` 内部图形由该 `PathIcon` 的类型与用户
自定义内容拥有，不属于 `icon` 契约。

### 2.4 title

`title` 表示标题文字区域，即 `TextBlock#PART_HeaderPresenter`，cardinality 为 `Single`。

它负责：

- 承载 `HeaderTitle` 文本的呈现与布局。
- 默认前景、字号、字重与字体样式分别由 owner 的 `HeaderTitleColor`、`HeaderFontSize`、`HeaderFontWeight`、
  `HeaderFontStyle` 经 `TemplateBinding` 投影；四个属性的默认值来自 SharedToken（`ColorText`、`FontSize`）。

适合定制标题的 `Foreground`、`FontSize`、`FontWeight`、`FontStyle`、`TextDecorations`、`Margin`、`Opacity` 与
`LetterSpacing`。

需要明确的优先级与分工：

- owner 的四个 Header 字体/颜色属性是本控件的**主路径**，适合按实例表达标题样式；`title` Part 适合按 class 或宿主作用域
  批量覆盖标题样式，以及定制 owner API 没有暴露的项（`TextDecorations`、`Margin`、`Opacity` 等）。
- 生成的 `GroupBoxTitleStyle` 通过 class selector 生效，其 Setter 按 Avalonia 原生 `BindingPriority.StyleTrigger` 应用，
  优先级高于 `TemplateBinding`（`BindingPriority.Template`）与主题普通 Style Setter（`BindingPriority.Style`）。因此
  `title` 上的 Semantic Setter **可以有意覆盖** `HeaderTitleColor` / `HeaderFontSize` 等属性投影出的默认视觉值，这是
  [Semantic Part 系统设计 §7](../../../../architecture/systems/theming/semantic-parts.md)允许的关系；反之把值写回 owner
  属性仍是主路径。

标题、图标与 Header 内边距的相对排布由 `header` 内部的 `StackPanel` 决定，不属于 `title` 契约。

### 2.5 content

`content` 表示分组内容区域，即 `ContentPresenter#PART_ContentPresenter`，cardinality 为 `Single`。

它负责：

- 承载 `Content` / `ContentTemplate` 生成的内容。
- 把 owner 的 `Padding` 作为自身 `Margin` 投影到内容区域（`Margin="{TemplateBinding Padding}"`），默认值来自
  `ContentPadding` Token。

适合定制内容的 `Padding`（`ContentPresenter` 自身的内边距）、`Background`、`TextElement.*` 继承类属性、
`HorizontalContentAlignment` / `VerticalContentAlignment`、`MinHeight` / `MaxHeight` 与 `Opacity`。

需要注意 `Margin` 与 owner `Padding` 的关系：该节点的 `Margin` 已被 owner `Padding` 占用为内容内边距的承载，Semantic
Setter 覆盖 `Margin` 会与 `Padding` 属性产生两套并行语义。给内容区域增加内边距应优先使用 owner `Padding`（主路径）或该
Part 的 `Padding`，而不是覆盖 `Margin`。

内容区域背景与 root 背景的关系：owner 的 `Background` 绘制在分组边框几何范围内（从 Header 中线到边框底部），`content`
的 `Background` 绘制在该承载节点自己的矩形内，两者可以叠加。需要整块分组底色时应使用 root `Background`；需要内容区域
独立底色时使用 `content` 的 `Background`。

`content` 不包含边框与圆角；分组边框与圆角属于 root。

## 3. Selector 用法

应用级样式先限定 GroupBox owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护与
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|GroupBox">
        <atom:GroupBoxHeaderStyle x:SetterTargetType="Border">
            <Setter Property="Background" Value="#F0F0F0" />
            <Setter Property="Padding" Value="12,2" />
        </atom:GroupBoxHeaderStyle>

        <atom:GroupBoxTitleStyle x:SetterTargetType="TextBlock">
            <Setter Property="Foreground" Value="#141414" />
        </atom:GroupBoxTitleStyle>

        <atom:GroupBoxIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="Width" Value="18" />
            <Setter Property="Height" Value="18" />
            <Setter Property="IconBrush" Value="#1677FF" />
        </atom:GroupBoxIconStyle>

        <atom:GroupBoxContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Padding" Value="16" />
        </atom:GroupBoxContentStyle>
    </Style>
</Application.Styles>
```

对特定 GroupBox class 或状态定制时，把 class 或属性选择器放在 owner 一侧：

```xml
<Style Selector="atom|GroupBox.danger-group">
    <atom:GroupBoxHeaderStyle x:SetterTargetType="Border">
        <Setter Property="Background" Value="#FFF1F0" />
    </atom:GroupBoxHeaderStyle>

    <atom:GroupBoxTitleStyle x:SetterTargetType="TextBlock">
        <Setter Property="Foreground" Value="#CF1322" />
    </atom:GroupBoxTitleStyle>
</Style>

<Style Selector="atom|GroupBox[HeaderTitlePosition=Center]">
    <atom:GroupBoxContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Padding" Value="12" />
    </atom:GroupBoxContentStyle>
</Style>
```

`header`、`icon`、`title`、`content` 都是 owner 自身模板内的静态 Part，生成 Style 的路由是单一的 `/template/` 边界，不需要
`.semantic-scope-*` 中间锚点，也不存在跨视觉根或容器回收路径。生成 Style 已封装完整路由，用户样式不得复制这些 route，
也不得依赖 `PART_*` 名称或内部节点层级。

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `Border.semantic-header` 或 `:is(Border).semantic-header`。
- `TextBlock.semantic-title` 或 `IconPresenter.semantic-icon`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序，例如用 `PART_HeaderContainer` 或 `StackPanel` 层级收窄选择器。

root 的定制不通过 `.semantic-root`，而是在 owner 一侧直接写属性 Selector。**边框与背景的定制只能走这条路径**——
分组边框和背景由 `GroupBox.Render` 自绘，模板中没有承载它们的节点：

```xml
<Style Selector="atom|GroupBox.compact">
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="CornerRadius" Value="10" />
</Style>

<Style Selector="atom|GroupBox.danger-group">
    <Setter Property="BorderBrush" Value="#FF4D4F" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="CornerRadius" Value="12" />
    <Setter Property="Background" Value="#FFF1F0" />
</Style>
```

该路径依赖 Avalonia 原生优先级：owner 作用域普通 Setter 以 `BindingPriority.Style` 应用，与 ControlTheme 的默认
`<Setter Property="BorderBrush" ...>` 同级，由样式宿主顺序与声明顺序决定；用户 `Styles` 晚于 ControlTheme 生效，
因此可以正常覆盖默认值。覆盖后的值会进入 `GroupBox.Render` 的自绘几何（背景与边框都作为填充几何绘制，
`DrawGeometry` 的 pen 为 `null`）。给 Part（`header` / `content`）设置 `BorderBrush` 只影响该 Part 自身的节点，
不会改变分组边框。

## 4. 状态与数量语义

GroupBox 没有 hover、pressed、focus、disabled、loading、selected、expanded 或 checked 状态，也不拦截输入事件。它不实现
`ISizeTypeAware`，没有尺寸档。因此五个 Part 在任何状态下都稳定命中固定数量：

| 状态 | root | header | icon | title | content | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| 默认 | 1 | 1 | 1 | 1 | 1 | 模板常驻节点，四个 marker 各一。 |
| `HeaderIcon=null` | 1 | 1 | 1 | 1 | 1 | 图标节点 `IsVisible=false` 但不缺席，marker 不变。 |
| `HeaderTitle=null` | 1 | 1 | 1 | 1 | 1 | 标题节点仍存在，只是文本为空。 |
| `HeaderTitlePosition` Left/Center/Right | 1 | 1 | 1 | 1 | 1 | 只切换 `header` 的 `HorizontalAlignment`。 |
| 带 / 不带图标 | 1 | 1 | 1 | 1 | 1 | 只改变 `header` 内容宽度与缺口宽度，不增删节点。 |
| `Background` Transparent / 半透明 / 实色 | 1 | 1 | 1 | 1 | 1 | 只改变 owner 自绘几何的画笔。 |
| `BorderThickness` / `CornerRadius` 变化 | 1 | 1 | 1 | 1 | 1 | 只改变自绘边框几何与缺口排除区域。 |
| 内容为空 | 1 | 1 | 1 | 1 | 1 | 内容承载节点始终存在。 |
| 模板重应用 | 1 | 1 | 1 | 1 | 1 | 静态 marker 随模板重建，数量不变。 |

GroupBox 没有 `Optional` 或 `Multiple` Part，也没有由 C# 创建 marker 的 Part：不存在条件分支模板、容器 prepare/clear/
recycle 路径、`RuntimeCreated=true` 注入点，也不存在跨视觉根 Part。这与 `Expander.body` 需要 `Optional` 的原因形成对比：
后者取决于 motion actor 内部的挂接时机，而 GroupBox 的四个 Part 都是模板常驻静态节点，任何状态下都恰好命中一个。

## 5. 尺寸基线

GroupBox 的尺寸链与 Semantic Setter 的关系：

| 项目 | 内容 |
| --- | --- |
| 完整尺寸分支 | GroupBox 没有 `SizeType`、`ISizeTypeAware` 或 `ICustomizableSizeTypeAware`；不存在 `Large` / `Middle` / `Small` / `Custom` 分支。Header 与内容尺寸由公开属性、Token 和内容自然测量决定。 |
| 布局 owner | 整体高度由 `MeasureOverride` 测量 `PART_Frame` 得出（Header 通道 + 内容 `Padding` + 内容 `DesiredSize`）。Header 通道 = `PART_HeaderContainer`（`HeaderContainerMargin` 外边距）+ `PART_HeaderContent`（`HeaderContentPadding` 内边距 + 图标 `IconSizeLG` + `HeaderIconMargin` + 标题）。内容区域 = `PART_ContentPresenter` 的 `Margin`（来自 owner `Padding`）。 |
| Token 映射 | `HeaderContainerMargin`、`HeaderContentPadding`、`HeaderIconMargin`、`ContentPadding` 为 GroupBox 组件级 Token；图标尺寸取 SharedToken `IconSizeLG`，标题颜色/字号取 SharedToken `ColorText` / `FontSize`，边框与圆角取 SharedToken `BorderThickness` / `BorderRadius`。 |
| 外部映射 | 上游不存在与 GroupBox 对应的公开 owner，因此不存在上游尺寸档名称到 AtomUI 分支的映射。GroupBox 作为 AtomUI 原生控件，尺寸由上述自然测量链定义。 |
| 失败回归 | 给 `header` 或 `content` 设置固定 `Height` / `MinHeight` 会绕过自然测量，使 [§7 兼容性不变量](overview.md#7-兼容性不变量)中“未设置显式高度时 `DesiredSize.Height` 必须包含 Header 通道、内容内边距和内容自身期望高度”失效；给 `header` 设置固定 `Width` 会让边框缺口与标题实际宽度脱钩，破坏“缺口至少覆盖 Header 内容实际 bounds”的契约。 |

Semantic Setter 与尺寸链的优先级关系：`header` 的 `Padding`、`IconStyle` 的尺寸、`content` 的 `Padding` 等 Setter 通过 class
selector 以 `BindingPriority.StyleTrigger` 应用，高于主题 Style Setter 与 `TemplateBinding`，因此可以有意覆盖 Token 档
基线并参与自然测量。固定 `Height` / `Width` / Min/Max 类布局 Setter 会与缺口几何或自动高度契约竞争，不作为推荐定制路径；
改变 Header/内容间距应通过 `header` 的 `Padding`、`content` 的 `Padding`、owner `Padding` 或 Token 实现。

## 6. 定制边界

以下区域明确不属于 GroupBox Semantic Part：

- `PART_Frame`（`Border`）：模板布局根，只承载 `CornerRadius` / `Margin` 投影并提供边框 bounds 参考。**可见边框与背景
  由 `GroupBox.Render` 自绘，不落在任何模板节点上**，因此 `BorderBrush`、`Background`、`BorderThickness`、`CornerRadius`
  不是 Part 属性，只能通过 root 的公开属性或整体替换 owner `ControlTheme` 定制。设置 `PART_Frame` 的 `Background` /
  `BorderBrush` 不会改变分组边框观感。
- `PART_HeaderContainer`（`Panel`）：Header 行的纯布局包装，唯一职责是施加 `HeaderContainerMargin` 外边距。它不承载视觉
  语义，因此不发布为 Part；需要调整 Header 与分组外边界距离时应通过 Token 或主题实现，而不是给该节点加 Semantic 标记。
- Header 缺口几何：由控件渲染模型拥有（`PART_HeaderContent` bounds → 缺口矩形 → 边框几何排除）。缺口不是 Part，也不
  因为 `header` 获得 `Background` 能力而改变实现方式；缺口永不退化为背景遮挡。
- Header 的 `StackPanel` 内部排布：图标与标题的相对位置由 `header` 节点内部的 `StackPanel` 决定，该层级不是公共契约。
- Title 与 icon 之外的其他 Header 内容：GroupBox 没有 `extra` / `addOn` 之类的第三块 Header 区域，不需要也**不得**为了
  对称而虚构这类 Part。
- Token 语义值：`TextPaddingInline`、`OrientationMarginPercent`、`VerticalMarginInline` 是 GroupBox 的 fieldset 语义保留
  Token，不表示模板节点或实例状态，不是 Part。
- 用户内容子树：`HeaderIcon` 传入的 `PathIcon` 内部图形、`ContentTemplate` 生成的子树由用户拥有；内层控件以自己的
  owner 作用域独立匹配，不继承 GroupBox 的 Part。嵌套 GroupBox 同理。
- Popup、Flyout、Overlay 与独立 TopLevel：GroupBox 不创建任何弹层或独立宿主，`CrossVisualRoot=false`。
- `PART_*` 名称、internal 类型与视觉祖先顺序。

Semantic Style 服从 Avalonia 原生属性优先级（`LocalValue` 0 > `StyleTrigger` 1 > `Template` 2 > `Style` 3 > `Inherited` 4）。
Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 测量、主题绑定或自绘几何约束，应按跨节点布局约束排查，不能
把它解释为 Semantic Style 优先级失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让内置模板缺少 marker，均属于公共主题
契约变更。把 `Decorator#PART_HeaderContent` 再退回 `Decorator` 会同时收窄 `header` 的 `ContractType` 并移除
`Background` 能力，属于破坏性变更。

验证至少覆盖：

- descriptor 中只有 `root`、`header`、`icon`、`title`、`content`，字段值与 §1 表格一致：四个非 root Part 均为
  `Customization=Selector`、`CrossVisualRoot=false`、`RuntimeCreated=false`、`Cardinality=Single`，不携带显式
  `SelectorRoute`，`ContractType` 分别为 `Border` / `IconPresenter` / `TextBlock` / `ContentPresenter`；`root` 的
  `ContractType` 为 `GroupBox` 且 `StyleType` 为 `null`。
- `GroupBoxTheme.axaml` 的 `semantic-header`、`semantic-icon`、`semantic-title`、`semantic-content` 静态 marker 各一个；
  模板只有一个 `ControlTemplate`、没有 Browser 或派生变体，因此不存在 marker 覆盖缺口。
- 四个非 root Part 在任意状态组合下恰好命中一个节点：`HeaderIcon=null`、`HeaderTitle=null`、`HeaderTitlePosition` 三档、
  带/不带图标、`Background` 三态、`BorderThickness` / `CornerRadius` 变化与模板重应用都不改变命中数量。
- `header` 的承载节点类型是 `Border`（可赋值给 `Decorator`），`GroupBox.OnApplyTemplate` 的 `Find<Decorator>` 与
  `CalculateHeaderGapBounds` 在节点类型提升后继续按 `Bounds` 工作，缺口宽度仍等于该节点 bounds。
- 生成的 `GroupBoxHeaderStyle` / `GroupBoxIconStyle` / `GroupBoxTitleStyle` / `GroupBoxContentStyle` 各自精确命中一个节点
  （不是 0 个、也不是多个）。
- 缺口与背景互不干扰：在不透明 `header` 背景、半透明背景与 `Background="Transparent"` 三种情况下，标题下方都不出现被
  还原的边框短线；该断言与既有 `GroupBoxRenderTests` 的几何排除断言一致，并在新增背景下保持成立。
- 优先级关系成立：`GroupBoxHeaderStyle` 的 `Padding` 覆盖 `HeaderContentPadding` 基线，`GroupBoxTitleStyle` 的
  `Foreground` 覆盖 `HeaderTitleColor` / `FontSize` 投影，且不改变缺口宽度之外的行为。
- `HeaderIcon=null` 时图标节点仍携带 marker 但 `IsVisible=false`，且不保留占位宽度。
- 自动高度契约不受 Part 影响：未设置显式高度时 `DesiredSize.Height` 仍包含 Header 通道、内容内边距与内容期望高度。
- Generator 静态输出与 NativeAOT 路径不依赖反射或运行时扫描：四个 marker 通过静态 AXAML class 在既有模板路径一次性添加，
  不引入 VisualTree 搜索、动态 marker 绑定或运行时 AXAML 解析。
- 文档一致性：本文件与 `overview.md` 的 Part 摘要表保持一致，descriptor 与实际模板 marker 无差集。
