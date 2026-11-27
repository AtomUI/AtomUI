# 主题算法策略化 实施计划

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让主题算法成为可注册、可设置的策略，算法可以计算全部 Token（Map 必写，Alias 与控件 Token 可选改写，Ant 公式兜底），并用 PrimeNG 六个预制算法重做 PrimeNG 预设包，全程不影响性能。

**Architecture:** `ThemeAlgorithm` 从枚举改为 `(Catalog, Id)` 结构体，算法随包注册进 `ThemeSchemaRegistry`。Token 属性改为 partial 属性，生成器补上记录写入的 setter；编译器据此执行「显式值 → 默认公式 → 算法 → 带标记重算」的三步流程。Alias 与控件阶段是两个可选接口，直接写 Token 对象。

**Tech Stack:** .NET 10（SDK 10.0.403，`LangVersion latest`，partial 属性与 `field` 关键字）、Avalonia 12.1.6、Roslyn 增量生成器与 analyzer（`AtomUI.Generator`，netstandard2.0）、MSBuild 任务（`AtomUI.Toolchain`）、xunit v3 + Shouldly。

**Spec:** `docs/developments/specs/2026-11-13-theme-algorithm-strategy-design.md`。执行者两份都要读。本计划与规格不一致的地方已在「与规格的差异」登记，并已回写规格。

**进度台账:** `docs/developments/progress/2026-11-13-theme-algorithm-strategy.md`（Task 1 创建）。每个 Task 结束时登记：提交号、测试结果、性能数字、与计划的偏差。

## Global Constraints

- 工作分支 `feature/design-language`，基线 `release/6.0`。执行开始前已按用户决定变基到 `release/6.0` 的 `f11459de0`（含 Avalonia 12.1.6、6.2.14 发版准备、SDK 11 构建支持）。本计划里的文件行号取自变基之前，只作定位提示，以符号名为准。
- 提交方式（用户确认）：实现者**不提交**；每个任务经审阅通过后由主控按任务里给出的提交信息提交到本分支，不推送。各任务的「提交」步骤由主控执行。
- 不引入设计语言抽象层，不按语言切换 ControlTheme，不改控件结构，不新增全局 Seed Token，不在主题 XML 里加表达式，不做「暗色对应算法」元数据（规格 1.3）。
- 不考虑对旧 API 的兼容（用户确认）。破坏性变更写进双语 CHANGELOG 和 `docs/releases` 下的 API 变更文档。
- 优先级固定：**Ant 默认公式 < 算法 < 主题显式值 < 更内层作用域的显式值**。
- 使用内置 `Default / Dark / Compact` 的主题，编译结果逐槽位不变；唯一例外是 Task 5 的全局 Alias 修复，差异要列进台账。
- 性能是硬约束（规格 4.11）：Token 的 getter 必须是 `get => field`；setter 不记账时只多一次空引用判断，不分配内存；内置三算法不多跑任何一遍默认公式、不多创建任何对象。门槛：默认主题冷编译与明暗切换的中位数差异落在基线波动范围内且不超过 2%；编译分配增量不超过「Token 实例数 × 8 字节」；启动耗时落在基线波动范围内；程序集与 NativeAOT 发布体积增幅超过 1% 时停下来回到设计。不过门槛先改实现，仍不过则停止并报告，不放宽门槛。
- 算法各阶段必须是纯函数：相同输入相同输出，不读时钟、环境、静态可变状态，不改输入对象。
- 指纹与缓存键里的字符串一律用仓库现有的确定性指纹构建器逐字符计入，禁止 `string.GetHashCode()`（进程间随机）。
- 代码内 `summary` 注释用英文。提交信息单行、`<type>(<scope>): <subject>`，经 `commit-msg` 技能生成，不带任何 AI/agent 署名或 trailer。
- 不写 `.superpowers/`；`docs/developments` 只放 Markdown。
- 测试遵守 `docs/engineering/development/test-value-and-lifecycle.md`：契约测试与生效测试常驻；快照对比等一次性验证用完即删。
- 命令：`dotnet test tests/<Project>/<Project>.csproj`；全量回归用 `scripts/run-full-regression.sh`，没有它最后的 `[SUMMARY]` 行不得声称全量通过。
- 人工验收的宿主窗口用 `atom:Window`。

## 与规格的差异

写计划时对照代码发现规格有几处与事实不符，已回写规格，执行时以本节为准：

1. **`AtomUI` Catalog 没有保留规则。** 控件身份今天就没有任何保留检查（运行时和生成器都没有）。算法身份同样不加，重复身份由 `ThemeSchemaRegistry` 现有的唯一性检查拒绝（`Duplicate algorithm identity`）。
2. **Token 类的数量。** 带 `[ControlDesignToken]` 的类共 158 个：124 个 `internal sealed`、32 个 `internal abstract` 中间层、2 个已公开；Own Token 属性共 1182 个。中间层必须一起公开（公开类不能继承内部基类）。`AtomUI.Toolkits.GalleryBase` 的 8 个类只改 partial，不公开。所以改公开的是 148 个。
3. **记账按属性名，不按槽位号。** 中间层的属性在不同终端类里槽位不同（槽位是终端类全部 Token 名称的序数排序），生成的 setter 无法写死槽位。改为 `RecordWrite(nameof(P))`，记账结束后由编译器经描述符把名称解析成槽位。不记账时路径仍是一次空引用判断。
4. **`CalculateTokenValues` 收窄为 `protected virtual`，不是 `protected internal`。** `protected internal` 在有无 `InternalsVisibleTo` 的程序集里要求不同的 override 修饰符，GalleryBase 和第三方包会编译不过。调用入口由生成的那一半提供。
5. **partial 属性可以保留初始化器。** 已用 SDK 10.0.403 验证：`public partial double A { get; set; } = 17;` 可编译，初始化器直接写字段、不经过 setter。35 处初始化器不用搬。
6. **XML 引用未知算法的构建期报错**没有独立诊断号，沿用 `ATOMUITOKEN002` 的自由文本（未知 Control 就是这样报的）。
7. **契约测试套件**先做成仓库内的测试支持库，不单独发 NuGet 包。

## Review Focus

- `default(ThemeAlgorithm)`（`Catalog`、`Id` 为 null）被传进 `WithAlgorithms`、描述符或注册表时，应得到指明原因的 `ArgumentException` 或 `ATMTHM4004` 诊断，不能是空引用异常。Task 1 覆盖。
- 同一条链里一个算法写了两次，一次带 `Catalog="AtomUI"`、一次省略，应判为重复。XSD 的 `xs:unique` 对缺失属性不生效，必须由 Reader 判定。Task 2 覆盖。
- 注册表修订号和配置指纹在两个进程里对同一组算法必须相同（持久缓存依赖它）。Task 1 覆盖。
- 算法写入的值恰好等于字段当前值、Ant 默认值、`0` 或 `Colors.Transparent` 时也算「写过」，照样带动派生字段、压过默认公式、触发阶段越界检查。写入看的是 setter 调用，不是值。Task 8 与 Task 11 覆盖。
- 算法的 Alias 或控件阶段抛异常时，编译失败的信息里要有算法身份，并且记账必须已关闭（否则同一 Token 对象上后续写入会被错记）。Task 11 覆盖。

---

## 阶段一：算法身份开放

### Task 1: `ThemeAlgorithm` 改为结构体，全仓编译通过，行为不变

**Files:**
- Create: `docs/developments/progress/2026-11-13-theme-algorithm-strategy.md`
- Modify: `src/AtomUI.Core/Theme/Algorithms/ThemeAlgorithm.cs`、`ThemeAlgorithmAttribute.cs`、`DefaultThemeVariantCalculator.cs:8-11`、`DarkThemeVariantCalculator.cs:9-12`、`CompactThemeVariantCalculator.cs:7-10`
- Modify: `src/AtomUI.Core/Theme/Schema/ThemeAlgorithmDescriptor.cs:17-42`、`ThemeSchemaRegistry.cs:16,135-139,197-201,691-697`
- Modify: `src/AtomUI.Core/Theme/Configuration/ThemeConfigNormalizer.cs:46-101`、`NormalizedThemeConfig.cs:335-339,366-370`
- Modify: `src/AtomUI.Core/Theme/Definitions/ThemeDocumentReader.cs:292-322`（本任务只让它编译通过，仍只认三个内置 Id）
- Modify: `src/AtomUI.Core/Utils/ThemeAlgorithmTypeConverter.cs`
- Modify: `src/AtomUI.Toolchain/AtomUI.Toolchain.csproj:46`（链接 `Theme/Schema/SchemaIdentifier.cs`，结构体依赖它）、`Tasks/GenerateCompiledThemeInputTask.cs:79-87`、`Common/ThemeDefinitions/CompiledThemeInput.cs:16,77-86,113`
- Modify: `src/AtomUI.Generator/ThemeSchema/ThemeAlgorithmInfo.cs:32-66`、`GeneratedThemeSchemaWriter.cs:33,247-260`、`Registration/RegistrationFingerprint.cs:101-131`、`ThemeDefinitions/CompiledThemePackageWriter.cs:32,40,86,92,97,133`
- Modify: `src/AtomUI.Toolkits.GalleryBase/Shell/GalleryWorkspaceViewModel.cs:394`、`controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/ViewModels/ThemePresetCatalog.cs:34,71-72`
- Test: 新建 `tests/AtomUI.Core.Tests/Theme/ThemeAlgorithmIdentityTests.cs`；修改 `ThemeSchemaRegistryTests.cs:151-176`、`ThemeConfigNormalizerTests.cs:107,161`、`ThemeGeneratedDescriptorContractTests.cs:163`、`ThemeDocumentReaderTests.cs:105`、`tests/AtomUI.Generator.Tests/ThemeSchemaGeneratorTests.cs:44-78,192-225,384-441`

**Interfaces:**
- Produces:

```csharp
// src/AtomUI.Core/Theme/Algorithms/ThemeAlgorithm.cs — modelled on ControlTokenIdentity
[TypeConverter(typeof(ThemeAlgorithmTypeConverter))]
public readonly struct ThemeAlgorithm : IEquatable<ThemeAlgorithm>
{
    public ThemeAlgorithm(string catalog, string id);   // SchemaIdentifier.Validate on both
    public string Catalog { get; }
    public string Id { get; }
    public static ThemeAlgorithm Default { get; }       // AtomUI:Default
    public static ThemeAlgorithm Dark { get; }          // AtomUI:Dark
    public static ThemeAlgorithm Compact { get; }       // AtomUI:Compact
    internal bool IsUninitialized { get; }              // true for default(ThemeAlgorithm)
    internal static int Compare(ThemeAlgorithm left, ThemeAlgorithm right);  // ordinal Catalog, then Id
    // ordinal Equals/GetHashCode, ==, !=, ToString() => "Catalog:Id"
}

public ThemeAlgorithmAttribute(string catalog, string id, int revision, ThemeAppearanceEffect appearanceEffect);
// properties: Catalog, Id, Algorithm (ThemeAlgorithm), Revision, AppearanceEffect

public ThemeAlgorithmDescriptor(ThemeAlgorithm algorithm, int revision, ThemeAppearanceEffect appearanceEffect, Func<IThemeAlgorithm> factory);
// throws ArgumentException("Theme algorithm identity is uninitialized.") for default(ThemeAlgorithm)
```

- Produces（Toolchain 中间文件）：`CompiledThemeInput.Algorithms` 与 `ThemeControlInput.Algorithms` 的元素类型从 `int` 改为 `ThemeAlgorithmInput { string Catalog; string Id; int Line; int Column; }`。文件格式版本号加一，读到旧版本时抛 `InvalidDataException("Compiled theme input format is out of date; rebuild the project.")`。
- Produces（生成代码）：算法一律输出为 `new global::AtomUI.Theme.Algorithms.ThemeAlgorithm("AtomUI", "Dark")`，不再有整数强转。

- [ ] **Step 1: 创建台账并写失败测试**

台账写入标题、规格与计划路径、当前基线提交。然后在 `ThemeAlgorithmIdentityTests.cs` 写：

```csharp
[Fact] public void BuiltIns_Have_AtomUI_Catalog_And_Stable_Ids()
// ThemeAlgorithm.Dark.Catalog == "AtomUI"; .Id == "Dark"; ToString() == "AtomUI:Dark"
[Fact] public void Equality_Is_Ordinal_On_Catalog_And_Id()
// new("PrimeNG","Aura") == new("PrimeNG","Aura"); != new("PrimeNG","aura"); != new("primeNG","Aura")
[Theory] [InlineData("", "A")] [InlineData("A", "")] [InlineData("1A", "B")] [InlineData("A", "B C")]
public void Constructor_Rejects_Invalid_Identifiers(string catalog, string id)   // ArgumentException
[Fact] public void Default_Struct_Is_Rejected_By_Descriptor_And_Normalizer()
// new ThemeAlgorithmDescriptor(default, 1, Light, factory) -> ArgumentException
// WithAlgorithms(default(ThemeAlgorithm)) -> Normalize yields ATMTHM4004 "Algorithm identity is uninitialized."
[Fact] public void Compare_Orders_By_Catalog_Then_Id()
// sorted: AtomUI:Compact, AtomUI:Dark, AtomUI:Default, PrimeNG:Aura, PrimeNG:Aura.Dark
```

在 `ThemeSchemaRegistryTests` 追加：

```csharp
[Fact] public void Registry_Revision_Is_Deterministic_For_Algorithm_Identities()
// Build two registries with the same algorithms in different input order -> Revision equal.
// Revision == a hard-coded constant captured from the first green run (pins cross-process stability).
[Fact] public void Registry_Revision_Changes_With_Algorithm_Catalog_Or_Id()
```

在 `ThemeConfigNormalizerTests` 追加：

```csharp
[Fact] public void Config_Fingerprint_Is_Deterministic_For_Algorithm_Identities()
// same chain normalized twice -> equal fingerprint, and equal to a hard-coded constant captured from the first green run
```

- [ ] **Step 2: 运行，确认失败**

Run: `dotnet build src/AtomUI.Core/AtomUI.Core.csproj`
Expected: 编译失败（测试引用了不存在的构造函数）。

- [ ] **Step 3: 实现 Core 运行时部分**

逐处替换调查报告列出的断点：`Enum.IsDefined`（3 处）改为 `IsUninitialized` 检查；`(int)` 强转和枚举排序改为 `ThemeAlgorithm.Compare`；注册表修订号与配置指纹改为计入 `Catalog`、`Id`、`Revision`、`AppearanceEffect`；三个 `const ThemeAlgorithm Algorithm` 字段删除（无引用）；`GalleryWorkspaceViewModel.cs:394` 的常量模式改为 `!=`；`ThemePresetCatalog` 导出 XML 时 `Id` 属性写 `algorithm.Id`，缓存键用 `ToString()`。`ThemeAlgorithmTypeConverter` 接受 `Default`、`Dark`、`Compact` 与 `Catalog:Id`，删掉 `DefaultAlgorithm` 这类旧拼写（无引用）。

- [ ] **Step 4: 实现生成器与 Toolchain 部分**

`ThemeAlgorithmInfo.Create` 改读两个字符串参数，字段改为 `Catalog`、`Id`、`Revision`、`AppearanceEffect`、`TypeName`；标识符不合法或 `revision <= 0` 时仍静默忽略（与现状一致）。排序与 `ComputeGlobalSchema` 改为按 `Catalog`、`Id`。`GenerateCompiledThemeInputTask` 写出 `ThemeAlgorithmInput`（行列取自 `ThemeAlgorithmDocument.Location`）。`CompiledThemePackageWriter` 的摘要字段改为 `Catalog + ":" + Id`。

- [ ] **Step 5: 修测试桩并跑测试**

`ThemeSchemaGeneratorTests` 的源码桩（`enum ThemeAlgorithm`、Attribute 与描述符构造函数）改成新形态；`Ignores_An_Undefined_ThemeAlgorithm_Attribute_Value` 改名为 `Ignores_A_ThemeAlgorithm_Attribute_With_An_Invalid_Identifier`，用 `"1Bad"` 作 Id。测试里的 `(ThemeAlgorithm)999` 改为 `new ThemeAlgorithm("Test", "Unregistered")` 或 `default`，按各自原意选。

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`，`dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`，`dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj`
Expected: 全部通过。然后跑 `scripts/run-full-regression.sh`，以 `[SUMMARY]` 行为准。

- [ ] **Step 6: 提交**

```bash
git add -A
git commit -m "refactor(Theme): 主题算法身份由枚举改为 Catalog 加 Id 的结构体"
```

### Task 2: 主题 XML 与 XSD 接受任意算法身份

**Files:**
- Modify: `docs/reference/theming/schemas/atomui-theme-v1.xsd:49-55,72-74,131-134,168-171`
- Modify: `src/AtomUI.Core/Theme/Definitions/ThemeDocumentReader.cs:292-322`、`ThemeDefinitionBinder.cs:64-70`
- Modify: `controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/ViewModels/ThemePresetCatalog.cs:71-72`
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeDocumentReaderTests.cs`、`ThemeDefinitionBinderTests.cs`、`tests/AtomUIGallery.Tests/ThemePresetsShowCaseTests.cs`

**Interfaces:**
- Consumes: Task 1 的 `ThemeAlgorithm(string catalog, string id)`。
- Produces: `<Algorithm Catalog="PrimeNG" Id="Aura.Dark" />`；`Catalog` 省略时为 `AtomUI`。Reader 的两条诊断（都是 `ATMTHM1002`）：`"Theme algorithm identity '{catalog}:{id}' is not a valid identifier."`、`"Theme algorithm '{catalog}:{id}' is duplicated."`。Binder 的 `ATMTHM2001` 文本带 `Catalog:Id`。

- [ ] **Step 1: 写失败测试**

```csharp
// ThemeDocumentReaderTests
[Fact] public void Read_Maps_Catalog_And_Id_To_Algorithm_Identity()
// <Algorithm Catalog="PrimeNG" Id="Aura.Dark"/> -> new ThemeAlgorithm("PrimeNG","Aura.Dark")
[Fact] public void Read_Defaults_Omitted_Catalog_To_AtomUI()
[Fact] public void Read_Rejects_Duplicate_When_One_Entry_Omits_The_Catalog()
// <Algorithm Catalog="AtomUI" Id="Dark"/><Algorithm Id="Dark"/> -> ATMTHM1002 "...'AtomUI:Dark' is duplicated."
[Fact] public void Read_Accepts_Same_Id_Under_Different_Catalogs()
[Fact] public void Read_Rejects_An_Invalid_Identifier_At_The_Reader_Boundary()   // Id="1Bad"
[Fact] public void Xsd_Still_Offers_The_Three_BuiltIn_Ids()   // replaces Xsd_Algorithm_Values_Match_The_ThemeAlgorithm_Enum
// ThemeDefinitionBinderTests
[Fact] public void Bind_Reports_An_Unregistered_Algorithm_With_Catalog_Id_And_Location()
// ATMTHM2001, message contains "PrimeNG:Aura", diagnostic location == the element's line/column
```

- [ ] **Step 2: 运行，确认失败**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`
Expected: 新测试失败（`Catalog` 属性被 XSD 拒绝）。

- [ ] **Step 3: 改 XSD 与 Reader**

XSD：`AlgorithmType` 增加可选属性 `Catalog`（`theme:IdentifierType`）；`Id` 的类型改为 `ThemeAlgorithmType` 与 `IdentifierType` 的 `xs:union`；删除 `UniqueControlAlgorithmId` 与 `UniqueGlobalAlgorithmId` 两个 `xs:unique`（缺失属性时它们不生效），重复判定由 `ReadAlgorithm` 所在的调用方用 `HashSet<ThemeAlgorithm>` 完成。XSD 同时被 Core 和 Toolchain 嵌入，只改这一份。

- [ ] **Step 4: 改导出**

`ThemePresetCatalog` 导出时，`Catalog` 不是 `AtomUI` 才写 `Catalog` 属性。`ThemePresetsShowCaseTests` 里按 XSD 校验导出结果的测试继续通过。

- [ ] **Step 5: 运行测试并提交**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`，`dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj`
Expected: 通过。

```bash
git commit -am "feat(Theme): 主题 XML 的 Algorithm 元素支持 Catalog 属性与任意合法标识符"
```

### Task 3: 包里的算法进入注册表

**Files:**
- Modify: `src/AtomUI.Core/Theme/IThemeManagerBuilder.cs`、`ThemeManagerBuilder.cs:83-91,229,396-410`、`ThemeStartupPreparation.cs:8-45`
- Modify: `src/AtomUI.Core/Theme/ThemeManager.cs:1930-1931,2019-2032`、`ThemeManager.PackageLoading.cs:46-53`
- Modify: `src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs:244-279`、`src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs:264-285`
- Modify: `src/AtomUI.Generator/TokenResourceKeyGenerator.cs:184-187`；Create: `src/AtomUI.Generator/ThemeSchema/ThemeAlgorithmRegistrationWriter.cs`
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeSchemaRegistryTests.cs`、新建 `ThemeAlgorithmRegistrationTests.cs`；`tests/AtomUI.Generator.Tests/ThemeSchemaGeneratorTests.cs`

**Interfaces:**
- Produces:

```csharp
// IThemeManagerBuilder
IThemeManagerBuilder AddThemeAlgorithms(IEnumerable<ThemeAlgorithmDescriptor> algorithms);

// ThemeSchemaRegistry
internal static ThemeSchemaRegistry CreateGlobalOnly(IReadOnlyList<ThemeAlgorithmDescriptor> packageAlgorithms);
internal bool ExtendsGlobalSchemaOf(ThemeSchemaRegistry previous);
// true when global tokens are reference-identical and every algorithm descriptor of `previous`
// is present by reference here. Replaces HasSameGlobalSchema in ThemeCompiler.CanReuseGlobal.

// ThemeStartupInputs gains: IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms
```

- Produces（生成代码，声明了算法的程序集才输出，Core 除外）：

```csharp
namespace AtomUI.Generated.<SanitizedAssemblyName>;
public static class GeneratedThemeAlgorithmRegistration
{
    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder);  // builder.AddThemeAlgorithms(GeneratedThemeSchema.GetAlgorithms())
}

// One per catalog declared in the assembly; namespace = the project's RootNamespace (fallback: assembly name).
public static class PrimeNGAlgorithms
{
    public static ThemeAlgorithm Aura { get; }        // PrimeNG:Aura
    public static ThemeAlgorithm AuraDark { get; }    // PrimeNG:Aura.Dark  ('.', '-' removed from the member name)
}
```

两个 Id 去掉分隔符后撞名（`A.B` 与 `AB`）时报 `ATOMUITOKEN002`：`"Algorithm ids 'A.B' and 'AB' in catalog 'X' map to the same member name."`。

- [ ] **Step 1: 写失败测试**

```csharp
// ThemeAlgorithmRegistrationTests (Core.Tests)
[Fact] public void Builder_Algorithms_Reach_The_Startup_Registry()
// builder.AddThemeAlgorithms([Descriptor("Test","Flat")]) -> manager registry TryGetAlgorithm(new("Test","Flat")) == true
[Fact] public void Builder_Algorithms_Reach_The_Global_Only_Registry_By_Reference()
// CaptureStartupInputs().Algorithms contains the same descriptor instance (ReferenceEquals);
// fullRegistry.ExtendsGlobalSchemaOf(ThemeSchemaRegistry.CreateGlobalOnly(inputs.Algorithms)) == true (startup reuse holds)
[Fact] public void Duplicate_Algorithm_Identity_Across_Packages_Is_Rejected()
// ThemeSchemaException, message contains "Duplicate algorithm identity 'Test:Flat'"
[Fact] public void A_Theme_Using_A_Package_Algorithm_Compiles_And_Passes_It_The_Seed()
[Fact] public void A_User_Directory_Theme_Xml_Can_Reference_A_Registered_Package_Algorithm()
[Fact] public void A_User_Directory_Theme_Xml_Referencing_An_Unregistered_Algorithm_Is_Reported_And_The_App_Still_Starts()
// the theme is absent from AvailableThemes; ThemeCatalogDiagnostics has ATMTHM2001 naming "Test:Missing"; default theme applied
// ThemeSchemaRegistryTests
[Fact] public void ExtendsGlobalSchemaOf_Accepts_Added_Algorithms_And_Rejects_Replaced_Ones()
[Fact] public void Package_Append_With_A_New_Algorithm_Reuses_The_Committed_Global_Snapshot()
// ThemeCompiler.CompilePackageAppend with registry = previous + one algorithm -> succeeds, global reused
[Fact] public void Package_Append_That_Replaces_An_Existing_Algorithm_Descriptor_Is_Rejected()
// InvalidOperationException "Package append cannot change committed global theme inputs."
// ThemeSchemaGeneratorTests
[Fact] public void Generates_Public_Algorithm_Registration_And_Catalog_Identity_Class()
[Fact] public void Reports_Colliding_Algorithm_Member_Names()
[Fact] public void Does_Not_Generate_Algorithm_Registration_For_An_Assembly_Without_Algorithms()
```

- [ ] **Step 2: 运行，确认失败**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`
Expected: 编译失败（`AddThemeAlgorithms` 不存在）。

- [ ] **Step 3: 实现运行时通道**

`ThemeManagerBuilder` 保存登记的描述符（同一实例，不复制），交给 `ThemeManager` 和 `CaptureStartupInputs`。`ThemeManager` 里 6 个构造注册表的位置改为统一取「Core 的 `GetAlgorithms()` 加已登记的包算法」。`ThemeStartupPreparation.Prepare` 改用 `CreateGlobalOnly(inputs.Algorithms)`。运行时追加包（`PreparePackages`）把新 builder 登记的算法并入。

- [ ] **Step 4: 实现生成器输出**

`ThemeAlgorithmRegistrationWriter` 在 `algorithms.Length > 0` 且不是 Core 时输出上面两个类。

- [ ] **Step 5: 运行测试并提交**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`，`dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`
Expected: 通过。

```bash
git commit -am "feat(Theme): 包声明的主题算法随注册进入 Schema 注册表并覆盖启动与追加路径"
```

### Task 4: 构建契约携带算法，XML 引用未知算法时构建期报错

**Files:**
- Modify: `src/AtomUI.Core/Theme/Definitions/Compiled/ThemeBuildContractAttributes.cs`
- Modify: `src/AtomUI.Generator/ThemeDefinitions/ThemeBuildContracts.cs:44-115,188`、`CompiledThemePackageWriter.cs:32,86-92`
- Test: 新建 `tests/AtomUI.Generator.Tests/ThemeDefinitions/CompiledThemeAlgorithmContractTests.cs`

**Interfaces:**
- Consumes: Task 1 的 `ThemeAlgorithmInput`（带行列）。
- Produces:

```csharp
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ThemeBuildAlgorithmAttribute(int version, string catalog, string id, int revision, int appearanceEffect) : Attribute
```

每个声明了算法的程序集（含 Core）在 `GeneratedThemeBuildContracts.g.cs` 里输出它；`ThemeBuildContracts.Build` 从直接引用的程序集和本地声明读取，产出 `Algorithms` 集合（键为 `Catalog + '\u001f' + Id`）。主题 XML 引用了不在集合里的算法时报 `ATOMUITOKEN002`，文本 `"Unknown algorithm '<Catalog>:<Id>'."`，位置是该元素的行列。

- [ ] **Step 1: 写失败测试**

```csharp
[Fact] public void Writes_Algorithm_Build_Contracts_For_Local_Algorithms()
[Fact] public void Reads_Algorithm_Build_Contracts_From_Referenced_Assemblies()
[Fact] public void Reports_Unknown_Algorithm_In_Theme_Xml_With_Line_And_Column()
// input references PrimeNG:Missing -> one ATOMUITOKEN002, message "Unknown algorithm 'PrimeNG:Missing'.", mapped line/column
[Fact] public void Reports_Unknown_Algorithm_In_A_Control_Custom_Chain()
[Fact] public void Accepts_A_BuiltIn_Algorithm_From_The_Core_Contract()
```

- [ ] **Step 2: 运行，确认失败**

Run: `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`
Expected: 新测试失败。

- [ ] **Step 3: 实现**

`ThemeBuildContracts` 的读取遵循现有 Owner/Token 记录的写法（版本号 1、参数个数校验、异常转 `"Invalid Token build metadata"`）。重复的算法契约报 `"Duplicate algorithm build contract: <Catalog>:<Id>"`。

- [ ] **Step 4: 运行测试并提交**

Run: `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`，`dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj`
Expected: 通过；四个预设包照常构建。

```bash
git commit -am "feat(Theme): 构建契约携带算法身份并在编译主题包时校验未知算法"
```

---

## 阶段二：全局 Alias 显式值先行

### Task 5: 显式 Alias 值带动派生 Alias

**Files:**
- Modify: `src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.cs:72-316`
- Modify: `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs:586-594,760-770`
- Test: 新建 `tests/AtomUI.Core.Tests/Theme/GlobalAliasExplicitOverrideTests.cs`

**Interfaces:**
- Produces（`DesignToken`，做法照 `AbstractControlDesignToken`）：

```csharp
internal void MarkExplicit(int globalSlot);
internal bool IsExplicitSlot(int globalSlot);
internal void ResetExplicit();
private bool IsExplicit(SharedTokenKind kind);   // SharedTokenKind ordinal == global slot
```

- Produces: `CompileGlobalBuilder` 与 `ReevaluateControlGlobal` 的 Alias 段固定为「标记并写入 Alias 显式值 → `CalculateAliasTokenValues()` → 再写一次 Alias 显式值 → `ResetExplicit()`」。显式位图不随 `CopyBuilder` 复制。

需要守卫的字段（本方法内先赋值、后面又被读取）共 8 个：`ColorErrorOutline`、`ColorWarningOutline`、`LineWidthFocus`、`ControlOutlineWidth`、`ColorControlOutline`、`ControlPaddingHorizontal`、`ControlPaddingHorizontalSM`、`UniformlyPaddingSM`。写法：`if (!IsExplicit(SharedTokenKind.ControlOutlineWidth)) { ControlOutlineWidth = LineWidth * 2; }`。Task 6 的分析器会核对这份清单是否完整。

- [ ] **Step 1: 写失败测试（确认缺口真实存在）**

```csharp
[Fact] public void Explicit_ControlOutlineWidth_Drives_InputActiveShadow_Spread()
// Tokens["ControlOutlineWidth"]="0" -> Global<BoxShadows>("InputActiveShadow") first shadow Spread == 0
[Fact] public void Explicit_ColorControlOutline_Drives_InputActiveShadow_Color()        // "#FF0000"
[Fact] public void Explicit_ColorErrorOutline_Drives_InputErrorActiveShadow_Color()
[Fact] public void Explicit_LineWidthFocus_Drives_FocusVisualBorderThickness()          // "1" -> Thickness(1)
[Fact] public void Explicit_ControlPaddingHorizontal_Drives_InputPaddingLG_Left()       // "20" -> Left == 20 - LineWidth
[Fact] public void Explicit_UniformlyPaddingSM_Drives_InputPadding_Left()
[Fact] public void Explicit_Derived_Alias_Still_Wins_Over_The_Recalculation()           // InputActiveShadow set explicitly stays
[Fact] public void Explicit_Zero_Counts_As_Explicit()                                   // ControlOutlineWidth="0" is not treated as unset
[Fact] public void Control_Scoped_Global_Override_Honors_The_Same_Order()               // ControlAlgorithmMode.Global path
[Fact] public void Without_Explicit_Alias_Values_The_Result_Is_Unchanged()              // slot-by-slot equal to a snapshot taken before the change
```

- [ ] **Step 2: 运行，确认前 6 个失败**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`
Expected: 前 6 个与第 9 个失败（派生值仍按默认算）。如果它们一开始就通过，说明规格 2.3 读错了代码，停下来报告。

- [ ] **Step 3: 实现位图、守卫与编译器顺序**

- [ ] **Step 4: 跑测试，处理受影响的预设断言**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`，`dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj`，然后 `scripts/run-full-regression.sh`
Expected: 通过。预设主题里因此变化的值（例如 Aura 的 `InputActiveShadow`）逐项登记进台账；对应断言按新值更新。

- [ ] **Step 5: 新增一条生效测试**

按 `docs/engineering/development/theme-effect-tests.md` 的格式，在 `tests/AtomUI.Desktop.Controls.Rendering.Tests` 里增加一条：设置 `ControlOutlineWidth` 后，输入框获得焦点时的外发光宽度随之变化（设为 0 时没有外发光）。这是常驻测试。

Run: `dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj`
Expected: 通过。

- [ ] **Step 6: 提交**

```bash
git commit -am "fix(Theme): 显式设置的全局 Alias Token 先于默认计算写入并带动派生值"
```

### Task 6: 分析器覆盖 `DesignToken.CalculateAliasTokenValues`

**Files:**
- Modify: `src/AtomUI.Generator/DesignToken/OwnTokenDerivationBaseAnalyzer.cs:42-68,190-209`、`OwnTokenSentinelAnalyzer.cs:39-71`
- Test: `tests/AtomUI.Generator.Tests/DesignToken/OwnTokenDerivationBaseAnalyzerTests.cs`、`OwnTokenSentinelAnalyzerTests.cs`

**Interfaces:**
- Produces: `ATOMUIGEN048` 与 `ATOMUIGEN040` 的范围增加「带 `[GlobalDesignToken]` 的类型里名为 `CalculateAliasTokenValues` 的实例方法」。全局范围的守卫形态是 `if (!IsExplicit(SharedTokenKind.P))`，其中 `P` 等于被赋值的属性名。只检查带 `[DesignTokenKind(Alias)]` 的属性的赋值。

- [ ] **Step 1: 写失败测试**

```csharp
[Fact] public void Reports_Unguarded_Global_Alias_Assignment_Whose_Value_Is_Read_Later()
[Fact] public void Accepts_Global_Alias_Assignment_Guarded_By_SharedTokenKind()
[Fact] public void Ignores_Global_Alias_Assignment_That_Is_Never_Read_Later()
[Fact] public void Ignores_Map_Stage_Properties_Read_In_The_Alias_Method()
[Fact] public void Reports_IsNaN_Sentinel_On_A_Global_Token_In_CalculateAliasTokenValues()
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`
Expected: 先失败后通过。

- [ ] **Step 3: 用分析器核对 Task 5 的清单**

Run: `dotnet build src/AtomUI.Core/AtomUI.Core.csproj -warnaserror:ATOMUIGEN048`
Expected: 0 个诊断。有诊断说明 Task 5 的 8 个字段不全，补守卫并补测试。

- [ ] **Step 4: 提交**

```bash
git commit -am "feat(Generator): ATOMUIGEN040 与 048 覆盖全局 Alias 计算方法"
```

---

## 阶段三：Token 记账

### Task 7: 性能基线

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.Tests/Theme/Performance/ThemeCompileMeasurement.cs`
- Modify: 台账

**Interfaces:**
- Produces: 一个显式运行的测量（`[Fact(Explicit = true)]`，不进常规回归；如果 `AvaloniaFact` 不支持 `Explicit`，改为环境变量 `ATOMUI_RUN_MEASUREMENTS=1` 时才运行，否则跳过）。它是测量工具，不是回归测试，所以不含断言。在注册了全部桌面控件包的应用里测量并输出一行 `MEASURE` 文本：

```csharp
[AvaloniaFact(Explicit = true)] public async Task Measure_Default_Theme_Compile_And_Switch()
// cold compile of DaybreakBlue (new ThemeCompiler, no cache): median of 15 runs after 3 warm-ups, ms
// light <-> dark switch via ApplyThemeAsync: median of 15, ms
// GC.GetAllocatedBytesForCurrentThread() delta of one cold compile, bytes
// number of token instances created in one cold compile (global builders + control builders)
// SHA-256 over every global slot and every control own slot of DaybreakBlue light, dark, compact (hex)
```

- [ ] **Step 1: 写测量并运行 3 次**

Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release`（按项目的 filter 方式只跑这一条显式测试）
Expected: 三次的中位数彼此接近。把三组数字、三次之间的波动范围、机器与 SDK 版本记进台账，作为基线。

- [ ] **Step 2: 记录启动与体积基线**

按 `tools/performances/AtomUI.StartupPerformance/README.md` 的格式写一份 `variants.json`，场景取 `atom-window` 与 `atom-splash`，放在临时目录，不提交。

Run: `python3 tools/performances/AtomUI.StartupPerformance/run_samples.py --config <临时目录>/variants.json --output <临时目录>/startup-baseline --rounds 10 --warmup 1 --timeout 90`
再记录 Release 构建下 `AtomUI.Core.dll`、`AtomUI.Controls.dll`、`AtomUI.Desktop.Controls.dll`、`AtomUI.Desktop.Controls.DataGrid.dll`、`AtomUI.Desktop.Controls.ColorPicker.dll` 的字节数，以及 Gallery NativeAOT 发布产物的体积（发布命令取仓库现有的发布文档；没有可用的 AOT 发布流程时在台账写明，体积门槛只看程序集）。

- [ ] **Step 3: 提交**

```bash
git commit -am "test(Theme): 增加默认主题编译与切换的显式测量并登记性能基线"
```

### Task 8: 记账基类 API 与生成器产出 partial 属性的另一半

**Files:**
- Modify: `src/AtomUI.Core/Theme/DesignTokens/AbstractDesignToken.cs`
- Create: `src/AtomUI.Core/Theme/DesignTokens/TokenWriteLog.cs`
- Create: `src/AtomUI.Generator/DesignToken/TokenPropertyImplementationGenerator.cs`、`TokenPropertyImplementationWriter.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`、`AtomUIDiagnosticDescriptors.cs`、`docs/engineering/development/compiler-diagnostics-guidelines.md`
- Test: 新建 `tests/AtomUI.Core.Tests/Theme/TokenWriteRecordingTests.cs`、`tests/AtomUI.Generator.Tests/DesignToken/TokenPropertyImplementationGeneratorTests.cs`

**Interfaces:**
- Produces（Core）：

```csharp
public abstract class AbstractDesignToken
{
    private TokenWriteLog? _writeLog;                       // null == not recording; the only added field

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RecordWrite(string tokenName)            // if (_writeLog is { } log) log.Add(tokenName);
    internal void BeginRecordingWrites();                   // allocates the log; throws if already recording
    internal IReadOnlyCollection<string> EndRecordingWrites();   // distinct names in first-write order; sets _writeLog = null
    internal bool IsRecordingWrites { get; }
}
```

- Produces（生成器）：一个独立的增量生成器 `TokenPropertyImplementationGenerator`，用 `RegisterSourceOutput`（不是 `RegisterImplementationSourceOutput`，否则 IDE 设计期会报 partial 属性缺实现）。每个带 `[ControlDesignToken]` 或 `[GlobalDesignToken]` 的类型声明输出一个文件 `<TypeName>.TokenProperties.g.cs`：

```csharp
partial class ToolTipToken
{
    public partial global::Avalonia.Media.Color ToolTipBackground
    {
        get => field;
        set { field = value; RecordWrite(nameof(ToolTipBackground)); }
    }
    // Terminal (sealed) control tokens only:
    internal void RunDefaultCalculation(bool isDarkMode) => CalculateTokenValues(isDarkMode);
}
```

可访问性与声明一致（`DataGridToken` 有 45 个 `internal` 属性）。带 `[NotTokenDefinition]` 的属性不处理。`DesignToken` 分布在 14 个 partial 文件里，按每个带 `[GlobalDesignToken]` 的声明各出一份，文件名带声明所在源文件名。
- Produces（诊断）：`ATOMUIGEN049`（Error）`"Token property '{0}' on '{1}' must be declared partial, and '{1}' must be a partial class"`。

- [ ] **Step 1: 写失败测试**

```csharp
// TokenWriteRecordingTests — uses a hand-written token whose setters call RecordWrite
[Fact] public void Writes_Are_Not_Recorded_Unless_Recording_Is_On()
[Fact] public void Recording_Captures_Names_In_First_Write_Order_Without_Duplicates()
[Fact] public void Writing_The_Default_Value_Is_Still_Recorded()          // Colors.Transparent, 0, default(Thickness)
[Fact] public void EndRecording_Turns_Recording_Off()
[Fact] public void BeginRecording_Twice_Throws()
[Fact] public void A_Token_That_Never_Records_Allocates_No_Log()          // GC.GetAllocatedBytesForCurrentThread delta == 0 across 1000 writes
// TokenPropertyImplementationGeneratorTests
[Fact] public void Emits_Field_Backed_Getter_And_Recording_Setter_For_Each_Token_Property()
// ShouldContain("get => field;") and ShouldContain("RecordWrite(nameof(Height));")
[Fact] public void Emits_Matching_Accessibility_For_Internal_Token_Properties()
[Fact] public void Emits_RunDefaultCalculation_Only_On_Sealed_Control_Tokens()
[Fact] public void Emits_Implementations_For_Abstract_Layers_In_Their_Own_Assembly()
[Fact] public void Skips_NotTokenDefinition_Properties()
[Fact] public void Reports_ATOMUIGEN049_For_A_Non_Partial_Token_Property()
[Fact] public void Reports_ATOMUIGEN049_For_A_Non_Partial_Token_Class()
[Fact] public void Keeps_An_Initializer_On_The_Declaring_Half_Compilable()   // generated + source compile with 0 errors
```

- [ ] **Step 2: 运行，确认失败**

Run: `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`
Expected: 失败（生成器不存在）。

- [ ] **Step 3: 实现**

此时仓库里的 Token 类还不是 partial，`ATOMUIGEN049` 会让全仓构建失败。本任务里先把它注册为默认关闭（`isEnabledByDefault: false`），Task 9 改完代码后再打开。生成器只为「类声明是 partial 且属性声明是 partial」的属性输出实现；其余情况只报 `ATOMUIGEN049`、不输出任何代码，所以本任务结束时全仓照常构建。

- [ ] **Step 4: 运行测试并提交**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`，`dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`
Expected: 通过。

```bash
git commit -am "feat(Generator): 为 partial Token 属性生成记录写入的实现"
```

### Task 9: 把全部 Token 改成 partial 属性，控件 Token 类改公开

**Files:**
- Create: `scripts/codemods/partial_token_properties.py`（一次性脚本，本任务结束时删除，不提交）
- Modify: `src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.*.cs`（14 个文件，255 个属性）
- Modify: `src/AtomUI.Core/Theme/DesignTokens/AbstractControlDesignToken.cs:73`
- Modify: 158 个 `[ControlDesignToken]` 类所在文件（`AtomUI.Desktop.Controls` 140、`AtomUI.Controls` 3、`ColorPicker` 6、`DataGrid` 1、`AtomUI.Toolkits.GalleryBase` 8），共 1182 个属性、144 处 `CalculateTokenValues` 重写
- Modify: `src/AtomUI.Generator/ThemeSchema/GeneratedThemeSchemaWriter.cs:241-243`、`Diagnostics/AtomUIDiagnosticDescriptors.cs`（打开 `ATOMUIGEN049`）
- Modify: `tests/AtomUI.Registration.Fixtures/ThirdParty/TriggerControls.cs:8-35`，以及直接调用 `.CalculateTokenValues(` 的测试：`tests/AtomUI.Core.Tests/Theme/ThemeTestSnapshotFactory.cs:102,117`、`ThemeTokenResourceProviderTests.cs:348`、`tests/AtomUI.Desktop.Controls.DataGrid.Tests/Theme/DataGridTokenAlignmentTests.cs:25`、`tests/AtomUI.Desktop.Controls.Tests/Calendar/LunarCalendarThemeTests.cs:56`、`Separator/SeparatorTokenTests.cs:22`、`Skeleton/SkeletonBehaviorTests.cs:38`、`Select/SelectTokenTests.cs:27`、`NavMenu/NavMenuTokenTests.cs:21,36,51,69,86`
- Modify: `tests/AtomUI.Generator.Tests` 里用真实 `AtomUI.Core.dll` 编译的源码桩（`Registration/TypeMapIntegrationTests.cs:91,152,175,311,344,373,426,639`、`TypeMapRegistrationGeneratorTests.cs:56,98`、`ResourcePlatformAvailabilityTests.cs:158`）与自带桩的测试（`ThemeSchemaGeneratorTests.cs:43,505`、`TokenResourceKeyGeneratorTests.cs:949-950,1910`）

**Interfaces:**
- Consumes: Task 8 的生成器与 `RunDefaultCalculation`。
- Produces:
  - `AbstractControlDesignToken.CalculateTokenValues` 变为 `protected virtual void CalculateTokenValues(bool isDarkMode)`；全部重写变为 `protected override`。
  - 生成的 Evaluate 委托变为 `static (token, appearance) => ((global::X.ToolTipToken)token).RunDefaultCalculation(appearance == global::AtomUI.Theme.ThemeAppearance.Dark)`。
  - `AtomUI.Controls`、`AtomUI.Desktop.Controls`、`ColorPicker`、`DataGrid` 四个包里的 148 个内部 Token 类（116 个终端类加 32 个中间层）变为 `public`；GalleryBase 的 8 个保持 `internal`。全部 158 个类变为 `partial`。
  - 测试里要直接触发默认计算的地方改调 `RunDefaultCalculation`；测试自己手写的 Token 类自带同名内部方法。

- [ ] **Step 1: 写脚本并先在一个文件上验证**

脚本只做文本替换，规则：在带 `[ControlDesignToken]` 或 `[GlobalDesignToken]` 的类里，给不带 `[NotTokenDefinition]` 的非静态属性声明在类型前插入 `partial`（保留可访问性、特性、初始化器、多余空格）；类声明插入 `partial`；四个控件包里的 `internal sealed class` / `internal abstract class` 改 `public`；`public override void CalculateTokenValues` 改 `protected override void CalculateTokenValues`。脚本结束时打印改动的类数与属性数。

Run: 先只对 `src/AtomUI.Desktop.Controls/Tooltip/ToolTipToken.cs` 运行，再 `dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj`
Expected: 该文件编译通过（其余文件此时还是旧写法，`ATOMUIGEN049` 仍关闭）。

- [ ] **Step 2: 全量运行脚本，核对数量**

Expected: 脚本报告 158 个类、1182 + 255 个属性、144 处重写。数量对不上时逐一查明差异，不要手工凑数。

- [ ] **Step 3: 改基类、Evaluate 委托、测试调用点，打开 `ATOMUIGEN049`**

- [ ] **Step 4: 全仓构建**

Run: `dotnet build`（解决方案根）
Expected: 0 错误。公开后暴露出的可访问性不一致（公开属性的类型是内部类型，CS0053）逐个处理：该类型本应公开就公开，否则把属性改成 `internal`，并登记进台账。

- [ ] **Step 5: 确认编译结果不变**

Run: Task 7 的测量测试
Expected: 三个主题的 SHA-256 与基线逐字相同。不同就是改坏了，查清原因再继续。

- [ ] **Step 6: 全量回归并提交**

Run: `scripts/run-full-regression.sh`
Expected: `[SUMMARY]` 全部通过。删除脚本后提交：

```bash
git add -A
git commit -m "refactor(Theme): 全部 Token 属性改为 partial 属性并公开控件 Token 类"
```

### Task 10: 性能门槛验收

**Files:**
- Modify: 台账

**Interfaces:**
- Consumes: Task 7 的基线与测量方法。

- [ ] **Step 1: 重跑 Task 7 的全部测量**

同一台机器、同一配置、同样的轮数。

- [ ] **Step 2: 逐项对照门槛并登记**

| 项目 | 通过条件 |
| --- | --- |
| 冷编译中位数 | 与基线之差落在基线三次测量的波动范围内，且不超过 2% |
| 明暗切换中位数 | 同上 |
| 冷编译分配字节 | 增量 ≤ 该次编译创建的 Token 实例数 × 8 |
| `ThemeResourceLookupPerformanceTests` | 原样通过，断言未改 |
| 启动耗时 | 各场景中位数落在基线波动范围内 |
| 程序集与 AOT 体积 | 增幅 ≤ 1% |

- [ ] **Step 3: 不过则处理**

先改实现再重测，可选做法：生成器把 `if` 判断直接展开在 setter 里而不经 `RecordWrite`；给 `RecordWrite` 的冷路径加 `NoInlining` 的辅助方法。仍不过：**停止**，在台账写明数字，向用户报告，不放宽门槛，不进入阶段四。

- [ ] **Step 4: 提交台账**

```bash
git commit -am "docs(Theme): 登记 Token 记账改造后的性能验收结果"
```

---

## 阶段四：Alias 阶段

### Task 11: Alias 阶段契约与编译器三步流程

**Files:**
- Create: `src/AtomUI.Core/Theme/Algorithms/IThemeAliasAlgorithm.cs`、`ThemeAlgorithmContext.cs`、`ThemeAlgorithmContractException.cs`
- Create: `src/AtomUI.Core/Theme/Compilation/StagedTokenEvaluation.cs`
- Modify: `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs:586-594,760-787`
- Test: 新建 `tests/AtomUI.Core.Tests/Theme/ThemeAliasAlgorithmTests.cs`

**Interfaces:**
- Consumes: Task 5 的显式位图与顺序；Task 8 的 `BeginRecordingWrites` / `EndRecordingWrites`。
- Produces:

```csharp
public readonly struct ThemeAlgorithmContext
{
    internal ThemeAlgorithmContext(ThemeAppearance appearance, IReadOnlyList<ThemeAlgorithm> algorithms);
    public ThemeAppearance Appearance { get; }
    public IReadOnlyList<ThemeAlgorithm> Algorithms { get; }
}

public interface IThemeAliasAlgorithm : IThemeAlgorithm
{
    void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token);
}

public sealed class ThemeAlgorithmContractException : InvalidOperationException
{
    public ThemeAlgorithm Algorithm { get; }
    public string? TokenName { get; }
}

// One runner shared by the Alias stage (this task) and the Own-token stage (Task 14).
internal static class StagedTokenEvaluation
{
    internal const int MaxRounds = 3;
    internal static void Run(
        AbstractDesignToken token,
        Action markAndApplyExplicit,          // marks explicit slots and writes explicit values
        Action runDefault,                    // the Ant default calculation for this stage
        Action applyExplicit,                 // writes explicit values again
        Action resetMarks,
        IReadOnlyList<StagedAlgorithmStep> steps,          // empty => today's behaviour, nothing else runs
        Func<string, TokenDescriptor?> resolveToken,       // name -> descriptor, null when unknown
        Func<TokenDescriptor, bool> isWritableInStage,
        Action<TokenDescriptor> markAlgorithmWritten);
}
internal readonly record struct StagedAlgorithmStep(ThemeAlgorithm Algorithm, Action<AbstractDesignToken> Evaluate);
```

规则照规格 4.3：`steps` 为空时只做「标记写入 → 默认 → 再写」，不创建任何对象；否则打开记账依次运行各步、关闭记账（放在 `finally` 里），没有任何写入则结束；有写入则把这些槽位并入显式集合、重跑默认计算、再运行各步、再写用户显式值；两次运行写入的槽位或值不同则再来一轮，超过 `MaxRounds` 抛异常。

三条异常文本：
- `"Algorithm '{algorithm}' wrote token '{name}' outside the {stage} stage."`
- `"Algorithm '{algorithm}' did not stabilize after 3 rounds; unstable token '{name}'."`
- 算法自身抛出的异常包成 `"Algorithm '{algorithm}' failed in the {stage} stage."`，原异常作 `InnerException`。

Alias 阶段的算法实例**复用 Map 阶段 `EvaluateAlgorithms` 创建的那一批**：其中实现了 `IThemeAliasAlgorithm` 的才收进 `steps`，列表按需创建。内置三算法因此不多创建任何对象。`ReevaluateControlGlobal` 走同一条路。

- [ ] **Step 1: 写失败测试**

测试用手写算法 `TestAliasAlgorithm : IThemeAlgorithm, IThemeAliasAlgorithm`，Map 阶段先调 `DefaultThemeVariantCalculator` 再按需改写。

```csharp
[Fact] public void Alias_Stage_Value_Overrides_The_Ant_Default()
// EvaluateAlias: token.ColorTextPlaceholder = token.ColorTextSecondary -> snapshot equals ColorTextSecondary
[Fact] public void Alias_Stage_Write_Drives_Derived_Defaults()
// EvaluateAlias: token.ControlOutlineWidth = 0 -> InputActiveShadow Spread == 0
[Fact] public void Explicit_Theme_Value_Wins_Over_The_Alias_Stage()
// config ControlOutlineWidth="3", algorithm writes 0 -> 3, and InputActiveShadow Spread == 3
[Fact] public void Writing_A_Map_Token_With_Its_Current_Value_Is_Still_A_Stage_Violation()
// token.ColorPrimary = token.ColorPrimary in EvaluateAlias -> ThemeAlgorithmContractException (a write is a setter call, not a value change)
[Fact] public void Writing_Zero_Or_Transparent_Counts_As_Written()
// ControlOutlineWidth = 0 -> Spread == 0; ColorControlOutline = Colors.Transparent -> shadow colour transparent
[Fact] public void Later_Algorithm_In_The_Chain_Overrides_An_Earlier_One()
[Fact] public void Context_Reports_The_Resolved_Appearance_And_The_Full_Chain()
// chain [Test:Light, AtomUI:Dark] -> context.Appearance == Dark; Algorithms sequence equals the chain
[Fact] public void Writing_A_Map_Token_In_The_Alias_Stage_Fails_With_Algorithm_And_Token()
// token.ColorPrimary = ... -> compile fails, ThemeAlgorithmContractException, Algorithm == Test:X, TokenName == "ColorPrimary"
[Fact] public void A_Self_Referential_Write_Is_Reported_As_Unstable()
// token.ControlOutlineWidth = token.ControlOutlineWidth + 1 -> "did not stabilize after 3 rounds", TokenName == "ControlOutlineWidth"
[Fact] public void An_Exception_In_The_Alias_Stage_Names_The_Algorithm_And_Leaves_Recording_Off()
// EvaluateAlias throws -> ThemeAlgorithmContractException with InnerException; the DesignToken passed in has IsRecordingWrites == false
[Fact] public void BuiltIn_Chains_Create_No_Extra_Objects_And_Run_The_Default_Once()
// counting DesignToken subclass / allocation delta equal to the pre-change number recorded in Task 7
[Fact] public void Control_Custom_Algorithm_Mode_Runs_The_Alias_Stage_For_The_Control_Global()
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`
Expected: 先失败后通过。

- [ ] **Step 3: 确认内置主题不变、性能不变**

Run: Task 7 的测量测试
Expected: 三个 SHA-256 与基线相同；冷编译中位数和分配字节仍满足 Task 10 的门槛。

- [ ] **Step 4: 提交**

```bash
git commit -am "feat(Theme): 增加可选的 Alias 算法阶段与三步求值流程"
```

### Task 12: 可复用的算法契约测试

**Files:**
- Create: `tests/AtomUI.Theme.ContractTests/AtomUI.Theme.ContractTests.csproj`（类库，不含测试用例）、`ThemeAlgorithmContract.cs`、`ContrastRatio.cs`
- Modify: `src/AtomUI.Core/AtomUI.Core.csproj:35-56`（`InternalsVisibleTo` 加 `AtomUI.Theme.ContractTests`）、解决方案文件
- Test: 新建 `tests/AtomUI.Core.Tests/Theme/BuiltInAlgorithmContractTests.cs`、`ThemeAlgorithmContractSelfTests.cs`

**Interfaces:**
- Consumes: Task 3 的注册表通道，Task 11 的 Alias 阶段。
- Produces:

```csharp
public sealed record ThemeAlgorithmContractViolation(string Check, ThemeAlgorithm Algorithm, string? TokenName, string Message);

public sealed record ThemeAlgorithmContractInput(
    IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms,
    IReadOnlyList<ControlTokenDescriptor> Controls)
{
    // Runs the caller's registration (e.g. builder => builder.UsePrimeNGThemes()) against a collecting
    // builder and captures everything it registered, plus the Core built-ins.
    public static ThemeAlgorithmContractInput Capture(Action<IThemeManagerBuilder> register);
}

public static class ThemeAlgorithmContract
{
    public static IReadOnlyList<ThemeAlgorithmContractViolation> Verify(
        ThemeAlgorithmContractInput input,
        ThemeAlgorithm algorithm,
        bool standalone = true);
}
```

`Check` 的取值与规格 4.6 的表一一对应：`Identity`、`MapComplete`、`Deterministic`、`InputsUnchanged`、`SeedResponsive`、`StatesDistinct`、`Readable`、`AppearanceConsistent`、`ComposableWithCompact`、`HybridChainReadable`、`Stable`、`StageBoundary`。阈值：正文对比度 4.5，实色上的文字 3（WCAG 相对亮度公式）。`MapComplete` 用两组不同的预填值各跑一次，跑完仍随预填值不同的 Map 槽位即未写字段；每种值类型的两个样本在库里集中定义。

- [ ] **Step 1: 写失败测试**

```csharp
// BuiltInAlgorithmContractTests
[Theory] [InlineData("Default")] [InlineData("Dark")] [InlineData("Compact")]
public void BuiltIn_Algorithm_Satisfies_The_Contract(string id)
// Verify(ThemeAlgorithmContractInput.Capture(_ => { }), new("AtomUI", id), standalone: id == "Default").ShouldBeEmpty()
// ThemeAlgorithmContractSelfTests — each check must be able to fail
[Fact] public void MapComplete_Reports_A_Map_Token_The_Algorithm_Did_Not_Write()       // TokenName == the skipped token
[Fact] public void Deterministic_Reports_An_Algorithm_That_Reads_A_Static_Counter()
[Fact] public void InputsUnchanged_Reports_An_Algorithm_That_Mutates_The_Seed()
[Fact] public void SeedResponsive_Reports_A_Hard_Coded_Primary()
[Fact] public void StatesDistinct_Reports_Identical_Hover_And_Active()
[Fact] public void Readable_Reports_Low_Contrast_Text()
[Fact] public void AppearanceConsistent_Reports_A_Light_Result_From_A_Dark_Algorithm()
[Fact] public void StageBoundary_Reports_A_Map_Write_From_The_Alias_Stage()
[Fact] public void ContrastRatio_Matches_Known_Pairs()   // black/white == 21, #777777/white ≈ 4.48
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`
Expected: 先失败后通过。`Dark` 和 `Compact` 不能单独使用（`standalone: false`），对它们跳过 `MapComplete`。如果内置算法有真实违例（例如某个对比度不到阈值），登记进台账并向用户报告，不要为了通过去调阈值。

- [ ] **Step 3: 提交**

```bash
git commit -am "test(Theme): 增加可复用的主题算法契约校验并覆盖内置三算法"
```

---

## 阶段五：控件阶段

### Task 13: 控件阶段的契约、声明、生成与注册

**Files:**
- Create: `src/AtomUI.Core/Theme/Algorithms/IControlTokenAlgorithm.cs`、`ControlTokenAlgorithmAttribute.cs`
- Create: `src/AtomUI.Core/Theme/DesignTokens/IControlToken.cs`
- Create: `src/AtomUI.Core/Theme/Schema/ControlTokenAlgorithmDescriptor.cs`
- Modify: `src/AtomUI.Core/Theme/IThemeManagerBuilder.cs`、`ThemeManagerBuilder.cs`、`ThemeManager.cs`、`Schema/ThemeSchemaRegistry.cs`
- Modify: `src/AtomUI.Generator/DesignToken/TokenPropertyImplementationWriter.cs`、`ThemeSchema/ThemeAlgorithmRegistrationWriter.cs`、`TargetMarkConstants.cs`
- Test: 新建 `tests/AtomUI.Core.Tests/Theme/ControlTokenAlgorithmRegistryTests.cs`、`tests/AtomUI.Generator.Tests/ThemeSchema/ControlTokenAlgorithmGeneratorTests.cs`

**Interfaces:**
- Consumes: Task 3 的 `GeneratedThemeAlgorithmRegistration`，Task 9 的公开 Token 类。
- Produces:

```csharp
public interface IControlTokenAlgorithm<TToken> where TToken : AbstractControlDesignToken
{
    void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal);
    void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TToken token);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ControlTokenAlgorithmAttribute(string catalog, string id, int revision) : Attribute

// Implemented by the generated half of every terminal control token that is bound to a control.
public interface IControlToken
{
    static abstract ControlTokenIdentity ControlIdentity { get; }
}

public sealed class ControlTokenAlgorithmDescriptor
{
    public static ControlTokenAlgorithmDescriptor Create<TToken, TAlgorithm>(ThemeAlgorithm algorithm, int revision)
        where TToken : AbstractControlDesignToken, IControlToken
        where TAlgorithm : class, IControlTokenAlgorithm<TToken>, new();
    public ThemeAlgorithm Algorithm { get; }
    public ControlTokenIdentity Control { get; }
    public int Revision { get; }
    internal object CompilationIdentity { get; }
    internal void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal);
    internal void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AbstractControlDesignToken token);
}

// IThemeManagerBuilder
IThemeManagerBuilder AddControlTokenAlgorithms(IEnumerable<ControlTokenAlgorithmDescriptor> algorithms);

// ThemeSchemaRegistry
internal bool TryGetControlAlgorithm(ThemeAlgorithm algorithm, ControlTokenIdentity control,
    [NotNullWhen(true)] out ControlTokenAlgorithmDescriptor? descriptor);
```

`Create` 持有 `TAlgorithm` 的单个实例（算法是纯函数）。生成器：`GeneratedThemeAlgorithmRegistration.Register` 追加 `builder.AddControlTokenAlgorithms(...)`，每个 `[ControlTokenAlgorithm]` 一条 `ControlTokenAlgorithmDescriptor.Create<ToolTipToken, PrimeNGToolTipTokens>(new ThemeAlgorithm("PrimeNG","Aura"), 1)`；只声明了控件阶段、没声明算法的程序集也输出这个注册类。注册表修订号计入排序后的（算法身份，控件身份，`Revision`）。同一（算法，控件）重复时抛 `ThemeSchemaException("Duplicate control algorithm '{algorithm}' for control '{control}'.")`。对应的算法或控件没注册时，该实现留在注册表里但永不被调用。

「终端 Token 类绑定到哪个控件」取自 `TokenResourceKeyGenerator` 现有的匹配结果（`controlInfos`）；`IControlToken` 的实现由哪个生成器输出由实施者定，只要每个绑定了控件的终端 Token 类恰好实现一次。

没有 Own Token 类的控件身份本次不支持控件阶段（规格第 9 节），在「编写主题算法」指南里写明。

- [ ] **Step 1: 写失败测试**

```csharp
// ControlTokenAlgorithmRegistryTests
[Fact] public void Registered_Control_Algorithm_Is_Found_By_Algorithm_And_Control()
[Fact] public void Duplicate_Control_Algorithm_Is_Rejected()
[Fact] public void Control_Algorithm_For_An_Unregistered_Control_Is_Kept_But_Not_Resolvable_For_Other_Controls()
[Fact] public void Registry_Revision_Changes_With_Control_Algorithm_Revision_And_Is_Order_Independent()
// ControlTokenAlgorithmGeneratorTests
[Fact] public void Generated_Terminal_Token_Implements_IControlToken_With_Its_Control_Identity()
[Fact] public void Emits_One_Descriptor_Per_ControlTokenAlgorithm_Attribute()
[Fact] public void Emits_Registration_For_An_Assembly_With_Only_Control_Stage_Classes()
[Fact] public void Reports_A_Control_Stage_Class_Whose_Token_Type_Is_Not_A_Bound_Terminal_Token()
// ATOMUITOKEN002 "'{type}' targets '{token}', which is not bound to a control."
[Fact] public void Reports_A_Control_Stage_Class_Without_A_Public_Parameterless_Constructor()
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`，`dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj`
Expected: 先失败后通过。

- [ ] **Step 3: 提交**

```bash
git commit -am "feat(Theme): 增加控件 Token 算法阶段的契约、声明与注册"
```

### Task 14: 编译器执行控件阶段

**Files:**
- Modify: `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs:483-582`、`ThemeCacheKey.cs`
- Test: 新建 `tests/AtomUI.Core.Tests/Theme/ControlTokenAlgorithmCompilationTests.cs`

**Interfaces:**
- Consumes: Task 11 的 `StagedTokenEvaluation.Run` 与 `ThemeAlgorithmContext`；Task 13 的 `TryGetControlAlgorithm`、`EvaluateGlobal`、`EvaluateOwn`。
- Produces: `ThemeAlgorithmContractInput` 增加 `IReadOnlyList<ControlTokenAlgorithmDescriptor> ControlAlgorithms { get; init; }`（默认空，`Capture` 一并收集）；`ThemeAlgorithmContract.Verify` 的 `Deterministic`、`InputsUnchanged`、`Stable`、`StageBoundary` 四项检查扩展到该算法的每个控件阶段实现。
- Produces: 控件编译顺序（规格 4.3）：得到有效全局 Token → 链上各算法该控件的 `EvaluateGlobal`（记账，写入直接生效，不触发 Map 与 Alias 重算）→ 写入用户对该控件的全局覆盖 → Own Token 走 `StagedTokenEvaluation.Run`。使用哪条链：`ControlAlgorithmMode.Custom` 用控件自己的链，其余用作用域的全局链。控件缓存键计入参与的各控件阶段描述符的 `Revision` 与 `CompilationIdentity`。链上没有任何控件阶段实现的控件，走原路径，不多做任何事。

- [ ] **Step 1: 写失败测试**

```csharp
[Fact] public void EvaluateOwn_Value_Overrides_The_Control_Default()
[Fact] public void EvaluateOwn_Write_To_A_Guarded_Token_Drives_Derived_Own_Tokens()
[Fact] public void Explicit_Control_Token_Wins_Over_EvaluateOwn()
[Fact] public void EvaluateGlobal_Changes_The_Global_Value_For_That_Control_Only()
// another control in the same scope still sees the scope value
[Fact] public void Explicit_Control_Scoped_Global_Override_Wins_Over_EvaluateGlobal()
[Fact] public void EvaluateOwn_Sees_The_Final_Effective_Global_Including_EvaluateGlobal_Writes()
[Fact] public void EvaluateOwn_Mutating_EffectiveGlobal_Is_Reported()
// "wrote token 'ColorPrimary' outside the Control stage"
[Theory] [InlineData(ControlAlgorithmMode.Unspecified)] [InlineData(ControlAlgorithmMode.Disabled)]
         [InlineData(ControlAlgorithmMode.Global)] [InlineData(ControlAlgorithmMode.Custom)]
public void Control_Stage_Runs_With_The_Right_Chain_In_Every_Mode(ControlAlgorithmMode mode)
[Fact] public void Later_Algorithm_In_The_Chain_Overrides_An_Earlier_Control_Stage()
[Fact] public void Changing_A_Control_Algorithm_Revision_Invalidates_That_Controls_Cache_Entry()
[Fact] public void Controls_Without_A_Control_Stage_Compile_Exactly_As_Before()
[Fact] public void Contract_StageBoundary_Reports_A_Control_Stage_That_Mutates_EffectiveGlobal()
[Fact] public void Contract_Stable_Reports_A_Self_Referential_Control_Stage_Write()
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`
Expected: 先失败后通过。

- [ ] **Step 3: 确认内置主题不变、性能不变**

Run: Task 7 的测量测试
Expected: SHA-256 与基线相同，门槛仍满足。

- [ ] **Step 4: 提交**

```bash
git commit -am "feat(Theme): 编译器执行控件 Token 算法阶段"
```

### Task 15: 运行时追加包对控件阶段实现的接受规则

**Files:**
- Modify: `src/AtomUI.Core/Theme/ThemeManager.PackageLoading.cs:40-126`、`Schema/ThemeSchemaRegistry.cs`
- Test: `tests/AtomUI.Core.Tests/Theme/ControlTokenAlgorithmRegistryTests.cs`

**Interfaces:**
- Produces: `internal void ThemeSchemaRegistry.ValidateAppendedControlAlgorithms(ThemeSchemaRegistry previous)`。追加带来的控件阶段实现，只有当它的算法**或**它的控件是这次追加新注册的才接受；否则抛 `ThemeLoadException("Package append cannot add a control algorithm for the already registered algorithm '{algorithm}' and control '{control}'.")`。

- [ ] **Step 1: 写失败测试**

```csharp
[Fact] public void Append_Accepts_A_Control_Algorithm_For_A_Newly_Added_Algorithm()
[Fact] public void Append_Accepts_A_Control_Algorithm_For_A_Newly_Added_Control()
[Fact] public void Append_Rejects_A_Control_Algorithm_For_An_Existing_Algorithm_And_Existing_Control()
[Fact] public void Rejected_Append_Leaves_The_Committed_Registry_And_Snapshot_Untouched()
```

- [ ] **Step 2: 运行确认失败，实现，再运行，提交**

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj`

```bash
git commit -am "feat(Theme): 追加包时拒绝会改变已提交结果的控件算法实现"
```

---

## 阶段六：PrimeNG 算法与预设包

### Task 16: 存下 12 个 profile 的编译快照

**Files:**
- Create: `tests/AtomUI.Themes.Tests/PrimeNGMigrationSnapshot.cs`（一次性工具，Task 21 删除）
- 基线目录：仓库之外的临时目录，由环境变量 `ATOMUI_PRIMENG_BASELINE_DIR` 指定，**不提交**（仓库约定 `docs/developments` 只放 Markdown，不放数据转储）。执行者在派发任务时给出这个目录的绝对路径。

**Interfaces:**
- Produces: `PrimeNGMigrationSnapshot.Dump(ThemeSnapshot) : string`（每行 `G <TokenName> = <格式化值>` 或 `C <Catalog>:<ControlId> <TokenName> = <格式化值>`，按名称序数排序，值用 Token 描述符的 `Format`），以及 `Diff(string baseline, string current) : IReadOnlyList<string>`，`BaselineDirectory`（读环境变量，未设置时抛出说明性异常）。一个显式测试把 12 个 profile 各写一份 `<ProfileId>.txt` 到基线目录。

- [ ] **Step 1: 写工具并生成基线**

Run: 显式测试 `Dump_Baseline_For_All_Twelve_Profiles`
Expected: 12 个文件，每个都包含全部全局槽位和全部已注册控件的 Own 槽位。

- [ ] **Step 2: 把改造前的 12 份主题 XML 也拷进基线目录**

把 `src/AtomUI.Themes.PrimeNG/Themes/*.theme.xml` 当前的 12 个文件原样复制到基线目录下的 `xml/` 子目录。Task 18–20 用它们找出「旧 XML 显式写过哪些 Token」。

- [ ] **Step 3: 提交**

```bash
git add tests/AtomUI.Themes.Tests/PrimeNGMigrationSnapshot.cs
git commit -m "test(Themes): 增加 PrimeNG 预设改造用的编译快照导出与对比工具"
```

### Task 17: PrimeNG 色阶与 Aura 的 Map 阶段

**Files:**
- Create: `src/AtomUI.Themes.PrimeNG/Algorithms/PrimeNGPalette.cs`、`PrimeNGTailwindPalettes.cs`、`PrimeNGPreset.cs`、`PrimeNGPresetAlgorithm.cs`、`PrimeNGAuraAlgorithms.cs`
- Modify: `src/AtomUI.Themes.PrimeNG/PrimeNGThemes.cs`、`README.md`
- Modify: `tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj`（引用 `AtomUI.Theme.ContractTests`）
- Test: 新建 `tests/AtomUI.Themes.Tests/PrimeNGPaletteTests.cs`、`PrimeNGAlgorithmContractTests.cs`

**Interfaces:**
- Consumes: Task 3 的注册类，Task 11 的 Alias 接口，Task 12 的 `ThemeAlgorithmContract.Verify`。
- Produces:

```csharp
internal static class PrimeNGPalette
{
    internal static readonly int[] Stops = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950];
    internal static Color Mix(Color first, Color second, double firstWeightPercent);  // per-channel sRGB, JS Math.round (half up)
    internal static Color[] Generate(Color seed500);   // i <= 5: Mix(white, seed, 19 * (5 - i)); i > 5: Mix(black, seed, 15 * (i - 5))
    internal static Color[] Resolve(Color seed500);    // known Tailwind 500 -> the transcribed table; otherwise Generate
}

internal sealed record PrimeNGPreset(/* rule table: which stop each semantic token takes, per appearance; size ladder */);

// Named PrimeNGPresetAlgorithm so it does not read like the generated identity class PrimeNGAlgorithms (Task 3).
public abstract class PrimeNGPresetAlgorithm : IThemeAlgorithm, IThemeAliasAlgorithm
{
    private protected PrimeNGPresetAlgorithm(PrimeNGPreset preset, bool dark);
}

[ThemeAlgorithm("PrimeNG", "Aura", 1, ThemeAppearanceEffect.Light)]
public sealed class PrimeNGAuraAlgorithm : PrimeNGPresetAlgorithm { }
[ThemeAlgorithm("PrimeNG", "Aura.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class PrimeNGAuraDarkAlgorithm : PrimeNGPresetAlgorithm { }
```

`UsePrimeNGThemes` 先调 `GeneratedThemeAlgorithmRegistration.Register(builder)` 再注册主题包。四舍五入必须是「半数向上」（`Math.Floor(x + 0.5)`），不能用 C# 默认的银行家舍入。Tailwind 色板原表（emerald、blue、slate、zinc、red，以及成功、警告、信息家族用到的；blue 是本任务和 Task 22 的测试要用的）从 PrimeUIX 的 primitive Token 转录，README 记下来源包名与版本号。Aura 的映射表取规格 4.7：标「现值」的行是验收基线；标「提议」的行对照 PrimeUIX 的 Aura 预设源码核对后写进 README，核对不了的保留提议值并在 README 标明。暗色算法接在别的算法之后（`previousMap` 非空）时只重算颜色，尺寸沿用前一个 Map。

- [ ] **Step 1: 写失败测试**

```csharp
// PrimeNGPaletteTests — values derived by hand from the formula verified against @primeuix/styled 1.2.0
[Fact] public void Generate_Keeps_The_Seed_At_500()            // Generate(#10B981)[5] == #10B981
[Fact] public void Generate_Tints_Toward_White_Below_500()     // Generate(#10B981)[0] == #F3FCF9
[Fact] public void Generate_Shades_Toward_Black_Above_500()    // Generate(#10B981)[10] == #042E20
[Fact] public void Mix_Rounds_Half_Up()                        // Mix(white, #10B981, 95).G == 252 (251.5 rounds up)
[Fact] public void Resolve_Returns_The_Tailwind_Table_For_A_Known_500()
// Resolve(#10B981): [0]=#ECFDF5 [2]=#A7F3D0 [3]=#6EE7B7 [4]=#34D399 [6]=#059669 [7]=#047857
[Fact] public void Resolve_Generates_For_An_Unknown_Seed()     // Resolve(#123456) sequence equals Generate(#123456)
[Theory] [InlineData("#FFFFFF")] [InlineData("#000000")] [InlineData("#8010B981")]
public void Resolve_Does_Not_Throw_For_Extreme_Or_Translucent_Seeds(string seed)   // 11 colours; alpha of the seed preserved
// PrimeNGAlgorithmContractTests
[Theory] [InlineData("Aura")] [InlineData("Aura.Dark")]
public void Aura_Algorithm_Satisfies_The_Contract(string id)
// ThemeAlgorithmContract.Verify(ThemeAlgorithmContractInput.Capture(b => b.UsePrimeNGThemes()), new("PrimeNG", id)).ShouldBeEmpty()
[Fact] public void Aura_Map_Matches_The_Baseline_Colours()
// light: ColorPrimary #10B981, Hover #059669, Active #047857, Bg #ECFDF5, ColorText #334155, ColorBorder #CBD5E1, ColorError #F87171
// dark:  ColorPrimary #34D399, Hover #6EE7B7, Active #A7F3D0, ColorBgContainer #09090B, ColorBorder #52525B, ColorError #FCA5A5
[Fact] public void Changing_The_Primary_Seed_Moves_Hover_Active_And_Bg()   // seed #3B82F6 -> ColorPrimaryHover #2563EB
[Fact] public void Dark_After_Another_Algorithm_Keeps_The_Previous_Sizes()
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj`
Expected: 先失败后通过。手算的三个色值如与官方包的实际输出不一致，以官方包为准，改测试并在台账说明。

- [ ] **Step 3: 提交**

```bash
git commit -am "feat(Themes): 增加 PrimeNG 色阶生成与 Aura 明暗算法的 Map 阶段"
```

### Task 18: Aura 的 Alias 与控件阶段，XML 瘦身

**Files:**
- Modify: `src/AtomUI.Themes.PrimeNG/Algorithms/PrimeNGPresetAlgorithm.cs`
- Create: `src/AtomUI.Themes.PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs`、`PrimeNGCheckBoxTokens.cs`、`PrimeNGSelectTokens.cs`、`PrimeNGCardTokens.cs`、`PrimeNGDialogTokens.cs`、`PrimeNGToolTipTokens.cs`
- Modify: `src/AtomUI.Themes.PrimeNG/Themes/PrimeNGAura.theme.xml`、`.Dark`、`.Compact`、`.DarkCompact`
- Test: `tests/AtomUI.Themes.Tests/PrimeNGThemePackageTests.cs`、新建 `PrimeNGAuraMigrationTests.cs`

**Interfaces:**
- Consumes: Task 13/14 的控件阶段，Task 16 的 `Dump` 与 `Diff`。
- Produces: 六个控件阶段类，各带 `[ControlTokenAlgorithm("PrimeNG","Aura",1)]` 与 `[ControlTokenAlgorithm("PrimeNG","Aura.Dark",1)]`，用 `context.Appearance` 区分明暗。四份 Aura XML 只保留算法链、种子、规则表达不了的常量；`PrimeNGAura.theme.xml` 与 `PrimeNGAura.Dark.theme.xml` 除算法链外内容相同，暗色主题的 `ColorPrimary` 种子也是 `#10B981`。

Alias 阶段至少覆盖：`ColorFocusBorder` 等于 `ColorPrimary`；`FocusVisualBorderThickness` 为 1；`ControlOutlineWidth` 为 0；`ColorTextPlaceholder` 与 `ColorTextDescription` 等于次要文字色；`ColorBgContainerDisabled` 取中性 200（暗色 700）；`ColorTextLightSolid` 浅色为白、暗色为中性 900；两组阴影。尺寸类的值从 Map 的尺寸 Token 推出，不写常量。

- [ ] **Step 1: 写失败测试**

```csharp
// PrimeNGAuraMigrationTests
[Theory] [InlineData("PrimeNGAura")] [InlineData("PrimeNGAura.Dark")] [InlineData("PrimeNGAura.Compact")] [InlineData("PrimeNGAura.DarkCompact")]
public async Task Tokens_The_Old_Xml_Set_Explicitly_Keep_Their_Values(string profile)
// parse the pre-change XML copy under <baseline dir>/xml, and for every explicit Token assert
// the new snapshot value equals the baseline snapshot value for that slot (temporary test, removed in Task 21)
[Fact] public async Task Swapping_The_Chain_To_Aura_Dark_Equals_The_Dark_Profile()
// PrimeNGAura with chain [PrimeNG:Aura.Dark] == PrimeNGAura.Dark, slot by slot (Diff is empty)
[Fact] public async Task Appending_Compact_Equals_The_Compact_Profile_Except_Listed_Slots()
// Diff(PrimeNGAura + AtomUI:Compact, PrimeNGAura.Compact) contains only the slots named in the README's "不等价项" list
[Fact] public async Task Primary_Seed_Change_Moves_Focus_Ring_And_Selected_Background()
// seed #3B82F6 -> ColorFocusBorder == ColorPrimary; ControlItemBgActive changes from the emerald value
[Fact] public void Aura_Xml_No_Longer_Contains_Rule_Tokens()
// the four files contain no <Token Name="ColorPrimaryHover"|"ColorFocusBorder"|"ControlOutlineWidth"|...> entries
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj`
Expected: 先失败后通过。`PrimeNGThemePackageTests` 里与新行为冲突的断言按新值更新。

- [ ] **Step 3: 列差异清单**

对四个 profile 跑 `Diff(baseline, current)`，把每一行差异连同原因写进台账（例如「原由 Ant 色板派生，现按 PrimeNG 取级」「`InputActiveShadow` 现在跟随 `ControlOutlineWidth`」）。没有原因的差异就是缺陷，修掉。

- [ ] **Step 4: 提交**

```bash
git commit -am "feat(Themes): PrimeNG Aura 的 Alias 与控件阶段进入算法并精简主题 XML"
```

### Task 19: Lara

**Files:**
- Create: `src/AtomUI.Themes.PrimeNG/Algorithms/PrimeNGLaraAlgorithms.cs`
- Modify: Task 18 的六个控件阶段类（追加 Lara 两个身份的 Attribute）、`Themes/PrimeNGLara*.theme.xml`（4 份）
- Test: `PrimeNGAlgorithmContractTests.cs`、新建 `PrimeNGLaraMigrationTests.cs`

**Interfaces:**
- Produces: `PrimeNGLaraAlgorithm`（`PrimeNG:Lara`，Light）、`PrimeNGLaraDarkAlgorithm`（`PrimeNG:Lara.Dark`，Dark）。规则表从现有 4 份 Lara XML 转录，XML 的值是验收基线；Lara 额外设置的 `ColorControlOutline` 进 Alias 阶段。

- [ ] **Step 1: 写失败测试**（与 Task 18 的五个测试同构，profile 换成 Lara；另加）

```csharp
[Fact] public async Task Lara_Keeps_Its_Larger_Control_Height_And_Select_Padding()
// ControlHeight == 44; Own<Thickness>("Select","OptionPadding") == Thickness(16,10); "PopupContentPadding" == Thickness(0,8)
```

- [ ] **Step 2: 运行确认失败，实现，再运行，列差异清单，提交**

```bash
git commit -am "feat(Themes): 增加 PrimeNG Lara 明暗算法并精简主题 XML"
```

### Task 20: Nora

**Files:**
- Create: `src/AtomUI.Themes.PrimeNG/Algorithms/PrimeNGNoraAlgorithms.cs`、`Algorithms/Controls/PrimeNGStepsTokens.cs`
- Modify: 六个控件阶段类（追加 Nora 两个身份）、`Themes/PrimeNGNora*.theme.xml`（4 份）
- Test: `PrimeNGAlgorithmContractTests.cs`、新建 `PrimeNGNoraMigrationTests.cs`

**Interfaces:**
- Produces: `PrimeNGNoraAlgorithm`、`PrimeNGNoraDarkAlgorithm`。规则表从现有 4 份 Nora XML 转录。与 Aura 的已知差异（规格 4.7）：主色取级浅色 600、700、800，暗色 500、400、300；文字与边框取更深的中性级；次要边框透明；暗色的焦点环与选中底色取法不同。`PrimeNGStepsTokens` 只关联 `PrimeNG:Nora.Dark`（以及它在浅色外观下不写任何值，以便混合链安全）。

- [ ] **Step 1: 写失败测试**（与 Task 18 同构，另加）

```csharp
[Fact] public async Task Nora_Takes_Darker_Primary_Stops()
// light: ColorPrimary == emerald-600 #059669, Hover #047857, Active #065F46
[Fact] public async Task Nora_Dark_Steps_Override_Comes_From_The_Control_Stage()
// the Steps own tokens the old Nora.Dark XML set keep their baseline values; no <Control Id="Steps"> remains in the XML
```

- [ ] **Step 2: 运行确认失败，实现，再运行，列差异清单，提交**

```bash
git commit -am "feat(Themes): 增加 PrimeNG Nora 明暗算法并精简主题 XML"
```

### Task 21: PrimeNG 收尾

**Files:**
- Modify: `src/AtomUI.Themes.PrimeNG/README.md`、`tests/AtomUI.Themes.Tests/PrimeNGThemePackageTests.cs`
- Delete: `tests/AtomUI.Themes.Tests/PrimeNGMigrationSnapshot.cs`、三个 `PrimeNG*MigrationTests.cs` 里依赖基线文件的用例；仓库外的基线临时目录一并删除

**Interfaces:**
- Produces: README 的「映射与来源」一节（色阶公式、色板原表的来源包与版本、三个预设的规则表、紧凑的不等价项清单）；「换算法不等于选 profile」的说明改为：把链换成同族暗色算法与选暗色 profile 等价；全局追加 `AtomUI:Dark` 得到的是混合链，不等价。

- [ ] **Step 1: 把长期有价值的断言留进常驻测试**

暗色等价、紧凑等价（含不等价项）、种子跟随这三类测试不依赖基线文件，保留在 `PrimeNGThemePackageTests`。

```csharp
[Theory] [InlineData("Aura")] [InlineData("Lara")] [InlineData("Nora")]
public async Task Dark_Profile_Equals_Light_Profile_With_The_Dark_Algorithm(string preset)
[Theory] [InlineData("Aura")] [InlineData("Lara")] [InlineData("Nora")]
public async Task Global_Dark_Toggle_Yields_A_Readable_Hybrid_Chain(string preset)
// chain [PrimeNG:<preset>, AtomUI:Dark]: contrast(ColorText, ColorBgContainer) >= 4.5
[Fact] public async Task All_Six_PrimeNG_Algorithms_Satisfy_The_Contract()
```

- [ ] **Step 2: 删除一次性工具与基线，跑全量回归，提交**

Run: `scripts/run-full-regression.sh`

```bash
git commit -am "docs(Themes): 更新 PrimeNG 预设包的映射说明并移除改造用的快照工具"
```

---

## 阶段七：Gallery 与文档

### Task 22: Gallery 主色色块

**Files:**
- Modify: `controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/ViewModels/ThemePresetsViewModel.cs`、`Views/ThemePresetsShowCase.axaml`、`Localization/ThemePresetsShowCaseLangResourceKind.cs` 及对应语言数据
- Modify: `docs/gallery/authoring/theme-presets-page.md`
- Test: `tests/AtomUIGallery.Tests/ThemePresetsShowCaseTests.cs`

**Interfaces:**
- Produces（视图模型）：

```csharp
public IReadOnlyList<Color> PrimarySwatches { get; }     // emerald, blue, violet, rose, amber 500s + the preset's own accent
public Color? SelectedPrimary { get; set; }              // null == the preset's own seed
// PreviewConfig gains .WithToken("ColorPrimary", <hex>) when SelectedPrimary is set
```

- [ ] **Step 1: 写失败测试**

```csharp
[Fact] public async Task Selecting_A_Swatch_On_A_PrimeNG_Preset_Moves_Hover_Active_And_Focus_Ring()
// SelectedPrimary = #3B82F6 -> preview snapshot ColorPrimaryHover == #2563EB and ColorFocusBorder == ColorPrimary
[Fact] public async Task Clearing_The_Swatch_Restores_The_Preset_Seed()
[Fact] public async Task Copied_Theme_Xml_Includes_The_Selected_Primary_And_Validates_Against_The_Xsd()
```

- [ ] **Step 2: 运行确认失败，实现，再运行**

Run: `dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj`

- [ ] **Step 3: 人工验收**

用 `atomui-gallery-local-debug` 技能启动本仓库的 Gallery（宿主是 `atom:Window`），在预设页选 PrimeNG Aura，点选色块，确认按钮悬停、按下、焦点环、选中底色都跟着变；明暗 profile 各看一遍。结果记进台账。

- [ ] **Step 4: 提交**

```bash
git commit -am "feat(Gallery): 主题预设页增加主色色块以演示算法跟随种子"
```

### Task 23: 文档、CHANGELOG 与 API 变更文档

**Files:**
- Modify: `docs/reference/theming/theme-definition-xml-v1.md`（第 5 节：算法集合不再封闭，`Catalog` 属性）、`docs/reference/theming/compiled-theme-abi-v1.md`、`docs/architecture/systems/theming/runtime.md`（编译顺序、三阶段、记账）、`docs/guides/theming/preset-theme-packages.md`、`docs/guides/theming/customization.md`、`docs/modules/themes/overview.md`、`docs/engineering/development/control-token-guidelines.md`（partial 属性、`protected override`）、`docs/engineering/development/compiler-diagnostics-guidelines.md`（`ATOMUIGEN049`，040/048 的新范围）
- Modify（Task 9 审阅后补入，这四份还在展示旧的 Token 写法）: `docs/guides/theming/third-party-control-packages.md`、`docs/architecture/systems/theming/control-design-token-inheritance.md`、`docs/modules/toolkits-gallery-base/theming-localization.md`、`.github/copilot-instructions.md`。文档要写明：抽象中间层公开后可被外部继承，它们的 `protected` 辅助方法是公开承诺的一部分。
- Create: `docs/guides/theming/writing-theme-algorithms.md`
- Modify: `CHANGELOG.md`、`CHANGELOG.zh-CN.md`、`docs/releases/` 下当前版本的 API 变更文档（中英各一）、`README.md:247`、`DESIGN.md:24,512`、`DESIGN.en-US.md:24,512`

**Interfaces:**
- Produces: 「编写主题算法」指南，覆盖：声明与注册（`[ThemeAlgorithm]`、`GeneratedThemeAlgorithmRegistration.Register`、`<Catalog>Algorithms`）；三个阶段各能写什么；四条共同约束（纯函数、不改输入、不需要重算派生值、尺寸从 Map 推出）；不支持用字段自己的当前值算新值；链的组合与混合链；控件阶段与 `[ControlTokenAlgorithm]`；没有 Own Token 类的控件不支持控件阶段；用 `ThemeAlgorithmContract.Verify` 自测；三条异常文本的含义。API 变更文档逐条列出规格第 5 节的表。

- [ ] **Step 1: 写文档**

- [ ] **Step 2: 核对文档里的代码与事实**

文档里每个类型名、方法签名、诊断号、异常文本都对照代码核一遍；XML 示例用 `ThemeDocumentReader` 实际读一遍（放进现有的文档示例测试，或临时测试用完即删）。

- [ ] **Step 3: 全量回归与最终性能复测**

Run: `scripts/run-full-regression.sh`，然后 Task 7 的全部测量
Expected: `[SUMMARY]` 通过；默认主题的 SHA-256 与基线相同（Task 5 登记的差异除外，那是在 Task 7 之前发生的，基线已包含）；Task 10 的门槛全部满足。PrimeNG 主题的冷编译耗时单独记进台账，不设门槛。

- [ ] **Step 4: 提交**

```bash
git commit -am "docs(Theme): 更新主题算法策略化的参考、指南与变更说明"
```

---

## 不在本计划内

- Material、Bootstrap、shadcn 三个预制算法（规格 4.10，各自单独立项，实施前各补一页映射表）。
- 「暗色对应算法」元数据与 Gallery 全局明暗开关的同族切换。
- 把契约测试库作为 NuGet 包发布。
- 没有 Own Token 类的控件身份参与控件阶段。
