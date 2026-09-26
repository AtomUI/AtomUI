# WindowTitleBar 桌面版架构设计

`WindowTitleBar` 是 `AtomUI.Desktop.Controls` 中用于构成桌面窗口标题栏的模板化控件。本文档定义控件的设计定位、公共契约、状态模型、模板语义和集成边界。AddOn 按钮控件族见 [WindowTitleBar AddOn 按钮设计](window-title-bar-addon-buttons-design.md)，系统 caption button 的能力与呈现模型见 [WindowTitleBar Caption Button 配置设计](caption-button-configuration-design.md)，内部实现与跨平台标题布局见 [WindowTitleBar 实现原理](implementation.md)，视觉变量见 [WindowTitleBar Token 设计](token.md)，契约变化见 [WindowTitleBar Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/General/Window` |
| 控件状态 | Stable |

`WindowTitleBar` 负责以下内容：

- 展示窗口 Logo、标题以及标题模板。
- 承载标题栏左右两侧的应用自定义内容。
- 为系统 caption buttons 提供统一交互表面；连接到 Window 后同时参与窗口拖动和双击最大化。
- 自动发现最近的 AtomUI `Window`，接收平台、激活状态、窗口状态和操作命令，并投影为稳定的模板状态。
- 在不同平台和窗口装饰模式下保持标题、原生按钮、managed buttons 与 add-on 互不覆盖。

`WindowTitleBar` 不是通用工具栏或导航栏。业务操作应放入 `LeftAddOn`、`RightAddOn` 或专用控件，并保留标题栏拖动区域和系统窗口操作的优先级。需要复用 AtomUI managed caption visual 的业务按钮使用 `WindowTitleBarButton` 或 `WindowTitleBarToggleButton`；这两个控件不承载系统窗口操作。

## 2. 设计语言

标题栏由四类语义内容构成：

| 语义 | 内容 | 责任 |
| --- | --- | --- |
| Leading | Windows/Linux: `Logo + LeftAddOn`；macOS: `LeftAddOn` | 承载靠近起始侧的应用操作，并占用标题安全空间。Windows/Linux 中可见 Logo 位于物理最左侧，先于 `LeftAddOn`；两者同时有效时使用 `LogoAndLeftAddOnSpacing` 分隔。 |
| Title | Windows/Linux: `Title`；macOS: `Logo + Title` | 承载标题内容并执行对齐和裁剪。macOS 保留连续 Logo/Title 标题组以配合原生窗口按钮安全区。 |
| Trailing | `RightAddOn + CaptionButtonGroup` | 承载结束侧应用操作和 managed window operations。 |
| Native chrome | 平台原生窗口按钮或 overlay | 不进入 visual tree，通过窗口边缘安全区参与布局。 |

三块 managed 区域与 native chrome 的完整几何关系由本文第 8 节和 [WindowTitleBar 实现原理](implementation.md) 定义。Windows/Linux 的 Logo 属于 Leading，占用左侧操作安全空间；Logo 与 `LeftAddOn` 的条件间距同样属于 Leading 实测宽度。macOS 的 Logo 属于 Title，以保持平台标题行为。左右 add-on 属于操作区，不参与标题中心计算。

控件家族的职责边界：

| 类型 | 可见性 | 职责 |
| --- | --- | --- |
| `WindowTitleBar` | public | 公共内容契约、宿主状态投影、交互入口和主题入口。 |
| `WindowTitleBarButton` | public | 在 LeftAddOn/RightAddOn 中提供与 managed caption visual 一致的普通图标按钮。 |
| `WindowTitleBarToggleButton` | public | 在 LeftAddOn/RightAddOn 中提供与 managed caption visual 一致的 checked/unchecked 图标按钮。 |
| `WindowTitleBarLogoVisibility` | public | 定义 Logo 的显示策略。 |
| `WindowTitleBarTitleAlignment` | public | 定义标题组的跨平台对齐语义。 |
| `CaptionButtonGroup` | internal | 根据宿主投影的能力、requested visibility 和窗口状态推导 effective visibility，并把固定窗口操作转发给宿主命令。 |
| `CaptionButton` | internal | 承载系统 caption icon、checked icon 和按钮视觉状态。 |
| `WindowsCaptionButton` | internal | 提供 Windows 方形 caption button 尺寸和 hover 状态修正。 |

internal 类型服务于 AtomUI 内置主题，不属于应用可直接创建或继承的公共控件 API。

## 3. API 与契约模型

### 3.1 标题内容

| API | 默认值 | 语义 |
| --- | --- | --- |
| `Logo` | `null` | Logo 内容对象；只承载开发者显式值，框架不写入默认值。未设置时标题栏渲染层回退到窗口 `Icon`（再回退主窗口 Logo/Icon），运行时设为 `null` 即回到该默认，彻底隐藏用 `LogoVisibility=Never`。 |
| `LogoTemplate` | `null` | Logo 数据模板；与 `Logo` 同为显式值，未设置时由渲染层的 effective 解析决定。 |
| `LogoVisibility` | `Auto` | Logo 显示策略。 |
| `Title` | `null` | 标题内容对象。 |
| `TitleTemplate` | `null` | 标题数据模板。 |
| `TitleAlignment` | `Auto` | 标题组对齐策略。 |
| `IsTitleVisible` | `true` | 是否渲染标题栏内的标题文字。设为 `false` 时仅隐藏标题栏呈现，系统级窗口标题（任务栏、窗口切换）不受影响；注意与 `IsTitleBarVisible`（隐藏整条标题栏，含 caption 按钮）区分；`LogoVisibility=Auto` 此时按无标题分支联动。 |

`Logo` 与 `Title` 都接受任意对象。字符串标题为空或仅包含空白时按“无标题内容”处理；非字符串对象按存在标题内容处理。标题文字的有效显示模型见第 4.2 节。

### 3.2 Add-on

| API | 默认值 | 语义 |
| --- | --- | --- |
| `LeftAddOn` | `null` | Leading 区域内容。 |
| `LeftAddOnTemplate` | `null` | Leading 内容模板。 |
| `RightAddOn` | `null` | Trailing 区域中位于 caption buttons 之前的内容。 |
| `RightAddOnTemplate` | `null` | Trailing 内容模板。 |

Add-on 可以包含可交互控件，也可以是 `null`、隐藏节点或当前没有孩子的容器。内置模板必须保留其命中测试能力，同时避免 Title 覆盖这些区域。Windows/Linux 仅在 `PART_Logo` 与 `PART_LeftAddOn` 两个 presenter 同时可见时产生 `LogoAndLeftAddOnSpacing`；Logo 隐藏、LeftAddOn 为 `null` 或 presenter 不可见时不保留该间距。该条件遵循 Avalonia sibling layout 的可见性语义，不根据子内容的实测宽度建立第二套状态。Leading 或 Trailing 的实测宽度为零时，该区域不占用标题安全空间，也不产生 `HeaderHorizontalSpacing`；内容出现、隐藏或动态替换后由正常 measure invalidation 重新计算。

需要与标题栏 managed caption visual 保持一致的应用操作，可以直接使用 `WindowTitleBarButton` 或
`WindowTitleBarToggleButton`。前者继承 `IconButton`，使用 `Icon`、`Command` 和标准按钮输入语义；后者继承
`ToggleIconButton`，使用 `CheckedIcon`、`UnCheckedIcon`、`IsChecked` 和标准 ToggleButton 输入语义。两个控件的
业务命令、启用状态和 checked 状态仍由应用所有，不映射到 Window 的系统操作。Windows 中两者默认占满标题栏可用高度并形成方形交互面，使用直角、透明常态背景和 Arrow 光标，业务图标继续使用 `CaptionButtonIconSize`；Linux/macOS 保持内容驱动的 30 逻辑像素方形命中面，把圆角换成 `CaptionButtonCornerRadius`，并以 `CaptionGroupSpacing` 与 caption group 分隔，Linux 额外用 `CaptionButtonBackgroundInset` 把背景收成与相邻 caption button 相同的 26 逻辑像素圆形，因此圆形按钮与相邻 managed caption button 的圆角、可见尺寸和间距同源。按钮容器内部间距由应用内容所有，Windows 中需要连续 hover 按钮带时应使用零 spacing。

### 3.3 宿主状态

| API | 语义 |
| --- | --- |
| `IsWindowActive` | 表示宿主窗口是否激活，并驱动标题和 caption buttons 的 active/inactive 视觉。 |
| `IsMotionEnabled` | 控制标题栏及 caption button 的状态过渡。 |
| `OsType` | 只读平台标识，用于选择平台主题和布局 Strategy。 |
| `OsVersion` | 只读平台版本，用于受版本约束的平台能力判断。 |

`OsType` 和 `OsVersion` 由 AtomUI 平台感知基础设施写入。应用不通过 CLR setter 修改这两个值。

### 3.4 交互事件

`MaximizeWindowRequested` 表示标题栏收到有效的主按钮双击请求。`WindowTitleBar` 不直接修改 `WindowState`；宿主 `Window` 根据 `CanResize`、`CanMaximize` 和当前状态决定最大化或还原。

事件在对应的 `PointerReleased` 阶段发出，避免窗口同步 resize 破坏当前 pointer capture。pointer capture 丢失或释放条件不匹配时，请求被取消。

每个连接到 AtomUI `Window` 的 `WindowTitleBar` 都是窗口交互表面：空白区域可按 `Window.IsMoveEnabled` 启动窗口拖动，主按钮双击可请求 Normal/Maximized 切换。该交互连接与 caption 状态投影由同一个 host lease 管理；标题栏离开逻辑树、切换宿主或 Window 关闭后不再操作旧宿主。标题栏高度提示和 CSD 几何仍只由 Window 模板正式接入的默认标题栏提供。

## 4. 行为与状态模型

### 4.1 Logo 显示模型

`LogoVisibility` 的三个值具有以下语义：

| 值 | 规则 |
| --- | --- |
| `Always` | 存在 `Logo` 或 `LogoTemplate` 时显示。 |
| `Never` | 始终隐藏。 |
| `Auto` | 根据标题内容、平台和窗口状态计算。 |

`Auto` 的计算矩阵：

| 条件 | 结果 |
| --- | --- |
| 不存在 Logo 内容和 Logo 模板 | 隐藏。 |
| 存在有效标题内容且标题未被 `IsTitleVisible=False` 隐藏 | 显示。 |
| 无标题内容且平台为 macOS | 隐藏。 |
| 无标题内容、平台不是 macOS、窗口非全屏 | 显示。 |
| 无标题内容、平台不是 macOS、窗口全屏 | 隐藏。 |

该模型只控制 Logo 的有效可见性，不修改 `Logo`、`Window.Icon` 或应用图标来源。

每个 `WindowTitleBar` 先解析自己的显式 `Logo` / `LogoTemplate`；两者都未设置时，才消费宿主 `Window` 解析出的有效 Logo。因而独立标题栏和窗口内的自定义标题栏都保留自身公共内容契约，宿主 `Icon` 与主窗口 Logo/Icon 仅作为回退，不覆盖标题栏的显式值。

### 4.2 标题显示模型

`IsTitleVisible` 控制标题栏内的标题文字呈现，与 `Window.IsTitleBarVisible`（隐藏整条标题栏）作用域不同：

| `IsTitleVisible` | `Title` | 有效可见性 |
| --- | --- | --- |
| `true`（默认） | `null` | 隐藏。 |
| `true`（默认） | 非 `null`（含空字符串） | 显示；空字符串渲染空内容，与历史行为逐位一致。 |
| `false` | 任意 | 隐藏；系统级窗口标题（任务栏、窗口切换）不受影响。 |

- 有效值计算为 `IsEffectiveTitleVisible = IsTitleVisible && Title is not null`；三平台模板的 `PART_ContentPresenter` 与全屏层 `FullscreenTitleText` 只绑定该 internal direct property（全屏层绑定 Window 侧 `IsEffectiveFullscreenTitleVisible`）。
- `IsTitleVisible=False` 时 `LogoVisibility=Auto` 按“无标题”分支联动：macOS 上 Logo 随标题一起隐藏，全屏层同步；非 macOS 非全屏时 Logo 仍显示。`Always`/`Never` 不受影响。
- `Window` 通过 `NotifyConfigureTitleBar` 投影 `IsTitleVisible`，与 `LogoVisibility` 同优先级。

### 4.3 窗口状态

`WindowTitleBar` 从宿主 `Window` 的单向属性投影接收状态并维护以下伪类：

| 伪类 | 条件 |
| --- | --- |
| `:active` | 宿主窗口处于激活状态。 |
| `:normal` | `WindowState.Normal`。 |
| `:minimized` | `WindowState.Minimized`。 |
| `:maximized` | `WindowState.Maximized`。 |
| `:fullscreen` | `WindowState.FullScreen`。 |

窗口激活状态同时写入 `IsWindowActive`，供模板中的内部协作控件使用。状态 owner 始终是宿主 `Window`；模板节点不反向维护第二份窗口状态。

标题栏在进入逻辑树时自动选择最近的 AtomUI `Window` 作为宿主，并为自身持有一个可释放的 host projection lease。同一 Window 可以包含多个 `WindowTitleBar`，每个实例都独立接收同一宿主状态；标题栏从逻辑树移除或转移到另一 Window 时，旧投影必须释放并由新宿主重新建立。

### 4.4 Caption buttons

caption button 的公共配置属于宿主 `Window`：

- `CanMinimize`、`CanMaximize` 和平台能力决定操作是否允许，不承担 managed button 的呈现配置。
- `IsMinimizeCaptionButtonVisible`、`IsMaximizeCaptionButtonVisible`、`IsCloseCaptionButtonVisible`、`IsFullScreenCaptionButtonVisible` 和 `IsPinCaptionButtonVisible` 分别表达 managed button 的 requested visibility。
- `Topmost`、`WindowState`、平台 backend、requested visibility 和 operation capability 共同决定 checked state 与 effective visibility。

Minimize、Maximize 和 Close 默认显示，FullScreen 和 Pin 默认隐藏。全屏时隐藏最小化和最大化按钮；最大化时隐藏进入全屏按钮；capability 为 `false` 时不显示不可执行的 managed button。Wayland backend 不提供置顶按钮。隐藏按钮不修改 `CanMinimize`、`CanMaximize`、`Topmost` 或其他窗口操作入口。完整状态矩阵、平台边界和单向命令流见 [WindowTitleBar Caption Button 配置设计](caption-button-configuration-design.md)。

### 4.5 拖动和双击

`Window` 通过每个标题栏的 host lease 监听 pointer 事件，并在移动距离超过拖动阈值后调用原生 `BeginMoveDrag`。拖动状态记录具体来源标题栏，同一 Window 中其他标题栏的移动、释放或 capture lost 不能推进该次交互。`IsMoveEnabled=False` 或全屏状态禁止拖动。双击最大化与拖动共用标题栏输入表面，但 caption buttons 和 add-on 的已处理输入不应触发窗口拖动。

应用直接放入 Window 内容区的 `WindowTitleBar` 自动获得 caption 状态、窗口操作命令、拖动和双击最大化语义。它不参与标题栏高度提示、CSD 最小高度或唯一 CSD geometry owner 计算；这些几何职责只属于 Window 模板正式接入的默认标题栏。

## 5. 视觉与主题模型

`WindowTitleBarTheme` 是 `WindowTitleBar` 的 ControlTheme 入口。内置主题保留以下稳定 template part 与语义节点：

| 节点 | 类型 | 契约 |
| --- | --- | --- |
| `Frame` | `Border` | 绘制标题栏背景并提供完整可见 frame 的布局边界。 |
| `PART_Logo` | `ContentPresenter` | 展示有效 Logo；Windows/Linux 模板中位于 Leading 最左侧，macOS 模板中位于 Title 内容前。 |
| `PART_ContentPresenter` | `ContentPresenter` | 展示标题，可见性绑定 `IsEffectiveTitleVisible`；字符串标题在安全宽度不足时使用字符省略号，且不参与命中测试。 |
| `PART_LeftAddOn` | `ContentPresenter` | 展示 Leading 内容并投影 AddOn active、motion 和 platform 上下文；Windows/Linux 中由 Leading 容器负责它与有效 Logo 之间的条件间距。 |
| `PART_RightAddOn` | `ContentPresenter` | 展示 Trailing add-on 并投影 AddOn active、motion 和 platform 上下文。 |
| `PART_CaptionButtonGroup` | `CaptionButtonGroup` | 消费宿主投影，推导 managed button 状态并转发固定窗口操作。 |

`PART_CaptionButtonGroup` 是 `WindowTitleBar` 模板中的稳定协作 part，通过 `TemplateBinding` 接收能力、requested visibility、窗口状态和宿主命令。其内部 `PART_CloseButton`、`PART_MinimizeButton`、`PART_MaximizeButton`、`PART_FullScreenButton` 和 `PART_PinButton` 属于 `CaptionButtonGroup` 模板，不是 `WindowTitleBar` 的 public template part。

Windows/Linux 的 Leading 容器使用 `HorizontalSpacing` 消费 `LogoAndLeftAddOnSpacing`，不通过菜单、按钮或 `PART_LeftAddOn.Margin` 补偿相邻 Logo。该组合规则使间距跟随两个 presenter 的可见性，并允许任意 `LeftAddOn` 内容获得一致的视觉隔离。macOS 的 Leading 只有 `PART_LeftAddOn`，Logo/Title 间距继续由 Title role 的 `LogoAndTitleSpacing` 管理。

平台主题可以改变 caption button 外观和 native chrome 来源，但不得改变 Public API 语义、Title/Leading/Trailing 角色或窗口操作行为。应用替换完整 ControlTheme 时负责提供等价区域、裁剪和命中测试；internal caption 类型不作为定制 API。

视觉尺寸、间距、active/inactive 颜色和 caption button 状态颜色由 [WindowTitleBar Token 设计](token.md) 管理。标题对齐值、CSD 状态和窗口状态不是 Token。

## 6. 控件家族或集成关系

### 6.1 Window

`Window` 创建默认 `WindowTitleBar`，并为逻辑树内的所有标题栏定义同一套 host projection：caption requested visibility、窗口能力、窗口状态、active state、Topmost、平台/CSD 输入和宿主命令。每个 `WindowTitleBar` 独立持有 Window 返回的 projection lease；`Window` 不使用只能服务单个标题栏的共享 binding 容器。

默认标题栏另外接收 `Window` 的标题内容、标题可见性、Logo、对齐和 add-on 配置。应用通过 `Window` 的 add-on 和 caption visibility 属性配置默认标题栏；`NotifyCreateTitleBar` 和 `NotifyConfigureTitleBar` 是派生窗口替换标题栏类型与补充默认内容配置的 protected 扩展点，不承担通用宿主发现。

`Window` 负责窗口移动、最大化/还原、原生 chrome metrics、CSD 状态和标题栏高度提示。`WindowTitleBar` 负责内容布局，不直接调用平台窗口 API。标题栏必须横跨完整可见窗口 frame；原生窗口按钮安全区作为布局输入传递，不能通过给整个标题栏添加单侧 Padding 或 Margin 来改变窗口中心。

### 6.2 ImagePreviewer

`ImagePreviewerTitleBar` 是 internal 派生标题栏。它使用显式预览图标替代 Logo，并把图片工具栏放入 Leading。预览图标与标题仍组成一个连续 Title 组，caption buttons 和右侧 add-on 仍属于 Trailing。

### 6.3 全屏标题宿主

`WindowDrawnDecorations` 和 `FullscreenPopoverLayer` 中的全屏标题宿主复用标题栏 Token、窗口操作语义和标题对齐模型。全屏宿主只计算当前实际可见的 native/managed 区域，不继承普通窗口状态下已经消失的原生按钮占位。

## 7. 兼容性不变量

- Public API 的类型、默认值、绑定语义和事件时序保持稳定。
- `Auto` Logo 规则和标题对齐的显式枚举语义保持稳定。
- `IsTitleVisible` 默认 `true`；默认路径与历史行为逐位一致（含空字符串标题语义）。设为 `false` 只影响标题栏与全屏层的标题呈现，不改变系统级窗口标题。
- `PART_CaptionButtonGroup`、内容 presenter 名称、ControlTheme key 和伪类保持稳定。
- Title 内容不参与命中测试；add-on 和 caption buttons 保持可交互。
- Windows/Linux 中 Logo 始终位于 Leading 最左侧并参与左侧安全空间；有效 Logo 与有效 `LeftAddOn` 之间使用独立的条件间距。macOS 中 Logo 和 Title 作为连续 Title 组；add-on 不进入标题中心计算。
- CSD 开关只改变 chrome metrics 来源和可见操作区，不改变显式标题对齐含义。
- 标题栏替换时释放旧的 Window 投影；template reapply 不建立逐按钮 Click handler 或 CaptionButtonGroup 到 Window 的宿主引用。
- 每个逻辑树内的 `WindowTitleBar` 自动连接最近的 AtomUI Window；detach、宿主切换和 Window close 必须释放旧 projection lease。
- 内容区标题栏与默认标题栏共享拖动、双击最大化和 caption 操作语义，但不获得标题栏高度提示或唯一 CSD chrome role。
- 平台选择和 Token 发现不依赖运行时反射或程序集扫描。

## 8. 专项模型

标题对齐使用 Leading、Title、Trailing 与 Native chrome 四类区域。`Auto` 只解析平台默认值；`Left`、`Center`、`WindowCenter` 和 `Right` 在所有平台保持同一几何语义。标题安全宽度由 managed operation、Padding 与 native chrome inset 共同约束，平台输入 owner、坐标系和共享公式见 [WindowTitleBar 实现原理](implementation.md)。

## 9. 文档导航、LLMS 导出与验证策略

关联文档：

- [WindowTitleBar 实现原理](implementation.md)
- [WindowTitleBar Caption Button 配置设计](caption-button-configuration-design.md)
- [WindowTitleBar Token 设计](token.md)
- [WindowTitleBar Changelog](changelog.md)
- [Window 控件设计](../window/overview.md)

稳定行为由以下测试覆盖：

- `WindowTitleBarLogoVisibilityTests`：Logo 默认值、平台规则、全屏规则和 Window 投影。
- `WindowTitleBarTitleVisibilityTests`：标题有效可见性计算、`IsTitleVisible` 默认值与空字符串语义、Logo Auto 联动、Window 投影和全屏层绑定。
- `WindowTitleBarLayoutPanelTests`：共享几何、Logo/LeftAddOn 条件间距、margin 单次计入、左右 add-on 动态内容、窄窗口和非法 metrics。
- `WindowTitleBarLayoutStrategyTests`：平台 `Auto`、CSD、WindowState 与 native inset 归一。
- `WindowTitleBarTokenTests`：Token 默认值、三平台 caption 视觉和 Windows edge layout。
- `WindowCaptionButtonConfigurationTests`：caption visibility 默认值、能力隔离、状态矩阵、内容区/多标题栏宿主发现、真实 pointer 双击切换、宿主切换、动态投影和模板生命周期。
- `WindowTitleBarButtonTests`：AddOn 按钮继承关系，active/motion/platform 状态投影，checked/unchecked 图标切换，detach 回退，Windows 40×40 与自定义高度方形几何，真实 pointer hover/pressed/exit 状态，macOS 与相邻 managed caption button 的正圆一致性，Linux 尺寸/圆角/光标回归，以及独立主题资产注册和指针输入隔离。
- `ImagePreviewerTitleBarThemeTests`：派生标题栏的标题组、操作区和平台模板契约。

LLMS 语义区域：

| Part | AtomUI 节点 | 职责 | 相关 API | 相关 Token | 稳定性 |
| --- | --- | --- | --- | --- | --- |
| `root` | `WindowTitleBar` | 承载公共内容契约、平台状态和标题栏主题入口。 | `Logo`、`Title`、`TitleAlignment` | `Height`、`TitleBarPadding`、标题字体与颜色 | public |
| `frame` | `Border#Frame` | 绘制标题栏背景并定义完整可见 frame。 | `Background`、`Padding` | `Height`、`TitleBarPadding` | template-stable |
| `leading` | Windows/Linux: `PART_Logo` + `PART_LeftAddOn`；macOS: `PART_LeftAddOn` | 承载起始侧应用操作并占用标题安全空间。Windows/Linux 中 Logo 是物理最左内容，且仅在 Logo 与 LeftAddOn 同时有效时产生内部间距。 | `Logo`、`LogoTemplate`、`LogoVisibility`、`LeftAddOn`、`LeftAddOnTemplate` | `LogoSize`、`LogoAndLeftAddOnSpacing`、`HeaderHorizontalSpacing` | template-stable |
| `title` | Windows/Linux: `PART_ContentPresenter`；macOS: `PART_Logo` + `PART_ContentPresenter` | 展示、测量、对齐和裁剪标题内容；macOS 同时保留 Logo/Title 连续标题组。 | `Logo`、`LogoTemplate`、`LogoVisibility`、`Title`、`TitleTemplate`、`IsTitleVisible` | `LogoAndTitleSpacing`、标题字体与颜色 | template-stable |
| `trailing` | `PART_RightAddOn` + `PART_CaptionButtonGroup` | 承载结束侧应用操作和 managed window operations。 | `RightAddOn`、`RightAddOnTemplate`；五个 Window caption visibility 属性 | `HeaderHorizontalSpacing`、caption button 尺寸、间距与状态颜色 | template-stable |
| `native-chrome` | 平台原生窗口按钮安全区 | 以逻辑像素 inset 约束标题安全空间，不进入 visual tree。 | 平台、CSD、WindowState | 不适用 | internal-observable |

LLMS 生成使用以下来源，不手工修改 `docs/AI/generated/llms` 产物。

LLMS 导出来源：

| LLMS 内容 | 来源 | 说明 |
| --- | --- | --- |
| 单控件完整文档 | `overview.md` + 源码 public surface + `token.md` | 生成 `controls/window-title-bar/index-cn.md` |
| 单控件语义文档 | `overview.md` + `implementation.md` + Themes | 生成 `controls/window-title-bar/semantic-cn.md` |
| API 表 | 源码 public surface | 不在 `overview.md` 机械复制完整表 |
| Design Token 表 | `WindowTitleBarToken.cs` + `token.md` | 不手工维护第二份生成表 |
| 示例 | AtomUIGallery `General / Window` + source snippet catalog | `window-title-visibility` 是稳定示例键；临时验收 Demo 不进入生成来源 |
| 源码索引 | `implementation.md` | 用于定位控件源码、主题和测试 |

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档 | 运行 LLMS `verify`、`git diff --check` 并检查相对链接。 |
| Public API 与状态 | 覆盖属性默认值、Logo 规则、窗口状态、事件时序和 Window 投影。 |
| Theme 与 Template | 检查三平台结构、稳定 part、Windows/Linux Leading 条件间距、命中测试、ImagePreviewer 与全屏宿主。 |
| 标题布局 | 覆盖四种显式对齐、平台 `Auto`、CSD/native inset、Logo/LeftAddOn 动态可见性和窄窗口退化。 |
| Token | 检查 Token 默认值、generated resource key、Logo/LeftAddOn 间距映射、Light/Dark 和 active/inactive 状态。 |
