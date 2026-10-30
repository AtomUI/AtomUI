# WindowDrawnDecorations 实战指南

> 面向团队成员的 Avalonia 12 自定义窗体装饰（CSD）完整使用文档。

---

## 目录

1. [背景：为什么是 WindowDrawnDecorations](#1-背景为什么是-windowdrawndecorations)
2. [核心概念与对象模型](#2-核心概念与对象模型)
3. [激活流程（必读）](#3-激活流程必读)
4. [跨平台行为矩阵](#4-跨平台行为矩阵)
5. [自定义 ControlTheme（手把手）](#5-自定义-controltheme手把手)
6. [必须实现的 PART 与可选 PART](#6-必须实现的-part-与可选-part)
7. [伪类与样式选择器](#7-伪类与样式选择器)
8. [关键计算属性：ShadowThickness / FrameThickness / WindowDecorationMargin](#8-关键计算属性shadowthickness--framethickness--windowdecorationmargin)
9. [Window 端正确配置](#9-window-端正确配置)
10. [Resize 抓手机制与"幽灵 resize 区"问题](#10-resize-抓手机制与幽灵-resize-区问题)
11. [X11 输入区裁剪（点击穿透阴影）](#11-x11-输入区裁剪点击穿透阴影)
12. [Wayland 上的对应方案](#12-wayland-上的对应方案)
13. [阴影设计建议](#13-阴影设计建议)
14. [Window ControlTheme 跨平台设计模式](#14-window-controltheme-跨平台设计模式)
15. [常见坑](#15-常见坑)
16. [架构验证清单](#16-架构验证清单)

---

## 1. 背景：为什么是 WindowDrawnDecorations

Avalonia 12 使用统一的 **`WindowDrawnDecorations`** 机制承载自定义窗体装饰，命名空间为
`Avalonia.Controls.Chrome`。

它的本质是：**一个 `StyledElement`**（注意：**不是 `Control`、也不是可视树上的节点**），由框架在窗体激活 CSD 时延迟创建一次、把模板里的 PART 拆成三个 LayerWrapper（Underlay / Overlay / FullscreenPopover），分别插到 `TopLevel.VisualChildren` 的不同层级。

简化结构：

```
Window (TopLevel)
└── VisualChildren
    ├── LayerWrapper(Underlay)         ← Decorations 模板的 Underlay slot
    ├── 用户内容 (Window.Content)
    ├── LayerWrapper(Overlay)          ← Decorations 模板的 Overlay slot
    ├── LayerWrapper(FullscreenPopover)
    └── ResizeGripLayer (内部)
```

Underlay 在用户内容**底下**（画背景圆角/标题栏底色），Overlay 在用户内容**上面**（放 caption 按钮），ResizeGripLayer 在最上层吃 resize 鼠标事件。

---

## 2. 核心概念与对象模型

### 关键类型

| 类型 | 作用 |
|---|---|
| `WindowDrawnDecorations : StyledElement` | 装饰控件本体。**不是 Visual，无法用 GetVisualDescendants 找到它**。 |
| `WindowDrawnDecorationsTemplate : ITemplate` | 模板包装，跟 `ControlTemplate` 类似，只生成 `WindowDrawnDecorationsContent`。 |
| `WindowDrawnDecorationsContent : Control` | 三槽容器，提供 `Underlay / Overlay / FullscreenPopover` 三个 `Control` 属性。 |
| `IWindowDrawnDecorationsTemplate` | 接口（供主题用）。 |
| `DrawnWindowDecorationParts` | `[Flags]` 枚举：`Shadow / Border / TitleBar / ResizeGrips / All`。 |
| `WindowDecorations` | 用户级开关：`None / TitleBar / Full`（设在 `Window` 上）。 |
| `WindowDecorationProperties` | 附加属性提供方，给 PART 标记角色（`ElementRole`）。 |
| `WindowDecorationsElementRole` | 枚举：`DecorationsElement / TitleBar / CloseButton / MinimizeButton / MaximizeButton / FullScreenButton / ResizeN/S/E/W/NW/NE/SW/SE`。 |
| `ResizeGripLayer` | 内部不可见 8 抓手 Control，自动被框架管理。 |

### Decorations vs Decoration**s**（容易搞混）

- `Window.WindowDecorations`：用户开关（`None` 完全无装饰、`TitleBar` 仅标题栏、`Full` 标题栏+边框+阴影）。
- `WindowDrawnDecorations`：装饰控件本体（被框架自动创建，用户不直接 new）。

---

## 3. 激活流程（必读）

不理解这个流程 → 改半天看不到效果。

```
[用户代码] Window.ExtendClientAreaToDecorationsHint = true
          │
          ▼
[Window.cs] PlatformImpl.SetExtendClientAreaToDecorationsHint(true)
          │
          ▼
[平台后端 IWindowImpl] 决定是否需要管理装饰：
          • Win32  : NeedsManagedDecorations = _isClientAreaExtended
                     RequestedDrawnDecorations = TitleBar
          • X11    : NeedsManagedDecorations = UseManagedDecorations
                     ⚠ 取决于 X11PlatformOptions.EnableDrawnDecorations(实验性)
                     RequestedDrawnDecorations = TitleBar | Border | Shadow | ResizeGrips
          • Wayland: NeedsManagedDecorations 由 xdg-decoration 协商结果决定
                     CSD 时请求 TitleBar | Border | Shadow | ResizeGrips
          • macOS  : NeedsManagedDecorations 硬编码 = false  ❌ 永远不激活
          │
          ▼
[平台后端] 触发 ExtendClientAreaToDecorationsChanged 事件
          │
          ▼
[Window.UpdateDrawnDecorations()]
   ├─ 不 needs → 拆掉旧的（如有）
   └─ needs    → ComputeDecorationParts()  → (parts, state, theme)
                 │
                 ▼
            [TopLevelHost.UpdateDrawnDecorations(parts, state, theme)]
              ├─ 首次：new WindowDrawnDecorations(); 应用 ControlTheme
              ├─ 把 Underlay/Overlay/FullscreenPopover 插入 VisualChildren
              ├─ _decorations.Attach(window)：绑定按钮点击/拖动
              └─ UpdateResizeGripThickness()
```

**关键事实**：

1. `ExtendClientAreaToDecorationsHint=true` 是激活开关。
2. **X11 还要额外打开 `X11PlatformOptions.EnableDrawnDecorations=true`**，否则框架不会启用 managed decorations，原生标题栏不消失，自定义装饰也不生成。
3. Wayland 的 CSD/SSD 协商可在运行期变化，必须响应 `DrawnDecorationsRequestChanged`，不能只在 Window 构造时读取一次。
4. macOS 永远走原生 NSWindow titlebar，自定义模板会被忽略。

### 在 `Program.cs` 里选择 Linux 后端

```csharp
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
        .UseAtomUIPlatformDetect()
        .WithAtomUIDefaultOptions()
        .WithInterFont()
        .LogToTrace();
```

`UseAtomUIPlatformDetect()` 在 `WAYLAND_DISPLAY` 可用时优先原生 Wayland，否则使用 X11；
`ATOMUI_WINDOWING_PLATFORM=wayland|x11` 或显式枚举参数可以覆盖自动选择。X11 CSD 开关由
`WithAtomUIDefaultOptions()` 配置。

---

## 4. 跨平台行为矩阵

| 后端 | NeedsManagedDecorations | RequestedDrawnDecorations | `:has-shadow` | `:has-border` | 备注 |
|---|---|---|---|---|---|
| **Win32** | `_isClientAreaExtended` | `TitleBar` | ❌ | ❌ | OS 还在画 frame/shadow，自定义只覆盖标题栏区。 |
| **X11**（`EnableDrawnDecorations=true`） | `true` | `TitleBar \| Border \| Shadow \| ResizeGrips` | ✅ | ✅ | 完全 CSD，框架画一切。 |
| **X11**（默认） | `false` | `default` (None) | — | — | WM 全权负责，自定义 hint 被忽略。 |
| **macOS Native** | 硬编码 `false` | `default` (None) | — | — | 永远走 NSWindow，自定义无效。 |
| **Wayland** | 取决于 xdg-decoration 协商 | CSD 时为 `TitleBar \| Border \| Shadow \| ResizeGrips` | CSD 时 ✅ | CSD 时 ✅ | GNOME 通常 CSD；支持 SSD 的合成器可能动态切换。 |

### 4.1 三条平台契约

1. **macOS：永不启用 WindowDrawnDecorations**
   - 当前平台契约中 `NeedsManagedDecorations=false`，`Window.ComputeDecorationParts()` 不创建绘制装饰。
   - **后果**：在 macOS 上写 `ControlTheme x:Key="{x:Type WindowDrawnDecorations}"` 是**死代码**，不会被实例化。需要自定义 macOS 标题栏请走 `ExtendClientAreaTitleBarHeightHint` + 系统红绿灯（与 CSD 完全是两套机制）。

2. **Linux（X11）：保留 X11 专用几何和输入区处理**
   - `RequestedDrawnDecorations` 在 managed decorations 生效时包含 Border、ResizeGrips、TitleBar 和 Shadow。
   - 激活条件是 `X11PlatformOptions.EnableDrawnDecorations=true` 与 `Window.ExtendClientAreaToDecorationsHint=true` 同时成立。
   - **后果**：四件套（标题栏 / 边框 / 阴影 / resize 抓手）全部由 Avalonia 渲染；`:has-shadow` / `:has-border` / `:has-resize-grips` 伪类全部为 `true`。本文 §11 的 X Shape 输入区裁剪、`_GTK_FRAME_EXTENTS` 和 Xlib 初始几何**仅在此平台成立**。

3. **Windows（Win32）：仅标题栏可定制**
   - 客户区扩展时 `NeedsManagedDecorations=true`，`RequestedDrawnDecorations` 只请求 TitleBar。
   - 左右下边框、阴影、resize 抓手由 **DWM** 提供。
   - **例外：Windows 10 的顶边边框**。Avalonia 12 在扩展客户区时把客户区顶到窗口最上沿，Win10 的 DWM 不会在客户区内补画顶边，表现为顶边没有可见边框；Win11 的 DWM 有可见边框绘制，不受影响。AtomUI 在 `WindowDrawnDecorationsTheme.axaml` 的 Overlay 中按 `IsLegacyWindowsFrameBorderVisible`（Win10 + CSD + Full + Normal）补画 1 个物理像素的顶边，颜色由 `FrameBorderActive/Inactive` Token 复现 DWM 阴影纹理。该补偿不改变 NC 几何、resize hit-test 或窗口动画；详见 [Window 实现原理](../../controls/desktop/window/window/implementation.md#62-windows-10-顶边-frame-补偿)。
   - **后果**：
      - `:has-shadow` / `:has-border` / `:has-resize-grips` 伪类**永远为 false** —— 任何依赖这三个伪类的样式（margin / boxshadow / 圆角）在 Win32 都不会触发，**必须用无伪类选择器写一份兜底**。
      - 在 `WindowDrawnDecorations` 里画 `BoxShadow` 也是浪费 —— DWM 已经画了一层，你画的会被它的客户区裁切。
      - X11 SHAPE 那套点击穿透**完全不需要** —— Win32 根本没有阴影 buffer。
      - `ShadowThickness` 在 Win32 上保持 `0` 即可（也是默认值）。
      - Win10 顶边补偿只存在于 Overlay 的 1px `Border`；不要把它扩展到 Win11，也不要用清除 `WS_CAPTION` 的方式换边框（会关闭窗口动画）。

### 4.2 一图速记

```
                            macOS              Linux/X11           Windows
WindowDrawnDecorations 实例化  ❌ 永远 null      ✅ (双门激活)        ✅
PART_TitleBar 渲染           —                ✅                 ✅
PART_WindowBorder 渲染       —                ✅                 ❌ (DWM 画；Win10 顶边由 AtomUI Overlay 补 1px)
BoxShadow / ShadowThickness  —                ✅                 ❌ (DWM 画)
ResizeGrips                  —                ✅                 ❌ (DWM 提供)
:has-shadow / :has-border    —                ✅                 ❌ 永 false
需要 SHAPE 点击穿透          —                ✅ (§11)            ❌
```

---

## 5. 自定义 ControlTheme（手把手）

> AtomUI 使用公开的 `Window.WindowDecorationsTheme`。同时保留
> `{x:Type WindowDrawnDecorations}` 资源 key，并通过 `DynamicResource` 显式赋给该属性，确保合并字典
> 完成后解析且能跟随主题切换。

### Step 1：创建主题资源字典 `Themes/CustomWindowDrawnDecorations.axaml`

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:chrome="clr-namespace:Avalonia.Controls.Chrome;assembly=Avalonia.Controls">

  <ControlTheme x:Key="{x:Type chrome:WindowDrawnDecorations}"
                TargetType="chrome:WindowDrawnDecorations">

    <Setter Property="DefaultTitleBarHeight" Value="36"/>
    <Setter Property="DefaultFrameThickness" Value="1"/>
    <Setter Property="DefaultShadowThickness" Value="32"/>

    <Setter Property="Template">
      <chrome:WindowDrawnDecorationsTemplate>
        <chrome:WindowDrawnDecorationsContent>

          <!-- ===== UNDERLAY：用户内容下方，画背景/边框/标题栏底色 ===== -->
          <chrome:WindowDrawnDecorationsContent.Underlay>
            <Panel x:Name="PART_UnderlayWrapper">
              <Border x:Name="PART_WindowBorder"
                      Background="White"
                      BorderBrush="#22000000"
                      BorderThickness="{TemplateBinding FrameThickness}"
                      CornerRadius="10"
                      IsHitTestVisible="False"/>
              <Border x:Name="PART_TitleBar"
                      VerticalAlignment="Top"
                      Height="{TemplateBinding TitleBarHeight}"
                      Background="#F5F5F5"
                      CornerRadius="10,10,0,0"
                      IsVisible="{TemplateBinding HasTitleBar}"
                      chrome:WindowDecorationProperties.ElementRole="TitleBar"/>
            </Panel>
          </chrome:WindowDrawnDecorationsContent.Underlay>

          <!-- ===== OVERLAY：用户内容上方，放 caption 按钮+标题文字 ===== -->
          <chrome:WindowDrawnDecorationsContent.Overlay>
            <Panel x:Name="PART_OverlayWrapper">
              <DockPanel VerticalAlignment="Top"
                         Height="{TemplateBinding TitleBarHeight}"
                         IsVisible="{TemplateBinding HasTitleBar}">
                <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
                  <Button x:Name="PART_MinimizeButton" Content="—"
                          chrome:WindowDecorationProperties.ElementRole="MinimizeButton"/>
                  <Button x:Name="PART_MaximizeButton" Content="□"
                          chrome:WindowDecorationProperties.ElementRole="MaximizeButton"/>
                  <Button x:Name="PART_FullScreenButton" Content="⛶"
                          chrome:WindowDecorationProperties.ElementRole="FullScreenButton"/>
                  <Button x:Name="PART_CloseButton" Content="×"
                          chrome:WindowDecorationProperties.ElementRole="CloseButton"/>
                </StackPanel>

                <Panel x:Name="PART_TitleTextPanel">
                  <TextBlock Text="{Binding $parent[Window].Title}"
                             VerticalAlignment="Center"
                             HorizontalAlignment="Center"/>
                </Panel>
              </DockPanel>
            </Panel>
          </chrome:WindowDrawnDecorationsContent.Overlay>

          <!-- ===== FullscreenPopover：全屏模式下从顶部下拉的浮层 ===== -->
          <chrome:WindowDrawnDecorationsContent.FullscreenPopover>
            <Border Background="#80000000" Height="36">
              <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                <Button x:Name="PART_PopoverFullScreenButton" Content="⛶"/>
                <Button x:Name="PART_PopoverCloseButton" Content="×"/>
              </StackPanel>
            </Border>
          </chrome:WindowDrawnDecorationsContent.FullscreenPopover>

        </chrome:WindowDrawnDecorationsContent>
      </chrome:WindowDrawnDecorationsTemplate>
    </Setter>

    <!-- 伪类样式（见 §7） -->
    <Style Selector="^:has-shadow /template/ Panel#PART_UnderlayWrapper">
      <Setter Property="Margin" Value="{TemplateBinding ShadowThickness}"/>
    </Style>
    <Style Selector="^:has-shadow /template/ Panel#PART_OverlayWrapper">
      <Setter Property="Margin" Value="{TemplateBinding ShadowThickness}"/>
    </Style>
    <Style Selector="^ /template/ Border#PART_WindowBorder">
      <Setter Property="BoxShadow"
              Value="0 6 16 0 #14000000,
                     0 3 6 -4 #1F000000,
                     0 9 28 8 #0D000000"/>
    </Style>
    <Style Selector="^:maximized /template/ Border#PART_WindowBorder">
      <Setter Property="CornerRadius" Value="0"/>
    </Style>
    <Style Selector="^:maximized /template/ Border#PART_TitleBar">
      <Setter Property="CornerRadius" Value="0"/>
    </Style>
  </ControlTheme>
</ResourceDictionary>
```

### Step 2：把字典合并进 `App.Resources`

```xml
<!-- App.axaml -->
<Application xmlns="https://github.com/avaloniaui" …>
  <Application.Styles>
    <FluentTheme/>
  </Application.Styles>
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://YourApp/Themes/CustomWindowDrawnDecorations.axaml"/>
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

合并到 **`Application.Resources`**，不是 `Application.Styles`。`ControlTheme` 通过 `x:Key="{x:Type WindowDrawnDecorations}"` 索引、覆盖 Fluent 内置的同 key 主题。

---

## 6. 必须实现的 PART 与可选 PART

`WindowDrawnDecorations.OnApplyTemplate` 通过命名查找绑定按钮事件。**缺一不可**（缺则按钮不工作或抛 NRE）。

### 必填 PART

| 名称 | 类型 | 作用 |
|---|---|---|
| `PART_TitleBar` | `Border` (or any Control) | 拖动区，自动挂 `BeginMoveDrag`。 |
| `PART_CloseButton` | `Button` | 关闭。 |
| `PART_MinimizeButton` | `Button` | 最小化。 |
| `PART_MaximizeButton` | `Button` | 最大化/还原（同一按钮，按 WindowState 切换 glyph）。 |
| `PART_FullScreenButton` | `Button` | 全屏切换。 |
| `PART_PopoverCloseButton` | `Button` | 全屏弹出层的关闭按钮。 |
| `PART_PopoverFullScreenButton` | `Button` | 全屏弹出层的退出全屏按钮。 |

### 可选 PART（命名约定，便于样式定位）

| 名称 | 用途 |
|---|---|
| `PART_WindowBorder` | 主背景/圆角/阴影承载 `Border`。 |
| `PART_UnderlayWrapper` | Underlay 根 Panel，便于设 ShadowThickness Margin。 |
| `PART_OverlayWrapper` | Overlay 根 Panel，同上。 |
| `PART_TitleTextPanel` | 标题文字区，便于 `:has-border` 时整体偏移。 |

### `WindowDecorationProperties.ElementRole` 附加属性

不是 PART 命名，但**框架靠它判断元素角色**（拖动 / 双击最大化 / 系统菜单等）。建议给所有按钮 + TitleBar 都标。

枚举：`DecorationsElement / TitleBar / CloseButton / MinimizeButton / MaximizeButton / FullScreenButton / ResizeN/S/E/W/NW/NE/SW/SE`。

```xml
<Border chrome:WindowDecorationProperties.ElementRole="TitleBar" .../>
<Button chrome:WindowDecorationProperties.ElementRole="CloseButton" .../>
```

---

## 7. 伪类与样式选择器

`WindowDrawnDecorations` 在不同状态下自动加伪类：

| 伪类 | 触发条件 |
|---|---|
| `:normal` | `WindowState == Normal` |
| `:maximized` | `WindowState == Maximized` |
| `:fullscreen` | `WindowState == FullScreen` |
| `:has-titlebar` | RequestedParts 含 `TitleBar` |
| `:has-shadow` | RequestedParts 含 `Shadow`（X11 CSD 和 Wayland CSD 满足） |
| `:has-border` | RequestedParts 含 `Border`（X11 CSD 和 Wayland CSD 满足） |
| `:has-minimize` | `Window.CanMinimize == true` |
| `:has-maximize` | `Window.CanMaximize == true` |
| `:has-fullscreen` | `Window.CanFullScreen == true` |

样式写法（`^` 是当前 ControlTheme 自身）：

```xml
<!-- 仅最大化时圆角归零 -->
<Style Selector="^:maximized /template/ Border#PART_WindowBorder">
  <Setter Property="CornerRadius" Value="0"/>
</Style>

<!-- 没有最小化按钮时隐藏对应 Button -->
<Style Selector="^:not(:has-minimize) /template/ Button#PART_MinimizeButton">
  <Setter Property="IsVisible" Value="False"/>
</Style>
```

⚠ Win32 上 `:has-shadow` / `:has-border` 永远 false（OS 自己画框），任何依赖这俩伪类的样式在 Windows 不会生效。要 Win32 也有阴影/边框 → 写无条件选择器（不带伪类）。

---

## 8. 关键计算属性：ShadowThickness / FrameThickness / WindowDecorationMargin

### 在 `WindowDrawnDecorations` 上

| 属性 | 类型 | 说明 |
|---|---|---|
| `DefaultShadowThickness` | `Thickness` | 主题里设；激活了 Shadow part 才生效。 |
| `DefaultFrameThickness` | `Thickness` | 边框宽度；激活了 Border part 才生效。 |
| `DefaultTitleBarHeight` | `double` | 标题栏高度。 |
| `ShadowThickness` (`DirectProperty`) | `Thickness` | **运行时实际值**（DPI rounded、parts gating 之后）。 |
| `FrameThickness` (`DirectProperty`) | `Thickness` | 同上。 |
| `TitleBarHeight` (`DirectProperty`) | `double` | 同上。 |

### 在 `Window` 上

```csharp
// AtomUI Window theme contract
public static readonly DirectProperty<Window, Thickness> WindowDecorationMarginProperty;

public Thickness WindowDecorationMargin { get; private set; }
// = (frame.Left+shadow.Left, titleBarHeight+frame.Top+shadow.Top, ...)
```

**这是用户内容应该 inset 的 margin**。直接绑给 `Window.Content` 根容器：

```xml
<Window …>
  <Border Margin="{Binding $parent[Window].WindowDecorationMargin}"
          Background="White">
    <!-- 用户实际内容 -->
  </Border>
</Window>
```

不绑 → 用户内容会铺满整个 OS 客户区（包括阴影 buffer + 标题栏区域），跟 underlay 不对齐。

---

## 9. Window 端正确配置

```xml
<Window xmlns="https://github.com/avaloniaui" …
        Title="My Window"
        Width="900" Height="600"
        ExtendClientAreaToDecorationsHint="True"
        Background="Transparent"
        TransparencyLevelHint="Transparent"
        WindowDecorations="Full">
  <Border Margin="{Binding $parent[Window].WindowDecorationMargin}"
          Background="White"
          CornerRadius="0,0,10,10">
    <!-- 用户内容 -->
  </Border>
</Window>
```

| 属性 | 必填 | 说明 |
|---|---|---|
| `ExtendClientAreaToDecorationsHint="True"` | ✅ | 激活 CSD 流程。 |
| `Background="Transparent"` | ✅ | 让 Underlay 的圆角能看见桌面背景。 |
| `TransparencyLevelHint="Transparent"` | ✅ X11/Win32 | 请求 OS 提供透明合成。 |
| `WindowDecorations` | 可选 | `Full`（默认）/ `TitleBar` / `None`。 |

---

## 10. Resize 抓手机制与"幽灵 resize 区"问题

Avalonia 12 按以下规则维护 resize grip：

```csharp
// Grips strictly cover frame + shadow area, never client area
_resizeGrips.GripThickness = new Thickness(
    frame.Left + shadow.Left, frame.Top + shadow.Top,
    frame.Right + shadow.Right, frame.Bottom + shadow.Bottom);
```

`ResizeGripLayer` 占据 **整个 frame + shadow 区**，鼠标进去就：
1. 光标变 ↘ / ↔（resize cursor）
2. 鼠标事件被 OS 窗体吃掉，**无法穿透到背后的应用**（X11 默认 input region = 整个 OS 窗口矩形）

视觉上那块是透明的、看着像"窗外"，但其实功能上是"窗框边缘"。

**取舍**：
- `ShadowThickness` 越大 → 阴影越好看 → resize 抓手区越大 → "幽灵区"越违和
- 不加任何处理时建议 `ShadowThickness ≤ 16`

AtomUI 保留共享 `BoxShadowsSecondary` 的完整 offset、blur、spread 和 surface shadow extents，
不通过缩小或裁剪阴影来改变命中范围。当前 Wayland 适配通过反射把 Avalonia 内部
`ResizeGripLayer` 的命中厚度归零，再由 AtomUI `WindowResizer` 使用
`(FrameThickness + ShadowThickness) / 3` 的逐边命中带。反射成员使用 `DynamicDependency`
保留元数据，这只解决 trimming/NativeAOT 的成员保留问题；它不会把 Avalonia 私有字段变成稳定 API，
也不提供跨框架升级的兼容保证。X11 仍通过 input region 把实际输入带限制在最多 10 DIP。

Avalonia 会按 `RenderScaling` 将绘制装饰的 shadow extents 对齐到物理像素，因此分数缩放下四边可能是
fractional DIP；Wayland 的 `xdg_surface.set_window_geometry` 只能接收整数。当前 Wayland 后端的
`WSurface.MaybeEmitWindowGeometry` 会对这些边使用不同方向的整数取整，而 `WindowImpl.ApplyConfigureBatch`
又加回未取整的 extents，可能让每轮 configure 的 surface size 净增一个逻辑像素。AtomUI 在 Wayland
平台边界将传给 `IWindowImpl.SetShadowExtents` 的四边归一到整数逻辑像素，使 compositor 排除量和
configure 加回量闭合；绘制装饰仍使用 Avalonia 的原始 fractional thickness，不改变阴影视觉。

---

## 11. X11 输入区裁剪（点击穿透阴影）

要兼顾“大阴影 + 不违和”，AtomUI 在 X11 下使用 SHAPE input region 把可输入区域收窄到可见窗体，并在
边缘保留一条窄 resize 抓手带。当前源码不再保留独立的 `ClickThroughShadow` helper；高层策略属于
`src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs`，底层 XCB 调用属于
`src/AtomUI.Native`。

### 当前源码分层

```text
X11WindowChromeManager
  -> WindowExtensions.SetWindowInputRectangle / ResetWindowInputRegion
  -> WindowUtilsLinux.SetInputRectangle
  -> XcbConnectionHolder
  -> xcb_shape_rectangles_checked(XCB_SHAPE_SK_INPUT)
```

`X11WindowChromeManager` 负责所有和 Window 状态有关的策略：

- 只在 platform handle descriptor 为 `XID` 时生效，不能只判断 Linux OS。
- 监听 `Opened`、`Bounds`、`WindowDecorationMargin`、`WindowState`、`TransparencyLevelHint`、
  `CanResize`、`FrameShadowThickness` 和 `IsCsdEnabled`。
- 输入区坐标先按 `RenderScaling` 从 DIP 转成 device pixel。
- 普通状态下使用 `FrameShadowThickness` 计算四边 inset，并保留 `ShadowInputRegionResizeBand = 10.0`
  DIP 的 resize 抓手带；`CanResize=false` 时不保留 resize band。
- 最大化、全屏或没有需要穿透的 shadow inset 时，通过 `ResetWindowInputRegion()` 恢复整个 surface。

`AtomUI.Native` 只负责已经确定坐标后的低层操作：

- `WindowExtensions.SetWindowInputRectangle()` / `ResetWindowInputRegion()` 是 Desktop manager 调用的统一入口。
- `WindowUtilsLinux.SetInputRectangle()` 只接收 XID 和 device pixel 矩形，不读取 Window token 或模板状态。
- `XcbConnectionHolder` 维护进程级独立 XCB 连接，缓存 SHAPE 扩展可用性，并在 `SyncRoot` 下串行发送请求。
- `WindowUtilsInterop` 只声明 XCB/Xlib P/Invoke、结构体和枚举。

维护该路径时不要重新引入 Linux 版 `SetWindowIgnoreMouseEventsLinux()`、shape-query 旧实现、每次请求
connect/disconnect 的连接模型，也不要把 Window 事件订阅、resize band 或 theme token 下沉到 Native。

---

## 12. Wayland 上的对应方案

`Avalonia.Wayland` 支持 xdg-shell window/popup、CSD/SSD 协商、分数缩放、
IME、剪贴板、拖放和 portal 文件对话框。AtomUI 的自动入口在 `WAYLAND_DISPLAY` 可用时选择该后端；
Avalonia 自带的 `UsePlatformDetect()` 在 Linux 仍只选择 X11，不能代替 AtomUI 的入口。

### AtomUI 的处理边界

1. Wayland 的 `IWindowImpl.Handle` 为 `null`，`Position` 没有全局语义，`Move(PixelPoint)` 是 no-op。
   因此不得调用 Xlib 初始几何、读取 XID 或自行计算绝对居中位置。
2. Avalonia 会把 `WindowDrawnDecorations.ShadowThickness` 通过
   `IWindowImpl.SetShadowExtents` 转为 xdg surface window geometry。AtomUI 只需维护绘制装饰主题和阴影厚度，
   不应复制这条协议路径。
3. Wayland CSD/SSD 可动态协商。AtomUI 监听 `DrawnDecorationsRequestChanged`，并以
   `NeedsManagedDecorations` 更新 `IsCsdEnabled`，不能继续用 X11 options 推断。
4. `BeginMoveDrag` / `BeginResizeDrag` 保留。输入事件 platform cookie 会把正确 serial 传给
   Wayland 后端，调用方无需自行处理协议 serial。
5. 标题栏中的 Popup 补偿仍保留。绘制装饰内容位于 `TopLevelHost` 的独立视觉根，普通
   `TopLevel.GetTopLevel()` 和 light-dismiss root 仍不能覆盖所有标题栏 Popup 场景。
6. Avalonia 的 `OverlayPopupHost.Screens` 使用 `PopupOverlayLayer.AvailableSize` 作为定位边界；AtomUI 的
   `VisualLayerManager` 覆盖整个 CSD surface，所以该尺寸包含透明阴影缓冲区。不能给 overlay layer 设置全局
   Margin 或修改 popup offset，否则 Drawer、Adorner 和已有的 CSD frame 补偿会被重复 inset；应保留完整
   surface 坐标系，并在窗口视觉层根节点统一裁剪阴影区域。
7. `PopupOverlayLayer`、OverlayLayer、AdornerLayer 和 LightDismissOverlayLayer 都由窗口模板中的
   `VisualLayerManager` 管理。Linux Window 模板使用 `WindowVisualLayerClip` 在该根节点外统一设置窗口 frame
   Clip，按 `FrameShadowThickness` 排除透明阴影缓冲区，同时保持 VLM 的完整 surface 尺寸和 popup 坐标系。
   裁剪区域包含标题栏，并由模板绑定随窗口尺寸、阴影厚度和圆角更新；Windows 和 macOS 模板不引入该层。

### 当前限制

Wayland 核心协议提供 `wl_surface.set_input_region`，但当前 Avalonia 公共 API 不能表达 surface input region。
更关键的是，Avalonia Wayland 后端把协议对象放在专用 `AvaloniaWayland` worker 线程：

- `WindowImpl` 位于 UI 线程，只持有生成的 `WXdgTopLevelProxy`。
- `WaylandWorkerClient.Marshaller` 把普通协议状态写入 `PostWithCommit`，使请求与下一次 surface commit
  在 worker 线程上按序执行。
- 真实 `WSurface`、`WlSurface` 和 `WaylandGlobals` 是 worker-thread state；源码明确不允许 UI 线程
  绕过 proxy 直接访问。

当前 AtomUI 的 `WaylandWindowReflectionExtensions` 会从 proxy 取出真实 target，再由
`WaylandWindowUtils` 直接调用 NWayland。该路径能表达协议请求，但不符合上述线程和重连契约，属于待修复的
私有 API 技术债，不能在本文中描述为 Avalonia 支持的安全扩展点。正确方案必须把 input-region 状态放进
Avalonia 的 persistent surface 模型，并通过 `PostWithCommit`/生成 proxy 下发；在框架提供公开 API 前，
AtomUI 不能把当前反射路径视为稳定架构。

另外，Wayland 的 CSD 选择具有 sticky 语义。`WindowImpl.SetWindowDecorations(None/TitleBar)` 会设置
`_csdSticky=true`、销毁 `zxdg_toplevel_decoration_v1`，之后即使再设 `Full` 也不会恢复 SSD。因此不能把
`IsCsdEnabled=False` 简单映射为 `WindowDecorations=None`：当合成器刚协商出 SSD 时，这个 Setter 会立即
把窗口永久切回 CSD。若要尊重 CSD/SSD 协商，SSD 分支必须保留 `WindowDecorations=Full`。

Wayland 还不支持应用控制绝对窗口位置、激活、置顶、窗口图标、任务栏显示以及最小化/最大化能力提示；
`Screen.WorkingArea` 当前等同 `Bounds`。这些是后端能力限制，不应由 AtomUI Window hack 模拟。

---

## 13. 阴影设计建议

### 配方一：轻量悬浮风格

`ShadowThickness=32`，需要 input region 裁剪：

```xml
<Setter Property="BoxShadow"
        Value="0 6 16 0 #14000000,    <!-- rgba(0,0,0,0.08) -->
               0 3 6 -4 #1F000000,    <!-- rgba(0,0,0,0.12) -->
               0 9 28 8 #0D000000"/>  <!-- rgba(0,0,0,0.05) -->
```

### 配方二：macOS Big Sur 风（深、自然）

`ShadowThickness=24`，需要 input region 裁剪：

```xml
<Setter Property="BoxShadow"
        Value="0 2 6 0 #33000000,    <!-- ambient -->
               0 12 36 0 #59000000"/> <!-- key -->
```

### 配方三：保守（不裁剪 input region 也不违和）

`ShadowThickness=16`：

```xml
<Setter Property="BoxShadow"
        Value="0 1 3 0 #2E000000,
               0 6 14 0 #4A000000"/>
```

### 通用规则

- **不要带颜色**（青/蓝/紫）→ 永远显假。只用纯黑 + 低 alpha。
- **至少两层**：ambient（紧贴）+ key（远投），单层永远糊。
- **Spread 慎用**：spread > 0 让阴影向外膨胀；负 spread 让阴影"内缩"，做近距亮影时配 `0 3 6 -4` 挺好用。
- **BoxShadow 总延展 ≤ ShadowThickness**：估算公式 `|offsetY| + blur/2 + spread` 必须 ≤ ShadowThickness，否则被 OS 客户区裁切发硬。
- **Color #AARRGGBB**：rgba alpha 转 hex：`Math.Round(alpha * 255).ToString("X2")`。0.08 ≈ `#14`，0.12 ≈ `#1F`，0.05 ≈ `#0D`。

---

## 14. Window ControlTheme 跨平台设计模式

> **场景**：你给 `Window`（或自定义 `AppWindow : Window`）写了一份 ControlTheme，里面有自绘的标题栏控件（比如 macOS 风红绿灯）。
> macOS 下 `WindowDrawnDecorations` 永不实例化，必须由 Window 自己的模板出标题栏；
> Win32 / Linux 下 `WindowDrawnDecorations` 已经替你画了标题栏 —— **如果 Window 模板里也画一份，就出现"双标题栏"**。
>
> 这一节给出团队推荐的优雅解法。

### 14.1 核心信号：`WindowDecorationMargin.Top > 0`

不要用 `OperatingSystem.IsMacOS()` 之类做 OS 嗅探 —— 嗅探**真正的运行时信号**才是 Avalonia way：

| 平台 | `WindowDecorationMargin.Top` | 含义 |
|---|---|---|
| macOS | **永远 `0`**（`NeedsManagedDecorations` 硬编码 false）| 没人替你画 → 你自己画 |
| Win32（extend hint=on） | **= 标题栏高** > 0 | `WindowDrawnDecorations` 在画 |
| X11（双门激活） | **= 标题栏高 + 阴影厚** > 0 | `WindowDrawnDecorations` 在画 |
| Wayland（协商为 CSD） | **= 标题栏高 + 阴影厚** > 0 | `WindowDrawnDecorations` 在画 |
| 任意平台关 extend hint | `0` | OS 原生标题栏 |

**结论**：`WindowDecorationMargin.Top > 0 ⇔ "WindowDrawnDecorations 正在替我画标题栏"**。

这个信号有三大优点：
1. **天然跨平台**：不写 `#if MACOS`
2. **运行时响应**：用户切 `ExtendClientAreaToDecorationsHint`、X11 关掉 `EnableDrawnDecorations`、最大化导致阴影变 0 → 全部自动适配
3. **Wayland 友好**：CSD/SSD 协商变化时，布局随实际装饰状态更新

### 14.2 推荐方案：用 Style 替换 `Template`，而不是 `IsVisible`

**反模式（不推荐）**：

```xml
<!-- ❌ Border 实例一直存在，只是不显示 -->
<Border x:Name="PART_CustomTitleBar"
        IsVisible="{Binding $parent[Window].WindowDecorationMargin.Top,
                            Converter={x:Static local:DoubleIsZeroConverter.Instance}}">
  <local:MacTrafficLights/>
</Border>
```

问题：CSD 激活时，`PART_CustomTitleBar` 仍在 visual tree 里占内存、参与 measure 遍历；同时 `WindowDrawnDecorations` 内部又有一份 PART_TitleBar —— **两份 caption 实例并存**。

**推荐模式**：在 ControlTheme 里挂 `Style`，伪类激活时**整体替换 `Template`**，让自绘标题栏从根本上不存在：

```xml
<ControlTheme x:Key="{x:Type local:AppWindow}" TargetType="local:AppWindow">

  <!-- 默认模板：包含自绘标题栏（macOS / 关 CSD 场景） -->
  <Setter Property="Template">
    <ControlTemplate>
      <Panel>
        <Border x:Name="PART_CustomTitleBar"
                Height="32" VerticalAlignment="Top"
                Background="{DynamicResource TitleBarBrush}">
          <local:MacTrafficLights/>
        </Border>
        <ContentPresenter Content="{TemplateBinding Content}"
                          Margin="0,32,0,0"/>
      </Panel>
    </ControlTemplate>
  </Setter>

  <!-- CSD 激活：换一份不带自绘 TitleBar 的模板 -->
  <Style Selector="^:csd-active">
    <Setter Property="Template">
      <ControlTemplate>
        <Panel>
          <ContentPresenter Content="{TemplateBinding Content}"
                            Margin="{Binding $parent[local:AppWindow].WindowDecorationMargin}"/>
        </Panel>
      </ControlTemplate>
    </Setter>
  </Style>

</ControlTheme>
```

伪类切换时 Avalonia 会**销毁旧 visual tree、按新 Template 重建**，自绘标题栏的 Border **从此不存在**。`Content` 是引用绑定，应用内容状态不丢。

### 14.3 维护伪类：`AppWindow` 基类

```csharp
public class AppWindow : Window
{
    static AppWindow()
    {
        WindowDecorationMarginProperty.Changed
            .AddClassHandler<AppWindow>((w, _) => w.UpdateCsdPseudoClass());
    }

    public AppWindow() => UpdateCsdPseudoClass();

    private void UpdateCsdPseudoClass()
        => PseudoClasses.Set(":csd-active", WindowDecorationMargin.Top > 0);
}
```

`WindowDecorationMargin` 是 `StyledProperty`，订阅静态 `Changed` 事件即可全局响应所有实例。

### 14.4 两种方案对照

| 维度 | `IsVisible` 隐藏 | Style 替换 Template ⭐ |
|---|---|---|
| Border 实例存在？ | ✅ 一直在 | ❌ CSD 激活时根本不创建 |
| 切换时 visual tree | 不变 | 重建一次（轻量） |
| Measure / Arrange 节点 | 包含隐藏节点 | 完全干净 |
| 输入命中遍历 | 跳过 IsVisible=False | 完全跳过 |
| 模板内部状态 | 保留 | 丢失（`Content` 不受影响）|
| 代码风格 | 命令式 | 声明式 |
| Avalonia 内部惯用？ | 偶尔 | **是**，Fluent 主题大量使用 |

**结论**：永远用 Style 替换 Template，除非你确实需要在切换时保留模板内部状态。

### 14.5 共享 caption UI：避免实现两份红绿灯

如果不想在 Window ControlTheme 和 `WindowDrawnDecorations` ControlTheme 里各写一份红绿灯按钮，把它抽成 `UserControl`：

```
App.Controls/
  AppWindow.cs                    -- 维护 :csd-active 伪类
  MacTrafficLights.axaml(.cs)     -- ⭐ 共享 caption 控件

Themes/
  AppWindow.axaml                 -- 双 Template ControlTheme（§14.2）
  CustomWindowDrawnDecorations.axaml
                                  -- WindowDrawnDecorations 的 ControlTheme，
                                     PART_TitleBar 里也塞 <local:MacTrafficLights/>
```

两个标题栏宿主复用同一个 caption 实现：

```xml
<!-- Themes/CustomWindowDrawnDecorations.axaml -->
<Border x:Name="PART_TitleBar" Height="36">
  <local:MacTrafficLights/>          <!-- ⭐ 同一个 -->
</Border>

<!-- Themes/AppWindow.axaml （默认模板分支）-->
<Border x:Name="PART_CustomTitleBar" Height="32">
  <local:MacTrafficLights/>          <!-- ⭐ 同一个 -->
</Border>
```

**唯一一份 caption UI 实现，两个宿主复用** —— 跨平台视觉自动对齐，零冗余。

### 14.6 反模式集合（别这么干）

```csharp
// ❌ OS 嗅探：写死分支无法响应运行时变化
if (OperatingSystem.IsMacOS()) titleBar.IsVisible = true;
```

```xml
<!-- ❌ x:OnPlatform 在 XAML load 时一次性求值，不响应运行时切换 -->
<Border IsVisible="{x:OnPlatform True, OSX=False}"/>
```

```xml
<!-- ❌ 既画自绘 TitleBar 又开 ExtendClientAreaToDecorationsHint，没做任何条件控制 -->
<!-- → Linux/Win32 上双标题栏并存 -->
```

```csharp
// ❌ 想从外部读 WindowDrawnDecorations 的属性来判断
var hasCsd = window.GetVisualDescendants<WindowDrawnDecorations>().Any();
//                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^
// WindowDrawnDecorations 是 StyledElement，不是 Visual，永远找不到
```

### 14.7 小结流程

把这套模式落地的清单：

1. **创建** `AppWindow : Window`，监听 `WindowDecorationMargin` 维护 `:csd-active` 伪类
2. **抽出** caption 控件成 `UserControl`（如 `MacTrafficLights`）
3. **写** `AppWindow` 的 ControlTheme，用 §14.2 的双 Template 模式
4. **写** `WindowDrawnDecorations` 的 ControlTheme，`PART_TitleBar` 里复用同一个 caption 控件
5. **应用** `ExtendClientAreaToDecorationsHint=true` 到所有 `AppWindow` 实例
6. **配置** `X11PlatformOptions { EnableDrawnDecorations = true }`

最终效果：

| 平台 | 谁画标题栏 | caption 控件实例 |
|---|---|---|
| macOS | `AppWindow` 默认模板的 `PART_CustomTitleBar` | 1 份 |
| X11 | `WindowDrawnDecorations` 的 `PART_TitleBar` | 1 份 |
| Win32 | `WindowDrawnDecorations` 的 `PART_TitleBar` | 1 份 |

**永远只有一份 caption 实例**，零 OS 分支，自动响应运行时变化。

---

## 15. 常见坑

| 坑 | 现象 | 解决 |
|---|---|---|
| 没设 `EnableDrawnDecorations=true` | X11 上原生标题栏还在 | `Program.cs` 加 `X11PlatformOptions { EnableDrawnDecorations = true }` |
| `EnableDrawnDecorations` 编译警告 | `AVALONIA_X11_CSD` 实验性 | csproj `<NoWarn>AVALONIA_X11_CSD</NoWarn>` |
| `WindowDecorationsTheme` 找不到主题 | `StaticResource` 在合并字典构造时过早解析 | 使用指向 `{x:Type WindowDrawnDecorations}` 的 `DynamicResource` |
| 用户内容跟标题栏不对齐 | 内容铺满 OS 客户区 | `Margin="{Binding $parent[Window].WindowDecorationMargin}"` |
| 阴影太假（青色光晕） | BoxShadow 带颜色 | 改纯黑 + 低 alpha + 双层 |
| 阴影区变 resize 光标 | `ResizeGripLayer.GripThickness = frame+shadow` | (a) 缩小 ShadowThickness；(b) 用 X11 SHAPE 裁 input region |
| macOS 自定义没生效 | `NeedsManagedDecorations` 硬编码 false | 接受现实，macOS 走原生 |
| Win32 没阴影/边框 | Win32 只 request `TitleBar` | 用无条件选择器（不带 `:has-shadow`）画 BoxShadow |
| 模板里少 PART | 按钮无响应 / 启动 NRE | 7 个 `PART_*` 按钮 + `PART_TitleBar` 一个不能少 |
| `GetVisualDescendants<WindowDrawnDecorations>()` 找不到 | 它是 `StyledElement` 不是 `Visual` | 别从可视树找，需要它的属性请用 `Window.WindowDecorationMargin` 间接读，或在调用方手动传 |
| Wayland 启动时空 handle/位置异常 | X11 几何 hack 在所有 Linux 后端执行 | 按后端拆分 chrome；Wayland 禁止读取 XID、设置绝对位置和调用 X Shape（详见 §12） |
| Linux/Win32 出现双标题栏 | Window ControlTheme 自绘 + `WindowDrawnDecorations` 也画 | 用 Style 替换 Template 的双模板模式（详见 §14）|
| 用 OS 嗅探切自绘标题栏 | `#if MACOS` / `OperatingSystem.IsMacOS()` | 改用 `:csd-active` 伪类 + `WindowDecorationMargin.Top > 0` 信号（§14.1）|
| 用 `IsVisible=False` 隐藏自绘标题栏 | Border 实例残留，两份 caption 并存 | 改用 Style 替换 Template，让自绘标题栏从 visual tree 消失（§14.2）|

---

## 16. 架构验证清单

| 集成点 | 必须保持的契约 |
|---|---|
| `WindowDrawnDecorations` | 默认 thickness、运行时 `ShadowThickness`、template parts 与伪类同步一致 |
| Window decorations | `WindowDecorationMargin`、绘制装饰和 resize grips 随几何变化同步 |
| X11 | `XID` gating、四类绘制装饰和 SHAPE input region 只在 X11 路径生效 |
| Win32 | 仅自定义标题栏，边框、阴影和 resize hit-test 继续由 DWM/Avalonia Win32 承担 |
| macOS | 原生窗口装饰路径不创建 `WindowDrawnDecorations` |
| Wayland | CSD/SSD、shadow extents、worker proxy 与 commit 顺序保持一致 |

框架升级时针对当前依赖重新验证这些契约。外部类型可作为当次调查入口，但具体版本、tag、commit、
文件路径和源码行号都不是 AtomUI 的架构事实来源。

`dotnet run` 即可启动。只有验证 X11/XWayland 路径时才需要确认 X server 提供 SHAPE 扩展；
原生 Wayland 不使用 `xprop` 或 X11 SHAPE。
