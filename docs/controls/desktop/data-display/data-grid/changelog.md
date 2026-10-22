# DataGrid Changelog

本文档记录 DataGrid 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

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
