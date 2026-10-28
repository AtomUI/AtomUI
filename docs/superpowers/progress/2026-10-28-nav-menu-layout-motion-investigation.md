# NavMenu 布局动画卡顿问题与调查记录

日期：2026-10-28。状态：未解决，已取得后端对照数据；尚未通过因果干预确认各项成本对掉帧的贡献。

Gallery 的侧栏展开、收缩和内联子菜单开合存在明显卡顿。本机 Debug 对照中，Wayland 绘制阶段间隔中位数约 33ms，
X11/XWayland 约 16ms；Wayland 同时出现远多于实际绘制次数的布局通知。
源码显示，两个后端对渲染循环的唤醒和节流不同，UI 动画又以合成批次 `Processed` 作为继续推进的条件，
因此同一份控件代码可以产生截然不同的布局次数。共享布局失效传播会进一步放大这些更新的工作量。

## 问题范围与复现条件

- AtomUI 分支：`feature/motion`；调查基线：`79af86f4e26c2ec334a6fc1cc0e142326ecb27fa`。
- 运行配置：Debug，NuGet Avalonia `12.1.4`；本机 Linux，分别使用 Wayland 和 X11/XWayland。
- 两后端 `RenderScaling` 均为 `1.6666666666666667`。Wayland 客户区为 `1300.2 × 900 DIP`，
  X11 为 `1300.8 × 900 DIP`，存在窗口像素取整造成的 `0.6 DIP` 宽度差异。
- 保持 Gallery Overview 页面、侧栏主题和动画开启；重复切换侧栏收缩状态、打开和关闭内联子菜单。
- 脚本通过公开 `IsInlineCollapsed`、`INavMenuItem.IsSubMenuOpen` 属性驱动真实控件路径。
  此方法覆盖动画执行，不代替真实鼠标输入与视觉验收。

用户同时报告三层子菜单卡顿、子菜单背景和文字闪烁、Modal mask 动画卡顿，并观察到 Overlay Popup 与
Ant Design 网页更流畅。这些是关联问题；本轮量化数据覆盖 Gallery 侧栏和内联子菜单，未完成三层子菜单、
Modal 和闪烁问题的独立验证。

期望在本机 60Hz 显示条件下开合连续平滑，并保留真实布局、文字排版、裁剪、命中、滚动和子菜单状态恢复。
用户提出的 120FPS 目标尚无本轮实测证据。

## 同缩放后端对照

每个场景使用独立进程，预热一次后执行三次开关循环，每个动作观察 650ms。
次数与累计耗时取三次动作的中位数；绘制间隔只在单次动作内计算相邻时间差，再合并同方向样本求 P50/P95。

| 动作 | 后端 | 布局通知数 | 绘制次数 | 绘制间隔 P50 | 绘制间隔 P95 | 布局累计耗时 |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 侧栏展开 | Wayland | 125 | 8 | 33.81ms | 40.56ms | 42.53ms |
| 侧栏展开 | X11 | 18 | 16 | 16.59ms | 19.74ms | 35.21ms |
| 侧栏收起 | Wayland | 24 | 6 | 33.16ms | 50.76ms | 17.51ms |
| 侧栏收起 | X11 | 14 | 14 | 16.31ms | 22.68ms | 15.66ms |
| 子菜单展开 | Wayland | 137 | 7 | 32.91ms | 48.15ms | 11.22ms |
| 子菜单展开 | X11 | 14 | 14 | 16.11ms | 22.29ms | 4.02ms |
| 子菜单关闭 | Wayland | 142 | 7 | 33.58ms | 41.61ms | 8.84ms |
| 子菜单关闭 | X11 | 14 | 15 | 16.56ms | 24.62ms | 3.69ms |

“布局通知数”指根窗口 `LayoutUpdated` 次数，同时采集 `LayoutPassTimed` 和 measure/arrange 指标交叉观察，
不等于所有控件 `MeasureOverride` 调用数。“绘制次数”来自 `avalonia.comp.render.time`，
间隔为该绘制阶段结束时间戳之差，不是显示器 present 间隔。X11 的 P95 仍超过 16.67ms，不能据此宣称稳定 60FPS 验收通过。

最初的“子菜单展开 129 次布局、8 次绘制”来自另一 Wayland 进程，其每动作布局累计耗时中位数约 10.07ms，
绘制阶段累计约 81.05ms，后者不包含完整 GPU flush/swap/present。
这表明布局与绘制数量严重失配，但不能把 129 次布局全部视为昂贵的完整重排，或直接换算成丢失的帧数。

Wayland 三个侧栏进程的展开布局中位数分别为 130、240、125，而绘制间隔均约 33ms。
这些次数并非固定常量；布局次数减少也不自动意味着绘制节奏改善。

## 为什么相同代码会产生不同布局次数

### UI 动画跟随批次处理反馈继续推进

Avalonia 的 `MediaContext.CommitCompositor` 等待 `batch.Processed`，完成后在还有活跃动画时调度下一轮 UI render pass。
该 pass 推进动画时钟，并执行布局和提交。`ServerCompositor.ExecuteGlobalPasses` 在调用窗口目标的实际 `Render` 之前
发出 `Processed`。因此，“批次已处理”只代表变化已被合成器接收处理，不代表该画面已经绘制或上屏。

相关源码：[MediaContext.Compositor](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Base/Media/MediaContext.Compositor.cs)、
[MediaContext](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Base/Media/MediaContext.cs)、
[ServerCompositor](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Base/Rendering/Composition/Server/ServerCompositor.cs)。

### 后端对唤醒采用不同的节流方式

X11 初始化使用 `RenderLoop.FromTimer`，默认后台路径采用 `SleepLoopRenderTimer`。
渲染循环已运行时，`Wakeup` 只标记待处理工作，由后续 timer tick 执行；定时器按目标刷新频率等待。
因此提交一个批次通常不会额外触发一次立即执行的渲染循环。
见 [X11Platform](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.X11/X11Platform.cs)、
[DefaultRenderLoop](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Base/Rendering/RenderLoop.cs)、
[SleepLoopRenderTimer](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Base/Rendering/SleepLoopRenderTimer.cs)。

Wayland 的 `EnqueueBatch → Wakeup → DoTick` 可以在新批次到达后唤醒处理。
但 `WSurface.State` 在上一帧的 frame callback 仍 pending 时不返回 Ready，
`ServerCompositionTarget.Render` 因而可以跳过实际绘制。此时批次仍可完成 `Processed`，解除 UI 动画的等待。
见 [WaylandWorker.RenderTimer](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Wayland/Server/WaylandWorker.RenderTimer.cs)、
[WSurface](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Wayland/Server/Persistent/WSurface.cs)、
[ServerCompositionTarget](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Base/Rendering/Composition/Server/ServerCompositionTarget.cs)。

由此存在如下循环：

```text
UI 动画推进 → 属性变化 → 布局与提交 → 唤醒 Wayland 合成处理
     ↑                                    ↓
     └──────────── Processed 完成 ──────────┘

实际绘制另受窗口 Ready / frame callback 约束。
上述循环可能在两次实际绘制之间执行多轮。
```

动画按经过的时间计算进度，并不预先限定必须采样多少次。同一份代码被调用得越频繁，就可能产生越多微小进度变化，
继而触发布局。异步线程调度、每轮工作耗时和回调到达时机也会影响一次动画内能完成多少轮，
所以同一 Wayland 后端的不同进程也可能出现 130 与 240 这样的计数差异。
这些因素对本轮进程间波动的具体贡献尚未分离。

以上调用关系是源码事实；其作为本轮布局放大的解释与后端对照一致。
尚未通过单变量修改调度并复测，证明它贡献了多少冗余布局、以及是否足以解释约 33ms 的绘制间隔。
这属于当前 Avalonia 后端实现与上层动画的配合问题，不能概括为 Wayland 协议天然只能运行 30FPS。

### AtomUI 会放大每次动画更新的失效范围

[ContentViewport](../../../src/AtomUI.Controls/Primitives/ContentViewport.cs) 的 `LayoutLease.SetProgress` 在进度值变化时调用
`InvalidateMotionLayout`：使自身 measure 失效，并逐级使所有可视祖先的 arrange 失效。
Avalonia 的 [Layoutable](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Base/Layout/Layoutable.cs)
又在这两种失效路径中调用 `InvalidateVisual`，因此影响可能继续传播至 UI 绘制指令重录和合成内容失效。
当前没有有效的逐帧 dirty-region 面积数据，不将其描述为已测得全窗口重绘。

[MotionPlayback](../../../src/AtomUI.Core/Animations/Motion/Playback/MotionPlayback.cs) 用普通 Avalonia `Animation`
推进进度；[MotionPresentation](../../../src/AtomUI.Core/Animations/Motion/Playback/MotionPresentation.cs) 在 UI 线程应用布局及视觉属性。
[NavMenu](../../../src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs) 的根宽度动画则直接运行 Avalonia `Animation`，
不经过 `MotionPlayback`。其宽度还经 [GalleryShellView](../../../src/AtomUI.Toolkits.GalleryBase/Shell/GalleryShellView.cs)
绑定到侧栏容器，改变 `Auto,*` 布局下右侧内容的约束。
因此问题同时涉及平台调度、共享布局基础设施及 NavMenu 消费路径，不能只归为某个插值函数的问题。

## 补充实验及解释边界

- 纯框架最小窗口：`600 × 400 DIP`、两个 Border、`Grid Auto,*`，无 AtomUI 控件和 Motion 播放器。
  使用与产品相同的 Wayland 平台初始化时，一秒原生 Width 动画约 80 次属性更新、80 次 measure pass、60 次绘制，
  绘制间隔 P50 为 16.66ms。真实宽度布局在轻负载下可以达到约 60Hz 绘制节奏。
- 同一最小窗口的 UI Opacity 动画约 119 次属性更新、61 次绘制；Composition Opacity 约 62 次绘制、无 UI 属性逐帧更新。
  合成渲染器与在合成线程求值的动画后端是不同概念；产品已使用前者。
- 完全原始的 `UseWayland` 初始化还暴露另一 UI 时钟问题；产品已有
  [TryShareManagedClock](../../../src/AtomUI.Core/Reflection/DispatcherReflectionExtensions.cs) 兼容处理，
  不能用未适配初始化的结果解释当前 Gallery 的全部现象。
- 固定 Overview 页面宽度后仍约 33–37ms；替换为空白页面的两个独立进程分别约 33ms 和 17–18ms。
  页面负载有贡献的可能性仍在，但空白替换没有得到稳定收益，不能据此锁定右侧页面为唯一原因。
- 单独开启 ActivityListener 的定位运行中，展开期间所有 NavMenu 后代 TextBlock 合计实际 Measure/Arrange 各 1026 次，
  ContentViewport 合计 Measure 1052 次。计数跨多个实例；该探针开销较大，其时间数据不参与后端速度比较。
- 原有 NavMenu Popup 诊断场景未配置 `OpenMotion/CloseMotion`，仅发生静态开关，已排除其数据，
  避免用它证明用户所述 Overlay Popup 动画流畅。

本次尚不能区分 GPU 提交与呈现等待、窗口绘制就绪时机、UI 工作及合成绘制成本各自对长帧的贡献。
`Processed` 和 `Rendered` 批次完成通知均未作为上屏 FPS 指标。

## 版本与证据保留

运行依赖 `12.1.4` 对应上游提交 `2d79ba55fae1b1e71c69f93a668080c9029e6a31`。
另按本地优先规则检查 Avalonia 最新主线 `da1dafcdfbd94250c061d86125230fcb9be5538d`。
上述 MediaContext、ServerCompositor、Wayland 渲染循环、DefaultRenderLoop 和 SleepLoopRenderTimer
在这两个提交间未变化；X11Platform 有其他功能变动，但上述定时器与渲染循环初始化路径未变。
最新主线仅做源码对照，未编译运行。

本地原始 JSON、汇总与原始报告保留于 `.artifacts/performance/motion-investigation-20261028/layout-research/`，
该目录为本地忽略产物，不随此文档进入版本控制。关键结果已直接记录在本文中。
表格来自 `sidebar-wayland-final.json`、`sidebar-x11-matched.json`、`submenu-base-2.json`、`submenu-x11-matched.json`；
精确汇总位于 `summary.json`。

本轮诊断使用临时工程，已删除诊断源码、可执行文件及专用日志，诊断进程已退出；未新增永久测试，未运行全量回归。
此前性能实验已按用户要求撤回；基线提交 `79af86f4e` 只保留展开前占位初始化与准备失败清理的正确性修复，
不代表此性能问题已解决。本次仅新增问题记录。
