# 启动重构步骤 2–6：本机验收记录

日期：2026-10-16。工作树：`release/6.0`，基点 `60dd72afb`，含本轮未提交改动；6.2.5 基线为
`f0e9b4133` 的独立 worktree。原始样本、汇总与日志保存在工作区
`.artifacts/verification/startup-final/`，没有删除离群点。本文是本机 macOS arm64 的测量记录，
不替代 Windows/Linux 或 Browser 画面验收。

## 测量身份与口径

- Gallery 基线与候选使用字节一致的临时 `StartupProbe`、EventSource 监听器与后台 UI 间隔探针；
  候选的测量构建额外支持环境变量切换 Splash、手动 MCJ，NativeAOT 测量构建在 Commit 边界额外输出直接计时。
  这些探针与开关均已从产品源码删除。完整 Shell 首帧以 `Launch.ToShellFirstBatch` 衡量；渲染批次完成不等于物理显示器上屏。
- 每组为每变体 1 次预热、10 个交错冷进程。`run_samples.py` 现在要求正式变体列出关键指标；缺失指标或进程失败会使整组失败。
- 本机曾有明显背景负载。下述 JIT 组的高负载标记分别为 49/50、30/30、40/40，AOT 为 20/20；
  因此跨组绝对值不能直接比较，同轮配对差与快于基线的轮数是主要依据。
- JIT Gallery 的测量版托管入口 SHA-256：6.2.5
  `b1e0f0c380e4f68e552867e1926204d6f0836705596c0a461475ddf935b47b03`；本轮普通 JIT
  `f4d577600eca72f4fd52203ba5be379fe41d2f2b871277fed42bf80fee488f47`。AOT 原生测量入口分别为
  `09246daa6c4279e30e272b1229fc9e31fc9e2daf1744ed4183fdefec182b67c5` 与
  `7a80c9cbba70491880d0a22dcb9043ff449f5576e0a0cbe9e1dfef2ec0bc6e5e`。

## 冷启动结果

| 组别与形态 | 基线完整 Shell 首帧 | 候选中位 | 同轮配对中位差 | 更快轮数 | Commit 中位 |
|---|---:|---:|---:|---:|---:|
| JIT 首组、默认 Splash | 2279 ms | 2757 ms | +477 ms | 1/10 | 121 ms |
| JIT 首组、关 Splash | 2279 ms | 1601 ms | −682 ms | 10/10 | 135 ms |
| JIT 第二组、默认 Splash | 2058 ms | 1583 ms | −491 ms | 10/10 | 114 ms |
| JIT 第二组、关 Splash | 2058 ms | 1414 ms | −644 ms | 10/10 | 123 ms |
| JIT 第三组、默认 Splash | 2056 ms | 1568 ms | −496 ms | 10/10 | 115 ms |
| 第三组、非单文件 R2R＋Splash | 2056 ms | 942 ms | −1112 ms | 10/10 | 55 ms |
| 第三组、非单文件 MCJ 第二次起＋Splash | 2056 ms | 1246 ms | −825 ms | 10/10 | 56 ms |
| 第四组、裁剪单文件 R2R＋共享 TypeMap 缓存 | 2064 ms | 724 ms | −1338 ms | 10/10 | 39 ms |
| NativeAOT（Auto 不显示 Splash） | 544 ms | 384 ms | −160 ms | 10/10 | 13.2 ms |
| 缓存变更后 NativeAOT 复测 | 547 ms | 385 ms | −162 ms | 10/10 | 13.2 ms |

首组 Splash 交接期出现约 1.04 秒的 UI 卡顿，9/10 次慢；其后两组主窗体首帧到 Ready 的间隔约 170 ms，
MaxUiGap 中位数分别约 56–57 ms。诊断构建将慢时的额外耗时定位到 `SplashWindow.Close()` 的同步调用：
直接关闭约 317 ms、先 Hide 再关闭约 328 ms，停止淡出或进度动画也未消除；仅 Hide 约 1 ms，
但留下未关闭窗口，不符合生命周期契约。后续同一关闭调用也观察到约 11 ms，说明此 macOS 路径受宿主/合成器状态影响。
保留两类原始样本；未用删除 Splash、留下隐藏窗口或延后主窗体内容伪造收益。

同一非单文件 R2R 二进制的 MCJ A/B：关闭时完整首帧 986 ms、Commit 55 ms；已生成配置后启用时分别为
725 ms、23 ms，10/10 轮更快。JIT 独立进程首次运行生成约 200 KB 配置文件，第二次冒烟 Commit
从约 133 ms 降到 49 ms；首次运行不计作 MCJ 收益。真实的**裁剪单文件** R2R 包另测为 881 ms（MCJ 关）
与 866 ms（开），差约 17 ms、仅 6/10 轮更快，生成文件只有 64 B。因此当时单文件发布不启动 MCJ，
只在非单文件 JIT/R2R 中启用。

交付清理时已移除 Gallery 产品入口的 MultiCoreJit profile 启动代码；上述数据保留为当时的实验结果，
当前 Gallery 不再自动启用 MCJ。可复用启动基准仍可通过 `--multicorejit` 显式测量该方案。

裁剪单文件 R2R 的首轮 TypeMap 预热试验在同轮 10 次冷进程中得到 Shell 首帧 844 ms，
比 6.2.5 基线的 2058 ms 快 1215 ms（10/10）；但 Commit 仍为 138 ms、Theme 段为 123 ms，
未达到 100 ms 线。临时诊断确认 8 个包都进入后台预热，而每包调用仅约 0.001–0.554 ms。
原因是官方 TypeMap 入口每次返回新映射，原预热结果没有被提交路径复用；这批样本记录为未完成优化的诊断对照，
保存在 `.artifacts/verification/startup-final/r2r-warm-no-cache-samples/`。随后生成器保留后端要求的原始 accessor，
另加只在成功后发布、失败可重试的共享缓存。相同三变体交错 10 轮中，仅预热的 Commit 为 131 ms，
缓存后为 39 ms，Theme 提交为 116→23 ms，完整 Shell 首帧为 839→724 ms；这三项同轮差值中位分别为
−92、−92、−117 ms，全部 10/10 轮更快。候选 Splash 首帧中位 292 ms、单次启动最长 UI 阻塞的中位数 18 ms。
原始样本在 `.artifacts/verification/startup-final/r2r-cached-samples/`，高负载标记 30/30，未删样本。

## NativeAOT 阶段与 Ready 补采

NativeAOT 正式产物默认关闭 EventSource，因此另用**临时直接探针**记录同一批阶段、状态、包和物化事件；
Gallery 窗口渲染探针与原 AOT 测量相同。每变体预热 1 次后，与只含 Commit 直接计时的同版本 AOT 产物交错
10 次冷进程，22 项必需指标全部齐备、零失败、零剔除。直接探针产物的绝对 Shell 首帧中位 591 ms，
同轮最小探针产物 586 ms，配对差中位 +28 ms（3/10 更快）；本轮高负载标记 20/20。
这与前述另一时段的 385 ms 不能直接比较，也不能把配对差全部归因于探针。

| NativeAOT 节点，自进程启动起 | 中位数 |
|---|---:|
| 进入 `Main` | 24.6 ms |
| AtomUI Commit / Avalonia Setup 完成 | 160.3 ms |
| 主窗口打开 | 267.4 ms |
| UI 首次处理消息 / 进入 Presenting | 342.5 / 342.6 ms |
| 主窗口首个渲染批次 | 357.3 ms |
| 进入框架 Ready | 371.7 ms |
| Gallery Shell 挂载 | 564.4 ms |
| 完整 Shell 首个渲染批次 | 591.3 ms |

| NativeAOT 阶段自身耗时 | 中位数 |
|---|---:|
| 配置 `Configure` / 后台准备 `Prepare` / 等待 `PrepareWait` | 2.1 / 5.2 / 0.002 ms |
| Commit 总计 | 13.8 ms |
| 本地化 / 主题 / 自有服务 / 初始化器 | 0.27 / 12.23 / 0.05 / 1.19 ms |
| 八包 Enable 合计 / Commit 钩子合计 | 1.85 / 1.19 ms |
| 单次启动最长 UI 阻塞的中位数 | 37.9 ms |

`Ready` 是框架交接状态，此轮比完整 Shell 渲染早约 221.5 ms，不能作为 Gallery 内容可见的替代指标。
主题耗时内含自有服务阶段，各阶段与包耗时也有嵌套，不能逐行相加。首屏按需物化 37 个控件片段和
37 个主题资产。原始样本、哈希和指标全集保存在 `.artifacts/verification/startup-final/aot-direct-phases-samples/`；
临时直接探针已从产品源码逐字节恢复。随后重新发布的无探针 NativeAOT 程序在显式设置
`ATOMUI_STARTUP_PROBE=1` 时仍保持运行 5 秒，且没有输出任何 `PERF|` 测量记录，确认最终产物未携带探针。

## 方案门槛

- **Splash 首帧通过**：同一 harness 10 轮，纯 Avalonia 首批中位 396 ms，AtomUI Splash 首帧 489 ms；
  同轮差中位 92 ms，全部低于允许的 150 ms。
- **AOT 完整首帧与 Commit 通过**：384/544＝70.7%，低于 80% 上限；Commit 13.2 ms，低于 20 ms；
  MaxUiGap 由 35 ms 降到 19 ms。共享 TypeMap 缓存后的独立 10 轮复测为 385/547＝70.4%，
  Commit 13.2 ms，MaxUiGap 中位数 37→22 ms，10/10 轮快于基线；原始样本在
  `.artifacts/verification/startup-final/aot-cached-samples/`。
- **普通未预热 JIT 的严格门槛未达**：默认 Splash 在后两组为基线的 76.9%/76.2%，
  距 75% 线分别约 39/26 ms；Commit 114–115 ms，高于 100 ms。首组因平台关闭卡顿甚至退步。
  非单文件 R2R/已生成配置的 MCJ 可跨过完整首帧线，且 Commit 55–56 ms；裁剪单文件 Release R2R
  完整首帧为 6.2.5 的 35.1%，Commit 39 ms，也通过发行路径门槛。正式发行默认 NativeAOT，非 AOT
  Release 默认 R2R。这里没有修改或降低原门槛。
- **HEAD 就绪对照有证据缺口**：尝试以相同临时探针重建 `60dd72afb`，主窗体首批出现后该旧分级启动
  持续处于 `WaitingForHost`，只有加载占位，8–25 秒未出现完整 Shell。两种探针调度优先级均复现，
  因此没有用超时样本冒充 HEAD 速度；历史步骤 0 数据仅供方向参考。

## 运行与裁剪门禁

- 清理后的 Core 330、Generator 注册/启动 124、Gallery 565、Controls Splash 3、Localization 127，
  Desktop ListView/Upload/Window 541 项定向测试通过。Gallery 旧的 `LanguageTag.Parse` 静态断言先失败，
  已改为检查实际数据化模块清单、`AddModuleData` 注册和语言标签，精确用例及 Gallery 全模块随后通过。
  net8 Core/Localization Release 构建 0 警告、0 错误。`scripts/verification/test.py audit`、`git diff --check` 通过。
- 真正的 Headless 应用把官方与 Gallery 五个包的 **370 个**主题资产逐一调用工厂、查询导出键并释放 scope；
  CI workflow 独立执行此门禁。门禁先揭出了 ListView、Upload 两个类型静态初始化环，现已修复。
- 最终**无测量探针**的 Browser Release publish 产出 `dotnet.native.*.wasm` 与 Gallery/AtomUI 程序集；
  macOS NativeAOT 完成恢复、裁剪、原生链接和产物检查，真实进程保持运行 5 秒；从 AOT 切回
  单文件 R2R 后重新发布成功，该干净程序也保持运行 5 秒。首次 AOT 运行曾因生成器调用 TypeMap 的
  `ContainsKey` 崩溃；改用受支持的 `TryGetValue` 后，重发可执行文件通过完整 Shell 探针。
- Gallery 非 AOT 单文件 R2R 的模式切换曾混入上次裁剪 `linked/R2R` 中间文件，使 ReactiveUI 接口
  调用方遇到 `MissingMethodException`。发布脚本现只清理当前应用 RID 的这两处生成目录，清理失败即停止；
  重新发布的单文件程序已实际运行。Toolchain 仍保持 portable net10.0；独立非单文件发布的 PE 头确认
  `AtomUI.Core.dll` 与 Gallery Desktop DLL 有 `RTR\0` 原生头，普通 JIT 无原生头。

Browser 画面/交互、Windows 与 Linux 桌面、Prism 多平台实机仍需在对应宿主验证；本机的自动浏览器接入曾被
自动审批拒绝，不能借换端口或其它浏览器绕过并宣称已验证。没有运行未经请求的全量回归，也没有创建 commit。
