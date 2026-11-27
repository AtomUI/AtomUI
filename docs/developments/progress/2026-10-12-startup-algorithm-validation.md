# 启动算法重构验证记录

状态：本轮实现、定向验证、临时资产清理及独立审查修复已完成；待用户审查，未创建提交。

## 范围与身份

- 输入为 `release/6.0` 的 `4fab9a043` 加实施开始时的未提交分级启动改动；没有重置、提交或改写历史。
- 计划：[启动算法与两阶段初始化优化](../plans/2026-10-12-startup-algorithm-optimization.md)。
- 架构：[分级演化](../../architecture/foundations/staged-startup-evolution.md)、
  [惰性物化](../../architecture/systems/theming/lazy-materialization.md)、
  [本地化运行时](../../architecture/systems/localization/runtime.md)。
- 用户明确批准新增兼容工厂 `ControlThemeResourceRegistration.CreateGenerated`，旧构造函数和生成入口保留。
- 原始 RED/GREEN、输入 patch、源码/产物哈希、独立进程样本位于本计划的
  `.superpowers/sdd/2026-10-12-startup-algorithm-optimization/`；交付前保留简要证据，删除一次性验证源码和产物。

## 已实现机制

1. Stage 2 根据实际包贡献刷新 registry 身份，跨代数字 slot 不再复用其他控件结果。
2. 独立 cell 共享不可变计算输入，切断历史 snapshot 父链；首次物化更新有界缓存计费。
3. 静态资源固定挂载在 Token 层之后的查询优先级，演化只交换表，已加载字典不重挂 owner。
4. 生成器提供完整键证据；索引合并已知候选和未知资产，保留 variant/include/动态键的既有查询语义。
5. 主题与本地化统一校验、CommitCore，再 Publish；终态释放一次性注册闭包和构建输入。
6. 独立判断全局输入是否相同，复用全局 Token；无控件差量的 Shared 查询不物化 Own。
7. 按 Catalog identity 共享未变语言行，单次读取捕获匹配的 registry/revision。
8. 指定宿主的绘制批次及请求的激活控制第二阶段；冻结的纯准备后台执行，Shell 另行调度。

## 有效失败与同条件通过

| 机制 | 修复前的有效失败 | 修复后证据 |
|---|---|---|
| 注册身份 | 空 Stage 1/普通指纹包增补后 revision 不变；插入控件继承旧 slot 红色覆盖 | registry/compiler/registration/cache 51/51 |
| 历史引用 | 100 次变更、32 个缓存项，最早 60 个 snapshot 全被保活 | 两类历史序列均回收；并发首次求值只成功计算一次，失败可重试 |
| 缓存计费 | 物化后 retained bytes 仍为 42,294 | 物化后估值更新；相关集 30/30 |
| 资源所有权 | Token 查询构造未用资产；内层字典无 owner；后续桥实现错误压过应用覆盖 | 动态绑定、解绑、复用 owner、应用优先级相关集 22/22 |
| 统一提交 | ThemeChanged 看不到延迟 Catalog；闭包保活；关闭后排队调用已释放服务 | 初始边界集 31/31 |
| 全局复用 | 仅控件增补仍重算 globals；Shared 查询物化 Own | 相关集 41/41 |
| 语言增量 | slot 前插后旧 CompositeFormat 重解析；当前语言新 Culture/方向未更新 | 增量与覆盖集通过，独立本地化集 48/48 |
| 宿主调度 | 其他窗口 RAF 提前释放目标；关闭后仍完成；单回调占用整个第二阶段 | async 集成 45/45，宿主扩展集 23/23 |
| 自定义元数据 | 自定义 Uri.ToString 被移到 worker | 保留自定义回调 UI 亲和性，相关集 19/19 |
| Shell 调度 | Framework Completed 回调中同步创建完整 Shell | 实际 Desktop Gallery 与现有布局集 7/7 |
| 挂载异常 | 已加载代理的 owner 回调失败后仍保留宿主，无法重试 | 同一失败后重试通过；扩展 Core 集 64/64，含字节限额、并发读取、陈旧候选拒绝 |

上表是各步骤证据，不把相互重叠的数量相加，也不将其称为全量回归。

兼容验证曾尝试新生成器消费发布版 Core 6.2.4，构建失败原因是此前 schema 优化的三个既有 ABI 尚不存在，
不是新增资源键工厂。已在注册架构中明确版本匹配边界；工厂 fallback 的独立测试使用实际当前 Core 程序集移除
该工厂后的消费面，保留全部既有 schema ABI。外部 include fixture 的首次失败是未引用目标程序集，修正 fixture
后保留“可解析外部程序集、未知字典键”的真实条件；这两项不算产品修复的 RED。

## 原生 UX

当前工作区 Release Gallery 曾由正式 launcher 启动为 PID 62177；用户确认“红绿灯位置、macOS 顶部应用名、
窗内主菜单三项均正常”，并提供截图 `codex-clipboard-f07be4b2-ac36-4cd6-bf9c-fcee4c865e6b.png`。
截图显示红绿灯正确内缩以及 Settings / Theme / Language 菜单。顶部应用名依据用户文字确认。
CUA 无法按绝对路径绑定该进程，按本仓库调试规则停止自动视觉操作，没有用名称或其他安装副本替代。
这一证据对应当时记录哈希的 Release 版本；后续逻辑与性能探针产物另有独立身份记录。

## 测量方法

- 三种模式使用同一探针二进制：完整同步；框架同步且 Shell 在宿主就绪后构建；完整 staged。
- 每组 Release JIT、NativeAOT 各 10 个独立进程，轮次交替正反顺序，采样期间无并行 build/test。
- 使用当前工作区精确路径并逐进程核对 PID command；源码、apphost/native binary、DLL/dylib 分别记录 SHA-256。
- OS 文件缓存经过构建和预采样预热，未清空缓存；这不是重启操作系统后的冷盘启动实验。
- `RootRender` 为窗口根 Render 回调；`RootBatchRendered` 为另行请求的 compositor 批次完成；均不等同于显示器 presentation。
- `Activated` 为实际平台事件；所有最终样本都等待它。`StageTwoCompleted` 仅表示框架完成。
- `ShellBatchRendered` 为 Shell 挂载后的根 Render 及后续 compositor 批次；输入指标为 Input 优先级队列标记和 10ms timer 间隔，非硬件输入延迟。
- .NET Stopwatch 与父进程 `mach_absolute_time` 时间基对应后才计算外部 launch 指标；环境中的 Python monotonic 是进程相对值，初步样本未混用该时间基。
- 初步 `jit`/`aot` 样本同步组在激活前退出，仅用于调整探针；正式结论使用 `jit-final`/`aot-final`。
- 稳态缓存与历史释放由定向生命周期测试验证。退出前 `GC.GetTotalMemory(false)` 只是当前托管堆估值，不能称作 retained memory 或峰值。

## 原生测量结果（毫秒）

每格为 mean / median / min–max；每个模式 10 个独立进程，不估算 P95。

| Runtime / mode | Main → root Render | Main → rendered batch | Main → Shell batch |
|---|---:|---:|---:|
| jit-final / sync | 1788.6 / 1773.6 / 1686.1–1999.8 | 1968.7 / 1955.0 / 1854.8–2187.0 | 1968.7 / 1954.9 / 1854.8–2187.0 |
| jit-final / shell | 1430.5 / 1406.3 / 1359.6–1589.4 | 1549.6 / 1517.8 / 1448.4–1781.1 | 2190.7 / 2175.9 / 2081.5–2382.8 |
| jit-final / staged | 1092.0 / 1091.6 / 1032.9–1177.1 | 1194.2 / 1198.4 / 1107.1–1264.6 | 2172.5 / 2152.7 / 2117.9–2274.3 |
| aot-final / sync | 405.1 / 321.5 / 308.4–1126.6 | 454.0 / 371.3 / 360.9–1175.4 | 454.0 / 371.3 / 360.9–1175.4 |
| aot-final / shell | 329.7 / 322.0 / 300.8–432.1 | 385.2 / 359.5 / 336.1–552.9 | 549.7 / 537.1 / 504.3–675.4 |
| aot-final / staged | 305.5 / 290.0 / 267.7–375.5 | 375.4 / 357.4 / 292.3–486.5 | 562.9 / 537.1 / 502.5–669.9 |

| Runtime / mode | Launch → root Render (median) | Launch → rendered batch (median) | Activated (Main, median) | Max input timer gap (median) |
|---|---:|---:|---:|---:|
| jit-final / sync | 1839.2 | 2018.3 | 2025.7 | 271.7 |
| jit-final / shell | 1472.3 | 1582.8 | 1513.6 | 523.2 |
| jit-final / staged | 1153.3 | 1263.8 | 1170.3 | 527.0 |
| aot-final / sync | 371.2 | 419.9 | 418.1 | 140.7 |
| aot-final / shell | 361.5 | 409.1 | 376.4 | 87.3 |
| aot-final / staged | 339.6 | 411.2 | 347.4 | 109.1 |

## 实施取舍与剩余成本

以历史 Main→根 Render 口径复核目标：两阶段 JIT 中位数 1.092s，未达 0.95s；AOT 中位数 0.290s，
本组范围 0.268–0.376s，达到 0.4s。外部 launch→rendered batch 中位数则为 JIT 1.264s / AOT 0.411s，
不能用 Main 口径宣称这一更强指标达标。

JIT 两阶段比仅后置 Shell 的根 Render 提前约 315ms，合成批次提前约 319ms；两组 Shell 完成中位数相近。
AOT 两组根 Render 差约 32ms，但 rendered batch 中位数只差约 2ms，Shell 完成中位数均约 537ms；
这组证据不足以把两阶段视为 AOT 默认更快的方案。完整同步的 AOT Shell 完成中位数约 371ms。
同步 AOT 首个样本明显较慢（Main→根 Render 1.127s、外部 launch→根 Render 2.629s），已保留在 mean/range 中，未剔除。

三组都只执行一次全局 Token 编译；同步/仅后置 Shell 的 Own 编译次数为 14，两阶段为 19；
静态 factory 次数分别为 33/33/32。JIT 的 Shell 创建/布局仍产生约 0.5s 的最大 timer 间隔，不能声称消除了 UI 长任务。

- 保留公开 Workspace/Navigation 构造行为。10 个 Release 独立进程的 VM 构造中位数 53.278ms，范围 50.760–66.482ms；
  导航部分中位数 9.347ms，范围 8.900–12.772ms，分配 34,808 字节。菜单状态和命令需首阶段可用，当前没有据此再增加惰性工厂/释放语义。
- 控件复用先拒绝跨 registry revision，避免 slot/schema 混代；这意味着 Stage 2 可能重新编译部分已使用的 chrome Own Token。
- 全局 schema 等价采用 descriptor 引用和有效输入证据，不增加公开哈希 ABI。
- 缓存首次物化后重新估算有界快照集合，成本 O(缓存项 × 槽数)；共享 cell 保守重复计费。
- 任意 Deferred Action、Resolver、evaluator 和资源工厂不能被安全抢占。后台化仅覆盖已冻结的纯准备，Shell 创建及布局仍有 UI 长任务。
- 未增加未知 key 的负缓存，保持运行期加键可见；开放资产查询仍可能遍历所有未知候选。
- 紧凑完整 schema 表及额外小索引仍为条件实验；不能因计划列出就未经收益证明扩大架构。

## 最终验证与清理

首轮清理后的 focused 验证：Core 119、Localization 48、Generator 97、Desktop 14、Gallery 14、Build Tasks 23 全部通过。
Core/Localization/Desktop/Generator 均真实 pack，net8/net10 可构建；net8 按约定报告 ATOMUIREG009 warning 并完整注册。
四个 NuGet 包布局与发布版 6.2.4 一致，真实 Generator 包的 build-task-isolation 检查通过。

无探针 Gallery NativeAOT 发布通过，精确产物进程存活 8 秒、无异常输出，随后由验证脚本终止自己的 PID。
非 staged StartupPerformance NativeAOT 发布并实际触发根绘制后退出 0；该单次运行是 smoke，不作为性能统计。
发布保留 ReactiveUI 的聚合 trim/AOT warning 及本机 Homebrew 库最低系统版本 linker warning，不宣称零 warning。

一次 fresh-context 整体审查（gpt-6-astra）发现三个 Important，均先有效复现再修复：

- Show→Hide→全局 RAF→Show 后就绪不再永久阻塞；晚于声明 MainWindow 设置 ShowActivated=false 不再等待不存在的激活。
  原始两例均超时，修复后连同现有启动契约 22/22 通过。
- 自定义同名 ResourceInclude/MergeResourceInclude 保持开放资源查询，不把其 Source 当作完整键集合。
  两例原先生成错误闭合注册；修复后连同生成/消费验证 68/68 通过。

审查的一项 Minor 保留：等待期取消且应用仍通过其他窗口存活时，ThemeManager 的构建集合保留到 scope 释放。
Actions/hooks 已释放，没有继续运行的初始化任务；后续可统一这一终态的集合清理。该限制未隐瞒为已解决。

本轮所有一次性测试均已按要求删除，未新增常驻测试；原有用例仅按异步完成契约调整等待。性能探针、临时 friend
引用和测试项目引用已撤销。性能样本对应记录哈希的测量版本；之后的挂载异常清理和审查边界修复以定向测试验收，
不把旧样本冒充最终无探针二进制的重新计时。

审查修复清理后：Core 119/119（2.4961s）、Generator 97/97（13.4806s）再次通过；其余未再变更模块沿用本轮
最终证据，共 315 项不同定向用例通过，没有执行全量回归。net8 实际 StartupPerformance 也已构建、首次绘制后退出 0。

Browser 真实裁剪验证的环境处置：原 fixture 和全新输出目录都在 SDK `GenerateWasmBootJson` 失败。
输入诊断定位四个 Avalonia DLL 同时从 NuGet 与仓库共享输出传入，两份 SHA-256 分别完全一致。
仅在临时 fixture 中去掉这四组重复路径，保留相同 DLL、原始 ProductApp、第三方包、注册和裁剪配置后，
真实 publish 通过，TypeMap 回执为 `output-verified` / `atomui-typemap-v1-illink-10.0.8`。
CUA 实际页面显示 `PRODUCT_PASS browser`，验证跨包 ResourceInclude、局部 Own Token、模板、应用主题覆盖、
dark 切换、scope 更新和启动后冻结。此结果不等于未经规范化的原 fixture 已能直接 publish；仓库 build 文件未为此改动。

审查修复后的最终 Gallery NativeAOT 再次发布通过，精确最终产物完成 8 秒无错误输出存活检查并清理自己的进程。
临时 Browser 页面和 HTTP 服务已关闭；临时 fixture、probe、发布目录、nupkg 与源码备份均已删除。
普通 Release JIT 输出也重新构建，以清除此前测量 apphost 对临时探针方法的引用；不将“源文件已删除”误当二进制已清理。

### 最终命令范围

- Core：`dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter ...`；
  filter 为 ApplicationScope、ApplicationLocalizationStartup、ControlRegistrationRuntime、DenseThemeCompiler、
  TypedThemeSnapshotCache、BoundedThemeCache、ControlCompilationCache、ThemeManagerTransaction、ThemeScopeGraph、
  ThemeContextLeaseLifecycle、ControlThemeAssetContract 对应测试类。
- Localization：LanguageSnapshot、LanguageCatalogRegistry、LanguageManager、Localizer、LanguageResourceProvider、LocalizationBuilder。
- Generator：TypeMapRegistrationGenerator、TypeMapIntegration、ThemeResourceDictionary、ThemeAssetRegistration、
  ResourcePlatformAvailability、QualifiedSemanticProperty。
- Desktop：TypedPackageEntry、NumericUpDownRootSurface；Gallery：WorkspaceWindowLayout、AtomUIGalleryModule、CaseNavigationLayout。
- Build Tasks：RegistrationBuildTasks、RegistrationSourceConsumer、TypeMapPublishGate。
- 专项：四包 Release pack；`verify-package-layout.ps1 -BaselineVersion 6.2.4`；真实包 `verify-build-task-isolation.ps1`；
  Gallery 官方 PublishToLocal AOT；StartupPerformance 非 staged AOT/net8；Browser 真实 trimmed publish 与运行。
- 最终 `git diff --check` 与改动文档相对链接检查通过。命令全文和各输出保存在本计划 ledger workspace；未运行 full/change scope。

## 后续修正：正常 Release 构建的 ATOMUIREG009

用户再次指出 net8 构建中的回退警告后，定位到 Desktop 项目无条件为两个目标声明同一份关键集。
MSBuild 输入复现为 net8/net10 均有 40 项。现将 `AtomUICriticalControls` 限定到兼容 net10.0 的目标；
net8 不声明不支持的包内分区配置，继续完整单阶段包注册。net10 的 40 项及平台后缀目标配置保持原样。
生成器的诊断和 severity 保留，未加入 NoWarn；显式误配仍会报告。

真实 Release 双目标构建使用 `-warnaserror:ATOMUIREG009`，结果 0 warnings / 0 errors（43.30s）；实际 analyzer
输入验证为 net8 空、net10 40 项。首次 `--no-restore` 遇到既有 assets 缺 net8 目标，正常 restore/build 后通过。
临时配置探针已删除，未增加常驻测试。

后续修正清理后定向验证：TypeMapRegistrationGeneratorTests 10/10、TypedPackageEntryTests 6/6 通过；
`git diff --check` 通过。保留生成器对显式不兼容配置的警告契约。

## 后续修正：第二阶段后内置主题菜单仍只有 Daybreak Blue

用户截图的问题已用实际 Desktop GalleryApplication / WorkspaceWindow 的 Headless 场景复现：第二阶段后
ThemeManager.AvailableThemes 已有五款主题，但窗内主题菜单仍只有 DaybreakBlue。根因是目录演化提交了新 Catalog，
却只发布 ThemeChanged，遗漏 ThemeCatalogChanged；Workspace ViewModel 的目录订阅因此保留第一阶段列表。

修复归 Core 所有：Stage 2 与手动刷新共用目录事件发布路径及 generation 序列；主题、语言均提交后再通知。
目录内容/诊断不变时不额外通知，观察者异常沿用隔离与日志规则。没有新增菜单手动刷新或复制主题名单。
同链路还复现了用户目录来源丢失：Stage 2 只使用显式 Resolver 列表，遗漏 UseUserThemeDirectory 生成的来源。
两阶段现共用 Builder 的完整有效来源构建逻辑，用户主题及后续 Reload 均保留。

验证：实际菜单五款主题 ID 和四款替代主题逐一切换/选中状态通过；事件单次发布、语言状态已提交、观察者异常隔离、
后续 generation 增长、用户目录保留/刷新均通过。临时 Core/UI 用例及 Desktop 测试引用已删除。

常驻测试处置：将既有 ThemeCatalogReloadTests 目录提交契约参数化为手动刷新和 staged 演化，净增一个轻量分支，
复用原 fixture。已单独恢复旧发布行为证明“手动分支通过、staged 分支事件缺失失败”，恢复修复后通过。
原同步 Gallery 与手动刷新测试、编译器、生成诊断及 AOT 引用检查均无法发现这个仅在 staged 发布链路中的漏事件；
这是影响所有目录订阅者的共享公共契约，需要常规执行，单分支执行成本为毫秒级，无需常驻昂贵的 Gallery 窗口测试。
同时把事件内的状态断言改为捕获后断言，防止断言异常被产品的观察者隔离逻辑吞掉。

最终定向验证初次为 79/80：唯一失败是旧 ThemeStartupTests 假定字典直接处于第一层 MergedDictionaries，
与既有稳定复合资源层不符。该断言已改为真实包资源可查且实例相同，继续保护挂载契约。清理后最终结果为
Core 80/80（2.0246s）、Gallery 11/11（2.4362s）。
新增的 staged 常驻契约分支执行耗时 2ms。Core Release net8/net10 与 Desktop Gallery Release 构建均为
0 warnings / 0 errors（分别 3.93s、22.30s）；临时目录与测试项目引用清理、文档路径及 diff 检查通过。
