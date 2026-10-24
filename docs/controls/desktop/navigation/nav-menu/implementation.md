# NavMenu 桌面版实现原理

本文档描述 NavMenu 桌面版的 entry 集合、内部容器生成、语义导航树、交互 handler、选择协调、默认路径 replay、popup 接入和主题状态维护。公共设计与 API 契约见 [NavMenu 桌面版架构设计](overview.md)，项激活的提交契约见 [NavMenu 项激活事务设计](item-activation-design.md)，Semantic Part 契约见 [NavMenu Semantic Part 契约](semantic-part.md)，Token 语义见 [NavMenu Token 设计](token.md)，变化记录见 [NavMenu Changelog](changelog.md)。

Popup 接入边界：`NavMenu` / `NavMenuItem` 负责业务状态和内容准备，submenu Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

NavMenu 的实现目标是在 `ItemsControl` 容器体系内维护包含节点、分组和分隔线的有序 entry 树，同时保持节点导航语义独立，并按 mode 选择不同交互策略。实现文档聚焦 `NavMenu`、entry 数据模型、三类内部容器、语义 owner、命令投影、handler、selection coordinator、semantic navigator、inline collapsed coordinator 和 theme part 的协作关系。

路由切换、权限过滤、业务命令编排和页面生命周期不属于 NavMenu 实现范围。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs`：公开控件、属性、ItemsControl 容器入口、默认路径 replay 和 mode 状态同步。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs`：内部容器、header 转发、子菜单、popup、选中和打开状态。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuEntry.cs`：`INavMenuEntry` 最小标记契约和无状态 `NavMenuDivider` 数据模型。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuNode.cs`：`INavMenuNode` 契约、接口级协变 entry 读取入口、`NavMenuNode.Entries` 唯一可写集合和 `Children` 实时兼容视图；`NavMenuNode` 使用 `[GenerateScopedResourceHost]` 获得 scoped `IResourceHost` / `IThemeVariantHost` 生命周期。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuGroup.cs`：公开分组 entry 数据模型，管理透明 entry 子树和 scoped resource-host 生命周期。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuEntryCollection.cs`、`NavMenuEntryGraph.cs`、`NavMenuEntryOwnershipCoordinator.cs`：内置 entry owner 的事务前校验、循环检测、完整图唯一性协调、弱 structural owner 和嵌套 source 生命周期。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuGroupItem.cs`、`NavMenuDividerItem.cs`：内部不可交互结构容器。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuEntryContainerCoordinator.cs`：统一节点、分组和分隔线的容器类型分派、准备、清理和回收键。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuSemanticNavigator.cs`：按语义 owner 遍历已生成节点容器，跳过结构 entry。
- `src/AtomUI.Desktop.Controls/NavMenu/INavMenu.cs`、`INavMenuItem.cs`、`INavMenuElement.cs`：菜单和容器的内部/公共契约。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuSelectionCoordinator.cs`：选择状态和祖先路径状态同步。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuItemContainerBinder.cs`：节点数据与容器状态绑定。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuInteractionHandlerBase.cs`：交互 handler 基类，拥有激活事务状态、统一提交管线、释放合法性判定和取消路径。
- `src/AtomUI.Desktop.Controls/NavMenu/DefaultNavMenuInteractionHandler.cs`：Vertical / Horizontal popup 模式交互策略，父节点激活确保 popup 打开，独立拥有 hover 延迟打开 / 关闭流程。
- `src/AtomUI.Desktop.Controls/NavMenu/InlineNavMenuInteractionHandler.cs`：Inline 展开收起交互策略，父节点激活切换展开状态。
- `src/AtomUI.Desktop.Controls/NavMenu/Header/`：三种 header 控件。
- `src/AtomUI.Desktop.Controls/NavMenu/Themes/`：root、item、header 和 popup 主题。
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuToken.cs`：控件 Token。

## 3. 核心类职责

`NavMenu` 是树形导航根，负责 mode、theme、entry source、固定 Header/Footer、默认路径、受控选择、事件和子容器状态下发。

`NavMenuItem` 是内部容器，负责承载单个节点的 header、icon、子节点、popup 或 inline child items，并维护 `IsSelected`、`IsInSelectedPath`、`IsSubMenuOpen`、`Level`、`IsTopLevel` 等状态。它实现 `ICommandSource`，是节点命令的唯一执行者和 `CanExecuteChanged` 订阅 owner。

`NavMenuGroupItem` 是不可聚焦的内部 `ItemsControl`，承载分组标题和同一语义层级的 entry；`NavMenuDividerItem` 只承载分隔视觉。两者都不实现 `INavMenuItem`、`ISelectable` 或 `ICommandSource`，不会进入事件、路径或选择模型。

`NavMenuEntryContainerCoordinator` 是根 `NavMenu`、`NavMenuItem` 和 `NavMenuGroupItem` 的共享容器入口。它按 entry 类型选择容器和独立 recycle key，并统一注入 owner menu、local entry owner、semantic parent、level、mode 和 theme 上下文，避免三个 ItemsControl 各自复制容器逻辑。根 ItemsPanel 直接消费公开 `ItemSpacing`；后代 ItemsPanel 消费内部可继承的 `EntryItemSpacing`。主题默认把 `VerticalItemsPanelSpacing` 写入后代值，公开 `ItemSpacing` 有有效设置时只在根控件投影一次，不进入逐容器 relay binding。

`NavMenuNode` 是 owner-managed 非 Visual `AvaloniaObject`。它保存节点数据、`Command` 和 `CommandParameter`，但不持有 generated container，不执行命令，也不直接订阅 `ICommand.CanExecuteChanged`。scoped resource-host 样板由 generator 生成，owner 侧 attach token 与节点属性 relay binding 使用同一个容器 disposable 生命周期。

折叠 Tooltip 内容由 `NavMenuNode.Tooltip` 独立保存；`null` 表示回退到节点 `Header`，`IsTooltipEnabled` 表示节点级开关。节点不承载 `ToolTip.Tip` 附加属性，因为它不是视觉宿主；实际附加属性只存在于生成的 `VerticalNavMenuItemHeader`。

inline collapsed coordinator 由 `NavMenu` 拥有，负责根据 `Mode` 和 `IsInlineCollapsed` 计算 effective mode，缓存 inline 打开路径，关闭折叠期间的临时 popup，并把折叠视觉状态下发到 `NavMenuItem` 和 header。它不拥有选择状态，也不直接修改 `Mode`。

`NavMenuSelectionCoordinator` 统一处理旧选中节点清理、新选中节点设置、祖先路径标记和事件派发，避免选择逻辑散落在 click handler、默认路径 replay 和 property changed 分支中。它保存最后一次已应用选择的节点身份，并把当前 realized `NavMenuItem` 仅作为可失效缓存；容器回收后通过节点语义路径重新解析当前容器，不扫描或扁平化整棵 entry 树。每个 node container 完成 owner、node 和 semantic parent 准备后，都必须由 coordinator 投影当前 `IsSelected` / `IsInSelectedPath`，因此延迟打开的任意深度 popup 不依赖先前的 dispatcher 刷新时机。

interaction handler 按 mode 分工：Inline handler 处理视觉树内展开，Default handler 处理 popup 打开、延迟关闭、窗口失焦和同级互斥。激活事务由基类统一持有：待提交项与建立事务的指针身份描述一次事务，非空待提交项即代表事务存在；`CommitItemActivation` 是指针合法释放与键盘 Enter/Space 的公共提交入口，按激活目标分派为叶子选择提交（进入 `NavMenuSelectionCoordinator`）与父节点展开激活，最后统一执行命令与 `NavMenuItemClick` 派发。释放合法性以释放点对待提交项 Header 视觉子树的包含判定为唯一判据，不使用捕获期间的 `IsPointerOver`。键盘导航由 interaction handler 层统一接入，负责 active/focus 漫游、层级进入/返回、Enter 提交和 Esc 关闭当前 popup 分支，不能散落到各个 `NavMenuItem` 的局部 key handler 中。

keyboard navigation coordinator 只拥有临时 active/focus 状态，不拥有选择状态。它可以请求打开或关闭子菜单，但叶子节点提交必须进入 `NavMenuSelectionCoordinator`，以保持 click、默认路径 replay 和键盘提交使用同一个选择入口。

`NavMenuSemanticNavigator` 只读取已生成容器和显式语义 owner，不从逻辑祖先距离推导导航层级。它提供同层首项、末项、前项、后项和 inline 可见树前后项查找；分组透明、分隔线跳过，遍历过程不创建扁平节点列表。

Header 控件只承担显示和局部视觉状态，不拥有选择或打开逻辑。

## 4. 状态与数据流

状态流：

```text
NavMenu public API / NavMenuNode
  Mode / IsInlineCollapsed / InlineCollapsedWidth
  IsDarkStyle / IsItemBackgroundEnabled
  SelectedItem / DefaultSelectedPath / DefaultOpenPaths
  Header / Icon / ItemKey / Entries / Children view / IsEnabled
  Tooltip / IsTooltipEnabled
  NavMenuGroup / NavMenuDivider
  Command / CommandParameter
      ↓
Effective mode + inline collapsed open path cache
      ↓
Entry container generation
      ↓
NavMenuEntryContainerCoordinator
  dispatch NavMenuItem / NavMenuGroupItem / NavMenuDividerItem
  sync semantic owner and menu state
      ↓
NavMenuItem
  Level / IsTopLevel / HasSubMenu / IsSubMenuOpen
  NodeHeader / Tooltip / EffectiveCollapsedTooltip
  ICommandSource / effective command enabled state
      ↓
Interaction handler + KeyboardNavigationCoordinator + SelectionCoordinator
      ↓
Header theme / Popup frame / Inline child frame
```

`SelectedItem` 是持续选择状态。selection coordinator 中的已应用节点身份用于描述当前视觉投影，不替代 `SelectedItem` 公共状态；realized container 引用只在对应容器仍服务同一节点时有效。容器回收只失效临时引用，视觉树 detach 也不能清空已应用节点身份，因为同一控件及其选择视觉可以随后重新挂载。只有显式设置 `SelectedItem=null`、interaction handler 的显式 clear 或 public `Close()` 才能让公共选择和已应用投影共同复位。pointer 外点、窗口停用、平台失焦、非客户端点击、Esc、inline collapsed 过渡和 public `Mode` 切换只负责关闭 popup/submenu，不能借用 public `Close()` 清空选择。`DefaultSelectedPath` 和 `DefaultOpenPaths` 只在初始路径应用中参与 replay。程序连续设置多个选择时，过期 replay 必须被忽略，只应用最新 revision。

`IsInlineCollapsed` 是 `Inline` 模式附加状态。进入折叠时，当前 inline 打开路径写入 cache，主视觉树中的 inline 子菜单关闭，effective mode 切为 `Vertical`；退出折叠时，折叠期间打开的 popup 关闭，再从 cache 恢复 inline 打开路径。这个流程不能调用 `NavMenu.Close()`，不能改写 `SelectedItem`。

`NavMenuItem` 是折叠 Tooltip 有效状态的唯一 owner。节点 `Header` 单独投影为 `NodeHeader`，避免 generated container 的 `Header` 节点对象被当作文本内容。只有菜单和节点开关同时启用、`IsInlineCollapsed=true`、`IsTopLevel=true` 且 `HasSubMenu=false` 时，`EffectiveCollapsedTooltip` 才取 `Tooltip ?? NodeHeader`；其他状态统一为 `null`。模板通过 `TemplateBinding` 把有效内容、placement 和 delay 写到实际 `VerticalNavMenuItemHeader` 的 `ToolTip` 附加属性，继续复用共享 `ToolTipService`。

`InlineCollapsedWidth` 是布局输入。默认值由 `NavMenuTheme.axaml` 通过 `NavMenuToken.InlineCollapsedWidth` 提供；本地属性值覆盖 token 默认值。该属性只影响 `Mode=Inline && IsInlineCollapsed=true` 的根宽度和测量，不影响 `Vertical` / `Horizontal`。宽度约束由 `NavMenu` 内部通过 `Width` / `MinWidth` metadata coercion 表达：折叠时 effective `Width` 收敛到 `InlineCollapsedWidth`，较大的 effective `MinWidth` 向下收敛，展开后恢复原始 base value 或绑定。过渡动画不能挂在 root `Width` 的 `DoubleTransition` 上，因为 `IsInlineCollapsed` 切换时宽度来自 coercion，不是普通 styled value 变化；动画应由内部 `InlineCollapsedLayoutWidth` motion 按帧驱动 coercion。不要使用 `BindingPriority.Animation` relay binding 控制根宽度，也不要设置 `MaxWidth`，否则会破坏用户 base `Width` 或把收缩动画立即夹到目标宽度。

键盘 active/focus 是临时交互状态。active 项变化不能写入 `SelectedItem`，不能触发 `NavMenuNodeSelected`，不能改变 `IsInSelectedPath`。只有 Enter 在叶子节点上提交时，才进入 selection coordinator。激活事务的待提交项与 pointer-hold 视觉同样是临时交互状态，按下不写入任何选择状态，提交或取消时终结。

`IsItemBackgroundEnabled` 下发到 `NavMenuItem` 和 header theme，但它只控制 item / submenu 背景块，不关闭 header 文本状态。

导航层级状态由容器准备上下文直接赋值：根或根分组的节点使用 `Level=0`、`IsTopLevel=true`；`NavMenuItem` 的子 entry 使用 `Level=parent.Level+1`、`SemanticParentItem=parent`；分组的子 entry 继承分组当前的 semantic parent 和 child level。逻辑树只负责视觉和资源生命周期，不能再作为 `ParentNode`、`Level`、`IsTopLevel` 或键盘同层判断的事实来源。

## 5. 生命周期与模板接入

`NavMenu` 在 mode 或 inline collapsed 状态变化时同步 root pseudo-class、ItemsPanel 方向、interaction handler、effective mode 和已打开子菜单状态。切换 public `Mode` 必须通过保留选择的递归关闭流程结束旧模式下的 popup 或 inline 子菜单，避免旧 handler 的 pointer、delay、popup 或 motion 状态泄漏，同时保持 `SelectedItem` 和重新生成容器上的 selected path。切换 `IsInlineCollapsed` 同样不能走 public `Close()` 路径，而要走 inline collapsed coordinator 的 cache / restore 流程。

Default interaction handler 将 pointer 外点、窗口停用、平台失焦和非客户端点击统一映射为关闭已打开的顶层菜单项。该路径递归关闭对应 popup 分支，但不调用 `NavMenu.Close()`；public `Close()` 保留显式“关闭全部子菜单并清空选择”语义，不能作为 popup 生命周期的通用内部入口。

`IsInlineCollapsed` 变化时的生命周期顺序必须固定：

```text
collapse:
  collect open inline paths
  close inline child frames without clearing SelectedItem
  switch effective mode to Vertical
  reattach interaction handler if needed
  sync collapsed visual state

expand:
  close transient popup branches
  switch effective mode to Inline
  reattach interaction handler if needed
  replay cached inline paths
  sync expanded visual state
```

`NavMenuItem.OnApplyTemplate` 获取 header、popup、popup frame、inline motion actor、child frame、items presenter 和 active indicator。模板替换时必须解除旧 part 事件订阅，并重新绑定 handler 需要的 part。模板重应用只负责 template part、事件订阅和局部 motion 资源的替换，不能重置 `IsSubMenuOpen`、`IsInSelectedPath` 或 `IsSelected`；这些状态分别由 open-path coordinator、selection coordinator 和明确的容器清理路径维护。`NavMenuEntryContainerCoordinator.PrepareContainer` 在 node binding 和 semantic context 就绪后统一调用 selection coordinator，把当前已应用节点身份投影到新容器。只有 `ClearContainer` 确认容器退出当前数据项并进入回收流程时，才清除 `IsSubMenuOpen`、`IsInSelectedPath`、`IsSelected` 和 owner context，并通知 selection / interaction coordinator 忘记对该 realized container 的引用，避免 re-template 破坏持续状态或 recycle 把旧节点状态转移给新节点。

Root template 把 Header、菜单 entry 区和 Footer 组织为三个稳定区域。Inline/Vertical 的 Header/Footer 位于 ScrollViewer 外部，中间 entry 区独立滚动；Horizontal 使用左侧 Header、中间 entry 区和右侧 Footer。Header/Footer content 为空时 presenter 使用 `IsVisible=false` 退化，不改变无 slot 的测量结果。有效 inline collapsed 状态下 Header presenter 保持存在以承载展开入口，Footer presenter 退出布局；展开后同一 Footer content 恢复，不重建或替换用户内容。

默认路径 replay 依赖容器生成和模板应用。实现必须使用有界 replay，不使用固定 sleep 或 timer 作为容器可用性的长期机制。

节点进入容器时必须建立一条完整的 scoped 生命周期：

```text
PrepareContainerForItemOverride
  -> reset current container CompositeDisposable
  -> retain node identity and bind owner menu policy
  -> register root menu visual attach/detach in the container CompositeDisposable
  -> while the root menu is attached, create an entry binding scope
     -> NavMenuNode.AttachResourceHost(owner)
     -> bind HeaderTemplate / Tooltip / IsTooltipEnabled / ItemKey and other node properties
     -> bind Command / CommandParameter to NavMenuItem

Root menu visual detach / reattach
  -> detach: dispose entry bindings and resource-host token
  -> reattach: create a new scope and project current node values

ClearContainerForItemOverride / rebind / recycle
  -> forget the generated container in selection and interaction coordinators
  -> invalidate keyboard-active, pressed and delayed hover targets
  -> dispose the same CompositeDisposable
  -> release node-to-container bindings
  -> release resource-host attachment token
  -> clear NodeHeader / Tooltip / effective collapsed Tooltip inputs
  -> clear command value from the old NavMenuItem
  -> NavMenuItem unsubscribes old Command.CanExecuteChanged
```

`BindEntryLifetime` 在开始发布资源和节点属性前先登记活动 `CompositeDisposable`。资源更新或命令初始 `CanExecute` 可以同步触发节点删除、容器回收或重新挂载；旧作用域一旦释放，后续 binding 不得继续建立，也不能写回旧节点身份。作用域绑定到根菜单视觉生命周期，因此普通 popup 关闭不释放仍在菜单中的节点资源。根菜单 detach 必须同时释放资源宿主和节点到容器的属性订阅，仅释放资源宿主仍会留下数据对象到容器的强引用。

资源宿主 attachment、节点属性 binding 和命令 binding 不能分散到不同的无 owner subscription 中。re-template、Items reset、container recycle 或节点替换必须复用同一个 clear 路径；不允许依赖 GC、DataContext 清空或页面导航释放旧关系。

分组进入容器时使用同一 acquire/release 结构：prepare 时设置 Entries ItemsSource 和语义 owner，根菜单处于挂载状态时 attach `NavMenuGroup` resource host 并绑定 Header/HeaderTemplate；根菜单 detach 释放该活动作用域，reattach 恢复当前标题、模板和资源；clear/rebind/recycle 时先 dispose 容器 binding owner，再清除 ItemsSource、owner property local value 和语义上下文。分隔线虽然没有数据对象属性，但 mode/dark owner relay binding 仍由独立 disposable 持有；clear 时先释放 binding，再清除 mode、orientation 和 owner 上下文，防止回收后继承旧层级状态。

## 6. 交互与事件处理

激活事务契约见 [NavMenu 项激活事务设计](item-activation-design.md)。pointer 路径：

- 按下只建立事务：确认主按钮与节点有效、取消旧事务、记录待提交项与指针身份，先置与 selected 背景一致且不覆盖文字颜色的 pointer-hold 视觉，再捕获带 `Cursor=Hand` 的 item header，最后按 mode 尝试移动真实焦点。该顺序避免捕获改变原生 `:pointerover` 时暴露默认背景，并保证按住期间保持手型指针。它不改写 keyboard-active owner 或 selected 状态，不进入 selection coordinator、不执行命令、不触发路由事件、不切换 inline 展开状态。
- 按住阶段移动处理只维护 pointer-hold 视觉：拖出待提交项清除，移回恢复；不更新 `SelectedItem`。
- 合法释放（同一指针、主按钮、释放点命中待提交项 Header 视觉子树、节点与全部语义祖先仍可用、未被卸载或遗忘）进入 `CommitItemActivation`；先清空事务字段并解除 header 的 capture-lost 订阅、释放捕获，但保留 pointer-hold selected-background 视觉；selection 提交与用户回调完成后才在 `finally` 清除 pointer-hold，避免浅蓝背景在释放瞬间闪回 hover，同时防止命令或导航重入残留旧事务。
- 取消路径统一无副作用：释放到另一节点或菜单外、当前捕获指针丢失、按下后节点被移除或禁用、菜单 detach 或 handler 替换、非主按钮释放、新按下替代旧事务；其他指针的 capture-lost 事件不得终止当前事务。捕获已转移到另一控件时只清理旧事务，不得释放新控件的捕获。Inline 父项展开后，子项位于父容器的视觉子树内，但不属于父 Header 的命中范围；按下父 Header、在子 Header 释放必须取消父项激活。
- pointer-hold 与 keyboard-active 由独立 owner 持有并分别投影：前者通过 header 的 `IsPointerHold` 只使用 `ItemSelectedBg` / `DarkItemSelectedBg`，不覆盖文字颜色；后者通过 `IsKeyboardActive` 使用 `ItemActiveBg`。清除指针事务不得清除键盘导航锚点；pointer-hold 不依赖捕获期间的原生 `:pointerover`，也不写入 keyboard active、`IsSelected` 或 `IsInSelectedPath`。
- header 背景 transition 使用全局 `MotionDurationSlow`（默认 300ms）和 NavMenu Own Token `ItemBackgroundMotionEasing`（默认 `Spline(0.25,0.1,0.25,1)`），对应参考 Menu 的 `background-color 0.3s ease`；Base、Inline、Horizontal 主题只通过强类型 `NavMenuTokenResource` 消费该曲线，不在 AXAML 中重复构造 easing。mouse down 后背景从 hover 灰进入 selected 浅蓝，但合法释放才提交 selection。

Inline handler：

- 父节点合法释放时切换 `IsSubMenuOpen`；按下阶段不切换。
- 叶子节点合法释放进入 selection coordinator。
- `IsAccordionMode=true` 时，根 `NavMenu` 在顶层节点请求打开时同步关闭其他顶层节点；互斥协调不等待 `SubmenuOpened` 动画完成事件，因此旧分支收起与新分支展开并行进行。
- Inline 收放由 `NavMenuItem` 为既有 child actor 持有的 Core 内部 `ContentExpansionAnimator` 统一执行，通过原生 `Animation` 驱动内部进度和透明度。展开目标取当前宽度下的自然测量高度，收起目标为零；内容保持正常文字尺寸，由 actor 裁剪溢出区域。`VerticalChildItemsMargin` 固定应用于内部 `PART_ChildItemsFrame`，随完整内容高度一起裁剪，不作为 actor 外部的独立动画值，避免首尾间距恢复引发文字回弹。
- 收放共用 `MotionDurationMid` 和 `Spline(0.645,0.045,0.355,1)` 缓动。新的请求从当前呈现的高度和透明度接续，取消旧动画，不忽略动画期间的点击；只有当前执行可以提交稳定状态，菜单层同时校验排队请求身份、当前 animator 和目标状态后发布完成事件。
- 启动动画前显式保持当前高度和透明度，覆盖第一个时钟 tick 之前的绘制窗口；使用 `FillMode.Forward` 保持最终帧，覆盖时钟完成到异步清理之间的绘制窗口。动画完成后解除内部进度与布局接入，恢复自然布局；关闭 motion、template 重应用和 visual detach 时取消动画并恢复请求状态。每次动画的取消令牌在完成或取消后释放，已失效操作不得继续写入旧 actor。
- 当 `IsInlineCollapsed=true` 时不使用 Inline handler；有效交互切换到 Default handler，使带子菜单的顶层项通过 popup 打开。

Default handler：

- pointer enter 可延迟打开 popup。
- pointer leave 可延迟关闭 popup。
- 延迟打开 / 关闭任务记录各自的目标容器；目标 container clear、handler detach 或新 pointer 状态替换旧任务时精确取消并清空目标引用。hover 延迟流程独立于激活事务。
- 父节点合法释放时确保 popup 打开（已打开则保持）。
- 叶子节点合法释放进入 selection coordinator。
- popup close 由 pointer、窗口失焦、非客户端点击和同级打开状态共同控制。
- Horizontal 顶层 popup 放置在下方，非顶层和 vertical popup 使用侧向层级。
- inline collapsed 使用 Default handler 的 popup 路径，但仍保留 public `Mode=Inline`，以保持 API 语义和文档语义一致。

Keyboard navigation：

- NavMenu 在根菜单范围接收方向键、Enter 和 Esc，并根据当前 active/focus 项分派给当前 mode 的导航策略。
- active 项优先来自当前键盘焦点所在的 `NavMenuItem`；没有可用 focus 时，先解析当前 `SelectedItem` 对应的可见容器作为方向键移动锚点，并把本次方向键 delta 应用到该锚点上；找不到选中容器时再使用当前可见层级中的第一个可交互项。可交互项必须 effective visible、effective enabled，并且从自身到根的所有语义父项仍处于打开状态，不能提交已关闭 popup 中残留的容器。
- Up / Down 在当前语义可见层级内循环移动，跳过分组、分隔线、禁用项和不可交互项。
- Horizontal 根层使用 Left / Right 在顶层项之间循环移动，Down 或 Enter 打开 active 子菜单并进入第一项。
- Vertical 根层和 popup 子菜单使用 Right 或 Enter 进入子菜单，Left 或 Esc 返回父级并关闭当前 popup 分支。
- Inline 模式使用 Up / Down 遍历当前展开后的可见树；Left / Right 只折叠或展开当前 active 子菜单并保持 active 项不变，叶子项上为 no-op；Enter 在父节点上切换展开，在叶子节点上提交选择。
- Inline collapsed 模式使用 effective vertical 键盘策略，根层只遍历顶层项，打开子菜单后进入 popup 子级。
- 键盘打开 popup 后必须确保子容器可生成，并把 active/focus 移动到第一个可交互子项；不能依赖固定 timer 等待 popup content。
- Esc 只关闭当前 popup 分支，active/focus 回到父项；不能调用 `NavMenu.Close()`，避免清空 `SelectedItem`。

`NavMenuItemClick` 表达 item 激活，`NavMenuNodeSelected` 表达叶子节点选择提交。禁用项不得触发有效激活、命令或选择。叶子提交顺序固定：selection coordinator 更新选中路径与 `IsSelected` / `IsInSelectedPath`、更新 `NavMenu.SelectedItem`、触发 `NavMenuNodeSelected`、执行节点 `Command`、触发 `NavMenuItemClick`；父节点提交不修改 `SelectedItem`。`SelectedItem` 更新后和 `NavMenuNodeSelected` 返回后都检查该节点是否仍是当前选择；同步重入改写选择时，原提交立即标记为 superseded，不发布陈旧选择事件，也不继续执行原节点命令或 `NavMenuItemClick`。程序化 `SelectedItem` 只进入 selection coordinator，不进入激活或命令路径。

## 7. 内部算法与关键流程

### 7.1 容器绑定

`NeedsContainerOverride`、`CreateContainerForItemOverride`、`PrepareContainerForItemOverride` 和 `ClearContainerForItemOverride` 由三个 entry owner 委托给统一 coordinator。节点、分组、分隔线必须使用三个稳定且不同的 recycle key；own-container 项使用 `null`，不能把不同生成容器放入 Avalonia 的同一回收池。

节点数据到容器的绑定必须包括 Header、HeaderTemplate、Icon、ItemKey、IsEnabled、Command、CommandParameter、Entries、owner menu、local entry owner、semantic parent、level、mode、dark style、background mode 和 motion 状态。根菜单直接拥有的节点从 NavMenu 接收 effective inline-collapsed 状态；根层透明分组拥有的节点从该分组继续接收同一状态，任意数量的根层透明嵌套分组都不得截断状态；节点或非根分组中的后代固定使用非折叠 popup/inline 子级视觉。容器解绑时必须释放资源宿主关系、relay binding、语义 owner 和命令事件订阅。

`NavMenuNode` 作为非 Visual Avalonia binding target 时使用 `[GenerateScopedResourceHost]`。container binder 先把 generated `AttachResourceHost(owner)` token 加入当前 `NavMenuItem` 的 `CompositeDisposable`，再通过 AvaloniaProperty overload 建立节点属性投影。自定义 `INavMenuNode` 使用强类型 getter overload；实现了 `INotifyPropertyChanged` 的节点保持运行期更新，普通节点取得初始值，未声明命令的既有实现使用接口的 `null` 默认值。所有 `BindUtils.RelayBind` 返回值必须加入同一个 disposable；禁止丢弃返回值或把最后一次容器永久挂回节点。

owner menu、parent item 或 parent group 到生成容器的运行期状态关系无法由固定 ControlTemplate 的 `TemplateBinding` 表达，因此 coordinator 使用 `BindUtils.RelayBind`。这些 binding 的 owner 分别是 node、group、divider 容器中的 `CompositeDisposable`；clear 路径必须先 dispose 订阅，再对 target property 执行 `ClearValue` 和默认状态复位，避免 DirectProperty binding 被丢弃后继续持有已回收容器。

### 7.2 Entry 集合与父级投影

`NavMenuNode.Entries` 是唯一有序存储，`Children` 是延迟创建的实时节点视图。纯节点 entry 的枚举和变更走直接快路径；遇到 `NavMenuGroup` 时只在该语义层级内递归穿透分组，不进入子节点后代。

`INavMenuNode.Entries` 的接口类型是 `IEnumerable<INavMenuEntry>`。默认实现直接返回继承自 `ITreeNode<INavMenuNode>` 的 `IEnumerable<INavMenuNode> Children`，依赖 `IEnumerable<T>` 协变完成类型投影，不创建 wrapper、缓存或第二份集合。`NavMenuNode` 公开可写的 `IList<INavMenuEntry> Entries`，并以显式接口实现把同一实例暴露为读取入口；默认 TreeDataTemplate 统一绑定接口属性。内置实时集合同时实现泛型与非泛型 `IList`，满足 Avalonia 12 对可通知 ItemsSource 的集合契约。自定义节点若只保留既有 `Children`，其集合通知和节点更新语义保持不变；只有需要混合结构 entry 的自定义节点才覆盖接口 `Entries`。

所有 entry owner 共用一个集合验证职责。根 `NavMenu` 同时监听 direct `Items` 和 `ItemsSource` 投影后的 `ItemsView`；`NavMenuNode` 与 `NavMenuGroup` 在各自 `Entries` 集合入口执行同一类型边界。每个数据项必须是 `INavMenuNode`、`NavMenuGroup` 或 `NavMenuDivider`；仅实现 `INavMenuEntry` 的未知种类也在该入口拒绝，不能等到 container coordinator 的类型分派才失败。异常信息包含 entry owner、索引和实际类型。

内置 `NavMenuNode` / `NavMenuGroup` 另外维护唯一 structural owner。该 owner 与 `ParentNode` / `SemanticParentNode` 不同：前者描述 entry 实例实际存放在哪个根、节点或分组集合，后者描述分组透明之后的导航父级。structural owner 必须使用弱引用，不能让外部持有的数据对象反向保留根 `NavMenu` 或原父 entry。`NavMenuEntryCollection` 在单项 Add、Insert、Replace mutation 前先构造 prospective tree，完成 candidate 类型、循环、现有 owner 和完整图重复校验，再执行集合写入；批量初始化先物化并预检完整批次，避免后续冲突时保留前面已加入的项。成功 mutation 后必须先统一同步 structural owner，再执行可能进入用户实现的 `INavMenuNode.UpdateParentNode` callback 或分组语义父级投影，最后发送属性与集合通知；callback 不能在 ownership 提交窗口内把本次 entry 或其内置后代重挂到其他 owner。Remove、Replace、Clear 在集合移除后释放已经离开当前 scope 的 owner。失败路径不得留下集合项、父级投影或 owner 残留。无状态 `NavMenuDivider` 不参与 owner 跟踪。

既有自定义 `INavMenuNode` 实例自身不登记 structural owner，避免改变旧数据模型契约；但其 entry enumerable 中的内置状态 entry 不能绕过唯一性。每个 `NavMenuEntryCollection` 与根 `NavMenu` 都持有同一种 `NavMenuEntryOwnershipCoordinator`。协调器从自己的 entry 根遍历当前 structural scope：直接 entry 参加校验；custom node 继续递归且后代继承当前 scope owner；内置 node/group 是 scope 边界，其内部集合由该 entry 自己的协调器负责。因此离线 built-in owner、已挂入根菜单的 built-in owner 和根级 custom source 使用同一规则，不依赖根菜单出现后才补做校验。

协调器用引用相等集合检测同 owner、跨 owner、跨 custom wrapper 和跨根菜单的重复 occurrence，并使用 Avalonia weak collection subscription 监听当前 scope 内所有可通知的 custom entry source。built-in child collection 不由祖先协调器订阅，避免深树形成 O(N²) 订阅和重复全子树扫描；其 mutation 由 child 自身协调器在本地处理，structural owner 冲突仍提供跨 scope 的全局唯一性边界。custom source 动态加入内置后代时，协调器复用 entry graph 插入校验，拒绝 candidate 指回当前 built-in scope owner 或其任意 built-in 祖先；通过校验后最近 scope owner 才取得 ownership。后代移除或 source 离开当前 scope 时释放 ownership 和订阅。weak structural owner 与 weak collection subscription 都不能反向强持有根菜单或离线 built-in owner。

根 `NavMenu` 的 direct `Items` 和外部 `ItemsSource` 都通过 `ItemsView.CollectionChanged` 到达控件。由于该通知是 post-mutation，根协调器每次根据当前 root scope 构建引用相等 occurrence/owner 映射，统一检测非法类型、循环、同一状态 entry 的重复 occurrence 和已被其他 structural owner 占用的实例，然后对保留、移除和新增的 root scope owner 及 custom source 弱订阅做 reconcile。校验失败时必须确定性抛出，但不得在通知回调中重入修改调用方 collection 来伪造事务回滚。已从当前 scope 消失的旧 root owner 即使本次新图非法也要释放，避免一次失败的 source replacement 永久锁住旧 entry。内置 `Entries` 集合则在 mutation 前完成同一 scope 校验；`AddRange` 一次写入完整批次，先同步整批 ownership，再投影父级并发送单次批量 Add 通知，保持批量失败原子性并阻止 parent callback 或集合观察者抢占未提交 entry。

根验证不能以 `Items.IsReadOnly` 区分 direct `Items` 和 `ItemsSource`。该状态只描述集合写入口，不代表 source 数据已经满足类型契约；否则 direct Items 会被检查，而绑定 ItemsSource 的初始项、Replace 或 Reset 会绕过校验。

Replace 在调用旧 entry detach 前先提交新成员及结构所有权。其事务覆盖父关系投影和集合通知，并在 `finally` 解除同一集合的重入写保护；回调异常不能让已经提交的替换停在未通知状态。分组递归投影逐项完成后传播异常，保证一个自定义节点的回调失败不会阻止其他后代更新父级。

节点模板 binding 同时观察节点 `HeaderTemplate` 和 owner `ItemTemplate`，以 `nodeTemplate ?? ownerTemplate` 维持当前回退。owner 模板变化可能触发 ItemsControl 重建容器，新容器沿同一准备路径建立绑定。`ItemKey` 使用强类型 getter relay 持续投影，并在 scope 释放时解除订阅。

兼容视图写操作遵守以下映射：

- `Add` 把节点追加到 owner 的直接 `Entries`。
- `Insert` 在目标语义节点当前所在 entry owner 中插入。
- indexer replace、`Remove` 和 `RemoveAt` 写回目标实际 owner。
- `Clear` 清空 owner 的整个 `Entries`，包括结构 entry。
- 直接纯节点变化转发精确集合事件；分组内部结构变化可以向兼容视图发出 `Reset`，但不能建立第二份缓存集合。

entry 集合负责在 Add、Remove、Replace、Move、Reset 时维护最近节点父级。节点加入 `NavMenuNode` 时 `ParentNode` 指向该节点；加入一个或多个嵌套分组时继续使用分组外最近的 `NavMenuNode`；根分组保持 `ParentNode=null`。分组移动不改变其后代的相对节点父级，分组循环引用必须在集合入口拒绝。

### 7.3 命令投影流程

```text
NavMenuNode.Command / CommandParameter changed
      ↓ container-scoped relay binding
NavMenuItem.Command / CommandParameter
      ↓ ICommandSource
CanExecute determines effective enabled state
      ↓ effective click / keyboard commit
Execute once
```

`NavMenuItem` 在 command property 变化时先解除旧命令的 `CanExecuteChanged`，再订阅新命令；logical-tree detach 时解除当前命令订阅。container disposable 释放 command relay binding 后，旧命令不能继续持有已回收容器。`CanExecute=false` 只影响容器 effective enabled state，不写回 `NavMenuNode.IsEnabled`。

`NotifyClicked` 仅在路由事件 `Source` 是当前容器时执行命令，子项 Click 冒泡只保留事件路由。Inline 和 popup 打开流程通过 `NavMenuSemanticNavigator.EnumerateDirectItems` 刷新已实现语义子项的 `CanExecute`，透明分组中的节点也参与刷新。

`CommandParameter` 不做 `ItemKey` fallback。自动 fallback 会让显式 `null` 失去语义，并在 `ItemKey`、参数 binding 和容器复用之间引入第二套同步状态。业务需要 key 时由调用方显式绑定或赋值。

### 7.4 选择流程

```text
Commit leaf item
      ↓
NavMenuSelectionCoordinator
      ↓
old selected container IsSelected=false
old ancestor IsInSelectedPath=false
new selected container IsSelected=true
new ancestors IsInSelectedPath=true
      ↓
NavMenu.SelectedItem update
      ↓
确认 SelectedItem 仍是目标节点
      ↓
NavMenuNodeSelected
      ↓
再次确认 SelectedItem 仍是目标节点
      ↓
Command → NavMenuItemClick
```

祖先路径只标记导航路径，不应通过 ancestor pointer state 让父级 header 进入 hover 背景。协调器保存已应用路径的容器集合，清理时不依赖可能已被节点移除操作清空的 `ParentNode`。每次转换拥有独立的当前投影集合；属性通知前更新投影，嵌套选择接管后旧转换立即退出，避免集合被回收回调修改或旧清理覆盖新选择。

选择祖先从 `SemanticParentItem` 迭代，不使用 `GetLogicalParent<NavMenuItem>()`。节点位于任意层级分组内时，分组容器不会出现在 selected path；顶层分组中的节点仍由 root selection owner 直接选择。

选择协调器必须区分节点身份和容器身份。最后一次已应用的节点身份跨 template reapply、inline collapsed 切换和视觉树 detach 保留；realized container 被回收时只清除容器引用。新容器 prepare 时根据已应用节点及其 parent path 精确设置自身 selected/path 状态；如果程序化 `SelectedItem` 正在等待 revision replay，prepare 继续投影旧的已应用节点，不能提前覆盖 coordinator 状态。下一次选择若没有可用缓存，仅在旧节点仍属于当前菜单时沿其 semantic parent path 解析旧选中容器；旧祖先状态通过已应用路径集合清除。容器回收同时从该集合移除，避免保留失效容器。查找工作只覆盖路径深度和各层透明分组的结构查找，不能为一次选择构造全树快照。

键盘提交必须复用同一激活流程。active 项不是选择项，方向键移动不进入 selection coordinator。Enter 与指针合法释放进入同一提交入口：先由 selection coordinator 更新选中路径与 `SelectedItem` 并触发 `NavMenuNodeSelected`，确认提交未被同步重入替换后，再执行节点命令并触发 `NavMenuItemClick`。程序化 `SelectedItem` 只复用前半段的选择协调流程。

### 7.5 默认路径 replay

`TreeNodePath` 通过 `ItemKey` 定位节点路径。路径 replay 先打开中间节点，再选中叶子节点。由于容器生成依赖 layout 和 ItemsPresenter，replay 可以在 loaded priority 下有界重试。

`SelectedItem` replay 通过 selection revision 丢弃过期请求。默认选中路径排队时捕获该 revision，并在每次重试及最终提交前同时检查 revision 未变且 `SelectedItem` 仍为空；排队期间发生显式选择后，默认请求立即失效。

路径查找先使用当前 entry owner 的 `ContainerFromItem` 直接快路径；未命中时只递归检查已生成的 `NavMenuGroupItem`。`TreeNodePath` segment 只匹配 `INavMenuNode.ItemKey`，结构 entry 永远不消费 segment。根节点合法性检查同样穿透根分组，但不能穿过另一个节点。

### 7.6 背景块模型

`NavMenuItem` 背景和 `NavMenuItemHeader` 背景是两层不同职责：

- item / child frame 背景表达 inline 子菜单背景块和连续层级背景。
- header 背景表达 hover、selected、disabled 等交互视觉。

`IsItemBackgroundEnabled=false` 只关闭 item / child frame 背景块和背景块专用外距。header 前景、hover、selected、selected path 和 disabled 仍由 header theme 处理。

### 7.7 Popup 模型

Popup shell 位于 `NavMenuItem` 模板内，popup content 由 `ItemsPresenter` 承载。打开 popup 前后必须确保子容器可生成，默认路径 replay
不能依赖固定等待时间。

钉住打开（`NavMenu.IsPopupPinnedOpen`）的请求状态由 `NavMenuPinnedOpenCoordinator` 持有，并以 `INavMenuNode` 为键而不是以
运行时生成的 `NavMenuItem` 容器为键。属性变更、attach、Mode 切换都发生在容器生成之前或容器重建之际（声明式用法如 Gallery 语义
预览在 AXAML 中写 `IsPopupPinnedOpen="True"`，会在模板应用、容器生成之前就写入属性；Mode 切换会让 `ItemsPresenter` 回收并重建
全部容器），把请求表达成节点路径后，它可以独立于容器生命周期存活，任何触发点只更新请求，容器实现后再由同一份请求收敛补齐。

协调器暴露五个入口：`Reconcile` 在属性变更、attach、Mode 切换时立即求值；`PrepareContainer` 是容器 prepare 阶段的钉住不变量，
与选择状态走同一容器时序契约，容器实现时按请求补齐；`QueueReconcile` 供条目集合变化使用，`ContainerFromItem` 在集合变更回调内可能
仍返回尚未重新绑定的旧容器，因此合并调度到 Loaded 优先级、在 `ItemsPresenter` 收敛后再求值，generation 保证旧调度不复活；
`ExtendToOpenedItem` 在用户或 API 打开子菜单后把钉住路径扩展为当前展开链（只接受真实子菜单的展开事件，没有子项的 `NavMenuItem`
也会派发 `SubmenuOpened`）；`ForgetContainer` 在容器回收时摘掉钉住记录，仅当容器节点已离开菜单条目图时才做生命周期关闭。
`IsPopupPinnedOpen=false` 只解除关闭拦截，已打开的子菜单保持打开。

钉住会打开子菜单，打开又同步派发 `SubmenuOpened` 回到 `ExtendToOpenedItem`，因此下发路径带 `_isApplying` 重入保护：应用过程中只更新
请求路径，实际下发由外层应用循环用最新路径完成，避免递归重入破坏正在应用的容器集合。

Popup 背景使用 `MenuPopupBg` / `DarkMenuPopupBg`，不能回退成普通 elevated background。

### 7.8 键盘漫游模型

键盘漫游按“层级容器 + 当前 active 项”计算：

```text
KeyDown
      ↓
resolve active item
      ↓
resolve current visible level
      ↓
move sibling / enter child / return parent / commit leaf
      ↓
sync focus + keyboard active visual
```

当前可见层级由根 `NavMenu`、已打开 inline 子树或已打开 popup content 决定。层级内只包含已生成、可见、可交互的 `NavMenuItem` 容器。分组作为透明 local entry owner 被 navigator 穿过，分隔线直接跳过。`ItemsSource` 数据节点不能直接参与键盘导航，必须通过容器定位，避免数据和视觉状态出现两个 owner。

同层导航使用 local entry owner 链查找前后节点：当前 group 已到边界时回到外层 entry owner 继续查找，但语义 parent 不变。Inline 可见树使用“打开子级优先、无后继时沿 semantic parent 回退”的游标算法，不在每次按键时递归生成 `List<NavMenuItem>`。

初始化 active 项时只能使用当前已经生成且可交互的容器。`SelectedItem` 可以作为 keyboard active 的初始移动锚点，但第一次方向键必须立即移动到选中项前后相邻节点，不能把 active 停在选中项本身；也不能为了初始化 active 项而强制打开隐藏 popup 或 inline 分支。如果选中节点不可见，导航应回退到第一个可导航节点。

打开子菜单时先设置 `IsSubMenuOpen`，再执行必要的模板和 layout 接入以生成子容器，然后把 active/focus 移入子级第一项。关闭子菜单时先关闭当前 popup 分支，再把 active/focus 放回父级触发项。Inline 模式下关闭子菜单不应清理其子树中的 selection path；selection path 只由 selection coordinator 维护。

keyboard active 视觉通过 header 的内部状态表达，使用 `ItemActiveBg` 语义。该状态优先级低于 selected，高于默认态；它不能复用 `IsSelected` 或 `IsInSelectedPath`，否则会把“浏览候选”和“已提交选择”混为同一个状态。`IsInSelectedPath` 只表达选中路径文字语义，不能屏蔽 keyboard active 背景。

### 7.9 Inline collapsed 模型

Inline collapsed 模型按“public state + effective mode + open path cache”组织：

```text
Mode=Inline + IsInlineCollapsed=false
      ↓
effective mode = Inline
inline child frame owns open visuals

Mode=Inline + IsInlineCollapsed=true
      ↓
effective mode = Vertical
popup owns temporary open visuals
inline open paths live in cache
```

折叠状态切换不应复用 public `Mode` 变化逻辑，因为 public mode change 当前会清理打开状态和选择状态。折叠只是 inline 的紧凑呈现状态，必须保留 `SelectedItem`、selected path 和可恢复的 inline open path。

open path cache 应记录路径语义而不是持有容器引用。容器可能因 ItemsSource reset、template reapply、detached 或 popup 生命周期变化而失效；cache 只保存可重新 replay 的 `TreeNodePath` 或等价节点路径。恢复时复用默认路径 replay 的有界容器生成策略，不能固定 sleep。

折叠期间的 popup 打开状态是临时交互状态。它可以改变 `IsSubMenuOpen` 以驱动 popup，但不得写回 inline open path cache。退出折叠时必须先关闭这些 popup 分支，再恢复 cache，避免 popup 与 inline child frame 同时认为自己拥有同一个分支的打开视觉。

初始加载时如果 `IsInlineCollapsed=true` 且存在 `DefaultOpenPaths`，默认展开路径进入 inline open path cache，不立即展开 inline 子树；首次展开时再 replay cache。`DefaultSelectedPath` / `SelectedItem` 仍可应用 selected leaf 和 selected path，不能因为子树未展开而丢失选中语义。

折叠视觉由 theme 层表达：顶层 header 隐藏标题和箭头，icon 使用 `CollapsedIconSize` 居中；没有 icon 的顶层项从 `NodeHeader` 显示首字符。顶层叶子项的 `EffectiveCollapsedTooltip` 附加到真实 header control，显式节点 `Tooltip` 优先，未设置时回退到 `NodeHeader`；带子菜单节点使用 popup，不同时显示折叠 Tooltip。根分组标题隐藏，分组容器把 owner 已解析的 effective inline-collapsed 状态继续投影给子 entry，使任意层透明根分组都保持顶层折叠视觉；节点或 popup 内的非根分组从其 owner 接收 `false`，继续显示普通 vertical 标题和 item。root 宽度约束属于控件布局状态，由 C# metadata coercion 表达，以保留用户的 base `Width` / binding。inline collapsed 宽度过渡由内部 `InlineCollapsedLayoutWidth` motion 临时接管 coercion 输入，完成后必须清理回 `double.NaN`，让稳态宽度重新由 `InlineCollapsedWidth` 或用户原始 `Width` / binding 决定。C# 层不应为了折叠视觉改写 `Header` 或临时替换 `HeaderTemplate`。

宽度motion由UI线程的`DispatcherTimer`每16ms推进，以`Stopwatch`墙钟计算进度并使用`CubicEaseOut`。时长继续读取Application的Shared `MotionDurationMid`，缺失时为200ms。起点来自当前实际宽度，展开终点来自用户base Width或最近展开宽度；这些测量及中断轨迹无法由ControlTheme等价计算。CTS身份限定唯一有效motion，改向、停用动效和卸载都取消旧timer，完成时清除临时宽度。后台worker繁忙不阻止仍在处理任务的UI产生中间布局帧。

### 7.10 内容展开的共享接入边界

`NavMenuItem.SetInlineChildItemsOpenAsync` 将目标状态、方向和有效时长提交给 Core 内部
`ContentExpansionAnimator`。动画执行器持有私有展开进度附加属性、布局失效通知、单次执行的取消源与原生 `Animation`；
进度值和透明度在同一 actor 上同步播放。`BaseMotionActor` 只通过内部 `IMotionActorLayout` 委托自然测量和完整内容排列。
算法所有者见 [内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)。

- 根 `NavMenu` 同步协调顶层互斥；item 持有 `IsSubMenuOpen`、排队请求身份和当前模板的 animator。
- Dispatcher 回调在接触 actor 前核对请求版本、animator、mode 与目标状态，完成后再次确认有效性；同值旧请求不能因当前布尔值相同而重新获得执行权。
- 测量输入包含 `PART_ChildItemsFrame` 的完整内容及间距。裁剪主轴为高度，内容靠近 header 的顶边为锚点。
- 取消前捕获呈现值、首个 tick 前保持起点、最终 tick 后保持终点以及执行资源释放由动画执行器统一处理。
- 完成结果返回 item 后，由菜单层发送对应完成事件及处理命令可用性；取消与过期请求不发布完成事件。
- mode 切换、进入 Inline Collapsed、模板替换及 detach 使旧请求与执行失效；共用层不操作路径缓存、selection 或 Popup。
- 内容或嵌套子菜单尺寸变化沿布局失效更新自然尺寸，正在执行的展开目标从当前帧接续且保留原截止时间。
- 稳定状态解除内部进度与布局接管，保留用户或模板的尺寸、transform 和 transition；根菜单折叠宽度动画使用独立职责。

## 8. 资源、性能与 AOT 边界

NavMenu 不应通过反射访问 template part 或内部状态。Header、popup、inline child frame 和 active indicator 均通过稳定 template part 和 Avalonia 属性接入。

`NavMenuNode` 必须通过 `[GenerateScopedResourceHost]` 生成 `IResourceHost` / `IThemeVariantHost`、attachment count、host generation 和 `IDisposable` attach token。generation 用于使跨 host 切换后遗留的 stale token 失效，尤其不能让 `A -> B -> A` 中第一轮 A token 释放当前 A attachment。`Header`、`Tooltip` 等节点属性的 `DynamicResource` 都使用该 scoped owner；scoped resource host 只解决动态资源宿主及其事件订阅，不替代节点到容器 binding 的释放，也不替代 `ICommand.CanExecuteChanged` 的解绑。

`NavMenuGroup` 遵守相同的 generated scoped resource-host 生命周期；结构容器不能永久持有 group 或 owner menu。`NavMenuDivider` 不需要资源宿主和订阅。

命令能力的完整释放边界由三层共同组成：

1. generated scoped resource host 释放 `ResourcesChanged` / `ActualThemeVariantChanged` owner 订阅；
2. container `CompositeDisposable` 释放节点到 `NavMenuItem` 的 `Command`、`CommandParameter` 和其他属性 binding；
3. `NavMenuItem` 在 command replacement 与 logical-tree detach 时解除 `CanExecuteChanged`。

缺少任意一层都不能宣称节点命令生命周期完整。不得通过弱化动态资源、改为静态值、永久 owner 引用、全局 command cache 或延迟清理规避释放问题。

`CanExecuteChanged` 可能在一次同步命令执行中快速发出 `false -> true`，共享同一命令的多个叶子容器如果逐次立即更新 effective enabled，会同时启动 disabled 前景色过渡并产生闪动。`NavMenuItem` 将通知统一 marshal 到 UI Dispatcher，并以 `Input` 优先级合并同一 UI 周期内的重复通知；回调只重新读取一次当前 `Command.CanExecute(CommandParameter)`，因此持续 `false` 仍会在下一轮交互前生效，而瞬时变化不会暴露中间视觉状态。待处理 operation 由当前 container 持有，并在 command replacement、parameter replacement 和 logical-tree detach 时 abort，不能让已回收容器被 dispatcher callback 延迟持有或被旧 command 状态回写。

handler 持有事件订阅时必须在 mode 切换、detached 或模板替换时释放。延迟打开 / 关闭任务必须支持取消，避免旧 pointer 状态影响新 mode 或新 popup。任务必须记录对应目标容器，container clear 只取消引用该容器的 pending open / close，不能误取消其他项的当前 hover。

inline collapsed cache 不得持有 `NavMenuItem`、header、popup 或 template part 引用。状态失效边界包括 ItemsSource reset、container clear、detach、mode change 和 default path replay revision 变化。

生成容器进入 clear/recycle 时必须通过 `NavMenu.ForgetGeneratedContainer` 同时通知 selection coordinator 和当前 interaction handler。该入口清除 selection coordinator 的 realized container 引用、keyboard-active 引用、激活事务的待提交项与 pointer-hold 视觉，以及 Default handler 指向该容器的 pending open / close；之后才释放 binding、resource host 和 entry context，避免旧交互回调读取已清空的数据对象。

键盘导航状态持有的 active item 引用必须随 detach、mode 切换、container clear、popup close 和 item disabled 变化失效。popup 内容可能在关闭后继续保留生成容器，因此有效性不能只看局部 `IsVisible`；必须同时验证 effective visible/effective enabled 和完整语义父链的打开状态。失效后第一次 Enter 只从当前可见层级重新建立 active 项，不提交旧项。

默认路径 replay 必须有界，避免容器生成失败时形成无休止 dispatcher 队列。

纯节点菜单必须保持一个数据节点对应一个 `NavMenuItem`，不得为统一模型增加额外 wrapper。只有实际存在 `NavMenuGroup` 或 `NavMenuDivider` 时才生成结构容器。语义 `Level` 在 prepare 上下文中 O(1) 得到；直接容器和路径查找优先走无分组快路径；spacing 只在根控件计算公开值与后代默认值的关系，再通过内部继承属性和 ItemsPanel `TemplateBinding` 传播，不为每个节点增加 relay binding。

默认 TreeDataTemplate 对 `INavMenuNode.Entries` 使用编译绑定；自定义节点通过协变接口默认实现回退到 `Children`。内置集合直接满足 Avalonia ItemsSource 的 `IList` / `INotifyCollectionChanged` 契约，该默认路径不分配适配集合。实现不使用反射、运行期类型扫描、动态 Style、dispatcher 延时或持久化扁平导航缓存，保持 NativeAOT 可分析。

## 9. 维护不变量

内部重构必须保持以下不变量：

- mode 切换时重新挂接 handler，并通过保留选择的关闭路径清理旧模式打开状态；当前 `SelectedItem` 和 selected path 必须在当前容器上继续投影。
- pointer 外点、窗口停用、平台失焦和非客户端点击关闭 popup 时不得调用 `NavMenu.Close()`，不得清空 `SelectedItem`；public `Close()` 的显式清空合同保持不变。
- `IsInlineCollapsed` 切换不得改写 public `Mode`，不得调用 `Close()`，不得清空 `SelectedItem`。
- inline collapsed 进入时缓存 inline open path，退出时恢复 cache；折叠期间 popup 打开状态不得污染 cache。
- `InlineCollapsedWidth` 默认来自 `NavMenuToken.InlineCollapsedWidth`，本地属性值必须按 Avalonia 优先级覆盖 token 默认值。
- 折叠视觉不能通过改写 `Header`、删除 `HeaderTemplate` 或动态创建替代 header 实现。
- 节点 `Tooltip` 与 `Header` 保持独立；未设置 `Tooltip` 时才回退到 `Header`，不能引入第二个标题属性代替 Tooltip 语义。
- `ToolTip.Tip` 只能附加到实际 header control，不能扩大到非 Visual `NavMenuNode`；有效内容只由 `NavMenuItem` 计算。
- container rebind、clear 和 recycle 必须同时释放节点 Tooltip binding 与菜单 Tooltip policy binding，并使旧 header 的有效 `ToolTip.Tip` 归零。
- 点击 item 不得临时关闭 motion。
- 默认路径应用不使用固定 50ms sleep 作为稳定策略。
- selection coordinator 是选择状态的统一入口。
- keyboard active/focus 状态不能替代 selection coordinator。
- 激活事务由 interaction handler 基类唯一持有；按下只建立事务、置仅覆盖背景的 pointer-hold selected-background 视觉并按 mode 尝试移动真实焦点，不覆盖 keyboard-active owner、不写 selected 状态、不进入 selection coordinator、不执行命令、不触发路由事件、不切换 inline 展开状态。
- 指针提交以“释放点命中待提交项 Header 视觉子树”为唯一合法性判据，不使用捕获期间的 `IsPointerOver`；取消路径（拖离释放、当前指针捕获丢失、节点移除或禁用、detach、非主按钮释放、新按下替代）零副作用，其他指针的 capture-lost 不得误取消。
- 调用命令与触发 `NavMenuItemClick` 前必须先清理事务字段，并仅在指针仍由该事务的 Header 捕获时释放捕获，防止用户回调重入时残留旧事务。
- 叶子提交顺序固定：选中路径与 `IsSelected` 更新、`SelectedItem` 更新、`NavMenuNodeSelected`、节点 `Command`、`NavMenuItemClick`；同步重入替换选择时停止原提交尚未发生的事件与动作。父节点提交不修改 `SelectedItem`。
- pointer-hold 与 keyboard-active 独立持有并分别投影到 header 的 `IsPointerHold` 与 `IsKeyboardActive`；前者只使用 selected 背景 Token 且不覆盖文字颜色，后者使用 active Token。pointer-hold 不写入 keyboard active、`IsSelected` / `IsInSelectedPath`，也不直接依赖捕获期间的原生 `:pointerover`。
- 方向键移动不得触发点击或选中事件。
- keyboard active 初始解析可以复用可见 `SelectedItem` 容器作为方向键移动锚点，但不得吃掉第一次方向键、触发选择事件或自动打开隐藏分支。
- Esc 关闭 popup 分支不得调用 `NavMenu.Close()`，不得清空 `SelectedItem`。
- 键盘打开 popup 或 inline 子项不得依赖固定 timer 生成容器。
- header hover / selected 背景不通过父级 item hover 状态误触发。
- `IsItemBackgroundEnabled=false` 不关闭 header 颜色和交互状态。
- popup、root、inline child frame、header 四类背景职责保持分离。
- handler 取消逻辑不能泄漏事件订阅或延迟任务。
- `NavMenuNode` 不实现 `ICommandSource`，不直接执行命令或订阅 `CanExecuteChanged`。
- scoped resource-host attachment、node relay binding 和 command subscription 必须各自具有确定释放点。
- `CanExecuteChanged` 的合并 operation 必须由当前 container 持有，并在 command / parameter 替换和 logical-tree detach 时取消。
- container rebind、clear、recycle、Items reset 和 re-template 后，旧节点、旧命令和旧 owner 不得继续持有当前容器。
- `CommandParameter` 不隐式使用 `ItemKey`，避免显式 `null` 和容器同步语义分叉。
- `Entries` 是唯一结构集合，`Children` 不得拥有第二份节点存储。
- direct `Items`、`ItemsSource`、节点 `Entries` 和分组 `Entries` 必须共用 `INavMenuEntry` 校验语义；初始装载、source replacement、Add、Replace 和 Reset 不得存在绕过路径。
- 内置 `NavMenuNode` / `NavMenuGroup` 必须以弱 structural owner 保证同一实例只有一个直接挂载位置；`ParentNode` 和 `SemanticParentNode` 不能替代该结构所有权。无状态 `NavMenuDivider` 不进入 owner 跟踪。
- custom node 自身不登记 owner，但其内置后代必须由最近 built-in/root scope 的完整图协调器检测；嵌套 collection subscription 必须是弱订阅并在 source 离图时释放。
- custom observable source 的 post-mutation 同步必须重新执行 owner-cycle 校验，不能把当前 built-in owner 或任意 built-in 祖先登记为自己的后代。
- built-in child collection 由 child 自身协调器负责，祖先不得递归订阅；纯 built-in 深树的订阅数量不得随祖先/后代组合增长为 O(N²)。
- 非法 entry 必须在容器生成和资源 attach 前确定性失败；不能静默忽略、降级为普通 content 或依赖后续 cast 暴露错误。
- 分组和分隔线不得实现或模拟 `INavMenuNode`、`ISelectable`、`ICommandSource` 或 keyboard active 状态。
- 节点的 `ParentNode`、容器的 `Level` / `IsTopLevel`、选择祖先和键盘父级只能来自 semantic owner，不得从逻辑树距离推导。
- 节点、分组和分隔线使用不同 recycle key，clear 必须移除各自 owner、binding 和状态。
- 纯节点菜单不增加结构容器、递归扁平缓存或每项 spacing binding。
- root Header/Footer 保持固定，空 slot 不占布局；结构标题和分隔线的 mode/collapsed 变体由各自内部 ControlTheme 维护。

## 10. Semantic Part 实现

Semantic Part 声明位于 `src/AtomUI.Desktop.Controls/NavMenu/NavMenu.SemanticParts.cs`，`NavMenu` 本体保持 partial。上游
12 个键路径全部声明在 `NavMenu` 上：`NavMenuItem`、`NavMenuGroupItem`、`NavMenuDividerItem` 是 internal 容器，不能持有
descriptor，也不能作为 `ContractType`（菜单项容器取其最近 public 基类 `HeaderedSelectingItemsControl`）。marker 放置：

| marker | 放置方式 | 位置 |
| --- | --- | --- |
| `.semantic-item`（一级）/ `.semantic-sub-menu-item`（子菜单） | 运行时按层级幂等注入 | `NavMenuEntryContainerCoordinator` 的 `NavMenuItem` prepare 分支。 |
| `.semantic-scope-group`（一级）/ `.semantic-sub-menu-group`（子菜单） | 运行时按层级幂等注入 | 同一协调器的 `NavMenuGroupItem` prepare 分支；路由跳点，不是 Part。 |
| `.semantic-scope-header` | 静态模板节点 | `NavMenuItemTheme.axaml` 三个 mode 模板的 `HorizontalNavMenuItemHeader#PART_Header` / `VerticalNavMenuItemHeader#PART_Header` / `InlineNavMenuItemHeader#PART_Header`；路由跳点，不是 Part。 |
| `.semantic-item-icon` + `.semantic-sub-menu-item-icon`、`.semantic-item-content` + `.semantic-sub-menu-item-content` | 静态模板节点 | 三个 header 主题的 `IconPresenter#ItemIconPresenter` / `ContentPresenter#ItemTextPresenter`；同一节点同时携带两个层级 marker，由 route 的容器 anchor 决定命中哪个 Part。 |
| `.semantic-item-title` + `.semantic-sub-menu-item-title`、`.semantic-list` + `.semantic-sub-menu-list` | 静态模板节点 | `NavMenuGroupItemTheme.axaml` 的 `ContentPresenter#PART_HeaderPresenter` / `ItemsPresenter#PART_ItemsPresenter`；同上。 |
| `.semantic-popup-root` | 静态模板节点 | `NavMenuItemTheme.axaml` 的 Horizontal / Vertical 模板 `NavMenuPopupFrame#PART_PopupFrame`。 |

`NavMenuEntryContainerCoordinator` 是三类容器唯一的创建与 prepare 入口（`NavMenu`、`NavMenuItem`、`NavMenuGroupItem` 都
委托它）。层级 marker 依赖 `EntryContext.IsTopLevel`，而 `CreateContainer` 是静态工厂、拿不到 owner 与层级，因此层级
marker 在 `PrepareContainer` 路径经 `ApplySemanticLevelClass` 注入：先移除另一层级的类，再幂等补齐当前层级。容器在
owner 之间转移或被回收复用时既不残留旧层级，也不重复添加。marker 不放进容器的 ControlTheme 模板。

路由边界：

- 全部非 root Part 的 route 以 `>>` 开头：容器是运行时生成物，不在 `NavMenu` 模板内，无法用 `/template/` 到达；按生成器
  route 语法（以 `>>` 开头仅允许 `CrossNestedOwners=true` 部件）声明 `CrossNestedOwners`。
- 层级隔离靠互斥 marker，不靠 route。`DescendantSelector` 沿完整逻辑祖先链匹配、不在最近的语义 owner 处停止，所以
  `>> .semantic-item` 会同时命中一级与子菜单容器；只有把层级编码进容器 marker 才能让 `item` 与 `subMenu.item` 各命中
  一层。不得用更宽的 descendant 或 owner scope 替代层级 marker。
- `>> .semantic-item /template/ .semantic-scope-header /template/ .semantic-item-icon` / `...-content` 与
  `>> .semantic-scope-group /template/ .semantic-item-title` / `...-list` 依赖跳点节点的真实模板边界。跳点必须静态声明在
  承载它的模板中，不能改由代码按 Name 查找后动态设置 class。
- `popup.root` 的 `>> .semantic-popup-root` 覆盖任意深度子菜单的弹层框体；节点位于 `Popup.Child` 属性值子树，因此同时
  声明 `CrossVisualRoot=true`。Inline 模板没有该节点，该模式实例数为 0。

生命周期与性能不变量：

- marker 只在容器 prepare 时按层级添加。选择、hover、pressed、keyboard-active、disabled、dark style、
  `IsItemBackgroundEnabled`、`IsSubMenuOpen` 与 mode 切换一律不增删 marker。
- 容器回收复用遵循同一注入路径：先移除另一层级 marker，再补齐当前层级；不得依赖 GC、DataContext 清空或页面导航清理。
- marker 使用静态 `Classes.semantic-*="True"` 或 `Classes.Add(<生成常量>)`，不建立 Binding、selector activator 或持久订阅；
  未声明用户 Semantic Style 时不产生额外 VisualTree 查询。
- 默认主题不选择 `.semantic-*`。Part 不引入运行时 descriptor 查询、以 Visual 为 key 的缓存或永久监听器。

验证范围见 [NavMenu Semantic Part 契约](semantic-part.md) 的兼容性与验证章节，以及本文档第 11 节的 Semantic Part 条目。

## 11. 测试与验证

验证范围：

- 激活事务：按下零副作用（已选中 A 时按下 B，选择、事件与命令均不变）；同项按下并释放单次提交；完整提交顺序（`SelectedItem` 与 `NavMenuNodeSelected` 先于节点 `Command` 与 `NavMenuItemClick`，命令执行时公共状态已是新值）；同一节点重复激活不重复触发 `NavMenuNodeSelected`。
- 重入与取消：同步 `SelectedItem` 观察者改写选择时不产生陈旧选择事件或原节点 invocation；当前捕获指针丢失会取消、其他指针 capture-lost 不会误取消；释放到另一节点或菜单外、按下后禁用节点、handler 替换、新按下替代均零提交；按下 B 拖出后移回 B 释放提交且仅一次。
- 父节点激活：Inline 父节点合法释放切换展开一次，Default 父节点合法释放打开 popup；按下阶段不切换。Enter/Space 与指针释放的分派、顺序和命令执行一致。
- 事务视觉：pointer-hold 的背景与 selected 完全一致、文字颜色不变且不提交选择，低于真实 selected 优先级，按下置位 / 拖出清除 / 移回恢复；合法释放保持该视觉直到 selection 提交后再清除，且与使用 active Token 的 keyboard-active 独立持有。
- Inline、Vertical、Horizontal 打开、关闭、hover、click 和同级互斥。
- Inline collapsed 切换、effective mode、open path cache、初始 `DefaultOpenPaths` 缓存、展开恢复、popup 临时打开和 selected path 保持。
- Inline、Vertical、Horizontal 的 Up / Down / Left / Right / Enter / Esc 键盘漫游、层级进入/返回、leaf commit 和 popup close。
- 方向键移动 active 项不触发 `NavMenuItemClick` / `NavMenuNodeSelected`。
- Esc 关闭当前 popup 分支但不清空 `SelectedItem`。
- 禁用项、隐藏项和未展开子项不进入键盘漫游序列。
- 任意层级分组和分隔线不进入键盘、选择、命令或路径序列；跨分组前后移动和 semantic parent 返回保持正确。
- `Entries` / `Children` 覆盖 Add、Remove、Move、Replace、Reset、嵌套分组、兼容自定义节点和 parent 投影。
- direct `Items` 和 `ItemsSource` 覆盖初始装载、source replacement、Add、Replace、Reset；`null` 和非 `INavMenuEntry` 项均在容器生成前抛出包含 owner、索引和实际类型的 `InvalidOperationException`。
- 重复挂载覆盖同一/不同 `Entries` owner、根 source、Remove 后重挂载、source removal 后换根菜单、root owner WeakReference 回收，以及 `NavMenuDivider` 多位置复用。
- custom wrapper 覆盖离线和根菜单中的初始共享内置后代、嵌套 observable source 动态获取/释放 owner、动态重复、动态指回 built-in owner/祖先、Remove 后重挂载，以及外部 custom source 不得保留 owner。
- 批量初始化覆盖同步观察者重入，确保完整批次在首次通知前已经提交 ownership；纯 built-in 深链覆盖祖先不订阅后代 collection。
- `SelectedItem`、`DefaultSelectedPath`、`DefaultOpenPaths`、stale replay、clear selection、容器回收后的 sibling selection、视觉树 detach 后的程序化选择切换，以及任意深度 collapsed popup 关闭重开后的 selected leaf 投影。
- 点击子节点时父级 header 不出现错误 hover 背景。
- `IsItemBackgroundEnabled=true/false` 下 inline 背景块、header 背景和间距分别正确。
- Dark root、popup、submenu、header、selected 和 hover 颜色与 Token 语义一致。
- Popup 打开、关闭、失焦、pointer leave 和 mode 切换后无旧状态残留。
- Motion 不因点击、打开或关闭流程被临时禁用。
- 节点 `Command` / `CommandParameter` 在 pointer 和 keyboard 提交时只执行一次，参数保持显式值。
- `CanExecute=false` 正确影响 effective disabled，命令变化后旧 `CanExecuteChanged` 订阅被解除。
- 同步 `ReactiveCommand` 的瞬时 `false -> true` 不得让当前叶子、共享命令叶子或 selected path 祖先暴露中间 disabled 视觉。
- container rebind、clear、recycle、Items reset、re-template 和页面卸载后，旧节点、旧命令和 ViewModel 可被回收。
- scoped resource host 覆盖 DynamicResource WeakReference、owner resource 优先、resource update、repeated attach 和 attach token release。
- group scoped resource host、container clear/recycle、ItemsSource replacement 和 WeakReference 回收完整释放。
- 外部持有 DynamicResource 节点或分组时，卸载菜单仍可 GC；重复 detach/reattach 保持 owner/application 资源回退、最新数据与选择。资源重新挂载回调删除节点后，旧容器保持清理状态。
- Replace 覆盖跨 owner 抢占、同集合重入、回调异常和嵌套分组父关系投影；通知时成员、owner 与可更新后代的 ParentNode 一致。
- 父 Header 左、中、右按下并在子 Header 释放均取消，正常父 Header 整行点击仍可用；真实 capture 转移保留新控件捕获。
- 节点模板覆盖 null→template→null 及 owner 模板更新；ItemKey 覆盖更新与清空。
- Header/Footer 固定区域、空 slot 退化、Horizontal 左右区域、root/popup/collapsed 分组视觉和 divider orientation 稳定。
- 纯节点容器数量保持不变，三种 generated container recycle key 相互隔离，键盘移动不创建扁平 item list。
- Semantic Part：`NavMenu` descriptor 的 Part 名称逐字等于上游 12 个键路径，顺序、Selector、route、ContractType、
  cardinality 与 [Semantic Part 契约](semantic-part.md) 一致；`NavMenuItem`、`NavMenuGroupItem`、`NavMenuDividerItem` 与
  三个 `*NavMenuItemHeader` 不持有 descriptor；三种 mode 与 inline collapsed 下各 Part 的 marker 数量正确，空集合为 0。
- Semantic Part 层级隔离：一级容器只带 `.semantic-item` / `.semantic-scope-group`，子菜单容器只带
  `.semantic-sub-menu-item` / `.semantic-sub-menu-group`；`item` 的 `>>` route 不命中子菜单容器，`subMenu.item` 的 `>>`
  route 不命中一级容器；位于一级分组内部的项仍算一级，位于子菜单内分组内部的项仍算子菜单项。
- Semantic Part route 可达性：全部非 root Part 的 `>>` route 在 Inline、Vertical、Horizontal 与 inline collapsed 下可达，
  且在 PopupRoot 与 OverlayPopupHost 两条宿主路径下分别验证；经 `.semantic-scope-header` 的两段 `/template/` route 命中
  菜单项的 icon / content，经分组跳点的 route 命中分组标题与分组列表；Inline 模式 `popup.root` 为 0。
- Semantic Part 生命周期：容器在 owner 之间转移、prepare / clear / recycle、Items reset、re-template 与 Popup
  打开-关闭-重新打开后 marker 数量不变，不重复添加、不残留另一层级，也不随选择、hover、pressed、keyboard-active、
  disabled 或 mode 切换增删。
- Semantic Part 用户入口：`atom|NavMenu` owner-scoped 生成的 Semantic Style 与 `x:SetterTargetType` 可编译并命中最低
  public 类型（菜单项容器为 `HeaderedSelectingItemsControl`）；`popup.root` 的 setter 在 PopupRoot 与 OverlayPopupHost
  两条路径都生效；默认主题不消费 `.semantic-*`。
- 文档改动运行 `git diff --check`。
- 共用内容动效的验证还需覆盖不等高分支交换、三层嵌套中内容尺寸变化、原有 ScrollViewer 约束与命中范围；
  使用可控时钟和 1× / 2× 帧采样验证共享契约，不能只检查最终可见性。
