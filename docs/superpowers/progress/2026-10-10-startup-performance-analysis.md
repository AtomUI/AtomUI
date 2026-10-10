# AtomUI 启动性能分析与优化候选

分析日期：2026-10-10。状态：源码分析完成；空窗口阶段基线已实测（见下节"空窗口启动基线实测"）；
六个热点的源码级定位与修复算法设计另见
[启动热点深挖与修复方案](2026-10-10-startup-hotspots-and-fix-designs.md)；优化尚未实施。

正式启动链路由[启动与注册链路](../../architecture/foundations/startup-and-registration.md)维护；本记录保存当次发现、
候选方案及后续验证条件，不作为新的架构契约或已实现优化的证据。

## 空窗口启动基线实测（2026-10-10 补充）

方法：临时骨架应用（标准 UseAtomUI：en-US 单语言、默认主题、AlibabaSans、UseDesktopControls、空白
Avalonia Window），进程内 Stopwatch 分阶段打点，窗口 Opened 后退出；Release/Debug 各连跑 3 次。
基线状态：HEAD `ee1db4001` + 工作树两项未提交改动（ThemeSchemaRegistry 字符指纹优化、ILC8 条件注册后端）。
测量探针与骨架已全部删除，工作树还原。

总量（macOS arm64，本机）：

| 配置 | Main → UseAtomUI 返回 | 窗口 Opened | Dispatcher Idle |
|---|---|---|---|
| Release 3 次均值 | ~1.04s | ~1.21s | ~1.29s |
| Debug 3 次均值 | ~1.05s | ~1.28s | ~1.37s |

阶段占比（Release，UseAtomUI 同步段合计 ~460-490ms）：

| 阶段 | 耗时 | 说明 |
|---|---|---|
| Main→App.Initialize 进入 | ~210-325ms | 运行时装载/JIT/Avalonia 平台初始化，非 AtomUI 可控 |
| UseLanguages | ~7ms | 单语言 |
| UseAlibabaSansFont | ~20ms | FontCollection 注册 |
| UseDesktopControls | ~230ms | 319 个 fragment：183 控件 descriptor + 318 资产 descriptor（CreateGenerated 内含双重 255 全局 token 名指纹） |
| LocalizationBuilder.Build | ~33ms | 单语言 Snapshot；Gallery 四语言按比例 ~130ms |
| ThemeManagerBuilder.Build | ~87ms | descriptor 汇集、ThemeSchemaRegistry 冻结（全量排序/校验/Revision 指纹）、资产去重 |
| BuildOwnedServices | ~12ms | |
| LoadThemeCatalog | ~40ms | 读内置主题 XML + SHA256 + 绑定 |
| MountStaticResources | ~140-195ms | **324 个资源工厂全量物化**（生成式双层 ResourceInclude 包装），启动期最重单项 |
| ApplyThemeAsync(初始) | ~120-160ms | CompileGlobal ~20ms（255 token）+ CompileControls ~100ms（187 控件，每控件 CopyBuilder 按 Format/Parse 复制全局 token） |
| Styles 挂载等收尾 | ~100ms | rootContextStyle + Application.Styles.Add(this)（携带 324 字典资源树挂载） |
| ModuleInitializers | ~27ms | |
| Window 构造→Opened | ~150-170ms | 首个 TopLevel 平台初始化 + 首帧 |

实测确认的问题点排序（占 UseAtomUI 段比例）：包注册与 descriptor 物化（UseDesktopControls+Builder，~68%）＞
静态资源全量物化（MountStaticResources，~35%）＞首主题编译（CompileControls 为主）＞装载/平台（不可控项单列）。
Gallery 全量配置（四语言、五包、五个内置主题、用户主题目录、WorkspaceWindow）按比例推算约 1.8s，
与用户观察一致。

## 分析基线

分析基于本地 HEAD `f0e9b4133` 及工作树。当时已有两项未提交改动：

- `src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs`：字符指纹计算优化。
- `src/AtomUI.Toolchain/Backends/ILC8/Adapter/ConditionalRegistrationBackend.cs`：条件注册后端改动。

本次分析和文档整理没有修改上述代码。后续基准必须固定这两项改动的状态，不能混用 HEAD 与工作树结果归因。
没有运行启动计时、产品测试或 AOT 发布；下文的排序基于可定位的重复工作、预估改动风险和验证难度，不代表实测成本排名。

## 当前实现中的候选点

### 资源键二维表预创建

[ThemeSchemaRegistry](../../../src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs) 的
`CreateControlSharedResourceKeys()` 为每个注册控件、每种 SharedTokenKind 创建并装箱 ControlTokenResourceKey，
还分配对应二维数组。成本随注册控件数与共享 Token 种类数的乘积增长。

本次搜索生产源码、测试、性能工具及现有生成 C#，`GetControlSharedResourceKey()` 的外部调用只出现在
ThemeTokenResourceProviderTests 和 ThemeResourceLookupPerformanceTests。实际
[ThemeTokenResourceProvider](../../../src/AtomUI.Core/Theme/Resources/ThemeTokenResourceProvider.cs) 通过 identity/type
解析 slot，并使用全局资源键表；这张控件共享键二维表未发现生产消费者。实施前须重新搜索，排除新增调用。

候选：消除全量预创建；如保留内部键工厂，仅按实际请求提供稳定键。不能一并删除生产查询使用的全局键表，
也不能把键分配移入每次 TryGetResource。验证 bound/unbound key、owner、非法 slot/kind、主题切换及热查询零分配。

### Gallery 重复构建完整配置

[AtomUIGalleryModule.RegisterViews](../../../controlgallery/AtomUIGallery/AtomUIGalleryModule.cs) 调用
`CreateConfiguration()`，构建 branding、导航、路由和源码展示配置；
[UseGalleryBase](../../../src/AtomUI.Toolkits.GalleryBase/ThemeManagerBuilderExtensions.cs) 又调用 Configure 和
BuildConfiguration 构建正式配置。本次源码包含 78 条路由声明，声明保存工厂，不等于创建 78 个页面。

候选：RegisterViews 直接复用 ConfigureRoutes，只创建其需要的路由表。避免为此新增跨 Application 的可变配置缓存。
目标是减少一次完整配置构建，路由注册本身仍必须完成；需验证 ViewLocator、默认导航、所有路由和配置隔离一致。

### 语言 Snapshot 逐词条扫描 bundle

[LanguageSnapshotBuilder](../../../src/AtomUI.Localization/LanguageSnapshotBuilder.cs) 对每个词条按来源优先级、
语言候选、完整 bundle 列表逐层查找。Gallery 启动时构建全部四种支持语言的 Snapshot。

候选：为每个 Catalog、每种目标语言预先形成有序候选 bundle 链，词条循环只判断候选值是否存在。
保留来源优先级先于候选语言具体程度、英文最终回退、同优先级非重叠 bundle 合法互补、覆盖与格式错误诊断。
不能用单 bundle 字典覆盖合法的多个互补来源，也不把非当前语言的校验推迟到首次切换。

### 主题资产指纹重复排序

[ComputeResourceKeySchemaFingerprint](../../../src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs) 对每个资产重新
按 slot 排序全局 Token，而 registry 已经构建有序 Token 表。

候选：内部受控路径复用已经规范化的顺序，避免重复排序。接受任意输入的其他入口仍需维持排序语义；
生成式契约证据和当前 registry 的独立校验必须保留。新旧指纹逐位相等，过期资产、owner 冲突和诊断行为一致。
工作区已有字符 hash 优化，不能把该改动的收益再次归入重复排序优化。

### 主题读取缓冲区分配

[CompiledThemeCatalog.ReadBounded](../../../src/AtomUI.Core/Theme/Definitions/CompiledThemeCatalog.cs) 每次分配
81,920 字节缓冲区。Core 一个默认主题与 Gallery 四个附加内置主题都成功走该路径时，仅此固定缓冲区累计分配
409,600 字节，即 400 KiB；不包含内容副本、XML 和绑定对象，也不表示峰值存活内存。

候选：实测后评估适当缓冲区大小或池化。必须保留读取上限、异常、内容摘要和 reload 变更识别；池化缓冲区须
在 finally 归还，不能进入持久 Snapshot。冷进程首次租用可能仍产生分配，不能只用热池结果宣称启动获益。

### 控件 Token 编译中的全局复制

[ThemeCompiler.CompileControl](../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs) 对进入该分支的控件
复制完整全局 DesignToken，包含部分引用值的格式化/解析和调色板复制。无 Own Token 且无配置的控件已走轻量路径，
不能把复制次数直接视为全部注册控件数。

候选：先量化复制占比，再评估保持隔离的强类型复制或其他设计。`EffectiveGlobalToken` 可变，控件计算入口可扩展，
直接共享实例可能造成控件间污染。[主题架构](../../architecture/systems/theming/runtime.md) 中的 copy-on-write 目标
也不能当作当前代码已经实现的事实。该项改动面较大，后置于可直接减少重复工作的项目。

## 已有机制和风险边界

- 生成式不可变 descriptor 已缓存；ThemeManager 内已有定义加载、Snapshot 与控件编译缓存。
  这些机制有各自的生命周期与命中前提，新进程冷启动仍需首次创建。
- Gallery 路由保存工厂；图片 pipeline 已使用 DeferredHttpImageTransport。不要重复提出这些已存在的优化。
- 全部语言覆盖检查、跨包 schema 校验、资源优先级与首帧完整主题提交是必须保留的契约。
- [MediaBreakPointThemeBootstrapper](../../../src/AtomUI.Desktop.Controls/Window/MediaBreakPointThemeBootstrapper.cs)
  静态持有 ThemeManager/ControlTheme；本次未做多 Application 复现。将其列为隔离与生命周期基线检查项，
  不以本次源码阅读宣称已证实或修复泄漏。
- 资源工厂延迟执行会触及资源可用时机、查找顺序和首次使用错误，不属于首批等价减量候选。

## 建议的实施与验收顺序

1. ~~补齐真正的进程启动与阶段计时。~~ 已完成（2026-10-10 空窗口基线实测，见上节）；下一步将骨架固化为
   常驻测量工具（可并入 GalleryPerformance 或独立 StartupPerformance harness）再开始改动。
2. 先单独验证资源键二维表，再依次验证 Gallery 配置、语言候选链、Schema 重复排序。每轮固定相同输入，
   只改变一个因素，核对目标阶段和总启动耗时；没有有效收益或出现回归时不保留该性能改动。
3. 根据阶段占比决定是否处理读取缓冲区和 Token 复制。若资源物化占主导，另行设计，保留原始首帧和首次交互契约。
4. 在清理临时探针后，对最终改动模块进行 focused 验证；涉及生成器、注册与裁剪边界时完成对应 AOT 专项。

基于实测占比补充的优先级修正（2026-10-10）：

- **静态资源延迟物化（候选第一优先）**：324 个 GeneratedThemeAssetResource 双层包装在
  `MountStaticResources` 全量实例化是启动期最重单项（~140-195ms）。GeneratedThemeAssetResource 本身是
  ResourceDictionary+ResourceInclude 壳，可评估延迟到首次资源查找/按 owner 控件首次实例化时物化；
  必须保留资源查找顺序契约（包内 ControlResourcePhase 顺序）与 reload 行为。
- **CompileControls 复制减量**：187 控件逐一 `CopyBuilder`（Format/Parse 往返 255 token）贡献 ~100ms；
  与既有"控件 Token 编译中的全局复制"候选项合并评估（无 config 且无 Own Token 的控件已走空快照轻路径，
  剩余为有 Own Token 控件的必要复制，需量化可共享比例）。
- **CreateGenerated 双重指纹**：318 个资产 descriptor 每个都要对 255 个全局 token 名做两轮 FNV
  （contract + resourceKey 指纹），这部分包含在 UseDesktopControls ~230ms 内；可评估指纹结果内联为
  编译期常量、运行时只校验一次的方案（需保留过期资产生成契约校验语义，放宽为抽查或构建期断言）。

基准固定源码、SDK、配置、RID、字体、用户主题目录内容、窗口尺寸和默认路由。Release CoreCLR、NativeAOT、Browser
分别记录；每组至少 10 次独立进程，报告样本数、均值、中位数、范围和分配；P95 需要更多样本并说明稳定性。
区分进程冷启动与 OS/浏览器资产缓存状态，前后交替采样，不在计时范围内构建或发布。

验收复用现有 ThemeStartup/ThemeFollowSystem、注册与资产契约、LanguageSnapshot/LanguageCatalogRegistry、
ApplicationScope 和资源查询测试。新增差分验证默认临时保存，比较快照值、诊断、通知、失败行为与对象隔离后删除；
实际首帧、窗口尺寸、主题/语言切换、首次弹层与交互由对应宿主复核。

本记录没有优化后的数据，不声明启动提速、内存下降或任何产品行为已经通过验证。
