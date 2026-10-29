# AtomUI.Native — macOS 平台 API 文档

## 概述

macOS 平台通过 Objective-C Runtime（`libobjc.A.dylib`）直接调用 AppKit 框架的 `NSWindow` / `NSView` API，实现窗口鼠标事件穿透、标题栏红绿灯按钮位置控制及窗口关闭按钮状态管理，并通过 `MacOSPopupShadowPassThrough` 实现窗口型 Popup 阴影区域的输入穿透（见第 5 节）。所有原生调用均标注 `[SupportedOSPlatform("macos")]`。

---

## 1. 公共扩展方法

### 1.1 SetWindowIgnoreMouseEvents

设置窗口是否忽略（穿透）鼠标输入事件。

```csharp
internal static void SetWindowIgnoreMouseEvents(this WindowBase window, bool flag)
```

**参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `window` | `WindowBase` | Avalonia 窗口实例 |
| `flag` | `bool` | `true` 启用穿透，`false` 禁用穿透 |

**异常：**

| 异常类型 | 条件 |
|----------|------|
| `ArgumentException` | 窗口句柄无效（`IntPtr.Zero`） |

**macOS 实现原理：**

调用 `NSWindow` 的 `setIgnoresMouseEvents:` 方法：

```objc
[nsWindow setIgnoresMouseEvents:flag];
```

通过 ObjC Runtime 消息发送实现：

```csharp
objc_msgSend_void_bool(handle, SetIgnoresMouseEventsSelector, flag);
```

---

### 1.2 IsWindowIgnoreMouseEvents

查询窗口当前是否处于鼠标事件穿透状态。

```csharp
internal static bool IsWindowIgnoreMouseEvents(this WindowBase window)
```

**参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `window` | `WindowBase` | Avalonia 窗口实例 |

**返回值：**

| 类型 | 说明 |
|------|------|
| `bool` | `true` 表示窗口正在穿透鼠标事件 |

**macOS 实现原理：**

调用 `NSWindow` 的 `ignoresMouseEvents` 属性 getter：

```csharp
objc_msgSend_bool(handle, IgnoresMouseEventsSelector);
```

---

### 1.3 SetMacOSOptionButtonsPosition ⟨macOS 专属⟩

自定义窗口标题栏红绿灯按钮（关闭、最小化、缩放）的位置。

```csharp
[SupportedOSPlatform("macos")]
public static void SetMacOSOptionButtonsPosition(
    this WindowBase window,
    double x,
    double y,
    double spacing = 10.0)
```

**参数：**

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `window` | `WindowBase` | — | Avalonia 窗口实例 |
| `x` | `double` | — | 第一个按钮（关闭按钮）的 X 坐标偏移 |
| `y` | `double` | — | 按钮的 Y 坐标偏移（Cocoa 坐标系，取负值向下移动） |
| `spacing` | `double` | `10.0` | 按钮之间的水平间距 |

**异常：**

| 异常类型 | 条件 |
|----------|------|
| `ArgumentException` | 窗口句柄无效 |

**行为说明：**

- 在设计模式（`Design.IsDesignMode`）下直接返回，不执行任何操作
- 按钮按 **Close → Minimize → Zoom** 的顺序从左到右排列
- 每个按钮的 Y 坐标使用 `-y`（Cocoa 坐标系 Y 轴向上，取负实现向下偏移）
- 修改完成后强制重绘标题栏（`setNeedsDisplay:` on superview）

**实现流程：**

```
1. GetStandardWindowButton(CloseButton)      → 获取关闭按钮 NSView
2. GetStandardWindowButton(MiniaturizeButton) → 获取最小化按钮 NSView
3. GetStandardWindowButton(ZoomButton)        → 获取缩放按钮 NSView
4. 逐个设置 frame.Origin = (offset, -y)
5. offset += spacing + buttonWidth
6. SetNeedsDisplay(superview, true)           → 强制重绘标题栏
```

---

### 1.4 GetMacOSOptionsSize ⟨macOS 专属⟩

获取窗口标题栏红绿灯按钮组的总尺寸。

```csharp
[SupportedOSPlatform("macos")]
public static Size GetMacOSOptionsSize(this WindowBase window, double spacing = 10.0)
```

**参数：**

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `window` | `WindowBase` | — | Avalonia 窗口实例 |
| `spacing` | `double` | `10.0` | 按钮之间的间距（用于计算总宽度） |

**返回值：**

| 类型 | 说明 |
|------|------|
| `Size` | 三个按钮组的总宽度和最大高度。设计模式下返回 `default` |

**计算逻辑：**

```
totalWidth = closeBtn.Width + spacing
           + minimizeBtn.Width + spacing
           + zoomBtn.Width
maxHeight  = max(closeBtn.Height, minimizeBtn.Height, zoomBtn.Height)
```

---

### 1.5 SetMacOSWindowClosable ⟨macOS 专属⟩

启用或禁用窗口的关闭按钮。

```csharp
[SupportedOSPlatform("macos")]
public static void SetMacOSWindowClosable(this WindowBase window, bool flag)
```

**参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `window` | `WindowBase` | Avalonia 窗口实例 |
| `flag` | `bool` | `true` 启用关闭按钮，`false` 禁用关闭按钮 |

**实现流程：**

```
1. GetStandardWindowButton(CloseButton)   → 获取关闭按钮 NSView
2. GetStyleMask(window)                   → 读取当前样式掩码
3. SetStyleMask(window, styleMask)        → 重新设置样式掩码以刷新按钮状态
4. SetEnabled(closeButton, flag)          → 设置按钮启用/禁用
```

---

## 2. P/Invoke 声明

> 命名空间：`AtomUI.Native.MacOS`  
> 类：`internal static class WindowUtilsInterop`  
> 平台：`[SupportedOSPlatform("macos")]`

### 2.1 Objective-C Runtime 核心函数

#### objc_getClass

获取指定名称的 Objective-C 类对象。

```csharp
[DllImport("/usr/lib/libobjc.A.dylib")]
public static extern IntPtr objc_getClass(string className);
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `className` | `string` | ObjC 类名（如 `"NSWindow"`） |

**返回值：** 类对象指针（`IntPtr`）。

---

#### sel_registerName

注册或获取一个 Objective-C Selector。

```csharp
[DllImport("/usr/lib/libobjc.A.dylib")]
public static extern IntPtr sel_registerName(string selectorName);
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `selectorName` | `string` | Selector 名称（如 `"setIgnoresMouseEvents:"`） |

**返回值：** Selector 指针（`IntPtr`）。

---

#### objc_msgSend 重载族

Objective-C 消息发送函数的不同签名重载：

| 函数签名 | 返回类型 | 参数 | 说明 |
|----------|----------|------|------|
| `objc_msgSend_intptr(IntPtr, IntPtr)` | `IntPtr` | receiver, selector | 无参消息，返回指针 |
| `objc_msgSend_intptr_long(IntPtr, IntPtr, long)` | `IntPtr` | receiver, selector, long arg | 单 long 参数消息 |
| `objc_msgSend_void(IntPtr, IntPtr)` | `void` | receiver, selector | 无参无返回值消息 |
| `objc_msgSend_void_bool(IntPtr, IntPtr, bool)` | `void` | receiver, selector, bool arg | 单 bool 参数消息 |
| `objc_msgSend_bool(IntPtr, IntPtr)` | `bool` | receiver, selector | 无参消息，返回 bool |
| `objc_msgSend_void_rect(IntPtr, IntPtr, NSRectFull)` | `void` | receiver, selector, rect | 单 NSRect 参数消息 |
| `objc_msgSend_rect(IntPtr, IntPtr)` | `NSRectFull` | receiver, selector | 无参消息，返回 NSRect |
| `objc_msgSend_point(IntPtr, IntPtr)` | `NSPoint` | receiver, selector | 无参消息，返回 NSPoint（两个 double，x64/arm64 均寄存器返回，不走 `_stret`） |
| `objc_msgSend_ulong(IntPtr, IntPtr)` | `ulong` | receiver, selector | 无参消息，返回 ulong |
| `objc_msgSend_void_ulong(IntPtr, IntPtr, ulong)` | `void` | receiver, selector, ulong arg | 单 ulong 参数消息 |

---

### 2.2 预注册 Selector 缓存

所有常用 Selector 在静态字段中预注册，避免运行时重复查找：

| 字段名 | Selector 字符串 | 对应 ObjC 方法 |
|--------|-----------------|----------------|
| `SetIgnoresMouseEventsSelector` | `"setIgnoresMouseEvents:"` | `[NSWindow setIgnoresMouseEvents:]` |
| `IgnoresMouseEventsSelector` | `"ignoresMouseEvents"` | `[NSWindow ignoresMouseEvents]` |
| `MouseLocationOutsideOfEventStreamSelector` | `"mouseLocationOutsideOfEventStream"` | `[NSWindow mouseLocationOutsideOfEventStream]` |
| `StandardWindowButtonSelector` | `"standardWindowButton:"` | `[NSWindow standardWindowButton:]` |
| `FrameSelector` | `"frame"` | `[NSView frame]` |
| `SetFrameSelector` | `"setFrame:"` | `[NSView setFrame:]` |
| `SuperviewSelector` | `"superview"` | `[NSView superview]` |
| `SetNeedsDisplaySelector` | `"setNeedsDisplay:"` | `[NSView setNeedsDisplay:]` |
| `StyleMaskSelector` | `"styleMask"` | `[NSWindow styleMask]` |
| `SetStyleMaskSelector` | `"setStyleMask:"` | `[NSWindow setStyleMask:]` |
| `SetEnabledSelector` | `"setEnabled:"` | `[NSControl setEnabled:]` |

---

### 2.3 辅助方法

#### GetStandardWindowButton

获取窗口的标准按钮（关闭、最小化、缩放等）。

```csharp
public static IntPtr GetStandardWindowButton(IntPtr window, long buttonType)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `window` | `IntPtr` | NSWindow 指针 |
| `buttonType` | `long` | 按钮类型（参见 `NSWindowButton` 枚举） |

**返回值：** 按钮的 `NSView` 指针。

---

#### GetFrame / SetFrame

获取或设置 NSView 的 frame 矩形。

```csharp
public static NSRectFull GetFrame(IntPtr view)
public static void SetFrame(IntPtr view, NSRectFull frame)
```

---

#### GetSuperview

获取 NSView 的父视图。

```csharp
public static IntPtr GetSuperview(IntPtr view)
```

---

#### SetNeedsDisplay

标记 NSView 需要重绘。

```csharp
public static void SetNeedsDisplay(IntPtr view, bool flag)
```

---

#### GetStyleMask

获取窗口的样式掩码。

```csharp
public static ulong GetStyleMask(IntPtr window)
```

---

#### SetEnabled

设置控件（如按钮）的启用状态。

```csharp
public static void SetEnabled(IntPtr control, bool enabled)
```

---

### 2.4 结构体定义

#### CGPoint

Core Graphics 点结构体。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct CGPoint
{
    public double X;
    public double Y;
}
```

#### CGSize

Core Graphics 尺寸结构体。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct CGSize
{
    public double Width;
    public double Height;
}
```

#### NSRect

AppKit 矩形结构体（CGPoint + CGSize 版本）。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct NSRect
{
    public CGPoint Origin;
    public CGSize Size;
}
```

#### NSPoint / NSSize

AppKit 点和尺寸结构体。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct NSPoint { public double X; public double Y; }

[StructLayout(LayoutKind.Sequential)]
public struct NSSize { public double Width; public double Height; }
```

#### NSRectFull

AppKit 矩形结构体（NSPoint + NSSize 版本），用于 `frame` 操作。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct NSRectFull
{
    public NSPoint Origin;
    public NSSize Size;
}
```

---

### 2.5 枚举定义

#### NSWindowButton

标准窗口按钮类型。

```csharp
public enum NSWindowButton : long
{
    CloseButton            = 0,  // 关闭按钮（红色）
    MiniaturizeButton      = 1,  // 最小化按钮（黄色）
    ZoomButton             = 2,  // 缩放按钮（绿色）
    ToolbarButton          = 3,  // 工具栏按钮
    DocumentIconButton     = 4,  // 文档图标按钮
    DocumentVersionsButton = 5,  // 文档版本按钮
    FullScreenButton       = 7   // 全屏按钮
}
```

#### WindowButtonType

窗口按钮类型（与 `NSWindowButton` 对应）。

```csharp
public enum WindowButtonType : long
{
    CloseButton            = 0,
    MinimizeButton         = 1,
    ZoomButton             = 2,
    ToolbarButton          = 3,
    DocumentIconButton     = 4,
    DocumentVersionsButton = 5,
    FullScreenButton       = 7
}
```

#### NSWindowStyleMask

窗口样式掩码（Flags 枚举）。

```csharp
[Flags]
public enum NSWindowStyleMask : ulong
{
    Borderless             = 0,         // 无边框
    Titled                 = 1 << 0,    // 带标题栏
    Closable               = 1 << 1,    // 可关闭
    Miniaturizable         = 1 << 2,    // 可最小化
    Resizable              = 1 << 3,    // 可调整大小
    TexturedBackground     = 1 << 8,    // 纹理背景
    UnifiedTitleAndToolbar = 1 << 12,   // 统一标题和工具栏
    FullScreen             = 1 << 14,   // 全屏
    FullSizeContentView    = 1 << 15,   // 全尺寸内容视图
    UtilityWindow          = 1 << 4,    // 工具窗口
    DocModalWindow         = 1 << 6,    // 文档模态窗口
    NonactivatingPanel     = 1 << 7,    // 非激活面板
    HUDWindow              = 1 << 13    // HUD 窗口
}
```

#### NSWindowCollectionBehavior

窗口集合行为（Flags 枚举）。

```csharp
[Flags]
public enum NSWindowCollectionBehavior : ulong
{
    Default                    = 0,
    CanJoinAllSpaces           = 1 << 0,
    MoveToActiveSpace          = 1 << 1,
    Managed                    = 1 << 2,
    Transient                  = 1 << 3,
    Stationary                 = 1 << 4,
    ParticipatesInCycle        = 1 << 5,
    IgnoresCycle               = 1 << 6,
    FullScreenPrimary          = 1 << 7,
    FullScreenAuxiliary        = 1 << 8,
    FullScreenNone             = 1 << 9,
    FullScreenAllowsTiling     = 1 << 11,
    FullScreenDisallowsTiling  = 1 << 12
}
```

#### NSWindowLevel

窗口层级。

```csharp
public enum NSWindowLevel : int
{
    Normal      = 0,
    Floating    = 3,
    ModalPanel  = 8,
    MainMenu    = 24,
    StatusBar   = 25,
    PopUpMenu   = 101,
    ScreenSaver = 1000
}
```

#### NSBackingStoreType

窗口后备存储类型。

```csharp
public enum NSBackingStoreType : ulong
{
    Buffered = 2
}
```

---

## 3. 使用示例

```csharp
// 启用窗口鼠标事件穿透
window.SetWindowIgnoreMouseEvents(true);

// 查询是否处于穿透状态
bool isPassThrough = window.IsWindowIgnoreMouseEvents();

// 自定义红绿灯按钮位置
if (OperatingSystem.IsMacOS())
{
    window.SetMacOSOptionButtonsPosition(x: 12, y: 8, spacing: 8);
}

// 获取红绿灯按钮组总尺寸
if (OperatingSystem.IsMacOS())
{
    Size buttonsSize = window.GetMacOSOptionsSize(spacing: 8);
    // buttonsSize.Width = 三个按钮宽度 + 2 × spacing
    // buttonsSize.Height = 最高按钮的高度
}

// 禁用窗口关闭按钮
if (OperatingSystem.IsMacOS())
{
    window.SetMacOSWindowClosable(false);
}
```

---

## 4. 注意事项

1. **设计模式保护** — `SetMacOSOptionButtonsPosition`、`GetMacOSOptionsSize`、`SetMacOSWindowClosable` 在 `Design.IsDesignMode` 下直接返回，避免 XAML 预览器中的原生调用崩溃
2. **Cocoa 坐标系** — macOS 使用左下角原点坐标系，Y 轴向上。代码中 `frame.Origin.Y = -y` 将逻辑上的"向下偏移"转换为 Cocoa 坐标
3. **Selector 缓存** — 所有 Selector 通过 `static readonly` 预注册，避免每次调用 `sel_registerName` 的开销
4. **按钮可能为空** — 如果窗口样式不包含某些按钮（如 `Borderless` 窗口），`GetStandardWindowButton` 可能返回 `IntPtr.Zero`，代码中已做非空检查
5. **强制重绘** — 修改按钮位置后需要调用 `SetNeedsDisplay(superview, true)` 触发标题栏重绘，否则视觉位置不会更新

---

## 5. MacOSPopupShadowPassThrough（窗口型 Popup 阴影输入穿透）

由 `NativePopupShadowHitTest.TryAttach` / `SuspendInputBeforeShow` 在 macOS 上创建与调用，实现 `IPopupShadowHitTest`。
控件侧语义见 [Popup 实现原理](../../controls/desktop/other/popup/implementation.md)“8.1 窗口型 Popup 阴影输入穿透”。

AppKit 没有独立于可见内容的输入区域，透明窗口中 alpha 大于 0 的阴影像素仍接收鼠标，因此只能整窗切换 `ignoresMouseEvents`：

| 状态（`MacOSPopupInputState`） | `ignoresMouseEvents` | 轮询 | 条件 |
|---|---|---|---|
| `Receive` | `NO` | 否 | 光标在内容形状内、窗口外，或本窗口持有鼠标按键拖拽 |
| `Suspended` | `YES` | 否 | 打开/关闭动效期间，或 `PopupRoot.IsHitTestVisible` 为 false |
| `PassThrough` | `YES` | 16ms `DispatcherTimer` | 光标在窗口内、内容形状外（阴影） |

- 进入阴影由 `IInputManager.Process` 的原始 Move 判定（按 PopupRoot 的 `PresentationSource` 过滤）。阴影不参与命中，
  路由 `PointerMoved` 在阴影上不会引发。
- 穿透期间读取 `mouseLocationOutsideOfEventStream`：窗口忽略事件时仍返回实时光标；窗口基准坐标原点在左下，
  `DesktopScaling` 为 1，翻转 Y 即客户区 DIP。不需要辅助功能权限。
- `setIgnoresMouseEvents:` 只在状态变化时下发；缓存初始为未下发，首次必定下发，不读取 `ignoresMouseEvents`。AppKit 按窗口选择鼠标事件接收者，同进程与跨进程一致穿透。
- `SuspendInputBeforeShow` 在显示前置为 `YES`（此时原生句柄已存在）。
- 释放：停止轮询、取消订阅；`PlatformImpl` 仍存在时恢复为 `!PopupRoot.IsHitTestVisible`，与 Avalonia.Native
  `SetHitTestVisible`（同样使用 `setIgnoresMouseEvents:`）保持一致。窗口已销毁时 `Invalidate()` 只停止轮询。
