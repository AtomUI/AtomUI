# NavMenu Changelog

本文档记录 NavMenu 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-10-29

- 内联子菜单展开、收起时共享内容视口只失效自身测量，不再逐帧失效全部祖先排列（原实现使窗口外框等绘制型祖先每帧整窗重绘）；侧栏往返后的分配缺口改为仅重新排列 DesiredSize 变化的祖先来修复。
- Wayland 启动时窗口先按缩放 1 布局再切到真实缩放导致的菜单文字尺寸变化，随 Window 初始缩放预测消除。

## 2026-10-28

- 共享显示隐藏入口在展开可见前准备零占位，避免首个 Loaded 边界前临时发布自然高度；准备失败时释放临时覆盖。

- 修复选择页面后收起、展开侧栏时，展开路径已恢复但子菜单仍被零高度视口裁剪，导致需要先关闭再打开的问题。共享内容视口同步失效祖先排列，保留宽度动效、选中路径和单次点击语义。

## 2026-10-27

- Inline 子菜单改用 `ContentViewport` 和主题声明的 `atom:Motion`，移除 item 持有的旧展开执行器，保留版本、互斥与业务事件核对。

- Inline Collapsed 根宽度改由原生 Animation 驱动内部布局值，移除16ms延时循环与逐帧Dispatcher投递；保留CubicEaseOut、主题时长、尺寸coercion和选中/展开路径语义。
- 反转在取消前捕获当前呈现宽度；Mode切换取消旧宽度执行，避免临时宽度继续影响新的布局模式。动画绑定释放后由当前请求完成一次收尾，保留base Width/MinWidth及绑定。

## 2026-09-14

- 修复根菜单卸载后，外部持有的节点/分组经资源宿主和属性订阅保留整个菜单的问题。挂载作用域统一管理资源 attachment 与数据绑定，重新挂载恢复最新资源、数据和原选择；资源回调删除节点后不再恢复旧绑定。
- 修复 Entries Replace 同步重入导致成员和结构 owner 不一致的问题。替换先提交 ownership，再完成父关系及通知；回调异常继续完成其他投影，嵌套分组同样处理，同集合的未完成 Replace 拒绝重入写入。
- 修复默认选中路径的排队请求覆盖后续显式选择，以及移除选中节点后旧祖先路径残留。已应用路径按容器生命周期维护，并允许清理期间同步回收或改选。
- 修复 Inline 父项按下后在子 Header 释放仍触发父项，以及 capture-lost 清理误取消新控件捕获的问题。命中检查限定到原 Header，并保留正常整行交互。
- 修复子菜单重新打开后未刷新语义子项 CanExecute，以及无命令子项的 Click 冒泡执行祖先命令的问题。
- 补齐节点 HeaderTemplate 与 ItemKey 的运行期投影；模板清空时恢复当前 owner ItemTemplate。
- 增加竞态、回调异常、指针几何、键盘激活、DynamicResource 弱引用回收与反复重新挂载回归；公共 API、Token 和默认主题布局保持既有契约。

## 2026-09-11

- Semantic Part
  - 为 `NavMenu` 公开 12 个上游 Semantic 键路径：一级 `root`、`item`、`itemIcon`、`itemContent`、`itemTitle`、`list`，
    子菜单 `subMenu.item`、`subMenu.itemIcon`、`subMenu.itemContent`、`subMenu.itemTitle`、`subMenu.list`，以及
    `popup.root`；新增 `NavMenu.SemanticParts.cs` descriptor，`NavMenu` 改为 partial，生成 `NavMenuItemStyle`、
    `NavMenuItemIconStyle`、`NavMenuSubMenuItemStyle`、`NavMenuPopupRootStyle` 等 11 个 `AtomUI.Theme.Styling` 类型
    （since 6.2.0）。
  - `NavMenuItem`、`NavMenuGroupItem`、`NavMenuDividerItem` 是 internal 容器，不能持有 descriptor，也不能作为
    `ContractType`，因此 Part 全部声明在唯一的 public owner `NavMenu` 上；菜单项容器的 `ContractType` 取最近 public
    基类 `HeaderedSelectingItemsControl`，弹层框体取 `Border`。
  - 层级区分靠互斥运行时 marker：一级容器 `.semantic-item` / `.semantic-scope-group`，子菜单容器
    `.semantic-sub-menu-item` / `.semantic-sub-menu-group`。`NavMenuEntryContainerCoordinator` 在 prepare 时先移除另一
    层级再幂等补齐当前层级，保证容器在 owner 之间转移与回收复用时既不残留也不重复。
  - 全部非 root Part 的 route 以 `>>` 开头并声明 `CrossNestedOwners`：容器是运行时生成物、不在 `NavMenu` 模板内，
    无法用 `/template/` 到达；`popup.root` 的节点还位于 `Popup.Child` 属性值子树，因此同时声明 `CrossVisualRoot`。
    `>>` 只沿逻辑祖先链定位锚点，不在最近语义 owner 处停止，因此层级隔离由 marker class 承担而非 route。
  - 静态 marker 落在 `NavMenuItemTheme.axaml`（`.semantic-scope-header` 路由跳点、`.semantic-popup-root`）、三个 header
    主题（icon / content 的两个层级 marker 在同一节点共存）与 `NavMenuGroupItemTheme.axaml`（标题 / 列表的两个层级
    marker 同理）。默认主题不消费 `.semantic-*`。
- Docs
  - 新增 `semantic-part.md`，定义唯一 owner 边界、12 个 Part、marker 放置与路由、route 组合符与 `>>` 契约、Selector
    用法、状态与数量语义、尺寸基线、定制边界和验证矩阵。
  - 更新 `overview.md` §3.1 Semantic Part 摘要与 §9 验证策略，更新 `implementation.md` §10 Semantic Part 实现与 §11
    验证范围。
- Gallery
  - `MenuShowCase` 从 `GalleryStickyTabsHost` 切换到 `GalleryShowCaseHost`，新增 Semantic Parts Tab，并补齐 en-US / zh-CN /
    zh-TW / pt-BR 文案。
  - Semantic Parts Tab 提供两个预览，夹具逐项对齐上游语义示例：`Navigation One`（Mail 图标）、`Navigation Two`
    子菜单（Appstore 图标）内含 `Item 1` 分组与 `Option 1`（Mail 图标）/ `Option 2`，以及一级分组 `Navigation Three`
    与 `Option 3` / `Option 4`；两者都用 `DefaultOpenPaths` / `DefaultSelectedPath` 复刻上游语义示例的
    `openKeys` / `selectedKeys`，使子菜单默认展开、首项默认选中。
  - Inline 预览覆盖不依赖弹层的全部 Part：两级容器都在视觉树内展开，`item`、`itemTitle`、`list`、`subMenu.*`
    都有实例；`popup.root` 在 Inline 模板里没有弹层节点，实例数为 0，与上游 inline 模式 popup 不生效一致。
  - Vertical 预览补齐弹层侧覆盖：code-behind 通过公开的 `INavMenuItem` 契约打开第一个可展开项，并把该弹层的
    `Popup.Child` 注册进 `SemanticPartPreview.AdditionalRoots`（同 DropdownButton / InfoFlyout 的既定模式），
    让 `popup.root` 与垂直模式 `subMenu.*` 成为可定位目标。这里不使用 `NavMenu.IsPopupPinnedOpen`（internal，
    只供测试/内部诊断），弹层打开完全走公开业务契约；`GetTemplateDescendants` 在运行时生成的菜单项容器处中断，
    无法自动发现该 Popup，因此必须显式注册根。
  - Vertical 预览改为在 AXAML 里声明 `IsPopupPinnedOpen="True"`，由 NavMenu 自身负责打开与保持，不再由 Gallery
    代码手动开关子菜单：`NavMenu.IsPopupPinnedOpen` 提升为公开属性（与 `Tour`、`DropdownButton`、`AbstractSelect`
    同一家族约定），`NavMenuItem` 上的同名属性保持 internal。Gallery 代码只负责把已打开弹层的 `Popup.Child` 注册进
    `SemanticPartPreview.AdditionalRoots`。
  - 修复声明式钉住在容器生成前写入时不生效的问题：钉住请求此前只落在"当下已实现的容器"上，属性变更、attach、
    Mode 切换都发生在容器生成之前或容器重建之际，声明式钉住（预览在 AXAML 中声明）当场丢失。现在把请求以
    `INavMenuNode` 为键保存在 `NavMenuPinnedOpenCoordinator` 中，独立于容器生命周期；条目集合变化走合并调度，
    在 `ItemsPresenter` 收敛后求值。迟到或重建的容器都能重新带上钉住。
  - `subMenu.item`、`subMenu.itemIcon`、`subMenu.itemContent`、`subMenu.itemTitle`、`subMenu.list` 补声明
    `CrossVisualRoot=true`：Vertical / Horizontal 下它们位于菜单项模板的 `Popup.Child` 子树，属于跨视觉根内容，
    只扫描主视觉树无法解析到这些容器；Inline 模式下节点在主视觉树内，声明不影响可达性。
- Tests
  - 新增 `NavMenuSemanticPartTests`，覆盖 descriptor 的 Part 名称/顺序/字段、模板静态 marker、三种 mode 与 inline
    collapsed 的层级 marker 数量、`subMenu.*` route 不命中一级容器，以及展开与选择不改变 marker 数量。
  - `MenuShowCasePageTests` 新增两条：一条实例化 Semantic Parts Tab，锁定两个预览的挂载与 owner 解析、各自 12 个
    Part 描述、Inline 预览两级部件的真实高亮与 `popup.root` 目标数为 0、Vertical 预览弹层被钉住打开（并忽略普通
    关闭请求）、`popup.root` 与 `subMenu.*` 的真实高亮，以及跨预览的悬停互斥；另一条锁定示例项使用生成
    Semantic Part Style（含 `x:SetterTargetType`）且不依赖 `PART_*`。
  - 新增 `Pinned_Open_Declared_Before_Containers_Exist_Still_Opens_Submenu`，覆盖"容器生成前声明钉住"；
    禁用条目集合变化的钉住收敛后该用例失败，证明它锁住了本次修复的缺陷。另新增
    `Pinned_Open_Request_Survives_Container_Rebuild_And_Late_Items`，覆盖 Mode 切换重建容器后请求仍在、
    以及请求写入后追加条目不影响已解析目标；`IsPopupPinnedOpen_Is_Public_Api_For_Cross_Assembly_Previews`
    锁定跨程序集预览的公开入口。
  - 新增 `SourceKey="menu-semantic-part"` 示例，按项目规范使用 owner-scoped Control Selector + 生成的 Semantic Part
    Style 演示定制：root 视觉属性、`NavMenuItemStyle`、`NavMenuItemIconStyle`、`NavMenuItemContentStyle`、
    `NavMenuItemTitleStyle`、`NavMenuListStyle`、`NavMenuSubMenuItemStyle`、`NavMenuSubMenuItemTitleStyle`、
    `NavMenuSubMenuItemContentStyle`、`NavMenuSubMenuListStyle` 与 `NavMenuPopupRootStyle`，并以 Vertical / Inline
    两个 NavMenu 对照展开态与弹层态。


## 2026-09-10

- Design
  - 登记与 Collapse、Expander 共用的[内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)，明确 Core 执行职责、菜单状态和事件所有权、模板裁剪及嵌套测量边界。
- Implementation
  - 展开进度由动画执行器的私有附加属性持有，基础 actor 通过内部 `IMotionActorLayout` 协作，分离通用布局与内容开合职责。
  - `NavMenuItem` 接入 Core 内部 `ContentExpansionAnimator`，原生 `Animation` 同步驱动内部布局进度与透明度；控件继续拥有展开状态、排队请求身份和完成事件。
- Behavior
  - Inline 手风琴互斥在打开请求时生效，使旧分支收起与新分支展开同时开始。
  - 收放动画覆盖含内部间距的完整高度和透明度，连续点击从当前画面接续；过期动画不得提交完成状态或事件。
  - 第一个时钟 tick 前保持当前帧，完成 tick 后保持最终帧，避免动画与稳定状态交接时闪烁或位置突跳。
- Theme
  - Inline 收放改用 `MotionDurationMid`，配合统一缓动与既有 actor 裁剪，保留自然文字尺寸、最终布局和滚动契约。
  - 将 `VerticalChildItemsMargin` 放入内部内容 frame，随 viewport 高度裁剪，避免结束时外部间距先恢复再消失导致文字回弹。
- Lifecycle
  - 关闭 motion、重新应用模板及 visual detach 时使请求失效，取消动画并解除内部进度和布局接入，保留自定义尺寸与变换。
- Verification
  - 新增真实指针输入与可控时钟测试，覆盖完整高度插值、等高分支切换、快速反转、多分支连续切换、过期事件、motion 关闭与 detach/reattach，并检查首个时钟 tick 前、完成 continuation 前的画面状态及子项文字坐标。

## 2026-09-04

- Architecture
  - 将用户激活定义为“按下准备、合法释放提交”的有序事务：指针合法释放与键盘 Enter/Space 复用激活入口；程序化 `SelectedItem` 只进入选择协调器，不执行节点命令或触发 `NavMenuItemClick`。新增专项设计文档 [NavMenu 项激活事务设计](item-activation-design.md)。
  - `SelectedItem` 语义收窄为已提交选择；指针按下只建立待提交事务、显示 selected 背景的 pointer-hold 视觉并按模式尝试移动真实焦点，文字颜色保持按下前状态，不覆盖 keyboard-active owner，不改变选择、不切换子菜单、不执行命令、不触发路由事件。
  - 定义指针提交合法性判据（同一指针、主按钮、释放点命中待提交项视觉子树、节点与语义祖先可用）与取消路径集合（拖离释放、当前捕获指针丢失、节点移除或禁用、detach、非主按钮释放、新按下替代）。
  - 修正叶子提交事件顺序：选中路径与 `IsSelected`、`SelectedItem`、`NavMenuNodeSelected` 先于节点 `Command` 与 `NavMenuItemClick`，事件回调读取的公共状态为已提交新值；父节点提交不修改 `SelectedItem`。
  - 为同步重入定义 superseded policy：`SelectedItem` 在选择事件前被改写时不发布陈旧 `NavMenuNodeSelected`，在选择事件处理期间被改写时停止原节点后续命令与 `NavMenuItemClick`，避免部分提交。
  - 交互 handler 基类统一持有激活事务（待提交项与指针身份），按激活目标分派叶子选择提交与父节点展开激活；Inline 父节点切换展开，Default 父节点确保 popup 打开，hover 延迟打开流程保持独立。
- Theme
  - pointer-hold 与 keyboard-active 由独立 owner 和独立主题输入维护：pointer-hold 通过 `IsPointerHold` 仅使用对应的 light/dark selected 背景 Token，不覆盖既有文字颜色；keyboard-active 通过 `IsKeyboardActive` 使用 `ItemActiveBg`。前者只预览 selected 背景，不写入 `IsSelected` / `SelectedItem`，拖出清除、移回恢复。
  - 对齐 Ant Design 6.6.2 录屏中的背景变化：header 背景改用 300ms `MotionDurationSlow` 与 CSS `ease` 等价曲线；按下先置 pointer-hold 再捕获，合法释放时保留 pointer-hold 直到 selection 提交完成，避免中间 hover、透明或默认背景闪烁。
  - 指针捕获目标改为带 `Cursor=Hand` 的 item header，按住期间持续显示手型指针；capture-lost 生命周期跟随实际捕获目标。
- Token
  - 新增 `ItemBackgroundMotionEasing` 作为 Base、Inline、Horizontal item header 背景 transition 的统一缓动入口，默认映射 CSS `ease`；主题不再内联构造或复制 `SplineEasing`。
- Verification
  - 覆盖按下零提交副作用、背景与 selected 相同且文字颜色不变、按下/释放背景无中间闪烁、300ms ease 背景过渡、手型捕获目标、合法释放单次提交、完整事件顺序、同步选择重入、捕获指针身份、handler 替换、父节点释放激活、键盘与指针提交一致性，以及 pointer-hold / keyboard-active 独立所有权。

## 2026-08-26

- Architecture
  - Separate transient popup/submenu closing from persistent selection clearing: pointer outside, window deactivation, platform focus loss, non-client click, inline-collapsed transitions, and mode changes preserve `SelectedItem` and selected path.
  - Keep public `Close()` compatible as the explicit operation that closes all submenus and clears selection.
- Verification
  - Cover platform focus loss, mode replacement, popup reopen projection, and the public `Close()` selection-clearing boundary.

## 2026-08-25

- Docs
  - Add the shared Popup pinned-open design link and record NavMenu/NavMenuItem as the semantic owner for NavMenu.
  - Preserve ordinary close behavior after unpinning and allow lifecycle teardown to release the Popup host.

## 2026-08-19

- API
  - 节点 `Tooltip` 支持直接传入 `ToolTip` 实例：实例上显式设置的呈现类附加属性（位置、颜色、箭头、文本换行等）在打开时优先于菜单级与宿主配置，未设置的回落；纯文本用法不变。
- Verification
  - 覆盖 ToolTip 实例经节点、binder、容器到 `PART_Header` 的原样透传。

## 2026-08-18

- API
  - 新增 `NavMenuNode.Tooltip` / `INavMenuNode.Tooltip` 独立节点提示内容，未设置时回退到节点 `Header`。
  - 新增节点级 `IsTooltipEnabled`，以及菜单级 `IsCollapsedTooltipEnabled`、`CollapsedTooltipPlacement`、`CollapsedTooltipShowDelay`、`CollapsedTooltipBetweenShowDelay` 折叠提示策略。
- Architecture
  - 由 `NavMenuItem` 统一计算 `EffectiveCollapsedTooltip`，只允许有效 inline collapsed 状态下的顶层叶子节点生成提示内容。
  - 将节点真实 `Header` 单独投影为 `NodeHeader`，避免 generated container 的节点对象进入首字符转换或 Tooltip fallback。
  - 节点 Tooltip、节点开关和菜单策略 binding 归入现有 container `CompositeDisposable`，在 rebind、clear 和 recycle 时统一释放并清空。
- Theme
  - 将 `ToolTip.Tip`、placement 和 delay 附加到实际 `VerticalNavMenuItemHeader`，保持 `ToolTip` 服务的视觉宿主边界。
  - 无图标折叠项的首字符改为从节点 `Header` 投影取得，不再对整个节点调用 `ToString()`。
- Verification
  - 覆盖显式 Tooltip、Header fallback、节点/菜单禁用、submenu 抑制、模式切换、运行期更新、自定义节点通知、动态资源和容器回收。

## 2026-08-05

- API
  - 定义 `INavMenuEntry` 作为节点、分组和分隔线的共同结构契约，`INavMenuNode` 保持唯一可交互节点语义。
  - 定义 `NavMenuGroup`、`NavMenuDivider` 和 `NavMenuNode.Entries`，支持在根、子菜单、popup 和分组中任意层级组合结构 entry。
  - 保留 `NavMenuNode.Children : IList<INavMenuNode>`，将其定义为 `Entries` 的实时语义节点兼容视图；`INavMenuNode.Entries : IEnumerable<INavMenuEntry>` 通过协变接口默认 `Entries => Children` 保持既有自定义节点兼容，`NavMenuNode.Entries` 继续提供 `IList<INavMenuEntry>` 可写入口。
  - 定义 `NavMenu.Header`、`HeaderTemplate`、`Footer`、`FooterTemplate` 和 `ItemSpacing`；继续以 `IsInlineCollapsed` 作为唯一折叠状态源，不增加相反语义的 `Expanded` 属性。
- Architecture
  - 定义 `NavMenuItem`、`NavMenuGroupItem`、`NavMenuDividerItem` 三类内部容器，并由统一 entry container coordinator 管理类型分派、独立 recycle key、prepare 和 clear。
  - 定义 direct `Items`、`ItemsSource`、节点 `Entries` 和分组 `Entries` 的统一 entry source 校验；初始装载、source replacement、Add、Replace、Reset 均只接受 `INavMenuNode`、`NavMenuGroup` 或 `NavMenuDivider`，仅实现 marker 的未知种类也在容器生成前确定性失败。
  - 为内置 `NavMenuNode` / `NavMenuGroup` 定义唯一弱 structural owner，禁止同一有状态实例在 entry 树中重复挂载，并在 Remove、Replace、Clear 和根 source removal 后释放；无状态 `NavMenuDivider` 保持可复用。
  - 每个内置 entry owner 和根 `NavMenu` 都协调自己的 structural scope：递归穿过 custom node，遇到 built-in child 后由 child 自身协调器接管。custom node 的内置后代继承最近 scope owner，使离线树和根菜单中的 custom wrapper 都无法绕过唯一性；动态 source 重新执行 owner-cycle 校验，拒绝指回当前 built-in owner 或祖先。协调器只弱订阅当前 scope 的 custom source，避免纯 built-in 深树形成 O(N²) 祖先订阅和重复扫描。
  - `Entries` 的 Add、Insert、Replace 和批量初始化在 mutation 前预检完整 prospective tree；成功写入先提交全部 structural ownership，再调用自定义 parent callback、投影父级并发送通知，后续项冲突、callback 重入或同步观察者重入都不能留下部分写入或抢占 entry。
  - 将逻辑树父级与导航语义父级分离；分组对 `ParentNode`、`Level`、`IsTopLevel`、selection path、default path、Accordion 和 keyboard navigation 保持透明。
  - 定义无扁平列表分配的 semantic navigator，按已生成容器处理同层和 inline 可见树漫游，跳过分组、分隔线和不可交互项。
  - 将 generated container clear 定义为 selection 与 interaction 的统一失效出口，清除 realized selection、keyboard active、pointer press/release 目标和指向旧容器的 hover 延迟任务。
  - 明确模板重应用与容器回收的状态边界：`NavMenuItem.OnApplyTemplate` 只替换 template part、订阅和局部 motion 资源，不清理打开路径或选中路径；节点状态只由对应 coordinator 和真实 container clear 路径维护。
  - selection coordinator 区分已应用节点身份和临时 realized container 引用；容器回收只失效临时引用，视觉树 detach 保留已应用节点身份，后续选择按语义路径解析并清除当前旧容器，避免模板重建或重新挂载后出现多个 selected leaf。
  - generated node container 在 prepare 完成后统一从 selection coordinator 投影 `IsSelected` / `IsInSelectedPath`，使任意深度 collapsed popup 延迟生成、关闭重开或容器复用时恢复同一持久选择，不依赖 popup 打开事件或 dispatcher 刷新时机。
  - generated node container clear 完整复位 `IsSelected`、`IsInSelectedPath` 和 `IsSubMenuOpen`，避免 recycle 把旧节点视觉状态转移给新节点。
  - keyboard active 有效性同时检查 effective visible/effective enabled 与语义父链打开状态，避免 popup 关闭后提交仍被框架保留的 child container。
- Theme
  - 定义 Inline/Vertical 固定 Header/Footer 与中间滚动 entry 区，Horizontal 左 Header、右 Footer 和中间菜单区。
  - 定义 root inline collapsed 隐藏分组标题、popup 分组标题保持可见、Horizontal 根分组透明和 divider orientation 规则。
  - 明确 root inline collapsed 的透明分组后代继续继承折叠视觉并使用 `CollapsedIconSize` 居中；Footer 在折叠态退出布局，Header 保持可见以承载展开入口。
  - 根 ItemsPanel 通过 `TemplateBinding` 消费公开 `ItemSpacing`；后代 ItemsPanel 消费内部可继承的 `EntryItemSpacing`，既保持 Horizontal 根层默认 `0`，也使 popup、submenu 和 group 正确使用垂直 spacing Token；该路径不使用穿透子控件模板的 selector 或逐容器 binding。
- Token
  - 复用 `GroupTitleColor`、`DarkGroupTitleColor`、`GroupTitleLineHeight`、`GroupTitleFontSize` 表达非交互分组标题，不新增专属 divider token。
  - 明确 `CollapsedIconSize` 默认映射全局 `IconSizeLG`，相对普通 `ItemIconSize=IconSize` 使用大一档图标尺寸。
  - 明确 `VerticalItemsPanelSpacing=0` 保持默认 block margin 视觉；显式 `NavMenu.ItemSpacing` 是实例级额外 panel spacing，可覆盖根与后代默认 ItemsPanel，但不改写 Token。
  - 将 `MenuPopupMaxHeight` 默认值定义为八个标准菜单项高度；短菜单保持自然高度，长菜单在固定可见范围内滚动。
- Verification
  - 定义任意层级结构 entry、集合全动作、非法数据确定性失败、路径、选择、键盘、collapsed、Header/Footer、spacing、资源释放、容器回收隔离和纯节点快路径验证矩阵。
  - 覆盖 active container 移除后的 Enter、pressed container 移除后的 pointer release、pending open/close 目标回收，以及 popup 关闭后隐藏 child 不得被 Enter 提交。
  - 覆盖开启宽度 motion 后连续两轮 inline collapsed 折叠/展开，验证任意深度的 selected item、祖先 selected path 和 cached inline open path 在模板重应用后完整恢复。
  - 覆盖连续折叠/展开后切换 sibling、视觉树 detach 后程序化切换 sibling，以及 node container clear 的完整选择状态复位。
  - 覆盖与 Gallery 相同的三级 collapsed popup 路径，验证当前 leaf 在两轮 popup 关闭重开和容器重新生成后持续保持唯一 selected 状态。
  - 覆盖 node/group 同 owner 与跨 owner 重复拒绝、释放后重挂载、root source 重复、跨根迁移、弱 owner 回收和 divider 复用。
  - 覆盖离线和根菜单中的 custom wrapper 共享内置后代、嵌套 observable source 动态获取/释放 owner、动态重复、动态 owner/祖先环拒绝、custom source weak subscription 回收、批量初始化原子失败、parent callback 与集合通知同步重入，以及纯 built-in 深树不产生祖先重复订阅。
  - Menu Gallery 增加结构化 NavMenu 示例，覆盖固定 Header/Footer、根与嵌套分组、分隔线和显式 `ItemSpacing`，并由页面结构测试与 approved snapshot 保护。

## 2026-07-13

- API
  - 新增 `NavMenuNode` / `INavMenuNode` 的 `Command`、`CommandParameter` 节点命令契约，明确节点只承载配置，实际执行和 `CanExecute` 生命周期由 `NavMenuItem` 负责。
  - `INavMenuNode` 的新增命令成员提供 `null` 默认实现，保持既有自定义节点实现兼容。
  - 明确 `CommandParameter` 不隐式回退到 `ItemKey`，业务 key 由调用方显式绑定或赋值。
- Implementation
  - 将 `NavMenuNode` 资源宿主设计收敛到 `[GenerateScopedResourceHost]`，由 generator 生成 `IResourceHost` / `IThemeVariantHost` 和可释放 attachment token。
  - scoped resource-host token 增加 host generation 身份，避免 `A -> B -> A` 重入时旧 token 提前释放当前 owner。
  - 通过 container `CompositeDisposable` 将节点命令投影到当前 `NavMenuItem`，并在 container clear、recycle 和 source replacement 时同时释放命令 binding 与 resource-host attachment。
  - `NavMenuItem` 在 UI Dispatcher 上按周期合并 `CanExecuteChanged`，避免同步 `ReactiveCommand` 的瞬时 `false -> true` 让共享命令叶子触发 disabled 颜色闪动；pending operation 在 command / parameter 替换和 detach 时取消。
- Tests
  - 覆盖 pointer / keyboard 单次执行、`CanExecute`、同步 `ReactiveCommand` 瞬时状态合并、命令替换、container clear、ItemsSource replacement、custom node、owner resource 更新、repeated attach 和 WeakReference 回收。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `NavMenu`.
  - Align generated output paths with `controls/nav-menu/index-cn.md` and `controls/nav-menu/semantic-cn.md`.

## 2026-06-23

- Docs
  - 补充 NavMenu inline collapsed 设计模型，明确 public `Mode` 保持 `Inline`，内部通过 effective mode 切换到 vertical popup 交互。
  - 在 `overview.md` 和 `implementation.md` 中记录 inline open path cache、折叠期间 popup 临时状态、展开恢复和 selected path 保持规则。
  - 在 `overview.md` 中补充 NavMenu 键盘导航设计，明确 keyboard active/focus、open path 和 `SelectedItem` 三类状态分离。
  - 在 `implementation.md` 中补充 keyboard navigation coordinator 的职责边界、按键语义、active 视觉、popup 分支关闭和生命周期失效规则。
  - 明确 keyboard active 初始解析优先使用当前可见 `SelectedItem` 容器作为方向键移动锚点，无选中项或选中项不可见时回退到第一个可导航节点。
- API
  - 记录 `IsInlineCollapsed` 和 `InlineCollapsedWidth` 的 NavMenu 契约：折叠不改写 public `Mode`，本地 `InlineCollapsedWidth` 覆盖 token 默认宽度。
  - 新增 `IsInlineCollapsed` 和 `InlineCollapsedWidth` 公共属性，并在 Gallery API 表中登记。
  - 明确键盘 active 项不作为公共 API 暴露，业务侧仍通过 `SelectedItem`、`NavMenuItemClick` 和 `NavMenuNodeSelected` 获取已提交选择。
- Theme
  - 明确 inline collapsed header 视觉由 theme 表达：顶层 icon 使用 `CollapsedIconSize` 居中，标题和箭头收起，无 icon 顶层项显示标题首字符；root 宽度由控件内部 coercion 约束。
  - 落地 inline collapsed 视觉状态：root 宽度使用 `InlineCollapsedWidth` 对 `Width` / `MinWidth` 做 effective value coercion，顶层 header 通过 AXAML selector 切换折叠视觉，popup 子项保持普通 vertical 视觉。
  - 为 inline collapsed 的根宽度变化补充内部 `InlineCollapsedLayoutWidth` motion，避免 root `Width` coercion 绕过 `DoubleTransition` 后产生瞬间跳变，并避免使用 animation-priority relay binding 或 `MaxWidth` 夹宽度。
  - 明确 keyboard active 视觉使用 `ItemActiveBg` 语义，且优先级低于 selected，不复用 `IsSelected` 或 `IsInSelectedPath`；`IsInSelectedPath` 不应屏蔽 active 背景。
- Token
  - 记录 `NavMenuToken.InlineCollapsedWidth` 作为 inline collapsed 宽度默认值，初始设计值为 `48`。
  - 新增 `NavMenuToken.InlineCollapsedWidth`，并在 Gallery Token 表中登记。
  - 明确 `CollapsedWidth` 保留兼容，新的 inline collapsed 宽度语义优先使用 `InlineCollapsedWidth`。
- Implementation
  - 新增 internal `EffectiveMode` 和 inline collapsed open path cache，使 `Mode=Inline && IsInlineCollapsed=true` 复用 vertical popup 交互，同时保留 public `Mode`、`SelectedItem` 和 selected path。

## 2026-06-19

- Docs
  - 新增 `implementation.md`，记录 NavMenu 容器绑定、interaction handler、selection coordinator、默认路径 replay、popup 和背景块实现边界。
  - 将 `overview.md` 收敛为模式语义、公共契约、行为状态、视觉主题模型和验证入口。
  - 在 Navigation 分类入口中登记 NavMenu 实现原理文档。
  - 按控件文档规范补齐 NavMenu 桌面版架构设计、Token 设计和控件级 changelog。
  - 在 Navigation 分类入口中登记 NavMenu 文档。
- Theme
  - 记录 `IsItemBackgroundEnabled` 对 item 背景块和 inline submenu 背景块的控制边界。
  - 明确 root background、popup background、header background 和 inline submenu background 的职责分离。
- Token
  - 记录 NavMenuToken 的颜色、间距、popup、horizontal、dark 和 danger 分类。
  - 明确 `VerticalChildItemsMargin` 只服务背景模式下的 inline submenu 背景块外距。
