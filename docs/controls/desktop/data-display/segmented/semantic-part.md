# Segmented Semantic Part 契约

本文档定义 `Segmented` 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Segmented 的整体设计见
[Segmented 桌面版架构设计](overview.md)，descriptor 与真实模板节点映射见 [Segmented 桌面版实现原理](implementation.md)，
系统级规则见 [AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Segmented 主控件公开 `root`、`item`、`icon` 与 `label` 四个职责区域，与上游稳定 Semantic DOM 对齐。上游基线为
6.6.0 稳定发布的 `SegmentedSemanticType`（`classNames` / `styles` 均为 `{ root?, icon?, label?, item? }`）：

- `root` 消费于 `.ant-segmented` 根节点，`icon` 消费于 `.ant-segmented-item-icon`，由上游 `Segmented` 组件下发；
- `item` 消费于 `.ant-segmented-item` 选项容器，`label` 消费于 `.ant-segmented-item-label`，由 rc-segmented 选项
  渲染路径消费；
- 上游选中滑块（MotionThumb）没有 Semantic key，AtomUI 同样不公开。

AtomUI 四个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since` 统一为 `6.2.0`。

`SegmentedItem` 不持有独立 Semantic descriptor：

- 上游 `Segmented` 只提供一个 owner 的 Semantic DOM；选项没有独立公开的 Semantic DOM Props。
- `SegmentedItem` 是 Segmented 的运行时容器，其职责通过 `Segmented` 的 `item` Part 对外公开；item 模板内的图标与
  文本节点通过 `icon`、`label` Part 以多跳 route 公开。
- `AbstractSegmented` 与 `AbstractSegmentedItem` 是跨平台共享基类，不是对应用公开的独立 owner，不声明 descriptor。

因此本控件的 Semantic Part 只由 `Segmented` owner 公开。

### 1.1 `Segmented`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Segmented` |
| Part | `root` |
| Selector | Segmented 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Segmented` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Segmented owner（表面投影到 `Frame`） |
| 职责 | Segmented root 是选项数据、选择状态、方向、形状与轨道表面样式的统一 owner。轨道背景由 owner `Render` 直接绘制，`Frame` 承载圆角、内边距与内容裁剪。 |
| 相关 API | `ItemsSource`、`ItemTemplate`、`SelectedIndex`、`SelectedItem`、`SelectionChanged`、`SizeType`、`Orientation`、`Shape`、`IsExpanding`、`IsMotionEnabled` |
| 相关 Token | `TrackBg`、`TrackPadding`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `Segmented` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| Style Type | `SegmentedItemStyle` |
| ContractType | `SegmentedItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `SegmentedItem` 容器 |
| 职责 | 统一表示单个选项容器的背景、前景、圆角、内边距、最小高度、光标与选择 / 悬浮 / 按压 / 禁用视觉；对应上游 `.ant-segmented-item`。 |
| 相关 API | `SegmentedItem.Icon`、`SegmentedItem.Content`、`SegmentedItem.IsSelected`、`SizeType`、`Shape`、`IsMotionEnabled` |
| 相关 Token | `ItemColor`、`ItemHoverColor`、`ItemSelectedColor`、`ItemHoverBg`、`ItemActiveBg`、`ItemSelectedBg`、`ItemMinHeightLG`、`ItemMinHeight`、`ItemMinHeightSM`、`SegmentedItemPadding`、`SegmentedItemPaddingSM` |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `Segmented` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `> .semantic-item /template/ .semantic-icon` |
| Style Type | `SegmentedIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `SegmentedItem` 模板中的 `IconPresenter#IconPresenter` |
| 职责 | 统一表示每个选项的图标区域：图标画刷状态色、图标尺寸与可见性；对应上游 `.ant-segmented-item-icon`。 |
| 相关 API | `SegmentedItem.Icon`、`SizeType` |
| 相关 Token | `ItemColor`、`ItemHoverColor`、`ItemSelectedColor`、SharedToken（`IconSizeLG` / `IconSize` / `IconSizeSM`、`ColorTextDisabled`） |
| 稳定性 | stable since 6.2.0 |

#### `label`

| 字段 | 值 |
| --- | --- |
| Owner | `Segmented` |
| Part | `label` |
| Selector | `.semantic-label` |
| SelectorRoute | `> .semantic-item /template/ .semantic-label` |
| Style Type | `SegmentedLabelStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 `SegmentedItem` 模板中的 `ContentPresenter#Content` |
| 职责 | 统一表示每个选项的文本区域：文本呈现、居中对齐、省略与图文间距；对应上游 `.ant-segmented-item-label`。 |
| 相关 API | `SegmentedItem.Content`、`SegmentedItem.ContentTemplate` |
| 相关 Token | `SegmentedItemContentMargin` |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不声明 `.semantic-root` marker。`item` 的 marker `.semantic-item` 在 `SegmentedItem` 创建路径
一次性添加，`PrepareContainerForItemOverride` 幂等补齐（覆盖回收容器与用户直接提供容器的路径）。`icon`、`label`
的 marker 声明在 `SegmentedItemTheme.axaml` 模板内的 `IconPresenter#IconPresenter` 与 `ContentPresenter#Content`
节点上，随容器模板实例化而存在；由于它们只在运行时随 item 容器创建，descriptor 声明为 `RuntimeCreated`，route 以
`> .semantic-item` 为作用域跳点，再经 `/template/` 进入 item 模板。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。

## 2. Part 说明

### 2.1 root

`root` 是 Segmented owner 本身，在 Segmented 实例的整个生命周期内始终存在，并且每个 Segmented 恰好一个。

它负责：

- 承载 `ItemsSource`、`ItemTemplate`、选择状态、`SizeType`、`Orientation`、`Shape`、`IsExpanding` 等公共状态和 API。
- 提供轨道表面视觉：背景（`TrackBg`）由 owner `Render` 直接绘制到自身，圆角、内边距（`TrackPadding`）与内容裁剪
  投影到模板中的 `Frame`。
- 作为 `item`、`icon`、`label` owner-scoped Selector 的作用域边界。

选中滑块（selected thumb）不是 root 表面的一部分：它由 owner `Render` 在背景之上直接绘制，依赖
`SelectedThumbPos` / `SelectedThumbSize` / `SelectedThumbBg` / `SelectedThumbCornerRadius` / `SelectedThumbBoxShadows`
internal 状态，没有模板 Visual 节点，因此不属于任何 Part（见 6 节）。

适合通过 root 定制 Segmented 整体背景、圆角、padding 与对齐。需要根据状态改变根样式时，应在 owner Selector 上
组合公开属性或伪类。

root 不表示模板中的 `Frame`、`PART_ItemsPresenter`、`SegmentedStackPanel` 节点本身；这些节点的名称、数量与层级
不属于 root 契约。

### 2.2 item

`item` 表示一个选项容器，每个 `SegmentedItem` 恰好一个 marker，cardinality 为 `Multiple`。marker `.semantic-item`
在 `SegmentedItem` 创建路径一次性添加，prepare 幂等补齐；容器生命周期内不增删 marker。

它负责：

- 设置选项背景与前景状态色（默认 / hover / pressed / selected / disabled）。
- 设置选项圆角（SizeType 分支与 `Shape=Round`）、内边距与最小高度。
- 承载 `Icon` / `Content` / `ContentTemplate` 的最终布局（DockPanel 图文排列）。
- 承载 `:selected`、`:pointerover`、`:pressed`、`:disabled`、`:has-icon` 伪类与选择交互。

适合定制 `Background`、`Foreground`、`CornerRadius`、`Padding`、`MinHeight`、`FontSize`、`Cursor`、`Opacity` 等
属性。选项背景默认由选中滑块覆盖表达选中态：非选中项背景为 `Transparent`，选中项背景在滑块下方不可见，覆盖
`item` 的 `Background` 不会改变滑块外观（滑块不属于任何 Part）。

item 不公开模板中的 `Frame`、`DockPanel` 布局节点，也不公开用户 `ItemTemplate` 生成的子树。

### 2.3 icon

`icon` 表示一个选项的图标区域，每个 `SegmentedItem` 模板恰好一个 `IconPresenter#IconPresenter` 节点，
cardinality 为 `Multiple`（每个已实例化容器一个）。

它负责：

- 承载 `SegmentedItem.Icon` 的呈现（`PathIcon`）。
- 设置图标画刷状态色：默认 `ItemColor`，hover `ItemHoverColor`，selected `ItemSelectedColor`，disabled
  `ColorTextDisabled`。
- 设置图标尺寸：`IconSizeLG` / `IconSize` / `IconSizeSM` 按 SizeType 三档。

节点可见性由数据驱动：`Icon` 为 null 时 `IconPresenter` 折叠，marker 不移除；纯图标选项（`Content` 为 null）与
图文选项（`:has-icon`）共用同一节点。

适合定制 `IconBrush`、`Width`、`Height`、`Margin` 等属性；覆盖尺寸时应同时验证三档 SizeType 与图文 / 纯图标
两种内容形态。

### 2.4 label

`label` 表示一个选项的文本区域，每个 `SegmentedItem` 模板恰好一个 `ContentPresenter#Content` 节点，
cardinality 为 `Multiple`（每个已实例化容器一个）。

它负责：

- 承载 `SegmentedItem.Content` 与 `ContentTemplate` 的文本呈现，水平垂直居中，`TextTrimming=CharacterEllipsis`。
- 承载图文间距：`:has-icon` 状态下应用 `SegmentedItemContentMargin`。

节点可见性由数据驱动：`Content` 为 null 时（纯图标选项）`ContentPresenter` 折叠，marker 不移除。

适合定制 `Margin`、`Foreground`（随容器前景继承）、`TextTrimming` 等属性；文本颜色通常由 `item` 的
`Foreground` 状态机统一表达，单独覆盖 `label` 前景会脱离状态机，需要自行覆盖全部状态分支。

## 3. Selector 用法

应用级样式先限定 Segmented owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Segmented">
        <atom:SegmentedItemStyle x:SetterTargetType="atom:SegmentedItem">
            <Setter Property="MinHeight" Value="40" />
        </atom:SegmentedItemStyle>
        <atom:SegmentedIconStyle x:SetterTargetType="atom|IconPresenter">
            <Setter Property="IconBrush" Value="#1677FF" />
        </atom:SegmentedIconStyle>
        <atom:SegmentedLabelStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Margin" Value="8,0,0,0" />
        </atom:SegmentedLabelStyle>
    </Style>
</Application.Styles>
```

对特定 Segmented class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|Segmented.semantic-custom:pointerover">
    <atom:SegmentedItemStyle x:SetterTargetType="atom:SegmentedItem">
        <Setter Property="Foreground" Value="#1677ff" />
    </atom:SegmentedItemStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|SegmentedItem.semantic-item` 或 `:is(atom|SegmentedItem).semantic-item`。
- 直接复制 `> .semantic-item` 或 `> .semantic-item /template/ .semantic-icon` route 作为用户主路径；route 只属于
  descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。
- 试图通过任何 Part 定位选中滑块；滑块没有 Visual 节点，Semantic Style 无法触达。

## 4. 状态与数量语义

数量契约以已实例化的 AtomUI 内置容器为边界。`item`、`icon`、`label` 是 `RuntimeCreated` Part：marker 随容器实例
存在，不随 item 数据迁移。

| 场景 | root | item | icon / label | 说明 |
| --- | --- | --- | --- | --- |
| 普通选项 N 个 | 1 | N | N / N | 每个 `SegmentedItem` 一个 `semantic-item`，模板内各一个 `semantic-icon`、`semantic-label`。 |
| 空集合 | 1 | 0 | 0 / 0 | 不创建容器，也不实例化 item 模板。 |
| 集合重置 / 增删 | 1 | 随容器数 | 随容器数 | 新容器创建时建 marker；容器释放时 marker 随之消失。 |
| 纯图标选项 | 1 | N | N / N | `label` 节点折叠但 marker 保留。 |
| 纯文本选项 | 1 | N | N / N | `icon` 节点折叠但 marker 保留。 |
| hover / selected / pressed / disabled | 1 | N | N / N | 状态切换只改变有效视觉属性，不增删 marker。 |
| 选中滑块移动 | 1 | N | N / N | 滑块是 owner `Render` 绘制层，不产生也不消耗 marker。 |

## 5. 尺寸基线

Segmented 通过 `SizeType` 提供 Large、Middle / Custom、Small 三档基线，`item`、`icon` 与 root 圆角同时分档：

- root 圆角：`BorderRadiusLG` / `BorderRadius` / `BorderRadiusSM`；item 圆角：`BorderRadius` / `BorderRadiusSM` /
  `BorderRadiusXS`。
- item 最小高度：`ItemMinHeightLG` / `ItemMinHeight` / `ItemMinHeightSM`；item padding：`SegmentedItemPadding` /
  `SegmentedItemPaddingSM`。
- 图标尺寸：`IconSizeLG` / `IconSize` / `IconSizeSM`。

`Shape=Round` 在三档之后统一覆盖 root 与 item 圆角为 `9999` 几何胶囊。`Orientation=Vertical` 与 `IsExpanding`
只影响布局度量，不改变 marker 数量与 Part 边界。

Semantic Style 覆盖 `item` 的 `MinHeight`、`Padding`、`CornerRadius` 或 `icon` 尺寸时，应同时验证三档 SizeType、
`Shape=Round` 胶囊、横竖方向与图文 / 纯图标两种内容形态下选中滑块仍与选项对齐（滑块矩形由 item 最终 Bounds
推导）。

## 6. 定制边界

以下区域明确不属于 Segmented Semantic Part：

- 选中滑块：由 owner `Render` 直接绘制（`SelectedThumbPos` / `SelectedThumbSize` / `SelectedThumbBg` /
  `SelectedThumbCornerRadius` / `SelectedThumbBoxShadows`），没有 Visual 节点，上游也没有对应 Semantic key；滑块
  视觉由 `IsMotionEnabled`、SizeType 与 Shape 状态机决定，Semantic Style 不能触达。
- `SegmentedItem` 容器自身不作为独立 owner：它的公开职责全部由 `item` Part 表达。
- `SegmentedStackPanel`（internal 方向感知 items panel）、`Frame`、`DockPanel`、`PART_ItemsPresenter` 等模板结构
  节点。
- 用户 `ItemTemplate` 创建的子树。
- `AbstractSegmented` / `AbstractSegmentedItem` 跨平台基类与 internal 状态属性。

`SegmentedItem` 显式作为 `Items` / `ItemsSource` 成员直接提供时（explicit items），同样走容器生成或直接容器路径，
marker 由创建 / prepare 路径幂等补齐，descriptor 不因容器来源不同而变化。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间
节点的 Height、Min/Max、Padding、Margin 覆盖约束，应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级
失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让内置模板缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`item`、`icon`、`label`，字段值与本文表格一致；`SegmentedItem` 不持有 descriptor。
- N 个选项时 `semantic-item` 共 N 个，item 模板内 `semantic-icon`、`semantic-label` 各 N 个；空集合均为 0。
- 集合重置与增删后，既有容器的 marker 不增删；新增容器 marker 就位。
- explicit items 与 generated items（`ItemsSource`）两条路径 marker 行为一致。
- 图文 / 纯图标 / 纯文本选项：折叠节点的 marker 保留，可见性由数据驱动。
- owner-scoped Semantic Style（`SegmentedItemStyle` / `SegmentedIconStyle` / `SegmentedLabelStyle`）与
  `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- 三档 SizeType、`Shape=Round`、横竖方向与 `IsExpanding` 下，`item` / `icon` / `label` 布局基线一致，选中滑块
  与选项对齐。
- 选中滑块移动与选择变化不增删任何 marker。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
