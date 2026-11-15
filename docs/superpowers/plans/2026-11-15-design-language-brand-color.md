# 设计语言与品牌色分离 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 主题定义显式声明所属设计语言、品牌色能力（`Seed`/`Fixed`）与变体（外观、密度）；Core 据此解析选择；Gallery 让每种设计语言独立记住品牌色，`Fixed` 语言禁用选色。

**Architecture:** 元数据从 XML 进入 `ThemeDocument`（运行时 XML 与编译主题共用 `ThemeDocumentReader`），经 `CompiledThemeCatalog`（XML）或 `CompiledThemeProgram` ABI 2（编译包）进入 `ThemeInfo`。单定义规则在 reader 里校验，跨定义规则在生成器（包内）与目录合并（全部来源）校验。`ThemeSelectionResolver` 是纯函数，把选择解析成 profile + 追加算法 + `ColorPrimary` 种子；品牌记忆归调用方（GalleryBase 工作区视图模型）。

**Tech Stack:** .NET 10 / C#、Avalonia 12.1.6、xUnit v3 + Shouldly、Roslyn 源生成器、MSBuild 任务（AtomUI.Toolchain）。

**Spec:** `docs/superpowers/specs/2026-11-15-design-language-brand-color-design.md`

## Global Constraints

- 公开 API 只增不改：`ThemeInfo` 只追加 `init` 属性；`ThemeRequest`/`ThemeConfig` 语义不变。
- 属性：`DesignLanguage`（标识符，缺省 `null`）、`BrandColor`（`Seed`/`Fixed`，缺省 `Fixed`）、`Density`（`Normal`/`Compact`，缺省 `Normal`）。
- 品牌派生 Token 集合 = `ColorPrimaryHover, ColorPrimaryActive, ColorPrimaryBg, ColorPrimaryBgHover, ColorPrimaryBorder, ColorPrimaryBorderHover, ColorPrimaryText, ColorPrimaryTextHover, ColorPrimaryTextActive, ColorControlOutline`。`ColorLink` 不在其中。
- ABI 1 编译包继续加载，三项取缺省；新包写 ABI 2。
- 跨定义冲突在运行时目录只发 Warning、冲突者退出分组（`DesignLanguage = null`），不阻止启动；在生成器包内是 Error。
- 各包声明：`DaybreakBlue` → `AntDesign`/`Seed`；PrimeNG → `PrimeNG.Aura|Lara|Nora`/`Seed`；Bootstrap → `Bootstrap`/`Fixed`；Material → `Material`/`Fixed`；Shadcn → `Shadcn.Zinc|Slate|Stone`/`Fixed`。
- Gallery 删除 PolarGreen/SunsetOrange/GoldenPurple/Magenta 四个主题文件，改为 `AntDesign` 命名色块（色值取自被删文件的 `ColorPrimary`：`#52C41A`、`#FA8C16`、`#722ED1`、`#EB2F96`，加 Daybreak Blue `#1677FF`）。
- 代码内 summary 用英文；提交信息走 `commit-msg` skill，不加任何 AI 署名。
- 视觉验收用 `atom:Window` 宿主。
- 前提：本日其他未提交修复已提交到 `release/6.0`；另一会话对 Bootstrap 包、Gallery 分组菜单（`WorkspaceWindow.axaml.cs`）、主题文档、`docs/releases/unreleased-*` 的改动已落地。是否用独立分支/worktree 按 `docs/engineering/contributing/branch-and-worktree-guidelines.md` 先向用户申请。

## Review Focus

1. 用户主题目录里放一个 `DesignLanguage="AntDesign"`、Light/Normal 的 XML（与内置 `DaybreakBlue` 冲突）→ 应用照常启动，发 Warning，该定义可按 Id 使用但不进 `AntDesign` 分组。测试：Task 4。
2. 在支持的语言选了品牌色，切到 `Fixed` 语言再切回 → 回来时品牌色仍在；`Fixed` 语言根主题的 `ColorPrimary` 等于它自己的种子。测试：Task 7。
3. 用户目录主题热重载把 `BrandColor` 从 `Seed` 改为 `Fixed`（或改了 `DesignLanguage`）→ 重载后的分组与能力随之更新，不残留旧值。测试：Task 4。
4. 设计语言没有某个变体的 profile（Ant Design 无 `.Dark`）且外观为 Dark/System → 回退到基础 profile 的算法链 + 内置 Dark/Compact，而不是丢掉基础链。测试：Task 5。
5. 非 Tailwind 500 的品牌色（如 `#123456`）用于 PrimeNG → 走 palette 公式，派生的 Hover/Bg 与种子不同且明暗都生效。测试：Task 6。

---

### Task 1: 元数据进入 XML 管线与 `ThemeInfo`

**Files:**
- Create: `src/AtomUI.Core/Theme/ThemeDesignLanguageMetadata.cs`（两个枚举）
- Modify: `src/AtomUI.Core/Theme/ThemeInfo.cs`、`src/AtomUI.Core/Theme/Definitions/ThemeDocument.cs:49-79`、`src/AtomUI.Core/Theme/Definitions/ThemeDocumentReader.cs:267-279`（`ReadTheme`）及 `Finish` 处的 `new ThemeDocument(...)`、`src/AtomUI.Core/Theme/Definitions/CompiledThemeCatalog.cs:500-520`（构造 `ThemeInfo`）、`docs/reference/theming/schemas/atomui-theme-v1.xsd`
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeDocumentReaderTests.cs`、`tests/AtomUI.Core.Tests/Theme/CompiledThemeCatalogResolverTests.cs`

**Interfaces:**
- Produces:
  - `public enum ThemeDensity : byte { Normal, Compact }`、`public enum ThemeBrandColorSupport : byte { Fixed, Seed }`（命名空间 `AtomUI.Theme`）
  - `ThemeInfo` 新增 `public string? DesignLanguage { get; init; }`、`public ThemeDensity Density { get; init; }`、`public ThemeBrandColorSupport BrandColor { get; init; }`、`public IReadOnlyList<ThemeAlgorithm> Algorithms { get; init; } = [];`（定义自身的算法链，供解析器回退使用）
  - `ThemeDocument` 新增同名只读属性 `DesignLanguage`、`Density`、`BrandColor`

- [ ] **Step 1: 写失败测试**（reader）

```csharp
[Fact]
public void Read_Maps_Design_Language_Metadata()
{
    var doc = Read("""<Theme xmlns="https://atomui.net/schemas/theme/v1" Id="A.Dark" Name="A" Appearance="Dark"
                      Density="Compact" DesignLanguage="PrimeNG.Aura" BrandColor="Seed"><Algorithms><Algorithm Id="Default"/></Algorithms></Theme>""");
    doc.DesignLanguage.ShouldBe("PrimeNG.Aura");
    doc.Density.ShouldBe(ThemeDensity.Compact);
    doc.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
}

[Fact]
public void Read_Defaults_Missing_Metadata_To_Null_Normal_Fixed() { /* 同上去掉三属性：null / Normal / Fixed */ }

[Fact]
public void Read_Rejects_Invalid_Design_Language_Identifier() { /* DesignLanguage="a b" → ATMTHM1002 schema 或标识符错误 */ }
```

目录测试 `Catalog_Publishes_Design_Language_Metadata_In_ThemeInfo`：用户目录 XML 带三属性 → `manager.AvailableThemes` 对应项 `DesignLanguage/Density/BrandColor/Algorithms` 正确。

`ThemeInfo` 是 record，按值比较被现有测试使用（如 `GalleryBuiltInThemeTests` 用 `new ThemeInfo(...)` 与目录项比较）。新增测试 `ThemeInfo_Equality_Compares_Algorithms_By_Sequence`：两个 `Algorithms` 内容相同但实例不同的 `ThemeInfo` 相等、`GetHashCode` 相同；`Algorithms` 不同则不等。

- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~ThemeDocumentReaderTests|FullyQualifiedName~CompiledThemeCatalogResolverTests"`，新测试编译失败或 FAIL。
- [ ] **Step 3: 实现** — `ThemeInfo` 重写 `Equals(ThemeInfo?)`/`GetHashCode` 使 `Algorithms` 按序列比较（其余成员沿用 record 语义，含 `AccentColor` 与三项新属性）；XSD 给 `Theme` 元素加三个可选属性（`DesignLanguage` 用与 `Id` 相同的标识符类型；`Density`、`BrandColor` 为枚举限制）；`ReadTheme` 读取（非法值走现有 schema 诊断）；`CompiledThemeCatalog` 用 `document` 的三项与 `document.Algorithms` 填 `ThemeInfo`（`with { ... }` 或 init 初始化器）。
- [ ] **Step 4: 运行确认通过** — 同 Step 2 命令，全绿；再跑 `dotnet test tests/AtomUI.Core.Tests` 无回归。
- [ ] **Step 5: 提交**（`commit-msg` skill）

### Task 2: 单定义规则 3101（`Seed` 不得手写品牌派生 Token）

**Files:**
- Modify: `src/AtomUI.Core/Theme/Definitions/ThemeDocumentReader.cs`（`Finish` 前校验；新增常量 `SeedPinsBrandTokenCode = "ATMTHM3101"`）
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeDocumentReaderTests.cs`

**Interfaces:**
- Consumes: Task 1 的 `ThemeDocument.BrandColor`
- Produces: 诊断码 `ATMTHM3101`（Error）。编译主题因 `GenerateCompiledThemeInputTask` 同样调用 `ThemeDocumentReader.Read`，构建期自动报同一错误。

规格 5.3 的 3102 不实现：`DesignToken` 总为 `ColorPrimary` 提供默认种子，条件永不成立（规格允许实现者删去）。

- [ ] **Step 1: 写失败测试**

```csharp
[Theory]
[InlineData("ColorPrimaryHover")]
[InlineData("ColorControlOutline")]
public void Read_Rejects_Seed_Theme_That_Pins_A_Brand_Token(string token)
{
    var result = ReadResult($"""<Theme ... BrandColor="Seed"><Algorithms><Algorithm Id="Default"/></Algorithms>
        <Tokens><Token Name="{token}" Value="#FF0000"/></Tokens></Theme>""");
    result.Success.ShouldBeFalse();
    result.Diagnostics.ShouldContain(d => d.Code == "ATMTHM3101");
}

[Fact] public void Read_Rejects_Seed_Theme_With_Control_Level_ColorPrimaryBg() { /* <Controls><Control ...><Tokens><Token Name="ColorPrimaryBg" .../> → ATMTHM3101 */ }
[Fact] public void Read_Accepts_Seed_Theme_With_Only_ColorPrimary_And_ColorLink() { /* Success */ }
[Fact] public void Read_Accepts_Fixed_Theme_That_Pins_Brand_Tokens() { /* Success */ }
```

- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~ThemeDocumentReaderTests"`
- [ ] **Step 3: 实现** — `BrandColor == Seed` 时检查全局 Token 与每个 `ControlThemeDocument.Tokens` 的名字是否在 Global Constraints 的集合内；命中则以该 Token 的位置报 `ATMTHM3101`，消息写明 Token 名与"declare BrandColor="Fixed" or remove the token"。
- [ ] **Step 4: 运行确认通过**（同上）
- [ ] **Step 5: 提交**

### Task 3: 编译主题管线（输入格式 6、ABI 2、包内跨定义规则）

**Files:**
- Modify: `src/AtomUI.Toolchain/Common/ThemeDefinitions/CompiledThemeInput.cs`（`FormatVersion` 5→6，新增字段 `DesignLanguage`(string, 空串表示 null)、`Density`(int)、`BrandColor`(int)）、`src/AtomUI.Toolchain/Tasks/GenerateCompiledThemeInputTask.cs:80-87`、`src/AtomUI.Generator/ThemeDefinitions/CompiledThemePackageWriter.cs:38-40,112-116`、`src/AtomUI.Core/Theme/Definitions/Compiled/CompiledThemeProgram.cs:10-33`
- Test: `tests/AtomUI.Generator.Tests/ThemeDefinitions/CompiledThemeMetadataTests.cs`（新建，沿用 `CompiledThemeAlgorithmContractTests` 的生成器驱动方式）、`tests/AtomUI.Core.Tests/Theme/CompiledThemeProgramAbiTests.cs`（新建）

**Interfaces:**
- Consumes: Task 1 的枚举与 `ThemeInfo` 属性
- Produces:
  - 现有构造函数保持签名，只接受 `abiVersion == 1`，`Info` 三项取缺省、`Algorithms` 为空。
  - 新增 `protected CompiledThemeProgram(int abiVersion, string sourceIdentity, string sourceRevision, string contentDigest, string programDigest, string id, string name, ThemeAppearance appearance, bool isDefault, Color? accentColor, string? designLanguage, ThemeDensity density, ThemeBrandColorSupport brandColor, string[] algorithms)`，只接受 `abiVersion == 2`；`algorithms` 元素格式 `"Catalog:Id"`，解析为 `ThemeAlgorithm`。
  - 生成器写 `base(2, ...)`；programDigest 的 `fields` 追加三项与 `compiled-abi-v2`。

- [ ] **Step 1: 写失败测试**
  - `CompiledThemeProgramAbiTests.Abi1_Program_Loads_With_Default_Metadata`：一个手写 ABI 1 子类 → `Info.DesignLanguage == null`、`Density == Normal`、`BrandColor == Fixed`。
  - `Abi2_Program_Publishes_Metadata_And_Algorithms`：ABI 2 子类传 `"PrimeNG.Aura", Compact, Seed, ["PrimeNG:Aura","AtomUI:Compact"]` → `Info` 对应；`Algorithms` 为两个 `ThemeAlgorithm`。
  - `Abi_Mismatch_Throws`：ABI 1 构造函数传 2、ABI 2 构造函数传 1 → `NotSupportedException`。
  - `CompiledThemeMetadataTests.Generator_Emits_Abi2_With_Metadata`：两份 XML（Light/Normal、Dark/Normal，同 `DesignLanguage`、同 `BrandColor`）→ 生成源码含 `base(2,` 与 `"PrimeNG.Aura"`，无诊断。
  - `Generator_Reports_Duplicate_Variant_In_Package`：两份同 `DesignLanguage` 且同 (Appearance, Density) → 生成器错误，消息含 `ATMTHM3103`。
  - `Generator_Reports_Mixed_Brand_Support_In_Package`：同语言一份 `Seed` 一份 `Fixed` → 错误含 `ATMTHM3104`。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~CompiledThemeProgramAbiTests"`；`dotnet test tests/AtomUI.Generator.Tests --filter "FullyQualifiedName~CompiledThemeMetadataTests"`
- [ ] **Step 3: 实现** — 输入编解码追加字段（旧格式按现有"out of date; rebuild"处理）；任务从 `document` 映射；生成器在 `Write` 循环里按 `DesignLanguage` 收集 (Appearance, Density) 与 BrandColor，冲突时用现有 `Error(...)` 报出并跳过冲突项；构造函数二选一分派。
- [ ] **Step 4: 运行确认通过**（同上），再跑 `dotnet build AtomUI.slnx` 确认所有主题包重新生成无错。
- [ ] **Step 5: 提交**

### Task 4: 运行时目录的跨定义规则（Warning + 退出分组）

**Files:**
- Modify: `src/AtomUI.Core/Theme/Definitions/CompiledThemeCatalog.cs:256-297`（`BuildFromSlices`，在 `defaults` 检查之前）
- Test: `tests/AtomUI.Core.Tests/Theme/CompiledThemeCatalogResolverTests.cs`、`tests/AtomUI.Core.Tests/Theme/ThemeCatalogReloadTests.cs`

**Interfaces:**
- Consumes: Task 1 的 `ThemeInfo` 属性
- Produces: 诊断 `ATMTHM3103`/`ATMTHM3104`，`ThemeDiagnosticSeverity.Warning`；冲突者的 `CompiledThemeDefinition` 以 `entry with { Info = entry.Info with { DesignLanguage = null } }` 替换（字典与有序列表都替换）。

- [ ] **Step 1: 写失败测试**
  - `Catalog_Demotes_User_Theme_That_Duplicates_A_Package_Variant`（Review Focus 1）：包内 `AntDesign` Light/Normal + 用户目录同语言同变体 → 加载成功；诊断含 Warning `ATMTHM3103`；用户主题 `DesignLanguage == null`，包主题不变；按 Id 仍可 `ApplyThemeAsync`。
  - `Catalog_Lets_A_User_Theme_Add_A_Missing_Variant`：用户目录 `AntDesign` Dark/Normal、`BrandColor="Seed"` → 无诊断，保留 `DesignLanguage`。
  - `Catalog_Demotes_A_Variant_With_Different_Brand_Support`：同语言新增变体但 `Fixed` → Warning `ATMTHM3104`，退出分组。
  - `Reload_Recomputes_Design_Language_Grouping`（Review Focus 3，放 `ThemeCatalogReloadTests`）：重载前 `Seed`、重载后改为 `Fixed` 且冲突 → 新目录中该项被降级；改回后恢复。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~CompiledThemeCatalogResolverTests|FullyQualifiedName~ThemeCatalogReloadTests"`
- [ ] **Step 3: 实现** — 合并后按加载顺序遍历，以 `DesignLanguage` 记录首个定义的 `BrandColor` 与已占用的 (Appearance, Density)；冲突者降级并加 Warning（`source` 为其 `Revision.SourceIdentity`，`path` 为 `$`）。
- [ ] **Step 4: 运行确认通过**（同上）
- [ ] **Step 5: 提交**

### Task 5: `ThemeSelectionResolver`

**Files:**
- Create: `src/AtomUI.Core/Theme/ThemeSelection.cs`（`ThemeSelection`、`ThemeSelectionResult`、`ThemeSelectionResolver`）
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeSelectionResolverTests.cs`（新建；用手造 `ThemeInfo` 列表，不需要主题管理器）

**Interfaces:**
- Consumes: Task 1 的 `ThemeInfo` 属性
- Produces:

```csharp
public sealed record ThemeSelection(string DesignLanguage, ThemeAppearance Appearance, ThemeDensity Density, Color? BrandColor);
public sealed record ThemeSelectionResult(string ThemeId, IReadOnlyList<ThemeAlgorithm>? Algorithms, Color? BrandSeed, bool BrandColorIgnored);
public static class ThemeSelectionResolver
{
    public static ThemeSelectionResult Resolve(IReadOnlyList<ThemeInfo> catalog, ThemeSelection selection);
    public static IReadOnlyList<string> DesignLanguages(IReadOnlyList<ThemeInfo> catalog); // 按目录顺序去重
    public static ThemeBrandColorSupport BrandColorSupport(IReadOnlyList<ThemeInfo> catalog, string designLanguage);
}
```

规则见规格 5.4。回退时 `Algorithms = base.Algorithms` 去掉其中的 `ThemeAlgorithm.Dark`/`Compact` 后，再按选择追加 `ThemeAlgorithm.Dark`、`ThemeAlgorithm.Compact`（顺序：基础链、Compact、Dark，与现 `GalleryWorkspaceViewModel` 一致）；精确匹配时 `Algorithms = null`。

- [ ] **Step 1: 写失败测试**
  - `Exact_Variant_Uses_The_Profile_Chain`：Aura 四变体，选 Dark/Compact → `ThemeId == "PrimeNGAura.DarkCompact"`、`Algorithms == null`。
  - `Missing_Variant_Falls_Back_To_Base_Chain_Plus_BuiltIns`（Review Focus 4）：`AntDesign` 只有 `DaybreakBlue`（`Algorithms=[Default]`），选 Dark/Compact → `ThemeId == "DaybreakBlue"`、`Algorithms == [Default, Compact, Dark]`。
  - `Seed_Language_Returns_The_Brand_Seed` / `Fixed_Language_Ignores_The_Brand`（`BrandSeed == null`、`BrandColorIgnored == true`）/ `Null_Brand_Is_Not_Ignored`。
  - `Unknown_Language_Throws`（`ArgumentException`）、`DesignLanguages_Skips_Ungrouped_Themes`。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~ThemeSelectionResolverTests"`
- [ ] **Step 3: 实现**（纯函数，无状态）；在 `docs/guides/theming/customization.md` 增加"设计语言与品牌色"一节（属性、规则、解析器用法示例），`docs/releases/unreleased-theme-algorithms-api-changes.md` 与 `.zh-CN.md` 列出新增公开 API。
- [ ] **Step 4: 运行确认通过**（同上）
- [ ] **Step 5: 提交**

### Task 6: 各主题包声明

**Files:**
- Modify: `src/AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml`；`src/AtomUI.Themes.PrimeNG/Themes/*.theme.xml`（12）；`src/AtomUI.Themes.Bootstrap/Themes/*.theme.xml`（4）；`src/AtomUI.Themes.Material/Themes/*.theme.xml`（4）；`src/AtomUI.Themes.Shadcn/Themes/*.theme.xml`（12）；各包 `README.md` 增加一行能力说明
- Test: `tests/AtomUI.Themes.Tests/{PrimeNG,Bootstrap,Material,Shadcn}ThemePackageTests.cs`、`tests/AtomUI.Core.Tests/Theme/`（DaybreakBlue）

**Interfaces:**
- Consumes: Task 1–3（属性进入编译包）、Task 5（解析器）

每个 profile 写 `DesignLanguage`、`BrandColor`，非 Normal 的写 `Density="Compact"`；值见 Global Constraints。Bootstrap 的 4 个 XML 由另一会话在改，按其落地后的内容加属性。

- [ ] **Step 1: 写失败测试**
  - 每个包一个 `Profiles_Declare_Design_Language_And_Brand_Support`：遍历该包 4/12 个 Id，断言 `DesignLanguage`、`BrandColor`、`Density` 与 `Appearance` 组合，且 `manager.ThemeCatalogDiagnostics` 为空。
  - `PrimeNGThemePackageTests.Brand_Seed_Drives_Derived_States_In_Both_Appearances`（Review Focus 5）：对 `PrimeNGAura` 与 `PrimeNGAura.Dark` 以 `ThemeConfig` 写 `ColorPrimary=#123456` 应用 → `ColorPrimary`、`ColorPrimaryHover`、`ColorPrimaryBg`、`ColorControlOutline` 都不等于默认 emerald 的对应值，且暗色 `ColorPrimary` 与亮色不同（取 400 档）。
  - Core：`DaybreakBlue` 为 `AntDesign`/`Seed`/`Normal`/Light。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Themes.Tests`；`dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~DaybreakBlue"`
- [ ] **Step 3: 实现**（只改 XML 属性与 README）
- [ ] **Step 4: 运行确认通过**（同上），并跑 `dotnet test tests/AtomUIGallery.Tests` 确认无回归
- [ ] **Step 5: 提交**

### Task 7: GalleryBase 工作区：按设计语言切换与品牌记忆

**Files:**
- Modify: `src/AtomUI.Toolkits.GalleryBase/Shell/GalleryWorkspaceViewModel.cs`（删除 `ProfileVariantSuffixes`、`GetProfileBaseId`、`IsProfileFamily`、`FindProfileVariant`、`HasTheme` 及按后缀的分支）
- Test: `tests/AtomUIGallery.Tests/Toolkits/GalleryBaseFoundationTests.cs`

**Interfaces:**
- Consumes: Task 5 的解析器；Task 6 的声明
- Produces（公开，只增）：
  - `string? CurrentDesignLanguage { get; }`（当前主题的 `DesignLanguage`；`null` 表示"其他主题"）
  - `bool CanChooseBrandColor { get; }`（当前语言为 `Seed`）
  - `Color? BrandColor { get; }`（当前语言记忆的品牌色；`null` = 主题自身）
  - `ReactiveCommand<string, Unit> SwitchDesignLanguageCommand`
  - `ReactiveCommand<Color?, Unit> SetBrandColorCommand`（`Fixed` 时为 no-op；与主题 `AccentColor` 相同视为 `null`）
  - 现有 `SwitchThemeCommand` 保留，用于"其他主题"。

切换/外观/紧凑/动效变化统一走：有 `CurrentDesignLanguage` → `Resolve(AvailableThemes, new(lang, appearance, density, _brandByLanguage.GetValueOrDefault(lang)))` → `ThemeRequest(result.ThemeId, config(Algorithms 非 null 时指定；BrandSeed 非 null 时写 ColorPrimary；EnableMotion/EnableWaveSpirit), reason)`；无设计语言 → 现有"基础算法 + Compact/Dark"路径。显式选择某语言后外观模式跟随所得 profile 的规则沿用本日实现。

- [ ] **Step 1: 写失败测试**
  - `Workspace_Remembers_Brand_Color_Per_Design_Language`（Review Focus 2）：`AntDesign` 设 `#52C41A` → 切 `PrimeNG.Aura` 设 `#6366F1` → 切 `Shadcn.Zinc`（`CanChooseBrandColor == false`，根主题 `ColorPrimary == #18181B`）→ 切回 `AntDesign`，根 `ColorPrimary == #52C41A`；切回 Aura 为 `#6366F1`。
  - `Fixed_Language_Ignores_SetBrandColor`：Shadcn 上执行 `SetBrandColorCommand(#FF0000)` 后 `BrandColor == null`、`ColorPrimary` 不变。
  - `Toggles_Keep_The_Brand_Color`：Aura + 品牌色后切暗色与紧凑 → profile 为 `PrimeNGAura.DarkCompact`，`ColorPrimary` 由品牌色派生（≠ 默认）。
  - 改写现有 `Workspace_ViewModel_Switches_Theme_Id_And_Preserves_Orthogonal_Settings`（不再用 `PolarGreen`：改为 `AntDesign` + `SetBrandColorCommand(#52C41A)`，断言 Dark/Compact 与动效保持）及 `GalleryBaseFoundationTests.cs:492` 处用例。
  - 本日新增的三个 profile 切换测试保留并应继续通过。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUIGallery.Tests --filter "FullyQualifiedName~GalleryBaseFoundationTests"`
- [ ] **Step 3: 实现**（`_brandByLanguage` 为 `Dictionary<string, Color?>`，仅内存）；更新 `docs/modules/toolkits-gallery-base/theming-localization.md` 描述新命令与记忆。
- [ ] **Step 4: 运行确认通过** — 同上，再跑 `dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests`
- [ ] **Step 5: 提交**

### Task 8: AtomUIGallery：菜单、Ant 色块、删除四个预设

**Files:**
- Delete: `controlgallery/AtomUIGallery/Assets/Themes/{PolarGreen,SunsetOrange,GoldenPurple,Magenta}.theme.xml`；`controlgallery/AtomUIGallery/AtomUIGallery.csproj:13-16` 的四条 `AtomUIThemeDefinition`
- Modify: `controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml.cs`（菜单按 `DesignLanguage` 分组，去掉 `GetThemeDesignLanguage` 的 Id 前缀表）、`Workspace/Localization/WorkspaceWindowLang/*`（设计语言显示名与 "Brand color" / "此设计语言使用固定品牌色" 文案，四种语言）
- Test: `tests/AtomUIGallery.Tests/Workspace/GalleryBuiltInThemeTests.cs`、`tests/AtomUIGallery.Tests/Workspace/WorkspaceWindowLayoutTests.cs`

**Interfaces:**
- Consumes: Task 7 的视图模型成员
- Produces: 菜单结构 Theme ▸ Design language（单选）/ Brand color（色块 + Reset；`CanChooseBrandColor == false` 时禁用并显示提示）/ Appearance / Compact Mode / Other themes（`DesignLanguage == null` 的主题）。

色块：Tailwind 500 的 17 色（emerald、green、lime、orange、amber、yellow、teal、cyan、sky、blue、indigo、violet、purple、fuchsia、pink、rose、red 的 500 档）+ 当前语言基础定义的 `AccentColor`；`AntDesign` 额外显示 5 个命名色（Global Constraints）。显示名按 `DesignLanguage` 精确查本地化资源，查不到退回基础定义的 `Name`。

- [ ] **Step 1: 写失败测试**
  - `GalleryBuiltInThemeTests`：改为断言目录中**没有** `PolarGreen` 等四个 Id，`DaybreakBlue` 存在且为 `AntDesign`/`Seed`；新增 `Ant_Named_Swatches_Match_The_Removed_Presets`，断言 Ant 色块列表含五个命名色及其色值。
  - `WorkspaceWindowLayoutTests`：第 72-74 行改用 `SwitchDesignLanguageCommand("AntDesign")`；新增 `Theme_Menu_Groups_By_Design_Language`（菜单项与 `DesignLanguages(...)` 一致）与 `Brand_Swatches_Disabled_For_Fixed_Language`（选 Shadcn 后色块菜单 `IsEnabled == false`）。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUIGallery.Tests --filter "FullyQualifiedName~GalleryBuiltInThemeTests|FullyQualifiedName~WorkspaceWindowLayoutTests"`
- [ ] **Step 3: 实现**；全仓搜索 `PolarGreen|SunsetOrange|GoldenPurple` 确认 Gallery 内无残留主题引用（`Badge`/`Tooltip` 展示页里的 `Magenta` 是颜色名，不改）。
- [ ] **Step 4: 运行确认通过**，再跑 `dotnet test tests/AtomUIGallery.Tests`；按 `atomui-gallery-local-debug` 启动 Gallery 用 `atom:Window` 目视验收菜单与切换（无法绑定 UI 时如实报告）。
- [ ] **Step 5: 提交**

### Task 9: Theme Presets 页改用声明

**Files:**
- Modify: `controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/ViewModels/ThemePresetCatalog.cs`（删除 `FixesPrimaryStates`；`Export` 写出 `DesignLanguage`、`BrandColor`、`Density` 属性）、`ThemePresetsViewModel.cs`（`CanChoosePrimary` 改读 `ThemeInfo.BrandColor == Seed`；`IsProfile` 后缀判断改用 `DesignLanguage` 分组；预览配置改用 `ThemeSelectionResolver` 的结果）
- Test: `tests/AtomUIGallery.Tests/ThemePresetsShowCaseTests.cs`

**Interfaces:**
- Consumes: Task 1、Task 5

- [ ] **Step 1: 写失败测试**
  - `Primary_Choice_Follows_Declared_Brand_Support`：Aura `CanChoosePrimary == true`；Bootstrap、Material、Shadcn 为 `false`。
  - `Export_Writes_Design_Language_Metadata`：导出 `PrimeNGAura.DarkCompact` 的 XML 含 `DesignLanguage="PrimeNG.Aura"`、`BrandColor="Seed"`、`Density="Compact"`，且能被 `ThemeDocumentReader.Read` 无错读回。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUIGallery.Tests --filter "FullyQualifiedName~ThemePresets"`
- [ ] **Step 3: 实现**
- [ ] **Step 4: 运行确认通过**（同上），最后跑全量回归：`bash scripts/run-full-regression.sh`，以 `[SUMMARY]` 行为准。
- [ ] **Step 5: 提交**
