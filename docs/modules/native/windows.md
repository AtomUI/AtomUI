# AtomUI.Native — Windows 平台 API 文档

## 概述

Windows 平台通过 `user32.dll` 的窗口扩展样式（Extended Window Styles）机制实现窗口级鼠标事件穿透，并通过
`WindowsBackgroundHook` 提供首次显示前的原生背景填充（见第 5 节）。所有 API 均标注 `[SupportedOSPlatform("windows")]`。

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

