# FloatButton 桌面版实现原理

本文档描述 FloatButton 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [FloatButton 桌面版架构设计](overview.md)，变化记录见 [FloatButton Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [FloatButton Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 FloatButton 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/FloatButton/BackTopFloatButton.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/BackTopFloatButtonHost.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButton.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonGroup.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonGroupHost.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonHost.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonItemsControl.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonToken.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButton.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/BackTopFloatButton.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonGroup.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.cs`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/BackTopFloatButtonHostTheme.axaml`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/BackTopFloatButtonTheme.axaml`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonGroupHostTheme.axaml`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonGroupTheme.axaml`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonHostTheme.axaml`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonItemsControlTheme.axaml`
- `src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonTheme.axaml`
- `src/AtomUI.Controls/FloatButton/AbstractBackTopFloatButton.cs`
- `src/AtomUI.Controls/FloatButton/AbstractBackTopFloatButtonHost.cs`
- `src/AtomUI.Controls/FloatButton/AbstractFloatButton.cs`
- `src/AtomUI.Controls/FloatButton/AbstractFloatButtonHost.cs`
- `src/AtomUI.Controls/FloatButton/BackTopProgressRing.cs`
- `src/AtomUI.Controls/FloatButton/FloatButtonEnums.cs`
- `src/AtomUI.Controls/FloatButton/FloatButtonSeparatorLayer.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。
- Semantic descriptor 由各 public owner 的 `[SemanticPart]` 声明生成；模板只使用静态 `Classes.semantic-*="True"` marker。
  `FloatButtonHost`、`FloatButtonGroupHost` 与 `BackTopFloatButtonHost` 不是 Semantic owner；它们在 Overlay Layer 中
  创建的真实控件才是 marker 与 descriptor 的归属。

## 3. 核心类职责

- `AbstractBackTopFloatButton`：回到顶部按钮共享基类，负责滚动目标、显示阈值、动效和点击后委托 Avalonia Button 命令语义。
- `AbstractBackTopFloatButtonHost`：回到顶部 host 共享基类，负责公开 host API 并把属性投影到 overlay 中的真实 `BackTopFloatButton`。
- `AbstractFloatButton`：`FloatButton` 交互基类，继承 Avalonia `Button`，是点击、命令、`CanExecute`、禁用状态和按钮伪类的真实 owner。
- `AbstractFloatButtonHost`：普通 host 共享基类，管理 scoped overlay 生命周期，并统一把 host 公共属性投影给运行时创建的真实按钮。
- `AbstractFloatButtonTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `BackTopProgressRing`：internal 进度环绘制元素（`AtomUI.Controls.Commons`），把 `ScrollProgress` 渲染为轨道与进度指示；线宽、颜色、圆角和显隐由 ControlTheme 注入。
- `BackTopFloatButton`：动作触发类型，负责点击、导航或局部操作状态。
- `BackTopFloatButtonHost`：模板协作类型，承载内容展示、宿主或视觉边界。
- `FloatButton`：动作触发类型，负责点击、导航或局部操作状态。
- `FloatButtonGroup`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `FloatButtonGroupHost`：overlay group host，负责把 group 公共属性、打开状态和 `DataContext` 投影给运行时创建的 `FloatButtonGroup`。
- `FloatButtonHost`：模板协作类型，承载内容展示、宿主或视觉边界。
- `FloatButtonItemsControl`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `FloatButtonSeparatorLayer`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `FloatButtonToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- `AbstractFloatButton` 是命令语义 owner；host 类型不能绕过它手动执行 `ICommand`。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

FloatButton 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`CloseIcon`、`Description`、`DescriptionTemplate`、`Icon`。
- 选择与集合：`BadgeCount`、`BadgeOverflowCount`、`IsTriggerMode`。
- 交互与状态：`IsBadgeEnabled`、`IsDotBadge`、`IsMotionEnabled`、`IsOpen`、`IsShowProgress`。`FloatButtonGroup.IsOpen` 与 `FloatButtonGroupHost.IsOpen` 默认双向绑定。
- 命令与动作：`Command`、`CommandParameter`、`Href`。普通按钮直接继承 Avalonia Button 语义；host 类型只把命令投影给 overlay 中的真实按钮。
- 视觉与布局：`BadgeColor`、`BadgeOffset`、`BoxShadow`、`FloatOffsetX`、`FloatOffsetY`、`MenuPlacement`、`Orientation`、`Placement`、`SeparatorBrush`、`Shape` 等 12 项。
- 动效与异步：`MenuMotionDuration`、`MotionDuration`、`ToTopDuration`。
- 其他稳定入口：`ButtonType`、`Target`、`Tooltip`、`Trigger`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- `IsOpen` 由 group 与 host 共享为受控打开状态；hover/click、open request 和 close request 应使用 current value 语义回写，不得以 style priority 覆盖绑定或本地值。
- `Command`、`CommandParameter` 和 `CanExecute` 不建立 host 私有状态机；`FloatButtonHost`、`BackTopFloatButtonHost` 创建的真实按钮通过属性投影接收命令契约。
- `IsShowProgress` 只控制进度环显隐；`ScrollProgress` 是从 `Target` 派生的 internal 状态，`BackTopFloatButtonHost` 通过 `AddOwner` 把开关投影给 overlay 中的真实按钮，不建立第二份滚动状态。
- `BackTopFloatButton` 点击时先检查有效可用状态；当命令不可执行导致按钮不可用时，不应触发滚动动作。
- `FloatButtonGroupHost` 创建 overlay group 后必须把 host `DataContext` 传递给 group，使未设置本地 `DataContext` 的子 `FloatButton` 可继续解析 `Command="{Binding ...}"`。
- 子按钮显式设置的本地 `DataContext` 优先级高于 host 继承数据上下文，group host 不应强制覆盖。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

命令投影流：

```text
FloatButtonHost / BackTopFloatButtonHost
  -> CreateFloatButton(...)
  -> host Command / CommandParameter / DataContext property projection
  -> real FloatButton / BackTopFloatButton
  -> Avalonia Button click + CanExecute + disabled semantics
```

Group 子项绑定流：

```text
FloatButtonGroupHost.DataContext
  -> runtime FloatButtonGroup.DataContext
  -> FloatButtonItemsControl / logical children
  -> child FloatButton.Command Binding
```

BackTop 进度流：

```text
Target ScrollViewer (ScrollChanged / OnLoaded)
  -> ScrollProgress (internal, 0~1, 由 AbstractBackTopFloatButton 推导)
  -> TemplateBinding Progress -> BackTopProgressRing#ProgressRing
  -> 轨道描边 + 进度扇形填充 Render
```

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。
- Host 到 overlay 真实按钮或 group 的属性投影属于运行时动态目标绑定，AXAML 无法通过 `TemplateBinding` 表达；允许使用 C# binding 或 `BindUtils.RelayBind`，但必须挂到当前 overlay 生命周期的 `CompositeDisposable`。
- `PrepareScopedOverlayLayer` 创建 overlay 目标时建立属性投影；`CleanupScopedOverlayLayer` 移除 overlay 目标并释放同一批投影绑定。
- Group host 搬移 `Children` 到 overlay group 时必须保持逻辑继承路径；释放 overlay group 前应先解除子项挂载，避免同一 child 被旧 group 保留。

稳定 template part 接入点：

- `PART_BadgeLayout`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ItemsLayout`：承载集合项、布局面板或虚拟化内容。

Semantic marker 与模板结构的真实映射：

- `AbstractFloatButtonTheme` 的 Circle/Square 双模板中，`IconPresenter.semantic-icon` 静态存在；
  `ContentPresenter.semantic-content` 只在 Square 模板存在。`BackTopFloatButtonTheme` 在既有 Panel 上声明 `atom:Motion`，
  由 `IsActive` 驱动 Fade，提供同一组 marker；不持有方向 guard、显隐执行器或 Actor。
- `FloatButtonGroupTheme` 的 Default 模板只有 `FloatButtonItemsControl.semantic-list`；Click/Hover 模板中
  `FloatButton.semantic-trigger` 与 `Border#PART_MenuSurface` 是 Canvas 下的 sibling，`semantic-list` 位于该静态表面内。
  表面隔离菜单位移与 list 自定义变换；有效时长和启用状态由主题绑定。位移距离取 trigger 当前 DIP 尺寸，
  控件在提交可见性前一次性构建 Motion 参数快照，播放与替代由 Core 负责。
- Badge adorner 由 `ConfigureBadge` 在运行时创建并加入 `PART_BadgeLayout`，属于 Badge 家族 descriptor 的范围；
  FloatButton 不为其声明 marker。
- 已知不一致：`BackTopFloatButtonTheme` 的模板把徽标画布命名为 `BadgeLayout`（缺少 `PART_` 前缀），而
  `AbstractFloatButton.OnApplyTemplate` 查找 `PART_BadgeLayout`，因此 BackTop 的徽标属性当前不会投影到模板；
  该问题属于既有缺陷，修复需单独评审，不随 Semantic Part 改造变更。

## 6. 交互与事件处理

FloatButton 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。
- `FloatButtonGroupHost` trigger 点击只处理展开/收起，不默认触发业务 `Command`；业务命令应挂在 group 内部子 `FloatButton`。
- `FloatButtonHost` 与 `BackTopFloatButtonHost` 对外表现为可绑定命令的 host，但命令执行仍发生在 overlay 内真实按钮的 Avalonia Button 点击路径中。

稳定事件路径包括 `Clicked`、`Closed`、`Opened`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- Host 公共属性到 overlay 真实按钮的投影，以及 detach 时的投影释放。
- Group host `DataContext` 到 overlay group 和子按钮命令绑定的继承链。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- Semantic Part marker 不参与状态切换；`IsOpen`、`IsActive`、`Shape` 与 `Trigger` 只改变模板选择或目标节点的
  `IsVisible`，模板重套用后由新模板重新提供同一 marker 契约。

### 回到顶部进度环

- 进度计算：`maxScroll = max(Extent.Height - Viewport.Height, 0)`，`ScrollProgress = maxScroll > 0 ? Clamp(Offset.Y / maxScroll, 0, 1) : 0`。进度只在 `Target` 的 `ScrollChanged` 与控件 `OnLoaded` 两个入口更新；Avalonia `ScrollChanged` 在 extent/viewport 变化时同样触发，内容尺寸变化（等效 resize 场景）已被覆盖，不引入额外的 SizeChanged 监听。
- 绘制实现：`BackTopProgressRing` 自定义 `Render` 绘制轨道与进度指示。轨道是圆角矩形描边，`CornerRadius` 为高度一半（Circle 形状由基类尺寸同步设置）时即为整圆；进度指示是以环中心为顶点、半径取环矩形半对角线的扇形填充，从 12 点方向（-90°）顺时针扫 `360 × progress` 度（进度满 100% 时扇形退化，直接用进度色填充整个环带），再裁剪到环带。
- 环几何不变量：`CalculateRingMetrics` 统一推导三个边界——外边界半径等于按钮自身圆角半径（与按钮轮廓完全重合，圆角处不留缝隙），中心线半径 = 外边界半径 − 半线宽，内边界半径 = 外边界半径 − 线宽；对应矩形依次为 `bounds`、`bounds.Deflate(半线宽)`、`bounds.Deflate(线宽)`。`CornerRadius` 是**外边界**的半径语义，不是中心线半径：把它当中心线使用会让外缘比按钮轮廓大出半线宽，在四个圆角处露出背景与环之间的缝隙（圆形因半径被 clamp 到尺寸一半而恰好不暴露）。
- ControlTheme 兜底理由：扇形角度是随滚动连续变化的运行时值，ControlTheme selector/Setter 无法表达，故按主题绑定优先约束的兜底条款采用自定义 Render；线宽、颜色与显隐等静态契约仍由 `BackTopFloatButtonTheme.axaml` 注入——`StrokeThickness` 取 `LineWidthBold`、`TrackBrush` 取 `ColorBorderSecondary`、`IndicatorBrush` 取 `ColorPrimary`，显隐由 `^[IsShowProgress=True] /template/ atomc|BackTopProgressRing#ProgressRing` selector 控制，默认 `IsVisible=False`。
- Square 环带裁剪：进度扇形通过 `PushGeometryClip` 裁剪到「外圆角矩形 − 内圆角矩形」的 `CombinedGeometry`（`Exclude`）环带，Circle 与 Square 复用同一渲染路径。扇形半径取半对角线以保证覆盖环带最外点。Circle 形状下环带为同心圆环，扇形与环带的交集即原圆弧进度带。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不通过 VisualTree 扫描维护 Semantic Part；Gallery Preview 使用生成 descriptor 和 owner-scoped marker 解析。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 命令执行、`CanExecute` 和禁用状态优先复用 Avalonia Button 原生实现，不维护重复命令缓存。
- Host 动态投影绑定数量应保持为公共属性映射所需的最小集合，生命周期绑定到 overlay target，避免 detach 后保留 host 或 child。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 FloatButton 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- Avalonia Button 命令语义：`CanExecute`、禁用状态、点击事件和命令执行顺序不得被 host 手动调用路径绕开。
- Host overlay 投影的 acquire/release 必须成对；不能留下命令绑定、数据上下文绑定或 child 逻辑父级保留。
- Group 子按钮的命令绑定必须能继承 host `DataContext`，同时保留子项本地 `DataContext` 的优先级。
- `BackTopProgressRing` 显隐契约：进度环默认隐藏，仅 `IsShowProgress=True` 的主题 selector 置可见；进度环线宽与颜色来自全局 SharedToken，不得引入控件实例色值或运行时状态色。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- FloatButton 命令支持变更需覆盖 host 命令执行、`CommandParameter` 转发、`CanExecute=false` 禁用、BackTop 命令路径、group child 继承 host `DataContext`、child 本地 `DataContext` 不被覆盖、attach 后新增 child 的命令绑定。
- BackTop 进度环变更运行 `tests/AtomUI.Desktop.Controls.Tests/FloatButton/BackTopFloatButtonProgressTests.cs`，覆盖进度计算夹取、`IsShowProgress` 显隐与 host 投影。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
