# 窗口型 Popup 阴影与鼠标穿透方案记录

日期：2026-10-29。状态：Windows 第一层 + 第二层与阴影 token 统一已实现并通过脚本化视觉验证（见文末“Windows 实施结果”）；
Linux X11 已实现并验证输入形状（见“X11 实施结果”）；原生 Wayland 已实现并在 GNOME Mutter 上验证（见“Wayland 实施结果”）；
macOS 已实现并在 Apple Silicon 实机验证（见“macOS 实施结果”）。

窗口型 Popup（`ShouldUseOverlayPopup=False`，独立原生窗口）的阴影体验明显差于 Overlay Popup。
原因是原生窗口的阴影只能画在窗口自身 surface 内：阴影画得越大，窗口越大，阴影区域会拦截鼠标事件。
用户看到阴影下面的按钮、文字，以为可以直接点击或悬停，但事件被 Popup 窗口吃掉。
为避免这个问题，当前窗口型 Popup 只使用很小的阴影，与 Overlay Popup 视觉不一致。

## 现状

- `src/AtomUI.Desktop.Controls/Popup/PopupToken.cs`：
  - Overlay Popup 使用 `OverlayHostShadow = BoxShadowsSecondary`（Ant Design 的大范围多层阴影）。
  - 窗口型 Popup 使用 `PopupRootShadow`：`0 1 6 0 rgba(0,0,0,.14)` 与 `0 0 4 0 rgba(0,0,0,.10)`，
    刻意收小以减少被拦截的面积。
- `Popup.ConfigureFrameShadow()` 按 `IsUsingOverlayLayer` 在两者之间切换。
- AtomUI 大部分 Popup 默认 `ShouldUseOverlayPopup=True`；窗口型 Popup 主要出现在显式关闭 overlay 的
  场景（如 InfoFlyout、PopupConfirm 的部分示例）以及 Popup 需要超出主窗口的场景。
- `AtomUI.Native` 已有整窗级鼠标穿透 `WindowExtensions.SetWindowIgnoreMouseEvents`
  （Windows：`WS_EX_LAYERED | WS_EX_TRANSPARENT`；macOS：`setIgnoresMouseEvents:`），没有按区域穿透的能力。

## 目标

1. 窗口型 Popup 使用与 Overlay Popup 相同的阴影 token，视觉一致。
2. 阴影区域不拦截鼠标：悬停、点击都落到阴影下面的控件；点击阴影区域应触发 light dismiss。
3. 打开/关闭动画（缩放、位移、透明度）中阴影与内容严格同帧，不出现错位。

## 核心思路：单窗口，只拆分“输入区域”

阴影继续与内容画在同一个窗口 surface 中，保证动画同帧；只把窗口的输入区域收缩到内容形状
（圆角矩形 + 箭头），阴影部分对输入透明。各平台都有“输入区域与可见区域分离”的机制，差别只在
能否跨进程穿透。

## 已否决：独立阴影窗口

做法：内容窗口只与 Popup 本体等大，另建一个 `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE`
的阴影窗口贴在下方，专门绘制阴影（Office、Visual Studio、WPF GlowWindow 的做法）。

否决原因是动画无法同帧：

- 两个窗口各自 Present，DWM 不保证同一帧合成，与 live resize 对向边缘错帧属于同一类问题。
- Popup 动画在 Avalonia 视觉树内完成（缩放、位移、透明度），阴影窗口需要另一套渲染并与内容窗口共享
  时钟逐帧驱动，仍会有约 1 帧偏差，缩放动画时边缘“呼吸”错位最明显。
- `DeferWindowPos` 只能原子化窗口位置，不能同步窗口内容；`SetLayeredWindowAttributes` 只能做整窗
  alpha，不能做缩放，节奏也与 Avalonia 动画时钟无关。
- 折中的“动画期间隐藏阴影、结束后淡入”会让阴影晚到一拍，正是要解决的体验问题。

## Windows

### 第一层：`WM_NCHITTEST` 按像素返回 `HTTRANSPARENT`

原理：系统派发鼠标消息前先向窗口发送 `WM_NCHITTEST`。窗口返回 `HTTRANSPARENT` 时，系统继续向下寻找
**同一线程**中的下一个窗口，并把该次鼠标事件交给它。

做法：

1. 通过公开 API `Win32Properties.AddWndProcHookCallback` 挂到窗口型 Popup 的 TopLevel 上，无需反射。
   hook 先于 Avalonia 自身的消息处理执行。Avalonia `PopupImpl.WndProc` 在 Popup 整体不可命中时也是返回
   `HTTRANSPARENT`（Avalonia 12.1.4 `src/Windows/Avalonia.Win32/PopupImpl.cs`）。
2. 收到 `WM_NCHITTEST`：`lParam` 是屏幕物理坐标，`ScreenToClient` 后除以 `RenderScaling` 得到 DIP；
   用内容元素（`ShadowsAwareContainer` 内的内容 Border）当前边界与渲染变换计算内容形状（圆角矩形 + 箭头）。
   - 点在内容形状内：不处理，交还 Avalonia（`HTCLIENT`）。
   - 点在阴影上：`handled = true`，返回 `HTTRANSPARENT`。
3. 动画期间整窗返回 `HTTRANSPARENT`，动画结束后恢复按像素命中，避免处理动画中途的变换。
   需要“动画中也可交互”时，再改为读取内容元素的实时渲染变换计算形状。

预期行为（与 Overlay Popup 一致）：

- 指针从内容区移到阴影上时，Popup 收到 `WM_MOUSELEAVE`（指针离开），下面的控件开始收到悬停；
  `Trigger=Hover` 的浮层按“移出”处理。
- 点击阴影区域：主窗口收到点击，light dismiss 正常关闭 Popup，下面的控件同时收到这次点击。
- 鼠标捕获（拖拽）期间系统不做命中测试，不受影响。

局限：`HTTRANSPARENT` 只在同一线程的窗口之间转发。主窗口与其他 Popup 都在 UI 线程上，覆盖绝大多数
场景；当 Popup 超出主窗口、阴影盖在其他进程窗口或桌面上时，那部分阴影仍由 Popup 接收输入（与现状相同）。

### 第二层（可选）：阴影位于主窗口外时动态切换整窗穿透

原理：顶层窗口同时具备 `WS_EX_LAYERED | WS_EX_TRANSPARENT` 时，系统对所有进程穿透该窗口。

做法：

1. 仅当 Popup 窗口矩形超出所属主窗口时启用，平时不走此路径。
2. 第一层判定指针位于主窗口外的阴影上时，为 Popup 加上 `WS_EX_TRANSPARENT`，事件随即交给下面的
   其他进程窗口。
3. 穿透后 Popup 收不到鼠标消息，用按帧运行的 `DispatcherTimer` + `GetCursorPos` 轮询光标；光标回到
   内容形状内时移除 `WS_EX_TRANSPARENT`。Popup 关闭时停止轮询。
   Electron `setIgnoreMouseEvents({ forward: true })` 是同一思路（Windows 上使用低级鼠标钩子）。
4. 切换 `WS_EX_TRANSPARENT` 只改变输入行为，不触发重绘，对动画无影响。

待验证风险：`WS_EX_TRANSPARENT` 依赖 `WS_EX_LAYERED`，后者应在窗口创建时设置并配合
`SetLayeredWindowAttributes(alpha=255)`。需要确认它与 `RedirectionSurface` 下 Avalonia 的旧式透明
（`DwmEnableBlurBehindWindow`）共存时不会丢失逐像素 alpha（阴影变黑边）。若不兼容，第二层放弃，
保持第一层行为。

## Linux X11

机制：XShape / XFixes 的 `ShapeInput` 只设置输入区域，不影响可见区域，跨进程生效。

Avalonia 12.1.4 现状：`X11Window.SetHitTestVisible(bool)` 已用
`XFixesSetWindowShapeRegion(..., ShapeKind.ShapeInput, ...)` 实现整窗级穿透（空区域 = 全穿透，
`None` = 恢复整窗）。注意它同时处理了 `_handle` 与 GPU 渲染子窗口 `_renderHandle`：只设置父窗口时，
渲染子窗口仍会捕获指针。

AtomUI 现有能力：`AtomUI.Native` 已提供 `WindowExtensions.SetWindowInputRectangle()` /
`ResetWindowInputRegion()`，通过 XCB SHAPE `XCB_SHAPE_SK_INPUT` 把输入区限制为一个矩形，
`X11WindowChromeManager` 正用它让 Window 自身的阴影区域点击穿透（见
[AtomUI.Native Linux 平台 API](../../modules/native/linux.md)“SHAPE input region”）。

做法：窗口型 Popup 复用同一 Native 能力，输入矩形取内容区（不含阴影），内容大小或位置变化、动画结束时更新；
动画期间可设为空区域。

待确认：

- 现有 API 只支持单矩形。箭头位于内容矩形外时需要扩展为多矩形（`xcb_shape_rectangles` 本身支持多个矩形）；
  圆角处几个像素的偏差可接受。
- GPU 渲染时 Avalonia 在 `_handle` 下有渲染子窗口 `_renderHandle`，Avalonia 自己的 `SetHitTestVisible` 会同时设置
  两者。需要确认现有 `SetWindowInputRectangle` 只设置顶层 XID 时，Popup 的渲染子窗口是否仍会捕获指针
  （Window 路径已在实机验证，Popup 需单独验证）。

## Linux Wayland

机制：`wl_surface.set_input_region` 设置 surface 的输入区域，区域外的指针与触摸事件由 compositor 交给
下面的 surface，跨客户端生效。

Avalonia 12.1.4 现状：`Avalonia.Wayland/Server/Persistent/WSurface.ApplyInputRegion()` 已按
hit-test 状态设置空区域或 `null`（协议默认的无限输入区域），同样只有整窗级开关。

AtomUI 现有能力：`AtomUI.Native/Linux/WaylandWindowReflectionExtensions.cs` 从 Avalonia 私有 proxy 取出真实
surface，由 `WaylandWindowUtils` 直接调用 NWayland 设置 input region，`WaylandWindowChromeManager` 用它让 Window
阴影穿透。该路径绕过 Avalonia worker 线程的 marshalling 与重连模型，在
[AtomUI.Native Linux 平台 API](../../modules/native/linux.md) 中明确记为技术债，不是线程安全的稳定扩展点。

做法：创建 `wl_region`，按内容形状 `add` 若干矩形后 `set_input_region`，在下一次 `commit` 生效。
输入区域是 double-buffered state，应随渲染帧一起提交，动画结束时更新一次即可。

取舍：

- 短期：窗口型 Popup（`xdg_popup` surface）复用现有反射路径，与 Window 继承同一技术债；需要先确认反射能取到
  Popup 的 surface。
- 正确方向：向 Avalonia 上游提议在 persistent surface/proxy 上提供按区域设置输入形状的接口
  （例如 `SetHitTestRegion(IReadOnlyList<Rect>)`），经 `PostWithCommit` 下发，同时可覆盖 X11，
  届时 Window 与 Popup 一起迁移。

## macOS

机制与限制：无边框透明 `NSWindow` 中 alpha 为 0 的像素会穿透点击，但阴影像素 alpha 大于 0，仍会拦截。
macOS 没有独立于可见内容的输入区域 API。

可选方案：

1. 系统阴影：`NSWindow.hasShadow = YES`，阴影由 WindowServer 绘制在窗口外，天然不拦截输入，
   并按窗口内容 alpha 生成形状，动画中自动跟随。代价是阴影样式由系统决定，不能使用 Ant Design token；
   内容变化后需要 `invalidateShadow` 刷新阴影形状。
2. 动态整窗穿透：与 Windows 第二层相同思路。指针进入阴影区域时 `setIgnoresMouseEvents:YES`
   （`AtomUI.Native` 已封装），用 `NSEvent.mouseLocation` 按帧轮询，或使用全局鼠标移动监听，
   回到内容区时恢复。可保留自绘阴影，跨进程穿透。
   待确认：全局鼠标移动监听是否需要辅助功能权限；轮询方案不需要权限。

建议 macOS 优先评估方案 2，以保持与其他平台一致的阴影视觉；方案 1 作为样式可接受时的低成本备选。

### macOS 设计（2026-10-29 用户确认采用方案 2）

否决方案 1：系统阴影不能使用 `BoxShadowsSecondary`，与本分支统一窗口型 / Overlay 阴影的目标冲突。

与 Windows 同属逐点查询：内容判定复用 `Popup.AttachShadowHitTest()` 已传入的 `isInContentShape`
（`PopupRoot.InputHitTest`），Popup 侧不新增回调。差别在于 macOS 只有整窗开关 `setIgnoresMouseEvents:`，
没有 `WM_NCHITTEST` 那样的逐次询问，因此用“指针事件进入阴影 → 整窗穿透 → 轮询光标 → 恢复”的状态切换实现。

1. 判定入口：`PopupRoot` 上以 `handledEventsToo` 监听 `PointerMoved`（实施时改为原始指针输入，见“macOS 实施结果”）。点不在内容形状内时调用
   `setIgnoresMouseEvents:YES` 并启动 16ms 的 `DispatcherTimer`（`DispatcherPriority.Input`，与 Windows 第二层相同）。
2. 轮询：读取 `[NSWindow mouseLocationOutsideOfEventStream]`。它在窗口忽略鼠标事件时仍返回实时光标，坐标为窗口
   基准坐标（原点左下），按 content view 高度翻转 Y 即为客户区 DIP（macOS `DesktopScaling` 为 1），不做屏幕坐标换算，
   不需要辅助功能权限。光标回到内容形状内或离开窗口范围时恢复 `NO` 并停止轮询；离开窗口后再次进入会重新收到指针事件。
   `NSPoint` 为两个 double，x64 与 arm64 都按寄存器返回，使用普通 `objc_msgSend`，不走 `_stret`。
3. 动效期间：`SuspendInputBeforeShow` 在窗口显示前设为 `YES`，避免窗口出现在指针下时打断下方悬停（沿用 X11 结论，
   macOS 需实机确认是否必要）；`Invalidate()` 按 `isInputSuspended` 与当前光标重算：动效中保持 `YES`，动效结束后
   光标在内容内恢复 `NO`，在阴影上保持 `YES` 并启动轮询。
4. 穿透范围：AppKit 按窗口选择鼠标事件接收者，同进程主窗口与其他程序、桌面一致生效，不分两层。点击阴影交给主窗口
   light dismiss，与 Windows 结论相同，不改变语义。
5. 释放：`Dispose()` 停止轮询、移除事件处理；窗口仍存活时恢复挂接前的 `ignoresMouseEvents`。

实现：`src/AtomUI.Native/MacOS/MacOSPopupShadowPassThrough.cs`（`IPopupShadowHitTest`），
`MacOS/WindowUtils.Interop.cs` 补充所需 selector 与 `NSPoint` 返回的 `objc_msgSend`，`NativePopupShadowHitTest` 增加 macOS 分支。
“是否穿透 / 是否轮询”的判定抽为纯函数并先写失败测试。

实机验证（本机 macOS，Gallery）：Hover 浮层阴影下按钮悬停；内容 → 阴影按移出处理；点击阴影触发 light dismiss；
Popup 超出主窗口时阴影下的其他程序收到事件；打开 / 关闭动效期间无闪关；回到内容后交互恢复。

## 动画期间的命中策略（全平台）

- 默认：Popup 打开/关闭动画期间整窗不可命中（等价于现有整窗级 `SetHitTestVisible(false)`），动画结束后
  再应用内容形状的输入区域。行为与 Overlay Popup 动画期间基本一致，且无需每帧更新输入区域。
- 动画只影响输入区域的更新时机，不影响绘制：阴影与内容始终在同一 surface，同帧上屏。

## 验证计划

沿用 live resize 验证的脚本化方法（洋红置顶背景、`ddagrab` 录制、UI Automation 定位元素）：

1. 输入穿透：打开 InfoFlyout 窗口型浮层，把光标移到阴影覆盖的按钮上，确认按钮进入悬停态；点击阴影区域，
   确认 light dismiss 关闭 Popup 且下方按钮收到点击。
2. 动画同帧：逐帧录制打开/关闭动画，检查阴影与内容边缘无错位。
3. 视觉一致：窗口型与 Overlay Popup 阴影截图对比（统一 token 后）。
4. Windows 第二层（若实施）：Popup 超出主窗口时，点击主窗口外阴影下的其他程序窗口，确认事件到达；
   同时确认 `WS_EX_LAYERED` 不影响透明阴影。
5. 平台覆盖：Windows 10/11、X11、Wayland（GNOME/KDE 各一）、macOS。

## 实施顺序

1. Windows 第一层 + 窗口型 Popup 阴影统一为 `BoxShadowsSecondary`。
2. X11 / Wayland 输入区域：复用 `AtomUI.Native` 现有的 Window 阴影穿透能力；Wayland 同时推进上游 API。
3. macOS 动态穿透方案评估。
4. 收到“阴影盖住其他程序时点不穿”的反馈后，再实施 Windows 第二层。

## Windows 实施结果（2026-10-29）

分支 `feature/windows-popup-shadow-hit-test`（基于 `experiment/windows-live-resize-layout-sync`）。正式设计见
[Popup 实现原理](../../controls/desktop/other/popup/implementation.md)“8.1 窗口型 Popup 阴影输入穿透”与
[AtomUI.Native Windows](../../modules/native/windows.md)“6. WindowsPopupShadowHitTestHook”。

实现与方案的差异：

- 内容形状不手算圆角矩形 + 箭头，直接用 `PopupRoot.InputHitTest(point, enabledElementsOnly: false)`：Avalonia 绘制列表
  命中按圆角矩形与箭头几何计算，且不包含 `BoxShadow`。
- `Popup.Host` 与 `IPopupHostProvider` 在 Avalonia 12.1.4 均为 internal，改用 `TopLevel.GetTopLevel(Child) is PopupRoot` 判定，不用反射。
- 第二层按点判定（阴影点在主窗口外才加 `WS_EX_TRANSPARENT`），等价于“仅 Popup 超出主窗口时启用”；轮询在光标回到
  内容、回到主窗口或移出 Popup 时停止。`WS_EX_LAYERED` + alpha 255 在挂接时对所有窗口型 Popup 设置，运行时不再改动。
- “点击阴影时下方控件同时收到点击”并非无条件成立：主窗口 `LightDismissOverlayLayer` 在 light-dismiss Popup 打开期间
  覆盖整个客户区，点击阴影与点击 Popup 外部任意位置相同；只有 `OverlayDismissEventPassThrough=True` 时下方控件收到点击，
  且该期间主窗口控件不进入悬停态。这与 overlay Popup 一致，未改变 light dismiss 语义。

阴影改为 `BoxShadowsSecondary` 后暴露了既有定位缺陷：`MarginToAnchor` 非零时 native 主轴偏移忽略朝向 anchor 的残余阴影
（旧阴影约 7px 被箭头抵消，未显现），内容离 anchor 多出约 40 DIP。已改为 `MarginToAnchor - 残余阴影` 并移除箭头隐藏时的
单独补偿。

验证（Windows 11，2560×1600，150%，`RedirectionSurface`；脚本在本地 `tmp/visual-verification/windows/`）：

| 脚本 | 内容 | 结果 |
|---|---|---|
| `layered-probe.ps1`、`popup-shadow-anim.ps1` | `WS_EX_LAYERED` + alpha 255 前置验证；打开/关闭动效 ddagrab 逐帧 | 无黑框、逐像素 alpha 保留，阴影与内容同帧 |
| `popup-directions.ps1` | 12 方向 × 箭头显示/隐藏/居中，窗口型 vs overlay 截图对比 | 36 组 frame/阴影/箭头/margin 全部重合，仅文字子像素差异 |
| `popup-shadow-layer1.ps1` | Hover 浮层阴影下按钮悬停；内容→阴影按移出关闭；点击 Click 浮层阴影 | 全部通过（点击穿透需 `OverlayDismissEventPassThrough=True`） |
| `popup-shadow-layer2.ps1` + `target-window.ps1` | 浮层超出主窗口 107px，独立进程 WinForms 靶子 | 靶子收到 ENTER/CLICK；回到内容后样式移除、靶子 LEAVE、点击内容不关闭；主窗口内阴影不启用第二层；切换期间内容区 204 帧无变化 |

单元测试：Popup/Flyout/ToolTip 370 项中 362 通过；`PopupMarginPlacementTests` 8 项失败在基线上同样失败
（headless 非 CSD 窗口无法创建 Popup host），与本改动无关。

Windows 10 build 19045 已复测（见 [Windows 10 实机验证快速上手](2026-10-29-windows10-resize-and-popup-shadow-quickstart.md)）：
`WS_EX_LAYERED` 兼容，第一层、第二层通过。定位时 30/36 组窗口型比 overlay 高 1 物理像素，原因待查。

未覆盖：WinUI/DirectComposition 合成模式下的 `WS_EX_LAYERED` 兼容性、多显示器混合 DPI。

## X11 实施结果（2026-10-29）

- 两个待确认项的结论：
  - 多矩形：`WindowUtilsLinux.SetInputRectangles()` 提交多矩形；内容形状逐 device pixel 行光栅化，圆角与箭头三角形按像素中心取舍，
    不再是“圆角处几个像素的偏差”。
  - GPU 渲染子窗口：X 服务器查找指针所在窗口时，父窗口输入形状外的点不会进入其子窗口，因此只设置顶层 XID 即可（与 Window 路径一致）。
- 输入形状必须在窗口映射前设置：只在 `Opened`（晚于映射）挂接时，窗口以整窗输入形状映射到指针下，X 服务器立即给下方窗口
  发 Leave，Hover 浮层随即关闭（实机录屏中表现为反复开关）。改为 Child 挂入 PopupRoot 视觉树（首次布局，早于映射）时先提交空形状。
- 平台相关实现全部位于 `AtomUI.Native`：`X11PopupShadowInputRegion`（挂接/提交/恢复）与 `PopupContentRegion`（光栅化，原名
  `X11PopupContentRegion`，Wayland 实施时改为平台共用）；
  Desktop.Controls 只通过 `ShadowsAwareContainer.GetContentShape()` 提供平台无关的 DIP 几何 `PopupContentShape`。
- Windows 路径不变：`WindowsPopupShadowHitTestHook` 只新增空的 `IPopupShadowHitTest.Invalidate()`，`TryAttach` 在 Windows 上
  传入的参数与创建的 hook 与之前相同；新增的几何回调只在 X11 上被调用。
- 验证：临时测试覆盖光栅化（缩放、圆角、四个方向箭头）与真实 X 服务器上多矩形/空区域/恢复默认形状的往返；
  临时探针应用（X11 后端、XWayland）读取 Popup 窗口实际 `ShapeInput`，结果见 Popup 实现原理第 11 节。临时测试与探针已删除。
- 未覆盖：鼠标实际穿透到下方窗口（同进程/跨进程）的交互录制、非整数缩放、GNOME/KDE 原生 X11 会话。

## Wayland 实施结果（2026-10-29）

- 方案中的“短期复用 Window 反射路径”改为不在 UI 线程直接操作协议对象：经反射取得 worker 侧 `WXdgPopup`
  （`PopupImpl._surfaceProxy._target`）与 `WaylandWorker.PostWithCommit`，区域提交在 worker 线程随合成批次执行，surface 已映射时
  按 `CanCommitOutOfBand` 额外 commit。仍依赖 Avalonia 私有成员，上游提供按区域输入 API 后迁移。
- 输入区域坐标是 surface 逻辑坐标，与 X11 共用 `PopupContentShape`/`PopupContentRegion`，按缩放 1 光栅化。
- 不需要映射前置空：`Opened` 时的首次提交与 Popup 首帧同批次，先于首个 buffer attach。
- 关键发现（GNOME Mutter）：以空输入区域映射的 xdg_popup，之后设置任何输入区域都收不到指针；映射后空区域↔内容形状切换正常。
  因此动效期间（打开动效始于映射前）使用 surface 原点 1×1 占位区域而非空区域。
- 同时修复 `ShadowsAwareContainer.GetContentShape()` 的箭头定位（X11 共用）：`ArrowIndicatorLayoutBounds` 相对模板箭头定位面板，
  底部箭头时被算进主体内部。
- 验证：GNOME Mutter 原生 Wayland 166.7% 缩放，Mutter RemoteDesktop 驱动真实指针 + `WAYLAND_DEBUG` 协议日志，结果见 Popup 实现原理
  第 11 节。临时探针已删除。
- 未覆盖：跨客户端穿透、KDE/wlroots、compositor 重连。

## macOS 实施结果（2026-10-29）

正式设计见 [Popup 实现原理](../../controls/desktop/other/popup/implementation.md)“8.1”与
[AtomUI.Native macOS](../../modules/native/macos.md)“5. MacOSPopupShadowPassThrough”。

与“macOS 设计”一节的差异：

- 进入阴影的判定由 PopupRoot 路由 `PointerMoved` 改为 `IInputManager.Process` 原始 Move。实机日志：从内容移入阴影时原始 Move
  命中为空，Avalonia 不引发路由事件，窗口始终不穿透，Hover 浮层保持打开；从窗口外移入阴影同理。
- `PopupRoot.IsHitTestVisible` 为 false 时按暂停处理，释放时恢复为 `!IsHitTestVisible`：Avalonia.Native `SetHitTestVisible`
  同样使用 `setIgnoresMouseEvents:`，记录挂接前的值会读到 `SuspendInputBeforeShow` 刚设置的 `YES`。
- `SuspendInputBeforeShow` 需要实机确认的一项已确认：调用时原生句柄已存在，挂接时窗口处于穿透。

验证（Apple Silicon，Gallery InfoFlyout 示例，CGEvent 驱动真实指针，临时日志与独立进程靶子窗口；结果见 Popup 实现原理第 11 节）：
内容 → 阴影按移出关闭；阴影下相邻按钮悬停；窗口外 → 阴影进入穿透、离开恢复；点击阴影 light dismiss；跨进程靶子收到
ENTER/MOVE/DOWN；停留锚点与内容时浮层不反复开关。临时日志、探针与靶子已删除。

未覆盖：禁用内容、多显示器、Intel x64 实机、动效中途交互的逐帧录制。

## 实现入口

- `src/AtomUI.Desktop.Controls/Popup/Popup.cs`（`ConfigureFrameShadow`）
- `src/AtomUI.Desktop.Controls/Popup/PopupToken.cs`
- `src/AtomUI.Desktop.Controls/Popup/Themes/PopupRootTheme.axaml`（`ShadowsAwareContainer`）
- `src/AtomUI.Native/WindowExtensions.cs`、`src/AtomUI.Native/Windows/`、`src/AtomUI.Native/MacOS/`
- `src/AtomUI.Native/Linux/`（`SetWindowInputRectangle`、`WaylandWindowUtils`、`WaylandWindowReflectionExtensions`）
- `src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs`、`WaylandWindowChromeManager.cs`（Window 阴影穿透的现有用法）
- Avalonia 参考：`src/Windows/Avalonia.Win32/PopupImpl.cs`、`src/Avalonia.X11/X11Window.cs`
  （`SetHitTestVisible`）、`src/Avalonia.Wayland/Server/Persistent/WSurface.cs`（`ApplyInputRegion`）
