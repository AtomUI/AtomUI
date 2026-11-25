# Shadcn 对齐 shadcn/ui 现行标准 Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 把 `AtomUI.Themes.Shadcn` 重做为 shadcn/ui 现行的 8 种 Style × 7 种 Base Color × 18 种 Theme，弹层阴影与描边逐项对齐官方；Core 新增与品牌色对称的中性色轴。

**Architecture:** Core 新增可空 Seed `ColorNeutral` 与 `NeutralColor="Seed|Fixed"` 声明（ABI 3），解析器把用户选的中性色写进 `ColorNeutral`。Shadcn 包按 PrimeNG 包的分层实现：一张 Tailwind v4.3.5 色板 + 取档规则生成 Base Color 与 Theme 颜色，8 个 Style 的几何表，Map/Alias 算法与控件阶段；32 个 XML profile 只写算法链与默认种子。Gallery 增加中性色色块组，Shadcn 改用自己的 Theme 色块。

**Tech Stack:** .NET 10、Avalonia、AtomUI 主题系统（`IThemeAlgorithm`、`IControlTokenAlgorithm<T>`、XML v1 编译主题包）、xUnit + Shouldly、Python 3（附录生成脚本，已存在）。

**Spec:** `docs/superpowers/specs/2026-11-15-shadcn-latest-alignment-design.md`；取值附录 `docs/superpowers/specs/2026-11-15-shadcn-latest-alignment-data.md`（以下简称"附录"）。执行者两份都要读。

## Global Constraints

- 取值唯一来源是附录；Tailwind 版本 `tailwindcss@4.3.5`；`--radius` = 10px，`sm/md/lg/xl/2xl/3xl/4xl` = 6/8/10/14/18/22/26。
- Ant Design、PrimeNG、Bootstrap、Material 现有主题的任何 Token 值不得改变（每个改 Core 或控件的任务都要有对应断言）。
- 新增控件 Token 一律"默认值等于现状"，Ant 渲染不变（同 `f619ba31d` 的做法）。
- 公开 API 的英文 summary 遵循 `AtomUI.Tools/docs/reference/agent-docs` 规范；代码内 summary 必须英文。
- 提交信息走 `commit-msg` skill：单行、仓库现有中文风格、**不得带任何 AI/工具署名或 trailer**。
- `docs/superpowers/` 只放 Markdown；视觉验收截图不进仓库。
- 视觉验收的宿主必须是 `atom:Window`；容差：功能正确 + 关键区域对比，通道 ≤ 8，只看边缘。
- 诊断码：`ATMTHM3106`（Seed 中性色定义显式写了中性派生 Token，Error）、`ATMTHM3107`（`NeutralColor="Seed"` 但无 `ColorNeutral` 种子且算法链不提供缺省，Warning）、`ATMTHM3108`（同一设计语言内 `NeutralColor` 声明不一致，与 ATMTHM3104 同级处理）。

## Review Focus

1. 非官方主色种子（如 `#3B82F6`）在暗色 Shadcn 下：前景与主色对比度必须 ≥ 4.5，不能出现看不清的按钮文字。→ Task 6 测试 `CustomPrimary_Dark_Foreground_Is_Readable`。
2. 在 Shadcn 选了中性色后切到 Ant Design：Ant 不得收到 `ColorNeutral`（`NeutralColorIgnored`），渲染与未选时逐 Token 相同。→ Task 11 测试 `NeutralColor_Is_Not_Applied_To_AntDesign`。
3. 解析器回退路径（语言缺某个变体时把 `AtomUI:Compact` / `AtomUI:Dark` 追加到基础链）：`ColorNeutral` 仍要原样到达 Alias 与控件阶段，Style 尺寸不丢。→ Task 7 测试 `Neutral_Seed_Survives_Appended_Compact_And_Dark`。
4. 用户目录里的运行时 XML（非编译包）声明 `NeutralColor="Seed"`：读取器与生成器行为一致（含三条新诊断）。→ Task 2 测试 `Runtime_And_Compiled_Readers_Agree_On_NeutralColor`。
5. Maia 的 `shadow-2xl` 菜单与带箭头的 Popover：原生弹层窗口不裁切阴影，箭头弹层不出现双重描边。→ Task 13 视觉验收清单第 1、2 项。

---

## File Structure

| 文件 | 职责 |
|---|---|
| `src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.Seed.cs` | 新增 `Color? ColorNeutral` |
| `src/AtomUI.Core/Theme/ThemeDesignLanguageMetadata.cs` | 新增 `ThemeNeutralColorSupport { Fixed, Seed }` |
| `src/AtomUI.Core/Theme/ThemeInfo.cs` | `NeutralColor`、`NeutralAccentColor` init 属性，纳入相等性 |
| `src/AtomUI.Core/Theme/Definitions/ThemeDocument.cs`、`ThemeDocumentReader.cs` | 读 `NeutralColor` 属性；ATMTHM3106/3107 |
| `src/AtomUI.Core/Theme/Definitions/CompiledThemeCatalog.cs` | 填充 `ThemeInfo` 新属性；ATMTHM3108 |
| `src/AtomUI.Core/Theme/Definitions/Compiled/CompiledThemeProgram.cs` | ABI 3 构造函数 |
| `src/AtomUI.Generator/ThemeDefinitions/CompiledThemePackageWriter.cs` | 发出 ABI 3；生成期 ATMTHM3108 |
| `docs/reference/theming/schemas/atomui-theme-v1.xsd` | `NeutralColor` 属性 |
| `src/AtomUI.Core/Theme/ThemeSelection.cs` | `NeutralColor` 选择、`NeutralSeed` 结果、`NeutralColorSupport()` |
| `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnTailwindPalette.cs` | Tailwind v4.3.5 的 24 个色阶（7 中性 + 17 彩色，red 在其中），每个 11 档 hex |
| `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnSurface.cs` | `ShadcnSurface` 记录与 `ShadcnBaseColors.Resolve` |
| `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnThemeColor.cs` | 由 `ColorPrimary` 解析主色与前景 |
| `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnStyles.cs` | 8 个 Style 的几何与弹层表 |
| `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnStyleAlgorithm.cs` | 共享 Map/Alias 实现 |
| `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnStyleAlgorithms.cs` | 16 个 `[ThemeAlgorithm]` 注册（8 Style × 明暗） |
| `src/AtomUI.Themes.Shadcn/Algorithms/Controls/*.cs` | 控件阶段（弹层、输入、按钮、卡片、选择类等） |
| `src/AtomUI.Themes.Shadcn/Themes/Shadcn<Style>[.Dark|.Compact|.DarkCompact].theme.xml` | 32 个 profile（删除旧 12 个） |
| `src/AtomUI.Desktop.Controls/Tooltip/ToolTipToken.cs` 等 | 5.8 的控件 Token（Task 9） |
| `controlgallery/AtomUIGallery/Workspace/GalleryBrandSwatches.cs`、`GalleryNeutralSwatches.cs`（新） | 色块来源 |
| `src/AtomUI.Toolkits.GalleryBase/Shell/GalleryWorkspaceViewModel.cs` | 中性色记忆与命令 |
| `controlgallery/AtomUIGallery/Workspace/Views/GalleryThemeMenu.cs` | 中性色菜单组 |
| `controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/**` | Theme Presets 页色块与导出 |
| `tests/AtomUI.Themes.Tests/Shadcn/*.cs` | 包测试（替换 `ShadcnThemePackageTests.cs`） |

---

### Task 1: Core Seed Token `ColorNeutral`

**Files:**
- Modify: `src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.Seed.cs`
- Test: `tests/AtomUI.Core.Tests/Theme/ColorNeutralSeedTests.cs`（新）

**Interfaces:**
- Produces: `public partial Color? ColorNeutral { get; set; }`（Seed 段，与 `ColorTextBase` 同样可空、缺省 null）；生成的 `SharedTokenKind.ColorNeutral` 资源键。

- [ ] **Step 1: 写失败测试** `ColorNeutral_Is_A_Nullable_Seed_That_Changes_No_AntDesign_Token`：对 `DaybreakBlue` 分别以 `null` 配置和 `new ThemeConfigBuilder().WithToken("ColorNeutral", "#71717B").Build()` 应用，断言 `snapshot.Registry.TryGetGlobalToken("ColorNeutral", …)` 为 true，且除 `ColorNeutral` 外所有全局 Token 值逐个相等（亮、暗、紧凑三条链各跑一次）。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~ColorNeutralSeedTests"`，期望因 Token 不存在而 FAIL。
- [ ] **Step 3: 在 `DesignToken.Seed.cs` 加 `ColorNeutral`**，英文 summary 写明"neutral family representative color; ignored by the Ant Design calculators"。生成器会产出资源键，无需手写。
- [ ] **Step 4: 运行确认通过**，同一命令 PASS；再跑 `dotnet test tests/AtomUI.Theme.ContractTests` 全绿。
- [ ] **Step 5: 提交**（`feat(Theme): 新增可空 Seed Token ColorNeutral（中性色阶代表色），Ant Design 计算器忽略`）。

### Task 2: Core 中性色声明（XML、目录、诊断）

**Files:**
- Modify: `src/AtomUI.Core/Theme/ThemeDesignLanguageMetadata.cs`、`ThemeInfo.cs`、`Definitions/ThemeDocument.cs`、`Definitions/ThemeDocumentReader.cs:16,122-164,340-360`、`Definitions/CompiledThemeCatalog.cs:305-380,574-600`、`docs/reference/theming/schemas/atomui-theme-v1.xsd`
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeDocumentReaderTests.cs`、`CompiledThemeCatalogResolverTests.cs`

**Interfaces:**
- Produces:
  - `public enum ThemeNeutralColorSupport : byte { Fixed, Seed }`
  - `ThemeInfo.NeutralColor { get; init; }`（`ThemeNeutralColorSupport`）、`ThemeInfo.NeutralAccentColor { get; init; }`（`Color?`，定义自己 `<Tokens>` 里的 `ColorNeutral`）；两者加入 `Equals`/`GetHashCode`
  - `ThemeDocument.NeutralColor`（构造参数追加 `ThemeNeutralColorSupport neutralColor = ThemeNeutralColorSupport.Fixed`）
  - 中性派生 Token 集合（ATMTHM3106 用）：`ColorBgLayout, ColorBgContainer, ColorBgElevated, ColorBgSpotlight, ColorBgSolid, ColorBgSolidHover, ColorBgSolidActive, ColorText, ColorTextHeading, ColorTextSecondary, ColorTextTertiary, ColorTextQuaternary, ColorTextDescription, ColorTextPlaceholder, ColorTextDisabled, ColorIcon, ColorIconHover, ColorFill, ColorFillSecondary, ColorFillTertiary, ColorFillQuaternary, ColorFillContent, ColorBgContainerDisabled, ColorBorder, ColorBorderSecondary, ColorSplit, ControlItemBgHover, ControlItemBgActive, ControlItemColorActive`（与设计 5.4 一致；`ColorTextBase`、`ColorBgBase` 不在集合内）

- [ ] **Step 1: 写失败测试**（`ThemeDocumentReaderTests`）：
  - `NeutralColor_Attribute_Defaults_To_Fixed_And_Reads_Seed`：无属性 → `Fixed`；`NeutralColor="Seed"` → `Seed`。
  - `Seed_Neutral_Definition_Pinning_A_Neutral_Token_Reports_ATMTHM3106`：`NeutralColor="Seed"` 且 `<Token Name="ColorBorder" …/>`（分别放全局与 `<Controls>` 下）→ 各一条 Error `ATMTHM3106`。
  - `Seed_Neutral_Definition_Without_Seed_Reports_ATMTHM3107_Warning`：`NeutralColor="Seed"`、无 `ColorNeutral`、算法链为 `AtomUI:Default` → Warning `ATMTHM3107`。
  - `Runtime_And_Compiled_Readers_Agree_On_NeutralColor`（Review Focus 4）：同一 XML 经运行时目录与 Task 3 的编译路径得到的 `ThemeInfo.NeutralColor`、`NeutralAccentColor` 与诊断码集合相同（Task 3 完成后此测试才会通过，先标 `Skip` 并在 Task 3 Step 4 去掉）。
- [ ] **Step 2: 写失败测试**（`CompiledThemeCatalogResolverTests`）：`Mixed_NeutralColor_In_One_Language_Reports_ATMTHM3108`，第二个定义被降级出分组，行为对照现有 ATMTHM3104 用例。
- [ ] **Step 3: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~ThemeDocumentReaderTests|FullyQualifiedName~CompiledThemeCatalogResolverTests"`。
- [ ] **Step 4: 实现**：枚举、`ThemeInfo` 属性、XML 读取（`reader.GetAttribute("NeutralColor") == "Seed"`）、两个 Reader 诊断（照 `ValidateSeedBrandTokens` 的写法）、目录填充（`NeutralAccentColor` 照第 574 行 `accentColor` 取 `ColorNeutral`）、ATMTHM3108（照第 343 行分支）、XSD 增加可选属性 `NeutralColor`（枚举 `Seed|Fixed`）。
- [ ] **Step 5: 运行确认通过**（Skip 的那条除外），并跑 `dotnet test tests/AtomUI.Core.Tests` 全绿。
- [ ] **Step 6: 提交**（`feat(Theme): 主题定义新增 NeutralColor 声明与 NeutralAccentColor 元数据，及 ATMTHM3106/3107/3108 诊断`）。

### Task 3: 编译主题 ABI 3 与生成器

**Files:**
- Modify: `src/AtomUI.Core/Theme/Definitions/Compiled/CompiledThemeProgram.cs`、`src/AtomUI.Generator/ThemeDefinitions/CompiledThemePackageWriter.cs:40-63,94,131-139`、`src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`、`src/AtomUI.Generator/Diagnostics/CompiledThemeDiagnosticDescriptors.cs`（生成期 ATMTHM3108，照 ATMTHM3104 的声明）
- Test: `tests/AtomUI.Generator.Tests/ThemeDefinitions/CompiledThemeMetadataTests.cs`、`tests/AtomUI.Core.Tests/Theme/ThemeDocumentReaderTests.cs`

**Interfaces:**
- Consumes: Task 2 的 `ThemeNeutralColorSupport`、`ThemeDocument.NeutralColor`。
- Produces: `protected CompiledThemeProgram(int abiVersion /*=3*/, …ABI 2 参数…, ThemeBrandColorSupport brandColor, ThemeNeutralColorSupport neutralColor, Color? neutralAccentColor, string[] algorithms)`；生成器对所有定义发出 `base(3, …)`。ABI 1、2 构造函数保留，新属性取 `Fixed` / `null`。

- [ ] **Step 1: 写失败测试**：
  - `Abi3_Program_Publishes_NeutralColor_And_NeutralAccentColor`（生成器测试）：编译 `NeutralColor="Seed"` 且 `ColorNeutral=#71717B` 的 XML，加载后 `Info.NeutralColor == Seed`、`Info.NeutralAccentColor == #71717B`。
  - `Abi2_Program_Keeps_Loading_With_Neutral_Defaults`：手写 ABI 2 子类实例，`NeutralColor == Fixed`、`NeutralAccentColor == null`。
  - `Generator_Reports_ATMTHM3108_For_Mixed_NeutralColor`：同包同语言两个定义声明不同 → 生成器 Error（照第 45 行 ATMTHM3104）。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Generator.Tests --filter "FullyQualifiedName~CompiledThemeMetadataTests"`。
- [ ] **Step 3: 实现** ABI 3 构造函数（ABI 版本不为 3 时抛 `NotSupportedException`，校验 `Enum.IsDefined(neutralColor)`）；生成器把 `neutralColor`、`neutralAccent`（与 `accent` 同法从全局 `ColorNeutral` 字面量取）加入 `fields` 摘要与构造参数。
- [ ] **Step 4: 去掉 Task 2 的 Skip**，运行 `dotnet test tests/AtomUI.Generator.Tests tests/AtomUI.Core.Tests tests/AtomUI.Themes.Tests` 全绿（现有四个预设包重新生成为 ABI 3 后测试不变）。
- [ ] **Step 5: 提交**（`feat(Generator): 编译主题 ABI 3 发布 NeutralColor 与 NeutralAccentColor，ABI 1/2 照常加载`）。

### Task 4: 解析器中性色

**Files:**
- Modify: `src/AtomUI.Core/Theme/ThemeSelection.cs`
- Test: `tests/AtomUI.Core.Tests/Theme/ThemeSelectionResolverTests.cs`

**Interfaces:**
- Produces:
  - `ThemeSelection(string DesignLanguage, ThemeAppearance Appearance, ThemeDensity Density, Color? BrandColor, Color? NeutralColor = null)`
  - `ThemeSelectionResult(string ThemeId, IReadOnlyList<ThemeAlgorithm>? Algorithms, Color? BrandSeed, bool BrandColorIgnored, Color? NeutralSeed = null, bool NeutralColorIgnored = false)`
  - `public static ThemeNeutralColorSupport ThemeSelectionResolver.NeutralColorSupport(IReadOnlyList<ThemeInfo> catalog, string designLanguage)`（对称于第 178 行 `BrandColorSupport`）

- [ ] **Step 1: 写失败测试**：`Seed_Language_Passes_Neutral_Seed`、`Fixed_Language_Ignores_Neutral_Color`（`NeutralSeed == null && NeutralColorIgnored`）、`No_Neutral_Color_Leaves_Seed_Null`、`NeutralColorSupport_Reports_First_Definition`。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Core.Tests --filter "FullyQualifiedName~ThemeSelectionResolverTests"`。
- [ ] **Step 3: 实现**，判定依据为该语言第一个定义的 `NeutralColor`（同品牌色第 142 行）。
- [ ] **Step 4: 运行确认通过**，并跑 `dotnet test tests/AtomUI.Core.Tests`。
- [ ] **Step 5: 提交**（`feat(Theme): ThemeSelectionResolver 支持中性色种子，Fixed 语言忽略并置标志`）。

### Task 5: Shadcn 色板与 Base Color 生成

**Files:**
- Create: `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnTailwindPalette.cs`、`ShadcnSurface.cs`
- Create: `tests/AtomUI.Themes.Tests/Shadcn/ShadcnAppendix.cs`（解析附录 Markdown 的测试辅助）、`ShadcnBaseColorTests.cs`
- Modify: `src/AtomUI.Themes.Shadcn/AtomUI.Themes.Shadcn.csproj`（加 `InternalsVisibleTo AtomUI.Themes.Tests`，同 PrimeNG）

**Interfaces:**
- Produces:
  - `internal static class ShadcnTailwindPalette { IReadOnlyDictionary<string, Color[]> Families; Color Stop(string family, int stop); IReadOnlyList<string> NeutralFamilies /* neutral, stone, zinc, mauve, olive, mist, taupe */; IReadOnlyList<string> ThemeFamilies /* 17，附录 B 顺序 */ }`，`Color[]` 下标 0..10 对应 50,100,200…900,950。
  - `internal sealed record ShadcnSurface(Color Background, Color Foreground, Color Card, Color Popover, Color Primary, Color PrimaryForeground, Color Secondary, Color SecondaryForeground, Color Muted, Color MutedForeground, Color Accent, Color AccentForeground, Color Destructive, Color Border, Color Input, Color Ring)`
  - `internal static class ShadcnBaseColors { ShadcnSurface Resolve(Color? neutralSeed, bool dark); string? FamilyOf(Color neutralSeed); Color DefaultSeed /* #737373 */ }`
- 取档规则：照附录 A 开头一段（浅色 950/900/100/500/200/400 + 白；暗色 950/900/200/800/50/400/500 + 白 10%/15%；destructive red-600 / red-400）。
- 自定义种子：把种子视为 500 级，在 OKLCH 中生成 11 档（明度取 neutral 各档，色相取种子，色度 = Zinc 各档色度 × 种子色度 / 0.016），再按同一规则取值。OKLCH↔sRGB 换算放在 `ShadcnSurface.cs` 内的私有静态方法（与附录脚本 `oklch_hex` 同公式）。

- [ ] **Step 1: 写测试辅助** `ShadcnAppendix`：`BaseColors()` 从仓库根（向上找 `.git`）读取附录，解析 A.1/A.2 表格每格反引号内的 hex，返回 `(family, dark, variable) → Color`；`Themes()` 同法解析附录 B，返回 `(theme, dark) → (primary, foreground)`（Task 6 使用）。
- [ ] **Step 2: 写失败测试**：
  - `Official_Seed_Generates_Appendix_Values`（`[Theory]` 7 家族 × 明暗）：`ShadcnBaseColors.Resolve(seed, dark)` 的 16 个字段逐个等于附录。
  - `Palette_Five_Hundred_Stops_Are_The_Seeds`：7 个家族的 500 级等于附录 A.1 `--muted-foreground` 一行。
  - `Custom_Seed_Is_Not_Mistaken_For_An_Official_Family`：七个官方种子 `FamilyOf` 各自命中；`#6B7280` 返回 null。
  - `Custom_Seed_Dark_Surface_Has_Light_Text`：`Resolve(#6B7280, dark:true)` 的 `Foreground` 与 `Background` 对比度 ≥ 7。
- [ ] **Step 3: 运行确认失败** — `dotnet test tests/AtomUI.Themes.Tests --filter "FullyQualifiedName~ShadcnBaseColorTests"`。
- [ ] **Step 4: 实现** 色板（hex 由附录脚本同样的换算从 `tailwindcss@4.3.5/theme.css` 得出；在 gen.py 旁临时加一段输出 C# 数组即可，脚本改动不必提交）与 `ShadcnBaseColors`。
- [ ] **Step 5: 运行确认通过。**
- [ ] **Step 6: 提交**（`feat(Shadcn): 新增 Tailwind v4.3.5 色板与 Base Color 取档规则（含自定义中性色派生）`）。

### Task 6: Theme 主色解析

**Files:**
- Create: `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnThemeColor.cs`、`tests/AtomUI.Themes.Tests/Shadcn/ShadcnThemeColorTests.cs`

**Interfaces:**
- Consumes: Task 5 的 `ShadcnTailwindPalette`、`ShadcnSurface`。
- Produces: `internal readonly record struct ShadcnPrimary(Color Primary, Color Foreground, string? Theme /* null = 自定义, "neutral" = 跟随 Base Color */)`；`internal static class ShadcnThemeColor { ShadcnPrimary Resolve(Color seed, ShadcnSurface surface, bool dark); }`
- 规则（设计 5.3.2）：种子 = 任一中性家族 900 级 → `neutral`，取 `surface.Primary/PrimaryForeground`；种子 = 彩色家族 700 级（lime、yellow 为 400 级）→ 浅色 700/50、暗色 800/50（lime、yellow：400/900、500/900）；其余为自定义：浅色主色 = 种子；暗色主色在 OKLCH 中明度减 Δ、色度乘 k，Δ 与 k 取 17 个官方 Theme 浅→暗的平均值（在实现里由色板计算，不手写常数）；前景在"同色相 L=0.97 C=0.015"与"同色相 L=0.42 C 同主色×0.45"中取与主色对比度 ≥ 4.5 者，优先前者。

- [ ] **Step 1: 写失败测试**：
  - `Official_Themes_Match_Appendix`（`[Theory]` 17 × 明暗）：从附录 B 解析期望值。
  - `Neutral_Seed_Follows_Base_Color`：`Resolve(#18181B /*zinc-900*/, zincSurface, dark)` 等于 `zincSurface.Primary`。
  - `CustomPrimary_Dark_Foreground_Is_Readable`（Review Focus 1）：`#3B82F6`、`#F59E0B`、`#10B981` 在明暗下对比度 ≥ 4.5。
- [ ] **Step 2: 运行确认失败** — `--filter "FullyQualifiedName~ShadcnThemeColorTests"`。
- [ ] **Step 3: 实现。**
- [ ] **Step 4: 运行确认通过。**
- [ ] **Step 5: 提交**（`feat(Shadcn): Theme 主色按 Tailwind 700/800/50 级（Lime、Yellow 为 400/500/900 级）解析，自定义主色派生暗色与前景`）。

### Task 7: Style 表、Map/Alias 算法与 Nova 四个 profile

**Files:**
- Create: `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnStyles.cs`、`ShadcnStyleAlgorithm.cs`、`ShadcnStyleAlgorithms.cs`
- Create: `src/AtomUI.Themes.Shadcn/Themes/ShadcnNova.theme.xml`、`ShadcnNova.Dark.theme.xml`、`ShadcnNova.Compact.theme.xml`、`ShadcnNova.DarkCompact.theme.xml`
- Delete: `src/AtomUI.Themes.Shadcn/Themes/Shadcn{Zinc,Slate,Stone}*.theme.xml`（12 个）、`tests/AtomUI.Themes.Tests/ShadcnThemePackageTests.cs`
- Modify: `src/AtomUI.Themes.Shadcn/ShadcnThemes.cs`（同时调用 `GeneratedThemeAlgorithmRegistration.Register` 与 `GeneratedThemePackageRegistration.Register`，照 `PrimeNGThemes.cs`）
- Create: `tests/AtomUI.Themes.Tests/Shadcn/ShadcnPackageTests.cs`

**Interfaces:**
- Consumes: Task 1–6。
- Produces:
  - `internal sealed record ShadcnStyle(string Id, double ControlHeight, double ControlHeightSM, double ControlHeightLG, double ControlHeightXS, double ButtonRadius, double ButtonRadiusSM, double InputRadius, double PaddingX, double PaddingXSM, double PaddingXLG, double FontSize, double FontSizeSM, FontWeight ButtonWeight, double FocusRingWidth, byte FocusRingAlpha, double CardRadius, double CardPadding, string CardShadow, byte CardRingAlpha, ShadcnOverlay Overlay)`；`internal sealed record ShadcnOverlay(string MenuShadow, double MenuRadius, double MenuPadding, string PopoverShadow, double PopoverRadius, double PopoverPadding, byte RingAlphaLight, byte RingAlphaDark, double TooltipRadius, double TooltipArrowRadius, string DialogShadow, double DialogRadius, double DialogPadding, string DrawerShadow, byte MaskAlpha)`。阴影字段为级别名（`none/xs/sm/md/lg/xl/2xl`），`ShadcnStyles.Shadow(string level)` 返回设计 5.6.1 表的 `BoxShadows` 字面量。
  - `ShadcnStyles.All`（8 项，取值照设计 5.5、5.6.1 两张表与附录 C）；`ShadcnStyles.Resolve(in ThemeAlgorithmContext context)`（照 `PrimeNGPresets.Resolve`，按算法链中的 `Shadcn:<Style>` 或 `Shadcn:<Style>.Dark` 识别）。
  - 算法 Id：`Shadcn:Vega`、`Shadcn:Vega.Dark` … `Shadcn:Rhea.Dark`（16 个）。
  - 计算后的 `ColorNeutral` 在 `nextMap` 中保留（为 null 时写 `ShadcnBaseColors.DefaultSeed`）。
- XML（以 Nova 浅色为例）：`Id="ShadcnNova" Name="Shadcn Nova" Appearance="Light" DesignLanguage="Shadcn.Nova" BrandColor="Seed" NeutralColor="Seed"`，算法 `<Algorithm Catalog="Shadcn" Id="Nova" />`，Token 只有 `ColorPrimary=#171717`、`ColorNeutral=#737373`；Dark 用 `Nova.Dark`；Compact 追加 `<Algorithm Catalog="AtomUI" Id="Compact" />`、`Density="Compact"`。

- [ ] **Step 1: 写失败测试**（`ShadcnPackageTests`，先只覆盖 Nova）：
  - `Nova_Profiles_Declare_Language_And_Seeds`：4 个 profile 的 `DesignLanguage`、`BrandColor=Seed`、`NeutralColor=Seed`、`Density`、`AccentColor=#171717`、`NeutralAccentColor=#737373`；`ThemeCatalogDiagnostics` 为空。
  - `Nova_Colors_Follow_Base_Color`（`[Theory]` 7 家族 × 明暗）：以 `ColorNeutral=<family 500>` 应用，断言设计 5.4 表的每个 Token 等于附录 A 对应变量（含 `ColorBgElevated=--popover`、`ColorBorder=--border`，暗色为 `#1AFFFFFF`）；并断言整链解析后的全局 `ColorNeutral` 仍等于所填种子（设计 5.3.0 第 4 条：后续阶段反查的依据）。
  - `Nova_Geometry`：`ControlHeight=32`、`ControlHeightSM=28`、`ControlHeightLG=36`、`BorderRadius=10`、`FontSize=14`。
  - `Dark_Neutral_Tokens_Are_Light_On_Dark`：每个家族暗色下，Task 2 中性派生集合里的每个文字/图标 Token 与 `ColorBgContainer` 对比度 ≥ 3，且亮度高于背景。
  - `Neutral_Seed_Survives_Appended_Compact_And_Dark`（Review Focus 3）：请求 `ShadcnNova` + `WithAlgorithms(Shadcn:Nova, AtomUI:Compact, AtomUI:Dark)` + `ColorNeutral=#79716B`，Alias 阶段得到的 `ColorControlOutline` 等于 stone 暗色 ring 按 Nova 透明度（50%）合成的值，`ControlHeight` 为 Compact 后的值。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Themes.Tests --filter "FullyQualifiedName~Shadcn"`。
- [ ] **Step 3: 实现** `ShadcnStyles`、`ShadcnStyleAlgorithm`（Map：解析种子 → 用副本种子把 `ColorTextBase`/`ColorBgBase` 设为当前外观的 foreground/background 后调用 `DefaultThemeVariantCalculator` 或 `DarkThemeVariantCalculator` → 写设计 5.4 全表、尺寸与 `ColorNeutral`；Alias：`ColorControlOutline`、`ColorPrimaryHover/Active/Bg/BgHover/Border/BorderHover/Text*`（设计 5.3.3）、`BoxShadows`、`BoxShadowsSecondary`、`ColorBgMask`），删除旧 XML 与旧测试，新建 Nova 四个 XML。
- [ ] **Step 4: 运行确认通过**；`dotnet build src/AtomUI.Themes.Shadcn` 无诊断。
- [ ] **Step 5: 提交**（`feat(Shadcn): 按 shadcn/ui 现行标准重做 Shadcn 主题包骨架：Style 算法、Base Color 与 Theme 解析、Shadcn Nova 四个 profile，删除旧 Zinc/Slate/Stone`）。

### Task 8: Nova 控件阶段（弹层优先）

**Files:**
- Create: `src/AtomUI.Themes.Shadcn/Algorithms/Controls/ShadcnOverlayTokens.cs`（`Popup`、`FlyoutHost`、`ArrowDecoratedBox`）、`ShadcnDialogTokens.cs`、`ShadcnDrawerTokens.cs`、`ShadcnToolTipTokens.cs`、`ShadcnInputTokens.cs`（LineEdit、TextArea、Select、Cascader、DatePicker、TimePicker、AutoComplete、Mentions 等输入族的控件级全局覆盖）、`ShadcnButtonTokens.cs`、`ShadcnCardTokens.cs`、`ShadcnSelectionTokens.cs`（CheckBox、RadioButton、ToggleSwitch、Select 选项、菜单项）
- Create: `tests/AtomUI.Themes.Tests/Shadcn/ShadcnOverlayTests.cs`、`ShadcnControlTests.cs`

**Interfaces:**
- Consumes: Task 7 的 `ShadcnStyles.Resolve(context)`、`ShadcnBaseColors.FamilyOf`、`ShadcnBaseColors.Resolve`（控件阶段从 `effectiveGlobal.ColorNeutral` 反查 surface）。
- 每个类声明 16 个 `[ControlTokenAlgorithm("Shadcn", "<Style>[.Dark]", 1)]`（照 `PrimeNGButtonTokens`），也可用一个共享基类集中声明。
- 取值：
  - 环色 = `--foreground` 按 `RingAlphaLight/Dark` 的 alpha（例：Neutral 浅色 Nova `#1A0A0A0A`、暗色 `#1AFAFAFA`），**写在阴影之前**。
  - `PopupToken.PopupRootShadow` / `OverlayHostShadow` = 环 + `MenuShadow`。
  - `FlyoutHostToken.PopupRootShadow` / `OverlayHostShadow` = 仅 `PopoverShadow`；`ArrowDecoratedBoxToken.ContentBorderColor` = 环色、`ContentBorderThickness` = 1。
  - `Controls[Dialog]` 的 `BoxShadows` = 环 + `DialogShadow`（Nova 为 `none`，即只有环）。
  - Drawer 四向阴影 = `DrawerShadow`。
  - Tooltip：`ToolTipBackground = --foreground`、`ToolTipColor = --background`、`ToolTipCornerRadius = TooltipRadius`、`ContentPadding = 12,6`。
  - 输入族：控件级 `ColorPrimary`/`ColorPrimaryHover` = `--ring`（聚焦边框），`ColorBorder = --input`，`ColorBgContainer` 按设计 5.5 输入底色表。
  - Button：`FontWeight` 按 Style；默认主按钮悬停 = 主色 80% 合成（已在 Alias）。
  - Card：`ColorBgContainer = --card`、圆角与内边距、`CardShadows` = 环 + `CardShadow`。
  - Message / Notification：`Controls[Message]`、`Controls[Notification]` 的 `BoxShadows`。官方 Toast 由 sonner 绘制，registry 中没有 class：先在 `https://ui.shadcn.com/docs/components/base/sonner` 打开一个 Toast，用浏览器读出其计算后的 `box-shadow`、`border` 与圆角，记入 README 后照写；测试 `Toast_Matches_Measured_Sonner` 断言该实测值。
  - 输入族的聚焦色用"控件级全局 Token 覆盖"实现（`EvaluateGlobal` 写 `controlGlobal`，同 `PrimeNGButtonTokens` 写 `ColorLink`），不需要改控件；若某个输入控件不读取被覆盖的 Token，记录下来并在 Task 9 补一个控件 Token（默认等于现状）。

- [ ] **Step 1: 写失败测试**（`ShadcnOverlayTests`，Nova × 明暗 × Neutral 与 Zinc）：`Menu_Popup_Shadow_Is_Ring_Then_Md`、`Arrow_Flyout_Has_Integrated_Border_And_Plain_Shadow`、`Dialog_Has_Ring_Only`、`Drawer_Shadow_Is_Lg`、`Mask_Is_Black_10`、`Tooltip_Is_Inverted`，期望值全部取自设计 5.6.1 表与附录 A。
- [ ] **Step 2: 写失败测试**（`ShadcnControlTests`）：`Input_Focus_Border_Is_Ring`、`Input_Border_Is_Input_Color_In_Dark`（`#26FFFFFF`）、`Card_Uses_Card_Surface_And_Ring`、`Button_Weight_Is_Medium`。
- [ ] **Step 3: 运行确认失败。**
- [ ] **Step 4: 实现。**（Tooltip 阴影与 Drawer 边框依赖 Task 9，这两个断言先标 `Skip`，Task 9 去掉）
- [ ] **Step 5: 运行确认通过。**
- [ ] **Step 6: 提交**（`feat(Shadcn): Nova 控件阶段：弹层描边环与阴影、带箭头弹层一体描边、Dialog 仅描边环、Tooltip 反色、输入族 ring 聚焦与 input 边框`）。

### Task 9: 控件改动：Tooltip 阴影、Drawer 贴边边框

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/Tooltip/ToolTipToken.cs` 及 Tooltip 弹层读取阴影处；`src/AtomUI.Desktop.Controls/Drawer/DrawerToken.cs` 及 Drawer 主题 AXAML
- Modify（条件项）: Task 8 记录的"不读取被覆盖 Token"的输入类控件，为其补控件 Token（默认值等于现状）
- Test: `tests/AtomUI.Desktop.Controls.Tests`（Tooltip、Drawer 现有测试目录）、`tests/AtomUI.Themes.Tests/Shadcn/ShadcnOverlayTests.cs`
- Docs: `docs/controls/desktop/**/tooltip/token.md`、`changelog.md`，Drawer 同（按 `atomui-control-documentation`）

**Interfaces:**
- Produces: `ToolTipToken.ToolTipShadows`（`BoxShadows`，默认等于 Tooltip 目前实际绘制的弹层阴影）；`DrawerToken.EdgeBorderColor`（`Color`，默认 `Colors.Transparent`）与 `DrawerToken.EdgeBorderThickness`（`double`，默认 0），只在贴边一侧绘制。

- [ ] **Step 1: 确认现状**：找出 Tooltip 弹层阴影的来源（`ToolTip.cs` 的 `IShadowMaskInfoProvider` 与其 Popup），写一条断言当前 Ant 主题下 Tooltip 阴影值的测试 `Tooltip_Shadow_Is_Unchanged_Under_AntDesign` 并让它在改动前通过（记录现值）。
- [ ] **Step 2: 写失败测试**：`ToolTipShadows_Drives_The_Tooltip_Popup_Shadow`（设为空后弹层 `FrameShadow` 为空）；`Drawer_Edge_Border_Draws_Only_On_The_Anchored_Side`（左抽屉只有右边框）；`Drawer_Defaults_Draw_No_Edge_Border`。
- [ ] **Step 3: 运行确认失败。**
- [ ] **Step 4: 实现** Token 与消费；更新控件 token 文档与 changelog（中英文）。
- [ ] **Step 5: 运行** 控件测试、Step 1 的不变测试、去掉 Task 8 的两条 Skip 后的 Shadcn 测试，全部 PASS。
- [ ] **Step 6: 提交**（`feat(Controls): Tooltip 新增 ToolTipShadows、Drawer 新增贴边一侧边框 Token，默认值保持 Ant Design 渲染不变`）。

### Task 10: 其余 7 种 Style

**Files:**
- Create: `src/AtomUI.Themes.Shadcn/Themes/Shadcn{Vega,Maia,Lyra,Mira,Luma,Sera,Rhea}[.Dark|.Compact|.DarkCompact].theme.xml`（28 个）
- Modify: `ShadcnStyles.cs`（补齐 7 项）、控件阶段中按 Style 取值的分支
- Test: `ShadcnPackageTests`、`ShadcnOverlayTests`、`ShadcnControlTests` 改为 `[Theory]` 覆盖 8 个 Style

**Interfaces:**
- Consumes: Task 7–9 的全部接口，不新增。

- [ ] **Step 1: 把 Task 7、8 的测试参数化到 8 个 Style**，期望值逐行取自设计 5.5（几何）与 5.6.1（弹层）两张表；新增：`Maia_Menu_Shadow_Is_2xl_With_Ring_5`、`Luma_Dialog_Has_Xl_Shadow`、`Sera_Button_Is_Semibold_12`、`Lyra_Radius_Is_Zero_Everywhere`、`Mira_Focus_Ring_Is_2px_30`。
- [ ] **Step 2: 运行确认失败**（7 个 Style 的 profile 不存在）。
- [ ] **Step 3: 新建 28 个 XML，补齐 `ShadcnStyles`**，对照附录 C 逐组件核对。
- [ ] **Step 4: 运行确认通过**，`ThemeCatalogDiagnostics` 为空。
- [ ] **Step 5: 提交**（`feat(Shadcn): 补齐 Vega、Maia、Lyra、Mira、Luma、Sera、Rhea 七种 Style 的 profile 与取值`）。

### Task 11: Gallery

**Files:**
- Modify: `controlgallery/AtomUIGallery/Workspace/GalleryBrandSwatches.cs`；Create: `controlgallery/AtomUIGallery/Workspace/GalleryNeutralSwatches.cs`
- Modify: `src/AtomUI.Toolkits.GalleryBase/Shell/GalleryWorkspaceViewModel.cs`、`controlgallery/AtomUIGallery/Workspace/Views/GalleryThemeMenu.cs`、`controlgallery/AtomUIGallery/Workspace/Localization/WorkspaceWindowLang/*`、`controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/ViewModels/ThemePresetsViewModel.cs`、`Views/ThemePresetsShowCase.axaml`、其本地化
- Test: `tests/AtomUIGallery.Tests/ThemePresetsShowCaseTests.cs`、`Workspace/WorkspaceWindowLayoutTests.cs`、`Toolkits/GalleryBaseFoundationTests.cs`、`tests/AtomUI.Toolkits.GalleryBase.Tests/Shell/*`

**Interfaces:**
- Produces:
  - `GalleryBrandSwatches.ShadcnLanguagePrefix = "Shadcn."`；`GalleryBrandSwatches.Shadcn`：`Neutral` 不入表（由 `baseColor` 提供），其后 17 个 Theme，颜色为附录 B 浅色主色（700 级，Lime/Yellow 400 级），名称为英文 Theme 名；`For()` 对 Shadcn 语言返回"语言自身主色 + 17 个"。
  - `internal sealed record GalleryNeutralSwatch(string Name, Color Color)`；`GalleryNeutralSwatches.For(string designLanguage, Color? baseColor)`：Shadcn 语言返回 7 个（附录 A.1 `--muted-foreground`，顺序 Neutral、Stone、Zinc、Mauve、Olive、Mist、Taupe），其他语言返回空。
  - `GalleryWorkspaceViewModel`：`bool CanChooseNeutralColor`、`Color? NeutralColor`、`ReactiveCommand<Color?, Unit> SetNeutralColorCommand`；内部 `_neutralByLanguage`，与 `_brandByLanguage` 同法；`CreateSelection` 传入 `NeutralColor`；应用时 `NeutralSeed` 写 `WithToken(nameof(SharedTokenKind.ColorNeutral), …)`；等于 `NeutralAccentColor` 视为未选。

- [ ] **Step 1: 写失败测试**：
  - GalleryBase：`Neutral_Color_Is_Remembered_Per_Language`、`NeutralColor_Is_Not_Applied_To_AntDesign`（Review Focus 2：Shadcn 选 Zinc 后切 AntDesign，Ant 快照与未选时逐 Token 相同）、`Choosing_The_Definition_Neutral_Clears_The_Choice`。
  - Gallery：`Theme_Menu_Lists_Eight_Shadcn_Languages`、`Shadcn_Brand_Swatches_Are_Official_Themes`、`Neutral_Group_Is_Disabled_For_Fixed_Languages`、`Theme_Presets_Shadcn_Offers_Neutral_Row_And_Exports_Seeds`（导出 XML 含 `NeutralColor="Seed"` 与所选 `ColorPrimary`、`ColorNeutral`）。
  - 现有三个引用 `ShadcnZinc` 等旧 ID 的测试改用 `ShadcnNova`。
- [ ] **Step 2: 运行确认失败** — `dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests tests/AtomUIGallery.Tests`。
- [ ] **Step 3: 实现**，`GalleryThemeMenu.cs:37-39` 的语言名映射改为 8 个 Style；本地化新增 8 个语言名、7 个中性色名、17 个 Theme 名与"中性色"组标题（中英文，其余语言按现有回退）。
- [ ] **Step 4: 运行确认通过。**
- [ ] **Step 5: 提交**（`feat(Gallery): 主题菜单新增中性色组并按设计语言记忆，Shadcn 改用官方 Theme 色块，Theme Presets 页支持中性色与导出种子`）。

### Task 12: 文档

**Files:**
- Modify: `src/AtomUI.Themes.Shadcn/README.md`（重写）、`docs/guides/theming/preset-theme-packages.md`、`docs/modules/themes/overview.md`、`docs/guides/theming/customization.md`（如提及 Shadcn）、XML 参考（`NeutralColor` 属性与 ATMTHM3106–3108）、`docs/modules/toolkits-gallery-base/theming-localization.md`、`docs/releases/unreleased-theme-algorithms-api-changes.md` 与 `.zh-CN.md`

- [ ] **Step 1: README**：维度表、颜色来自 Tailwind v4.3.5 的说明、取档规则、设计 5.4 映射表、设计 5.7 不支持清单（Sera 下划线输入注明可由应用使用 AtomUI 已有的 `Underlined` 变体近似）、设计 5.3.3 与 6.2、6.3 中的近似取舍，链接附录。
- [ ] **Step 2: 其余文档**：新 ID、`ColorNeutral` Seed、`NeutralColor` 声明、ABI 3、解析器新参数、三条诊断；变更说明中记录 Shadcn ID 变化。按合入时的实际发布状态写：`AtomUI.Themes.Shadcn` 尚未发布则只记录 ID 变化；已发布过旧 ID 则补迁移说明（旧 ID → `ShadcnVega` + 对应中性色，Slate 改用 Mist 或自定义中性色）。
- [ ] **Step 3: 检查** `grep -rn "ShadcnZinc\|ShadcnSlate\|ShadcnStone\|Shadcn.Zinc" docs src controlgallery tests` 仅剩历史设计/计划文档（`tests/AtomUI.Desktop.Controls.Rendering.Tests/Temporary/` 下的临时视觉测试如仍引用旧 ID，按测试生命周期改用新 ID 或删除）。
- [ ] **Step 4: 提交**（`docs(Theme): 记录中性色轴与 ABI 3，重写 Shadcn 主题包 README 与预设文档`）。

### Task 13: 视觉验收与全量回归

**Files:** 无代码；验收记录写入 `docs/superpowers/progress/2026-11-15-shadcn-latest-alignment.md`（只写文字结论，截图不入库）。

- [ ] **Step 1: 视觉验收**（按 `atomui-visual-acceptance` skill，`atom:Window` 宿主），每种 Style 亮暗各一次，与 `https://ui.shadcn.com/create` 同 Style、同 Base Color 的对应组件对比：
  1. Maia 菜单 `shadow-2xl` 完整、无裁切（Review Focus 5）；
  2. 带箭头 Popover / PopupConfirm：框体与箭头一体描边，无双线、箭头底边无横线（Review Focus 5）；
  3. Select / 菜单面板描边环在暗色下可见且与官方一致；
  4. Dialog：Nova 等无阴影仅描边环，Luma / Rhea 为 xl；
  5. Tooltip 反色、箭头圆角；
  6. 按钮与输入高度、圆角、聚焦边框色（每 Style 一组）。
- [ ] **Step 2: 全量回归**（`full-regression` skill），以脚本最终 `[SUMMARY]` 行为准。
- [ ] **Step 3: 写验收记录并提交**（`docs(Shadcn): 记录 shadcn/ui 现行标准对齐的视觉验收与回归结果`）。
