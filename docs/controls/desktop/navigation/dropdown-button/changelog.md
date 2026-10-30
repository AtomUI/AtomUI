# DropdownButton Changelog

本文档记录 DropdownButton 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-10

- API
  - `MatchAnchorWidth` 重命名为 `IsPopupMatchSelectWidth`，与全局同语义属性命名对齐
    （`AbstractSelect` / `Select` / `TreeSelect` / `Cascader` / `AbstractAutoComplete`，对应 antd
    `popupMatchSelectWidth`）；语义不变——开启后弹层 presenter 最小宽度锚定到按钮自身宽度。
    同步 Gallery 示例、页面契约测试、Examples 快照与语义文档。
- Fix
  - Gallery Semantic Parts 预览两个缺陷修复：
    - 触发按钮去掉硬编码 `Width="160"` 并改为 `ButtonType="Link"` + `TriggerType="Hover"`（antd
      `_semantic.tsx` 的触发器是链接式锚点，只给弹层 `styles.root` 设 `width:200`，触发器保持内容宽度），
      消除 "Hover me" 左右大片留白。
    - 弹层根（`MenuFlyoutPresenter`）与嵌套子菜单弹层根经 `DropdownButtonShowCase.axaml.cs` 在
      `Loaded` / `Opened` 时递归注册进 `SemanticPartPreview.AdditionalRoots`（同 InfoFlyout 的
      跨视觉根弹层注册模式；`SemanticPartHighlightSession` 只自动发现 owner 模板内 Popup，Flyout
      代码创建的弹层与子菜单弹层均不可达），修复弹层侧 `popup.root` / `itemTitle` / `item` /
      `itemContent` / `itemIcon` 悬停高亮全部无目标的问题。页面测试按 InfoFlyout 先例收窄
      `Loaded=` 禁令为「仅允许 SemanticPartPreview 的语义根注册处理器」。
  - 新增 `DropdownButtonSemanticPartHighlightTests`：悬停各 Part 卡片断言 adorner 数量
    （root=1、popup.root=1、item=6、itemTitle=2、itemContent=6、itemIcon=3，覆盖子菜单内条目），
    并断言触发器 `Width` 为 NaN 且布局宽度等于测量期望（无固定宽、无拉伸留白）。

## 2026-09-09

- Design
  - 补齐上游 antd Dropdown 的 `itemTitle` Semantic Part：新增 `MenuItemGroup` 控件承载分组标题
    （标题渲染为 `GroupTitlePresenter`，对应 antd 的 `ant-menu-item-group-title`），`itemTitle` 声明为
    `Multiple` + `CrossVisualRoot` + `CrossNestedOwners` + `RuntimeCreated`，route 为
    `>> .semantic-item-title-group /template/ .semantic-item-title`。
  - 将 `MenuItemGroup` 接入 `Menu` / `MenuFlyoutPresenter` / `MenuItem` 的容器创建与准备路径，分组内的子菜单项
    与普通菜单项一致携带 `semantic-item` marker。
- Fix
  - 移除 `MenuItemTheme.axaml` / `TopLevelMenuItemTheme.axaml` 中 `PART_Popup.IsOpen` 对 `IsSubMenuOpen`
    的 TwoWay TemplateBinding，改为 `MenuItem` 代码同步弹层开关：模板应用后的属性变更同步开/关，
    声明式 `IsSubMenuOpen="True"`（对应 antd `defaultOpenKeys`）延迟到下一调度帧再打开，
    `Opened` / `Closed` 事件回写 `IsSubMenuOpen`。修复声明式打开子菜单在父弹层强制布局期间同步
    Open 导致的 `Collection was modified` 崩溃（`PopupOverlayLayer.MeasureOverride`）。
- Docs
  - `semantic-part.md` 由 4 个 Part 更新为 5 个 Part（`popup.root` / `itemTitle` / `item` / `itemContent` /
    `itemIcon`），移除「省略 `itemTitle`」说明并同步 overview / implementation 的语义区域表与 marker 注入说明。
  - Gallery Semantic Parts 预览对齐 antd `_semantic.tsx`：`Group title`（SaveOutlined / EditOutlined）、
    `SubMenu`（`Item 1` → `Option 1` / `Option 2`）、分隔符、`Delete`（DeleteOutlined + `ColorError`），
    PartDescriptions 顺序与 antd 语义部件顺序一致。

## 2026-09-08

- Docs
  - Add the DropdownButton Semantic Part contract (`semantic-part.md`): 4 parts — `popup.root`
    （`ArrowDecoratedBox` 弹层根视觉面，对应上游 antd 的弹层根 `root`）、`item`（`MenuItem` 容器，Multiple）、
    `itemIcon`
    （`ItemIconPresenter`）、`itemContent`（`ItemTextPresenter`）；`root` 为隐式 owner Part。全部弹层部件声明
    `CrossVisualRoot` + `RuntimeCreated`，`itemIcon` / `itemContent` 另声明 `CrossNestedOwners`，全部
    `Since = "6.2.0"`。对齐上游 antd Dropdown Semantic DOM：不发布触发侧部件，省略 `itemTitle`（AtomUI Menu
    无分组标题）。
  - Rewrite the overview LLMS semantic-region table to the 4-part popup contract and record the runtime marker
    injection (`MenuFlyoutPresenter.OnApplyTemplate` for `popup.root` on the `ArrowDecoratedBox` surface;
    `MenuFlyoutPresenter` / `MenuItem` container paths for `item`) in the implementation doc.
- API
  - Promote `IsPopupPinnedOpen` from internal to public so the Gallery semantic-part preview can pin the
    flyout open, consistent with `AbstractSelect` / `FlyoutHost`.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record DropdownButton as the semantic owner, with DropdownFlyout/MenuFlyout used only as relay adapters.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-03

- Design
  - Inherit Button `IconWidth` / `IconHeight` semantics for the user and loading icons across desktop and Browser templates.
  - Keep `OpenIndicator` sizing independent from the inherited user/loading icon sizing contract.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `DropdownButton`.
  - Align generated output paths with `controls/dropdown-button/index-cn.md` and `controls/dropdown-button/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete DropdownButton desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish DropdownButton desktop architecture documentation under `docs/controls/desktop/navigation/dropdown-button/overview.md`.
  - Add DropdownButton implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add DropdownButton control-level changelog.
  - Document that DropdownButton does not require a dedicated Token document and records its theme dependencies in the overview.
