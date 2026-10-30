# InfoFlyout 桌面版架构设计

本文档定义 `InfoFlyout` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [InfoFlyout 桌面版实现原理](implementation.md)，InfoFlyout Token 的专项设计见 [InfoFlyout Token 设计](token.md)，设计和契约变化记录见 [InfoFlyout Changelog](changelog.md)。

该控件的 Popup 钉住打开属于共享弹层契约，详见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。本控件的语义 owner 为 `FlyoutHost`，其 `IsPopupPinnedOpen` 已公开（先例：AbstractColorPicker），供测试诊断与 Gallery 语义预览钉住弹层使用；设置为 true 时保持 Flyout open state 并 relay 到 Flyout Popup，设置为 false 时只解除关闭拦截。控件卸载、锚点失效、TopLevel 改变和模板重建仍按共享生命周期规则清理。

InfoFlyout 公开 5 个 Semantic Part（`root`、`popup.root`、`popup.container`、`popup.content`、`popup.arrow`），与 Ant Design Popover 的 Semantic Part 语义对齐（上游 `title` 槽位因 InfoFlyout 无标题节点而省略）；完整 Part 表、存在条件、Selector 用法与定制边界见 [InfoFlyout Semantic Part 契约](semantic-part.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/InfoFlyout` |
| 控件状态 | Stable |

InfoFlyout 是 AtomUI 桌面控件体系中的信息浮出层家族，用于从目标元素弹出普通内容、菜单内容或树形内容。

InfoFlyout 不负责 Dialog 模态系统、Tooltip 简短提示或导航菜单本体。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Flyouts`

## 2. 设计语言

InfoFlyout 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | InfoFlyout 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | InfoFlyout 是 AtomUI 桌面控件体系中的信息浮出层家族，用于从目标元素弹出普通内容、菜单内容或树形内容。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Content`、`ItemContainerTheme`、`ItemTemplate`、`ItemsSource`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、collection/filter、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | InfoFlyout Token + ControlTheme。 |

## 3. API 与契约模型

InfoFlyout 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Content`、`ItemContainerTheme`、`ItemTemplate`、`ItemsSource` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `DisplayPageSize` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsArrowVisible`、`IsLightDismissEnabled`、`IsMotionEnabled`、`IsPointAtCenter`、`ShouldUseOverlayPopup` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `ArrowPosition`、`MarginToAnchor`、`OverlayHostShadow`、`Placement`、`PlacementAnchor`、`PlacementGravity`、`PopupRootShadow`、`RequestedPlacement`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 弹层与窗口 | `Flyout`、`FlyoutPresenterTheme`、`IsPopupPinnedOpen` | 控制 popup、flyout、dialog、window 或 overlay 宿主协作。 |
| 动效与异步 | `CloseMotion`、`MotionDuration`、`MouseEnterDelay`、`MouseLeaveDelay`、`OpenMotion` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |
| 其他稳定入口 | `AnchorTarget`、`Trigger`、`TriggerType` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

稳定事件包括 `MenuItemClicked`、`TreeViewItemClicked`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`Flyout`、`FlyoutHost`、`FlyoutMenuItemClickedEventArgs`、`FlyoutPresenter`、`FlyoutStateHelper`、`FlyoutTreeViewItemClickedEventArgs`、`MenuFlyout`、`MenuFlyoutPresenter`、`TreeViewFlyout`、`TreeViewFlyoutPresenter`。
- 枚举：`FlyoutTriggerType`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ArrowDecorator` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_ContentPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_ItemsPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

InfoFlyout 的状态流按以下路径收敛：

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
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

InfoFlyout 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `FlyoutHostTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |
| `MenuFlyoutPresenterTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |
| `TreeViewFlyoutPresenterTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |

InfoFlyout 使用 `FlyoutHostToken`、`TreeFlyoutToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close、collection/filter、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

InfoFlyout 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Flyout`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `FlyoutHost`：模板协作类型，承载内容展示、宿主或视觉边界。
- `FlyoutHostToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `FlyoutPresenter`：模板协作类型，承载内容展示、宿主或视觉边界。
- `FlyoutStateHelper`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuFlyout`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MenuFlyoutPresenter`：模板协作类型，承载内容展示、宿主或视觉边界。
- `MenuFlyoutPresenterTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `TreeFlyoutToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `TreeViewFlyout`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TreeViewFlyoutPresenter`：模板协作类型，承载内容展示、宿主或视觉边界。
- `TreeViewFlyoutPresenterTheme`：ControlTheme 类型入口，连接主题资源和控件类型。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 InfoFlyout 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 不破坏已发布的 Semantic Part 名称、selector class / route、ContractType 与数量语义（见 [Semantic Part 契约](semantic-part.md)）。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 弹层与宿主模型

InfoFlyout 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

### 8.2 集合与数据同步模型

InfoFlyout 的集合状态必须能处理 source replace、reset、clear 和 container recycle。业务数据对象不应反向持有视觉对象，虚拟化或懒创建路径必须在容器回收时清理旧状态。

### 8.3 动效模型

InfoFlyout 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.4 视觉选项模型

InfoFlyout 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [InfoFlyout 桌面版实现原理](implementation.md)
- [InfoFlyout Semantic Part 契约](semantic-part.md)
- [InfoFlyout Token 设计](token.md)
- [InfoFlyout Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、collection/filter、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
