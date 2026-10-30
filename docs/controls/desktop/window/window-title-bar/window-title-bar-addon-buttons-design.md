# WindowTitleBar AddOn 按钮设计

本文档定义 `WindowTitleBar` 的 AddOn 按钮控件族、宿主状态同步、视觉主题和平台边界。标题栏总体公共契约见
[WindowTitleBar 桌面版架构设计](overview.md)，源码职责与生命周期见 [WindowTitleBar 实现原理](implementation.md)，系统窗口按钮的能力与呈现模型见
[WindowTitleBar Caption Button 配置设计](caption-button-configuration-design.md)，标题栏视觉变量见
[WindowTitleBar Token 设计](token.md)。

## 1. 设计定位

`WindowTitleBarButton` 和 `WindowTitleBarToggleButton` 是放入 `LeftAddOn` 或 `RightAddOn` 的公共语义控件。它们为应用自定义操作提供与 AtomUI managed title bar 一致的图标尺寸、背景、active/inactive、hover、pressed、disabled 和 motion 视觉，同时保留普通应用按钮的命令与状态所有权。

该控件族只定义 **AddOn 视觉语义**，不定义系统窗口操作。系统最小化、最大化、全屏、置顶和关闭入口仍由 internal `CaptionButtonGroup`、`CaptionButton` 和 `WindowsCaptionButton` 维护；AddOn 按钮不进入系统按钮序列，也不参与 native chrome 安全区或 Windows snap 语义。

## 2. 设计原则

- **视觉复用、职责分离**：AddOn 控件复用 WindowTitleBar 的 managed caption 视觉变量，但不复用系统窗口操作 owner。
- **复用公共按钮基类**：普通 AddOn 按钮基于 `IconButton`，可切换 AddOn 按钮基于 `ToggleIconButton`；不复制命令、指针、禁用和 ToggleButton 基础行为。
- **宿主状态单向投影**：窗口激活状态从 `Window` 经 `WindowTitleBar` 单向流向 AddOn 控件；标题栏 motion 状态沿 `WindowTitleBar.IsMotionEnabled` 及其主题默认值投影。控件不反向修改 Window 或标题栏状态。
- **应用状态归应用所有**：`Command`、`CommandParameter`、`IsEnabled`、`IsChecked` 和 checked/unchecked 图标均由应用或 ViewModel 管理。
- **平台视觉适配、native 语义隔离**：Windows AddOn 与 managed caption button 共享方形交互几何和状态反馈；Linux/macOS AddOn 与相邻 managed caption band 共享圆形背景、背景 inset 和语义间距。Windows 原生 caption glyph、element role 和 snap 语义不外溢到 AddOn。
- **输入边界明确**：AddOn 按钮消费自己的 pointer input，不触发标题栏空白区域拖动或主按钮双击最大化。
- **Token 单一来源**：AddOn 按钮使用已有 `WindowTitleBarToken` 语义，不新增与 `CaptionButtonPadding`、`CaptionButtonIconSize`、active/inactive 颜色重复的 Token。
- **模板可替换**：内置主题提供稳定的 AddOn 语义样式；应用可以替换控件主题，但必须保留 host state、命令和可交互区域语义。

## 3. 专项模型与 Public API

### 3.1 控件模型

| 类型 | 基类 | 语义 | 状态 owner |
| --- | --- | --- | --- |
| `WindowTitleBarButton` | `IconButton` | 单图标 AddOn 操作按钮。 | 应用命令和 `IsEnabled` |
| `WindowTitleBarToggleButton` | `ToggleIconButton` | 带 checked/unchecked 图标的 AddOn 切换按钮。 | 应用 `IsChecked`、`CheckedIcon` 和 `UnCheckedIcon` |

两个类型位于 `AtomUI.Desktop.Controls` 命名空间，并使用 `https://atomui.net` AXAML 命名空间。它们不新增普通按钮已有的命令、点击、焦点、指针、图标尺寸或 ToggleButton 状态 API。

### 3.2 继承的公共契约

`WindowTitleBarButton` 继承 `IconButton` 的以下语义：

- `Icon`、`IconWidth`、`IconHeight` 和 `IconBrush`。
- `Command`、`CommandParameter`、`IsEnabled`、AtomUI `ToolTip` service 和 Avalonia `Button` 的输入语义。
- `IsMotionEnabled` 以及既有的初始化、Loaded 和 transition 生命周期。

`WindowTitleBarToggleButton` 继承 `ToggleIconButton` 的以下语义：

- `CheckedIcon`、`UnCheckedIcon`、`IconWidth`、`IconHeight` 和 `IconBrush`。
- Avalonia `ToggleButton.IsChecked` 的 nullable checked 状态、绑定和 changed event。
- `Command`、`CommandParameter`、`IsEnabled`、`IsMotionEnabled` 和标准 ToggleButton 输入语义。

AddOn 控件不引入 `NormalIcon`、`CaptionButtonAction`、`HostWindowState` 或窗口能力属性。普通按钮使用 `Icon`；切换按钮使用 `UnCheckedIcon` 与 `CheckedIcon`，保持与现有 public button family 一致。

### 3.3 标题栏宿主上下文

AddOn 控件接收以下 host context：

| 输入 | 来源 | 语义 | 控件行为 |
| --- | --- | --- | --- |
| `IsWindowActive` | `Window` -> `WindowTitleBar` host projection | 宿主窗口是否处于 active 状态。 | 选择 active/inactive 图标与背景视觉。 |
| `IsMotionEnabled` | `WindowTitleBar` host context，或控件自身主题默认值 | 标题栏是否允许状态过渡。 | 控制背景、图标颜色和 pressed 状态的 transition。 |
| `HostOsType` | `WindowTitleBar.OsType` -> AddOn presenter host context | AddOn 所在标题栏的平台。 | Windows 选择 caption-aligned 几何与视觉；其他值选择通用 managed visual。 |
| `WindowTitleBar` host identity | 最近的 AtomUI `WindowTitleBar` | AddOn 是否处于标题栏宿主上下文。 | 只影响状态投影，不授予 Window 操作能力。 |

`IsWindowActive` 和 `HostOsType` 是 host-owned 的 styled state：控件主题可以读取它们，应用不通过它们改变 Window 激活状态或平台。实现使用标题栏 host projection 的 inheritable styled/attached state，并由 AddOn 控件的 internal owner property 消费；这些 owner properties 不构成应用设置宿主状态的公共 API。控件脱离标题栏宿主后使用 standalone 默认值，并不保留旧宿主引用。`IsMotionEnabled` 仍遵循 `IMotionAwareControl` 的公共契约；显式的控件级样式或绑定可以覆盖主题默认值，但不会创建额外的 Window 订阅。

### 3.4 AddOn 组合模型

`LeftAddOn` 和 `RightAddOn` 仍接受任意对象、容器或模板。多个 AddOn 按钮的分组和顺序由应用内容容器所有，例如 `StackPanel` 或自定义布局；标题栏不会把 AddOn 内容转换为系统 caption item collection。

按钮容器的 spacing 由应用内容所有，标题栏不重写显式容器值。Windows 中需要与相邻 managed caption buttons 构成连续按钮带时使用零 spacing；Linux/macOS 中标题栏在 AddOn 操作区与 `PART_CaptionButtonGroup` 之间应用 `WindowTitleBarTokenResource CaptionGroupSpacing`，让圆形 AddOn 按钮与相邻 caption button 保持同一可见间距，AddOn 容器内部间距仍由应用所有并可选同一 Token。按钮自身的尺寸、padding 和背景不通过应用手写 margin 复制。

## 4. 变体、平台与状态策略

### 4.1 平台策略矩阵

| 平台或模式 | AddOn 控件主题 | 系统 caption buttons | AddOn 约束 |
| --- | --- | --- | --- |
| Windows managed title bar | 使用 `CaptionButtonIconSize` 保留业务图标尺寸；默认背景透明，占满标题栏可用高度形成方形交互面，使用直角背景和 Arrow 光标，并复用 active/inactive hover 与 pressed Token。 | `WindowsCaptionButton` 保留 Windows glyph、element role、方形尺寸和 snap hover。 | AddOn 不设置 `WindowDecorationProperties.ElementRole`，不参与 native caption button 顺序；容器 spacing 仍由应用负责。 |
| Linux managed title bar | 使用圆形背景、内容驱动方形尺寸、Hand 光标及 active/inactive、hover、pressed 和 motion；圆角使用 `CaptionButtonCornerRadius`，背景 inset 使用 `CaptionButtonBackgroundInset`，并以 `CaptionGroupSpacing` 与 caption group 分隔。 | `CaptionButton` 使用同一 managed visual，但由 `CaptionButtonGroup` 管理固定系统操作。 | AddOn 命令完全由应用负责；不根据 Linux backend capability 隐藏。 |
| macOS managed AddOn | 使用 managed visual、内容驱动方形尺寸和 Hand 光标；圆角使用 `CaptionButtonCornerRadius`，与相邻 managed caption button 的正圆一致，并以 `CaptionGroupSpacing` 与 caption group 分隔；不模拟 AppKit 红黄绿按钮。 | native standard buttons 或 AtomUI 提供的辅助 managed buttons。 | AddOn 不承诺独立控制 native standard buttons 的显示和能力。 |
| Native decorations | AddOn 仍是标题栏内容树中的 managed content。 | 操作系统拥有 native window chrome。 | AddOn 视觉不改变 native chrome inset、CSD 或窗口状态 owner。 |

Windows AddOn 只对齐 AtomUI `WindowsCaptionButton` 的 managed 几何与交互视觉，不模拟 native window operation。系统 glyph、native role、snap hover 和窗口命令仍只属于系统 caption contract。

Linux/macOS 的几何一致性由 `CaptionButtonCornerRadius`、`CaptionButtonBackgroundInset` 和 `CaptionGroupSpacing` 提供：圆角 Token 与 `CaptionButton` 的实测正圆同源，背景 inset Token 与 Linux `CaptionButton` 的背景内缩同源。Windows 维持直角铺满标题栏的方形交互面，不消费这些 Token。

### 4.2 状态矩阵

| 状态 | `WindowTitleBarButton` | `WindowTitleBarToggleButton` |
| --- | --- | --- |
| Active window | Windows 默认背景透明，其他平台使用 `ActiveBgColor`；图标使用 `ActiveColor`，hover/pressed 使用 active 状态背景。 | 同上；checked icon 由 `IsChecked` 决定。 |
| Inactive window | Windows 默认背景透明，其他平台使用 `InactiveBgColor`；图标使用 `InactiveColor`，hover 使用 inactive 状态背景。 | 同上；不清除应用自己的 checked 状态。 |
| Pointer over | 使用标题栏 hover 背景和 icon 颜色。 | 使用同一 hover 规则，checked icon 不改变 hover 颜色策略。 |
| Pressed | 使用标题栏 pressed 背景和 icon 颜色。 | 使用同一 pressed 规则。 |
| Disabled | 遵循公共 `IconButton` / `ToggleIconButton` disabled 语义，不执行命令。 | 保留 `IsChecked` 值，禁用时不触发切换。 |
| Motion disabled | 状态立即切换，不创建 transition。 | 状态立即切换，不创建 transition。 |
| Standalone | 使用 active 默认视觉，不读取旧标题栏状态。 | 使用 active 默认视觉，不读取旧标题栏状态。 |

## 5. 架构、文件结构与职责

```text
src/AtomUI.Desktop.Controls/WindowTitleBar/
├── WindowTitleBar.cs                         # 标题栏宿主上下文和 AddOn 投影边界
├── WindowTitleBarButton.cs                   # public IconButton 语义类型
├── WindowTitleBarToggleButton.cs             # public ToggleIconButton 语义类型
├── CaptionButton.cs                          # internal 系统 managed caption button
├── WindowsCaptionButton.cs                   # internal Windows native-aware button
├── WindowsCaptionButtonLayout.cs             # internal Windows 方形测量共享算法
└── Themes/
    ├── WindowTitleBarButtonTheme.axaml       # AddOn IconButton theme
    ├── WindowTitleBarToggleButtonTheme.axaml # AddOn ToggleIconButton theme
    ├── CaptionButtonTheme.axaml              # internal generic system caption theme
    └── WindowsCaptionButtonTheme.axaml       # internal Windows caption theme
```

| 类型或层 | 职责 | 不负责 |
| --- | --- | --- |
| `Window` | 持有窗口 active state、motion state 和宿主 projection 定义。 | 不枚举或操作 AddOn button descendants。 |
| `WindowTitleBar` | 发现最近宿主，建立可释放 host context，并把 active、motion 和 platform 状态投影到标题栏内容树。 | 不成为 AddOn command owner，不解释业务 checked state。 |
| `WindowTitleBarButton` | 基于 `IconButton` 提供标题栏 AddOn 视觉身份和输入隔离；在 Windows host 中使用 caption-aligned 方形测量。 | 不执行系统窗口操作，不写入 WindowState 或 Topmost。 |
| `WindowTitleBarToggleButton` | 基于 `ToggleIconButton` 提供标题栏 AddOn toggle 视觉身份；在 Windows host 中使用同一方形测量。 | 不把 checked state 映射为窗口操作，不读取 `CaptionButtonAction`。 |
| `WindowsCaptionButtonLayout` | 归一 Windows caption 类按钮的测量约束，并计算占满可用高度的方形 `DesiredSize`。 | 不设置样式、不解释按钮命令或 native role。 |
| `CaptionButtonGroup` | 维护固定系统 caption slots、effective visibility 和 Window command 参数。 | 不承载业务 AddOn buttons。 |
| AddOn content container | 维护业务按钮顺序、分组和容器间距。 | 不复制 Window capability 或 native chrome metrics。 |
| AddOn themes | 基于公共 button themes 覆盖标题栏 Token 和 active/inactive selectors。 | 不创建新的 Window host、timer 或 platform API 调用。 |

AddOn 控件不持有 `Window` 引用。host context 使用继承式 styled state 或等价的强类型投影传递，状态变化只更新固定属性，不遍历标题栏内容树，不创建每个按钮独立的 Window event relay。

## 6. Template、组合与集成契约

### 6.1 WindowTitleBar 模板

现有模板结构保持不变：

```text
WindowTitleBar
└── WindowTitleBarLayoutPanel
    ├── Leading
    │   └── PART_LeftAddOn -> application content
    ├── Title
    └── Trailing
        ├── PART_RightAddOn -> application content
        └── PART_CaptionButtonGroup -> internal system caption slots
```

`PART_LeftAddOn` 和 `PART_RightAddOn` 继续是 `ContentPresenter`，不新增 `PART_AddOnButtonGroup`，也不把业务按钮插入 `PART_CaptionButtonGroup`。新控件作为普通 content descendant 获取标题栏 host context。

### 6.2 AddOn ControlTheme

`WindowTitleBarButtonTheme` 基于 public `IconButtonTheme`，只覆盖以下标题栏语义：

- `IconWidth`、`IconHeight` 使用 `CaptionButtonIconSize`。
- `Padding` 使用 `CaptionButtonPadding`。
- `CornerRadius` 使用标题栏 managed button 的圆角策略；Linux/macOS host 下取 `CaptionButtonCornerRadius`。
- `Background`、`IconBrush` 和 active/inactive、hover、pressed、disabled selectors 使用 `WindowTitleBarTokenResource`。
- `IsMotionEnabled` 使用标题栏 motion context 或主题默认值。

`WindowTitleBarToggleButtonTheme` 基于 public `ToggleIconButtonTheme`，复用相同尺寸、背景、颜色和 motion 语义；checked/unchecked presenter 仍由 `ToggleIconButton` 的标准模板负责。

managed caption 的几何由 internal `CaptionButtonFrame` 提供。它自带 ControlTheme，模板内固定两层：内缩的圆角背景层（`Margin={TemplateBinding BackgroundInset}`、`CornerRadius`）与决定布局盒的内容层（`Padding`）。`CaptionButtonTheme` 与两个 AddOn theme 都只组合该外壳并各自提供 presenter，所以背景 inset 只收视觉、不改变 30 逻辑像素命中面，两层结构也只在 `CaptionButtonFrameTheme.axaml` 里定义一次。改动 AddOn 模板时只需保持 presenter 契约与 `IconButtonTheme` / `ToggleIconButtonTheme` 一致。

Windows host 下两个 AddOn theme 进一步设置 `VerticalAlignment=Stretch`、`CornerRadius=0`、透明常态背景和 Arrow 光标；`WindowsCaptionButtonLayout` 同时被系统 `WindowsCaptionButton` 与两个 AddOn 控件复用。默认 40 逻辑像素标题栏下的交互面为 40×40；标题栏高度定制后随实际可用高度调整。业务图标仍由 `CaptionButtonIconSize` 控制，不消费系统 glyph 专用的 `WindowsCaptionIconSize`。Linux/macOS host 保持内容驱动的 30 逻辑像素方形命中面，把圆角换成 `CaptionButtonCornerRadius`，并由标题栏模板在 Trailing 容器上以 `CaptionGroupSpacing` 分隔 AddOn 操作区和 caption group；Linux 额外把 `CaptionButtonBackgroundInset` 传给共享外壳，把背景收成与相邻 caption button 相同的 26 逻辑像素圆形，因此圆角、可见尺寸和间距都来自与相邻 managed caption button 相同的语义。

两个 AddOn theme 不定义系统 `ElementRole`、native glyph、窗口状态伪类或 `CaptionButtonAction` command parameter。应用替换 AddOn theme 时必须保留可交互的 root、图标 presenter、禁用状态和 host active state 选择能力，并继续复用 `CaptionButtonFrame`，否则会失去与 managed caption band 一致的几何。

### 6.3 与窗口交互集成

标题栏 host projection 只向 AddOn 控件提供状态上下文，不向它们注入系统 caption command。AddOn 按钮的 `Command`、`Click` 和 `IsChecked` 完全由应用所有。

AddOn 按钮沿用 `IconButton`、`ToggleIconButton` 及其 Avalonia 基类的 pointer handling；标题栏拖动路径必须尊重这些控件已处理的指针输入，不把 AddOn 按钮误判为空白标题栏输入。AddOn 控件的 `Focusable`、键盘和无障碍语义仍遵循其公共基类，不因位于标题栏而变成 system caption element。

### 6.4 Public AXAML 用法

应用通过普通容器控制按钮顺序和分组；按钮不需要额外的 Window 引用或 caption action 参数：

```xml
<atom:WindowTitleBar.RightAddOn>
    <StackPanel Orientation="Horizontal"
                Spacing="0">
        <atom:WindowTitleBarButton Icon="{Binding OpenSearchIcon}"
                                   Command="{Binding OpenSearchCommand}"
                                   ToolTip.Tip="Search" />
        <atom:WindowTitleBarToggleButton
            CheckedIcon="{Binding PinnedIcon}"
            UnCheckedIcon="{Binding UnpinnedIcon}"
            IsChecked="{Binding IsPinned, Mode=TwoWay}"
            Command="{Binding TogglePinnedCommand}"
            ToolTip.Tip="Pin panel" />
    </StackPanel>
</atom:WindowTitleBar.RightAddOn>
```

`WindowTitleBarButton` 使用 `Icon`；`WindowTitleBarToggleButton` 使用 `CheckedIcon`、`UnCheckedIcon` 和业务
`IsChecked`。示例使用零 spacing，使 Windows 中的方形 hover 背景与相邻 caption buttons 形成连续按钮带；容器间距始终由应用所有，Linux/macOS 可根据产品需要改用 `CaptionGroupSpacing`。Linux/macOS 中操作区与 caption group 之间的 `CaptionGroupSpacing` 由标题栏提供，示例无需手写 margin。示例中的命令和 checked state 不会自动映射到窗口最小化、最大化、全屏、置顶或关闭操作。

## 7. 核心数据流与生命周期

### 7.1 状态数据流

```text
Window.IsActive
  -> Window title-bar host projection
  -> WindowTitleBar host context
  -> inherited AddOn state / WindowTitleBarButton owner properties
  -> AddOn ControlTheme selectors
  -> icon brush, background, transition and visual state

WindowTitleBar.IsMotionEnabled / theme default
  -> WindowTitleBar host context
  -> inherited AddOn state / WindowTitleBarButton owner properties
  -> AddOn ControlTheme selectors

WindowTitleBar.OsType
  -> PART_LeftAddOn / PART_RightAddOn HostOsType
  -> inherited AddOn state / WindowTitleBarButton owner properties
  -> Windows caption-aligned theme and measure branch
```

窗口 active state 是唯一来源。AddOn 控件不通过逻辑祖先重新发现 `Window`，不监听平台窗口对象，也不建立第二套 active state。应用改变 `IsChecked`、`Icon` 或 `IsEnabled` 时，变化只沿控件自己的 public property 和标准 ToggleButton 数据流传播。

### 7.2 生命周期

1. `WindowTitleBar` 连接最近的 AtomUI `Window`，建立自己的 host projection lease。
2. host context 在标题栏内容树中继承到 `WindowTitleBarButton` 和 `WindowTitleBarToggleButton`。
3. active、motion、platform、主题变体或 AddOn 内容变化只触发属性、样式和正常 measure/render invalidation。
4. AddOn 内容被替换、控件移出标题栏或标题栏 detach 时，旧 host context 自动失效；控件不保留旧 `Window` 引用。
5. `WindowTitleBar` template reapply 不注册逐按钮 Click handler，也不增加每个 AddOn 按钮的 Window subscription。

空 AddOn、隐藏 AddOn 或零尺寸 AddOn 不产生额外标题安全空间；容器的 `DesiredSize` 和既有 Leading/Trailing 布局公式继续决定占位。AddOn button 的内部 padding 已计入自身 DesiredSize，不由 `WindowTitleBarLayoutPanel` 重复累加。

## 8. 资源、性能与 AOT 边界

- 控件使用静态注册的 public `IconButton` / `ToggleIconButton` properties 和静态 `ControlTheme`，不引入反射、程序集扫描或运行时类型发现。
- host context 采用固定数量的继承式 styled state 或强类型绑定，包括 `HostOsType`；不遍历 AddOn descendants，不为每个按钮创建 Window relay binding。
- 状态变化不创建 options 对象、button collection、converter graph、timer 或异步任务。
- AddOn 主题只消费已有 `WindowTitleBarTokenResource` 和公共 button theme，不新增 DynamicResource owner 或非 Visual resource host。
- NativeAOT 通过静态类型引用、固定 ControlTheme key 和已有 icon registration 工作；不使用动态命令参数解析或反射调用平台 API。
- Windows 方形尺寸由无状态纯函数归一约束，不分配临时对象，不保存布局缓存。
- 热路径只涉及有限属性更新、样式重新匹配和按钮自身的 measure/render；窗口拖动仍由 `Window` 的现有 host lease 处理。

## 9. 兼容性与定制边界

- 新增 `WindowTitleBarButton` 和 `WindowTitleBarToggleButton` 是 additive public API，不改变现有 `Window`、`WindowTitleBar`、`CaptionButtonGroup` 或系统 caption button API。
- `PART_CaptionButtonGroup`、系统 caption button 顺序、Windows `ElementRole` 和 native chrome metrics 保持稳定。
- Windows AddOn 的 managed 交互几何与 AtomUI Windows caption buttons 对齐；Linux/macOS AddOn 的圆角、可见背景尺寸和操作区间距与相邻 managed caption button 对齐。该承诺不包含 Windows/macOS native 窗口按钮的系统语义。
- `WindowTitleBarButton` 的命令不会自动绑定 `Window` 操作；应用必须显式提供 `Command` 或 `Click` 处理。
- `WindowTitleBarToggleButton` 的 checked state 不代表 `WindowState`、`Topmost` 或任何系统能力；应用必须显式绑定业务状态。
- 应用替换 `WindowTitleBarTheme` 时，如果移除 `PART_LeftAddOn`、`PART_RightAddOn` 或等价的 host context 传播，AddOn 控件将退化为 standalone 视觉；应用负责提供等价的宿主状态投影。
- 应用可以替换两个 AddOn `ControlTheme`，但必须保留 `Icon`/checked presenter、可交互 root、disabled 反馈、host active state 和 motion 语义。
- 应用通过 AddOn 容器控制按钮顺序和分组，不应依赖 internal `CaptionButtonGroup`、`CaptionButton` 或 `WindowsCaptionButton` 类型。

## 10. 验证要求

### Public API 与语义

- `WindowTitleBarButton` 继承 `IconButton`，可绑定 `Icon`、`Command`、`CommandParameter`、`IsEnabled` 和 `IsMotionEnabled`。
- `WindowTitleBarToggleButton` 继承 `ToggleIconButton`，可绑定 `CheckedIcon`、`UnCheckedIcon`、`IsChecked`、`Command` 和 `IsEnabled`。
- 两个控件均不暴露 `CaptionButtonAction`、`HostWindowState`、`CanMinimize`、`CanMaximize` 或 Window operation API。
- active/inactive host state 的默认值、绑定优先级和 detach 后的 standalone 回退行为稳定。

### Theme 与布局

- Light/Dark 主题覆盖 active、inactive、hover、pressed、disabled 和 motion disabled 状态。
- Windows 中默认 40 高度得到 40×40 AddOn 交互面，自定义标题栏高度后仍为方形；真实 pointer hover/pressed/exit 分别切换完整背景面。
- Linux 与 macOS 都使用内容驱动的 30 逻辑像素方形命中面、与相邻 managed caption button 一致的圆角，以及同一语义的 `CaptionGroupSpacing` 操作区间距；Linux 背景额外消费 `CaptionButtonBackgroundInset`，可见圆形与 caption band 同为 26 逻辑像素，不被 Windows 宿主样式污染。
- `CaptionButtonIconSize`、`CaptionButtonPadding` 和应用所有的容器 spacing 不产生重复 margin 或错误安全空间。
- AddOn 内容动态替换、隐藏、零尺寸和多个按钮分组后，Leading/Trailing 实测宽度和标题对齐保持正确。
- 默认 `WindowTitleBar`、内容区 `WindowTitleBar`、ImagePreviewer 派生标题栏和全屏标题宿主不丢失既有 `PART_CaptionButtonGroup` 契约。

### 交互、生命周期与平台

- AddOn 按钮点击、Toggle 切换、键盘输入和禁用状态不触发标题栏拖动或双击最大化。
- Window active state 切换后 AddOn 按钮同步 active/inactive 视觉；标题栏 detach、宿主切换和 Window close 后不保留旧状态或引用。
- Windows 验证 AddOn 不获得 native `ElementRole`、snap hover 或系统 caption 顺序；Linux/macOS 验证 AddOn 按钮和 toggle 按钮的圆角、可见背景尺寸、背景 inset 与操作区间距等于相邻 managed caption button，AddOn 缺席时不产生幽灵间距，命中面仍为 30 逻辑像素；Linux 额外验证平台 native chrome 边界不受影响。
- 主题重新应用不会重复建立宿主投影、事件处理器或资源订阅。

### AOT 与 Gallery

- Desktop Controls 定向测试和适用的 NativeAOT publish 不产生新的反射、trimming 或动态资源宿主风险。
- Gallery 或稳定示例至少覆盖普通 AddOn 图标按钮和 checked/unchecked AddOn 按钮的公共 AXAML 用法，并与控件 API 文档保持一致。
