# Pagination 桌面版实现原理

本文档描述 Pagination 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Pagination 桌面版架构设计](overview.md)，Semantic Part 契约见 [Pagination Semantic Part 契约](semantic-part.md)，变化记录见 [Pagination Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Pagination Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Pagination 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Pagination/AbstractPagination.cs`
- `src/AtomUI.Desktop.Controls/Pagination/Localization/PaginationLangResourceKind.cs`
- `src/AtomUI.Desktop.Controls/Pagination/Localization/en-US.xlf`
- `src/AtomUI.Desktop.Controls/Pagination/Localization/zh-CN.xlf`
- `src/AtomUI.Desktop.Controls/Pagination/Localization/zh-TW.xlf`
- `src/AtomUI.Desktop.Controls/Pagination/PageNavRequestArgs.cs`
- `src/AtomUI.Desktop.Controls/Pagination/PageSizeComboBoxItem.cs`
- `src/AtomUI.Desktop.Controls/Pagination/Pagination.cs`
- `src/AtomUI.Desktop.Controls/Pagination/PaginationSizeChangerContext.cs`
- `src/AtomUI.Desktop.Controls/Pagination/Pagination.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Pagination/PaginationNav.cs`
- `src/AtomUI.Desktop.Controls/Pagination/PaginationNavItem.cs`
- `src/AtomUI.Desktop.Controls/Pagination/PaginationToken.cs`
- `src/AtomUI.Desktop.Controls/Pagination/QuickJumpEdit.cs`
- `src/AtomUI.Desktop.Controls/Pagination/QuickJumperBar.cs`
- `src/AtomUI.Desktop.Controls/Pagination/SimplePagination.cs`
- `src/AtomUI.Desktop.Controls/Pagination/SimplePagination.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationNavItemTheme.axaml`
- `src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationNavTheme.axaml`
- `src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationTheme.axaml`
- `src/AtomUI.Desktop.Controls/Pagination/Themes/QuickJumperBarTheme.axaml`
- `src/AtomUI.Desktop.Controls/Pagination/Themes/SimplePaginationTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractPagination`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Pagination`：完整分页控件，是 `CurrentPage`、`PageSize`、`Total`、page-size changer 和导航状态的唯一 owner。
- `PaginationSizeChangerContext`：owner-managed 的公开非 Visual `AvaloniaObject`，向 `SizeChangerTemplate` 投影有效
  `PageSize` 与 `SizeType`，并把模板写入转成 owner 的页大小更新请求。
- `PageNavRequestArgs`：internal 导航请求数据，连接导航条目与 `Pagination`。
- `PageSizeComboBoxItem`：internal 默认 ComboBox 条目，保存页大小与本地化显示内容。
- `PaginationNav` / `PaginationNavItem`：internal 固定容器池与导航条目，承载页码窗口、选择状态和点击交互。
- `PaginationToken`：14项Global-only／常量默认，以V2静态函数求值；SimplePagination保留独立Global identity，显式复用Pagination Own。两项未消费字段按兼容保留处理，详见Token设计。
- `QuickJumpArgs` / `QuickJumpEdit` / `QuickJumperBar`：internal 快速跳页请求、输入和组合栏。Small普通输入Width／简洁输入MinWidth读取Mini资源，其余尺寸读取常规宽度；不改变导航或校验。
- `SimplePagination`：简洁分页控件，复用 `AbstractPagination` 状态模型，但不包含 page-size changer。
- `PaginationLangResourceKind`：稳定的本地化 Catalog enum；三个 XLIFF 文件提供随模块发布的内置翻译，生成器负责编译资源表和 XAML 扩展。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- `PaginationSizeChangerContext` 是模板数据桥，不是独立状态 owner；它不持有视觉对象、资源宿主或 `Pagination` 的公开引用。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Pagination 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Icon`、`JumpToText`、`PageText`、`PaginationItemType`、`TotalInfoTemplate`、`SizeChangerTemplate`。
- 选择与集合：`CurrentPage`、`IsHideOnSinglePage`、`IsSelected`、`PageCount`、`PageSize`；`CurrentPage` 和 `PageSize` 注册为默认 `TwoWay` 受控状态。
- 交互与状态：`IsMotionEnabled`、`IsPressed`、`IsReadOnly`、`IsShowQuickJumper`、`IsShowSizeChanger`、`IsShowTotalInfo`。
- 视觉与布局：`Align`、`SizeType`。
- 其他稳定入口：`Maximum`、`Minimum`、`Total`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 内部导航、quick jumper、size changer 和页码修正必须用 `SetCurrentValue` 写入 `CurrentPage` / `PageSize`，保留外部 binding owner 并触发默认 `TwoWay` 写回。
- 自定义模板不能直接成为 `Pagination.PageSize` 的竞争 owner。模板写入先进入 Context，再由 `Pagination` 使用
  `SetCurrentValue(PageSizeProperty, value)` 提交；owner 属性变化通过独立同步入口回放到 Context，不以抑制标志形成双向循环。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 组合结构模型

### 5.1 控件角色图

```text
Pagination (public state owner)
  -> DashedBorder (root visual)
     -> StackPanel#PART_RootLayout
        -> ContentPresenter#PART_TotalInfoPresenter
        -> PaginationNav#PART_Nav (internal-observable)
           -> PaginationNavItem fixed container pool (internal-observable)
        -> ContentPresenter#PART_SizeChangerPresenter (template-stable)
           -> ComboBox (default, runtime-created)
           -> SizeChangerTemplate(PaginationSizeChangerContext) (custom, mutually exclusive)
        -> ContentPresenter#PART_QuickJumperBarPresenter
           -> QuickJumperBar (internal-observable)
```

### 5.2 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| 分页状态 owner | `Pagination` | `Pagination.cs` | 控件实例 | `CurrentPage`、`PageSize`、`Total`、`SizeChangerTemplate` | public | 所有分页状态更新最终收敛到 owner。 |
| 模板数据桥 | `PaginationSizeChangerContext` | `PaginationSizeChangerContext.cs` | `Pagination` | `SizeChangerTemplate`、`PageSize`、`SizeType` | public | 只投影和转发状态，不持有视觉对象或独立业务状态。 |
| 页大小宿主 | `PART_SizeChangerPresenter` / `ContentPresenter` | `PaginationTheme.axaml` | 当前 ControlTemplate | `IsShowSizeChanger`、`SizeChangerTemplate` | template-stable | 可替换内容，不改变 part 名称、可见性 owner 或默认布局位置。 |
| 默认页大小输入 | `ComboBox` + `PageSizeComboBoxItem` | `Pagination.cs` | `Pagination` | `PageSize`、`PageSizeOptions`、`SizeType` | internal-observable | 仅在 `SizeChangerTemplate=null` 时存在。 |
| 自定义页大小输入 | `IDataTemplate` 生成的 Control | 应用/Gallery AXAML | `PART_SizeChangerPresenter` | `SizeChangerTemplate`、Context 状态 | public customization | 输入形态、格式、候选值和可访问性由模板负责。 |
| 页码导航 | `PaginationNav` + `PaginationNavItem` | Pagination Themes + C# | `Pagination` | `CurrentPage`、`PageCount`、`IsShowLessItems` | internal-observable | 只用于理解固定容器池和导航行为，应用不依赖 internal 类型。 |

默认与自定义 page-size changer 是互斥分支。`SizeChangerTemplate` 只替换
`PART_SizeChangerPresenter` 的内容，不替换 `PaginationNav`、total info、quick jumper 或根布局，也不新增 Semantic Part。

## 6. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- `Pagination` 创建并持有单个 `PaginationSizeChangerContext`；Context 与 owner 同生命周期，不注册全局事件或资源宿主。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- `SizeChangerTemplate=null` 时创建或复用默认 ComboBox，建立 `SizeType` relay binding 和 `SelectionChanged` 订阅；
  自定义模板生效前必须解除默认 ComboBox 的事件与 binding。模板恢复为 `null` 时重新建立默认路径并回放当前有效页大小。
- ControlTemplate 重套用只更换 presenter 宿主，不创建第二个 Context；新 presenter 必须接收当前有效内容和模板。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_Frame`：承载根视觉、边框、背景或尺寸基线。
- `PART_InfoIndicator`：展示指示器、进度、分页或状态反馈。
- `PART_JumpToContentPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_Nav`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_NextNavItem`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_PageContentPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_PageLineEdit`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_PreviousNavItem`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_QuickJumper`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_QuickJumperBarPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_RootLayout`：`StackPanel` 水平布局入口，保持 Pagination 原有节点顺序。
- `PART_RootLayoutPart`：承载根视觉、边框、背景或尺寸基线。
- `PART_SizeChangerPresenter`：在默认 ComboBox 与 `SizeChangerTemplate` 生成内容之间互斥切换，并保持
  `IsShowSizeChanger` 可见性语义。
- `PART_TotalInfoPresenter`：展示用户内容、文本、图标或模板化数据。

Semantic marker 接入点：

- `Pagination` 的 `item` descriptor 为运行时创建（`RuntimeCreated = true`），路由
  `/template/ .semantic-scope-nav > .semantic-item`。`PaginationTheme.axaml` 在 `PART_Nav` 上声明
  `Classes.semantic-scope-nav="True"` 作用域标记；`PaginationNavItem` 在初始化与 `PaginationItemType`
  变化时同步 `semantic-item` marker，`JumpPrevious` / `JumpNext` 类型移除 marker，其他类型加回。
- `SimplePagination` 的 `item` descriptor 为静态标记，`SimplePaginationTheme.axaml` 在
  `PART_PreviousNavItem` 与 `PART_NextNavItem` 上声明 `Classes.semantic-item="True"`。
- `SimplePagination` 的 `info` descriptor 为静态标记，`SimplePaginationTheme.axaml` 在
  `PART_InfoIndicator` 上声明 `Classes.semantic-info="True"`，覆盖 "当前页 / 总页数" 信息文本。
- `Pagination` 与 `SimplePagination` 根模板均在根布局 `StackPanel` 外包裹 TemplateBind 根视觉属性的
  `atom:DashedBorder`：`Background` / `BackgroundSizing` / `BorderBrush` / `BorderThickness` /
  `CornerRadius` / `Padding` 来自 `TemplatedControl`，`StrokeDashArray` / `StrokeDaskOffset` 来自
  `AbstractPagination` 新增的 `BorderDashArray` / `BorderDashOffset`，使 root Part 的边框（含虚线）、
  背景与内边距定制可渲染；默认值不改变既有外观。

## 7. 交互与事件处理

Pagination 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。
- 自定义 page-size changer 只提交正整数。Context 将 `PageSize<=0` 投影为 `DefaultPageSize`；空值、非整数、格式和
  最小值限制由具体模板控件负责，非法值不得进入 owner 状态。

稳定事件路径包括 `Click`。事件参数和触发时机属于兼容边界。

## 8. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。

### 8.1 导航项容器池与显示区间

`PaginationNav` 固定预建 `Pagination.MaxNavItemCount`（9）个 `PaginationNavItem` 容器，上一页/下一页占用
首尾位置，其余 7 个位置按显示区间复用。`PaginationNavigationModel` 按 `CurrentPage`、`PageCount`、
`IsShowLessItems` 与 `IsShowPrevNextJumpers` 生成页码项和 `JumpPrevious` / `JumpNext` 快速跳页项。默认模式保持
7 个中间项；`IsShowLessItems=True` 时使用 5 个中间项，并把跳转跨度从 5 页改为 3 页。未进入显示区间的容器保持隐藏。容器池是固定的：`CurrentPage`、`PageSize` 或
`Total` 变化只重排容器内容与可见性，不重建容器；`semantic-item` marker 由 `PaginationItemType` 驱动同步，
与容器可见性解耦。

重排时必须先生成新窗口，再按槽位原地更新仍被使用的容器；不能先把所有中间容器设为不可见后重新显示。
当 jump 类型和方向不变时应复用原 `EllipsisOutlined` 与双箭头实例，仅清理新窗口未使用的尾部容器。该不变量
保证鼠标静止点击 jump 后 `:pointerover`、双箭头可见性与 opacity transition 不会被无意义重置。

### 8.2 Page-size changer 状态桥

page-size changer 使用以下单向收敛流程：

```text
Pagination.PageSize / SizeType
  -> 同步 PaginationSizeChangerContext
  -> SizeChangerTemplate 的编译绑定更新自定义输入

自定义输入
  -> PaginationSizeChangerContext.PageSize
  -> Pagination.SetCurrentValue(PageSizeProperty, normalizedValue)
  -> AbstractPagination 重新计算 PageCount 并收敛 CurrentPage
  -> 同步 Context 与模板
```

owner-to-context 同步与 context-to-owner 请求使用不同入口，并在值相同时停止传播，不使用 `_ignoreXxx`、
`_suppressXxx` 或延迟刷新掩盖循环。页大小变化继续复用 `AbstractPagination` 的页数计算和
`CurrentPageChanged` 事件路径，不建立自定义模板专属事件模型。页数收敛先写回 `CurrentPage`，由递归属性处理完成唯一一次
最终状态通知，避免旧页码通知在页大小变化后重新生成错误 PageRequest。

### 8.3 尺寸与状态基线矩阵

| SizeType | 默认值来源 | 条目尺寸（`PaginationNavItemTheme`） | 布局间距（根模板） |
| --- | --- | --- | --- |
| `Large` | 显式设置 | `Height` / `MinWidth` = `ItemSize` | `PaginationLayoutSpacing` |
| `Middle` | `CustomizableSizeTypeControlProperty.SizeTypeProperty` 默认值 | `Height` / `MinWidth` = `ItemSize` | `PaginationLayoutSpacing` |
| `Small` | 显式设置 | `Height` / `MinWidth` = `ItemSizeSM` | `PaginationLayoutMiniSpacing` |
| `Custom` | 显式设置 | `Height` / `MinWidth` = `ItemSize` | `PaginationLayoutSpacing` |

条目尺寸由 `PaginationNavItemTheme` 的 `^[SizeType=...]` selector 独占维护，控件代码不参与尺寸计算；
`Middle` 与 `Custom` 共用 `ItemSize` 基线，`Small` 使用 `ItemSizeSM`。`SizeType` 通过
`CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner` 注册，默认值为 `Middle`。基线矩阵是
稳定契约：改变任一格的 Token、selector 或默认值必须同步 Pagination 与 SimplePagination 两个根模板、
`PaginationNavItemTheme`、Token 文档与尺寸相关回归测试。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 9. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- `PaginationSizeChangerContext` 只使用显式 Avalonia 属性和强类型 owner 同步；内置与 Gallery 模板使用带
  `x:DataType` 的编译绑定，不使用字符串 path、`ReflectionBinding`、动态成员发现或运行时组件扫描。
- Context 不消费 `DynamicResource` 或 TokenResource，因此不实现 scoped resource host；模板内视觉控件从自身
  ControlTheme 和视觉树获得主题资源。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- Context 每个 `Pagination` 实例只创建一次；页大小变化只更新值，不重建模板生成的视觉树。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 10. 维护不变量

维护 Pagination 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- `CurrentPage` / `PageSize` 的默认 `TwoWay` binding metadata，以及内部写入不破坏外部 binding 的 `SetCurrentValue` 路径。
- `SizeChangerTemplate` 默认为 `null`；默认 ComboBox 行为、`PageSizeOptions`、本地化文案和当前页大小插入规则保持不变。
- 默认 ComboBox 与自定义模板内容互斥，`PART_SizeChangerPresenter` 继续作为唯一宿主；模板替换不改变
  `IsShowSizeChanger`、`PageCount`、`CurrentPageChanged` 或禁用态语义。
- Template part 名称、ControlTheme key、伪类和资源 key。
- Semantic Part descriptor、`semantic-scope-nav` 作用域标记、`semantic-item` / `semantic-info` marker 同步规则与
  生成的 `PaginationItemStyle` / `SimplePaginationItemStyle` / `SimplePaginationInfoStyle` 类型。快速跳页项
  无 `semantic-item` marker 属于上游语义对齐的稳定契约，不能通过主题或代码改动破坏。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 11. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- page-size changer 定制覆盖：默认值与默认 ComboBox 回归、自定义模板物化、Context 双向同步、正整数边界、
  `PageSize=0` 有效值、外部 TwoWay binding 保留、`CurrentPage` 收敛、禁用态继承、模板运行时切换和 re-template 释放。
- Semantic Part 契约变更运行 `tests/AtomUI.Desktop.Controls.Tests/Pagination/PaginationSemanticPartTests.cs`，
  Gallery 语义预览与样式示例变更运行 `tests/AtomUIGallery.Tests/ShowCases/PaginationShowCasePageTests.cs`。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- Gallery 自定义模板使用 `x:DataType="atom:PaginationSizeChangerContext"`，并由 Gallery 测试锁定公开 API 用法。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证；自定义模板实现后至少验证编译绑定与真实
  Gallery NativeAOT publish。

## 状态动效初始化

`PaginationNavItem` 构造时建立共享初始化会话，覆盖导航容器挂载后才同步的初始页码状态；加载后恢复前景、边框和背景过渡。
共享目标与生命周期契约见 [状态动效初始化架构](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)。
