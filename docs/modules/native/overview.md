# AtomUI.Native 模块概览

`AtomUI.Native` 是 AtomUI 的原生平台交互层，RootNamespace 为 `AtomUI.Native`。它通过 P/Invoke
封装 Windows、macOS、Linux/X11 的窗口级能力，并通过 NWayland 表达 Wayland 协议请求。

## 职责

- 提供 Windows/macOS 整窗鼠标穿透能力。
- 提供 macOS 标题栏按钮位置、尺寸、关闭按钮状态等窗口能力。
- 提供 Linux/X11 输入区域、窗口几何等底层辅助能力。
- 承载 Wayland input-region 的协议对象解析与 NWayland 调用；调用时机、输入矩形计算和 Window 状态策略仍属于
  `AtomUI.Desktop.Controls`。
- 承载实验性 Wayland EGL 帧准备与延迟提交；Desktop 负责兼容性探测，已验证的 Wayland EGL 后端默认启用，可显式关闭。
- 为 `AtomUI.Core` 和 `AtomUI.Desktop.Controls` 提供内部基础设施。
- 为目标 `AtomUI.Mobile.Controls` 保留 internal 能力边界；当前没有 Mobile 实现消费者。

未来 Mobile adapter 只有在 Avalonia 公共 API 无法表达所需 OS 能力时才消费 Native。Native 可以拥有 P/Invoke、native
handle、原生结构体、协议对象、可释放 hook 和错误转换；Mobile Controls 继续拥有 viewport、gesture、Overlay、Theme、
owner 生命周期与平台默认策略。预留的 `InternalsVisibleTo` 不代表 Mobile 包已经实现或通过平台验证。

## 目录结构

```text
src/AtomUI.Native/
├── WindowExtensions.cs
├── Windows/
├── MacOS/
└── Linux/
    ├── X11/XCB/Xlib helpers
    ├── WaylandWindowReflectionExtensions.cs
    ├── WaylandWindowUtils.cs
    └── Rendering/                # Wayland EGL prepared frame、callback relay 与错误处理
```

每个平台目录中通常拆分为：

- `WindowUtils.Interop.cs`：P/Invoke 声明、结构体、枚举。
- `WindowUtils.<Platform>.cs`：平台业务逻辑。

## 模块文档与系统入口

- [architecture.md](architecture.md)
- [windows.md](windows.md)
- [macos.md](macos.md)
- [linux.md](linux.md)
- [Windowing 系统](../../architecture/systems/windowing/overview.md)
- [Mobile 系统](../../architecture/systems/mobile/overview.md)
- [Mobile 平台 Adapter](../mobile-controls/platform-adapters.md)
- [跨平台 Window 定制指南](../../guides/windowing/cross-platform-window.md)
- [WindowDrawnDecorations 指南](../../guides/windowing/window-drawn-decorations.md)
