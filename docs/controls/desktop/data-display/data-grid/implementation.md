# DataGrid 桌面版实现原理

本文档描述 DataGrid 桌面版的内部 ownership、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见
[DataGrid 桌面版架构设计](overview.md)，不可变查询、范围数据源、异步协调和虚拟化算法见
[DataGrid Query 与 Range Source 设计](query-range-source-design.md)，列宽算法和 presenter 协作见
[DataGrid 列宽分配设计](column-sizing-design.md)，分页面板 Extra Content 组合与对齐见
[DataGrid 分页面板 Extra Content 设计](pagination-extra-content-design.md)，Control Own Token 见
[DataGrid Token 设计](token.md)，变化记录见 [DataGrid Changelog](changelog.md)。

Popup 接入边界：`DataGrid` 负责 column-filter Query intent 与候选内容，filter Flyout 只作为 relay 适配层，filter Popup 负责
实际显示。模板重建或宿主切换时先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被
拦截，detach、窗口销毁、跨 TopLevel 和无效锚点走生命周期关闭并释放 Popup host。完整状态机见
[Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

DataGrid 的实现同时维护四条互不争夺 ownership 的链路：

- Data：不可变 Query、Source schema、range request/result、snapshot 和 selection。
- Presentation：range cache、display slot、已提交 viewport、row/group container 与 scroll extent。
- Columns：列定义、列宽、冻结列、水平虚拟化和 header/cell 视觉投影。
- Theme：ControlTheme、Template Part、伪类、Token、Popup 和平台宿主。

DataGrid 是 public contract 与 UI transition owner；Source 执行业务查询；presenter 只布局已经提交的数据；template part 只投影
状态。任何功能都不能通过在这些层之间建立第二份可变状态来完成同步。

## 2. 源码文件结构

稳定 ownership 按职责组织：

```text
src/AtomUI.Desktop.Controls.DataGrid/
├── DataGrid.cs                         public contract + lifecycle
├── DataGrid.Query.cs                   Query commit and visual projection
├── DataGrid.RangeLoading.cs            viewport-to-coordinator integration
├── DataGrid.Virtualization.cs          desired/committed viewport + container bridge
├── Data/
│   ├── Query/                          immutable values, scalar and validation
│   ├── Source/                         schema, request, result and capability contracts
│   │   └── Local/                      typed local source and projection
│   └── Virtualization/                 viewport scope, request scheduler, snapshot, cache, index and heights
├── Column/                             column contract, header, sorting/filtering interaction
├── Column/Filters/                     filter indicator, flyout and candidate presentation
├── Row/                                row, group header, details and row presenters
├── Cell/                               cell state and horizontal virtualization
├── EventArgs/                          public event payloads
├── Themes/                             static templates, selectors and resource binding
├── Localization/                       generated-catalog-backed localized strings
└── Utils/                              narrow shared algorithms and converters
```

维护规则：

- 主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- partial 与 helper 按稳定 owner 拆分，不按 public/private 或单个触发点拆分。
- Theme 文件负责静态视觉结构、Template Part、selector 和资源绑定；不在 C# 中动态复制同一结构。
- Token 只保存组件视觉变量，不保存 Query、loading、selection、expanded 或 popup runtime state。
- `DataGridToken.cs` 保存字段与17项显式别名，`DataGridToken.Defaults.cs` 保存DependencyDefaultsV2／revision 1的61项静态默认函数。生成计划按当前Global与声明的Own依赖求值，再投影别名；默认计算不访问控件、Query或Source，具体依赖和独立覆盖合同见[Token设计](token.md#52-非别名字段的默认依赖)。
- Gallery 只展示用法和验证行为，不成为运行时状态 owner。
- GeneratedFiles 由对应 generator 维护，不手工编辑。

## 3. 核心类职责

| 类型 | Ownership | 主要输入与输出 |
| --- | --- | --- |
| `DataGrid` | public API、模板生命周期、presentation transition | 接收 Query/Source/input，发布 applied state 与视觉更新。 |
| `DataGridQueryController` | Query validation、revision 和交互策略 | 把外部设置、sort/filter/group intent 归一为唯一 Query。 |
| 分页面板 Extra Content | Content/Template presentation | 把顶部/底部 Extra Content 投影到 Pagination 对侧，不拥有分页或 Source 状态。 |
| `IDataGridSource` | schema、range fetch、snapshot 与 invalidation | 消费 DataGridFetchRequest，返回不可变精确切片。 |
| `DataGridRangeCoordinator` | generation、viewport scope、请求优先级、block lease、pending cache 和错误 | 保留最新目标仍需要的工作，取消过时工作，确保 visible range 覆盖并形成可提交 snapshot。 |
| `DataGridPresentationSnapshot` | 一次完整 applied presentation | 固定 Source/query/generation/snapshot、page、expansion、totals 与 viewport。 |
| `DataGridPresentationIndex` | slot/entry/offset 映射 | 管理有限 block、sparse height delta 与 key/index metadata。 |
| `DataGridDisplayData` | 已实现纵向容器和回收池 | 在 committed range 上复用 row/group header，不读取 Source。 |
| `DataGridRowsPresenter` | 同步纵向布局与 child index | 只测量/排列 committed containers。 |
| `DataGridCellsPresenter` | 可见列和冻结列布局 | 维持水平 virtualization，不拥有纵向 range 请求。 |
| `DataGridColumn` | FieldId、列级能力、SortState | 从 Query/schema 推导 header 与 cell 状态。 |
| `DataGridLocalSource<T>` | 本地 typed projection | 在稳定快照上 filter/sort/group，并只物化请求 range。 |

协作边界：

- Source 不捕获 DataGrid，也不返回可继续修改的 entry collection。
- Coordinator 不解释业务协议；LocalSource 和业务 Source 不生成视觉对象。
- RowsPresenter、DisplayData、Row 和 Cell 不发起 FetchAsync。
- Header、FilterIndicator 和 Flyout 只产生 Query intent，不直接改 range cache。
- 可选 editable、movable、key-lookup 和 bulk-selection 能力与只读 Source 分离。
- DataGrid 是列宽 solver owner；header/rows/cells presenter 只提供有限 viewport 或内容测量输入。

## 4. 状态与数据流

### 4.1 Query 到 presentation

```text
Public Query / column gesture / filter confirmation / group intent
  -> DataGridQueryController validates structure and Source.Schema
  -> commit pending edit
  -> structural no-op check
  -> QueryRevision++ and visual projection
  -> QueryChanged
  -> DataGridRangeCoordinator
  -> IDataGridSource.FetchAsync
  -> result contract validation
  -> atomic DataGridPresentationSnapshot commit
  -> rows / groups / pagination / scrollbar / selection projection
```

Query 是 sort/filter/group 的唯一 owner；AppliedQuery 只属于已提交 snapshot。QueryChanged handler 若同步设置另一个 Query，新的
revision 使外层 transition 失效。没有 suppress flag、ignore flag 或 Dispatcher 延时。

Source replacement、Reload、Invalidated、snapshot expiry、PageRequest 与 GroupExpansion 变化递增 DataGeneration。每个 request
同时携带 QueryRevision 与 DataGeneration，任何迟到或来自旧 Source/snapshot 的结果在 commit 前丢弃。

### 4.2 Desired 与 committed viewport

```text
scroll / thumb / keyboard / bring-into-view intent
  -> coalesced DesiredViewport
  -> replace active ViewportRequestScope
  -> pure ViewportPlanner
  -> retain shared visible block leases and cancel orphaned work
  -> foreground scheduler fills missing blocks (synchronous hit or asynchronous miss)
  -> atomic CommittedViewport swap
  -> one layout pass over committed blocks
```

DesiredViewport 是最新用户意图；CommittedViewport 是当前可以同步布局的完整范围。缓存 miss 时继续显示旧 committed rows，
不创建 placeholder 或 null row。同一 generation 只有一个 active viewport scope；新 scope 先取得仍需 block 的 lease，再释放旧
scope，避免相邻目标取消并重取共享 block。失去所有有效 lease 的工作从可发现 inflight 集合移除并取消，后续请求不会附着到
已经取消的 task。prefetch 从属于产生它的 committed scope，只填 cache，不改变 loading visual、不触发布局、不创建 control，
也不能阻塞新的 visible target。

### 4.3 Load 与错误状态

```text
ItemsSource == null                    -> Idle
no applied snapshot + active request  -> Loading
applied snapshot + active request     -> Refreshing
visible target atomically committed   -> Ready
non-cancellation failure              -> Error
```

internal `EffectiveIsOperating` 只合并用户显式 `IsOperating` 与 Loading：

```text
EffectiveIsOperating = IsOperating || LoadState == Loading
```

Loading 表示没有可展示的已提交 snapshot，实际挂起时驱动现有 Spin。Refreshing 保留旧 rows 的几何与完整不透明度，
不自动启动 Spin；读取类 selection 可以继续投影，edit/delete/move 仍被禁止。调用方显式设置 `IsOperating=true` 时，任意
LoadState 都继续显示 Spin。

只有 ValueTask 实际挂起时才发布 Loading/Refreshing。同步 Source 与 cache hit 在当前调用中完成 result validation、snapshot
commit 和 Ready 投影，不增加一次 Dispatcher 调度，也不让嵌套 DataGrid 在一帧内停留于虚假的 Loading。

用户 Query/page/group/scroll 失败时恢复最后成功 snapshot 的 Query、page、expansion、viewport、totals 和视觉；只有实际 Query
回退发出一次 LoadRollback QueryChanged。初次失败保持空 presentation。Invalidated 刷新失败保留 stale 只读 snapshot 并等待
Reload。Cancellation 和 superseded request 不进入 Error。

### 4.4 选择与 current

```text
DataGridSelectionState + CurrentRowKey
  -> committed DataGridSourceEntry(RowKey, DataIndex)
  -> realized DataGridRow/DataGridCell pseudo-classes
```

Selection 使用 ExplicitKeys、AllMatchingQuery、IndexIntervals 和 ExcludedKeys 表达声明式状态，内存不随 total row count 增长。
Row/Cell 只投影当前 entry 的判定结果，container recycle 不改变 selection。CurrentRowKey 是 identity；DataIndex hint 只用于定位。
未加载 key 的解析依赖可选 `IDataGridKeyLookupSource`，不能伪造 SelectedItem 或 index。

### 4.5 分页

```text
IsShowPageSizeSelector / PageSize / current page intent
  -> checked DataGridPageRequest(long data start, int count)
  -> DataGridFetchRequest
  -> applied TotalDataCount and page state
  -> PART_TopPaginationPanel / PART_BottomPaginationPanel
     -> Extra Content Presenter + PART_TopPagination / PART_BottomPagination
```

`IsShowPageSizeSelector` 只把可见性转发给两个 `Pagination.IsShowSizeChanger`。PageSize 为 0 时使用连续模式；非零时先按业务行切
PageRequest，再插入 group header、应用 expansion 和 display Range。页大小选择器更新 `Pagination.PageSize` 后，DataGrid 将
`PageChangedEventArgs.PageSize` 转换为新的 `DataGridPageRequest`，保持可保留的页序号并在新页数不足时收敛到最后一页。上下两个
Pagination 只投影同一 applied state；模板 reapply 先回放 state，再订阅 input。翻页成功后 vertical offset 归零，失败时整体回退。
Extra Content 使用独立顶部/底部 Content/Template 属性，位置由 Pagination Align 推导到相反一侧，不进入该状态流。

### 4.6 列宽

```text
DataGridColumnHeadersPresenter / DataGridGroupColumnHeadersPresenter
  -> header desired widths + finite viewport when rows are absent
DataGridRowsPresenter / DataGridCellsPresenter
  -> CellsWidth + realized cell desired widths when rows are present
DataGrid
  -> finish Auto measurement
  -> AdjustColumnWidths resolves star widths
DataGridColumn display widths
  -> headers / rows / cells / filler / scrollbars
```

普通与分组列头共享有限宽度入口；有行时使用 CellsWidth。Presenter 不独立修改 star 列，不把 filler 当作求解结果。完整矩阵见
[DataGrid 列宽分配设计](column-sizing-design.md)。

### 4.7 过滤与 pinned popup

```text
Column.Filters / presentation metadata
  -> filter Flyout Query intent
  -> immutable DataGridQuery.Filters
  -> Source request + Header/Indicator/Flyout projection
```

Filters 只表示候选内容；Query.Filters 是 applied 条件 owner。Flyout 打开时从 Query 初始化 checked state，确认时构建一个完整新
Query。Schema 声明 operator、arity 和 scalar kinds；LocalSource descriptor 提供 typed evaluator，远端 Source 显式翻译。

Pinned filter 由 DataGrid 独占 ownership：

```text
DataGrid.IsPopupPinnedOpen
  -> first eligible column in DisplayIndex order
  -> DataGridColumnHeader
  -> DataGridFilterIndicator
  -> current filter Flyout
  -> Popup
```

replacement 先 lifecycle-close 旧 Popup、释放 relay 和 callback，再接入新目标。Unpin 只解除关闭拦截；DataGrid detach、template
reapply、禁用、隐藏、TopLevel 变化或锚点失效强制关闭旧 Popup。

### 4.8 行拖动

```text
PointerPressed
  -> RowReorderSession(Pressed, pointer, Source, snapshot, RowKey)
  -> drag threshold
  -> RowReordering
  -> revalidate owner/query/snapshot/capability
  -> RowsPresenter ghost row
  -> key-relative IDataGridMovableSource request
  -> cleanup
  -> RowReordered only after successful mutation
```

每个 DataGrid 只有一个会话。handle 不修改数据，presenter 不决定移动语义，Source 不持有视觉对象。Source、Query、snapshot、
row、pointer 或 capability 变化均取消会话。完成和取消共用幂等清理入口，释放 capture、ghost、offset、transition 和引用。

## 5. 组合结构模型

### 5.1 控件角色图

```text
DataGrid (DataGridTheme.axaml)
  -> PixelAlignedBorder#Frame (template-stable)
     -> Border#FrameContentClip (template-stable)
        -> Spin (internal-observable)
           -> DockPanel
              -> DockPanel#PART_TopPaginationPanel (template-stable)
                 -> ContentPresenter#PART_TopPaginationExtraContentPresenter (template-stable)
                 -> Pagination#PART_TopPagination (template-stable)
              -> PixelAlignedBorder#TitleFrame
                 -> ContentPresenter#Title
              -> DockPanel#PART_BottomPaginationPanel (template-stable)
                 -> ContentPresenter#PART_BottomPaginationExtraContentPresenter (template-stable)
                 -> Pagination#PART_BottomPagination (template-stable)
              -> ContentPresenter#Footer
              -> Grid
                 -> DataGridTopLeftColumnHeader#PART_TopLeftCorner (template-stable)
                 -> Border#ColumnHeadersPresenterFrame
                    -> Panel
                       -> DataGridColumnHeadersPresenter#PART_ColumnHeadersPresenter (template-stable)
                       -> DataGridGroupColumnHeadersPresenter#PART_GroupColumnHeadersPresenter (template-stable)
                 -> PixelAlignedBorder#ColumnHeadersAndRowsSeparator
                 -> DataGridRowsPresenter#PART_RowPresenter (template-stable)
                    -> DataGridRow (internal-observable, virtualized)
                       -> DataGridCellsPresenter (internal-observable, horizontally virtualized)
                    -> DataGridRowGroupHeader (internal-observable, virtualized)
                 -> ContentPresenter#EmptyIndicator
                 -> ScrollBar#PART_VerticalScrollbar (template-stable)
                 -> ScrollBar#PART_HorizontalScrollbar (template-stable)
                 -> Border#DisabledVisualElement
                 -> DataGridColumnDraggingOverIndicator#PART_DraggingOverIndicator (template-stable)
```

模板中的 Spin 节点复用 DataGrid 的 operating state；Query/Range Source 只改变该状态的有效输入，不增加平行 overlay。RowsPresenter
下面的 row、group header 和 cells 由 container lifecycle 动态接入，但必须继续遵守同一个 template/scroll ownership。

DataGridToken保留规范属性和历史Table字段，17项明确兼容关系由`ControlTokenAlias`声明。生成的descriptor包含不可变别名表；配置及定义在各层绑定时归一，Compiler发布前投影历史字段。ControlTheme继续引用原资源键，不在控件或模板内维护别名同步、刷新标记或第二份Token字典。规范名/历史名和消费区域的完整对应见[Token兼容入口](token.md#51-规范字段与历史入口)。

### 5.2 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| DataGrid | public control | `DataGrid.cs` | 应用/VisualTree | Source、Query、Selection、layout/theme API | public | 唯一 UI state owner。 |
| Frame / FrameContentClip | template node | `DataGridTheme.axaml` | DataGrid template | border、corner、background | template-stable | 维护外框和内容裁剪，不能下沉到 row。 |
| Spin | public child control | `DataGridTheme.axaml` | DataGrid template | IsOperating、LoadState | internal-observable | 只投影 effective operating state。 |
| Top/Bottom Pagination Panel | template node | `DataGridTheme.axaml` | DataGrid template | PaginationAlign、Extra Content、visibility | template-stable | 组合 Extra Content 与 Pagination；Extra Content 始终位于 Pagination 对侧。 |
| Top/Bottom Extra Content Presenter | `ContentPresenter` | `DataGridTheme.axaml` | DataGrid template | Top/Bottom Extra Content 与 Template | template-stable | 只物化应用内容，不保存分页、Query 或 Source 状态。 |
| Top/Bottom Pagination | public child control | `DataGridTheme.axaml` | DataGrid template | `IsShowPageSizeSelector`、PageSize、visibility、align | template-stable | `IsShowPageSizeSelector` 转发到 `Pagination.IsShowSizeChanger`；只投影 applied page state。 |
| Column headers presenters | internal presenters | `DataGridTheme.axaml` | DataGrid template | headers、column width、Query intent | template-stable | 提供测量与交互，不拥有 Query。 |
| RowsPresenter | internal presenter | `DataGridTheme.axaml` | DataGrid template | rows、scrolling、selection | template-stable | 只布局 committed entries，不调用 Source。 |
| Row / GroupHeader | item containers | C# container generation | DataGridDisplayData | item/group/details/selection | internal-observable | recycle 时完整清理 entry state。 |
| CellsPresenter | internal presenter | Row theme/C# | row container | columns、frozen、horizontal scroll | internal-observable | 继续拥有横向 virtualization。 |
| EmptyIndicator | template node | `DataGridTheme.axaml` | DataGrid template | EmptyIndicator | internal-observable | 只由 committed empty state 控制。 |
| ScrollBars | template nodes | `DataGridTheme.axaml` | DataGrid template | scrollbar visibility | template-stable | 与 PresentationIndex 的 extent/offset 同步。 |
| DraggingOverIndicator | internal control | `DataGridTheme.axaml` | row reorder session | CanUserReorderRows | template-stable | 只绘制反馈，不提交数据。 |
| Filter Flyout / Popup | internal overlay chain | filter themes/C# | current header/indicator/flyout | column filter options、pinned state | internal-observable | relay Query intent，replacement 对称释放。 |

## 6. 生命周期与模板接入

### 6.1 控件生命周期

- 构造阶段注册属性与静态状态，不依赖 Template Part。
- OnApplyTemplate 开始先解绑旧 part，再获取新 part、回放 applied state 并建立事件连接。
- Attach 时订阅当前 Source.Invalidated；Source replacement 与 detach 时对称退订。
- Detach 先失效 revision/generation identity，再取消 request、dispose CTS、释放 pending/applied cache、part 与 relay。
- 迟到 continuation 必须核对 Source、revision、generation 和 snapshot；identity 失效后直接结束。
- DataGrid 不 dispose 外部 Source；Source 自己拥有数据库连接、集合订阅和自身 IDisposable/IAsyncDisposable 生命周期。

### 6.2 Source replacement

Source replacement 遵循预验证事务：候选 schema、当前 Query、列与 page 首先在旧状态外验证；随后提交 edit；只有这些步骤成功
才一次性切换 Source identity 和 generation。切换后取消/退订旧 owner、清空旧 cache/totals/key resolution，订阅新 Source 并
加载首个范围。切换点之后旧 Source 永不重新成为有效 owner。

### 6.3 Request 与 cache 生命周期

| 资源 | Owner | 释放或失效 |
| --- | --- | --- |
| generation CTS | RangeCoordinator | Query/generation change、detach、完成后 Dispose |
| viewport request scope | RangeCoordinator | 不同 DesiredViewport、generation replacement 或 detach 时 supersede；先转移共享 lease，再释放其余 lease |
| block request lease | active viewport scope | scope supersede、visible commit 后 prefetch 失效、generation replacement 或 detach |
| block work item CTS | block request scheduler | 最后一个有效 lease 释放或 generation/detach 失效时取消；work item 进入终态后 Dispose |
| Source request CancellationToken | 单 block work item | 由 generation 与 work item 生命周期共同约束；不受已失效 caller continuation 复活 |
| pending blocks | pending snapshot | commit、failure、query/source change、detach |
| applied blocks | applied snapshot | snapshot/source change、detach |
| realized/edit/drag block pin | 对应 visual/session owner | container/session detach 后 |
| pooled entry/index arrays | LocalSource 或 block | projection/block eviction 或 Source dispose |
| sparse height entries | PresentationIndex block/user state | block eviction 或显式 state 清理 |

### 6.4 Template Part 接入

- `PART_RowPresenter` 只承载 committed row/group containers；空数据隐藏，不成为列宽求解前置。
- 显式有限高度的首个 snapshot 可以先只提交数据，由首次布局按真实 viewport 实现 rows；自动高度且尚无 Bounds 的嵌套
  DataGrid 在模板就绪后实现一个已提交 bootstrap entry，以建立非零 DesiredSize，期间不从 Measure 请求 Source。
- `PART_TopPaginationPanel` 与 `PART_BottomPaginationPanel` 先建立 Extra Content/Pagination 组合和有效可见性，再让内部 Pagination 接收 applied state、订阅翻页和页大小 intent；`IsShowPageSizeSelector` 通过模板绑定同步到两个 `Pagination.IsShowSizeChanger`。
- `PART_TopPaginationExtraContentPresenter` 与 `PART_BottomPaginationExtraContentPresenter` 独立解析 Content/Template；模板替换和 re-template 不复用旧 Visual，不把同一个 Control 实例挂到两个 Presenter。
- `PART_ColumnHeadersPresenter` 与 `PART_GroupColumnHeadersPresenter` 接入同一个 DataGrid-owned column-width solver。
- 现有 Spin 绑定 EffectiveIsOperating；首次异步 loading 显示 Spin，refreshing 保持已提交内容完全可见，两者都不改变
  Frame/Header/scroll geometry。
- SortIndicator、FilterIndicator、row/cell/group header 继续使用既有 theme、part 与 pseudo-class。
- FrameContentClip、FrameCornerRadius 和 FrameBorderThickness 的 ownership 不因异步数据架构改变。

### 6.5 语义部件（Semantic Part）接入

语义部件对齐 Ant Design 6.6.3 Table Semantic DOM，owner 为 `DataGrid`（public、非泛型，满足契约校验器）。声明位于 `DataGrid.SemanticParts.cs`，生成器为每个部件产出专用 Style 类（`DataGrid<PartPath>Style`，如 `header.wrapper` → `DataGridHeaderWrapperStyle`）与 marker 常量。

Owner 事实表：

| 部件 | 声明者 | 目标控件可见性 | SelectorRoute | 基数 | RuntimeCreated |
| --- | --- | --- | --- | --- | --- |
| `root` | 生成器隐式合成 | `DataGrid` 控件自身 | 无（Customization=Root） | Single | 否 |
| `section` | `DataGrid` | 模板静态节点 | `/template/ .semantic-section` | Single | 否 |
| `title` | `DataGrid` | 模板静态节点 | `/template/ .semantic-title` | Single | 否 |
| `content` | `DataGrid` | 模板静态节点（主 Grid） | `/template/ .semantic-content` | Single | 否 |
| `header.wrapper` | `DataGrid` | 模板静态节点 | `/template/ .semantic-header-wrapper` | Single | 否 |
| `header.cell` | `DataGrid` | `DataGridColumnHeader`（internal） | `>> .semantic-header-cell` | Multiple | 是 |
| `body.wrapper` | `DataGrid` | 模板静态节点 | `/template/ .semantic-body-wrapper` | Single | 否 |
| `body.row` | `DataGrid` | `DataGridRow` / `DataGridRowGroupHeader`（public） | `>> .semantic-body-row` | Multiple | 是 |
| `body.cell` | `DataGrid` | `DataGridCell`（public） | `>> .semantic-body-row >> .semantic-body-cell`（CrossNestedOwners） | Multiple | 是 |
| `footer` | `DataGrid` | 模板静态节点 | `/template/ .semantic-footer` | Single | 否 |

Marker 预算与注入方式：

- 静态模板 marker 共 8 个（section/title/content/header.wrapper/body.wrapper/footer/pagination.root×2），在 `DataGridTheme.axaml` 模板节点上以 `Classes.semantic-*="True"` 一次性声明；内置主题默认视觉不消费 `.semantic-*` selector，继续使用 `^ /template/ Type#Name` 形态。
- 运行时 marker 共 4 类：`DataGridColumnHeader`、`DataGridRow`、`DataGridRowGroupHeader` 与 `DataGridCell` 在模板根应用时用生成常量注入（构造或 ApplyTemplate 一次 `Classes.Add`，回收复用不重复注入）；`pagination.item` 复用 `Pagination` 模板既有的 `semantic-item` class，经 `>> .semantic-pagination-root >> .semantic-item` 跨根路由命中。
- 分组头行（`DataGridRowGroupHeader`）同样注入 `semantic-body-row`：分组头在视觉上属于表体行，用户对 `body.row` 的定制应自然覆盖。
- `header.row` 不声明：AtomUI 模板中表头 presenter 直接承载列头，不存在独立表头行节点，不为对齐上游而虚构结构。
- `pagination.root` 指向模板内两处 `Pagination` 槽位（顶部/底部，`Multiple`）；`pagination.item` 经 `>> .semantic-pagination-root >> .semantic-item` 跨视觉根命中分页器内部页码项，与上游 Table 的 pagination 区域对齐。

虚拟化 ownership 与性能边界：

- `body.row` / `body.cell` 目标节点由 DisplayData slot 回收与 Range 管线在运行时创建/回收；marker 只在控件模板应用时注入一次，回收复用不产生额外 Classes 写入或 VisualTree 扫描。
- 语义系统构造时不读 registry；descriptor 查询只发生在专用 Style 解析阶段。
- 冻结列、分组模式、行详情与拖动指示器不改变上述 selector 命中拓扑。

## 7. 交互与事件处理

- Header pointer、keyboard、tooltip 与 `SetSort` 都调用同一 `DataGridSortPolicy`；resize 边界命中优先于排序手势。
- Filter Flyout 只收集候选值并提交 Query intent；Query validation、edit commit 和 request scheduling 仍由 DataGrid 管理。
- 滚轮、触控惯性、scrollbar line/page/thumb、Home/End/PageUp/PageDown 和 ScrollIntoView 统一产生 DesiredViewport。
- Scroll handler 只合并 intent；到内部边界前由 DataGrid 消费，边界后允许外层 ScrollViewer 继续滚动。
- QueryChanged 在内存提交与视觉投影后、Source request 前触发；回调重入后重新核对 revision。
- SelectionChanged 描述声明式 OldSelection/NewSelection，不为事件参数加载 cache 外 item。
- RowReordering 在超过拖动阈值且创建 ghost 前最多触发一次；回调后重新验证 Source、Query、snapshot、row 和 capability。
- RowReordered 只在 Source mutation 成功且 capture、ghost、offset、transition 和会话引用清理后触发。
- Disabled、detach、template reapply、Source/query replacement、capture lost 或 row recycle 终止当前 mutation/drag session。
- Loading/Refreshing/stale 阻止 edit/delete/move 提交，但不能截断滚动 intent 的合并或破坏现有 focus/keyboard 路径。

## 8. 内部算法与关键流程

### 8.1 Source result validation

UI commit 前验证：

- Source identity、QueryRevision、DataGeneration、request Range 与 ExpectedSnapshot 匹配。
- StartIndex、Entries.Length 与 totals 满足精确切片公式。
- Data/Group entry 的 payload、RowKey/GroupKey、WindowDataIndex 与 DataIndex 合法。
- 当前 cache 范围内 row/group key 分域唯一且 data indices 严格递增。
- 相同 snapshot 下 TotalDataCount、WindowDataCount 和 TotalEntryCount 的条件恒定。

任何合同错误进入 `DataGridSourceContractException`，不部分提交。连续模式不能用 int 精确表示活动 data/display count 时抛出
`DataGridPresentationLimitExceededException`，不能截断、饱和或取模。

### 8.2 三个索引域

| Domain | 类型 | 使用位置 |
| --- | --- | --- |
| Source data | long | TotalDataCount、PageRequest.DataStartIndex、entry.DataIndex |
| Window data | int | WindowDataCount、entry.WindowDataIndex、DataGridRow.Index、IChildIndexProvider |
| Display slot | int | TotalEntryCount、Range.StartIndex、DataGridRow.Slot、vertical virtualization |

不同 domain 只通过 committed entry 显式映射。GroupHeader 不占 WindowDataIndex。分页可以访问 long global index，但单个 UI
window 的 child/slot count 必须保持准确 int。

### 8.3 Range planning、cache 与 commit

Block size 由 Schema.PreferredRangeSize 在 `[32, MaximumRangeSize]` 内确定。generation 尚未取得 result identity 时，bootstrap
是 generation 级前置工作，不随每个中间 viewport 重启；identity 建立后只为最新 target 补齐 visible block。最大 Source 并发
为 2。

Coordinator 在锁内完成 scope 交换和 lease 转移，在锁外执行取消与 Source 调用，避免业务 Source continuation 或 cancellation
callback 进入内部 gate。新 scope 先取得仍需 visible block 的 lease，再释放旧 scope；没有 lease 的排队 work item 立即从调度队列
和可发现 inflight 集合移除，没有 lease 的执行中 work item 立即取消。相同 generation/block 的有效 work item 继续去重；已经标记
取消的 work item 不得被新 scope 复用，其迟到结果也不能写入 active pending cache、错误状态或 presentation。

调度器维护 visible 与 prefetch 两个有界优先级。空闲并发槽始终先取 visible；prefetch 只在没有 visible 缺口时进入 Source。
新 visible intent 会先撤销旧 scope 的 prefetch lease，再调度自身缺口。已经进入 Source 的旧工作依赖 Source 协作取消释放槽位，
但无论 Source 是否遵守取消，迟到结果都不能通过 generation、scope 与 lease identity 校验。

同步 Source 或 cache hit 仍沿当前调用栈完成，不被优先级队列强制异步化。caller cancellation 只结束该 caller 的等待；只要同一
work item 仍被 active scope 使用，就不能由单个 waiter 取消共享 Source 工作。scope supersede 和 generation 失效才改变 work item
的有效 lease。旧 scope completion 不得覆盖新 scope 的 Loading/Refreshing；prefetch failure 只保留诊断信息，不改变可见状态。

Cache 是有限 LRU，容量不依赖 TotalDataCount。prefetch block 不生成 control。visible blocks 完整且全部通过验证后一次性 pin
新 block、交换 snapshot/viewport、更新 totals、回收旧容器、实现新容器、回放 selection/current/edit/details、更新
extent/offset/scrollbar 并进入 Ready。旧容器 detach 后才解除旧 block pin。

若 totals 变化导致 page/scroll target 越界，丢弃 pending presentation，递增 generation 并只请求最终合法 target，不能显示
瞬时空页。

### 8.4 Height、extent 与 anchor

固定行高使用 O(1) offset/slot 公式。自动行高、GroupHeader 和 RowDetails 使用：

```text
estimatedOffset(slot) = slot * defaultEstimate + sparsePrefixDelta(slot)
```

SparseHeightDeltaIndex 只保存 pinned/current-LRU block 的实测差值；passive block eviction 时把样本吸收到常量大小、按 entry
kind/group level 分类的 estimator 并删除明细。prefix sum 和 offset-to-slot 是 O(log M)，M 受 cache/pin 与用户显式状态上限
约束。禁止从 slot 0 扫描到 first slot，也禁止按 total/历史访问量创建全局 Fenwick array。

自动 `RowHeightEstimate` 的 owner 是 committed data generation。`CommitRangePresentation` 先用 `DefaultRowHeight` 构建新
generation 的高度索引；只有 range、snapshot、pins 与 staged containers 均准备成功、即将交换 presentation 时，才调用
`ResetRowHeightEstimateForDataGeneration`。这样失败或迟到的 generation 不污染当前高度状态；Source 换绑、Query/filter、分页和
Invalidated/collection Reset 成功后都从新 baseline 采样。首个有效 data row 的正常 measure 会更新 `RowHeightEstimate` 并 rebase
稀疏索引，显式 `RowHeight` 继续优先。重置为 O(1)，不新增订阅、timer、CTS 或按总行数分配的结构。

测量修正以首个完整可见 entry key 与 intra-row offset 为锚。Query/PageRequest 成功后 vertical offset 归零；GroupExpansion
保持操作 header 的屏幕 Y；Invalidated 优先按首行 key 恢复，失败时 clamp 到最近合法 slot。

重叠range视口完成容器更新后，先按当前实测高度归一化已显示首行和行内偏移，再发布滚动条Value／Maximum。不能先发布按旧首行计算的偏移、等下一轮measure才推进首行，否则同步观察者会看到偏移超过首行高度的中间状态。归一化只发生在容器提交点，不在逐行measure遍历中提前改动容器集合。

DataGrid 写入 vertical scrollbar 前仍把 extent travel clamp 到非负；`AbstractScrollBar` 再对所有有限值统一保证
`Maximum >= Minimum`。小于一个物理像素的 travel range 收敛到 Minimum，因此 Auto scrollbar 不抖动，也不会通过非法负
Maximum 触发 Avalonia RangeBase 校验异常。

### 8.5 Container recycle

回收时清理 DataContext、RowKey、DataIndex、WindowDataIndex、Slot、group metadata、selection/current/edit/hover、RowDetails、
cell sort/filter 状态和事件订阅。`DataGridDisplayData` 继续复用循环 displayed list 与 row/group pool；不能引入第二个
ItemsRepeater、VirtualizingStackPanel 或 scroll owner。

行头模板由 `DataGridRow` 持有，优先使用行级 `HeaderContentTemplate`，其次使用表格级 `RowHeaderContentTemplate`。
行数据变化时通过 `IRecyclingDataTemplate.Build` 由模板决定能否复用旧视觉；普通 `IDataTemplate` 为当前数据重新创建内容。
模板缓存不代表内容仍挂接在 `Header` 上。回收时解除挂接并清空内容的 DataContext，复用时为当前数据重新应用模板并挂接内容，
非回收卸载时同时释放模板缓存。同模板的重复应用保留调用方显式设置的 `Header`；有效模板变更后由新模板接管生成内容。
替换或清空模板必须释放旧内容的数据引用，包括行头隐藏时；新内容的创建仍等待行头可见。行头模板更新独立于 RowDetails 的
可见性和加载/卸载事件，不能复用详情的状态标志。

RowsPresenter.Children 只包含 visible 加一个 editing row 和一个 drag row 上限的 controls。Measure、Arrange、prepare 和 recycle
不 fetch、不等待、不分配 range buffer。嵌套滚动在内部边界前由 DataGrid 消费，到边界后交给外层 ScrollViewer。

### 8.6 Query 执行顺序

Source 使用固定顺序：

```text
filter
  -> stable Groups then Sorts
  -> PageRequest over business rows
  -> insert GroupHeader
  -> apply GroupExpansion
  -> flatten display entries
  -> Range slice
```

Groups 形成领先 sort，同一 FieldId 不能重复出现在 Groups 与 Sorts。Source 省略 collapsed descendants；DataGrid 不为完整结果
建立全局 group-header 或 collapsed-slot table。`GroupEntry.LeafCount` 表示当前 PageRequest 内包含 collapsed descendants 的业务
行数，不是跨页 aggregate。

### 8.7 LocalSource

DataGridLocalSource<T> 在 owner thread 捕获稳定引用快照和 sourceVersion；后台只处理快照的 int index buffer。Filter 不复制业务
对象，sort 依次比较 Groups、Sorts 和原 source ordinal，保证稳定输出。PageRequest 先切业务行；plain table 不建 group 对象；
range fetch 只创建请求范围大小的 entry buffer。

空 Query、无分页、无分组的 plain table 直接按 request range 读取稳定 source snapshot，不创建全量 projection cache。只有
filter/sort/group/page 路径进入 index projection 与有限 LRU；因此百万行 plain 首屏仍只物化一个有界 range。

typed descriptor 提供 `Func<T,TValue>` getter、`IComparer<TValue>`、operator evaluator 与 scalar converter。比较循环不执行 LINQ、
反射、字符串 path、文化对象创建或 delegate 组合分配。projection cache 以 sourceVersion、Query、PageRequest 和 Expansion 的
相应 identity 分层，并受有限 LRU 管理。

### 8.8 Sort/filter visual delta

Query.Sorts 被归一为 FieldId 到 `(direction, priority)` 的小型映射，只更新 old/new delta 涉及的 column。Column.SortState 驱动
Header 的既有 sort pseudo-class 和 indicator。Cell 不长期订阅 Header；prepare/recycle 从 OwningColumn.SortState 初始化，Query
delta 只遍历已实现 cells。

Filter Flyout 从 Query.Filters 初始化，提交完整新 Query。候选 Filters 的集合变化只重新物化候选内容，不能绕过 Query owner。

### 8.9 Frame、列宽与水平 virtualization

- FrameCornerRadius 与 FrameContentClip 共同约束根内容裁剪；FrameBorderThickness 只由 frame border option 决定。
- 最后一行 divider 与 Frame 底边保持唯一绘制责任；Footer、底部分页或横向滚动条存在时恢复行 divider。
- 有限列 viewport 只进入 DataGrid-owned solver；Auto completion 与 star distribution 是独立阶段。
- `Auto` / `SizeToCells` 只消费已实现 cells 的正常 measure 结果；列宽发现不得 fetch 未显示 range 或扩大纵向 realized window。
- 首屏需要稳定内容 extent 的场景由列 `Pixel Width` 或 `MinWidth` 提供基线；水平 scrollbar 继续只投影真实 extent，不承担宽度策略。
- Star 列仍能吸收空间时 filler 为零；只有所有可调列达到 min/max 后才允许正 filler。
- 水平可见列、冻结列和 cell count 继续由 DataGridCellsPresenter 管理；纵向 commit 不全量重建 cells。

## 9. 资源、性能与 AOT 边界

### 9.1 热路径

- Scroll input 只更新/合并 DesiredViewport，handler 返回后由 coordinator 调度 request。
- 每次不同的 DesiredViewport 只替换一个 active scope；旧排队项不会作为无界 task 留在 semaphore 前等待。
- PointerMoved 只更新 drag session、ghost offset、target key 和必要自动滚动 intent。
- Measure/Arrange/prepare/recycle 只做 committed index lookup 与 container state 投影。
- Query no-op 不请求、不刷新 rows；sort state 没有 per-cell 长期订阅。
- Ready 模板不增加 visual node；prefetch 不触发布局。
- 同步 Source/cache hit 不发布瞬时 Loading/Refreshing；异步首次加载驱动 Spin，已有 snapshot 的异步刷新不启动自动遮罩。

### 9.2 复杂度与资源上限

| 路径 | 上限 |
| --- | --- |
| fixed-height offset lookup | O(1) |
| variable-height offset lookup | O(log M)，M 由 cache/pin/显式状态约束 |
| realized row/group controls | visible + editing row + drag row |
| request concurrency | 默认最多 2 |
| active viewport scope | 每个 generation 1 个 |
| queued obsolete viewport work | 0；scope supersede 时从可发现队列移除并取消 |
| prefetch priority | 低于所有 visible 缺口，并绑定产生它的 committed scope |
| range size | Schema.MaximumRangeSize，最大 4096 |
| cache | 有限 block LRU，与 total 无关 |
| snapshot-expiry auto retry | 每个用户操作最多 1 次 |

所有 request、cache、pool、subscription 和 sparse state 必须在长时间往返滚动后达到稳态，不能随滚动次数或曾访问 row 数持续
增长。

### 9.3 AOT

- Query/Source public 类型只位于 DataGrid package，不引入 Core/Shared 反向依赖。
- LocalSource 使用静态泛型 descriptor，并把 typed getter 显式投影为 schema `DisplayAccessor`；generated
  `IDataMemberAccessor` 可通过 `FromDataMember<TItem>` 复用。自动列只消费该 accessor 并生成 compiled binding，绝不把
  FieldId 当作 CLR path。
- 没有 `DisplayAccessor` 的远端 schema 字段只支持显式 Column/Binding；`PropertyChangedName` 与 FieldId 相互独立，binding
  dispose/recycle 时解除 `INotifyPropertyChanged` 订阅。
- 不使用 PropertyInfo.GetValue、Expression.Compile、assembly scan、runtime registration、动态泛型构造或字符串 Binding。
- 不新增 linker root、trimming suppression 或反射 fallback。
- Source generator 输出只通过输入源和 generator 更新。

## 10. 维护不变量

维护 DataGrid 时不得破坏：

- Query、AppliedQuery、Source、Selection、CurrentRowKey 和 PresentationSnapshot 的单一 ownership。
- QueryRevision/DataGeneration 与 Source identity/snapshot 的迟到结果隔离。
- Result 精确切片、key 唯一性、totals 恒定和三索引域映射。
- DesiredViewport 与 CommittedViewport 隔离，layout 热路径零 I/O。
- 每个 generation 只有一个 active viewport scope；新 scope 先转移共享 block lease，再取消 orphaned visible/prefetch work。
- visible queue 优先于 prefetch queue；旧 prefetch 不能占据新 visible target 的排队顺序。
- Cache、pool、height metadata、selection 和 RowDetails 状态的有界或用户显式增长属性。
- Container reset 完整性，以及 realized/edit/drag/applied block 的 pin 生命周期。
- Pinned filter 的唯一目标和 Header -> Indicator -> Flyout relay 对称释放。
- Row reorder 的 DataGrid session、presenter ghost 与 Source mutation 职责分离。
- 列宽 solver 不依赖 RowsPresenter 可见性；filler 不掩盖 star 分配。
- ControlTheme key、Template Part、伪类、Token、Semantic Part 和 Ready 视觉优先级。
- Light/Dark、Browser/Desktop、SizeType、冻结列、RowDetails 和 nested scrolling 的一致语义。
- 文档、源码 public surface、Gallery 与 tests 的一致性。
- Extra Content 与 Pagination 共享有效可见性；Pagination Align 为 Start/End 时分别形成左右相反布局，Center 使用右侧 Extra Content 加剩余区域居中 Pagination。

## 11. 测试与验证

### 11.1 Query、Source 与状态机

- Query validation、equality/hash、no-op、sort policy、schema capability 和 QueryChanged reentrancy。
- Range/result 边界、snapshot/totals consistency、entry payload、key uniqueness 与 index mapping。
- 旧 Query/Source/request 迟到、忽略 cancellation、snapshot expiry、Source replacement 和 detach。
- viewport scope 替换、共享 block lease 转移、orphaned request 协作取消、取消后 inflight 不可复用和 visible/prefetch 优先级。
- Initial error、refreshing error、stale invalidation、Query/page/group/scroll rollback。
- LocalSource typed filter/sort/group/page、stable tie-break、collection invalidation、cancellation 与 NativeAOT。
- 分页页大小选择器转发、页大小变化生成 `DataGridPageRequest`、页序号保持/末页收敛，以及顶部/底部状态同步。

### 11.2 虚拟化与交互

- 0、1、短尾、分组、collapse 和 int 上限下 SlotCount/WindowDataCount/child index 精确。
- Source probe 证明 layout、prepare 和 recycle 的 fetch/wait 计数为 0。
- 固定/自动行高、RowDetails、extent、anchor、滚轮、惯性、thumb、keyboard 与 ScrollIntoView。
- 快速 viewport 跨多个 Dispatcher turn 时，旧排队/执行请求收到取消且最终 visible target 不等待过时队列；共享 block 不重复读取。
- cache miss 保留旧 rows，superseded cancellation 不进入 Error/rollback，成功只提交最终 viewport，失败恢复 offset/slot，且无
  null/duplicate/flash reset。
- block pin、container reset、row/group pool 与 sparse heights 在 10,000 次滚动后不增长。
- selection/current 跨 range/page/recycle；全量选择不枚举数据；无 bulk capability 时不伪造全量命令成功。
- row reorder threshold、capture lost、session revalidation、key-relative target、source replacement 和异常清理。
- nested scrolling 在 DataGrid 边界处正确向外层 ScrollViewer 链接。
- 分页 Extra Content 覆盖四个 API、Content/Template 解析、顶部/底部独立实例、Start/End/Center 对齐、全部可见性条件和 re-template 生命周期。

### 11.3 视觉、性能和发布

- Light/Dark、所有 SizeType、普通/分组 header、empty/data、Title/Footer、上下 pagination、Extra Content 对侧布局、冻结列、scrollbar 与 RowDetails。
- selected/sorted、hover/focus/disabled、Loading/Refreshing 与 rollback 的结构和截图基线。
- 100 万本地/逻辑远端行的 sort、首屏、连续滚动、快速跳转、cache、并发、offset lookup 与 allocation。
- DataGrid detail/recycle/offset/extent 测试保持或加强，不删除或放宽既有行为断言。
- 运行 DataGrid 专用测试、完整 solution tests、DataGrid performance state verifier、AOT/trim verify、Gallery
  NativeAOT publish/startup smoke 和 `git diff --check`。

### 滚动后的静止指针命中

RowsPresenter在滚动布局更新后请求所属Window刷新最终scene的指针命中。同窗口请求合并，跨窗口独立；不轮询重绘。更新悬停不能改动滚动条、冻结列、选择或滚动链，隐藏／重新显示及关闭窗口后的回收均按既有显示生命周期处理。
