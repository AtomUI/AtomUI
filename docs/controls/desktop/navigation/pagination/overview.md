# Pagination 桌面版架构设计

本文档定义 `Pagination` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，Semantic Part 契约见 [Pagination Semantic Part 契约](semantic-part.md)，内部实现原理见 [Pagination 桌面版实现原理](implementation.md)，Pagination Token 的专项设计见 [Pagination Token 设计](token.md)，设计和契约变化记录见 [Pagination Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/Pagination` |
| 控件状态 | Stable |

Pagination 是 AtomUI 桌面控件体系中的分页控件，用于在有限页集合中切换页码、页大小和上一页下一页。

Pagination 不负责数据源过滤、排序或虚拟化列表本身。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

Pagination 与 SimplePagination 的公开语义区域使用 `root` 和 `item` 两个 Semantic Part，与上游 Pagination 语义结构对齐；`SimplePagination` 额外公开 AtomUI 扩展的 `info` Part，用于格式化分页信息文本。完整契约见 [Pagination Semantic Part 契约](semantic-part.md)。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Pagination`

## 2. 设计语言

Pagination 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Pagination 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Pagination 是 AtomUI 桌面控件体系中的分页控件，用于在有限页集合中切换页码、页大小和上一页下一页。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Icon`、`JumpToText`、`PageText`、`PaginationItemType`、`TotalInfoTemplate`、`SizeChangerTemplate`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active、collection/filter、input/value、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Pagination Token + ControlTheme。 |

## 3. API 与契约模型

Pagination 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Icon`、`JumpToText`、`PageText`、`PaginationItemType`、`TotalInfoTemplate` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `CurrentPage`、`IsHideOnSinglePage`、`IsSelected`、`PageCount`、`PageSize` | 维护选择、展开、过滤、分页、分组或集合状态；`CurrentPage` 和 `PageSize` 默认 `TwoWay`。 |
| 交互与状态 | `IsMotionEnabled`、`IsPressed`、`IsReadOnly`、`IsShowQuickJumper`、`IsShowSizeChanger`、`IsShowTotalInfo`、`IsShowLessItems`、`IsShowPrevNextJumpers` | 表达用户可观察状态、可用性、页码密度、快速跳页、清除、加载或反馈语义。 |
| 局部组件定制 | `SizeChangerTemplate`、`PaginationSizeChangerContext` | 只替换 page-size changer 的输入组件；分页状态、可见性、禁用态和页数计算仍由 `Pagination` 管理。 |
| 视觉与布局 | `Align`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `Maximum`、`Minimum`、`Total` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

稳定事件包括 `Click`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 控件与数据契约：`AbstractPagination`、`Pagination`、`SimplePagination`、`PaginationSizeChangerContext`、`PageChangedEventArgs`。
- 枚举：`PaginationAlign`。
- 本地化 Catalog：`PaginationLangResourceKind`。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Frame` | `Border` | 承载 `PaginationNav` 的裁剪、圆角和布局边界。 |
| `PART_InfoIndicator` | `TextBlock` | 展示 `SimplePagination` 的“当前页 / 总页数”文本。 |
| `PART_JumpToContentPresenter` | `ContentPresenter` | 展示快速跳页输入框前置文案。 |
| `PART_Nav` | `PaginationNav` | 承载上一页、下一页、页码和快速跳页项。 |
| `PART_NextNavItem` | `PaginationNavItem` | 承载 `SimplePagination` 的下一页操作。 |
| `PART_PageContentPresenter` | `ContentPresenter` | 展示快速跳页输入框后置文案。 |
| `PART_PageLineEdit` | `LineEdit` | 接收 `QuickJumperBar` 的目标页输入。 |
| `PART_PreviousNavItem` | `PaginationNavItem` | 承载 `SimplePagination` 的上一页操作。 |
| `PART_QuickJumper` | `QuickJumpEdit` | 接收 `SimplePagination` 的目标页输入。 |
| `PART_QuickJumperBarPresenter` | `ContentPresenter` | 承载 `Pagination` 的快速跳页组件。 |
| `PART_RootLayout` | `StackPanel` | 承载 `Pagination` 与 `QuickJumperBar` 的水平布局。 |
| `PART_RootLayoutPart` | `StackPanel` | 承载 `SimplePagination` 的水平布局。 |
| `PART_SizeChangerPresenter` | `ContentPresenter` | 在默认 ComboBox 与 `SizeChangerTemplate` 生成内容之间切换，稳定承载 page-size changer。 |
| `PART_TotalInfoPresenter` | `ContentPresenter` | 展示数据总量与当前范围文本。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

Pagination 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- 用户点击页码、快速跳转或切换页大小时，通过 `CurrentPage` / `PageSize` 写回同一个受控状态；绑定方不需要显式设置 `Mode=TwoWay`。
- 默认横向布局顺序固定为 total info、page navigation、page-size changer、quick jumper。分页宿主的额外区域必须位于 Pagination 外围，不得移动或复制 page-size selector。
- `SizeChangerTemplate` 只替换页大小输入组件。模板写入经 `PaginationSizeChangerContext` 收敛到 `Pagination.PageSize`，不能成为第二个分页状态 owner。
- selection/checked/active、collection/filter、input/value、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

Pagination 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `PaginationNavItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `PaginationNavTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `PaginationTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `QuickJumperBarTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `SimplePaginationTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |

Pagination 使用 `PaginationToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 selection/checked/active、collection/filter、input/value、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- `PART_SizeChangerPresenter` 保持为唯一页大小组件宿主；默认 ComboBox 与自定义模板不得同时物化。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Pagination 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractPagination`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Pagination`：完整分页控件，是页码、页大小、总量和局部组件定制的状态 owner。
- `PaginationSizeChangerContext`：公开的非 Visual 模板数据契约，把页大小输入与 `Pagination` 的受控状态连接起来。
- `PaginationNav` / `PaginationNavItem`：internal 导航容器与条目，承载固定容器池和导航交互。
- `PageSizeComboBoxItem`：internal 默认 ComboBox 条目，只服务内置 page-size changer。
- `PaginationToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `QuickJumpEdit` / `QuickJumperBar`：internal 快速跳页输入与组合栏。
- `SimplePagination`：简洁分页控件，不提供 page-size changer 定制入口。
- `PaginationLangResourceKind`：稳定的本地化 Catalog enum；内置翻译由同目录三种语言 XLIFF 提供并在编译期生成。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Pagination 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- `SizeChangerTemplate=null` 时必须继续使用现有 ComboBox、`PageSizeOptions` 和本地化文案；设置自定义模板不能改变 `IsShowSizeChanger` 的可见性语义。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

Pagination 的当前项状态必须由单一 owner 推导。public 选择属性、集合项容器和伪类之间只能做单向同步，集合替换、清空和模板重套用时必须回放当前状态。

`CurrentPage` 与 `PageSize` 是用户可控分页状态。内部页码修正、按钮导航、quick jumper 和 size changer 必须使用 `SetCurrentValue` 更新它们，避免覆盖外部 binding，同时让默认 `TwoWay` binding 写回 ViewModel。

### 8.2 Page-size changer 定制模型

`SizeChangerTemplate` 是 `Pagination` 的局部组件替换入口，默认值为 `null`。`null` 使用内置 ComboBox；非空模板由
`PART_SizeChangerPresenter` 实例化，并以 `PaginationSizeChangerContext` 作为模板数据。`IsShowSizeChanger` 仍是唯一
可见性开关，设置模板本身不会强制显示 page-size changer。

`PaginationSizeChangerContext` 只暴露模板交互必需的稳定状态：

| 成员 | 类型与默认绑定 | 语义 |
| --- | --- | --- |
| `PageSize` | `int`，TwoWay | 显示当前有效页大小；写入正整数时请求 `Pagination` 更新 `PageSize`。原始 `PageSize=0` 按 `DefaultPageSize` 投影。 |
| `SizeType` | `CustomizableSizeType`，OneWay | 投影 owner 的尺寸档，供自定义输入控件与分页布局保持一致。 |

自定义模板位于 `Pagination` 的视觉树中，禁用态通过 Avalonia 有效禁用状态自然继承，不复制到 Context。
`PageSizeOptions`、默认选项文案和 ComboBox 选择模型只属于内置 page-size changer；自定义模板自行决定输入控件、候选值、
格式和可访问性描述。Context 由 `Pagination` 创建，应用不直接实例化；它不能公开 owner 引用，也不能直接持有视觉对象。

### 8.3 集合与数据同步模型

Pagination 的集合状态必须能处理 source replace、reset、clear 和 container recycle。业务数据对象不应反向持有视觉对象，虚拟化或懒创建路径必须在容器回收时清理旧状态。

### 8.4 动效模型

Pagination 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.5 视觉选项模型

Pagination 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

## 9. 文档导航与验证策略

关联文档：

- [Pagination Semantic Part 契约](semantic-part.md)
- [Pagination 桌面版实现原理](implementation.md)
- [Pagination Token 设计](token.md)
- [Pagination Changelog](changelog.md)

Part 的 Selector、ContractType、数量语义与定制边界以 [Pagination Semantic Part 契约](semantic-part.md) 为唯一完整来源。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 自定义 page-size changer | 覆盖默认/自定义内容互斥、Context 双向写入、外部 binding 保留、禁用态继承、模板运行时切换和 re-template。 |
| 状态模型 | 覆盖 selection/checked/active、collection/filter、input/value、motion、visual option、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
| AOT | Gallery 模板使用带 `x:DataType` 的编译绑定；不得为 Context 引入字符串 path、反射扫描或动态成员访问。 |
