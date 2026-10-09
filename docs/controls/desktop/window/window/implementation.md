# Window 桌面版实现原理

本文档描述 Window 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Window 桌面版架构设计](overview.md)，Window 与标题栏的对齐协作见 [WindowTitleBar 实现原理](../window-title-bar/implementation.md)，caption button 的能力与呈现模型见 [WindowTitleBar Caption Button 配置设计](../window-title-bar/caption-button-configuration-design.md)，变化记录见 [Window Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Window Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Window 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Window/Chrome/WindowChromeManager.cs`
- `src/AtomUI.Desktop.Controls/Window/Chrome/AbstractLinuxWindowChromeManager.cs`
- `src/AtomUI.Desktop.Controls/Window/Chrome/GenericLinuxWindowChromeManager.cs`
- `src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs`
- `src/AtomUI.Desktop.Controls/Window/Chrome/WaylandWindowChromeManager.cs`
- `src/AtomUI.Desktop.Controls/Window/Chrome/WindowsWindowChromeManager.cs`
- `src/AtomUI.Desktop.Controls/Window/MacStandardWindowButtons.cs`
- `src/AtomUI.Desktop.Controls/Window/MediaBreakPointThemeBootstrapper.cs`
- `src/AtomUI.Desktop.Controls/Window/ReactiveWindow.cs`
- `src/AtomUI.Desktop.Controls/Window/Themes/FullscreenPopoverLayerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml`
- `src/AtomUI.Desktop.Controls/Window/Themes/WindowResizerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml`
- `src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.cs`
- `src/AtomUI.Desktop.Controls/Window/Utils/FullscreenPopoverLayer.cs`
- `src/AtomUI.Desktop.Controls/Window/Utils/WindowDrawnDecorationsReflectionExtensions.cs`
- `src/AtomUI.Desktop.Controls/Window/Utils/WindowResizer.cs`
- `src/AtomUI.Desktop.Controls/Window/Utils/WindowTitleBarShadowBackground.cs`
- `src/AtomUI.Desktop.Controls/Window/Utils/WindowVisualLayerClip.cs`
- `src/AtomUI.Desktop.Controls/Window/Window.cs`
- `src/AtomUI.Desktop.Controls/Window/WindowToken.cs`
- `src/AtomUI.Native/WindowExtensions.cs`
- `src/AtomUI.Native/Linux/WaylandWindowReflectionExtensions.cs`
- `src/AtomUI.Native/Linux/WaylandWindowUtils.cs`
- `src/AtomUI.Native/Linux/WindowUtils.Linux.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- `Window/Chrome` 只承载平台 chrome manager：选择后端、订阅 Window/PlatformImpl 事件、合并 frame geometry 更新，并把 X11、Wayland、Windows 的原生能力投影为 Window 内部状态。
- `Window/Utils` 承载 Window 模板内部视觉 helper 和 Desktop drawn decorations 反射边界，例如 visible frame clip、managed resize grip、macOS 全屏 popover 与 `DynamicDependency` 标注；这些类型是 internal 协作对象，不是用户 API。
- `AtomUI.Native` 只执行已经确定后端之后的底层平台调用，例如 XCB input region、Xlib geometry、Wayland `wl_surface.set_input_region`。X11 shadow 输入区订阅策略和 resize band 仍属于 `X11WindowChromeManager`，不下沉到 Native。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Window`：public 窗口控件，持有 public API、主题上下文、平台状态投影、标题栏连接、template part 接入和显示生命周期；定义完整的 title-bar host projection，为每个标题栏连接创建可释放 lease，并发布 CSD 内容区使用的 effective frame margin。
- `ReactiveWindow<TViewModel>`：public ReactiveUI 窗口基类，维护 `ViewModel` / `DataContext` 同步和 AOT 友好的 view activation。
- `MacStandardWindowButtons`：public macOS 标准窗口按钮布局附加能力，封装 spacing、offset 和按钮布局入口。
- `WindowChromeManager` / `IWindowChromeManager`：按平台创建 chrome manager，并定义 Window 与平台能力之间的内部协作接口。
- `AbstractLinuxWindowChromeManager`：Linux 后端的抽象共享 manager，负责 X11/Wayland/Generic 后端识别、CSD 状态同步、frame geometry 更新合并、title-bar height hint 和 visible frame border 更新。
- `GenericLinuxWindowChromeManager`：Linux generic fallback manager，承接无法确定为 X11 或 Wayland 的后端并复用共享 Linux chrome 行为。
- `X11WindowChromeManager`：X11 专属 manager，负责 map 前初始 geometry、`_GTK_FRAME_EXTENTS`、shadow input region 订阅策略和 10 DIP resize band 保留。
- `WaylandWindowChromeManager`：Wayland 专属 manager，负责 shadow extents 归一、managed resize grip 接管和 input region 矩形计算。
- `WindowsWindowChromeManager`：Windows 专属 manager，负责 Windows CSD frame dark mode 与可见 frame border 策略。
- `FullscreenPopoverLayer`：internal 模板协作层，维护 macOS 全屏标题栏 popover 的显示、按钮和宿主 Window 订阅。
- `WindowResizer`：internal 模板协作控件，使用 `GripThickness` 和 `BeginResizeDrag` 提供 managed resize grip。
- `WindowVisualLayerClip`：internal visible frame 裁剪 helper，是完整 layer surface 排除 `FrameShadowThickness` 后的共享计算入口。
- `WindowTitleBarShadowBackground`：internal drawn decorations 标题栏背景绘制 helper，按 visible frame 和圆角裁剪 Linux 标题栏背景。
- `WindowDrawnDecorationsReflectionExtensions`：Desktop Window 内部反射边界，集中访问 Avalonia drawn decorations、drawn title-bar height、frame geometry 和 resize grip layer；不发现或返回 Dialog/Drawer 业务 host。
- `WindowTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `WindowToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Window 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`ContentFrameBackground`、`ContentFrameLayer`、`ContentFrameLayerOpacity`、`ContentFrameLayerTemplate`、`IsTitleBarVisible`、`LogoTemplate`、`LeftAddOn`、`LeftAddOnTemplate`、`RightAddOn`、`RightAddOnTemplate`、`TitleBarFrameBackground`、`TitleBarFrameLayer`、`TitleBarFrameLayerOpacity`、`TitleBarFrameLayerTemplate` 等。
- 选择与集合：`ViewModel`。
- 交互与状态：`IsMinimizeCaptionButtonVisible`、`IsMaximizeCaptionButtonVisible`、`IsCloseCaptionButtonVisible`、`IsFullScreenCaptionButtonVisible`、`IsPinCaptionButtonVisible`、`IsMoveEnabled`。
- 弹层与窗口：`WindowFrameLayer`、`WindowFrameLayerOpacity`。
- 其他稳定入口：`Logo`、`LogoVisibility`、`IsTitleVisible`、`MediaBreakPoint`、`OsType`、`OsVersion`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。
- `TitleBarFrameLayer` 的数据流终点是标题栏背景/装饰层；它不作为普通 Avalonia 交互控件入口。默认标题栏按钮、菜单、搜索框等由 `LeftAddOn` 或 `RightAddOn` 通过 `WindowTitleBar` 承载。
- Window 是 caption capability、WindowState、Topmost、全屏恢复状态和窗口操作的 owner。五个 caption visibility 属性只作为 requested presentation 单向投影给 `WindowTitleBar`，不写回能力属性；Window 创建强类型 host projection lease，但不持有任意内容区标题栏的生命周期；`CaptionButtonGroup` 不持有 Window 引用或第二套窗口状态。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

### 5.1 TitleBar 宿主投影生命周期

Window 对标题栏的协作拆成两条独立路径：

1. 通用 host projection：Window 定义 caption requested visibility、窗口能力、WindowState、active state、Topmost、平台/CSD 输入、native chrome metrics、CaptionButtonCommand 和 effective Logo 通知的强类型集合，并把标题栏双击与拖动 pointer 订阅纳入同一个 `IDisposable` lease。
2. 默认内容配置：`NotifyConfigureTitleBar` 只投影 Title、IsTitleVisible、Logo、TitleAlignment、LeftAddOn、RightAddOn 及其模板，服务 Window 模板创建的默认或派生标题栏。

每个 `WindowTitleBar` 按自己的 logical attach/detach 生命周期持有 host projection lease。同一 Window 中多个标题栏分别持有独立 lease；标题栏从 Window A 移到 Window B 时，必须先释放 A 的 lease，再从 B 创建新 lease。默认标题栏在 `OnApplyTemplate` 中无条件提前连接宿主，随后进入逻辑树时命中幂等路径；派生类覆盖 `NotifyConfigureTitleBar` 不能跳过通用宿主连接。

所有连接到 Window 的标题栏都通过各自 lease 获得 pointer 拖动、双击最大化、caption 状态和命令。只有默认标题栏额外注册 `SizeChanged` 并参与标题栏高度提示、Windows CSD 最小高度和唯一 CSD geometry owner 生命周期。

Window 自身解析显式 Logo、`Icon` 与主窗口 Logo/Icon 的回退链。子 Window 打开前只解析主窗口当前值，不持有订阅；`OnOpened` 重新解析，且只有在窗口已打开并使用主窗口回退时才订阅主窗口 `Logo`、`LogoTemplate` 和 `Icon`。本地显式值或本地 `Icon` 接管时立即释放，`OnClosed` 也无条件释放。默认和自定义 `WindowTitleBar` 再以自身显式值优先、宿主 effective 值兜底的方式形成各自模板输入。

稳定 template part 接入点：

- `PART_CloseButton`：承载用户触发入口、导航或关闭动作。
- `PART_ContentPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_FullScreenButton`：承载用户触发入口、导航或关闭动作。
- `PART_FullscreenPopoverLayer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_OverlayWrapper`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_PopoverBorder`：承载根视觉、边框、背景或尺寸基线。
- `PART_PopoverCloseButton`：承载用户触发入口、导航或关闭动作。
- `PART_PopoverFullScreenButton`：承载用户触发入口、导航或关闭动作。
- `PART_RootLayout`：承载根视觉、边框、背景或尺寸基线。
- `PART_TitleBar`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_TitleBarPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_TransparencyFallback`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_UnderlayWrapper`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_VisualLayerManager`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_WindowFrame`：承载根视觉、边框、背景或尺寸基线。
- `PART_WindowResizer`：稳定模板协作入口，重命名前必须同步主题和实现。

## 6. 交互与事件处理

Window 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 弹层、窗口或 overlay 类路径必须稳定处理打开、关闭、取消、重复打开和宿主失活。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 值提交或命令触发必须保持继承控件的事件顺序。

当前没有抽取到控件专属 public 事件；交互语义主要通过继承事件、命令、属性变化和 Gallery 可观察行为体现。

标题栏双击与最大化按钮复用 Window 的 `ToggleMaximize` 操作入口；双击额外遵守 `CanResize`。最小化、最大化和恢复只修改 Avalonia `WindowState`，由当前平台窗口实现执行实际状态转换。Window 不为 caption 操作直接调用 Win32、AppKit、X11 或 Wayland API，也不使用 managed 缩放动画模拟系统窗口动画。

### 6.1 标题栏背景层、TitleBar 与 CSD 命中模型

Window 的标题栏存在两套输入模型，维护时必须同时成立：

- 非 CSD 自绘模板使用普通 Avalonia hit test。`TitleBarFrameLayer` 是默认 `TitleBar` 下方的背景/装饰层，不应承担用户输入。
- Avalonia `WindowDrawnDecorations` CSD 模板使用 `WindowDecorationProperties.ElementRole` 参与平台 chrome hit test。`ElementRole="TitleBar"` 表示拖拽区域；`TitleBarFrameLayer` 属于背景/装饰语义，可以随 `PART_TitleBar` 进入该 role。
- CSD 路径中承载默认 `WindowTitleBar` 的 `PART_TitleBarPresenter` 使用 `ElementRole="User"` 或等价 client input 语义；默认标题栏的按钮、菜单、输入框通过 `LeftAddOn` 或 `RightAddOn` 进入该路径。
- CSD 且 `IsTitleBarVisible=false` 时保持 `WindowDecorations.Full`；`WindowDrawnDecorationsTheme` 用 `HasTitleBar && IsTitleBarVisible` 隐藏 `PART_TitleBar`、shadow 和 `PART_TitleBarPresenter`，不能通过 `BorderOnly` 删除平台状态转换所需的完整窗口装饰能力。
- Avalonia 的 `WindowDecorationMargin.Top` 同时包含 drawn title-bar、frame 和 shadow。Window 因此发布 `EffectiveContentFrameMargin`：标题栏可见或非 CSD 时直接等于 `WindowDecorationMargin`；CSD 且标题栏隐藏时只从 Top 扣除实际 `WindowDrawnDecorations.TitleBarHeight`，并把结果下限限制为 `0`，Left、Right、Bottom 保持不变。drawn title-bar height 尚不可用、非有限或不大于 `0` 时保持原 margin，不能猜测 Token 高度或修改平台 title-bar hint。
- `EffectiveContentFrameMargin` 在 `WindowDecorationMargin`、`IsTitleBarVisible`、`IsCsdEnabled` 变化时重算，并在 `OnOpened` 后补算一次，确保 Avalonia drawn decorations host 已创建。CSD 内容模板只消费该投影；平台 chrome 几何仍由原始 `WindowDecorationMargin`、title-bar height hint 和 chrome manager 管理。
- 默认 `WindowTitleBar`、caption buttons、全屏弹出层和背景/装饰层职责不能混用：caption buttons 保持自身窗口操作 role，`LeftAddOn` 和 `RightAddOn` 表达默认标题栏用户交互，`TitleBarFrameLayer` 只表达背景、遮罩或装饰视觉。
- 不允许通过捕获异常、转发单个按钮 `Click`、延迟重新命中或给特定 Demo 写特殊判断来让 `TitleBarFrameLayer` 支持交互；这会模糊背景层和标题栏交互层的职责。

目标结构按以下模型维护：

| 路径 | 背景/装饰层 | 拖拽 / 默认标题栏层 | 用户交互层 |
| --- | --- | --- | --- |
| 非 CSD `WindowTheme.axaml` | `TitleBarFrameLayer` / `TitleBarFrameBackground` 位于默认 `WindowTitleBar` 下方，表达背景和装饰。 | `WindowTitleBar` 负责标题、Logo、caption buttons 和空白区域拖拽。 | `LeftAddOn` 和 `RightAddOn` 内部控件负责默认标题栏的按钮、菜单、搜索框等交互。 |
| CSD `WindowDrawnDecorationsTheme.axaml` | `PART_TitleBar` 可承载 `TitleBarFrameLayer`，并使用 `ElementRole="TitleBar"` 表达标题栏拖拽区域。 | 默认 `WindowTitleBar` 的展示由 overlay presenter 承载，平台 chrome role 不应吞掉该 presenter 内部交互。 | `PART_TitleBarPresenter` 承载默认 `WindowTitleBar`，使用 `ElementRole="User"`；`LeftAddOn` 和 `RightAddOn` 在此获得 client input。 |

实现时必须避免以下错误结构：

- 把按钮、菜单、搜索框等交互控件放入 `TitleBarFrameLayer` 或 `TitleBarFrameLayerTemplate`。
- 为了让 `TitleBarFrameLayer` 内部按钮可点，把背景/装饰层提升到 `TitleBar` 上方或改成 `ElementRole="User"`。
- 给 `TitleBarFrameLayer` 的根节点设置可绘制透明背景后尝试承载整条标题栏交互；这会抢占拖拽区域并混淆职责。
- 为了让某个按钮可点，在 code-behind 中手动转发 pointer 或 click；这会绕过 Avalonia 原生输入、焦点和命令语义。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- 内容、命令和视觉状态在模板节点之间的同步。
- Caption requested visibility、capability、WindowState 和平台支持到 effective button visibility 的单向投影，以及 caption command 到 Window 操作入口的返回路径。
- Window 创建 host projection lease、WindowTitleBar 持有并释放 lease 的职责分离，以及默认内容配置与通用宿主上下文的独立生命周期。
- 标题栏背景/装饰层、自定义 `TitleBar` 与 Avalonia CSD chrome hit test 的职责划分。
- 原始 `WindowDecorationMargin` 到 `EffectiveContentFrameMargin` 的单向几何投影；隐藏 CSD 标题栏只释放内容区的 title-bar reservation，不改变平台装饰能力或 frame/shadow reservation。
- 完整 layer、visible frame 和 content bounds 的职责划分；`WindowVisualLayerClip.CalculateClipBounds` 是排除 client-drawn frame shadow 的共享计算入口。
- 状态变化时避免创建不必要的视觉对象、订阅或动画对象。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

### 7.1 首次显示主题表面准备

`Window` 的所有 AtomUI `Show` / `ShowDialog` 入口共用一条平台无关的准备路径：

```text
Resolve owner ThemeContext
  -> attach ThemeContextLease and publish RequestedThemeVariant
  -> synchronously resolve WindowToken.DefaultBackground from that scoped Snapshot
  -> set temporary Background and TransparencyBackgroundFallback values at Template priority
  -> prepare platform chrome geometry
  -> call Avalonia base.Show / base.ShowDialog
  -> release temporary surface values
  -> keep ThemeContextLease until close; WindowTheme owns subsequent theme updates
```

实现必须满足以下约束：

- 资源读取发生在 `ThemeContextLease` 的资源桥挂载之后，确保 owner 局部主题和 Window control token override 优先于根主题。
- 显示调用是同步临界区，首帧背景只做一次 Snapshot-backed 资源读取和临时属性赋值，不创建 `DynamicResource`、resource observable 或短生命周期 Token 订阅。
- 临时属性值使用 `BindingPriority.Template` 或等价的低优先级可释放值帧，因此不得覆盖用户 local value；释放时也不得影响用户值。
- `base.ShowDialog` 返回窗口生命周期 `Task` 后即可释放首帧临时值，不得让临时值存活到 Dialog 关闭。
- `base.Show` / `base.ShowDialog` 抛出异常时，释放本次新建的临时值和 ThemeContext lease，不保留资源桥、事件订阅或错误 owner。
- 不通过提前应用整套 ControlTheme 改变 `WindowOpenedEvent` 前所有 Window Setter 的可观察时序；共享准备阶段只处理平台可见前不可缺少的 theme context、variant 和 surface background。
- Windows、macOS、X11 和 Wayland chrome manager 不参与 Token 解析。只有在实机证据证明 managed 首帧已经正确但特定后端仍显示原生空白 surface 时，才允许在统一接口后增加平台后备。

回归验证至少覆盖：根 Dark 主题在 `WindowOpenedEvent` 前的背景、owner 局部 ThemeContext、用户显式背景不被覆盖、关闭后 lease/资源桥释放，以及失败显示的回滚。Headless 测试只能证明 managed 状态顺序；Windows、macOS 和 Linux 的最终首帧必须通过各平台实机显示或录屏验证。

### 7.2 不透明表面的动态 fallback 背景

`WindowTheme` 负责通过 Token 资源更新 `Background` 和 `TransparencyBackgroundFallback`，用户 local value
继续优先。实际透明度为 `None` 时，`PART_TransparencyFallback` 必须同步当前有效的
`TransparencyBackgroundFallback`；透明表面仍由 Avalonia 清空该部件背景。

Avalonia `TopLevel` 在应用模板和透明度变化时以 local value 写入该 Border 的 `Background`，普通
ControlTheme Setter 和 `TemplateBinding` 无法覆盖这条赋值路径。`Window` 因此只补充 fallback 属性变化时
的同步，不新增资源订阅，不参与 Token 解析，也不接管透明度切换。部件引用在模板重套用时替换、关闭时释放；
没有该部件的自定义模板保持自身绘制行为。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- TitleBar 宿主发现只使用逻辑祖先和强类型 AvaloniaProperty binding，不使用全局 Window registry、反射或字符串 binding path；状态变化复用现有 lease，template reapply 不重复创建 lease。
- 首次显示主题表面使用一次性 Snapshot 读取，不为同步 `Show` 临界区创建资源 observable 或订阅。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。

## 9. 维护不变量

维护 Window 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Caption visibility 与 capability 分离；隐藏 managed button 不修改 `CanMinimize`、`CanMaximize` 或其他窗口操作入口。
- Window 定义 title-bar host projection，WindowTitleBar 拥有每次连接的 lease；同一 Window 支持多个标题栏，detach、宿主切换和 Window close 必须释放旧 lease。
- 子 Window 的主窗口 Logo/Icon 回退订阅只在 Window 已打开且依赖回退时存在，本地来源接管或子 Window close 必须释放；从未显示的 Window 不得持有主窗口订阅。两个全屏标题宿主统一消费 Window effective 标题与 Logo。
- 默认标题栏内容投影与通用宿主投影保持分离；所有已连接标题栏获得拖动、双击最大化和 caption 操作，只有默认标题栏获得尺寸提示和 CSD chrome 几何协作。
- CSD 隐藏默认标题栏时保持 `WindowDecorations.Full`，由平台窗口管理器负责最小化/恢复和最大化/还原的原生转换及可用动画。
- CSD 隐藏默认标题栏时，内容区必须通过 `EffectiveContentFrameMargin` 消除实际 drawn title-bar 高度的占位，同时保留 frame/shadow margin；不得通过把 height hint 设为 `0`、硬编码 Token 高度或修改 `WindowDecorations` 来消除空白。
- Template part 名称、ControlTheme key、伪类和资源 key。
- `TitleBarFrameLayer` 的背景/装饰层语义，以及标题栏交互内容必须通过 `TitleBar` 承载的职责边界。
- 上层 Dialog/Drawer 不按 OS 或 CSD 状态复制 Window frame 几何，而是消费 Window 发布的 `FrameShadowThickness`、visible frame 和引用计数 chrome suppression lease。
- `WindowDrawnDecorations` overlay 只包含 chrome；Dialog/Drawer 仍在 owning Window `TopLevel` 内，多个 owner 的 suppression lease 必须在最后一次释放后才恢复 chrome。
- 所有桌面平台共用 Window 首次显示主题表面准备流程，`WindowTheme` 是显示完成后的唯一长期背景所有者。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- CSD 标题栏可见性或 frame geometry 变更运行 `WindowResizeArtifactTests`，覆盖 effective content margin 计算、模板绑定和完整装饰契约。
- Dialog/Drawer 与 popup 分层变更运行 Dialog、Drawer、Window 回归、Popup 原语与控件家族矩阵，以及 DataGrid Popup 专项测试；真实桌面交互证据按平台独立记录。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。

本专项当前实机证据为 Windows、macOS 和 Ubuntu GNOME Wayland 已测试；Wayland 证据仅覆盖 Dialog
内容 Popup 真实窗口人工回归，不包含 Drawer。Linux X11 仍未测试。Headless 结果只能证明 managed 层级与生命周期，
不能替代未执行平台或未执行控件的真实窗口、chrome、pointer 和 focus 验证。
