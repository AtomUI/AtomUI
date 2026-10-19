# TreeView 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.TreeView` 桌面版的最新设计定位、公共契约、行为状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [TreeView 桌面版实现原理](implementation.md)，Semantic Part 契约见 [TreeView Semantic Part 契约](semantic-part.md)，TreeView Token 的专项设计见 [TreeView Token 设计](token.md)，设计和契约变化记录见 [TreeView Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `TreeView`，其 internal `IsPopupPinnedOpen` 只供测试和内部诊断使用；设置为 true 时保持 tree Flyout open state，并 relay 到 `TreeViewFlyout` 及其 Popup，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/TreeView` |
| 控件状态 | Stable |

TreeView 是桌面端数据展示类树形结构控件，用于展示具有父子层级的数据、文件目录、组织结构、权限结构、分类结构和可展开节点集合。它以 Avalonia `TreeView` 为基础，扩展 AtomUI 的节点数据模型、图标、连线、选择、勾选、过滤、高亮、异步加载、拖拽、空状态、动效、Form 集成和 Token 体系。

TreeView 的职责是管理树节点容器生成、层级展开、选择、勾选、过滤展示、节点异步加载和节点拖拽重排。它不负责业务路由、文件系统访问、权限计算、搜索数据源请求、远程分页、虚拟化列表、节点详情面板或业务命令编排。

TreeView 支持两种节点提供方式：

- 直接放置 `TreeViewItem`。
- 通过 `ItemsSource` 绑定实现 `ITreeItemNode` 的节点集合。

直接子元素适合静态树或少量手写节点；`ItemsSource` 适合数据驱动树、默认路径回放、异步加载和 Form / Select 体系协同。

## 2. 设计语言

TreeView 表达的是“层级结构 + 当前节点状态”的信息浏览语义。用户应能通过缩进、展开图标、节点图标、连线、hover 背景、选中背景、勾选状态和过滤高亮理解节点之间的层级关系和当前操作目标。

设计语言由以下维度组成：

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 层级结构 | 节点在树中的深度和父子关系。 | 缩进、switcher、连线。 |
| 展开状态 | 节点子级是否可见。 | expand / collapse 图标、展开动效。 |
| 选择状态 | 当前业务选择目标。 | selected 背景、`SelectedItem` / `SelectedItems`。 |
| 勾选状态 | 批量选择或节点状态标记。 | checkbox、radio、半选。 |
| 过滤状态 | 搜索或过滤命中的节点。 | 高亮文本、隐藏未命中节点、展开命中路径。 |
| 加载状态 | 子节点异步加载中。 | loading switcher icon。 |
| 拖拽状态 | 节点重排的当前目标。 | drag preview、drop indicator、drag-over 状态。 |

TreeView 的视觉强度应保持克制。节点 header 是可扫描的信息行，不应被绘制成卡片；连线和 switcher 是层级辅助，不应覆盖选中、禁用、过滤和拖拽状态的可读性。

## 3. API 与契约模型

TreeView 的公共契约由 TreeView API、TreeViewItem API、节点数据 API、事件 API、template part 和伪类组成。

TreeView 核心 API：

| API | 语义 |
| --- | --- |
| `IsAutoExpandParent` | 子节点展开时是否自动展开父节点。 |
| `IsDraggable` | 是否启用节点拖拽重排。 |
| `IsShowIcon` | 是否显示节点图标。 |
| `IsShowLine` | 是否显示树形连线。 |
| `IsDefaultExpandAll` | 加载后是否默认展开全部节点。 |
| `NodeHoverMode` | 节点 hover 背景范围，支持 `Default`、`Block`、`WholeLine`。 |
| `SwitcherExpandIcon` / `SwitcherCollapseIcon` / `SwitcherRotationIcon` / `SwitcherLoadingIcon` / `SwitcherLeafIcon` | switcher 图标入口。 |
| `IsShowLeafIcon` | 是否显示叶子节点图标。 |
| `IsSwitcherRotation` | 是否使用旋转图标表达展开收起。 |
| `IsSelectable` | 是否允许节点选择。 |
| `IsSelectOnRightClick` | 右键节点时是否更新选择。 |
| `ToggleType` | 节点勾选模式，支持 none、checkbox、radio。 |
| `IsCheckStrictly` | checkbox 模式下是否关闭父子级级联。 |
| `DefaultSelectedPaths` / `DefaultCheckedPaths` / `DefaultExpandedPaths` | 初始选择、勾选和展开路径入口。 |
| `DataLoader` / `AsyncLoadTimeout` | 异步加载子节点入口和超时时间。 |
| `Filter` / `FilterValue` / `FilterValueSelector` / `FilterStrategy` | 过滤、高亮、展开路径和隐藏未命中节点入口。 |
| `FilterResultCount` | 当前过滤命中数量。 |
| `EmptyIndicator` / `EmptyIndicatorTemplate` / `IsShowEmptyIndicator` / `EmptyIndicatorPadding` | 空状态展示入口。 |
| `IsMotionEnabled` / `OpenMotion` / `CloseMotion` | 展开收起动效入口。 |

TreeViewItem 节点 API：

| API | 语义 |
| --- | --- |
| `Icon` | 节点图标。 |
| `IsChecked` | checkbox / radio 状态，支持半选。 |
| `IsLeaf` | 是否为叶子节点。 |
| `IsLoading` | 是否处于异步加载状态。 |
| `GroupName` | radio 分组名。 |
| `Value` | 节点值。 |
| `IsIndicatorEnabled` | 当前节点 checkbox / radio 是否可用。 |
| `ItemKey` | 路径匹配和节点身份标识。 |

`ITreeItemNode` 是数据驱动树的节点契约，提供 `Header`、`Icon`、`ItemKey`、`Children`、`IsEnabled`、`IsChecked`、`IsSelected`、`IsExpanded`、`IsIndicatorEnabled`、`GroupName`、`IsLeaf`、`Value` 和 parent node 更新能力。数据驱动场景下 `Children` 是节点结构的权威子集合；TreeViewItem 只投射节点状态和视觉交互，不作为结构数据源。

节点数据类型边界：

| 类型 | 定位 | 绑定能力 | 兼容边界 |
| --- | --- | --- | --- |
| `TreeItemNode` | 轻量数据节点，适合 C# 构造、异步加载结果和普通 ItemsSource。 | 作为 binding source 使用，不承载 Avalonia binding target 语义。 | 保持 POCO / record 形态，不改造成 `AvaloniaObject`。 |
| 用户自定义 `ITreeItemNode` | 业务自己的树节点模型。 | 可自行实现 `INotifyPropertyChanged` 或其他业务通知机制。 | TreeView 只依赖 `ITreeItemNode` 契约，不要求继承 AtomUI 节点基类。 |
| `BindableTreeItemNode` | 绑定型节点，适合在 XAML 中把节点属性作为 Avalonia binding target 或 `DynamicResource` target。 | 使用 Avalonia 属性系统，支持 `Header`、`Icon`、`IsChecked`、`IsSelected`、`IsExpanded` 等节点属性绑定。 | 独立于 `TreeItemNode`，不替代轻量节点；作为 owner-managed 非 Visual `AvaloniaObject` 必须遵守 scoped resource host 生命周期。 |

事件 API：

| 事件 | 语义 |
| --- | --- |
| `CheckedItemsChanged` | `CheckedItems` 增删变化。 |
| `TreeItemLoaded` | 异步节点加载完成。 |
| `ItemDragStarted` / `ItemDragCompleted` | 节点拖拽开始和结束。 |
| `ItemDragEnter` / `ItemDragLeave` / `ItemDragOver` | 拖拽目标变化。 |
| `ItemDropped` | 节点完成 drop 操作。 |
| `ItemExpanded` / `ItemCollapsed` | 节点展开和收起。 |
| `ItemClicked` | 节点 header 点击。 |
| `ItemContextMenuRequest` | 节点右键上下文菜单请求。 |

稳定 template part：

| Template Part | 所属控件 | 职责 |
| --- | --- | --- |
| `ItemsPresenter` | `TreeView` | 承载顶层节点容器。 |
| `EmptyIndicator` | `TreeView` | 空状态内容展示。 |
| `Header` | `TreeViewItem` | 节点 header 控件。 |
| `PART_ItemsPresenterMotionActor` | `TreeViewItem` | 子节点展开收起动效容器。 |
| `PART_NodeSwitcherButton` | `TreeViewItemHeader` | 展开、收起、加载和叶子图标入口。 |
| `PART_IconPresenter` | `TreeViewItemHeader` | 节点图标展示。 |
| `PART_HeaderContentFrame` | `TreeViewItemHeader` | header 内容背景、hover、selected、pressed 和 pointer 事件边界。 |

伪类契约：

- `TreeView :draggable` 表示启用拖拽。
- `TreeViewItem :toggle` 表示 checkbox 模式。
- `TreeViewItem :radio` 表示 radio 模式。
- `TreeViewItemHeader :toggle` / `:radio` 与节点 toggle 模式同步。

### 3.1 Semantic Part 契约

TreeView 是递归层级容器，使用两个递归 Semantic owner 表达与上游 Tree 稳定 Semantic DOM 对齐的六个 Part，完整契约见
[TreeView Semantic Part 契约](semantic-part.md)：

| Owner | Part | Selector | AtomUI 节点 | Cardinality | 定制方式 |
| --- | --- | --- | --- | --- | --- |
| `TreeView` | `root` | 控件本身 | `TreeView` owner | `Single` | owner 选择器 + 公开属性 |
| `TreeView` | `item` | `.semantic-item` | 每个顶层 `TreeViewItem` 容器 | `Multiple` | `TreeViewItemStyle` |
| `TreeViewItem` | `root` | 控件本身 | 每个 `TreeViewItem` 容器 | `Single` | owner 选择器 + 公开属性 |
| `TreeViewItem` | `item` | `.semantic-item` | 每个子级 `TreeViewItem` 容器 | `Multiple` | `TreeViewItemItemStyle` |
| `TreeViewItem` | `itemSwitcher` | `.semantic-item-switcher` | 每个 header 模板的 `PART_NodeSwitcherButton` | `Multiple` | `TreeViewItemItemSwitcherStyle` |
| `TreeViewItem` | `itemIndicator` | `.semantic-item-indicator` | 每个 header 模板的 `ToggleCheckbox` / `ToggleRadio` | `Multiple` | `TreeViewItemItemIndicatorStyle` |
| `TreeViewItem` | `itemIcon` | `.semantic-item-icon` | 每个 header 模板的 `PART_IconPresenter` | `Multiple` | `TreeViewItemItemIconStyle` |
| `TreeViewItem` | `itemTitle` | `.semantic-item-title` | 每个 header 模板的 `HeaderPresenter` | `Multiple` | `TreeViewItemItemTitleStyle` |

`TreeView` 与 `TreeViewItem` 是递归 owner：`TreeView.item` 覆盖顶层容器，`TreeViewItem.item` 递归覆盖下一层子容器，
二者使用同一 `.semantic-item` 身份，保证任意深度的节点都可命中。`itemSwitcher` / `itemIndicator` / `itemIcon` /
`itemTitle` 声明在 `TreeViewItem` 上，route 从 `TreeViewItem` owner 出发经 `.semantic-scope-header` 跳点以两次
`/template/` 进入 header 模板，因此对每个节点（无论层级）都可达。`TreeViewItem` 因此持有独立 descriptor（与
`SegmentedItem` 不同；与单层容器的 `ListView` / `ListBox` 也不同，见 [semantic-part.md §1](semantic-part.md#1-owner-边界)）。

`item`、`itemSwitcher`、`itemIndicator`、`itemIcon`、`itemTitle` 是运行时生成 Part：`.semantic-item` 在容器创建与
prepare 路径幂等添加，其余四个 marker 静态声明于 `TreeViewItemHeaderTheme.axaml`。`itemIndicator` 是 checkbox / radio
两个备选节点共用的单一 Part，每个容器恒有两个 marker 实例（对应两个备选节点），同一时刻最多一个可见。marker 不随
展开/收起、勾选、禁用、拖拽、过滤、ToggleType 切换与容器回收增删。定制摘要：

- 状态型定制（选择、勾选、展开、禁用、拖拽、过滤、hover mode、show-line/show-icon）通过 owner 公开属性完成，不改变
  marker 数量。
- 局部视觉定制通过生成的 Semantic Style 完成，`ContractType` 收缩到公开类型（`TreeViewItem` / `ToggleButton` /
  `IconPresenter` / `ContentPresenter`），internal 节点（`NodeSwitcherButton`、`CheckBoxIndicator`、
  `TreeViewItemHeader`）不作为公共依赖类型。
- `TreeViewItemHeader`、`NodeSwitcherButton`、`FloatableTreeView` 不持有独立 Semantic descriptor；`FloatableTreeView`
  复用 `TreeView` owner scope。
- checkbox / radio 勾选指示、过滤高亮节点、header 内容框与树形连线自绘逻辑不属于 Semantic Part，见
  [semantic-part.md §7](semantic-part.md#7-定制边界)。

## 4. 行为与状态模型

TreeView 的状态模型由节点状态、选择状态、勾选状态、展开状态、过滤状态、异步加载状态、拖拽状态和空状态组成。

选择行为：

- `IsSelectable=false` 时不允许节点被选中，并清空 TreeView 当前选择。
- `IsSelectOnRightClick=false` 时，右键不更新选择。
- `SelectionMode` 继承 Avalonia `TreeView` 语义，单选使用 `SelectedItem`，多选使用 `SelectedItems`。
- Form 集成以单选 / 多选模式分别读取和写入 `SelectedItem` 或 `SelectedItems`，并保持原始节点对象 / 列表实例，不把节点值转换为字符串。

勾选行为：

- `ToggleType=None` 时不显示勾选入口。
- `ToggleType=CheckBox` 时显示 checkbox。
- `ToggleType=Radio` 时只在叶子节点显示 radio。
- `IsCheckStrictly=false` 时 checkbox 勾选会级联子树，并根据子级状态更新父级 true / false / null。
- `IsCheckStrictly=true` 时 checkbox 只同步当前节点，不级联父子级。
- `CheckedItems` 是当前勾选数据集合，变化会同步已实现容器状态并触发 `CheckedItemsChanged`。

展开行为：

- switcher 触发展开收起。
- `IsDefaultExpandAll=true` 时加载后展开全部节点，并优先于 `DefaultExpandedPaths`。
- 默认路径通过 `TreeNodePath` 和 `ItemKey` / `Value` 匹配。
- 展开收起动效由 `IsMotionEnabled`、`OpenMotion`、`CloseMotion` 和 `MotionDuration` 控制。

过滤行为：

- `Filter`、`FilterValue` 和 `FilterValueSelector` 共同决定节点是否命中。
- `FilterStrategy` 控制高亮 match、整行高亮、加粗、展开命中路径和隐藏未命中节点。
- `FilterResultCount` 表示命中节点数量。
- 过滤模式下空状态依据 `FilterResultCount` 判断。

拖拽行为：

- `IsDraggable=true` 时，左键按下并超过拖拽阈值后进入拖拽。
- TreeView 创建拖拽预览和 drop indicator。
- drop 目标支持插入到根、插入到兄弟前后、插入到目标节点内部。
- 不允许把节点 drop 到自身或自身后代内。
- 拖拽命中以已实现的 `TreeViewItem` 容器计算，结构修改以数据源为权威。
- drop 操作通过 TreeView 内部数据控制器移动 root 集合或节点 `Children`，不直接修改生成容器的 `Items`。
- 节点移动是结构重排，不是业务删除；选中、勾选和展开状态按节点身份保留。

异步加载行为：

- `DataLoader` 只在 `ItemsSource` 数据驱动场景下使用。
- 未加载节点点击 switcher 时触发加载。
- 加载中节点显示 loading switcher icon。
- 加载成功后把返回子节点写入目标节点 `Children`，并展开目标节点。

绑定型节点行为：

- `TreeItemNode` 的定位是轻量数据源节点，不承载 `DynamicResource`、Avalonia styled binding target 或资源宿主职责。
- 需要在 XAML 中直接绑定节点属性，或把节点 `Header`、`Icon`、状态属性设置为 `DynamicResource` 时，使用独立的 `BindableTreeItemNode`。
- `BindableTreeItemNode` 进入 TreeView 容器生命周期时，由 owner TreeView / TreeViewItem attach scoped resource host；离开容器、detach、re-template 或 container recycle 时释放 attach token。
- 绑定型节点属性变化应同步当前生成的 `TreeViewItem` 容器；容器交互导致的 checked、selected、expanded 等状态变化也应按契约回写节点状态。
- 自定义 `ITreeItemNode` 仍按普通数据模型处理；TreeView 不要求用户模型继承 `BindableTreeItemNode`。

## 5. 视觉与主题模型

TreeView 主题按 root、item、header、switcher 四层组织。

```text
TreeViewTheme
  root scroll viewer
  items presenter
  empty indicator

TreeViewItemTheme
  header
  child items motion actor

TreeViewItemHeaderTheme
  switcher
  checkbox / radio
  icon
  header content frame
  filter highlighter

NodeSwitcherButtonTheme
  current icon presenter
  hover background
  rotation / loading transition
```

视觉规则：

- `NodeHoverMode=Default` 时 header 内容背景按内容宽度绘制。
- `NodeHoverMode=Block` 时 header 内容背景横向拉伸到剩余区域。
- `NodeHoverMode=WholeLine` 时背景由 TreeViewItem 行级绘制，覆盖整行宽度。
- disabled 节点应使用 disabled 文本色和弱化图标，不应保留可交互 hover 视觉。
- filter match 时显示 `FilterHighlighter`，未命中时显示普通 `HeaderPresenter`。
- `IsShowLine=true` 时，TreeViewItem 自绘树形连线。
- drag indicator 由 TreeView 自绘，不进入节点模板内部。

TreeViewToken 提供节点高度、hover / selected 背景、目录树选中颜色、节点间距、header padding、switcher / icon 间距、拖拽指示线宽和过滤高亮色。Token 详情见 [TreeView Token 设计](token.md)。

## 6. 控件家族或集成关系

TreeView 属于 Data Display 分类，与 List、DataGrid、Card、Descriptions、GroupBox 等控件共同服务结构化数据展示。

集成关系：

- Avalonia `TreeView`：继承 ItemsControl、Selection、容器生成和基础 keyboard / focus 语义。
- `TreeViewItem`：节点容器，承载 icon、checked、loading、leaf、value 和 event。
- `TreeItemNode` / `ITreeItemNode`：数据驱动节点模型。
- `BindableTreeItemNode`：绑定型节点模型，为 XAML binding target、`DynamicResource` 和动态资源场景提供 Avalonia 属性承载。
- `NodeSwitcherButton`：节点展开、收起、加载和叶子图标入口。
- `FloatableTreeView`：TreeView 的浮层变体，用于 TreeViewFlyout 场景。
- Form：通过 `IFormItemAware` 读取和写入选择值；单选值是 `SelectedItem`，多选值是 `SelectedItems`。
- Motion：展开收起和背景状态过渡遵守 `IsMotionEnabled`。
- AsyncLoad：通过 `ITreeItemNodeLoader` 加载子节点。

TreeView 不实现 CompactSpace、Button 家族或 Popup 菜单导航模型。

## 7. 兼容性不变量

维护 TreeView 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 TreeView / TreeViewItem / ITreeItemNode public API。
- `TreeItemNode` 保持轻量 POCO / record 数据节点定位，不直接改造成 `AvaloniaObject`。
- 绑定型节点能力通过独立 `BindableTreeItemNode` 承载，不能通过破坏 `TreeItemNode` record 语义、init 属性或相等性来实现。
- `DefaultSelectedPaths`、`DefaultCheckedPaths`、`DefaultExpandedPaths` 是默认状态入口，不是持续受控状态。
- `IsDefaultExpandAll=true` 优先于 `DefaultExpandedPaths`。
- `SelectedItem` / `SelectedItems` 的 Avalonia 选择语义不变。
- `CheckedItems` 与已实现容器的 `IsChecked` 必须双向同步，内部同步不得递归触发重复事件。
- `IsCheckStrictly=false` 时 checkbox 保持父子级级联和半选语义；`true` 时只同步当前节点。
- `ToggleType=Radio` 只在叶子节点显示 radio，并遵守 `GroupName` 分组。
- `TreeNodePath` 匹配优先使用 `ItemKey`，没有 `ItemKey` 时才使用 `Value` 字符串。
- `ItemsSource` 变化后应尽量按节点身份路径恢复运行期选择和勾选状态，再回放默认状态。
- `ItemsSource` 替换期间的容器清理和状态回放不应对外暴露中间选择或勾选；最终状态未变时不触发对应事件，真正变化时 `SelectionChanged`、`CheckedItemsChanged` 与 Form 值变更只通知最终差异。
- 多选模式下 `SelectedItems` 是运行期选择恢复的权威来源，`ItemsSource` 变化或容器首次回放不能因 `SelectedItem` 非空而把多选折叠成单选。
- filter 清除后必须恢复过滤前节点可见性、展开状态和高亮状态。
- 异步加载只在数据节点模型下写入 `ITreeItemNode.Children`，不修改普通手写 `TreeViewItem` 子树。
- 非 Visual `AvaloniaObject` 节点只要承载 `DynamicResource` 或 token-resource binding，就必须使用 scoped resource host，并有明确 attach/release 路径。
- 拖拽不得允许节点 drop 到自身或自身后代。
- 拖拽结构修改必须通过内部数据控制器执行，不能在 `ItemsSource` 场景直接写 `TreeView.Items` 或 `TreeViewItem.Items`。
- TreeView 维护节点到父级、兄弟集合和索引的内部索引；拖拽过程中不能为每次 drop 全树扫描定位节点。
- 跨父级移动后，节点 `ParentNode`、root / child 集合、选中集合、勾选集合和展开状态必须保持一致。
- Template part 名称和职责不擅自修改。
- Token 名称和语义不擅自重命名或迁移为实例状态。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 默认状态回放模型

TreeView 加载后按固定顺序回放默认选中、默认勾选、过滤和默认展开。`IsDefaultExpandAll=true` 时展开全部节点，并覆盖 `DefaultExpandedPaths` 的展开路径。

### 8.2 Path 模型

`TreeNodePath` 通过路径段定位节点。路径段匹配 `TreeViewItem.ItemKey` 或 `TreeViewItem.Value`，用于默认状态回放、ItemsSource 变化后的状态恢复和选中路径定位。

### 8.3 勾选模型

TreeView 同时支持 checkbox 和 radio。checkbox 可以按 `IsCheckStrictly` 选择级联或严格模式；radio 使用 `GroupName` 和 radio group manager 保持同组互斥。

### 8.4 过滤模型

过滤模型由 `FilterStrategy` 组合控制：高亮命中片段、整行高亮、加粗、展开路径、隐藏未命中。过滤不是数据源查询，只作用于已实现的 TreeViewItem 容器。

### 8.5 异步加载模型

异步加载通过 `ITreeItemNodeLoader` 与 `AsyncLoadTimeout` 控制。加载请求由 switcher 触发，同一节点加载请求应合并或等待已有请求完成。

### 8.6 拖拽模型

拖拽模型分为 UI 命中层和数据结构层：

- UI 命中层使用 `TreeViewItem` header bounds、层级关系和 viewport 可见性计算拖拽发起节点、drag-over 节点和 drop indicator。
- 数据结构层使用内部数据控制器把 drop 结果转换为节点移动请求，统一处理插入到 root、兄弟前后和目标节点内部。
- 数据控制器只修改权威集合：root `ItemsSource` 对应的可变集合，或目标 `ITreeItemNode.Children`。手写 `TreeViewItem` 子树只作为静态 / 少量直接子元素模型，不作为数据驱动拖拽的权威路径。
- TreeView 维护轻量节点索引，记录节点、父节点、所在集合和兄弟索引。索引在 root 数据源变化、节点 `Children` 变化和异步加载结果进入时增量更新，drop 时不全树扫描。
- 同集合移动按 source index 和 target index 统一修正插入位置；跨集合移动先从源集合移除，再插入目标集合，并同步 parent node。
- 节点移动保留节点身份，因此选中、勾选和展开状态不得按删除清理。只有节点真正离开整棵树时，状态集合才进行边界清理。
- drop 事件语义应以数据节点、源父级、目标父级、插入位置和 drop position 为核心，容器只作为可观察 UI 上下文。

### 8.7 绑定型节点模型

绑定型节点模型用于解决“节点属性本身需要成为 Avalonia binding target”的场景。它不改变 `ITreeItemNode` 作为数据契约的定位，也不要求业务节点继承 AtomUI 类型。

设计规则：

- `TreeItemNode` 继续服务轻量数据源场景；`BindableTreeItemNode` 服务 XAML binding target、`DynamicResource` 和动态主题资源场景。
- `BindableTreeItemNode` 继承 `AvaloniaObject`，暴露节点属性对应的 Avalonia 属性，并实现 `ITreeItemNode`。
- `BindableTreeItemNode` 使用 `[GenerateScopedResourceHost]` 生成 scoped `IResourceHost` / `IThemeVariantHost` 生命周期样板代码。
- TreeView / TreeViewItem 是 owner，负责在容器准备时 attach resource host，在容器清理、detach 或 recycle 时释放。
- 节点属性变化只同步当前生成容器，不把容器或视觉树对象永久挂回节点。
- `Header` 仍遵守 Avalonia TreeView 数据项语义：生成容器的 `Header` 是节点对象，`node.Header` 通过 `TreeDataTemplate` 绑定更新展示内容。
- 普通 `TreeItemNode`、用户自定义 `ITreeItemNode` 与 `BindableTreeItemNode` 可以在同一个 TreeView 数据源中并存。

## 9. 文档导航与验证策略

关联文档：

- [TreeView 桌面版实现原理](implementation.md)
- [TreeView Semantic Part 契约](semantic-part.md)
- [TreeView Token 设计](token.md)
- [TreeView Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| 文档 | `overview.md`、`implementation.md`、`semantic-part.md`、`token.md`、`changelog.md` 链接有效。 |
| Public API | TreeView、TreeViewItem、ITreeItemNode、事件参数和默认值与源码一致。 |
| 状态回放 | 默认选中、默认勾选、默认展开、默认展开全部和 ItemsSource 变化恢复。 |
| 勾选 | strict / cascading、半选、CheckedItems 同步、radio group。 |
| 过滤 | 高亮、加粗、展开路径、隐藏未命中、清除过滤和空状态。 |
| 异步加载 | loading 状态、成功写入子节点、超时 / 取消事件结果和 detach 取消。 |
| 绑定型节点 | XAML binding target、DynamicResource、owner resource 优先级、属性变化同步容器、container clear 释放。 |
| 拖拽 | preview、drop indicator、根 / 子级插入、自身后代保护和事件顺序。 |
| Theme / Token | template part、hover mode、selected / disabled、line、switcher、drag indicator 和 token 表。 |
| Semantic Part | 检查 descriptor 数量/顺序/字段、marker 数量与类型、容器回收后的 marker 身份，见 [semantic-part.md §8](semantic-part.md#8-兼容性与验证)。 |
