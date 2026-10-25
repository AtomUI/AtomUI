# DataGrid Changelog

本文档记录 DataGrid 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-06

- API
  - `DataGridSelectTriggerType` 新增 `Row`：点击行内任意位置改变选择，`Extended` 下普通点击切换单行、Shift 范围选择；`SelectIndicator` 默认值与 `Cell` 语义不变。
- Interaction
  - `Row` 下点击进入编辑或行处于编辑时保留选择；选择指示器增删和 `None` 禁选与其他触发方式一致。
- Gallery
  - 选择示例新增“点击选择框 / 点击行”触发方式切换，同步四种语言的选择说明。

## 2026-11-05

- Implementation
  - 编辑器的 OneWay 绑定副本保留完整 XAML 作用域与锚点，修复 ElementName 来源丢失；移除公共编辑路径中的反射绑定构造依赖，原定义与 Source 提交语义保持不变。

## 2026-11-04

- Changed
  - 默认 OpenMotion/CloseMotion 改为按样式实例创建的直接主题值，移除默认值回传与几何绑定链；播放准备保留原方向、原点和覆盖优先级。定义不持有控件或运行资源。

## 2026-11-03

- Changed
  - 筛选弹层通过其实际 Flyout 继承的 OpenMotion/CloseMotion 配置。DataGrid 本体不增加控制行、列、编辑及全部弹层的同名入口。
  - 明确 Motion 纯定义、单向配置和旧宿主释放边界；实际 AXAML 与共享定义的可回收性作为交付门禁。实现与验证状态见共享 Motion 验证页。

## 2026-11-02

- Changed
  - 明确 Dropdown 根层 Slide、嵌套菜单 ZoomBig 与实际 Flyout 的配置/释放责任。见[控件架构](overview.md)与共享动效契约；已完成对应默认规格接入。

- Interaction
  - `SelectIndicator` 左键点击普通单元格或选择列空白时保留已有选择；选择指示器继续增删选中行，单元格 current、焦点和编辑路径保持可用。
  - `Cell` 保留普通点击及 Ctrl/Shift 扩展选择语义。
- Gallery
  - 选择示例采用 `SelectIndicator`，同步四种语言中的选择操作说明。

## 2026-11-01

- Implementation
  - 主动行移动及其自失效重载按当前可见 slot 与行内偏移保持视口，修复拖至首行后目标行被卷出视口、拖动原首行后视口跳动的问题。
  - 同步、异步移动共用请求执行上下文；普通外部失效及共享数据源的其他表格继续按内容行 key 锚定。
  - Public API、主题、Token、尺寸、滚动条和拖动事件合同保持不变。
- Interaction
  - 右冻结列表头左缘（及相邻右冻结列之间）的拖拽手柄改为调整该右冻结列自身，向左拖变宽；原先调整的是可能已滚出视口或被冻结区遮住的前一列。

## 2026-10-31

- API
  - `DataGridFieldSchema` 新增 `IsEditable`；`DataGridLocalSourceDescriptor<T>.Field` 与 `DataGridLocalField<T,TValue>` 新增 `setter`、`fromScalar`。
  - `DataGridLocalSource<T>` 实现 `IDataGridEditableSource`（Commit 全有或全无、Delete、Cancel；Add 不支持）。
  - 新增 `EditCommitFailed` 事件与 `DataGridEditCommitFailedEventArgs`；`DataGridOperationColumn` 改为 public sealed。
- Implementation
  - 范围数据源下补齐单元格编辑与操作列整行编辑：编辑器使用列绑定的 OneWay 副本，提交时经列转换器转为字段值与 `DataGridScalar`，统一走 `CommitAsync`。
  - 返回请求 snapshot 的 mutation 不再触发重载，只重建被编辑行的相关单元格，Tab 连续编辑不再被重载取消。
  - `BeginEdit`、F2 以当前单元格为前提，不再要求行处于选中状态；修复 `EditingColumnIndex` 从未赋值导致恒为 0 的问题。
- Gallery
  - 可编辑单元格示例的列改为可编辑，新增“可编辑行”示例，并同步四种语言资源。

## 2026-10-26

- Implementation
  - 默认主题使用透明背景，通过根 Frame 的既有背景绑定补齐空白区命中，修复固定高度、有边框的表格在数据未填满时无法从空白区触发上下文菜单的问题；保持布局、裁剪和显式背景覆盖。
- Validation
  - 在已有 Frame 契约测试中覆盖左、中、右空白位置的真实右键输入与菜单显示；临时验证空/非空数据、背景覆盖、嵌套滚动及 Skia 像素一致性。

## 2026-10-25

- Fix：纵向滚动改变指针下方行后，按最终scene刷新静止指针的悬停命中；同一窗口的请求合并为一次呈现，不改变冻结列、尺寸、滚动条、选择或滚动链。
- Validation：本次纵滚后悬停变化已获用户视觉批准；Mac managed／NativeAOT功能检查通过，完整平台及原生最终像素继续单独验收。


## 2026-10-24

- Token：78个既有字段迁移至DependencyDefaultsV2／revision 1；17别名和61默认函数保留名称、类型、资源编号、slot及原默认公式。
- Theme：8个非别名直接副本按当前规范Own更新默认值；派生字段的独立明确配置优先，清除后恢复当前依赖，合法零值保留。
- Implementation：字段及别名声明与静态默认函数按职责分文件维护，默认求值不访问视觉或业务状态。


## 2026-10-22

- Theme：17项历史Table入口显式映射到规范Own字段；保留属性、slot和生成资源键，规范配置到达现有AXAML消费者。
- Compatibility：各层归一后覆盖；同层旧名与规范名不同值返回冲突，同值归一。其余内部字段及WholeV1默认公式保持。
- Validation：产品Window/Skia中明暗、三档密度、默认/历史配置及悬停/点击选择场景前后精确比较；覆盖实际header/cell/footer/details框体和两种过滤器Presenter的输入消费。该范围不代表原生显示面或全部DataGrid交互验证。


## 2026-09-26

- API
  - 新增 `IsShowPageSizeSelector`，默认值为 `true`，把页大小选择器可见性转发给顶部和底部 `Pagination.IsShowSizeChanger`，不修改 Pagination 既有属性名。
- Design
  - 定义顶部/底部 `PaginationExtraContent` 与 `PaginationExtraContentTemplate`，Extra Content 位于 Pagination 对侧；`Start`/`End` 形成左右互换布局，`Center` 使用右侧 Extra Content 加剩余区域居中 Pagination。
  - Pagination 默认保持 page navigation -> page-size selector；页大小选择器输入转换为 `DataGridPageRequest`，保持可保留的页序号并在页数不足时收敛到最后一页。
- Implementation
  - 顶部和底部 Pagination 共享同一 applied page state；修复页码收敛期间重复状态通知导致的分页请求回写。
  - 分页区域使用两个 `DockPanel` 组合 Extra Content 与 Pagination，按 Start/End 相反对齐，Center 保留右侧 Extra Content。
  - Extra Content 相对分页面板垂直居中，Start/End 通过 `PaginationPanelMargin` / `PaginationPanelMarginSM` 保持两侧一致水平间距。
- Validation
  - 定义 Extra Content API、对齐、可见性、生命周期和 Pagination 原始 UX 的验证矩阵。
- Docs
  - 新增 `pagination-extra-content-design.md`，并同步 overview、implementation、Pagination 默认布局边界和 LLMS 来源清单。
- Gallery
  - Basic Paging ShowCase 同时展示右侧 page-size selector 与左侧 Bottom Pagination Extra Content，并同步四种语言描述。

## 2026-09-14

- Implementation
  - Reattach cached row-header content to the current item when a row is reused, clear item references during recycle, and release the template cache on permanent detach. Fixes [#475](https://github.com/AtomUI/AtomUI/issues/475).
  - Keep row-header template replacement and removal independent of RowDetails visibility and loading/unloading events.
  - Honor each header template's recycling behavior, preserve explicit Header values when reapplying the same template, and release invalidated content while row headers are hidden.
- Validation
  - Cover row numbers through scrolling beyond the first viewport and back with ordinary and frozen columns, header data rebinding and cleanup, and runtime template replacement/removal.
- Docs
  - Document row-header template ownership and the separation between reusable visuals and item-scoped attachment.
- Design
  - 补充 `pagination.root` / `pagination.item` 语义部件：`pagination.root` 指向模板内顶部/底部两处 `Pagination` 槽位（`Multiple`），`pagination.item` 经 `>> .semantic-pagination-root >> .semantic-item` 跨根路由命中页码项，与上游 Table 的 pagination 语义区域对齐；生成器产出 `DataGridPaginationRootStyle` / `DataGridPaginationItemStyle`。

## 2026-09-13

- Design
  - 对齐 Ant Design 6.6.3 Table Semantic DOM：`DataGrid` 作为语义 owner 声明 `section`、`header.wrapper`、`header.cell`、`title`、`body.wrapper`、`body.row`、`body.cell`、`footer`、`content` 九个部件（`root` 由生成器隐式合成；`header.row` 无独立节点不声明）。
  - `body.row` 同时覆盖数据行与分组头行；`header.cell`/`body.cell`/`body.row` 的 marker 由目标控件构造时注入并在类被 `Classes.Replace` 清空后重挂，列拖拽 ghost 显式排除。
- API
  - 新增生成的专用部件 Style 类（`DataGridSectionStyle`、`DataGridTitleStyle`、`DataGridContentStyle`、`DataGridHeaderWrapperStyle`、`DataGridHeaderCellStyle`、`DataGridBodyWrapperStyle`、`DataGridBodyRowStyle`、`DataGridBodyCellStyle`、`DataGridFooterStyle`）与 `DataGridSemanticParts` 常量（Since 6.2.0）。
- Token
  - `CellPaddingMD` 从 12×12 修正为横向 8、纵向 12（AntD `cellPaddingInlineMD=paddingXS`、`cellPaddingBlockMD=paddingSM`）。
  - `HeaderSplitColor` 从 `ColorSplit` 修正为 `ColorBorderSecondary`（AntD `headerSplitColor`）。
  - `FilterDropdownBg` 从 `ColorBgElevated` 修正为 `ColorBgContainer`（AntD `filterDropdownBg`）。
  - 左右冻结列分界阴影从外扩 `±10px 0 8px 0` 修正为 `inset ±10px 0 8px -8px`（AntD `fixed.ts getShadowStyle`）。
- Gallery and validation
  - DataGrid Gallery 页迁移到 `GalleryShowCaseHost` 并新增 Semantic Parts 预览页签与 "Custom Semantic Part styling" 专用部件样式示例（延迟加载）。
  - 新增 `DataGridSemanticPartTests`（描述符、静态 marker、运行时 marker、回收保持、专用 Style 命中）、`DataGridTokenAlignmentTests`（四处视觉对齐锁定）与 Gallery 语义高亮页面测试。

## 2026-09-06

- Design
  - Define one active viewport request scope per generation, retain shared block leases before superseding the prior scope, and cancel orphaned visible or prefetch work so rapid thumb input cannot accumulate historical requests ahead of the final target.
  - Define foreground-visible scheduling ahead of scope-bound prefetch, generation-level bootstrap ownership, cooperative Source cancellation as the latency contract, and deterministic cleanup for scope, lease, work-item CTS and inflight state.
- API
  - Keep the public data-entry name `ItemsSource`, redefine its type as `IDataGridSource?`, and remove the transitional `Source` name without adding a compatibility alias or restoring the legacy enumerable/CollectionView path.
  - Name the immutable pagination value `DataGridPageRequest`, expose it consistently through `DataGrid.PageRequest`, `DataGrid.AppliedPageRequest`, and `DataGridFetchRequest.PageRequest`, and remove the provisional pagination-request names without compatibility aliases.
- Implementation
  - Replace the mutable ItemsSource/CollectionView execution path with immutable `DataGridQuery`, range-based `IDataGridSource`, atomic presentation snapshots, stable-key selection/current state and optional key-based mutation capabilities.
  - Keep the existing RowsPresenter/DisplayData container virtualizer, add bounded range cache/pins, two-request concurrency, latest-wins cancellation, snapshot-expiry recovery and sparse O(log M) variable-height lookup.
  - Add the empty-query local range fast path, synchronous Source/cache-hit commit path, exact fixed-height first realization and auto-sized nested-grid bootstrap without Source I/O in layout.
  - Remove per-cell sort subscriptions and project sort/filter/loading state from the single committed Query/presentation owner.
  - Make schema display access explicit with `DataGridFieldDisplayAccessor`; auto-generated columns now use disposable compiled bindings and never interpret protocol `FieldId` values as CLR property paths.
  - Keep committed content fully opaque while `LoadState` is `Refreshing`; only initial `Loading` or explicit `IsOperating` drives the existing Spin, so sort and other query refreshes do not produce a transient fade.
  - Scope automatic row-height estimates to the committed data generation: successful source replacement, filter/query, page, and invalidation/reset presentations restart sampling from the default baseline, while pending or failed generations preserve the current estimate.
  - Preserve the finite ScrollBar range invariant `Maximum >= Minimum`, including non-zero minima and subpixel travel ranges, so transient DataGrid extent changes cannot enter an invalid Avalonia RangeBase state.
- Gallery and validation
  - Add the deterministic million-row fake-remote showcase with latency, cancellation, failure/reload and snapshot-expiry controls.
  - Give the Basic Paging example explicit Auto-column `MinWidth` baselines so its first committed page exposes natural horizontal overflow without preloading later ranges, and cover scrollbar stability across page changes with an attached headless Gallery regression.
  - Add Query/Source, selection, mutation, lifecycle, virtualization, RowDetails, Gallery and million-row performance coverage plus a dedicated repeatable DataGrid benchmark.
  - Add the formal Issue457 regression suite for source rebind, filter, last-page, and collection-reset transitions, asserting generation height resampling, vertical scrollbar visibility, positive legal Maximum, and last-row reachability.
- Docs
  - Synchronize the public overview, implementation guide, formal Query/Range Source design, generated LLMS sources and Data Display navigation with the final contract.
  - Clarify that virtualized Auto sizing measures realized cells only and that stable first-page content extent must come from declared column geometry rather than offscreen Source I/O, Star compression or forced scrollbar visibility.

## 2026-09-05

- Design
  - Define immutable `DataGridQuery`, `IDataGridSource` range requests, Source schema, stable row/group keys and snapshot-based atomic presentation as the single DataGrid data model for local and remote sources.
  - Separate long source-data indices, int window-data indices and int display slots; keep the existing RowsPresenter/DisplayData/container pools as the only visual virtualizer.
  - Define latest-wins DesiredViewport/CommittedViewport coordination, bounded block cache and prefetch, sparse variable-height metrics, pin ownership, full failure rollback and zero Source I/O in layout/container hot paths.
  - Define declarative key/query/interval selection and key-relative optional mutation capabilities for data outside the loaded range.
- Docs
  - Add the dedicated Query and Range Source design and synchronize the DataGrid overview, implementation ownership, LLMS source map and Data Display navigation.

## 2026-09-04

- Implementation
  - Keep `DataGridCell` pseudo-classes authoritative with row state: push `:selected` to all cells when `DataGridRow.IsSelected` changes, and initialize pseudo-classes at cell creation, instead of relying only on opportunistic recycle/current-cell refresh paths.
  - In the cell ControlTheme, make the sorted-column background (`BodySortBg`) yield to the row selection background when the cell is in a selected row, matching Ant Design where the selected-row cell background outranks `td.ant-table-column-sort`; unselected rows keep the sort tint. Fixes [#454](https://github.com/AtomUI/AtomUI/issues/454).

## 2026-08-25

- Design
  - Define `DataGrid` as the internal pinned-filter semantic owner and select one eligible filter column in DisplayIndex order.
- Implementation
  - Relay pinned state through Header -> FilterIndicator -> Menu/Tree Flyout -> Popup without reflection or runtime discovery.
  - Close and release the previous target on column, presenter, template, or lifecycle replacement while keeping an already open filter Flyout open after ordinary unpin.

## 2026-08-18

- Implementation
  - Make finite star-column resolution independent of `DataGridRowsPresenter` visibility by storing the active column viewport on `DataGrid` and accepting empty-state widths from the normal or group header presenter.
  - Separate initial Auto measurement completion from star-width distribution, while preserving existing min/max, resize, frozen-column, scrollbar and filler handling through `AdjustColumnWidths`.
- Docs
  - Add the dedicated DataGrid column sizing design covering width-mode semantics, DataGrid-owned star resolution, presenter viewport ownership, empty-state template integration, filler boundaries, compatibility, and verification.
  - Synchronize the architecture and implementation documents with the shared finite-viewport column sizing model.

## 2026-07-23

- Implementation
  - Make pagination state projection independent of `ItemsSource`, `PageSize`, and template-application order.
  - Replay CollectionView pagination state before subscribing newly acquired top and bottom pagination parts.
- Docs
  - Define the CollectionView-owned pagination flow, template-part lifecycle, and replay invariants.

## 2026-07-04

- Docs
  - Define and implement the DataGrid column filter binding model: `Filters` as bindable filter item source and `SelectedFilterValues` as the single selected-state owner.
  - Document column `DataContext` binding support, generated accessor requirements for filter item DTOs, filter mode enums, `FilterDescriptions` projection rules, flyout checked-state synchronization invariants and explicit `Binding.DataType` usage when row `x:DataType` is active.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `DataGrid`.
  - Align generated output paths with `controls/data-grid/index-cn.md` and `controls/data-grid/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete DataGrid desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish DataGrid desktop architecture documentation under `docs/controls/desktop/data-display/data-grid/overview.md`.
  - Add DataGrid implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add DataGrid control-level changelog.
  - Add DataGrid Token documentation covering DataGridToken.
