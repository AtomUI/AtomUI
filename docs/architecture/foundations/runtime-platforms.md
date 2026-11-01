# 运行平台策略

AtomUI 当前面向 Windows、macOS、Linux 桌面运行，同时支持 Browser Gallery 作为展示和验证路径。移动端已有批准的
iOS/Android 目标架构，但当前解决方案没有 `AtomUI.Mobile.Controls` 项目或 Mobile Gallery Host。

## 桌面端

桌面端通过 Avalonia Desktop/Native 平台运行，主包是 `AtomUI.Desktop.Controls`。桌面端支持：

- Native Window 能力。
- Popup、Overlay、Window、WindowTitleBar 等依赖桌面窗口系统的控件和服务。
- `ToolTipService` 等在 `UseDesktopControls()` 初始化回调中注册的桌面服务。
- Native 层提供的窗口穿透、macOS 标题栏按钮控制、Linux X11 相关能力。

## 浏览器端

Browser Gallery 使用 `net10.0-browser` 和 `Avalonia.Browser`。浏览器环境下 `RuntimePlatform.Features.SupportsNativeWindow` 为 false，注册链路会切换到浏览器 Provider：

- `BrowserCommonControlThemesProvider`
- `BrowserDesktopControlThemesProvider`

`UseDesktopControls()` 也会使用浏览器安全的 Token 类型列表，避免注册需要 Native Window 的控件 Token。浏览器端主要用于控件展示和兼容验证，不应假定所有桌面窗口能力都可用。

## Native 层

`AtomUI.Native` 采用 partial class 加平台目录组织：

- `Windows/`：Win32 P/Invoke。
- `MacOS/`：Objective-C Runtime P/Invoke。
- `Linux/`：XCB/X11 P/Invoke 与 Wayland input-region 协议 helper。

上层通过 `WindowExtensions` 调用统一入口，内部按运行时平台路由。Native 能力属于基础设施，不直接面向普通控件使用者暴露。

## 移动端

`AtomUI.Mobile.Controls` 与 iOS/Android 宿主位于独立仓库 [AtomUI/AtomUI.Mobile](https://github.com/AtomUI/AtomUI.Mobile)，其运行时架构、平台能力与验证状态由该仓库维护，入口见
[Mobile 系统](../systems/mobile/overview.md)。本仓库维护 Mobile 依赖的上游契约（见[项目依赖关系](dependency-graph.md)）与
[Apple iOS 工程环境](../../engineering/platforms/apple-ios/overview.md)；Android 工程文档在获得真实工具链证据前只定义验证边界。
