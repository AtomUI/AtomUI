# WindowTitleBar 桌面版实现原理

本文档说明 `WindowTitleBar` 控件家族的源码职责、运行时 composition、状态流、模板生命周期和跨平台标题布局。公共契约见 [WindowTitleBar 桌面版架构设计](overview.md)，AddOn 按钮控件族的模型与宿主状态边界见 [WindowTitleBar AddOn 按钮设计](window-title-bar-addon-buttons-design.md)，caption button 的能力与呈现模型见 [WindowTitleBar Caption Button 配置设计](caption-button-configuration-design.md)，视觉变量见 [WindowTitleBar Token 设计](token.md)，契约变化见 [WindowTitleBar Changelog](changelog.md)。

## 1. 实现定位

`WindowTitleBar` 的实现分为四层：

1. `Window` 持有真实窗口状态、平台 chrome 状态和原生窗口能力，并定义可释放的标题栏宿主投影。
2. `WindowTitleBar` 发现最近宿主，持有自己的投影 lease，并投影公共内容、窗口状态和标题栏交互。
3. `CaptionButtonGroup` 根据投影输入推导 effective visibility，并通过声明式命令转发固定窗口操作。
4. ControlTheme 声明平台视觉结构、语义 part 和 Token 绑定。

状态从 `Window` 单向流向标题栏及其模板 part。只有 caption button 命令、拖动和双击请求返回宿主窗口；模板视觉不成为窗口状态 owner。

## 2. 源码文件结构

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

## 3. 核心类职责

| 文件或类型 | Owner 职责 |
| --- | --- |
| `Window.cs` | 创建和配置默认标题栏，持有窗口状态与窗口操作，处理所有已连接标题栏的拖动与最大化请求，发布 caption capability、platform、CSD 和 native chrome metrics，并创建完整的 title-bar host projection lease。 |
| `WindowTitleBar.cs` | 注册公共契约，在逻辑树接入时发现最近的 AtomUI Window，独立持有宿主投影 lease，维护有效 Logo、窗口伪类和标题栏输入事件。 |
| `WindowTitleBarButton.cs` | 基于 `IconButton` 提供应用 AddOn 图标操作，消费继承式 active/motion/platform host state；Windows 使用 caption-aligned 方形测量，不持有 `Window` 引用。 |
| `WindowTitleBarToggleButton.cs` | 基于 `ToggleIconButton` 提供应用 AddOn checked/unchecked 图标操作，保留业务 `IsChecked` 和图标所有权；Windows 复用同一方形测量。 |
| `WindowTitleBarHostContext.cs` | internal inheritable attached properties；把标题栏 active/motion/platform 状态从模板的 `ContentPresenter` 投影给 AddOn 内容。 |
| `WindowTitleBarLayoutPanel.cs` | 测量并排列 Leading、Title、Trailing，集中执行共享标题对齐公式。 |
| `Strategies/*` | 解释平台 `Auto` 值并归一有效 native chrome insets，不操作 Visual。 |
| `CaptionButtonGroup.cs` | 从标题栏投影输入推导按钮 effective visibility 和 checked state，并向固定按钮提供宿主命令。 |
| `CaptionButton.cs` | 计算 effective icon、圆形背景和 transition 初始化时序。 |
| `CaptionButtonFrame.cs` | managed caption 的共享几何外壳：内缩的圆角背景层 + 决定布局盒的内容层，供 `CaptionButton` 与两个 AddOn 按钮复用同一套几何来源。 |
| `WindowsCaptionButton.cs` | 使用共享 Windows 方形测量，并维护窗口状态切换后的 pointer-over 修正。 |
| `WindowsCaptionButtonLayout.cs` | 以无状态纯函数归一 Windows caption 类按钮的测量约束与方形 `DesiredSize`，供系统按钮和 AddOn 按钮共享。 |
| `WindowTitleBarToken.cs` | 从 SharedToken 计算标题栏视觉变量，不保存实例状态。 |
| `Themes/*.axaml` | 声明静态 composition、平台模板、selector、命中测试角色和 Token 消费；`CaptionButtonFrameTheme` 是两层几何的唯一声明处，caption band 与 AddOn themes 只组合该外壳并各自提供 presenter。 |

## 4. 状态与数据流

### 4.1 Window state 与 active state

```text
Window.WindowState / Window.IsActive
  -> Window title-bar host projection lease
  -> WindowTitleBar projected properties
  -> :normal / :minimized / :maximized / :fullscreen / :active
  -> IsWindowActive
  -> ControlTheme selectors and CaptionButtonGroup

WindowTitleBar template PART_LeftAddOn / PART_RightAddOn
  -> WindowTitleBarHostContext.IsWindowActive / HostMotionEnabled / HostOsType (inheritable attached state)
  -> WindowTitleBarButton / WindowTitleBarToggleButton owner properties
  -> AddOn theme active/inactive, motion and Windows platform selectors
```

窗口状态伪类在每次状态通知中完整设置，不能依赖前一个状态自行清除。`IsWindowActive` 继续传给 caption buttons，使标题文本和按钮图标使用同一 active/inactive 状态。

### 4.2 Caption button configuration

```text
Window caption visibility + capability + WindowState + platform support
  -> Window creates title-bar host projection lease
  -> WindowTitleBar projected inputs
  -> CaptionButtonGroup effective visibility / checked state
  -> CaptionButton TemplateBinding
```

Window 只发布 requested visibility、窗口能力、状态和操作命令。`CaptionButtonGroup` 只计算派生视觉，不通过逻辑祖先重新发现 Window，也不反向写入 `CanMinimize`、`CanMaximize`、`WindowState` 或 `Topmost`。完整公式和平台矩阵见 [WindowTitleBar Caption Button 配置设计](caption-button-configuration-design.md)。

### 4.3 Effective Logo

Logo 的内容解析与可见性解析分为两层：

**Window 内容层（`Window.EffectiveLogo` / `Window.EffectiveLogoTemplate`）**。公开的 `Logo` / `LogoTemplate` 只承载开发者显式值，框架从不写入。Window 在解析点按显式 `Logo`/`LogoTemplate` → 本窗口 `Icon` → 主窗口显式 `Logo`/`LogoTemplate` → 主窗口 `Icon` 的顺序求值；`WindowIcon` 内容配套内置模板。窗口打开前可以解析主窗口当前值，但不建立长期订阅；打开后且确实依赖主窗口回退时，子 Window 才持有对主窗口 `Logo`、`LogoTemplate` 和 `Icon` 的固定订阅 lease。`OnOpened` 重新解析以取得最新值，本地显式值或本地 `Icon` 接管时释放该 lease，窗口关闭时同样释放。两个全屏标题宿主直接绑定 Window effective 属性。

**WindowTitleBar 内容层（`WindowTitleBar.EffectiveLogo` / `WindowTitleBar.EffectiveLogoTemplate`）**。标题栏自己的 `Logo` 或 `LogoTemplate` 任一非空时，effective 内容取标题栏显式值；两者都为空时才取 host projection 通知的 Window effective 内容。三平台 `PART_Logo` 通过 `TemplateBinding` 消费标题栏 effective 属性，因此 standalone 用法和 Window 内自定义标题栏的显式值不会被宿主回退覆盖。运行时设置 `Logo = <Control>` 直接渲染且不残留 WindowIcon 模板；`Logo = null` 回到宿主默认图标，彻底隐藏使用 `LogoVisibility = Never`。

**可见性层（`IsEffectiveLogoVisible`）**。标题栏 effective 内容、`LogoVisibility`、`Title`、`IsTitleVisible`、`OsType` 或全屏状态变化时重新计算；`hasLogo` 只读取标题栏 effective 内容。`Auto` 分支要求标题内容有效且标题未被 `IsTitleVisible=False` 隐藏。Theme 只绑定 internal direct property，不在平台模板中复制 Logo 决策。

### 4.4 Effective Title

`Title` 或 `IsTitleVisible` 变化时重新计算 `IsEffectiveTitleVisible = IsTitleVisible && Title is not null`；构造函数先行初始化，属性变更分发中标题重算先于 Logo 重算执行（Logo 的 `Auto` 分支读取标题有效值）。三平台模板的 `PART_ContentPresenter` 与全屏层 `FullscreenTitleText`（绑定 Window 侧 `IsEffectiveFullscreenTitleVisible`）只绑定 internal direct property，不在模板中复制标题判空逻辑。

### 4.5 标题对齐

```text
Window.TitleAlignment + add-on content/templates + platform/native metrics
  -> WindowTitleBar layout inputs
  -> WindowTitleBarLayoutPanel
  -> platform Strategy
  -> shared safe-region and alignment math
  -> Leading / Title / Trailing rectangles
```

平台层只发布逻辑像素 metrics；Panel 不查找 `Window`、不调用 native API。详细输入、CSD 矩阵、公式和失效条件由本文第 8 节集中定义。

### 4.6 Theme 与 Token

```text
SharedToken
  -> WindowTitleBarToken.CalculateTokenValues
  -> generated WindowTitleBarTokenResource keys
  -> WindowTitleBar / CaptionButton ControlTheme
  -> template properties and selectors
```

标题栏背景可以由宿主控件的专属 Token 覆盖，但 caption 尺寸和交互状态仍使用 WindowTitleBar 语义变量。

AddOn 内容不会经过 `CaptionButtonGroup`。Windows、Linux 和 macOS 模板中的 `PART_LeftAddOn`、`PART_RightAddOn`
均是普通 `ContentPresenter`，只在 presenter 根节点设置 `WindowTitleBarHostContext` 的三个继承属性：`IsWindowActive`、`HostMotionEnabled` 和 `HostOsType`；因此容器、
按钮和按钮模板都沿 Avalonia 正常继承路径接收状态。AddOn themes 通过 `WindowTitleBarButton` 或
`WindowTitleBarToggleButton` 的 owner property selector 消费这些状态，模板重新应用时无需建立 C# relay binding。`HostOsType=Windows` 同时启用方形测量和 Windows 视觉 selector；standalone 或脱离 presenter 后回到 `OsType.Unknown` 与通用 managed visual。

## 5. 组合结构模型

### 控件角色图

```text
AtomUI Window logical tree
├── Window template / drawn decorations host
│   └── default WindowTitleBar
├── Window content logical tree
│   └── additional WindowTitleBar instances
│       └── same Window-defined host projection contract
└── WindowTitleBar template
    └── WindowTitleBarLayoutPanel
        ├── Leading (Windows/Linux)
        │   └── DockPanel (HorizontalSpacing=LogoAndLeftAddOnSpacing)
        │       ├── ContentPresenter#PART_Logo
        │       └── ContentPresenter#PART_LeftAddOn
        │           └── application AddOn content / WindowTitleBarButton family
        ├── Leading (macOS)
        │   └── ContentPresenter#PART_LeftAddOn
        ├── Title (Windows/Linux)
        │   └── DockPanel
        │       └── ContentPresenter#PART_ContentPresenter
        ├── Title (macOS)
        │   └── DockPanel
        │       ├── ContentPresenter#PART_Logo
        │       └── ContentPresenter#PART_ContentPresenter
        └── Trailing
            ├── ContentPresenter#PART_RightAddOn
            │   └── application AddOn content / WindowTitleBarButton family
            └── CaptionButtonGroup#PART_CaptionButtonGroup
                └── platform caption button template parts
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `WindowTitleBar` | public control | `WindowTitleBar.cs` | 自身 logical attach/detach 与 host projection lease | 全部标题栏 public surface | public | 可直接使用、派生和替换 ControlTheme；在 AtomUI Window 内容树中自动获得 caption 宿主上下文。 |
| `WindowTitleBarHostContext` on AddOn presenters | internal attached host state | `WindowTitleBarTheme.axaml` | `WindowTitleBar` template | AddOn active/motion/platform visual state | internal-observable | 仅用于宿主状态继承；不授予 Window operation 或 native caption role。 |
| `WindowTitleBarButton` / `WindowTitleBarToggleButton` | public AddOn controls | application content | control instance and normal logical/visual lifecycle | application command, icon and checked state | public | 可直接放入 `LeftAddOn`、`RightAddOn` 或其容器；不属于系统 caption slots。 |
| `WindowTitleBarLayoutPanel` | layout panel | `WindowTitleBarTheme.axaml` | `WindowTitleBar` template | `TitleAlignment`、内容与 add-on | internal-observable | 仅用于理解布局；应用不直接依赖类型或 Role。 |
| `PART_LeftAddOn`、`PART_Logo`、`PART_ContentPresenter`、`PART_RightAddOn` | presenters | `WindowTitleBarTheme.axaml` | `WindowTitleBar` template | 对应内容与模板属性 | template-stable | 可用于主题维护；变更需同步主题、实现和文档。 |
| `PART_CaptionButtonGroup` | internal control part | `WindowTitleBarTheme.axaml` | `WindowTitleBar` | Window caption 配置 | template-stable | 作为稳定协作 part；应用不直接创建 internal 类型。 |
| `CaptionButton` / `WindowsCaptionButton` | internal button | caption themes | `CaptionButtonGroup` | 用户可观察的窗口操作 | internal-observable | 只用于理解平台结构和状态，不作为应用 API。 |
| `ImagePreviewerTitleBar` | internal derived control | ImagePreviewer theme | `ImagePreviewer` | 继承标题栏内容语义 | internal-observable | 只用于维护派生宿主一致性。 |

`ImagePreviewerTitleBar` 将预览 toolbar 放入 Leading，并使用预览图标与标题构成 Title。全屏标题宿主使用同一布局角色和算法，不维护第二套标题居中逻辑。

AddOn 按钮的容器和顺序由应用内容所有。`WindowTitleBar` 只提供 presenter 和 host state projection，不把内容转换为
button collection，也不为每个 AddOn 控件订阅 Window 事件。按钮自己的 `Command`、`IsEnabled`、`IsChecked` 和
checked/unchecked icon 继续沿 `IconButton` / `ToggleIconButton` 公共契约工作。

Windows/Linux 默认模板的 Leading `DockPanel` 使用 `LogoAndLeftAddOnSpacing` 分隔 `PART_Logo` 与 `PART_LeftAddOn`。该间距属于 sibling layout，不属于 add-on 固定 margin；只有两个 presenter 的 `IsVisible` 都为 `true` 时才产生。macOS 的 Leading 不包含 Logo，Title `DockPanel` 继续使用 `LogoAndTitleSpacing` 分隔 Logo 与标题。

## 6. 生命周期与模板接入

### 6.1 创建与配置

`Window.OnApplyTemplate` 的标题栏接入顺序为：

1. 释放旧默认标题栏的内容投影、`SizeChanged` 订阅和 host projection lease。
2. 通过 `NotifyCreateTitleBar(oldTitleBar)` 创建或替换标题栏。
3. 无条件把新标题栏连接到当前 Window 的 host projection；该步骤不能由派生类 override 跳过。
4. 只给默认标题栏连接 `SizeChanged`，用于标题栏高度提示和 CSD 几何。
5. 通过 `NotifyConfigureTitleBar` 投影默认标题栏专属的 Title、标题可见性、Logo、对齐和 add-on 内容。
6. 将结果写入 internal `TitleBar`，交给 Window template 展示。

派生 `Window` 可以覆盖两个 protected 方法，但通用宿主投影不依赖 override 实现。重复 apply template 不能让旧标题栏继续持有 Window 事件或 projection lease。

### 6.2 宿主投影生命周期

宿主投影使用以下 acquire/release 模型：

1. `WindowTitleBar.OnAttachedToLogicalTree` 查找最近的 AtomUI `Window`。
2. `WindowTitleBar.AttachHost(window)` 对相同 Window 幂等；宿主变化时先释放旧 lease。
3. `Window` 创建一个固定内容的 host projection lease：把 caption requested visibility、窗口能力、WindowState、Topmost、active state、平台支持、CSD/native chrome 输入和宿主命令单向绑定给该标题栏，通知 Window effective Logo 变化，并订阅该标题栏的双击请求与拖动 pointer 事件。
4. `WindowTitleBar` 持有返回的 `IDisposable` lease；Window 不集中保存任意内容区标题栏的 binding 或交互订阅生命周期。
5. `OnDetachedFromLogicalTree`、宿主切换、默认标题栏替换或 Window close 释放 lease 和宿主引用。

默认标题栏在 `Window.OnApplyTemplate` 中提前执行同一 `AttachHost`，确保进入 visual/logical tree 前已经具有完整宿主输入；随后 logical attach 只能命中幂等路径。`NotifyConfigureTitleBar` 只保留默认标题栏的内容 facade 投影，不创建 caption host binding。`WindowTitleBar` 保留宿主引用供 lease identity、detached popup、routed event 等标题栏级集成使用，但不通过该引用旁路读取 caption 状态；`CaptionButtonGroup` 不持有宿主引用，也不通过逻辑祖先建立第二套 Window 订阅。

### 6.3 Template reapply

`WindowTitleBar.OnApplyTemplate` 只接入标题栏自身必须持有的 template part。标题、effective Logo、add-on 和 caption 配置通过 `TemplateBinding` 获取；Logo 的宿主回退由标题栏状态层统一解析，不在模板中旁路读取 Window，也不重复建立 host projection。Template reapply 与 logical host lifecycle 正交，不增加 Window binding 数量。

LeftAddOn 或 RightAddOn 的内容、可见性、子节点、模板和 margin 变化沿 Avalonia visual tree 使布局重新测量。Windows/Linux 中 Logo 或 LeftAddOn 的有效可见性变化同时重新计算 Leading `DockPanel` 的条件 sibling spacing。Panel 始终读取当前 `DesiredSize`，不保存 add-on 宽度或内部间距缓存，也不需要由标题栏代码手工调用 `InvalidateMeasure`。

`PART_LeftAddOn` 和 `PART_RightAddOn` 的 host context 绑定属于标题栏模板声明：re-template 时旧 presenter 随模板
释放，新的 presenter 重新接收当前 `IsWindowActive`、`IsMotionEnabled` 和 `OsType`。AddOn 控件本身没有逐窗口事件订阅，
因此内容替换、宿主切换和 Window close 不会留下旧窗口引用；脱离 presenter 后继承属性回到控件的 standalone 默认值。

`CaptionButtonGroupTheme` 为 `PART_CloseButton`、`PART_MinimizeButton`、`PART_MaximizeButton`、`PART_FullScreenButton` 和 `PART_PinButton` 声明固定 command parameter。Template reapply 不注册逐按钮 Click handler；Windows pointer-over 状态由按钮自身消费窗口状态输入并失效。

## 7. 交互与事件处理

`WindowTitleBar` 在主按钮双击的 `PointerPressed` 阶段只记录 pending 状态，在匹配的 `PointerReleased` 阶段发出 `MaximizeWindowRequested`。pointer capture 丢失、释放按钮不匹配或其他结束路径都会清除 pending。

`Window` 通过每个 host projection lease 为已连接标题栏注册拖动和双击最大化。按下时同时记录输入来源标题栏和窗口坐标；只有同一标题栏后续的移动、释放或 capture lost 能推进或结束该次拖动，避免同一 Window 中多个标题栏混用 pointer 状态。移动超过 `Constants.DragThreshold` 后先清理本地状态，再调用 `BeginMoveDrag`。`IsMoveEnabled=False`、FullScreen 或非主按钮输入不进入拖动。add-on 与 caption button 的已处理输入不会进入标题栏拖动或双击最大化路径。

内容区 `WindowTitleBar` 通过同一 host projection 获得 caption command、拖动和双击最大化语义。只有默认标题栏额外注册 `SizeChanged`，所以内容区标题栏不会修改 `ExtendClientAreaTitleBarHeightHint`、Windows CSD 最小高度或唯一 CSD geometry owner。

Caption button 通过宿主命令提交固定窗口操作；`Window` 统一执行：

- FullScreen 在进入时保存原 WindowState，退出时恢复；无可恢复值时回到 Normal。
- Maximize 在 Normal/Maximized 间切换，并尊重 `CanMaximize` 与 FullScreen。
- Minimize 写入 `WindowState.Minimized`。
- Pin 切换 `Window.Topmost` 并同步 checked state。
- Close 标记用户 caption close 请求后调用 `Window.Close()`。

双击与最大化按钮复用同一个 `ToggleMaximize` 执行入口；双击在进入该入口前额外遵守 `CanResize`。最小化、最大化和恢复始终写入 Avalonia `WindowState`，不直接调用 Win32、AppKit、X11 或 Wayland 状态 API。CSD 隐藏默认标题栏时，Window Theme 保持 `WindowDecorations.Full` 并仅隐藏 AtomUI drawn title-bar visual，使平台窗口管理器继续拥有原生状态转换和动画能力；具体动画是否呈现由操作系统、窗口管理器和用户动画设置决定。

窗口状态变化后，Windows caption buttons 抑制旧 pointer-over 视觉，直到新的 pointer enter/move 恢复 hover。标题栏和 caption buttons 在初始化阶段禁用 transition，Loaded 后通过 Dispatcher 恢复。

## 8. 内部算法与关键流程

`CaptionButtonGroup` 的有效可见性集中计算：每个按钮先遵循独立 requested visibility，再与窗口 capability、WindowState 和 backend 支持组合。全屏按钮在最大化时隐藏；最小化和最大化按钮在全屏时隐藏；置顶按钮同时受配置与 backend 能力约束；关闭按钮遵循宿主配置。Wayland 不提供置顶能力，Linux backend 识别由 `AbstractLinuxWindowChromeManager` 统一收敛。Visibility 不写回 capability，具体公式见 [WindowTitleBar Caption Button 配置设计](caption-button-configuration-design.md)。

标题布局只在 `WindowTitleBarLayoutPanel` 中执行：先归一 native chrome inset，再在原生安全边界后应用 managed Padding，最后把 Leading 和 Trailing 转换为标题安全边界并执行 `Left`、`Center`、`WindowCenter` 或 `Right` 的共享公式。基础边界为 `BL = clamp(NL + PL, 0, W)` 与 `BR = clamp(W - NR - PR, 0, W)`；native extent 和 Padding 各计算一次，`WindowCenter` 仍以完整 frame 的 `W / 2` 为轴。平台 Strategy 不测量 Visual，也不复制对齐公式。

操作区占位使用当前实测宽度与条件间距：

```text
Windows/Linux:
LeadingWidth = LogoWidth + LeftAddOnWidth
             + (IsLogoVisible && IsLeftAddOnVisible ? LogoAndLeftAddOnSpacing : 0)

ML = LeadingWidth > 0 ? LeadingWidth + HeaderHorizontalSpacing : 0
MR = TrailingWidth > 0 ? TrailingWidth + HeaderHorizontalSpacing : 0
```

`LeadingWidth` 与 `TrailingWidth` 来自 direct role child 的 `DesiredSize.Width`，已经包含内部容器 spacing 和该 child 自身 margin，因此 Panel 不再额外累加。区域缺失、不可见、内容为空或孩子实测为零时，对应占位和外部间距同时为零。Windows/Linux Leading 内的 `LogoAndLeftAddOnSpacing` 按两个 presenter 的可见性产生，不由 Panel 根据内容宽度二次推导；Title 组内的 `LogoAndTitleSpacing` 遵循同类 sibling layout 语义。

Windows/Linux 默认模板把有效 Logo 放入 Leading direct role child，并排在 `PART_LeftAddOn` 之前；因此 Logo 宽度及其与 LeftAddOn 的条件间距都作为 `LeadingWidth` 的一部分参与安全空间计算。macOS 默认模板、ImagePreviewer 标题宿主和全屏标题宿主仍可把图标放在 Title role 内，并继续由同一标题对齐公式处理。

Windows caption 类按钮的测量由 `WindowsCaptionButtonLayout` 统一：水平约束为无限时以可用高度归一测量约束，最终边长取可用宽高的较小值与内容最小边长的较大值。因此默认 40 逻辑像素标题栏下得到 40×40 交互面，自定义标题栏高度后随实际高度变化；这一算法供 `WindowsCaptionButton`、`WindowTitleBarButton` 和 `WindowTitleBarToggleButton` 共享。Linux/macOS 仍使用控件内容和 padding 驱动的最小方形测量。

## 9. 资源、性能与 AOT 边界

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

## 10. 维护不变量

- Window-defined host projection 与 `Window.NotifyConfigureTitleBar` 的默认内容投影必须分离：前者服务所有逻辑树内标题栏，后者只配置默认标题栏的 Title、Logo、对齐和 add-on。
- 每个 `WindowTitleBar` 的宿主投影保持单向、完整且独立；AttachHost 对相同 Window 幂等，detach 或宿主切换必须释放旧 lease；CaptionButtonGroup 不通过 logical attach/detach 建立 Window 状态副本。
- `WindowTitleBar` 的显式 Logo/Template 优先于宿主 effective Logo；三平台模板只消费标题栏 effective 属性。Window 的两个全屏宿主只消费 Window effective 属性，不能回退到原始 Logo/Title 判空。
- 默认标题栏的 `LeftAddOn`、`LeftAddOnTemplate`、`RightAddOn` 和 `RightAddOnTemplate` 由 `Window` 的同名 public API 以 `Template` 优先级提供，派生标题栏 local add-on 不被覆盖。
- 所有已连接标题栏获得 Window 的拖动、双击最大化和 caption 宿主上下文；只有默认标题栏获得尺寸提示和 CSD 高度协作。
- CSD 下隐藏默认标题栏必须保留 `WindowDecorations.Full`，`WindowDrawnDecorationsTheme` 以 `HasTitleBar && IsTitleBarVisible` 控制 frame、shadow 和 presenter 可见性。
- 三个平台 ControlTemplate 保持相同语义角色、稳定 part 名称和平台 caption button 顺序。
- Windows/Linux 的 Logo 始终位于 Leading 最左侧；有效 Logo 与有效 LeftAddOn 之间只由 Leading `DockPanel.HorizontalSpacing` 消费 `LogoAndLeftAddOnSpacing`。macOS、ImagePreviewer 与全屏标题宿主可将图标与 Title 保持为连续 Title 组。无论图标位于哪个 role，标题对齐公式只读取 Leading、Title、Trailing 三个 direct role child 的实测宽度。
- Leading/Trailing 为零宽时不产生操作区间距；add-on margin 只通过 `DesiredSize` 计入一次。
- ImagePreviewer 与两个全屏标题宿主复用同一标题布局模型。
- Title 不参与命中测试；add-on 与 caption buttons 保持可交互。
- Windows AddOn 仅对齐 managed caption 几何、背景反馈和光标；不设置 `WindowDecorationProperties.ElementRole`，不获得 snap hover、native glyph 或窗口操作命令。Linux/macOS 保持原有圆角、Hand 光标与内容驱动尺寸。
- `WindowTitleBarToken`、generated resource key 和 Theme 消费名保持同步。

## 11. 测试与验证

- `WindowTitleBarLogoVisibilityTests` 覆盖 Logo 默认值、平台规则、两个全屏宿主的 effective 绑定和 Window 投影。
- `WindowTitleBarEffectiveLogoTests` 覆盖 Icon 回退、运行时内容替换、自定义标题栏显式值优先级、主窗口动态回退、显示前零订阅及 close 释放。
- `WindowTitleBarAddOnTests` 覆盖 Window add-on API 默认值、模板类型及到默认标题栏的单向实时投影。
- `WindowTitleBarButtonTests` 覆盖 AddOn 普通/Toggle 控件继承关系，checked/unchecked 图标切换，active/motion/platform host projection，detach 后 standalone 回退，Windows 方形几何与真实 pointer hover/pressed/exit，Linux/macOS 尺寸、圆角、光标回归，以及独立主题资产注册。
- `WindowCaptionButtonConfigurationTests` 覆盖五个 visibility 属性默认值、capability 隔离、effective truth table、默认/内容区/多标题栏宿主发现、真实 pointer 双击切换、宿主切换、动态状态投影、lease 释放和 template reapply。
- `WindowTitleBarTokenTests` 覆盖 Token 默认值、三平台 caption 视觉和 Windows edge layout。
- `ImagePreviewerTitleBarThemeTests` 覆盖派生标题栏的标题组、操作区和平台模板契约。
- 标题几何测试覆盖所有 alignment、对称与非对称操作区、Windows/Linux Logo/LeftAddOn 同时可见及任一 presenter 隐藏时的条件间距、Padding/native inset、窄窗口和非法 metrics。
- Windows、macOS、Linux 实机验证覆盖 CSD/非 CSD、缩放、最小化/任务栏恢复、最大化/还原和全屏状态；平台支持且系统动画开启时应保留原生窗口状态动画。
- 文档改动运行 LLMS `verify`、相对链接检查和 `git diff --check`；行为、Theme 或 Public API 变更运行对应 Desktop Controls 测试。
