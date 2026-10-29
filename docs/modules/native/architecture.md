# AtomUI.Native 架构

## 定位

`AtomUI.Native` 是 AtomUI 的内部底层 OS 能力封装层。它按“是否属于操作系统、窗口系统或原生运行时
调用”划定边界，而不是按 `Window`、`Dialog`、主题、控件等上层概念划定边界。只要实现需要直接接触
P/Invoke、原生 handle、原生结构体、平台协议对象、native hook 或平台错误码，就应优先收敛到
`AtomUI.Native`。

窗口相关能力可以进入 `AtomUI.Native`。Win32、DWM、Objective-C runtime、Xlib/XCB、Wayland protocol
等窗口系统原生调用，本质上是底层 OS/backend 能力，不应因为服务于 Window/Dialog 就回到控件层。

`AtomUI.Native` 不决定控件策略、主题策略或用户可观察行为。上层库负责选择何时启用某个原生能力，
并承担 Avalonia 控件生命周期、事件订阅、属性同步和模板状态。Native 层提供能力，不拥有业务状态。

Avalonia drawn decorations、`TopLevelHost`、resize grip layer 等 Desktop 控件私有结构发现通常保留在
`AtomUI.Desktop.Controls`。只有当实现必须解析原生协议对象、调用原生 API，或把 P/Invoke/协议细节
压回 Native 边界时，才应进入 `AtomUI.Native`。

```text
AtomUI.Desktop.Controls
  - 选择 Window chrome manager
  - 管理 Window 事件、属性和模板生命周期
  - 决定何时启用某个原生能力
        |
        v
AtomUI.Native
  - 校验原生 handle / 参数
  - 调用 Win32、Objective-C、Xlib/XCB 或 NWayland
  - 封装原生结构体、常量、消息和协议对象
  - 管理原生资源、hook 和错误边界
```

所有成员目前都是 `internal`，通过 `InternalsVisibleTo` 向 `AtomUI.Desktop.Controls` 和目标程序集名称
`AtomUI.Mobile.Controls` 开放。当前仓库没有 Mobile 实现消费者；这项预留只定义未来可见性边界，不是 Mobile 源码、
平台验证或发布证据。它是实现包，不是公共 Native SDK。

### 能力与策略分界

| 层级 | 可以做 | 不应该做 |
|---|---|---|
| `AtomUI.Native` | 封装 OS/backend 原生调用、窗口系统底层能力、消息结构体、hook/façade、句柄校验、原生资源释放 | 决定窗口主题、Dialog 行为、控件状态机、Token、默认平台策略 |
| `AtomUI.Core` | AtomUI 基础设施、主题/Token/语言、资源、动画、应用启动默认选项；必要时调用 `AtomUI.Native` 获取底层 OS 能力 | 直接散落 P/Invoke、原生结构体、平台协议细节，或依赖具体控件生命周期 |
| `AtomUI.Desktop.Controls` | Window/Dialog/Popup 等桌面控件语义、生命周期、何时调用 Native 能力 | 直接维护 P/Invoke、原生结构体或重复实现 OS 协议 |
| `AtomUI.Mobile.Controls`（目标） | Viewport、Gesture、Overlay、Theme、owner 生命周期、平台默认策略，以及何时调用 Native 能力 | 直接维护 P/Invoke、原生 handle/结构体、不可释放 hook，或把 OS 类型暴露到 Public Control API |

`AtomUI.Core` 可以依赖 `AtomUI.Native`。`Core` 是 AtomUI 的基础设施层，不等于必须排除所有 Window
或 OS 概念；判断标准是能力本身是否属于底层 OS/native 操作。例如安全打开文件、解析真实路径、读取系统
窗口 metric、封装窗口系统原生调用，都可以由 `Core` 通过 `AtomUI.Native` 使用。相反，Dialog 的默认尺寸、
最大/最小尺寸策略、控件状态同步和模板生命周期仍属于上层控件库。

## 当前依赖

| 依赖 | 用途 | 影响 |
|---|---|---|
| `Avalonia` | `Window`、`WindowBase`、geometry 和 platform handle | AtomUI 基础依赖 |
| `NWayland` | `WlSurface`、`WlCompositor`、`WlRegion` | 会进入所有引用 Native 的依赖图 |

`AtomUI.Native` 当前与 `Avalonia.Wayland` 共享 NWayland 类型。具体版本以集中包管理和最终 restore
依赖图为准；升级任一依赖时必须同步核对类型版本和跨包 ABI，不能把当前偶然一致视为兼容保证。

上层项目是否引用 `AtomUI.Native` 取决于是否存在真实底层 OS/native 调用点。不能为了让 `Core` 看起来
“纯净”而把 P/Invoke、native struct 或平台错误处理留在 `Core`；也不能因为某个 native 能力最终服务于
Window/Dialog，就把底层 OS 细节放回 `AtomUI.Desktop.Controls`。

未来 Mobile adapter 同样遵循这条边界：优先使用 Avalonia 公共 API；只有 Safe Area、system bar、input pane、返回手势、
haptic 或生命周期能力确实需要原生调用时才进入 Native。Native 负责底层资源和确定释放，Mobile Runtime 负责
`TopLevel` owner、订阅、snapshot 合并、取消和产品行为。

## 目录和职责

```text
src/AtomUI.Native/
├── AtomUI.Native.csproj
├── NativeWindowSizing.cs
├── WindowExtensions.cs
├── FileSystem/
│   └── NativeFileSystem.cs
├── Windows/
│   ├── WindowsCsdSizingHook.cs
│   ├── WindowUtils.Interop.cs
│   └── WindowUtils.Windows.cs
├── MacOS/
│   ├── WindowUtils.Interop.cs
│   └── WindowUtils.MacOS.cs
└── Linux/
    ├── WindowUtils.Interop.cs
    ├── WindowUtils.Linux.cs
    ├── XcbConnectionHolder.cs
    ├── WaylandWindowReflectionExtensions.cs
    ├── WaylandWindowUtils.cs
    └── Rendering/
```

### Windows

- `SetWindowIgnoreMouseEventsWindows()` 修改 `WS_EX_TRANSPARENT/WS_EX_LAYERED`。
- `WindowsCsdSizingHook` 封装 opt-in 的 `WM_GETMINMAXINFO` track size 修正和
  `WM_ENTERSIZEMOVE/WM_EXITSIZEMOVE` live resize 信号。
- `WindowsBackgroundHook` 在首次显示前用 GDI 填充客户区背景，避免原生空白帧；它只处理
  `WM_ERASEBKGND`/`WM_SHOWWINDOW`，并在 `WM_ENTERSIZEMOVE` 与 `WM_EXITSIZEMOVE` 之间跳过擦除填充。
  默认 `RedirectionSurface` 下 GDI 填充会先于渲染帧被 DWM 合成，live resize 期间填充会表现为整窗闪背景色
  （见 [Windows Live Resize 与窗口装饰架构](../../architecture/systems/windowing/windows-live-resize.md)）。
- Windows 目录可以封装 DWM 属性、系统 metric、Win32 消息结构体和受限的可释放 hook。
- Windows 目录不决定 live resize 策略、合成后端策略、CSD/SSD 策略或控件行为；合成后端顺序由
  `AtomUI.Core` 的 `WithAtomUIDefaultOptions()` 决定。
- 窗口装饰与 resize hit-test 由 Avalonia CSD 管理；AtomUI 不处理 `WM_NCCALCSIZE` 或
  `WM_NCHITTEST`，不返回 resize hit-test。
- 标题栏按钮通过 Avalonia 公共 `WindowDecorationProperties.ElementRole` 接入原生行为，
  不需要 AtomUI 自定义 caption WndProc hook。
- 允许 Native 层提供窄范围、显式 opt-in、可释放的 sizing/message helper，例如只处理
  `WM_GETMINMAXINFO` 或 live resize 生命周期信号；这类 helper 不能实现第二套 window chrome。

### macOS

- Objective-C runtime 调用 `NSWindow.ignoresMouseEvents`。
- 查询和重排 standard window buttons。
- x64 的 32-byte `NSRect` 返回走 `objc_msgSend_stret`，其他架构走 `objc_msgSend`。

### Linux/X11

- Xlib 设置 map 前初始 geometry 和 `WM_NORMAL_HINTS`。
- `_GTK_FRAME_EXTENTS` 告知 Mutter/KWin CSD 阴影范围。
- 进程级独立 XCB 连接复用 SHAPE input-region 请求。

X11 能力必须以 `IPlatformHandle.HandleDescriptor == "XID"` 为前置条件，不能只判断 Linux OS。

### Linux/Wayland

`WaylandWindowUtils` 只接受已经解析出的 NWayland `WlSurface/WlCompositor`，创建 region 并调用
`SetInputRegion()`。Avalonia 私有字段解析集中在 `WaylandWindowReflectionExtensions`，不能扩散到
`WaylandWindowUtils` 或上层业务控件。

但 Avalonia Wayland 后端的真实 surface 属于专用 worker。当前调用方从 UI 线程越过
`WXdgTopLevelProxy` 直接调用真实 `WlSurface`，不符合上游线程和重连契约。`DynamicDependency` 只保证
reflection metadata 不被裁剪，不能解决该问题。正确边界应由 Avalonia persistent surface/proxy 提供
input-region 方法，并通过 `WaylandWorkerClient.PostWithCommit` 下发。

`Linux/Rendering` 的实验性帧调度沿用 Avalonia Wayland worker 和现有 EGL 资源 owner：
允许在上一帧 callback 等待期间准备一个后备帧，callback 清理后再提交。Native 管理有界状态、
静态 session、callback relay 与 EGL 错误；Desktop 管理受限反射、版本门槛及默认启用、可显式关闭的诊断开关。
资源与恢复约束见 [Linux 帧调度](linux.md#实验性-wayland-egl-帧调度)。

### FileSystem

- `NativeFileSystem` 封装跨平台文件系统原生调用，例如 no-follow 打开文件和解析目录 canonical path。
- 文件系统能力可以被 `AtomUI.Core` 使用；判断标准是是否需要 P/Invoke、平台错误码、native handle 或
  OS-specific flag，而不是调用方是否位于 Core。
- Native 层只负责底层安全打开、路径解析和错误转换，不决定 ThemeDefinition 的加载策略、搜索路径或
  用户目录信任模型。

## 设计约束

1. 平台判断和后端判断是两层条件。Linux 不等于 X11，非 X11 也不等于 Wayland。
2. 只有 Avalonia 公共 API 无法表达、且涉及原生调用或原生协议细节的能力才进入 Native。
3. Window 相关 native 能力可以进入 Native；限制点是不能在 Native 层拥有 Window/Dialog 控件策略。
4. Native 层可以持有原生资源或可释放 hook，但 owner 必须清晰，上层必须有明确释放点。
5. Native 层不修改 Avalonia control properties，不决定 resize band、主题 token 或 Dialog/Window 行为。
6. Native 层不订阅控件业务事件；需要事件驱动时，上层负责订阅 Avalonia 事件并调用 Native 能力。
7. 原生调用失败不能让 Window manager 的更新队列永久卡死；异步/延迟状态更新需要 `try/finally` 恢复标志。
8. AOT 要同时满足 metadata 保留和真实 publish；`DynamicDependency`、普通 build、源码字符串测试分别只能
   覆盖一部分风险。

## 已知结构问题

### 平台依赖扩散

`AtomUI.Native` 当前只面向桌面控件等需要原生能力的上层库开放。`AtomUI.Desktop.Controls` 直接引用
`Avalonia.Desktop` 和 `Avalonia.Wayland`，Browser 当前通过 WASM 内部 item 过滤部分 desktop assembly，
这只是构建补偿，不是清晰的依赖边界。

长期建议把 desktop bootstrap 与具体后端拆成可选 assembly，例如 X11、Wayland、Win32、macOS backend
包；Browser 只引用平台无关 controls，不应先引入桌面包再从 bundle 中删除 assembly。

### API 命名和校验

`WindowExtensions` 只承担内部跨平台路由，具体实现保留在 `Windows`、`MacOS` 和 `Linux` 目录。
只有 Avalonia 公共 API 无法表达的能力才进入 Native；平台实现应统一校验 handle descriptor、零句柄、
尺寸范围和平台。是否继续拆分文件取决于资源所有权和生命周期复杂度，不为了形式上的分层搬移代码。

### 测试层级

当前 Native 相关测试主要读取源码并断言字符串。这能防止某些旧代码回归，但不能验证 ABI、线程、资源释放、
协议顺序或真实 NativeAOT。至少需要：

- 纯计算和参数校验单元测试。
- 可替换 native façade 的调用顺序/错误路径测试。
- X11/Wayland 集成测试或最小 smoke app。
- Linux/Windows/macOS 的 NativeAOT publish matrix。

## 依赖升级检查

Native 架构结论以 AtomUI 的平台边界、公开契约和验证矩阵为准。升级 Avalonia 或原生协议依赖时，至少重新验证：

- Wayland proxy、worker 和 persistent surface 的线程所有权与 commit 顺序。
- CSD/SSD 协商、shadow extents 和 window geometry 的状态传播。
- X11 handle descriptor、drawn decorations gating 和 input-region 行为。
- Win32/macOS 平台能力仍由公开 API 或隔离的 Native 边界承担。

外部框架的类型名可以作为调查入口，但文件路径、源码行号和某次源码快照都不是 AtomUI 的架构契约。
