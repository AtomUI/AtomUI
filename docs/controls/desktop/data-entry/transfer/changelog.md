# Transfer Changelog

本文档记录 Transfer 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-01

- Semantic Part（方向限定对齐）
  - Rename `source` / `target` Part to `source.section` / `target.section`（marker 与 selector class 不变，仅
    Path/StyleType 命名对齐上游语义键；不考虑旧名兼容）。
  - Add direction-qualified section parts per owner: `source.header` / `target.header` / `source.title` /
    `target.title` / `source.body` / `target.body` / `source.list` / `target.list` / `source.footer` /
    `target.footer`（`Single`，与未限定部件共享终端 marker，经 `.semantic-source` / `.semantic-target` 方向锚点
    路由区分实例）。
  - Add item-level parts at the Transfer owner level: `item` / `itemIcon` / `itemContent` 与方向限定
    `source.item` / `target.item` / `source.itemIcon` / `target.itemIcon` / `source.itemContent` /
    `target.itemContent`（`>>` 后代步进路由，生成的样式直接挂在 Transfer 作用域下命中条目目标；仅 `ListTransfer`
    发布 itemIcon / itemContent，树侧条目区域由 `TreeViewItem` 家族契约覆盖）。`TransferListItem` /
    `TransferListView` / `TransferTreeView` 不再注册为独立 Semantic owner；方向条目类仍由视图按 `ViewType` 在
    prepare 路径幂等补挂（路 A）。
  - Generator / runtime: 部件名接受点分驼峰段；selector 去重键由 selector class 放宽为解析路由；路由文法支持
    首段自锚点过滤类；`SemanticPartTargetResolver` 高亮按锚链匹配（候选祖先链须携带全部中间锚点类）。
- API
  - 将 `AbstractTransfer` 的 `BuildSourcePanelSource`（原 `List<IItemKey>?`）与 `BuildTargetPanelSource`（原 `IEnumerable<IItemKey>?`）返回类型统一收敛为 `IReadOnlyList<IItemKey>?`：两者实现均为立即物化的具体集合（`null` / 数组 / `List`），只读集合契约更精确并从类型层面消除 "possible multiple enumeration"。两方法均为非 virtual 的 protected 成员且仓库内仅基类内部调用，兼容风险可忽略。`TreeTransfer` 目标面板局部变量同步声明为 `IReadOnlyList<IListItemData>`。
- Semantic Part
  - Add Semantic Part descriptors for `ListTransfer` / `TreeTransfer`（公开 owner；`AbstractTransfer` 为抽象基类不发布）: `root`、`source` / `target`（`TransferItemDecorator` 实例，`.semantic-source` / `.semantic-target`，`Single`）、`actions`（`StackPanel#ActionsLayout`，`.semantic-actions`，`Single`）与分区 Part `header` / `title` / `body` / `list` / `footer`（路由 `/template/ .semantic-scope-section /template/ .semantic-*`，`Multiple` 恒为 2，`RuntimeCreated=true`，标记节点位于 `TransferItemDecorator` 模板内，与 Collapse / Splitter 先例一致），对齐上游语义键 `source.*/target.*/header/title/body/list/footer`。
  - Add `body` Part via a new `DockPanel#BodyLayout` wrapper node in `TransferItemDecoratorTheme`（过滤输入与列表宿主的父节点，上游 `body` 对齐）；`Frame` 增加 `Background` 模板投影；footer 在 Dock 顺序中前移至 body 之前。
  - Add `TransferListItem` as an independent public owner publishing `itemIcon`（`CheckBox#SelectedIndicator`，`.semantic-item-icon`）与 `itemContent`（`ContentPresenter`，`.semantic-item-content`），对齐上游条目级语义键。
  - Fix nested view containers to carry the inherited item marker: `TransferListView.CreateContainerForItemOverride` / `PrepareListViewItem` 与 `TransferTreeView.CreateContainerForItemOverride` 现在补挂 `.semantic-item`（与 `ListView` / `TreeView` 建路径一致）。
- Gallery
  - Migrate the Transfer ShowCase from `GalleryStickyTabsHost` to `GalleryShowCaseHost` with a lazy Semantic Parts Preview（ListTransfer 与 TreeTransfer 各一）and align the custom Semantic Part styling example with the upstream `style-class` demo: exactly two `ListTransfer` instances over 20 items with 9 target keys — an `Error` instance styled via `source` / `target`（`#80FAFAFA` 分区底色）、`header`（主色前景 + 加粗）与 `actions`（`#99FFF2E8`，经嵌套子样式作用于方向按钮本体），and a `Warning` instance adding `source` / `target` 背景 `#99F6FFED` + 边框 `#B7EB8F` 与 `header` 前景 `#8DBCC7`（error / warning 状态边框由默认主题驱动）。
  - Align the ListTransfer Semantic Parts preview with the upstream `_semantic` demo: filter input（"Search here"）、`source` / `target` 分区底色 `#FFF7E6` / `#E6F7FF`、200×250 分区尺寸、20 项中 2 项在目标侧（key 3 / 9）以及双侧 `Custom Footer`，使 `footer` 部件可被高亮定位；TreeTransfer 预览同样补双侧 `Custom Footer`。
- Docs
  - Add `semantic-part.md` 公开契约文档；overview LLMS 语义区域表更新为 11 行真实 Part 集合；implementation.md 记录上游审计证据、marker 纪律与 owner `SizeType` 不驱动视觉分支的尺寸基线。

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record AbstractTransfer as the semantic owner, with TransferSelectDropdown used only as the relay adapter.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-07-05

- API
  - Document `TargetKeys` and `SelectedKeys` as default `TwoWay` key-state contracts with collection mutation support.
- Implementation
  - Record the single-owner Transfer key flow, attached-only collection subscriptions and in-place writable collection updates.
- Docs
  - Add the controlled key binding model to the architecture and implementation documents.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Transfer`.
  - Align generated output paths with `controls/transfer/index-cn.md` and `controls/transfer/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Transfer desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Transfer desktop architecture documentation under `docs/controls/desktop/data-entry/transfer/overview.md`.
  - Add Transfer implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Transfer control-level changelog.
  - Add Transfer Token documentation covering TransferToken.
