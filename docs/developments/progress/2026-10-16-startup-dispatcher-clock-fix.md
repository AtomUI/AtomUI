# 启动 Dispatcher 时钟修复与源码优化验证

日期：2026-10-16。交付分支：`release/6.0`，工作树基点 `0b7eae1f6`；引入点由用户逐提交复核为 `c1b950e8e`。

本文中的 .artifacts 统计路径是取证时的位置。后续用户清理构建目录时这些原始统计文件已一并删除；
本文保留当时的数值、样本口径与二进制 SHA，不再将这些路径视为现存附件。

## 原因与修复

`UseAtomUI` 曾在窗口平台初始化之前访问 `Dispatcher.UIThread`，使 Dispatcher 的内部计时起点早于随后建立的
平台 Dispatcher。当前依赖 Avalonia 12.1.3 的本地源码（`/Users/chinboy/Projects/ReferenceProjects/Avalonia`，
`8eeda4f6f5`）表明：Dispatcher 在构造时固定自己的 `_timeProvider`；替换平台实现后，DispatcherTimer 仍使用
原时钟生成绝对到期时间，平台 `UpdateTimer` 却减去自己的 `Now`。每次走这条定时器路径都会额外等待起点差。
MediaContext 在没有待提交合成批次时使用 16 ms DispatcherTimer 继续驱动动画；合成批次完成也能驱动后续帧，
不能把该问题描述成所有合成帧固定增加同样延迟。

修复保留提前执行的配置回调和不可变输入捕获，在 `AfterPlatformServicesSetup` 中才启动后台 Prepare。
平台先建立 UI Dispatcher，Prepare 仍与 Application 创建并行，Attach 继续复用准备结果。工厂启动后清空，
不增加运行期逐帧工作，不引入反射或新的公开 API。

启动测量工具也移除平台初始化前的 Dispatcher 捕获。`Mark.FirstUiPump` 在首个探针回调中按 Main 时间轴记录，
探针窗口长度仍按探针自己的启动点计算，避免推迟探针后漏算平台初始化耗时。

## 原生失败／通过证据

临时 macOS 原生应用引用当前源码，在 `UseAtomUI` 与平台 Setup 之间固定等待 200 ms，再执行真实 16 ms
DispatcherTimer（8 次）及三个连续的 200 ms 透明度动画。相同程序、相同断言，修复前退出 1，修复后退出 0。
以下稳态中位数固定使用最后 6 次；前两次启动样本也完整列出，不作隐藏剔除。

| 指标 | 修复前 | 修复后 | 公式 | 改善 | 结论 |
|---|---:|---:|---|---:|---|
| 16 ms 定时器稳态间隔中位数 | 971.94 ms | 16.09 ms | (前−后)/前 | 98.3% | 恢复预期计时周期；不是界面 FPS 提升比例 |
| Gallery 完整 Shell 首批中位数 | 1753.25 ms | 1787.34 ms | (前−后)/前 | −1.94% | 启动略慢；同轮配对差中位数为 +49.07 ms |

定时器全部样本（ms）：

- 修复前：38.58、117.93、973.77、972.02、971.38、971.97、970.50、971.90。
- 修复后：64.71、57.78、16.56、15.57、16.20、16.24、15.97、15.37。

透明度属性更新次数分别为修复前 15/14/15、修复后 16/15/16。此独立动画在两侧均有更新，不能将定时器结果
推导成所有动画类型的帧率对比。Windows/Linux 共用的启动入口与修复路径已审查，本轮没有冒充这两个平台的实机证据。

## 启动性能

真实 Gallery 普通 JIT Release、未裁剪、未启用 R2R；两侧使用同一 Gallery 程序和平台就绪后启动的相同探针，
仅替换 AtomUI.Core.dll。每侧预热 1 次，正式各 10 个冷进程，奇偶轮倒序交错，20/20 成功，无剔除。
所有样本均被工具标记背景负载偏高，因此主要依据配对差，不把单次冒烟或跨时段数值当作收益。

- 完整 Shell 首批：配对中位差 +49.07 ms，修复版 2/10 轮更快；约为基线中位数的 2.8%。
- PrepareWait：0.04 → 66.55 ms；Prepare 自身：82.53 → 77.59 ms。新增等待主要来自失去与平台初始化的重叠。
- Commit：134.22 → 136.30 ms；配对差中位数 +1.69 ms。
- 原始统计保存在 `.artifacts/verification/startup-clock-gallery/{samples.json,summary.json,summary.md}`。
- 采样配置中的 baseline 字符串沿用了引入提交 `c1b950e8e`；实际两侧二进制都含后续首帧交接实现
  `HideAsync`/`AwaitCompositionAsync`，与当前 `release/6.0` 源码一致。相同 Gallery DLL 与不同 Core DLL 均已核对。
- Core SHA-256：基线 `752431108d90491108d7d1a067ff6095372aa209dfdf74a357a05716f9567678`；
  修复版 `d80b73e9567e0b55c5186dcf0579397048d1a38f4757a497e6b08d6468c90ab5`。

本次修复每个启动配置新增一个一次性平台回调、一个待启动工厂字段和一个内部方法；后台 Task 与计算输入继续复用，
没有新增运行期订阅、定时器或逐帧分支。接受该少量启动延迟以恢复正确动画调度；没有宣称零启动代价。

## 验证与清理

- 临时边界用例 2/2 通过：后台 Prepare 在平台服务就绪后、Application 构造期间启动；准备失败回退；
  同一 AppBuilder 创建两个独立应用实例。
- 临时测试、Gallery 测量插桩、原生复现工程与两侧临时发布应用均已删除，Gallery Program 已逐字节恢复。
  正式测试集合净新增为 0；只有统计与本记录保留，不保留可执行临时项目。
- 清理后 Startup、ThemeStartup、ApplicationLocalizationStartup、ImageLoadingApplicationBuilder 精确定向验证
  17/17 通过，零失败、零跳过；Core net8 Release 构建零警告、零错误。
- 当前 `release/6.0` 的无插桩 Gallery Release 构建零警告、零错误。启动基准工具构建成功，保留既有的
  `PlainHarnessApplication` 非 partial 对应 `ATOMUIREG015` 警告（该 plain 场景不启用 AtomUI）。
- 只读代码审查通过；未创建 git commit。

## 用户确认后的全局源码复查

本轮以用户已确认的时钟修复版为基线，复查 Startup 编排、Splash、包清单/惰性登记、主题快照与资源查询、
生成式 TypeMap 选择和启动测量入口。仅保留两项有量化收益的算法改动：

- 资产候选由已选中登记项一次建立反向索引，键包含所属包和资产 ID，保持包隔离、候选顺序与重复 membership 的旧语义。
  追加包仍复用旧 entry/asset；不在索引阶段激活片段或资源工厂。
- Type 身份键与资源键以 `ConditionalWeakTable<Type, ...>` 复用，缓存值仅保存两条字符串。
  不缓存任意资源名称，不强引用程序集或应用；临时可卸载程序集回收验证通过。

清理了 Splash 隐藏路径上必然重复的 ReferenceEquals 判断、未使用的 StartupProcess.IsTerminal、Gallery 窗口中的
旧调度 using，以及正式文档中遗留的“交接淡出”和旧 Prepare 启动点表述。保留主窗体首帧后的隐藏/延后关闭顺序。

### 局部成本

使用真实 Gallery 的 7 个包、209 个 Token Control、370 个资产；预加载不可变清单，10 次预热、每轮 80 次索引构造，
正式 7 轮取中位数。类型键每轮 100,000 组查询，正式 7 轮。数据是这些操作的成本，不代表整窗或页面加载改善比例。

| 指标 | 本轮优化前 | 本轮优化后 | 公式 | 改善 | 结论 |
|---|---:|---:|---|---:|---|
| 每次包索引构造 | 2.3665 ms | 0.7565 ms | (前−后)/前 | 68.0% | 避免按每个资产重复遍历全部登记项 |
| 每次包索引分配 | 3,808,701 B | 628,792 B | (前−后)/前 | 83.5% | 减少临时遍历对象与候选数组 |
| 每组热类型身份/资源键查询 | 815.56 ns | 14.34 ns | (前−后)/前 | 98.2% | 不再重复读取程序集名称和构造字符串 |
| 每组热类型身份/资源键查询分配 | 1256.1 B | 0 B | (前−后)/前 | 100% | 预热后零分配 |

10,000 组热键查询分配预算在修改前因 12,561,024 B 明确失败，修改后为 0 B 并通过。
临时验证另覆盖候选优先级/回退、重复关联、未选中片段、包范围隔离、追加资产复用、可卸载 Type 的 GC，以及
10,000 次并发类型键读取。全部通过后删除临时项目，热键零分配断言合入既有资源查询用例，常驻用例数量净新增为 0。
实现增加一个候选关系 helper 与一个只存字符串的私有弱缓存值类型；未引入新的状态机、运行期定时器或公共 API。

### 真实 Gallery 冷启动

同一外部临时宿主调用真实 Gallery 的 BuildAvaloniaApp，产品源码无测量插桩。两侧 Gallery、宿主和依赖字节一致，
仅替换 Core；都包含已确认的时钟修复。普通 JIT Release，每侧 1 次预热 + 10 次交错独立进程，20/20 成功、零剔除。
20 个样本均有高背景负载标记，以同轮配对差为主。没有用上一组绝对值推导本轮收益。

- 完整 Shell 首批中位数：1741.84 → 1731.77 ms；配对中位差 −30.06 ms，8/10 轮更快。
- Commit 中位数：140.48 → 127.34 ms；配对中位差 −14.80 ms，10/10 轮更快。
- Splash 首帧配对差中位数 −3.93 ms，6/10 轮更快；PrepareWait 基本持平。
- 原始统计：`.artifacts/verification/startup-source-review-gallery/`；局部成本：
  `.artifacts/verification/startup-source-review/{before,after}-microbench.json`。
- Core SHA-256：本轮基线 `e1343847bfb6b6dde35b52787668ba583f5f68ac792641be340c6f20bd14bcd3`；
  优化版 `a817b61931b1321d1932951778f2746e062ae633cc1ecedc460319a84e006a23`。

临时 benchmark、功能/GC/并发 fixture、外部 Gallery 宿主及前后发布副本均已删除。仓库无 Temporary 目录、
无产品测量开关和新增运行时反射；保留正式启动测量工具和最终数值记录。未运行全量回归，未创建 commit。

### 最终清理后的验收

- Core 启动、主题启动、本地化启动、图像服务接入、登记运行时与资源查询性能：43/43 通过。
- Desktop TypedPackageEntryTests：6/6 通过。原有 5 项断言仍依赖旧 ControlPackages/Initializers 集合，
  已迁到现行 manifest 与依赖包契约；保留重复声明不变性、共享不可变元数据、应用状态隔离与冻结检查。
  同时验证桌面运行时服务在声明阶段不创建、Commit 后实际替换，应用 scope 在测试结束时处置。
- Controls SplashDefaultThemeTests：3/3 通过。上述定向验证共 52 项，零失败、零跳过，常驻用例数量净新增为 0。
- 真实 Gallery Desktop Release 构建与 Core net8.0 Release 构建：均零警告、零错误。
- 复核并同步启动指南中 Prism 的 Splash 交接描述，以及 Splash 错误态恢复可见性的描述。
- 只读审查未发现运行时阻断项；审查指出的 Commit 回调验证缺口已在既有用例中补齐并通过。
- git diff --check 通过；分支仍为 release/6.0。临时项目、探针、发布副本与专用临时配置再次核查均不存在。

启动样本有高背景负载，真实窗口实测仅覆盖 macOS。本轮没有执行 NativeAOT 发布或 Windows/Linux 实机测量，
不以普通 JIT 构建和局部 benchmark 代替这些平台的性能结论。

## 启动类型命名整理

根据用户对 Startup 目录命名的反馈，类型与文件名统一去掉 AtomUI 前缀，包括 StartupCommit、StartupRun、
StartupProcess、StartupConfiguration、StartupEventSource、StartupProgress、MainWindowContext、DispatcherPump
和 ISplashPresenter。公开入口使用 ApplicationStartup，避免与既有 AtomUI.Startup.Data 命名空间冲突；
观察接口同步简化为 IStartup、StartupState 与 StartupProgressEventArgs。

生成器的 metadata 名、Attach 发射代码、手写挂接识别和诊断提示同步更新；产品、测试、fixture、工具和当前文档
的引用均已检查。历史设计与进度快照保留当时的名称。AtomUI-Startup 的 EventSource provider 名及事件契约保持一致。
未发布 API 的名称与重新构建要求已同步到中英文启动迁移文档。

- 先更新既有生成器用例中的 API 声明与调用：旧生成器因仍查找旧类型而 5 项失败、5 项通过。
- 完成重命名后，最终生成器 10/10、Core 启动相关 43/43、Controls Splash 3/3、Desktop 包入口 6/6 通过，
  合计 62 项，零失败、零跳过。复用既有用例，无新增临时或常驻测试。
- 最终 Gallery Desktop Release 构建通过，零警告、零错误；git diff --check 通过，仍在 release/6.0，未提交。
- 只读审查确认没有额外行为变化，前序时钟修复保留。本节仅整理名称；上文性能数据对应此前记录的测量二进制。

## 启动诊断阶段的类型约束

按用户确认的方案添加内部 StartupPhase（9 个阶段）与 PackagePhase（3 个阶段），状态继续使用 StartupState。
所有 PhaseStart/PhaseStop、PackageTiming 和 StateChanged 调用改传枚举。StartupEventSource 的 NonEvent 包装
先检查 IsEnabled，再由显式 switch 转换为固定字符串，私有 Event 方法保留既有 provider、事件 Id/名称、字段名及负载类型。
未新增公开 API 或动态调用；原有启动顺序与 Dispatcher 初始化边界保持一致。正式标准记入启动验证文档第 3 节。

本次是保持外部事件契约的重构，使用真实 EventListener 做前后特征验证：

- 改造前临时验证 2/2 通过；改造后同一组外部断言及关闭路径映射检查 3/3 通过。
- 验证 9 组启动阶段起止、6 个状态、3 个包阶段及其余 4 类事件，共 31 条实际事件。
  名称、Id、字段名、负载值与类型均保留，EventSource 没有构造异常或额外错误事件。
- 关闭监听的 27 个打点组成一轮，预热后执行 10,000 轮共 270,000 次调用，分配为 0 B。
  关闭路径传入未定义枚举值也不执行映射，验证开关判断在转换之前。
- 专用临时测试文件及目录已删除，常驻测试数量净新增为 0；清理后的 Core 定向验证 43/43 通过，测试执行 603 ms。
- 清理后的 Core net8.0 Release 构建通过（8.90 s），真实 Gallery Desktop Release 构建通过（85.35 s），
  均零警告、零错误；git diff --check 通过。仍在 release/6.0，未创建 commit。
- 只读审查未发现阻断项，确认映射完整、私有 writer 静态可达且没有额外运行时反射依赖。

## Debug 启动挂接产物不一致

用户报告 Gallery 的 VerifyAttached 抛出未调用 ApplicationStartup.Attach。通过当前仓库的绝对 Debug 路径复现，
进程 56064 以 134 退出，异常与用户提供的堆栈一致。GalleryApplication 源码已经声明为 partial。

重建前的 Debug 生成器仍包含 AtomUI.AtomUIStartup 查找字符串（产物时间 18:05），Core 已只提供 ApplicationStartup。
生成器的 hasStartup 检查因此为 false，跳过挂接生成；该 Debug Gallery 程序集中没有 RegisterServices 或
IGeneratedStartupImageHost，而同一工作树的 Release Gallery 中存在两者。根因是重命名后的运行时与旧 Debug 生成器
产物混用，没有证据表明现行生成逻辑漏掉了 Gallery 的 partial 声明。

首次重建期间发生并行 IDE 清理，obj 目录与已生成的 DLL 被移除。用户暂停 IDE 构建后，重新 restore 并从启动脚本
完整构建 Debug 依赖链；新生成器已确认只查找 AtomUI.ApplicationStartup。本轮未添加手写挂接或运行期补救逻辑。

重建后 Debug 编译成功（61.31 s），零警告、零错误；Gallery 程序集已包含 RegisterServices 和
IGeneratedStartupImageHost。启动器核验的进程 56928 来自同一绝对路径，原生窗口列表显示该 PID 拥有可见的
1300×900、layer 0 窗口，与 WorkspaceWindow 的声明尺寸一致。日志为空，验证进程正常退出，启动器会话退出码为 0，
PID 文件已清空；原始 VerifyAttached 异常不再出现。

CUA 无法按绝对可执行文件路径绑定该应用（Invalid app），因此没有继续视觉/交互自动化；窗口存在性由已核验 PID
的原生窗口元数据确认，没有将其冒充控件画面或交互回归。专用测试工程与插桩净新增为 0，git diff --check 通过，未提交。

## IDE 项目引用的启动语言清单缺失

用户报告 Rider 在 GalleryApplication 上显示 ATOMUIREG014，指向 CommonLangResourceKind/pt-BR 的 bundle。
StartupReferenceReader 原先仅处理 PortableExecutableReference，IDE 使用 CompilationReference 时被引用项目的
包、主题和语言清单被直接跳过，而本地化编译仍能从项目符号解析 Catalog 并生成附加翻译，导致启动语言校验缺少 Catalog。

临时真实 Roslyn 复现将 Catalog 所属模块作为 CompilationReference，保留同一葡语 bundle，修改前精确失败为
“Unresolved or incompatible translation bundle 'AtomUI.Controls:AtomUI.Controls.Localization.CommonLangResourceKind/pt-BR'
from 'AtomUI.Controls.I18n.PtBR'”。修改后直接读取该编译的程序集 Attribute，两种引用形式共用不可变输出与 marker/重复清单
检查。PE 路径继续读取原始 metadata；没有通过 Emit、文件 IO、运行时反射或跳过设计时校验取得结果。

- 临时验证 7/7 通过：原始葡语覆盖、三类清单的 PE/项目引用等价、真实 slot 不兼容继续失败，以及缺少清单、重复清单、
  named arguments、null payload 的校验保持一致。用例执行 920 ms。
- 临时文件和目录已删除，常驻用例净新增为 0。清理后的启动挂接、语言 bootstrap、Catalog 生成与增量缓存定向验证
  30/30 通过，执行 2 s；未运行全量回归。
- 只读审查未发现问题；输出只保留字符串数据，不持有 Compilation 或符号对象。生成器模块文档已补齐两种引用的读取边界。
- 最终 Debug Gallery 构建通过（60.30 s），零警告、零错误；已确认 Debug 生成器产物包含项目引用读取逻辑。
