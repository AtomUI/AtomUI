# 本地化生成与构建架构

本文定义 `AtomUI.Generator`、Toolchain 的 Tasks worker（`AtomUI.Build.Tasks.dll`）和 MSBuild targets 如何把 Catalog/XLIFF 转换为 AOT 友好的
运行时代码。运行时设计见 [runtime.md](runtime.md)，语言包协议见
[language-packs.md](language-packs.md)。

## 构建链总览

```mermaid
flowchart LR
    Enum["LanguageCatalog enum"] --> Generator["AtomUI.Generator"]
    Xlf["Project XLIFF"] --> Additional["AdditionalFiles"]
    ModulePack["Module package en-US XLIFF"] --> Additional
    Pack["I18n package target XLIFF"] --> Additional
    Reference["Referenced Catalog enum assembly"] --> Generator
    Additional --> Generator
    Tasks["AtomUI.Build.Tasks"] -->|"pack / manifest / props / export"| Xlf
    Generator --> Source["Generated descriptors / tables / extensions / bootstrap"]
    Source --> Assembly["Application or library assembly"]
```

MSBuild 负责发现文件、项目引用桥接、AdditionalFiles metadata 投影以及 pack/export 副作用；Generator 负责
编译项目中的输入规范化、Catalog/module 绑定、active/dormant 分类、Catalog/Bundle 语义校验和强类型代码生成。
静态语言包项目不生成运行时代码，因此仍由 `PrepareLanguagePackageTask` 在 pack 前完整校验。两条路径共享中立的
XLIFF 解析与文件级校验模型，但普通应用和模块编译不再运行重复的 MSBuild XLIFF 语义校验 Task。

## MSBuild items

`build/AtomUI.Localization.targets` 集中拥有输入发现、项目引用桥接、模板导出和打包流程。它自动收集标准目录中的
XLIFF，并排除 `bin/`、`obj/` 和生成目录：

```xml
<AtomUILanguage Include="**/Localization/**/*.xlf" />
<AtomUILanguageOverride Include="Localization/Overrides/**/*.xlf" />
```

targets 将这些 item 作为带元数据的 `AdditionalFiles` 传给 Generator。来源类型、source identity、module ID、
契约校验级别和源 fingerprint 必须保存在 item metadata 中。Generator 不从磁盘路径或 NuGet 包名猜测优先级或身份。

静态语言包项目由 targets 自动扫描目标 XLIFF，并以项目级 `AtomUILanguageModuleId` 作为唯一必需归属声明：

- 如果作者期组件引用提供了该 module 的权威 `ModuleBuiltIn` `en-US` assets，Build Tasks 按 XLIFF `file id` 绑定
  Catalog，补全 package path 和 source fingerprint，并把资产标记为 `Verified`。
- 如果该 module 完全没有权威源 assets，Build Tasks 只从目标 XLIFF 计算 package path 和 source fingerprint，把
  资产标记为 `Deferred`，并输出一次 `ATOMUILOC010` warning。
- 如果已经存在部分权威源 assets，则所有目标 Catalog 必须完整匹配；缺失或未知 Catalog 是 Error，不能逐文件退回
  `Deferred`。

语言包项目的契约严格程度由构建系统自动推断：存在普通（非 Analyzer）`ProjectReference` 时，表示作者声明了
一个作者期模块契约，缺少权威 `en-US` 资产会使 pack 失败；没有该引用的社区包默认允许 `Deferred`。

第三方语言包的 `buildTransitive/*.props` 只能追加声明式 item，不能运行初始化代码、修改应用源码或注册运行时
程序集。MSBuild item 层只排除相同文件的重复 Include；不同路径或不同包提供相同 Catalog/语言时，由 Generator
根据 source identity 报告同优先级冲突，不执行按 package identity 合并。

仓库内应用若需要直接消费源码语言包项目，使用显式的 `AtomUILanguagePackProjectReference` item，而不是复制语言包
目录或把 I18n 项目作为运行时 `ProjectReference`：

```xml
<AtomUILanguagePackProjectReference
    Include="../../src/LanguagePacks/pt-BR/AtomUI.Controls.I18n.PtBR/AtomUI.Controls.I18n.PtBR.csproj" />
```

语言包项目通过 `AtomUIGetLanguagePackProjectAssets` target 返回当前可用的权威 `en-US` Catalog 源文件和经过
`PrepareLanguagePackageAssetsTask` 校验并标记契约模式的目标语言文件。该轻量 Task 不扫描最终 NuGet 内容、
不写 manifest，也不生成 props；它只为源码项目引用准备 Generator 所需的编译期资产。消费项目的
`AtomUIResolveLanguagePackProjectReferences` target 在 `GenerateMSBuildEditorConfigFileShouldRun` 和 `CoreCompile` 之前调用
这些项目 target，并把 Generator 所需的 `StaticLanguagePack` source kind、source identity、module ID、
`AtomUILanguageContractValidation` 和 source fingerprint 投影到 `AdditionalFiles`。规范化 package path 只属于语言包
pack、manifest 和审计模型，不是 Generator 输入或 Catalog identity。
该协议只提供编译期输入，不复制 XLIFF、不产生运行时 DLL，也不改变聚合包的 NuGet 依赖图。

`AtomUILanguagePackProjectReference` 不跨普通 `ProjectReference` 传递。源码仓库中的最终应用宿主必须直接声明语言包
项目引用，并直接以 Analyzer 方式引用 `AtomUI.Generator`；具体 `Application` 类型必须是可生成 partial 实现的
`partial` 类型。只有这样最终应用 Generator 才能把外部 Translation Bundle 写入
`IGeneratedApplicationLanguageBootstrap`。把语言包项目引用放在应用类库、Shell 类库或控件类库中，不能替代最终
Desktop、Browser、测试宿主的声明。

产品级聚合语言包不追加 `AtomUILanguage` item。它只通过 NuGet 依赖传递模块语言包，因而同一模块包无论由聚合包
还是应用显式引用，都只产生一组 `buildTransitive` 输入。

普通模块打包只再导出 `SourceKind=ModuleBuiltIn`、`SourceIdentity` 属于当前 `AtomUILanguageModuleId` 的权威
`en-US.xlf`。依赖包提供的 Catalog 仍参与编译和契约验证，但不能被当前包复制并重新声明为自己的来源，
否则下游同时引用原包与当前包时会得到重复的内置 Bundle。

## Generator 输入

Generator 使用 Incremental Generator API 组合以下输入：

1. 当前 Compilation 中带 `[LanguageCatalog]` 的 enum symbol。
2. 当前项目 XLIFF `AdditionalText`。
3. 引用项目或 NuGet 程序集中的 `[LanguageCatalog]` enum symbol，以及模块主包携带的权威 `en-US` XLIFF。
4. 静态 I18n 包和应用 Override 提供的目标 XLIFF。
5. AnalyzerConfigOptions 提供的 `AtomUILanguageModuleId`、`PackageId`、`AssemblyName` 和 Generator 无法从 symbol
   推断的最小 source metadata。

输入必须按规范化 Catalog ID、语言标签、来源优先级和 unit Key 排序，确保不同操作系统、文件枚举顺序和增量
构建下生成结果一致。

## Generator 内部架构

`src/AtomUI.Generator/Localization` 只使用三层物理结构。目录表达稳定领域边界，不为每个流水线阶段建立
`Pipeline/`、`Model/`、`Symbols/` 或 `Semantics/` 子目录：

```text
Localization/
├── LocalizationGenerator.cs             # 注册 Incremental Generator 输入
├── LocalizationPipeline.cs              # 只编排阶段
├── LocalizationGenerationResult.cs      # 整体编译计划与诊断
├── LocalizationDiagnosticFactory.cs     # 共享诊断到 Roslyn Diagnostic 的适配
├── LocalizationSourceEmitter.cs         # 唯一源码输出入口
├── *SourceWriter.cs                      # 纯输出组件
├── Catalog/                              # Catalog symbol、索引、规划、语义和 Bundle 编译
└── Xliff/                                # AdditionalFiles 解析、metadata 校验和 typed 输入
```

核心模型使用结构化 `CatalogKey(ModuleId, FileId)`，不得使用拼接字符串作为字典 identity。`CatalogSymbolIndex`
在一次 Generator compilation 中建立一次当前及引用 module/Catalog 索引；Catalog 语义校验器只消费规范化
`CatalogDefinition`，不直接扫描 Roslyn `Compilation`。

Generator 流水线固定为：

1. 解析 AdditionalText XLIFF，保留源文件位置。
2. 校验所有输入都必须满足的语言标签、metadata、fingerprint 形态和当前文件 fingerprint。
3. 统一解析当前和引用 Catalog symbol，建立 `CatalogSymbolIndex`。
4. 保留 `Verified`/`Deferred` 契约模式，并独立分类 `Active`/`Dormant` 激活状态。
5. 按 `CatalogKey` 建立工作集并选择唯一权威 `en-US` 源契约。
6. 校验来源冲突、unit、source、placeholder、状态和权威 fingerprint。
7. 将已验证 unit 编译为 ordinal slot 数组和不可变 `LocalizationCompilationPlan`。
8. `LocalizationSourceEmitter` 调用 Catalog、module 和 application Writer 生成确定性源码。

`LocalizationPipeline` 只能协调这些阶段，不能重新实现具体规则。`CatalogSemanticValidator` 可以在同一文件中用聚焦
方法或内部策略组织规则；只有形成独立复用边界时才增加新 `.cs` 文件。Writer 的唯一输入是验证后的
`LocalizationCompilationPlan`，不得重新查找 Catalog、判断 dormant 或解决 Bundle 冲突。

### 静态语言包激活

Generator 在解析 `StaticLanguagePack` 的 Catalog 前建立当前项目和引用程序集的 Language Module/Catalog 索引。
目标 module 存在且 XLIFF `file id` 能绑定该 module 中唯一 Catalog 时，静态输入为 active，并执行全部严格校验。
module 存在但 Catalog 缺失、Catalog 属于其他 module 或 module identity 冲突都属于 Error，不得退回 dormant。

Generator 必须先解析并校验 XLIFF 2.1 结构、语言标签、校验级别、module ID 和 source fingerprint。基础输入有效后，
如果 module ID 不存在，该静态输入为 dormant。dormant 输入不进入 Catalog 深层校验、冲突检测、覆盖计算
或生成源码，因此聚合语言包不会要求应用安装所有组件。fingerprint metadata 的存在、64 位小写 SHA-256 格式及其与
当前目标 XLIFF source 内容的一致性仍必须通过；只有它与尚不可见权威 `en-US` fingerprint 的比较延迟到 active。
激活判断必须只依赖 Roslyn symbol 和显式 assembly metadata，不扫描程序集、不读取 NuGet 目录，也不从包名或文件
路径猜测模块。

当 module active 时，`Deferred` 输入按 `file id` 解析唯一 Catalog symbol，并使用模块权威 `en-US` 输入执行与
`Verified` 相同的 Catalog、unit、source、占位符、fingerprint 和完整覆盖校验。
无法唯一解析、模块存在但 Catalog 缺失或源契约不一致时必须报错。延迟的是校验阶段，不是最终应用的校验强度。

ModuleBuiltIn、项目本地 XLIFF 和 ApplicationOverride 不允许 dormant。模块存在但文件 ID、module ID、unit、source
或权威 `en-US` fingerprint 不匹配时仍产生原有诊断。这样可以跳过真正未安装的模块，同时保留对已安装模块和
损坏包的强校验。

## Generator 输出

对于每个 Catalog，Generator 生成：

```text
XxxLangResourceExtension（直接引用 Catalog enum 的强类型 Markup Extension）
```

对于每个 Language Module，Generator 生成：

```text
GeneratedLanguageModuleRegistration（一个 Data 常量与一次 AddModuleData 调用）
数据中的 Catalog/unit schema、enum 数值到 slot 映射及各语言译文
程序集 AtomUILanguageModuleId metadata
LanguageModuleManifestAttribute（版本、模块 id、同一 Data 常量）
```

对于最终应用，Generator 另外生成：

```text
GeneratedApplicationLanguageBootstrap
GeneratedApplicationLanguageData（I18n package 与 application Override 的数据常量及登记入口）
应用翻译数据对应的 LanguageModuleManifestAttribute
```

`XxxLangResourceKind` 由开发者声明，不再根据三份 C# 翻译类推导。enum 成员名是 Catalog 的唯一 Key，成员不声明
显式数字值；`XxxLangResourceExtension` 保留现有 XAML 形态，但底层通过模块数据目录索引和按需 Snapshot 查询。

目录 Markup Extension 仍按 Catalog metadata identity 独立输出。目录 schema、enum 数值到 slot 映射和内置译文
合并为模块数据常量；修改一个 XLIFF 只改变模块数据 source，未变化的目录 Extension 和应用 bootstrap 保持稳定。
改变 enum 成员声明顺序会改变编译期数值映射，数据必须同步变化；稳定 Key、译文槽排序和译文内容不受该顺序影响。
Generator 的增量测试同时验证输入 step 的 `Modified`/`Cached` 状态、必要的模块 source diff 和翻译语义。

## 应用 bootstrap

当项目存在本地化输入时，Generator 为唯一的具体、顶层、非泛型 `partial` Avalonia `Application` 类型实现一个
受控的生成式 host 契约。抽象 Application 基类不参与唯一性判断；没有 Application 的类库仍只生成模块注册，
不生成应用 bootstrap，也不回退到程序集扫描。`UseAtomUI()` 只做接口判断和直接调用：

```text
Application
  -> generated application module
  -> application Catalogs
  -> language-pack bundles
  -> application overrides
```

这不是程序集扫描：具体 Application 类型、生成类型和调用目标都在编译期可见。应用包含本地化输入但
Application 类型不可扩展时，Generator 必须产生诊断，不能回退到 `Assembly.GetTypes()`。

类库/控件包生成自己的 `GeneratedLanguageModuleRegistration`。包级 `UseXxx()` 入口调用生成注册代码，将该模块的
Catalog 和内置翻译交给 `ILocalizationBuilder`；开发者不手写 descriptor、Catalog ID 或 Provider 列表。打包时
`AtomUIPrepareLanguageModuleAssets` 自动把完整 `en-US` XLIFF 与 `<PackageId>.props` 放入主包，使最终应用可以用
引用程序集的 Catalog enum 校验外部翻译。

语言包没有程序集，其 Translation Bundle 由最终应用 Generator 生成。Bundle 可以早于或晚于目标 Language Module
进入 Builder，Registry 构建阶段统一关联，注册顺序不构成覆盖规则。

dormant 静态输入不生成 Translation Bundle。应用以后增加相应组件引用时，Incremental Generator 将其重新分类为
active，并在同一次编译中完成校验和生成；不需要新增运行时加载 API 或修改应用语言配置。

## AOT 约束

生成代码必须直接包含：

- 强类型 Extension 对 Catalog enum 的直接引用，以及数据中的 Catalog ID、程序集简单名和 CLR 元数据名。
- 编译期 enum 数值到 unit slot 的整数映射，以及 slot 对应的规范 Key 表。
- 各语言译文编码在单份版本化数据常量中；运行时索引保存有界切片，目录值行在首次查询时建立。
- 格式化标记和编译期验证过的格式文本；`CompositeFormat` 在目录首次物化时创建。
- Language Module 和应用 bootstrap 的直接注册调用。

正常路径禁止：

- `Assembly.GetTypes()`、`Type.GetFields()`、`Enum.GetNames()`。
- `Enum.GetName()` 或 enum `ToString()` 参与资源 Key 解析。
- 通过 Attribute 反射发现 Catalog。
- `Activator.CreateInstance()` 创建 Provider 或 Markup Extension 注册项。
- 运行时 XML/XLIFF 解析、路径 glob 或 NuGet 包探测。
- 依赖字符串类型名构造资源键。

## Toolchain 的 Tasks profile

物理项目与任务源码位于：

```text
src/AtomUI.Toolchain/AtomUI.Toolchain.csproj  # AtomUIToolchainProfile=Tasks
src/AtomUI.Toolchain/Tasks/
```

该 profile 继续产出内部 MSBuild worker `AtomUI.Build.Tasks.dll`，不作为应用运行时引用，也不单独发布
`AtomUI.Build.Tasks.LocalizationBuild`。主要 Task 为：

| Task | 职责 |
|---|---|
| `ExportLanguageTemplatesTask` | 按目标语言导出/更新可翻译 XLIFF 模板 |
| `PrepareLanguagePackageAssetsTask` | 为源码项目引用消费准备静态语言包资产，执行 final 状态、Verified/Deferred、fingerprint 和契约绑定校验，但不写 manifest 或扫描 NuGet 内容 |
| `PrepareLanguagePackageTask` | 完整校验静态语言包 XLIFF、单一 module/目标语言和包内容，绑定可用源契约，确定 `Verified`/`Deferred`，生成审计 XML manifest 和 contentFiles 清单 |
| `GenerateLanguagePackagePropsTask` | 为模块主包或静态语言包生成带契约校验级别的声明式 buildTransitive props |

Task 内部协作组件包括：

```text
src/AtomUI.Toolchain/Common/Localization/
Xliff21Parser
Xliff21Writer
XliffMergeEngine
LanguageFileValidation
LanguagePackageManifestWriter
PackagePropsWriter
```

Generator 与 Build Tasks 对 XLIFF 使用同一规范化模型、fingerprint 和文件级中立诊断。这些共享源码由
`AtomUI.Toolchain/Common/Localization` 物理拥有，保留 `AtomUI.Build.Tasks.LocalizationBuild` 命名空间，并以源码链接方式编译进
Generator；共享模型不依赖 Roslyn `Diagnostic` 或 MSBuild
`BuildEngine`。Generator 通过 `LocalizationDiagnosticFactory` 映射源位置和诊断描述符，Build Tasks 映射为
MSBuild error/warning。该目录不增加公开运行时包，也不让 MSBuild Task 依赖 Roslyn workspace。

普通应用和模块资源由 Generator 固定要求 publishable `translated`、`reviewed` 或 `final` target。静态语言包的
项目引用消费路径由 `PrepareLanguagePackageAssetsTask` 固定要求 `final`，pack 路径由
`PrepareLanguagePackageTask` 固定要求 `final`，都不暴露可降低要求的 MSBuild 属性。状态顺序为
`initial < translated < reviewed < final`，任何 `needs-review` subState 均不能满足语言包发布门禁。
两个 Prepare task 在完全缺少作者期源契约时报告一次 `ATOMUILOC010` 并生成 `Deferred` 资产；语言包项目存在普通
`ProjectReference` 时相同情况自动升级为 Error。

## Generator NuGet 布局

```text
AtomUI.Generator.nupkg
├── analyzers/dotnet/cs/AtomUI.Generator.dll
├── tools/net10.0/AtomUI.Build.Tasks.dll
├── tools/net10.0/AtomUI.Build.Tasks.deps.json
├── tools/net10.0/AtomUI.Build.Tasks.runtimeconfig.json
├── tools/net10.0/Microsoft.Build.Framework.dll
├── tools/netstandard2.0/System.Reflection.Metadata.dll
├── tools/netstandard2.0/System.Collections.Immutable.dll
├── tools/netstandard2.0/System.Memory.dll
├── tools/netstandard2.0/System.Buffers.dll
├── tools/netstandard2.0/System.Numerics.Vectors.dll
├── tools/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll
└── buildTransitive/
    ├── AtomUI.Generator.props
    ├── AtomUI.Generator.targets
    ├── AtomUI.Localization.props
    ├── AtomUI.Localization.targets
    └── AtomUI.ThemeAssets.targets
```

该树仅列本地化相关资产；完整后端/工具白名单由[构建与打包](../../foundations/build-and-packaging.md)定义。
`AtomUI.Build.Tasks.dll` 的依赖必须随 tools 目录完整发布。Generator 项目继续隔离 `PublishAot`、trim、single-file 和
RuntimeIdentifier 等全局发布属性，不能被最终应用当作运行时项目参与 NativeAOT publish。

`AtomUI.Localization.targets` 在 NuGet `_GetPackageFiles` 收集之前准备模块/语言包资产，保证动态加入的 XLIFF、
manifest 和 props 真正进入 `PackTask`。普通应用和模块编译时，targets 只做文件发现、AdditionalFiles metadata
投影和源码项目引用桥接；输入语义由 Generator 校验。静态语言包项目设置 `AtomUIBuildLanguagePackage=true` 后只由
Build Tasks 校验和打包；它自身不运行 Localization Generator 生成 Catalog 或运行时注册。相同 XLIFF 进入消费应用后
才由 Generator 编译为静态字符串表。

纯聚合语言包是单独的普通 pack 项目：`IncludeBuildOutput=false`，不设置 `AtomUIBuildLanguagePackage`，不调用上述
任务。源码项目中的模块语言包 `ProjectReference` 只作为仓库构建顺序边；.NET SDK pack 会把普通项目引用版本写成
最低版本范围，因此不能用它表达官方包要求的精确同版本依赖。聚合包必须使用除 NuGet 包 README 外无文件 payload
的自定义 nuspec 作为依赖图的唯一权威来源，并把每个模块语言包版本写成 `[$version$]`。聚合包自身不得向消费项目
传递 XLIFF、manifest、props、analyzer、AdditionalFiles、build targets、DLL、runtime asset 或组件包依赖。

源码仓库构建中，`AtomUI.Build.Tasks.dll` 可能在消费项目完成 MSBuild 求值之后才由 Generator 的项目依赖生成。
因此 targets 必须无条件登记 `UsingTask` 薄适配器，在实际执行时启动独立 worker；不得在 `UsingTask` 上使用
求值期 `Exists(...)` 条件。需要任务的 Target 仍在执行期检查工具是否存在，这样冷构建、静态图构建和
NativeAOT publish 都不会因“文件已经生成但任务未登记”而产生 `MSB4036`。

## 编译期与启动期校验边界

编译项目中的 XLIFF、Bundle 完整性、Catalog 契约、重复来源和语言包 props metadata 由 Generator 校验；静态语言包
项目的相同文件规则和 pack 契约由 `PrepareLanguagePackageTask` 校验。manifest 由已校验的同一组 XLIFF 确定性生成，
不是另一份编译输入。普通编译不得先运行 Build Task 再由 Generator 重复报告同一语义错误。

生成模块以 `LanguageModuleManifestAttribute(version, moduleId, data)` 暴露完整目录与译文元数据。
应用链接器必须结合最终支持语言、所有模块 manifest、静态语言包和应用 override 验证每个 unit 的覆盖、来源冲突和
最终 en-US 源文本。只有通过这层校验的生成数据才可以走运行时的按需快照路径；单个模块编译成功不表示应用所有语言
已闭合。手工 descriptor 继续由运行时检查注册闭合、优先级冲突、en-US 源文本与支持语言覆盖。
生成目录只要含手工 Bundle 贡献，其格式与支持语言覆盖也必须在 Build 时检查；仅目录和全部 Bundle 都来自生成数据时，才跳过运行时逐值校验。

### 模块数据传输协议 v1

`LanguageModuleManifestAttribute` 允许一个程序集含多个实例，只包含 `int Version`、`string ModuleId`、
`string Data`。运行时 `AddModuleData(string moduleId, string data)` 只接受唯一模块 id；重复登记明确失败。
解析器 `LanguageModuleDataCodec` 的同一份纯 BCL 源码用于 Generator、应用链接器和 Localization 运行时。

数据以 ASCII `AUIL1;` 开始。非负整数以十进制数字加 `:` 终止；字符串是 UTF-16 code unit 长度、`:`、原文，
可空译文以 `-1:` 表示，空译文以 `0:` 表示。字段不使用分隔符转义，冒号、换行、引号、反斜杠及 Unicode 均保真。
模块总长上限 16 Mi UTF-16 code units，单个记录集合上限 65,536，所有长度/数量/枚举槽在访问前验证，尾随数据拒绝。

字段顺序如下：

1. 模块 id 字符串。
2. 目录数；每个目录依次是 catalog id、程序集简单名、CLR 元数据名（嵌套类型用 `+`）、unit 数。
3. 每个 unit 依次是稳定 Key、编译时 enum 数值、格式化标记（0 或 1）；unit 以 Key ordinal 排序，数值必须唯一且落在目录范围内。
4. bundle 数；每个 bundle 依次是 catalog id、规范 BCP 47 language、source kind（0 内置、1 静态包、2 应用覆盖）、source identity、值数。
5. 每个值是可空字符串，槽顺序与对应目录的 unit 顺序相同。bundle 值数必须等于被引用目录的 unit 数。

模块拥有的目录及内置译文合并为单个 `const string Data`，`GeneratedLanguageModuleRegistration.Register` 仅调用一次
`builder.AddModuleData(moduleId, Data)`。应用的外来 bundle 单独生成 `GeneratedApplicationLanguageData`，模块 id 为
`<AssemblyName>:ApplicationTranslations`，目录数为零，由应用 bootstrap 登记；其 manifest 同样提供链接期覆盖信息。
运行时解析只保留整数切片，不生成全部译文字符串数组；只在实际查询目录时提取匹配语言的文本。

## 导出与合并

标准命令使用 MSBuild target，不创建独立 CLI：

```bash
dotnet msbuild \
  -t:AtomUIExportLanguageTemplates \
  -p:AtomUITargetLanguage=ja-JP
```

重复导出必须确定性合并：新增 unit 加入目标文件，删除 unit 标为 obsolete，源文本变化将目标标记为需复核，
已有 target、state 和 notes 保留。工具不得静默删除译文或把旧译文视为新源文本的已确认翻译。导出 target 必须至少
发现一个权威 `en-US` 源资产；没有作者期组件引用时输出“添加 `PrivateAssets=all` 组件 PackageReference”的可操作
提示，而不是从目标 XLIFF 反向伪造模板。
