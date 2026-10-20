# 启动系统验证

本文定义启动系统的测量口径、验收线、诊断事件与测试覆盖。

## 1. 测量工具与口径

- **测量程序**：`tools/performances/AtomUI.StartupPerformance`。场景、参数与输出键见该目录的 `README.md`。
  - `plain`：不初始化 AtomUI 的 Avalonia 空窗口，作为平台基准。
  - `atom-window`：AtomUI 初始化 + AtomUI `Window`，接近真实应用。
  - `atom-splash`：`atom-window` + `UseSplash(SplashMode.Always)`，测量 Splash 首帧与启动分段。
- **采样**：同目录的 `run_samples.py`，正式采样至少 10 轮并加预热，每个变体声明验收所需的 `required_metrics`；缺失指标的进程计为失败。变体交错执行，输出中位数、区间与配对差值。
  交错是在机器存在背景负载时保持组内比较有效的前提；跨时段的绝对值不可直接比较。
- **Gallery 对比**：发布基线提交与候选的 Gallery（jit-release 与 NativeAOT），在两侧 `Main` 中以相同方式临时接入同一
  测量探针，记录主窗体首个渲染批次、完整 Shell 首个渲染批次与主窗体首帧之后的最长 UI 阻塞，交错采样后比较。探针只用于
  测量，不进入仓库。
- **时刻定义**：渲染批次时刻指合成批次在渲染线程完成，不是显示器实际呈现；需要确认上屏时结合录屏或窗口列表。
- **探针初始化边界**：UI 间隔探针在 `AfterPlatformServicesSetup` 中捕获 Dispatcher，再启动探针线程；不得为了测量
  在平台初始化之前访问 Dispatcher，避免探针本身改变动画与定时器的时钟基准。比较两侧必须使用相同的探针启动点。

## 2. 验收线

| 编号 | 判定 | 说明 |
|---|---|---|
| Splash 首帧 | `atom-splash` 的 `Mark.SplashFirstFrame` 中位数 ≤ `plain` 的 `Mark.FirstRenderedBatch` 中位数 + 150 ms | Splash 必须几乎与空窗口一样早出现 |
| 不退步（JIT） | Gallery jit-release 到完整 Shell 首个渲染批次的配对中位差（候选 − 基线）≤ 0 | 对上一个基线提交 |
| 不退步（AOT） | Gallery NativeAOT 到完整 Shell 首个渲染批次不差于上一个正式版本 | 同机、交错 |
| 完整首帧（JIT） | 不超过同机 6.2.5 基线的 75% | 至少 10 次交错冷进程 |
| 完整首帧（AOT） | 不超过同机 6.2.5 基线的 80% | 任一指标不得劣于 6.2.5 |
| 内核 Commit（JIT） | 中位数 ≤ 100 ms | 包钩子计入提交关键路径 |
| 内核 Commit（AOT） | 中位数 ≤ 20 ms | 测量版可显式启用 EventSource，须记录开关 |
| 响应性 | 记录主窗体首帧之后的最长 UI 阻塞 | 不再出现旧分级启动在首帧后的整段冻结 |

未达线时用 `Phase.*` 分段定位，不降低验收线。

调度边界是启动优化的前置验收项：在相同平台后端与依赖下，对比未应用该项优化的基线，确认候选没有提前创建
Dispatcher、改变其线程归属或平台初始化顺序，也没有引入或放大定时器的稳定延迟。若优化越界，调整或撤回该项优化，
不得通过校准时钟或补偿到期时间通过验收。不启用 AtomUI 时仍能复现的调度问题应单独记录，不能混入启动优化的修复范围。

## Gallery JIT / R2R 发行策略

Gallery Desktop 的非 AOT Release 发布默认启用 ReadyToRun；NativeAOT 正式发布路径保持独立。
Gallery 产品入口不自动创建或读取 MultiCoreJit 配置文件。可复用启动基准工具仍支持显式
`--multicorejit` 实验；采样时需区分普通 JIT、显式预热实验与 R2R，不能把预热收益归给普通 JIT 首次运行。

## 主题资产物化门禁

```bash
dotnet run --project tools/validation/AtomUI.ThemeAssetsValidation/AtomUI.ThemeAssetsValidation.csproj -c Release
```

此工具创建启用官方控件包和 Gallery 的真实 Headless 应用，沿当前 owner/资源桥物化选中资产、已知资源键和
延迟导出的主题，并在释放应用 scope 后打印 `[SUMMARY]`。异常或零资产会使进程失败。
`verify-affected.yml` 在测试选择之外单独执行该命令并保存 `theme-assets.log`；不以静态文本或 mock 代替工厂执行。
本门禁不替代原生窗口画面、ControlTemplate 布局或用户交互验证。

## 3. `AtomUI-Startup` 事件

`StartupEventSource`（名称 `AtomUI-Startup`），默认关闭；关闭时不映射枚举或构造事件负载。测量程序用进程内
`EventListener` 订阅；NativeAOT 默认裁掉 EventSource（`EventSourceSupport=false`），AOT 测量依赖测量程序自身打点。

打点入口使用内部枚举：全局分段为 `StartupPhase`，包阶段为 `PackagePhase`，状态沿用 `StartupState`。
调用方直接传枚举，禁止自行拼接阶段名或使用 `ToString()` 生成诊断负载：

```csharp
StartupEventSource.Log.PhaseStart(StartupPhase.CommitInitializers);
// 执行初始化
StartupEventSource.Log.PhaseStop(StartupPhase.CommitInitializers);
```

`[NonEvent]` 包装先检查 `IsEnabled()`，再通过显式 switch 映射固定字符串，交给私有 `[Event]` 方法写出。
事件源名称、事件 Id、事件名、字段名与字符串负载共同构成测量契约。新增枚举成员时须补齐映射；
枚举的 C# 名称与数值不是输出格式，重命名枚举成员时仍保持既有负载字符串。

| 事件 | Id | 负载 | 含义 |
|---|---|---|---|
| `PhaseStart` / `PhaseStop` | 1 / 2 | 分段名 | 分段起止 |
| `StateChanged` | 3 | 状态名 | 进入 `Committed`、`Presenting`、`Ready`、`Failed`、`Canceled` |
| `SplashFirstFrame` | 4 | — | Splash 首个合成批次渲染完成 |
| `MainWindowFirstFrame` | 5 | — | 主窗体首个渲染批次完成（交接时刻） |
| `PackageTiming` | 6 | packageId、阶段、毫秒 | 包的 Enable、Commit、FirstUse 耗时 |
| `FragmentMaterialized` | 7 | packageId、控件 metadata name | 成功物化一个登记片段 |
| `ThemeAssetMaterialized` | 8 | packageId、assetId | 成功物化资产工厂；重试成功只计一次 |

分段名：

| 分段 | 线程 | 内容 |
|---|---|---|
| `Configure` | UI 线程 | 平台服务就绪后执行配置回调；同步回退宿主在 Attach 中执行 |
| `Prepare` | 后台 | 仅全局主题准备 |
| `PrepareWait` | 主线程 | `Attach` 等待后台 Prepare |
| `Splash` | 主线程 | 创建、显示 Splash 并驱动到首帧 |
| `Commit` | 主线程 | 一次性提交整体 |
| `Commit.Localization` / `Commit.Theme` / `Commit.OwnedServices` / `Commit.Initializers` | 主线程 | 提交各段 |

测量程序把分段写为 `Phase.<分段名>`，把状态写为 `Mark.State.<状态>`，把两个首帧事件写为 `Mark.SplashFirstFrame`、
`Mark.MainWindowFirstFrameEvent`。

包数据输出为 `Package.<packageId>.<phase>` 和 `Count.Fragments.<packageId>`、`Count.ThemeAssets.<packageId>`。
计时器和事件 payload 在监听关闭时不创建；关闭路径已用分配探针验证。采样脚本同时换算
`Launch.ToSplashFirstFrame`、`Launch.ToReady` 等自进程启动起算的时刻。`Ready` 表示框架交接状态，Gallery 完整内容
仍以独立的 `ShellFirstBatch` 指标与画面确认，不用较早的状态事件替代。

## 4. 测试覆盖

| 不变量 | 测试 |
|---|---|
| 生成器为每个具体 partial 应用生成挂接；手写重写漏调 `Attach` 报 `ATOMUIGEN039`；抽象、非 partial 不生成；同名应用的生成文件不冲突 | `tests/AtomUI.Generator.Tests/Startup/ApplicationStartupHookGeneratorTests.cs` |
| 默认主题只依赖全局 Token 与 Splash 自有类型；模板只描述结构、外观值在样式中（可被 BasedOn 局部覆盖）；`SplashWindow` 从自带资源应用窗口主题与 Splash 模板 | `tests/AtomUI.Controls.Tests/Splash/SplashDefaultThemeTests.cs` |
| 配置在平台就绪后、Application 构造前执行，每实例一次；允许间接构造 AvaloniaObject，内核在 Initialize 前提交并进入 Ready | `tests/AtomUI.Core.Tests/Startup/StartupContractTests.cs` |

首次 Dispatcher 初始化必须另用真实独立进程验收，不能用已经初始化平台的 headless 会话替代：

```bash
dotnet run --project tools/performances/AtomUI.StartupPerformance -c Release -f net10.0 -- --verify-configuration-boundary
```

该入口验证登记阶段没有执行回调或创建 Dispatcher，模块配置间接构造 `AvaloniaObject` 发生在平台就绪之后，且先于
Application 构造、内核先于 Initialize 提交。原生主循环启动后采样 2 秒的 16 ms DispatcherTimer，输出样本数、
中位数和最大间隔；样本少于 20 或中位数超过 64 ms 时失败。该宽松阈值用于识别稳定调度退化，不代表 64 ms 是
动画流畅度目标，也不替代 Gallery 原场景的视觉复核。按需运行，不增加默认全局测试用例。

宿主侧行为（Splash 先出现、主窗体首帧后隐藏关闭、提交期间进度动画、交接前关闭即退出）依赖真实窗口与合成器，按平台实机检查：
macOS、Windows、Linux X11 与 Wayland 各执行一次，检查 Gallery 与一个 Prism 应用。

## 5. 已知不一致

- 启动交接须验证主窗体首个合成批次完成后 Splash 才隐藏；呈现失败需恢复可见错误态。
- macOS 主窗体首帧前的渲染线程阻塞可能让 Splash 动画停顿，但不得造成 Splash 提前消失后的长时间空档，见 [Splash 已知问题](splash.md#7-已知问题)。
