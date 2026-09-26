# WindowTitleBar

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## 概述

`WindowTitleBar` 负责以下内容：

- 展示窗口 Logo、标题以及标题模板。
- 承载标题栏左右两侧的应用自定义内容。
- 为系统 caption buttons 提供统一交互表面；连接到 Window 后同时参与窗口拖动和双击最大化。
- 自动发现最近的 AtomUI `Window`，接收平台、激活状态、窗口状态和操作命令，并投影为稳定的模板状态。
- 在不同平台和窗口装饰模式下保持标题、原生按钮、managed buttons 与 add-on 互不覆盖。

`WindowTitleBar` 不是通用工具栏或导航栏。业务操作应放入 `LeftAddOn`、`RightAddOn` 或专用控件，并保留标题栏拖动区域和系统窗口操作的优先级。需要复用 AtomUI managed caption visual 的业务按钮使用 `WindowTitleBarButton` 或 `WindowTitleBarToggleButton`；这两个控件不承载系统窗口操作。

## 包与命名空间

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/General/Window` |
| 状态 | Stable |

## 何时使用

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

## 公共 API

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

## 事件与命令

业务命令、启用状态和 checked 状态仍由应用所有，不映射到 Window 的系统操作。Windows 中两者默认占满标题栏可用高度并形成方形交互面，使用直角、透明常态背景和 Arrow 光标，业务图标继续使用 `CaptionButtonIconSize`；Linux/macOS 保持内容驱动的 30 逻辑像素方形命中面，把圆角换成 `CaptionButtonCornerRadius`，并以 `CaptionGroupSpacing` 与 caption group 分隔，Linux 额外用 `CaptionButtonBackgroundInset` 把背景收成与相邻 caption button 相同的 26 逻辑像素圆形，因此圆形按钮与相邻 managed caption button 的圆角、可见尺寸和间距同源。按钮容器内部间距由应用内容所有，Windows 中需要连续 hover 按钮带时应使用零 spacing。
### 3.4 交互事件
事件在对应的 `PointerReleased` 阶段发出，避免窗口同步 resize 破坏当前 pointer capture。pointer capture 丢失或释放条件不匹配时，请求被取消。

## 使用示例

稳定示例来源于 Gallery ShowCase 和源码查看片段。生成器只输出可从 `ShowCaseItem` 追溯的示例，不维护第二套手写示例。

以下示例来自 Gallery 源码查看使用的 `ShowCaseItem` 片段，并已按中文资源规范化。

### 基础窗口

来源：`controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml:34`

Gallery key：`ExamplesContent` / item `0`

```axaml
<atom:Button ButtonType="Primary"
```

### 标题对齐

来源：`controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml:47`

Gallery key：`ExamplesContent` / item `1`

```axaml
<atom:Button ButtonType="Primary"
```

### 隐藏标题文字

来源：`controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml:61`

SourceKey：`window-title-visibility`

```axaml
<atom:Button ButtonType="Primary"
```

### Logo 显示策略

来源：`controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml:74`

Gallery key：`ExamplesContent` / item `3`

```axaml
<atom:Button ButtonType="Primary"
```

## 状态模型

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

## 主题与 Design Token

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

Token 来源：

`WindowTitleBarToken` 是 scope id 为 `WindowTitleBar` 的 internal control token，源码位于 `src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs`。它从 `SharedToken` 计算标题栏和 caption button 的视觉变量，并通过生成的 `WindowTitleBarTokenResource` key 供 AXAML 使用。

Token 负责尺寸、间距、字体和状态颜色，不负责以下运行时语义：

- `TitleAlignment`、Leading/Title/Trailing 角色和布局公式。
- CSD、native chrome insets、WindowState 和 backend 能力。
- Logo、标题、add-on 或 caption button 的有效可见性。
- pointer capture、拖动、checked state 和窗口操作。

## AOT 与裁剪注意事项

- Window 定义强类型 host projection 字段集合并创建 lease；每个 WindowTitleBar 按 logical attach/detach 生命周期独立持有和释放 lease。effective Logo 通知属于同一 lease；宿主引用只用于解析 Logo 回退，不作为 caption 状态旁路，CaptionButtonGroup 不持有 Window relay binding 或宿主引用。
- 宿主发现只遍历当前逻辑祖先，不使用全局 Window registry、反射、字符串 binding path 或程序集扫描。
- 每个标题栏与每次宿主连接只创建一个固定大小 lease；状态更新复用现有 binding 和交互订阅，template reapply 不重建 lease。
- 标题布局 Strategy 使用静态无状态实例；measure/arrange 不创建 Context、Plan、binding 或临时 Visual。
- TemplateBinding 和 selector 承担静态视觉投影，不在状态变化时重建模板节点。
- Windows caption 类按钮共享无状态测量函数，不保存 Visual、不创建 binding，不引入反射或 AOT 注册。
- Logo 计算只在显式内容、Icon、宿主 effective 内容或显示策略相关状态变化时执行；子 Window 的主窗口回退订阅只在窗口已打开且依赖回退时存在，并在本地接管或 close 时释放。
- native chrome metrics 缓存属于 Window/platform manager，不能复制到 Panel 或 Strategy。
- 平台 Strategy 使用封闭 `OsType` switch，不使用反射、程序集扫描、字符串类型发现或运行时 DI。
- Token 通过生成的静态资源入口消费；不反射枚举 public API 或 Token 属性。

## 源码索引

```text
src/AtomUI.Desktop.Controls/
├── Window/
│   ├── Window.cs
│   ├── MacStandardWindowButtons.cs
│   ├── Chrome/
│   │   ├── WindowChromeManager.cs
│   │   ├── AbstractLinuxWindowChromeManager.cs
│   │   ├── GenericLinuxWindowChromeManager.cs
│   │   ├── X11WindowChromeManager.cs
│   │   ├── WaylandWindowChromeManager.cs
│   │   └── WindowsWindowChromeManager.cs
│   ├── Utils/
│   │   ├── FullscreenPopoverLayer.cs
│   │   └── WindowVisualLayerClip.cs
│   └── Themes/
│       ├── WindowTheme.axaml
│       ├── WindowDrawnDecorationsTheme.axaml
│       └── FullscreenPopoverLayerTheme.axaml
├── WindowTitleBar/
│   ├── WindowTitleBar.cs
│   ├── WindowTitleBarButton.cs
│   ├── WindowTitleBarToggleButton.cs
│   ├── WindowTitleBarHostContext.cs
│   ├── WindowTitleBarLogoVisibility.cs
│   ├── WindowTitleBarTitleAlignment.cs
│   ├── WindowTitleBarLayoutPanel.cs
│   ├── CaptionButtonGroup.cs
│   ├── CaptionButton.cs
│   ├── CaptionButtonFrame.cs
│   ├── WindowsCaptionButton.cs
│   ├── WindowsCaptionButtonLayout.cs
│   ├── WindowTitleBarToken.cs
│   ├── Strategies/
│   │   ├── IWindowTitleBarLayoutStrategy.cs
│   │   ├── MacOSWindowTitleBarLayoutStrategy.cs
│   │   ├── WindowsWindowTitleBarLayoutStrategy.cs
│   │   └── LinuxWindowTitleBarLayoutStrategy.cs
│   └── Themes/
│       ├── WindowTitleBarTheme.axaml
│       ├── WindowTitleBarButtonTheme.axaml
│       ├── WindowTitleBarToggleButtonTheme.axaml
│       ├── CaptionButtonGroupTheme.axaml
│       ├── CaptionButtonFrameTheme.axaml
│       ├── CaptionButtonTheme.axaml
│       └── WindowsCaptionButtonTheme.axaml
└── ImagePreviewer/
    ├── ImagePreviewerTitleBar.cs
    └── Themes/ImagePreviewerTitleBarTheme.axaml
```

标题布局输入和中间结果使用标量、`Thickness`、`Rect` 与命名 tuple 传递，不建立独立 Context、Plan 或布局结果类型。

## 相关文档

- 源设计文档：`docs/controls/desktop/window/window-title-bar/overview.md`
- 实现文档：`docs/controls/desktop/window/window-title-bar/implementation.md`
- Token 文档：`docs/controls/desktop/window/window-title-bar/token.md`
- 变更记录：`docs/controls/desktop/window/window-title-bar/changelog.md`
- 语义结构：`./semantic-cn.md`
