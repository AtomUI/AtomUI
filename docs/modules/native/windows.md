# AtomUI.Native — Windows 平台 API 文档

## 概述

Windows 平台通过 `user32.dll` 的窗口扩展样式（Extended Window Styles）机制实现窗口级鼠标事件穿透，通过
`WindowsBackgroundHook` 提供首次显示前的原生背景填充（见第 5 节），并通过 `WindowsPopupShadowHitTestHook` 实现窗口型
Popup 阴影区域的按形状输入穿透（见第 6 节）。所有 API 均标注 `[SupportedOSPlatform("windows")]`。

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
| `PlatformNotSupportedException` | 当前平台不受支持 |

**Windows 实现原理：**

通过修改窗口扩展样式（`GWL_EXSTYLE`）中的 `WS_EX_TRANSPARENT` 和 `WS_EX_LAYERED` 位标志：

- **启用穿透**：`ExStyle |= (WS_EX_TRANSPARENT | WS_EX_LAYERED)`
- **禁用穿透**：`ExStyle &= ~(WS_EX_TRANSPARENT | WS_EX_LAYERED)`

> ⚠️ `WS_EX_LAYERED` 是 `WS_EX_TRANSPARENT` 生效的前提条件，两者必须同时设置。

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

**Windows 实现原理：**

读取窗口扩展样式并检查 `WS_EX_TRANSPARENT` 位：

```csharp
(GetWindowLongPtr(handle, GWL_EXSTYLE) & WS_EX_TRANSPARENT) != 0
```

---

## 2. P/Invoke 声明

> 命名空间：`AtomUI.Native.Windows`  
> 类：`internal static class WindowUtilsInterop`  
> 平台：`[SupportedOSPlatform("windows")]`

### 2.1 Win32 函数

#### GetWindowLongPtr

获取窗口的扩展样式或其他窗口属性。

```csharp
[DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
public static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `hWnd` | `IntPtr` | 窗口句柄 |
| `nIndex` | `int` | 属性索引（如 `GWL_EXSTYLE = -20`） |

**返回值：** 请求的窗口属性值（`long`）。

---

#### SetWindowLongPtr

设置窗口的扩展样式或其他窗口属性。

```csharp
[DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
public static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `hWnd` | `IntPtr` | 窗口句柄 |
| `nIndex` | `int` | 属性索引 |
| `dwNewLong` | `long` | 新的属性值 |

**返回值：** 设置前的旧属性值（`long`）。

---

### 2.2 常量定义

| 常量 | 值 | 类型 | 说明 |
|------|----|------|------|
| `GWL_STYLE` | `-16` | `int` | 窗口样式索引 |
| `GWL_EXSTYLE` | `-20` | `int` | 扩展窗口样式索引 |
| `WS_EX_TRANSPARENT` | `0x20L` | `long` | 鼠标事件穿透标志位 |
| `WS_EX_LAYERED` | `0x80000L` | `long` | 分层窗口标志位（穿透的前提条件） |
| `WM_ERASEBKGND` | `0x0014` | `int` | 擦除背景消息，`WindowsBackgroundHook` 在此填充客户区 |
| `WM_SHOWWINDOW` | `0x0018` | `int` | 显示/隐藏消息，`WindowsBackgroundHook` 在显示时主动填充一次 |
| `WM_ENTERSIZEMOVE` | `0x0231` | `int` | 进入模态移动/调整大小循环 |
| `WM_EXITSIZEMOVE` | `0x0232` | `int` | 退出模态移动/调整大小循环 |
| `WM_DESTROY` | `0x0002` | `int` | 窗口销毁，`WindowsPopupShadowHitTestHook` 停止轮询 |
| `WM_NCHITTEST` | `0x0084` | `int` | 命中测试，`WindowsPopupShadowHitTestHook` 按内容形状返回 |
| `HTTRANSPARENT` | `-1` | `int` | 命中测试结果：交给同线程下方窗口 |
| `LWA_ALPHA` | `0x2` | `uint` | `SetLayeredWindowAttributes` 使用整窗 alpha |
| `GA_ROOTOWNER` | `3` | `uint` | `GetAncestor` 取根 owner（Popup 所属主窗口） |
| `DWMWA_EXTENDED_FRAME_BOUNDS` | `9` | `int` | 可见窗口边界（不含不可见 resize 边框） |

第 6 节另用到 `SetLayeredWindowAttributes`、`GetWindowRect`、`GetAncestor`、`GetCursorPos` 与
`DwmGetWindowRectAttribute`（`DwmGetWindowAttribute` 的 `RECT` 输出重载，独立命名以避免与 `out uint` 重载在 `out var` 调用处二义）。

---

## 3. 使用示例

```csharp
// 启用窗口鼠标事件穿透
window.SetWindowIgnoreMouseEvents(true);

// 查询是否处于穿透状态
bool isPassThrough = window.IsWindowIgnoreMouseEvents();

// 禁用穿透，恢复正常鼠标交互
window.SetWindowIgnoreMouseEvents(false);
```

---

## 4. 注意事项

1. **句柄有效性** — 调用前确保 `window.PlatformImpl?.Handle?.Handle` 不为 `null` 或 `IntPtr.Zero`
2. **样式组合** — `WS_EX_TRANSPARENT` 必须与 `WS_EX_LAYERED` 同时使用才能实现完整的鼠标穿透
3. **线程安全** — Win32 窗口操作应在 UI 线程执行
4. **副作用** — 设置 `WS_EX_LAYERED` 可能影响窗口的渲染行为，对于已经是分层窗口的场景无额外影响

---

## 5. WindowsBackgroundHook（原生背景填充）

> - 命名空间：`AtomUI.Native.Windows`
> - 类：`internal sealed class WindowsBackgroundHook : INativeWindowBackgroundHook`
> - 入口：`NativeWindowBackground.TryAttachWindowsBackgroundHook(Window)`
> - 调用方：`WindowsWindowChromeManager.PrepareInitialShowState()`，窗口关闭时释放

### 5.1 职责

在 Avalonia 首帧渲染上屏之前，用 GDI 以窗口背景色填充客户区，避免首次显示时出现原生空白（白色或黑色）帧。
颜色来自 `Window.Background`，其次 `TransparencyBackgroundFallback`，只接受不透明分量；颜色为空时不填充。

通过公开 API `Win32Properties.AddWndProcHookCallback` 挂接，不使用反射：

| 消息 | 行为 |
|------|------|
| `WM_SHOWWINDOW`（显示） | 主动 `GetDC` 填充一次客户区 |
| `WM_ERASEBKGND` | 在 live resize 之外填充客户区并返回已处理 |
| `WM_ENTERSIZEMOVE` / `WM_EXITSIZEMOVE` | 维护 live resize 状态 |

### 5.2 live resize 期间不填充

`WM_ENTERSIZEMOVE` 与 `WM_EXITSIZEMOVE` 之间，`WM_ERASEBKGND` 不做填充，交还 Avalonia 默认行为
（Avalonia 窗口类的 `hbrBackground` 为空，不擦除）。

原因：AtomUI 默认合成模式为 `RedirectionSurface`。该模式下 GDI 填充写入 redirection surface，会先于
Avalonia 的渲染帧被 DWM 合成；拖动调整大小时每次 `WM_ERASEBKGND` 填充都会表现为整窗闪背景色。
脚本化验证（510 帧）中，关闭该填充后整窗白屏帧由 2～3 帧降为 0，且对向边缘无错帧。

维护约束：

- 不得在 live resize 期间恢复 GDI 背景填充，也不得改为在 `WM_SIZE`/`WM_PAINT` 中填充。
- 首次显示前的填充（`WM_SHOWWINDOW` 和非 live resize 的 `WM_ERASEBKGND`）保持不变。
- 完整根因、验证方法与数据见
  [Windows Live Resize 与窗口装饰架构](../../architecture/systems/windowing/windows-live-resize.md)。

## 6. WindowsPopupShadowHitTestHook（窗口型 Popup 阴影输入穿透）

> - 命名空间：`AtomUI.Native.Windows`
> - 类：`internal sealed class WindowsPopupShadowHitTestHook : IDisposable`
> - 入口：`NativePopupShadowHitTest.TryAttach(TopLevel popupRoot, Func<bool> isInputSuspended, Func<Point, bool> isInContentShape)`，
>   非 Windows 返回 `null`
> - 调用方：`AtomUI.Desktop.Controls.Popup` 在 `Opened` 时对 `PopupRoot` 挂接，`Closed` 时释放

### 6.1 职责

窗口型 Popup 的阴影与内容画在同一窗口 surface（动效同帧），窗口矩形包含阴影 buffer。本 hook 只把输入区域收缩到
内容形状，阴影部分对输入透明。内容形状与“是否处于动效”由调用方以委托提供，hook 只负责 Win32 输入路由：

| 输入 | 来源 |
|------|------|
| `isInContentShape(Point)` | 客户区 DIP 坐标是否在内容形状内（Popup 用 Avalonia `InputHitTest` 实现） |
| `isInputSuspended()` | 为 true 时整窗穿透（打开/关闭动效期间） |

### 6.2 两层判定

| 层 | 条件 | 行为 |
|----|------|------|
| 第一层（同进程） | `WM_NCHITTEST` 的点在内容形状外，或动效期间 | `handled = true`，返回 `HTTRANSPARENT`，系统交给同线程下方窗口 |
| 第二层（跨进程） | 第一层条件成立，且该点位于所属主窗口（`GA_ROOTOWNER` 的 `DWMWA_EXTENDED_FRAME_BOUNDS`）之外 | 加 `WS_EX_TRANSPARENT`，启动 16ms `DispatcherTimer` 轮询 `GetCursorPos` |
| 恢复 | 轮询发现光标回到内容形状、回到主窗口内或移出 Popup 窗口 | 移除 `WS_EX_TRANSPARENT`，停止轮询，交还第一层 |

内容形状内的 `WM_NCHITTEST` 不处理，交还 Avalonia `PopupImpl.WndProc`（其整窗 `SetHitTestVisible(false)` 仍然生效）。
Popup 完全位于主窗口内时不会进入第二层。

### 6.3 分层窗口样式

`WS_EX_TRANSPARENT` 只在 `WS_EX_LAYERED` 窗口上跨进程穿透。hook 创建时：

1. 记录原扩展样式，补上 `WS_EX_LAYERED`；
2. `SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA)`。整窗 alpha 255 不改变逐像素透明。

运行时只切换 `WS_EX_TRANSPARENT`：仅 `SetWindowLongPtr(GWL_EXSTYLE)`，不调用 `SetWindowPos(SWP_FRAMECHANGED)`，
不触发重绘或影响动效。

兼容性（Windows 11，默认 `RedirectionSurface`，Avalonia 旧式透明 `DwmEnableBlurBehindWindow`）已用 `ddagrab`
逐帧验证：透明圆角与阴影无黑框、不丢逐像素 alpha，打开/关闭动效每帧阴影与内容同帧；两次样式切换期间
内容区录像无任何变化帧。WinUI/DirectComposition 合成模式（`WS_EX_NOREDIRECTIONBITMAP`）下未验证。

### 6.4 生命周期

- `WM_DESTROY`：标记窗口已销毁并停止轮询。
- `Dispose`：停止轮询、移除 WndProc hook；窗口仍存在时恢复原扩展样式（含移除 `WS_EX_LAYERED`）。
- Avalonia 关闭 Popup 时先销毁 `PopupRoot`，再触发 `Closed`，因此通常由 `WM_DESTROY` 先完成清理，`Dispose` 不再写样式。

### 6.5 维护约束

- 不得改为独立阴影窗口：两个窗口各自 Present，动效无法同帧。
- 不得在运行时增删 `WS_EX_LAYERED`（会重建 redirection surface 并重绘）；只切换 `WS_EX_TRANSPARENT`。
- `HTTRANSPARENT` 只在同线程窗口间转发，主窗口外的阴影必须由第二层处理。
- 命中只决定事件交给哪个窗口；主窗口的 light dismiss 遮罩语义不变（点击阴影 = 点击 Popup 外部）。

