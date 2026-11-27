# Avalonia 12.1.5：真实源码、动效时钟与性能复核

日期：2026-11-05。此记录补充[完整升级评估](../specs/2026-11-05-avalonia-12.1.5-upgrade-design.md)和[Linux 验收](2026-11-05-linux-avalonia-1215-acceptance.md)。

## 判定

源码影响与时钟接入已有直接依据；本轮不支持“升级后全面无性能回退”或“已实现逐帧精准同步”的结论。普通长词 Wrap 和长数字 W2 处理有明确收益，同时 WrapWithOverflow 超长词的布局语义和成本发生实质变化。Linux 选定 Gallery 动效的重复采样工作量波动大；已发现并纠正临时产物混用问题，CPU 数据不能用来归因菜单合成开销，GPU/NativeAOT 稳态与多输出仍未覆盖。

本轮没有修改产品源码或依赖。已有菜单修复保留原状；仅更新评估、计划和验收文字。没有创建 commit。

## 精确源码范围

- 比较 `2d79ba55fae1b1e71c69f93a668080c9029e6a31`（12.1.4）→ `5825bf8c66fba0c11e67c980d8ece6444576bffa`（12.1.5），参考仓库 `/atomware/workspace/projects/ReferenceProjects/Avalonia`；使用 `git show <tag>:<path>` / `git diff 12.1.4..12.1.5`，不以参考仓库当前 checkout 或 release notes 代替版本源码。
- 重新取得 339 个 diff 条目：托管/平台产品源码 131、native 2、测试 188、示例 11、构建/CI 7；生产 diff 共 5464 行。逐文件处置清单沿用完整升级评估附录，本次补充 E22，将两个 FreeDesktop IME 文件的真实行为从 E03 分类中单独展开。清单分类不能替代逐行为影响说明。
- 逐字节核对以下关键实现两个 tag 相同：MediaContext.cs / MediaContext.Clock.cs / MediaContext.Compositor.cs、Composition/Compositor.cs / ServerCompositor.cs、WaylandWorker.RenderTimer.cs、WaylandWorker.cs、WaylandConnection.cs、Wayland EGL render targets、Skia DrawingContextImpl.cs。Animation 目录唯一生产变化是 IterationCount 字符串比较。它们没有直接修改，不代表调用方 Dispatcher/Window/Popup 变化不会间接影响调度与成本。

## 三个时间域及因果关系

| 时间域 | 实际来源、单位 | 在 AtomUI 中的作用 |
| --- | --- | --- |
| Dispatcher | backend.Now + clockOffset，整数毫秒 | DispatcherTimer / Background / 16ms 空闲动效唤醒的截止值 |
| GlobalClock | MediaContext 自有 Stopwatch.Elapsed，TimeSpan | RenderCore 触发 Pulse；动画相对进度，未直接改为 Dispatcher.Now |
| Wayland 呈现 | wp_presentation 的 CLOCK_MONOTONIC，Linux Stopwatch 纳秒时间戳 | prepared-frame 的预计显示时间；仅固定刷新、时钟匹配时启用预测 |

12.1.5 的直接生产变化：[ReplaceImplementation](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Base/Threading/Dispatcher.cs#L116) 在解绑旧后端前保存旧 Now，安装后设置 `offset = oldNow - newImpl.Now`；[Now / UpdateOSTimer](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Base/Threading/Dispatcher.Timers.cs#L22) 返回 `newImpl.Now + offset`，向 OS 提交 `dueTime - offset`。因此逻辑时间连续，原截止值不必重写，过期 timer 可立即到期。旧版本固定捕获的 Stopwatch 与新后端 Stopwatch 起点不同，却没有转换截止值。

原 AtomUI TryShareManagedClock 通过反射将 ManagedDispatcherImpl._clock 替换为 Dispatcher 闭包中的 Stopwatch，使两个时间原点相同。新版已删除 `_timeProvider` 和该闭包，保留旧 hook 没有可绑定对象；删除是框架等价修复后的正确收敛。实际已发布的 12.1.4 基线带此 hook，不能拿未修补的旧框架延迟当作升级的速度收益。

[RenderCore](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Base/Media/MediaContext.cs#L136) 仍以自身 `_time.Elapsed` pulse GlobalClock；上一批 Composition 未处理完时暂缓 pulse，没有有效 commit 时依旧回退到 16ms DispatcherTimer。[批次节流](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Base/Media/MediaContext.Compositor.cs#L21) 仍在 Processed 后重新调度 UI；[Wayland starvation timer](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Wayland/Server/WaylandWorker.RenderTimer.cs#L25) 仍保留 20Hz 饥饿兜底。新版并未把这些路径改成新的 vblank 同步机制。

AtomUI MotionFrameDriver 的源代码本次升级未变：先收到 GlobalClock 相对时间，再用 `Stopwatch.GetElapsedTime(now, predictedPresentation)` 添加预计呈现提前量；以 FramesStarted 判断上一目标是否已被取走，避免重复消费；开始阶段最多吸收两个 refresh 的陈旧帧。不同时间域通过差值转换，没有直接相减不同原点，也没有把纳秒当 TimeSpan ticks。修复 Dispatcher 不替代 prepared-frame adapter，不应连带删除其帧回调、输入区域或初始缩放逻辑。

### 有正对照的定时器实验

临时外部探针在各自精确版本程序集上运行真实 ManagedDispatcherImpl.MainLoop：先创建 Dispatcher/150ms timer，等待 250ms，再反射调用框架自己的 ReplaceImplementation，启动 16ms timer 取 8 次。旧版 hook 通过原 AtomUI 方法执行；全部进程使用同一个 .NET 10.0.14。反射和等待仅存在于临时诊断工程。

| 版本/条件 | 16ms timer 稳态间隔 | 预排 timer 实际触发（进程相对时间） |
| --- | --- | --- |
| 12.1.4，故意不加旧 hook（故障正对照） | 均值 280.10ms，最大 280.73ms | 437.42ms |
| 12.1.4，原产品 hook 返回 true | 均值 15.94ms，最大 16.02ms | 291.16ms |
| 12.1.5，不加 hook | 均值 15.93ms，最大 16.01ms | 285.33ms |

原始失败对照证明检验能识别错误起点；新版与带修补的旧产品均恢复约 16ms，不能把均值的小差别解释为性能提升。结合上一轮真实 Wayland 初始化前后 timer 实验与 NativeAOT TryInstall/OnPresented 命中，支持本机时间轴接入正确。不是 timer 16ms = 显示 60FPS 的证明。

## 其他与性能相关的真实生产变化

| 源码变化 | 成本/行为变化及 AtomUI 影响 |
| --- | --- |
| [Bidi W2](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Base/Media/TextFormatting/Unicode/BiDiAlgorithm.cs#L929) | 从对每个 EuropeanNumber 反向查找强类型，改为一次前向扫描维护 precedingStrongClass；该规则的最坏扫描工作下降，输入/多语言长数字文本可受益；不把局部规则复杂度推广到整个 TextLayout。 |
| [断行 lookahead](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Base/Media/TextFormatting/TextFormatterImpl.cs#L877) | 限制当前可容纳长度之外 4096 字符的搜索；Wrap 避免反复扫描全部剩余长词；WrapWithOverflow 超长词会被拆行，不能视为保持输出相同的纯提速。 |
| [ShapeTogether buffer 释放](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Base/Media/TextFormatting/TextFormatterImpl.cs#L548) | split 两半各自保留池存储引用后，释放不再使用的源 buffer，避免 glyph 存储最后不能归还池。改善池复用条件；本轮没有据此宣称进程内存无限增长已被修复或确定节省某个百分比。 |
| [Skia Bounds / stroke cache](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Skia/Avalonia.Skia/GeometryImpl.cs#L41) | UI 侧先访问路径 Bounds，避免共享到 render thread 后惰性缓存竞态；先成功创建新 stroke 再释放旧缓存。主要是线程/异常安全，预计算也把部分成本移到构建侧，不等同整体更快。影响 Icon、裁剪和阴影几何。 |
| WindowBase.ArrangeCore / LayoutManager.ExecuteArrangePass | 原生约束使 measure 失效时先重测，可能多一个必要布局步骤；移到别的 root 的控件不再安排进原 root。Popup/Window resize 的 CPU 和通知次数可能变；正确性优先，不能保证 layout 次数总减少。 |
| X11PlatformThreading.RunLoop | 新增显式 Background processing；输入/分组事件和 epoll 等待顺序变化，影响 X11 后台任务饥饿、响应性及调度成本，不是 Wayland render loop 改版。 |
| AssemblyDescriptorResolver / FallbackStorageProvider | ConcurrentDictionary 与锁/Task 缓存改变并发资源/文件选择创建成本；解决竞争并减少重复 factory，不能推广成每帧渲染收益。 |
| Classes/TypeNameAndClassSelector/IterationCount 等字符串比较 | 改字符前缀或 ordinal 比较，语义更明确；没有相应页面级性能测量，不列作整体动效提速证据。 |
| Wayland PopupImpl 等 6 文件 | 原始 configure Geometry 保留可空状态；UI 侧加入父/子 Popup geometry origin 和位置通知，新增父事件订阅有 Dispose 解绑。跨根坐标、阴影原点、定位和动效 pivot 受影响；额外计算/事件是正确定位成本，frame callback/交换缓冲契约并未被替代。 |
| [IBus keycode](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.FreeDesktop/DBusIme/IBus/IBusX11TextInputMethod.cs#L170) / [退出 await](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.FreeDesktop/DBusIme/DBusTextInputMethodBase.cs#L221) | X11 keycode 减 8 转 evdev；Dispose 在等待前清状态、ConfigureAwait(false) 使关停后的错误可见。补全原报告未单独展开的行为；真实 IBus 输入仍未验收。 |

### 同环境文本微测量

Release 外部探针；同一个 CLR 10.0.14、DejaVu Sans 14、maxWidth=180 DIP、每种输入预热 3 次后测 12 次，TextLayout 构造并 Dispose。表内为第一对进程各 12 个样本的中位数；另以反向 B/A 顺序重复独立进程。它不是 Gallery 导航/页面加载基准；未跑 10 次独立冷启动，不主张页面整体提速。

| 输入/模式 | 12.1.4 | 12.1.5 | 输出检查与结论 |
| --- | ---: | ---: | --- |
| ascii 1000 字符 / Wrap | 1.613ms | 1.615ms | 50→50 行；宽高相同，小输入参考值，不外推整体收益 |
| ascii 1000 字符 / WrapWithOverflow | 0.202ms | 0.198ms | 1→1 行；宽高相同，小输入参考值，不外推整体收益 |
| rtl-digits 1000 字符 / NoWrap | 0.467ms | 0.131ms | 1→1 行；宽高相同，小输入参考值，不外推整体收益 |
| ascii 24000 字符 / Wrap | 729.960ms | 235.181ms | 1200→1200 行；宽高相同，局部耗时减少 67.8% |
| ascii 24000 字符 / WrapWithOverflow | 4.455ms | 214.549ms | 1→996 行；高度 16.3→16231.7 DIP，行为改变，不能作相同输出的性能比较 |
| rtl-digits 24000 字符 / NoWrap | 192.538ms | 2.906ms | 1→1 行；宽高相同，局部耗时减少 98.5% |

第二对进程复核 24000 字符：Wrap 为 739.867→232.259ms，RTL 长数字为 194.067→2.910ms；WrapWithOverflow 为 4.423→213.152ms。所有对应宽高、行数与第一对一致，两个顺序均支持局部源码收益及超长词语义变化。第二对新版本加载的菜单主题不同，但 TextLayout 探针不创建菜单，Avalonia/Core 程序集一致；此数据仅用于文本原语。

WrapWithOverflow 24000 字符由一行变成 996 行，宽度从 205898.4 变成 35174.3 DIP、高度从 16.3 变成 16231.7 DIP。测量耗时 4.455→214.549ms，主要因为输出和布局工作变多，不能称为相同任务退化 48 倍或性能优化；应用需要接受这种超长词断行语义。上游 TextFormatterLongRunWrapTests 明确要求该新行为；依据仍是对应生产 diff。全仓 src/controlgallery 没有显式 WrapWithOverflow，公共 TextWrapping 属性仍允许应用侧设置。

## 真实 Wayland 动效对照方法

- A：`031135f11` / Avalonia 12.1.4，包含原时钟 hook；P：`00197a821` / Avalonia 12.1.5 原始提交态；C：P 加当前工作树的三个菜单整体透明度主题修复。Release 产物在系统临时副本构建，主仓源码不变。
- **产物混用及纠正**：独立复核发现原标 B1/B2 的工具后续重编译时引用了已更新为 C 的目录。三次运行的实际 Desktop DLL SHA256 都为 `b45917b8d7c611afa631777232d36995333831af58632a2d1c1928b90237e6a2`，因此它们只能归入 C，原 B/C 差异归因已撤销。随后从 HEAD 恢复三个主题，重新构建 P，复制到独立只读使用的引用目录，工具输出也独立存放。P Desktop SHA256 为 `e65c75895a8d57fe0c26f7bd2ec1426f761c48308b8dada9e97b17fc8a480532`；运行时另读实际加载程序集的 hash 验证，而非只信构建标签。P/C 的 Core SHA256 同为 `6452d059050a1c56c3e0db4b1d53848c3eb49cdc906cb821b1d919ee975c0392`、Avalonia.Base 同为 `2398bdbfbdb7eb0a100d8395f625efb78a447ead98a92e3f8a8270acf6a13756`。
- 同一 .NET 10.0.14、TieredCompilation=0、GNOME Wayland / 5/3 缩放、约 60Hz、1300.2×900 DIP、Light、相同视觉数量（sidebar/submenu 408、popup 302），电源配置只读取不修改。各批采样期间均无并发 build；补测前先完成独立产物重建。保持 CLR 版本相同用于隔离 Avalonia/适配差异；不覆盖 .NET 10.0.11→10.0.14 或 NativeAOT 稳态性能差异。
- 复用实际 Gallery NavMenu 的 sidebar、inline submenu、popup 三种场景；每进程每场景预热 3 轮、测 12 轮，每轮双向操作，各观察至少 400ms。只通过公开控件 API触发；测量时输入/失焦使整进程样本作废，不更改原生交互行为。
- 既有工具在 AfterCommit 读 `_pendingBatch` 有竞态：Processed 的同步 continuation 能在 AfterCommit 前清空它，两个上游版本的相关源码相同。临时工具只取消依赖该不可靠观察的失败门槛及 HasIntermediateValues 的 Rendered 数量条件，保留中间属性值/终态/激活/输入验证；原始 missed 数仍保留为诊断，Rendered/Processed 子集完全不用于帧率或性能判定。产品和仓库工具未改。
- 仅用只读已有属性、layout hook、Process CPU、GC 分配、wp_presentation 原计数器；没有新增 GlobalClock 订阅、Timer 或强制 commit 来制造工作。计数器反射与观察者有成本，各版本方法相同。Popup 的 layout hook 只覆盖主窗，不能声称覆盖原生 PopupRoot 的所有布局成本。
- 原定顺序 A1/B1/C1/C2/B2/A2 中，C2 被 RawPointerEventArgs 打断，整组 AcceptedSamples=0，未重试。按真实加载 hash 重分类后为 A1/C-early1/C-early2/作废/C-late/A2，总 360 个有效操作。为补齐缺失的原始提交态，另测 P1/P2；它们是纠正实验条件的新采样，不是重试被输入打断的进程。

### CPU 结果：每次操作的约 400ms 观察窗

Cα/Cβ/Cγ 分别为原标签 B1/C1/B2；它们加载相同的菜单修复产物。列按有效运行时间顺序排列，不能视为负载稳定的配对。P1/P2 均各 72 个有效操作，实测加载的 Desktop/Core hash 与独立引用目录一致；合计 A 144、C 216、P 144，共 **504 个有效操作**。

| 场景 | A1 | Cα | Cβ | Cγ | A2 | P1 | P2 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sidebar | 306.33ms | 304.94ms | 304.46ms | 205.19ms | 202.26ms | 201.48ms | 193.20ms |
| submenu | 262.43ms | 256.42ms | 257.80ms | 254.48ms | 257.12ms | 239.74ms | 237.72ms |
| popup | 249.62ms | 252.63ms | 253.91ms | 232.06ms | 235.07ms | 226.27ms | 228.59ms |

重复期间 sidebar A1→A2 自身从 306.33 降到 202.26ms，UI 属性变化/布局次数与分配也改变；中途发生指针事件，初始 hover 状态和桌面负载未完全锁定，不能证明跨这一时段的工作量条件完全一致。不能合并求一个总体提升百分比，也不能把早晚运行差异归因为版本或菜单修复；正常样本均有中间动效值与正确终态，实际操作没有被降级成无动效。

原 B/C 分配比较已撤销：原标 B1 的 sidebar 18825KiB、submenu 9496KiB 与原标 C1 的 20088/10775KiB 来自**相同菜单修复产物**，已经说明进程间噪声，不能作为修复额外开销的证据。静态源码确认低于全不透明时整体合成需要离屏层，GPU 缓冲和 GPU 时间本轮未测；不能宣称菜单修复没有代价。

### 呈现预测与同步边界

| 场景 | A1 | Cα | Cβ | Cγ | A2 | P1 | P2 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sidebar | 31.8% | 46.3% | 40.4% | 59.6% | 36.7% | 38.0% | 37.4% |
| submenu | 25.1% | 32.0% | 47.5% | 50.4% | 67.6% | 56.2% | 61.7% |
| popup | 30.3% | 55.5% | 40.7% | 26.5% | 41.3% | 67.5% | 53.7% |

这些是现有 WaylandPresentationTiming.OnPresented 对实际 wp_presentation feedback 的统计，命中定义为预测与实际时间相差不超过 refresh/4（本机约 4.17ms）。命中率波动明显，不能用于建立版本因果；只对收到 OnPresented 的反馈计数，discarded 帧不在该分母，不能算掉帧率。它不是 FPS，也不证明显示异常。它说明“时钟接入/真实呈现链在工作”与“逐帧预测准确性达标”是两件事；后者本轮未闭环。

MotionFrameTiming.Clock 为全局接入，预测仅针对固定刷新/CLOCK_MONOTONIC，并非新的逐输出时钟同步设计。多屏不同刷新率、VRR、输出迁移/休眠恢复、GPU 饱和、实际原生输入延迟及 NativeAOT 稳态均不能从本机单屏 CLR 采样推出已通过。UI 值变化与 layout/commit 可多于显示帧，不拿它们换算 FPS。

## 决策与剩余门禁

1. 保留 12.1.5 的 Dispatcher 原生时间轴转换，删除旧反射 hook 的依据成立；保留独立的 Wayland prepared-frame、初始缩放和阴影输入适配。
2. 将超长词 WrapWithOverflow 列为应用侧可见兼容性变化，保留原生 IME E22、平台输入/多输出未验收事项。
3. 本机功能与 NativeAOT 启动/呈现通过沿用上一轮证据；本轮仅补充源码与 CLR 性能研究，不把已有 25 项基线失败删去，也没有重跑全量。
4. 全面无性能回退、菜单分组合成开销和逐帧精准同步仍不能判为通过。需要固定初始输入/hover 与负载条件的独立重复、完整呈现时间序列和 GPU/AOT 稳态测量后再关闭这些门禁；本轮不通过修改产品时序掩盖测量波动。

## 复核、验证与清理

- 独立源码/证据复核发现并要求修正上述 P2 级临时产物混用。已撤销原 B/C 比较，按实际加载 hash 重分类；缺失的原始升级态以独立引用/输出重新构建并测量两轮，构建端与运行时双重核对。其余 Dispatcher、文本、IME、布局与几何源码结论经复核与原始数值一致。
- 12.1.4、12.1.5 原提交态 Release Gallery 构建均为 0 warning/error，纠正后的原提交态重建亦为 0 warning/error。工具只存在于系统临时目录，本轮未新增常驻测试，未改变产品时序/交互或依赖；已有菜单修复的生产主题 diff 摘要与上一轮验收一致。
- `python3 scripts/verification/test.py audit` 退出 0；`git diff --check` 通过。临时基线/升级源码副本、引用 DLL、外部探针、原始性能/文本/时钟 JSON 和构建日志均已删除；本轮原生采样进程均已结束。未触碰用户原有五份未跟踪计划。
