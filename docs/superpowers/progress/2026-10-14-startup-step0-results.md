# 启动重构步骤 0 结果：基线与原型验证

日期：2026-10-14。状态：已完成（基线、S1–S5 结论齐全；Windows/Linux 复核待用户执行）。

## 1. 基线

### 1.1 方法

- 机器：macOS 26.6.2，arm64，10 核；SDK 10.0.300。
- 基线身份：6.2.5 = `f0e9b4133`；HEAD = `60dd72afb9192c7593faba1528f5c95f491fe851`（采样时实际值）。
- Gallery 探针：`controlgallery/AtomUIGallery.Desktop/StartupProbe.cs`（两 worktree 字节一致），
  SHA-256 `793db1312f20965680598e9727c42a9d1022cca24dd71a046c5f90894de446a0`。
- 入口适配：两侧 `Main` 结构一致，同一最小改动（`StartupProbe.MarkMain` → `AfterSetup(Attach)` → `Report`）直接适用，
  无版本差异。
- harness 说明：`tools/performances/AtomUI.StartupPerformance` 在 6.2.5 中不存在（10-12 分期工作引入），
  基线侧是**新引入同一份 harness** 而非沿用。已核对 6.2.5 具备 harness 依赖的全部 API；两侧 `UseAtomUI` 内部不同
  （HEAD 有 staged 判定与 `Build(allowUnresolvedBundles:)`，6.2.5 直接 `Build()`），`LanguageTags` 成员 HEAD 为表达式体、
  6.2.5 为属性——harness 只用公开配置方法，不触及这些差异。
- 采样：`run_samples.py`，每变体 1 次预热（不计入）+ 10 轮交错（奇数轮正序、偶数轮倒序），保留全部样本。
- 采样期间负载与失败/高负载样本：见各 summary.md。
- 不可用变体：R2R（harness 与 gallery）——仓库 Toolchain 构建边界与 R2R 的全图 RID 构建冲突
  （`src/AtomUI.Toolchain/AtomUI.Toolchain.csproj:66` 守卫，restore/build/trimmed+R2R 三种组合均复现，6.2.5 与 HEAD 相同）。
  详见 [步骤 0 计划](../plans/2026-10-14-startup-step0-measurement-and-spikes.md) 任务 3 步骤 5。
- 本节数据只适用于这台机器。

### 1.2 各标签汇总

**采样条件偏离声明**：本轮采样开始时机器负载约 6.5（10 核，阈值 5），**全部样本被高负载标记**——这是用户
"直接跑"的决定。后果：各变体的**绝对数值系统性偏高**，不能直接与空闲机器的历史数字相比；**配对中位差与
"更快轮数"（同轮交错，两变体承受相同负载）仍然有效**，是本次数据的主要判据。正式验收前建议在空闲机器上
重测基线作为分母。

Gallery 场景首帧批 = `Mark.ShellFirstBatch`；harness 场景 = `Mark.FirstRenderedBatch`。时间为自 Main 起算 ms，
格式"中位数（最小–最大）"。

#### gallery-aot

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 25（21–38） | 629（617–678） | 661（649–701） | — | — | 64（51–72） |
| head | 23（22–38） | 446（421–478） | 474（444–501） | — | — | 67（54–84） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.MaxUiGap | +6 | 4/10 |
| Mark.ShellFirstBatch | -184 | 10/10 |

#### gallery-jit-release

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 34（30–43） | 2336（2238–2421） | 2374（2275–2454） | — | — | 90（64–144） |
| head | 34（31–135） | 1918（1826–2305） | 1957（1859–2440） | — | — | 498（488–510） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.MaxUiGap | +406 | 0/10 |
| Mark.ShellFirstBatch | -411 | 10/10 |

#### harness-atom-plain-aot

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 13（12–15） | 237（198–255） | 250（210–270） | 14（14–15） | 209（177–231） | 12（3–34） |
| head | 13（12–15） | 214（180–272） | 227（192–285） | 5（4–5） | 185（163–245） | 12（4–27） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.FirstRenderedBatch | -27 | 8/10 |
| Mark.FirstUiPump | -29 | 7/10 |
| Mark.MaxUiGap | -2 | 5/10 |
| UseAtomUI.SyncTotal | -9 | 10/10 |

#### harness-atom-plain-jit-debug

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 34（32–37） | 1290（1257–1365） | 1324（1290–1399） | 874（857–920） | 1231（1190–1305） | 40（35–51） |
| head | 36（33–41） | 888（840–953） | 925（876–992） | 439（431–475） | 826（780–884） | 42（35–47） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.FirstRenderedBatch | -404 | 10/10 |
| Mark.FirstUiPump | -412 | 10/10 |
| Mark.MaxUiGap | -1 | 5/10 |
| UseAtomUI.SyncTotal | -429 | 10/10 |

#### harness-atom-plain-jit-release

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 37（34–70） | 1219（1165–1302） | 1257（1207–1340） | 785（768–808） | 1153（1120–1230） | 40（36–53） |
| head | 42（33–71） | 822（792–907） | 873（830–946） | 371（368–379） | 755（737–829） | 44（38–59） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.FirstRenderedBatch | -393 | 10/10 |
| Mark.FirstUiPump | -399 | 10/10 |
| Mark.MaxUiGap | +4 | 2/10 |
| UseAtomUI.SyncTotal | -413 | 10/10 |

#### harness-atom-window-aot

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 12（12–14） | 228（204–249） | 240（216–262） | 14（13–15） | 191（186–211） | 26（1–31） |
| head | 13（12–19） | 215（201–253） | 227（214–265） | 5（5–5） | 193（185–211） | 9（5–45） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.FirstRenderedBatch | -2 | 5/10 |
| Mark.FirstUiPump | +2 | 4/10 |
| Mark.MaxUiGap | -1 | 5/10 |
| UseAtomUI.SyncTotal | -9 | 10/10 |

#### harness-atom-window-jit-debug

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 58（39–99） | 1855（1473–2571） | 1909（1522–2650） | 1022（885–1044） | 1772（1397–2459） | 59（43–75） |
| head | 54（40–105） | 1260（1091–2195） | 1327（1130–2259） | 498（449–526） | 1178（1006–2086） | 59（46–80） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.FirstRenderedBatch | -465 | 9/10 |
| Mark.FirstUiPump | -465 | 9/10 |
| Mark.MaxUiGap | +3 | 4/10 |
| UseAtomUI.SyncTotal | -496 | 10/10 |

#### harness-atom-window-jit-release

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| 6.2.5 | 35（32–41） | 1288（1233–1370） | 1321（1266–1407） | 772（743–790） | 1237（1175–1290） | 50（41–64） |
| head | 36（33–41） | 905（850–974） | 940（886–1010） | 368（357–380） | 837（806–897） | 44（38–52） |

配对（head − 6.2.5，同轮）：

| 指标 | 中位差 ms | head 更快轮数 |
|---|---|---|
| Mark.FirstRenderedBatch | -381 | 10/10 |
| Mark.FirstUiPump | -393 | 10/10 |
| Mark.MaxUiGap | -5 | 8/10 |
| UseAtomUI.SyncTotal | -402 | 10/10 |

#### harness-plain-aot

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| head | 12（12–14） | 196（181–208） | 208（192–221） | — | 168（157–182） | 10（4–30） |

#### harness-plain-jit-debug

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| head | 34（33–37） | 426（392–439） | 459（429–475） | — | 355（345–371） | 42（36–56） |

#### harness-plain-jit-release

| 变体 | Launch.ToMain | 首帧批（ms，中位/最小–最大） | Launch.首帧批 | UseAtomUI.SyncTotal | FirstUiPump | MaxUiGap |
|---|---|---|---|---|---|---|
| head | 35（31–44） | 428（408–447） | 465（444–488） | — | 364（357–380） | 48（34–54） |

### 1.3 与 spec §12.3 预测的对照

本次基线在负载下测得，绝对数值与 spec 引用的历史批次不可直接比；以下只列出**结构性结论**与需要修正的模型输入：

| 项 | spec 引用 | 本次实测（负载下） | 判定 |
|---|---|---|---|
| 6.2.5 Gallery Shell 批次 | ≈1.955 s（10-12 同步模式，空闲机器） | 2.336 s | 偏差 +19%，**须以空闲机器重测为准**；不据此修正模型 |
| HEAD Gallery Shell 批次 | ≈1.97 s | 1.918 s | 接近；但同样在负载下，重测前不下结论 |
| HEAD 相对 6.2.5 的窗口提前 | 窗口提前约 0.7 s、Shell 晚约 0.2 s（rd-log §0.1） | MainWindowOpened −416 ms、ShellFirstBatch **−411 ms（10/10 轮）** | **与 rd-log 结论方向相反**：同口径下 HEAD 的 Shell 首帧并不比 6.2.5 晚。rd-log 的"晚 0.2 s"来自跨批次、跨口径对比。这是步骤 1 设计的重要输入：分期启动对"Shell 就绪"的拖累在一致口径下不存在 |
| HEAD 首帧后 UI 阻塞 | 分期启动把工作挪到首帧后 | Gallery MaxUiGap **498 ms**（6.2.5 为 90 ms，Δ+406，0/10） | **直接量化了分期启动的代价**：HEAD 在主窗体可见后仍有约 0.5 s 的 UI 线程阻塞（Stage 2 演化），这正是新架构要消除的 |
| JIT UseAtomUI 关键路径 | 6.2.5 同步 ≈579 ms；HEAD Stage 1 ≈0.28 s | 6.2.5 772 ms；HEAD 368 ms（atom-window，Δ−402，10/10） | 结构一致（Δ≈0.4 s），绝对值受负载抬高；新架构内核关键路径目标（40–90 ms）仍以 Commit 实测为准 |
| AOT 主窗体首帧 | 6.2.5 Opened ≈0.46 s；预测新架构 0.30–0.34 s | Gallery AOT ShellFirstBatch：6.2.5 629 ms、HEAD 446 ms（Δ−184，10/10） | 负载抬高绝对值；相对差距真实。新架构 AOT 预测区间待步骤 1 后用同机空闲重测校准 |
| plain Avalonia 空窗口（验收线 2 分母） | 同机纯 Avalonia | jit-release 428 ms（FirstRenderedBatch）/ 364 ms（FirstUiPump）；AOT 196 ms | 验收线 2 预测（Splash ≤ plain + 150 ms ≈ 578 ms）与该分母相容 |
| Launch.ToMain | 约 50–65 ms（历史） | 12–58 ms（依形态） | 一致 |

## 2. 原型 S1–S3

原型：`/private/tmp/atomui-test-startup-step0/spike-splash`（一次性，结论落档后删除）。
纯 Avalonia 12.1.3（无 AtomUI 依赖）：普通 `Window`（白色 Border 模板）+ `TextBlock` + 旋转方块
（合成层 `CreateScalarKeyFrameAnimation`，`RotationAngle`，`IterationBehavior.Forever`）。

### S1 主循环前驱动 Dispatcher 画出 Splash —— **可行（macOS）**

- 假设：`RegisterServices()` 里 `Show()` 后手动 `Dispatcher.UIThread.MainLoop(token)`，能在主循环开始前
  让 Splash 的第一个合成批次渲染完成。
- 结果：`--pump --block-ms 1500` 三次运行，`Mark.SplashFirstBatch` = 522–545 ms，
  `Mark.InitializeExited` ≈ 2086–2097 ms（早约 1.5 s）；无 `Mark.PumpTimedOut`；进程正常退出。
- 用户目视确认：Splash 窗口在屏幕上真实可见（不只是渲染图存在）。
- 对步骤 1 的影响：pump 方案在 macOS 可用，按 spec 设计实现；线程级超时（5 s）保留作为失败兜底。
  注意点：pump 会占用主线程额外 ~0.5 s（`Mark.PumpExited` ≈ 585–596 ms 含等待首帧），应计入 Splash
  首帧预算；等不到首帧必须超时退出并走 spec §17 退化方案。

### S2 合成层动画在 UI 线程阻塞期间是否持续 —— **可行**

- 假设：旋转动画由渲染线程驱动，UI 线程 `Thread.Sleep(2500)` 期间方块持续旋转。
- 数据：`Mark.SpinnerBatch`(≈657–666) < `Mark.PumpExited`(≈664–672) < `Mark.InitializeExited`(≈3165–3173)，
  满足前置断言（动画在 UI 线程被阻塞前已提交到渲染线程）。
- **用户目视确认：阻塞的 2.5 s 期间方块一直旋转，无冻结。**
- 录屏抽帧未能定位窗口（screencapture 视频疑似未录到该窗口，白像素占比恒定），以目视为准。
- 对步骤 1 的影响：Splash 的进度动画可以用合成层动画，主窗体构建期间不会冻住。

### S3 主窗体交接与提前关闭 —— **可行，交接瞬间有短暂卡顿（已知、可接受）**

- external（`OnFrameworkInitializationCompleted` 同步创建主窗体）：
  `MainWindowFirstBatch` 1521 ms → `SplashClosed` 1687 ms（间隔 166 ms = 淡出 150 + 关闭定时 160）；
  `MainWindowActivated` 出现；Splash 关闭晚于主窗体首帧，无空白。
- factory（模拟 `UseMainWindow` 异步工厂，主循环后 `Task.Delay(1000)` 再建主窗体）：
  同样顺序成立；`SplashClosed` 晚于 `MainWindowFirstBatch`。
- close-early（`--close-splash-at-ms 300`）：`SplashClosed`=981 ms、`ShutdownRequested`=981 ms，
  无 `MainWindowOpened`，5 s 内退出——提前关闭触发取消启动并主动 Shutdown 的语义正确。
- **交接卡顿（用户目视确认两次）**：第一次冷运行，主窗体出现前方块短暂冻结约 1 s（标记间隔
  `MainWindowFirstBatch`→`SplashClosed` 1043 ms），随后 Splash 消失、主窗体出现；第二次热运行间隔 177 ms。
  解释：主窗体第一帧的栅格化（冷字体/字形缓存）占住**渲染线程**，Splash 的渲染线程动画随之暂停——
  这是 S2 的镜像结论：UI 线程忙不影响合成动画，**渲染线程忙会**。
- 对步骤 1 的影响：交接瞬间的卡顿出现在"主窗体首帧栅格化"阶段，新架构无法完全消除（主窗体首帧总要
  栅格化）；缓解方向：Splash 关闭安排在主窗体首帧**之后**（当前方案即如此），可接受一次短暂停顿；
  若步骤 6（chrome 优化）后首帧栅格化显著缩短，卡顿自然减弱。不做"预渲染主窗体到离屏"之类的高复杂度方案。
- interactive 变体未单独跑：用户已多次实时目视确认交接过程，判定标准满足。

### Windows / Linux

spike-splash 目录已交付用户，在 Windows（x64）与 Linux（X11、Wayland）上执行同样命令后回收输出。
未回收前标注"未验证"。

## 3. 原型 S4：MultiCoreJit —— **有效**

同一二进制（HEAD jit-release），开/关 `ProfileOptimization` 两变体，10 轮交错 + 1 预热（预热即生成配置文件，
后续轮次读取已有配置）。全部样本仍带高负载标记（负载 ≈6.5），配对差有效。

| 场景 | 指标 | off | on | 中位差 | on 更快 |
|---|---:|---:|---:|---:|
| Gallery | `Launch.ToShellFirstBatch` | 1923 ms | 1614 ms | **−309 ms（−16.1%）** | 10/10 |
| harness atom-window | `Launch.ToFirstRenderedBatch` | 931 ms | 594 ms | −338 ms（−36%） | 10/10 |
| harness atom-window | `Mark.FirstUiPump` | 825 ms | 490 ms | −335 ms | 10/10 |

判定标准（≤ −10% 且 ≥ 8/10）满足，**记"有效"**。

边界与代价（写入步骤 5 的决策依据）：

- 收益来自"配置已存在"的第二次及以后运行；**首次运行无收益且要付配置写入开销**（早前 smoke：首次 1112 ms，
  第二次 571 ms，同二进制）。终端用户冷启动的"第一次"正是最需要优化的那一次——步骤 5 的默认策略要么接受
  首次无收益（第二次起 −16%），要么把 JIT 配置文件随应用分发（需要 per-machine 适配与签名问题评估），
  要么用 ReadyToRun（当前被 Toolchain 构建边界挡住，见 §1.1）。
- `.jitprofile` 体积：harness 186 KB、Gallery 203 KB。
- 本实验的配置由本机采样进程自身生成，与真实用户机器的热配置形态一致。

## 4. 原型 S5：生成器读取引用包清单的成本 —— **可行**

原型：8 个包 × 256 KB 清单常量（内存编译，与磁盘元数据读取同量级），两种生成器写法对拍，
每次编辑替换一棵语法树，20 次编辑取中位数与最大。运行 3 次：

| 写法 | 首次（ms） | 编辑中位（ms） | 编辑最大（ms） |
|---|---:|---:|---:|
| naive（每次编辑经 CompilationProvider 重读全部清单） | 37.7 / 38.8 / 38.4 | 0.02–0.03 | 2.89–3.31 |
| cached（按 MetadataReferencesProvider 增量，直接读元数据常量） | 9.8 / 9.3 / 9.5 | 0.02 | 1.04–1.15 |

判定标准（cached 编辑中位 < 5 ms 且首次 < 300 ms）满足，**记"可行"**。

- naive 的编辑开销也意外地小：Roslyn 5.0 的增量引擎下，重读内存元数据常量本身是亚毫秒级，两种写法的
  编辑期差异只有 2–3 倍；首次差异 4 倍（38 vs 9.5 ms）。包数量线性外推（30 包 × 256 KB）：cached 约
  35 ms、naive 约 140 ms，都在可接受范围。
- 结论：步骤 2 的启动镜像链接用 cached（`MetadataReferencesProvider` + 元数据常量读取）实现，
  无性能障碍；清单常量编码（§7.2）不需要为生成器性能做特殊设计。

## 5. 对步骤 1 的输入

### 5.1 原型判定汇总

| 原型 | 判定 |
|---|---|
| S1 主循环前 pump 画出 Splash | 可行（macOS）；保留 5 s 线程级超时兜底；pump 成本 ~0.5 s 计入 Splash 首帧预算 |
| S2 合成动画在 UI 阻塞期间持续 | 可行（渲染线程驱动） |
| S3 主窗体交接 | 可行；Splash 在主窗体首帧后关闭；交接瞬间渲染线程被首帧栅格化占用时动画会短暂冻结（冷缓存 ~1 s、热 ~0.18 s），接受 |
| S4 MultiCoreJit | 有效（Gallery −16.1%，10/10）；首次运行无收益，步骤 5 决策按此 |
| S5 生成器读清单 | 可行（cached 首次 ~9.5 ms、编辑 ~0.02 ms） |

### 5.2 spec §12.3 预测修正（本机、负载下口径）

- 本机负载下 6.2.5 Gallery `ShellFirstBatch` 中位 2.336 s（历史空闲 1.955 s，+19%）。预测模型
  （§12.2）应用到本机基线：2.336 − 0.49..0.54 ≈ **1.80–1.85 s**；空闲机器的 1.45–1.65 s 区间不动。
- 验收线 1 换算到本机负载口径：JIT 主窗体完整首帧 ≤ 6.2.5 × 0.75 = **1.75 s**（空闲机器重测后重算）。
- 验收线 2 分母：plain jit-release `FirstRenderedBatch` 428 ms（负载下）→ Splash 首帧线 ≈ **578 ms**；
  AOT 分母 196 ms。预测 Splash 0.37–0.52 s 与该线相容。
- AOT：6.2.5 Gallery ShellFirstBatch 629 ms、HEAD 446 ms（Δ−184，10/10）；预测区间待步骤 1 后
  同机空闲重测校准。
- R2R 预测保持"低可信度"：发布路径被 Toolchain 构建边界挡住（§1.1）。

### 5.3 步骤 1 子 spec 必须采纳的决定

1. pump 方案照 spec §5 实现，超时 5 s + 退化（spec §17）。
2. Splash 关闭严格安排在"主窗体首个渲染批次完成之后"，不做预渲染离屏等方案。
3. Splash 动画全部走合成层（渲染线程）；交接冻结作为已知行为写入 splash 文档。
4. 内核关键路径验收（Commit ≤ 0.1 s JIT）沿用 §12.4 第 4 条。
5. MCJ：步骤 5 再决策默认策略（首次无收益、配置随应用分发的可行性待评估）；步骤 0 不预设。
6. 步骤 2 链接器用 cached（MetadataReferencesProvider）实现。
7. R2R 的 Toolchain 边界问题作为独立产品改动提案记录，不在步骤 1–4 范围内。
8. 正式验收前在空闲机器重测基线（本机负载下数据只作相对比较）。

### 5.4 遗留

- Windows/Linux 的 S1/S3 复核：spike-splash 目录交付用户执行，未回收前"未验证"。
- 空闲机器基线重测（5.3-8）。
