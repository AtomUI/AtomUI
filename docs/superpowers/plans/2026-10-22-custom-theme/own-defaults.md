# Own Token 版本、生成与Compiler实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: 使用superpowers:executing-plans在当前会话逐项执行，不自动委派实现或提交。步骤使用checkbox记录；总状态只更新[tasks.md](tasks.md#0-执行进度)。

**Goal:** 完成CT-03–CT-05的显式版本化Own依赖默认值，使未覆盖下游读取有效上游，保留显式0/null和旧evaluator行为。

**Architecture:** 新版终端实现生成静态入口的接口；Roslyn读取声明/元数据，生成拓扑顺序及稳定revision。Compiler先写显式值并建立独立存在位，再调用新版计划；旧版流程不变。Global为完整强类型只读投影，无运行时图、反射或逐Token闭包。

**Tech Stack:** 现有.NET8/10、C#/Roslyn增量生成器、xUnit/Shouldly、Core Compiler及既有注册/TypeMap/trim管线。

**Spec:** [Own具体接口](../../../architecture/systems/theming/own-token-evaluation.md#9-ct-03具体构建与运行接口)、[生成设计](../../../modules/generator/own-token-defaults.md)、[路线图](overview.md)及[正式验收](../../../engineering/development/theming/verification.md)。M0审查见[readiness](readiness.md#m0审查结论)。

## Global Constraints

- 继续当前`feature/custom-theme`及worktree，不创建其他分支、worktree或clone；不自行commit。
- 保持exact identity、按名称Own slot、独立惰性Control槽、最终Snapshot唯一真源及深度不可变资源边界。
- 旧整体evaluator与新依赖默认值合同整链分版，不在同一终端定义链混跑；XML v1和编译ABI v1不静默扩义。
- 默认函数只接收Global/appearance只读上下文和明确Own参数，不推断任意方法体数据流或构建期执行函数。
- 本计划不迁移95个产品Token，不改Button/其他Control的C#/AXAML、默认外观或已有配置表现。CT-12等家族另采同条件前证后迁移；本批验证共享机制，不宣称B02真实按钮已修复。
- 新增验证默认Temporary；验证后删除，最终只跑实际修改模块的精确filter，不擅自执行change/full。任何保留的共享契约保障必须说明独立价值、全局必要性及成本。
- 高频采样离屏；原生显示面闪动仍由CT-12/CT-25专项处理，不用本批纯计算结果代替视觉验收。

## Review Focus

| 输入/故障 | 预期及负责步骤 |
| --- | --- |
| 旧Generator处理新版Attribute却忽略参数 | 终端缺少IControlTokenDefaults静态实现时C#编译失败，不静默WholeV1；CT-04A/B |
| 显式0、合法null及上游变化 | 存在位独立；默认函数不覆盖显式下游，移除覆盖恢复派生；CT-05A |
| 跨程序集protected默认或显式重定义 | 静态直接调用可访问方法，未知/重复/类型/环明确诊断；CT-04B |
| 默认语义/revision变而schema不变 | evaluator摘要/缓存隔离，Bind不丢版本，Own slot和schema不变；CT-04B/CT-05B |
| 新旧版本共存、未查询Control或默认函数抛错 | 整链版本分离，仍惰性求值、失败可重试及成功复用，不能全量实例化；CT-05B |

## 文件职责

| 区域 | 文件与职责 |
| --- | --- |
| 构建声明 | `src/AtomUI.Core/Theme/DesignTokens/ControlDesignTokenAttribute.cs`扩展版本/revision；新增`ControlTokenEvaluationVersion.cs`、`ControlTokenDefaultAttribute.cs`、`IControlTokenDefaults.cs` |
| 只读输入 | 新增同目录`ControlTokenDefaultContext.cs`、`ControlTokenGlobalValues.cs`；后者的全部Global getter由Generator生成，引用值沿现有typed codec复制 |
| 运行分派 | `src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs`新增版本/revision/静态delegate入口，保留旧构造与Evaluate；delegate统一放`IControlTokenDefaults.cs` |
| 构建模型 | 新增`src/AtomUI.Generator/DesignToken/ControlTokenDefaultModel.cs`负责版本/声明/类型/可访问性/图验证；稳定结果放`ControlTokenDefaultPlan.cs`，无跨Compilation symbol |
| 构建接线 | `ControlTokenPropertyWalker.cs`/`TokenInfo.cs`接入计划，新版跳过旧base调用校验但拒绝手写整体方法；`TokenResourceKeyGenerator.cs`接新增writer |
| 生成产物 | 新增`ControlTokenDefaultsWriter.cs`、`ControlTokenGlobalValuesWriter.cs`；`ThemeSchema/GeneratedThemeSchemaWriter.cs`分支生成新版descriptor；旧分支保持 |
| 诊断 | `Diagnostics/AtomUIDiagnosticIds.cs`、`AtomUIDiagnosticDescriptors.cs`集中分配`ATOMUIGEN040`版本/终端、`041`声明/覆盖/访问、`042`签名、`043`依赖；正式诊断规范同步 |
| Compiler | `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs`在既有单Control首次求值路径分版；`ThemeCacheKey.cs`补版本/revision |
| 验证 | 所属Core/Generator测试项目`Temporary/OwnDefaults`；跨包/trim/AOT临时工程放系统临时目录，不新增永久fixture工程 |

## Task CT-03：冻结具体接口和迁移边界

**Interfaces:** 产出正式Own设计第9节的类型/签名；CT-04/05仅使用该合同，不再各自定义一个默认上下文或Token目录。

- [x] **Step 1：核对现行路径及最小失败。** `ControlTokenDescriptor`只有整体Action；`ThemeCompiler.CompileControlCore`先Evaluate后ApplyOwn；B02是实际派生失败前证。
- [x] **Step 2：定义具体接口。** 正式Own设计已规定Attribute、上下文/完整Global只读视图、静态接口/delegate、descriptor重载、位集与revision；所有旧签名保留。
- [x] **Step 3：自审并更新交接。** 对照旧路径、跨包metadata、旧生成器拒绝、0/null、缓存和格式逐项核对；诊断编号确认未占用。纯文档检查及`git diff --check`通过后将CT-03标完成，再进入CT-04。

## Task CT-04A：可用的版本化声明与分派合同

**Interfaces:** 创建正式设计第9节列出的public类型。`ControlTokenDefaultContext`内部构造`(DesignToken effectiveGlobal, ThemeAppearance appearance)`；`ControlTokenGlobalValues`内部构造`(DesignToken source)`，仅私有字段/读入口持有builder。descriptor新增六参数构造及`EvaluateDefaults`，旧版版本为WholeEvaluatorV1/revision0。

- [x] **Step 1：临时合同RED。** Core Temporary测试真实构造旧/新descriptor：旧版仍Evaluate；新版对旧入口抛错，反之亦然；新版revision0、错误mask长度/未用高位拒绝。使用两个Own slot，对空mask和第0位显式的delegate观察实际入参，不只断言类型存在。修改返回的brush不能改变context.Global对应输入；完整Global getter行为在CT-04B补齐。
- [x] **Step 2：运行RED。** `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.OwnDefaults.DescriptorContractTests`。接口不存在的编译失败只用于确认合同缺失，不当作产品Bug复现；类型可编译后必须执行非零有效断言。
- [x] **Step 3：实现最小合同。** Attribute无参兼容；新版链revision正数校验由Generator负责，运行descriptor拒绝revision0。存在位空span为全部未指定，非空精确word数/高位校验。Bind同时保留两个版本的delegate、版本/revision及CompilationIdentity。Global值源只在私有成员中，不给用户函数取得mutable builder的入口。
- [x] **Step 4：GREEN并复核旧对照。** 上述filter及既有`ThemeSchemaRegistryTests`、`ControlTokenIdentityOwnershipTests`；必须真实发现，无skip。旧注册factory没有改成新版。

## Task CT-04B：声明验证、跨包元数据及静态计划

**Interfaces:** `ControlTokenDefaultModel.Build(INamedTypeSymbol terminal, IReadOnlyList<ControlTokenLayerInfo> layers, Compilation compilation, CancellationToken cancellationToken, ICollection<Diagnostic> diagnostics, out bool isDependencyDefaults)`返回`ControlTokenDefaultPlan?`。诊断写入本次调用持有的集合；稳定计划包含evaluator revision、每步目标slot/名称、完整方法类型/名称和Own字段名称，类型经验证并纳入摘要，字段类型仍由既有schema承载；不保存symbol。writer从同一计划生成终端静态入口和新版descriptor。

- [x] **Step 1：临时行为RED。** Generator Temporary用当前Core/Avalonia真实引用编译一个三字段Token，属性声明乱序，默认链Font→LineHeight→Padding。生成后Emit并执行静态入口，显式Font=20时下游40/相应padding，显式Padding=7保留；空mask使用默认，slot仍按名称。没有新版支持时生成/运行必须失败，不能仅检查字符串。
- [x] **Step 2：建立负例。** 逐项断言实际diagnostic ID和目标Token：未知版本/混合版本/Revision0/终端非partial或缺接口/手写入口或Calculate；缺默认/重复/虚假替换/不可访问；错误context/ref/Own参数/返回类型；未知Own和A→B→A环。无效终端不输出可用部分计划。无需生成所有维度笛卡尔积。
- [x] **Step 3：实现模型与writer。** 每层取本层Attribute；旧版继续原validator。新版读取方法Attribute参数，不读方法体推断依赖。用确定拓扑排序，按名称slot赋值；直接调用强类型函数，先判断存在位。跨包方法完整身份和访问性经Roslyn核定。Core只读Global writer投影全部有效字段，经typed codec复制引用值。
- [x] **Step 4：GREEN及确定性/跨包。** 用两个动态Compilation验证public/protected抽象层导出到终端、显式默认重定义；删生成器的对照必须因静态接口缺实现而编译失败。改变属性声明顺序输出相同；改变层revision摘要不同而字段/slot不变；只移动字段声明层schema不变。完整Global视图的输入/输出字段集合相等，brush/easing复制不反向修改Global。
- [x] **Step 5：精确既有验证。** `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Temporary.OwnDefaults|FullyQualifiedName~ThemeSchemaGeneratorTests|FullyQualifiedName~TokenResourceKeyGeneratorTests'`。检查实际发现数/结果；诊断规范及生成文档同步。
- [x] **Step 6：真实构建合同验证。** 系统临时项目引用当前Core和Generator，生成一个跨包默认链并真实编译；trim/NativeAOT运行至少一个有依赖/显式值的入口并核对数值，未使用Token构造计数0。对应产物与命令/平台写文本，不能以Roslyn Emit代替trim/AOT。

## Task CT-05A：Compiler有效Own与存在位

**Interfaces:** 沿`CompileControl`现有Builder和最终Effective Global/appearance，在新版分支先`ApplyAllValues(config.OwnTokens)`，按normalized descriptor slot设置存在位，再`descriptor.EvaluateDefaults(...)`；旧版分支仍先旧Evaluate再ApplyOwn。无显式值不用分配mask；大schema使用有界临时存储并在异常时释放，不缓存span/上下文。

- [x] **Step 1：真实Compiler RED。** Core Temporary构造新版descriptor及实际normalized配置，通过Compiler/惰性Control槽观察最终值：ControlHeight50/LineWidth2、Own行高10→padding18；显式padding3,7保留7；显式行高0合法且派生23；删除显式下游恢复18。合法null用现有允许null的动态descriptor codec验证，与缺省不同，不更改所有字符串codec语法。
- [x] **Step 2：最小实现并GREEN。** 新版先应用显式值/建立位集，生成计划求剩余字段；复制/冻结与资源投影仍沿既有路径。以最终Control算法/appearance构造context，不回读应用ThemeVariant，不引入新资源字典。
- [x] **Step 3：同条件扩展。** 明暗、自定义Control算法、局部Global、父子scope覆盖、清除覆盖均得到有效上游；一个默认函数抛错时保留该惰性槽输入并允许重试，不提前计算所有Control。旧整体evaluator对同输入维持原先“最后覆盖”结果。

## Task CT-05B：缓存、兼容与清理后确认

**Interfaces:** `ControlCompilationCacheKey`新增版本及evaluator revision字段，参与Equals/GetHashCode；保留CompilationIdentity和现有Global/config/appearance键。schema revision、Own slot和序列化ABI不因此变化。

- [x] **Step 1：缓存RED→GREEN。** 相同schema不同evaluator revision不能复用旧Control结果；Bind保留版本/revision；等价配置仍命中，兄弟Control保持独立惰性，失败按原契约可重试，成功复用。复用既有缓存测试helper，禁止建立第二个缓存。
- [x] **Step 2：模块精确验证。** `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Temporary.OwnDefaults|FullyQualifiedName~DenseThemeCompilerTests|FullyQualifiedName~TypedThemeSnapshotCacheTests|FullyQualifiedName~ThemeSchemaRegistryTests'`。新旧混合registry（每个终端链内部版本一致）实际运行，不能把所有Control迁成V2。
- [x] **Step 3：测试去留。** 临时故障/反例/跨包工程及专属产物全部删除。若在既有共享机制测试补最小新版行为保障，说明旧测试/编译器/AOT不能保护的依赖覆盖错误、每次全局验证必要性及实测成本；否则保持永久测试净增0。
- [x] **Step 4：最终工作树验证与文档。** 清理后复用对应Core/Generator精确filter，`git diff --check`。同步正式实现边界、子计划checkbox、总进度及文本结果；完整产品消费/视觉仍等待CT-12等任务，不将机制green改写为真实Button B02已修复。不提交。

## 交接与判定

按CT-03→CT-04A→CT-04B→CT-05A→CT-05B顺序实施；CT-04A/B合起来才算CT-04完成，CT-05A/B合起来才算CT-05完成。每轮只写实际命令、结果和未覆盖项。用户已经授权严格按任务清单继续，不在子步骤之间重复请求继续执行确认。
