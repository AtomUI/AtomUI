# ListView 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.ListView` 桌面版的最新设计定位、公共契约、行为状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，公共语义区域见 [ListView Semantic Part 契约](semantic-part.md)，内部实现原理见 [ListView 桌面版实现原理](implementation.md)，选择专项设计见 [ListView 选择模型设计](selection-model-design.md)，ListView Token 的专项设计见 [ListView Token 设计](token.md)，设计和契约变化记录见 [ListView Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/List` |
| 控件状态 | Stable |

ListView 是桌面端数据展示类列表视图控件，用于展示一组数据项，并在同一控件内提供选择、分组、排序、过滤、分页、空状态和操作中状态。它以 Avalonia `ItemsControl` 为基础，使用 AtomUI 的 `ListCollectionView` 管理数据视图，并用 `ListViewItem` 作为条目容器。

ListView 的职责是把用户提供的 `ItemsSource` 归一到可排序、可过滤、可分组、可分页的列表视图，并把视图状态同步到容器、选择模型和分页器。它不负责树形层级、表格列模型、远程数据请求、跨控件业务命令编排或候选列表提交逻辑；这些场景应分别使用 TreeView、DataGrid、业务组合控件或 Select / AutoComplete 等候选列表控件。

ListView 支持两类数据入口：

- `ItemsSource` 绑定任意 `IEnumerable`，控件会归一为具备 source-entry 映射能力的 AtomUI `ListCollectionView`。只有实现 internal entry bridge 的 collection view 可以直接接入。
- 直接 `Items` 适合少量静态条目，直接条目需要符合 ListView 的数据项语义。

推荐数据项实现 `IListItemData`，以便默认模板、禁用状态、分组字段和默认内容展示保持一致。`IListItemData` 不承载 ListView 运行时选择状态；复杂业务对象应提供 `ItemTemplate`、`GroupItemTemplate`、`FilterValueSelector` 和 `SortDescriptions`。

## 2. 设计语言

ListView 表达的是“数据视图 + 可操作列表项”的信息浏览语义。用户应能通过紧凑纵向结构、分组标题、hover 背景、selected 背景、可选选中标记、分页位置和操作中遮罩理解当前数据范围与交互目标。

设计语言由以下维度组成：

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 数据视图 | 原始集合经过过滤、排序、分组和分页后形成当前可见列表。 | `ListCollectionView`、`TotalItemCount`、`PageIndex`、`PageSize`。 |
| 列表结构 | 同级条目纵向排列，默认使用虚拟化面板。 | ScrollViewer、ItemsPresenter、VirtualizingStackPanel。 |
| 分组结构 | 组标题作为不可选择的列表项参与视图展示。 | `IsGroupEnabled`、`GroupItemTemplate`、组标题前景色。 |
| 选择状态 | 以 source entry 为单位表示的当前业务选中目标或选中集合。 | `Selection`、`SelectedIndex`、`SelectedIndexes`、`SelectedItem`、`SelectedItems`、selected 背景、选中指示器。 |
| 过滤状态 | 外部条件筛选当前视图数据。 | `Filter`、`FilterValue`、`IsFiltering`、`FilterContextChanged`。 |
| 分页状态 | 当前视图仅展示指定页数据。 | `TopPagination`、`BottomPagination`、`PaginationVisibility`。 |
| 空状态 | 无数据或过滤后无结果。 | `EmptyIndicator` 内容区域。 |
| 操作中状态 | 数据处理或外部操作期间覆盖列表。 | `Spin`、`OperatingMsg`、`CustomOperatingIndicator`。 |

ListView 的视觉强度应保持数据展示控件的克制感。条目不是卡片，分组标题不是额外列表控件，分页器也不改变列表数据语义，只表达当前视图页边界。

## 3. API 与契约模型

ListView 的公共契约由数据视图 API、选择 API、分页 API、视觉 API、事件 API、template part、伪类和主题入口组成。

数据视图 API：

| API | 语义 |
| --- | --- |
| `ItemsSource` / `Items` | 数据入口。`ItemsSource` 会归一为 `IListCollectionView`。 |
| `SortDescriptions` | 排序描述集合，交给 `IListCollectionView.SortDescriptions` 执行。 |
| `Filter` / `FilterValue` / `FilterValueSelector` | 过滤谓词、过滤条件和值选择器。 |
| `IsFiltering` | 当前是否存在有效过滤描述。 |
| `TotalItemCount` | 当前视图总数据量，用于空状态、伪类和分页同步。 |

分组 API：

| API | 语义 |
| --- | --- |
| `IsGroupEnabled` | 是否把当前视图按 group key 分组。 |
| `GroupPropertySelector` | 从数据项提取 group key；默认读取 `IGroupHeader.Group`。 |
| `GroupItemTemplate` | 组标题项模板，数据上下文为 collection view 插入的组标题数据项。 |

ListView public 分组模型是单层分组模型。`IsGroupEnabled=true` 时，ListView 会向 collection view 写入一个 `ListGroupDescription`，该描述由 `GroupPropertySelector` 提供 group key。`IListCollectionView.GroupDescriptions` 支持列表形式，但 ListView 当前 public API 不暴露多层分组配置；维护者不应把多层分组行为误写成 ListView public 契约。

选择 API：

| API | 语义 |
| --- | --- |
| `IsSelectable` | 是否允许用户通过 pointer 或 keyboard 更新选择。关闭时清空当前选择。 |
| `SelectionMode` | 选择模式，支持单选、多选、Toggle 和 AlwaysSelected 语义。 |
| `Selection` | ListView 持有的选择状态 owner。调用者可以执行选择命令，但不能替换模型或设置其数据源。 |
| `SelectedIndex` / `SelectedIndexes` | 选中 entry 的 source-index 输入与只读集合投影；排序、过滤、分组和分页不改变索引语义。 |
| `SelectedItem` / `SelectedItems` | 从选中 entries 派生的只读 item 投影；结果允许包含重复引用或重复值。 |
| `SelectedValue` / `SelectedValueBinding` | 从 `SelectedItem` 派生只读值，不按值反向查找条目。 |
| `ItemKeySelector` | 可选的稳定 `EntityKey` 提取器，仅用于 Reset 或 ItemsSource 替换后的选择恢复。 |
| `AutoScrollToSelectedItem` | 首次模板和视觉树就绪后滚动到 anchor 项。 |
| `IsTextSearchEnabled` | 是否启用文本增量搜索。 |
| `WrapSelection` | 键盘方向导航是否循环。 |

分页 API：

| API | 语义 |
| --- | --- |
| `PageIndex` | 当前页索引，内部与 `IListCollectionView.PageIndex` 同步。 |
| `PageSize` | 页大小，`0` 表示不分页。 |
| `TopPagination` / `BottomPagination` | 可注入的顶部和底部分页器。 |
| `PaginationVisibility` | 分页器显示位置：`None`、`Top`、`Bottom`、`Both`。 |
| `TopPaginationAlign` / `BottomPaginationAlign` | 对应分页器对齐方式。 |
| `IsHideOnSinglePage` | 透传给分页器的单页隐藏策略。 |

视觉与操作 API：

| API | 语义 |
| --- | --- |
| `SizeType` | 控制 root 圆角、空状态 padding、条目高度和 padding。 |
| `IsBorderless` | 是否隐藏 root 边框。 |
| `ItemHoverBg` / `ItemSelectedBg` | 条目 hover 和 selected 背景入口。 |
| `IsShowSelectedIndicator` / `SelectedIndicator` | 选中标记开关和图标模板。 |
| `IsMotionEnabled` | 是否启用条目和分页器动效。 |
| `EmptyIndicator` / `EmptyIndicatorTemplate` / `EmptyIndicatorPadding` / `IsShowEmptyIndicator` | 空状态展示入口。 |
| `IsOperating` / `OperatingMsg` / `CustomOperatingIndicator` / `CustomOperatingIndicatorTemplate` | 操作中遮罩和自定义指示器入口。 |
| `ItemClickMode` | 条目点击事件触发时机。 |

事件 API：

| 事件 | 语义 |
| --- | --- |
| `ItemClicked` | `ListViewItem.Clicked` 冒泡到 ListView 后触发。 |
| `ItemCountChanged` | `TotalItemCount` 变化后触发。 |
| `FilterContextChanged` | 过滤描述重新配置后触发。 |
| `SelectionChanged` | 选择模型变化后按 Avalonia routed event 形式触发。 |
| `IsSelectedChanged` | 容器 attached selected 状态变化的 routed event。 |

稳定模板命名和职责：

| 名称 | 所属控件 | 职责 |
| --- | --- | --- |
| `Frame` | `ListView` | root 背景、边框、圆角、padding、margin 与外框闭合边界。 |
| `TopPaginationPresenter` | `ListView` | 顶部分页器展示入口。 |
| `BottomPaginationPresenter` | `ListView` | 底部分页器展示入口。 |
| `PART_ScrollViewer` | `ListView` | 列表滚动容器。 |
| `ItemsPresenter` | `ListView` | 条目容器承载入口。 |
| `EmptyIndicator` | `ListView` | 空状态内容展示。 |
| `Frame` | `ListViewItem` | 条目背景与 padding 边界。 |
| `SplitLineFrame` | `ListViewItem` | 条目底部分割线表面。 |
| `SelectedIndicator` | `ListViewItem` | 选中标记展示入口。 |
| `ContentPresenter` | `ListViewItem` | 普通内容或组标题内容展示入口。 |

伪类契约：

- `:empty` 表示 `TotalItemCount == 0`。
- `:singleitem` 表示 `TotalItemCount == 1`。
- `ListViewItem` 继承 Avalonia 选择、hover、pressed、focus 和 disabled 相关伪类语义，其中 `:selected` 由 `ListView.IsSelected` attached property 驱动。

## 4. 行为与状态模型

ListView 的状态模型由数据视图状态、选择状态、分页状态、过滤状态、分组状态、空状态、操作状态、尺寸状态和动效状态组成。

数据视图状态：

- `ItemsSource` 设置后归一为具备 entry bridge 的 `IListCollectionView`，ListView 监听其 `CollectionChanged`、`PropertyChanged`、`PageChanging` 和 `PageChanged`。不具备 entry bridge 的自定义 view 从其 `SourceCollection` 重新归一。
- `SortDescriptions`、`Filter` / `FilterValue` / `FilterValueSelector`、`IsGroupEnabled` / `GroupPropertySelector` 分别写入 collection view 的排序、过滤和分组描述。
- `TotalItemCount` 和 `IsEmptyDataSource` 来自 collection view，用于伪类、空状态和分页器同步。

选择行为：

- `IsSelectable=false` 时，ListView 不响应 pointer 或 keyboard 选择更新，并清空 canonical selection。
- 数据源中的每一次出现拥有独立 internal EntryId；item 引用、`Equals`、`GetHashCode` 和显示值不参与源条目标识与选择映射。
- `ListViewSelectionModel` 持有 selected EntryIds、anchor 和 active entry；`SelectedIndex(es)`、`SelectedItem(s)`、`SelectedValue` 和容器 `IsSelected` 都是同一状态的投影。
- 公开选择索引始终表达原始数据源的 source index。排序、过滤、分组和分页通过 entry projection 映射容器，不通过 item `IndexOf` 反查源位置。
- 组标题是没有 EntryId 和 source index 的视图合成节点，所有选择入口均跳过组标题。
- Reset 或 ItemsSource 替换只通过唯一、非空 item key 恢复选择；没有稳定 key 的选择清除。
- `SelectionMode.AlwaysSelected` 在存在数据且丢失选择时恢复到首项。
- 完整选择、集合变化和恢复语义见 [ListView 选择模型设计](selection-model-design.md)。

键盘和文本搜索行为：

- 方向键按 Avalonia navigation direction 移动选择，`WrapSelection` 控制边界循环。
- 多选模式下，平台 select-all 手势调用 `Selection.SelectAll()`。
- Space / Enter 会尝试从事件源更新选择。
- `IsTextSearchEnabled=true` 时，文本输入按 `TextSearch.TextBinding` 或 `DisplayMemberBinding` 做前缀搜索，搜索词由短定时器清空。

过滤行为：

- `Filter != null && FilterValue != null` 时，ListView 进入过滤态并向 collection view 写入 `ListFilterDescription`。
- `FilterValueSelector` 优先用于提取过滤值；未提供时默认读取 `IListItemData.Content`。
- 过滤改变后触发 `FilterContextChanged`，并刷新空状态。

分组行为：

- `IsGroupEnabled=true` 时，ListView 用 `GroupPropertySelector` 构造 `ListGroupDescription` 并写入 collection view。
- 默认 `GroupPropertySelector` 读取 `IGroupHeader.Group`，因此推荐分组数据项实现 `IListItemData` / `IGroupHeader` 并提供稳定、非空的 `Group` 值。
- collection view 会为每个 group key 插入一个 `GroupListItemData` 作为组标题项，其 `Content` 来自 `groupKey.ToString()`，`IsGroupItem=true`。
- 组标题项使用 `GroupItemTemplate`，普通数据项仍使用 `ItemTemplate`。
- 组标题项参与当前视图枚举和容器生成，但它表达的是视觉分隔，不是业务数据项；pointer selection 路径会跳过 `IsGroupItem=true` 的容器。
- 分组开启后，selection model 仍以业务 source entries 为选择范围；当前 view projection 只增加没有 EntryId 的组标题节点。
- 与分页同时使用时，collection view 先按完整结果建立临时分组顺序，再按当前页重建对外可枚举的 group 结构。

分页行为：

- `PageSize=0` 表示不分页；`PageSize>0` 时 collection view 只枚举当前页。
- `TopPagination` 和 `BottomPagination` 接收当前 `Total`、`PageSize`、`CurrentPage`、对齐、启用状态、动效和单页隐藏策略。
- 分页器的 `CurrentPageChanged` 请求转换为 `IListCollectionView.MoveToPage(page - 1)`。
- `PaginationVisibility` 只控制分页器可见性，不改变 collection view 的分页数据。

空状态和操作态：

- `IsShowEmptyIndicator && TotalItemCount == 0` 时显示空状态。
- 空状态显示时隐藏滚动列表区域。
- `IsOperating=true` 时，root 模板内的 `Spin` 覆盖列表和空状态，用于表达外部操作中状态。

动效行为：

- ListViewItem 初始化阶段禁用 transitions，loaded 后启用，避免容器首次准备时出现非预期动画。
- 虚拟化容器保存、恢复和清理期间会临时关闭 motion，避免回收状态产生过渡。

## 5. 视觉与主题模型

ListView 主题按 root、分页、操作态、滚动内容、空状态和 item 两层组织。

```text
ListViewTheme
  Frame（ClipContentToCornerRadius=True）
  TopPaginationPresenter
  BottomPaginationPresenter
  Spin
    PART_ScrollViewer
      ItemsPresenter
    EmptyIndicator

ListViewItemTheme
  Frame
    SelectedIndicator
    ContentPresenter
  SplitLineFrame
```

视觉规则：

- root 外框由 `BorderBrush`、`BorderThickness`、`CornerRadius` 和 `IsBorderless` 共同决定。默认外框颜色是 `ColorSplit`，与条目分割线同色，表达“外框即列表闭合线”。
- root `Frame` 开启 `ClipContentToCornerRadius`：内容被裁剪到外框圆角内边缘，条目 hover / selected 背景等溢出圆角内边缘的内容被裁剪，不在圆角口袋区溢出；外框环由 `Frame` 自身一次绘制（`ColorSplit`），无叠加节点。
- `SizeType` 控制 root 圆角、空状态 padding、条目最小高度和条目 padding。条目表面保持直角，主题不设置条目圆角。
- 条目直接贴合 root 外框内边缘：`ContentPadding` 与 `ItemMargin` 均为 `Thickness(0)`，条目之间不留垂直间距，列表紧凑感由条目高度与分割线表达。
- 条目底部分割线默认是 1 DIP `ColorSplit` 底边线，由条目 `BorderThickness` / `BorderBrush` 驱动。无 `BottomPagination` 时最后一项的底部分割线被抑制，由 root 外框下边缘承担闭合线；配置 `BottomPagination` 时最后一项保留分割线（分隔条目区与分页器）；`IsBorderless` 时也保留（外框消失后由分割线承担闭合线）。
- 默认条目背景透明，hover 使用 `ItemHoverBg`，selected 使用 `ItemSelectedBg`；hover / selected 背景延伸到外框内边缘，溢出圆角口袋区的部分由 `Frame` 的圆角内容裁剪约束。
- 组标题使用 `GroupHeaderColor`，不应用普通条目的 hover / selected 状态背景，也不显示底部分割线。
- selected indicator 默认使用 `CheckOutlined`，颜色和尺寸来自 SharedToken。
- 空状态默认使用 `Empty` 的 simple preset image。
- 分页器 margin 使用 ListViewToken 的 `PaginationMargin`。

ListViewToken 提供内容 padding、条目文字颜色、条目状态背景、条目 padding、条目 margin、分页器 margin、组标题色和选中指示器 margin。Token 详情见 [ListView Token 设计](token.md)。

## 6. 控件家族或集成关系

ListView 属于 Data Display 分类，与 ListBox、TreeView、DataGrid、Card、Descriptions 等控件共同服务结构化数据展示。

集成关系：

- Avalonia `ItemsControl`：提供 Items、ItemsSource、容器生成、ItemsPresenter 和默认虚拟化入口。
- `ListCollectionView`：提供排序、过滤、分组、分页、当前视图枚举和源集合桥接。
- `ListViewItem`：条目容器，承载内容、组标题状态、选择状态、点击事件、pointer 输入和 selected indicator；非分组条目
  容器是 Semantic Part `item` 的节点。分组标题使用专用容器类型 `GroupHeaderItem`（继承 `ListViewItem`，
  `IsGroupItem` 恒为 true），是 Semantic Part `groupHeader` 的节点；两类容器的 marker 在构造时一次性建立。
- `IListItemData` / `ListItemData` / `GroupListItemData`：默认数据项和组标题数据契约。
- `ListViewSelectionModel`：选择状态 owner，以 EntryId 保存选择并向外投影 source index 和 item。
- `AbstractPagination`：顶部和底部分页器的交互与显示协作对象。
- `Spin` / `Empty`：操作态和空状态视觉组件。

ListView 与 ListBox 有相似条目视觉语义，但职责不同。ListBox 是轻量列表选择和候选列表基座；ListView 是数据视图列表，拥有 collection view、分组、排序和分页模型。

## 7. 兼容性不变量

维护 ListView 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 ListView / ListViewItem public API、事件、Avalonia 属性语义和默认值。
- `ItemsSource` 到 `IListCollectionView` 的归一化必须保持排序、过滤、分组、分页、entry projection 和选择映射可用。
- 只有具备 entry bridge 的用户 view 可以直接接入，且不由 ListView 当作自建 view 释放；其他 view 使用其 `SourceCollection` 创建由 ListView 持有的 AtomUI view。
- `SortDescriptions`、`FilterDescriptions` 和 `GroupDescriptions` 的重建必须保持 collection view 状态一致，不留下重复描述。
- `IsSelectable=false` 必须阻止用户选择更新，并清空当前选择。
- 组标题项必须保持不可通过普通 pointer selection 路径选中。
- 源条目标识必须来自 source entry，不能以 item equality、对象引用、source index 或 view index 替代 EntryId。
- 公开选择索引必须始终使用 source index；view index 和页内索引只能作为当前投影位置。
- 过滤、排序、分组和分页不得替换已选 entry；隐藏 entry 再次可见时必须恢复选中投影。
- Reset 和 ItemsSource 替换不得按 item equality 或相同索引猜测旧选择。
- `PaginationVisibility` 只表达分页器可见性，不改变分页数据和分页器 motion 状态语义。
- `PART_ScrollViewer`、`ItemsPresenter`、`EmptyIndicator`、分页 presenter、`SelectedIndicator` 和 `ContentPresenter` 的职责不得被无兼容说明地改变。
- 选中指示器、空状态和操作态节点应留在 AXAML 静态模板中，通过状态属性控制，不作为普通性能优化迁移到 C# 动态创建。
- 虚拟化容器回收时，容器本地值必须对称保存、恢复和清理，避免 disabled、group item、selected indicator 或 content 状态串扰。
- `ListView` 的 `root` / `item` / `groupHeader` Semantic Part 对齐上游 Listy 的 Semantic DOM：marker 放置、ContractType、
  cardinality 与根表面投影属于主题兼容契约，状态切换、容器复用/回收与模板重应用不得增删 marker；`item` 与
  `groupHeader` 的 marker 分别在 `ListViewItem` 与专用 `GroupHeaderItem` 容器构造时一次性建立，不随
  `IsGroupItem` 状态切换。
- root 外框与条目分割线必须保持同一 `ColorSplit` 基线；root `Frame` 的 `ClipContentToCornerRadius` 圆角内容裁剪保证条目状态背景不溢出圆角内边缘。
- 条目表面保持直角与贴边：不恢复条目圆角，不重新引入 `ContentPadding` / `ItemMargin` 垂直间距。
- 最后一项分割线抑制规则由控件状态机决定：无 `BottomPagination` 时最后一项不显示底部分割线，由 root 外框下边缘
  闭合；`BottomPagination` 或 `IsBorderless` 时保留；集合追加、删除与分页配置变化后必须重新同步所有已实现容器，
  Semantic Style 不能绕过该规则。
- Token 只表达组件设计语义，不承载选择、过滤、分页、空状态或虚拟化运行时状态。

## 8. 专项模型

### 8.1 Collection View 模型

ListView 的数据视图能力由 `IListCollectionView` 承担。ListView 负责把 public API 转换为 collection view 描述并监听视图变化；排序、过滤、分组、分页和源集合变化处理不应内聚到条目容器或模板层。

`ListCollectionView` 对原始集合保留 `SourceCollection`，同时根据排序、过滤、分页和分组需求维护当前视图。维护 ListView 时应区分“用户输入源集合”和“控件内部视图”，避免把二者的生命周期、绑定优先级和事件订阅混为同一个概念。

### 8.2 分组列表模型

分组开启后，ListView 通过 `ListGroupDescription` 从数据项提取 group key，并在当前视图中插入组标题项。组标题项是 collection view 生成的 `GroupListItemData`，其 `Content` 是 group key 的字符串形式，`IsGroupItem=true`。容器准备阶段会把该标记同步到 `ListViewItem.IsGroupItem`，主题再用该状态切换弱化文字色、禁用普通 hover / selected 背景，并选择 `GroupItemTemplate`。

该模型只改变当前视图展示和选择索引来源，不改变原始数据集合本身。分组数据项必须提供稳定 group key；当前实现不会为 `null` group key 自动创建默认分组。需要“未分组”或“其他”类别时，应由数据层或 `GroupPropertySelector` 返回明确 key。

分组、排序、过滤和分页的组合顺序由 collection view 维护：过滤和排序先决定候选集合与顺序，分组在该结果上生成 group 结构；分页开启时，当前页只暴露该页范围内需要展示的组标题和数据项。

分组模式下，维护者必须区分三类对象：

- 原始业务数据项，来自用户 `ItemsSource`。
- collection view 生成的组标题数据项，类型为 `GroupListItemData`。
- `ListViewItem` 容器，它只表达当前 view item 的视觉和交互状态。

组标题数据项不应被写回用户源集合，也不应作为业务选中值对外承诺。

### 8.3 选择模型

ListView 为原始数据源中的每一次出现建立独立 source entry，并用 internal EntryId 连接 source index、当前 view projection、选择模型和容器。选择模型保存 EntryId；公开 `SelectedIndex(es)` 投影为 source index，`SelectedItem(s)` 投影为对应业务数据。相同 item 引用、相同值和自定义相等语义不合并 source entries。

过滤、排序、分组和分页只改变 view projection。业务 entry 在当前投影中不可见时仍可保持选择；组标题没有 EntryId，不进入选择结果。Reset 和 ItemsSource 替换建立新的 entry 生命周期，只通过唯一 item key 恢复逻辑选择。完整契约见 [ListView 选择模型设计](selection-model-design.md)。

### 8.4 操作态模型

`IsOperating` 不改变数据、选择或分页状态，只在视觉上通过 `Spin` 覆盖当前列表内容。外部异步加载、刷新或批量操作应通过此模型表达忙碌状态，而不是在 ListView 内部引入远程请求或任务编排。

## 9. 文档导航与验证策略

文档导航：

- [ListView Semantic Part 契约](semantic-part.md)
- [ListView 桌面版实现原理](implementation.md)
- [ListView 选择模型设计](selection-model-design.md)
- [ListView Token 设计](token.md)
- [ListView Changelog](changelog.md)

验证策略：

| 分层 | 验证要求 |
| --- | --- |
| Public API | 检查 ListView / ListViewItem 属性、事件、默认值、选择模型和分页 API 符合当前契约。 |
| 数据状态 | 覆盖 ItemsSource 包装、entry-capable view 直连、普通 `IListCollectionView` 重新归一、排序、过滤、分组、分页和 `TotalItemCount`。 |
| 分组状态 | 覆盖默认 group selector、自定义 group selector、空 group key、组标题模板、组标题 pointer 不可选、排序 / 过滤 / 分页组合和虚拟化回收。 |
| 选择状态 | 覆盖重复 item、source-index 投影、selectable、AlwaysSelected、多选、Reset key 恢复、视图组合、SelectedValue 和 text search。 |
| AXAML | 检查 root、pagination、Spin、ScrollViewer、EmptyIndicator、item 和 group item 在 light / dark 和三种 SizeType 下显示稳定，外框 / 分割线 / 圆角内容裁剪规则一致。 |
| Token | 检查 `ListViewTokenKind`、AXAML token resource 和 token.md 语义说明保持一致。 |
| 虚拟化 | 覆盖 container prepare / clear、上下滚动后 disabled、group item 和 selected 状态不串扰。 |
| 文档 | 运行 `git diff --check`，确认链接存在且只记录最新设计状态。 |
