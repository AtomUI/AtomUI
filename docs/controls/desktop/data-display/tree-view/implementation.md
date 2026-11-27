# TreeView 桌面版实现原理

本文档描述 TreeView 桌面版的内部源码结构、状态流转、容器生命周期、默认状态回放、过滤、勾选、异步加载、拖拽和键盘导航实现。公共设计与 API 契约见 [TreeView 桌面版架构设计](overview.md)，Semantic Part 契约见 [TreeView Semantic Part 契约](semantic-part.md)，键盘导航专项设计见 [TreeView 键盘导航设计](keyboard-navigation-design.md)，Token 语义见 [TreeView Token 设计](token.md)，变化记录见 [TreeView Changelog](changelog.md)。

Popup 接入边界：`TreeView` 负责业务状态和内容准备，`TreeViewFlyout` 仅作为 relay 适配层，tree Flyout Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

TreeView 的实现目标是在 Avalonia `TreeView` 基类上增加 AtomUI 树形数据展示能力，并保持 API、行为和主题契约稳定。实现文档覆盖 `TreeView`、`TreeViewItem`、`TreeViewItemHeader`、`NodeSwitcherButton`、节点数据模型、interaction handler、过滤、拖拽、异步加载、键盘导航和主题接入。

TreeView 内部实现较多，按功能拆分 partial 文件。拆分边界服务于维护清晰度，不改变 public API 所在位置、属性定义顺序、事件语义和渲染结果。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/TreeView/TreeView.cs`：public API、事件、容器生成、生命周期、选择、空状态、Form 集成和公共方法。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.SemanticParts.cs`：TreeView Semantic Part descriptor 声明（`[SemanticPart]`），
  不承载模板节点、Setter、Style 实例或运行时 VisualTree 查找逻辑。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewItem.SemanticParts.cs`：TreeViewItem 递归 owner 的 Semantic Part descriptor
  声明（`[SemanticPart]`），覆盖子节点容器与 switcher / indicator / icon / title 内容区域。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.StateReplay.cs`：loaded 回放、ItemsSource 变化后的选择 / 勾选 / 展开状态恢复。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.PathTraversal.cs`：`TreeNodePath` 遍历、展开路径、容器确保和展开状态恢复。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.CheckedState.cs`：checkbox 严格 / 级联状态、按节点数据计算的勾选计划、`CheckedItems` 同步与引用镜像、容器准备时的勾选对齐、数据删除时的勾选清理和半选父级计算。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.Filter.cs`：过滤、高亮、隐藏未命中、展开路径、filter context 备份和恢复。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.AsyncItemDataLoad.cs`：异步加载、加载合并、超时、取消和 `TreeItemLoaded` 派发。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.SelectionState.cs`：节点选择镜像，让 `ITreeItemNode.IsSelected` 跟随 `SelectedItems`（含未实现的节点），订阅随视觉树 attach / detach。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.KeyboardNavigation.cs`：键盘活动项、可导航节点遍历（逐项按容器、范围选择按节点数据）、按键分派、提交策略、活动项视觉可见性、Tab 导航钩子、宿主转发入口 `HandleKeyDown` / `ClearKeyboardActiveItem` 和宿主开关 `IsKeyboardMoveSelectionEnabled`。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.DragAndDrop.cs`：拖拽命中测试、drag preview、drop indicator、drop 操作和拖拽事件。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.DataController.cs`：内部数据控制器、drop request / result、root / children 集合移动和 parent node 同步。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.NodeIndex.cs`：节点索引、节点父级、所在集合、兄弟索引和 collection changed 增量维护。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewItem.cs`：节点容器、展开收起与统一展开请求入口、line 渲染、check/radio 状态、`:keyboard-active` 呈现、drag bounds 和数据节点承载。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewItemHeader.cs`：header 视觉状态、pointer 状态、switcher mode、filter highlight runs 和 template part 订阅。
- `src/AtomUI.Desktop.Controls/TreeView/NodeSwitcherButton.cs`：switcher 当前图标选择、默认图标和 rotation 动效；点击时调用 `TreeViewItem` 的展开请求入口。
- `src/AtomUI.Desktop.Controls/TreeView/TreeItemNode.cs`、`ITreeItemNode.cs`：轻量数据节点模型、节点契约和父子关系维护。绑定型节点模型属于同一节点数据职责边界，必须保持独立类型，不改造 `TreeItemNode`。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.Interaction.cs`：树节点的激活、上下文菜单、单选分组和勾选提交，浮层树在实际需要时接入外部关闭事件。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewBehavior.cs`：供应用与设计语言贡献的节点标题激活策略。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewItemDecoration.cs`：由默认主题模板提供的整行背景和节点连线。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewToken.cs`：TreeView Token 定义。
- `src/AtomUI.Desktop.Controls/TreeView/Themes/`：TreeView、TreeViewItem、TreeViewItemHeader、NodeSwitcherButton 主题和模板常量。

## 3. 核心类职责

`TreeView` 是树根控件，负责 public API、ItemsControl 容器生成、默认状态回放、选择、勾选集合、过滤入口、拖拽状态、异步加载入口、空状态和 Form 集成。

`TreeViewItem` 是节点容器，负责节点 icon、checked、loading、leaf、value、line render、展开收起动效和 drag bounds。它实现 `ITreeItemNode`，使直接 `TreeViewItem` 和数据节点场景能共享路径和层级逻辑。

`TreeViewItemHeader` 是节点 header 的视觉和交互边界，承载 switcher、checkbox / radio、icon、内容、filter highlighter、hover、pressed、selected、drag 和 disabled 状态。

`NodeSwitcherButton` 是展开、收起、loading 和 leaf 图标切换入口。点击时调用 `TreeViewItem` 的展开请求入口；未加载节点由该入口触发 `NodeLoadRequestEvent`，由 TreeView 接管异步加载。键盘 → 键使用同一入口。

`DefaultTreeViewInteractionHandler` 处理 TreeView 级 pointer、右键上下文菜单、radio group、checked changed 和浮层关闭逻辑，使节点容器不直接持有全局输入订阅。

TreeView 的键盘导航部分（`TreeView.KeyboardNavigation.cs`）持有唯一的键盘活动项引用，负责遍历可导航节点、分派按键和执行提交策略。节点容器只呈现 TreeView 推送的 `:keyboard-active` 状态，不参与焦点和导航判断。

`TreeDataController` 是 TreeView 结构修改的唯一内部入口。它把拖拽、异步加载和可变节点集合操作归一到数据层，负责校验移动合法性、选择权威集合、计算插入索引、执行 remove / insert 和同步 parent node。

`TreeNodeIndex` 是 TreeView 内部的轻量节点索引。它记录节点所属父级、兄弟集合和索引，用于 drop 时快速定位 source / target 上下文。索引随 root 数据源、节点 `Children` 和异步加载结果增量更新，不参与每帧拖拽命中渲染。

`TreeItemNode` 是轻量数据驱动节点模型，维护 `Children` 与 `ParentNode` 的一致性。它不继承 `AvaloniaObject`，不承载 `DynamicResource` 或 Avalonia binding target 语义。

`BindableTreeItemNode` 是绑定型数据节点模型，继承 `AvaloniaObject` 并实现 `ITreeItemNode`。它用于节点属性需要作为 Avalonia binding target 的场景，资源宿主生命周期由 `[GenerateScopedResourceHost]` 生成，owner TreeView / TreeViewItem 负责 attach/release。

## 4. 状态与数据流

容器生成流：

```text
Items / ItemsSource
      ↓
TreeView.CreateContainerForItemOverride
      ↓
TreeViewItem
      ↓
ContainerForItemPreparedOverride
  OwnerTreeView
  ApplyNodeData(ITreeItemNode)
  BindableTreeItemNode attach resource host + two-way property sync
  plain ITreeItemNode: container state write-back
  ItemTemplate -> HeaderTemplate
  switcher icons
  filter / motion / hover / line / toggle / selectable state bindings
```

默认状态回放流：

首次布局前，模板化的节点应用当前可解析的默认展开路径。首次布局完成后，控件在呈现之前以静态方式应用可解析的默认选中和勾选路径。加载阶段补齐尚未应用的状态并执行过滤。

```text
首次布局完成
  默认选中和勾选路径，暂停动效
OnLoaded
  回放尚未应用的默认选中和勾选路径
  FilterTreeNode
  IsDefaultExpandAll ? ExpandAll(false) : ConfigureDefaultExpandedPaths
```

ItemsSource 变化恢复流：

```text
capture SelectedItem / SelectedItems / CheckedItems identity paths
clear current selection and checked collection
SelectionMode.Multiple ? restore runtime SelectedItems paths : restore runtime SelectedItem path
fallback to DefaultSelectedPaths
restore runtime checked paths if possible
fallback to DefaultCheckedPaths
restore expanded state or expand all
```

回放事务以最新 `ItemsSource` 修订为准。若属性观察者在恢复选择时同步再次换源，后一次回放接管节点路径与事务；最初的快照继续用于计算最终对外差异，旧回放不发布中间事件。

Form 值流：

```text
IFormItemAware.SetFormValue(value)
      ↓
SelectionMode.Multiple ? SelectedItems = value as IList : SelectedItem = value
      ↓
SelectedItem / SelectedItems class handler
      ↓
IFormItemAware.ValueChanged
```

TreeView 的 Form 适配层只投射 Avalonia 原生选择状态，不建立第二套选择值。单选模式写入和读取 `SelectedItem`，多选模式写入和读取 `SelectedItems`；写入时必须保留调用方传入的节点对象或 `IList` 实例，不能先转换为字符串。

勾选流：

```text
TreeViewItem.IsChecked changed            CheckedItems Add / Remove / Reset (code)
      ↓                                            ↓
DefaultTreeViewInteractionHandler.OnCheckedChanged  HandleCheckedItemsCollectionChanged
      ↓                                            ↓
Radio: RadioButtonGroupManager             (CheckBox cascading) plan over node data
CheckBox strict: sync current item only     (other modes) project the item only
CheckBox cascading: plan subtree + parents over node data
      ↓
CommitCheckedStatePlan: CheckedItems → node IsChecked → realized container IsChecked
      ↓
CheckedItemsChanged

container prepared from node data → ReconcilePreparedCheckedState (CheckBox only)
```

过滤流：

```text
Filter / FilterValue / FilterStrategy
      ↓
expand realized tree without motion
      ↓
backup item filter context
      ↓
post-order match descendants
      ↓
highlight / hide / expand-path state
      ↓
FilterResultCount + empty indicator
```

异步加载流：

```text
NodeSwitcherButton.Toggle / keyboard expand key
      ↓
TreeViewItem expand request
  if HasTreeItemDataLoader && !AsyncLoaded
      raise NodeLoadRequestEvent
      ↓
TreeView.LoadNodeDataAsync
      ↓
ITreeItemNodeLoader.LoadAsync
      ↓
append result.Data to ITreeItemNode.Children
      ↓
TreeItemLoaded + expand target node
```

键盘导航流：

```text
KeyDown on TreeView / host HandleKeyDown
      ↓
source filter（只处理 TreeView 自身或宿主转发）
      ↓
validate / repair active item → initial active item
      ↓
FlowDirection-aware key mapping
      ↓
move: preorder walk over navigable containers → active item → commit policy → BringIntoView
expand / collapse: TreeViewItem expand request（含 NodeLoadRequestEvent）
commit: checked entry / selection entry
      ↓
:keyboard-active + Handled
```

提交策略矩阵、初始活动项规则和活动项生命周期表见 [TreeView 键盘导航设计 §4、§7](keyboard-navigation-design.md#4-提交策略)。

拖拽 drop 数据流：

```text
Pointer drag over realized TreeViewItem
      ↓
DropTargetInfo from header bounds
      ↓
TreeDropRequest
  SourceNode + SourceParent + SourceIndex
  TargetParent + TargetIndex + DropPosition
      ↓
TreeDataController.CanMove
      ↓
TreeDataController.Move
      ↓
mutate authoritative root collection / ITreeItemNode.Children
      ↓
TreeNodeIndex incremental update
      ↓
NodeDropped / ItemDropped event projection
```

## 5. 生命周期与模板接入

`TreeView` 静态构造注册 drag/drop、TreeViewItem routed event class handler、filter class handler 和 Form value changed class handler。

`TreeView` 实例构造注册 TreeViewToken scope，并监听 root `Items.CollectionChanged`。`OnInitialized` 设置默认 filter、默认 filter value selector 和空状态。`OnAttachedToVisualTree` 挂接 interaction handler，`OnDetachedFromVisualTree` 解除 handler 并取消所有异步加载。`OnLoaded` 回放默认状态。

`TreeViewItem.OnApplyTemplate` 获取 `Header` 和 `PART_ChildrenViewport`，配置叶子状态，并按当前 `IsExpanded` 同步子项实际可见状态。

`TreeViewItemHeader.OnApplyTemplate` 获取 `PART_HeaderContentFrame`、`PART_IconPresenter` 和 `PART_NodeSwitcherButton`。设置新 `PART_HeaderContentFrame` 前必须解除旧 frame 的 pointer 事件订阅，避免旧模板节点继续影响 hover 状态。

`NodeSwitcherButton.OnAttachedToVisualTree` 设置默认图标。`OnLoaded` 后再启用 transitions，避免初始化阶段产生非预期动画。

### 5.1 Semantic Part marker 放置与路由

TreeView 家族使用两个递归 Semantic owner（`TreeView` 与 `TreeViewItem`），节点结构（marker 与 descriptor 声明、
生成常量的对应关系）：

```text
TreeView (root)                                            ← TreeView owner
  └─ TreeViewItem (item, .semantic-item)                   ← TreeView.item 覆盖顶层容器
       └─ [TreeViewItemTheme 模板]                          ← TreeViewItem owner 从这里开始
            └─ StackPanel
                 ├─ TreeViewItemHeader#Header (.semantic-scope-header)   ← internal 跳点，非 Part
                 │    └─ [TreeViewItemHeaderTheme 模板]
                 │         └─ PixelAlignedBorder#Frame
                 │              └─ Grid#ItemsLayout
                 │                   ├─ NodeSwitcherButton#PART_NodeSwitcherButton  ← .semantic-item-switcher
                 │                   ├─ CheckBox#ToggleCheckbox / RadioButton#ToggleRadio ← .semantic-item-indicator
                 │                   ├─ IconPresenter#PART_IconPresenter             ← .semantic-item-icon
                 │                   └─ Border#PART_HeaderContentFrame
                 │                        └─ Panel
                 │                             ├─ ContentPresenter#HeaderPresenter   ← .semantic-item-title
                 │                             └─ TextBlock#FilterHighlighter（非 Part，过滤高亮）
                 └─ ContentViewport#PART_ChildrenViewport（非 Part）
                          └─ ItemsPresenter
                               └─ TreeViewItem (item, .semantic-item)    ← TreeViewItem.item 递归覆盖子容器
```

marker 放置：

| marker | 放置方式 | 位置 |
| --- | --- | --- |
| `.semantic-item` | 运行时幂等 `Classes.Add` | `TreeView` 与 `TreeViewItem` 的 `CreateContainerForItemOverride` / `PrepareContainerForItemOverride`；`TreeView` 覆盖顶层容器，`TreeViewItem` 递归覆盖子容器 |
| `.semantic-scope-header` | 静态 `Classes.semantic-scope-header="True"` | `TreeViewItemTheme.axaml` 的 `TreeViewItemHeader#Header`；internal 协作类型上的跳点，不发布为 Part |
| `.semantic-item-switcher` | 静态 `Classes.semantic-item-switcher="True"` | `TreeViewItemHeaderTheme.axaml` 的 `NodeSwitcherButton#PART_NodeSwitcherButton` |
| `.semantic-item-indicator` | 静态 `Classes.semantic-item-indicator="True"` | `TreeViewItemHeaderTheme.axaml` 的 `CheckBox#ToggleCheckbox` 与 `RadioButton#ToggleRadio` |
| `.semantic-item-icon` | 静态 `Classes.semantic-item-icon="True"` | `TreeViewItemHeaderTheme.axaml` 的 `IconPresenter#PART_IconPresenter` |
| `.semantic-item-title` | 静态 `Classes.semantic-item-title="True"` | `TreeViewItemHeaderTheme.axaml` 的 `ContentPresenter#HeaderPresenter` |

`item` 在两个 owner 上各自声明一次，均为 `RuntimeCreated` Part：`.semantic-item` marker 在容器创建与 prepare 路径
幂等添加，覆盖用户显式 `TreeViewItem`、`ItemsSource` 数据驱动容器与递归子节点容器三条来源。

- `TreeView.item`：顶层容器是 `TreeView` 的直接逻辑子级，`>` 一步即可到达（与 `ListView` / `ListBox` 的
  `> .semantic-item` 同一模式）。
- `TreeViewItem.item`：子容器是当前 `TreeViewItem` 的直接逻辑子级，同样用 `>` 一步到达。递归 owner 模型保证任意
  深度的节点都命中同一个 `.semantic-item` 身份，而不是要求 `TreeView` 用宽泛 logical descendant 穿透多个容器层级。

`itemSwitcher`、`itemIndicator`、`itemIcon`、`itemTitle` 声明在 `TreeViewItem` owner 上，route 需要两次 `/template/`
跳点：先进入 `TreeViewItem` 模板命中 `TreeViewItemHeader#Header` 节点上的 `.semantic-scope-header` 跳点，再进入
`TreeViewItemHeader` 模板命中对应 marker。因为 owner 是 `TreeViewItem` 而不是 `TreeView`，这四个 route 对每个节点
（无论层级）都可达。`TreeViewItemHeader`、`NodeSwitcherButton` 与 `CheckBoxIndicator` 是 internal 类型，因此
`itemSwitcher` 与 `itemIndicator` 的 `ContractType` 使用其公开基类 `ToggleButton`（checkbox / radio 都继承自
`ToggleButton`）；`itemIcon` / `itemTitle` 分别使用公开的 `IconPresenter` / `ContentPresenter`。`.semantic-scope-header`
只用于路由与测试/工具定位，不单独发布为 Part。

`itemIndicator` 是 checkbox / radio 两个备选节点共用的单一 Part：`ToggleType=CheckBox` 时只显示
`CheckBox#ToggleCheckbox`，`ToggleType=Radio` 时只显示 `RadioButton#ToggleRadio`，`ToggleType=None` 时两者都隐藏。
两个节点都携带同一个 `.semantic-item-indicator` marker，因此每个容器恒有两个 marker 实例（对应两个备选节点），但
同一时刻最多一个可见。marker 身份不随 ToggleType、勾选状态或可见性切换增删。

marker 身份不随任何运行时状态切换增删：展开/收起、勾选、禁用、拖拽、过滤、leaf/loading 形态、容器复用/回收与
items 集合变化都不增删 marker；`IconEffectiveVisible`、`IsFilterMatch` 只切换目标节点可见性，节点与 marker 恒在。
过滤高亮模式下 `HeaderPresenter`（`itemTitle`）隐藏、`TextBlock#FilterHighlighter` 显示，但 `itemTitle` marker 仍保留
在 `HeaderPresenter` 上，`FilterHighlighter` 是过滤功能节点、不承载 Semantic Part marker。

## 6. 交互与事件处理

拖拽状态在发起流程中同步建立，外部拖拽事件回调返回后继续处理前确认拖拽仍有效。完成、失去捕获或控件卸载时统一释放节点状态、预览和指示器；卸载取消拖拽而不提交待定 drop。预览通过自身实际视觉父容器释放，不依赖已消失的 TreeView 根；完成通知在清理后发布，回调可以关闭窗口而不会重复清理或访问旧状态。

TreeView pointer 流程：

- `OnPointerPressed` 在可选择节点上更新选择；开启拖拽时记录起始点并阻止 gesture recognition。
- `DefaultTreeViewInteractionHandler.PointerReleased` 在 header bounds 内触发 click、context menu 和可选的右键选中。
- `UpdateSelectionFromEvent` 根据 `IsSelectOnRightClick` 决定右键是否进入 Avalonia 选择流程。

展开收起：

- SwitcherButton 通过 `IsChecked` 双向绑定到 `TreeViewItem.IsExpanded`。
- `TreeViewItem.HandleExpandedChanged` 捕获当前 owner、视口和请求版本，从 `OpenMotion` / `CloseMotion` 选择定义，通过可见性入口提交目标。
- `ContentViewport#PART_ChildrenViewport` 实现公开布局能力，内容保持自然尺寸排版。全量展开、路径回放和初始化可按该次请求关闭动效。
- 新展开意图连续替代当前动画；switcher 不因动画而忽略点击。完成后只有当前 owner、模板和目标均一致时才能执行自动展开祖先。
- 动效定义的 Duration 不被控件修改，实际时长通过目标的请求覆盖传入。模板替换和 detach 使旧完成失效；重新挂载按最新 `IsExpanded` 初始化。

键盘与焦点：

- TreeView 主题设置 `Focusable=True` 与空 `FocusAdorner`；`TreeViewItem`、`TreeViewItemHeader#Header`、`PART_NodeSwitcherButton`、`ToggleCheckbox` / `ToggleRadio` 固定 `Focusable=False`。指针按下节点时焦点落到 TreeView 自身。
- `TreeViewItem.IsKeyboardActive` 通过 `TemplateBinding` 投射到 `TreeViewItemHeader.IsKeyboardActive`，二者各自维护 `:keyboard-active` 伪类；默认主题在 header 的 hover 背景样式中合并 `:keyboard-active`，并排在 selected 样式之前，使 selected 优先。派生节点模板（例如 TreeSelect 候选树的 `TreeSelectTreeViewItemTheme`）替换整个 Template 时，必须同样把 `IsKeyboardActive` 投射到 header，并保持 header `Focusable=False`，否则节点有伪类而绘制层没有活动项背景。
- TreeView 重写 `OnKeyDown`，不调用 Avalonia `TreeView.OnKeyDown`：后者依赖把焦点移到节点容器，并在无选择时直接写 `SelectedItem`，与 AtomUI 的不可聚焦节点和 `IsSelectable` 约束冲突。平台 SelectAll 手势由 AtomUI 自己处理。
- TreeView 重新实现 Tab 导航的 `ICustomKeyboardNavigation` 钩子：Tab 进入时落在 TreeView 自身，从 TreeView 出发时交给默认 Tab 顺序。Avalonia 基类的实现会把 Tab 焦点导向节点容器，不可沿用。
- TreeView 重写 `OnGotFocus`，不走 Avalonia 基于节点焦点的方向导航选择更新。
- AtomUI `TreeViewItem` 重写 `OnKeyDown` 为空实现，不走 Avalonia 基于节点焦点的展开收起，按键冒泡到 TreeView；header 内用户可聚焦内容产生的按键，TreeView 不做导航处理。
- 键盘提交前检查 `TreeViewItem.IsHeaderEffectivelyEnabled`：宿主样式禁用 header（TreeSelect 的选择数量上限）时不提交。
- 宿主树（TreeSelect 下拉）设置 `Focusable=False`、`IsKeyboardMoveSelectionEnabled=False`，由 `TreeSelect.KeyboardNavigation.cs` 转发按键，关闭浮层时调用 `ClearKeyboardActiveItem`。
- 指针按下节点时更新活动项并隐藏活动项视觉；任意导航按键使其可见；失去焦点时隐藏，活动项保留。

上下文菜单：

- 右键 header 时可以先确保当前项被选中。
- 打开新上下文菜单前关闭 TreeView 或祖先 TreeViewItem 上仍打开的旧 ContextMenu。

拖拽：

- `OnPointerMoved` 超过拖拽阈值后创建 drag preview。
- 拖动中持续更新 drag-over 节点和 drop indicator。
- `OnPointerReleased` 或 pointer capture lost 完成拖拽并清理 preview、drag-over 和 indicator 状态。

## 7. 内部算法与关键流程

### 7.1 Path 遍历

`TraverseTreeViewPath` 是默认状态回放和路径操作的统一入口。它按 `TreeNodePath.Segments` 从 root 到 leaf 查找节点，支持以下选项：

- 关闭 motion。
- 展开命中节点。
- 操作完成后恢复原展开状态。
- 执行 layout pass 以确保子容器生成。

路径段匹配优先使用 `TreeViewItem.ItemKey.Value`，未命中时使用 `TreeViewItem.Value?.ToString()`。

### 7.2 默认状态回放

首帧前的默认路径回放复用选中集合和勾选级联逻辑。可解析的默认选中、勾选依次应用，其余路径由加载回放补齐。过滤和剩余展开状态在加载阶段处理。

ItemsSource 变化时，TreeView 在 Avalonia 清理旧容器之前捕获选中、勾选对象及运行期节点路径。Avalonia 的源重置可能逐项发出临时变化；TreeView 在回放事务中将中间 `SelectionChanged` 标记为已处理，暂缓 `CheckedItemsChanged` 与 Form 值通知。连续换源只回放最后一次源；调用方在换源后显式设置的选择或勾选优先于旧快照。同一事务内，可观察集合的连续增删更新最终路径，内部勾选级联不记录部分状态；显式赋值为普通 `IList` 后的同步修改在回放前重新读取。完成后按对象身份比较原状态与最终状态：未变则不通知，真实变化则分别发布一次包含移除与新增节点的选择或勾选事件；选择变化对应的 Form 值变更只通知一次。多选模式以 `SelectedItems` 路径为恢复权威，避免 `SelectedItem` 非空时折叠成单选；属性处理或回放抛错、控件脱离视觉树时清理事务状态。

### 7.3 勾选级联

`CheckedItems` 是勾选状态的权威。`TreeView` 另持一个按引用比较的镜像集合 `_checkedItemSet`：成员判断为常数时间，`Reset` 时据此得知此前勾选的项。`TreeItemNode` 是 record，`Equals` / `GetHashCode` 依赖可变状态，镜像与计划中的字典都必须用 `ReferenceEqualityComparer`。赋值为普通 `IList`（无变更通知）时，镜像在每个入口按列表重新读取。

一次勾选变化（交互、代码增删、`Reset`、容器准备时并入）先生成 `CheckedStatePlan`，再统一提交：

- 子节点来源：`TreeViewItem` 项（声明式树）取 `Items`；`ITreeItemNode` 取 `Children`；其他数据只能经已实现容器的 `Items` 访问，必要时临时展开该容器并在结束后恢复，这是唯一需要临时展开的情况。
- 父节点来源：已实现容器取逻辑父 `TreeViewItem`，否则取 `ITreeItemNode.ParentNode`。
- 可勾选判断：已实现容器取 `IsEnabled && IsIndicatorEnabled`，否则取节点数据的同名属性。递归谓词对已实现容器调用 `RecursiveCheckNodePredicate` / `RecursiveUnCheckNodePredicate`，对未实现节点调用私有保护的 `RecursiveCheckNodeDataPredicate` / `RecursiveUnCheckNodeDataPredicate`（`TransferTreeView` 据 `MaskKeys` 屏蔽）。
- 状态读取：计划中已有的项取计划值；否则在 `CheckedItems` 中为 true；不在时按容器或节点数据区分 null（半选）与 false。
- 取消勾选：被操作节点总是离开 `CheckedItems`；后代中非 false 且通过谓词的节点置 false。
- 父级更新：受影响节点的父级去重后按深度从深到浅各计算一次，遇到禁用父级停止向上。批量变化（`Reset`、多项增删）也只对每个父级算一次。
- 提交：`CommitCheckedStatePlan` 在 `CheckedItemsSyncScope` 与 `IsApplyingCheckedState` 内依次更新 `CheckedItems`（经 `AddCheckedItemCore` / `RemoveCheckedItemCore` 同步镜像）、写节点 `IsChecked`、投射到已实现容器（容器按一次性构建的“项 → 已实现容器”映射查找）。`IsApplyingCheckedState` 期间容器的 `IsChecked` 变化是投射，交互处理器直接返回，不再级联；提交期间关闭 TreeView 动效。

容器准备时，`PrepareTreeItemNodeData` 在 `ApplyNodeData` 之后调用 `ReconcilePreparedCheckedState`（仅 checkbox 模式）：节点在 `CheckedItems` 中则投射 true；节点数据为 true 而 `CheckedItems` 不含它时，按一次勾选并入（严格模式只加入该项）。`ApplyNodeData` 期间写入的 `IsChecked` 在 checkbox 模式下不触发交互处理器；其他模式下，尚无索引的容器同样不触发。

`CheckedItems` 的外部变化：`Add` / `Remove` / `Replace` 在 checkbox 级联模式下对可勾选项执行级联计划，其他模式只投射该项（容器变化照常经交互处理器，radio 由分组管理）。`Reset`（含赋值新集合）比较镜像：离开集合的项置 false，新进入的项在级联模式下勾选其子树，再更新两者的父级；此前从未进入 `CheckedItems` 的节点保留自身数据。

默认状态与运行期路径回放（`TryCheckNodePath`、`ConfigureDefaultCheckedPaths`）在目标容器已显示勾选时直接调用交互处理器，保证 `CheckedItems` 被清空后仍能恢复。

`CheckedItemsSyncScope` 防止内部批量更新 `CheckedItems` 时再次进入外部变化处理；作用域退出时恢复进入前的同步标记，可嵌套。

父级状态按所有有效子节点计算：

- 全部有效子节点 checked：父级 true。
- 部分有效子节点 checked 或 indeterminate：父级 null。
- 无有效子节点 checked：父级 false。

### 7.4 过滤和高亮

过滤采用后序遍历。先计算子节点是否命中，再处理当前节点，从而支持“当前未命中但后代命中时保留路径”的场景。

首次进入过滤模式时，TreeViewItem 保存 filter context backup。清除过滤时递归恢复可见性、展开状态、高亮文本和 match 状态。

`TreeViewItemHeader.BuildFilterHighlightRuns` 根据 `FilterStrategy` 构造 `InlineCollection`，将命中片段设置为高亮前景和可选加粗。

### 7.5 异步加载

`AsyncExpandLoadCoordinator` 以节点引用作为 key 合并同一节点的并发加载请求，并应用 `AsyncLoadTimeout`。加载成功后，返回的子节点会更新 parent node，再加入目标节点 `Children`。

detached 时必须调用 `CancelAll`，避免已离开视觉树的 TreeView 继续处理加载结果。

### 7.6 拖拽和 drop

拖拽命中包含三类 UI 查询：

- self-first 查询用于确认拖拽发起节点。
- child-first 查询用于当前 drag-over 节点。
- offset-y 查询用于计算最终 drop 位置。

drop indicator 根据目标 header 上半区、中间区域和下半区决定插入到前、插入到内部或插入到后。UI 查询只产出 `DropTargetInfo`，不直接修改节点结构。

结构修改由 `TreeDataController` 执行：

- 从 `TreeNodeIndex` 读取 source 节点所在父级、兄弟集合和原始索引。
- 根据 drop position 解析目标父级、目标兄弟集合和目标插入索引。
- 拒绝把节点移动到自身或自身后代。
- 拒绝不可变 root 集合或不可变 `Children` 集合，不通过捕获异常隐藏能力不支持。
- 同一集合内移动时，只有 `sourceIndex < targetIndex` 才修正目标索引，避免向前移动被错误偏移。
- 跨集合移动时先从源集合移除，再插入目标集合，并同步移动节点的 parent node。
- 移动完成后通过 collection changed 或控制器显式通知更新 `TreeNodeIndex`。
- 事件投射发生在数据结构移动成功之后；移动失败不派发 drop 完成事件。

拖拽结构重排不应触发删除式状态清理。`SelectedItem`、`SelectedItems`、`CheckedItems` 和展开状态以节点身份为准；只有节点不再属于当前 TreeView 数据树时，状态集合才清理该节点。

### 7.7 绑定型节点同步

绑定型节点只解决节点属性作为 Avalonia binding target 的场景，不替代轻量 `TreeItemNode`。

同步规则：

- `ContainerForItemPreparedOverride` 识别 `BindableTreeItemNode` 后，先 attach scoped resource host，再执行普通 `ITreeItemNode` 数据转接，并建立节点到当前 `TreeViewItem` 的属性同步。
- 节点到容器同步覆盖 `Icon`、`IsEnabled`、`IsChecked`、`IsSelected`、`IsExpanded`、`IsIndicatorEnabled`、`GroupName`、`Value`、`IsLeaf` 和 `ItemKey` 等当前容器状态。`Header` 保持 Avalonia TreeView 的数据项语义，容器 `Header` 仍是节点对象本身；显示内容通过 `TreeDataTemplate` 绑定 `node.Header` 更新。
- 容器交互导致的 checked、selected、expanded 状态变化应回写绑定型节点，避免节点状态和当前容器状态分叉。
- 同步绑定必须进入容器生命周期 disposable。`ClearContainerForItemOverride`、detach、re-template 或 container recycle 时释放，不允许让旧容器继续订阅节点。容器清理还必须释放 `TreeDataTemplate` 建立的 children binding，避免回收容器继续持有旧节点。
- 同一个节点被多个容器短暂使用时，resource host attach 必须使用 attachment count 或等价 scoped token，释放到 0 后再解除 owner 订阅。

普通节点（`TreeItemNode` 与自定义 `ITreeItemNode`）没有变更通知，只接收容器回写：

- `PrepareTreeItemNodeData` 在 `ApplyNodeData` 之后记录当前节点，`TreeViewItem.OnPropertyChanged` 把 `IsChecked`、`IsSelected`、`IsExpanded` 的变化写回节点（与绑定型节点共用 `SyncTreeItemNodeFromContainer`）。
- 回写只在 `ApplyNodeData` 之后开始，并在 `ClearPreparedTreeItemNodeData`（容器清理、logical detach）时先于 Avalonia 的容器清理停止；容器准备前后由 Avalonia 基类写入的选择标记不会写进新节点。
- 虚拟化面板回收的容器会带着上一个节点的状态被重新准备。`ApplyNodeData` 把节点状态写到容器上属于状态恢复，不是切换：checkbox 模式下由 `ReconcilePreparedCheckedState` 按 `CheckedItems` 对齐，其他模式下尚无索引的容器不触发交互处理器。

容器生命周期与数据删除的区分：

- 准备（`TreeView.PrepareContainerForItemOverride`，Avalonia `TreeViewItem` 会把嵌套容器的准备转发到这里）、清理（`TreeView` / `TreeViewItem.ClearContainerForItemOverride`、`ClearPreparedTreeItemNodeData`）与逻辑树 attach / detach 在 `BeginContainerLifecycle` 作用域内执行（结构体作用域，滚动时不分配）。这些路径替换或清空容器的 `ItemsSource`，不代表节点离开数据树。
- `ItemsSource` 变化时，Avalonia 在 `ItemsControl.OnPropertyChanged` 内把旧子项作为移除或 `Reset` 发出；`TreeViewItem` 在此期间忽略 `Items` 变化，由变化本身判断：生命周期内忽略；生命周期外表示节点的子集合被替换（如给 `BindableTreeItemNode.Children` 赋新集合），旧集合中不在新集合里的节点连同子树离开 `CheckedItems`。
- 子集合自身的移除 / 替换只把该项移出 `CheckedItems`（拖拽移动中的节点保留），不改写其节点数据；子集合 `Reset` 时移除从根已不可达的勾选项（逐层检查 `ParentNode.Children` 是否仍包含该节点），无节点模型的数据无法判断，保留。
- 根集合 `Reset`（含换源）在同步作用域内清空 `CheckedItems`，不把变化投射到已离开的旧数据；运行期状态由 ItemsSource 回放恢复。

### 7.8 键盘活动项维护

活动项以容器引用加 active 路径（从顶层起每层的索引和项）保存，路径在设置活动项时重建。只在以下路径更新：键盘导航、指针按下节点、祖先收起（`TreeViewItem` 的 `IsExpanded` 变为 false 时通知 TreeView，立即上移到被收起的节点）、按键前校验（被过滤隐藏或禁用时修复为最近的可导航祖先）、容器离开视觉树（`TreeViewItem.OnDetachedFromVisualTree` 通知 TreeView：离开的容器是活动项时释放容器引用和指示状态、保留路径）、容器准备（`TreeView` / `TreeViewItem.ContainerForItemPreparedOverride` 通知 TreeView，准备的项是路径末端且当前没有活动容器时重新绑定）、TreeView detach 与宿主关闭浮层（清空容器与路径）。

按键前若活动容器已被回收（离开视觉树，或作为自身容器被隐藏而不再有索引），沿路径逐层重新实现容器并校验每层的项；不一致时按无效处理。

遍历按索引取容器。没有容器时先对所属 `ItemsControl` 执行一次布局（刚展开节点的子容器），仍没有时调用 `ScrollIntoView(index)` 让虚拟化面板实现它；仍无容器或无可导航子节点时按“没有”处理，不等待、不轮询。非虚拟化面板不会进入这两步。

选择锚点保存为项，由指针按下、单选跟随、Enter / Space 提交和范围选择设置，只在清除活动项时清空，不随容器离开视觉树清空。`EnumerateNavigableTreeItems` 按先序产生可导航的项：有容器时用容器状态，没有容器时用自身容器 `TreeViewItem` 或 `ITreeItemNode` 的状态；范围选择在这个序列上按引用定位锚点和目标，并只把差量同步到 `SelectedItems`。

`SelectedItems` 是选择状态的权威。`TreeView.SelectionState.cs` 在 attach 时订阅 `SelectedItems` 的集合变化（`SelectedItems` 被替换时改订新集合），把增删同步到节点的 `IsSelected`，并按引用维护已同步节点的镜像；Reset / Move 或重新 attach 时按镜像求差。自身是容器的 `TreeViewItem` 由 Avalonia 直接标记，不经过镜像。这样未实现节点在选择变化后被实现时，`ApplyNodeData` 写入的就是当前选择。detach 时退订，避免应用持有的 `SelectedItems` 集合延长 TreeView 的生命周期。

## 8. 资源、性能与 AOT 边界

TreeView 不通过反射访问模板内部结构。模板接入依赖稳定 part 名称和 Avalonia 属性绑定。

`BindableTreeItemNode` 属于 owner-managed 非 Visual `AvaloniaObject`。它承载 `DynamicResource`、token-resource binding 或 XAML binding target 时，必须按 [Scoped Resource Host Source Generator 范式](../../../../modules/generator/scoped-resource-host-generator.md) 生成 scoped `IResourceHost` / `IThemeVariantHost` 生命周期样板代码。资源查找顺序必须是 owner TreeView / TreeViewItem 优先，再 fallback 到 `Application.Current`。

TreeView / TreeViewItem 对绑定型节点的 attach/release 是资源生命周期边界。不得把 generated `TreeViewItem`、header、container 或 Gallery ShowCase 永久挂回节点，也不得用清空 DataContext 或静态资源替代动态资源能力。

过滤和默认状态回放会触发 layout pass 以实现需要访问的子容器；勾选级联沿节点数据计算，只有无节点模型的数据需要临时展开容器。这些路径必须保持有界，不能引入固定延时等待。

勾选计划的开销为受影响子树大小加上每个受影响父级的子节点数；“项 → 已实现容器”映射每次计划构建一次，与已实现容器数成正比。容器准备时的对齐只做一次镜像查找，节点数据未勾选时不做额外工作。

拖拽 preview 使用 `AdornerLayer`，完成或取消拖拽时必须从 adorner layer 移除。drag indicator Pen 和 tree line Pen 均按 brush / width 缓存，避免每帧重复创建。

拖拽性能边界：

- pointer move 期间只查询已实现容器和 header bounds，不访问整棵数据树。
- drop 阶段通过 `TreeNodeIndex` 读取节点上下文，避免按节点引用全树扫描。
- `TreeNodeIndex` 只在 root 数据源替换、节点集合增删移和异步加载结果进入时增量维护。
- 移动的主要成本限制在源 / 目标兄弟集合的 remove / insert；不能引入与整棵树节点数线性相关的 drop 热路径。
- 索引订阅必须随 root source 替换、节点移除、TreeView detach 和节点 collection 替换释放，避免保留旧节点树。

异步加载必须支持取消、超时和 detach 清理。加载结果回到 UI 线程后才能修改节点集合和容器状态。

键盘导航不引入 binding、定时器、缓存或额外事件订阅；活动项是单一容器引用，每次按键最多触发一次局部布局。

Filter highlight runs 是 header 状态，不应写入 Token 或节点数据模型。Token 只提供默认颜色、尺寸和间距。

## 9. 维护不变量

内部重构必须保持以下不变量：

- public API、事件、Avalonia 属性字段和 CLR wrapper 不擅自变更。
- `TreeView.cs` 保留公共属性、事件、公共方法、生命周期和接口入口；复杂内部逻辑可继续按功能拆分 partial。
- `TraverseTreeViewPath` 是路径回放和路径操作的统一入口。
- 默认状态回放顺序保持 selected、checked、filter、expanded。
- ItemsSource 变化先尝试恢复运行期状态，再回退默认状态。
- 多选模式的 ItemsSource 变化恢复必须以 `SelectedItems` 为权威；不能因为 `SelectedItem` 非空而丢弃其它已选节点。
- `CheckedItemsSyncScope` 必须包裹内部批量 checked 集合更新；内部增删必须经 `AddCheckedItemCore` / `RemoveCheckedItemCore` 以保持引用镜像一致。
- 勾选级联与父级计算不得依赖容器是否已实现；容器只投射 `CheckedItems` / 节点数据，不作为勾选状态来源。
- 容器准备、清理和逻辑树 attach / detach 引起的 `ItemsSource` 变化必须处于 `BeginContainerLifecycle` 作用域，不能被当成数据删除清理 `CheckedItems`。
- filter 进入时备份上下文，退出时恢复。
- `TreeViewItemHeader` 替换 `PART_HeaderContentFrame` 时必须解除旧 pointer 事件。
- `DefaultTreeViewInteractionHandler.Detach` 必须释放 pointer、input manager、root handler 和 radio group 关系。
- `NodeSwitcherButton.Toggle` 在节点加载中不重复触发展开。
- 键盘与指针的展开、异步加载、选择和勾选必须经过同一入口；`TreeViewItem` 展开请求入口是 switcher 和键盘共用的唯一展开路径。
- 节点容器、header、switcher 和 indicator 保持不可聚焦；TreeView 不得调用 Avalonia `TreeView.OnKeyDown` 或沿用基类 Tab 导航钩子。
- 活动项不能保留已清理或已回收的容器；`IsSelectable=false` 时键盘路径不写入 `IsSelected`。
- drag preview、drag-over、drop target 和 indicator 状态必须在拖拽完成或取消时清理。
- 拖拽结构修改只能通过 `TreeDataController` 执行，不能直接写生成容器 `Items`。
- `TreeNodeIndex` 是 drop 定位的权威索引；drag pointer move 不得触发全树数据遍历。
- 节点 move 不能被当成 remove 清理选中、勾选或展开状态。
- root 数据源、节点 `Children`、parent node 和索引必须在移动后保持一致。
- `TreeItemNode` 保持轻量数据节点定位，不承载 Avalonia 属性系统；它与自定义 `ITreeItemNode` 一样接收容器状态回写，节点数据是容器回收后恢复状态的来源。
- `BindableTreeItemNode` 的 resource host attach、属性订阅和容器同步必须与容器生命周期成对释放。
- 绑定型节点不能永久保存当前 `TreeViewItem`、header、template part 或 visual container。
- Semantic Part 的 marker 放置（`TreeViewItemTheme.axaml` 的 `semantic-scope-header` 静态跳点、
  `TreeViewItemHeaderTheme.axaml` 的四个静态 Part marker、容器创建/prepare 路径的 `.semantic-item`）属于维护不变量：
  状态切换、容器复用/回收、items 集合变化与模板重应用不得增删 marker，默认主题不得消费 `.semantic-*` selector。
- `TreeViewItemHeader` / `NodeSwitcherButton` 不得改为 public owner 或承载独立 Semantic descriptor；`itemSwitcher` 与
  `itemIndicator` 的 `ContractType` 保持公开基类 `ToggleButton`，不把 internal `NodeSwitcherButton` /
  `CheckBoxIndicator` 泄漏进公共契约。

## 10. 测试与验证

验证范围：

- 临时验收覆盖 默认 selected / checked / expanded 回放顺序、`IsDefaultExpandAll` 优先级、ItemsSource 变化状态恢复、多选恢复优先级、strict / cascading checked。
- 临时验收覆盖 template 替换时旧 `PART_HeaderContentFrame` pointer 事件释放。
- 选择：单选、多选、`IsSelectable=false`、右键选择开关和 Form value 读写。
- 勾选：checkbox 级联、strict、半选父级、radio group、`CheckedItemsChanged` added / removed。
- 过滤：匹配、高亮、加粗、隐藏未命中、展开路径、清除过滤和空状态。
- 异步加载：加载成功、超时、取消、重复请求合并、detach 取消。
- 临时验收覆盖键盘导航的遍历与跳过规则、按键语义与 RTL、提交策略矩阵、异步加载、活动项生命周期、Tab 单停靠点、来源过滤和宿主转发，并覆盖 TreeSelect 的消费路径。要求见 [keyboard-navigation-design.md §10](keyboard-navigation-design.md#10-验证要求)。
- 拖拽：drag preview、drop indicator、数据源 root 插入、子节点插入、同集合索引修正、跨父级 parent node 同步、不可变集合取消、自身后代保护、状态保留和事件顺序。
- Theme：template part、hover mode、selected / disabled、line rendering、switcher icons、drag indicator 和 filter highlighter。
- 绑定型节点：节点属性变化同步当前容器、容器交互回写节点、DynamicResource 不 root 已移除节点、owner resource 优先于 Application resource。
- Semantic Part：覆盖两个 descriptor 的 Part 数量、顺序与字段（`TreeView` 为 `root`/`item`，`TreeViewItem` 为
  `root`/`item`/`itemSwitcher`/`itemIndicator`/`itemIcon`/`itemTitle`，非 root Part 为 `Multiple`、`RuntimeCreated=true`
  且携带显式 SelectorRoute）、`TreeView` 顶层 route 与 `TreeViewItem` 递归 route 命中、marker 数量与类型、容器回收后的
  marker 身份，见 [semantic-part.md §8](semantic-part.md#8-兼容性与验证)。
- 文档改动运行 `git diff --check`。
