# 本地化 Generator 优先构建设计

> 本文是本次 Generator 优先重构的补充决策记录，不是多语言模块的正式架构权威。长期设计、职责边界和不变量以
> [`docs/modules/localization/`](../../modules/localization/overview.md) 为准；实现过程中形成的新结论必须先同步到正式
> 模块文档，不能只修改本文。

## 目标

将尽可能多的本地化语义从 `build/AtomUI.Localization.props` 和
`build/AtomUI.Localization.targets` 下沉到 `AtomUI.Generator`，同时保持 MSBuild 文件简洁，
并保留现有工作流的核心语义：本地 XLIFF 编译为静态运行时数据，语言包项目生成声明式包资产，
项目引用可以提供源契约，模板导出仍然通过 MSBuild 命令完成。

这是一次有意的构建契约重设计。不要求兼容当前 targets 使用者，现有 MSBuild 属性和 metadata
可以删除或重命名。

## 非目标

- 不让运行时解析 XLIFF 或动态检查程序集。
- 不让 Source Generator 向 `obj`、`bin` 或 NuGet 包任意写文件。
- 不用反射、文件系统探测或包目录扫描替代 MSBuild 的项目间 target 协议。
- 除了把语义校验迁移到 Generator 所必需的部分，不改变现有运行时注册模型。

## 职责划分

### `AtomUI.Generator`

Generator 成为**编译项目中的 Catalog/Bundle 语义权威**，负责：

- XLIFF 2.1 解析以及带源文件位置的诊断。
- 语言标签、翻译状态、占位符、源文本和完整 Bundle 校验。
- 根据当前和引用程序集中的 Roslyn symbol 解析 Catalog/module。
- `Verified` 与 `Deferred` 契约处理，包括引用模块出现后的 Deferred 激活。
- source fingerprint、稳定 Key 和源文本校验。
- 重复翻译来源和 override unit 冲突检测。
- 文件、Catalog、Bundle 和生成源码的确定性排序。
- 生成 Catalog descriptor、Translation Bundle、模块注册和 Application bootstrap。

语义处理流水线应拆分为可复用的解析、规范化、符号索引、输入分类、语义校验和编译计划步骤，
并接入现有增量生成流程。这里的“权威”只覆盖编译项目中的 Catalog/Bundle 绑定和翻译语义，
不表示 Generator 取代静态语言包 pack 所需的 Build Task。

Generator 必须保持 AOT 友好，禁止使用反射、运行时 XML 解析、路径 glob 或包探测。

### `AtomUI.Build.Tasks`

Build Tasks 只保留必须使用 MSBuild 或产生文件系统副作用的操作：

- `PrepareLanguagePackageTask`：校验和准备静态语言包，存在权威源契约时完成绑定，计算包路径和
  fingerprint，并写入 manifest 输入。
- `GenerateLanguagePackagePropsTask`：写出语言包声明式 `buildTransitive` props。
- `ExportLanguageTemplatesTask`：将权威 `en-US` 源 Catalog 合并到目标语言模板。
- 如果没有独立使用者，可以将 `CollectLanguageCatalogsTask` 合并进导出流程。

普通模块/应用编译不再执行 `ValidateLanguageFilesTask`。静态语言包项目仍通过
`PrepareLanguagePackageTask` 执行校验，因为这类项目不会运行用于生成运行时代码的本地化 Generator。

XLIFF parser、规范化文档模型、fingerprint 计算、文件级校验规则和中立诊断模型继续放在
`src/AtomUI.Build.Tasks/LocalizationBuild`，由 `AtomUI.Build.Tasks` 物理拥有，并由 Generator 通过源码链接复用。
共享层不得依赖 Roslyn `Diagnostic` 或 MSBuild `BuildEngine`；Generator 和 Build Tasks 分别将中立诊断映射到
自己的输出机制。

### 第三方语言包项目

第三方作者是正式支持的构建场景，不应依赖官方模块包的特殊路径。第三方静态语言包项目没有运行时程序集，
也不需要编译 Catalog；它只需显式声明 package ID、目标语言和 module ID，然后由 pack targets 校验并打包 XLIFF。

支持两种作者模式：

- **Verified 模式**：项目以仅用于创作的 `PackageReference` 引用目标组件包，并设置 `PrivateAssets=all`。
  Build Tasks 从该包读取权威 `en-US` XLIFF 和 Catalog metadata，将每个目标文件绑定到唯一 Catalog，计算权威
  source fingerprint，并输出 `Verified` 包 metadata。
- **Deferred 模式**：作者无法取得权威源契约。Build Tasks 仍校验 XLIFF 结构、目标语言、翻译状态、包内容和
  规范化路径，输出 `Deferred` metadata 并给出 warning。该包可以发布，契约校验延迟
  到消费应用完成。

消费应用中的 Generator 对两种模式拥有最终权威：

1. 读取语言包生成的声明式 `buildTransitive` 输入。
2. 根据当前或引用组件程序集中的 module identity 和 Catalog enum，将目标文件的 `file id` 解析到 Catalog。
3. 如果模块存在，激活 Bundle，并完整校验源文本、unit、占位符、状态和 fingerprint。Deferred
   输入一旦激活，不得降低校验强度。
4. 如果模块不存在，静态输入保持 dormant，不生成 Bundle，也不报告依赖目标 Catalog 的深层语义错误；
   XLIFF 结构、语言标签、metadata 和 fingerprint 等基础输入规则仍必须通过。

因此，第三方包可以在无法引用所有可选 AtomUI 组件时先行发布；应用真正安装组件后，仍会得到严格的编译期诊断。
一个语言包必须只对应一个 `AtomUILanguageModuleId`；多模块翻译应拆成多个包，或另建显式的聚合 Meta Package。
聚合包只携带依赖，不得传递 XLIFF、localization `buildTransitive` item、analyzer 或运行时资产。

### `AtomUI.Localization.props`

props 文件只包含稳定默认值和 item 定义：

- module identity 回退值（优先 `PackageId`，其次 `AssemblyName`）；
- module-built-in 与 application-override item 的默认 metadata。

props 不维护并行版本字段；Generator 始终以真实 Catalog symbol、权威 `en-US` 和 source fingerprint 为语义权威。

不在 props 中定义校验策略、target 顺序、Task 路径或打包行为。

### `AtomUI.Localization.targets`

targets 文件压缩为三个职责域。

1. **输入投影**
   - 用一个规范化 item 流发现本地 XLIFF 和 override 文件。
   - 排除输出目录、中间目录和生成目录。
   - 将本地输入以及项目引用解析出的资产投影为 `AdditionalFiles`，只保留 Generator 无法从 symbol 推断的最小 metadata。
   - 只暴露必需的 compiler-visible item metadata 和项目属性。

2. **项目引用桥接**
   - 保留一个小型 provider target，返回模块权威 `en-US` 源资产。
   - 保留一个小型 consumer target，为显式语言包项目引用调用 provider，并在 editorconfig 生成和编译前将返回资产加入
     `AdditionalFiles`。
   - 该协议必须保留在 MSBuild 中，因为 Source Generator 无法在当前求值阶段调用另一个项目的 target。

3. **Pack/export 钩子**
   - 保留调用三个副作用 Build Task 的 pack 准备 target。
   - 保留公开的 `AtomUIExportLanguageTemplates` MSBuild 命令。
   - 删除编译期校验 target、重复的 metadata 转发块和可配置的 minimum-state 属性。普通编译输入固定要求
     `translated`，静态语言包准备固定要求 `final`。

尽量通过 `ItemDefinitionGroup` 只声明一次 item metadata，并用一个合并的 `AdditionalFiles` item 转发 module 与
override 输入，避免目前的重复 Include 块。

## 输入与诊断流

```text
MSBuild glob/项目引用桥接
  -> AdditionalFiles + 最小 source metadata
  -> 增量 Generator 解析并规范化 XLIFF
  -> Catalog symbol index
  -> Verified/Deferred 契约模式规范化
  -> Active/Dormant 激活状态分类
  -> 按 CatalogKey 建立编译工作集
  -> 源契约、来源冲突、unit 和翻译值校验
  -> 有效的 active Catalog 生成确定性源码
```

`Verified`/`Deferred` 与 `Active`/`Dormant` 是两个正交维度：前者描述语言包是否已在 pack 阶段绑定权威源契约，
后者描述消费项目当前是否包含目标 module。不得用单一枚举混合表达这两类状态。

即使某个 Catalog 最终不生成源码，Generator 也必须报告其诊断。唯一的有意例外是 dormant 静态输入：在对应模块
存在前只跳过依赖目标 Catalog 的深层语义校验。此时仍校验 fingerprint metadata 的存在性、格式和它与当前 XLIFF
源内容的一致性，但不能校验它是否等于尚不可见的权威 `en-US` fingerprint；一旦激活，Deferred 输入必须执行与
Verified 输入相同的严格契约和 Bundle 校验。

## Generator 多语言内部架构

目录表达稳定的领域边界，不为每个流水线阶段建立单独文件夹。保留现有的 `Catalog/` 和 `Xliff/`
两个领域目录，流水线编排、诊断适配和源码输出放在 `Localization/` 根目录：

```text
src/AtomUI.Generator/Localization/
├── LocalizationGenerator.cs
├── LocalizationPipeline.cs
├── LocalizationGenerationResult.cs
├── LocalizationDiagnosticFactory.cs
├── LocalizationSourceEmitter.cs
├── LanguageCatalogSourceWriter.cs
├── LanguageModuleSourceWriter.cs
├── ApplicationLanguageBootstrapWriter.cs
│
├── Catalog/
│   ├── LanguageCatalogInfo.cs
│   ├── LanguageCatalogSymbolParser.cs
│   ├── CatalogSymbolIndex.cs
│   ├── CatalogCompilationPlanner.cs
│   ├── CatalogSemanticValidator.cs
│   ├── TranslationBundleCompiler.cs
│   └── CompiledLanguageCatalog.cs
│
└── Xliff/
    ├── AdditionalLanguageFile.cs
    ├── AdditionalLanguageFileParser.cs
    ├── LanguageFileMetadataValidator.cs
    └── LanguageGeneratorOptions.cs
```

不新增运行时 public API。仅供生成源码使用的 marker 或 helper 保持 `internal` 或仅存在于 Generator 中。

### 结构化输入模型

语义层不使用 `$"{moduleId}:{fileId}"` 形式的隐式字符串键，使用不可变的 `CatalogKey(ModuleId, FileId)`。
主要输入模型包括：

- `CatalogDefinition`：规范化后的 Catalog/module、类型名、unit 和所有权信息。
- `LanguageFileInput`：规范化后的 XLIFF 文档、来源类型、source identity、声明的契约信息和 fingerprint。
- `CatalogSymbolIndex`：按 `CatalogKey` 索引当前及引用程序集中的 Catalog，并记录可用 module。
- `LanguageInputResolution`：记录输入是 `Active` 还是 `Dormant`，以及绑定后的 Catalog；它保留
  `LanguageFileInput` 中独立的 `Verified`/`Deferred` 契约模式，不把两类状态折叠成一个枚举。
- `CatalogCompilationPlan`：只包含已经完成来源选择、冲突检查和翻译 slot 编译的 Catalog 结果。
- `LocalizationCompilationPlan`：源码 Writer 的唯一输入。

这些类型均为不可变内部模型。语义校验器不直接依赖 AdditionalText 枚举顺序，也不修改输入对象。
为避免只有少量类型的模型文件继续扩散，`CatalogKey` 和 `CatalogDefinition` 放在 `LanguageCatalogInfo.cs`，
`LanguageFileInput` 和 `LanguageInputResolution` 放在 `AdditionalLanguageFile.cs`，Catalog 编译计划放在
`CompiledLanguageCatalog.cs`，整体计划放在 `LocalizationGenerationResult.cs`。

### 流水线职责

`LocalizationGenerator.cs` 只负责注册 Incremental Generator 输入；`LocalizationPipeline.cs` 负责按以下顺序
编排阶段，但不实现具体校验规则：

1. `AdditionalLanguageFileParser` 解析 XLIFF 并读取 compiler-visible metadata。
2. `LanguageFileMetadataValidator` 执行所有输入都必须满足的文件级规则。
3. `LanguageCatalogSymbolParser` 将当前和引用 Catalog symbol 归一化为 `CatalogDefinition`。
4. `CatalogSymbolIndex` 一次性建立 module/Catalog 索引，避免每个语言文件重复扫描 Compilation。
5. 输入分类器根据 source kind、module 和 Catalog 索引决定 `Active`、`Dormant` 或错误；`Verified`/`Deferred`
   已由输入模型独立记录。dormant 只跳过依赖目标 Catalog 的深层语义校验。
6. `CatalogCompilationPlanner` 按 `CatalogKey` 建立工作集，并选择唯一权威 `en-US` 源契约。
7. `CatalogSemanticValidator` 依次执行源契约、Bundle 来源冲突、unit、source 文本、placeholder、状态和 fingerprint 校验。
8. `TranslationBundleCompiler` 将已验证的 unit 映射成稳定的 ordinal slot 数组。
9. `LocalizationSourceEmitter` 调用现有三个 Writer 生成 Catalog、module registration 和 application bootstrap。

`CatalogSemanticValidator` 可以在一个文件中协调多个小的验证方法或内部策略类型；只有当某个规则需要独立测试、
独立复用或明显形成稳定策略时，才拆成新的 `.cs` 文件。目录拆分不应跟随方法拆分。
输入分类器作为 `CatalogCompilationPlanner.cs` 中的内部协作类型实现，不增加只有单一小类型的独立文件。

### Catalog 与引用解析

当前按文件调用 `ReferencedLanguageCatalogResolver` 的方式改为一次建立 `CatalogSymbolIndex`。索引负责：

- 当前程序集和引用程序集中的 `[LanguageCatalog]` enum 识别；
- module identity 和稳定 Catalog Key 读取；
- `CatalogKey` 到 CatalogDefinition 的唯一映射；
- module ID 或 Catalog ID 冲突诊断；
- 判断静态语言包是否真正拥有可激活的目标 module。

符号读取细节限制在 `Catalog/` 目录，`CatalogSemanticValidator` 只消费规范化模型，不直接操作 Roslyn Compilation。

### 诊断边界

共享的 XLIFF/Build 层只产生不依赖 Roslyn 和 MSBuild 的中立文件级诊断模型；Generator 通过
`LocalizationDiagnosticFactory` 将它映射成带 `AdditionalText` 文件位置的 Roslyn Diagnostic。
语义校验器返回诊断集合，不直接调用 `SourceProductionContext.ReportDiagnostic`，也不写生成文件。

诊断统一按文件路径、行、列、阶段、Diagnostic ID 和消息排序，确保增量构建、不同操作系统和文件枚举顺序下的输出一致。

### 输出层

`LanguageCatalogSourceWriter`、`LanguageModuleSourceWriter` 和 `ApplicationLanguageBootstrapWriter` 保持为纯输出组件。
它们不得重新执行 Catalog 查找、契约校验、dormant 判断或 Bundle 冲突处理，只消费 `LocalizationCompilationPlan`。

### 复杂度定位

该架构会增加若干内部类型和 `.cs` 文件，但这是有意的结构化复杂度，不是运行时复杂度，也不是设计缺陷。
收益是每个阶段可以单独测试和替换，新增来源类型或校验规则不会再次扩张 `LocalizationGenerator.cs` 或形成新的巨型
`LanguageCatalogCompiler`。真正禁止的是职责重新集中，而不是类型数量增加。

## 错误处理

- XLIFF 或 metadata 无效时，生成带文件位置的 Source Generator diagnostic。
- 没有源契约的静态包只有在 pack 过程标记为 `Deferred` 时才允许继续；消费端 Generator 决定它是 dormant 还是 active。
- 模块只能部分解析时报告错误；不能对同一模块的不同文件静默降级为 Deferred。
- 不吞掉诊断以生成部分源码。只有完整语义编译成功的 Catalog 才生成源码。

## 测试策略

### Generator 测试

扩展并按职责拆分 `tests/AtomUI.Generator.Tests/Localization`，覆盖：

- 原 `ValidateLanguageFilesTask` 覆盖的全部 XLIFF 结构和翻译诊断；
- module/source identity 默认值和显式 metadata 覆盖；
- 重复 Bundle 和重复 override unit；
- Verified/Deferred 与 fingerprint 路径；
- dormant 静态输入及通过引用 symbol 激活 Deferred 输入；
- 确定性输出和增量缓存行为。

测试类按生产代码职责组织，避免继续扩张单个 `LanguageCatalogCompilerGeneratorTests`：

```text
LanguageFileInputParserTests
LanguageFileMetadataValidatorTests
CatalogSymbolIndexTests
LanguageInputClassifierTests
CatalogSemanticValidatorTests
TranslationBundleCompilerTests
LocalizationPipelineTests
LocalizationSourceEmitterTests
LocalizationGeneratorDeterminismTests
```

Parser/metadata 测试不需要完整 Roslyn Compilation；Symbol Index 测试只构造最小 Compilation；
Pipeline 和 Source Emitter 测试验证端到端诊断与生成结果。所有输入顺序相关测试必须打乱 AdditionalFiles
和 Catalog 顺序，验证诊断及生成源码仍然确定。

### Build Task 测试

保留以下聚焦测试：

- 包内容安全和规范化包路径；
- Verified/Deferred 包准备及 manifest 输出；
- 生成的包 props；
- 模板导出和项目引用资产返回。

### 集成测试

更新端到端语言包 fixture，验证：

- 普通模块/应用构建依赖 Generator 诊断；
- 静态语言包项目中的无效内容仍在 pack 前失败；
- 消费应用编译 active Bundle，并忽略 dormant 可选包；
- 第三方 Verified 包能绑定引用模块契约；
- 第三方 Deferred 包发布时只在 pack 报告一次 warning；模块安装后激活并校验，模块缺失时保持 dormant；
- 聚合 Meta Package 不泄漏本地化输入或运行时资产；
- 生成包资产保持确定性。

最终验证命令：

```bash
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --no-restore
dotnet test tests/AtomUI.Build.Tasks.Tests/AtomUI.Build.Tasks.Tests.csproj --no-restore
dotnet test tests/AtomUI.Localization.IntegrationTests/AtomUI.Localization.IntegrationTests.csproj --no-restore
git diff --check
```

## 验收标准

- `build/AtomUI.Localization.props` 只包含默认值和 item 定义。
- `build/AtomUI.Localization.targets` 集中拥有输入、项目引用、导出和打包流程，且只有一条合并的本地化输入投影路径。
- 对编译项目而言，Generator 是唯一的 Catalog/Bundle 语义校验器；静态语言包 pack 的文件和包契约仍由 Build Tasks 负责。
- 静态语言包的 pack/export 行为仍有明确的 Build Task 覆盖。
- 第三方 Verified 和 Deferred 包流程都有端到端测试。
- 不引入运行时反射、运行时 XLIFF 解析或生成源码非确定性。
- 所有聚焦测试及 `git diff --check` 通过。
