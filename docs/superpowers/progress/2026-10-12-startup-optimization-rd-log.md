# 启动性能优化研发日志（2026-10-11 ~ 2026-10-12）

目的：完整记录本轮流空窗口启动优化（Gallery/AtomUI）的全部改动、机制、实测数据与排错索引，
供后续审查与问题定位。上游明细见 `2026-10-11-macos-startup-remaining-hotspots.md`（热点方案与
Task 级验证），本文是跨提交的全景账。

> 算法复核后的待办见[后续优化计划](../plans/2026-10-12-startup-algorithm-optimization.md)，正式目标见
> [分级启动演化](../../architecture/foundations/staged-startup-evolution.md)与
> [主题惰性物化](../../architecture/systems/theming/lazy-materialization.md)。本文各批次数字保留为历史记录；
> 不同版本/探针条件不可串接成累计收益，当前正确产物须重新绑定源码和二进制身份验收。

## 0. 总览：收益链（空窗口启动，Release JIT 终端，交错采样中位数）

| 阶段 | 空窗出现 | 提交 |
|---|---:|---|
| 优化前基线 | ~2.3s+ | — |
| 热点 A/B/D + 启动路径算法优化 | SyncTotal 595→473ms（−123） | `914ce40ef`、`b3557e188` |
| 热点 C（控件快照惰性编译） | 同批 SyncTotal 452.2→391.2ms（原始精度差 −60.9）；空窗 Opened 1.90s | `4fab9a043` |
| 路线 A（Gallery shell 后置，未提交） | Opened 1.37s（−293ms） | 工作区 |
| 分级启动（历史未提交版本） | Opened 0.82s；关键集/chrome 修正前数据，不作为完整首帧验收 | 当时工作区 |

## 0.1 优化前后终测矩阵（2026-10-12 第二轮复测，Debug / Release / AOT 三模式）

**方法**：基线 = `f0e9b4133`（6.2.5，优化系列起点 `ee1db4001` 之前）独立 worktree；当前 = 工作区
（热点 A-D 已提交 + 路线 A + 分级启动 + Stage 1 chrome 修复 + 演化/惰性物化第二轮优化）。
两侧注入同形时间线探针（临时，测毕即删），每模式预热 1 对后交错采样 8 对，取中位数（ms）。
环境：osx-arm64 本机。AOT 为 Release NativeAOT 发布（osx-arm64）。

| 阶段（里程碑） | Release 基线 | Release 当前 | Debug 基线 | Debug 当前 | AOT 基线 | AOT 当前 |
|---|---:|---:|---:|---:|---:|---:|
| InitializeEntered（进程→Initialize 进入） | 305 | 303 | 310 | 334 | 147 | 139 |
| UseAtomUI.SyncTotal（窗口前框架初始化） | 1067 | **287** | 1206 | **325** | 203 | **7.4** |
| FrameworkCompleted（框架初始化完成） | 1403 | 611 | 1552 | 684 | 352 | 151 |
| Window.Ctor（主窗体构造完成） | 1897 | 857 | 2048 | 923 | 452 | 243 |
| **Window.Opened（空窗出现，用户感知）** | **2197** | **1083** | **2416** | **1152** | **505** | **269** |
| ShellReady（shell 构建完成）¹ | 1897 | 1858 | 2048 | 2099 | 452 | 472 |
| Dispatcher.Idle（全就绪） | 2530 | **2165** | 2825 | **2461** | 648 | **519** |

**关键结论**：

- **空窗出现（Opened）**：Release −51%（2197→1083，−1114）、Debug −52%（2416→1152，−1264）、
  AOT −47%（505→269，−236）。三种构建形态下等待窗口的时间都缩短近半或过半。
- **窗口前框架初始化（UseAtomUI.SyncTotal）**：Release −73%、Debug −73%、AOT −96%
  （Stage 1 只剩 chrome 关键集，7.4ms）。
- **全就绪（Idle）全面改善**：−365（Release）/ −364（Debug）/ −129（AOT）ms——Stage 2 与首帧
  渲染并行，分期不是"挪后"而是"重叠"。
- **ShellReady 与基线持平（±2.5%）**：第二轮优化后两阶段一次性成本已被演化重构吸收
  （第一轮曾 +53/+56/+10ms）。
- 基线 ShellReady == Window.Ctor（老结构 ctor 内同步建全 shell）；当前在 StageTwoCompleted 后，
  语义不同，列完整性。
- AOT 体积：基线 82,961,320 B → 当前 82,961,784 B（+464 B，持平；第一轮曾 −82 KB，第二轮的
  演化/惰性物化代码补回了等量体积）。
- 第一轮终测（同日早些时候，分级启动+chrome 修复后）为 Opened −54%（908/1034/200ms）；
  本轮机器整体慢 ~10%（两侧同漂移，相对对比有效），且绝对值不可跨轮直接比较——收益以
  轮内基线对照为准。

## 1. 已提交改动

### 1.1 `914ce40ef` + `b3557e188`：热点 A/B/D + 启动路径算法（Tasks 0-5）

机制与实测（详见上游 progress 文档"最终实测收益"节）：

- **指纹前缀续算 / 预排序直通 / Revision 组合化**（热点 A/D 类）：ThemeContentFingerprint 与
  ThemeSchemaRevision 计算去重复遍历；实测 UseDesktopControls −24.2ms、热点 D 3.1-4.7ms
  （主份额已被前轮 `914ce40ef` 的生成式 `CopyGlobalValues` 取走）。
- **主题资源惰性挂载（热点 B，方案 B）**：`DeferredControlThemeResourceProvider`
  （`src/AtomUI.Core/Theme/Resources/`）——挂载期只存工厂，首查才构造真实字典；实测 PostConfigure
  −102.2ms。
  - **排错要点**：owner 不转发给真实字典（`AddOwner` 广播 → DynamicResource 重估 → 嵌套加载的
    栈溢出环路，真实套件曾 SIGABRT）；已知限制=资产内字典作用域 DynamicResource 不解析（仓库零
    命中，终审 F3 裁定）；`_isLoading` 重入按 miss 处理（ResourceInclude 语义）。
- 生成器模板契约：`ConditionalRegistrationWriter` 指纹赋值删除并钉注释（net8 后端绑定纯调用
  模板）；陈旧契约错误消息增强（旧生成器包给出可操作提示）。

### 1.2 `4fab9a043`：热点 C——控件主题快照按 slot 惰性编译

- **机制**：`LazyControlSnapshots`（Count+索引器，不实现枚举）+ `ControlSlotState`
  （Materialized/ReuseParent/Uncompiled 构造期急切解析）+ `LazyControlSnapshotSource`（冻结编译
  输入，`_gate` 双检单飞）；`CompileControl` 同函数同缓存键，结果逐位等价；估算 = 构造期基线 +
  `MaterializedBytes` 增量（Interlocked，饱和语义与估算器 `Add` 对齐）；旧急切路径保留为
  `CompileControlsEagerForTest` 差分钩子。
- **契约修订**（runtime.md §7/§8）：`ApplyTheme` 返回时全局完整提交；控件快照首查编译；**控件级
  编译错误改为首查抛出**（原 ApplyTheme 原子失败），全局维度原子性保持。
- **实测**：SyncTotal 452→391ms（−60.9）；空窗 MaterializedCount=0（193 槽位注册表）；主题重编译
  降至 ~43ms（全局 only）——**这是分级启动可行性的基石**。
- **排错索引**：差分方法=结构比较（Easing/Brush/Transform 每编译自有克隆是既有所有权设计，
  引用不等≠值差异，eager-vs-eager 探针可判）；估算漂移=缓存插入期采样不回灌（有界，只影响驱逐）；
  `Compile_Uses_The_ChangeSet_...` 测试的评估计数断言在首访后（契约时点，非缺陷）。

## 2. 工作区改动（未提交）

### 2.1 路线 A：Gallery shell 后置（`WorkspaceWindow.axaml.cs`）

- **机制**：构造器只留 ViewModel + InitializeComponent + 静态占位（"…"）；`OnOpened →
  RequestAnimationFrame → Dispatcher.Background` 执行 `BuildDeferredContent()`（主题菜单/导航/
  GalleryShellView），SystemIdle 收尾。
- **实测**：Opened 1667→1374ms（Release，−293）/ 1907→1499（Debug，−408）；全就绪 1888→1750
  （Release）——窗口早出现且全就绪也提前。
- **排错要点**：占位符静态无动画（防抢渲染）；分级启动落地后本编排将重构为事件订阅（官方参照）。

### 2.2 临时探针（测量资产，验收后删除）

- Gallery 侧：`controlgallery/AtomUIGallery/TemporaryStartupTimeline.cs`（PERF|key=value →
  /tmp 日志；Debug 常开 / Release `ATOMUI_STARTUP_TIMELINE=1`）+ `Program.cs` /
  `GalleryApplication.cs` / `WorkspaceWindow.axaml.cs` 打点。
- Core 侧：`src/AtomUI.Core/TemporaryStartupProbe.cs`（PROBE|key=ms → stderr）+
  `ApplicationExtensions.cs`（UseAtomUI 五段）/ `ThemeManager.cs`（TM 六段）/
  `ControlRegistrationRuntime.cs`（Register 六段/包）打点。
- **测量结论存档**（2026-10-12，Release JIT 中位数）：UseAtomUI 579 = Configure 346
  （Desktop Collect 141.6 JIT 主导 / Gallery 语言 96 / GalleryBase 23.8 / 三小包 14.6 / 字体 18）+
  PostConfigure 233（ScopeInit 90.3[CatalogLoad 35.6+ApplyTheme 42.6+Normalize 5.1] /
  LocalizationBuild 48.5 / Initializers 53.1 / ThemeBuilderBuild 32.5 / Owned 7.5）。
- **禁止混入提交**：任何 perf 提交前核对暂存区不含上述文件。

### 2.3 分级启动（Staged Startup，控件粒度分期）——已实施，未提交

Spec：`docs/superpowers/specs/2026-10-12-staged-startup-design.md`；计划：
`docs/superpowers/plans/2026-10-12-staged-startup.md`；台账：`.superpowers/sdd/2026-10-12-staged-startup/`。

- **机制链**（全部 opt-in，不调用 `Deferred` 的应用零执行路径）：
  - 生成器分区：控件包 csproj 声明 `AtomUICriticalControls`（逗号分隔简单名）→ 发射
    `CollectCritical/CollectDeferred` 两段收集 + 独立 `RegisterPartitionedPackage` ABI（关键集先行，余量委托）；
    未分区包保持 6 参调用原样（老 Core 二进制兼容）。裁剪路径 `CandidateKeysCritical ∪
    CandidateKeysDeferred == 全量 CandidateKeys`（测试锁定）；分区时不再发射全量数组（死数据）。
  - `builder.Deferred(Action)` 声明 Stage 2 注册块；`ApplicationScope.RunStageTwo()` 在启动
    TopLevel 首个 `RequestAnimationFrame` 后以 Background 优先级调度（不保证首帧提交、呈现或原生激活），
    Interlocked 单飞，五事件
    （StageOneCompleted 在 UseAtomUI 返回前同步派发——configure 期订阅可收到）。
  - 主题演化：Stage 2 窗口（受控重开/再冻结）→ 注册块 → 余量段收集（`RunDeferredCollects`，
    产出与包同 id/provider 的「余量段」贡献）→ `MergeStageTwoInto` → registry v2
    （`EvolveRegistrationAndReapplyTheme`：重建→目录重载→增量挂载→按当前请求重应用，
    `_lastCommittedRequestKey` 作废防同请求 NoOp）。
  - 本地化演化：`LanguageContext.Evolve`（registry volatile 交换，manager/provider/localizer
    零身份变化）；Stage 1 中间态容忍未解析的跨包 bundle（见下）。
  - Gallery 参照模式：Stage 1 = DesktopControls（关键集）+ GalleryBase + 语言/字体；
    `Deferred` = Extras/ColorPicker/DataGrid/GalleryControls；WorkspaceWindow 事件订阅
    （StageTwoCompleted → BuildDeferredContent，StageTwoFailed → 错误态占位）。
- **实测**（Release JIT，交错采样中位）：Gallery `Window.Opened` 1374→**~820-840ms**
  （历史关键集修正前版本，完整 chrome 验收不成立）；UseAtomUI.SyncTotal ~253ms；StageTwoCompleted ~1360ms；ShellReady ~1585ms；
  Idle ~1900ms。AOT 基线（4fab9a043）：空窗 Opened 215.9 / FirstFrame 238 / ShellReady 270.5。
- **实施中抓到的十二个真实缺陷**（排错索引价值最高）：
  1. **跨包语言包 bundle 未解析**：应用级 bootstrap（Stage 1）注册的语言包 bundle（如
     pt-BR）指向随包后置的 catalog → Stage 1 冻结抛 `LanguageCatalogException`。修复 =
     Stage 1 中间态容忍未解析 bundle（`LanguageCatalogRegistry.Create(..., allowUnresolvedBundles)`，
     终态恒严格）——跳过安全：目标 catalog 不存在 ⇒ 该 bundle 键不可达。
  2. **贡献通道不一致**：余量收集写入扁平集合（仅 Stage 1 构建可见），Stage 2 并入走逐包
     通道 → 余量段 token/语义/资产/资源全部丢失（症状：DataGrid 资产引用的 Pagination
     token 未注册）。修复 = 余量段一等化为 `ControlPackageRegistration`，两通道消费同一
     「贡献集合」（`EnumerateContributions`）。
  3. **合并字典查找序回归**（非 staged 门抓到，+73ms）：Avalonia 合并字典查找**自尾向头**；
     分段拼接把余量段置于 provider 列表末尾（查找起点），既有资源首查被迫构造余量段全部
     字典（实测一次 `TryGetResource(typeof(Window))` 物化 315 个字典）。修复 = provider 维度
     (Phase, Order, AssetId) 全局归并排序。**排错口诀**：`DeferredControlThemeResourceProvider`
     在 miss 时也 `Load()`——任何「首查扫长链」的路径都在隐式付费；诊断 = 数 `IsLoaded`。
  4. **Stage 2 余量段资源不挂载**：段与包同 provider，但 Stage 1 挂载后 provider 已从挂载
     队列清除——段必须重新登记 provider（挂载水位防重复进合并字典）。测试以挂载标记资源
     锁定（RED→GREEN）。
  5. **关键集被 `.editorconfig` 分号截断**：`CompilerVisibleProperty` 把 MSBuild 属性写入生成器
     analyzer config，而分号在 `.editorconfig` 中表示注释开始；原声明虽列出约 39 个名称，生成器
     实际只收到 `Window`。症状是 Stage 1 缺 `WindowDrawnDecorations`、`WindowTitleBar`、`Menu` 等
     chrome 资源，macOS 红绿灯贴顶且 Gallery 主菜单为空。修复 = 属性与文档统一为逗号分隔，生成器
     兼容逗号和直接 analyzer option 的分号；真实 Stage 1 资产闭包测试锁定窗口装饰与菜单主题。
  6. **Background 任务早于 macOS 原生激活**：只把 Stage 2 投递到 Dispatcher Background，仍会在
     AppKit 主 run loop 完成应用激活前占住 UI 线程；窗口已可见时系统菜单栏仍属于 Rider。修复 =
     解析启动 TopLevel，等待其首个 `RequestAnimationFrame` 后再投递 Stage 2。
  7. **net8 分区降级被错误诊断阻断**：条件注册后端按设计应忽略关键集并完整单阶段注册，但复用了
     Error 级 `ATOMUIREG006`，导致 Desktop.Controls 的多目标 Release 构建失败。修复 = 新增
     Warning 级 `ATOMUIREG009` 专门表达可用但无分期收益的回退；真正 ABI/平台不兼容继续保留
     `ATOMUIREG006` Error。
  8. **语言切换退回 Stage 1 Snapshot**：Stage 2 只替换 `LanguageContext` 当前 revision，
     `LanguageManager` 仍持有 Stage 1 snapshot/definition 集；之后 `ChangeLanguage` 会丢失延迟 Catalog。
     修复 = manager 原位演化 snapshots、definitions 和 supported languages，再统一发布资源通知。
  9. **Stage 2 分区包重复挂载且资源序错位**：包主体与余量段共享 provider，却各登记一次；挂载循环
     因此把同一批资源加入两次。Stage 1 provider 的余量资源只追加也破坏 `(Phase, Order, AssetId)`
     全局序。修复 = 按 PackageCommitOrdinal 聚合贡献、provider 引用去重，并在每次演化时复用惰性代理、
     按完整注册集合重建 provider 与全局合并序。
  10. **晚期失败泄露部分 Stage 2 状态**：主题并入/发布先于本地化终态构建，后者失败时 Semantic registry
      已可见。修复 = 本地化先预检，Semantic descriptor 暂存到主题提交成功后才发布，主题准备失败恢复
      Stage 1 registry/catalog/config 并丢弃未挂载资源。不可回滚的 Deferred initializer 明确在提交前拒绝。
  11. **分区重载破坏既有源码调用**：新增同名 7 参 `RegisterPackage` / `EnsurePackage` 让既有第 5 个
      未类型化 lambda 调用产生重载歧义。修复 = 改用独立 `RegisterPartitionedPackage` /
      `EnsurePartitionedPackage`，保留旧入口的源码与二进制兼容。
  12. **业务完成事件被误判为框架失败**：`StageTwoCompleted` 位于框架 try/catch 内且直接 multicast；
      首个业务建壳处理器异常会中断后续订阅者、把已提交状态改成 failed 并再发 `StageTwoFailed`。
      修复 = 所有启动事件逐订阅者隔离并记录 System warning，提交后业务异常不改变框架状态。
- **测试**（临时，验收后删）：`tests/AtomUI.Core.Tests/Temporary/StagedStartup{Event,ThemeWindow,
  Localization}Tests.cs`、`tests/AtomUI.Generator.Tests/Temporary/PartitionEmissionProbeTests.cs`、
  `tests/AtomUI.Desktop.Controls.Tests/Temporary/StagedStartupGalleryTests.cs`（chrome 闭包锁定：
  骨架窗实测闭包 ⊆ csproj 关键集）。
- **常驻最小契约**：复用 `TypedPackageEntryTests` 锁定 Stage 1 窗口装饰/标题栏/Menu 资产，复用
  `TypeMapRegistrationGeneratorTests` 锁定逗号关键集与 net8 单阶段 warning 回退；新增一个低成本
  `ApplicationScopeTests` 用例锁定 Stage 2 必须等待启动 TopLevel 首帧，并保留语言演化、资源去重/顺序
  与晚期失败不发布的核心事务契约。它们分别覆盖生成输入、真实包资产、调度时点和跨子系统原子性，
  现有编译检查无法替代；其余一次性探针与复现测试全部删除。

## 3. 设计决策记录（分级启动，已批 spec）

- 分期粒度三步演进：整包 Deferred → 用户否决"标准太低" → 控件（fragment）粒度（chrome 闭包
  Stage 1 ~20ms 替代整包 174ms）。
- 方案 B（Stage 1 原生骨架窗，0.65s）被"用户无感知"约束否决——完整 AtomUI chrome 必须首帧。
- 生成器约束从"零改动"升级为"契约兼容增量演进"（老包整包回退；模板门/裁剪等价/双 AOT 发布门禁）。
- 验收线：JIT ≤0.95s / AOT ≤0.4s（首任务实测基线棘轮只紧不松）；回退整包粒度则 JIT ≤1.05s /
  AOT ≤0.45s（回退需显式决策）。

## 4. 已知风险与存量问题（审查排错必读）

1. **PopupShadowTests.ShadowsAwareContainer_Releases_A_Replaced_Nested_Surface**：存量负载敏感
   flaky（GC 时机，IsAlive 断言；b3557e188 前即复现），隔离复跑 3/3 过；两轮验证均裁决非回归。
2. **TabContainerLayoutReviewTests**（参数化个别 case）：同上存量 flaky 类。
   （2026-10-12 补）**DialogLifecycleTests.Closed_Overlay_Session_Releases_Surface_When_A_Custom_Button_
   Is_Retained** 亦属同类（GC 时机 + 释放断言，隔离复跑 3/3 过；全套并发跑偶发）。
3. **ShowCasePanelStructureTests.GalleryShowCaseHeader_...**：存量陈旧测试（HEAD 可复现，断言与
   现生成代码不匹配），处置待用户裁决。
4. 估算记账：缓存插入期采样 + 物化增量不回灌；父快照链还可能绕过 LRU 保留历史，不能宣称保留内存有界。
   目标改为独立惰性 cell，并验证物化后计费与驱逐，见[惰性物化设计](../../architecture/systems/theming/lazy-materialization.md)。
5. 热点 C 失败语义时点变化（§1.2）——主题应用失败的排错先查 ApplyTheme 结果，再查首查异常栈。
6. Rider 启动含调试器税 ~0.8-1.5s（代码不可消除；日常开发建议 Release 运行配置）。

## 5. 验证与回滚索引

- 权威测量法：基线 worktree + 交错采样（取消负载漂移）；**禁止 `git stash` 翻转 A/B**。
- 分级启动回退：单提交 revert（opt-in 路径无跨模块状态迁移）；热点 C 回退：单提交 revert
  `4fab9a043`；热点 B 回退：`b3557e188` 内惰性挂载段。
- 全量回归仅显式指令或 release；iterate 验证 `python3 scripts/verification/test.py run --scope
  iterate --path <模块>`。

## 6. 算法复核后的后续工作

后续计划区分已完成的工作消除与仍需实证的成本转移。优先修复 Stage 2 指纹/slot 身份、历史快照链、静态资源
相对 Token provider 的优先级和重挂 owner 通知；随后实施统一准备/提交、全局计算及语言 Catalog 行复用、
已知资源键路由与有界调度。具体任务、临时失败测试、清理要求及 JIT/AOT 验收口径见新计划 O1–O15 / Task 0–11。

源码复核参考本地 Avalonia 12.1.3 的 `MediaContext` 动画调度与 macOS 激活路径：RAF 在媒体循环内执行，
不等于目标窗口 presentation；macOS 原生激活还有主队列调度。该结论修正此前时序保证，不替代具体产物的原生验收。
