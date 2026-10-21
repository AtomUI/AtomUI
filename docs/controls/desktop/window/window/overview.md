# Window 桌面版架构设计

本文档定义 `Window` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Window 桌面版实现原理](implementation.md)，Window 与标题栏的对齐协作见 [WindowTitleBar 实现原理](../window-title-bar/implementation.md)，caption button 的能力与呈现模型见 [WindowTitleBar Caption Button 配置设计](../window-title-bar/caption-button-configuration-design.md)，Window Token 的专项设计见 [Window Token 设计](token.md)，设计和契约变化记录见 [Window Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/General/Window` |
| 控件状态 | Stable |

Window 是 AtomUI 桌面控件体系中的桌面窗口控件，用于提供 AtomUI 自绘窗口、平台窗口能力和主题集成入口。

Window 不负责普通内容控件、Dialog 弹层或业务路由容器。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Window`

## 2. 设计语言

Window 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Window 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Window 是 AtomUI 桌面控件体系中的桌面窗口控件，用于提供 AtomUI 自绘窗口、平台窗口能力和主题集成入口。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `ContentFrameBackground`、`ContentFrameLayer`、`ContentFrameLayerOpacity`、`ContentFrameLayerTemplate`、`IsTitleBarVisible`、`LogoTemplate`、`TitleBarFrameBackground`、`TitleBarFrameLayer` 等 11 项。`TitleBarFrameLayer` 表示标题栏背景或装饰层，不作为按钮、菜单、搜索框等交互控件入口。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Window Token + ControlTheme。 |

## 3. API 与契约模型

Window 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ContentFrameBackground`、`ContentFrameLayer`、`ContentFrameLayerOpacity`、`ContentFrameLayerTemplate`、`IsTitleBarVisible`、`LogoTemplate`、`LeftAddOn`、`LeftAddOnTemplate`、`RightAddOn`、`RightAddOnTemplate`、`TitleBarFrameBackground`、`TitleBarFrameLayer`、`TitleBarFrameLayerOpacity`、`TitleBarFrameLayerTemplate` 等 | 定义控件展示内容、输入数据、模板或业务对象入口；`LeftAddOn` 和 `RightAddOn` 用于默认标题栏中的交互内容，`TitleBarFrameLayer` 仍只表示标题栏背景或装饰层。 |
| 选择与集合 | `ViewModel` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsMinimizeCaptionButtonVisible`、`IsMaximizeCaptionButtonVisible`、`IsCloseCaptionButtonVisible`、`IsFullScreenCaptionButtonVisible`、`IsPinCaptionButtonVisible`、`IsMoveEnabled` | 表达 managed caption button 呈现、窗口移动和用户可观察状态；visibility 不替代窗口 capability。 |
| 弹层与窗口 | `WindowFrameLayer`、`WindowFrameLayerOpacity` | 控制 popup、flyout、dialog、window 或 overlay 宿主协作。 |
| 其他稳定入口 | `Logo`、`LogoVisibility`、`IsTitleVisible`、`MediaBreakPoint`、`OsType`、`OsVersion`、`TitleAlignment` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。`IsTitleVisible` 只控制默认标题栏内的标题文字呈现（`WindowTitleBar` AddOwner facade），不影响系统级窗口标题；与 `IsTitleBarVisible`（隐藏整条标题栏）作用域不同。`Logo`/`LogoTemplate` 只承载显式值：未设置时标题栏渲染层回退到 `Icon`（再回退主窗口 Logo/Icon），运行时设为 `null` 回到默认，彻底隐藏用 `LogoVisibility=Never`。 |

当前没有抽取到控件专属 public 事件；交互通知主要来自继承事件、命令或 Gallery 可观察状态。

主要 public 类型与枚举：

- 类型：`Window`、`ReactiveWindow<TViewModel>`、`MacStandardWindowButtons`。
- 枚举：无。

Window 模板还使用 `FullscreenPopoverLayer`、`WindowResizer`、`WindowVisualLayerClip` 等 internal
协作类型。它们会影响主题和可观察窗口行为，但不是用户可直接依赖的 public API。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_CloseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_ContentPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_FullScreenButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_FullscreenPopoverLayer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_OverlayWrapper` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_PopoverBorder` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_PopoverCloseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_PopoverFullScreenButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_RootLayout` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_TitleBar` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_TitleBarPresenter` | `?` | 展示用户内容、文本、图标或模板化数据。 |
| `PART_TransparencyFallback` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_UnderlayWrapper` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_VisualLayerManager` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_WindowFrame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_WindowResizer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

Window 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- open/close 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。
- 默认标题栏的交互内容通过 `LeftAddOn`、`RightAddOn` 及其模板属性承载；`TitleBarFrameLayer` 只表达标题栏背景、遮罩或装饰视觉，不保证内部控件获得 pointer、focus、keyboard 或 command 事件。

Caption button 的 requested visibility 与窗口 capability 分离：Minimize、Maximize 和 Close 默认请求显示，FullScreen 和 Pin 默认隐藏。设置 visibility 为 `false` 只隐藏 AtomUI managed button，不修改 `CanMinimize`、`CanMaximize`、`WindowState`、`Topmost` 或其他窗口操作入口；capability 为 `false` 时对应 managed button 保持隐藏。完整模型见 [WindowTitleBar Caption Button 配置设计](../window-title-bar/caption-button-configuration-design.md)。

Window 为逻辑树内每个 `WindowTitleBar` 定义相同的宿主上下文投影，包括 caption 配置、窗口能力、WindowState、active state、Topmost、平台/CSD 输入和窗口操作命令。投影由 Window 创建为可释放 lease，由各标题栏实例分别持有；Window 不以单例 binding 容器限制一个窗口只能接入一个标题栏。

## 5. 视觉与主题模型

Window 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `FullscreenPopoverLayerTheme.axaml` | 定义 macOS 全屏标题栏 popover 的固定模板、caption buttons 和标题展示。 |
| `WindowDrawnDecorationsTheme.axaml` | 定义 Avalonia drawn decorations overlay 下的标题栏、caption buttons、shadow、visible frame 裁剪结构，以及 Windows 10 的 1px 顶边 frame 补偿；该 overlay 不承载 Dialog、Drawer 或其他业务 presentation。 |
| `WindowResizerTheme.axaml` | 定义 managed resize grip 的八向命中区域。 |
| `WindowTheme.axaml` | 定义普通 Window 模板、标题栏、内容 frame、visual layer、overlay host、fullscreen popover 和 managed resizer。 |

Window 使用 `WindowToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

### 5.1 标题栏背景层与自定义 TitleBar 模型

Window 标题栏按职责拆分为背景/装饰层、默认标题栏层和自定义标题栏层三类稳定语义。该模型同时约束普通自绘模板和 Avalonia `WindowDrawnDecorations` CSD 模板：

| 语义层 | 代表入口 | 职责 | 命中语义 |
| --- | --- | --- | --- |
| 标题栏背景/装饰层 | `TitleBarFrameBackground` / `TitleBarFrameLayer` / `TitleBarFrameLayerTemplate` | 提供标题栏背景、遮罩、纹理、圆角、裁剪或装饰视觉。 | 不作为用户交互入口；CSD 下可处于标题栏拖拽 role 中。 |
| 默认标题栏层 | `WindowTitleBar` | 展示标题、Logo、`LeftAddOn`、`RightAddOn` 与 caption buttons，并在空白区域提供窗口拖拽语义。 | add-on 与 caption buttons 按普通 Avalonia client input 语义命中；空白区域保留标题栏交互。 |
| 自定义标题栏层 | `NotifyCreateTitleBar` / `NotifyConfigureTitleBar` 扩展点 | 承载需要替换默认标题栏组成或行为的派生窗口实现。 | 派生窗口负责其自定义标题栏的 client input 与空白区域拖拽策略。 |
| 内容区标题栏 | Window 内容逻辑树中的 public `WindowTitleBar` | 自动消费最近 Window 的 caption 状态和操作命令，并在空白区域提供窗口拖动和双击最大化/还原语义。 | 不提供标题栏高度提示，也不取得唯一 CSD chrome role。 |

维护标题栏模板时，不应把 `TitleBarFrameLayer` 提升为可交互覆盖层。需要向默认标题栏加入按钮、菜单或搜索框时，使用 `LeftAddOn` 或 `RightAddOn`；只有需要替换整个标题栏组成或行为时，才在派生 `Window` 中重写标题栏创建与配置扩展点。`Window.TitleBar` 是模板生命周期拥有的 internal 状态，不作为应用 API 公开。

Window 的 title-bar host projection 服务默认、派生和内容区标题栏，并由每个标题栏的 logical attach/detach 生命周期持有。该 lease 同时包含 caption 状态/命令投影和窗口拖动、双击请求的交互订阅。默认标题栏另行消费 Window facade：`Window.TitleAlignment` add-owner `WindowTitleBar.TitleAlignmentProperty`，`LeftAddOn`、`LeftAddOnTemplate`、`RightAddOn` 和 `RightAddOnTemplate` 同样 add-owner 对应标题栏属性，并以 `Template` 优先级单向投影。派生标题栏以 local value 提供的内置操作区优先于 Window facade。`NotifyConfigureTitleBar` 只扩展这组默认内容配置，不负责通用宿主发现或 caption command 接入。Window 提供内容、平台、CSD、WindowState 和原生 chrome 安全区，但不实现标题排列公式。

CSD 模式下，`IsTitleBarVisible=false` 只隐藏 AtomUI drawn title-bar frame、shadow 和 presenter，不把 `WindowDecorations` 从 `Full` 降级为 `BorderOnly`。内容区同时移除 Avalonia drawn title-bar 对顶部 decoration margin 的占位，但继续保留 frame 和 shadow margin，因此用户内容可以到达窗口顶部且不破坏可调整大小边框。窗口最小化、最大化和恢复继续通过 Avalonia `WindowState` 表达，由 Windows DWM、macOS AppKit 或 Linux 窗口管理器/合成器在平台支持范围内执行原生状态转换和动画；AtomUI 不伪造窗口缩放动画，也不为 caption button 建立平台专用状态旁路。macOS 非 CSD 且隐藏原生标题栏时仍可使用 `BorderOnly`，该分支不改变 CSD 契约。

默认标题栏的 add-on 可直接使用 AXAML 属性元素配置：

```xml
<atom:Window>
  <atom:Window.LeftAddOn>
    <Button Content="Back" />
  </atom:Window.LeftAddOn>
  <atom:Window.RightAddOn>
    <Button Content="Settings" />
  </atom:Window.RightAddOn>
</atom:Window>
```
完整协作模型见 [WindowTitleBar 实现原理](../window-title-bar/implementation.md)。

### 5.2 跨平台首帧主题表面模型

Windows、macOS 和 Linux 共用同一个首次显示主题契约：平台窗口进入可见状态前，`Window` 必须已经获得目标
`ThemeContext`、与该 context 一致的 `RequestedThemeVariant`，以及当前 Window Token scope 中的首帧背景。
该契约由 `Window` 的共享显示生命周期负责，不属于 Win32、AppKit、X11 或 Wayland chrome manager 的职责。

首次显示按以下所有权模型维护：

- `WindowTheme.axaml` 是窗口背景的唯一长期视觉所有者，`WindowToken.DefaultBackground` 是默认背景语义真源。
- `Show` 和 `ShowDialog` 的所有 AtomUI 入口必须先解析 owner 作用域并挂载可释放的 `ThemeContext` lease，再进入 Avalonia 的平台显示流程。
- 在正式 ControlTheme 接管前，`Window` 从已提交的当前作用域 Snapshot 同步读取一次默认背景，并以低于用户 local value 的优先级临时预热 `Background` 与 `TransparencyBackgroundFallback`。
- 首帧预热只覆盖同步显示临界区，不订阅资源变化；Avalonia 完成同步样式应用后立即释放临时值，由正式 ControlTheme 继续响应主题切换。
- 用户显式设置的 `Background` 或 `TransparencyBackgroundFallback` 始终优先，首帧预热不得改写或清除用户 local value。
- 平台 chrome manager 只处理窗口装饰、原生几何和平台能力投影，不得分别复制 ThemeContext、Token 查找或首帧背景算法。

如果上述 managed 状态在平台显示前已经正确，某个平台仍然暴露尚未提交内容的原生空白 surface，则该问题属于平台后端边界。此时应通过统一的平台能力接口提供最小后备实现，并分别验证对应后端；不得把平台消息、延时显示或透明度切换混入共享主题状态机。

## 6. 控件家族或集成关系

Window 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `Window`：public 窗口控件，持有窗口 public API、主题上下文、平台状态投影和模板生命周期。
- `ReactiveWindow<TViewModel>`：public ReactiveUI 窗口基类，维护 ViewModel 同步和 activation。
- `MacStandardWindowButtons`：public macOS 标准窗口按钮布局附加能力。
- `Window/Chrome/*WindowChromeManager`：internal 平台 manager，负责 CSD/SSD、frame geometry、X11/Wayland/Windows 原生能力投影。
- `Window/Utils/FullscreenPopoverLayer`：internal macOS 全屏标题栏 popover 协作层。
- `Window/Utils/WindowResizer`：internal managed resize grip 协作控件。
- `Window/Utils/WindowVisualLayerClip`：internal visible frame 裁剪计算和模板裁剪 helper。
- `Window/Utils/WindowDrawnDecorationsReflectionExtensions`：internal Avalonia drawn decorations 反射边界。
- `AtomUI.Native` 的 `WindowExtensions` 与平台 `WindowUtils`：internal 底层平台调用入口，不拥有 Window 主题、订阅或策略。
- `WindowTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `WindowToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Window 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- `TitleBarFrameLayer` 是标题栏背景/装饰入口，不是标题栏用户交互入口；默认标题栏按钮、菜单、搜索框等交互内容必须通过 `LeftAddOn` 或 `RightAddOn` 承载。
- 逻辑树内每个 `WindowTitleBar` 都获得独立、可释放的 Window host projection；detach、宿主切换和 Window close 后不得保留旧 Window。
- 所有已连接标题栏共享拖动、双击最大化和 caption 操作语义；只有 Window 模板正式接入的默认标题栏提供标题栏高度提示和 CSD chrome 几何协作。
- CSD 隐藏 AtomUI 默认标题栏时保持 `WindowDecorations.Full`，只隐藏 managed title-bar visual，并从内容区顶部边距移除实际 drawn title-bar 高度；不能以 `BorderOnly`、清零平台 title-bar hint 或硬编码 Token 高度破坏平台装饰几何与原生窗口状态转换能力。
- Windows、macOS 和 Linux 共用同一套首次显示主题表面流程；平台可见前必须同步准备 ThemeContext、variant 和 Window Token 背景，正式显示后由 `WindowTheme` 单独持有长期主题状态。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

Window 的当前项状态必须由单一 owner 推导。public 选择属性、集合项容器和伪类之间只能做单向同步，集合替换、清空和模板重套用时必须回放当前状态。

### 8.2 弹层与宿主模型

Window 涉及弹层、窗口或 overlay 宿主时，打开状态、取消事件、定位和宿主释放必须保持一致。重复打开、关闭、窗口失活和 template reapply 都必须释放旧宿主引用。

Window 对上层 overlay 发布三种不可混用的几何语义：

- 完整 layer bounds：保持 TopLevel client surface 坐标系，供 mask 和 layer 自身使用。
- visible frame bounds：完整 layer 只排除 `FrameShadowThickness`，仍包含 managed/drawn 标题栏，供 Dialog Surface、Drawer 和需要贴合可见窗口边缘的 overlay 使用。
- content bounds：在 visible frame 基础上排除标题栏和内容装饰，供普通 Window 内容或明确要求正文安全区的反馈使用。

`WindowVisualLayerClip` 是 visible frame 外轮廓计算真源。Window 的平台 manager 只负责把 X11、Wayland、Win32 和 macOS 原生能力投影为 `FrameShadowThickness`、CSD 状态和 chrome 几何，不允许上层控件再次按 OS 复制几何算法。

`WindowDrawnDecorations` overlay 只拥有 drawn chrome。Modal Dialog 与 Window Drawer 的业务视觉始终留在 owning Window
`TopLevel` 内；它们活跃时各自从 Window 获取引用计数的 chrome suppression lease，最后一个 lease 释放后才恢复 drawn
chrome。这样 Dialog/Drawer 内容中的 Popup、Flyout、ToolTip 与 ContextMenu 仍由 owning Window 的 Avalonia popup 服务管理，
完整不变量与家族矩阵见 [Modal 内容弹层叠放设计](../../feedback/modal/popup-layering-design.md)。

## 9. 文档导航与验证策略

关联文档：

- [Window 桌面版实现原理](implementation.md)
- [WindowTitleBar Caption Button 配置设计](../window-title-bar/caption-button-configuration-design.md)
- [Window Token 设计](token.md)
- [Window Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 open/close、disabled、hover、pressed、focus 以及控件特有状态。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| 首次显示主题表面 | 覆盖根窗口和 owner 局部主题、Light/Dark、用户显式背景优先级、显示失败回滚，并在 Windows、macOS 和 Linux 实机检查首帧。 |
| Dialog/Drawer 与 Popup 分层 | 运行 Desktop Controls Dialog/Drawer/Window 回归、Popup 原语与控件家族矩阵、DataGrid Popup 专项测试，并按平台独立记录真实桌面交互证据。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |

当前 Dialog/Drawer 与 Popup 分层实机证据：

| 平台 | 状态 | 已验证范围 |
| --- | --- | --- |
| Windows | 已测试 | CSD Window 下 drawn chrome suppression、owning TopLevel presentation 与 Dialog 内容 Popup 基本交互。 |
| macOS | 已测试 | 原生 chrome Window 下 owning TopLevel presentation 与 Dialog 内容 Popup 基本交互。 |
| Linux Wayland (GNOME) | 已测试 | Ubuntu 26.04、GNOME Shell 50.1 下完成 Dialog 内容 Popup 真实窗口人工回归；不包含 Drawer。 |
| Linux X11 | 未测试 | 尚无本专项实机证据，不能标记为通过。 |
