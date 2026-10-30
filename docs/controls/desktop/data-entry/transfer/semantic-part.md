# Transfer Semantic Part 契约

本文档定义 `ListTransfer` 与 `TreeTransfer` 公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[Transfer 桌面版架构设计](overview.md)，真实模板、marker 映射与尺寸基线见
[Transfer 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Transfer 家族有两个具体 public owner：`ListTransfer` 与 `TreeTransfer`。两者各自持有独立 ControlTheme 与模板，
分区 Part 集合完全一致；条目 Part 仅 `ListTransfer` 发布 `itemIcon` / `itemContent` 及其限定变体（树侧条目区域
由 `TreeViewItem` 家族契约覆盖）：`root`、`source.section`、`target.section`、`actions`、`header`、`title`、`body`、
`list`、`footer`，以及方向限定变体 `source.header` / `target.header` / `source.title` / `target.title` /
`source.body` / `target.body` / `source.list` / `target.list` / `source.footer` / `target.footer`（§1.1–1.9，
命名与上游 `source.section` 等语义键逐字对齐，`.` 为层级分隔符）。抽象基类 `AbstractTransfer` 没有自己的 ControlTemplate，不声明任何 Part；internal
`TransferItemDecorator`、`TransferSelectDropdown`、`TransferTreeViewItemHeader` 不注册独立 descriptor，也不能通过
模板复用获得其他 owner 的 owner-scoped Semantic Style。

条目级 Part（`item` / `itemIcon` / `itemContent` 及其 `source.*` / `target.*` 限定变体）由 Transfer owner
直接发布（§1.10 / §1.11），使语义清单与样式用法都保持组件级单列表——嵌套视图与条目容器不再注册为独立
Semantic owner：`TransferListView` 继承 `ListView` 的 `root` / `item` / `groupHeader` 家族契约，
`TransferTreeView` 继承 `TreeView` 契约，条目容器的指示 / 内容区域由本节 Part 覆盖；`TransferTreeViewItem` 的条目区域由
`TreeViewItem` 家族契约覆盖，`TransferRemoveItemButton` 的内部区域由 `IconButton` 家族契约与组合模型约束。Transfer
owner 不穿透这些嵌套 owner 的模板（§5）。

`source.section` / `target.section` / `actions` 是 owner 模板中的静态节点，`Single`；方向限定部件
`source.*` / `target.*` 与对应未限定部件共享终端 marker，由路由中的方向锚点（`.semantic-source` /
`.semantic-target`）或条目自锚点（`.semantic-source-item` / `.semantic-target-item`）区分实例；`header` / `title` / `body` / `list` /
`footer` 位于 internal `TransferItemDecorator` 的共享模板内，由源面板和目标面板两个实例同时实例化，`Multiple`
（数量恒为 2）。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `root` |
| Selector | owner 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ListTransfer` / `TreeTransfer` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | Transfer owner |
| 职责 | 承载数据、选择状态、过滤、布局模式和 owner-scoped Semantic Style 入口。 |
| 相关 API | `ItemsSource`、`TargetKeys`、`SelectedKeys`、`IsOneWay`、`IsStretchView`、`ListWidth`、`ListHeight`、`IsFilterEnabled`、`PageSize`、`Status`、`SizeType` |
| 相关 Token | `ListTransferToken` / `TreeTransferToken`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是控件自身，不声明 `.semantic-root` marker。它适合定制 owner 级 `Foreground`、`Opacity` 与整体布局约束；
源/目标面板与中间操作区的结构由各 Part 负责，不通过 root Setter 改写。

### 1.2 `source.section`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `source.section` |
| Selector | `.semantic-source` |
| SelectorRoute | `/template/ .semantic-source` |
| Style Type | `ListTransferSourceSectionStyle` / `TreeTransferSourceSectionStyle` |
| ContractType | `TemplatedControl`（internal `TransferItemDecorator` 的最低 public 类型） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | internal `TransferItemDecorator#SourceDecoratorView`（源面板 section） |
| 职责 | 源方向列表分区的外框，承载 header、过滤输入、列表宿主和 footer 的组织边界。 |
| 相关 API | `SourceTitle`、`SourceTitleTemplate`、`SourceViewFooter`、`SourceViewFooterTemplate`、`ListWidth`、`ListHeight` |
| 相关 Token | `ListTransferToken` / `TreeTransferToken` 的 `HeaderHeight`、`HeaderPadding`、SharedToken 边框圆角 |
| 稳定性 | stable since 6.2.0 |

`source` 对应内部源面板装饰器实例。`BorderBrush`、`BorderThickness`、`CornerRadius` 经模板投影到分区外框
`Frame`；`Background` 同样投影到 `Frame`（默认为 null，即分区主体保持透明，header 保留自身背景 token）。面板宽度
由 owner `ListWidth` / `IsStretchView` 决定，`Width` 类 Setter 会与模板投影值按 Avalonia 原生优先级竞争。section
内部的 header / list / footer 区域由 §1.5–1.8 的共享 Part 表达，`source` 不展开其内部结构。

### 1.3 `target.section`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `target.section` |
| Selector | `.semantic-target` |
| SelectorRoute | `/template/ .semantic-target` |
| Style Type | `ListTransferTargetSectionStyle` / `TreeTransferTargetSectionStyle` |
| ContractType | `TemplatedControl`（internal `TransferItemDecorator` 的最低 public 类型） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | internal `TransferItemDecorator#TargetDecoratorView`（目标面板 section） |
| 职责 | 目标方向列表分区的外框，承载 header、过滤输入、列表宿主和 footer 的组织边界。 |
| 相关 API | `TargetTitle`、`TargetTitleTemplate`、`TargetViewFooter`、`TargetViewFooterTemplate`、`IsOneWay`、`ListWidth`、`ListHeight` |
| 相关 Token | 同 `source` |
| 稳定性 | stable since 6.2.0 |

`target` 与 `source` 结构、route 深度和可定制属性完全一致，仅方向不同；`IsOneWay=True` 时目标面板仍存在，只是
条目选择与回移入口按 API 语义禁用或隐藏。方向差异化定制（如目标面板换背景）是 `source` / `target` 分区级 Part 的
核心用途。

### 1.4 `actions`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `actions` |
| Selector | `.semantic-actions` |
| SelectorRoute | `/template/ .semantic-actions` |
| Style Type | `ListTransferActionsStyle` / `TreeTransferActionsStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 模板内 `StackPanel#ActionsLayout`（垂直布局容器） |
| 职责 | 组织"移至目标 / 移回源"两个操作按钮的中间操作区。 |
| 相关 API | `ToTargetTransferIcon`、`ToSourceTransferIcon`、`ToTargetButtonText`、`ToSourceButtonText`、`IsOneWay` |
| 相关 Token | `SpacingXXS`（按钮间距）、`SpacingXS`（与面板间距） |
| 稳定性 | stable since 6.2.0 |

`actions` 是操作区容器，适合定制 `Spacing`、`Margin`、`Opacity` 与对齐。`IsOneWay=True` 时"移回源"按钮仅切换
可见性，容器与 marker 不变。容器内的两个按钮是 public `Button` 实例，属于嵌套 Button 家族契约（§5），不通过
`actions` Setter 改写按钮内部视觉。

### 1.5 `header`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `/template/ .semantic-scope-section /template/ .semantic-header` |
| Style Type | `ListTransferHeaderStyle` / `TreeTransferHeaderStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Multiple`（源、目标各一，恒为 2） |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TransferItemDecorator` 模板内 `PixelAlignedBorder#HeaderFrame` |
| 职责 | 面板头部分区，承载全选指示、选择计数与标题的组织边界。 |
| 相关 API | `IsShowSelectAllCheckbox`、`IsShowSelectDropdownMenu`、`SelectionsIcon`、`SourceTitle` / `TargetTitle` |
| 相关 Token | `HeaderHeight`、`HeaderPadding`、`ColorSplit`、`ColorBgContainer` |
| 稳定性 | stable since 6.2.0 |

`header` 的高度由 `HeaderHeight` token 经模板投影（`ControlHeightLG` 派生的固定基线），`Height` 类 Setter 按原生
优先级参与测量，但需与分区外框和 owner 布局协调（见 implementation.md 尺寸基线）。header 内部的全选
`CheckBox`、internal 下拉指示按钮与选择计数 presenter 属于 header 组合结构，不单独发布（§5）。`.semantic-scope-section`
是路由边界标记（源、目标两个装饰器实例同时携带），不进入公开 Part 表。五个分区 Part（`header` / `title` / `body` /
`list` / `footer`）的标记节点都位于 `TransferItemDecorator` 自身的 ControlTheme 模板内，由装饰器实例化产生，
描述符因此声明 `RuntimeCreated=true`（生成器据此跳过 owner 模板的静态 marker 计数，与 Collapse、Splitter 先例一致）。

### 1.6 `title`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-scope-section /template/ .semantic-title` |
| Style Type | `ListTransferTitleStyle` / `TreeTransferTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple`（源、目标各一，恒为 2） |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TransferItemDecorator` 模板内 `ContentPresenter#TitleContentPresenter` |
| 职责 | 承载 `SourceTitle` / `TargetTitle` 及其模板的最终呈现。 |
| 相关 API | `SourceTitle`、`SourceTitleTemplate`、`TargetTitle`、`TargetTitleTemplate` |
| 相关 Token | `HeaderPadding`（右对齐留白） |
| 稳定性 | stable since 6.2.0 |

`title` presenter 始终存在于静态模板结构，`Title=null` 时仍保留节点身份；适合定制 `Foreground`、`Opacity`、
`Margin` 与对齐。`SourceTitleTemplate` / `TargetTitleTemplate` 创建的用户子树不属于 Transfer Semantic Part。

### 1.7 `body`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-scope-section /template/ .semantic-body` |
| Style Type | `ListTransferBodyStyle` / `TreeTransferBodyStyle` |
| ContractType | `DockPanel` |
| Cardinality | `Multiple`（源、目标各一，恒为 2） |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TransferItemDecorator` 模板内 `DockPanel#BodyLayout`（分区主体包裹节点） |
| 职责 | 分区主体区域，承载过滤输入与列表宿主的组织边界。 |
| 相关 API | `IsFilterEnabled`、`FilterPlaceholderText`、`ListHeight` |
| 相关 Token | `MarginXS`（过滤输入外距）、SharedToken |
| 稳定性 | stable since 6.2.0 |

`body` 是 header 与 footer 之间的分区主体包裹节点，与上游 `body` 语义键对齐：过滤输入与视图宿主是它的直接子
节点。`Background`、`Padding`、`Margin` 类 Setter 直接作用于主体区域，与 header 的背景 token 相互独立；过滤输入
（LineEdit 家族契约）与列表宿主（`list` Part）各有边界，`body` Setter 不展开其内部结构。`IsFilterEnabled` 只切换
过滤输入可见性，不改变 `body` 节点身份。

### 1.8 `list`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `list` |
| Selector | `.semantic-list` |
| SelectorRoute | `/template/ .semantic-scope-section /template/ .semantic-list` |
| Style Type | `ListTransferListStyle` / `TreeTransferListStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple`（源、目标各一，恒为 2） |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TransferItemDecorator` 模板内 `ContentPresenter#ContentPresenter`（视图宿主） |
| 职责 | 承载源/目标视图控件（`TransferListView` / `TransferTreeView`）的宿主分区。 |
| 相关 API | `ListHeight`、`SourceView` / `TargetView`、`PageSize`、`ItemTemplate` |
| 相关 Token | `ListHeight`（宿主高度基线）、`BorderRadiusLG`（底部圆角） |
| 稳定性 | stable since 6.2.0 |

`list` 是 Transfer owner 拥有的视图宿主，不是嵌套视图控件本身：宿主的 `Height` 由 `ListHeight` 模板投影，
`CornerRadius` 由 footer 存在性驱动（有 footer 时底部圆角归零）。条目、分组、分页等视图内部区域的定制属于嵌套
owner 契约（§5）。`Height` 类 Setter 与 owner `ListHeight` API 按原生优先级竞争，需按 implementation.md 尺寸基线
理解最终 Measure/Arrange 结果。

### 1.9 `footer`

| 字段 | 值 |
| --- | --- |
| Owner | `ListTransfer` / `TreeTransfer` |
| Part | `footer` |
| Selector | `.semantic-footer` |
| SelectorRoute | `/template/ .semantic-scope-section /template/ .semantic-footer` |
| Style Type | `ListTransferFooterStyle` / `TreeTransferFooterStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Multiple`（源、目标各一，恒为 2；未设置 footer 内容时隐藏） |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TransferItemDecorator` 模板内 `PixelAlignedBorder#FooterFrame` |
| 职责 | 面板底部分区，承载 `SourceViewFooter` / `TargetViewFooter` 及其模板的呈现边界。 |
| 相关 API | `SourceViewFooter`、`SourceViewFooterTemplate`、`TargetViewFooter`、`TargetViewFooterTemplate` |
| 相关 Token | `HeaderPadding`、`ColorSplit` |
| 稳定性 | stable since 6.2.0 |

`footer` 节点始终属于静态模板结构，可见性由对应方向 footer 内容驱动，未设置时隐藏但保留 marker 与对象身份；
设置 footer 内容时分区 body 的底部圆角归零，由 footer 闭合外框。footer presenter 内的用户内容子树不属于 Transfer
Semantic Part。

### 1.10 条目 Part（`item` / `itemIcon` / `itemContent`）

条目级 Part 由 Transfer owner 直接发布（不再以 `TransferListItem` / 嵌套视图为 owner），样式与语义清单保持
组件级单列表，与上游 Transfer 的 `item` / `itemIcon` / `itemContent` 语义键对齐：

| Part | Selector | SelectorRoute | Style Type | ContractType | Cardinality | AtomUI 节点 |
| --- | --- | --- | --- | --- | --- | --- |
| `item` | `.semantic-item` | `>> .semantic-item` | `ListTransferItemStyle` / `TreeTransferItemStyle` | `TransferListItem` / `TransferTreeViewItem` | `Multiple` | 视图容器（`TransferListItem` / `TransferTreeViewItem`） |
| `itemIcon` | `.semantic-item-icon` | `>> .semantic-item /template/ .semantic-item-icon` | `ListTransferItemIconStyle` | `CheckBox` | `Multiple` | `TransferListItem` 模板内 `CheckBox#SelectedIndicator` |
| `itemContent` | `.semantic-item-content` | `>> .semantic-item /template/ .semantic-item-content` | `ListTransferItemContentStyle` | `ContentPresenter` | `Multiple` | `TransferListItem` 模板内 `ContentPresenter#ContentPresenter` |

`itemIcon` / `itemContent` 仅 `ListTransfer` 发布（树侧条目模板无对应节点，指示 / 图标 / 标题区域由
`TreeViewItem` 家族的 `itemIndicator` / `itemIcon` / `itemTitle` 契约覆盖）。`>>` 是路由的后代步进
（从 owner 视觉子树任意深度命中），运行时以锚链匹配。容器由视图在创建 / prepare 路径建立 `.semantic-item`
与方向条目 marker（§1.11），prepare、restore、recycle 不切换任何 marker。`itemIcon` 是条目选择指示
CheckBox，`IsCheckable=false`（单向模式目标面板）时隐藏但保留节点身份，右缘的移除按钮与选择行为不属于
Part；`itemContent` 承载 `ItemTemplate` 内容，其创建的用户子树不属于 Semantic Part。

### 1.11 方向限定部件（`source.*` / `target.*`）

每个分区内部件与条目件都有方向限定变体，命名与上游语义键逐字对齐（`.` 为层级分隔符）。限定部件与对应未限定
部件**共享终端 marker**，由路由中的方向锚点区分实例，因此不新增任何模板 marker；`Since` 均为 `6.2.0`。

**owner 级分区限定部件**（`ListTransfer` / `TreeTransfer`，`Single`，`RuntimeCreated=true`）：

| Part | SelectorRoute | ContractType | AtomUI 节点 |
| --- | --- | --- | --- |
| `source.header` / `target.header` | `/template/ .semantic-source（或 target）/template/ .semantic-header` | `PixelAlignedBorder` | 装饰器模板内 `PixelAlignedBorder#HeaderFrame` |
| `source.title` / `target.title` | 同链至 `.semantic-title` | `ContentPresenter` | `ContentPresenter#TitleContentPresenter` |
| `source.body` / `target.body` | 同链至 `.semantic-body` | `DockPanel` | `DockPanel#BodyLayout` |
| `source.list` / `target.list` | 同链至 `.semantic-list` | `ContentPresenter` | `ContentPresenter#ContentPresenter`（视图宿主） |
| `source.footer` / `target.footer` | 同链至 `.semantic-footer` | `PixelAlignedBorder` | `PixelAlignedBorder#FooterFrame` |

**条目限定部件**（`Multiple`，`RuntimeCreated=true`，由 Transfer owner 直接发布）：
`source.item` / `target.item`（`>> .semantic-source-item` / `>> .semantic-target-item`）、
`source.itemIcon` / `target.itemIcon`（`>> .semantic-source-item /template/ .semantic-item-icon` 等，仅
`ListTransfer`）、`source.itemContent` / `target.itemContent`（同形，仅 `ListTransfer`）。方向条目类由视图在
prepare 路径按自身 `ViewType` 一次性补挂（路 A：容器与视图绑定后不迁移、`ViewType` 不可变，marker 保持静态
稳定），从而与未限定条目部件共享终端 marker。Style Type 依 Path 生成：
`ListTransferSourceHeaderStyle` / `TreeTransferTargetListStyle` / `ListTransferSourceItemIconStyle` 等，
全部直接挂在 Transfer 作用域下使用（如 `<atom:ListTransferSourceItemIconStyle>` 置于
`<Style Selector="atom|ListTransfer.my-demo">` 内即可只命中源面板条目图标）。

## 2. 模板变体与 route

`ListTransfer` 与 `TreeTransfer` 的内置模板结构一致：owner 模板提供源装饰器实例（`.semantic-source` +
`.semantic-scope-section`）、目标装饰器实例（`.semantic-target` + `.semantic-scope-section`）与操作区
（`.semantic-actions`）；分区内部节点位于 internal `TransferItemDecorator` 的共享 ControlTheme 模板中，两个实例
共用同一套 `.semantic-header` / `.semantic-title` / `.semantic-body` / `.semantic-list` / `.semantic-footer`
marker，因此每个未限定内层 Part 只有一条 `SelectorRoute`，经 `.semantic-scope-section` 一步进入装饰器模板，
同时命中源、目标两个实例；方向限定变体（§1.11）则把中间锚点替换为 `.semantic-source` / `.semantic-target`，
只命中单侧实例。运行时高亮与样式选择器都按锚链解析：终端 marker 之外，候选节点到 owner 的祖先链必须携带路由
中的全部中间锚点类。

| 变体 | 触发条件 | 视图控件 | route 链 |
| --- | --- | --- | --- |
| ListTransfer | 默认 | `TransferListView`（源、目标） | `.semantic-source` / `.semantic-target` → `.semantic-scope-section` → 装饰器模板内 Part |
| TreeTransfer | 默认 | `TransferTreeView`（源）+ `TransferListView`（目标） | 同链 |

`IsStretchView`、`IsOneWay`、`IsFilterEnabled`、`IsPaginationEnabled` 与分页开关只改变布局或可见性，不改变模板
结构、Part 集合或 marker。`.semantic-scope-section` 是路由边界，不进入公开 Part 表。

## 3. Selector 用法

生成的 Style Type 已封装 owner 类型保护和跨装饰器模板的 `SelectorRoute`，应用不直接复制 `/template/` 路径：

```xml
<Style Selector="atom|ListTransfer.semantic-style-demo">
    <atom:ListTransferSourceSectionStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Background" Value="#F6FFED" />
        <Setter Property="BorderBrush" Value="#B7EB8F" />
    </atom:ListTransferSourceSectionStyle>
    <atom:ListTransferTargetSectionStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Background" Value="#FFF7E6" />
    </atom:ListTransferTargetSectionStyle>
    <atom:ListTransferHeaderStyle x:SetterTargetType="Border">
        <Setter Property="Background" Value="#FCFFE6" />
    </atom:ListTransferHeaderStyle>
    <atom:ListTransferTitleStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#389E0D" />
    </atom:ListTransferTitleStyle>
    <atom:ListTransferBodyStyle x:SetterTargetType="DockPanel">
        <Setter Property="Background" Value="#F6FFED" />
    </atom:ListTransferBodyStyle>
    <atom:ListTransferActionsStyle x:SetterTargetType="StackPanel">
        <Setter Property="Opacity" Value="0.85" />
    </atom:ListTransferActionsStyle>
</Style>
```

条目级定制使用条目容器自己的 owner scope：

```xml
<Style Selector="atom|ListTransfer.semantic-style-demo atom|TransferListItem">
    <atom:TransferListItemItemIconStyle x:SetterTargetType="Control">
        <Setter Property="Opacity" Value="0.9" />
    </atom:TransferListItemItemIconStyle>
    <atom:TransferListItemItemContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#389E0D" />
    </atom:TransferListItemItemContentStyle>
</Style>
```

`TreeTransfer` 使用对应的 `TreeTransfer*Style` 类型，用法相同。不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `TemplatedControl.semantic-source`、`Border.semantic-header` 等 `ContractType` 写入 Part 身份 selector。
- 直接复制 `/template/ .semantic-scope-section /template/ .semantic-*` route；scope class 只用于生成 Style 的
  owner-relative 路由。
- 穿过 `SourceTitleTemplate`、footer 模板或 `ItemTemplate` 创建的用户内容继续匹配内部 Visual。
- 把 `TransferItemDecorator`、`TransferSelectDropdown` 或 `TransferListView` / `TransferTreeView` 当作 Transfer
  Part 的 descriptor owner；分区内部区域与视图条目的契约分属各自 owner。

## 4. 状态与数量语义

`source`、`target`、`actions` 为 `Single`；`header`、`title`、`body`、`list`、`footer` 为 `Multiple` 且实例数恒为
2。所有 Part 均为静态模板节点，状态变化只切换内容、可见性或有效视觉值，不增删 marker，也不改变对象身份。

| 场景 | root | source | target | actions | header | title | body | list | footer | 说明 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 默认（无 footer） | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2（隐藏） | 静态节点均已实例化。 |
| 设置单侧 footer | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2（一侧可见） | footer 只改变可见性与圆角。 |
| `IsOneWay=True` | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2 | 回移按钮隐藏，操作区不变。 |
| `IsFilterEnabled=True` | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2 | 过滤输入可见性切换，`body` 不变。 |
| `IsStretchView=True` | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2 | 仅列布局 Star/Auto 切换。 |
| `PageSize>0`（分页） | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2 | 分页器属于嵌套视图内部。 |
| 空数据源 | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2 | 宿主仍存在；嵌套视图条目 marker 为 0。 |
| `Status=Error/Warning` | 1 | 1 | 1 | 1 | 2 | 2 | 2 | 2 | 2 | 分区边框状态色由装饰器投影。 |

条目级 Part（`TransferListItem` 的 `itemIcon` / `itemContent`）数量随实例化条目数变化：N 个条目各 1 个；空数据源
为 0；容器回收后随复用重建，marker 保持不变。

## 5. 定制边界

以下区域明确不属于 Transfer Semantic Part：

- **嵌套视图 owner 的内部区域**：`TransferListView`（继承 `ListView` 的 `root` / `item` / `groupHeader`）、
  `TransferTreeView`（继承 `TreeView` 的 `root` / `item`）的分组、分页与滚动区域；`TransferTreeViewItem` 的条目
  区域（由 `TreeViewItem` 家族契约覆盖）。条目级的 `itemIcon` / `itemContent` 由 `TransferListItem` 自身发布
  （§1.10），上游 `item` 键由 `ListView.item` 契约覆盖，Transfer 实现必须保证嵌套视图容器创建路径正确建立继承
  marker（见 implementation.md），但条目级定制使用嵌套 owner 的 Semantic Style，不由 Transfer Part 承担。
- **操作按钮**：操作区内的"移至目标 / 移回源"两个 public `Button` 实例，属于 Button 家族契约。
- **过滤输入**：`body` 内的过滤 `LineEdit` 是 public 嵌套控件，属于 LineEdit 家族契约；`body` Setter 不展开其
  内部结构。
- **header 组合结构**：全选 `CheckBox`、internal 下拉指示按钮（含其运行时 `MenuFlyout` Popup 与菜单项）和选择计数
  presenter；上游 Ant Design Transfer 也未把这些节点公开为分区级 Semantic DOM。
- **条目操作**：`TransferListItem` 右缘的移除按钮（`TransferRemoveItemButton`，`IconButton` 家族契约；上游语义键
  中也无对应分区）。
- **方向内层区域**：装饰器模板由源、目标实例共享，静态 marker 无法按方向区分，`source.header` / `target.list` 一类
  方向作用域的内层定制不发布；方向差异化由 `source` / `target` 分区级 Part 承担。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。
Semantic Style 服从 Avalonia 原生属性优先级，布局结果仍受 owner `ListWidth` / `ListHeight`、装饰器模板投影值与
分区裁剪约束（见 implementation.md 尺寸基线）。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或者让任一内置模板变体缺少
marker，均属于公共主题契约变更。

验证至少覆盖：

- `ListTransfer` 与 `TreeTransfer` descriptor 均只包含 `root`、`source`、`target`、`actions`、`header`、`title`、
  `body`、`list`、`footer`，`TransferListItem` descriptor 只包含 `root`、`itemIcon`、`itemContent`，字段值与本文
  一致；`AbstractTransfer` 不注册 descriptor。
- 两个 owner 模板各自包含 `semantic-source`、`semantic-target`、`semantic-actions` 与 `semantic-scope-section`
  marker；装饰器模板包含 `semantic-header`、`semantic-title`、`semantic-body`、`semantic-list`、
  `semantic-footer` marker；`TransferListItem` 模板包含 `semantic-item-icon`、`semantic-item-content` marker；
  root 不声明 `.semantic-root`。
- 生成的 `ListTransfer*Style` / `TreeTransfer*Style` / `TransferListItem*Style` 可以编译并跨装饰器模板边界命中
  最低 public `ContractType`；两个 owner 的 Part 集合与命中行为一致。
- `IsOneWay`、`IsStretchView`、`IsFilterEnabled`、footer 存在性、空数据与 `Status` 变化时保持对象身份和数量语义。
- 嵌套 `TransferListView` / `TransferTreeView` 容器在创建与回收路径稳定携带继承 `semantic-item` marker，回收后
  不泄漏旧 class；`TransferListItem` 的条目 Part 随容器实例化与回收保持 marker。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts Tab 延迟创建 Preview，`ListTransfer` 与 `TreeTransfer` 均有可解析的 Part 演示，每个
  Part 可高亮解析到全部 1/2/N 个目标。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
