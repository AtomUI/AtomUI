# Menu Changelog

本文档记录 Menu 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-18

- 修复 pin 向所有兄弟项传播导致的开合冲突；同级活动链由 owner 维护，普通关闭使用 coercion 而非关闭后回弹。
- 修复 Menu、分组、MenuFlyout/Dropdown 间复用残留 SemanticLevel；已实现和延迟实现后代同步到当前 owner 的专用 Style 契约。

## 2026-09-13

- Features
  - 补齐 plain `Menu` 自己的 Semantic Part 契约：上游 antd 6.6.3 公开的 12 个键路径（`root`、`item`、`itemIcon`、
    `itemContent`、`itemTitle`、`list`、`subMenu.item`、`subMenu.itemIcon`、`subMenu.itemContent`、`subMenu.itemTitle`、
    `subMenu.list`、`popup.root`）全部声明在 `Menu` 上，并生成 `MenuItemStyle` 等 11 个 Semantic Style。此前 plain Menu
    的语义 marker 借用 `DropdownButtonSemanticParts`，没有自己的 descriptor。
  - 引入 `MenuSemanticLevel` 与 `MenuSemanticLevelScope`：在容器 prepare 时按语义层级注入互斥 marker。层级只在 plain Menu
    子树内下发，`Menu` 给直接子项标一级、`MenuItem` / `MenuItemGroup` 给子容器标子菜单层，复用方（ContextMenu、
    MenuFlyout、DropdownButton 弹层）保持既有 `.semantic-item` 行为不变。
  - `TopLevelMenuItemTheme` 增加 `IconPresenter#ItemIconPresenter`（与 `MenuItemTheme` 同名同绑定，`Icon` 为空时折叠），
    使菜单栏项的 `itemIcon` 成为真实 Part；`ContentPresenter#HeaderPresenter` 同时承载 `itemContent`。两个菜单项模板的
    `Border#PopupFrame` 增加 `popup.root` marker，`MenuItemGroupTheme` 的标题与列表节点增加双层级 marker。
  - `Menu.IsPopupPinnedOpen` 从 internal 提升为 public，供跨程序集的 Gallery 语义预览在 AXAML 中声明钉住；与 `NavMenu`
    同一家族约定。声明式钉住在容器生成前写入时，容器 prepare 阶段按同一份请求补齐，并加 `_isApplyingPinnedOpen` 重入保护
    （设置 `SelectedIndex` 会再次触发钉住求值）。
- Fixed
  - 修复放置目标位于可视区外时展开子菜单导致进程崩溃（`MenuItem` 子菜单开关状态与 `Popup.IsOpen` 互相触发、
    无限递归直至栈溢出）：`SyncSubMenuPopupOpenState` 加 `_isSyncingSubMenuPopupState` 非重入保护。这是既有产品缺陷，
    与本次语义改造无关；弹层无法保持打开时（放置目标跑出 TopLevel 可视矩形）必现。
  - 修复 `TopLevelMenuItemTheme` 新增的菜单栏一级项 `IconPresenter` 缺少尺寸 Setter：`IconPresenter` 自身不测量
    图标，只有 `Margin` 时图标量到 `0x0`，肉眼不可见却仍占据图标与文字之间的间距，菜单栏文字被无谓右移。改为复用
    弹层菜单项的 `ItemMargin` + `ItemIconSize` Setter；图标为空时由 `IsVisible` 折叠，不再产生幽灵间距。顶层项
    模板的图标列同时不再参与 `IconPresenter` `SharedSizeGroup`，避免无图标项让出图标列宽。
- Tests
  - 新增 `MenuSubmenuOpenStateRecursionTests`：可视区外的子菜单展开不再递归崩溃，可视区内的展开 / 收起行为保持不变。
  - 新增 `MenuSemanticPartTests`，覆盖 descriptor 的 12 个键路径、selector、route、ContractType 与 cardinality。
  - 新增 `MenuSemanticLevelTests`：真实视觉树中的一级 / 子菜单层级互斥、复用方 marker 边界、`IsPopupPinnedOpen` 公开性、
    模板双层级 marker 存在性、以及「容器生成前声明钉住」的容器 prepare 补齐。禁用层级应用或钉住补齐时用例失败，证明其
    承重。
  - `MenuShowCasePageTests` 扩展：Semantic Parts 页签新增普通 Menu 预览，锁定 3 个预览、12 个 Part 描述、一级
    `itemTitle` / `list` 在菜单栏语义下为 0、弹层侧 `popup.root` / `subMenu.*` 的真实高亮。
  - `MenuShowCasePageTests` 的样式示例用例断言菜单栏一级项图标尺寸非零（宽度与 Bounds 均大于 0），并断言
    弹层内所有菜单行共享同一文字左边缘；把分组行图标移除后该用例失败，证明其承重。
  - 示例的 `subMenu.*` 弹层内菜单项统一带图标：`MenuItemGroup` 自带 `ItemsPresenter`，其子项不在 popup 的
    共享尺寸作用域内，混用有/无图标会让分组行图标列量到 0、文字左移；示例改为一致后文字左边缘对齐。
- Docs
  - 新增 [Menu Semantic Part 契约](semantic-part.md)，同步 overview、implementation 的语义摘要、pin 口径与验证矩阵。

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record Menu/MenuFlyout as the semantic owner for Menu.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-07-28

- Docs
  - Add the Menu popup scroll mode design covering `IsScrollEnabled`, `DisplayPageSize`, inherited configuration, `MenuFlyout` presenter relay and the internal `MenuPopupScrollHost` template branch.
  - Synchronize Menu overview and implementation docs with the popup scroll API, template composition, height algorithm, compatibility boundaries and verification requirements.

## 2026-07-14

- Docs
  - Define a single-owner hover intent model for delayed submenu open and close behavior.
  - Require cancellable or invalidatable delayed callbacks across `Menu`, `ContextMenu` and the default `MenuFlyoutPresenter`.
  - Document close, window deactivation and detach cleanup boundaries without changing the existing public interaction-handler contract.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Menu`.
  - Align generated output paths with `controls/menu/index-cn.md` and `controls/menu/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Menu desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Menu desktop architecture documentation under `docs/controls/desktop/navigation/menu/overview.md`.
  - Add Menu implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Menu control-level changelog.
  - Add Menu Token documentation covering MenuToken.
