# TreeSelect 桌面版实现原理

本文档描述 TreeSelect 桌面版的输入壳体、树数据集合、选择同步、过滤、候选弹层、Form 和 Token 资源边界。共享输入分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，公共设计与 API 契约见 [TreeSelect 桌面版架构设计](overview.md)，Token 语义见 [TreeSelect Token 设计](token.md)，变化记录见 [TreeSelect Changelog](changelog.md)。

Popup 接入边界：`AbstractSelect` 负责业务状态和内容准备，`PART_Popup` 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

TreeSelect 的实现以 `AbstractSelect` 为选择与弹层基类，输入表面复用 `InputControlFrame` / `AddOnDecoratedBox`，`TreeSelect` 本体负责树数据、树选择、过滤输入和结果展示协调。树节点容器、展开、勾选和异步加载由 TreeView 家族承担。本文档聚焦 TreeSelect 自身的状态编排、生命周期和维护边界，不重新说明 TreeView 的通用容器回放、shared frame 的输入外观或 PopupHost 的全局资源规则。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/TreeSelect/TreeSelect.cs`：public TreeSelect API、生命周期、弹层树懒创建、选择同步、过滤输入、Form 映射和私有算法。
- `src/AtomUI.Desktop.Controls/TreeSelect/TreeSelect.SemanticParts.cs`：TreeSelect 的 13 个 Semantic Part 声明（见 [TreeSelect Semantic Part 契约](semantic-part.md)）。
- `src/AtomUI.Desktop.Controls/TreeSelect/TreeSelectAddOnDecoratedBox.cs`：TreeSelect 输入布局扩展，复用 `InputControlFrame` 并承载多选和选择空状态。
- `src/AtomUI.Desktop.Controls/TreeSelect/TreeSelectTreeView.cs`：TreeSelect 候选树，使用 TreeView 样式键并创建专用容器。
- `src/AtomUI.Desktop.Controls/TreeSelect/TreeViewSelectTreeViewItem.cs`：候选树节点容器，承接最大选择数状态。
- `src/AtomUI.Desktop.Controls/TreeSelect/Converters/*`：树节点显示转换辅助。
- `src/AtomUI.Desktop.Controls/TreeSelect/TreeSelectToken.cs`：TreeSelect 控件 Token。
- `src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml`：根模板、输入壳体、右侧内容、结果区域、popup 和 selector。
- `src/AtomUI.Desktop.Controls/Tooltip/OverflowTip.cs`：共享溢出 tooltip attached behavior，供单选结果和多选 tag 复用。

## 3. 核心类职责

`TreeSelect` 是树形选择协调器。它维护 `Items`、`ItemsSource`、`SelectedItem`、`SelectedItems`、`EffectiveSelectedItems`、过滤值、勾选策略和 popup 树内容同步。

`TreeSelectAddOnDecoratedBox` 是 TreeSelect 输入布局扩展。它复用 `InputControlFrame` 的 variant、effective status、Addon、CompactSpace 和尺寸能力，并额外承载 `IsMultiple`、`IsSelectionEmpty` 和 `IsDropDownOpen`；不重新定义输入表面状态优先级。

`TreeSelectTreeView` 是候选树边界。它继承 AtomUI `TreeView`，复用 TreeView 的视觉和交互能力，并把 `IsMaxSelectReached` 转发到 `TreeViewSelectTreeViewItem`。

`TreeViewSelectTreeViewItem` 是候选节点容器。它继承 TreeViewItem，并把最大选择数状态继续传给子节点容器，保证层级节点状态一致。

`OverflowTip` 不参与 TreeSelect 选择同步。TreeSelect 模板把单选 `SelectFilterTextBox.Text`、多选 `SelectTagAwareTextBox` 创建的 tag 文本以及 `OverflowTipDelay` / `OverflowTipPlacement` 接入该 behavior；单选显示节点以外层 `TreeSelectAddOnDecoratedBox` 作为 `PlacementTarget`，避免 tooltip 左边按内部文本 padding 对齐；它只在展示文本溢出时写入自己托管的 `ToolTip.Tip`，用户显式 tooltip 保持最高优先级。

## 4. 状态与数据流

初始化流：

```text
TreeSelect()
  → RegisterTokenResourceScope(TreeSelectToken)
  → Items.CollectionChanged += HandleItemsChanged
OnInitialized()
  → if Filter == null, use contains filter
  → ConfigureMaxSelectReached()
```

数据源流：

```text
ItemsSource changed
  → remember current selected node path
  → Items.SetItemsSource(new source)
  → remap selected node by ItemKey or Value path
  → clear selection when path cannot be resolved
```

选择同步流：

```text
TreeSelectTreeView.SelectionChanged / CheckedItemsChanged
  → SelectedItem or SelectedItems
  → NotifyFormValueChanged
  → SelectedCount / placeholder / max count / effective items
  → sync back to TreeView when public selection changes

SelectedItems ObservableCollection.CollectionChanged
  → NotifyFormValueChanged
  → refresh placeholder / SelectedCount / EffectiveSelectedItems
  → sync TreeView.SelectedItems or TreeView.CheckedItems
  → refresh max count state
```

TreeSelect 比较 owner 与 popup 的选择集合时按节点实例引用判等。`ItemKey` / `Value` 用于换源重映射；重映射后即使新旧节点的值相等，Form 值也必须持有新数据源的节点实例。

过滤流：

```text
SelectFilterTextBox.TextChanged
  → trim text
  → FilterValue = text or null
  → TreeView receives FilterValue / Filter / FilterStrategy
  → TreeView applies highlight, bold, expand path and hide unmatched strategy
```

Form 流：

```text
Form.SetValue(object?)
  → Single: SelectedItem
  → Multiple / checkable: SelectedItems
Form.GetValue()
  → Single: SelectedItem
  → Multiple / checkable: SelectedItems
Form.ClearValue()
  → clear current selection
DataValidationErrors
  → InputControlFrame.EffectiveStatus
ValidateStatus
  → FormStatus → InputControlFrame + feedback
FeedbackControl
  → FormFeedback → SelectHandle
```

## 5. 生命周期与模板接入

`AbstractSelect.OnApplyTemplate()` 负责稳定 popup part 接入、弹层最大高度计算、popup opened / closed 订阅和输入壳体基础 part 获取。

`TreeSelect.OnApplyTemplate()` 在基类接入前调用 `ClearPopupContent()`，确保旧 `TreeSelectTreeView`、popup frame 和事件订阅被释放。基类接入后，TreeSelect 获取 `PART_SingleFilterInput`，设置 `Popup.OverlayInputPassThroughElement`，并重新配置选择空状态、伪类、过滤输入、placeholder、单选结果、右侧 handle sibling 状态 binding 和已打开 popup 内容。

候选弹层内容是懒创建：

```text
OpenDropDown()
  → EnsurePopupContent()
  → create PopupFrame if null
  → create TreeSelectTreeView if null
  → bind tree view to TreeSelect properties
  → subscribe SelectionChanged / CheckedItemsChanged
  → Popup.Child = PopupFrame
```

`ClearPopupContent()` 必须解除 `_treeView.SelectionChanged` 和 `_treeView.CheckedItemsChanged`，清空 `ItemsSource`，移除 `TemplatedParent`，断开 `PopupFrame.Child` 和 `Popup.Child`，并清空 `_treeView` / `_popupFrame`。

## 6. 交互与事件处理

指针路径：

- popup 内部点击不关闭弹层。
- 多选 tag 的关闭按钮点击不触发外层打开/关闭切换。
- 控件按下时设置标准 `:pressed`，释放时根据状态打开或关闭弹层。
- 单选模式中点击树节点后关闭弹层。

搜索输入路径：

- 单选且 popup 打开并启用过滤时，`PART_SingleFilterInput` 可编辑。
- 单选结果展示态下，过滤输入只读并显示当前选中节点文本。
- 多选模式下，单选过滤输入不参与编辑，结果由 `SelectedItemsBox` 承载。

tag 关闭路径：

```text
SelectTag.Closed
  → locate ITreeItemNode from tag.Item
  → remove node and descendants from copied SelectedItems
  → assign SelectedItems copy
```

## 7. 内部算法与关键流程

### 7.1 节点路径重映射

`ItemsSource` 替换前，TreeSelect 将当前选择转换为 `TreeNodePath`。路径每一段优先使用节点 `ItemKey`，没有 `ItemKey` 时使用 `Value`。新数据源写入 `Items` 后，控件沿路径逐层查找新节点实例，并把选择映射到新实例。
弹层 TreeView 在自身换源事务结束后只发布最终选择与勾选差异；TreeSelect 完成选择重映射后，把 owner 的最终选择和勾选状态交给尚未结束的 TreeView 回放事务，避免逐项同步时的中间状态覆盖完整选择。TreeSelect 因此不会把容器重建时的临时移除和选回写入 public selection 或重复触发 Form 值变更。
弹层关闭并脱离视觉树后，其 TreeView selection / checked 事件不再作为输入处理。TreeSelect 保留自己拥有的选择，并在弹层重新打开时同步到候选树，避免隐藏树换源覆盖 Form 值。

### 7.2 选择同步抑制

TreeSelect 有两个内部同步状态：

- `_needSkipSyncSelection`：TreeView 事件正在写入 public selection 时，避免 public selection 再回写 TreeView。
- `_needSkipCollectionChangedEvent`：多选 selection 回写 TreeView.SelectedItems 时，避免集合事件再次写回 public selection。

这两个状态只用于确定性的同步闭环，必须通过 `try/finally` 成对进入和退出。维护时不能把它们扩展成通用“忽略下一次事件”的补丁开关。

### 7.3 勾选展示集合

`BuildEffectiveSelectedItems()` 根据 `ShowCheckedStrategy` 从真实 `SelectedItems` 派生展示集合：

- `All` 直接展示 `SelectedItems`。
- `ShowParent` 检测父节点子节点是否全部选中，并隐藏已展示父节点下的子节点。
- `ShowChild` 只展示叶子节点。

派生结果写入 internal `EffectiveSelectedItems`，供模板中的 `SelectedItemsBox` 展示。

### 7.4 最大选择数

`ConfigureMaxSelectReached()` 在 `IsMultiple=true` 且 `SelectedItems.Count >= MaxCount` 时设置 `IsMaxSelectReached=true`。该状态通过 `TreeSelectTreeView` 传给候选节点容器，用于阻止继续选择未选节点，同时保留取消已选节点的路径。

### 7.5 弹层尺寸

弹层最大高度由 `AbstractSelect` 根据 `ItemHeight`、`DisplayPageSize` 和 `PopupContentPadding` 计算。TreeSelect 自身通过 `TreeSelectToken.MinPopupWidth` 给候选弹层提供下限宽度，并在 `IsPopupMatchSelectWidth=true` 时继承输入控件当前宽度作为有效最小宽度。

### 7.6 右侧内容和 handle binding

TreeSelect 根模板中的稳定 template part 关系优先由 AXAML 表达：

- `SelectMaxCountIndicator` 通过 compiled ancestor binding 接收 `MaxCount`、`SelectedCount` 和 `IsShowMaxCountIndicator`。
- `PART_ContentRightAddOnPresenter` 通过 compiled ancestor binding 接收 `ContentRightAddOn`、`ContentRightAddOnTemplate`，并用 `ObjectConverters.IsNotNull` 控制显示。
- `SelectHandle` 通过 compiled ancestor binding 接收 Form feedback、loading 图标、展开图标、过滤状态、启用状态、motion、loading、clear、选择为空和下拉打开状态。

`SetupSelectHandleInputStateBindings()` 只保留 frame layout part 到 `SelectHandle` 的 hover / pressed sibling 状态转发。这个关系的 source 是同一模板中的 sibling part，不是 templated parent，无法用 `TemplateBinding` 或 ancestor binding 在不改变模板契约的前提下表达；它只转发交互状态，不改变 `InputControlFrame` 对 variant、effective status、error、warning 或 disabled 的唯一 ownership。该 binding 在每次重新应用模板前释放旧 `CompositeDisposable`，生命周期与模板 part 获取路径一致。

## 8. 资源、性能与 AOT 边界

TreeSelect 不依赖运行时反射发现模板结构。模板协作通过固定 template part、显式类型、AXAML binding、C# indexer binding 和事件完成。

资源和生命周期边界：

- `_selectHandleInputStateBindings` 每次模板接入前释放旧绑定，仅持有 frame layout part → SelectHandle 的 hover / pressed sibling 状态转发，不承载输入表面状态归一。
- `_treeView` 的事件订阅和 `ItemsSource` 必须在 `ClearPopupContent()` 中释放。
- 懒创建的 `PopupFrame` 和 `TreeSelectTreeView` 必须设置 `TemplatedParent`，并在清理时置空。
- `Items.CollectionChanged` 是控件实例持有自身集合的订阅，生命周期与控件实例一致。
- `_selectedItemsCollectionChangedSource` 只订阅当前 `SelectedItems` 中实现 `INotifyCollectionChanged` 的集合；`SelectedItems` 替换、控件 detach 时必须释放旧订阅。
- TreeView 异步加载由 TreeView 家族处理，TreeSelect 不直接持有异步任务状态。

AOT 边界：

- `TreeSelectToken` 通过 token generator 显式注册，生成 `TreeSelectTokenKind` 和 `TreeSelectTokenResourceExtension`。
- `ItemTemplate` 是 XAML 模板入口，不依赖运行时成员扫描。
- API 与 Token 契约由控件文档、源码 public surface、Token 类型或生成数据维护。

## 9. 维护不变量

内部重构必须保持以下不变量：

- public/protected API、Avalonia property、template part、Token 和 selector 契约不变。
- 构造函数之后必须先放 TreeSelect 自身 public/protected API，再放 Form 接口区，private 实现方法必须位于接口区之后。
- 可用 AXAML 表达的模板绑定不能回退为 `BindUtils.RelayBind`。
- C# relay binding 必须有与获取路径匹配的释放路径。
- `SelectedItem` / `SelectedItems` 与 TreeView selection / checked items 的同步不能形成递归事件。
- `SelectedItems` 集合引用替换和原地变更必须刷新同一组 value-state，避免 tag、`SelectedCount`、Form 值和 popup TreeView 状态不同步。
- `ShowCheckedStrategy` 只能派生展示集合，不能改写真实选择集合。
- `ItemsSource` 替换时的选择保留必须继续使用节点路径 identity。
- popup 内容清理必须断开事件、ItemsSource、TemplatedParent 和 popup child 引用。
- Semantic Part marker 的维护边界：`TreeSelectTheme.axaml` 承载触发区静态 marker（`semantic-scope-input`、`semantic-prefix`、`semantic-suffix`、`semantic-scope-handle`、`semantic-content`、`semantic-placeholder`、`semantic-input`、`semantic-scope-tags`、`semantic-popup-root`）；共享 `SelectHandleTheme.axaml` 承载清除按钮的 `semantic-clear` marker（`clear` Part 声明 `CrossNestedOwners=true`，生成器沿 SelectHandle 主题链校验）；共享 `TagTheme.axaml` 承载 `itemContent` / `itemRemove` marker。运行时注入点：共享 `SelectTagAwareTextBox` 创建标签时追加 `TagItemClass`、创建搜索框时追加 `TagSearchInputClass`；`TreeSelect.EnsurePopupContent` 创建候选树时追加 `TreeSelectSemanticParts.PopupListClass` 并显式 `SetTemplatedParent(this)`；`TreeViewSelectTreeViewItem` 构造函数追加 `TreeSelectSemanticParts.PopupListItemClass`。marker 随容器实例创建一次，prepare/clear/recycle 路径不得增删；`PopupFrame` 以 `Popup.Child` 取回，不在控件中缓存字段。

## 10. 测试与验证

对应验证：

- `tests/AtomUI.Desktop.Controls.Tests/TreeSelect/TreeSelectBehaviorTests.cs`：模板右侧 AXAML binding 和剩余 sibling relay binding。
- `tests/AtomUI.Desktop.Controls.Tests/TreeSelect/TreeSelectSelectionBindingTests.cs`：`SelectedItem` / `SelectedItems` 默认双向绑定、data validation metadata、`SelectedItems` 原地变更和 checkable TreeView 同步。
- `tests/AtomUI.Desktop.Controls.Tests/SizeType/CustomizableSizeTypeContractTests.cs`：TreeSelect 的 `ICustomizableSizeTypeAware` 契约。
- `tests/AtomUI.Desktop.Controls.Tests/TreeSelect/TreeSelectSemanticPartTests.cs`：descriptor 十三个 Part 契约、模板静态 marker 清单、默认主题不消费 semantic selector、生成 Style 命中触发区目标、标签部件样式命中、多选搜索输入 marker、前缀部件内联呈现、popup 部件 marker 暴露。
- `tests/AtomUIGallery.Tests/ShowCases/TreeSelectShowCasePageTests.cs`：Gallery TreeSelect 页面结构、示例和源码片段约束。

维护 TreeSelect 后至少运行：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore
git diff --check
```

涉及 Gallery 示例或文案时，追加运行：

```bash
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~TreeSelectShowCasePageTests
```

## 根表面主题投射

根几何进入实际输入 frame；多选 Custom 的默认紧凑 inset 与显式 inset 分开，标签内在高度保持。现有多选尺寸/前缀参数继续引用 Select 家族资源。

几何默认、原生优先级和组合边界见[共享输入表面](../input-control-architecture-design.md#81-根表面几何与预设默认值)。
