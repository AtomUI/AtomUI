# TreeView 桌面版实现原理

本文档描述 TreeView 桌面版的内部源码结构、状态流转、容器生命周期、默认状态回放、过滤、勾选、异步加载和拖拽实现。公共设计与 API 契约见 [TreeView 桌面版架构设计](overview.md)，Semantic Part 契约见 [TreeView Semantic Part 契约](semantic-part.md)，Token 语义见 [TreeView Token 设计](token.md)，变化记录见 [TreeView Changelog](changelog.md)。

Popup 接入边界：`TreeView` 负责业务状态和内容准备，`TreeViewFlyout` 仅作为 relay 适配层，tree Flyout Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

TreeView 的实现目标是在 Avalonia `TreeView` 基类上增加 AtomUI 树形数据展示能力，并保持 API、行为和主题契约稳定。实现文档覆盖 `TreeView`、`TreeViewItem`、`TreeViewItemHeader`、`NodeSwitcherButton`、节点数据模型、interaction handler、过滤、拖拽、异步加载和主题接入。

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
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.CheckedState.cs`：checkbox 严格 / 级联状态、`CheckedItems` 同步和半选父级计算。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.Filter.cs`：过滤、高亮、隐藏未命中、展开路径、filter context 备份和恢复。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.AsyncItemDataLoad.cs`：异步加载、加载合并、超时、取消和 `TreeItemLoaded` 派发。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.DragAndDrop.cs`：拖拽命中测试、drag preview、drop indicator、drop 操作和拖拽事件。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.DataController.cs`：内部数据控制器、drop request / result、root / children 集合移动和 parent node 同步。
- `src/AtomUI.Desktop.Controls/TreeView/TreeView.NodeIndex.cs`：节点索引、节点父级、所在集合、兄弟索引和 collection changed 增量维护。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewItem.cs`：节点容器、展开收起、line 渲染、check/radio 状态、drag bounds 和数据节点承载。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewItemHeader.cs`：header 视觉状态、pointer 状态、switcher mode、filter highlight runs 和 template part 订阅。
- `src/AtomUI.Desktop.Controls/TreeView/NodeSwitcherButton.cs`：switcher 当前图标选择、默认图标、loading load request 和 rotation 动效。
- `src/AtomUI.Desktop.Controls/TreeView/TreeItemNode.cs`、`ITreeItemNode.cs`：轻量数据节点模型、节点契约和父子关系维护。绑定型节点模型属于同一节点数据职责边界，必须保持独立类型，不改造 `TreeItemNode`。
- `src/AtomUI.Desktop.Controls/TreeView/DefaultTreeViewInteractionHandler.cs`：pointer、context menu、radio group、checked changed 和 floating tree light dismiss。
- `src/AtomUI.Desktop.Controls/TreeView/TreeViewToken.cs`：TreeView Token 定义。
- `src/AtomUI.Desktop.Controls/TreeView/Themes/`：TreeView、TreeViewItem、TreeViewItemHeader、NodeSwitcherButton 主题和模板常量。

## 3. 核心类职责

`TreeView` 是树根控件，负责 public API、ItemsControl 容器生成、默认状态回放、选择、勾选集合、过滤入口、拖拽状态、异步加载入口、空状态和 Form 集成。

`TreeViewItem` 是节点容器，负责节点 icon、checked、loading、leaf、value、line render、展开收起动效和 drag bounds。它实现 `ITreeItemNode`，使直接 `TreeViewItem` 和数据节点场景能共享路径和层级逻辑。

`TreeViewItemHeader` 是节点 header 的视觉和交互边界，承载 switcher、checkbox / radio、icon、内容、filter highlighter、hover、pressed、selected、drag 和 disabled 状态。

`NodeSwitcherButton` 是展开、收起、loading 和 leaf 图标切换入口。它在未加载节点上触发 `NodeLoadRequestEvent`，由 TreeView 接管异步加载。

`DefaultTreeViewInteractionHandler` 处理 TreeView 级 pointer、右键上下文菜单、radio group、checked changed 和浮层关闭逻辑，使节点容器不直接持有全局输入订阅。

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
  BindableTreeItemNode attach resource host + property sync
  ItemTemplate -> HeaderTemplate
  switcher icons
  filter / motion / hover / line / toggle / selectable state bindings
```

默认状态回放流：

```text
OnLoaded
  ConfigureDefaultSelectedPaths
  ConfigureDefaultCheckedPaths
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
TreeViewItem.IsChecked changed
      ↓
DefaultTreeViewInteractionHandler.OnCheckedChanged
      ↓
Radio: RadioButtonGroupManager
CheckBox strict: sync current item only
CheckBox cascading: check / uncheck subtree + update parents
      ↓
CheckedItems + CheckedItemsChanged
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
NodeSwitcherButton.Toggle
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

`TreeViewItem.OnApplyTemplate` 获取 `Header` 和 `PART_ItemsPresenterMotionActor`，配置叶子状态，并按当前 `IsExpanded` 同步子项实际可见状态。

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
                 └─ LayoutAwareMotionActor#PART_ItemsPresenterMotionActor（非 Part）
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
- `TreeViewItem.HandleExpandedChanged` 进入 `ExpandChildren` 或 `CollapseChildren`。
- 展开收起通过 `LayoutAwareMotionActor` 承载；全量展开、路径回放和内部状态同步可临时关闭 motion。

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

默认状态回放按 selected、checked、filter、expanded 的顺序执行。这样选中和勾选可以在过滤和展开之前完成基础状态同步，最后由 `IsDefaultExpandAll` 或 `DefaultExpandedPaths` 决定可见展开结构。

ItemsSource 变化时，TreeView 在 Avalonia 清理旧容器之前捕获选中、勾选对象及运行期节点路径。Avalonia 的源重置可能逐项发出临时变化；TreeView 在回放事务中将中间 `SelectionChanged` 标记为已处理，暂缓 `CheckedItemsChanged` 与 Form 值通知。连续换源只回放最后一次源；调用方在换源后显式设置的选择或勾选优先于旧快照。同一事务内，可观察集合的连续增删更新最终路径，内部勾选级联不记录部分状态；显式赋值为普通 `IList` 后的同步修改在回放前重新读取。完成后按对象身份比较原状态与最终状态：未变则不通知，真实变化则分别发布一次包含移除与新增节点的选择或勾选事件；选择变化对应的 Form 值变更只通知一次。多选模式以 `SelectedItems` 路径为恢复权威，避免 `SelectedItem` 非空时折叠成单选；属性处理或回放抛错、控件脱离视觉树时清理事务状态。

### 7.3 勾选级联

级联勾选需要临时展开未实现子树以确保子容器可访问，并在完成后恢复原展开状态。`CheckedItemsSyncScope` 用于防止内部批量更新 `CheckedItems` 时递归触发容器同步和重复事件。

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

## 8. 资源、性能与 AOT 边界

TreeView 不通过反射访问模板内部结构。模板接入依赖稳定 part 名称和 Avalonia 属性绑定。

`TreeViewToken` 通过 `IControlTokenDefaults` 的生成实现接入 `DependencyDefaultsV2`。12 个静态默认函数只读取 `ControlTokenDefaultContext.Global`，资源投影继续使用原字段类型和标识；没有运行时反射发现或 Own→Own 求值边。明确 Own 覆盖及撤回由通用求值器处理，TreeView 不持有额外缓存、作用域订阅或节点引用。字段语义与兼容保留项见 [Token 设计](token.md)。

`BindableTreeItemNode` 属于 owner-managed 非 Visual `AvaloniaObject`。它承载 `DynamicResource`、token-resource binding 或 XAML binding target 时，必须按 [Scoped Resource Host Source Generator 范式](../../../../modules/generator/scoped-resource-host-generator.md) 生成 scoped `IResourceHost` / `IThemeVariantHost` 生命周期样板代码。资源查找顺序必须是 owner TreeView / TreeViewItem 优先，再 fallback 到 `Application.Current`。

TreeView / TreeViewItem 对绑定型节点的 attach/release 是资源生命周期边界。不得把 generated `TreeViewItem`、header、container 或 Gallery ShowCase 永久挂回节点，也不得用清空 DataContext 或静态资源替代动态资源能力。

过滤、默认状态回放和勾选级联会触发 layout pass 以实现需要访问的子容器；这些路径必须保持有界，不能引入固定延时等待。

拖拽 preview 使用 `AdornerLayer`，完成或取消拖拽时必须从 adorner layer 移除。drag indicator Pen 和 tree line Pen 均按 brush / width 缓存，避免每帧重复创建。

拖拽性能边界：

- pointer move 期间只查询已实现容器和 header bounds，不访问整棵数据树。
- drop 阶段通过 `TreeNodeIndex` 读取节点上下文，避免按节点引用全树扫描。
- `TreeNodeIndex` 只在 root 数据源替换、节点集合增删移和异步加载结果进入时增量维护。
- 移动的主要成本限制在源 / 目标兄弟集合的 remove / insert；不能引入与整棵树节点数线性相关的 drop 热路径。
- 索引订阅必须随 root source 替换、节点移除、TreeView detach 和节点 collection 替换释放，避免保留旧节点树。

异步加载必须支持取消、超时和 detach 清理。加载结果回到 UI 线程后才能修改节点集合和容器状态。

Filter highlight runs 是 header 状态，不应写入 Token 或节点数据模型。Token 只提供默认颜色、尺寸和间距。

## 9. 维护不变量

内部重构必须保持以下不变量：

- public API、事件、Avalonia 属性字段和 CLR wrapper 不擅自变更。
- `TreeView.cs` 保留公共属性、事件、公共方法、生命周期和接口入口；复杂内部逻辑可继续按功能拆分 partial。
- `TraverseTreeViewPath` 是路径回放和路径操作的统一入口。
- 默认状态回放顺序保持 selected、checked、filter、expanded。
- ItemsSource 变化先尝试恢复运行期状态，再回退默认状态。
- 多选模式的 ItemsSource 变化恢复必须以 `SelectedItems` 为权威；不能因为 `SelectedItem` 非空而丢弃其它已选节点。
- `CheckedItemsSyncScope` 必须包裹内部批量 checked 集合更新。
- filter 进入时备份上下文，退出时恢复。
- `TreeViewItemHeader` 替换 `PART_HeaderContentFrame` 时必须解除旧 pointer 事件。
- `DefaultTreeViewInteractionHandler.Detach` 必须释放 pointer、input manager、root handler 和 radio group 关系。
- `NodeSwitcherButton.Toggle` 在节点加载中不重复触发展开。
- drag preview、drag-over、drop target 和 indicator 状态必须在拖拽完成或取消时清理。
- 拖拽结构修改只能通过 `TreeDataController` 执行，不能直接写生成容器 `Items`。
- `TreeNodeIndex` 是 drop 定位的权威索引；drag pointer move 不得触发全树数据遍历。
- 节点 move 不能被当成 remove 清理选中、勾选或展开状态。
- root 数据源、节点 `Children`、parent node 和索引必须在移动后保持一致。
- `TreeItemNode` 保持轻量数据节点定位，不承载 Avalonia 属性系统。
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

- `TreeViewStateReplayTests`：默认 selected / checked / expanded 回放顺序、`IsDefaultExpandAll` 优先级、ItemsSource 变化状态恢复、多选恢复优先级、strict / cascading checked。
- `TreeViewItemHeaderLifecycleTests`：template 替换时旧 `PART_HeaderContentFrame` pointer 事件释放。
- 选择：单选、多选、`IsSelectable=false`、右键选择开关和 Form value 读写。
- 勾选：checkbox 级联、strict、半选父级、radio group、`CheckedItemsChanged` added / removed。
- 过滤：匹配、高亮、加粗、隐藏未命中、展开路径、清除过滤和空状态。
- 异步加载：加载成功、超时、取消、重复请求合并、detach 取消。
- 拖拽：drag preview、drop indicator、数据源 root 插入、子节点插入、同集合索引修正、跨父级 parent node 同步、不可变集合取消、自身后代保护、状态保留和事件顺序。
- Theme：template part、hover mode、selected / disabled、line rendering、switcher icons、drag indicator 和 filter highlighter。
- 绑定型节点：节点属性变化同步当前容器、容器交互回写节点、DynamicResource 不 root 已移除节点、owner resource 优先于 Application resource。
- Semantic Part：覆盖两个 descriptor 的 Part 数量、顺序与字段（`TreeView` 为 `root`/`item`，`TreeViewItem` 为
  `root`/`item`/`itemSwitcher`/`itemIndicator`/`itemIcon`/`itemTitle`，非 root Part 为 `Multiple`、`RuntimeCreated=true`
  且携带显式 SelectorRoute）、`TreeView` 顶层 route 与 `TreeViewItem` 递归 route 命中、marker 数量与类型、容器回收后的
  marker 身份，见 [semantic-part.md §8](semantic-part.md#8-兼容性与验证)。
- 文档改动运行 `git diff --check`。
