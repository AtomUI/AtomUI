# Motion 帧流水线

本文拥有[Motion 系统](overview.md)每帧执行的合同：时钟订阅、空帧保活、变换写入通道、按呈现时间采样及平台时间源。
请求、替代与终态由[播放与生命周期](playback-and-lifecycle.md)拥有；内容视口的布局失效范围由
[内容展开](../control-infrastructure/content-expansion.md)拥有；验证数据见[验证状态](verification.md)。

## 1. 帧驱动

每次播放由 `MotionFrameDriver` 直接订阅框架全局动画时钟（`IGlobalClock`，经 `Reflection/GlobalClockReflection`
从 `AvaloniaLocator` 按类型名解析），不经 Avalonia `Animation`、关键帧或进度属性。

- 本地时间从该订阅收到的首个脉冲开始，与 Avalonia 动画的首帧语义一致；完成在本地时间达到 `Delay + Duration` 的脉冲判定。
- 完成延续以 `RunContinuationsAsynchronously` 投递，不在时钟脉冲内重入业务代码。
- 外部取消、替代与 detach 统一调用 `Stop`，退订时钟并结束完成任务。

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
Windows、macOS 渲染循环由系统合成器或 CVDisplayLink 驱动，本身不存在 Wayland 的过密脉冲问题。
