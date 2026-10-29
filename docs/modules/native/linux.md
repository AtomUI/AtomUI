# AtomUI.Native Linux 平台 API

## 范围

AtomUI 在 Linux 上有两条完全不同的窗口后端路径：

| 后端 | Avalonia 包 | 原生对象 | AtomUI 可使用的能力 |
|---|---|---|---|
| X11 / XWayland | `Avalonia.X11` | `IPlatformHandle`，descriptor 为 `XID` | Xlib 初始几何、`_GTK_FRAME_EXTENTS`、XCB SHAPE input region |
| 原生 Wayland | `Avalonia.Wayland` | `IWindowImpl.Handle == null` | xdg-shell move/resize、CSD/SSD、surface geometry；无公开 input-region API |

不得只用 `OperatingSystem.IsLinux()` 决定调用 X11 API。所有 Xlib/XCB 路径还必须验证窗口后端为
X11；原生 Wayland、Linux framebuffer 和 headless 后端都不是 X11。

本文以 AtomUI 当前公开的平台契约、后端隔离规则和验证条件为准。Avalonia 内部类型仅用于解释集成边界，
不能作为 AtomUI 可直接调用的稳定 API。

## X11 能力

### 初始窗口几何

`WindowExtensions.ConfigureLinuxInitialWindowGeometry()` 将 Avalonia DIP 尺寸转成 device pixel，
再调用 `WindowUtilsLinux.ConfigureInitialWindowGeometry()`：

1. `XOpenDisplay()` 打开独立 Xlib 连接。
2. `XSetWMNormalHints()` 写入 position、size、min/max size 和 resize increment。
3. `XMoveResizeWindow()` 在 map 前设置窗口位置和大小。
4. `XFlush()` 后关闭该次 Xlib 连接。

该逻辑依赖 XID 和全局屏幕坐标，只能由 `X11WindowChromeManager` 调用。Wayland surface 没有全局位置，
`Avalonia.Wayland.WindowImpl.Move(PixelPoint)` 明确是 no-op。

### CSD frame extents

`WindowExtensions.SetLinuxX11CsdFrameExtents()` 先验证 platform handle descriptor 为 `XID`，然后由
`WindowUtilsLinux.SetX11CsdFrameExtents()` 写入 `_GTK_FRAME_EXTENTS`。

属性格式为四个 32-bit CARDINAL，顺序是 `left, right, top, bottom`，单位为 device pixel。AtomUI 在
normal 状态写入阴影 extents；最大化和全屏写入全零，避免 Mutter/KWin 继续按旧阴影范围做放置和吸附。

### SHAPE input region

`WindowExtensions.SetWindowInputRectangle()` / `ResetWindowInputRegion()` 用 XCB SHAPE 的
`XCB_SHAPE_SK_INPUT` 把输入区限制为一个矩形，目的是让视觉阴影区域的点击穿透到下方窗口。
底层 `WindowUtilsLinux.SetInputRectangles()` 接受多个矩形（空列表 = 空输入区域，整窗穿透），
`ClearInputShape()` 用 `xcb_shape_mask(None)` 恢复默认输入形状（随窗口尺寸变化的整个窗口）。
Native 层只接收 device pixel 坐标并执行 input-region 请求，不决定 shadow 厚度、resize band 或订阅策略。

当前实现的真实生命周期：

1. `XcbConnectionHolder` 懒创建一条进程级独立 XCB 连接。
2. 首次初始化时查询并缓存 SHAPE 扩展是否可用。
3. 所有请求在 `XcbConnectionHolder.SyncRoot` 下串行执行。
4. `xcb_shape_rectangles_checked()` 替换 input region，随后检查请求并 flush。
5. 该连接在进程生命周期内复用；不是每次调用都 connect/disconnect。

`X11WindowChromeManager` 负责高层 shadow click-through 策略。它监听 `Opened`、`Bounds`、
`WindowDecorationMargin`、`WindowState`、`TransparencyLevelHint`、`CanResize`、`FrameShadowThickness`
和 `IsCsdEnabled`，按 `RenderScaling` 把 DIP 转成 device pixel，并在普通状态下只保留最多 10 DIP 的
resize band。最大化或全屏时 input region 恢复为整个 surface。

窗口型 Popup 由 `X11PopupShadowInputRegion`（经 `NativePopupShadowHitTest.TryAttach` 创建）负责：
Popup 提供平台无关的 `PopupContentShape`（DIP 圆角矩形 + 箭头三角形），`PopupContentRegion` 按 `RenderScaling`
逐像素行光栅化为多矩形提交；窗口映射前由 `SuspendInputBeforeShow()` 先置空（避免整窗输入形状映射到指针下产生越界事件）；
动效期间为空区域；`PopupRoot.LayoutUpdated` 与 `Invalidate()` 触发重新计算，结果未变时不发请求；
释放时窗口仍存活才恢复默认形状。只设置顶层 XID：父窗口输入形状外的点不会进入 GPU 渲染子窗口。详见
[Popup 实现原理](../../controls/desktop/other/popup/implementation.md)“8.1”。

这不是“整窗忽略鼠标”的 API。Linux 版本的 `SetWindowIgnoreMouseEventsLinux()`、
`IsWindowIgnoreMouseEventsLinux()`、`xcb_get_geometry()` 和 shape-query 旧实现已经删除；文档和新代码都
不得继续引用这些成员。

## Wayland 能力与线程模型

Avalonia Wayland 后端使用专用 `AvaloniaWayland` worker 线程：

```text
UI thread WindowImpl
  -> WXdgTopLevelProxy
  -> WaylandWorkerClient.Marshaller
  -> PostWithCommit / PostOob
  -> worker-thread WSurface / WlSurface
```

源码中的关键约束：

- `WaylandWorkerClient.CreateTopLevelHandle()` 只向 UI 线程暴露 `WXdgTopLevelProxy`，注释明确说明真实
  `WXdgTopLevel` 是 worker-thread state。
- 普通 surface 状态由 `PostWithCommit` 排队，与 compositor commit 保持顺序；move/resize 等需要输入
  serial 的请求使用 OOB 路径。
- `WSurface.OnBeforeNewBufferAttached()` 在 attach/commit 前统一应用 frame callback、ack configure、
  window geometry、viewport 和 buffer scale。
- `WindowImpl.SetShadowExtents()` 通过 proxy 更新 persistent surface；`WSurface` 再根据 shadow extents
  调用 `xdg_surface.set_window_geometry`。

Wayland 核心协议虽然有 `wl_surface.set_input_region`，但当前公开框架 API 不能表达该能力。AtomUI 当前
`AtomUI.Native/Linux/WaylandWindowReflectionExtensions.cs` 从私有 proxy 中取出真实 target，并由
`WaylandWindowUtils` 直接调用 NWayland。这绕过了 worker marshalling 和重连模型，只能作为当前技术债记录，
不能视为线程安全或受 Avalonia 支持的 Native 扩展点。后续正确实现应进入 Avalonia persistent surface/proxy，并通过
`PostWithCommit` 下发。

窗口型 Popup 的 `WaylandPopupShadowInputRegion` 同样经反射取得 worker 侧 surface，但不在 UI 线程操作协议对象：
通过反射取得的 `WaylandWorker.PostWithCommit` 把 `WaylandWindowUtils.SetInputRectangles()` 排入合成批次在 worker 线程执行，
surface 已映射（`CanCommitOutOfBand`）时额外 commit。它仍依赖 Avalonia 私有成员（版本升级须复核），但线程模型与
Avalonia 自身 `SetHitTestVisible` 一致。动效期间使用 1×1 占位区域而非空区域：GNOME Mutter 上以空输入区域映射的
xdg_popup 之后不再接收任何输入区域更新。详见 [Popup 实现原理](../../controls/desktop/other/popup/implementation.md)“8.1”。

## 实验性 Wayland EGL 帧调度

`Linux/Rendering` 提供 internal 的单帧准备与延迟提交适配器，版本/成员校验（`WaylandFramePacingCompatibility`）、
render interface 包装和安装入口（`WaylandFramePacing.TryInstall`）均位于 Native。Desktop 只负责策略：
在 Wayland 初始化前根据开关调用安装入口、记录日志，并把失败回调与呈现时间交给上层。
仅对已验证 ABI 的 EGL surface 启用；软件、dmabuf、X11 及其他 surface 原样转发。
已验证的 Wayland EGL 后端默认启用。诊断对照时可在 AppBuilder 初始化前设置
`AppContext.SetSwitch("AtomUI.Experimental.WaylandFramePacing", false)` 显式关闭；设为 true 则显式开启。
该开关不是稳定公共 API；版本或成员签名不匹配时记录拒绝原因并保留原路径，显式开启也不绕过兼容性校验。

- `WaylandSurfaceFrameState` 按 persistent surface 共享；graphics context 更换不能切断旧 callback 与待提交帧的联系。
- 每 surface 最多一个准备帧、一个未完成 callback。准备帧存在时停止继续绘制，避免覆盖尚未提交的后备缓冲区。
- 原 Avalonia target 继续创建、resize 和释放 `wl_egl_window` / `EglSurface`。Native session 使用同一 context，
  flush 后恢复上下文，原 listener 完成清理后才执行原 before-attach 和 EGL swap。
- 连接、surface、configure serial、scale 或 shadow extents 变化会使准备帧失效，退役 target 并请求重绘。
- EGL swap 保留原生返回值；context loss 通知框架重建。延迟提交异常记录后退回原渲染路径，清理本次失败提交
  新建的 callback，不能通过清除正常 callback 强行解除门控。
- 协议与 EGL 操作只在 owning worker 上运行；surface 断开时清理准备帧，不能由 UI 线程直接提交。

### 呈现时间预测

Avalonia 不绑定 `wp_presentation`。Native 经同一版本校验从 `WaylandGlobals` 的 `Registry` 与 `_knownGlobals`
按连接绑定一次，`WaylandPresentationTiming` 在每次准备帧提交前请求 feedback，记录 presented 时间、刷新周期，
以及最近 32 帧“提交→上屏”刷新数的众数。仅在 `CLOCK_MONOTONIC`（Linux 上即 Stopwatch 时钟）与固定刷新
（refresh>0）下提供预测；VRR、其他时钟或尚无 feedback 时返回空，Motion 退回脉冲时间。

- 每帧在**开始绘制**时登记其预计上屏时间（不早于上一帧 + 1 个刷新）；此时场景内容已固定，UI 可在整段绘制期间
  准备下一帧。最初在绘制完成时登记，重 GPU 帧只给 UI 留几毫秒，导致内容重复/跳帧。
- 记录已开始绘制的帧数与在途帧数（已绘制或等待 callback、尚未提交）。Core 只有在其上次提交之后确有新帧开始绘制，
  才把目标推进一个刷新；“渲染器空闲”不作为已消费证据（Wayland 脉冲多于帧，否则内容超前 2–3 个刷新）。
- presented 与登记时间的整刷新残差按 16 帧窗口积分为修正量（仅在窗口内有过半一致残差时调整，避免振荡），
  用于吸收回调晚于 vblank 等未建模阶段；本机最初存在稳定 1 刷新残差，修正后内容时间与上屏时间中位差为 0。

Native 通过安装回调交出 `WaylandPresentationTiming`；Native 不引用 Core，由 Desktop 的 `DesktopAppBuilderExtensions`
以私有适配器实现 Core 的 `IMotionPresentationClock` 并写入 `AtomUI.Animations.MotionFrameTiming.Clock`；
同一连接的多窗口/多输出共享一套计数与呈现网格，跨不同刷新输出时可能偏差（采样单调钳制，不回退）。
internal 计数（presented/predicted/hits）仅供诊断工具读取。

该路径不改变 NavMenu 布局或动画参数。当前实测有明显收益，但尚未满足所有场景的稳定刷新验收，
不能作为已经解决 60/120 FPS 的保证。测量与未覆盖项见
[Motion 验证状态](../../architecture/systems/motion/verification.md)，采样契约见
[Motion 帧流水线](../../architecture/systems/motion/frame-pipeline.md)；升级后需要重新做真实呈现、生命周期与 NativeAOT 验证。

## Wayland 初始缩放预测

Avalonia 12.1.4 的 Wayland toplevel 以缩放 1 起步，mutter 在窗口映射并进入输出后才发送
`wp_fractional_scale.preferred_scale`，因此首次布局与首帧按 1 进行，约 250ms 后整窗按真实缩放重新布局
（本机表现为 NavMenu 文字尺寸 62×22 → 62.4×22.2、窗口缩放 1 → 5/3，观感为“字体变化”，字体族与字号实际不变）。

Native 的 `WaylandInitialScaling`（`Linux/WaylandInitialScaling.cs`）由 AtomUI `Window` 构造末尾、首次布局前调用，
经版本校验的反射读取输出列表：
分数缩放可用时取 mode 宽 ÷ xdg 逻辑宽（考虑 90/270 旋转），按协议 1/120 取整；否则取整数输出缩放。
所有输出缩放一致时调用 impl 的 `UpdateScaling`，与合成器通知走同一路径；缓冲区按每帧 UI 缩放生成，
首帧即为正确缩放，随后到达的 preferred_scale 相同而不触发布局。输出缩放不一致或成员不匹配时不做预测，
预测错误时仍由合成器通知纠正。Popup 本就继承父窗口缩放，不需要处理。

剩余：首次 configure 时客户区宽度由 1300 变为 1300.2 DIP（合成器整数窗口几何 + 阴影边距），
仅页面区域重新布局一次，NavMenu 尺寸与文字不变；该几何取整未处理。

## Wayland CSD/SSD

Wayland 的 `NeedsManagedDecorations` 来自 xdg-decoration 协商：

- Client-side mode：请求 `TitleBar | Border | Shadow | ResizeGrips`。
- Server-side mode：`NeedsManagedDecorations=false`，由 compositor 提供 SSD。
- 协商结果变化时触发 `DrawnDecorationsRequestChanged`。

`WindowDecorations` 在 Wayland 上不是可逆的 CSD/SSD toggle。当前后端的
`SetWindowDecorations(None/TitleBar)` 会设置 `_csdSticky=true` 并销毁 decoration object，之后设置
`Full` 也不会恢复 SSD。因此，AtomUI 不能在 `IsCsdEnabled=False` 时无条件设置
`WindowDecorations=None`；这会把刚协商出的 SSD 立即永久切回 CSD。

## 依赖和边界

`AtomUI.Native` 与 `Avalonia.Wayland` 共享 NWayland 类型，必须在依赖升级时验证最终解析版本和 ABI 兼容性。
`NWayland` 因而会进入所有引用 `AtomUI.Native` 的依赖图，包括 Browser 的共享项目闭包。Browser 项目
目前通过内部 WASM item 过滤桌面 assembly，这能消除已知 P/Invoke collector warning，但不能消除
restore 图中的桌面依赖，也不是理想的模块边界。

长期结构应把 X11、Wayland、Win32、macOS 后端能力放到可选平台 assembly，避免
`AtomUI.Desktop.Controls`/Browser 为单个平台实现携带所有桌面后端和协议依赖。平台 manager 负责
Avalonia 控件生命周期和状态机；`AtomUI.Native` 只负责已经确定后端之后的原生调用，不负责主题、
reflection discovery 或窗口策略。

## 实现与升级检查入口

### AtomUI

- `src/AtomUI.Native/Linux/WindowUtils.Interop.cs`
- `src/AtomUI.Native/Linux/WindowUtils.Linux.cs`
- `src/AtomUI.Native/Linux/XcbConnectionHolder.cs`
- `src/AtomUI.Native/Linux/WaylandWindowReflectionExtensions.cs`
- `src/AtomUI.Native/Linux/WaylandWindowUtils.cs`
- `src/AtomUI.Native/Linux/Rendering/`
- `src/AtomUI.Native/Linux/WaylandInitialScaling.cs`
- `src/AtomUI.Desktop.Controls/DesktopAppBuilderExtensions.cs`（启用策略、日志与 Motion 时钟接线）
- `src/AtomUI.Native/WindowExtensions.cs`
- `src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs`
- `src/AtomUI.Desktop.Controls/Window/Chrome/WaylandWindowChromeManager.cs`

### 框架集成验证项

- Wayland 窗口只向 UI 线程暴露 proxy，真实协议对象留在 worker 线程。
- 普通 surface 状态通过 commit 队列提交，不能从 UI 线程直接操作协议对象。
- CSD/SSD 协商、shadow extents 与 window geometry 保持同一状态传播链。
- X11 路径继续以 `XID` descriptor 为门槛，不能退化为 Linux OS 判断。

框架升级时应针对当前依赖重新验证这些行为，不在长期文档中固化外部文件路径、行号或源码快照。
