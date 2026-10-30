# Menu 桌面版实现原理

本文档描述 Menu 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Menu 桌面版架构设计](overview.md)，弹层滚动模式见 [Menu 弹层滚动模式设计](popup-scroll-design.md)，变化记录见 [Menu Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Menu Token 设计](token.md)。

Popup 接入边界：`Menu` / MenuFlyout 负责业务状态和内容准备，menu Flyout Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文档覆盖 Menu 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Controls.Shared/IScrollAware.cs`
- `src/AtomUI.Desktop.Controls/Flyouts/MenuFlyout.cs`
- `src/AtomUI.Desktop.Controls/Flyouts/MenuFlyoutPresenter.cs`
- `src/AtomUI.Desktop.Controls/Flyouts/Themes/MenuFlyoutPresenterTheme.axaml`
- `src/AtomUI.Desktop.Controls/Menu/ContextMenu.cs`
- `src/AtomUI.Desktop.Controls/Menu/ContextMenuReflectionExtensions.cs`
- `src/AtomUI.Desktop.Controls/Menu/Converters/ToggleItemsLayoutVisibleConverter.cs`
- `src/AtomUI.Desktop.Controls/Menu/DefaultMenuInteractionHandler.cs`
- `src/AtomUI.Desktop.Controls/Menu/Menu.cs`
- `src/AtomUI.Desktop.Controls/Menu/Menu.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuItem.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuItemGroup.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuSemanticLevel.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuPinnedOpenScope.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuItemData.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuItemPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuPopupScrollHost.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuSeparator.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuSeparatorData.cs`
- `src/AtomUI.Desktop.Controls/Menu/MenuToken.cs`
- `src/AtomUI.Desktop.Controls/Menu/Themes/ContextMenuTheme.axaml`
- `src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml`
- `src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.cs`
- `src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemGroupTheme.axaml`
- `src/AtomUI.Desktop.Controls/Menu/Themes/MenuSeparatorTheme.axaml`
- `src/AtomUI.Desktop.Controls/Menu/Themes/MenuTheme.axaml`
- `src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `ContextMenu`：维护上下文菜单 public surface、Popup 宿主和关闭生命周期，默认使用与 `Menu` 一致的 AtomUI 交互处理器。
- `DefaultMenuInteractionHandler`：Menu 家族 pointer、keyboard、focus 和 command 事件的交互 owner；统一维护子菜单 hover intent、延迟调度和 attach/detach 清理。
- `Menu`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuFlyout`：用户直接配置 flyout 菜单的入口，创建 `MenuFlyoutPresenter` 并中继 presenter 需要的 public 配置。
- `MenuFlyoutPresenter`：`MenuFlyout` 的菜单项 presenter，维护 item container、弹层内容高度和 Menu 家族交互处理器。
- `MenuItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `MenuItemData`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `MenuPopupScrollHost`：internal 模板宿主，根据 `IsScrollEnabled` 在 `ScrollViewer` 承载和直接内容承载之间切换。
- `MenuItemTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `MenuSeparator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuSeparatorData`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `MenuToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `ToggleItemsLayoutVisibleConverter`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- `ScrollAwareControlProperty` 是滚动开关共享 property owner；`Menu`、`ContextMenu`、`MenuItem`、`MenuFlyout` 和 `MenuFlyoutPresenter` 通过 `AddOwner` 接入同一语义。
- `DefaultMenuInteractionHandler` 是延迟打开和关闭意图的唯一 owner；`MenuItem` 只保存选择、打开和单项视觉状态，不保存 timer 生命周期。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Menu 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Items`、`MenuItem`、`MenuItemData` 和 `MenuSeparatorData`。
- 选择与集合：`DisplayPageSize`、`IsScrollEnabled`。
- 交互与状态：`IsMotionEnabled`、`ShouldUseOverlayPopup`。
- 视觉与布局：`LineWidth`、`Orientation`、`OverlayHostShadow`、`PopupRootShadow`、`SizeType`。
- 动效与异步：`CloseMotion`、`MotionDuration`、`OpenMotion`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- `IsScrollEnabled=true` 时，`DisplayPageSize` 参与 popup 最大高度计算并由 `ScrollViewer` 承载溢出内容；`IsScrollEnabled=false` 时，最大高度使用 `double.PositiveInfinity`，弹层内容直接显示全部菜单项。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

子菜单钉住状态按层级管理：`MenuItem` 和 `MenuFlyoutPresenter` 的 `MenuPinnedOpenScope` 只持有当前活动子项，
分组对同级互斥关系透明。导航到兄弟子菜单时，先解除旧分支的 pin 并关闭其物理 Popup，再钉住新分支；
不能把 pin 绑定给所有兄弟项。普通收起请求在 `IsSubMenuOpen` coercion 中被拒绝，避免先关闭后重新触发
`SubmenuOpened` 形成递归。生命周期关闭先解除当前分支的 pin，再关闭后代与物理 Popup。

语义层级属于当前 owner，而不是容器的历史属性。`SemanticLevel` 切换同步更新当前 marker 与已实现后代，
容器 prepare 为延迟实现后代下发相同层级；生成容器 clear 与显式容器 Parent 移除均释放旧作用域。
`None` 恢复复用方的 `.semantic-item`，并移除 plain Menu 的子菜单/分组 marker；应用自定义 class 保持不变。

子菜单 pointer 状态流：

```text
PointerEntered(MenuItem)
  -> 更新菜单导航选择
  -> 取消与当前目标冲突的旧 intent
  -> 必要时安排 PendingOpen(target)

PointerExited(MenuItem)
  -> 未打开且仍在 PendingOpen：取消打开，不创建同目标关闭任务
  -> 已打开且 pointer 不在 submenu popup：安排 PendingClose(target)

PointerEntered(submenu popup / descendant)
  -> 保持父项选择路径
  -> 取消该父项 PendingClose

Keyboard / access key / pointer press
  -> 取消冲突 hover intent
  -> 立即提交 open/close，不等待 hover delay
```

`SelectedItem` 继续服务菜单导航和祖先选择路径，`IsSubMenuOpen` 继续服务 Popup 状态；延迟 callback 的有效性由交互处理器持有的 owner、目标身份和 intent 代次共同判定，不能从这两个属性反向推断。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。
- `Menu`、`ContextMenu` 和 `MenuItem` 的 `IsScrollEnabled` 传播依赖 inheritable styled property，不需要为每个菜单项增加独立 C# binding。
- `MenuFlyout` 与 `MenuFlyoutPresenter` 生命周期不同，`MenuFlyout.IsScrollEnabled` 必须与其他 presenter 配置一起中继给 presenter，并由 presenter binding disposable 释放。
- `MenuPopupScrollHost` 不持有外部事件、timer、subscription 或缓存；模板重套用时由 Avalonia 模板生命周期释放旧分支。

hover intent 生命周期规则：

- 交互处理器 attach 到 `MenuBase` 时建立 owner 关系；同一处理器不能同时服务多个 owner。
- 内部调度入口统一返回 `IDisposable`。默认调度使用 `DispatcherTimer.RunOnce` 返回的取消句柄，不得丢弃句柄。
- 已公开的 `Action<Action, TimeSpan>` 延迟注入构造函数保持不变；内部适配层必须使用 disposable、one-shot 的原子 callback gate，并在 callback 执行、dispose 或 runner 抛出时原子地清空并释放原始 `Action`。外部 runner 即使仍保留并调用过期 wrapper，也不能保留原始 callback graph 或提交菜单状态。
- pending open 和 pending close 分别保存目标身份与释放句柄。新意图替换旧意图、callback 完成或 owner 结束时，都必须 dispose 并清空对应引用。
- `Closed`、窗口失活、Popup/Flyout 关闭、visual detach 和 handler detach 都属于 owner 结束或失效边界，必须统一取消 pending intent。
- callback 执行前必须重新验证 owner、目标身份、当前 intent 代次和弹层关系；验证失败时只结束当前 intent，不修改 `SelectedItem` 或 `IsSubMenuOpen`。

稳定 template part 接入点：

- `PART_ItemsPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_Popup`：承载弹层宿主、打开关闭或候选内容。
- `PART_ToggleCheckbox`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ToggleRadio`：稳定模板协作入口，重命名前必须同步主题和实现。

弹层滚动组合结构：

```text
ContextMenu
  -> Border#Frame
     -> MenuPopupScrollHost
        -> ScrollViewer when IsScrollEnabled=true
           -> ItemsPresenter#PART_ItemsPresenter
        -> ItemsPresenter#PART_ItemsPresenter when IsScrollEnabled=false

MenuItem / TopLevelMenuItem submenu
  -> Popup#PART_Popup
     -> Border#PopupFrame
        -> MenuPopupScrollHost
           -> ScrollViewer when IsScrollEnabled=true
              -> ItemsPresenter#PART_ItemsPresenter
           -> ItemsPresenter#PART_ItemsPresenter when IsScrollEnabled=false

MenuFlyoutPresenter
  -> ArrowDecoratedBox#PART_ArrowDecorator
     -> MenuPopupScrollHost
        -> ScrollViewer when IsScrollEnabled=true
           -> ItemsPresenter#PART_ItemsPresenter
        -> ItemsPresenter#PART_ItemsPresenter when IsScrollEnabled=false
```

## 6. 交互与事件处理

Menu 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 值提交或命令触发必须保持继承控件的事件顺序。
- pointer hover 只能更新交互处理器拥有的 intent；Popup 打开状态必须由通过验证的 intent 或显式 keyboard/click 操作提交。
- pointer 从父项移动到其 Popup 时必须保持祖先选择路径，不得通过清空 `SelectedItem` 取消正常的 submenu traversal。
- pointer 在 Popup 打开前快速离开父项时必须取消 pending open，不能依赖随后再执行 pending close 修正结果。

稳定事件路径包括 `IsCheckStateChanged`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- 弹层最大高度计算：`IsScrollEnabled=true` 时使用 `ItemHeight * DisplayPageSize + verticalPadding`，`IsScrollEnabled=false` 时使用 `double.PositiveInfinity`。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

菜单项图标容器必须保持以下不变量：

- `IconPresenter` 自身不测量图标，尺寸由主题 Setter 提供。菜单栏一级项与弹层菜单项都必须声明 `Width` / `Height`
  （取 `MenuTokenResource ItemIconSize`）与 `Margin`（取 `MenuTokenResource ItemMargin`）；只声明 `Margin` 会让图标
  量到 `0x0`：图标不可见但间距仍在，菜单项文字出现幽灵右移。
- 图标为空时靠 `IsVisible` 折叠整个 `IconPresenter`，不得用固定占位宽度对齐无图标项。
- 菜单栏顶层项按自身内容排布，图标列不参与跨项 `SharedSizeGroup`；否则无图标项会让出图标列宽。
- 弹层内「直接子项」与「分组内子项」的图标列由不同共享尺寸作用域决定：`MenuItem` 模板的图标列带
  `SharedSizeGroup="IconPresenter"`，作用域锚在 popup 的 `PART_ItemsPresenter`；`MenuItemGroup` 模板自带
  `ItemsPresenter`，其子项位于该作用域之外，图标列不会被同层有图标的兄弟撑开。因此弹层内同一层菜单项若在
  有/无图标之间混用，分组行的图标列会量到 `0`、文字左移，与直接子项文字左边缘不齐。示例与演示内容应保持
  同一层菜单项的图标使用一致；该行为是既有布局契约，不在本次语义改造范围内。

子菜单延迟算法必须保持以下不变量：

- 每类 intent 最多只有一个当前目标；同一目标不能同时处于 pending open 和 pending close。
- 进入新目标首先使冲突 intent 失效，再建立新 intent，callback 不负责猜测后续 pointer 状态。
- pointer 在打开延迟内离开时，子菜单始终保持关闭；不得出现“先打开再由关闭 timer 修正”的中间状态。
- 已打开子菜单允许在父项与其 Popup 之间移动，关闭延迟只保护这条 traversal 路径。
- 兄弟项切换时，旧项关闭与新项打开各自验证目标身份，过期 callback 不能关闭或打开当前项之外的菜单。
- menu close、detach 或 handler owner 变化后，任何历史 callback 都是无效 callback。
- `Menu`、`ContextMenu` 和默认 `MenuFlyoutPresenter` 使用同一套不变量；不能只修复其中一个入口。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。
- hover intent 调度只使用显式类型、委托和 `IDisposable` 生命周期，不依赖反射或运行时类型扫描。
- 滚动模式使用共享 styled property、AXAML 模板分支和 `TemplateBinding`，不引入运行时反射、字符串 binding、动态注册或生成器变更。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 默认滚动路径允许保留一个轻量 `MenuPopupScrollHost` 节点，以换取四套弹层模板的统一分支；禁用滚动路径必须移除 `ScrollViewer` 子树，但会让全部菜单项直接参与测量和显示。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。
- 默认调度路径在 intent 失效时停止实际 timer；兼容的外部 delay runner 至少必须通过原子 callback clearing/release 使 callback 失效，避免过期任务修改状态并保留 owner graph。
- 交互处理器只保留当前 pending open/close 目标，不维护随 pointer 移动增长的历史队列。

## 9. 维护不变量

维护 Menu 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- `DefaultMenuInteractionHandler` 的公开类型、构造函数和外部注入能力。
- `IsScrollEnabled` 默认值、继承传播、本地覆盖和 `MenuFlyout` 到 presenter 中继语义。
- 滚动禁用时不创建 `ScrollViewer`，滚动开启时 `DisplayPageSize` 继续限制弹层最大高度。
- 选择状态、Popup 状态与 hover intent 的职责分离。
- `MenuItem.SyncSubMenuPopupOpenState` 的非重入性：`Popup.IsOpen` 的打开 / 关闭结果会回写 `IsSubMenuOpen`，写入
  又触发同一同步方法。弹层无法保持打开时（放置目标跑出 `TopLevel` 可视矩形）该回写链曾无限递归直至栈溢出，因此同步
  必须由 `_isSyncingSubMenuPopupState` 守卫；移除守卫会让可视区外的子菜单展开直接崩溃。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- plain Menu 的语义层级只在其自身子树内下发；共享容器在 ContextMenu / MenuFlyout / DropdownButton 弹层中的 marker 行为不变。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

子菜单 hover intent 至少覆盖以下回归场景：

- pointer 进入带子菜单项后在打开延迟内离开，延迟完成后子菜单仍未打开。
- pointer 离开已打开父项后，在关闭延迟内进入父项或其 Popup，子菜单保持打开。
- pointer 从一个带子菜单项快速移动到兄弟项，只允许当前目标打开，旧目标 callback 不提交状态。

子菜单弹层开关状态同步至少覆盖以下回归场景：

- 放置目标位于可视区外时展开子菜单不触发状态同步递归（不得出现 `StackOverflowException`）。
- 放置目标位于可视区内时，`IsSubMenuOpen` 与 `Popup.IsOpen` 的打开 / 收起行为保持不变。
- pointer 从带子菜单项移动到叶子项，已打开兄弟子菜单按关闭延迟关闭，叶子项不产生打开 intent。
- menu close、Popup/Flyout 关闭、窗口失活和 visual detach 后执行历史 callback，不得重新打开或修改菜单项。
- 外部 delay runner 无法物理取消任务时，dispose 后执行 callback 仍不得提交状态。
- keyboard、access key 和 pointer press 的即时打开、选择首项、关闭与事件顺序保持不变。
- `Menu`、`ContextMenu`、默认 `MenuFlyoutPresenter` 和 detached title-bar menu 路径分别覆盖。
- `IsScrollEnabled` 默认值、继承传播、本地覆盖、`MenuFlyout` presenter 中继、`ScrollViewer` 有无和最大高度算法分别覆盖。

Semantic Part 至少覆盖以下回归场景（见 [Menu Semantic Part 契约](semantic-part.md)）：

- `Menu` descriptor 的 Part 名称逐字等于上游 12 个键路径，顺序、Selector、route、ContractType、cardinality 与契约一致；
  `MenuItem`、`MenuItemGroup`、`MenuSeparator`、`MenuPopupScrollHost` 不持有 descriptor。
- 层级 marker 互斥：菜单栏项与一级分组内的项只带 `.semantic-item`，子菜单容器只带 `.semantic-sub-menu-item`；一级分组带
  `.semantic-scope-group`，子菜单内分组带 `.semantic-sub-menu-group`；容器回收 / 复用 / re-template 后不残留另一层级。
- 复用方边界：未经 `Menu` 下发层级的 `MenuItemGroup` / `MenuItem`（ContextMenu、MenuFlyout、DropdownButton 弹层）保持既有
  `.semantic-item` marker，不因 plain Menu 层级隔离而丢失或改变。
- 模板 marker 存在性：`MenuItemTheme` 与 `TopLevelMenuItemTheme` 的 icon / content 双层级 marker、两个模板的
  `Border#PopupFrame` popup.root marker、`MenuItemGroupTheme` 的标题 / 列表双层级 marker 均在模板中静态声明。
- 声明式钉住时序：`IsPopupPinnedOpen="True"` 在容器生成前写入时，容器 prepare 阶段补齐请求，普通关闭请求不得收起弹层。
- 用户入口：`atom|Menu` owner-scoped 生成 Semantic Style 可编译并命中；默认主题不消费 `.semantic-*`。
