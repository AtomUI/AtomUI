# WindowTitleBar 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

| Part | AtomUI 节点 | 职责 | 相关 API | 相关 Token | 稳定性 |
| --- | --- | --- | --- | --- | --- |
| `root` | `WindowTitleBar` | 承载公共内容契约、平台状态和标题栏主题入口。 | `Logo`、`Title`、`TitleAlignment` | `Height`、`TitleBarPadding`、标题字体与颜色 | public |
| `frame` | `Border#Frame` | 绘制标题栏背景并定义完整可见 frame。 | `Background`、`Padding` | `Height`、`TitleBarPadding` | template-stable |
| `leading` | Windows/Linux: `PART_Logo` + `PART_LeftAddOn`；macOS: `PART_LeftAddOn` | 承载起始侧应用操作并占用标题安全空间。Windows/Linux 中 Logo 是物理最左内容，且仅在 Logo 与 LeftAddOn 同时有效时产生内部间距。 | `Logo`、`LogoTemplate`、`LogoVisibility`、`LeftAddOn`、`LeftAddOnTemplate` | `LogoSize`、`LogoAndLeftAddOnSpacing`、`HeaderHorizontalSpacing` | template-stable |
| `title` | Windows/Linux: `PART_ContentPresenter`；macOS: `PART_Logo` + `PART_ContentPresenter` | 展示、测量、对齐和裁剪标题内容；macOS 同时保留 Logo/Title 连续标题组。 | `Logo`、`LogoTemplate`、`LogoVisibility`、`Title`、`TitleTemplate`、`IsTitleVisible` | `LogoAndTitleSpacing`、标题字体与颜色 | template-stable |
| `trailing` | `PART_RightAddOn` + `PART_CaptionButtonGroup` | 承载结束侧应用操作和 managed window operations。 | `RightAddOn`、`RightAddOnTemplate`；五个 Window caption visibility 属性 | `HeaderHorizontalSpacing`、caption button 尺寸、间距与状态颜色 | template-stable |
| `native-chrome` | 平台原生窗口按钮安全区 | 以逻辑像素 inset 约束标题安全空间，不进入 visual tree。 | 平台、CSD、WindowState | 不适用 | internal-observable |

## Abstract AXAML Structure

来源：`src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarTheme.axaml`

```xml
<Border Name="Frame">
    <WindowTitleBarLayoutPanel>
        <DockPanel>
            <ContentPresenter Name="PART_Logo" />
            <ContentPresenter Name="PART_LeftAddOn" />
        </DockPanel>
        <DockPanel>
            <ContentPresenter Name="PART_ContentPresenter" />
        </DockPanel>
        <StackPanel>
            <ContentPresenter Name="PART_RightAddOn" />
            <CaptionButtonGroup Name="PART_CaptionButtonGroup" />
        </StackPanel>
    </WindowTitleBarLayoutPanel>
</Border>
```

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
WindowTitleBar
  -> CaptionButtonFrame (control theme, CaptionButtonFrameTheme.axaml)
     -> Panel (template-stable)
        -> PixelAlignedBorder#PART_Frame (template-stable)
        -> Border (template-stable)
           -> ContentPresenter#PART_ContentPresenter (template-stable)
  -> CaptionButtonGroup (control theme, CaptionButtonGroupTheme.axaml)
     -> StackPanel#RootLayout (template-stable)
        -> CaptionButton#PART_FullScreenButton (template-stable)
        -> CaptionButton#PART_PinButton (template-stable)
        -> CaptionButton#PART_MinimizeButton (template-stable)
        -> CaptionButton#PART_MaximizeButton (template-stable)
        -> CaptionButton#PART_CloseButton (template-stable)
     -> StackPanel#RootLayout (template-stable)
        -> CaptionButton#PART_PinButton (template-stable)
     -> StackPanel#RootLayout (template-stable)
        -> WindowsCaptionButton#PART_FullScreenButton (template-stable)
        -> WindowsCaptionButton#PART_PinButton (template-stable)
        -> WindowsCaptionButton#PART_MinimizeButton (template-stable)
        -> WindowsCaptionButton#PART_MaximizeButton (template-stable)
        -> WindowsCaptionButton#PART_CloseButton (template-stable)
  -> CaptionButton (control theme, CaptionButtonTheme.axaml)
     -> CaptionButtonFrame (internal-observable)
        -> IconPresenter#PART_IconPresenter (template-stable)
  -> WindowTitleBarButton (control theme, WindowTitleBarButtonTheme.axaml)
     -> CaptionButtonFrame (internal-observable)
        -> IconPresenter#IconPresenter (internal-observable)
  -> WindowTitleBar (control theme, WindowTitleBarTheme.axaml)
     -> Border#Frame (template-stable)
        -> WindowTitleBarLayoutPanel (internal-observable)
           -> DockPanel (template-stable)
              -> ContentPresenter#PART_Logo (template-stable)
              -> ContentPresenter#PART_LeftAddOn (template-stable)
           -> DockPanel (template-stable)
              -> ContentPresenter#PART_ContentPresenter (template-stable)
           -> StackPanel (template-stable)
              -> ContentPresenter#PART_RightAddOn (template-stable)
              -> CaptionButtonGroup#PART_CaptionButtonGroup (template-stable)
     -> Border#Frame (template-stable)
        -> WindowTitleBarLayoutPanel (internal-observable)
           -> DockPanel (template-stable)
              -> ContentPresenter#PART_Logo (template-stable)
              -> ContentPresenter#PART_LeftAddOn (template-stable)
           -> DockPanel (template-stable)
              -> ContentPresenter#PART_ContentPresenter (template-stable)
           -> StackPanel (template-stable)
              -> ContentPresenter#PART_RightAddOn (template-stable)
              -> CaptionButtonGroup#PART_CaptionButtonGroup (template-stable)
     -> Border#Frame (template-stable)
        -> WindowTitleBarLayoutPanel (internal-observable)
           -> ContentPresenter#PART_LeftAddOn (template-stable)
           -> DockPanel (template-stable)
              -> ContentPresenter#PART_Logo (template-stable)
              -> ContentPresenter#PART_ContentPresenter (template-stable)
           -> StackPanel (template-stable)
              -> ContentPresenter#PART_RightAddOn (template-stable)
              -> CaptionButtonGroup#PART_CaptionButtonGroup (template-stable)
  -> WindowTitleBarToggleButton (control theme, WindowTitleBarToggleButtonTheme.axaml)
     -> CaptionButtonFrame (internal-observable)
        -> Panel#PART_RootLayout (template-stable)
           -> IconPresenter#PART_CheckedIconPresenter (template-stable)
           -> IconPresenter#PART_UnCheckedIconPresenter (template-stable)
  -> WindowsCaptionButton (control theme, WindowsCaptionButtonTheme.axaml)
     -> Border#PART_Frame (template-stable)
        -> IconPresenter#PART_IconPresenter (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `WindowTitleBar` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `CaptionButtonFrame` | control theme | `CaptionButtonFrameTheme.axaml` | WindowTitleBar | `Background`, `BackgroundInset`, `BorderBrush`, `BorderThickness`, `Content`, `ContentTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `Panel` | template node (Panel) | `CaptionButtonFrameTheme.axaml` | CaptionButtonFrame | `Background`, `BackgroundInset`, `BorderBrush`, `BorderThickness`, `Content`, `ContentTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Frame` | template node (PixelAlignedBorder) | `CaptionButtonFrameTheme.axaml` | CaptionButtonFrame | `Background`, `BackgroundInset`, `BorderBrush`, `BorderThickness`, `CornerRadius` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ContentPresenter` | template node (ContentPresenter) | `CaptionButtonFrameTheme.axaml` | CaptionButtonFrame | `Content`, `ContentTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `CaptionButtonGroup` | control theme | `CaptionButtonGroupTheme.axaml` | WindowTitleBar | `CaptionButtonCommand`, `HostWindowState`, `IsCloseButtonEffectivelyVisible`, `IsFullScreenButtonEffectivelyVisible`, `IsMaximizeButtonEffectivelyVisible`, `IsMinimizeButtonEffectivelyVisible` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `RootLayout` | template node (StackPanel) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `IsCloseButtonEffectivelyVisible`, `IsFullScreenButtonEffectivelyVisible`, `IsMaximizeButtonEffectivelyVisible`, `IsMinimizeButtonEffectivelyVisible`, `IsMotionEnabled` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_FullScreenButton` | template node (CaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `IsFullScreenButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive`, `IsWindowFullScreen` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_PinButton` | template node (CaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `IsMotionEnabled`, `IsPinButtonEffectivelyVisible`, `IsWindowActive`, `IsWindowPinned` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_MinimizeButton` | template node (CaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `IsMinimizeButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_MaximizeButton` | template node (CaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `IsMaximizeButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive`, `IsWindowMaximized` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_CloseButton` | template node (CaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `IsCloseButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_FullScreenButton` | template node (WindowsCaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `HostWindowState`, `IsFullScreenButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive`, `IsWindowFullScreen` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_PinButton` | template node (WindowsCaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `HostWindowState`, `IsMotionEnabled`, `IsPinButtonEffectivelyVisible`, `IsWindowActive`, `IsWindowPinned` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_MinimizeButton` | template node (WindowsCaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `HostWindowState`, `IsMinimizeButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_MaximizeButton` | template node (WindowsCaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `HostWindowState`, `IsMaximizeButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive`, `IsWindowMaximized` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_CloseButton` | template node (WindowsCaptionButton) | `CaptionButtonGroupTheme.axaml` | CaptionButtonGroup | `CaptionButtonCommand`, `HostWindowState`, `IsCloseButtonEffectivelyVisible`, `IsMotionEnabled`, `IsWindowActive` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `CaptionButton` | control theme | `CaptionButtonTheme.axaml` | WindowTitleBar | `Background`, `BackgroundInset`, `EffectiveCornerRadius`, `EffectiveIcon`, `HorizontalAlignment`, `IconHeight` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_IconPresenter` | template node (IconPresenter) | `CaptionButtonTheme.axaml` | CaptionButton | `EffectiveIcon`, `IconHeight`, `IconWidth` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `WindowTitleBarButton` | control theme | `WindowTitleBarButtonTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `BorderBrush`, `BorderThickness`, `CornerRadius`, `Icon`, `IconBrush` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `IconPresenter` | template node (IconPresenter) | `WindowTitleBarButtonTheme.axaml` | WindowTitleBarButton | `Icon`, `IconBrush`, `IconHeight`, `IconWidth` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `WindowTitleBar` | control theme | `WindowTitleBarTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `CanMaximize`, `CanMinimize`, `CaptionButtonCommand`, `EffectiveLogoPresenterContent`, `EffectiveLogoPresenterContentTemplate` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `Frame` | template node (Border) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `Background`, `CanMaximize`, `CanMinimize`, `CaptionButtonCommand`, `EffectiveLogoPresenterContent`, `EffectiveLogoPresenterContentTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `WindowTitleBarLayoutPanel` | template node (WindowTitleBarLayoutPanel) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `CanMaximize`, `CanMinimize`, `CaptionButtonCommand`, `EffectiveLogoPresenterContent`, `EffectiveLogoPresenterContentTemplate`, `HostWindowState` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `DockPanel` | template node (DockPanel) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `EffectiveLogoPresenterContent`, `EffectiveLogoPresenterContentTemplate`, `IsEffectiveLogoVisible`, `IsMotionEnabled`, `IsWindowActive`, `LeftAddOn` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Logo` | template node (ContentPresenter) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `EffectiveLogoPresenterContent`, `EffectiveLogoPresenterContentTemplate`, `IsEffectiveLogoVisible` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_LeftAddOn` | template node (ContentPresenter) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `IsMotionEnabled`, `IsWindowActive`, `LeftAddOn`, `LeftAddOnTemplate`, `OsType` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ContentPresenter` | template node (ContentPresenter) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `IsEffectiveTitleVisible`, `Title`, `TitleTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `StackPanel` | template node (StackPanel) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `CanMaximize`, `CanMinimize`, `CaptionButtonCommand`, `HostWindowState`, `IsCloseCaptionButtonVisible`, `IsFullScreenCaptionButtonVisible` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_RightAddOn` | template node (ContentPresenter) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `IsMotionEnabled`, `IsWindowActive`, `OsType`, `RightAddOn`, `RightAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_CaptionButtonGroup` | template node (CaptionButtonGroup) | `WindowTitleBarTheme.axaml` | WindowTitleBar | `CanMaximize`, `CanMinimize`, `CaptionButtonCommand`, `HostWindowState`, `IsCloseCaptionButtonVisible`, `IsFullScreenCaptionButtonVisible` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `WindowTitleBarToggleButton` | control theme | `WindowTitleBarToggleButtonTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `BorderBrush`, `BorderThickness`, `CheckedIcon`, `CornerRadius`, `IconBrush` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PART_RootLayout` | template node (Panel) | `WindowTitleBarToggleButtonTheme.axaml` | WindowTitleBarToggleButton | `CheckedIcon`, `IconBrush`, `IconHeight`, `IconWidth`, `IsChecked`, `UnCheckedIcon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_CheckedIconPresenter` | template node (IconPresenter) | `WindowTitleBarToggleButtonTheme.axaml` | WindowTitleBarToggleButton | `CheckedIcon`, `IconBrush`, `IconHeight`, `IconWidth`, `IsChecked` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_UnCheckedIconPresenter` | template node (IconPresenter) | `WindowTitleBarToggleButtonTheme.axaml` | WindowTitleBarToggleButton | `IconBrush`, `IconHeight`, `IconWidth`, `IsChecked`, `UnCheckedIcon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `WindowsCaptionButton` | control theme | `WindowsCaptionButtonTheme.axaml` | WindowTitleBar | `Background`, `EffectiveCornerRadius`, `EffectiveIcon`, `IconHeight`, `IconWidth`, `Padding` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |

## Template Parts

| 节点 | 类型 | 契约 |
| --- | --- | --- |
| `Frame` | `Border` | 绘制标题栏背景并提供完整可见 frame 的布局边界。 |
| `PART_Logo` | `ContentPresenter` | 展示有效 Logo；Windows/Linux 模板中位于 Leading 最左侧，macOS 模板中位于 Title 内容前。 |
| `PART_ContentPresenter` | `ContentPresenter` | 展示标题，可见性绑定 `IsEffectiveTitleVisible`；字符串标题在安全宽度不足时使用字符省略号，且不参与命中测试。 |
| `PART_LeftAddOn` | `ContentPresenter` | 展示 Leading 内容并投影 AddOn active、motion 和 platform 上下文；Windows/Linux 中由 Leading 容器负责它与有效 Logo 之间的条件间距。 |
| `PART_RightAddOn` | `ContentPresenter` | 展示 Trailing add-on 并投影 AddOn active、motion 和 platform 上下文。 |
| `PART_CaptionButtonGroup` | `CaptionButtonGroup` | 消费宿主投影，推导 managed button 状态并转发固定窗口操作。 |

## Pseudo Classes

`WindowTitleBar` 从宿主 `Window` 的单向属性投影接收状态并维护以下伪类：

| 伪类 | 条件 |
| --- | --- |
| `:active` | 宿主窗口处于激活状态。 |
| `:normal` | `WindowState.Normal`。 |
| `:minimized` | `WindowState.Minimized`。 |
| `:maximized` | `WindowState.Maximized`。 |
| `:fullscreen` | `WindowState.FullScreen`。 |

窗口激活状态同时写入 `IsWindowActive`，供模板中的内部协作控件使用。状态 owner 始终是宿主 `Window`；模板节点不反向维护第二份窗口状态。

## State Flow

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

## Theme and Token Boundaries

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

Token 边界：

`WindowTitleBarToken` 是 scope id 为 `WindowTitleBar` 的 internal control token，源码位于 `src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs`。它从 `SharedToken` 计算标题栏和 caption button 的视觉变量，并通过生成的 `WindowTitleBarTokenResource` key 供 AXAML 使用。

Token 负责尺寸、间距、字体和状态颜色，不负责以下运行时语义：

- `TitleAlignment`、Leading/Title/Trailing 角色和布局公式。
- CSD、native chrome insets、WindowState 和 backend 能力。
- Logo、标题、add-on 或 caption button 的有效可见性。
- pointer capture、拖动、checked state 和窗口操作。

## Customization Boundaries

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

维护不变量：

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
