# NavMenu 布局动画卡顿问题与调查记录

> 阅读说明（2026-10-29）：下文按时间保留调查过程，早期“尚未正式接入”等状态仅对应当时阶段。
> 当前已保留默认启用的 Native Wayland 适配器，但稳定60FPS仍未验收；Overlay mask 的完整现状见
> [专项问题记录](2026-10-29-overlay-dialog-mask-performance-investigation.md)。

日期：2026-10-28。状态：临时调度原型将侧栏提高到约 59FPS，三层子菜单长轮次复测约 57FPS；尚未落入正式代码，未完成稳定 60FPS 验收。

Gallery 的侧栏展开、收缩和内联子菜单开合存在明显卡顿。本机 Debug 对照中，Wayland 绘制阶段间隔中位数约 33ms，
X11/XWayland 约 16ms；Wayland 同时出现远多于实际绘制次数的布局通知。
源码显示，两个后端对渲染循环的唤醒和节流不同，UI 动画又以合成批次 `Processed` 作为继续推进的条件，
因此同一份控件代码可以产生截然不同的布局次数。共享布局失效传播会进一步放大这些更新的工作量。
后续通过临时修改 Avalonia 依赖副本，验证了“等待回调期间准备下一帧、回调到达后提交”的收益；
下文“帧调度因果实验与上屏验证”记录了原型、失败候选及实际呈现指标。

## 问题范围与复现条件

- AtomUI 分支：`feature/motion`；调查基线：`79af86f4e26c2ec334a6fc1cc0e142326ecb27fa`。
- 运行配置：Debug，NuGet Avalonia `12.1.4`；本机 Linux，分别使用 Wayland 和 X11/XWayland。
- 两后端 `RenderScaling` 均为 `1.6666666666666667`。Wayland 客户区为 `1300.2 × 900 DIP`，
  X11 为 `1300.8 × 900 DIP`，存在窗口像素取整造成的 `0.6 DIP` 宽度差异。
- 保持 Gallery Overview 页面、侧栏主题和动画开启；重复切换侧栏收缩状态、打开和关闭内联子菜单。
- 脚本通过公开 `IsInlineCollapsed`、`INavMenuItem.IsSubMenuOpen` 属性驱动真实控件路径。
  此方法覆盖动画执行，不代替真实鼠标输入与视觉验收。

用户同时报告三层子菜单卡顿、子菜单背景和文字闪烁、Modal mask 动画卡顿，并观察到 Overlay Popup 与
Ant Design 网页更流畅。这些是关联问题；初轮量化数据覆盖 Gallery 侧栏和内联子菜单。
后续增加了三层内联子菜单的最深层开关测试；Modal 和闪烁问题仍未完成独立验证。

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
初轮未通过单变量修改调度并复测；后续干预结果见下文，已区分 UI 过密采样和窗口绘制门控的影响。
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

初轮尚不能区分 GPU 提交与呈现等待、窗口绘制就绪时机、UI 工作及合成绘制成本各自对长帧的贡献。
`Processed` 和 `Rendered` 批次完成通知均未作为上屏 FPS 指标。

## 帧调度因果实验与上屏验证

补充实验的 AtomUI 基线为 `a1f63a29f`。仍使用真实 Gallery、Debug、原窗口尺寸、缩放、主题和内容。
渲染线程确认实际采用 Intel Arc（MTL）GPU、Mesa `26.0.8-1ubuntu0.3`、Wayland EGL WSI，
不是软件渲染。合成器 presentation feedback 报告刷新周期 `16,666,300ns`，约 60Hz。

所有候选只修改临时进程加载的 Avalonia DLL 副本。未修改 NuGet 缓存、参考源码、AtomUI 控件或产品初始化。
原型通过临时 IL 插桩验证调度关系，不是可发布的依赖补丁。

### 失败候选和绘制门控定位

- 只限制 `MediaContext` 动画 pulse 最小间隔为 8ms：侧栏展开的布局通知中位数从 98 降到 16，
  绘制间隔中位数仍为 36.94ms。减少布局采样本身没有解决约 30FPS 的节奏。
- 只启用 dmabuf swapchain：展开绘制间隔中位数仍为 34.46ms。没有证据支持切换这个选项即可解决问题。
- 跟踪 WSI 调用：`eglSwapBuffers` 本身中位数约 0.026ms；展开时，从 swap 返回到 frame callback
  约 16.04ms，callback 到下次 attach 约 19.28ms。这些中位数来自独立分段，不能简单相加当作精确单帧总耗时。
  观察到的调用顺序是先等待回调，再准备下一帧，从而容易错过下一次刷新。
- 临时取消 `WSurface.State` 对 pending callback 的绘制阻挡：不改菜单代码，展开绘制间隔中位数降到 16.22ms。
  恢复原依赖后重新回到约 33ms。此候选没有限制在途帧和回调数量，不能直接发布。
- 改为最多提前绘制一帧、只保留一个 frame callback，并合并过密的 UI pulse：绘制节奏明显改善，
  但 presentation feedback 仍发现部分更新被 `discarded`。提前画完后立即提交仍可能让尚未显示的更新被替换。
  该候选的侧栏展开上屏间隔均值为 18.68ms、收起为 23.02ms，仍不足以验收。

这组实验把两件事分开了：`Processed` 反馈驱动的过密采样产生冗余 UI 工作；
pending callback 阻止提前准备下一帧，则是本机真实 Gallery 约 30FPS 节奏的重要原因。
不能只在 AtomUI Motion 中减少属性设置次数，也不能用移除 Wayland 节流的方式直接交付。

### 有效原型：提前准备，按回调提交

原型将一个窗口的状态拆为“等待回调”和“已有一帧待提交”。
等待上一帧 callback 时允许准备一个后备缓冲区；绘制指令 flush 后恢复 GL 上下文，暂缓 swap。
callback 到达后，恢复该缓冲区的上下文，紧邻 swap 执行原有 `OnBeforeNewBufferAttached`，
再提交已经准备的帧。已有待提交缓冲区时禁止继续覆盖它。
因此每窗口最多只有一个待提交帧和一个未完成的 frame callback。

```text
原路径：提交 → 等待回调 → 准备下一帧 → 提交
原型：  提交 → 等待回调期间准备下一帧 → 回调到达后提交
```

同时保留 8ms 的 UI pulse 合并，仅作为本机诊断变量；正式实现应跟随输出刷新节奏与动画时间，不能固定成全平台限帧值。
本原型涉及 `WSurface.State/FrameListener`、WSI EGL session 的准备与提交边界、`MediaContext` 的采样调度，
不改变 NavMenu 的真实宽度布局或文字排版。

通过 Wayland `wp_presentation` 为每次提交请求 feedback，记录 `presented` 时间、刷新序号和 `discarded`。
以下是合成器报告的呈现结果，强于绘制结束时间，但不是高速相机的物理像素观测。
每个动作仍观察 650ms；只计算单个动作内部相邻呈现的间隔，不跨越动作间空闲期。
FPS 为这些间隔的数量除以总时长；P95 使用 nearest-rank，区别于初轮绘制表的向下取整算法。

| 场景 | 版本 | 每方向次数 | 上屏间隔均值 | 区间有效 FPS | 下一次刷新即显示 | 丢弃更新数 |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 侧栏展开 | 原版 | 5 | 33.33ms | 30.00 | 1/33 | 0 |
| 侧栏收起 | 原版 | 5 | 33.33ms | 30.00 | 0/32 | 0 |
| 侧栏展开 | 延迟提交原型，复测 | 20 | 16.80ms | 59.54 | 257/259 | 0 |
| 侧栏收起 | 延迟提交原型，复测 | 20 | 16.93ms | 59.06 | 249/252 | 0 |
| 三层子菜单最深层展开 | 延迟提交原型 | 5 | 16.67ms | 60.00 | 63/63 | 0 |
| 三层子菜单最深层关闭 | 延迟提交原型 | 5 | 16.67ms | 60.00 | 63/63 | 0 |
| 三层子菜单最深层展开 | 延迟提交原型，复测 | 20 | 17.60ms | 56.83 | 221/233 | 0 |
| 三层子菜单最深层关闭 | 延迟提交原型，复测 | 20 | 17.56ms | 56.94 | 229/242 | 0 |
| 侧栏展开 | 恢复原版 | 5 | 37.36ms | 26.77 | 0/29 | 0 |
| 侧栏收起 | 恢复原版 | 5 | 33.85ms | 29.54 | 3/32 | 0 |
| 三层子菜单最深层展开 | 恢复原版 | 5 | 35.48ms | 28.18 | 0/31 | 0 |
| 三层子菜单最深层关闭 | 恢复原版 | 5 | 36.21ms | 27.62 | 0/29 | 0 |

20 轮侧栏复测的两个方向上屏 P95 均约 16.68ms；仍出现少量跨两次或三次刷新的长帧，不能表述为完全不掉帧。
首次 5 轮原型测试的侧栏展开、收起分别约 56.36、58.06FPS，也纳入原始报告，不能只保留更好的复测。
三层子菜单 20 轮复测的 P95 仍约 33.33ms，约 5% 的间隔跨越多次刷新；不能用首次 5 轮的 60FPS
替代长轮次结果。恢复原依赖后再次回到约 27–30FPS，支持调度干预带来收益的判断，
但不同进程间仍有波动，不能将剩余长帧未经测量就全部归咎于控件布局、GPU 或后台负载。

三层场景使用 Gallery 的 “Inline menu” 真实控件，在诊断进程内为现有最深叶节点补两个子项，
形成三个可展开层级。先逐层展开祖先，再反复开关最深层；每次校验中间动画值、最终状态及祖先仍展开。
这不覆盖祖先快速反向切换、侧栏折叠后状态恢复、鼠标命中、滚动及文字背景闪烁的视觉验收。

### 正式修复边界

已验证的方向是修改 Avalonia Wayland 的绘制与提交流水线，并合并没有显示机会的 UI 动画采样。
产品已启用 GPU 合成；再增加一个“启用合成”开关不能替代这项修改。
正式依赖补丁需要处理缓冲区与 GPU 同步、resize/configure 的原子提交、窗口隐藏或销毁、
连接中断、多个窗口及不同刷新率；临时原型没有完成这些生命周期处理和 NativeAOT 验证。
AtomUI 的布局失效范围仍有优化空间，但本次从约 30FPS 提升到约 57–59FPS 的主要改善没有依赖改写 NavMenu。
120Hz、Modal mask 以及其他硬件/合成器均未测试。

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

补充实验的原始 JSON、绘制汇总、呈现汇总和依赖哈希保留于
`.artifacts/performance/motion-investigation-20261028/frame-pacing-experiment/`，同样为本地忽略产物。
呈现数据对应文件名中的 `present`；`deferred` 表示提前准备并延迟提交，`combined` 表示仅允许提前绘制并合并 pulse。

本轮诊断使用临时工程，已删除诊断源码、可执行文件及专用日志，诊断进程已退出；未新增永久测试，未运行全量回归。
此前性能实验已按用户要求撤回；基线提交 `79af86f4e` 只保留展开前占位初始化与准备失败清理的正确性修复，
不代表此性能问题已解决。截至该轮原型调查结束，仅更新调查记录；当时有效调度原型尚未进入产品代码。

## Native 与反射接入方案

后续方案见 [Wayland 帧准备与提交适配层设计](../specs/2026-10-28-wayland-native-frame-pacing-design.md)。
拟由 Desktop 层包装 Skia render interface、探测已知内部成员，Native 层在 Wayland worker 上实现
静态 EGL session 和 callback relay。先独立验证提前准备/延迟提交，再判断是否需要动画采样合并。

本轮补充探测使用实际依赖：NWayland `0.11.0` 的 `IWlEventsListener` 可由静态 C# relay 实现；
`WlProxy._listener` 是可写实例字段。CoreCLR 中反射创建 Avalonia `ClockBase`、绑定 `Pulse` 并订阅，
两次相隔 16.6667ms 的输入得到 `0, 16.6667ms`。该结果仅证明接入形态，未验证真实 callback 替换、
完整动画、NativeAOT 或反射适配器的呈现性能。第一阶段方案不包含自定义动画 clock。

该轮接入探测工程位于系统临时目录，验证后清理；当时没有新增永久测试或产品源码改动。


## 第一阶段 Native 适配器实现与验收（2026-10-29）

用户授权“开始实现”后，第一阶段代码已进入工作区，仍未提交。初轮验证时内部开关
`AtomUI.Experimental.WaylandFramePacing` 默认关闭，测试宿主在 AppBuilder 初始化前显式设为 true；
随后用户要求改为兼容 Wayland 默认启用，见本文末尾策略变更记录。下列测量仍为初轮显式开关对照结果。
通过 Desktop 的精确版本/成员校验，包装 render interface，并由 Native 静态 EGL session 在等待上一帧
callback 时准备一个缓冲区、callback 清理后提交。没有修改 NuGet 缓存、依赖 DLL、Motion clock、NavMenu 布局或动画参数。

状态由 persistent WSurface 持有，跨 EGL context/target 更换复用；断开时清理准备帧。原 target 保持 EGL/native
资源所有权。提交前比较连接、surface、configure serial、scale 和 shadow extents，失效时退役 target 并重绘。
EGL swap 的布尔返回值不再丢失，context loss 通知框架恢复；提交失败清理本次新建 callback，退回原渲染路径并记录错误。

### 实际呈现结果

本机 Debug、原生 Wayland、60Hz 输出、缩放 5/3，Gallery 窗口为 1300×900 DIP。
测量使用工作区 Gallery 应用及原依赖，临时宿主只注入开关、动作和 `wp_presentation` 观测；
每方向 20 次，动作观察窗 650ms，动作之间空闲 80ms。只统计同一动作内相邻 presented 时间，保留所有长帧，
不跨空闲段拼接。FPS 为间隔数/间隔总时长，P95 为 nearest-rank；单刷新间隔以 <17.2ms 计数。
反馈在实际提交前申请，不能把准备帧或布局次数当成呈现帧。

三层场景保留完整 Menu 页面，在进程内为 Inline menu 的最深叶节点补两个子项，形成三个可展开层级；
逐层打开祖先，反复切换最深层，校验最终状态和祖先仍展开。本轮完整页面的原版数据比此前 DLL 原型场景更慢，
仅使用本轮同场景开关对照，不能直接拼接此前数据。

| 场景 | 原路径 FPS | 适配器首次独立 20 轮 FPS | 适配器最终侧栏复测 FPS | 关闭开关复测 FPS | 适配器 P95 | 单刷新间隔 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 侧栏展开 | 29.32 | 57.45 | 58.41 | 39.92 | 16.675ms | 464/477 |
| 侧栏收起 | 30.73 | 58.76 | 59.02 | 30.46 | 16.675ms | 238/241 |
| 最深层展开 | 14.38 | 57.71 | — | 26.19 | 16.676ms | 242/252 |
| 最深层关闭 | 15.21 | 58.49 | — | 25.51 | 16.675ms | 264/271 |

表中侧栏 P95/单刷新比例取最终侧栏复测，子菜单取唯一独立 20 轮适配器测试；所有这些运行 discarded 为 0。
侧栏最终复测仍有最大约 50ms 的间隔，子菜单最大约 33.34ms。首次 5 轮适配器试跑与构建负载重叠，
只有约 43–46FPS，保留原始数据并标记干扰，不能删掉或当成独立对照。
独立测量没有并行构建；整个 Gallery 页面的呈现都计入观察窗，没有按看起来较好的帧挑选。

关闭开关的两组复测均为每方向 20 次，原路径 P95 约 50ms，收益方向可复现；但关闭开关后的 FPS 与最初基线差异明显，
说明跨进程负载/页面呈现状态仍有波动，不能把全部变化精确归因于适配器或宣称固定倍数提升。

结果支持当前调度适配器带来明显收益，但每个场景同时满足 ≥59FPS 与 ≥99% 单刷新间隔的门槛没有达到。
**不能宣称稳定 60FPS，更没有 120Hz 证据。** 第一阶段保持实验状态；第二阶段动画采样合并未实现，
剩余漏帧原因仍需按 UI 采样、布局、render commit 和实际呈现的关联时间继续定位。

### 正确性、恢复和 AOT

- 临时测试 11 项通过：单准备槽、提交顺序、旧 owner 取消、失效帧、重复回调、释放后回调、错误回退、
  已知 ABI 和无关 surface 转发、EGL 错误。真实原路径 FPS 断言失败；EGL context-loss 分类曾以错误异常类型失败，修正后通过。
- 临时帧对象模拟的 configure/owner/disconnect 测试只能验证状态机，不能单独证明真实协议生命周期。
- 真实 Gallery 在 callback 已创建、延迟提交尚未完成时注入异常：只报告一次，后续所有动作继续呈现，未卡死。
- 真实 Gallery 主动使 EGL context 失效：重建目标并继续动画。状态 owner 调整前后的运行都恢复成功，
  不能把该调整描述成已经复现的用户故障修复。
- 真实 Gallery 快速反向、resize、hide/show、原生及 Overlay Popup、双窗口、Modal mask 开关均通过脚本状态断言，
  本轮累计 7 个适配 surface、0 个错误。最初临时 Popup 缺少 AtomUI 类型/逻辑 owner，修正测试宿主后通过；未改 Popup 产品代码。
- 聚焦的 linux-x64 NativeAOT 宿主实际运行 `adapter targets=2, draws=106, errors=0`（最终含启用日志的复测），覆盖 resize、hide/show。
  初次发布的 IL2080 通过已知 Type 的局部数据流修正，无扩大 metadata 保留或隐藏裁剪告警。该证据不是完整 Gallery AOT 或 net8 AOT。
- 单次独立源码审查未发现 Critical/Important 缺陷；补上成功启用的诊断日志。该次审查以当时默认关闭的实验路径为范围。

框架 `RenderTargetBitmap` 截取了展开、收起中、收起后三张图并检查，能看见宽度状态变化，但正文存在裁剪/排版重叠差异；
这条离屏路径不能替代真实窗口视觉验收。当前 native UI automation 不可用，未完成屏幕录像、文字/背景闪烁、鼠标命中和滚动验收。
Modal mask 这里只验证打开/关闭与宿主状态，没有测得 mask 动画达到 60FPS。
真实 compositor 断开重连、输出缩放/刷新率切换、其他 GPU/合成器与 120Hz 也未验证。

原始呈现 JSON、逐场景汇总、恢复日志和 AOT 结果保留在本地忽略目录
`.artifacts/performance/wayland-native-pacing-20261028/`。11 项临时测试及两个临时宿主的源码、可执行产物均已在最终验证前删除，进程已退出；
保留红绿、恢复和呈现结果，无新增永久测试。它们属于本次实验和私有 ABI 的专项检查，
未证明每次全局运行的独立必要性，因此未转成常驻测试。最终聚焦验证结果如下。


### 清理后最终验证

- `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Window.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Dialog.'`：497 通过、4 失败，共 501；没有运行全量回归。
- 4 个失败为 `OverlayDialogOwnerResizeTests` 的 3 个参数化/普通案例，以及
  `OverlayDialogPresenterTests.Modal_With_Stretch_Steps_Keeps_Its_Natural_Size_In_Different_Owner_Widths`。
  独立运行仍失败；另从 `a1f63a29f` 导出无本次改动的源码副本，构建并执行相同 4 项，全部以相同 Y 偏差失败：
  预期 325/410/530，实得 316/401/521。属于本机基线已存在的 Headless Dialog 位置问题，未在本次扩大修复范围。
  对照副本已删除，日志保留；因此最终模块回归不能表述为全部通过。
- `dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --no-restore`：0 warning、0 error，包含 Native 依赖。
- 最终含启用日志的 linux-x64 NativeAOT 重新发布并执行：2 个适配 target、106 次 draw、0 error；发布日志无 warning/error。
- 当前工作区 `git diff --check`、本次文档本地链接检查通过；没有临时测试、临时诊断进程或宿主源码残留，未提交/推送。

这些构建与正确性检查不改变性能验收结论：稳定 60FPS、120Hz 及真实视觉/输入验收仍未完成。

## Windows 动画性能对照测试清单（2026-10-29）

状态：待执行。本节记录双方讨论后的测试点与方法，尚无 Windows 实测结果，也没有现成的 Windows 自动操作脚本。
目标是使用相同场景区分共享 Motion/布局路径与平台绘制、呈现调度的影响。当前新增适配器只作用于 Wayland，
Windows 先使用正常默认后端；不能把 Wayland 开关当成 Windows 上的优化变量。

### 环境与执行口径

- 记录提交号、未提交差异或源码摘要、Avalonia/.NET 版本、Windows 版本、CPU、GPU、驱动与工具版本。
  仅记录分支名不能证明两台机器使用同一份代码；当前 Native 实验实现尚未提交。
- 先测 Debug 且不附加调试器，再单独测 Release；需要发布验收时另测 Windows NativeAOT。不同配置不混合统计。
- 窗口统一为 1300×900 DIP，记录实际 ClientSize、RenderScaling、输出刷新率和实际渲染/合成后端。
  先固定 60Hz 和一个缩放比例，再补 100%/150%/200%、真实 120Hz；主题、数据量、页面和电源模式保持一致。
- 使用本机桌面，记录多显示器、VRR 等显示条件；远程桌面结果不能直接与本机输出比较。
  测量关闭 DevTools，不并行构建；轻量帧率采集、录屏、详细调用追踪分别运行。
- 首次打开与预热后重复操作分组；若要评价首次加载，至少采集 10 个独立进程。预热后每方向至少 20 次，
  建议主要场景重复三个独立进程，保留每轮结果。预热次数须在比较双方固定并记录。
- 每动作观察 650ms，之后空闲 80ms；动画本身超过 650ms 时预先统一延长观察窗。脚本在 UI 线程操作真实控件，
  记录场景、轮次、方向、触发时间、动画完成时间及最终状态，不通过移除页面内容降低负载。

### 测试场景

| 测试点 | 操作 | 观察与断言 |
| --- | --- | --- |
| Gallery 侧栏 | 展开/收起，分别固定 Overview 和 Menu 页面 | 宽度连续性、右侧重新布局、背景/文字闪烁 |
| 一级、二级子菜单 | 分别展开/关闭，各方向单独统计 | 高度连续性、其他菜单位移、最终高度 |
| 三层子菜单 | 三个实际可展开层级，展开祖先后反复开关最深层 | 最深层帧率、祖先保持展开、裁剪与状态恢复 |
| Modal Dialog＋mask | 固定内容、尺寸，反复打开/关闭 | mask 渐变与 Dialog 动画分别观察，关闭后恢复输入 |
| Overlay Popup | 明确配置并启用 OpenMotion/CloseMotion 后反复开关 | 动画、定位、阴影、裁剪及命中 |
| 原生 Popup | 与 Overlay Popup 保持相同内容和动画参数 | 独立窗口与主窗口的呈现差异 |
| 快速反向 | 开始后约 30–50ms 反向切换，连续重复 | 跳变、闪白、停住、最终宽高；祖先切换及侧栏折叠恢复 |
| 生命周期/DPI | 动画中 resize、最小化/恢复、隐藏/显示、关闭、切换页面、跨 DPI 显示器 | 黑帧、残影、位置错误、恢复后的帧率和输入 |
| 多窗口/长期重复 | 两窗口同时动画，重复开关至少 100 次 | 相互拖慢、CPU/内存/资源数量持续增长 |

三层场景不能用“两层可展开子菜单＋第三层叶节点”代替。前轮是在 Inline menu 的最深叶节点下临时补两个子项，
形成三个可展开层级；Windows 对照应复用相同结构。Popup 必须确认存在动画中间态，不能只断言 IsOpen。
前轮未配置动画的静态 Popup 开关数据已经排除，不作为流畅性基线。

### 分层采集与统计

| 层级 | 数据 | 作用 |
| --- | --- | --- |
| 动画/属性 | 进度、Width/Height/Opacity 更新次数与时间 | 识别过密采样与跳变 |
| 布局 | 根窗口 LayoutUpdated 次数、Measure/Arrange 次数和累计耗时 | 区分布局放大与实际成本 |
| 绘制 | compositor update/render 次数、耗时及间隔 | 判断更新是否进入实际绘制 |
| 呈现 | 显示帧间隔、P50/P95/P99、最大间隔、单刷新占比、未显示帧数 | 判断实际呈现节奏 |
| 资源 | CPU/GPU、分配量、GC、重复操作后的内存趋势 | 定位长帧及生命周期问题 |

当前依赖的观测入口包括 `avalonia.ui.measure.time`、`avalonia.ui.arrange.time`、`avalonia.ui.render.time`、
`avalonia.comp.update.time`、`avalonia.comp.render.time`；记录原始单位并统一换算。LayoutUpdated 不是所有控件的
Measure 调用总数，绘制完成也不是上屏。不得直接从“129 次布局、8 次绘制”换算丢帧数。

按动作、方向、窗口/呈现流分别统计；不相加多个窗口的 FPS，不跨动作间空闲计算间隔，不删除长帧。
有效 FPS = 1000 × 呈现间隔数量 ÷ 间隔总毫秒数，分位数沿用 nearest-rank。
另记触发到首次可见变化的延迟、有效更新帧数、完成时间，避免启动停顿后只有少量连续帧却被判断为流畅。
动作时间标记与呈现记录须对齐时钟；无法分辨其他页面更新时明确注明整窗口统计范围。

### Windows 呈现采集

先用 PresentMon 按实际 Gallery PID 输出 CSV。核对可执行路径，确保目标是当前工作区构建产物。
以下以下载后命名为 `PresentMon.exe` 为例，12345 替换成实际 PID：

```powershell
.\PresentMon.exe --process_id 12345 --qpc_time `
  --output_file windows-navmenu.csv `
  --timed 60 --terminate_after_timed
```

该命令只负责采集，不自动操作 Gallery。保留未显示帧，不使用 `--exclude_dropped` 或 `--no_track_display`。
按所用版本的 display/presentation 字段分析，不以 CPU FPS 或 Present 调用频率替代；多窗口、原生 Popup
分别识别其呈现流。显示数据缺失或无法关联目标窗口时标记“未测得”。参数和字段说明见
[PresentMon 官方文档](https://github.com/GameTechDev/PresentMon/blob/main/README-ConsoleApplication.md)。

发现持续 33ms/50ms 或异常长帧后，另开诊断运行采集 ETW。GPUView 官方流程是在工具目录的管理员命令行中
运行 `Log.cmd`，复现约 30–60 秒，再运行一次停止并生成 `Merged.etl`；检查 CPU 调度、GPU 工作与呈现等待。
见 [Microsoft GPUView 文档](https://learn.microsoft.com/en-us/windows-hardware/drivers/display/using-gpuview)。
保存采集工具版本和必要证据；详细 trace 的耗时不直接混入轻量性能基线。

### 验收与结果记录

- 60Hz 沿用各场景有效 FPS ≥59、P95 接近单刷新周期、≥99% 间隔在单刷新周期及事先声明的计时容差内。
  容差基于实际刷新周期与时间戳精度，不能为通过测试而调整；同时报告最大长帧、未显示帧和各轮波动。
- 120Hz 必须在真实输出上另测，单帧预算约 8.33ms；按相同单刷新标准评价，60Hz 结果不能代替 120Hz 验收。
- 背景/文字闪烁、裁剪错误、鼠标命中错误、滚动异常、祖先状态丢失或最终尺寸错误，任一项出现即正确性不通过。
- 真实窗口录屏与人工复核用于视觉验收；RenderTargetBitmap 离屏图不能替代。60FPS 录像不能证明 120Hz 连续性。
- 优先执行侧栏、三层子菜单、Modal＋mask、带动画的 Overlay Popup，再补其他矩阵；所有结论注明已测/未测。

每份结果保留：场景、方向、轮次、构建配置、缩放、刷新率、布局次数/耗时、绘制次数、呈现 FPS、
P95/P99/最大间隔、单刷新比例、未显示帧数、首帧延迟、视觉问题及原始 CSV/录像路径。
临时测试/宿主按仓库生命周期规范清理，只保留必要证据，不能把临时脚本描述成已有正式工具。

若 Windows 同样出现密集布局而绘制/呈现稀疏，继续关联共享采样、布局和提交时间线；若 Windows 平稳而 Wayland
异常，优先检查平台调度差异。这些只是下一步定位方向，不能仅凭不同硬件上的绝对 FPS 或体感判定责任。

## Wayland 默认启用策略变更（2026-10-29）

用户明确要求“在 wayland 平台适配器默认打开”。按此要求调整 Desktop 初始化入口：

- 未设置 `AtomUI.Experimental.WaylandFramePacing` 或显式 true：在已验证的 Wayland EGL 后端默认安装适配器。
- 在 AppBuilder 初始化前显式设置 false：保留原路径，供诊断对照。
- 版本/成员签名校验、失败日志及运行时提交失败回退保持生效；不扩大到其他平台或渲染 surface。

该策略调整不代表 mask 渐变或稳定 60/120FPS 已通过验收。此前 Modal 只验证开关、生命周期及恢复，
没有采集 mask 渐变的实际呈现帧率；该场景的性能验证仍待执行。

临时真实 Wayland 启动探针在修改前验证“未设置开关应安装适配器”失败，实际仍是原始 Skia render interface；
修改后未设置、显式 true 都安装 `WaylandFramePacingRenderInterface`，显式 false 保留原始接口，三个独立进程均通过。
探针构建 0 warning、0 error；其源码、产物与进程已清理，未新增永久测试。红绿与启动日志保留在
`.artifacts/performance/wayland-native-pacing-20261028/default-policy-*.log`。

清理后使用 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~WindowingPlatformDetectionTests` 聚焦验证：29 通过、0 失败，未运行全量回归。测试编译仍有既存 NavMenu 的两处 xUnit1051 警告。文档链接和 `git diff --check` 通过；未提交/推送。


## Overlay Dialog＋mask 专项调查（2026-10-29，缓存候选未通过验收）

**当前仍未修复，稳定 60FPS 验收未完成。** 本专项的完整测量、像素失败、测试处置和交接边界已集中到
[Overlay Dialog mask 动画卡顿问题记录](2026-10-29-overlay-dialog-mask-performance-investigation.md)。
该记录完整保留本节原有专项证据，并补充当前状态、复现入口、原始文件索引和未完成验收。

最新无缓存 UI mask、适配器开启的独立 20 轮结果为打开 59.35FPS、关闭 59.76FPS，
仍有约 50/33ms 长帧。初始四路对照中的同路径结果为 41.77/47.50FPS；统计窗口与进程阶段不同，
不能只取较高结果证明修复，也不能把缓存候选的全部变化归因于缓存。

BitmapCache 候选因原生文字、圆角及小数 bounds 差异已撤回；临时 composition mask 没有给出稳定收益，
且时钟/teardown 不等价，未进入产品。已有 Native 适配器及默认启用策略保留。

### 用户收紧实现边界（2026-10-29）

用户明确禁止自行编译或维护 Avalonia 包，该要求覆盖此前源码原型许可。
临时源码和宿主构建在运行前停止并清理，未实现 renderer retention，未产生新的性能结果。
工作区依赖、DLL、NuGet 缓存未替换；后续仅在 AtomUI 和现有 Avalonia 包公开 API 范围内推进，可只读参考源码。
相关方案现为[已撤回的设计记录](../specs/2026-10-29-overlay-dialog-background-retention-design.md#withdrawn-renderer-source-experiment-retain-the-composed-backdrop)，不能当作实施授权。
本轮未 commit/push，未重置工作区；完整证据与剩余验收见上述专项记录。
