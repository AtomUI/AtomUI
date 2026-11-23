# 删除 Bootstrap / Material 预设主题包 Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 彻底删除 `AtomUI.Themes.Bootstrap` 与 `AtomUI.Themes.Material` 两个预设主题包（从未发版，NuGet 上不存在），并清空全仓所有引用。

**Architecture:** 纯删除任务：删两个项目目录（各 7 个文件），再沿引用链逐层反注册——解决方案 → NuGet 包清单 → Gallery 运行时注册/菜单/本地化 → 测试项目 → 活文档。`BrandColor="Fixed"` 仍是 schema 级支持的运行时能力（用户自定义主题可用），因此 Gallery 固定品牌色路径的产品行为测试改用一个新的测试本地 fixture 承接，不新增产品代码。

**Tech Stack:** Avalonia / AtomUI 主题注册管线（`AtomUIThemeDefinition` + 生成器注册链）、xUnit、PowerShell 包清单。

**Spec:** 用户指令（2026-11-17 会话）："把红框的两个主题彻底删除"（截图红框 = 解决方案里的 AtomUI.Themes.Bootstrap、AtomUI.Themes.Material）。

## Global Constraints

- 分支纪律：直接在主工作区 `release/6.0` 上改；**禁止**创建分支/worktree。
- 提交纪律：**所有步骤不得 `git commit`**；改动停留在工作区交用户审查（用户已收回执行期自动提交授权）。计划中无 commit 步骤。
- 测试纪律：全局测试套件零净增永久测试；唯一新增物是 1 个测试本地 fixture 主题 XML + 1 行 csproj 注册（承接既有 5 个测试的 Fixed 品牌色路径覆盖，属基础设施补充而非新测试）；被删主题专属的测试随之删除。
- 视觉验收纪律：真机视觉验收未回传前不得宣称通过；本计划产出验收步骤并标注"待视觉验收"。
- 焦点验证：只跑改动模块的聚焦测试（明确 `--path` / 精确 filter），不主动跑全量/affected。
- 历史记录不动：`docs/superpowers/` 下已有 plans/specs/progress 与已发布 CHANGELOG 历史段不改写；只改 Unreleased 段与活文档。

## Review Focus

删除类任务的主要风险（对应任务内已安排验证）：

1. **残留引用导致编译失败** —— 引用链长（slnx/csproj/注册链/枚举/xlf/测试），任何一处漏改即编译错；由 Task 2/3/4 的构建与聚焦测试兜底，Task 6 的 grep 清零检查兜底。
2. **xlf 与枚举不同步** —— 删枚举成员不删 4 个 .xlf 的 unit 会让本地化目录校验失败；Task 2 成对删除并构建验证。
3. **Fixed 品牌色产品路径失去覆盖** —— 删掉两个唯一 Fixed 主题后，固定品牌色菜单提示/色板禁用/SetBrandColor 忽略等仍在产品中的逻辑无人测；由 Task 4 的 `GalleryFixedBrandFixture` 承接（有独立价值：schema 仍支持 Fixed，用户自定义主题可声明）。
4. **包清单与发布脚本脱节** —— `NuGetPackageProjects.ps1` 被 3 个脚本 dot-source；删除条目后需确认清单仍可加载且数量正确；由 Task 1 验证步骤兜底。
5. **Gallery 真机菜单/预设页残留条目** —— 代码删净但真机未看；由 Task 6 的书面视觉验收步骤 + 用户回传兜底（未回传不宣称通过）。

---

### Task 1: 删除两个主题项目目录、解决方案条目、NuGet 包清单条目

**Files:**
- Delete: `src/AtomUI.Themes.Bootstrap/`（整目录，7 文件）
- Delete: `src/AtomUI.Themes.Material/`（整目录，7 文件）
- Modify: `AtomUI.slnx:33-34`（`/Themes/` Folder 内两行 `<Project .../>`）
- Modify: `scripts/NuGetPackageProjects.ps1:12-13`（两条 PackageId/ProjectPath 记录）

**Interfaces:**
- Consumes: 无（首个任务）。
- Produces: 解决方案与发布包清单不再包含这两个项目；后续任务的构建必须全部通过。

- [ ] **Step 1: 删除两个项目目录**

```bash
rm -rf src/AtomUI.Themes.Bootstrap src/AtomUI.Themes.Material
```

预期：`ls src/ | grep -E "Bootstrap|Material"` 无输出。

- [ ] **Step 2: 移除 AtomUI.slnx 中两个 Project 条目**

删除第 33、34 行两条 `<Project Path="src/AtomUI.Themes.Bootstrap/..." />` / `...Material...`。`/Themes/` Folder 保留（仍有 PrimeNG、Shadcn）。

- [ ] **Step 3: 移除 NuGetPackageProjects.ps1 两条记录**

删除第 12-13 行两条 `[pscustomobject]@{ PackageId = "AtomUI.Themes.Bootstrap" ... }` / `...Material...` 记录。

- [ ] **Step 4: 验证清单可加载且数量正确**

```bash
pwsh -NoProfile -Command ". ./scripts/NuGetPackageProjects.ps1; $AtomUIReleasePackages.Count; ($AtomUIReleasePackages | Where-Object PackageId -match 'Bootstrap|Material').Count"
```

预期：第一条输出比删除前少 2；第二条输出 `0`。

### Task 2: Gallery 运行时反注册

**Files:**
- Modify: `controlgallery/AtomUIGallery/AtomUIGallery.csproj:27-28`（两条 ProjectReference）
- Modify: `controlgallery/AtomUIGallery/ThemeManagerBuilderExtensions.cs:4-5,21`（两个 using；注册链删 `.UseBootstrapThemes().UseMaterialThemes()`）
- Modify: `controlgallery/AtomUIGallery/Workspace/Views/GalleryThemeMenu.cs:33-34`（DesignLanguageNames 字典两条）
- Modify: `controlgallery/AtomUIGallery/Workspace/Localization/WorkspaceWindowLang/WorkspaceWindowLangResourceKind.cs:36-37`（两个枚举成员）
- Modify: `controlgallery/AtomUIGallery/Workspace/Localization/WorkspaceWindowLang/en-US.xlf`、`zh-CN.xlf`、`zh-TW.xlf`、`pt-BR.xlf`（各删 `DesignLanguageBootstrap`、`DesignLanguageMaterial` 两个 `<unit>`）
- Modify: `controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/ViewModels/ThemePresetsViewModel.cs:550-551`（`IconFor` 两条 switch 分支）

**Interfaces:**
- Consumes: Task 1（项目已删，引用悬空即编译失败——这是本任务的"失败先行"信号）。
- Produces: Gallery 不再注册/展示两主题；`WorkspaceWindowLangResourceKind` 无 `DesignLanguageBootstrap`/`DesignLanguageMaterial`。

- [ ] **Step 1: 逐文件删除上述引用**（csproj、注册链、菜单字典、枚举、4 个 xlf、IconFor）

注意：xlf 删除必须与枚举成员成对（每个 xlf 文件删 2 个 unit，共 4 文件 8 个 unit）。

- [ ] **Step 2: 构建验证**

```bash
dotnet build controlgallery/AtomUIGallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj 2>&1 | tail -5
```

预期：Build succeeded，0 errors（Desktop host 传递编译 Gallery；xlf/枚举不同步会在此报错）。

### Task 3: AtomUI.Themes.Tests 反注册并删除专属测试

**Files:**
- Modify: `tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj:22-23`（两条 ProjectReference）
- Delete: `tests/AtomUI.Themes.Tests/BootstrapThemePackageTests.cs`（整文件，主题专属）
- Delete: `tests/AtomUI.Themes.Tests/MaterialThemePackageTests.cs`（整文件，主题专属）
- Modify: `tests/AtomUI.Themes.Tests/AvaloniaTestApp.cs:4-5,23`（两个 using；注册链删两调用）

**Interfaces:**
- Consumes: Task 1。
- Produces: Themes.Tests 只注册 PrimeNG + Shadcn；PrimeNG/Shadcn 包测试不受影响。

- [ ] **Step 1: 删除两个专属测试文件 + csproj 引用 + 测试宿主注册链**

- [ ] **Step 2: 全项目测试（项目本身即主题模块，属聚焦范围）**

```bash
dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj --framework net10.0
```

预期：编译通过，全部 PASS（剩余 PrimeNG/Shadcn/内建算法测试）。

### Task 4: Gallery 测试改造——Fixed 品牌色路径改用本地 fixture 承接

**Files:**
- Create: `tests/AtomUIGallery.Tests/Fixtures/GalleryFixedBrandFixture.theme.xml`
- Modify: `tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj`（`AtomUIThemeDefinition` 增加一行，紧跟现有 GalleryPreviewFixture 条目）
- Modify: `tests/AtomUIGallery.Tests/ThemePresetsShowCaseTests.cs`（5 处，见步骤）
- Modify: `tests/AtomUIGallery.Tests/Workspace/WorkspaceWindowLayoutTests.cs:132`
- Modify: `tests/AtomUIGallery.Tests/Toolkits/GalleryBaseFoundationTests.cs:704`
- Modify: `tests/AtomUI.Desktop.Controls.Rendering.Tests/Temporary/FinalVisual/FinalVisualTemporaryTests.cs:67`（删 `("Material", null, "material-light")` 理论行）

**Interfaces:**
- Consumes: Task 2（Gallery 不再注册两主题，未改造的测试即失败——失败先行信号）。
- Produces: `GalleryFixedBrandFixture`（Id=`GalleryFixedBrandFixture`，DesignLanguage=`GalleryFixed`，BrandColor=`Fixed`，Default 算法链 + 少量种子 Token）；测试对 Fixed 品牌色产品路径的覆盖保留。

- [ ] **Step 1: 新建 fixture 主题 XML 并在 csproj 注册**

fixture 形态（对照 `GalleryPreviewFixture.theme.xml`）：根元素 `Id="GalleryFixedBrandFixture" Name="Fixed brand fixture" Appearance="Light" DesignLanguage="GalleryFixed" BrandColor="Fixed" IsDefault="false"`，`<Algorithms><Algorithm Id="Default" /></Algorithms>`，Tokens 给 `ColorPrimary` 一个显式值（如 `#7C3AED`）。csproj 加 `<AtomUIThemeDefinition Include="Fixtures/GalleryFixedBrandFixture.theme.xml" />`。

理由（测试价值）：`BrandColor="Fixed"` 仍是 schema 支持的运行时能力，菜单 Fixed 提示、色板禁用+注释、`SetBrandColor` 忽略、`CanChoosePrimary=false` 都是仍在产品中的活逻辑；删掉两个唯一 Fixed 预设后若无 fixture，这些路径零覆盖。fixture 走 Gallery 已有的 `GalleryPreviewFixture` 同款基础设施，不新增产品代码。

- [ ] **Step 2: ThemePresetsShowCaseTests.cs 五处改造**

1. `Primary_Choose_Follows_Declared_Brand_Support`：`new[] { "Bootstrap", "Material" }` 循环改为单条 `SelectedPresetId = "GalleryFixedBrandFixture"`（断言不变：CanChoosePrimary=false）。
2. `Package_Profiles_Are_Grouped_By_Family_And_Compiled_Package_Origin`：删除 Bootstrap 段（L127-138）与 Material 段（L139-149）两个块；PrimeNG/Shadcn 断言保留（Compiled_Package_Origin 覆盖不丢）。
3. `Local_Preview_Uses_Exact_Profile_Values_And_Never_Publishes_The_Root`：删除 `foreach (var family in new[] { "Bootstrap", "Material" })` 整个循环（L257-305，含其中 check/radio 禁用前景断言——该断言测的是 Material 类型化覆盖场景，随主题删除）。`publications.ShouldBe(0)` 等主断言保留。
4. `Swatches_Are_Disabled_With_A_Note_On_A_Preset_That_Fixes_Its_Primary_States`：循环内 `SelectedPresetId = "Material"` 改为 `"GalleryFixedBrandFixture"`（断言不变）。
5. 删除整个 `Own_Seed_Swatch_Follows_The_Previewed_Mode_Profile` 测试（L613-657）：它逐值钉死 Material 光/暗双自有种子（#6750A4/#D0BCFF），主题删除后无同形态预设；"Fixed 点击被忽略"已由第 4 条覆盖，"种子色板恢复"已由 `Clearing_The_Swatch_Restores_The_Preset_Seed`（PrimeNG noir）覆盖。

- [ ] **Step 3: 两个工作区测试重定向**

`WorkspaceWindowLayoutTests.cs:132` 与 `GalleryBaseFoundationTests.cs:704`：`SwitchDesignLanguageCommand.Execute("Bootstrap")` 改为 `Execute("GalleryFixed")`（fixture 的 DesignLanguage；菜单名回退主题名，无需新增枚举/翻译）。

- [ ] **Step 4: 渲染临时测试删行**

`FinalVisualTemporaryTests.cs:67` 删除 Material 理论数据行（该文件在 Temporary/ 目录，本就属临时生命周期）。

- [ ] **Step 5: 聚焦测试验证（Gallery 测试三件套 + 渲染项目编译）**

```bash
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --filter "FullyQualifiedName~ThemePresetsShowCaseTests|FullyQualifiedName~WorkspaceWindowLayoutTests|FullyQualifiedName~GalleryBaseFoundationTests"
dotnet build tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj 2>&1 | tail -3
```

预期：第一条全部 PASS（若 `GalleryFixed` 设计语言切换未被解析，按失败信息修 fixture 声明，不改产品代码）；第二条 Build succeeded。若临时测试项目编译仍报 Material 相关错，继续清残留行。

### Task 5: 活文档与 CHANGELOG 更新

**Files（活文档，删除两主题的提及；均为删行/改句，不重排结构）:**
- `docs/guides/theming/preset-theme-packages.md`（L4、L42、L55-56：包表格两行与示例）
- `docs/guides/theming/overview.md:4`
- `docs/guides/theming/customization.md:367,750,813`
- `docs/modules/themes/overview.md:5,14-15,43`（模块表两行）
- `docs/modules/overview.md:18`
- `docs/modules/toolkits-gallery-base/theming-localization.md:70-71`（代码示例链）
- `docs/architecture/systems/theming/overview.md:10`
- `docs/architecture/systems/theming/compiled-theme-packages/overview.md:196-199,201,218`（包对照表、依赖说明、profile 统计"Bootstrap 4、Material 4"）
- `docs/gallery/authoring/theme-presets-page.md:37,43-44,78`（Material.Dark/#6750A4/#D0BCFF 示例改用 PrimeNG/Shadcn 同位示例）
- `docs/controls/desktop/data-display/card/token.md:75`（"Material 的 elevation"举例换为 Shadcn/PrimeNG 等价例）
- `src/AtomUI.Themes.Shadcn/README.md:81`（列举句去掉 Bootstrap、Material）
- `docs/releases/unreleased-theme-algorithms-api-changes.md:214-219`（保留 PrimeNG focus-shadow 例，删 Bootstrap 具体值句）及其 zh-CN 对应段（L201-203）
- `CHANGELOG.md:9` 与 `CHANGELOG.zh-CN.md:11`：Unreleased 句改为只列 `AtomUI.Themes.PrimeNG` 与 `AtomUI.Themes.Shadcn`（两包从未发版，无用户可见影响，不另立删除条目）

**Interfaces:**
- Consumes: Task 1-4 的事实（包已不存在）。
- Produces: 活文档与实际代码一致。

- [ ] **Step 1: 逐文件更新上述 15 处**

- [ ] **Step 2: 文档内链接/锚点自检**

```bash
grep -rn "AtomUI.Themes.Bootstrap\|AtomUI.Themes.Material\|UseBootstrapThemes\|UseMaterialThemes" docs/guides docs/modules docs/architecture docs/gallery docs/controls docs/releases src/AtomUI.Themes.Shadcn CHANGELOG.md CHANGELOG.zh-CN.md
```

预期：无输出（`docs/superpowers/` 历史记录与已发布 CHANGELOG 段除外，不在检查范围）。

### Task 6: 最终验证、残留清零、视觉验收步骤

**Files:**
- Modify: 无代码（验证任务）
- 产出：会话内书面视觉验收步骤（不入库图片，仓库只留文字）

**Interfaces:**
- Consumes: Task 1-5 全部完成后的干净工作区（注意：工作区尚有用户既有未提交的 Segmented/Shadcn 改动，验证时排除在外，不混入）。

- [ ] **Step 1: 全仓残留引用清零检查**

```bash
grep -rn "AtomUI.Themes.Bootstrap\|AtomUI.Themes.Material\|UseBootstrapThemes\|UseMaterialThemes\|DesignLanguageBootstrap\|DesignLanguageMaterial" --include="*.cs" --include="*.csproj" --include="*.axaml" --include="*.xlf" --include="*.slnx" --include="*.ps1" src controlgallery tests scripts build tools
```

预期：无输出。

- [ ] **Step 2: 聚焦回归（改动模块逐一）**

```bash
dotnet test tests/AtomUI.Themes.Tests/AtomUI.Themes.Tests.csproj --framework net10.0
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0
```

预期：全部 PASS。（AtomUIGallery.Tests 整项目跑：其宿主注册链与预设页测试均受本改动影响，属聚焦范围。）

- [ ] **Step 3: 工作区检查**

```bash
git status --short
git diff --check
```

预期：`git status` 中两个主题目录显示删除（`D`），无 `.artifacts`/图片等杂物；`git diff --check` 无空白错误。

- [ ] **Step 4: 产出真机视觉验收步骤（用户执行并回传，未回传不宣称通过）**

书面步骤要点（执行时按 Gallery 实际菜单文案核对后交付）：
1. 启动 AtomUIGallery（Debug 即可）。预期：启动正常，默认主题 DaybreakBlue 不变。
2. 工作区「主题」菜单 → 「设计语言」子菜单。判定：列表中**无** Bootstrap、Material 条目；Ant Design、PrimeNG（Aura/Lara/Nora）、Shadcn 各家族仍在。
3. 左侧导航 General 分类 → ThemePresets（主题预设）页。判定：预设卡片**无** Bootstrap、Material；PrimeNG 3 项 + Shadcn 8 项仍在；切换 PrimeNG/Shadcn 预设与暗色/紧凑开关行为不变。
4. 「其他主题」列表。判定：GalleryPreviewFixture 等原有条目不变，无新增异常条目。
5. 截图范围：设计语言子菜单展开态 + ThemePresets 页整页。

标注：**待视觉验收**。

---

## Self-Review 记录

- **覆盖度**：用户指令 = "彻底删除"；摸底清单 9 类引用全部落到 Task 1-5；无关同名（`_bootstrapTask`、`MaterializeTypeMaps`、`MediaBreakPointThemeBootstrapper`、Core.Tests 合成目录数据 "Bootstrap"）明确不动。`ThemeSelectionResolverTests` 的合成 "Bootstrap" 字符串不依赖真实程序集，保留（改名属无谓翻动）。
- **TDD 适配说明**：删除任务无新增产品行为，"失败先行"由引用链断裂的编译失败与 Task 4 先失败后重定向的测试承担。
- **一致性**：fixture Id/DesignLanguage 在 Task 4 各步骤统一为 `GalleryFixedBrandFixture` / `GalleryFixed`。
- **比例**：无代码正文转录，步骤均给出精确文件/行/断言去向。
- **审查关注**：`Own_Seed_Swatch_Follows_The_Previewed_Mode_Profile` 整测删除与 `Local_Preview...` 中 check/radio 禁用断言随删，是本计划两个"覆盖取舍"点，请用户审阅时留意（备选：若要求保留，需新增带暗色 profile 的 fixture 并复刻断言，成本超出删除任务边界，默认不做）。
