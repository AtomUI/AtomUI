# TreeView Semantic Part 契约

本文档定义 `TreeView` 家族对应用公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。TreeView 的整体设计见
[TreeView 桌面版架构设计](overview.md)，descriptor 与真实模板节点映射见 [TreeView 桌面版实现原理](implementation.md)，
系统级规则见 [AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Owner 边界

TreeView 是递归层级容器：每个 `TreeViewItem` 既是 `TreeView` 的 item 容器，又是下一层子节点的生成器。这与
`ListView` / `ListBox` 的单层容器不同——后者的容器逻辑父级恒为 owner 本身，`> .semantic-item` 一步即可到达所有
容器；TreeView 中只有顶层容器是 `TreeView` 的直接逻辑子级，嵌套容器是父 `TreeViewItem` 的直接逻辑子级。

因此 TreeView 家族使用**两个递归 Semantic owner**，每个 owner 只声明自己稳定可达的区域：

| Owner | 公开 Part | 说明 |
| --- | --- | --- |
| `TreeView` | `root`、`item` | 树根 + 顶层节点容器。 |
| `TreeViewItem` | `root`、`item`、`itemSwitcher`、`itemIndicator`、`itemIcon`、`itemTitle` | 节点容器 + 下一层子节点容器 + 每个节点的 switcher / indicator / icon / title 内容区域。 |

两个 owner 的 `item` Part 都指向 `TreeViewItem` 容器、都使用 `.semantic-item` marker 与 `> .semantic-item` route：
`TreeView.item` 覆盖顶层容器，`TreeViewItem.item` 递归覆盖下一层子容器，二者共同保证任意深度的节点都命中同一个
`.semantic-item` 身份。`itemSwitcher` / `itemIndicator` / `itemIcon` / `itemTitle` 声明在 `TreeViewItem` 上，其 route
从 `TreeViewItem` owner 出发经 `.semantic-scope-header` 跳点进入 header 模板，因此对**每个**节点（无论层级）都可达。

上游基线为 6.6.0 稳定发布的 `TreeSemanticType`（`classNames` / `styles` 均为
`{ root?, item?, itemIcon?, itemTitle?, itemSwitcher? }`）。该类型在 `Tree.tsx` 中经 `useMergeSemantic` 合并
`ConfigProvider` 与组件级语义值后传入 `RcTree`，再由 `@rc-component/tree@1.4.0` 的 `TreeNode` 实际消费：

- `root` 消费于 Tree 根节点（`.ant-tree`，`rootClassName` / `rootStyle`）。
- `item` 消费于每个 `<div role="treeitem">` treenode 节点。
- `itemSwitcher` 消费于每个节点 switcher `<span>`（叶子 noop switcher 与展开/收起 switcher 两个形态共用）。
- `itemIcon` 消费于每个节点 icon `<span class="ant-tree-iconEle">`。
- `itemTitle` 消费于每个节点标题 `<span class="ant-tree-title">`。

上游 `TreeSemanticType` 只有上述五个键，不包含 checkbox / radio 勾选指示、indent、drag 指示线等节点。AtomUI 在
上游五个键之外，把节点勾选指示公开为额外的 `itemIndicator` Part（AtomUI 扩展，非上游 Semantic DOM 键）：它是
`ToggleType=CheckBox` 时的 `CheckBox#ToggleCheckbox` 与 `ToggleType=Radio` 时的 `RadioButton#ToggleRadio` 两个备选
节点共用的单一 Part，`ContractType` 收缩到公开基类 `ToggleButton`。AtomUI 用两个 owner 表达同一份语义：上游单一
`Tree` owner 的 `root`/`item` 职责落在 `TreeView`，递归 treenode 的 `item`/`itemSwitcher`/`itemIndicator`/`itemIcon`/
`itemTitle` 职责落在 `TreeViewItem`。所有 Part 的 `Since` 统一为 `6.2.0`。

以下类型不持有独立 Semantic descriptor：

- `TreeViewItemHeader` 与 `NodeSwitcherButton` 是 internal 协作类型，不是对应用公开的独立 owner。
  `TreeViewItemHeader` 只承载 `.semantic-scope-header` 路由跳点，不发布 Part。
- `FloatableTreeView` 是 TreeView 的公开浮层变体，没有独立 ControlTheme 与上游 Semantic DOM owner；它通过
  `TreeView` 的 descriptor 与 `atom|TreeView` owner scope 复用同一份契约，不单独声明 descriptor。

## 2. Semantic Parts

### 2.1 `TreeView`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeView` |
| Part | `root` |
| Selector | TreeView 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `TreeView` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | TreeView owner |
| 职责 | 树根是 Items、选择、勾选、展开、过滤、拖拽、异步加载、空状态、switcher 图标与动效配置的统一 owner；作为顶层 `item` Part 的 owner-scoped Selector 作用域边界。 |
| 相关 API | `Items`、`ItemsSource`、`SelectionMode`、`SelectedItem`、`SelectedItems`、`ToggleType`、`IsCheckStrictly`、`IsDefaultExpandAll`、`DefaultSelectedPaths`、`DefaultCheckedPaths`、`DefaultExpandedPaths`、`IsDraggable`、`IsShowIcon`、`IsShowLine`、`IsShowLeafIcon`、`NodeHoverMode`、`Switcher*Icon`、`IsSwitcherRotation`、`IsSelectable`、`IsSelectOnRightClick`、`DataLoader`、`Filter`、`FilterStrategy`、`EmptyIndicator`、`IsMotionEnabled`、`OpenMotion`、`CloseMotion` |
| 相关 Token | `TreeViewToken`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeView` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| Style Type | `TreeViewItemStyle` |
| ContractType | `TreeViewItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个**顶层** `TreeViewItem` 容器 |
| 职责 | 统一表示树中直接挂在 `TreeView` 根下的节点容器，覆盖 `TreeViewItem` 容器为公开 item 容器的顶层形态。 |
| 相关 API | `Header`、`HeaderTemplate`、`Icon`、`IsChecked`、`IsLeaf`、`IsLoading`、`IsSelected`、`IsExpanded`、`IsEnabled`、`IsDragging`、`IsDragOver`、`NodeHoverMode`、`IsShowLine` |
| 相关 Token | `TreeItemMargin`、`HeaderHeight`、SharedToken（`ColorBorder`） |
| 稳定性 | stable since 6.2.0 |

### 2.2 `TreeViewItem`

`TreeViewItem` 是递归 owner：其 `item` Part 覆盖下一层子节点容器，`itemSwitcher` / `itemIcon` / `itemTitle` 覆盖每个
节点的内容区域。`TreeViewItem` 的 `root` 是节点容器本身（implicit Part，无 `.semantic-root` marker，不生成 Style）。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeViewItem` |
| Part | `root` |
| Selector | TreeViewItem 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用 |
| ContractType | `TreeViewItem` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 每个 `TreeViewItem` 容器 |
| 职责 | 单个树节点容器：承载节点级状态（selected / checked / expanded / disabled / loading / drag / filter）与树形连线渲染表面；作为子节点容器与节点内容 Part 的 owner-scoped Selector 作用域边界。 |
| 相关 API | `Header`、`HeaderTemplate`、`Icon`、`IsChecked`、`IsLeaf`、`IsLoading`、`IsSelected`、`IsExpanded`、`IsEnabled`、`IsDragging`、`IsDragOver`、`NodeHoverMode`、`IsShowLine` |
| 相关 Token | `TreeItemMargin`、`HeaderHeight`、SharedToken（`ColorBorder`） |
| 稳定性 | stable since 6.2.0 |

#### `item`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeViewItem` |
| Part | `item` |
| Selector | `.semantic-item` |
| SelectorRoute | `> .semantic-item` |
| Style Type | `TreeViewItemItemStyle` |
| ContractType | `TreeViewItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个**子级** `TreeViewItem` 容器 |
| 职责 | 递归表示当前节点容器生成的下一层子节点容器。与 `TreeView.item` 使用同一 `.semantic-item` 身份，共同保证任意深度的节点都可被 `atom|TreeViewItem` / `atom|TreeView` owner scope 命中。 |
| 相关 API | 同 `TreeViewItem` root |
| 相关 Token | `TreeItemMargin`、`HeaderHeight` |
| 稳定性 | stable since 6.2.0 |

#### `itemSwitcher`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeViewItem` |
| Part | `itemSwitcher` |
| Selector | `.semantic-item-switcher` |
| SelectorRoute | `/template/ .semantic-scope-header /template/ .semantic-item-switcher` |
| Style Type | `TreeViewItemItemSwitcherStyle` |
| ContractType | `ToggleButton` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 header 模板中的 `NodeSwitcherButton#PART_NodeSwitcherButton` |
| 职责 | 统一表示节点展开/收起 switcher 区域：展开、收起、叶子与加载图标入口；对应上游 `.ant-tree-switcher` 节点。 |
| 相关 API | `SwitcherExpandIcon`、`SwitcherCollapseIcon`、`SwitcherRotationIcon`、`SwitcherLoadingIcon`、`SwitcherLeafIcon`、`IsSwitcherRotation`、`IsLeaf`、`IsLoading`、`IsExpanded` |
| 相关 Token | `HeaderHeight`、`NodeHoverBg`、`TreeNodeSwitcherMargin`、SharedToken（`IconSize`、`IconSizeXS`、`ColorTextSecondary`） |
| 稳定性 | stable since 6.2.0 |

#### `itemIndicator`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeViewItem` |
| Part | `itemIndicator` |
| Selector | `.semantic-item-indicator` |
| SelectorRoute | `/template/ .semantic-scope-header /template/ .semantic-item-indicator` |
| Style Type | `TreeViewItemItemIndicatorStyle` |
| ContractType | `ToggleButton` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 header 模板中的 `CheckBox#ToggleCheckbox`（`ToggleType=CheckBox`）与 `RadioButton#ToggleRadio`（`ToggleType=Radio`） |
| 职责 | 统一表示节点勾选指示区域：checkbox / radio 两个备选形态共用的单一 Part，承载勾选状态、radio 分组与禁用态；对应 AtomUI 的 `ToggleType` 勾选功能节点，非上游 Semantic DOM 键（AtomUI 扩展）。 |
| 相关 API | `ToggleType`、`IsChecked`、`IsIndicatorEnabled`、`GroupName`、`IsEnabled` |
| 相关 Token | SharedToken（`ColorBorder`、`ColorPrimary`、`ColorBorderSecondary`） |
| 稳定性 | stable since 6.2.0 |

#### `itemIcon`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeViewItem` |
| Part | `itemIcon` |
| Selector | `.semantic-item-icon` |
| SelectorRoute | `/template/ .semantic-scope-header /template/ .semantic-item-icon` |
| Style Type | `TreeViewItemItemIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 header 模板中的 `IconPresenter#PART_IconPresenter` |
| 职责 | 统一表示节点图标区域：`Icon` 内容的呈现、尺寸与边距；对应上游 `.ant-tree-iconEle` 节点。 |
| 相关 API | `Icon`、`IsShowIcon`、`IsShowLeafIcon` |
| 相关 Token | `TreeNodeIconMargin`、SharedToken（`IconSize`） |
| 稳定性 | stable since 6.2.0 |

#### `itemTitle`

| 字段 | 值 |
| --- | --- |
| Owner | `TreeViewItem` |
| Part | `itemTitle` |
| Selector | `.semantic-item-title` |
| SelectorRoute | `/template/ .semantic-scope-header /template/ .semantic-item-title` |
| Style Type | `TreeViewItemItemTitleStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 每个 header 模板中的 `ContentPresenter#HeaderPresenter` |
| 职责 | 统一表示节点标题文字区域：`Header` / `HeaderTemplate` 内容的呈现、颜色、字体与对齐；对应上游 `.ant-tree-title` 节点。 |
| 相关 API | `Header`、`HeaderTemplate`、`Content`、`ContentTemplate` |
| 相关 Token | SharedToken（`ColorText`、`ColorTextDisabled`） |
| 稳定性 | stable since 6.2.0 |

### 2.3 marker 放置与路由

`root` 是隐式 Part，不声明 `.semantic-root` marker。非 root Part 的 marker 放置：

- `TreeView.item` 与 `TreeViewItem.item` 的 marker `.semantic-item` 在 `TreeView` 与 `TreeViewItem` 的容器创建与
  prepare 路径幂等添加，覆盖用户显式 `TreeViewItem`、`ItemsSource` 数据驱动容器与递归子节点容器三条来源。
- `itemSwitcher`、`itemIndicator`、`itemIcon`、`itemTitle` 的 marker 静态声明在 `TreeViewItemHeaderTheme.axaml`（header
  模板）。`itemIndicator` 是 checkbox / radio 两个备选节点共用：两个节点都携带 `.semantic-item-indicator` marker，
  每个容器恒有两个 marker 实例，`ToggleType` 决定同一时刻最多一个可见。
- `.semantic-scope-header` 静态声明在 `TreeViewItemTheme.axaml` 的 `TreeViewItemHeader#Header` 上，是 `TreeViewItem`
  模板到 `TreeViewItemHeader` 模板之间的路由跳点，不发布为 Part。

`TreeViewItem` owner 的五个运行时 Part（`item`、`itemSwitcher`、`itemIndicator`、`itemIcon`、`itemTitle`）都位于
`TreeViewItem` 容器及其模板内部，descriptor 统一声明 `RuntimeCreated=true`，生成器不按 owner 主题资产做静态校验。

- `item` 的 route `> .semantic-item` 经一步 `>` 直达子容器：子 `TreeViewItem` 容器的逻辑父级是当前 `TreeViewItem`
  owner 本身。
- `itemSwitcher` / `itemIndicator` / `itemIcon` / `itemTitle` 需要两次 `/template/` 跳点：先进入 `TreeViewItem` 模板
  命中 `TreeViewItemHeader#Header` 上的 `.semantic-scope-header` 跳点，再进入 `TreeViewItemHeader` 模板命中对应
  marker。

`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过 `x:SetterTargetType` 提供 AXAML 编译期类型
上下文；它不参与 `.semantic-*` 的身份匹配。`itemSwitcher` 的真实节点 `NodeSwitcherButton` 与 `itemIndicator` 的真实
节点 `CheckBox#ToggleCheckbox` / `RadioButton#ToggleRadio` 都是 internal 或继承自公开基类的节点，因此二者 ContractType
都使用其公开基类 `ToggleButton`（checkbox / radio 的 `AbstractCheckBox` / `AbstractRadioButton` 都继承自
`ToggleButton`；与 Timeline 对 internal `TimelineIndicator` 节点使用公开 `Border`、Calendar 对 internal cell 使用
`TemplatedControl` 同一决策）。`itemIcon` 的节点 `IconPresenter` 与 `itemTitle` 的节点 `ContentPresenter` 均为公开
类型，直接取节点真实 public 类型作为最低依赖类型。

## 3. Part 说明

### 3.1 root

`TreeView.root` 是 TreeView owner 本身，在 TreeView 实例的整个生命周期内始终存在，并且每个 TreeView 恰好一个。

它负责：

- 承载 `Items` / `ItemsSource`、选择、勾选、展开、过滤、拖拽、异步加载、空状态、switcher 图标、hover mode 与动效
  等公共状态与 API，并作为全部顶层节点状态机的统一 owner。
- 承载 `:empty`、`:draggable` 等伪类。
- 作为顶层 `item` owner-scoped Selector 的作用域边界。

默认 `TreeViewTheme` 的 `Border#Frame` / `ScrollViewer` 不把 owner 的 `Background`、`BorderBrush`、
`BorderThickness`、`CornerRadius`、`Padding` 绑定到任何根表面节点：TreeView 默认是裸滚动容器，不承诺根盒面投影。
root 因此不承诺根背景、边框、圆角或内边距表面；这些视觉属于应用通过 owner 属性、owner-scoped Style 或替换
ControlTheme 自行提供的区域，上游 `.ant-tree` 的字号/颜色等根级排版也由 owner 属性与样式表达，具体节点投影不进入
公共契约。适合通过 root 定制整体字体、前景色与对齐；需要按状态改变根样式时，在 owner Selector 上组合公开属性或
伪类（例如 `atom|TreeView:empty`、`atom|TreeView[draggable]`）。

root 不表示模板中的 `Border#Frame`、`ScrollViewer`、`ItemsPresenter`、`EmptyIndicator` 或
`DefaultEmptyIndicator` 节点；这些节点的名称、数量与层级不属于 root 契约。

`TreeViewItem.root` 是每个节点容器本身。它承载节点级状态与树形连线渲染表面，并作为子节点容器与节点内容 Part 的
owner 作用域边界。适合通过 `TreeViewItem` owner 作用域定制节点级 `Padding`、`Margin`、`MinHeight`、`Foreground`、
`FontSize`、`Cursor` 等；`BorderBrush` / `BorderThickness` 同时驱动 `IsShowLine=true` 时的树形连线颜色与宽度。

### 3.2 item

`item` 表示树中的一个节点容器，每个 `TreeViewItem` 恰好一个 marker，cardinality 为 `Multiple`。marker
`.semantic-item` 在容器创建与 prepare 路径幂等添加；虚拟化回收、展开/收起、状态切换与集合变化不增删 marker。

`item` 在 `TreeView` 与 `TreeViewItem` 两个 owner 上各自声明一次，覆盖两条生成路径：

- `TreeView.item` 覆盖 `CreateContainerForItemOverride` 为顶层节点生成的容器（逻辑父级是 `TreeView`）。
- `TreeViewItem.item` 覆盖 `CreateContainerForItemOverride` 为子节点生成的容器（逻辑父级是 `TreeViewItem`）。

二者使用同一 `.semantic-item` 身份，保证任意深度的节点容器都可通过 `.semantic-item` 命中。覆盖应验证：

- 状态变化（选择、勾选、展开、禁用、拖拽、过滤、hover mode、show-line/show-icon）只改变有效视觉属性与伪类，不增删
  marker。
- 层级展开/收起与虚拟化回收：子容器展开时创建、收起后回收；marker 随容器实例存在，不随数据项迁移。
- 空集合时顶层 `item` 为 0。

### 3.3 itemSwitcher / itemIndicator / itemIcon / itemTitle

四个内容 Part 各对应 header 模板内一个常驻节点，cardinality 均为 `Multiple`：

- `itemSwitcher` 承载 `NodeSwitcherButton#PART_NodeSwitcherButton`：展开、收起、叶子与加载图标入口。节点恒存在于
  header 模板稳定结构（Grid.Column 0），leaf、loading、`IsSwitcherRotation` 切换只改变当前图标，不增删节点。图标
  尺寸来自 SharedToken `IconSize` / `IconSizeXS`，hover 背景来自 `NodeHoverBg`。
- `itemIndicator` 承载 `CheckBox#ToggleCheckbox` / `RadioButton#ToggleRadio`：节点勾选指示。两个备选节点都常驻于
  header 模板（Grid.Column 1），`ToggleType` 决定可见形态（CheckBox / Radio / 两者隐藏），勾选状态与 `IsIndicatorEnabled`
  只改变有效视觉属性，不增删节点或 marker。每个容器恒有两个 marker 实例，同一时刻最多一个可见。
- `itemIcon` 承载 `IconPresenter#PART_IconPresenter`：`Icon` 内容的呈现。`IconEffectiveVisible`（`IsShowIcon && Icon
  is not null`）只切换可见性，节点与 marker 恒存在。
- `itemTitle` 承载 `ContentPresenter#HeaderPresenter`：`Header` / `HeaderTemplate` 内容的呈现。节点恒存在。

适合定制 `itemSwitcher` 的 `Background`、`CornerRadius`、`Padding`、`Cursor`，`itemIndicator` 的 `Margin`、`Opacity`
与对齐，`itemIcon` 的 `Width` / `Height` / `Margin` / `Opacity`，`itemTitle` 的 `Foreground`、`FontSize`、`FontWeight`、
`Margin` 与对齐。节点可见性全部由数据与状态驱动，Semantic Style 覆盖可见性会绕过数据状态机，属于不推荐用法。

### 3.4 itemTitle 与过滤高亮的关系

过滤命中且 `FilterStrategy` 要求高亮时，header 模板用内部 `TextBlock#FilterHighlighter`（带 `FilterHighlightRuns`
的 inline runs）替换 `ContentPresenter#HeaderPresenter` 呈现标题：`IsFilterMatch=true` 时 `HeaderPresenter` 隐藏、
`FilterHighlighter` 显示，`IsFilterMatch=false` 时反之。`itemTitle` 只覆盖 `HeaderPresenter` 的常规标题呈现；
`FilterHighlighter` 是过滤功能节点，其命中片段前景由 `FilterHighlightForeground`（`FilterHighlightColor`）在 Run
级别设置，不属于 `itemTitle` 契约。过滤高亮模式下 itemTitle 的 Semantic Setter 不作用于高亮呈现，这是过滤功能对
标题的临时替换，不是 marker 缺失。

## 4. Selector 用法

应用级样式先限定 owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护与
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <!-- 顶层节点容器：TreeView owner scope -->
    <Style Selector="atom|TreeView">
        <atom:TreeViewItemStyle x:SetterTargetType="atom:TreeViewItem">
            <Setter Property="Margin" Value="0,0,0,2" />
        </atom:TreeViewItemStyle>
    </Style>

    <!-- 任意层级节点内容与子容器：TreeViewItem owner scope -->
    <Style Selector="atom|TreeViewItem">
        <atom:TreeViewItemItemStyle x:SetterTargetType="atom:TreeViewItem">
            <Setter Property="Margin" Value="0,0,0,2" />
        </atom:TreeViewItemItemStyle>
        <atom:TreeViewItemItemSwitcherStyle x:SetterTargetType="ToggleButton">
            <Setter Property="Background" Value="#E6F4FF" />
            <Setter Property="CornerRadius" Value="4" />
        </atom:TreeViewItemItemSwitcherStyle>
        <atom:TreeViewItemItemIndicatorStyle x:SetterTargetType="ToggleButton">
            <Setter Property="Margin" Value="0,0,8,0" />
        </atom:TreeViewItemItemIndicatorStyle>
        <atom:TreeViewItemItemIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="Width" Value="16" />
            <Setter Property="Height" Value="16" />
        </atom:TreeViewItemItemIconStyle>
        <atom:TreeViewItemItemTitleStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#1677FF" />
        </atom:TreeViewItemItemTitleStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|TreeViewItem.semantic-custom[NodeHoverMode=Block]">
    <atom:TreeViewItemItemTitleStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="FontWeight" Value="SemiBold" />
    </atom:TreeViewItemItemTitleStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|TreeViewItem.semantic-item` 或 `:is(atom|TreeViewItem).semantic-item`。
- `ToggleButton.semantic-item-switcher`、`ToggleButton.semantic-item-indicator` 或 `:is(ToggleButton)` 变体。
- `IconPresenter.semantic-item-icon` 或 `ContentPresenter.semantic-item-title`。
- 直接复制 `/template/ .semantic-scope-header /template/ .semantic-item-*` route 作为用户主路径；
  route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型（`NodeSwitcherButton`、`TreeViewItemHeader`）、Name 或视觉祖先顺序。

## 5. 状态与数量语义

数量契约以已实例化的 AtomUI 内置容器为边界。`item` 及其余三个 item 级 Part 都是 `RuntimeCreated` Part：marker 随
容器实例存在，不随数据项迁移。`itemSwitcher` / `itemIcon` / `itemTitle` 每个容器各一个 marker。

| 场景 | TreeView root | TreeView item | TreeViewItem root | TreeViewItem item | switcher / indicator / icon / title | 说明 |
| --- | --- | --- | --- | --- | --- | --- |
| 默认 TreeView N 个可见节点 | 1 | 顶层容器数 | 每容器 1 | 每容器子节点数 | switcher / icon / title 每容器各 1，indicator 每容器 2 | 任意深度的节点内容均可达。 |
| 显式 `TreeViewItem` 声明 | 1 | 顶层容器数 | 每容器 1 | 子容器数 | 同上 | 与生成容器语义一致。 |
| `ItemsSource` 数据项 | 1 | 顶层容器数 | 每容器 1 | 子容器数 | 同上 | 自动生成 `TreeViewItem` 容器，marker 随容器就位。 |
| 层级展开 / 收起 | 1 | 随顶层容器数 | 每容器 1 | 随已实现子容器数 | 随容器数 | 子容器展开时创建、收起后按虚拟化回收；marker 随容器。 |
| 空集合 | 1 | 0 | 0 | 0 | 0 | 显示空状态，不创建节点容器。 |
| 叶子节点 | 1 | 顶层容器数 | 每容器 1 | 0 | switcher / indicator / icon / title 每容器恒在 | 叶子 switcher 仍渲染 leaf/noop 形态，节点与 marker 不变。 |
| `Icon` 为 null 或 `IsShowIcon=False` | 1 | 顶层容器数 | 每容器 1 | 子容器数 | `itemIcon` 节点隐藏但存在。 |
| `ToggleType=None` | 1 | 顶层容器数 | 每容器 1 | 子容器数 | `itemIndicator` 两个备选节点都隐藏但 marker 仍在。 |
| 过滤命中 / 高亮 | 1 | 顶层容器数 | 每容器 1 | 子容器数 | 只切换 `HeaderPresenter` / `FilterHighlighter` 可见性，itemTitle marker 仍在。 |
| 勾选 / 禁用 / 拖拽状态 | 1 | 顶层容器数 | 每容器 1 | 子容器数 | 状态切换只改变有效视觉属性与伪类，不增删 marker。 |
| 节点增删 / 集合重置 | 1 | 随顶层容器数 | 随容器数 | 随子容器数 | 随容器数 | 新容器创建时建 marker；容器释放时 marker 消失。 |

## 6. 尺寸基线

TreeView 没有 `SizeType` 分档，视觉基线由 `TreeViewToken` 与全局 token 常量表达：

- 节点 header 最小高度 `HeaderHeight`（`ControlHeightSM`），同时作为 `NodeSwitcherButton` 的宽高默认值。
- 节点行垂直节奏 `TreeItemMargin`，header 内容内边距 `TreeItemHeaderPadding` / 外边距 `TreeItemHeaderMargin`。
- switcher 与 checkbox / radio 间距 `TreeNodeSwitcherMargin`，icon 与内容间距 `TreeNodeIconMargin`。
- 缩进层级由 `TreeViewItemTheme.MarginMultiplierConverter` 控制（每层 16 DIP），不属于 Token。
- 节点 icon 尺寸来自 SharedToken `IconSize`，switcher 旋转图标使用 `IconSizeXS`。
- 树形连线颜色来自 SharedToken `ColorBorder`（`TreeViewItem` 的 `BorderBrush`），宽度来自 `BorderThickness`。

Semantic Style 覆盖 `item` 的 `Padding` / `Margin` / `MinHeight`、`itemSwitcher` 的尺寸、`itemIndicator` 的
`Margin` / `Opacity`、`itemIcon` 的 `Width` / `Height`、`itemTitle` 的 `FontSize` / `LineHeight` 时，应验证：节点
header 最小高度基线不被固定高度破坏，switcher 命中区仍可点，indicator 与 switcher / icon 不重叠，icon 与标题内容
不重叠，且 `IsShowLine=true` 时树形连线仍沿 switcher 中心连续。布局型固定 `Height` / `Width` / Min/Max Setter 不作为
公共定制路径：节点高度由 `HeaderHeight` + header 内容自然测量驱动，固定高度会绕过内容测量。

## 7. 定制边界

以下区域明确不属于 TreeView Semantic Part：

- 过滤高亮节点（`TextBlock#FilterHighlighter`）、`FilterHighlightForeground` 与 `FilterStrategy` 高亮呈现。
- header 内容框 `Border#PART_HeaderContentFrame`、`NodeHoverMode` 的 hover / selected 背景绘制层与
  `TreeViewItem.Render` 的树形连线 / 整行背景自绘逻辑。
- 子节点展开动效 actor（`ContentViewport#PART_ChildrenViewport`）与 `ItemsPresenter`。
- 空状态（`EmptyIndicator`、`DefaultEmptyIndicator`）、拖拽 preview / drop indicator、缩进层级计算。
- 用户 `HeaderTemplate` / `ItemTemplate` 生成的子树、`Icon` 具体内容与 switcher 具体图标内容。
- `TreeViewItemHeader`（internal，`.semantic-scope-header` 只作为 route 跳点）、`NodeSwitcherButton`（internal，
  通过 `itemSwitcher` 的 `ToggleButton` ContractType 间接可达）、`CheckBoxIndicator`（internal，`itemIndicator` 通过
  `ToggleButton` ContractType 间接可达）、`TreeDataController`、`DefaultTreeViewInteractionHandler` 等内部协作类型。
- `PART_*` 名称、internal 类型、Name 与模板层级。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间
节点的 Height、Min/Max、Padding、Margin、缩进或自绘逻辑约束，应按跨节点布局约束排查，不能把它解释为 Semantic
Style 优先级失效。

## 8. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- `TreeView` descriptor 只有 `root`、`item`；`TreeViewItem` descriptor 只有 `root`、`item`、`itemSwitcher`、
  `itemIndicator`、`itemIcon`、`itemTitle`，字段值与本文表格一致（非 root Part 均为 `Multiple`、`RuntimeCreated=true`
  且携带显式 SelectorRoute；`item` 的 `ContractType` 为 `TreeViewItem`，`itemSwitcher` / `itemIndicator` 为
  `ToggleButton`，`itemIcon` / `itemTitle` 分别为 `IconPresenter` / `ContentPresenter`）。
- `TreeViewItem` 与 `FloatableTreeView` 不持有独立 descriptor；`FloatableTreeView` 复用 `TreeView` owner scope。
- N 个已实现节点时 switcher / icon / title 各 N 个 marker、indicator 2N 个 marker（checkbox + radio 两个备选）；空
  集合为 0；容器复用 / 回收与 items 变化不增删既有容器 marker。
- 显式 `TreeViewItem`、`ItemsSource` 生成容器与数据驱动容器三种来源 marker 语义一致。
- 层级展开 / 收起、勾选 / 禁用 / 拖拽 / 过滤 / ToggleType 切换只改变有效视觉属性与伪类，不增删 marker；叶子节点
  switcher 恒存在；`ToggleType=None` 时 indicator 节点隐藏但 marker 仍在。
- `TreeView` 的 `> .semantic-item` route 只命中顶层容器；`TreeViewItem` 的 `> .semantic-item` route 只命中子容器；
  `TreeViewItem` 的两段 `/template/` route（经 `.semantic-scope-header` 跳点）命中每个节点的 switcher / indicator /
  icon / title。
- owner-scoped Semantic Style（生成的 Style 类型）与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- `IsShowLine=true` 时 item 的 `BorderBrush` / `BorderThickness` 覆盖仍驱动树形连线；`NodeHoverMode=WholeLine` 时 item
  背景覆盖作用于整行背景。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
