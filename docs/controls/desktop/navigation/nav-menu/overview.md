# NavMenu 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.NavMenu` 桌面版的最新设计定位、公共契约、导航状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [NavMenu 桌面版实现原理](implementation.md)，NavMenu Token 的专项设计见 [NavMenu Token 设计](token.md)，项激活的提交契约见 [NavMenu 项激活事务设计](item-activation-design.md)，设计和契约变化记录见 [NavMenu Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `NavMenu` / `NavMenuItem`：`NavMenu.IsPopupPinnedOpen` 是公开属性（与 `Tour`、`DropdownButton`、`AbstractSelect` 等家族约定一致），因为跨程序集的 Gallery 语义预览需要在 AXAML 中声明钉住；`NavMenuItem` 上的同名属性保持 internal。设置为 true 时，NavMenu 选择首个可展开项并在路径上打开、钉住 submenu open state 并 relay 到 submenu Popup；设置为 false 时只解除关闭拦截。钉住请求以 `INavMenuNode` 为键保存在 `NavMenu` 上，独立于运行时生成的容器生命周期，因此声明式钉住在模板应用、容器生成之前写入也能对晚到或重建的容器生效。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/Menu`；结构化 entry 示例位于 Menu ShowCase 最后一项 |
| 控件状态 | Stable |

NavMenu 是 AtomUI 桌面导航体系中的层级菜单导航控件，用于表达应用页面、模块、功能入口或命令集合之间的层级关系。它以树形节点为数据模型，以 `Inline`、`Vertical`、`Horizontal` 三种模式映射到不同导航场景。

NavMenu 的职责是管理导航节点容器生成、层级展开、选中路径、弹出式子菜单、主题视觉和菜单交互。它不负责路由切换、页面生命周期、权限过滤、数据懒加载、业务命令编排或页面内容渲染。业务导航行为可以通过节点 `Command`、`NavMenuNodeSelected`、`NavMenuItemClick` 或外部 ViewModel 处理。

NavMenu 支持两种 entry 提供方式：

- 直接在 `NavMenu.Items` 中放置 `INavMenuEntry`。
- 通过 `ItemsSource` 绑定 `INavMenuEntry` 集合。

`INavMenuEntry` 是节点、分组和分隔线的共同结构契约。`INavMenuNode` 表达可交互导航节点，`NavMenuGroup` 表达带标题的透明结构分组，`NavMenuDivider` 表达不可交互分隔线。首版支持的 entry 种类封闭为这三类；仅自行实现 `INavMenuEntry` 不能注册第四种容器类型，扩展交互节点应实现 `INavMenuNode`。NavMenu 不支持把任意 `Control`、`Panel` 或 `TextBlock` 作为 entry 直接加入；Header、Footer 和分组标题的任意内容通过对应 content/template API 承载。

## 2. 设计语言

NavMenu 表达的是“层级入口 + 当前路径”的导航语义。用户应能通过文字、图标、缩进、选中态、悬浮态和展开状态理解当前位置、可进入的下级路径以及不同层级之间的关系。

三种模式对应不同产品语义：

| 模式 | 语义 | 典型场景 |
| --- | --- | --- |
| `Inline` | 子菜单在当前导航面板内展开，强调完整层级结构。 | 侧边栏主导航、管理后台模块导航。 |
| `Vertical` | 顶层项目垂直排列，子菜单通过 Popup 展开。 | 紧凑菜单、上下文导航、浮层式侧栏。 |
| `Horizontal` | 顶层项目水平排列，子菜单通过 Popup 展开。 | 顶部导航栏、一级模块切换。 |

Light 与 Dark 样式不是简单反色。Dark 样式有独立的根背景、Popup 背景、子菜单背景、选中背景和文字透明度规则，应保持与 参考 Menu 的层级背景语义一致。

`IsItemBackgroundEnabled` 控制菜单项背景块模型。开启时，inline 子菜单背景块、选中背景和 hover 背景形成连续层级背景；关闭时，菜单项背景块保持透明，但 header 文本颜色、选中路径颜色和交互状态仍然保留。

## 3. API 与契约模型

NavMenu 的公共 API 分为控件 API、节点 API 和事件 API。

控件 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Mode` | `NavMenuMode` | 菜单呈现模式，默认 `Inline`。 |
| `IsInlineCollapsed` | `bool` | `Mode=Inline` 时是否进入内联折叠状态，默认 `false`。折叠不改变 public `Mode`，只改变内部有效呈现和交互模式。 |
| `InlineCollapsedWidth` | `double` | 内联折叠状态下的菜单宽度。默认值来自 `NavMenuToken.InlineCollapsedWidth`，初始设计值为 `48`；开发者可通过本地值覆盖 token 默认宽度。 |
| `IsCollapsedTooltipEnabled` | `bool` | 是否为有效 inline collapsed 状态下的顶层叶子节点启用 Tooltip，默认 `true`。 |
| `CollapsedTooltipPlacement` | `PlacementMode` | 折叠 Tooltip 的放置方向，默认 `Right`。 |
| `CollapsedTooltipShowDelay` | `int` | 首个折叠 Tooltip 的显示延迟，单位毫秒，默认 `400`。 |
| `CollapsedTooltipBetweenShowDelay` | `int` | 连续折叠 Tooltip 之间允许立即切换的时间窗口，单位毫秒，默认 `100`。 |
| `SelectedItem` | `INavMenuNode?` | 当前选中节点，双向绑定入口。 |
| `DefaultSelectedPath` | `TreeNodePath?` | 初始选中路径；`SelectedItem` 非空时优先级更高。 |
| `DefaultOpenPaths` | `IList<TreeNodePath>?` | 初始展开路径集合。 |
| `IsAccordionMode` | `bool` | 顶层 inline/vertical 子菜单互斥展开策略。 |
| `IsDarkStyle` | `bool` | 使用 NavMenu dark style 视觉体系。 |
| `IsItemBackgroundEnabled` | `bool` | 控制 item / inline submenu 背景块模型，默认 `true`。 |
| `IsMotionEnabled` | `bool` | 控制 inline 展开收起和主题颜色过渡。 |
| `ShouldUseOverlayPopup` | `bool` | 控制弹出式子菜单是否使用 overlay popup。 |
| `Header` | `object?` | 根导航固定头部内容；Inline/Vertical 位于滚动菜单区上方，Horizontal 位于菜单项左侧。 |
| `HeaderTemplate` | `IDataTemplate?` | 根导航头部内容模板。 |
| `Footer` | `object?` | 根导航固定尾部内容；Inline/Vertical 位于滚动菜单区下方，Horizontal 位于菜单项右侧。 |
| `FooterTemplate` | `IDataTemplate?` | 根导航尾部内容模板。 |
| `ItemSpacing` | `double` | 默认 ItemsPanel 中相邻 entry 容器的额外间距；默认值来自当前 mode 的主题映射。 |
| `Close()` | method | 关闭所有子菜单并清空 `SelectedItem`。 |

节点 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `Header` | `object?` | 菜单项显示内容。 |
| `HeaderTemplate` | `IDataTemplate?` | 菜单项 header 模板。 |
| `Tooltip` | `object?` | 折叠叶子节点的独立 Tooltip 内容；为 `null` 时回退到节点 `Header`。传入 `ToolTip` 实例可获得完整定制能力：实例上显式设置的呈现类附加属性（位置、颜色、箭头、文本换行等）优先于菜单级与宿主配置，未设置的回落，详见 [Tooltip 桌面版架构设计](../../data-display/tooltip/overview.md) 的 Tip 实例定制模型。 |
| `IsTooltipEnabled` | `bool` | 是否允许当前节点显示折叠 Tooltip，默认 `true`。 |
| `ItemKey` | `EntityKey?` | 路径和业务标识。 |
| `Icon` | `PathIcon?` | 菜单项图标。 |
| `IsEnabled` | `bool` | 节点可用状态，默认 `true`。 |
| `Command` | `ICommand?` | 节点被有效触发时由当前 `NavMenuItem` 容器执行的业务命令。节点只承载命令配置，不负责订阅或执行。 |
| `CommandParameter` | `object?` | 传递给 `Command` 的参数。默认 `null`，不隐式回退到 `ItemKey` 或节点自身。 |
| `INavMenuNode.Entries` | `IEnumerable<INavMenuEntry>` | 容器读取子 entry 的协变契约。接口默认返回既有 `Children`，自定义节点无需新增实现。 |
| `NavMenuNode.Entries` | `IList<INavMenuEntry>` | 内置节点的唯一有序、可写子 entry 集合，也是 XAML content 属性；允许节点、任意层级分组和分隔线。 |
| `Children` | `IList<INavMenuNode>` | `Entries` 的实时语义节点兼容视图，不保存第二份集合；分组对该视图透明。 |

结构 entry API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `INavMenuEntry` | interface | NavMenu 有序 entry 的共同标记契约，不带选择、命令或路径语义；它不是自定义容器注册点。 |
| `NavMenuGroup.Header` | `object?` | 分组标题内容。 |
| `NavMenuGroup.HeaderTemplate` | `IDataTemplate?` | 分组标题模板。 |
| `NavMenuGroup.Entries` | `IList<INavMenuEntry>` | 分组内有序 entry 集合，允许继续嵌套分组。 |
| `NavMenuDivider` | class | 不可聚焦、不可选中、不进入路径或键盘漫游的结构分隔线。 |

事件 API：

| 事件 | 参数 | 语义 |
| --- | --- | --- |
| `NavMenuItemClick` | `NavMenuItemClickEventArgs` | 菜单项点击事件，事件参数暴露 `INavMenuItem` 作为只读交互上下文。 |
| `NavMenuNodeSelected` | `NavMenuNodeSelectedEventArgs` | 叶子节点选中事件，事件参数暴露 `INavMenuNode`。 |

`DefaultSelectedPath` 和 `DefaultOpenPaths` 是默认值入口，不是持续受控展开状态。运行期受控选择应使用 `SelectedItem`。默认选中路径排队后若发生显式选择，旧请求必须失效，不能覆盖用户最新选择。

节点 `HeaderTemplate` 与 `ItemKey` 在容器实现后继续响应属性通知。节点模板为 `null` 时使用当前 owner 的 `ItemTemplate`；设置节点模板后优先使用节点值，清空后恢复当前 owner 模板。自定义节点通过 `INotifyPropertyChanged` 通知这些变化；不可通知的节点在容器准备或重新挂载时读取当前值。

`NavMenuNode.Entries` 是内置节点子结构的唯一真源。`Children` 保持既有类型和节点语义：枚举时递归穿过同一语义层级内的 `NavMenuGroup`，但不进入子节点自身的后代；`Add` 把节点追加为直接 entry，`Insert`、替换和删除写回节点当前所在的实际 entry owner，`Clear` 清空当前节点的整个 `Entries`。`INavMenuNode.Entries` 使用 `IEnumerable<INavMenuEntry>`，利用 `IEnumerable<T>` 协变在接口默认实现中直接返回既有 `IEnumerable<INavMenuNode> Children`，不创建适配集合；`NavMenuNode` 通过显式接口实现把其可写 `Entries` 投影为该读取契约。因此既有纯节点自定义实现保持源码兼容，并继续由原 `Children` 集合通知驱动容器更新。

根 `NavMenu.Items`、根 `ItemsSource`、`NavMenuNode.Entries` 和 `NavMenuGroup.Entries` 使用同一 entry 类型边界：集合中的每一项必须是 `INavMenuNode`、`NavMenuGroup` 或 `NavMenuDivider`；`null`、普通业务对象、内部生成容器和仅实现 marker 的其他类型都不是合法数据项。直接 `Items` 变更与 `ItemsSource` 的初始装载、替换以及 Add、Replace、Reset 通知必须进入同一验证入口；发现非法项时立即抛出包含来源和索引信息的 `InvalidOperationException`，不能静默跳过、按普通内容呈现或等到容器绑定阶段再产生类型错误。

内置 `NavMenuNode` 和 `NavMenuGroup` 是有状态结构 entry，同一实例在整个 entry 树中只能拥有一个直接结构 owner，不能在同一集合重复，也不能同时挂到两个节点、分组或根 `NavMenu`。从原 owner Remove、Replace、Clear 或移除根 source 后，该实例可以重新挂载。结构 owner 使用弱引用；资源宿主 attachment 与 node/group 到容器的属性订阅随根菜单 visual detach 释放，重新挂载时按当前数据恢复。因此外部长期持有 node/group 不会反向保留已卸载且不可达的根菜单。普通子菜单关闭不结束根菜单的资源作用域。`NavMenuDivider` 不保存选择、展开、父级、资源或容器状态，因此同一 divider 实例允许在多个位置复用。

自定义 `INavMenuNode` 实例自身保持既有兼容契约，不强制登记内置 structural owner；但其 `Entries` / `Children` 中出现的内置 `NavMenuNode` 或 `NavMenuGroup` 仍必须参加完整引用唯一性检查。每个内置 `NavMenuNode` / `NavMenuGroup` entry owner 和根 `NavMenu` 都协调自己的结构 scope：遍历直接 entry，并递归穿过 custom node；遇到内置 node/group 后由该内置 entry 自己的协调器接管后代。custom node 下的内置后代继承最近的内置 entry owner，只有根级 custom node 的内置后代才由根菜单作为结构 owner。所有 scope 组合后覆盖完整 entry 图，因此离线构造的树也不能通过 custom wrapper 绕过唯一性，同时纯 built-in 深树不会形成祖先对后代集合的重复订阅。协调器只弱订阅当前 scope 内可通知的 custom entry source；custom node 增删内置后代时立即重新协调，在加入时拒绝指回当前 built-in owner 或任意 built-in 祖先的动态环，并在移除后释放 owner。不可通知的自定义 enumerable 按每次所属集合或可通知 custom 祖先变化时取得的当前快照校验。

`NavMenuNode.Entries` 和 `NavMenuGroup.Entries` 是控件拥有的可写集合，单项 Add、Insert、Replace 以及批量初始化先完成类型、循环、直接 owner 和批次重复校验，失败时不产生部分写入。成功写入先同步完整 structural ownership，再调用自定义节点的 parent callback 或分组语义父级投影，最后发送集合通知；因此可重入回调和观察者都不能抢占已经属于本次写入的 entry 或内置后代。Replace 先提交新旧成员与结构所有权，再执行父关系投影；投影和通知期间拒绝重入修改同一集合。已提交 Replace 遇到回调异常时，继续完成其他父关系投影（包括嵌套分组）和后续通知，再向调用者传播异常。批量初始化一次提交完整批次，并只发送单次 Add 通知。根 direct `Items` 与任意外部 `ItemsSource` 都通过 post-mutation 的 `ItemsView.CollectionChanged` 到达控件；根 `NavMenu` 对完整 entry 图做校验并确定性抛错，但不通过重入 Remove/Replace 回滚或篡改数据源。Move 和 Remove 仍执行正常的 owner、父级、容器和生命周期协调。

`IsInlineCollapsed` 是唯一折叠状态源。NavMenu 不增加语义相反的 `Expanded` / `IsExpanded` 属性；调用方需要正向展开状态时，通过双向 binding converter 映射 `IsInlineCollapsed`，避免两个公共状态互相写回。

节点命令遵守 Avalonia 命令语义：`NavMenuNode` / `INavMenuNode` 只保存 `Command` 和 `CommandParameter`，实际执行、`CanExecute` 评估和 `CanExecuteChanged` 生命周期由生成出的 `NavMenuItem` 容器负责。`INavMenuNode` 为两个成员提供 `null` 默认实现，使既有自定义节点实现无需声明命令也能继续工作。`CommandParameter=null` 表示显式空参数，控件不能自动替换为 `ItemKey`；需要使用业务 key 时，应显式把 `ItemKey` 绑定或赋值给 `CommandParameter`。命令只由实际激活项执行，子项 Click 冒泡不能执行祖先命令。子菜单重新打开时刷新当前已生成的语义子项（穿透分组）的 `CanExecute`，使关闭期间的命令状态变化在重新打开时生效。容器按 UI 周期合并连续的 `CanExecuteChanged` 通知：持续的 `false` 仍进入 disabled，同一同步执行周期内的 `false -> true` 瞬时变化只投影最终状态，避免多个共享命令节点触发无业务意义的禁用颜色闪动。

`IsInlineCollapsed` 只对 `Mode=Inline` 生效。`Mode=Vertical` 或 `Mode=Horizontal` 时设置该属性不应改变当前模式的 popup、布局或键盘语义。`InlineCollapsedWidth` 参与布局测量，默认通过 theme setter 取得 `NavMenuToken.InlineCollapsedWidth`；collapsed 状态下由控件内部对 `Width` / `MinWidth` 做有效值 coercion，本地设置的属性值应按 Avalonia 属性优先级覆盖 token 默认值，展开后原始 `Width` 或绑定必须恢复。

键盘漫游状态属于内部交互状态，不进入公共 API。NavMenu 保持与 参考 Menu 一致的分层：`SelectedItem` 表示已提交的导航选择，`DefaultOpenPaths` / `IsSubMenuOpen` 表示展开状态，键盘当前项只表示临时 active/focus 目标。业务代码不应通过公开属性控制键盘 active 项，也不应把 active 项误认为已选择节点。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ItemsPresenter` | `ItemsPresenter` | 承载顶层 entry 容器。 |
| `PART_HeaderPresenter` | `ContentPresenter` | 承载根导航固定 Header；内容为空时折叠。 |
| `PART_FooterPresenter` | `ContentPresenter` | 承载根导航固定 Footer；内容为空或处于有效 inline collapsed 状态时折叠。 |
| `PART_HorizontalLine` | `PixelAlignedBorder` | Horizontal light style 下的底部分割线。 |
| `PART_Header` | `BaseNavMenuItemHeader` 的 mode 专用实现 | 菜单项 header，承载文字、图标、箭头和交互视觉。 |
| `PART_Popup` | `Popup` | `Vertical` / `Horizontal` 模式下的子菜单浮层。 |
| `PART_PopupFrame` | `NavMenuPopupFrame` | Popup 背景、圆角、尺寸约束和内容边距。 |
| `PART_ChildItemsViewport` | `ContentViewport` | `Inline` 模式下的子菜单展开收起 motion 容器。 |
| `PART_ChildItemsFrame` | `Border` | `Inline` 模式下的子菜单背景块。 |
| `ChildItemsPresenter` | `ItemsPresenter` | `Inline` 模式下的子菜单内容承载。 |
| `PART_ActiveIndicator` | `Rectangle` | `Horizontal` 顶层 light style 下的活动指示条。 |

### 3.1 Semantic Part 契约摘要

NavMenu 家族对应用公开基于 Selector 的 Semantic Part 契约，完整定义见
[NavMenu Semantic Part 契约](semantic-part.md)。Part 只描述与实现结构无关的稳定视觉职责，不替代 StyledProperty、
伪类、事件或 Token；AtomUI 默认主题不消费 `.semantic-*`。

上游基线为 6.6.3 稳定发布源码公开的 12 个 Semantic 键路径：一级 `root`、`item`、`itemIcon`、`itemContent`、
`itemTitle`、`list`，子菜单 `subMenu.item`、`subMenu.itemIcon`、`subMenu.itemContent`、`subMenu.itemTitle`、
`subMenu.list`，以及 `popup.root`。Part 名称逐字沿用这些键路径，不新增、不改名、不合并。

NavMenu 家族只有 `NavMenu` 一个 public owner：`NavMenuItem`、`NavMenuGroupItem`、`NavMenuDividerItem` 都是 internal
容器，不能持有 descriptor，也不能作为 `ContractType`（菜单项容器取其最近 public 基类 `HeaderedSelectingItemsControl`）。
因此不能像 `TreeView` / `TreeViewItem` 那样按递归 owner 拆分，全部 12 个键声明在 `NavMenu` 上。

关键边界：

- `root` 是隐式 Part，不声明 `.semantic-root` marker，也不生成 Style；通过 owner 属性、owner-scoped Style 或替换
  ControlTheme 定制。
- 层级靠**互斥 marker**区分，不靠 DOM class：一级菜单项容器 `.semantic-item`、子菜单容器 `.semantic-sub-menu-item`；
  一级分组容器 `.semantic-scope-group`、子菜单内分组 `.semantic-sub-menu-group`。协调器在 prepare 时先移除另一层级的
  类再补齐当前层级，保证容器转移与回收复用不残留。
- 全部非 root Part 的 route 以 `>>` 开头并按生成器语法声明 `CrossNestedOwners`：容器是运行时生成物，不在 `NavMenu`
  模板内，无法用 `/template/` 到达；`popup.root` 的节点还位于 `Popup.Child` 属性值子树，因此同时声明
  `CrossVisualRoot`。
- `popup.root` 的 `Cardinality` 为 `Multiple`；Inline 模式在视觉树内展开，模板没有弹层节点，该模式实例数为 0。
- `NavMenuGroupItem`、`NavMenuDividerItem` 与三个 `*NavMenuItemHeader` 不持有 descriptor；分组标题与分组列表归
  `NavMenu` owner，分隔线在上游没有 semantic 键。
- 根 Header / Footer、`PART_HorizontalLine`、`PART_ActiveIndicator`、inline collapsed 首字符节点与 inline 子菜单内容容器
  不开放为 Part。
- 用户入口是生成的强类型 Semantic Style（如 `NavMenuItemStyle`、`NavMenuSubMenuItemStyle`、`NavMenuPopupRootStyle`），
  在 AXAML 中作为 `atom|NavMenu` owner-scoped 普通 Style 的嵌套样式声明，并显式给出 `x:SetterTargetType`。

Part 的 Selector、ContractType、cardinality、marker 放置、数量语义、尺寸基线、定制边界与验证矩阵以
[NavMenu Semantic Part 契约](semantic-part.md) 为准。

## 4. 行为与状态模型

NavMenu 的交互行为由 mode 决定。所有 mode 共用项激活事务契约：指针按下只建立待提交事务并尝试移动真实焦点，合法释放（按下与释放命中同一项）才按固定顺序提交选择并派发命令与事件，其余中断路径一律取消。键盘 Enter/Space 与指针合法释放复用激活入口；程序化 `SelectedItem` 是独立的选择入口，只同步选择状态，不执行节点命令或触发 `NavMenuItemClick`。完整状态机、提交顺序、同步重入和取消路径见 [NavMenu 项激活事务设计](item-activation-design.md)。

`Inline` 模式：

- 合法释放带子菜单的项目时切换 `IsSubMenuOpen`。
- 子菜单在当前视觉树中展开，使用 `ContentViewport` 承载展开收起 motion。
- 合法释放叶子节点时提交选择，并更新所有祖先 `IsInSelectedPath`。
- `IsAccordionMode=true` 时，顶层子菜单互斥展开；Inline 模式下旧分支收起与新分支展开同时开始，高度连续变化，动画期间再次点击可从当前进度反向切换。
- `IsInlineCollapsed=true` 时，public `Mode` 仍保持 `Inline`，但内部有效模式切换为 vertical popup 语义：顶层只显示图标或无图标首字符，inline 子树不在主视觉树中展开，带子菜单的顶层项目通过 popup 打开。
- 有效 inline collapsed 状态下，顶层叶子节点通过实际 header control 承载 Tooltip。节点显式 `Tooltip` 优先；未设置时回退到 `Header`；菜单级或节点级 Tooltip 被禁用、节点拥有子菜单或退出有效折叠状态时，不创建有效提示内容。
- 进入折叠时缓存当前 inline 打开路径并关闭主视觉树中的 inline 子菜单；退出折叠时恢复缓存路径。折叠和展开不得清空 `SelectedItem` 或 selected path。
- 键盘 Up / Down 在当前可见层级内移动 active/focus 项。
- Enter 在带子菜单项上切换展开状态，在叶子节点上提交选择。
- Left / Right 可作为桌面增强支持折叠或展开当前 active 子菜单，并保持 keyboard active 在当前项；叶子项上为 no-op，不能改变 `SelectedItem`。

`Vertical` 与 `Horizontal` 模式：

- 带子菜单的项目通过 Popup 展开。
- hover 可以延迟打开子菜单；pointer 离开后延迟关闭。hover 打开流程独立于激活事务。
- 合法释放叶子节点时提交选择；弹出层关闭由 pointer、窗口失焦、非客户端点击和同级打开状态共同控制。这些关闭入口只结束临时 popup open state，不清空持续的 `SelectedItem` 或 selected path。
- `Horizontal` 顶层菜单 popup 位于下方；非顶层 popup 按右侧边缘对齐。
- 键盘导航以当前打开的可见菜单层级为边界移动 active/focus 项，跳过禁用项、分割线和不可聚焦内容。
- 键盘 active 初次移动时优先以当前可见且已生成的 `SelectedItem` 容器作为方向键锚点，并立即移动到前一个或后一个可导航节点；如果没有选中项，或选中项隐藏在未打开的子菜单中，则从第一个可导航节点开始。
- `Horizontal` 顶层菜单使用 Left / Right 在顶层兄弟项之间移动，Down 或 Enter 打开当前 active 子菜单并进入子菜单第一项。
- `Vertical` 根层和所有 popup 子菜单使用 Up / Down 在同层兄弟项之间移动，Right 或 Enter 打开当前 active 子菜单并进入子菜单第一项，Left 或 Esc 返回父级并关闭当前 popup 分支。
- Enter 在叶子节点上提交选择并触发 `NavMenuNodeSelected` / `NavMenuItemClick`；方向键只改变 active/focus，不触发选择。
- Esc 只关闭当前键盘导航所在的 popup 分支并返回父级 active 项，不调用 `Close()`，因此不能清空 `SelectedItem`。

公共交互状态：

- `Disabled` 由节点 `IsEnabled` 和 command can-execute 共同决定，禁用项不应触发有效点击。
- 节点命令必须复用 `NavMenuItem` 的有效激活入口；pointer 与 keyboard 提交不能形成两条独立命令执行路径，也不能因选择事件再次执行命令。
- `PointerOver` 改变 header 前景和背景，但不能改变选中路径。
- `PressCandidate` 表示激活事务的待提交项，建立后一直保留到提交或取消；只有指针仍命中该项时才贡献与 selected 相同的背景，文字颜色保持按下前状态，不影响选中状态。
- `KeyboardActive` 表示键盘漫游中的当前项，只影响键盘导航锚点和 active 视觉，不改变选中路径；pointer-hold 与 keyboard-active 由独立 owner 维护，可以同时存在。
- `Selected` 表示当前叶子节点已提交选中。
- `IsInSelectedPath` 表示某个祖先位于当前选中路径中。
- `Open` 表示当前项目子菜单打开。

结构 entry 不进入公共交互状态：分组和分隔线不产生 `Selected`、`KeyboardActive`、`Open`、`ItemKey` 或 `Command`。分组中的节点仍使用最近的节点祖先作为 `ParentNode`；分组本身不增加 `Level`，也不进入 `TreeNodePath`。

## 5. 视觉与主题模型

Inline 内容动效的共享设计要求稳定内容排版、完整高度裁剪、从当前帧反转及当前请求独占完成权；菜单选择、路径和
顶层互斥继续由 NavMenu 拥有。具体执行路径由本控件实现文档描述，根菜单宽度切换和 Popup 动效分别维护。
共享设计来源：`docs/architecture/systems/control-infrastructure/content-expansion.md`。

NavMenu Theme 按 mode、dark style、header state 和 item background model 分层。

```text
NavMenuTheme
  root template by Mode
  fixed Header / scrollable entry region / fixed Footer
  root background / padding / scroll behavior
  top-level ItemsPanel orientation

NavMenuItemTheme
  item template by Mode
  popup frame
  inline child frame
  motion duration

NavMenuGroupItemTheme / NavMenuDividerItemTheme
  non-interactive structure containers
  group title / divider orientation

Header Themes
  shared text/icon/background/selection
  horizontal active bar
  inline indentation and arrow rotation
```

Theme 映射规则：

- Root 背景使用 `ItemBg`，Dark root 背景使用 `DarkMenuBg`。
- Popup 背景使用 `MenuPopupBg`，Dark popup 使用 `DarkMenuPopupBg`。
- Header 默认背景为 `Transparent`，hover / selected 背景由 header state 直接控制。
- Keyboard active 通过 `IsKeyboardActive` / `ItemActiveBg` 表达；指针按住通过独立的 `IsPointerHold` 只使用与 selected 相同的 `ItemSelectedBg`（dark 使用 `DarkItemSelectedBg`），不覆盖既有文字颜色。两者优先级都低于真实 `Selected`，清除其中一个 owner 不得覆盖另一个 owner 的状态。keyboard active 可以叠加在 `IsInSelectedPath` 父节点上，使父节点保留 selected-path 文字色的同时显示临时 active 背景。header 背景使用 `MotionDurationSlow`（默认 300ms）与 `ItemBackgroundMotionEasing`（默认 CSS `ease` 等价曲线），使 hover 灰在按下后过渡到 selected 色，而 selection 只在合法释放时提交。
- Inline collapsed 根宽度使用 `InlineCollapsedWidth`，默认来自 `NavMenuToken.InlineCollapsedWidth=48`。折叠视觉只作用于 `Mode=Inline && IsInlineCollapsed=true`：一级 icon 使用 `CollapsedIconSize` 居中，标题和箭头收起，未配置 icon 的一级项从节点 `Header` 显示首字符；顶层叶子项使用独立 `Tooltip`，未设置时回退到 `Header`。
- Inline/Vertical 的 Header 和 Footer 位于菜单滚动区之外；无 Header/Footer 时对应 presenter 折叠，不占用布局空间。Horizontal 中 Header 左停靠、Footer 右停靠，菜单项占用中间区域。进入 inline collapsed 后 Header 保持可见以承载展开入口，Footer 自动隐藏；Header 内容需要根据 `IsInlineCollapsed` 自适应折叠宽度。
- 根层 inline collapsed 分组标题隐藏，分组及其透明嵌套分组内的节点继续继承根折叠状态，按顶层节点使用 `CollapsedIconSize` 居中；popup 或非根语义层级中的分组标题和节点保持普通 vertical 视觉。Horizontal 根层把分组渲染为透明水平集合并隐藏标题，popup 中恢复垂直分组标题。
- Horizontal 根层分隔线为竖线；Inline、Vertical、popup 和 inline collapsed 根层分隔线为横线。
- 根默认 ItemsPanel 通过 `TemplateBinding` 消费公开 `ItemSpacing`；submenu、popup 和 group 默认 ItemsPanel 消费由根控件投影的内部 effective spacing。该路径不使用进入子控件模板的 selector，也不建立逐容器 binding。自定义 ItemsPanel 是否消费 spacing 由自定义面板负责。
- `IsItemBackgroundEnabled=true` 时，inline child frame 使用 `SubMenuItemBg` / `DarkSubMenuItemBg`，并应用背景块专用外距。
- `IsItemBackgroundEnabled=false` 时，inline child frame 背景为 `Transparent`，不应用背景块专用外距；header 的文字色、hover、selected 和 selected path 仍然生效。
- Horizontal 顶层 light style 通过 `PART_ActiveIndicator` 表达选中；dark style 可以使用 selected background。

Header 背景与 NavMenuItem / inline submenu 背景块是不同职责，不应混为一个 selector 控制。

## 6. 控件家族或集成关系

NavMenu 位于 Desktop Navigation 分类，与 Breadcrumb、Pagination、Steps、TabControl 等控件同属导航体系，但不共享状态模型。

集成关系：

- `ItemsControl`：承载 `Items`、`ItemsSource`、`ItemTemplate` 和容器生成。
- AtomUI Token：通过 `NavMenuToken.ScopeProvider` 注册控件级资源。
- Popup/Overlay：`Vertical` 和 `Horizontal` 子菜单通过 `Popup` 展开，并由 `ShouldUseOverlayPopup` 控制 overlay 使用策略。
- Motion：Inline 子菜单使用内容布局能力与 Expand / Collapse；根宽度保持控件拥有的原生布局动画。
- Resource Host：`NavMenuNode` 使用 scoped resource-host generator 实现 `IResourceHost` / `IThemeVariantHost`，由当前 owner menu/container attach，并通过可释放 token 使 `Header`、`Tooltip` 等节点动态资源跟随菜单资源域。
- Group Resource Host：`NavMenuGroup` 使用相同 scoped resource-host 规则，使动态 `Header` 和 `HeaderTemplate` 跟随当前菜单资源域，并在结构容器回收时释放 attachment。
- Gallery：除 inline、vertical、horizontal、dark、items source、默认选中路径和默认展开路径外，最后一个结构化示例同时展示固定 Header/Footer、根分组、嵌套分组、分隔线和显式 `ItemSpacing`。

NavMenu 不实现 Form、CompactSpace 或 Button 家族接口。

## 7. 兼容性不变量

维护 NavMenu 时必须保持以下不变量：

- `NavMenuMode.Vertical`、`Horizontal`、`Inline` 的名称、默认行为和模板模式不变。
- `Mode` 默认值保持 `Inline`。
- `IsInlineCollapsed` 不引入新的 `NavMenuMode`，也不直接改写 `Mode`；折叠只通过内部 effective mode、theme state 和 popup 交互表达。
- `InlineCollapsedWidth` 默认由 `NavMenuToken.InlineCollapsedWidth` 提供，开发者本地设置必须能覆盖 token 默认值。
- `SelectedItem` 优先级高于 `DefaultSelectedPath`。
- `DefaultOpenPaths` 和 `DefaultSelectedPath` 不依赖固定时间延迟。
- pointer 外点、窗口停用、平台失焦、非客户端点击和 `Mode` 切换只关闭 popup/submenu，不得通过 `Close()` 隐式清空 `SelectedItem`；显式调用 public `Close()` 仍保持“关闭全部子菜单并清空选择”的既有合同。
- 进入或退出 inline collapsed 不得调用 `Close()`，不得清空 `SelectedItem`，不得丢失 selected path。
- inline collapsed 期间打开的 popup 状态不得污染展开后恢复的 inline open path cache。
- 键盘 active/focus 状态不得进入公共 API，不得改变 `SelectedItem`、`DefaultSelectedPath` 或 `DefaultOpenPaths` 的语义。
- `NavMenuNode` / `INavMenuNode` 的 `Header`、`HeaderTemplate`、`Tooltip`、`IsTooltipEnabled`、`ItemKey`、`Icon`、`IsEnabled`、`Command`、`CommandParameter`、`Children` 名称、类型和语义不变。
- `Tooltip=null` 必须回退到节点 `Header`；折叠提示只作用于有效 inline collapsed 状态下的顶层叶子节点，不能扩展到带子菜单节点、普通 Vertical/Horizontal 或展开后的 Inline 状态。
- `NavMenuNode.Entries` 是子 entry 唯一真源；`Children` 只能作为同一集合的实时节点兼容视图，不能引入第二份节点集合或双向同步状态。
- direct `Items`、`ItemsSource`、节点 `Entries` 和分组 `Entries` 对非法 entry 的拒绝语义一致；不能因 source 是否只读或集合通知类型不同而绕过验证。
- 同一内置 `NavMenuNode` / `NavMenuGroup` 实例在 entry 树中只能有一个直接结构 owner；释放 owner 后才允许重挂载。无状态 `NavMenuDivider` 可以复用。
- custom `INavMenuNode` 本身保持兼容，但它暴露的内置 node/group 仍必须参加完整 entry 图唯一性校验；嵌套可通知 source 使用弱订阅。
- `NavMenuGroup` 和 `NavMenuDivider` 在任意数据层级都保持结构语义，不进入选择、命令、路径、层级缩进或键盘状态。
- 根分组中的节点仍为顶层节点；嵌套分组不能改变节点的 `ParentNode`、`Level` 或 `IsTopLevel`。
- `Header` / `Footer` 固定区域不能进入菜单 ItemsPanel 或随菜单项滚动；空 content 不得改变既有无 slot 布局。
- `ItemSpacing` 只控制根默认 ItemsPanel，并在具有有效设置时覆盖后代默认 ItemsPanel 的额外容器间距；未设置的 Horizontal 根层保持 `0`，其 popup、submenu 和 group 仍使用 `VerticalItemsPanelSpacing`。该属性不重定义 `ItemContentMargin`、`VerticalChildItemsMargin` 或自定义 ItemsPanel 的布局语义。
- `NavMenuNode` 只承载命令配置，不实现 `ICommandSource`，不直接订阅 `CanExecuteChanged`，也不保存当前 `NavMenuItem` 容器。
- `CommandParameter` 保持标准显式参数语义，不隐式回退到 `ItemKey`、`Header`、`SelectedItem` 或节点自身。
- `NavMenuItemClick` 和 `NavMenuNodeSelected` 的事件语义不变。
- `SelectedItem` 只表示已提交选择；指针按下不改变选择、不执行命令、不触发 `NavMenuItemClick` 或 `NavMenuNodeSelected`。
- 指针激活只有"合法释放提交"一种行为：按下与释放命中同一项才提交；释放到其他节点、菜单外以及激活事务的全部取消路径均不产生选择、命令或事件。
- 叶子提交顺序固定为：选中路径与 `IsSelected` 更新、`SelectedItem` 更新、`NavMenuNodeSelected`、节点 `Command`、`NavMenuItemClick`。`NavMenuNodeSelected` 开始派发前，`SelectedItem` 必须仍指向该事件节点；若同步观察者或事件处理器改写选择，原节点提交被视为 superseded，并停止尚未发生的事件或动作。父节点提交不修改 `SelectedItem`、不触发 `NavMenuNodeSelected`。
- 键盘 Enter/Space 与指针合法释放必须共用同一提交入口和事件顺序。
- pointer-hold 的背景必须与 selected 完全一致、文字颜色保持不变，且不得提前写入 selected 状态。按下先交接 pointer-hold 再捕获带 `Cursor=Hand` 的 header；合法释放时保留 pointer-hold 直到 selection 提交完成，再清除临时状态，过程不得闪回 hover、透明或默认背景。
- 方向键移动 active 项不得触发 `NavMenuItemClick` 或 `NavMenuNodeSelected`。
- Esc 关闭 popup 分支不得调用 `Close()`，不得清空已选中节点。
- `IsAccordionMode=true` 只控制同层展开互斥，不改变选中节点。
- `IsItemBackgroundEnabled=false` 不应关闭 header 前景色、hover、selected、selected path 或 disabled 视觉，只关闭 item / submenu 背景块。
- inline 子菜单背景块外距只在 `IsItemBackgroundEnabled=true` 时生效。
- popup frame 使用 `MenuPopupBg` / `DarkMenuPopupBg`，不回退为普通 shared elevated background。
- root background、popup background、header background 和 inline submenu background 必须保持职责分离。
- 点击子节点时，不应让父级 header 出现错误 hover 背景。
- NavMenu 优化不得关闭 motion 来规避点击、打开或关闭问题。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 Mode 模型

`Mode` 同时影响模板结构、交互 handler、popup 策略、ItemsPanel 方向和 header 主题。`IsInlineCollapsed` 是 `Inline` 模式的附加状态，不是第四种 mode。

| Mode | 子菜单承载 | 顶层排列 |
| --- | --- | --- |
| `Inline` | 视觉树内 `ContentViewport` | Vertical StackPanel |
| `Vertical` | Popup | Vertical StackPanel |
| `Horizontal` | Popup | Horizontal StackPanel |

有效模式按以下规则计算：

| Public state | Effective mode | 子菜单承载 |
| --- | --- | --- |
| `Mode=Inline, IsInlineCollapsed=false` | `Inline` | inline child frame |
| `Mode=Inline, IsInlineCollapsed=true` | `Vertical` | popup |
| `Mode=Vertical` | `Vertical` | popup |
| `Mode=Horizontal` | `Horizontal` | popup |

### 8.2 Inline Collapsed 模型

Inline collapsed 模型对齐 参考 Menu 的 `inlineCollapsed`：公开模式仍为 `Inline`，折叠状态只改变内部有效交互和视觉。折叠菜单宽度使用 `InlineCollapsedWidth`，默认来自 `NavMenuToken.InlineCollapsedWidth=48`；开发者可在控件实例上设置 `InlineCollapsedWidth` 获得更窄或更宽的折叠侧栏。

进入折叠时，NavMenu 记录当前已经打开的 inline path，然后关闭主视觉树中的 inline 子菜单，使根菜单只保留顶层项。带子菜单的顶层项在折叠状态下按 popup 子菜单打开；popup 打开关闭只属于折叠期间的临时交互，不写回 inline path cache。

退出折叠时，NavMenu 关闭折叠期间打开的 popup，并恢复折叠前缓存的 inline open path。`SelectedItem` 和 `IsInSelectedPath` 在折叠和展开之间保持稳定；如果初始加载时已经处于折叠状态，`DefaultOpenPaths` 应进入 inline path cache，等展开后再恢复到 inline 子树。

折叠视觉只应用于顶层项：一级 icon 居中并使用 `CollapsedIconSize`，标题和展开箭头收起；没有 icon 的一级项显示标题首字符。非顶层项只出现在 popup 中，继续使用 vertical popup 的正常文字、icon、箭头和宽度语义。

### 8.3 Entry Composition 模型

NavMenu 的有序结构由三类 entry 组成：

```text
INavMenuEntry
  INavMenuNode       interactive navigation entry
  NavMenuGroup       transparent structural owner
  NavMenuDivider     non-interactive separator
```

分组可以出现在根 `Items`、任意 `NavMenuNode.Entries`、popup 子菜单或另一个分组中。分组只改变视觉组合，不改变导航语义树。例如：

```text
Administration node
  Users group
    Members node
```

`Members.ParentNode` 仍是 `Administration`，其 `Level` 只比 `Administration` 增加一级。根分组中的节点保持 `ParentNode=null` 和 `IsTopLevel=true`。选择路径、打开路径、默认路径 replay、Accordion 同层判断和键盘同层遍历都忽略结构 entry。

`Entries` 保留完整结构顺序；`Children` 提供不分配第二份存储的节点视图。结构集合的 Add、Remove、Replace、Move、Reset 必须保持最近节点父级关系，移动分组不得把组内节点临时暴露为额外层级。

### 8.4 Selection 与 Path 模型

`TreeNodePath` 通过 `ItemKey` 定位节点路径。路径 replay 先打开中间节点，再选中叶子节点。`SelectedItem` 是持续选择状态，`DefaultSelectedPath` 是默认选择入口。两者同时存在时，`SelectedItem` 生效。

### 8.5 项激活事务模型

菜单项激活是同步、有序且可被重入替换的用户操作事务，完整契约见 [NavMenu 项激活事务设计](item-activation-design.md)：

| 输入入口 | 事务建立 | 提交 |
| --- | --- | --- |
| 指针 | 按下：记录待提交项、显示 selected-background pressed 视觉且保持文字颜色、捕获指针、移动焦点。 | 合法释放：释放点命中待提交项。 |
| 键盘 | 方向键移动 active 项。 | Enter / Space。 |
| 程序化选择 | — | `SelectedItem` 赋值只进入选择协调器，不属于项激活。 |

两个用户输入入口汇入同一激活管线；程序化选择只复用选择协调器。取消路径（拖离释放、当前捕获指针丢失、节点失效、非主按钮释放等）全部无副作用。pointer-hold 与 keyboard-active 独立持有，并分别投影到 header 的 selected-background 与 active 视觉。

### 8.6 Keyboard Navigation 模型

NavMenu 的键盘导航模型与选择模型分离：

| 状态 | 职责 | 是否公开 |
| --- | --- | --- |
| Keyboard active item | 当前键盘漫游和 focus 目标。 | 否 |
| Open item path | 当前已展开的 inline / popup 分支。 | 仅通过现有打开行为间接体现 |
| Selected item | 已提交的导航节点。 | 是，`SelectedItem` |

键盘导航只遍历当前可见且可交互的 `NavMenuItem`。禁用项、分割线、隐藏 popup 内容、尚未展开的 inline 子项和非菜单项内容不进入漫游序列。Vertical、Horizontal 和 inline collapsed 的 popup 层级中，打开子菜单时 active 项进入该子菜单的第一个可交互子项；关闭子菜单时，active 项回到父级触发项。Inline 展开态的 Left / Right 只控制当前 active 子菜单展开收起，不移动 active 项。

当 keyboard active 尚未初始化时，NavMenu 先尝试把当前 `SelectedItem` 对应的可见容器作为方向键移动锚点；第一次 Up / Down 应直接移动到选中项前一个或后一个可导航节点，而不是把 active 停在选中项本身。如果当前没有选中项，或选中项所在分支尚未展开、容器不可见，则回退到第一个可导航节点。这个初始化不会触发新的选择事件，也不会自动打开隐藏分支。

键盘提交遵循“浏览和提交分离”：Up / Down / Left / Right 只移动 active/focus 或打开/关闭层级，Enter 才能提交叶子节点选择。带子菜单项的 Enter 优先执行展开或进入子菜单，不直接选中父节点。

在 inline collapsed 状态下，键盘导航使用 effective vertical 模型：根层 Up / Down 在顶层项之间移动，Right 或 Enter 打开 active 子菜单 popup 并进入第一项，Left 或 Esc 关闭当前 popup 分支并回到父项。方向键仍不得触发选择事件。

### 8.7 Node Command 模型

节点命令采用“节点配置、容器执行”的模型：

```text
NavMenuNode / INavMenuNode
  Command + CommandParameter
      ↓ scoped container binding
NavMenuItem
  ICommandSource
  CanExecute / CanExecuteChanged
      ↓ effective click or keyboard commit
Command.Execute(CommandParameter)
```

该模型保持数据节点和交互容器职责分离：节点可以作为 Avalonia binding target 保存命令配置，但不能自行实现点击、焦点、键盘或有效禁用状态。`NavMenuItem` 是唯一命令执行入口，并把 `CanExecute=false` 投射为 effective disabled；该状态不能反向改写节点的 `IsEnabled`。

命令能力必须与节点资源和容器生命周期同时成立：`NavMenuNode` 的动态资源由 generated scoped resource host 管理；节点到 `NavMenuItem` 的 `Command` / `CommandParameter` 同步进入当前容器 disposable；`NavMenuItem` 对 `ICommand.CanExecuteChanged` 的订阅在命令替换和 logical-tree detach 时解除。只实现其中一层不能视为完整生命周期。

### 8.8 Item Background 模型

`IsItemBackgroundEnabled` 控制背景块，不控制 header 文本状态。该模型要求 `NavMenuItem` 背景和 `NavMenuItemHeader` 背景分离，不能通过禁用 header selector 来实现无背景模式。

### 8.9 Popup 模型

`Vertical`、`Horizontal` 和 inline collapsed 子菜单使用同一 popup shell。顶层 horizontal popup 放置在底部，非顶层、vertical 和 inline collapsed popup 使用右侧对齐。Popup 内容宽度、最大高度、背景、圆角和内边距由 NavMenuToken 和 PopupHostToken 共同决定。

### 8.10 内容展开动效模型

Inline 子菜单以 `IsSubMenuOpen` 为目标状态，根菜单在请求阶段完成顶层互斥。内容视口的高度与透明度使用同一进度，
内部 `PART_ChildItemsFrame` 的间距包含在完整内容尺寸内。等高分支切换保持内容总占位稳定，不等高分支在两种稳定
高度之间连续交换空间；动画反转接续当前已呈现值，过期请求不发送 `SubmenuOpened` / `SubmenuClosed`。

Core 共用机制的动画执行器、尺寸测量和裁剪只承担内部呈现职责，不新增公共动效 API，也不接管 `SelectedItem`、嵌套展开状态、
键盘导航或业务命令。嵌套子菜单改变内容尺寸时，外层自然测量需要失效，不能因临时固定高度长期裁掉内层内容。
算法与生命周期约束统一引用 [内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)。

## 9. 文档导航与验证策略

关联文档：

- [NavMenu 桌面版实现原理](implementation.md)
- [NavMenu 项激活事务设计](item-activation-design.md)
- [NavMenu Semantic Part 契约](semantic-part.md)
- [内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)
- [NavMenu Token 设计](token.md)
- [NavMenu Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| 文档 | `overview.md`、`implementation.md`、`item-activation-design.md`、`token.md`、`changelog.md` 链接有效。 |
| Public API | `NavMenu`、`NavMenuNode`、`INavMenuNode`、`INavMenu`、事件参数与文档一致。 |
| Entry API | `INavMenuEntry`、`NavMenuGroup`、`NavMenuDivider`、`Entries` 和 `Children` 实时兼容视图与文档一致。 |
| Entry source | direct `Items`、`ItemsSource`、节点 `Entries` 和分组 `Entries` 的初始装载、source replacement、Add、Replace、Reset 校验一致，非法项在容器生成前确定性失败。 |
| Pointer activation | 按下零提交副作用、背景与 selected 一致且文字颜色不变、合法释放单次提交、完整提交顺序、同步重入 superseded policy、无中间背景回退、捕获指针身份、handler 替换，以及 pointer-hold / keyboard-active 独立所有权测试覆盖。 |
| Mode 行为 | Inline、Vertical、Horizontal 的打开、关闭、选中、默认路径和 popup 逻辑稳定。 |
| Inline collapsed | `IsInlineCollapsed` 切换、`InlineCollapsedWidth` 覆盖、open path cache、popup 临时打开、selected path 保持和初始 `DefaultOpenPaths` 恢复稳定。 |
| Keyboard | Up、Down、Left、Right、Enter、Esc 在 Inline、Vertical、Horizontal 中的 active、focus、open、close 和 commit 语义稳定；container recycle 或 popup close 后不得提交失效容器。 |
| Structure | 任意层级分组、分隔线、root/popup/collapsed 视觉和语义透明层级稳定，结构 entry 不进入选择或键盘漫游。 |
| Selection | `SelectedItem`、`DefaultSelectedPath`、`DefaultOpenPaths`、stale replay 和 clear selection 测试覆盖。 |
| Command | `Command` / `CommandParameter` 投影、pointer / keyboard 单次执行、`CanExecute` disabled、命令替换解绑、container recycle、re-template、Items reset 和页面释放测试覆盖。 |
| Resource lifecycle | `NavMenuNode` / `NavMenuGroup` generated scoped resource host、owner resource 优先级、repeated attach、attach token 释放、container recycle 交互状态失效和 WeakReference 测试覆盖。 |
| AXAML | Template part 名称、header theme、popup frame、inline child frame、active indicator 和 item background selector 稳定。 |
| Semantic Part | `NavMenu` 与 `NavMenuItem` descriptor 的 Part 名称、Selector、route、ContractType、cardinality 与语义部件契约一致；三种 mode 与 inline collapsed 下 marker 可达且数量正确；容器 prepare / clear / recycle 与 re-template 不重复添加或丢失 marker；用户 Semantic Style 通过专用生成 Style 命中，默认主题不消费 `.semantic-*`。 |
| Layout | root item margin、inline child gap、popup item inset、background-enabled true/false gap 与 参考设计体系 规则一致。 |
| Root slots / spacing | Header/Footer 固定区域、空 slot 退化、Horizontal 左右布局和 `ItemSpacing` 在 root、submenu、group 中一致。 |
| Token | `NavMenuToken` 默认值、`InlineCollapsedWidth`、dark token、popup token 和 spacing token 与测试一致。 |
| Gallery | 运行 Navigation/Menu Showcase 相关测试，确认示例结构和 CaseNavigation 布局稳定。 |
