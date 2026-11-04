# Issue 528：无装饰顶层 HWND 样式完整性修复

**Goal:** 保留默认 RedirectionSurface，让普通 Avalonia Window 的 `Topmost + FullScreen + Show(owner)` 正常显示、激活和接收输入。

**Architecture:** Desktop 包应用作用域管理普通 Window 的接线和释放；AtomUI.Native 在 `WM_STYLECHANGING` 中补齐缺少类型位的 redirection 顶层 HWND。Avalonia 继续拥有状态、保存快照、装饰和几何。

**Tech Stack:** .NET 10、Avalonia 12.1.4、Win32/ANGLE/DXGI、强类型 hook、NativeAOT。

**Spec:** [AtomUI #528](https://github.com/AtomUI/AtomUI/issues/528)、[前次分析会话](codex://threads/01a1a52d-5848-7fe2-b666-b5603e380d92)及用户要求。

**执行方式:** `superpowers:executing-plans`；过程与验收证据见[执行记录](../progress/2026-11-04-issue528-fullscreen-repair.md)。

## 约束

- 不修改 Avalonia 源码；用户允许 Native hook/反射，但不为此引入不必要反射。
- 不改变 Topmost、全屏、owner、装饰、布局或默认合成顺序，不以软件渲染、延时刷新或失败重试规避问题。
- 覆盖普通 `Avalonia.Controls.Window`，不能仅修 AtomUI Window 或 Gallery 触发位置。
- 新成员 internal。Native 封装原生能力；Desktop 负责生命周期；不增加 Core 反向引用。
- 临时测试默认删除，不新增永久测试、公共 API 或测试专用生产抽象；无授权不 commit。

## 原始取证

源码：AtomUI HEAD `49f021e4774d4c686d2a6f0b20c7355eb2404718`；本地参考 Avalonia 位于
`D:/workspace/projects/AtomUI/.referenceprojects/Avalonia`，commit `2d79ba55fae1b1e71c69f93a668080c9029e6a31`，12.1.4，未修改。
环境：Windows build 26300、.NET SDK 10.0.300、2560 × 1600、150% 缩放；报告为 build 26200。
设备枚举到 NVIDIA RTX 4060 Laptop / Intel UHD，驱动 32.0.15.8129 / 32.0.101.7088；未单独证明实际适配器选择。

临时实窗探针通过按钮 Click 处理器创建窗口并执行 `Show(owner)`，脚本自动 RaiseEvent，未模拟物理鼠标。
100ms UI 心跳、500ms 实际屏幕采样；`PointToScreen(45,45)` 读取洋红色内容标记。
各场景独立进程、自动关闭，进程内外设超时；DXGI 诊断 hook 只记录/透传，另以无诊断 hook 用例交叉验证。

| 场景 | 标记可见 | ANGLE 错误 | 结论 |
|---|---:|---|---|
| 默认 + FullScreen + Topmost | 0/4 | 持续发生 | 复现 |
| 纯 Avalonia，相同 RedirectionSurface 配置 | 0/4 | 持续发生 | 排除 AtomUI Window/theme |
| WinUIComposition + FullScreen + Topmost | 4/4 | 无 | 对照正常 |
| 默认 + FullScreen，不置顶 | 4/4 | 无 | 对照正常 |
| 默认 + Maximized + Topmost | 4/4 | 无 | 对照正常 |
| Software + FullScreen + Topmost | 4/4 | 无 | 对照正常 |
| 原故障组合，仅 SWP_FRAMECHANGED | 0/4 | 持续发生 | 不是 frame refresh 问题 |
| 原故障组合，仅补 WS_POPUP | 4/4 | 无 | 连续三次独立重复正常 |

故障时 UI 心跳约 27～28 次，窗口激活，GetForegroundWindow 和 WindowFromPoint 均返回 demo HWND。
已证明的是无有效图像的全屏置顶窗口遮挡输入，没有证明操作系统死锁。

## 根因与实施期间的修正

```text
AngleEgl + RedirectionSurface
  -> Avalonia SetFullScreen 移除 WS_OVERLAPPEDWINDOW，未添加 WS_POPUP
  -> HWND style=16000000 / exStyle=00040008
  -> CreateSwapChainForHwnd: DXGI_ERROR_INVALID_CALL (887A0001)
  -> EGL_BAD_ALLOC (3003)，surface 无法创建
  -> 全屏置顶 HWND 仍占据屏幕并命中输入
```

仅将 style 改为 `96000000`，其他参数不变，DXGI 即成功且正常出图；改 swap effect、buffer count、宽度减一无效。
未追踪闭源 DXGI 的具体拒绝分支，不能推导为所有合法 Win32 无装饰窗口都必须有 WS_POPUP。

最初方案仅在全屏定位阶段读 `_isFullScreenActive` 并补位，原型通过原始复现与状态往返。
正式实施边界验证发现：禁用 CanResize/CanMinimize/CanMaximize 的无装饰普通窗口恢复时也生成同类 style，
窄方案只通过 5/8 次采样，出现 208 行 ANGLE 错误。普通尺寸也失败，说明全屏只是触发路径之一。

**最终决策：** 修复每次实际原生样式写入的不变量，替代私有全屏状态反射，统一覆盖首次显示、全屏进出、能力/装饰变更及恢复。

## Native 能力与接线

1. `UseDesktopControls()` commit 注册应用作用域的 `WindowsWindowStyleCompatibility`，由 `context.Own` 管理。
2. `WindowOpenedEvent` 路由事件早于首次初始化/样式/原生显示；在这里挂接，CLR Opened 太晚。
3. 先归一化已有 HWND，再在 `WM_STYLECHANGING` 对 `STYLESTRUCT.StyleNew` 应用同一幂等规则。
4. 已有 WS_CHILD、WS_POPUP 或任意 WS_OVERLAPPEDWINDOW 位时不变；已有 WS_EX_NOREDIRECTIONBITMAP 时不变；其余仅加 WS_POPUP。
5. 不设置 handled，不嵌套定位，不做 frame refresh，不反射私有字段，不改变 Avalonia 属性/快照。
6. 样式 API 和结构体签名匹配 32 位 DWORD；返回零检查 last-error；失败卸载并向上层每应用一次报告警告。
7. CWT 弱管理逐窗能力；隐藏再显示去重；WM_DESTROY/WM_NCDESTROY、Closed、作用域释放和启动回滚均清理。
8. 使用最初的 `IWin32OptionsTopLevelImpl` 卸载，避免 Closed 前 PlatformImpl 清空；只移除自己的 callback。
9. 不处理消息的观察者放在已有 callback 链之前，保留原链的 handled/非零返回值；不能只验证其他 callback 被调用。

| 文件 | 责任 |
|---|---|
| `src/AtomUI.Native/NativeWindowStyles.cs` | internal 跨平台入口 |
| `src/AtomUI.Native/Windows/WindowsBorderlessStyleHook.cs` | 消息、样式变换、原生错误边界、释放 |
| `src/AtomUI.Native/Windows/WindowUtils.Interop.cs` | 消息、常量、STYLESTRUCT、32 位样式 API |
| `src/AtomUI.Desktop.Controls/Window/WindowsWindowStyleCompatibility.cs` | 类事件、逐窗弱映射、去重、清理 |
| `src/AtomUI.Desktop.Controls/ThemeManagerBuilderExtensions.cs` | Desktop 包接线 |

## Task 1：原始 RED → Native 根因修复 → 实窗 GREEN

- 通过实际 `UseAtomUI(...UseDesktopControls())` 链路，先证明原条件在产品修改前失败。
- 实现共享样式不变量，覆盖普通 Avalonia Window，无新增公共 API/引用。
- 验证原始条件、Normal/Maximized/FullScreen 往返、Topmost、不可调整大小窗口、运行时能力变化、隐藏再显示、
  完整装饰、已有 popup、其他 hook、WinUIComposition、Software、非 Windows guard。
- 断言实际屏幕像素、原生输入目标、激活、Topmost、owner、全屏尺寸、callback 释放与关闭窗口可回收。

## Task 2：发布、审查、清理与模块验证

- 无反射/动态序列化探针通过真实产品接线 NativeAOT 发布并运行；不关闭 trim/AOT analyzer。
- 临时生命周期测试验证应用 class handler 释放和幂等 Dispose。
- 同步 Native/窗口架构文档，完成一次全改动新上下文只读审查。
- 删除临时测试、探针、专用产物并停止进程；清理后精确 Window/Dialog 模块验证与 diff-check。
- Windows 10、多屏、跨 DPI 切换未在本机证明，不泛化 Win11 结果；不擅自全量回归或提交。

具体运行结果、测试去留、清理及最终门禁由[执行记录](../progress/2026-11-04-issue528-fullscreen-repair.md)承载。
