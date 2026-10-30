# Menu 桌面版架构设计

本文档定义 `Menu` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Menu 桌面版实现原理](implementation.md)，弹层滚动专项设计见 [Menu 弹层滚动模式设计](popup-scroll-design.md)，Menu Token 的专项设计见 [Menu Token 设计](token.md)，设计和契约变化记录见 [Menu Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `Menu` 与 `MenuFlyout` 各自的具体控件：`Menu.IsPopupPinnedOpen` 是公开属性（与 `NavMenu`、`Tour`、`DropdownButton` 等家族约定一致），因为跨程序集的 Gallery 语义预览需要在 AXAML 中声明钉住；`MenuFlyout` 一侧的钉住入口保持 internal。设置为 true 时保持 menu open state 并 relay 到子菜单 Popup，设置为 false 时只解除关闭拦截。声明式钉住在容器生成之前写入时，`Menu` 在容器 prepare 阶段按同一份请求补齐，晚到的容器同样被钉住。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/Menu` |
| 控件状态 | Stable |

Menu 是 AtomUI 桌面控件体系中的菜单控件家族，用于组织命令列表、上下文操作、MenuFlyout 和数据驱动菜单项。

Menu 不负责页面级导航树、树形选择或任意弹层宿主。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Menu`

## 2. 设计语言

Menu 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Menu 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Menu 是 AtomUI 桌面控件体系中的菜单控件家族，用于组织命令列表、上下文操作、MenuFlyout 和数据驱动菜单项。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Items`、`MenuItem`、`MenuItemData` 和 `MenuSeparatorData`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、collection/filter、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Menu Token + ControlTheme。 |

## 3. API 与契约模型

Menu 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Items`、`MenuItem`、`MenuItemData`、`MenuSeparatorData` | 定义菜单项集合、数据驱动菜单项和分割项入口。 |
| 选择与集合 | `DisplayPageSize`、`IsScrollEnabled` | 维护弹层显示页数上限、滚动开关、选择、展开和集合状态。 |
| 交互与状态 | `IsMotionEnabled`、`ShouldUseOverlayPopup` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `LineWidth`、`Orientation`、`OverlayHostShadow`、`PopupRootShadow`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 动效与异步 | `CloseMotion`、`MotionDuration`、`OpenMotion` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |

稳定事件包括 `IsCheckStateChanged`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`ContextMenu`、`DefaultMenuInteractionHandler`、`FlyoutMenuItemClickedEventArgs`、`Menu`、`MenuFlyout`、`MenuFlyoutPresenter`、`MenuItem`、`MenuItemData`、`MenuSeparator`、`MenuSeparatorData`、`ToggleItemsLayoutVisibleConverter`。
- 枚举：无。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ItemsPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_Popup` | `?` | 承载弹层宿主、打开关闭或候选内容。 |
| `PART_ToggleCheckbox` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ToggleRadio` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

控件专属或内部伪类包括 `MenuItemPseudoClass.TopLevel`、`TopLevel=:toplevel`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

### 3.1 Semantic Part 契约摘要

Menu 家族对应用公开基于 Selector 的 Semantic Part 契约，完整定义见 [Menu Semantic Part 契约](semantic-part.md)。Part 只描述
与实现结构无关的稳定视觉职责，不替代 StyledProperty、伪类、事件或 Token；AtomUI 默认主题不消费 `.semantic-*`。

上游基线为 6.6.3 稳定发布源码公开的 12 个 Semantic 键路径，与 NavMenu 逐字相同：一级 `root`、`item`、`itemIcon`、
`itemContent`、`itemTitle`、`list`，子菜单 `subMenu.item`、`subMenu.itemIcon`、`subMenu.itemContent`、
`subMenu.itemTitle`、`subMenu.list`，以及 `popup.root`。Part 名称逐字沿用这些键路径，不新增、不改名、不合并。

关键边界：

- `Menu` 是 plain Menu 语义的唯一 owner；`MenuItem`、`MenuItemGroup`、`MenuSeparator` 虽是 public 类型，但被
  ContextMenu / MenuFlyout / DropdownButton 弹层复用，因此不持有 descriptor。
- 层级靠**互斥 marker**区分：一级容器 `.semantic-item` / 分组 `.semantic-scope-group`，子菜单容器
  `.semantic-sub-menu-item` / 分组 `.semantic-sub-menu-group`。层级只在 plain Menu 子树内由 `Menu` / `MenuItem` /
  `MenuItemGroup` 下发，复用方保持既有 `.semantic-item` 行为不变。
- 全部非 root Part 的 route 以 `>>` 开头并按生成器语法声明 `CrossNestedOwners`：容器是运行时生成物；`popup.root` 与
  全部 `subMenu.*` 位于 `Popup.Child` 属性值子树，因此同时声明 `CrossVisualRoot`。
- `itemTitle` / `list` 是分组标题与分组列表。`Menu` 是菜单栏（horizontal 语义），一级 `itemTitle` / `list` 恒为 0 实例
  （与上游 horizontal 一致）；子菜单内分组提供 `subMenu.itemTitle` / `subMenu.list`。
- 用户入口是生成的强类型 Semantic Style（如 `MenuItemStyle`、`MenuSubMenuItemStyle`、`MenuPopupRootStyle`），在 AXAML 中
  作为 `atom|Menu` owner-scoped 普通 Style 的嵌套样式声明，并显式给出 `x:SetterTargetType`。

## 4. 行为与状态模型

Menu 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- open/close、collection/filter、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `IsScrollEnabled` 控制弹层内容是否创建 `ScrollViewer`。滚动开启时 `DisplayPageSize` 参与最大高度计算；滚动禁用时弹层直接显示全部菜单项，不使用 `DisplayPageSize` 限高。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

子菜单的 pointer 交互采用独立的 hover intent 模型：

- `SelectedItem` 表达菜单导航和当前项状态，`IsSubMenuOpen` 表达已提交的弹层状态；二者都不能作为延迟任务是否仍然有效的唯一依据。
- pointer 进入带子菜单的非顶层项时，只为当前目标建立延迟打开意图。pointer 在延迟完成前离开该项时，打开意图立即失效，子菜单不得在离开后继续弹出。
- 已打开子菜单的关闭延迟只用于允许 pointer 从父项移动到其弹层。pointer 重新进入父项、子菜单弹层或其后代项时，待执行的关闭意图必须失效。
- 同一目标不能同时持有互相矛盾的打开和关闭意图。不同兄弟项切换时可以同时存在“关闭旧项”和“打开新项”，但每类意图最多只有一个当前目标。
- keyboard、access key 和 pointer press 触发的显式打开不经过 hover 延迟，不得被旧 hover callback 覆盖或回滚。
- 菜单关闭、窗口失活、宿主解除连接或交互处理器 detach 时，所有未完成 hover intent 必须统一失效。

## 5. 视觉与主题模型

Menu 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `ContextMenuTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `MenuItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `MenuSeparatorTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `MenuTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TopLevelMenuItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `src/AtomUI.Desktop.Controls/Flyouts/Themes/MenuFlyoutPresenterTheme.axaml` | 定义 `MenuFlyout` 菜单项 presenter 的弹层内容模板和滚动承载结构。 |

Menu 使用 `MenuToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close、collection/filter、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 弹层滚动开关通过内部 `MenuPopupScrollHost` 复用模板分支；禁用滚动时不能保留隐藏或禁用状态的 `ScrollViewer`。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Menu 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `ContextMenu`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `DefaultMenuInteractionHandler`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `Menu`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuFlyout`：用户配置 flyout 菜单内容和 presenter 选项的入口。
- `MenuFlyoutPresenter`：`MenuFlyout` 的实际菜单项 presenter，复用 Menu 家族滚动、动效和 overlay 语义。
- `MenuItem`：集合项、节点或容器类型，承载单项状态和模板协作。
- `MenuItemData`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `MenuPopupScrollHost`：internal 模板宿主，集中表达有滚动和无滚动两种弹层内容分支。
- `MenuItemTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `MenuSeparator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuSeparatorData`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `MenuToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `ToggleItemsLayoutVisibleConverter`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。
- `Menu`、`ContextMenu` 和默认 `MenuFlyoutPresenter` 必须共享 AtomUI 的子菜单 hover intent 语义，不能分别依赖行为不同的默认处理器。
- `NavMenu` 使用独立的导航菜单交互处理器，不与 Menu 家族共享选择状态，但延迟任务同样遵守可取消和 owner 释放原则。
- 用户显式注入 `IMenuInteractionHandler` 时，由该处理器负责自身的 pointer intent、定时任务和 attach/detach 生命周期；AtomUI 不在控件外再叠加第二套延迟状态。

## 7. 兼容性不变量

维护 Menu 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- `IsScrollEnabled` 默认值必须保持为 `true`；滚动禁用时视觉树中不得创建 `ScrollViewer`，也不得继续按 `DisplayPageSize` 限制弹层高度。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不把 `SelectedItem`、`IsSubMenuOpen` 或一次 callback 内的 pointer 判断当作 hover intent 的替代状态；延迟任务必须有明确 owner、目标身份和失效边界。
- 修复 hover 行为不得新增 public/protected API，也不得改变 `DefaultMenuInteractionHandler` 已公开类型和构造函数契约。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 弹层与宿主模型

Menu 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

子菜单 hover intent 的稳定状态转换如下：

```text
Idle
  -> PendingOpen(target)       pointer 进入带子菜单项
PendingOpen(target)
  -> Idle                      pointer 在打开前离开、菜单关闭或 handler detach
  -> Open(target)              延迟完成且 target/owner 仍有效
Open(target)
  -> PendingClose(target)      pointer 离开父项且未进入子菜单弹层
PendingClose(target)
  -> Open(target)              pointer 重新进入父项、弹层或后代项
  -> Idle                      延迟完成且 target/owner 仍有效，关闭子菜单
```

兄弟项切换时，旧项的 `PendingClose` 与新项的 `PendingOpen` 可以并存；目标身份变化、菜单关闭和生命周期结束必须使旧 callback 无法提交状态。Popup 动效只能表现已经提交的 open/close 状态，不能承担 hover intent 的取消或排序职责。

### 8.2 集合与数据同步模型

Menu 的集合状态必须能处理 source replace、reset、clear 和 container recycle。业务数据对象不应反向持有视觉对象，虚拟化或懒创建路径必须在容器回收时清理旧状态。

### 8.3 动效模型

Menu 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.4 视觉选项模型

Menu 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

### 8.5 弹层滚动模式

Menu 家族弹层滚动模式由 `IsScrollEnabled` 和 `DisplayPageSize` 共同表达。`IsScrollEnabled=true` 时，弹层内容使用 `ScrollViewer`，`DisplayPageSize` 按 item height 和 popup padding 计算最大高度；`IsScrollEnabled=false` 时，弹层内容直接承载 `PART_ItemsPresenter`，不创建 `ScrollViewer`，不使用 `DisplayPageSize` 限高。

`Menu`、`ContextMenu`、`MenuItem`、`MenuFlyout` 和 `MenuFlyoutPresenter` 共享同一公共语义。详细 API、模板、算法和验证边界见 [Menu 弹层滚动模式设计](popup-scroll-design.md)。

## 9. 文档导航与验证策略

关联文档：

- [Menu 桌面版实现原理](implementation.md)
- [Menu Semantic Part 契约](semantic-part.md)
- [Menu 弹层滚动模式设计](popup-scroll-design.md)
- [Menu Token 设计](token.md)
- [Menu Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、collection/filter、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
| Semantic Part | `Menu` descriptor 的 Part 名称、Selector、route、ContractType、cardinality 与契约一致；一级 / 子菜单层级 marker 互斥且数量正确；容器 prepare / recycle 与 re-template 不重复添加或丢失 marker；复用方 marker 行为不变；用户 Semantic Style 通过专用生成 Style 命中，默认主题不消费 `.semantic-*`。 |
