# Motion 帧流水线

本文拥有[Motion 系统](overview.md)每帧执行的合同：时钟订阅、空帧保活、变换写入通道、按呈现时间采样、平台时间源及渲染循环节拍源。
请求、替代与终态由[播放与生命周期](playback-and-lifecycle.md)拥有；内容视口的布局失效范围由
[内容展开](../control-infrastructure/content-expansion.md)拥有；验证数据见[验证状态](verification.md)。

## 1. 帧驱动

每次播放由 `MotionFrameDriver` 直接订阅框架全局动画时钟（`IGlobalClock`，经 `Reflection/GlobalClockReflection`
从 `AvaloniaLocator` 按类型名解析），不经 Avalonia `Animation`、关键帧或进度属性。

- 本地时间从该订阅收到的首个脉冲开始，与 Avalonia 动画的首帧语义一致；完成在本地时间达到 `Delay + Duration` 的脉冲判定。
- 完成延续以 `RunContinuationsAsynchronously` 投递，不在时钟脉冲内重入业务代码。
- 外部取消、替代与 detach 统一调用 `Stop`，退订时钟并结束完成任务。
- 在[主题提交窗口](theme-commit-settle.md)内开始订阅的驱动会收到一次远端时间并在该脉冲完成，按终值采样；
  驱动不感知窗口，按普通的迟到帧处理。

## 2. 空帧保活

`MediaContext` 在一次脉冲未请求合成提交时，改由固定 16ms 的 `DispatcherTimer` 安排下一次脉冲，该计时器不对齐显示刷新。
播放进行中（延迟段、数值未变的脉冲）驱动仍调用 `Compositor.RequestCompositionBatchCommitAsync()`，使下一次脉冲继续跟随合成器节奏。
已有待处理批次时框架会把请求合并到下一批次，不产生额外提交。该行为与平台无关。

## 3. 写入通道

| 通道 | 写入方式 | 每帧成本 |
| --- | --- | --- |
| 平移、缩放、旋转 | 写目标 `CompositionVisual` 的 `Translation` / `Scale` / `RotationAngle` / `CenterPoint`（`MotionCompositionTransform`） | 不触发 `InvalidateVisual`、绘制指令重录或布局 |
| 透明度、类型化属性 | Animation 优先级的可撤销属性覆盖 | 透明度变化会重录目标自身绘制指令 |
| 布局进度 | `IMotionLayout` 租约（内容视口） | 只失效视口自身测量 |

合成通道依据：`Visual.SynchronizeCompositionProperties` 只同步 `Offset`、`Size`、`Opacity`、`TransformMatrix` 等，从不写上述四个通道。
服务端按 `TransformMatrix`（应用的 `RenderTransform`）→ 绕 `CenterPoint` 缩放、旋转 → 平移的顺序组合，因此应用变换与动效变换互不覆盖，
不需要模板变换隔离表面。命中测试读取合成回读，跟随实际呈现（回读晚一帧）。

**行为变化**：动效期间目标的 `RenderTransform` 不再被写入；读取该属性或使用 `TranslatePoint` / `TransformToVisual` 的代码只看到应用自身变换，
与 CSS transform 语义一致。需要动效当前值时读取合成通道（测试使用 `MotionTransformProbe`）。

所有权：每个 `CompositionVisual` 的每个通道同一时刻只有一个所有者。新请求挂接时直接接管它动画的通道，不先恢复；
释放时只把仍归自己的通道恢复为首个动效挂接前的基线。因此替代交接连续，旧请求独有的通道回到基线，同一 visual 上的其他合成使用者
（例如 Spin 的旋转）保留自己的值。

## 4. 按呈现时间采样

平台提供呈现时间时，驱动按“当前内容预计上屏的时间”采样，而不是脉冲时间。契约 `IMotionPresentationClock`
（`MotionFrameTiming.Clock`）提供：

| 成员 | 含义 |
| --- | --- |
| `PredictPresentation(now)` | 此刻采样的内容最早能到达的上屏时间（Stopwatch 时钟） |
| `RefreshInterval` | 固定刷新周期；未知或可变刷新为 0 |
| `FramesStarted` | 已开始绘制的帧数；之后开始的帧携带此前提交的内容 |
| `FramesInFlight` | 已绘制或等待合成器、尚未提交的帧数 |

规则：

- 目标上屏时间只有在上次提交之后确有新帧开始绘制时才推进一个刷新；渲染器空闲不视为已消费（脉冲可能多于帧）。
- 推进前的重复脉冲解析到同一目标，不写入属性与布局，消除过密脉冲造成的重复布局。
- 首次推进时，空闲渲染器已连画的额外帧以一次起点顺延吸收（至多 2 个刷新），不用后续跳帧追赶，完成时间不漂移。
- 采样时间单调；平台无预测时使用脉冲时间，行为与无此能力时一致。

## 5. 平台时间源与受益范围

| 平台 | 时间源 | 状态 |
| --- | --- | --- |
| Linux Wayland | `wp_presentation` feedback，见 [Native Linux](../../../modules/native/linux.md) | 已实现，Desktop 在安装帧调度适配器时接线 |
| Windows | `DwmGetCompositionTimingInfo` | 未实现，退回脉冲时间 |
| macOS | CVDisplayLink `inOutputTime` | 未实现，退回脉冲时间 |
| X11 | 无 | 退回脉冲时间 |

帧驱动、空帧保活、合成通道和内容视口失效收窄与平台无关，Windows、macOS 同样适用；呈现时间采样目前只在 Wayland 生效。
Windows、macOS 的渲染循环按显示刷新出帧（见第 6 节），本身不存在 Wayland 的过密脉冲问题。

## 6. 渲染循环节拍源

合成动画（Spin、Skeleton 闪光带、Splash 不确定进度）和合成通道上的 Motion 由渲染循环逐帧出帧，节拍等于渲染循环计时器的节拍。
第 5 节的时间源决定“按哪个时刻采样”，本节的节拍源决定“多久出一帧”，两者独立。

下表依据 Avalonia 12.1.5 源码。只有 Windows 做过上屏实测，其余平台没有实测数据。

| 平台 | 节拍源 | 跟随显示刷新 | AtomUI 处理 |
| --- | --- | --- | --- |
| Windows，`RedirectionSurface`（AtomUI 默认首选） | `SleepLoopRenderTimer`（线程睡眠） | 否 | 换成 `DwmVsyncRenderTimer`，见 [Windows live resize 的渲染节拍](../windowing/windows-live-resize.md#渲染节拍) |
| Windows，其余合成模式 | 系统合成器或 DXGI 垂直同步 | 是 | 无 |
| macOS | CVDisplayLink（`PlatformRenderTimer.mm`）；Metal 在渲染线程经 `CAMetalLayer` 呈现 | 是 | 无 |
| Linux Wayland | `WaylandWorker` 自带的渲染循环，按合成器事件唤醒；合成器不要帧时以 20 fps 保底 | 是 | 第 4 节的呈现时间采样 |
| Linux X11 / XWayland | `SleepLoopRenderTimer`，目标帧率取 RandR 报告的最高刷新率，不低于 60 | 否 | 无 |

### X11 的差距（未处理）

X11 与 Windows `RedirectionSurface` 使用同一个睡眠计时器，根因相同。`UseAtomUIPlatformDetect()` 在存在 `WAYLAND_DISPLAY`
时选择原生 Wayland，X11 路径只出现在 X11 会话或显式选择 X11 后端时。

- 目标帧率跟随 RandR，不固定 60；多显示器时取所有显示器中的最高值。
- 节拍不锁垂直同步相位。GLX 路径只调用 `glXSwapBuffers`，Avalonia 不设置交换间隔，是否等待垂直同步取决于驱动默认值和用户配置。
- 软件渲染回退用 `XPutImage` 加 `XSync` 上屏，没有垂直同步节流。同一计时器在 Windows 计时精度 1 ms 下实测 61.5～61.8 fps、
  3.4%～4.2% 的帧跳过一帧内容（见上面链接的实测表）。X11 软件渲染预计接近这一行，未测。
- Linux 线程等待不受 Windows 约 15.6 ms 默认计时精度的限制，预计不会出现 Windows 上 19%～27% 的跳帧，未测。

处理前必须先实测，分 X11 GLX、X11 软件渲染、XWayland 三种情况，沿用 Windows 的变帧率录屏方法；默认 GLX 路径确实跳帧再做。
`RenderLoopReflection.TryReplaceTimer` 可以直接复用。X11 没有等价于 `DwmFlush()` 的统一接口，候选节拍源
（`GLX_OML_sync_control`、`GLX_SGI_video_sync`、Present 扩展的 `PresentNotifyMSC`）都依赖驱动或 X 服务器实现，需要逐个验证。

### macOS 的差距（未处理）

- CVDisplayLink 由 `CVDisplayLinkCreateWithActiveCGDisplays` 创建，之后没有调用 `CVDisplayLinkSetCurrentCGDisplay`。
  窗口位于刷新率与主屏不同的副屏时，预计仍按主屏节拍出帧。问题在 Avalonia 原生库内，属于上游；没有在混合刷新率环境验证。
- 回调不向托管侧传递 `inOutputTime`。第 5 节的 macOS 呈现时间采样要实现，需要 AtomUI 自建显示链路。
