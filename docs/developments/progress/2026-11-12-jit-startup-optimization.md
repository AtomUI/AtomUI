# JIT 启动优化记录

日期：2026-11-12。分支 `release/6.0`，基点 `d1b7e5590`。macOS arm64，本机采样期间 1 分钟负载约 20–27（其他会话占用），
以同轮配对差为主判据。

## 定位方法

`AtomUI.StartupPerformance` 新增按线程的 JIT 计数（`Jit.Phase.*`、`JitMethods.Phase.*`、`Jit.Thread.*`，取自
`JitInfo`），配合 `DOTNET_JitDisasmSummary` 方法清单与 EventPipe JIT 事件（`MethodJittingStarted` → `MethodLoadVerbose`）
逐方法计时。普通 JIT 下：

- Commit 约 120–135 ms 中 JIT 约 103–117 ms（约 87%），后台 Prepare 约 70–80 ms 中 JIT 约 60–72 ms。
- 单个最大热点是生成的 `GeneratedStandardLanguageDefinitions.TryCreate`：IL 73,141 字节，Tier0 编译约 26 ms，
  启动只调用几次。其次是应用启动镜像类型初始化器（IL 15,269 字节，约 5 ms）与 `Resolve_*` 片段桶（每个约 4 KB IL）。
- 不显示 Splash 时，Attach 在 UI 线程空等后台 Prepare 约 36–48 ms。

## 改动

| 改动 | 文件 | 结构变化 |
|---|---|---|
| 标准语言定义改为一条排序记录常量，手写查找 | `LanguageTagsSourceWriter.cs`、`StandardLanguageDefinitions.cs` | `TryCreate` IL 73,141 → 19 + 219 字节 |
| 不显示 Splash 时，Prepare 等待推迟到 `ApplicationScope.InitializeApplication` 前 | `StartupRun.cs`、`StartupCommit.cs` | 本地化、ThemeManager 构建、自有服务与 Prepare 并行 |
| 片段解析按两位哈希分 256 桶 | `StartupManifestWriter.cs` | 每桶约 4 KB → 70–370 字节 IL；分发方法约 4.5 KB |
| 启动镜像数组改为计数 + 打包字面量 | `StartupImageWriter.cs` | 类型初始化器 IL 15,269 → 84 字节 |

正确性边界：

- 语言记录字段由 TSV 制表符解析产生，生成器另行拒绝含分隔符的字段。全部 1,066 个标准标签与 6 个负例的 `TryCreate`
  结果与旧 switch 逐行一致（临时对拍后删除）。
- 推迟等待只在 Splash 不适用时生效。被推迟的步骤只创建主题来源注册包装、不调用主题来源，也不读取全局层；主题目录加载
  仍在 `ThemeManager.InitializeApplication` 内、等待之后。两线程只共享不可变生成数据与线程安全的 `Lazy`/TypeMap 缓存；
  Theme、Registration、Localization 中没有可变静态状态。Prepare 失败或超时沿用原退化路径；推迟的任务挂接观察故障的
  continuation，提交提前失败也不会留下未观察异常。
- 启动镜像以计数校验解码结果，空字段与空数组可区分。

## 验证

- Generator.Tests 463、Localization.Tests 135（新增表边界 4 例：首条、末条、前缀碰撞、文化名不同于标签）、
  Core.Tests 369、Desktop Buttons/Window/Steps/Startup 565，全部通过。
- 主题资产门禁：物化 370 个真实主题资产并释放 scope。
- `--verify-configuration-boundary` 通过（Timer 中位 16.7 ms，115 样本）。
- Localization 与 Core 全部目标框架 Release 构建无警告。

## 测量

`run_samples.py`，`atom-window`，预热 1，15 轮交错冷进程，零失败；两侧 harness 相同，只有产品代码不同。

| 指标（普通 JIT） | 基线中位 | 候选中位 | 配对中位差 | 更快轮数 |
|---|---:|---:|---:|---:|
| 自进程启动到首个渲染批次 | 1054 ms | 972 ms | −74.5 ms | 12/15 |
| 进入 `Application.Initialize` | 493 ms | 428 ms | −55.3 ms | 12/15 |
| Commit.Localization | 35.7 ms | 12.2 ms | −23.3 ms | 15/15 |
| PrepareWait | 48.4 ms | 7.8 ms | −40.8 ms | 13/15 |
| UI 线程到首次渲染累计 JIT | 573 ms | 530 ms | −34.7 ms | 14/15 |
| 首帧后最长 UI 阻塞 | 68.1 ms | 70.3 ms | −1.4 ms | 8/15 |

Commit.Theme +9.7 ms：无 Splash 时 PrepareWait 嵌套在该分段内（候选中位 7.8 ms），其余为两线程并行竞争；
Prepare +11.9 ms 同为并行竞争。净收益以首帧与 Initialize 为准。

R2R 与 NativeAOT 未在本分支复测。同一改动在 `feature/custom-theme` 上的 R2R 15 轮对比：进入 Initialize −30.3 ms
（12/15），PrepareWait −23.7 ms（15/15），Commit +8.1 ms（并行竞争），无退步。AOT 的 Prepare 约 5 ms、等待本来接近 0，
生成代码形状变化只减少 IL。

## 剩余 JIT 机会（未实施）

- Commit.Theme 仍有约 50 ms JIT（约 800 个方法），主要是 ThemeManager 构建与启动事务本身的代码量。
- 窗口打开阶段 AtomUI 相关 JIT 约 78 ms（EventPipe 计时，含放大）：XAML 主题代码约 22 ms、Desktop 控件约 16 ms、
  生成工厂与 Token 访问器约 12 ms；其余为 Avalonia 自身。
- `ContainsAsset`（12.7 KB IL，约 2 ms）未改：需先确认调用方是否只传入本包资产 ID。
- 启动路径的 LINQ 与值类型泛型实例化约 10–20 ms JIT，逐处替换收益分散。
- AlibabaSans 字体集合注册约 18 ms（JIT），主要是 Skia/Avalonia 字体代码，首帧排版前必需。
