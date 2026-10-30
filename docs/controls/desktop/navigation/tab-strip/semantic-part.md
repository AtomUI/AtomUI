# TabStrip Semantic Part 契约

本文档定义 `TabStrip`、`CardTabStrip` 与 `TabStripItem` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。整体设计见
[TabStrip 桌面版架构设计](overview.md)，真实模板与生命周期见 [TabStrip 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

`TabStrip`（Line 风格）与 `CardTabStrip`（Card 风格）是独立页签条的两个 public owner，各自持有独立 descriptor。
`TabStripItem` 是二者的 item container，同时是独立 public owner，公开 item 内部的图标、标题与关闭按钮区域。
内容页版本的 `TabControl` 家族的契约见 [TabControl Semantic Part 契约](../tab-control/semantic-part.md)。

`BaseTabStrip`、`TabStripOverflowMenuItem`、`TabStripScrollViewer` 与 `TabsContainerPanel` 不持有独立 Semantic
descriptor；它们是基类或 internal 协作类型，不能作为公共 descriptor owner。

独立页签条不承载内容页，因此 `TabStrip` / `CardTabStrip` 不公开 `content` Part；`TabStripItem` 的 `icon` /
`label` / `close` 与 `TabItem` 同名 Part 语义一致。

## 1. Semantic Parts

| Owner | Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `TabStrip` | `root` | owner | `TabStrip` | `Single` | `Root` | `false` | `false` |
| `TabStrip` | `item` | `.semantic-item` | `TabStripItem` | `Multiple` | `Selector` | `false` | `true` |
| `CardTabStrip` | `root` | owner | `CardTabStrip` | `Single` | `Root` | `false` | `false` |
| `CardTabStrip` | `add` | `.semantic-add` | `IconButton` | `Single` | `Selector` | `false` | `false` |
| `CardTabStrip` | `item` | `.semantic-item` | `TabStripItem` | `Multiple` | `Selector` | `false` | `true` |
| `TabStripItem` | `root` | owner | `TabStripItem` | `Single` | `Root` | `false` | `false` |
| `TabStripItem` | `close` | `.semantic-close` | `IconButton` | `Single` | `Selector` | `false` | `false` |
| `TabStripItem` | `icon` | `.semantic-icon` | `IconPresenter` | `Single` | `Selector` | `false` | `false` |
| `TabStripItem` | `label` | `.semantic-label` | `ContentPresenter` | `Single` | `Selector` | `false` | `false` |

`root` 是各 owner 自身，承载选择、集合、关闭、排序等 public API、主题入口和状态归一，不声明 `.semantic-root` marker。

`TabStrip.item` 与 `CardTabStrip.item` 的 marker 是运行时创建的语义标记，由 owner 在
`CreateContainerForItemOverride` / `PrepareContainerForItemOverride` 中应用到生成的 `TabStripItem` 容器；直接以
`TabStripItem` 实例加入 `Items` 的 item 同样在 prepare 阶段获得 marker。

`TabStripItem` 的 `close` / `icon` / `label` 与 `CardTabStrip` 的 `add` 是内置模板中的静态 marker，通过
`Classes.semantic-*="True"` 声明，运行期间不随可见性、选中或禁用状态增删。

## 2. Part 说明

### 2.1 `TabStrip`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TabStrip` |
| Part | `root` |
| Selector | 不适用（owner 自身） |
| SelectorRoute | 不适用 |
| ContractType | `TabStrip` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TabStrip` owner |
| 职责 | 页签条根语义区域，承载选择、集合、关闭与排序状态入口。 |
| 相关 API | `SelectedIndex`、`SelectedItem`、`ItemsSource`、`TabStripPlacement`、`SizeType`、`IsTabReorderEnabled`、`TabActivationTrigger`、`Closing`、`Closed`、`TabReordering`、`TabReordered` |
| 相关 Token | 关联控件 Token（TabControl Token）+ SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是 `TabStrip` owner 本身，在控件实例的整个生命周期内始终存在，每个实例恰好一个。`TabStripTheme.axaml` 的
根模板是 `Border#Frame > Panel#AlignWrapper`（header 区），root 视觉定制直接作用于 owner 自身的公共属性
（`Background`、`BorderBrush`、`BorderThickness`、`Padding` 等）。root 不表示模板中的 `AlignWrapper`、
`HeaderLayout`、`PART_TabsContainer` 或 `PART_SelectedItemIndicator` 等内部节点。

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `TabStrip` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| ContractType | `TabStripItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个页签项（`TabStripItem` container，Line shape） |
| 职责 | 承载单个页签项的尺寸、状态与点击/关闭语义。 |
| 相关 API | `Content`、`ContentTemplate`、`Icon`、`CloseIcon`、`IsClosable`、`IsAutoHideCloseButton`、`IsSelected`、`SizeType` |
| 相关 Token | `HorizontalItemPadding(SM/LG)`、`VerticalItemPadding`、`TitleFontSize(SM/LG)`、`ItemColor`、`ItemHoverColor`、`ItemSelectedColor` 等 |
| 稳定性 | stable since 6.2.0 |

`item` 覆盖页签条内的全部页签项。marker 是运行时创建的语义标记：`TabStrip` 在
`CreateContainerForItemOverride` 中把 `semantic-item` 应用到新建的 `TabStripItem` 容器，并在
`PrepareContainerForItemOverride` 中对复用或直接提供的容器重新确认 marker。`SelectorRoute` 为
`> .semantic-item`，即容器是 owner 的直接逻辑子节点。

它明确不覆盖：

- 选中指示墨条 `PART_SelectedItemIndicator`（motion actor，不属于 item）。
- `HeaderStartExtraContent` / `HeaderEndExtraContent` 与加号按钮等 header 附属节点。
- overflow 菜单项（`TabStripOverflowMenuItem`，internal 类型）。

`ContractType` 为 `TabStripItem`：容器是 public 控件，用户 Semantic Style 可以依赖 `TabStripItem` 的
`Background`、`Padding`、`Margin`、`FontSize` 等属性。

### 2.2 `CardTabStrip`

`CardTabStrip` 的 `root` / `item` 语义与 `TabStrip` 同名 Part 一致，区别仅在模板变体：

- `root` 的 owner 是 `CardTabStrip`，模板根为 `Border#Frame`。
- `item` 的容器是 Card shape 的 `TabStripItem`（`TabSharp.Card`），marker 应用路径与 `TabStrip` 相同，
  `SelectorRoute` 为 `> .semantic-item`。

#### `add`

| 字段 | 值 |
| --- | --- |
| Owner | `CardTabStrip` |
| Part | `add` |
| Selector | `.semantic-add` |
| SelectorRoute | `/template/ .semantic-add` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `PART_AddTabButton`（`IconButton`，Plus 图标） |
| 职责 | 承载新增页签的触发按钮视觉。 |
| 相关 API | `IsShowAddTabButton`、`AddTabRequest` |
| 相关 Token | `AddTabButtonMarginHorizontal`、`AddTabButtonMarginVertical`、`ItemColor`、`ItemHoverColor` |
| 稳定性 | stable since 6.2.0 |

`add` 是内置模板中的静态标记：`CardTabStripTheme.axaml` 在 `PART_AddTabButton` 上声明
`Classes.semantic-add="True"`。按钮节点始终存在于模板中，`IsShowAddTabButton` 只控制可见性，marker 不随该属性
增删。该 Part 只属于 `CardTabStrip`；Line 风格的 `TabStrip` 没有加号按钮，其 descriptor 不含 `add`。

### 2.3 `TabStripItem`

`TabStripItem` 是 `TabStrip` / `CardTabStrip` 的 item container，同时是独立 public owner。它的三个子 Part 与
`TabItem` 的同名 Part 语义一致，是内置模板中的静态标记，Line（`BaseTabStripItemTheme.axaml`）与 Card
（`CardTabStripItemTheme.axaml`）两套模板都必须完整声明，每个模板恰好一个 marker。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TabStripItem` |
| Part | `root` |
| Selector | 不适用（owner 自身） |
| SelectorRoute | 不适用 |
| ContractType | `TabStripItem` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TabStripItem` owner |
| 职责 | 页签项根语义区域，承载单项选择、关闭、图标槽与拖动状态。 |
| 相关 API | `Content`、`ContentTemplate`、`Icon`、`CloseIcon`、`IsSelected`、`IsClosable`、`IsAutoHideCloseButton`、`HasIcon`、`IsIconSlotReserved`、`SizeType`、`TabStripPlacement` |
| 相关 Token | 关联控件 Token（TabControl Token）+ SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是 `TabStripItem` owner 本身，每个 item 实例恰好一个。root 视觉定制作用于 owner 自身的
`Background`、`Foreground`、`Padding`、`Margin`、`FontSize`、`BorderBrush`、`BorderThickness`、`CornerRadius`
等属性；Line 模板根为 `Border#Frame`，Card 模板根为 `PixelAlignedBorder#Frame`。

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `TabStripItem` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ItemIconPresenter`（`IconPresenter`） |
| 职责 | 承载页签图标区域的尺寸、间距与视觉样式。 |
| 相关 API | `Icon`、`HasIcon`、`IsIconSlotReserved`、`SizeType`、`TabStripPlacement` |
| 相关 Token | `ItemIconMargin` + SharedToken（`IconSize`、`IconSizeSM`） |
| 稳定性 | stable since 6.2.0 |

`icon` 是内置模板中的静态标记，声明在 `BaseTabStripItemTheme.axaml` 与 `CardTabStripItemTheme.axaml` 的
`ItemIconPresenter` 上。节点始终存在于模板中；`HasIcon` / `IsIconSlotReserved` 只控制可见性，marker 不随可见性
增删。

#### `label`

| 字段 | 值 |
| --- | --- |
| Owner | `TabStripItem` |
| Part | `label` |
| Selector | `.semantic-label` |
| SelectorRoute | `/template/ .semantic-label` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 页签标题 `ContentPresenter`（承载 `Content`） |
| 职责 | 承载页签标题文本或标题模板的展示区域。 |
| 相关 API | `Content`、`ContentTemplate`、`SizeType` |
| 相关 Token | `TitleFontSize(SM/LG)`、`ItemColor`、`ItemHoverColor`、`ItemSelectedColor` |
| 稳定性 | stable since 6.2.0 |

`label` 是内置模板中的静态标记，声明在两套 item 模板的标题 `ContentPresenter` 上。它只覆盖页签标题区域，不覆盖
图标与关闭按钮。

#### `close`

| 字段 | 值 |
| --- | --- |
| Owner | `TabStripItem` |
| Part | `close` |
| Selector | `.semantic-close` |
| SelectorRoute | `/template/ .semantic-close` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `PART_ItemCloseButton`（`IconButton`） |
| 职责 | 承载页签关闭按钮的图标、尺寸与交互视觉。 |
| 相关 API | `CloseIcon`、`IsClosable`、`IsAutoHideCloseButton` |
| 相关 Token | `CloseIconMargin` + SharedToken（`IconSizeSM`） |
| 稳定性 | stable since 6.2.0 |

`close` 是内置模板中的静态标记，声明在两套 item 模板的 `PART_ItemCloseButton` 上。按钮节点始终存在于模板中，
`IsClosable` / `IsAutoHideCloseButton` 只控制可见性与透明度（`CloseButtonOpacity`），marker 不随状态增删。

## 3. Selector 用法

应用侧使用 owner-scoped selector 和生成的 Style 类型，不手写 `/template/` 路径：

```xml
<Style Selector="atom|TabStrip.semantic-demo">
    <atom:TabStripItemStyle x:SetterTargetType="TabStripItem">
        <Setter Property="Margin" Value="0,0,4,0" />
        <Style Selector="^:selected">
            <Setter Property="Background" Value="#1A1677FF" />
        </Style>
    </atom:TabStripItemStyle>
</Style>

<Style Selector="atom|CardTabStrip.semantic-demo">
    <atom:CardTabStripAddStyle x:SetterTargetType="IconButton">
        <Setter Property="CornerRadius" Value="4" />
    </atom:CardTabStripAddStyle>
</Style>

<Style Selector="atom|TabStripItem.semantic-demo">
    <atom:TabStripItemIconStyle x:SetterTargetType="IconPresenter">
        <Setter Property="Margin" Value="0,0,4,0" />
    </atom:TabStripItemIconStyle>
    <atom:TabStripItemLabelStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontWeight" Value="SemiBold" />
    </atom:TabStripItemLabelStyle>
    <atom:TabStripItemCloseStyle x:SetterTargetType="IconButton">
        <Setter Property="Opacity" Value="0.8" />
    </atom:TabStripItemCloseStyle>
</Style>
```

`TabStripItemStyle`、`CardTabStripItemStyle`、`CardTabStripAddStyle`、`TabStripItemIconStyle`、
`TabStripItemLabelStyle` 与 `TabStripItemCloseStyle` 位于 `AtomUI.Theme.Styling` 命名空间，由语义生成器根据对应
`<Control>.SemanticParts.cs` 生成。生成类型已封装 owner 类型保护与 `SelectorRoute`；owner-scoped selector 可以是
状态 selector（如 `atom|TabStrip[SizeType=Large]`）。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。条目状态（hover、pressed、selected、disabled）继续由 item 主题的
selector 表达；Semantic Style 的 Setter 以 trigger 优先级覆盖主题值，如果希望保留某个状态视觉，应在生成 Style
内用嵌套 selector（如 `^:selected`）重新声明。

## 4. 状态与数量语义

- `root` 始终恰好一个，不随状态、模板重套用或集合变化增删。
- `TabStrip.item` / `CardTabStrip.item` 的 marker 数量等于当前 item 容器数量；marker 在容器创建与 prepare 阶段
  应用，不随选中、禁用或拖动状态增删，也不随页签溢出进 overflow 菜单转移。
- `CardTabStrip.add` 的 marker 数量恒为 1，`IsShowAddTabButton` 只改变可见性。
- `TabStripItem` 的 `icon` / `label` / `close` 在 Line 与 Card 两套模板中各有恰好一个 marker，不随 `HasIcon`、
  `IsClosable`、`IsAutoHideCloseButton`、`IsSelected` 或 `SizeType` 增删。
- 尺寸档 `Large` / `Middle` / `Small` 由 `BaseTabStrip.SizeType` / `TabStripItem.SizeType` 归一（默认 `Middle`），
  Semantic Style 不创建新的尺寸档；未被 Semantic Setter 覆盖的属性继续由当前 `SizeType` 分支提供，不能跨档位
  拼接（详见 [TabStrip 桌面版实现原理](implementation.md) 的 Semantic Part 尺寸与状态基线）。

## 5. 定制边界

- 选中指示墨条 `PART_SelectedItemIndicator` 是 motion actor，不属于任何公开 Part；定制墨条颜色请继续使用
  TabControl Token 的 `InkBarColor`。
- `HeaderStartExtraContent`、`HeaderEndExtraContent` 是 header 附属内容区，不是公开 Part。
- overflow 菜单、`PART_TabsContainer` 滚动容器、边缘渐变指示器与 `TabsContainerPanel` 是内部协作节点，不属于
  公开 Part，应用不应通过 Semantic Style 依赖。
- `item` 与 `add` 的布局型 Setter（`Margin`、`Padding`、`Width`、`Height`）作用于 header 区的 Measure/Arrange；
  需要按 §4 的尺寸基线验证 owner 的 `EffectiveHeaderPadding`、滚动视口与加号按钮并排布局边界。
- `TabStripItem` 子 Part 的布局型 Setter 会影响 item 自然高度（item 无固定 `Height`），必须按 `SizeType` 三档与
  `TabStripPlacement` 四向验证，不能通过固定 `Height` 或像素偏移掩盖测量不一致。

## 6. 兼容性与验证

- 三个 owner 的 descriptor 均以 `root` 为隐式 owner，不存在 `.semantic-root` marker。
- `TabStrip.item` / `CardTabStrip.item` 的 `SelectorRoute` 为 `> .semantic-item`；`add` 与 `TabStripItem` 子 Part
  为默认 `/template/ .semantic-*`。删除、重命名 Part、修改 selector class 或 route、收窄 `ContractType`、改变
  cardinality 均属于破坏性变更，必须同步生成 descriptor、主题 marker、运行时代码与回归测试。
- 静态 marker 必须保持 `Classes.semantic-*="True"` 形式，不得使用字面量 `Classes`、`False`、Binding 或动态值。
- 运行时 marker 行为回归测试见 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabStripSemanticPartTests.cs`；
  内容页版本家族见 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabControlSemanticPartTests.cs`。
