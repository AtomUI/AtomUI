# AtomUI.StartupPerformance

启动基准工具。测量**冷启动进程**从 `Main` 到窗口首帧、渲染批次完成、UI 线程可响应的耗时，用于启动优化的
前后对比与验收线判定。

## 场景

| 场景 | 初始化 | 窗口 | 用途 |
| --- | --- | --- | --- |
| `plain` | 不初始化 AtomUI | Avalonia `Window`（自带最小模板） | Avalonia 侧基准 |
| `atom-plain` | AtomUI 初始化 | Avalonia `Window` | 隔离"AtomUI 初始化成本"与"AtomUI 窗口成本" |
| `atom-window` | AtomUI 初始化 | AtomUI `Window` | 接近真实的 AtomUI 应用启动 |
| `atom-splash` | AtomUI 初始化＋`UseSplash(SplashMode.Always)` | AtomUI `Window` | 测量启动 Splash 首帧与启动分段 |

各场景共用同一套平台与渲染选项（`UseAtomUIPlatformDetect` + `WithAtomUIDefaultOptions`），因此对比时
差异只来自 AtomUI 本身。`--platform-defaults-off` 可去掉 `WithAtomUIDefaultOptions`，得到"发行版默认
配置"的参考行；注意 macOS 上这会改动渲染后端（OpenGL ↔ Metal），参考行不与其它场景直接比较。

## 用法

```bash
dotnet publish tools/performances/AtomUI.StartupPerformance/AtomUI.StartupPerformance.csproj \
  -c Release -f net10.0 --self-contained false -o /tmp/startup-harness

/tmp/startup-harness/AtomUI.StartupPerformance --scenario atom-window
```

参数：

- `--scenario plain|atom-plain|atom-window|atom-splash`，默认 `atom-plain`。
- `--multicorejit <dir>`：启用 MultiCoreJit，配置文件写在 `<dir>`。首次运行生成配置、第二次起生效。
- `--platform-defaults-off`：不套 AtomUI 的平台/渲染默认（见上）。

工程是多目标（`TargetFrameworks`），发布必须显式 `-f net10.0`。AOT 发布需先带 RID 与
`-p:StartupPublishAot=true -p:StartupPublishTrimmed=true` 执行 `restore`，否则 assets 缺少
`net10.0/<rid>` 目标而报 `NETSDK1047`。

## 输出

stdout 为机器可读的 `PERF|key=value` 行（毫秒，invariant）。除 `Epoch.*` 与 `Flag.*` 外，里程碑键为
`Mark.*`；无对应事件的键不会出现（例如 `plain` 场景没有 `UseAtomUI.SyncTotal`）。

以 `Main` 为零点：

| 键 | 含义 |
| --- | --- |
| `Mark.InitializeEntered` | 进入 `Application.Initialize` |
| `UseAtomUI.SyncTotal` | 配置回调 `Configure(IAtomUIBuilder)` 的同步耗时（仅 AtomUI 场景） |
| `Phase.<name>` | AtomUI 启动分段耗时，来自 `AtomUI-Startup` EventSource（如 `Configure`、`Prepare`、`PrepareWait`、`Splash`、`Commit`、`Commit.*`） |
| `Mark.SplashFirstFrame` | Splash 首个合成批次渲染完成（仅 `atom-splash`） |
| `Mark.MainWindowFirstFrameEvent` | 框架判定的主窗体首帧（Splash 交接时刻） |
| `Mark.State.<state>` | 启动状态进入 `<state>`（`Committed`、`Presenting`、`Ready` 等）的时刻 |
| `Mark.FrameworkCompleted` | 进入 `OnFrameworkInitializationCompleted` |
| `Mark.WindowConstructed` | 主窗体构造完成 |
| `Mark.WindowOpened` | 主窗体 `Opened` |
| `Mark.FirstRender` | 根哨兵首次 `Render` |
| `Mark.FirstRenderedBatch` | 首帧所在合成批次在渲染线程完成 |
| `Mark.FirstUiPump` | UI 线程首次开始服务消息（"界面开始有反应"） |
| `Mark.MaxUiGap` | UI 线程开始响应之后的最长阻塞 |
| `Mark.DispatcherIdle` | 到达 `ApplicationIdle` |

`Epoch.MainUnixMicroseconds` 是 `Main` 的墙钟时刻，采样脚本据此换算自"进程启动"起算的 `Launch.*` 指标。

### UI 响应性口径

`Mark.FirstUiPump` 与 `Mark.MaxUiGap` 由独立后台线程以连续探针（`DispatcherPriority.Send`）测得。
**不要用 `DispatcherTimer` 测启动期卡顿**：它由平台 runloop 服务，macOS 上主循环开始前几乎不触发，
实测 176–330 ms 的测量窗口只 tick 1 次，会得出恒为 0 的假结果。

首个探针被服务的时刻记为 `Mark.FirstUiPump`——在它之前 UI 线程根本没泵消息（主循环尚未开始），
属于"界面还没反应"，不计入阻塞；此后的最长等待才记为 `Mark.MaxUiGap`。

`Diag.UiGapProbes` 是探针服务次数，用于判断采样是否充分：`< 20` 说明测量窗口过短，该次 `Mark.MaxUiGap`
不可信。`Diag.UiGapWindowMs` 是测量窗口长度。

## 采样脚本

单次运行只作冒烟。正式对比用 `run_samples.py` 交错运行独立进程（奇数轮正序、偶数轮倒序），
输出 `samples.json`、`summary.json`、`summary.md`。

```bash
python3 tools/performances/AtomUI.StartupPerformance/run_samples.py \
  --config variants.json --output /tmp/startup-samples --rounds 10 --warmup 1 --timeout 90
```

配置格式：

```json
{
  "label": "harness-atom-window-jit-release",
  "context": {"machine": "macOS arm64"},
  "variants": [
    {"name": "before", "command": ["/abs/AtomUI.StartupPerformance", "--scenario", "atom-window"],
     "env": {}, "hash_paths": ["/abs/AtomUI.StartupPerformance", "/abs/AtomUI.Core.dll"],
     "required_metrics": ["Launch.ToFirstRenderedBatch"]},
    {"name": "after", "command": ["/abs/AtomUI.StartupPerformance", "--scenario", "atom-window"],
     "env": {}, "hash_paths": ["/abs/AtomUI.StartupPerformance", "/abs/AtomUI.Core.dll"],
     "required_metrics": ["Launch.ToFirstRenderedBatch", "Phase.Commit"]}
  ],
  "compare": ["before", "after"]
}
```

口径规则：

- 每个变体至少 10 轮；少于 10 轮必须加 `--smoke`，且结果只能作为冒烟，不能作为优化结论。
- 正式采样的每个变体必须列出 `required_metrics`；缺少任一指标的该次进程记为失败，退出码为 1。
  比较两侧同一指标时，两侧都列入必需项，保证配对数不少于 10；基线没有的新指标仅列在候选侧。
- 每个变体先预热（默认 1 次），预热样本不计入统计。
- 保留全部样本，不剔除离群值；失败样本计入 `failures` 并使退出码为 1。
- 采样期间不得有构建、测试或其它重负载任务。每个样本记录采样前的 1 分钟负载，超过 CPU 核数一半的
  样本列在 `highLoadSamples` 中——这是**提示而非失败**：多核机器上短暂高负载很常见，整组都被标记时，
  判读重点看配对比较的中位差与"候选更快的轮数"，而不是看负载标记是否存在。
- 配对比较按轮次一一对应（`medianDelta` 为"候选 − 基线"），因此不受机器整体快慢漂移影响；这是本工具
  的主判据。

`hash_paths` 中记录被测二进制与其 `AtomUI.Core.dll` 的 SHA-256；若两个变体有同名文件哈希相同，
`warnings` 会提示"可能用错了二进制"。

## 指标口径边界

- 自 `Main` 起算，不含进程启动到 `Main` 之间的运行时初始化（约 30–90 ms，见 `Launch.ToMain`）。
- 不包含 GPU 上屏：`Mark.FirstRenderedBatch` 是合成批次在渲染线程完成，不是显示器的实际呈现。
- 需要真实显示器与窗口系统；headless 环境的数字不可与桌面环境对比。

包诊断输出包括 `Package.<packageId>.<phase>`（毫秒）、`Count.Fragments.<packageId>` 和
`Count.ThemeAssets.<packageId>`。采样器导出 `Launch.ToSplashFirstFrame` 与 `Launch.ToReady`；后者只表示框架
交接完成，Gallery 内容就绪继续使用 `Launch.ToShellFirstBatch` 与视觉检查。
