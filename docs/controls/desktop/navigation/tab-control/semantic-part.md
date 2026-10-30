# TabControl Semantic Part 契约

本文档定义 `TabControl`、`CardTabControl` 与 `TabItem` 对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。整体设计见
[TabControl 桌面版架构设计](overview.md)，真实模板与生命周期见 [TabControl 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

`TabControl`（Line 风格）与 `CardTabControl`（Card 风格）是同一控件家族的两个 public owner，各自持有独立 descriptor。
`TabItem` 是二者的 item container，同时是独立 public owner，公开 item 内部的图标、标题与关闭按钮区域。独立页签条
`TabStrip` 家族的契约见 [TabStrip Semantic Part 契约](../tab-strip/semantic-part.md)。

`BaseTabControl`、`BaseTabStrip`、`TabControlOverflowMenuItem`、`TabStripOverflowMenuItem`、`BaseTabScrollViewer` 与
`TabsContainerPanel` 不持有独立 Semantic descriptor；它们是基类或 internal 协作类型，不能作为公共 descriptor owner。

## 1. Semantic Parts

| Owner | Part | Selector | ContractType | Cardinality | Customization | CrossVisualRoot | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `TabControl` | `root` | owner | `TabControl` | `Single` | `Root` | `false` | `false` |
| `TabControl` | `header` | `.semantic-header` | `Border` | `Single` | `Selector` | `false` | `false` |
| `TabControl` | `content` | `.semantic-content` | `ContentPresenter` | `Single` | `Selector` | `false` | `false` |
| `TabControl` | `item` | `.semantic-item` | `TabItem` | `Multiple` | `Selector` | `false` | `true` |
| `TabControl` | `indicator` | `.semantic-indicator` | `Border` | `Single` | `Selector` | `false` | `false` |
| `CardTabControl` | `root` | owner | `CardTabControl` | `Single` | `Root` | `false` | `false` |
| `CardTabControl` | `header` | `.semantic-header` | `Border` | `Single` | `Selector` | `false` | `false` |
| `CardTabControl` | `add` | `.semantic-add` | `IconButton` | `Single` | `Selector` | `false` | `false` |
| `CardTabControl` | `content` | `.semantic-content` | `ContentPresenter` | `Single` | `Selector` | `false` | `false` |
| `CardTabControl` | `item` | `.semantic-item` | `TabItem` | `Multiple` | `Selector` | `false` | `true` |
| `TabItem` | `root` | owner | `TabItem` | `Single` | `Root` | `false` | `false` |
| `TabItem` | `close` | `.semantic-close` | `IconButton` | `Single` | `Selector` | `false` | `false` |
| `TabItem` | `icon` | `.semantic-icon` | `IconPresenter` | `Single` | `Selector` | `false` | `false` |
| `TabItem` | `label` | `.semantic-label` | `ContentPresenter` | `Single` | `Selector` | `false` | `false` |

`root` 是各 owner 自身，承载选择、集合、关闭、排序等 public API、主题入口和状态归一，不声明 `.semantic-root` marker。

`TabControl.item` 与 `CardTabControl.item` 的 marker 是运行时创建的语义标记，由 owner 在
`CreateContainerForItemOverride` / `PrepareContainerForItemOverride` 中应用到生成的 `TabItem` 容器；直接以
`TabItem` 实例加入 `Items` 的 item 同样在 prepare 阶段获得 marker。

`TabItem` 的 `close` / `icon` / `label`、两个 Control 的 `content` 与 `header`、`TabControl` 的 `indicator`、
`CardTabControl` 的 `add` 是内置模板中的静态 marker，通过 `Classes.semantic-*="True"` 声明，运行期间不随可见性、
选中或禁用状态增删。

## 2. Part 说明

### 2.1 `TabControl`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TabControl` |
| Part | `root` |
| Selector | 不适用（owner 自身） |
| SelectorRoute | 不适用 |
| ContractType | `TabControl` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TabControl` owner |
| 职责 | 页签控件根语义区域，承载选择、集合、关闭、排序与内容页状态入口。 |
| 相关 API | `SelectedIndex`、`SelectedItem`、`SelectedContent`、`ItemsSource`、`TabStripPlacement`、`SizeType`、`IsTabReorderEnabled`、`TabActivationTrigger`、`Closing`、`Closed`、`TabReordering`、`TabReordered` |
| 相关 Token | TabControl Token + SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是 `TabControl` owner 本身，在控件实例的整个生命周期内始终存在，每个实例恰好一个。它承载页签切换、内容页选择、
关闭流程与拖动排序的状态入口，并作为 `content` / `item` owner-scoped Selector 的作用域边界。

`TabControlTheme.axaml` 的根模板是 `PixelAlignedBorder#Frame > DockPanel`（header 区 + 内容区）。root 视觉定制直接作用于
owner 自身的公共视觉属性并 TemplateBinding 到模板根 `Frame`：`Background`、`BackgroundSizing`、`BorderBrush`、
`BorderThickness`、`CornerRadius`、`Padding` 同名绑定，`BorderDashArray` / `BorderDashOffset`（since 6.2.0）绑定到
`Frame` 的 `StrokeDashArray` / `StrokeDaskOffset`，因此虚线边框等 root 视觉直接渲染。标签条与内容区之间的分隔线由
internal `SeparatorBorderBrush` / `SeparatorBorderThickness` 承接主题 token，不再占用公开 `BorderBrush` /
`BorderThickness` 的默认值。root 不表示模板中的 `PART_AlignWrapper`、`HeaderLayout`、`PART_TabsContainer` 或
`PART_SelectedItemIndicator` 等内部节点。

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner | `TabControl` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `/template/ .semantic-header` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | header 包裹 `Border`（`Padding` = `EffectiveHeaderPadding`，内含 `HeaderLayout`、`PART_TabsContainer` 与 extra content） |
| 职责 | 承载标签条头部区域（页签列表 + 扩展内容）的背景、内边距与对齐视觉。 |
| 相关 API | `HeaderStartEdgePadding`、`HeaderEndEdgePadding`、`HeaderStartExtraContent`、`HeaderEndExtraContent`、`TabStripPlacement` |
| 相关 Token | TabControl Token + SharedToken |
| 稳定性 | stable since 6.2.0 |

`header` 是内置模板中的静态标记：`TabControlTheme.axaml` 在包裹标签条的 header `Border` 上声明
`Classes.semantic-header="True"`，每个内置模板恰好一个 marker。它覆盖整个标签条头部区域（含 `HeaderLayout` 与
`HeaderStartExtraContent` / `HeaderEndExtraContent`），但不覆盖 `PART_AlignWrapper` 的外边距（`TabStripMargin`）与
`PART_SelectedItemIndicator` 墨条。`HeaderStartExtraContent` / `HeaderEndExtraContent` 是 header 区域内部的附属节点，
不是公开 Part。

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `TabControl` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 内容区 `ContentPresenter`（承载 `SelectedContent`） |
| 职责 | 承载当前选中页内容的内容展示区域，可定制内边距、对齐与内容样式。 |
| 相关 API | `SelectedContent`、`SelectedContentTemplate`、`ContentPadding`、`HorizontalContentAlignment`、`VerticalContentAlignment` |
| 相关 Token | TabControl Token（`TabAndContentGutter` 等）+ SharedToken |
| 稳定性 | stable since 6.2.0 |

`content` 是内置模板中的静态标记：`TabControlTheme.axaml` 在模板末尾承载 `SelectedContent` 的
`ContentPresenter` 上声明 `Classes.semantic-content="True"`，每个内置模板恰好一个 marker。节点始终存在于模板中，
不随 `SelectedIndex`、`SelectedContent` 为空或禁用状态增删。

布局型 Setter（`Padding`、`Margin`、`HorizontalContentAlignment` 等）作用于内容展示区：owner 不固定内容区
`Height`，内容区尺寸由自然测量与父 `DockPanel` 排列决定；用户覆盖 `Padding` 时会覆盖 `ContentPadding` 的
TemplateBinding 值，需按 §4 的尺寸基线验证。

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `TabControl` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| ContractType | `TabItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 tab 头项（`TabItem` container，Line shape） |
| 职责 | 承载单个页签头的尺寸、状态与点击/关闭语义。 |
| 相关 API | `Header`、`Icon`、`CloseIcon`、`IsClosable`、`IsAutoHideCloseButton`、`IsSelected`、`SizeType` |
| 相关 Token | `HorizontalItemPadding(SM/LG)`、`VerticalItemPadding`、`TitleFontSize(SM/LG)`、`ItemColor`、`ItemHoverColor`、`ItemSelectedColor` 等 |
| 稳定性 | stable since 6.2.0 |

`item` 覆盖 tab 条内的全部页签头项。marker 是运行时创建的语义标记：`TabControl` 在
`CreateContainerForItemOverride` 中把 `semantic-item` 应用到新建的 `TabItem` 容器，并在
`PrepareContainerForItemOverride` 中对复用或直接提供的容器重新确认 marker；数据 item 与直接 `TabItem` 实例
两种用法保持一致。`SelectorRoute` 为 `> .semantic-item`，即容器是 owner 的直接逻辑子节点，不依赖模板作用域 marker。

它明确不覆盖：

- 选中指示墨条 `PART_SelectedItemIndicator`（motion actor，不属于 item）。
- `HeaderStartExtraContent` / `HeaderEndExtraContent` 与加号按钮等 header 附属节点。
- overflow 菜单项（`TabControlOverflowMenuItem`，internal 类型），溢出后的页签在菜单中以独立菜单项呈现，不携带
  `semantic-item` marker。

`ContractType` 为 `TabItem`：容器是 public 控件，用户 Semantic Style 可以依赖 `TabItem` 的
`Background`、`Padding`、`Margin`、`FontSize` 等属性；不要依赖 `TabItem` 模板内部的 `PART_*` 节点。

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `TabControl` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `/template/ .semantic-indicator` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `PART_SelectedItemIndicator`（`Border`，选中指示墨条） |
| 职责 | 承载选中页签指示墨条的视觉样式。 |
| 相关 API | `SelectedItem`、`SelectedIndex`、`TabStripPlacement` |
| 相关 Token | `InkBarColor`、`InkBarThickness` + SharedToken |
| 稳定性 | stable since 6.2.0 |

`indicator` 是内置模板中的静态标记：`TabControlTheme.axaml` 在 `Border#PART_SelectedItemIndicator` 上声明
`Classes.semantic-indicator="True"`，每个内置模板恰好一个 marker。墨条是 motion actor——宽度、高度与
`RenderTransform` 由控件在 `SetupSelectedIndicator` 中按选中 item 边界维护，用户 Semantic Style 对布局属性的覆盖
不会生效；颜色（`Background`）等视觉属性可通过生成样式定制，厚度通过 `InkBarThickness` token 定制。
`CardTabControl` 没有该 Part：Card 模板不含指示墨条节点，选中态由 `LineMask` 卡片遮罩表达，与 antd Card 型隐藏
ink bar 的行为一致。

### 2.2 `CardTabControl`

`CardTabControl` 的 `root` / `header` / `content` / `item` 语义与 `TabControl` 同名 Part 一致，区别仅在模板变体：

- `root` 的 owner 是 `CardTabControl`，模板根为 `PixelAlignedBorder#Frame`，对上述全部根视觉属性 TemplateBind。
- `header` 的 marker 声明在 `CardTabControlTheme.axaml` 包裹标签条的 header `Border` 上，`SelectorRoute` 为
  `/template/ .semantic-header`。
- `content` 的 marker 声明在 `CardTabControlTheme.axaml` 末尾承载 `SelectedContent` 的 `ContentPresenter` 上，
  `SelectorRoute` 为 `/template/ .semantic-content`。
- `item` 的容器是 Card shape 的 `TabItem`（`TabSharp.Card`），marker 应用路径与 `TabControl` 相同，
  `SelectorRoute` 为 `> .semantic-item`。
- `CardTabControl` 不公开 `indicator`：Card 模板不含 `PART_SelectedItemIndicator`，选中态由 `LineMask` 表达。

#### `add`

| 字段 | 值 |
| --- | --- |
| Owner | `CardTabControl` |
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

`add` 是内置模板中的静态标记：`CardTabControlTheme.axaml` 在 `PART_AddTabButton` 上声明
`Classes.semantic-add="True"`。按钮节点始终存在于模板中，`IsShowAddTabButton` 只控制可见性，marker 不随该属性
增删。该 Part 只属于 `CardTabControl`；Line 风格的 `TabControl` 没有加号按钮，其 descriptor 不含 `add`。

### 2.3 `TabItem`

`TabItem` 是 `TabControl` / `CardTabControl` 的 item container，同时是独立 public owner。它的三个子 Part 是内置
模板中的静态标记，Line（`BaseTabItemTheme.axaml`）与 Card（`CardTabItemTheme.axaml`）两套模板都必须完整声明，
每个模板恰好一个 marker。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TabItem` |
| Part | `root` |
| Selector | 不适用（owner 自身） |
| SelectorRoute | 不适用 |
| ContractType | `TabItem` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TabItem` owner |
| 职责 | 页签项根语义区域，承载单项选择、关闭、图标槽与拖动状态。 |
| 相关 API | `Header`、`Icon`、`CloseIcon`、`IsSelected`、`IsClosable`、`IsAutoHideCloseButton`、`HasIcon`、`IsIconSlotReserved`、`SizeType`、`TabStripPlacement` |
| 相关 Token | TabControl Token + SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是 `TabItem` owner 本身，每个 item 实例恰好一个。root 视觉定制作用于 owner 自身的
`Background`、`Foreground`、`Padding`、`Margin`、`FontSize`、`BorderBrush`、`BorderThickness`、`CornerRadius`
等属性；Line 模板根为 `Border#Frame`，Card 模板根为 `PixelAlignedBorder#Frame`，均对上述根视觉属性 TemplateBind。

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `TabItem` |
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

`icon` 是内置模板中的静态标记，声明在 `BaseTabItemTheme.axaml` 与 `CardTabItemTheme.axaml` 的
`ItemIconPresenter` 上。节点始终存在于模板中；`HasIcon` / `IsIconSlotReserved` 只控制可见性，marker 不随可见性
增删，因此对 `Single` 的 icon Part 设置可见性类属性时需注意会同时影响图标槽占位语义。

#### `label`

| 字段 | 值 |
| --- | --- |
| Owner | `TabItem` |
| Part | `label` |
| Selector | `.semantic-label` |
| SelectorRoute | `/template/ .semantic-label` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 页签标题 `ContentPresenter`（承载 `Header`） |
| 职责 | 承载页签标题文本或标题模板的展示区域。 |
| 相关 API | `Header`、`HeaderTemplate`、`SizeType` |
| 相关 Token | `TitleFontSize(SM/LG)`、`ItemColor`、`ItemHoverColor`、`ItemSelectedColor` |
| 稳定性 | stable since 6.2.0 |

`label` 是内置模板中的静态标记，声明在两套 item 模板的标题 `ContentPresenter` 上。它只覆盖页签标题区域，不覆盖
图标与关闭按钮；适合字体、前景色、内边距等标题样式定制。

#### `close`

| 字段 | 值 |
| --- | --- |
| Owner | `TabItem` |
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
<Style Selector="atom|TabControl.semantic-demo">
    <atom:TabControlHeaderStyle x:SetterTargetType="Border">
        <Setter Property="Background" Value="#80F5F5F5" />
    </atom:TabControlHeaderStyle>
    <atom:TabControlContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Padding" Value="16" />
        <Setter Property="Background" Value="#CCE6F7FF" />
    </atom:TabControlContentStyle>
    <atom:TabControlItemStyle x:SetterTargetType="atom:TabItem">
        <Setter Property="Padding" Value="6,10" />
    </atom:TabControlItemStyle>
</Style>

<Style Selector="atom|TabItem.semantic-demo">
    <atom:TabItemLabelStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontWeight" Value="Bold" />
        <Setter Property="Foreground" Value="#1890FF" />
    </atom:TabItemLabelStyle>
</Style>

<!-- root 视觉直接设置 owner 属性，TemplateBinding 到模板根 Frame -->
<atom:TabControl Classes="semantic-demo"
                 BorderThickness="2"
                 BorderBrush="#E0000000"
                 BorderDashArray="4,2"
                 Padding="16" />
```

`TabControlContentStyle`、`TabControlHeaderStyle`、`TabControlIndicatorStyle`、`TabControlItemStyle`、`CardTabControlAddStyle`、
`CardTabControlContentStyle`、`CardTabControlHeaderStyle`、`CardTabControlItemStyle`、`TabItemIconStyle`、
`TabItemLabelStyle` 与 `TabItemCloseStyle` 位于
`AtomUI.Theme.Styling` 命名空间，由语义生成器根据对应 `<Control>.SemanticParts.cs` 生成。生成类型已封装 owner
类型保护与 `SelectorRoute`；owner-scoped selector 可以是状态 selector（如 `atom|TabControl[SizeType=Large]`）。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。条目状态（hover、pressed、selected、disabled）继续由 item 主题的
selector 表达；Semantic Style 的 Setter 以 trigger 优先级覆盖主题值，如果希望保留某个状态视觉，应在生成 Style
内用嵌套 selector（如 `^:selected`）重新声明。

## 4. 状态与数量语义

- `root` 始终恰好一个，不随状态、模板重套用或集合变化增删。
- `TabControl.item` / `CardTabControl.item` 的 marker 数量等于当前 item 容器数量；marker 在容器创建与 prepare
  阶段应用，不随选中、禁用或拖动状态增删，也不随页签溢出进 overflow 菜单转移。
- `content` 的 marker 数量恒为 1，不随 `SelectedContent`、选中项或禁用状态增删。
- `TabControl.header` / `CardTabControl.header` 的 marker 数量恒为 1，不随页签溢出、滚动或 `TabStripPlacement` 增删。
- `TabControl.indicator` 的 marker 数量恒为 1，不随选中项、`TabStripPlacement` 或可见性增删。
- `CardTabControl.add` 的 marker 数量恒为 1，`IsShowAddTabButton` 只改变可见性。
- `TabItem` 的 `icon` / `label` / `close` 在 Line 与 Card 两套模板中各有恰好一个 marker，不随 `HasIcon`、
  `IsClosable`、`IsAutoHideCloseButton`、`IsSelected` 或 `SizeType` 增删。
- 尺寸档 `Large` / `Middle` / `Small` 由 `BaseTabControl.SizeType` / `TabItem.SizeType` 归一（默认 `Middle`），
  Semantic Style 不创建新的尺寸档；未被 Semantic Setter 覆盖的属性继续由当前 `SizeType` 分支提供，不能跨档位
  拼接（详见 [TabControl 桌面版实现原理](implementation.md) 的 Semantic Part 尺寸与状态基线）。

## 5. 定制边界

- 选中指示墨条 `PART_SelectedItemIndicator` 是 motion actor，其尺寸与位移由控件运行时维护；墨条颜色默认由
  `InkBarColor` token 提供，可通过 `TabControl.indicator` Part 的生成样式覆盖，厚度通过 `InkBarThickness` token
  定制。
- header 区域的附属节点 `HeaderStartExtraContent` / `HeaderEndExtraContent` 是 header Part 的内部子节点，不是公开
  Part。
- overflow 菜单、`PART_TabsContainer` 滚动容器、边缘渐变指示器与 `TabsContainerPanel` 是内部协作节点，不属于
  公开 Part，应用不应通过 Semantic Style 依赖。
- Card 模板中的 `LineMask`（选中态卡片遮罩）是内部视觉节点，不是公开 Part。
- `item` 与 `add` 的布局型 Setter（`Margin`、`Padding`、`Width`、`Height`）作用于 header 区的 Measure/Arrange；
  需要按 §4 的尺寸基线验证 owner 的 `EffectiveHeaderPadding`、滚动视口与加号按钮并排布局边界。
- `TabItem` 子 Part 的布局型 Setter 会影响 item 自然高度（item 无固定 `Height`），必须按 `SizeType` 三档与
  `TabStripPlacement` 四向验证，不能通过固定 `Height` 或像素偏移掩盖测量不一致。

## 6. 兼容性与验证

- 三个 owner 的 descriptor 均以 `root` 为隐式 owner，不存在 `.semantic-root` marker。
- `TabControl.item` / `CardTabControl.item` 的 `SelectorRoute` 为 `> .semantic-item`；`content`、`header`、`indicator`、`add` 与
  `TabItem` 子 Part 为默认 `/template/ .semantic-*`。删除、重命名 Part、修改 selector class 或 route、收窄
  `ContractType`、改变 cardinality 均属于破坏性变更，必须同步生成 descriptor、主题 marker、运行时代码与回归测试。
- 静态 marker 必须保持 `Classes.semantic-*="True"` 形式，不得使用字面量 `Classes`、`False`、Binding 或动态值。
- 运行时 marker 行为回归测试见 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabControlSemanticPartTests.cs`；
  独立页签条家族见 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabStripSemanticPartTests.cs`。
