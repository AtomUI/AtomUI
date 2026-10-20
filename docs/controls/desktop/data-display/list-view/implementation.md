# ListView 桌面版实现原理

本文档描述 ListView 桌面版的内部源码结构、数据视图归一、选择状态、分页、分组、过滤、容器生命周期、虚拟化上下文和主题接入。公共设计与 API 契约见 [ListView 桌面版架构设计](overview.md)，完整 Semantic Part 公共契约见 [ListView Semantic Part 契约](semantic-part.md)，完整选择设计见 [ListView 选择模型设计](selection-model-design.md)，Token 语义见 [ListView Token 设计](token.md)，变化记录见 [ListView Changelog](changelog.md)。

## 1. 实现定位

ListView 的实现目标是在 Avalonia `ItemsControl` 基础上增加 AtomUI 数据视图列表能力，并保持 public API、主题契约和容器生命周期稳定。实现文档覆盖 `ListView`、`ListViewItem`、选择 partial、分页 partial、虚拟化上下文、`ListCollectionView` 协作、ListView token 和 AXAML theme。

本文档不逐行复述属性注册、CLR wrapper 和小型 guard。维护具体行为时仍应直接阅读源码，尤其是 `ListView.Selecting.cs`、`ListCollectionView.cs` 和 `ListView.Pagination.cs`。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/ListView/ListView.cs`：public API、事件、ItemsSource 归一、collection view 订阅、分组、过滤、排序、容器生成、分割线同步、空状态、操作态和点击派发。
- `src/AtomUI.Desktop.Controls/ListView/ListView.Selecting.cs`：选择模型接入、source entry / view node 映射、SelectedIndex / SelectedIndexes / SelectedItem / SelectedItems / SelectedValue、键盘导航、文本搜索和容器 selected 状态同步。
- `src/AtomUI.Desktop.Controls/ListView/ListView.Pagination.cs`：PageIndex / PageSize、分页器接入、分页器属性同步和 page change 请求。
- `src/AtomUI.Desktop.Controls/ListView/ListView.Virtualizing.cs`：容器回收时的上下文保存、恢复和本地值清理。
- `src/AtomUI.Desktop.Controls/ListView/ListViewItem.cs`：条目容器、selected attached property、点击 routed event、pointer selection、组标题状态、selected indicator、分割线状态和 motion 生命周期。
- `src/AtomUI.Desktop.Controls/ListView/ListViewSelectionModel.cs`：ListView canonical selection model，持有 selected EntryIds、anchor 和 active entry，并发布 source-index 与 item 投影。
- `src/AtomUI.Desktop.Controls/ListView/ListDefaultFilter.cs`：把 collection view `FilterDescriptions` 聚合成 `Func<object, bool>`。
- `src/AtomUI.Desktop.Controls/ListView/ListPaginationVisibility.cs`：分页器可见性枚举。
- `src/AtomUI.Desktop.Controls/ListView/ListPseudoClass.cs`：`:empty` 和 `:singleitem` 伪类常量。
- `src/AtomUI.Desktop.Controls/ListView/ListViewToken.cs`：ListView 专属 Token 定义。
- `src/AtomUI.Desktop.Controls/ListView/Themes/ListViewTheme.axaml`：root 模板、分页 presenter、Spin、ScrollViewer、ItemsPresenter、EmptyIndicator、默认 item / group template、root 圆角内容裁剪和 root 样式。
- `src/AtomUI.Desktop.Controls/ListView/Themes/ListViewItemTheme.axaml`：条目模板、selected indicator、普通内容、组标题状态、分割线和条目状态样式。
- `src/AtomUI.Controls.Shared/Data/ListCollectionViews/`：`IListCollectionView`、source entry、view node、entry/index 映射、排序、过滤、分组和数据项契约。

## 3. 核心类职责

`ListView` 是列表根控件，负责把 Avalonia `ItemsSource` 归一为 `IListCollectionView`，将排序、过滤、分组和分页 public API 写入 collection view，并把 collection view 状态同步到空状态、分页器、伪类和选择模型。

`ListViewItem` 是条目容器，继承 `ContentControl` 并实现 `ISelectable` 和 `IListItemVirtualizingContextAware`。它负责内容承载、pointer selection、点击事件、组标题标记、selected indicator 可见性和初始化 / loaded 阶段动效启停。

`ListViewSelectionModel` 是选择状态的唯一 owner。它以 EntryId 保存选中集合、anchor 和 active entry，接收 source-index 或容器交互请求，并向 ListView 投影 `SelectedIndex(es)`、`SelectedItem(s)` 和 `SelectedValue`。

`ListCollectionView` 位于 `AtomUI.Controls.Shared`，负责 source entry 生命周期、当前视图枚举、排序、过滤、分组、分页、源集合变化、entry/index 映射和 page change 事件。ListView 不直接实现这些集合算法。

`AbstractPagination` 实例由用户注入到 `TopPagination` 或 `BottomPagination`。ListView 不创建默认分页器，只在分页器存在时同步 total、page size、current page、align、enabled、motion 和 hide-on-single-page。

## 4. 状态与数据流

ItemsSource 归一流：

```text
ItemsSource changed
      ↓
newItemsSource implements internal entry view bridge ?
  yes: use entry-capable IListCollectionView
  no : newItemsSource is IListCollectionView ?
         yes: wrap its SourceCollection with AtomUI ListCollectionView
         no : wrap newItemsSource with AtomUI ListCollectionView
      ↓
subscribe PropertyChanged / CollectionChanged / PageChanging / PageChanged
      ↓
SetValueNoCallback(ItemsSourceProperty, collectionView)
      ↓
ConfigureFilterDescription
ConfigureSortDescriptions
ConfigureGroupInfo
ReConfigurePagination
```

过滤、排序、分组流：

```text
Filter / FilterValue / FilterValueSelector
      ↓
ListFilterDescription
      ↓
IListCollectionView.FilterDescriptions
      ↓
ListDefaultFilter
      ↓
ListCollectionView.Refresh

SortDescriptions
      ↓
IListCollectionView.SortDescriptions

IsGroupEnabled / GroupPropertySelector
      ↓
ListGroupDescription
      ↓
IListCollectionView.GroupDescriptions
      ↓
ListCollectionViewGroupRoot
      ↓
GroupListItemData(Content = groupKey.ToString(), IsGroupItem = true)
```

分组视图流：

```text
Source item
      ↓
GroupPropertySelector(item)
      ↓
group key
      ↓
ListCollectionViewGroupRoot.AddToSubgroups
      ↓
new group?
  yes: create ListCollectionViewGroupInternal
       insert GroupListItemData before first leaf item
  no : append / insert leaf item into existing group
      ↓
RootGroup.GetLeafEnumerator
      ↓
ListView current view enumeration
```

容器生成流：

```text
Current view item
      ↓
CreateContainerForItemOverride -> ListViewItem / GroupHeaderItem
      ↓
PrepareContainerForItemOverride
  ContentTemplate <- ItemTemplate / GroupItemTemplate
  SizeType / Motion / SelectedIndicator / Item backgrounds
  IsShowSelectedIndicator / ItemClickMode
  IsSplitLineVisible（按视图位置计算）
  Restore IListItemData default context
  Restore virtualizing context by VirtualIndex
      ↓
ContainerForItemPreparedOverride
  sync container IsSelected from Selection
```

分割线状态流：

```text
视图位置 / BottomPagination / IsBorderless
      ↓
ListView.ConfigureSplitLineVisibility(container)
      ↓
ListViewItem.IsSplitLineVisible
      ↓
ListViewItem.ConfigureEffectiveBorderThickness
  IsSplitLineVisible && !IsGroupItem && BorderThickness.Bottom > 0
      ↓
IsSplitLineEffectiveVisible + EffectiveBorderThickness
      ↓
SplitLineFrame 底边线
```

选择状态流：

```text
Pointer / keyboard / text search / SelectedIndex / Selection command
      ↓
view node or source index
      ↓
EntryId
      ↓
ListViewSelectionModel
      ↓
SelectedIndex(es) / SelectedItem(s) / SelectedValue projections
      ↓
ListView.IsSelected attached property on container
      ↓
ListViewItem.IsSelectedIndicatorVisible
```

`SelectedIndex` 是 source-index 选择输入。`SelectedIndexes`、`SelectedItem`、`SelectedItems` 和 `SelectedValue` 是 canonical selection 的只读投影。Selection model 不从 item 或 selected value 反向查找 source entry，同一对象重复出现时可以生成多个独立选中结果。

分页流：

```text
PageSize
      ↓
IListCollectionView.PageSize
      ↓
Current view enumerates current page

Pagination.CurrentPageChanged
      ↓
IListCollectionView.MoveToPage(page - 1)
      ↓
PageChanged
      ↓
ListView.PageIndex
      ↓
TopPagination / BottomPagination CurrentPage
```

空状态流：

```text
CollectionChanged / TotalItemCount changed
ItemsSource changed
IsShowEmptyIndicator changed
      ↓
ConfigureEmptyIndicator
      ↓
IsEffectiveEmptyVisible = IsShowEmptyIndicator && TotalItemCount == 0
      ↓
PART_ScrollViewer hidden / EmptyIndicator shown
```

## 5. 生命周期与模板接入

`ListView` 静态构造完成以下配置：

- 覆盖默认 `ItemsPanel` 为 `VirtualizingStackPanel`。
- 注册 `IsSelectedChangedEvent` class handler，同步容器 selected 状态回选择模型。
- 注册 `ListViewItem.ClickedEvent` class handler，收敛到 `ItemClicked`。
- 监听 `IsSelectableProperty`，关闭可选状态时清空选择。
- 监听 `TotalItemCountProperty`，触发 `ItemCountChanged`。
- 监听 `ItemsSourceProperty`，进入 collection view 归一流程。

`ListView` 实例构造注册 `ListViewToken` scope，监听 `ItemsView` source changed、当前 items collection changed 和 `Items.CollectionChanged`。集合变化会清理虚拟化恢复上下文、刷新空状态，并校验直接 `Items` 的数据项语义。

`HandleItemsSourcePropertyChanged` 是数据视图生命周期核心。它只直接接入具备 internal entry bridge 的 collection view；普通自定义 `IListCollectionView` 从 `SourceCollection` 重新包装。view 替换时解除旧 view 的 `PropertyChanged`、`CollectionChanged`、`PageChanging` 和 `PageChanged` 订阅，仅释放 ListView 自建 view。新 view 接入后设置 `IsEmptyDataSource`、`TotalItemCount`、过滤 callback、排序描述、过滤描述、分组描述和分页状态。

`OnInitialized` 设置默认 `GroupPropertySelector`，刷新空状态和过滤态，并确保 collection view entry projection 与 selection model 已接入。

分组相关生命周期集中在 collection view 配置阶段：

- `ConfigureGroupInfo` 根据 `IsGroupEnabled` 和 `GroupPropertySelector` 写入或清理 `GroupDescriptions`。
- `ReConfigureGroupInfo` 在 `GroupPropertySelector` 变化时清空旧分组描述，再按当前配置重建。
- `IsGroupEnabled` 变化后，ListView 使用 `_collectionView.DeferRefresh()` 合并分组重建，并刷新 view-node 映射。
- 分组只改变 view projection。selection model 始终以业务 source entries 为选择范围，组标题节点不进入该范围。

`OnApplyTemplate` 接入模板后初始化默认 `EmptyIndicator`，刷新伪类、空状态和分页器可见性。默认空状态是 `Empty` simple preset。

`PrepareContainerForItemOverride` 是 root 状态同步到 item 的核心入口。它先确保 selection model 已创建，再根据 item 类型选择 `ItemTemplate` 或 `GroupItemTemplate`，使用 Avalonia `[!]` binding 将同生命周期状态绑定到 `ListViewItem`，然后恢复默认数据上下文和虚拟化上下文。它还按容器当前视图位置计算 `IsSplitLineVisible`，最后一项抑制与 `BottomPagination` / `IsBorderless` 例外见 7.8。

root 模板的 `Frame`（`PixelAlignedBorder`）开启 `ClipContentToCornerRadius`：内容被裁剪到外框圆角的内边缘，条目
hover / selected 背景不会溢出圆角口袋区，视觉上与 CSS `border-radius + overflow: hidden` 一致。外框环由 `Frame`
自身一次绘制（`BorderBrush` = `ColorSplit`，与分割线同色同粗细），不存在二次叠加。实现见 7.8。

`ClearContainerForItemOverride` 是容器回收清理入口。它临时关闭 motion 和容器 selected 同步，保存 `IsEnabled`、`IsGroupItem` 等虚拟化上下文，清理容器本地值，再调用基类释放容器。新增容器级状态时必须同步补齐 save、restore、clear 三条路径。

### 5.1 Semantic Part 处置

Batch 2 Gate A 审计结论：ListView 家族以 Ant Design 6.6.0 新增的 `Listy` 组件为上游基线，公开 `root`、`item`、
`groupHeader` 三个 Semantic Part。旧 `List` 组件已被上游标记 deprecated（`List.Item` 的 `actions` / `extra` 不是
ListView 家族的上游契约），详见下方证据。

上游契约（唯一来源 `.referenceprojects/ant-design/components/listy`，6.6.0）：

- `Listy` 是高性能虚拟化列表（`import { Listy } from 'antd'`；`virtual` 只渲染可视行、支持分组吸顶与命令式滚动）。
- Semantic DOM：`classNames` / `styles` 均为 `{ root?, item?, groupHeader? }`，三个 key 全部自 6.6.0 公开
  （`index.en-US.md` API 表与 `demo/_semantic.tsx`）。
- 语义职责（`demo/_semantic.tsx` 文案逐字）：`root` 是"根元素，即滚动容器，设置字体与相对定位"；`item` 是
  "条目元素，设置内间距、分割线与悬浮背景"；`groupHeader` 是"分组标题元素，设置吸顶定位与背景色"。
- 样式基线（`style/index.ts`）：`.ant-listy` 为 `resetComponent` + `position: relative`；`.ant-listy-item` 为
  `padding: itemPaddingBlock × itemPaddingInline`（默认 `paddingSM × padding`）、`borderBottom: lineWidth lineType
  colorSplit`、hover `controlItemBgHover`、`motionDurationMid` 背景过渡；`.ant-listy-group-header` 为
  `padding: paddingXS × itemPaddingInline`、`color: colorTextDescription`、`fontWeight: fontWeightStrong`、
  `background: colorBgContainer` + `linear-gradient(colorFillAlter)`，`-sticky` 变体 `position: sticky; top: 0`。
  ComponentToken 只有 `itemPaddingBlock` / `itemPaddingInline`。
- 实现实际消费调用方传入的语义值：`index.tsx` 经 `useMergeSemantic` 合并 ConfigProvider 与 props 的
  `classNames` / `styles` 后传给 `RcListy`，root 类名并入根元素 —— 满足系统设计 2.1 的完整准入 Gate。

AtomUI 对应审计与映射：

- 上游 Listy 家族直接对应 `ListView` 家族（`ListView` + `ListViewItem` + 分组标题容器）与 `ListBox` 家族
  （`root` / `item` 映射，`groupHeader` 不适用——ListBox 没有分组功能），两者共享 Gallery 页面 `DataDisplay/List`；
  ListBox 侧的映射见 [ListBox 桌面版实现原理](../list-box/implementation.md)。
- `root` Part → `ListView` owner 本身。滚动容器结构（`Frame` + `ScrollViewer` + `ItemsPresenter`）已经存在，无结构
  差距；隐式 Part，不生成 Style，不添加 `.semantic-root`。
- `item` Part → 每个非分组 `ListViewItem` 容器。已有 `ItemPadding*` 三档、`ItemHoverBg` 与 `IsMotionEnabled` 背景
  过渡；与 Listy 基线的差距是条目底部分割线（`borderBottom: colorSplit`），默认视觉对齐见下。
- `groupHeader` Part → 每个分组标题容器（专用 `GroupHeaderItem`，internal、继承 `ListViewItem`，`IsGroupItem` 恒为
  true）。已有 `GroupHeaderColor`（`ColorTextDescription`，与 Listy 颜色一致）与 `GroupItemTemplate`；差距是背景
  （`ColorBgContainer` 上叠加 `ColorFillAlter`，对应 Listy 的 `backgroundColor: colorBgContainer` +
  `backgroundImage: linear-gradient(colorFillAlter, colorFillAlter)`）、`FontWeightStrong` 与 padding 基线。
- 行为 API 对照：`virtual`（ListView 默认 `VirtualizingStackPanel`）、`height`（`Height`）、`items` / `itemRender`
  （`ItemsSource` / `ItemTemplate`）、`rowKey`（`ItemKeySelector`）、`group.key` / `group.title`（`IsGroupEnabled` +
  `GroupPropertySelector` + `GroupItemTemplate`）、`onScroll`（ScrollViewer 事件）均有直接对应。`sticky` 吸顶分组与
  `scrollTo` 命令式滚动是行为功能而非 Semantic Part，不属于本轮范围，作为重新评估触发条件记录。
- marker 方案（遵守批次约束：容器 marker 不得在 prepare、选择或状态变化期间切换）：分组标题使用专用容器类型
  `GroupHeaderItem`（internal，继承 `ListViewItem`，构造时自带 `IsGroupItem=true`），非分组条目使用 `ListViewItem`；
  `.semantic-item` 与 `.semantic-group-header` 在 `CreateContainerForItemOverride` 创建路径用生成常量一次性添加，
  `PrepareContainerForItemOverride` 按容器类型幂等补齐（覆盖用户直接提供容器、不经过创建路径的场景，与 Collapse
  `.semantic-scope-item` 的建立纪律一致）——容器角色由类型决定，prepare、restore、recycle、分组开关都不切换
  marker。`CreateContainerForItemOverride` 按 item 类型（`IGroupListItemData.IsGroupItem`）创建对应容器，
  `NeedsContainerOverride` 的 recycle key 区分两类容器。两个 Part 声明 `RuntimeCreated=true`，SelectorRoute 为
  `> .semantic-item` / `> .semantic-group-header`（容器逻辑父级是 ListView owner，沿逻辑树一步直达，同 Collapse
  scope 路由事实）；`ContractType` 为公开 `ListViewItem`（`GroupHeaderItem` 的公开基类）。
- 默认视觉对齐（照 Listy 基线，属默认外观变更，随 Gate A 一并批准）：默认主题补齐条目底部分割线（`ColorSplit` /
  `LineWidth`）与分组标题背景（`ColorFillAlter` 叠加于 `ColorBgContainer`，同 Listy 的
  `backgroundColor: colorBgContainer` + `backgroundImage: linear-gradient(colorFillAlter, colorFillAlter)`）、字重
  （`FontWeightStrong`）与 padding；root 外框与分割线统一为
  `ColorSplit`、条目表面直角、`ContentPadding` / `ItemMargin` 归零、root `Frame` 开启 `ClipContentToCornerRadius`
  内容裁剪；最后一项分割线由 root 外框下边缘闭合（`BottomPagination` 或 `IsBorderless` 时保留）。内容裁剪对应 Listy
  style-class demo 的 `overflow: hidden`（背景不溢出圆角口袋区），最后项规则对应 antd `List` split 样式的
  `:last-child` 抑制（最后一项之后仍有内容时恢复分割线，映射为 `BottomPagination` 场景）；Gallery List 页面与既有
  测试断言按新基线同步。
- 排除范围：selection（`SelectedIndicator`）、pagination、empty/loading、普通条目 content，以及 Listy 的
  `-sticky` / `-fixed` / `-holder` / `-group-section` / `-scrollbar` 内部节点，均不发布为 Part。

重新评估触发条件：`sticky` 吸顶分组或 `scrollTo` 命令式滚动作为独立行为 API 需求进入时，按新批次重新规划；上游
新稳定版调整 `root` / `item` / `groupHeader` 语义键时重新核对契约。

## 6. 交互与事件处理

pointer selection：

- 鼠标左键和右键按下时立即通过 owner ListView 更新 selection。
- touch 和 pen 的普通点击延迟到 release，避免按下阶段阻断滚动手势。
- pen 右键按下按鼠标右键处理。
- 组标题项在 `UpdateSelectionFromEventSource` 中被跳过。

click event：

- `ItemClickMode=Press` 时左键按下触发 `ListViewItem.Clicked`。
- `ItemClickMode=Release` 时左键释放触发 `ListViewItem.Clicked`。
- ListView 通过 class handler 接收后调用 `NotifyItemClicked`，再触发 public `ItemClicked`。

keyboard navigation：

- 方向键通过 `INavigableContainer` 查找下一个容器，并更新 selection 与 focus。
- `WrapSelection=true` 时同步 Avalonia focus wrap 行为。
- 多选模式下，平台 select-all 手势调用 `Selection.SelectAll()`。
- Space / Enter 从事件源更新选择。

text search：

- `IsTextSearchEnabled=true` 时，`OnTextInput` 累积输入文本。
- 搜索值优先使用 `TextSearch.TextBinding`，其次使用 `DisplayMemberBinding`。
- 命中第一个前缀匹配项后设置 `SelectedIndex`。
- 搜索词通过 1 秒 `DispatcherTimer` 清空。

pagination interaction：

- 分页器 `CurrentPageChanged` 只请求 collection view 移动页。
- collection view `PageChanged` 回写 `PageIndex`。
- 分页器替换时必须解除旧分页器事件和 relay binding。

## 7. 内部算法与关键流程

### 7.1 Collection view 桥接

ListView 接收 `ItemsSource` 后，只直接使用实现 internal entry view bridge 的 `IListCollectionView`，并且不拥有其生命周期。普通自定义 `IListCollectionView` 使用其 `SourceCollection` 创建 AtomUI `ListCollectionView`；其他 `IEnumerable` 直接包装，并在替换时释放自建 view。随后通过 `SetValueNoCallback` 把归一后的 view 写回 `ItemsSourceProperty`，避免重复进入 property changed handler。

此路径要求维护者严格区分：

- 用户设置的原始数据源。
- ListView 当前持有的 `_collectionView`。
- 控件是否拥有 `_collectionView`。
- collection view 持有的 source entries 与当前 view nodes。
- selection model 持有的 EntryIds 与公开 source-index 投影。

### 7.2 FilterDescriptions 聚合

ListView 使用 `ConfigureFilterDescription` 清空并重建 collection view 的 `FilterDescriptions`。当 `FilterValue` 和 `Filter` 同时存在时，创建 `ListFilterDescription`，并把 `FilterValueSelector` 或 `IListItemData.Content` 默认 selector 作为过滤值入口。

`ListDefaultFilter` 将多个 `FilterDescriptions` 做 AND 聚合。它通过 `WeakReference<IListCollectionView>` 持有 view，避免 filter callback 成为强引用链。

### 7.3 分组视图

分组开启时，`ConfigureGroupInfo` 向 collection view 写入 `ListGroupDescription`。默认 selector 从 `IGroupHeader.Group` 读取 group key，用户可通过 `GroupPropertySelector` 覆盖。

ListView public API 当前只创建一个 `ListGroupDescription`，因此 ListView 契约是单层分组。`IListCollectionView.GroupDescriptions` 本身是列表，属于 shared collection view 能力；除非 ListView 显式增加 public API，否则不应把多层分组写进 ListView 控件契约。

`ListCollectionViewGroupRoot.AddToSubgroups` 负责生成分组结构：

1. 使用当前层级的 `IListGroupDescription.GroupKeyFromItem` 取得 group key。
2. 如果 group key 命中已有 subgroup，则把数据项加入该 subgroup。
3. 如果 group key 是新 key，则创建 `ListCollectionViewGroupInternal`。
4. 新 subgroup 创建后，先插入一个 `GroupListItemData`，其 `Content` 为 `key.ToString()`，`IsGroupItem=true`。
5. 再把原始数据项加入同一 subgroup。

`RootGroup.GetLeafEnumerator` 会把组标题项和真实数据项一起展平成当前 view 枚举结果。ListView 因此不需要单独维护组标题集合；组标题与普通项走同一容器生成路径。

容器准备时，如果 item 实现 `IGroupListItemData` 且 `IsGroupItem=true`，ListView 使用 `GroupItemTemplate`；否则使用 `ItemTemplate`。组标题通过 `ListViewItem.IsGroupItem` 进入主题 selector，并在 pointer selection 入口被排除。

分组与分页组合时，collection view 使用两段式准备：

```text
PrepareTemporaryGroups
  build grouping for full filtered / sorted internal list
      ↓
PrepareGroupsForCurrentPage
  rebuild exposed _group from current page items
```

这保证分页后的枚举只包含当前页范围内需要展示的组标题和数据项，同时保留完整结果的分组顺序信息。

维护分组路径时必须注意：

- `GroupPropertySelector` 返回 `null` 时，当前分组算法不会自动创建默认 group；需要兜底分组时 selector 应返回明确 key。
- `GroupListItemData` 是 collection view 生成的展示项，不应写回用户 source collection。
- `TotalItemCount` 在分组态基于当前 view `Count`，可能包含组标题展示项；空状态和分页同步必须按当前实现语义处理。
- 分组节点没有 EntryId 和 source index；pointer、keyboard、range、select-all 和其他选择入口必须统一跳过组标题。
- 虚拟化上下文必须保存和清理 `IsGroupItem`，避免普通项被回收容器误显示成组标题，或组标题误使用普通项模板。

### 7.4 排序视图

`SortDescriptions` 直接同步到 collection view 的 `SortDescriptions`。`ListSortDescription.FromPath` 支持 property path 排序；NativeAOT 场景优先使用生成的数据成员访问器或显式 comparer，避免依赖运行期反射。

### 7.5 Entry 与索引映射

ListCollectionView 维护 source-index、EntryId 和 view-index 三个域：

```text
source index -> source entry
EntryId -> source index
view index -> item node / group header node
EntryId -> current view index
```

排序、过滤、分组和分页只重建 view node 顺序以及 `EntryId -> view index` 映射。公开选择索引始终由 `EntryId -> source index` 计算，不使用 `PageIndex * PageSize` 推测源位置，也不把 view item 传给 `IndexOf` 反查。

过滤或分页隐藏 entry 时，该 entry 没有当前 view index，但仍保留 source index 和选择状态。组标题只存在于 view node 序列，没有 source entry 映射。

### 7.6 SelectedValue

`SelectedValueBinding` 使用 `BindingEvaluator<object?>` 复用 evaluator。主选中 entry 变化或被 Replace 时，ListView 根据对应 item 计算 `SelectedValue`。`SelectedValue` 是只读投影，不按 value 遍历 items 反向选择条目。

`BeginInit` / `EndInit` 和 DataContext update 期间，ListView 使用选择事务延迟应用 source-index 输入与公开投影通知，避免 Items 和 selection 更新顺序暴露中间状态。

### 7.7 虚拟化上下文

`IListVirtualizingContextAware` 以容器 `VirtualIndex` 保存上下文。当前默认保存：

- `ListViewItem.IsEnabled`
- `ListViewItem.IsGroupItem`

恢复时通过 `ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure` 标记上下文操作，避免恢复过程触发错误的本地状态传播。清理时移除 `Content`、`ContentTemplate`、`IsEnabled` 和 `IsGroupItem` 本地值。

### 7.8 条目分割线与圆角内容裁剪

`ListViewItem` 持有三个 internal DirectProperty：`IsSplitLineVisible`（ListView 写入的分割线决策）、
`IsSplitLineEffectiveVisible` 与 `EffectiveBorderThickness`（由 `ConfigureEffectiveBorderThickness` 合成）。
合成规则为 `showSplitLine = IsSplitLineVisible && !IsGroupItem && BorderThickness.Bottom > 0`；条目模板的
`SplitLineFrame` 只绑定 `EffectiveBorderThickness`、`IsSplitLineEffectiveVisible` 和 `BorderBrush`，因此 Semantic
Style 对容器 `BorderThickness` / `BorderBrush` 的覆盖直接作用到分割线。

`IsSplitLineVisible` 由 ListView 在 `PrepareContainerForItemOverride` 按容器视图位置计算：
`IsBorderless || index < Items.Count - 1 || BottomPagination is not null`。无 `BottomPagination` 时最后一项的
分割线被抑制，由 root 外框下边缘承担闭合线；`BottomPagination` 存在时保留（分隔条目区与分页器）；`IsBorderless`
时保留（外框消失后由分割线承担闭合线）。容器回收后重新 prepare 会按新位置重算；集合变化（`Items.CollectionChanged`）、
`BottomPagination` 与 `IsBorderless` 变化时，ListView 对所有已实现容器重新同步，保证追加条目后原最后一项恢复分割线、
删除后新最后一项被抑制。

圆角闭合由内容裁剪承担：root 模板的 `Frame`（`PixelAlignedBorder`）设置 `ClipContentToCornerRadius="True"`，
`DashedBorder` 按 `Frame` 自身的 `BorderThickness` / `CornerRadius` 用
`RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI`（`BackgroundSizing.InnerBorderEdge`，即外框环的
内边缘：边界缩小一个边框厚度、半径减半线宽）构建圆角矩形几何并赋给内容（`DockPanel`）的 `Visual.Clip`。裁剪几何在
子内容坐标系中构建：子内容被 padding + 边框厚度内缩，因此先按 padding + 边框厚度把子内容边界膨胀回外框内边缘，
再交给 builder 内缩（两次抵消后裁剪矩形恰好是子内容边界加回 padding）。条目 hover / selected 背景因此被裁剪到
圆角内边缘，视觉上与 CSS `border-radius + overflow: hidden` 一致；滚动时裁剪随内容平移持续生效。外框环由 `Frame`
自身一次绘制，无任何叠加节点，`BorderBrush`（`ColorSplit`）与分割线同色同粗细。

裁剪会参与 Avalonia 组合器命中测试（`visual.Clip?.FillContains(point)`），因此 `DashedBorder` 在应用裁剪前用
裁剪图形自身探测平台几何命中能力（`FillContains(内部点) && !FillContains(外部点)`）：生产后端（Skia）对圆角几何的
包含判定正确，裁剪正常生效；若运行平台无法正确判定圆角几何包含（headless 测试平台的流式几何桩只跟踪 ArcTo 端点、
按连续三点三角扇近似），裁剪降级为不应用，保证列表内指针输入不受影响。探测点选择由 `SupportsGeometryClipHitTesting`
虚方法承载，测试可覆写以验证裁剪路径。

## 8. 资源、性能与 AOT 边界

ListView 不通过反射访问模板内部结构。模板接入依赖稳定 part 名称、template binding、theme selector 和 Avalonia 属性绑定。

同生命周期的 root 到 item 状态同步使用 Avalonia `[!]` binding。容器由 ItemsControl 管理，binding 生命周期随容器生命周期结束。

默认 ItemsPanel 是 `VirtualizingStackPanel`。容器回收路径必须保持本地值对称清理，避免 selected、disabled、group item 和 content 状态泄漏到新数据项。

`ListCollectionView` 在排序、过滤、分页或分组启用时维护本地数组。维护过滤路径时应避免无活跃过滤条件时引入不必要的 collection view 本地数组刷新。

选择映射使用 source entries、`EntryId -> source index` 和 `EntryId -> view index` 索引表。容器准备、选中查询和选择到容器的映射不得执行基于 item equality 的线性 `IndexOf`。完整 Refresh 允许 O(n) 重建索引表；增量变化更新受影响区间并在同一 refresh transaction 中发布。

`ListSortDescription.FromPath` 存在运行期属性路径访问能力；NativeAOT 应优先使用 `GenerateDataMemberAccessors` 生成访问器，或使用 `ListSortDescription.FromComparer` 提供显式 comparer。

分页器 relay binding 和事件订阅必须在分页器替换时释放。collection view 事件订阅必须在 view 替换时释放，自建 view 必须释放其源集合弱转发器。

文本搜索 timer 不应成为长期后台工作；停止搜索时必须解除 tick 订阅并停止 timer。

## 9. 维护不变量

内部重构必须保持以下不变量：

- `ListView.cs` 保留 public API、事件、ItemsSource 归一、容器生命周期和 collection view 配置入口。
- `ListView.Selecting.cs` 保持选择模型接入、entry/index 映射、SelectedValue 和键盘 / 文本搜索职责，不把选择状态写入数据项或容器作为 owner。
- `ListViewSelectionModel` 是 selected EntryIds、anchor 和 active entry 的唯一 owner；公开属性不得维护平行选择状态。
- 标识映射路径不得使用 item `Equals`、对象引用、`IList.IndexOf` 或 `IListCollectionView.IndexOf` 解析 source entry。
- `SelectedItem`、`SelectedItems` 和 `SelectedValue` 只能从 canonical selection 投影，不作为反向选择输入。
- `ListView.Pagination.cs` 只处理分页器接入和 collection view page 状态同步，不执行数据请求。
- `ListViewItem.cs` 保持条目容器角色，不承载排序、过滤、分页或跨列表全局状态。
- collection view 替换时必须解绑旧 view 事件；只有具备 entry bridge 的 view 可以直接接入，且只有 ListView 自建 view 才能由 ListView dispose。
- `ConfigureFilterDescription`、`ConfigureSortDescriptions` 和 `ConfigureGroupInfo` 重建描述前必须清理旧描述。
- `IsGroupEnabled` 切换后只刷新 view projection，并保持组标题项没有 EntryId 且不进入业务选择结果。
- `GroupPropertySelector` 必须返回稳定 group key；需要兜底分组时由 selector 返回明确 key，不依赖 collection view 自动处理 `null`。
- `GroupListItemData` 是展示层合成项，不得写回用户源集合或被当作业务数据模型扩展点。
- 分组与分页组合时必须保持 `PrepareTemporaryGroups` 和 `PrepareGroupsForCurrentPage` 的顺序，不得只对当前页局部数据直接推断全局分组顺序。
- `PrepareContainerForItemOverride` 新增状态同步时，必须在 `ClearContainerForItemOverride` 或 virtualizing clear 路径中对称清理。
- EntryId 映射必须同时覆盖 selection change、container prepared、container index changed、auto scroll 和 selected indicator 同步。
- Add、Remove、Move 和 Replace 必须按 collection change index 更新 entries，不按 item equality 定位变化目标。
- Reset 和 ItemsSource 替换只通过唯一非空 item key 恢复选择，不按 item equality 或旧索引回退。
- 分页器替换时必须解除旧 `CurrentPageChanged` 和 relay binding。
- Token 变更必须同步 `ListViewTokenKind`、AXAML 引用和 token.md 语义说明。
- Semantic Part 边界：`ListView` 只发布 `root` / `item` / `groupHeader` 三个 Part；`item` 与 `groupHeader` 的 marker
  分别在 `ListViewItem` 与专用 `GroupHeaderItem` 容器构造时一次性建立，不随 `IsGroupItem` 状态切换，静态模板不增删
  marker，默认主题不消费 `.semantic-*` selector；selection、pagination、empty/loading 与条目 content 不属于 Part。
- 分割线状态机：`IsSplitLineVisible` 只由 ListView 按容器视图位置与 `BottomPagination` / `IsBorderless` 计算，容器
  不得自行决定；集合变化、分页器与 borderless 变化必须重新同步已实现容器；Semantic Style 不能绕过最后一项抑制。
- 条目模板保持 `SplitLineFrame` 绑定 `EffectiveBorderThickness` / `IsSplitLineEffectiveVisible`，默认分割线为 1 DIP
  `ColorSplit`；root 外框与分割线共享同一 `ColorSplit` 基线。
- root 模板保持 `Frame` 开启 `ClipContentToCornerRadius`：内容被裁剪到外框圆角内边缘，条目状态背景不溢出圆角
  口袋区；外框环由 `Frame` 自身一次绘制（`ColorSplit`，与分割线同色），不得再叠加任何覆盖节点。裁剪应用前必须通过
  `SupportsGeometryClipHitTesting` 探测平台几何命中能力，无法正确判定圆角几何包含的平台降级为不应用裁剪。
- 条目表面保持直角与贴边：不恢复条目圆角，`ContentPadding` / `ItemMargin` 保持 `Thickness(0)`。

## 10. 测试与验证

ListView 相关验证入口：

- `tools/performances/AtomUI.Performance/Suites/ListView/ListViewStateVerification.cs`
- `tools/performances/AtomUI.Performance/Suites/ListView/ListViewScenarios.cs`
- `controlgallery/AtomUIGallery/ShowCases/DataDisplay/List/`
- `tests/AtomUI.Controls.Shared.Tests/` 中与 collection view、排序、过滤或数据成员访问器相关的测试。
- `tests/AtomUI.Desktop.Controls.Tests/` 中与 ListView、Selection 或 Pagination 相关的测试。

建议验证命令：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj -c Debug --framework net10.0 --no-restore -- --verify-listview-states
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj -c Debug --framework net10.0 --no-build -- --suite listview --count 60
dotnet test tests/AtomUI.Controls.Shared.Tests/AtomUI.Controls.Shared.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore
git diff --check
```

验证重点：

- ItemsSource 包装、替换、entry-capable view 直连、普通 `IListCollectionView` 重新归一和各自生命周期。
- 默认列表、空列表、操作态、分组、过滤、排序和分页显示。
- selected indicator 生命周期：选中、取消选中、切换选中项和虚拟化回收。
- selection 生命周期：重复 item、单选、多选、AlwaysSelected、`IsSelectable=false`、SelectedValue、Reset key 恢复、分页和分组。
- pagination 生命周期：分页器替换、可见性、对齐、motion、page size、current page 和 detach 后不再被 mutation。
- 虚拟化回收后 disabled、group item、content template 和 selected 状态不串扰。
- Semantic Part 边界：N 个条目时 `semantic-item` 共 N 个、`semantic-group-header` 等于组数；空集合为 0；分组开关、
  容器复用/回收与 items 变化不增删 marker；root 表面投影生效；默认主题不消费 `.semantic-*` selector。
- 分割线与内容裁剪：hover / selected 背景不溢出圆角内边缘（被 `Frame` 的圆角内容裁剪约束）；无 `BottomPagination`
  时最后一项无分割线且外框下边缘为单一闭合线；`BottomPagination` / `IsBorderless` 时最后一项保留分割线；追加 / 删除
  条目后原最后一项恢复、新最后一项抑制；组标题无分割线；三档 SizeType 下规则一致。
- Gallery List ShowCase 的基础和进阶示例显示稳定。

## 状态动效初始化

`ListViewItem` 在构造阶段抑制过渡，覆盖 owner 准备容器时写入的初始选中外观；加载后恢复最新集合。父控件在清理容器之前复位会话，避免离树前的清理写入启动过渡。
共享目标与生命周期契约见 [状态动效初始化架构](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)。
