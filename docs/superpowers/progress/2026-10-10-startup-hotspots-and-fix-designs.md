# AtomUI 启动热点与优化方案处置

初次分析：2026-10-10。实测与源码同步：2026-10-11。

**Windows 优化已在 `914ce40ef` 实施并提交；本文件按实际保留实现更新原六项方案。**
本轮空窗口启动至根 Render 回调缩短约 202–209 ms，但 AtomUI Window 场景仍约 1.66 s。
收益成立而幅度有限，不能继续把原先六项算法推算相加，宣称启动可降至约 1 s。

完整测量口径和跨平台基线区分见[启动性能分析与优化进展](2026-10-10-startup-performance-analysis.md)；
本轮数据来自[Windows 结果报告](2026-10-11-windows-startup-algorithm-results.md)和
[60 条原始样本及证据](2026-10-11-windows-startup-algorithm-evidence.json)。
本次没有重新运行性能测试。后文微测量来自该轮各专项，样本策略不同，不是单项独立的整机启动收益。

## 总体实测与方案状态

| 场景：启动至根 Render 回调 | 基线中位数 | 优化后中位数 | 改善率 |
| --- | ---: | ---: | ---: |
| 纯 Avalonia 对照 | 530.947 ms | 531.143 ms | -0.04% |
| AtomUI 初始化＋Avalonia Window | 1571.324 ms | 1369.487 ms | 12.85% |
| AtomUI 初始化＋AtomUI Window | 1864.418 ms | 1655.117 ms | 11.23% |

每版本每场景 10 次独立进程；改善率为 `(基线 - 优化后) / 基线`。两个 AtomUI 场景初始化托管分配下降约 43%，
但这不等于整体启动快 43%。根 Render 回调不代表屏幕实际呈现或首屏可交互，未控制空 OS 缓存状态。

| 原问题 | 本轮保留方案 | 原方案处置 |
| --- | --- | --- |
| 1 静态资源包装 | 有严格适用条件的 AddDeferred / 直接构造 | 通用惰性代理不能按原设计兑现收益；未实施 |
| 2 生成资产指纹/排序 | 等价 FNV 编码减量、owner 合并等 | 未重排哈希字段、未改为抽查 |
| 3 全局 DesignToken 复制 | 生成式强类型复制，保留隔离 | 未共享可变根 builder |
| 4 Builder/Registry | 复用受控索引、有序表和冻结资产，减少注册分配 | 未以包常量替代 Registry.Revision，未全面取消排序 |
| 5 主题 XML 与缓冲 | Core 内置主题构建期校验；外部流池化缓冲 | 未把全部 avares 来源视为免校验可信源 |
| 6 语言 Snapshot | 按需扩展候选链，合并部分覆盖/冲突扫描 | 未实现原先的全量预建索引＋覆盖位图方案 |

下面分别记录成本来源、实际实现和必须保留的边界；“仍有成本”不等于已经证明下一项优化。

## 问题 1：静态资源包装与挂载

### 当前实现和实测边界

[ThemeManager.MountStaticResources](../../../src/AtomUI.Core/Theme/ThemeManager.cs)仍逐个执行 staged factory，
再按既定顺序挂载资源。优化发生在
[TypeMapRegistrationWriter](../../../src/AtomUI.Generator/Registration/TypeMapRegistrationWriter.cs)生成的工厂中：

- 符合精确结构和构造条件的 typed theme：创建 ResourceDictionary，通过 AddDeferred 注册现有编译主题构造器，
  保留按键首次查找时创建主题及每字典缓存。
- 已有 authored x:Class 且可安全构造的字典：直接调用原构造器，保留其 AXAML。
- 其余资产沿用[包装生成任务](../../../src/AtomUI.Toolchain/Tasks/GenerateThemeAssetWrappersTask.cs)及原回退路径。

Windows 专项记录 Desktop 315 个资产中 49 个走 deferred typed、4 个直接构造、262 个保留包装。
这是该次产物的资产口径，不与原 macOS 记录的 324 个总挂载工厂混用。
局部热态测量为 typed factory 2784→472 B；已有 x:Class 字典 fixture 约 7624→6000 B，
后者冷态耗时区间重叠，不能宣称确定的冷启动耗时收益。

候选插桩记录中挂载阶段为 183.207 / 186.835 / 176.225 ms，并没有接近原预测的 5–15 ms。
这些区间包含调用/JIT/挂载成本，不能全归为包装对象分配。

### 原通用代理为何不能直接采用

本次框架核对使用本地源码 `/Users/chinboy/Projects/ReferenceProjects/Avalonia`，tag 为 `12.1.3`，
与当前依赖版本一致，工作树无改动。关键实现为：

- `Avalonia.Markup.Xaml/Styling/ResourceInclude.cs`：AddOwner、HasResources、OwnerChanged 订阅等都会访问 Loaded。
- `Avalonia.Base/Controls/ResourceDictionary.cs`：OnAddOwner 遍历子 provider，调用 AddOwner 与 HasResources；
  TryGetResource 对 MergedDictionaries 逆序查找。
- `Avalonia.Base/Styling/Styles.cs`：Styles 获得 owner 时继续向资源字典和子项传递 owner。

原代理设计让 AddOwner/HasResources 调用 EnsureMaterialized，因此至迟在资源树获得 owner 时会物化，
无法避免启动阶段工作。即使推迟 owner 传播，首次反向资源查找也可能物化多个未命中字典；
没有可靠的键覆盖信息，就不能声称“查找 Window 只物化一个包装”。

ResourceInclude 字典加载、字典内 deferred 主题实例化和 ControlTemplate 实例化是不同层级。
原文把它们概括为“真实主题启动期根本不会加载”过于绝对，后续测量须分别记录。

### 保留边界及后续研究

普通 classless 资源根改写可能改变 IRootObjectProvider、词法资源、BaseUri、owner 和构造时机；
Windows 原型已发现这类兼容性差异，因此保留 262 个包装。
[RegistrationModelBuilder](../../../src/AtomUI.Generator/Registration/RegistrationModelBuilder.cs)还对构造器、可见性、
required members、TargetType 及诊断条件做守卫，不能为扩展优化覆盖率直接去掉回退。

ReloadThemesAsync 更新主题定义 Catalog 与快照，不是重新注册控件包；原方案所写“reload 替换全部代理”
不是现有流程。后续如探索更广泛延迟，需要另行证明资源优先级、owner/variant 通知、首次失败时机、作用域和裁剪语义。
不得把修改 owner 行为或把成本转到首次交互视为已经成立的等价优化。

## 问题 2：包注册期资产指纹与规范化

### 当前实现

[ControlThemeAssetDescriptor.CreateGenerated](../../../src/AtomUI.Core/Theme/Schema/ControlThemeAssetDescriptor.cs)
仍逐资产验证生成契约，并用生成时全局 Token 名生成资源 schema 指纹；
[ThemeSchemaRegistry](../../../src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs)相关路径保留当前 registry 的独立验证。

本轮保留 FNV 零后缀乘法折叠，维持原八字节编码、整数扩展和零哨兵语义；
资产 owner 规范化减少空输入的重复遍历，非空输入保留跨 binding 冲突检查。
局部空 descriptor 测量为 2016→1392 B，不能据此推出 UseDesktopControls 可减少 100–150 ms。

### 原共享前缀与抽查方案的修正

把全局名字段移到 FNV 输入前面会改变指纹，不能同时要求“与原算法逐位相等”。
如果未来要改变协议，需要明确兼容性与版本策略，不能用生成器和运行时同仓提交代替混合版本消费者的评估。

生成契约指纹使用编译可表达的类型元数据，资源指纹包含运行时完整类型身份，registry 校验又使用实际已注册 schema。
这些校验承担不同的证明责任，不能因为都包含全局名就认为可以相互替代。
首个资产通过也不能证明其他资产常量或 owner/export 证据正确，因此本轮没有把逐资产校验降级为抽查。

该处仍有重复计算可以继续量化，但不能先承诺“318×255 降为 255+318”或固定节省时间。
任何等价减量都要保留旧协议差分、过期资产、owner 冲突和跨包验证，并对真实裁剪消费者验证。

## 问题 3：控件全局 DesignToken 复制

### 当前实现和局部证据

[ThemeCompiler.CopyBuilder](../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)只在 descriptor 对象身份
与规范生成式全局 schema 一致时使用 GeneratedThemeSchema.CopyGlobalValues；
[GeneratedThemeSchemaWriter](../../../src/AtomUI.Generator/ThemeSchema/GeneratedThemeSchemaWriter.cs)生成强类型复制代码。
自定义 accessor/schema 仍走原有读取、格式化/解析与写入路径。

CompileControl 保留独立 effectiveGlobalBuilder，调色板保留复制。FontFamily 的已知不可变值和部分冻结表投影
可复用，但不把可变 Brush 或自定义回调结果视为天然可共享。
无 Own Token 且无配置的原有轻量路径继续存在，不把所有注册控件都算成复制次数。

专项局部测量：每次热态复制分配 16,880→3,936 B，首次复制中位数 18.451→7.443 ms。
这不是全部 CompileControls 的耗时；原先“约 100 ms→25–35 ms”没有独立实测支持，撤下该预测。

### 原共享根 builder 方案的修正

[AbstractControlDesignToken.EffectiveGlobalToken](../../../src/AtomUI.Core/Theme/DesignTokens/AbstractControlDesignToken.cs)
指向可变 DesignToken；公共 descriptor 支持自定义 evaluator。
仓库内控件只读不能证明外部扩展只读，Debug 写保护也不能保证 Release 隔离，属性写检查还需考虑可变对象的内部修改。

本轮通过更便宜的复制保留隔离，没有引入共享可变根实例或完整 copy-on-write 机制。
后续仍应检查自定义 accessor、副作用/调用顺序、Light/Dark/Compact、控件级 Global/Custom 算法和局部 scope。
原始调色板 memoization 原型因首次计算变慢已撤回，不把热态收益视为启动收益。

## 问题 4：Builder、Registry 与 Manifest 重复工作

### 已实施的减量

- 前一轮已移除控件共享键二维表的全量预创建、复用绑定 descriptor 的规范化 OwnTokens，
  并让 Manifest 使用 registry 有序全局表；见[前一轮结果](2026-10-10-core-startup-performance-results.md)。
- 本轮 [ThemeSchemaRegistry](../../../src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs)复用 bound identity map，
  在一次操作内缓存重复类型名信息，并保存冻结资产顺序。
- [ControlThemeAssetManifest](../../../src/AtomUI.Core/Theme/Schema/ControlThemeAssetManifest.cs)的受控启动入口
  复用 registry 已验证内容；接受任意公共输入的入口仍防御性复制、验证指纹与冲突。
- [ControlPackageRegistrationBuilder](../../../src/AtomUI.Core/Registration/ControlPackageRegistrationBuilder.cs)
  使用静态带状态回调等减少闭包/重复边界成本，保留递归注册与失败污染语义。

局部热态全局 registry 分配约 320,569→90,997 B，Resource Add 分配 96→0 B。
它们不是 Builder 总体或冷进程的独立 A/B。候选插桩中的 Builder 三次为 54.470 / 53.571 / 54.471 ms；
不能与旧 macOS 的约 87 ms 直接相减。

### 未采用的简化

Registry 仍需要处理实际选中的控件、跨包集合与 owner/类型证据；包内有序不意味着合并输入天然满足全部规范顺序。
本轮未把所有排序换成预排序直通，也未用包级编译常量替代运行时 ComputeRevision。

Revision 服务于实际 schema/算法的缓存身份，必须覆盖选择集合及自定义输入；
“只需要进程内一致”本身不能证明包常量组合足够。原预计节省 50–65 ms 不再保留为收益承诺。

## 问题 5：内置主题 XML 校验与外部读取

### 当前实现和证据

[GenerateBuiltInThemeDocumentTask](../../../src/AtomUI.Toolchain/Tasks/GenerateBuiltInThemeDocumentTask.cs)
在构建时复用权威 ThemeDocumentReader/XSD，生成 Core 内置主题文档及内容摘要。
只有封闭的 Core 自有
[BuiltInThemeDefinitionSource](../../../src/AtomUI.Core/Theme/Definitions/BuiltInThemeDefinitionSource.cs)
使用该结果；运行时仍按实际 registry 绑定，保留来源 URI 和原始资源访问契约。

[CompiledThemeCatalog](../../../src/AtomUI.Core/Theme/Definitions/CompiledThemeCatalog.cs)对其他来源继续读流、
计算摘要并执行 Reader。读取租用 4 KiB 缓冲，在 finally 归还，按实际读取字节限制大小，不信任 Length/CanSeek。
reload 仍可识别同长度、同时间戳但内容变化的外部定义。

专项 3 进程证据中 Catalog 首次使用约 33–45→12–13 ms，仅作为局部小样本；
512 B 外部读取的热态分配 83,590→1,584 B，不能用热池数据保证冷进程同幅度收益。

### 原免 XSD 方案的修正

ControlTheme 资产指纹校验的是导出主题、owner、绑定及资源 schema，不能证明主题定义 XML 满足 XSD。
avares 只是资源位置，也不自动意味着内容已经被相同 Reader/XSD 验证。
因此本轮是把特定自有来源的校验前移，不是关闭所有嵌入来源的校验。

构建期生成必须保留 XML/XSD 变更触发重建、无效输入使构建失败、旧生成产物失效及原诊断位置。
对外部来源继续保留超限、异常、摘要、无效定义诊断和回退；不能用流的报告长度替代真实读取上限。

## 问题 6：本地化 Snapshot 候选匹配

### 当前实现与小输入边界

[LanguageSnapshotBuilder](../../../src/AtomUI.Localization/LanguageSnapshotBuilder.cs)使用 MatchedBundleResolver，
按实际词条需要逐步扩展有序候选前缀；首个命中内联保存，立即命中时无需列表或扫描后续无关 bundle。
[LanguageCatalogRegistry](../../../src/AtomUI.Localization/Catalog/LanguageCatalogRegistry.cs)合并部分冲突/英文覆盖扫描，
保留原缺失词条诊断顺序。

候选枚举以来源优先级为外层，再处理语言候选，en-US 保持最终回退；
同语言同来源的非重叠 bundle 仍能互补。原方案的“候选语言×优先级”文字不应理解为可交换优先级。
所有支持语言 Snapshot 仍在启动时验证，不把翻译缺失推迟到用户首次切换语言。

局部分配证据：四语言 4817 词条场景 5,706,648→465,752 B，单词条 808→744 B；
这些阶段的耗时对噪声敏感，没有据此证明单语言可由 33 ms 降到 8–12 ms，
也不能把单语言耗时线性乘四作为 Gallery 多语言实测。

前一轮 macOS 的全量候选链方案曾因小 Catalog 提前命中产生额外分配而撤回；
Windows 后续保留的是按需扩展方案。原“全量字典索引＋覆盖位图”不作为当前实现描述。

## 六项以外的本轮改动

完整处置见[Windows 报告](2026-10-11-windows-startup-algorithm-results.md)，不能把整轮约 200 ms
收益仅归给本文件六个热点：

- Gallery 路由使用私有只读 78 路由元数据；记录的完整配置构建 7→1 次，应用独立持有配置。
  热态 RegisterViews 分配 241,708→157,476 B；ViewLocator.Map 累积复制仍保留，没有真实 Gallery 整体启动 A/B。
- 图片 scheduler 在首次入队时启动 worker pool，空闲 Task 数 26→0；保留构造时 ExecutionContext、
  取消/销毁和部分启动失败清理，首次请求仍要支付 worker 启动成本。
- 语言标签生成、Semantic Part 解析、配置 owner 合并和 Window 的一次无用反射读取也有定向减量。
  Window 的该项没有独立计时收益；不能把结构减量换算为启动毫秒数。
- Icon attach relay、Media 默认主题惰性 hook、部分缓存与调色板原型因兼容性或测量结果未保留，
  不作为待直接推广的成功模式。

## 下一轮的研究顺序与完成边界

1. 以已经优化的 Windows 产物重新固定基线，保留纯 Avalonia 对照；
   分清初始化、根 Render、真实呈现、首次交互，真实 Gallery 与空窗口分别采样。
2. 优先细分候选插桩中仍较重的包注册、资源挂载、首次主题应用。
   全部第二轮样本与嵌套关系见[分析文档](2026-10-10-startup-performance-analysis.md)，不能只挑较快两次或把父子区间相加。
3. 先证明成本来源与等价减量，再单因素 A/B；同时测首次资源查找、主题/语言切换、导航与弹层，
   避免把成本从启动挪到首次使用。原方案的各项固定毫秒收益预测全部撤下。
4. 保留公共 API、应用隔离、资源查找优先级、失败/诊断时机、AOT 与裁剪契约。
   按项目规范使用临时差分验证并清理，再做所改模块 focused 验证和必要真实发布；无收益或回归时撤回性能改动。

Windows 性能改善已被该场景样本支持；原生 Gallery 视觉验收、Windows NativeAOT 链接/运行、
3 项符号链接权限测试和原基线同样失败的 24 B 分配断言仍无补齐证据。
原 Windows 环境的临时证据目录清理状态见结果报告，本次未重新核实。
这些是验收范围限制，不表示本轮没有收益，也不能因提交已存在就宣布所有验收完成。
