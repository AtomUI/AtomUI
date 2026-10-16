# 启动重构步骤 1 测量结果

> 关联：子 spec `docs/superpowers/specs/2026-10-14-startup-step1-lifecycle-and-splash-design.md`；
> 计划 `docs/superpowers/plans/2026-10-14-startup-step1-lifecycle-and-splash.md` Task 9；
> 步骤 0 基线 `docs/superpowers/progress/2026-10-14-startup-step0-results.md`。

> 2026-10-15 接续核验：下述首轮数据为历史测量；当前 Metal 复测、独立终审与清理后的验证见文末。
> 步骤 1 的功能实现已落地，但默认 Splash 的性能不退步门禁仍未通过，不得据模块测试或构建成功宣布整体优化完成。

## 结论速览

| 验收项 | 判定线 | 结果 | 判定 |
|---|---|---|---|
| 验收线 2：Splash 首帧 | `atom-splash` `Mark.SplashFirstFrame` 中位 ≤ `plain` `Mark.FirstRenderedBatch` 中位 + 150 ms | 562.0 ≤ 443.9 + 150 = 593.9 | **达成**（余量 31.9 ms） |
| 不退步：Gallery JIT | `Launch.ToShellFirstBatch` 配对中位差（当前 − HEAD）≤ 0 | +1150.3 ms，HEAD 10/10 轮更快 | **未达成** |
| 不退步：Gallery AOT | 不差于步骤 0 的 6.2.5（同机） | 716.6 ms 对 661 ms（+55.6） | **名义未达成，结论不可靠**（见"负载"） |
| 记录：主窗体首帧后最长 UI 卡顿 | 期望远低于 HEAD 的约 0.5 s | JIT 302 ms（HEAD 546）；无 Splash 时 137 ms（HEAD 528） | 改善 |

按计划规则：未达线时用 `Phase.*` 分段定位、写入本文并停下向用户汇报，不降低验收线。定位结论：**退步几乎全部来自
Gallery 默认启用的 Splash**；不显示 Splash 时新生命周期对 HEAD 基本持平（+60 ms，4/10 轮更快），最长 UI 卡顿下降约 74%。

## 方法

- 机器：macOS 26.6.2，arm64，10 核；SDK 10.0.300。与步骤 0 同机。
- 采样：`tools/performances/AtomUI.StartupPerformance/run_samples.py`，每组 10 轮正式 + 1 轮预热，变体交错执行；
  原始数据在 `.artifacts/performance/startup-step1/{splash,gallery-jit-release,gallery-aot,gallery-jit-diag}/`。
- harness：Release JIT 发布（`--self-contained false`），新增 `atom-splash` 场景（`atom-window` + `UseSplash(SplashMode.Always)`），
  进程内 `StartupEventListener` 订阅 `AtomUI-Startup` EventSource 写出 `Phase.*` / `Mark.State.*` / `Mark.SplashFirstFrame`。
- Gallery：HEAD 基线 worktree（`60dd72afb`）与当前工作区各发布 jit-release；当前工作区另发布 NativeAOT（`osx-arm64`）。
  探针沿用步骤 0 的 `StartupProbe.cs`，从步骤 0 计划正文重新提取，SHA-256
  `793db1312f20965680598e9727c42a9d1022cca24dd71a046c5f90894de446a0`，与步骤 0 记录逐字节一致；两侧 `Main` 接入方式相同
  （`MarkMain` → `AfterSetup(Attach)` → `Report`）。当前工作区的探针与 `Main` 改动仅为发布临时加入，发布后已按 SHA-256 恢复。
- 诊断组（`gallery-jit-diag`）：为定位单独构建的当前 Gallery，探针内加 `AtomUI-Startup` 监听，并以环境变量
  `ATOMUI_SPLASH_OFF=1` 切换 `SplashMode.Off`；与 HEAD 三方交错。该组只用于归因，不参与验收判定。
- **负载**：全部采样期间 loadavg 7–27（10 核），来自同机的虚拟机、Rider、ZCode 等常驻负载，无法在测量期间消除。
  交错采样保证同组变体承受相同负载，组内相对比较有效；与步骤 0 数据的跨时段绝对比较（AOT 一项）不可靠。

## 验收线 2：Splash 首帧（harness，`step1-splash-vs-plain`）

| 指标（ms，以 Main 为零点） | plain 中位 | atom-splash 中位 |
|---|---:|---:|
| `Mark.FirstRenderedBatch` | 443.9 | 992.6 |
| `Mark.SplashFirstFrame` | — | 562.0 |
| `Mark.FirstUiPump` | 371.6 | 468.0 |
| `Mark.MaxUiGap` | 46.9 | 402.8 |

`atom-splash` 启动分段（中位）：`Phase.Configure` 31.8、`Phase.Prepare` 67.9（`Phase.PrepareWait` 0.0，后台准备与平台初始化
完全重叠）、`Phase.Splash` 333.8、`Phase.Commit` 257.1（`Commit.Theme` 193.7、`Commit.Localization` 32.8、`Commit.Initializers` 31.6、
`Commit.OwnedServices` 7.2）。harness 的 `Mark.MaxUiGap` 从 UI 线程首次响应（Splash 泵消息）起算，因此包含提交本身的阻塞，
与 Gallery 探针"主窗体首帧后"的口径不同。

## 不退步：Gallery JIT（`step1-gallery-jit-release`，HEAD 对当前）

| 指标（中位，ms） | HEAD | 当前 | 配对中位差 | 当前更快轮数 |
|---|---:|---:|---:|---:|
| `Launch.ToShellFirstBatch` | 2167.5 | 3317.8 | **+1150.3** | 0/10 |
| `Mark.SetupCompleted` | 557.1 | 1151.3 | +580.4 | 0/10 |
| `Mark.MainWindowFirstBatch` | 1081.8 | 1843.2 | +750.8 | 0/10 |
| `Mark.ShellFirstBatch` | 2112.1 | 3261.5 | +1149.3 | 0/10 |
| `Mark.MaxUiGap` | 546.0 | 302.4 | −248.1 | 10/10 |

## 定位：Gallery JIT 三方诊断（`step1-gallery-jit-diag`）

| 指标（中位，ms） | HEAD | 当前（Splash 开） | 当前（Splash 关） |
|---|---:|---:|---:|
| `Launch.ToShellFirstBatch` | 2121.3 | 3019.8 | 2179.4 |
| `Mark.SplashFirstFrame` | — | 829.7 | — |
| `Phase.Configure` | — | 161.8 | 165.3 |
| `Phase.Prepare`（后台） | — | 89.7 | 91.8 |
| `Phase.Splash` | — | 380.7 | — |
| `Phase.Commit` | — | 308.9 | 326.1 |
| `Phase.Commit.Theme` | — | 230.2 | 225.4 |
| `Mark.SetupCompleted` | 554.0 | 1161.6 | 806.8 |
| `Mark.MainWindowOpened` | 787.7 | 1538.3 | 1302.0 |
| `Mark.MainWindowFirstBatch` | 1076.5 | 1817.5 | 1706.8 |
| `Mark.ShellAttached` | 2050.1 | 2934.6 | 2067.6 |
| `Mark.MaxUiGap` | 527.7 | 297.7 | 137.1 |

配对：HEAD 对 Splash 关——`Launch.ToShellFirstBatch` +60.0 ms（当前 4/10 轮更快，处于噪声范围），`Mark.MaxUiGap` −393.5 ms（10/10）；
Splash 开对 Splash 关——`Launch.ToShellFirstBatch` +693.5 ms（9/10 轮更慢）。

归因：

1. **新生命周期本身基本持平**：不显示 Splash 时，完整提交移到主窗体之前使主窗体晚出现（`MainWindowFirstBatch` +601 ms），
   但完整 Shell 出现时刻与 HEAD 相当；HEAD 主窗体首帧后约 0.5 s 的分级启动冻结消失（`MaxUiGap` 528 → 137 ms）。
2. **Splash 的代价约 0.7–0.9 s，分三段**：提交之前约 355 ms（`Phase.Splash` 380.7：建窗、自带主题与首帧泵送，处于关键路径上）；
   主窗体首帧约 +110 ms；主窗体首帧之后到 Shell 可见约 +750 ms（Splash 开 1117 ms 对 Splash 关 361 ms），本轮未进一步拆分，
   候选因素见"后续"。
3. **Splash 进度条冻结**（实机录屏逐帧量化，不影响上表数值；上表测量时仍为 OpenGL 优先）。分两层原因：
   - **动画被移除（已修复）**：OpenGL 与 Metal 两次运行都冻结在同一像素位置，即布局给进度块的 Offset(0,0)。Avalonia 的
     `Visual.SynchronizeCompositionProperties` 每次同步都会改写布局元素合成视觉的 `Offset`，而动画属性的直接赋值会
     `RemoveAnimationForProperty`。`SplashProgressBar` 已改为在挂接的 `CompositionCustomVisual` 上按合成时钟绘制不确定进度。
   - **渲染线程阻塞（未修复，Avalonia 层面）**：改为 Metal 优先并修复上一条后，提交期间进度条持续移动；但主窗体上屏前仍有约
     1.1 s 停顿。`sample` 显示渲染线程约 1.3 s 阻塞在 `AvnMetalRenderTarget::BeginDrawing → -[CAMetalLayer nextDrawable]`
     （drawable 池耗尽，1 s 超时），停顿结束时刻与主窗体上屏时刻吻合（5.18 s 对 5.23 s）。`Window.ShowCore` 先
     `StartRendering()` 再 `PlatformImpl.Show()`，主窗体在真正上屏前就被渲染，其 Metal 帧无人消费；所有窗口共用一个渲染线程，
     Splash 随之停顿。OpenGL 优先时同一阶段改为"帧经主线程上屏"而冻结。

## 不退步：Gallery AOT（`step1-gallery-aot`）

AOT 下 `SplashMode.Auto` 不显示 Splash。当前：`Launch.ToShellFirstBatch` 716.6 ms（686.1–840.9）、`Mark.ShellFirstBatch` 688.6、
`Mark.MainWindowFirstBatch` 404.0、`Mark.MaxUiGap` 105.1。步骤 0 同机数据：6.2.5 `Launch` 首帧批 661、`ShellFirstBatch` 629、
`MaxUiGap` 64；HEAD 474 / 446 / 67。本组采样时 loadavg 11.6–15.3，且无法与 6.2.5 交错（步骤 0 的二进制已清理），
+55.6 ms 的差值不足以判定退步，需要在安静的机器上与重新发布的 6.2.5 AOT 交错复测。

## 待用户决定

1. 是否保留 Gallery 默认启用 Splash（`SplashMode.Auto`）。保留则 JIT 完整 Shell 约晚 0.7–0.9 s，但 Splash 在约 0.8 s 出现；
   关闭则对 HEAD 基本持平且卡顿大幅下降。"不退步"线是否按"是否显示 Splash"分别定义，需要用户裁定（本次不降低验收线）。
2. 是否在步骤 1 内继续拆分并优化 Splash 的成本（尤其主窗体首帧后的约 750 ms，以及提交前的首帧泵送）。
3. macOS 默认渲染已按用户指示试改为 Metal 优先（Avalonia 12.1.0 起 #21588 已修复 Metal live resize 抖动，需用户实机确认缩放
   表现）；主窗体上屏前约 1.1 s 的渲染线程阻塞属于 Avalonia 多窗口共用渲染线程的行为，是否向上游反馈或在 Gallery 侧缩短
   "主窗体 Show 到上屏"的主线程工作，需要用户决定。以 Metal 为默认后，Gallery 的 JIT/AOT 对比需重测。
4. AOT 是否在安静机器上与 6.2.5 交错复测后再判定。

## 后续（未做，供决定后执行）

- 拆分 Splash 开启时"主窗体首帧 → Shell 可见"的约 750 ms：候选因素包括 Splash 淡出与关闭期间 `Forever` 合成动画持续出帧、
  IOSurface 呈现回调占用主线程、两个窗口共用渲染线程。
- 提交前 `Phase.Splash`（约 335–380 ms）：建窗与等待首帧的耗时细分。
- 安静机器上的 AOT 交错复测（6.2.5 / HEAD / 当前）。

## 交付记录（规范《测试价值与生命周期》§8）

**验证对象**：步骤 1 的目标契约——启动入口迁移到 `AppBuilder.UseAtomUI`、挂接生成器与守卫、全局 Token 后台预备与
一次性提交、Splash（含自定义主题与错误态）、主窗体交接、分级启动与 `BootstrapRegistration` 删除、`AtomUI-Startup` 测量事件。

**实现前**：基线提交 `60dd72afb`（= 6.2.5 + 8 个启动优化提交）。执行 `python3 scripts/verification/test.py run --scope iterate --path …`
与逐项目 `dotnet test`；预期新入口/生成器/Splash 类型不存在，分级启动测试存在。实际：基线工作树 `/private/tmp/atomui-step1-baseline`
上分级启动相关用例通过、新类型缺失，与变更后形成对照。

**实现后**：同一范围已执行，存在下表明确列出的失败，不能表述为全部通过。逐任务证据在
`.superpowers/sdd/2026-10-14-startup-step1-lifecycle-and-splash/`（`t1-…` 至 `t6-…` 日志、`t*-red.log` / `t*-green.log` 红绿对）。

**测试处置**：
- 删除（需求随分级启动失效）：`ApplicationScopeTests` 4 例、`ApplicationLocalizationStartupTests` 1 例、`ControlRegistrationRuntimeTests.Stage_Two_*` 1 例。
- 改写（保留仍有效的契约，去掉分级启动/首阶段断言）：`ThemeCatalogReloadTests.Catalog_Changes_Publish_An_Atomic_Catalog_Event`、
  `TypeMapRegistrationGeneratorTests`、`ControlRegistrationRuntimeTests.Collector_Uses_Only_TryGetValue_…`、
  `TypedPackageEntryTests.Generated_descriptors_are_reused_…`、`TypeMapIntegrationTests` 2 例。逐条理由见
  `.superpowers/sdd/2026-10-14-startup-step1-lifecycle-and-splash/t1-test-deletions.md`。
- 删除的临时测试（本计划新建，验证后删除）：Core 5 个文件、Controls 2、Desktop.Controls 3、Gallery 1，共 11 个文件，
  及 4 个 `Temporary/startup-step1/` 与 `Temporary/` 目录。Extras 包移除时一并删除 31 例 Splash 测试。
- 保留（常驻候选，逐条准入）：
  1. `tests/AtomUI.Generator.Tests/Startup/ApplicationStartupHookGeneratorTests.cs`（10 次执行，含别名参数化与反向用例）。契约：生成器为每个具体 partial
     应用生成 `RegisterServices` 挂接，手写重写漏调 `Attach` 报 `ATOMUIGEN039`，抽象类/非 partial 不生成。不可替代性：
     生成器诊断只能由生成器测试兜底，编译器不覆盖；漏生成或生成器异常会使任意应用失去启动挂接，因此需进入全局运行。成本：纯编译期，本轮该类 10 次执行合计约 1 s；跨文件、无基类主声明、命名冲突合并至现有用例，没有另建永久类。
  2. `tests/AtomUI.Core.Tests/Startup/AtomUIStartupContractTests.cs`（1 例）。契约：内核在 `Initialize()` 之前提交并到达 `Ready`。
     全局运行必要性：Prism（开发者实际在用）在 `Initialize()` 中创建并显示 Shell，`App.axaml` 也在此时引用 AtomUI 资源；
     提交一旦推迟，两者都拿不到主题与本地化，而这是静默失败。此前仓内测试应用全部手写挂接，该用例首次经真实生成器路径覆盖。
     反证：把 `Attach` 的提交临时推迟到 `Initialize` 之后 → RED（`ThemeManagerSeenInInitialize` 为 false），恢复 → GREEN。
  3. `tests/AtomUI.Controls.Tests/Splash/SplashDefaultThemeTests.cs` 新增
     `Default_theme_templates_keep_appearance_values_in_styles`（连同既有权重 3 例）。契约：默认主题的外观值必须留在样式中，
     模板只描述结构，否则 `BasedOn` 局部覆盖改不动（Task 10 实测发现）。反证：把 `Width="420"` 写回 `Card` → RED，恢复 → GREEN。
- 保留上述 3 类契约保障；这是永久测试准入例外，不能称为全局用例净新增为零。生成器当前 10 次执行，Core 1 次，Splash 3 次；本轮修复仅扩充既有生成器输入，不再净增用例。

**清理结果（接续核验）**：本任务临时测试与专用 AXAML、全部 `Temporary/` 目录已删除；原会话的
`/private/tmp/atomui-test-startup-step1/`、两个独立终审复现工程与基线 worktree `/private/tmp/atomui-step1-baseline` 已删除。
原始采样 JSON、二进制哈希、必要验证日志保留在 `.artifacts/performance/startup-step1/`；不再保留可运行临时复现工程。
生产 Gallery 探针已复原。AOT 临时发布目录也已在运行结束后删除，结果见下文专项记录。

**原 Claude 会话模块验证**（清理前历史结果，`--scope iterate` 及未执行项目补跑，不能认证接续修复后的工作树）：

| 项目 | 结果 |
|---|---|
| Build.Tasks | 29/29（最后一次 iterate 输出） |
| Controls.Shared | 18/18 |
| Controls | 29/29（含后来删除的两条临时 Splash 用例） |
| Core | 364/364（含新增契约用例） |
| Desktop.Controls | 4141/4147 |
| Desktop.Controls.DataGrid | 139/139 |
| Desktop.Controls.Rendering | 77/77 |
| Generator | 466/468 |
| Icons.Shared | 1/1 |
| Localization.IntegrationTests | 4/5 |
| Toolkits.GalleryBase.Generator | 10/10 |
| Toolkits.GalleryBase | 211/211 |
| AtomUIGallery | 564/565 |

账本中已用基线或精确复跑归类的失败如下；这些历史结论不表示最终套件全绿：Centered_Card_* 组、FeedbackCardMotion.Cancel_*、
ShowCasePanelStructure.GalleryShowCaseHeader_*、BuildLayoutTests ×2、PopupShadow/Pinning ×2、AvatarImageLoading ×1（单跑通过）、
MasonryLayoutTests.LayoutStrategy_Is_Forwarded_To_The_Default_ItemsPanel ×1（测试清理期 `Compositor` 构造的线程归属异常，
非断言失败，单跑 23/23 通过）、Language_Template_Is_Exported_…（Pack 内 `-restore` 超 120 s 上限，机器空闲时 23 s 通过，Task 2 已定级）。
`git diff --check` 无输出；无残留进程。

**专项与 UX**：
- NativeAOT：`PublishToLocal.ps1` 发布 Gallery AOT 通过；实机运行主窗体完整出现、不显示 Splash（`Auto` 在 AOT 下的预期），
  进程稳定 66 s 后正常退出。窗口标题无法读取（本机未授予窗口名权限），肉眼确认待用户补。
- Browser：Release 构建通过；未启动实跑（本机未做浏览器侧运行验证）。
- 设计器：未在 Rider 中打开 `.axaml` 预览验证（手动机器操作，未做）。
- 性能：见本文上文测量章节。验收线 2 达成；"不退步"线未达成，定位结论为 Gallery 默认 Splash 的代价，待用户裁定。
- 平台：Windows 与 Linux（X11/Wayland）实机清单未回收，标"未验证"。

## 终审（会话内自审）

整分支终审原计划交子代理，两次因 API 余额（403/402）中断，改为在会话内逐文件审查新增源码（Core/Startup 全部、Splash 全部、
启动钩子生成器、ThemeStartup* 与提交路径）。原会话报告两处，接续核验后其中第二处不成立：

1. **生成器按文本判定 `Attach` 调用（已修）**：`ApplicationStartupHookGenerator.CallsAttach` 用
   `target.ToString().EndsWith("AtomUIStartup")` 匹配，别名写法 `using Startup = AtomUI.AtomUIStartup; Startup.Attach(this);`
   会被误报为「缺少 Attach」（`ATOMUIGEN039`）。改为按符号判定（解析到 `AtomUI.AtomUIStartup.Attach`）。
   证据：临时用例三条写法（限定名 / `global::` / 别名）中别名一条 RED，其余两条本就通过；修复后三条全绿。
   该契约并入既有常驻用例 `Skips_an_application_that_already_calls_Attach_however_it_is_written`，
   并补反向用例 `Does_not_treat_an_unrelated_Attach_call_as_hooked`（不得因文本相似而漏挂接）。这次参数化与新增反向用例增加了执行次数，不能称为用例数不增加。
2. **`SplashProgressBar` 重挂接缺陷不成立（接续核验更正）**：会话工具记录中首次修改前的源码已经在挂接时调用
   `SyncIndicator()`。移除该调用再得到 RED，只能证明原有同步必要，不能证明修复了已有缺陷。
   终审新增的 handler 字段、跨线程 `IndicatorIsActiveForTesting` 属性及 handler 状态访问器仅供探针使用，现已删除；
   原有挂接同步和自定义合成视觉动画保留。对应临时重挂接用例与别名/partial 探针文件、空目录均已清理。

审查中确认无问题并记录的两点：`AtomUIStartupCommit.Commit` 的失败清理顺序（`ApplicationScope` 构造失败时
`ownedServices` 仍由 catch 释放）正确；`AtomUIStartupRun.PresentAsync` 的主窗体取消与超时交接路径不泄漏订阅。
接续更正：正常交接的 `CloseSplashAsync` 在淡出开始前已经退订 `ClosedByUser`，淡出期间用户关闭不会再调用启动取消处理器。
原会话关于淡出时仍会取消启动的说明没有沿事件订阅链核验，不作为实现事实。


## 2026-10-15 接续：独立终审、Metal 复测与最终收尾

### 独立终审与修复证据

使用新上下文 reviewer 审查当前未提交源码；6 个发现均经真实 Roslyn driver 或 Headless 窗口复现：

| 缺陷 | 修复前证据 | 根因修复与修复后证据 |
|---|---|---|
| 手写挂接位于另一 partial 文件 | `Resolves_Attach_in_another_partial_file`：CS8785 / Syntax node is not within syntax tree | 按方法声明的 SyntaxTree 取得 SemanticModel；GREEN |
| 首 partial 不含基类声明 | `Finds_application_when_first_partial_has_no_base_list`：零挂接、无诊断 | 语法候选包含所有 class，由唯一主声明解析完整符号；GREEN |
| A.B 与 A_B 命名空间 hint 碰撞 | `Does_not_collapse_namespace_separators_into_underscores`：AddSource 重名异常 | hint 保留命名空间分隔符；GREEN |
| Splash 首帧泵内关闭丢失事件 | `closeHadSubscriber=False; startup=Ready; completionCanceled=False` | Show 前订阅；取消后不提交。相同探针 `True; Canceled; True`，退出码 0 |
| 主窗体 Opened 内关闭被误判 Ready | `startup=Ready; completionCanceled=False`，错误等待 3 s | Show 前进入 Presenting 并订阅 Closed；相同探针 `Canceled; True`，退出码 0 |
| Dispatcher 不支持受控泵时遗留 Splash | 真实呈现器 `returnedNull=True; retainedWindow=True; windowVisible=True` | internal 呈现器接缝增加立即 Close，失败路径退订并关闭；`retainedWindow=False` |

生成器临时 3 例先全部 RED，修复后连同既有启动生成器用例 13/13；复现输入随后合并至既有常驻用例，临时文件删除。
取消与 fallback 使用 `/private/tmp` 中独立复现工程，引用本工作树 DLL；无新公共 API、无反射运行时路径。
实际反射只存在临时取证代码中。`SplashPresenter` 的窗体事件和关闭按钮订阅在 Closed 时成对释放；初始化失败也关闭已创建的窗口。

### Metal 默认下补测

复用原会话已发布并接入相同探针的产物，不重新注入生产源码：每变体 1 次预热 + 10 轮正式，奇偶轮倒序交错；
JIT 四方 40/40、AOT 两方 20/20、harness 两方 20/20 成功。数据位于
`.artifacts/performance/startup-step1/{metal-gallery-jit-recovery,metal-gallery-aot-recovery,metal-splash-recovery}/`。
配置及二进制 SHA-256 已保存；这些产物形成于 Task 10/11 之前，用于补完当时被锁屏中断的对比，不能冒充当前最终源码的性能认证。
未改 UI、移除内容或降低验收线；该测量只说明下面指标，不包含 GPU 实际上屏。

| 指标（中位 ms） | HEAD | Metal + Splash | Metal 关闭 Splash | OpenGL + Splash |
|---|---:|---:|---:|---:|
| 进程启动 → 完整 Shell 首个渲染批次 | 2211.3 | 4265.8 | 2199.6 | 3088.0 |
| 主窗体首帧后最大 UI 探针等待 | 531.9 | 293.7 | 234.7 | 281.9 |
| Main → Splash 首个渲染批次 | — | 808.3 | — | 786.2 |

配对主判据：HEAD → Metal + Splash 为 +2158.9 ms，0/10 轮更快，**不退步门禁未通过**；
HEAD → Metal 关闭 Splash 为 −10.6 ms，5/10 轮更快，整体持平，不能宣称启动加速。
Metal 关闭 Splash → Metal + Splash 为 +1892.7 ms；OpenGL + Splash → Metal + Splash 为 +1158.5 ms，均 0/10 轮更快。
Metal 双窗口组显著更慢，与此前渲染线程等待 drawable 的证据相符，单凭这组采样不能进一步分摊每个内部函数成本。

| 收益指标 | 基线 | 当前 | 公式 | 改善 | 结论 |
|---|---:|---:|---|---:|---|
| 关闭 Splash 时主窗体首帧后最大 UI 等待 | 531.9 ms | 234.7 ms | (531.9−234.7)/531.9 | 55.9% | 卡顿下降，非整体启动加速 |
| 默认 Metal + Splash 完整 Shell 就绪 | 2211.3 ms | 4265.8 ms | (2211.3−4265.8)/2211.3 | −92.9% | 退步，不能交付为性能优化成功 |

AOT 同批 HEAD 592.9 ms、当前 623.0 ms，配对 +22.1 ms，3/10 轮更快；不能再拿旧的跨时段 6.2.5 数据判断当前。
相对 HEAD 的严格不退步门禁仍未通过；相对 6.2.5 的原门禁仍缺同批产物证据。
harness：plain 首帧 460.5 ms、atom-splash 的 Splash 首帧 571.8 ms，571.8 ≤ 460.5+150，**验收线 2 通过**，余量 38.7 ms。

### Splash 为何需要 800 多毫秒

真实 Gallery：Main → Splash 首帧 808.3 ms；逐轮加上启动到 Main 的耗时后，进程启动 → Splash 首帧中位 878.2 ms
（788.9–1102.6 ms）。配置回调 152.7 ms；后台全局 Token Prepare 75.2 ms，但 PrepareWait 为 0，已被平台启动覆盖。
按每轮 `Committed − Commit − Splash` 反推，Splash 阶段前约 443.7 ms；其中除配置之外约 283.2 ms，包含平台初始化、
应用创建、JIT 等，现有探针不足以逐项归因。Splash 阶段入口 → 首帧约 367.8 ms，含创建原生窗体、默认主题、Logo、布局与首帧。
Phase.Splash 399.6 ms 还含首帧完成后的主线程恢复开销。完整 Commit 294.9 ms 在首帧之后，不是这 808 ms 的原因。
上述反推是近似区间，独立中位数不可强行相加成精确总数。最小 harness 首帧 571.8 ms，说明前置 Gallery 配置和首次建窗成本都需关注。

### 接续验收状态

Gallery 默认开关尚未得到新的选择，保留 Auto；用户追问的是时长与原因，不视为同意关闭。
步骤 2 仍需单独确认，未开始、未提交、未向外部服务发送消息。性能门禁未过与 Windows/Linux/Rider 手动验证缺口继续显式保留。


### 清理后的最终模块验证

本轮仅对修改的模块执行精确测试；其余模块使用上文明确标为历史的证据，不声称全量回归或整套测试全绿。
全部命令带 `--framework net10.0 --no-restore -m:1 /nr:false`，日志保留在
`.artifacts/performance/startup-step1/recovery-validation/`：

| 项目 | 精确 filter | 结果 | 用例执行耗时 |
|---|---|---|---|
| Core | `FullyQualifiedName~AtomUI.Core.Tests` | 364/364 | 1 s |
| Controls | `FullyQualifiedName~AtomUI.Controls.Tests` | 27/27 | 589 ms |
| Generator | `FullyQualifiedName~ApplicationStartupHookGeneratorTests` | 10/10 | 1 s |

这些命令在临时测试删除后执行。Scope 只包含所修改模块；没有擅自运行全量或 change scope。
Browser 最终源码 Release 构建通过，0 warning / 0 error（36.34 s）；本地 WasmAppHost 启动成功并已停止。
浏览器画面与 console 未验证：CUA 自动安全审批拒绝 `http://127.0.0.1:5127` 的 origin 授权，未通过换端口、浏览器或其他工具绕过。
实际宿主另打印了动态分配的地址；未访问该地址，不将本地宿主成功等同于浏览器页面成功。
Rider 预览、Windows、Linux X11/Wayland 仍待人工回收；Prism 按用户原指示只做源码启动兼容分析，没有创建或实跑示例。


### NativeAOT 与清理验收

`pwsh -NoLogo -NoProfile -File controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1
-publishRootPath /tmp/atomui-startup-recovery-aot -runtime osx-arm64 -buildType Release -publishAot true`
退出 0，restore assets 与 NativeAOT output validation 均通过，文件为 arm64 Mach-O。
发布保留 ReactiveUI/ReactiveUI.Avalonia 的 IL2104、IL3053 警告，以及本机 OpenSSL/Brotli dylib 的最低系统版本链接警告；
没有把带警告发布写成零警告、没有顺手更改依赖。

直接运行发布的确切路径，PID 92702 经 ps 核对；审计时已稳定 41 s，stdout/stderr 无输出，之后显式 SIGTERM 停止（exit143）。
CUA 绑定该裸可执行文件的精确路径返回 `Invalid app`，按 Gallery 精确路径规则停止视觉自动化；
没有按名称绑定另一份 Gallery。因此本轮只证明产物可启动且进程稳定，**主界面画面和 Auto 下未显示 Splash 未经本轮视觉确认**。
日志与 SHA-256 保存到 `recovery-validation/`；两个 AOT 临时发布目录均删除，普通无 RID restore 已恢复。

最后删除了没有常驻调用方的 `CreateForTesting`、注入呈现器构造函数与字段；这些是已删除探针的生产侧专用辅助入口。
正常产品的资格判断与呈现器创建路径保留，清理后复验 Core/Controls；生成器既有10次执行证据仍适用。
上述 Browser/AOT 专项构建在删除这些无调用辅助入口之前执行，未冒充清理后的完全相同源码哈希发布认证。
若要求发布级最终准入，仍须补最终产物视觉确认、Browser 授权后的实际页面、Rider 与其他平台检查，以及性能不退步门禁。
