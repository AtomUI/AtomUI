# DataGrid 桌面版架构设计

本文档定义 `DataGrid` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [DataGrid 桌面版实现原理](implementation.md)，查询、范围数据源与虚拟化见 [DataGrid Query 与 Range Source 设计](query-range-source-design.md)，列宽测量与分配见 [DataGrid 列宽分配设计](column-sizing-design.md)，分页面板扩展区域见 [DataGrid 分页面板 Extra Content 设计](pagination-extra-content-design.md)，DataGrid Token 的专项设计见 [DataGrid Token 设计](token.md)，设计和契约变化记录见 [DataGrid Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `DataGrid`，其 internal `IsPopupPinnedOpen` 只供测试和内部诊断使用；设置为 true 时按 DisplayIndex 选择第一个有效列过滤入口，并通过 Header、FilterIndicator 和 Flyout relay 到具体 Popup，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变、目标列替换和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls.DataGrid` |
| .NET 命名空间 | `AtomUI.Desktop.Controls.DataGrid` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/DataGrid` |
| 控件状态 | Stable |

DataGrid 是 AtomUI 桌面控件体系中的数据表格控件，用于列模型、行选择、排序、过滤、分组、编辑、冻结列、连续虚拟滚动和分页展示。本地集合与远端服务通过同一个范围数据源契约接入。

DataGrid 不负责简单列表、树视图或业务数据访问层。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls.DataGrid`

## 2. 设计语言

DataGrid 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | DataGrid 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | DataGrid 以不可变 Query 描述数据语义，以 Range Source 支持本地或远端数据，并只物化当前视口。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ItemsSource : IDataGridSource?`、`Query`、`GroupExpansion`、列 `FieldId`、`CellTemplate` 和 `CellEditingTemplate`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | AppliedQuery、LoadState、Selection、CurrentRowKey、sort/filter/group projection、motion 和 visual option。 |
| 主题语义 | ControlTheme、SharedToken、Control Own Token 和模板绑定如何表达视觉。 | DataGrid Token + ControlTheme。 |

## 3. API 与契约模型

DataGrid 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ItemsSource`、`Query`、`AppliedQuery`、`GroupExpansion`、`TotalItemCount`、`TotalEntryCount`、`AutoGenerateColumns`、`CellTemplate`、`CellEditingTemplate` | 定义数据输入、查询、范围 presentation、模板和业务对象入口；`ItemsSource` 的类型是 `IDataGridSource?`。 |
| 选择与当前项 | `Selection`、`CurrentRowKey`、`SelectionChanged`、`ClipboardCopyMode` | 以稳定 row key、query scope 和 index interval 维护可跨 range/page 的声明式状态。 |
| 交互与加载 | `CanUserFilterColumns`、`CanUserReorderRows`、`CanUserResizeColumns`、`CanUserSortColumns`、`QueryChanged`、`LoadState`、`LoadError`、`IsDataStale`、`Reload()` | 表达用户查询意图、异步生命周期、错误与可提交能力。 |
| 视觉与布局 | `BottomPaginationAlign`、`ColumnWidth`、`HorizontalAlignment`、`HorizontalScrollBarVisibility`、`IsShowPageSizeSelector`、`MaxColumnWidth`、`MinColumnWidth`、`RowHeight`、`SeparatorBrush`、`SizeType`、`SublevelIndent` 等 15 项 | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 分页面板扩展 | `TopPaginationExtraContent`、`TopPaginationExtraContentTemplate`、`BottomPaginationExtraContent`、`BottomPaginationExtraContentTemplate` | 在顶部/底部分页区域提供与 Pagination 相反一侧的独立 Extra Content；默认均为 `null`，不拥有分页状态。 |
| 列查询契约 | `FieldId`、`CanUserSort`、`SupportedSortDirections`、只读 `SortState`、过滤候选展示属性 | 让列声明协议字段与能力覆盖，显示 Binding 不参与查询 identity。 |
| 其他稳定入口 | `CellTheme`、`CustomOperatingIndicator`、`EmptyIndicator`、`Footer`、`FormatString`、`GridLinesVisibility`、`Level`、`Maximum`、`Minimum` 等 | 保留非数据架构 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

`ItemsSource` 只接受 `IDataGridSource?`，不是旧集合型 `IEnumerable` 入口。本地集合使用 `DataGridLocalSource.Create(...)`
建立强类型 range source，远端实现使用同一接口翻译 Query 并按请求范围返回数据；DataGrid 不拥有或 dispose 外部 Source。

稳定事件包括 `QueryChanged`、`SelectionChanged`、`RowReordering` 和 `RowReordered`。`QueryChanged` 是不可取消的观察事件，在 Query 的内存提交与视觉投影之后、对应 Source 请求之前触发；其他事件的参数和触发顺序同样属于契约。

行拖动重排由 `CanUserReorderRows`、`DataGridRowReorderColumn`、`RowReordering`、`RowReordered` 和可选
`IDataGridMovableSource` 共同表达。DataGrid 使用 source row key 与目标 before/after key 提交移动，不把 view、window 或 display
索引解释为 Source 集合索引。Source 不支持移动、当前 Query 语义禁止移动、编辑未能提交或 snapshot 失效时，handle 不进入
可提交状态，也不发布 `RowReordered`。

分页公共契约由 `PageSize`、`IsShowPageSizeSelector`、`PaginationVisibility`、`TopPaginationAlign`、
`BottomPaginationAlign` 和 `IsHideOnSinglePage` 共同表达。`IsShowPageSizeSelector` 默认为 `true`，转发到顶部和底部
`Pagination.IsShowSizeChanger`；它只控制页大小选择器可见性，不创建独立的 DataGrid 页大小状态。`PageSize=0` 表示连续模式；
非零值形成 `DataGridPageRequest`，其中 data start 使用 long、单页 count 使用 int。分页总数只来自已提交 Source result 的
`TotalDataCount`，顶部和底部 Pagination 只投影同一份 applied page state。翻页成功后垂直 offset 归零；失败时页码、rows、
totals、scrollbar 和 Query 共同回退到最后成功 presentation。

分页面板可以在 Pagination 对侧提供 Extra Content。`Start` 对齐时 Pagination 左置、Extra Content 右置；`End` 对齐时
Extra Content 左置、Pagination 右置；`Center` 对齐时 Extra Content 右置，Pagination 在剩余区域内居中。顶部和底部
Extra Content 使用独立 Content/Template 属性，完整契约见 [DataGrid 分页面板 Extra Content 设计](pagination-extra-content-design.md)。

列宽公共契约由 `DataGrid.ColumnWidth`、`DataGridColumn.Width`、控件级与列级最小/最大宽度，以及
`DataGridLengthUnitType` 共同表达。`ColumnWidth` 默认为 `Auto`；单列可以使用 `Pixel`、`Auto`、
`SizeToHeader`、`SizeToCells` 或 `Star`。内容驱动模式先形成期望宽度，star 模式再按权重分配有限列视口中的
剩余空间。列宽状态与统一分配算法由 `DataGrid` 持有，普通表头、分组表头和 rows/cells presenter 只提供当前
布局可证明的有限宽度或内容测量结果。空数据时列宽求解不能依赖已隐藏的 rows presenter，完整契约见
[DataGrid 列宽分配设计](column-sizing-design.md)。

列过滤候选项与过滤条件分离。列 `Filters` 只负责菜单或树形弹层中的候选内容，可以绑定 ViewModel 集合或数据库返回的
DTO；`DataGrid.Query.Filters` 是已应用条件的唯一 owner。Flyout checked state、FilterIndicator 激活态与 Source request 都从
Query 投影，用户确认后一次性生成新的不可变 Query。过滤 DTO 的显示成员可以使用 generated data member accessor；Query 中的
值则转换为确定性的 `DataGridScalar`，不保留可变 object、predicate 或反射 path。

过滤交互模式使用显式枚举表达：`FilterPresenterMode` 表达菜单或树形弹层，`FilterSelectionMode` 表达单选或多选，`FilterApplyMode` 表达确认、关闭或选择变化时应用过滤。枚举状态比布尔开关更适合作为长期 public API，因为它能把展示方式、选择方式和提交时机拆成三个正交状态，避免布尔组合在状态流中产生歧义。

主要公开类型与枚举：

- 类型：`DataGrid`、`DataGridQuery`、`IDataGridSource`、`DataGridSourceSchema`、`DataGridFieldSchema`、`DataGridFieldDisplayAccessor`、`DataGridRange`、`DataGridPageRequest`、`DataGridRangeResult`、`DataGridSelectionState`、`DataGridColumn`、`DataGridRow` 和 `DataGridCell` 等。
- 枚举：`DataGridSortDirection`、`DataGridSortUpdateMode`、`DataGridLoadState`、`DataGridSourceEntryKind`、`DataGridClipboardCopyMode`、`DataGridEditingUnit`、`DataGridLengthUnitType`、`DataGridPaginationVisibility` 和 `DataGridRowDetailsVisibilityMode` 等。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Ascending` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_BottomGridLine` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_BottomPaginationExtraContentPresenter` | `ContentPresenter` | 展示底部分页面板 Extra Content。 |
| `PART_BottomPaginationPanel` | `DockPanel` | 组合底部 Extra Content 与 Pagination，并执行相反侧对齐。 |
| `PART_BottomPagination` | `Pagination` | 投影 applied page state 并转发底部翻页意图。 |
| `PART_ColumnHeadersPresenter` | `DataGridColumnHeadersPresenter` | 测量普通列头，并在空数据布局中提供有限列视口宽度。 |
| `PART_ContentFrame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_ContentPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_Descending` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_FocusVisual` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_Frame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_GroupColumnHeadersPresenter` | `DataGridGroupColumnHeadersPresenter` | 测量分组列头，并与普通列头共享列宽输入契约。 |
| `PART_HeaderPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_HorizontalIndicator` | `?` | 展示指示器、进度、分页或状态反馈。 |
| `PART_IndicatorIconButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_ItemsPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_RightGridLine` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_RootLayout` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_RowPresenter` | `DataGridRowsPresenter` | 承载已物化行；空数据时保持隐藏，不作为 star 求解的必要前置。 |
| `PART_SortIndicator` | `?` | 展示指示器、进度、分页或状态反馈。 |
| `PART_TopPaginationExtraContentPresenter` | `ContentPresenter` | 展示顶部分页面板 Extra Content。 |
| `PART_TopPaginationPanel` | `DockPanel` | 组合顶部 Extra Content 与 Pagination，并执行相反侧对齐。 |
| `PART_TopPagination` | `Pagination` | 投影 applied page state 并转发顶部翻页意图。 |
| `PART_VerticalIndicator` | `?` | 展示指示器、进度、分页或状态反馈。 |
| `PART_VerticalSeparator` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

控件专属或内部伪类包括 `DataGridPseudoClass.EmptyColumns`、`DataGridPseudoClass.EmptyRows`、`DataGridPseudoClass.FirstColumnHeader`、`DataGridPseudoClass.GroupItemLeaf`、`DataGridPseudoClass.LastColumnHeader`、`DataGridPseudoClass.MiddleColumnHeader`、`DataGridPseudoClass.RowHover`、`EmptyColumns=:empty-columns`、`EmptyRows=:empty-rows`、`FirstColumnHeader=:first-column-header`、`GroupItemLeaf=:is-leaf-item`、`LastColumnHeader=:last-column-header` 等 14 项。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

DataGrid 的状态流按以下路径收敛：

```text
Public API / inherited command / Source invalidation / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- Query、Selection、current、loading、motion 和 visual option 状态由 DataGrid 或明确 Source capability 单向推导，不能在 template part 之间双向竞争。
- 排序、过滤和分组只由 `Query` 拥有；列、Header、Cell 和 Flyout 只投影相应字段状态。
- `Filters` 候选项替换、reset 或 clear 时可以重新物化 Flyout 内容，但不能直接改变已应用 Query；用户确认或显式 API 才提交新的 Query。
- 分页状态以 applied PageRequest 和 `TotalItemCount` 为 owner；页大小选择器的输入转换为新的 `DataGridPageRequest`，保持可保留的页序号并在新页数不足时收敛到最后一页。上下 Pagination 不能互相覆盖，也不能在模板重建时反向重置 Query 或 Source。
- Extra Content 只属于 DataGrid 分页面板，不参与 Query、PageRequest、PageSize 或 Source request。它与内部 Pagination 共享有效可见性，不能在单页隐藏或连续模式下独立残留。
- Loading 没有可展示的已提交 presentation，实际挂起时驱动 Spin；Refreshing 保持旧 presentation 的几何与完整不透明度，
  不自动启动 Spin。两种状态都禁止 edit/delete/move，成功时原子交换，失败时完整回退。
- 连续滚轮、惯性或 scrollbar thumb 输入只保留最新有效 `DesiredViewport`。新目标先接管仍需要的 block，再使旧视口 scope
  失效；不再被任何有效 scope 使用的排队或执行中请求立即收到取消，旧 prefetch 不能排在新 visible range 之前。
- 同步 Source 与 cache hit 在当前调用中直接进入 Ready，不发布瞬时 Loading/Refreshing；调用方显式设置 `IsOperating=true`
  时仍可在任意 LoadState 显示 Spin。
- 行拖动状态以当前 `DataGrid` 的单一拖拽会话为 owner；handle 和 RowsPresenter 只投影输入与 ghost row，不能保存
  跨 DataGrid 共享的静态拖拽状态。Pointer capture、源 row key、Source snapshot 和目标邻接 key 必须属于同一个会话。
- `RowReordering` 在超过拖动阈值后且创建 ghost row 前触发一次；事件取消或事件回调改变 DataGrid、源行、
  Source、Query、snapshot 或移动能力时，本次 Pointer 会话保持取消状态，不得在后续移动帧重复开始。
- `RowReordered` 只在 movable Source 成功提交顺序变化，并且 ghost、capture、动画与会话状态全部清理后触发。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

DataGrid 的视觉模型由控件模板、ControlTheme、SharedToken 和 DataGrid Own Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `DataGridCellTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridColumnGroupHeaderTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridColumnHeaderTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridHeaderViewItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridOperationButtons.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `DataGridRowExpanderTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridRowGroupHeaderTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridRowHeaderTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridRowReorderHandle.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridRowTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `DataGridTopLeftColumnHeaderTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridFilterIndicatorTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `DataGridFilterMenuItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridFilterTreeItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `DataGridMenuFilterFlyoutPresenterTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |
| `DataGridSortIndicatorTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `DataGridTreeFilterFlyoutPresenterTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |

DataGrid 拥有独立 Control identity；`DataGridToken` 只表达 DataGrid Own Token 语义，不承载 selection/checked/active、collection/filter、motion 或 visual option 运行时状态。Control 级 Global Token 覆盖与 Own Token 通过 `DataGridTokenResource` 统一读取。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

DataGrid 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `DataGrid`：public data/query/selection API、模板生命周期和 presentation transition 的 owner。
- `DataGridQueryController`：不可变 Query validation、revision、交互策略与观察事件的 owner。
- `IDataGridSource`：schema、范围读取、snapshot 和 invalidation 的数据边界。
- `DataGridRangeCoordinator`：generation、最新视口 scope、前台/预取调度、请求取消、block lease、pending cache、错误和原子提交的 owner。
- `DataGridPresentationIndex`：display slot、window data index、range block 与高度元数据的映射 owner。
- `DataGridDisplayData`：committed row/group container、循环显示列表和回收池的 owner。
- `DataGridRowsPresenter`：已提交纵向 viewport 的同步布局和 `IChildIndexProvider` 投影。
- `DataGridColumn`：FieldId、列级查询能力和 SortState 投影。
- `DataGridColumnHeader`、`DataGridFilterIndicator` 与 filter Flyout：Query intent 和视觉状态的交互/模板投影。
- `DataGridCellsPresenter`：可见列、冻结列和横向 cell virtualization 的 owner。
- `DataGridLocalSource<T>`：强类型本地 query projection 和 range materialization。
- `DataGridFieldDisplayAccessor`：schema 中可选的 AOT-safe 显示访问；自动列只据此创建 compiled binding，FieldId 不作为属性路径。
- 可选 editable、movable、key-lookup 与 bulk-selection Source 接口：未加载数据上的明确能力边界。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 Source、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 DataGrid 时必须保持以下不变量：

- `ItemsSource`、`Query`、`Selection` 和可选 Source capability 的 identity 与 ownership 不能被模板或 presenter 改写。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- Ready 状态的视觉树、尺寸、列宽、滚动条、冻结列、RowDetails、selected/sorted 优先级和嵌套滚动链保持稳定。
- Template part 重新应用、Source 替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅、请求、cache 和资源宿主。
- 不通过隐藏延迟、强制刷新、ignore flag 或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- Source result 必须满足精确切片、snapshot、totals、key 唯一性和 index mapping 合同；无效结果不得部分显示。
- Source 必须协作处理 FetchAsync 的 CancellationToken 才能获得快速跳转的低延迟保证；无论 Source 是否协作，旧 viewport 结果
  都不能覆盖最新 presentation，DataGrid 也不能突破有界并发掩盖不可抢占 I/O。
- 连续模式超出 int presentation 上限时明确失败；分页通过 long data start 访问更大数据，不截断或饱和索引。
- Measure、Arrange、container prepare/recycle 和 input handler 不执行 Source I/O 或同步等待。
- 未声明 movable capability 时安全拒绝移动，不能错误提交、部分提交或发出成功事件。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

`DataGridSelectionState` 是不可变选择 owner，使用 ExplicitKeys、AllMatchingQuery、IndexIntervals 与 ExcludedKeys 表达单行、
跨页全选和范围选择。容器只按 RowKey/DataIndex 投影状态；回收不会改变选择。`CurrentRowKey` 是 current identity，internal
DataIndex hint 只用于定位。Source replacement 清空 selection/current；Query 与 invalidation 按
[Query 与 Range Source 设计](query-range-source-design.md#91-声明式选择)定义确定的保留和失效规则。

### 8.2 集合与数据同步模型

DataGrid 的数据状态遵循单一路径：

```text
immutable Query
  -> QueryController revision
  -> RangeCoordinator request/cache
  -> validated Source snapshot
  -> atomic PresentationSnapshot
  -> realized containers
```

Source replacement、Invalidated、Reload、Query、PageRequest 和 GroupExpansion 通过单调 revision/generation 隔离迟到结果。
`DesiredViewport` 与 `CommittedViewport` 分离；每个 generation 同时只有一个活动 viewport scope，scope 通过 block lease 保留新旧
目标仍共享的请求，并取消已经离开最新目标的 visible/prefetch 工作。自动行高估值也由 committed generation 所有：成功切换
generation 后重新采样，pending/失败状态继续使用旧 presentation 的估值。layout 只读取已提交并 pin 的 range block。完整 Source、
snapshot、请求优先级、缓存、索引域、虚拟化和回滚合同见
[DataGrid Query 与 Range Source 设计](query-range-source-design.md)。

### 8.3 行拖动重排模型

行拖动重排采用“控件会话负责交互、movable Source 负责数据提交”的单向模型：

```text
PointerPressed
  -> DataGrid row reorder session (Pressed)
  -> DragThreshold
  -> RowReordering
  -> session validation
  -> RowsPresenter ghost row (Dragging)
  -> IDataGridMovableSource move by row/neighbor keys + snapshot
  -> session cleanup
  -> RowReordered
```

每个 `DataGrid` 同时最多拥有一个行拖动会话。会话跟踪 Pointer、源 handle、源 row、Source identity、snapshot、
源 RowKey、目标邻接 key 和拖动几何。Pointer 不匹配、capture 丢失、控件禁用、`CanUserReorderRows=false`、
Source、Query 或 snapshot 替换、行回收、重排列移除、模板重建或 DataGrid detach
都会取消会话。取消和完成共用同一个清理边界，必须移除 ghost row、恢复动效、释放 capture 并清除会话引用。

移动提交由 Source 维护自己的数据、snapshot 和 invalidation。DataGrid 不推断 Source 集合结构，也不把 display/window
index 当作数据索引。Source 不支持移动时，handle 不进入可提交的 Dragging 状态。异常路径先无条件清理视觉会话，且不能留下
ghost row、旧 key 或成功事件。

### 8.4 列过滤模型

列过滤模型由三层组成：

```text
Column.Filters / filter presentation metadata
  -> Query intent
  -> DataGrid.Query.Filters
```

`Filters` 只表达候选过滤项来源；`DataGrid.Query.Filters` 表达已应用条件。`DataGridFilterIndicator`、菜单 flyout 和树形 flyout
是 Query 的视觉投影，不拥有业务状态。它们打开或重新物化时按 Query 初始化 checked state，在确认时构建一个已验证的
不可变 Query。Source schema 声明 operator、arity 和 scalar kinds；LocalSource descriptor 提供 typed evaluator，远端 Source
显式翻译 operator，DataGrid 不猜测字符串匹配规则。

DataGrid 的 internal pinned filter 只选择一个目标，目标顺序来自 `ColumnsInternal.GetDisplayedColumns()`，即当前
DisplayIndex 顺序。候选列必须不是 filler、可见、允许过滤、具有过滤项并已创建 Header；前一目标因 DisplayIndex、
可见性、`CanUserFilter`、过滤项或列集合变化而失效时，先对旧 Header 执行 lifecycle close 和 unpin，再 pin 新目标。

Pinned 状态只沿以下单向路径传播：

```text
DataGrid
  -> DataGridColumnHeader
  -> DataGridFilterIndicator
  -> DataGridMenuFilterFlyout / DataGridTreeFilterFlyout
  -> Popup
```

Header 和 Indicator 都是 internal relay adapter，不成为第二个测试入口，也不拥有过滤业务状态。Unpin 只解除当前 Flyout
的关闭拦截；目标替换、Header/Indicator detach、Flyout presenter mode replacement、DataGrid detach 和 template reapply
必须强制关闭旧 Popup 并释放两级 relay。

### 8.5 列宽分配模型

列宽采用“presenter 提供几何输入，DataGrid 统一求解，所有视觉消费同一结果”的单向模型：

```text
header / rows / cells available width and content measure
  -> DataGrid column sizing state
  -> constrained Auto completion and star distribution
  -> DataGridColumn display widths
  -> header / row / cell / filler / scrollbar projection
```

有已物化行时，rows/cells 路径提供包含行头和滚动布局修正后的 `CellsWidth`；空数据时，当前可见的普通或分组
列头 presenter 使用自己的有限测量宽度驱动同一个求解器。`AutoSizingColumns` 只表示初始内容测量阶段，完成
Auto 测量和分配 star 剩余空间是两个独立职责。filler 只承载所有可调整列达到 min/max 约束后仍无法分配的正
剩余空间，不能替代 star 列分配。

range source、分页和纵向虚拟化只让已实现 cells 参与 `Auto` / `SizeToCells` 测量；DataGrid 不为发现未加载内容宽度而请求其他 range。需要首屏稳定且保留内容宽度的水平 extent 时，应由列的 `Pixel Width` 或 `MinWidth` 明确提供几何基线，而不是扫描其他页、改用会压缩内容的 `Star`，或强制显示没有真实 overflow 的 scrollbar。

### 8.6 动效模型

DataGrid 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.7 视觉选项模型

DataGrid 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

规范Own Token及17个历史Table入口指向相同有效语义；每层先归一，再参与优先级合并。同层不同值诊断冲突，历史资源键继续有效，完整对应见[Token兼容入口](token.md#51-规范字段与历史入口)。

## 9. 文档导航与验证策略

关联文档：

- [DataGrid 桌面版实现原理](implementation.md)
- [DataGrid Query 与 Range Source 设计](query-range-source-design.md)
- [DataGrid 列宽分配设计](column-sizing-design.md)
- [DataGrid Token 设计](token.md)
- [DataGrid Changelog](changelog.md)

映射取舍：

- AntD `header.row`（thead 的 tr）在 AtomUI 中没有独立节点：`DataGridColumnHeadersPresenter` 直接承载各列头，因此不声明 `header.row`，避免虚构不存在的结构。
- `DataGridRowHeader`（行头）无 AntD 对应物，不纳入语义部件。
- 过滤/排序浮层属于列头触发的 Flyout 内容，遵循共享弹层契约，不在 Table 语义区域内声明。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 selection/checked/active、collection/filter、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
