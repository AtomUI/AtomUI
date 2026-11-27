# AOT Sidecar Manifest 与静态注册计划优化设计

> 历史记录：本文涉及的旧控件注册/裁剪方案已被 [TypeMap 正式架构](../../architecture/foundations/aot-and-trimming.md)替代，不再作为维护约束。其余内容保留当时的设计上下文。


> 当时记录的状态：2026-08-16 提案，待实现与基线验证。
>
> 本文记录方案选择、迁移路径和验收门槛。实现稳定后，长期契约应同步到
> [`docs/architecture/foundations/aot-and-trimming.md`](../../architecture/foundations/aot-and-trimming.md)、
> [`docs/architecture/foundations/aot-registration-unit-granularity.md`](../../architecture/foundations/control-registration-contracts.md)
> 和 AOT 开发规范；正式架构文档不依赖本文才能成立。

## 1. 结论

采用“**Package 构建期预计算 Sidecar Manifest + 应用 AOT/Trim 编译期静态计划**”架构，替换当前把 Unit 依赖递归写进
生成方法的实现。

最终路径为：

1. 普通 Debug 和未启用 AOT/Trim 的 Release 不加载 linked-publish 分析器，不收集 AXAML usage，不扫描 C# usage，
   不创建 SemanticModel，也不生成 Application Plan。
2. Control Package 只在打包或作为 AOT/Trim `ProjectReference` 构建时，自动生成版本化 sidecar。包作者不写 Attribute、
   Unit 依赖、linker XML、额外目录规则或配置。
3. 应用只在 `PublishAot=true`、`PublishTrimmed=true` 或 `RunAOTCompilation=true` 时读取 sidecar，并对应用自身源码做候选驱动的
   增量分析。
4. Package Unit fragment 不再调用其他 Unit，也不再调用 `TryEnterUnit`。应用生成器在编译期完成 Unit 闭包和去重，输出
   每个选中 Unit 一次且仅一次的强类型静态调用。
5. 任何无法证明的 ownership、动态调用、协议缺失或分析预算超限，都只把对应 Package 扩大为 full registrar。禁止生成
   可能漏注册的“猜测结果”。

这套方案同时消除两个同源问题：Generator 不再递归遍历方法体和反复创建 SemanticModel；ILLink 也不再沿 Unit fragment
之间的调用边扩张成巨大的可达方法图。

## 2. 背景与证据

当前 `RegistrationUnitDependencyAnalyzer` 对 Directory 粒度 Package 执行以下工作：

- 顺序遍历 Compilation 中的全部 SyntaxTree 和全部 `DescendantNodes()`。
- 对大量节点请求 `SemanticModel.GetOperation(...)`。
- 从调用、属性、字段和泛型实参继续进入被引用成员的方法体。
- 按 Unit 维护访问集合，并把发现的 Control 依赖写成 Unit fragment 之间的递归 `Add(...)` 调用。

这使分析成本接近“程序集级调用图遍历”，且生成代码把分析中的保守扩张直接变成 ILLink 静态可达边。一个 Window/TextBox
样例因此能够把 Button、Picker、ListBox、Input primitives 等大量 Unit 一并保留。

2026-08-16 在同一台 macOS、同一 SDK、`osx-arm64`、Release、self-contained NativeAOT 条件下，对真实 Button/Window GUI
样例移除中文字体和未使用 ViewLocator 后，主程序基线为：

| 实现 | 主程序 bytes | MiB |
| --- | ---: | ---: |
| Fluent | 14,723,096 | 14.0410 |
| AtomUI | 28,744,968 | 27.4133 |
| 差值 | 14,021,872 | 13.3723 |

已排除的主要误判方向：

- AlibabaSans 字体只贡献 `710,032` bytes，约 `0.6771 MiB`。
- 最终只保留 15 个 Ant Design Icon，ILC map 中 Icon 节点合计约 `18,999` bytes。
- 移除 `UseUserThemeDirectory()` 后主程序大小没有变化。

主要差值集中在 `__managedcode`、`__const` 和 EH table，与过大的托管方法可达闭包一致。字体和 Icon 仍应独立优化，但不是本次
约 13.37 MiB 差值的根因。

## 3. 不可破坏的不变量

### 3.1 应用用户零配置

应用继续只调用真实产品入口：

```csharp
builder.UseDesktopControls();
```

不增加 `UseGenerated...`、注册列表、root Attribute、Manifest API 或不同的 AOT 启动方式。静态使用不能自动启用未调用入口的
可选 Package。

### 3.2 包作者零新增学习成本

普通第三方 Control Package 继续使用默认 `Package` 粒度。新方案不要求作者新增：

- Control 或 helper Attribute。
- Unit、Unit dependency 或 ownership 列表。
- linker XML、RD.XML 或 `DynamicDependency`。
- 新的源码目录规则。
- 运行时注册代码、反射扫描或延迟初始化。

现有公开注册入口和 Theme 组织契约不因本方案增加步骤。Sidecar 的生成、打包和导入由 NuGet build assets 自动完成。

### 3.3 普通构建零 linked-analysis 成本

对应用和普通类库：

- `dotnet build -c Debug` 不运行 linked-publish 分析。
- `dotnet build -c Release` 在未启用 AOT/Trim 时也不运行 linked-publish 分析。
- 不因为 `OutputType=Exe`、安装了 AtomUI Package、开启普通 Roslyn analyzer 或存在 AXAML 而隐式启用。
- 只有 AOT/Trim 发布和显式的内部验证目标可以启用应用级分析。

Control Package 的 NuGet `Pack` 是唯一额外例外：它在包生产阶段预计算 sidecar，把成本从所有消费项目移到一次打包中。
这不是普通 Debug/Release 编译路径，且不要求作者手工触发额外命令。

### 3.4 运行时零新增分析成本

“零成本”指本次优化不在运行时新增推导、发现或图处理。运行时不得：

- 读取或解析 sidecar/Manifest。
- 遍历 Unit dependency graph。
- 使用字典查找 Package 或 Unit。
- 扫描程序集、反射类型或查找 Attribute。
- 因缺失资源捕获异常后重试 full registrar。
- 延迟注册、后台注册或增加分析分配。

现有 Theme descriptor、资源集合和 `ControlPackageRegistration` 的必要构造属于产品注册本身，不属于推导成本。本方案还会删除
生成 fragment 对 `TryEnterUnit` 的调用，从 generated path 移除 `_enteredUnits` HashSet 分配。

### 3.5 正确性优先且单调扩大

分析结果只有两种合法状态：

- `Exact`：证据足以生成精确静态 Unit plan。
- `PackageFallback`：证据不完整，调用该 Package 的 full registrar。

不存在 `BestEffort`。任何不确定性只能增加保留内容，不能减少运行所需内容。

## 4. 能保证什么，不能保证什么

对于包含任意反射、运行时脚本、动态程序集、未知 Source Generator 或插件输入的程序，不可能同时保证：

1. 理论最小发布体积。
2. 包作者完全不声明动态边界。
3. 运行时完全不做发现。

本方案选择保证第 2、3 项和运行正确性。静态证据充分时得到细粒度裁剪；证据不充分时自动扩大到 Package full registrar。
因此它能保证“不漏”，但不承诺每一个动态程序都达到理论最小值。

## 5. 总体架构

```text
Control Package source / AXAML
        |
        | Pack 或 linked ProjectReference build
        v
Package Manifest Compiler
        |
        +--> leaf Unit fragments in package assembly
        |
        `--> <assembly>.atomui-link.json
                    |
                    | NuGet buildTransitive / ProjectReference target output
                    v
Application AOT/Trim compilation
        |
        +--> application C#/AXAML direct usage candidates
        +--> transitive usage sidecars
        +--> package declaration/dependency sidecars
        v
Compile-time closure + fallback resolver
        v
GeneratedApplicationRegistrationPlan.g.cs
        |
        `--> direct static Unit calls, no runtime graph
```

架构分成三个职责独立的组件：

| 组件 | 运行时机 | 职责 |
| --- | --- | --- |
| Ordinary Generator | 所有需要 Theme 生成的构建 | 生成现有 Theme schema、resource wrapper、full registrar 和 leaf Unit fragment |
| Package Manifest Compiler | Pack 或 linked ProjectReference | 预计算 Package/Unit/ControlMap、Unit edges、usage 和 uncertainty sidecar |
| Application Plan Generator | 仅 AOT/Trim 应用编译 | 合并 sidecar 与当前应用 usage，生成最终静态 plan |

## 6. 物理隔离 linked-publish 分析器

只在回调入口检查 `AtomUILinkedPublish=false` 仍不能提供最强的 Debug 保证，因为包含多个 Generator 的 Analyzer assembly 仍会被
`csc` 加载并初始化。因此必须物理拆分：

```text
AtomUI.Generator.dll
    ordinary Theme/Token/Localization generation

AtomUI.Generator.LinkedPublish.dll
    package manifest compiler frontend
    application usage analyzer
    application plan generator
```

MSBuild 只在以下条件之一成立时把 `AtomUI.Generator.LinkedPublish.dll` 加入 `@(Analyzer)`：

```text
PublishAot == true
or PublishTrimmed == true
or RunAOTCompilation == true
or AtomUIEmitLinkedManifest == true   # 仅 Pack/仓库验证内部使用
```

`AtomUIUseGeneratedRegistration=true` 不再作为普通构建隐式加载整套分析器的理由。测试若需要 generated path，必须通过隔离的
验证 target 显式启用，不能污染产品 Debug 构建。

`AtomUILinkedPublish` 的最终值仍作为 CompilerVisibleProperty 传给 linked analyzer，但 Build target 必须在创建 AXAML usage、
收集 AdditionalFiles 和注入 Analyzer 之前先判定开关。关闭时连空的 usage XML 都不创建。

## 7. Sidecar Manifest

### 7.1 资产边界

Sidecar 是纯编译资产：

- NuGet 路径：`buildTransitive/AtomUI.LinkedRegistration/<assembly>.atomui-link.json`。
- 通过 Package 自己的 `<PackageId>.targets` 在 linked build 中加入 `AdditionalFiles`。
- `CopyToOutputDirectory` 和 `CopyToPublishDirectory` 始终为 `Never`。
- 不作为 EmbeddedResource，不写入运行时程序集，不进入 `.app` 主程序或发布目录。
- ProjectReference 通过返回 item 的 MSBuild target 传递 `obj/.../*.atomui-link.json`，不复制到源码树。

所有 sidecar 使用 UTF-8、确定性 JSON 和唯一 codec。字段顺序、数组排序、路径规范化和转义由 writer 统一负责，禁止各 Generator
手工拼字符串。

### 7.2 顶层模型

示意模型如下，实际实现使用强类型 record 和结构化 JSON reader/writer：

```json
{
  "protocol": 2,
  "producer": "AtomUI.Generator.LinkedPublish/6.0",
  "assembly": {
    "name": "AtomUI.Desktop.Controls",
    "contractHash": "sha256:..."
  },
  "packages": [],
  "usages": [],
  "fallbacks": []
}
```

一个 assembly sidecar 同时允许包含：

| 记录 | 内容 |
| --- | --- |
| `Package` | Package ID、entry method identities、full/shared fragment、粒度、稳定顺序 |
| `Unit` | Unit ID、leaf fragment type/method、稳定 order key |
| `ControlMap` | CLR metadata name 到 Package/Unit 的唯一 ownership |
| `UnitEdge` | source Unit 到 target Unit 的直接编译期依赖及证据类型 |
| `Usage` | 当前程序集使用的 entry、Control Unit 或 Package root |
| `Fallback` | Package fallback、原因码和可选的相对来源位置 |

发布给消费者的 sidecar 不包含绝对源码路径。Package 构建阶段的本地诊断可以包含真实 Location；写入包时只保留相对路径、
程序集身份或稳定 reason code。

### 7.3 版本和绑定

- `protocol` major 不兼容时构建失败，不能猜测字段含义。
- 未识别的 optional minor 字段可以忽略。
- `contractHash` 覆盖 Package、Unit、ControlMap、fragment identity 和依赖记录，用于发现陈旧 sidecar。
- ProjectReference sidecar 必须与本次引用输出的 assembly identity/hash 匹配；不匹配时重新生成，不能复用旧缓存。
- NuGet sidecar 与包内 assembly 版本共同发布，包内 target 只能导入自己的相对路径资产。

## 8. Package Manifest 生成算法

### 8.1 默认 Package 粒度

普通第三方包默认只有一个 Unit，因此根本不需要 Unit dependency 推导。Manifest compiler 只输出 Package、单一 Unit、
ControlMap、entry 和可能的动态 fallback。

这是“作者零负担”和“正确性默认安全”的主要保证。Directory 分析不是普通第三方包的接入前置条件。

### 8.2 Directory 粒度的一次性摘要

Directory 模式只用于经过体积和运行验证的大型多控件包。分析器不再从一个调用点递归进入另一个方法体，也不再对每棵树执行
全量 `DescendantNodes()`。

使用 Incremental `SyntaxProvider` 只索引以下候选：

- Control 类型声明和基类。
- 显式/隐式 `new`。
- `typeof(...)` 和已知静态 Type 参数。
- Package registration entry 调用。
- 同程序集、跨 Unit 的直接方法/构造调用。
- delegate、dynamic、reflection 和已知动态创建 API。
- 结构化 AXAML 中的元素类型、TargetType、BasedOn、selector、template 和 `x:Type`。

每个源码文件先按现有 Registration Unit 策略自动归入：

- 一个 Directory Unit；或
- 始终保留的 Package Core。

分析只生成“直接证据”：

- Unit A 的文件直接创建或引用 Unit B Control，生成 `A -> B`。
- Unit A 直接调用 Unit B 拥有的成员，保守生成 `A -> B`，但不进入 B 方法体。
- Package Core 直接依赖 Unit B，生成 Package root `-> B`。
- AXAML owner Unit 引用 Unit B 类型，生成 `A -> B`。

Unit 图的传递闭包只在应用 plan 阶段对紧凑 Unit ID 图计算。它不在 Roslyn syntax 上递归，也不生成 fragment 之间的调用图。

### 8.3 不确定性

以下情况直接产生 `PackageFallback`：

- owner 文件不能唯一归入 Unit 或 Package Core。
- dynamic invocation、反射 Type 名称、运行时脚本或插件输入无法解析。
- interface/delegate 调用可能跨 Unit，但静态 target 集不能封闭。
- 其他 Source Generator 产生的最终 IL 使用未被声明或验证。
- AXAML namespace、loose XAML、动态 ResourceDictionary 或 selector 无法绑定到唯一 Package。
- 控制图、Unit ownership 或 fragment identity 冲突。
- 分析结构预算超限。

Sidecar 生成后增加一次仅在 Pack/linked ProjectReference 运行的最终程序集验证。验证器读取 PE metadata 和编译资源，检查已知
`newobj`、`ldtoken`、entry call 和 generated resource wrapper 是否都能被 sidecar 解释。它不构建程序集级递归调用图；发现
未覆盖证据时降级为 PackageFallback。

### 8.4 确定性结构预算

禁止用墙钟超时决定输出，因为这会让不同机器产生不同 Manifest。使用确定性结构上限，例如候选数、直接边数、Unit 数和
Sidecar bytes。超过上限时：

1. 停止细粒度推导。
2. 写入 reason code 为 `AnalysisBudgetExceeded` 的 PackageFallback。
3. linked publish 输出一次明确 Warning；strict 验证将其提升为 Error。

这样异常代码规模只会牺牲体积，不会再次形成无限循环、栈溢出或数 GB ILLink 图。

正式预算与实现统一为：C#/AXAML candidates 50,000、Unit 2,048、UnitEdge 10,000、ControlMap/Usage 50,000、
Manifest record 64,000、单 sidecar/AXAML usage 4 MiB、累计 sidecar 32 MiB。应用超限时只线性保留 entry 和 Package root；
Sidecar 超限时只保留 Package、entry 和 `AnalysisBudgetExceeded` fallback。Unit SCC 与组件排序使用完全迭代算法。

## 9. 应用 AOT/Trim 消费算法

Application Plan Generator 的输入只包含：

- 当前应用的候选驱动 C#/AXAML usage。
- ProjectReference 返回的 sidecar。
- NuGet buildTransitive 导入的 sidecar。
- 用户已经显式调用的 Package registration entry。

处理顺序为：

1. 验证所有 sidecar major version、hash、Package ownership 和 fragment symbol。
2. 汇总当前应用和传递类库中的 entry、Control usage、Unit root、Package root。
3. 检查每个被使用 Package 是否显式调用入口；缺失入口仍为构建错误。
4. 对 `Exact` Package 从直接 Unit roots 计算 Unit 图闭包。
5. 对存在任何 fallback 的 Package选择 full registrar。
6. 把选中 Unit 按 Package 声明顺序和依赖 SCC 的确定顺序排序。
7. 生成一个最终静态 Application Plan。

复杂度接近：

```text
O(当前应用 usage candidates + sidecar bytes + selected Unit vertices + selected Unit edges)
```

应用编译不分析第三方 Package 源码，不遍历第三方方法体，也不让 ILLink 参与依赖推导。

## 10. 叶子 Unit 与静态计划

### 10.1 Package 输出

Unit fragment 只注册自己的 descriptor 和 Theme，不调用 dependency Unit：

```csharp
public static partial class GeneratedRegistrationUnit_Button_0123
{
    public static void Add(AotTrimControlPackageRegistrationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.AddControl(GeneratedThemeSchemaDescriptorFactory.CreateButton());
        AddThemes(builder);
    }

    static partial void AddThemes(AotTrimControlPackageRegistrationBuilder builder);
}
```

fragment 中禁止出现：

```csharp
builder.TryEnterUnit(...);
OtherUnit.Add(builder);
AddDependencies(builder);
```

Theme Asset Generator 也只贡献本 Unit 的 Theme 内容；它把 AXAML direct edges 写入 sidecar，不再写入 C# 方法调用。

### 10.2 应用输出

应用编译期闭包完成后直接生成：

```csharp
var packageBuilder = new AotTrimControlPackageRegistrationBuilder(
    builder,
    provider,
    includeIdentity,
    selectAssets);

GeneratedDesktopPackageSharedFragment.Add(packageBuilder);
GeneratedRegistrationUnit_Window_0123.Add(packageBuilder);
GeneratedRegistrationUnit_TextBox_4567.Add(packageBuilder);
GeneratedRegistrationUnit_Button_89AB.Add(packageBuilder);
packageBuilder.Register();
```

每个 fragment 在源码中最多出现一次。Unit cycle 在编译期先折叠成 SCC，再按稳定 Package order 输出；运行时不需要 visited set。

### 10.3 运行时桥接

`UseDesktopControls()` 保持现有 FeatureSwitch 分支和静态 Package switch bridge。本方案不新增运行时查找结构。可以把
`AotTrimRegistrationPlanRegistry` 的 heap record 改为静态 owner/delegate 字段，避免安装计划时的额外 record 分配；这属于独立的
常量成本清理，不改变本方案的正确性。

## 11. 传递类库和未知二进制

普通应用类库在 Debug 中不生成 usage metadata。它只在以下时机自动生成 usage sidecar：

- 被 AOT/Trim 应用作为 ProjectReference 构建。
- 自身被 `Pack` 为 NuGet 包。

NuGet Pack 自动把 usage sidecar 和唯一的 `<PackageId>.targets` 一同打入 `buildTransitive`。作者不维护打包 item。

消费到旧版或非 AtomUI-aware 二进制时采用保守策略：

| 情况 | 行为 |
| --- | --- |
| 完整且可验证的 sidecar | 使用精确 usages |
| assembly 引用 AtomUI Control Package 但没有 usage sidecar | 对该 assembly 可能涉及且已调用入口的 Package full fallback |
| 无法判断具体 Package | 对应用已调用的 AtomUI Package full fallback，并报告来源 assembly |

缺失 sidecar 不允许被解释为“没有使用 Control”。

## 12. MSBuild 激活矩阵

| 场景 | Linked Analyzer | AXAML Usage Task | Sidecar 输出 | Application Plan |
| --- | --- | --- | --- | --- |
| Debug build | 不加载 | 不运行 | 无 | 无 |
| 普通 Release build | 不加载 | 不运行 | 无 | 无 |
| Release + analyzer warnings | 不加载 | 不运行 | 无 | 无 |
| `PublishTrimmed=true` | 加载 | 运行 | ProjectReference 自动输出 | Exe/WinExe 输出 |
| `PublishAot=true` | 加载 | 运行 | ProjectReference 自动输出 | Exe/WinExe 输出 |
| `RunAOTCompilation=true` | 加载 | 运行 | ProjectReference 自动输出 | Exe/WinExe 输出 |
| NuGet Pack | manifest 模式加载 | 仅为 sidecar 运行 | 打包 | 无 |
| 仓库 strict 验证 | 显式加载 | 运行 | 验证输出 | 按 target 决定 |

非 AOT/Trim 的 `dotnet publish` 也不启用。`AtomUILinkedPublish` 必须在 `PrepareForBuild` 前稳定下来，后续 target 不得通过
`OutputType` 或 Configuration 再推断一次。

## 13. 增量和缓存

- 每个 SyntaxProvider 输出按单个候选或单个 AXAML 文件建模，使用可比较的 immutable value，不把 `Compilation`、
  `SemanticModel` 或 SyntaxNode 放入缓存值。
- AXAML Task 使用输入文件 hash、Package/Unit roots hash 和 protocol version 作为增量 key。
- Sidecar writer 先比较 canonical content hash，内容未变化时不改时间戳。
- Application Plan 对每个 sidecar 单独解析并缓存，单一 Package 更新不使其他 Package 重新分析。
- Unit closure 使用整数索引和紧凑 adjacency arrays；禁止在 generated runtime code 中携带这些结构。
- 禁止在 generator callback 中嵌套调用 `GetSemanticModel` 并重新遍历另一个方法体。

## 14. Fallback 与诊断矩阵

| 条件 | 输出 | 正确性 |
| --- | --- | --- |
| 精确 Control/AXAML usage | 选中 Unit 闭包 | 精确 |
| Package 默认粒度 | 单一 Package Unit | 保守且完整 |
| dynamic/reflection/loose AXAML | 当前 Package full registrar | 完整，体积扩大 |
| 缺失或陈旧 sidecar | 当前相关 Package full registrar | 完整，体积扩大 |
| 未知 sidecar major | 构建 Error | 禁止不兼容输出 |
| fragment symbol 不存在 | 构建 Error | 禁止产生坏调用 |
| 分析预算超限 | 当前 Package full registrar | 有界完成 |
| 检测到 usage 但未调用 entry | 构建 Error | 保持显式启用契约 |
| Unit cycle | 编译期 SCC 全选，每个 Unit 调用一次 | 完整，无运行时递归 |

Warning 必须说明“哪个 Package 因为什么原因扩大”，不能只写“可能不兼容 AOT”。strict 模式可把自动 fallback 提升为 Error，
但普通用户不需要理解或修复 Unit graph。

## 15. 明确拒绝的方案

### 15.1 继续优化当前递归方法分析器

即使增加 visited set 或修复某个循环，它仍把 Roslyn 方法图、生成方法图和 ILLink 可达图绑定在一起。性能和体积问题会以新的
代码形态再次出现。

### 15.2 在运行时读取 Manifest

这会增加启动 I/O、解析、分配、错误分支和 AOT 动态行为，违反运行时零分析成本。

### 15.3 要求作者声明 Unit 依赖

这是第二份实现事实来源，重构后容易过期，也把本应由工具承担的 AOT 正确性转移给库作者。

### 15.4 为每个 Control 增加 Attribute 或 linker XML

这增加学习成本且不能覆盖 AXAML、generated code、动态 factory 和资源 ownership。

### 15.5 让 ILLink 自己推导并修补注册

ILLink 只知道可达性，不知道 AtomUI Theme registration 的产品顺序和资源语义。自定义 linker step/IL rewriting 还会绑定
NativeAOT、trimmed JIT 和 WebAssembly 的内部实现，不适合作为稳定跨平台协议。

### 15.6 用静态构造函数自动注册 Control

这会引入运行时副作用、顺序不确定、额外分配和难以裁剪的全局状态，不满足用户继续只调用 Package 入口的契约。

## 16. 迁移计划

### Phase 0：冻结事故面

- 为当前递归 analyzer 增加确定性结构上限和 Package fallback，先保证不会再次无限扩张。
- 确认普通 Debug/Release 的 analyzer、AXAML target 和 publish analyzer 开关均为关闭。
- 固化当前 Minimal、Button/Window、DynamicFallback 和 Full 的 size/map/binlog 基线。

### Phase 1：叶子 fragment 与 sidecar

- 新增强类型 model/codec 和 deterministic writer。
- Theme Schema/Asset writer 停止生成 Unit dependency 调用和 `TryEnterUnit`。
- Package manifest compiler 输出 UnitEdge 和 fallback。

### Phase 2：静态 Application Plan

- Application generator 读取 sidecar，在编译期计算 closure/SCC。
- 生成直接 Unit 调用并验证 fragment symbol。
- 删除当前 Application Plan 对 recursive Unit fragment 的依赖。

### Phase 3：Debug 物理隔离

- 拆出 `AtomUI.Generator.LinkedPublish.dll`。
- 更新 buildTransitive Analyzer 注入和 AXAML target 条件。
- 增加 binlog/Compiler command line 测试，证明普通构建没有加载 linked analyzer。

### Phase 4：传递 sidecar 与 Pack

- 完成 ProjectReference target output 聚合。
- Pack 自动生成 usage/package sidecar 和唯一 consumer target。
- 验证 sidecar 不进入 runtime/publish 资产。

### Phase 5：收尾

- 删除 AssemblyMetadata dependency graph 和 `TryEnterUnit` 生成调用。
- 删除只服务递归 Unit fragment 的 analyzer、codec 和测试。

每个 Phase 都必须可以独立回滚。未通过正确性矩阵时只能回退到 Package full registrar，不能回退到不完整的细粒度 plan。

## 17. 验收矩阵

### 17.1 Debug/Release 成本

普通 Debug 和非 AOT/Trim Release 必须同时满足：

- `Csc` 的 Analyzer item 中不存在 `AtomUI.Generator.LinkedPublish.dll`。
- `CollectAtomUIAxamlUsage` 和 sidecar target 均为 skipped。
- `obj` 中不产生 linked usage XML、sidecar 或 Application Plan。
- Generator telemetry 中 linked analyzer 执行次数为 0。
- 与移除 linked-publish 功能的对照构建相比，中位耗时和峰值内存差异处于测量噪声内；目标不超过 1%。

### 17.2 AOT/Trim 编译性能

在固定 SDK、RID、机器和冷/热缓存条件下记录 binlog 与 `/usr/bin/time`：

- 不允许出现全树 `DescendantNodes()` 或递归成员 body traversal。
- Package manifest 的 no-op rebuild 只做 hash/cache 命中，不重新分析未变文件。
- AtomUI.Desktop.Controls clean manifest 分析目标不超过总 `CoreCompile` 的 10%，且不超过 2 秒。
- Application Plan 生成目标不超过 500 ms。
- linked analyzer 峰值附加内存目标低于 256 MiB。
- 任意预算超限必须在有界时间内产生 PackageFallback，不得继续增长。

阈值需用实现前基线校准；调整必须记录原始数据和理由，不能为了让测试通过静默放宽。

### 17.3 运行时正确性

- ordinary full registrar 与 generated registrar 的 descriptor、Theme asset、Language、Provider、initializer 和冻结时序快照一致。
- 每个 Directory Unit 都有隔离 fixture：创建公开 Control、应用默认 Theme、打开 Window 并完成首帧。
- Window/Button、TextBox、DatePicker、DataGrid、ColorPicker、dynamic fallback 和 PackageShared 场景全部 smoke 通过。
- Unit cycle fixture 每个 Unit 只调用一次，资源和 descriptor 不缺失。
- trimmed JIT、macOS/Windows/Linux NativeAOT、WebAssembly AOT 按平台能力验证。

### 17.4 发布体积

固定 `osx-arm64`、Release、self-contained NativeAOT、相同 SDK 和相同示例源码：

- Button/Window 主程序移除中文字体后，以 `<= 18 MiB` 为第一阶段硬目标。
- 以 `<= 16 MiB` 或不超过同场景 Fluent 125% 为后续优化目标；达不到时必须由 ILC map 解释剩余 Package Core 成本。
- 新增一个未使用 Unit 后主程序增量继续不超过 `256 KiB`。
- ILC map 中不得因 Window/Button usage 保留无直接/传递证据的 Picker、ListBox、DatePicker 等 Unit fragment。
- sidecar、Generator、Build Tasks、PDB 和分析缓存不得进入主程序或 publish 目录。

体积目标是实现验收门槛，不是仅凭设计即可承诺的结果。任何数字都必须以删除缓存后的真实 publish 和 ILC map 为准。

### 17.5 协议和缓存

- JSON round-trip、确定性排序、转义、未知 major、minor compatibility 和 hash mismatch 测试通过。
- 同输入连续生成 sidecar byte-for-byte 相同，第二次不更新时间戳。
- 改动一个 Unit 只使该 Package sidecar 和最终 Application Plan 失效。
- 缺失 sidecar、损坏 sidecar和未知二进制均走定义好的 Error 或 full fallback。

## 18. 实现影响面

预计主要修改所有权如下：

| 区域 | 改动 |
| --- | --- |
| `src/AtomUI.Generator` | leaf fragment、移除递归 dependency writer、保留 ordinary generation |
| 新 linked generator project | candidate analysis、sidecar reader、Application Plan |
| `src/AtomUI.Build.Tasks` | deterministic sidecar/AXAML/PE validation 和 MSBuild task |
| `build/AtomUI.LinkedRegistration.*` | 严格激活矩阵、ProjectReference/NuGet sidecar 传递 |
| `src/AtomUI.Core/Registration` | generated path 不调用 TryEnter；可选移除 plan entry record 分配 |
| Generator tests | protocol、candidate、fallback、static plan、determinism |
| publish fixtures/scripts | Debug 零成本、真实 AOT、size/map/runtime smoke |

本方案不要求修改 Control public API、应用启动 API 或 Package registration entry API。若实现中发现必须新增 Public API，必须停止
并单独评审，不能把临时 ABI 混入本次性能修复。

## 19. 风险与回滚

| 风险 | 控制 |
| --- | --- |
| Sidecar 与 assembly 陈旧 | contract hash + symbol validation；不匹配时重建或 full fallback |
| NuGet transitive asset 丢失 | pack contract test + consumer fixture；缺失不解释为空 usage |
| Directory direct edge 仍过宽 | ILC map Unit 归因；优化第一方目录/Package Core ownership，不给第三方加规则 |
| Generated code 无法观察 | final assembly direct-evidence verifier；无法解释则 full fallback |
| Sidecar 与 assembly 协议不一致 | major-aware catalog；缺可信 graph 时 Package fallback |
| 注册顺序变化 | stable order key + SCC order + ordinary/generated snapshot |
| 分析规模异常 | 确定性结构预算 + PackageFallback，无墙钟非确定性 |

紧急回滚开关只允许把 Package 切换为 full registrar。不得重新启用 recursive fragment dependency graph，也不得通过关闭
诊断保留不完整 plan。

## 20. 最终判断

这是在现有约束下最接近“完美”的可行解：

- 应用和包作者的使用方式不变。
- 普通 Debug/Release 从构建图上移除整套 linked analysis。
- AOT/Trim 的推导成本有界、增量、可缓存，并从方法图缩小为候选摘要和 Unit 图。
- 运行时没有 Manifest、依赖图、反射或 fallback 逻辑。
- ILLink 只看到最终选中的叶子 fragment，不再替 Generator 承担错误的依赖闭包。
- 静态信息不足时明确牺牲体积而不是牺牲正确性。

它不依赖“分析器永远足够聪明”的假设，而是把可证明精确、不可证明回退、编译性能上限和真实发布验收同时写进协议。
