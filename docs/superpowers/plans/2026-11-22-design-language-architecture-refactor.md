# AtomUI 设计语言架构重构 Implementation Plan

> Follow the implementation steps and track progress with Markdown checkboxes (`- [ ]`).

**Goal:** 把 AtomUI 的主题系统重构为《AtomUI 设计语言架构方案》规定的形态——Ant Design、Shadcn、PrimeNG 三种设计语言平级，控件只声明 Token、取值全部归语言一侧——并且重构完成后现有每种设计语言、每个变体的渲染与现状逐像素一致。

**Architecture:** 先在旧架构内做等价清理，把所有旁路变成普通 Token；再一次性替换「值怎么算出来」这一层（语言契约、三类 Token、单遍管线、主题定义 v2），运行时发布层原样保留；然后按 Ant Design → Shadcn、PrimeNG 的顺序把取值迁入语言包与取值包。每个里程碑以改造前的同一棵树为基准，用取值比对与像素比对两道门槛验收。

**Tech Stack:** .NET 10 / C# 13、Avalonia 12.1、Roslyn 增量生成器与分析器（`AtomUI.Generator`）、MSBuild 任务（`AtomUI.Toolchain`）、xUnit v3 + Avalonia.Headless、现有性能与截图工具（`tools/performances`）。

**Spec:** `docs/superpowers/specs/2026-11-22-design-language-architecture-design.md`（下文称「方案」，引用其章节号）。

**进度跟踪：** `docs/superpowers/progress/2026-11-22-design-language-refactor-progress.md`。全部任务的编号、依赖、状态与证据以它为准；每完成一项就更新它。

**本文件的定位：** 方案覆盖多个子系统与三个仓库，本文件是总实施方案：给出里程碑、每个工作包的文件、接口、做法与验收门槛。里程碑 M0、M1 在现有架构内完成，步骤已写到可直接执行；M2 之后的工作包在其所属里程碑开工时各出一份步骤级计划（`docs/superpowers/plans/` 下，文件名带里程碑编号），因为它们的精确签名依赖前一里程碑定稿的接口。

## Global Constraints

- **基准树：** release/6.0 `8c946e4f5`。所有「与现状一致」都指与这棵树一致。
- **像素一致是硬门槛：** 48 个变体下，Gallery 全部 77 个已注册独立 showcase 的整页截图（另有 15 个子视图文件，由所属页承载）与每个控件类型的全部状态截图，稳定像素差异必须为 0。48 个变体 = Ant Design 4 个（常规、暗色、紧凑、暗色紧凑）+ PrimeNG 12 个（Aura / Lara / Nora × 4）+ Shadcn 32 个（Vega / Nova / Maia / Lyra / Mira / Luma / Sera / Rhea × 4）。
- **Ant Design 零渲染差异：** Ant Design 的 4 个变体均要求稳定渲染一致。Palette 命名空间文字按台账中记录的批准在抓拍时统一，整帧仍逐像素比较。
- **其他语言的例外须经批准：** Shadcn 与 PrimeNG 如有无法保持的差异，逐条写入例外清单（位置、原因、前后截图路径），经项目负责人批准后才算通过。预期只有一类：非 Ant 语言页面里使用局部算法链的区域（方案 7.7 取消了算法链整体替换）。
- **取值一致先于像素一致：** 每个工作包先过取值比对，再过像素比对。取值比对按 Token 名逐项相等；重构中改名或迁移的 Token 记在映射表里。
- **不提交、不推送：** 执行过程中不做 `git commit`。每个里程碑到达门槛后停下，由项目负责人评审，再分组提交。唯一的例外是 M0-5：M0 的工具改动评审通过后提交一次，作为基线构建的固定提交。提交信息不带任何署名或工具标记，经 `commit-msg` 技能生成。
- **只在本分支的 worktree 内工作：** `feature/design-language-products`，`/Users/chinboy/Projects/dotnet/AtomUIV6-design-language-products`。
- **基线构建：** M0-6 建立的 worktree `/Users/chinboy/Projects/dotnet/AtomUIV6-design-language-baseline`（检出 M0-5 的提交）的 Release 构建。整个重构期间不改动；每次验收都拿它与候选构建现场对比，不在仓库里存放截图或取值文件。
- **出现实施方案没写到的情况先请示：** 需要自行决定的地方停下来问项目负责人，不自行裁定。
- **dotnet 命令一律带** `--disable-build-servers -nodeReuse:false`。
- **代码内的 XML 文档注释用英文**；文档用中文。`docs/superpowers/` 只放 Markdown，截图与基线产物放 `.artifacts/`，不进仓库。
- **第一方无特权：** 语言包与取值包不得使用 `InternalsVisibleTo`；M1 完成后这一条由构建校验。
- **性能不退化：** 每个里程碑按现有门槛对照基准树测量（方案 12.5）。
- **命名：** 语言包 `AtomUI.DesignLanguages.<Lang>`；取值包 `<控件包名>.Themes.<Lang>`，每个带控件 Token 的控件包一对一，不合并。`<Lang>` 取 `AntDesign`、`Shadcn`、`PrimeNG`。
- **Token 声明不搬家：** 控件的 Token 类留在控件包里、与控件定义同目录，只移走其中的取值公式。

## Review Focus

方案没有逐条写明、而最容易在使用中出问题的五种情况。每条的测试落在标注的工作包里。

1. **用户目录里还放着 v1 主题文件。** 期望：应用照常启动，该文件不可用，主题目录诊断里明确指出是 v1 格式并给出换写命令；不抛未处理异常。→ M2-5
2. **启用了控件包却没装它在当前语言下的取值包。** 期望：配置提交时失败，消息写出缺的包名（例如 `AtomUI.MachGrid.Themes.Shadcn`）；不会拖到某个控件首次显示才出错。→ M2-6
3. **运行时切换语言时，某个局部作用域还带着旧语言的语言 Token 覆盖。** 期望：这些覆盖在新语言下不生效并产生一条诊断，切换本身成功；切回旧语言后覆盖恢复生效。→ M2-4
4. **控件包升级后新增了 Token，而第三方取值包还是旧的。** 期望：启动时按 Token Schema 摘要比对失败，消息指出不匹配的两个包及各自的摘要；不会带着缺值运行。→ M2-6
5. **在控件上覆盖种子的主题（v1 默认不重新派生，v2 一律重新派生）。** 期望：换写命令对这类条目逐条提示且不自动改写；v2 下的行为与方案 7.5 一致，并有测试固定。→ M2-5

---

## 文件结构

重构结束时主仓里与本工作相关的工程：

```text
src/
  AtomUI.Core/                                          改：Theme/ 下的编译前半段、配置、定义、Schema
  AtomUI.Controls/                                      改：3 个 Token 类清为纯声明，留在原处
  AtomUI.Desktop.Controls/                              改：142 个 Token 类清为纯声明
  AtomUI.Desktop.Controls.ColorPicker/                  改：6 个 Token 类清为纯声明
  AtomUI.Desktop.Controls.DataGrid/                     改：1 个 Token 类清为纯声明
  AtomUI.Toolkits.GalleryBase/                          改：8 个 Token 类清为纯声明 + 同程序集内的通用推导
  AtomUI.Generator/                                     改：DesignToken/、ThemeDefinitions/、ThemeSchema/、Registration/
  AtomUI.Toolchain/                                     改：Tasks/GenerateCompiledThemeInputTask.cs、Common/ThemeDefinitions/
  AtomUI.Themes.Shadcn/                                 删
  AtomUI.Themes.PrimeNG/                                删

  DesignLanguages/                                      新：设计语言相关的全部工程，按语言归拢
    AntDesign/
      AtomUI.DesignLanguages.AntDesign/                 语言包：AntDesignLanguage、AntDesignToken、全局推导
      AtomUI.Controls.Themes.AntDesign/                 取值包：跨端共用控件的推导与共享推导方法
      AtomUI.Desktop.Controls.Themes.AntDesign/         取值包
      AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign/
      AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign/
    Shadcn/                                             同构的五个工程（语言包由 AtomUI.Themes.Shadcn 改造）
    PrimeNG/                                            同构的五个工程（语言包由 AtomUI.Themes.PrimeNG 改造）
tools/performances/AtomUI.GalleryPerformance/           改：按变体运行、取值导出（M0）
tools/performances/AtomUI.ControlBaseline/              改：按变体运行（M0）
tools/performances/run_perf_guards.sh                   改：可选的变体参数（M0）
tools/performances/run_design_language_guards.sh        新：逐变体运行守卫并比对取值（M0）
tools/performances/compare_token_dumps.py               新：取值导出的比对（M0）
```

主仓共 4 个带控件 Token 的控件包，3 种语言，12 个取值包，加 3 个语言包，共 15 个新工程。取值包引用对应的控件包（为了拿到
Token 类型）与对应的语言包；依赖其他控件包的，还引用那个控件包在同一语言下的取值包（例如
`AtomUI.Desktop.Controls.Themes.AntDesign` 引用 `AtomUI.Controls.Themes.AntDesign`）。

**目录与解决方案分组一致。** 磁盘上按 `src/DesignLanguages/<Lang>/<工程名>/` 归拢，做法与现有的
`src/LanguagePacks/pt-BR/` 相同；`AtomUI.slnx` 里现有的 `/Themes/` 分组改为 `/DesignLanguages/`，其下按语言再分一层
（`/DesignLanguages/AntDesign/`、`/DesignLanguages/Shadcn/`、`/DesignLanguages/PrimeNG/`），与现有的 `/Localization/pt-BR/`
相同。每个分组里是该语言的语言包与它的全部取值包。

- 控件包留在 `src/` 根下与 `/Libraries/` 分组；各语言的测试工程留在 `tests/` 与 `/Tests/` 分组。
- 新增一种语言时，只新增一个目录与一个分组。
- 工程移到更深一层目录后，`Directory.Build.props`、`build/*.props` 与 `build/*.targets` 里按相对路径定位仓库根或兄弟工程的
  地方要一并核对；`src/LanguagePacks/` 下的工程已经在同样的深度上工作，以它们的写法为准。
- 分组在 M3-2（建立 Ant Design 的目录与分组）与 M4-5（建立另外两个、删除 `/Themes/` 分组与两个旧工程目录）落实。

`AtomUI.Core/Theme/` 内的归属：

| 目录 | 现状 | 处理 |
| --- | --- | --- |
| `Algorithms/` | 算法契约、三个 Ant 计算器、色板工具 | 契约删除；计算器的规则迁入 Ant 语言包；色彩数学留下，改名 `Derivation/` |
| `Compilation/` | `ThemeCompiler`（1474 行）、`StagedTokenEvaluation`、快照、缓存 | 重写 `ThemeCompiler` 的全局与控件求值；删 `StagedTokenEvaluation`；快照与缓存保留 |
| `Configuration/` | `ThemeConfig`、Merger、Normalizer、`ControlAlgorithmMode` | 改字段与合并规则；删 `ControlAlgorithmMode` |
| `Definitions/` | XML v1 的 Reader、Binder、编译主题 ABI v1、来源与目录 | Reader、Binder、ABI 换成 v2；来源与目录保留 |
| `DesignTokens/` | Token 基类、`DesignToken`（257 个）、特性 | 删公式；加语言 Token 基类；写入记账换成冻结位与读集 |
| `Schema/` | 注册表、Token 与控件描述、算法描述 | 加语言与推导的登记；删算法描述 |
| `Resources/`、`SemanticParts/`、作用域与事务相关文件 | 资源解析、语义部件、`ThemeScopeGraph`、`ThemeTransaction` 等 | 不动 |

---

## 验收门槛的统一定义

后文各里程碑的「门槛」都指下面四项。G-取值 与 G-像素 由 `tools/performances/run_design_language_guards.sh <基线目录> <候选目录> <输出目录>`
一次跑完（每个变体附带泄漏检查：候选的存活实例不得多于基线）；全量 48 个变体的运行频率见 M0-6 的决定。

| 门槛 | 含义 | 通过标准 |
| --- | --- | --- |
| G-取值 | 48 个变体 × 种子组合（共 484 份），基线构建与候选构建各导出全部全局 Token 与控件控件 Token，逐键比对 | `compare_token_dumps.py` 退出 0；差异只能出现在已批准的改名映射或例外清单里 |
| G-像素 | 48 个变体，各跑一遍 `run_perf_guards.sh`：Gallery 全部页面整页截图与每个控件类型的全部状态截图，基线与候选各两遍 | 稳定像素差异为 0；尺寸不变；例外清单按 Global Constraints |
| G-性能 | 主题编译与切换（Ant Design 三条链）、启动，方法见 M0-7；资源查询零分配由现有测试保证 | 按现有门槛不退化 |
| G-回归 | `scripts/run-full-regression.sh` | 全部通过 |

---

## M0 基线与护栏

目标：在动任何产品代码之前，让现有的验收工具能按变体运行，并建立一份整个重构期间不变的基线构建。

**做法（2026-11-22 与项目负责人确认）：** 不新建工程，不往仓库里存基线文件。基线是一份没有改动产品代码的构建，放在
独立 worktree 中；每次验收把它与改动后的构建现场对比。像素比对复用 `tools/performances/run_perf_guards.sh`（Gallery
整页截图、控件全状态截图、两边各拍两遍屏蔽不稳定像素、泄漏检查），只补「按变体运行」；取值比对在同一套工具里加导出与
比对。

**变体的写法：** `<设计语言> <明暗> <密度>`。设计语言用改造前主题目录里的分组标识：`AntDesign`、`PrimeNG.Aura`、
`PrimeNG.Lara`、`PrimeNG.Nora`、`Shadcn.Vega`、`Shadcn.Nova`、`Shadcn.Maia`、`Shadcn.Lyra`、`Shadcn.Mira`、`Shadcn.Luma`、
`Shadcn.Sera`、`Shadcn.Rhea`；明暗为 `Light` / `Dark`；密度为 `Normal` / `Compact`。共 12 × 4 = 48 个。这个写法在整个
重构期间不变，重构后由工具在内部映射到新的选择模型。变体的键与产物目录名为 `<设计语言>.<明暗>.<密度>`，例如
`Shadcn.Maia.Dark.Compact`。

### Task M0-1：GalleryPerformance 按变体运行

**Files:**
- Create: `tools/performances/AtomUI.GalleryPerformance/DesignLanguageVariants.cs`
- Modify: `tools/performances/AtomUI.GalleryPerformance/Program.cs`（参数解析；主窗口显示后、导航前应用变体）
- Modify: `tools/performances/AtomUI.GalleryPerformance/GalleryAuditProbe.cs`（截图与泄漏两种模式都在开始前应用变体）

**Interfaces:**
- Produces:
  - `internal readonly record struct DesignLanguageVariant(string DesignLanguage, ThemeAppearance Appearance, ThemeDensity Density)`，
    带 `string Key`（`<设计语言>.<Light|Dark>.<Normal|Compact>`）。
  - `internal static class DesignLanguageVariants`：`IReadOnlyList<DesignLanguageVariant> All`（48 个，顺序固定：AntDesign、
    PrimeNG 三种、Shadcn 八种；每种内 Light/Normal、Light/Compact、Dark/Normal、Dark/Compact）；
    `bool TryParse(string designLanguage, string appearance, string density, out DesignLanguageVariant variant)`；
    `ThemeRequest CreateRequest(IReadOnlyList<ThemeInfo> catalog, DesignLanguageVariant variant, Color? brand, Color? neutral)`，
    与 `GalleryWorkspaceViewModel.ApplyThemeSettingsAsync` 的设计语言分支相同：`ThemeSelectionResolver.Resolve` 给出的主题与
    算法链，`BrandSeed` 写入 `ColorPrimary`，未被忽略的 `NeutralSeed` 写入 `ColorNeutral`，颜色按 `#RRGGBB` 格式化。
    `CreateRequest` 供 M0-2、M0-3 使用。
  - 命令行：`--design-language <id> --appearance <Light|Dark> --density <Normal|Compact>`，三个同时给出或都不给；
    `--list-variants` 输出 48 行 `<设计语言> <明暗> <密度>`。
- 切换方式：主窗口显示后、导航任何页面之前，依次执行 `WorkspaceWindowViewModel` 的 `SetAppearanceModeCommand`
  （`ThemePreference.Light` / `Dark`）、`ToggleCompactModeCommand`（是否紧凑）、`SwitchDesignLanguageCommand`（设计语言），
  等待主题事务提交。这是用户点菜单走的同一条路。
- 核对方式（指纹比对，2026-11-22 项目负责人确认）：菜单命令切换后，再用 `CreateRequest` 对同一个变体应用一次，要求两次的
  主题 Id 相同、明暗与变体一致、`ThemeState.Fingerprint`（快照的内容指纹）相同，否则非零退出。Ant Design 的紧凑只存在于
  算法链里，从公开状态读不到，指纹比对覆盖了它；同时证明菜单路径与 M0-2、M0-3 所用的解析器路径结果一致。
  **这是重构期间的临时机制，M5 验收通过后删除（M5-6）。**

- [ ] **Step 1：** 实现 `--list-variants`。Release 构建后运行
  `dotnet .artifacts/bin/Release/net10.0/AtomUI.GalleryPerformance.dll --list-variants | wc -l`，期望 `48`。
- [ ] **Step 2：** 实现三个变体参数与切换。以 `Shadcn.Maia Dark Compact` 截 `button` 一页
  （`--audit --audit-full --audit-showcases button --snapshot-dir <dir>`），打开截图确认是该变体：暗色、紧凑、按钮为胶囊圆角。
- [ ] **Step 3：** 不带变体参数、与带 `AntDesign Light Normal`，各截 `button` 页两遍，用 `compare_snapshots.py` 比对，期望退出码 0。

### Task M0-2：ControlBaseline 按变体运行

**Files:**
- Modify: `tools/performances/AtomUI.ControlBaseline/AtomUI.ControlBaseline.csproj`（引用 `src/AtomUI.Themes.PrimeNG`、
  `src/AtomUI.Themes.Shadcn`；以链接方式编译 `../AtomUI.GalleryPerformance/DesignLanguageVariants.cs`）
- Modify: `tools/performances/AtomUI.ControlBaseline/Program.cs`

**Interfaces:**
- Consumes: M0-1 的 `DesignLanguageVariants`。
- Produces: `--mode atomui` 下同样的三个变体参数。
- 注册：AtomUI 模式额外调用 `atom.Theme.UsePrimeNGThemes().UseShadcnThemes()`，只把两个包的主题加入目录，初始主题仍是
  `DaybreakBlue`。
- 切换：`SetupWithLifetime` 之后、创建任何控件之前，用 `DesignLanguageVariants.CreateRequest`（不带种子）应用并等待提交。
  ControlBaseline 没有 Gallery 菜单，这是菜单背后同一个解析路径。

- [ ] **Step 1：** 实现。
- [ ] **Step 2：** 不带变体参数、与带 `AntDesign Light Normal`，各跑两遍 `--state-snapshots`，用 `compare_snapshots.py` 比对，
  期望退出码 0。
- [ ] **Step 3：** 以 `Shadcn.Maia Dark Compact` 跑一遍 `--state-snapshots`，打开 Button 的截图确认是该变体。

### Task M0-3：取值导出与比对

**Files:**
- Modify: `tests/AtomUI.Themes.Tests/ThemeSnapshotDump.cs`：把文件末尾的测试类 `ThemeSnapshotDumpTests` 原样移到同目录新文件
  `ThemeSnapshotDumpTests.cs`，辅助类 `ThemeSnapshotDump` 不改。原因：GalleryPerformance 要以链接方式编译这个辅助类，而测试类
  依赖 xUnit。
- Modify: `tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj`：
  `<Compile Include="../../../tests/AtomUI.Themes.Tests/ThemeSnapshotDump.cs" Link="Theme/ThemeSnapshotDump.cs" />`
- Modify: `src/AtomUI.Core/AtomUI.Core.csproj`：`<InternalsVisibleTo Include="AtomUI.GalleryPerformance"/>`（导出时读取主题快照的
  内部数据；已有 `AtomUI.ThemeAssetsValidation` 这个先例）
- Create: `tools/performances/AtomUI.GalleryPerformance/TokenDumpProbe.cs`
- Create: `tools/performances/compare_token_dumps.py`

**Interfaces:**
- Consumes: M0-1 的 `DesignLanguageVariants`；`ThemeSnapshotDump.Dump(ThemeSnapshot)`（行格式 `G <Token> = <值>` 与
  `C <Catalog>:<Control> <Token> = <值>`）。
- Produces:
  - `--dump-tokens <dir>`：按 Gallery 的注册方式启动（`GalleryApplication.Configure`），对 48 个变体 × 种子组合逐个用
    `DesignLanguageVariants.CreateRequest` 应用并等待提交，把 `ThemeSnapshotDump.Dump` 的结果写到
    `<dir>/<变体 Key>[@<种子名>].tokens.txt`，共 484 个文件。
  - 种子组合（默认种子之外）：

    | 设计语言 | 种子名与值 |
    | --- | --- |
    | `AntDesign` | 品牌色 `brand-52C41A`、`brand-FA8C16`、`brand-722ED1`、`brand-EB2F96` |
    | `PrimeNG.*` | 品牌色 `brand-10B981`、`brand-3B82F6`、`brand-F59E0B` |
    | `Shadcn.*` | Base Color（中性色，Tailwind v4.3.5 各族 500 档）：`base-neutral` `#737373`、`base-stone` `#79716B`、`base-zinc` `#71717B`、`base-mauve` `#79697B`、`base-olive` `#7C7C67`、`base-mist` `#67787C`、`base-taupe` `#7C6D67`；Theme（品牌色，700 档，yellow 取 400 档）：`theme-blue` `#1447E6`、`theme-rose` `#C70036`、`theme-green` `#008236`、`theme-violet` `#7008E7`、`theme-yellow` `#FDC700` |

    合计：AntDesign 4 × 5、PrimeNG 12 × 4、Shadcn 32 × 13，共 484。种子组合只用于取值比对；像素比对只跑默认种子。
  - `compare_token_dumps.py BASE_DIR CANDIDATE_DIR [--renames FILE] [--exceptions FILE] [--allow-added]`：按文件、按键比对
    （键是 ` = ` 之前的部分），输出四类：只有一边有的文件、仅基线有的键、仅候选有的键、值不同的键。`--renames` 每行
    `旧键 -> 新键`；`--exceptions` 每行一个键，命中的差异跳过并在末尾列出；`--allow-added` 时「仅候选有的键」只列出、不计为
    失败（供新增 Token 的工作包使用）。无失败退出 0，否则 1。改名映射与例外清单的文件在第一次需要时建立，位置为
    `tools/performances/design_language_token_renames.txt` 与 `tools/performances/design_language_token_exceptions.txt`。

- [ ] **Step 1：** 拆出 `ThemeSnapshotDumpTests.cs`。运行
  `dotnet test tests/AtomUI.Themes.Tests --disable-build-servers -nodeReuse:false --filter FullyQualifiedName~ThemeSnapshotDump`，
  期望与拆分前相同的 3 个测试通过。
- [ ] **Step 2：** 实现 `--dump-tokens`。运行一次，期望写出 484 个文件、退出码 0。
- [ ] **Step 3：** 实现 `compare_token_dumps.py`。同一构建导出两遍互相比，期望退出码 0；改动其中一份的一行再比，期望退出码 1
  并打印该行；在候选一侧加一行，带 `--allow-added` 比，期望退出码 0 且列出该行。

### Task M0-4：守卫脚本

**Files:**
- Modify: `tools/performances/run_perf_guards.sh`：参数改为
  `BASELINE_BIN CANDIDATE_BIN OUTPUT_DIR [SHOWCASES] [DESIGN_LANGUAGE APPEARANCE DENSITY]`。给出变体时，GalleryPerformance 的
  截图与泄漏、ControlBaseline 的状态截图与泄漏都带上三个变体参数；不给时行为与现在完全相同。
- Create: `tools/performances/run_design_language_guards.sh BASELINE_BIN CANDIDATE_BIN OUTPUT_DIR [SHOWCASES] [VARIANT_FILTER]`：
  1. 用 `dotnet CANDIDATE_BIN/AtomUI.GalleryPerformance.dll --list-variants` 取变体列表；`VARIANT_FILTER` 是匹配
     `<设计语言> <明暗> <密度>` 整行的正则，给出时只跑匹配的变体；
  2. 每个变体调用 `run_perf_guards.sh`，输出到 `OUTPUT_DIR/<变体 Key>/`；
  3. 两边各跑一次 `--dump-tokens`，再运行 `compare_token_dumps.py`；改名映射与例外清单文件存在时自动带上；
  4. 打印每个变体与取值比对的结果和耗时；任一失败即非零退出。
- Modify: `tools/performances/README.md`：「性能改动的视觉与泄漏守卫」一节补上变体参数与新脚本的用法。

- [ ] **Step 1：** 用本 worktree 的同一份 Release 构建同时作为基线与候选，只跑 `AntDesign Light Normal` 与 `button` 一页，期望 PASS。
- [ ] **Step 2：** 同样用同一份构建，只跑 `Shadcn.Nova` 的 4 个变体与 `button` 一页，期望 PASS。

### Task M0-5：M0 评审与提交

- [ ] M0-1…4 完成后**停下**，由项目负责人评审改动。
- [ ] 评审通过后提交 M0 的工具改动（只含 M0-1…4 的改动，不含产品代码，提交信息经 `commit-msg` 技能）。这是本次重构里唯一
  一次在门槛之前的提交，用途是让基线构建有一个固定的提交可以检出。

### Task M0-6：基线构建、全量自检与运行频率

**Produces:** worktree `/Users/chinboy/Projects/dotnet/AtomUIV6-design-language-baseline`（分离头指针，检出 M0-5 的提交）；基线
目录 `/Users/chinboy/Projects/dotnet/AtomUIV6-design-language-baseline/.artifacts/bin/Release/net10.0`。整个重构期间这个 worktree
不做任何改动。

- [ ] **Step 1：** `git worktree add --detach /Users/chinboy/Projects/dotnet/AtomUIV6-design-language-baseline <M0-5 的提交>`；在其中以
  `-c Release --framework net10.0 --disable-build-servers -nodeReuse:false` 构建 `AtomUI.GalleryPerformance` 与 `AtomUI.ControlBaseline`。
- [ ] **Step 2：** 本 worktree 同样构建，运行
  `tools/performances/run_design_language_guards.sh <基线目录> .artifacts/bin/Release/net10.0 .artifacts/design-language-guards/m0-selfcheck all "^AntDesign Light Normal$"`
  （全部页面、一个变体，加上全部 484 份取值比对），期望 PASS，记下耗时。
- [ ] **Step 3：** 把单个变体的耗时、48 个变体的估算写进进度表，**停下请示**全量 48 个变体的运行频率（例如每个门槛跑全量、
  工作包内只跑受影响的变体）。决定写进本节与进度表后，M0-6 才算完成。

### Task M0-7：性能基线

- [ ] **Step 1：** 主题编译与切换：在基线 worktree 运行现有的显式测量
  `tests/AtomUI.Desktop.Controls.Tests/Theme/Performance/ThemeCompileMeasurement.cs` 的 `Measure_Default_Theme_Compile_And_Switch`
  （`ATOMUI_MEASURE_OUTPUT` 指定输出文件，`ATOMUI_MEASURE_LABEL=baseline`），跑两遍。
- [ ] **Step 2：** 启动：按 `tools/performances/AtomUI.StartupPerformance/README.md` 的多样本方式在基线 worktree 采样两轮。
- [ ] **Step 3：** 把数值、两轮之间的波动（即本机的分辨力）、机器型号与系统版本写进进度表「门槛记录」。

已知缺口：现有的编译测量只覆盖 Ant Design 默认主题的三条链（默认、暗色、紧凑），Shadcn 与 PrimeNG 的编译耗时没有现成工具，
M0 不补。这个测量依赖的编译器内部接口会在 M2 被替换，M2-3 需要一并改写它。

稳态正式验收沿用既有 R2 口径。关闭 .NET 分层编译，冷编译 3001 次计时与 300 次预热，切换 501 次计时与 50 次预热，每侧至少 7 次交错。冷编译与缓存切换使用 P25，未缓存切换使用中位数。原生启动采样保持默认分层。[R2 采样与统计口径](../progress/2026-11-13-theme-algorithm-strategy.md)

**M0 门槛：** M0-1…7 完成：基线 worktree 构建完毕，单变体全量自检 PASS，全量运行频率已定，性能基线已记录。停下评审。

---

## M1 旧架构内的等价清理

目标：趁管线还没换，把所有「靠旁路实现」的东西变成普通 Token。每一项都能单独用 G-取值 与 G-像素 验证，出问题时范围很小。M1 结束时，语言包不再使用无类型旁路，也不再依赖内部 API。逐控件的全局改写（`EvaluateGlobal`）留到 M2，由新管线的控件视图直接取代（方案 6.1）。

### Task M1-1：行为开关 Token 化

**Files:**
- Modify: `src/AtomUI.Desktop.Controls/Buttons/ButtonThemeRecipe.cs`（删除）、`Buttons/Button.cs`、`Buttons/Button.Variants.cs`、`Buttons/Themes/ButtonTheme.axaml`、`Buttons/Themes/DropdownButtonBaseTheme.axaml`、`AdornerLayer/FocusOutlineAdorner.cs`
- Modify: `src/AtomUI.Desktop.Controls/Buttons/AbstractButtonGeometryToken.cs`（新增属性）
- Modify: `src/AtomUI.Desktop.Controls/Primitives/InputControlFrame.cs` 及 `InputFrameAppearance` 所在文件
- Modify: `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonThemeInputs.cs` 及 FloatButton 的 Token 类
- Modify: `src/AtomUI.Desktop.Controls/SplitButton/SplitButton.cs` 及 SplitButton 的 Token 类
- Modify: `src/AtomUI.Core/Theme/DesignTokens/AbstractControlDesignToken.cs`、`Theme/Compilation/ControlThemeSnapshot.cs`、`Theme/Compilation/ThemeCompiler.cs`（删 `AppearanceSettings`）
- Modify: `src/AtomUI.Themes.Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs`、`ShadcnFloatButtonTokens.cs`、`ShadcnInputTokens.cs`；`src/AtomUI.Themes.PrimeNG/Algorithms/Controls/PrimeNGButtonTokens.cs`
- Test: `tests/AtomUI.Desktop.Controls.Tests/` 下对应控件的测试

**Interfaces:**
- Produces（新增的控件 Token 属性，名字此处定死）：
  - `AbstractButtonGeometryToken` 新增七个属性，对应 `ButtonThemeRecipe` 的六个字段（可空的 `DisabledOpacity` 拆成一个开关加一个数值）：

    | 属性 | 类型 | Ant 默认值 |
    | --- | --- | --- |
    | `RecipeVariants` | `bool` | `false` |
    | `OutlineOnlyDefaultShadow` | `bool` | `false` |
    | `DisabledByOpacity` | `bool` | `false` |
    | `DisabledOpacity` | `double` | `1.0` |
    | `PressMoves` | `bool` | `false` |
    | `UnderlineLinkOnHover` | `bool` | `false` |
    | `UnderlineLinkAtRest` | `bool` | `false` |
  - SplitButton 的 Token 类：`bool InsetSeparator`，Ant 默认 `false`。
  - 输入族与 FloatButton：把 `InputFrameAppearance`、`FloatButtonOutlineAppearance` 的每个字段一对一变成各自 Token 类上的同名属性，Ant 默认值取「没有该对象时」的现有行为。

- [ ] **Step 1：** 为 Button 写测试：在 `ShadcnNova` 下 `ButtonToken.PressMoves` 为 `true`、`RecipeVariants` 为 `true`；在 `DaybreakBlue` 下全部为 Ant 默认值。期望失败（属性不存在）。
- [ ] **Step 2：** 加属性；Ant 的 `CalculateTokenValues` 写默认值；两个语言包的 `EvaluateOwn` 把原来塞进 `AppearanceSettings` 的记录改为逐个属性赋值；控件改为从 Token 读。测试通过。
- [ ] **Step 3：** 对 SplitButton、输入族、FloatButton 重复同样的步骤。
- [ ] **Step 4：** 删除 `AppearanceSettings` 及其在快照与编译器中的传递。全仓搜索 `AppearanceSettings` 期望无结果。
- [ ] **Step 5：** G-取值（新增属性不在基线内，以 `compare_token_dumps.py --allow-added` 列为「仅候选有」，其余逐项相等）；G-像素 全部 48 个变体。

### Task M1-2：逐控件全局改写的清点

现状：两个语言包在 22 个文件里共有 125 处直接改写 `controlGlobal.X`（Shadcn 45 处、涉及 33 个全局 Token；PrimeNG 80 处、涉及 57 个），另有经辅助方法的改写。它们同时影响两样东西：控件自己的默认公式（读 `EffectiveGlobalToken`），以及控件模板里直接读全局 Token 的地方。

**Files:**
- Create: `docs/superpowers/progress/2026-11-22-control-global-rewrite-inventory.md`

- [ ] **Step 1：** 列出全部改写，一行一条：`语言 | 阶段类 | 控件 Token 类 | 被改写的全局 Token | 新值的表达式 | 条件（风格、明暗）`。来源：`grep -rnE 'controlGlobal\.[A-Za-z0-9]+ *=' src/AtomUI.Themes.Shadcn src/AtomUI.Themes.PrimeNG`，加上被 `EvaluateGlobal` 调用的辅助方法（`ShadcnButtonAppearance.ApplyGlobal` 等）里的写入。
- [ ] **Step 2：** 对每个「控件 × 全局 Token」对，查出它被谁读：该控件 Token 类的 `CalculateTokenValues`（记下读它的控件 Token）；该控件及其部件的 ControlTheme（`{atom:<Control>TokenResource <SharedKind>}` 与 `OwnerTokenResource`）；控件 C# 代码里的强类型读取。写进同一张表的后三列。
- [ ] **Step 3：** 清点表的用途：M4 移植语言代码时，作为把每处改写换写为 `view.Global` 覆盖（方案 6.1）的对照清单；「EvaluateOwn 依赖」列标出语言自己的推导读了改写值的地方，换写时改为读 `view.Global`（得到覆盖后的值）。

### Task M1-4：去掉语言包对内部 API 的依赖

**Files:**
- Modify: `src/AtomUI.Core/AtomUI.Core.csproj:44-45`、`src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj:28-29`（删四行 `InternalsVisibleTo`）
- Modify: `src/AtomUI.Core/Theme/Algorithms/ThemeAlgorithmContext.cs`（`HasConfiguredValue` 改为 `public bool IsConfigured(SharedTokenKind kind)`）
- Modify: 删掉 IVT 后编译报错的每一处

- [ ] **Step 1：** 删四行 IVT，构建 `dotnet build src/AtomUI.Themes.Shadcn src/AtomUI.Themes.PrimeNG --disable-build-servers -nodeReuse:false`，把报错逐条列出。
- [ ] **Step 2：** 逐条处理：语言包确实需要的能力改为公开 API（每个新公开的成员写进 `docs/releases/unreleased-theme-algorithms-api-changes.zh-CN.md`）；只是顺手用了内部辅助的，在语言包里改写。
- [ ] **Step 3：** 两个包构建通过；G-取值、G-像素、G-回归。

**M1 门槛：** 四项门槛全过，Ant Design 无例外。停下评审并分组提交。此时仓库仍是旧架构，可以正常发布。

---

## M2 核心：新契约与单遍管线

目标：替换 `AtomUI.Core` 里「值怎么算出来」的那一层，并让生成器与工具链支持新模型。M2 到 M4 是切换窗口：窗口内只保证 Ant Design 可用（M3 门槛），Shadcn 与 PrimeNG 在 M4 恢复。窗口内两个旧语言包暂时移出解决方案。

本里程碑的六个工作包接口相互咬合，必须一起落地；每个工作包开工前出步骤级计划。下面定死它们之间的接口。

### Task M2-1：三类 Token 的基础设施

**Files:**
- Modify: `src/AtomUI.Core/Theme/DesignTokens/AbstractDesignToken.cs`、`AbstractControlDesignToken.cs`、`TokenWriteLog.cs`（删）
- Create: `src/AtomUI.Core/Theme/DesignTokens/LanguageDesignToken.cs`、`LanguageDesignTokenAttribute.cs`
- Modify: `src/AtomUI.Generator/DesignToken/TokenPropertyImplementationWriter.cs`、`TokenPropertyImplementationGenerator.cs`
- Test: `tests/AtomUI.Core.Tests/Theme/TokenFreezeTests.cs`、`TokenReadSetTests.cs`；`tests/AtomUI.Generator.Tests/`

**Interfaces:**
- Produces:
  - `public abstract class LanguageDesignToken : AbstractDesignToken`；`[LanguageDesignToken]` 标在语言的 Token 类上；语言 Token 的属性用现有 `[DesignTokenKind(DesignTokenKind.Seed)]` 标种子。
  - `AbstractDesignToken` 内部成员：`void Freeze(int slot)`、`bool IsFrozen(int slot)`、`void BeginReadSet(TokenReadSet set)`、`void EndReadSet()`。
  - 生成的属性实现：`set { if (!IsFrozen(slot)) field = value; }`；`get { NoteRead(slot); return field; }`，其中 `NoteRead` 在没有打开读集时只做一次空引用判断。
  - `CalculateTokenValues`、`CalculateAliasTokenValues`、`IsExplicit`、`RecordWrite` 全部删除。

验收：冻结的槽位写入被忽略、读取得到冻结值；读集准确记录被读的槽位；未打开读集时 getter 不分配。

### Task M2-2：语言契约与登记

**Files:**
- Create: `src/AtomUI.Core/Theme/Languages/DesignLanguage.cs`、`DesignLanguageAxes.cs`、`DesignVariant.cs`、`DesignStyle.cs`、`DesignDensity.cs`、`DesignIdiom.cs`、`DerivationContext.cs`、`IControlDerivation.cs`、`ControlDerivationAttribute.cs`
- Modify: `src/AtomUI.Core/Theme/Schema/ThemeSchemaRegistry.cs`；新增 `Schema/DesignLanguageDescriptor.cs`、`ControlDerivationDescriptor.cs`
- Delete: `src/AtomUI.Core/Theme/Algorithms/` 下的 `IThemeAlgorithm.cs`、`IThemeAliasAlgorithm.cs`、`IControlTokenAlgorithm.cs`、`ThemeAlgorithm*.cs`、`ControlTokenAlgorithmAttribute.cs`、`IThemeVariantCalculator*.cs`；`Schema/ThemeAlgorithmDescriptor.cs`、`ControlTokenAlgorithmDescriptor.cs`
- Modify: `src/AtomUI.Core/Theme/IThemeManagerBuilder.cs`、`ThemeManagerBuilder.cs`
- Test: `tests/AtomUI.Core.Tests/Theme/DesignLanguageRegistryTests.cs`、`ControlDerivationLookupTests.cs`

**Interfaces:**
- Produces（签名定稿于此，后续工作包按此使用）：

```csharp
public abstract class DesignLanguage
{
    public abstract string Id { get; }
    public abstract DesignLanguageAxes Axes { get; }
}

public abstract class DesignLanguage<TLanguageToken> : DesignLanguage
    where TLanguageToken : LanguageDesignToken, new()
{
    public abstract void DefineSeeds(in DesignVariant variant, DesignToken seed, TLanguageToken languageSeed);
    public abstract void DeriveLanguage(in DesignVariant variant, DesignToken seed, TLanguageToken language);
    public abstract void DeriveGlobal(in DerivationContext<TLanguageToken> context, DesignToken token);
}

public readonly record struct DesignStyle(string Id);
public readonly record struct DesignDensity(string Id)  { public static DesignDensity Default; public static DesignDensity Compact; }
public readonly record struct DesignIdiom(string Id)    { public static DesignIdiom Desktop;   public static DesignIdiom Mobile; }
public readonly record struct DesignVariant(DesignStyle Style, ThemeAppearance Appearance, DesignDensity Density, DesignIdiom Idiom);

public sealed record DesignLanguageAxes(
    IReadOnlyList<DesignStyle> Styles, DesignStyle DefaultStyle,
    IReadOnlyList<DesignDensity> Densities,
    IReadOnlyList<DesignIdiom> Idioms,
    IReadOnlyList<TokenDescriptor> AdjustableSeeds);

public readonly ref struct DerivationContext<TLanguageToken> where TLanguageToken : LanguageDesignToken
{
    public DesignVariant Variant { get; }
    public TLanguageToken Language { get; }
}

// 一个控件的视图（方案 6.1）：Token 是它的控件 Token；Global 是它看到的全局 Token，读得到该控件的值，
// 写入只对该控件生效，只允许 Map 与 Alias。
public sealed class ControlView<TToken> where TToken : AbstractControlDesignToken
{
    public TToken Token { get; }
    public DesignToken Global { get; }
}

public interface IControlDerivation<TLanguageToken, TToken>
    where TLanguageToken : LanguageDesignToken
    where TToken : AbstractControlDesignToken
{
    void Derive(in DerivationContext<TLanguageToken> context, ControlView<TToken> view);
}
```

  - 构建器入口：`IThemeManagerBuilder.AddDesignLanguage(DesignLanguageDescriptor)`、`AddControlDerivations(IEnumerable<ControlDerivationDescriptor>)`。
  - 查找：`ThemeSchemaRegistry.FindDerivation(DesignLanguageDescriptor language, ControlTokenDescriptor control)`，沿语言 Token 类型的继承链由具体到一般，到 `LanguageDesignToken` 为止。登记表是每种语言一张按控件槽位索引的数组。

验收：专属推导、基语言推导、根类型的通用推导三级查找各有测试；同一（语言 Token 类型，控件 Token 类型）重复登记报错。

### Task M2-3：单遍编译器

**Files:**
- Modify: `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs`（重写 `CompileGlobal`、`CompileControl` 及其辅助，删除算法链、控件阶段、`CopyBuilder` 的逐控件全局副本）
- Delete: `src/AtomUI.Core/Theme/Compilation/StagedTokenEvaluation.cs`
- Modify: `ThemeCompileInput.cs`、`ThemeCacheKey.cs`、`ControlCompilationCache.cs`、`ThemeSnapshot.cs`（加语言 Token 值表）
- Modify: `tests/AtomUI.Desktop.Controls.Tests/Theme/Performance/ThemeCompileMeasurement.cs`（改用新编译器入口，测量项与输出格式不变，供 G-性能 与 M0-7 的基线对照）
- Test: `tests/AtomUI.Core.Tests/Theme/SinglePassCompilerTests.cs`，并改写现有 `DenseThemeCompilerTests.cs`、`ControlOwnTokenExplicitOverrideTests.cs`、`GlobalAliasExplicitOverrideTests.cs`

**Interfaces:**
- Consumes: M2-1、M2-2 的全部产出。
- Produces: 管线按方案 6.2 的七步执行；控件快照相对全局的差量由控件推导写入的 `view.Global` 覆盖与控件级配置合成；`ThemeSnapshot.LanguageTokenValues`；缓存键包含（语言身份、变体、种子、显式值）。运行时发布层（`ThemeContext`、`ThemeScopeGraph`、`ThemeTransaction`、`LazyControlSnapshots`、资源解析）的公开行为不变。

验收：显式值冻结后跨类传递（语言 Token → 全局 → 控件 Token）；推导对 `view.Global` 的覆盖只出现在该控件的快照里，推导覆盖种子被拒绝，控件级显式值冻结后推导对它的写入被忽略；覆盖了种子或固定了轴的控件自成求值单元，其余控件共享全局对象（用引用相等断言）；每个推导在一次编译中恰好执行一次。

### Task M2-4：配置、选择与主题管理器

**Files:**
- Modify: `src/AtomUI.Core/Theme/Configuration/ThemeConfig.cs`、`ThemeConfigBuilder.cs`、`ControlThemeConfig.cs`、`ControlThemeConfigBuilder.cs`、`NormalizedThemeConfig.cs`、`ThemeConfigMerger.cs`、`ThemeConfigNormalizer.cs`
- Delete: `src/AtomUI.Core/Theme/Configuration/ControlAlgorithmMode.cs`
- Modify: `src/AtomUI.Core/Theme/ThemeSelection.cs`（重写）、`ThemeInfo.cs`、`ThemeRequest.cs`、`IThemeManager.cs`、`ThemeManager.cs`、`ThemeState.cs`、`ThemeDesignLanguageMetadata.cs`（删）
- Test: 改写 `ThemeConfigMergerTests.cs`、`ThemeConfigNormalizerTests.cs`、`ThemeSelectionResolverTests.cs`、`ThemeManagerTests.cs`、`ThemeFollowSystemTests.cs`；新增 `ThemeLanguageSwitchTests.cs`

**Interfaces:**
- Produces:
  - `ThemeConfig`：`Inherit`、`Language?`、`Style?`、`Appearance?`、`Density?`、`Idiom?`、`Tokens`、`LanguageTokens`、`Controls[identity] { Appearance?, Density?, Tokens }`。没有 `Algorithms`。
  - `ThemeSelection.Of<TLanguage>(DesignStyle? style = null)`、`.WithTheme(string id)`、`.WithAppearance(ThemeAppearance)`、`.FollowSystemAppearance()`、`.WithDensity(DesignDensity)`、`.WithSeed(Action<DesignToken>)`、`.WithLanguageSeed<TToken>(Action<TToken>)`。
  - `IThemeManager`：`IReadOnlyList<DesignLanguageInfo> Languages`、`IReadOnlyList<ThemeInfo> Themes`、`ThemeSelection CurrentSelection`、`Task<ThemeTransitionResult> ApplyAsync(ThemeSelection)`。

验收：方案 7.7 的轴确定与取值分层逐条有测试，其中「父暗色 + 子只写紧凑 = 暗色紧凑」必须有；**Review Focus 3**（切换语言时旧语言的语言 Token 覆盖不生效并产生诊断，切回后恢复）。

### Task M2-5：主题定义 XML v2 与编译主题 ABI v2

**Files:**
- Create: `docs/reference/theming/theme-definition-xml-v2.md`、`docs/reference/theming/schemas/atomui-theme-v2.xsd`、`docs/reference/theming/examples/acme-brand.theme.xml`、`docs/reference/theming/compiled-theme-abi-v2.md`
- Modify: `src/AtomUI.Core/Theme/Definitions/ThemeDocument.cs`、`ThemeDocumentReader.cs`、`ThemeDefinition.cs`、`ThemeDefinitionBinder.cs`、`CompiledThemeCatalog.cs`、`Definitions/Compiled/*.cs`
- Modify: `src/AtomUI.Generator/ThemeDefinitions/CompiledThemePackageWriter.cs`、`ThemeBuildContracts.cs`；`src/AtomUI.Toolchain/Tasks/GenerateCompiledThemeInputTask.cs`、`Common/ThemeDefinitions/CompiledThemeInput.cs`
- Create: `src/AtomUI.Toolchain/Tasks/ConvertThemeDefinitionTask.cs`（v1 → v2 换写命令）
- Delete: `docs/reference/theming/schemas/atomui-theme-v1.xsd`、`theme-definition-xml-v1.md`、`compiled-theme-abi-v1.md`（内容由 v2 文档取代）
- Test: 改写 `ThemeDocumentReaderTests.cs`、`ThemeDefinitionBinderTests.cs`、`CompiledThemeProgramAbiTests.cs`、`UserDirectoryThemeDefinitionResolverTests.cs`、`ThemeDocumentationExampleTests.cs`；新增 `ThemeDefinitionWhenTests.cs`、`ThemeDefinitionConversionTests.cs`

**Interfaces:**
- Consumes: M2-2 的语言登记（绑定 `Language`、`Style`、密度与形态的取值）、M2-4 的 `ThemeConfig` 模型。
- Produces: 方案 7.2–7.6 的文档结构（`Theme` 根属性、`Tokens`、`LanguageTokens`、`Controls`、`When`、`Locked`）；`ThemeDefinition` 携带基础段与按文档顺序的条件段，条件对求值单元的有效变体求值；换写命令 `dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<dir>`。

验收：方案 7.10 列出的每一条语义校验各有一个带行列与元素路径的诊断测试；v1 第 13 节的一致性验收逐条迁到 v2；**Review Focus 1**（用户目录里的 v1 文件：应用启动、文件不可用、诊断指出格式与换写命令）；**Review Focus 5**（换写命令对「控件上覆盖种子且 `Algorithm="Disabled"`」逐条提示且不改写；v2 下覆盖种子触发重新派生）。

### Task M2-6：生成器与校验

**Files:**
- Modify: `src/AtomUI.Generator/DesignToken/ControlThemeInfo.cs`、`ControlTokenInheritanceModel.cs`、`ControlTokenPropertyWalker.cs`、`TokenResourceKeyGenerator.cs`、`ThemeSchema/` 下的 Schema 生成
- Delete: `src/AtomUI.Generator/DesignToken/ControlTokenCalculationValidator.cs`、`OwnTokenDerivationBaseAnalyzer.cs`、`OwnTokenSentinelAnalyzer.cs`、`TokenCalculationScope.cs`
- Create: `src/AtomUI.Generator/Derivation/ControlDerivationRegistrationGenerator.cs`、`DerivationCompletenessAnalyzer.cs`、`TokenCoverageAnalyzer.cs`、`DesignLanguageRegistrationGenerator.cs`、`TokenSchemaDigestWriter.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`、`AtomUIDiagnosticDescriptors.cs`
- Modify: `build/AtomUI.Generator.props`、`build/AtomUI.Generator.targets`、`build/AtomUI.GeneratorConsumer.targets`
- Test: `tests/AtomUI.Generator.Tests/` 下每个新生成器与分析器各一组

**Interfaces:**
- Produces:
  - 控件包内生成（位置与命名空间不变）：Token 属性实现、资源键枚举、Schema、Token 标记扩展，以及程序集级特性 `[assembly: TokenSchemaDigest("<sha256>")]`。控件与 Token 类型的绑定方式沿用现状。
  - 语言包内生成：`AtomUI.Generated.<程序集标识>.GeneratedDesignLanguageRegistration.Register(IThemeManagerBuilder)`。
  - 取值包内生成：`GeneratedControlDerivationRegistration.Register(IThemeManagerBuilder)`，以及 `[assembly: CoversTokenSchema("<控件程序集名>", "<sha256>")]`。
  - 分析器：推导未在所有路径上写全 Token 属性 → 错误；读取了尚未写入的本 Token 属性 → 错误；对应控件包里有 Token 类没有本语言的推导 → 错误；语言包引用了任何控件包 → 错误；控件包引用了语言包或取值包 → 错误；取值包里出现控件类型的实例化、继承或 ControlTheme → 错误。

验收：每个分析器有正例与反例；**Review Focus 2**（启用控件包而缺取值包：配置提交失败，消息含缺失的包名）；**Review Focus 4**（`CoversTokenSchema` 的摘要与实际控件包不符：启动失败，消息含两个包名与两个摘要）。

**M2 门槛：** `AtomUI.Core`、`AtomUI.Generator`、`AtomUI.Toolchain` 及其测试工程构建并通过各自的单元测试。此时控件包尚未迁移，整个解决方案不要求可构建。停下评审。

---

## M3 Ant Design

目标：主仓各控件包的 Token 类清为纯声明，Ant Design 的全部规则迁入语言包与取值包；Gallery 在 Ant Design 的 4 个变体下与基线逐像素一致。

### Task M3-1：Token 类清为纯声明

**Files:**
- Modify: `src/AtomUI.Controls/**/*Token.cs`（3 个）、`src/AtomUI.Desktop.Controls/**/*Token.cs`（142 个）、`src/AtomUI.Desktop.Controls.ColorPicker/**/*Token.cs`（6 个）、`src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs`
- Create: `.artifacts/m3/formulas/<控件程序集名>/<TokenClass>.cs.txt`（产物，不进仓库）

Token 类不移动、不改名、不改命名空间，生成的枚举与标记扩展照旧在控件包内可用。

- [ ] 每个 Token 类只保留属性声明与文档注释。`CalculateTokenValues` 的方法体、它用到的私有辅助方法与常量，原样剪到上面的产物目录，供 M3-3 使用。
- [ ] 文档注释不足以让人只看注释就知道「画在哪里、表达什么状态、与哪个 Token 成对」的属性，补全注释（英文）。这是其他语言的取值包作者的唯一依据。
- [ ] 四个控件工程各自构建通过（此时还没有任何推导，运行不了，只要求编译）。

### Task M3-2：Ant Design 语言包

**Files:**
- Create: `src/DesignLanguages/AntDesign/AtomUI.DesignLanguages.AntDesign/AtomUI.DesignLanguages.AntDesign.csproj`、`AntDesignLanguage.cs`、`AntDesignToken.cs`、`AntDesignStyles.cs`、`Derivation/AntDesignSeeds.cs`、`Derivation/AntDesignColorMaps.cs`、`Derivation/AntDesignSizeMaps.cs`、`Derivation/AntDesignAlias.cs`、`Derivation/AntDesignCompact.cs`、`Derivation/AntDesignDark.cs`、`AntDesignBuilderExtensions.cs`
- Move: `src/AtomUI.Core/Theme/Algorithms/DefaultThemeVariantCalculator.cs`、`DarkThemeVariantCalculator.cs`、`CompactThemeVariantCalculator.cs`、`AbstractThemeVariantCalculator.cs`、`PresetPalettes.cs`、`PresetPrimaryColor.cs` 的规则部分 → 上述 `Derivation/` 文件
- Move: `src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.Alias.cs` 里 `CalculateAliasTokenValues` 的方法体 → `Derivation/AntDesignAlias.cs`
- Keep in Core（改名到 `src/AtomUI.Core/Theme/Derivation/`）：`PaletteGenerator.cs`、`ColorMap.cs`、`CalculatorUtils.cs` 里与语言无关的色彩数学
- Delete: `src/AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml`（其种子成为 `DefineSeeds` 的默认值）
- Test: `tests/AtomUI.DesignLanguages.AntDesign.Tests/`（新工程）

**Interfaces:**
- Consumes: M2-2 的语言契约。
- Produces: `public sealed class AntDesignLanguage : DesignLanguage<AntDesignToken>`，`Id = "AntDesign"`；轴：单一风格 `AntDesignStyles.Default`，密度 `Default`、`Compact`，形态 `Desktop`；可调种子 `ColorPrimary`。入口 `IAtomUIBuilder.UseAntDesign(Action<AntDesignLanguageBuilder>? configure = null)`。`DeriveGlobal` 内的顺序固定为：亮色或暗色的 Map（取决于 `Variant.Appearance`）→ 紧凑的尺寸改写（取决于 `Variant.Density`）→ Alias。这一顺序对应现有链 `[Default, Compact, Dark]` 与 `[Default, Dark]` 的求值结果，以 G-取值 为准。

- [ ] 契约测试：纯函数性、257 个全局 Token 全部写入、成对角色可读性，覆盖 4 个变体。
- [ ] 取值比对（只比全局 Token 行）：4 个 Ant 变体 × 种子矩阵，与基线逐项相等。

### Task M3-3：Ant Design 取值包

**Files:**
- Create: 四个取值包工程，各含一个注册入口：

  | 工程 | 引用 | 入口（`AntDesignLanguageBuilder` 上的扩展方法） |
  | --- | --- | --- |
  | `src/DesignLanguages/AntDesign/AtomUI.Controls.Themes.AntDesign/` | `AtomUI.Controls`、`AtomUI.DesignLanguages.AntDesign` | `AddSharedControls()` |
  | `src/DesignLanguages/AntDesign/AtomUI.Desktop.Controls.Themes.AntDesign/` | `AtomUI.Desktop.Controls`、`AtomUI.Controls.Themes.AntDesign` | `AddDesktopControls()`，内部先调用 `AddSharedControls()` |
  | `src/DesignLanguages/AntDesign/AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign/` | `AtomUI.Desktop.Controls.ColorPicker`、`AtomUI.Desktop.Controls.Themes.AntDesign` | `AddDesktopColorPicker()` |
  | `src/DesignLanguages/AntDesign/AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign/` | `AtomUI.Desktop.Controls.DataGrid`、`AtomUI.Desktop.Controls.Themes.AntDesign` | `AddDesktopDataGrid()` |

- Create: 每个取值包内 `<控件目录>/AntDesign<Token 类名去掉 Token 后缀>Derivation.cs`，每个 Token 类一个文件，目录与控件工程里的目录同名
- Test: `tests/AtomUI.Themes.AntDesign.Tests/`（新工程，覆盖四个取值包）

**Interfaces:**
- Consumes: M3-1 清理后的 Token 类与存下的公式、M3-2 的 `AntDesignToken`。
- Produces: 上表的四个入口。重复调用同一个入口是幂等的。跨端共用的抽象 Token 层（`AtomUI.Controls` 里的 `AbstractSkeletonToken` 等）的取值，由 `AtomUI.Controls.Themes.AntDesign` 以公开静态方法 `AntDesign<Base>Derivation.DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, <Base> token)` 提供，下游取值包调用（`ControlView<T>` 是不变泛型，共享推导接收视图的两个部分）。

每个 Token 类的推导由 M3-1 存下的公式按固定规则换写，不做任何取值上的改动：

| 原公式里的写法 | 推导里的写法 |
| --- | --- |
| `protected override void CalculateTokenValues(bool isDarkMode)` | `public void Derive(in DerivationContext<AntDesignToken> context, ControlView<XxxToken> view)`，方法体开头 `var token = view.Token; var global = view.Global;` |
| `EffectiveGlobalToken.P` | `global.P` |
| 对本类属性的写 `P = …` 与读 `P` | `token.P = …` 与 `token.P` |
| `isDarkMode` | `context.Variant.Appearance == ThemeAppearance.Dark` |
| `if (!IsExplicit(Kind.P)) P = v;` | `token.P = v;`（冻结语义已保证显式值不被覆盖） |
| `base.CalculateTokenValues(isDarkMode)` | 调用基类 Token 对应的共享推导方法 `AntDesign<Base>Derivation.DeriveShared(in context, global, token)` |

- [ ] 先做 `AtomUI.Controls.Themes.AntDesign`（3 个 Token 类），再做桌面控件。桌面控件按目录字母序分批，每批约 15 个 Token 类（批次清单由 `find src/AtomUI.Desktop.Controls -name '*Token.cs' -not -path '*/GeneratedFiles/*' | sort` 生成并写进本工作包的步骤级计划）。每批换写后构建，完整性分析器无报错，然后对该批控件跑取值比对（只比这些控件的行，4 个 Ant 变体 × 种子矩阵），逐项相等才进下一批。最后做 ColorPicker 与 DataGrid。
- [ ] 全部完成后，四个取值包的覆盖校验无报错：各自对应的控件包里的每个 Token 类都有且只有一个 Ant Design 推导。

### Task M3-4：Gallery、测试与工具接入

**Files:**
- Modify: `src/AtomUI.Toolkits.GalleryBase/**/*Token.cs`（8 个：改为声明）；Create `src/AtomUI.Toolkits.GalleryBase/Derivation/*.cs`（8 个绑定到 `LanguageDesignToken` 的通用推导，公式原样换写）
- Modify: `controlgallery/AtomUIGallery/App.axaml.cs`、`controlgallery/AtomUIGallery/Workspace/Views/GalleryThemeMenu.cs`、`Workspace/GalleryBrandSwatches.cs`、`src/AtomUI.Toolkits.GalleryBase/Shell/GalleryWorkspaceViewModel.cs`、`controlgallery/AtomUIGallery/ShowCases/General/ThemePresets/**`、`ShowCases/General/CustomizeTheme/**`
- Modify: 各测试工程的启动配置（`HeadlessTestApp.cs`、`AvaloniaTestApp.cs`）：`UseAntDesign(l => l.AddDesktopControls())`，用到 ColorPicker 或 DataGrid 的另加 `AddDesktopColorPicker()`、`AddDesktopDataGrid()`
- Modify: `tools/performances/AtomUI.GalleryPerformance/DesignLanguageVariants.cs`、`AtomUI.ControlBaseline/Program.cs`（变体切换与 `CreateRequest` 改用新的选择模型）；`tests/AtomUI.Themes.Tests/ThemeSnapshotDump.cs`（适配新的快照结构，输出格式不变）
- Modify: `tests/AtomUIGallery.Tests/Fixtures/*.theme.xml`（换写为 v2）

说明：Gallery 自己的 8 个 Token 类是画廊外壳，不属于产品控件，方案 6.1 的「第一方不得用通用推导」不适用于它；它们的公式只读全局 Token，本来就与语言无关。

- [ ] 两个工具的变体参数保持不变（仍接受 `AntDesign`、`PrimeNG.Aura` 这类写法），内部改用新的选择模型；导出格式不变。基线构建不动，比对照常进行。
- [ ] 解决方案里除两个旧语言包外的全部工程构建通过。

**M3 门槛：** Ant Design 的 4 个变体：G-取值 全等（含种子矩阵）、G-像素 为 0、G-性能 不退化、G-回归 通过（涉及 Shadcn 与 PrimeNG 的测试在此门槛标记为跳过，M4 恢复）。零例外。停下评审。

---

## M4 Shadcn 与 PrimeNG

目标：两种语言在新架构下恢复，44 个变体与基线逐像素一致。

两种语言各做同样的两个工作包，Shadcn 先做。

### Task M4-1 / M4-3：语言包（Shadcn / PrimeNG）

**Files（以 Shadcn 为例，PrimeNG 同构）:**
- Create: `src/DesignLanguages/Shadcn/AtomUI.DesignLanguages.Shadcn/AtomUI.DesignLanguages.Shadcn.csproj`、`ShadcnLanguage.cs`、`ShadcnToken.cs`、`ShadcnStyles.cs`、`ShadcnRecipes.cs`、`Derivation/ShadcnSeeds.cs`、`Derivation/ShadcnLanguageTokens.cs`、`Derivation/ShadcnGlobal.cs`、`Derivation/ShadcnPalette.cs`、`ShadcnBuilderExtensions.cs`、`README.md`
- Move: `src/AtomUI.Themes.Shadcn/Algorithms/ShadcnStyles.cs`、`ShadcnSurface.cs`、`ShadcnThemeColor.cs`、`ShadcnTailwindPalette.cs` 的内容 → 上述文件；`ShadcnStyleAlgorithm.cs` 的 `Evaluate` 与 `EvaluateAlias` → `Derivation/ShadcnGlobal.cs`
- Delete: `src/AtomUI.Themes.Shadcn/`（含 32 个主题 XML）
- Modify: `src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.Seed.cs`（删 `ColorNeutral`）、`tests/AtomUI.Core.Tests/Theme/ColorNeutralSeedTests.cs`（迁到 Shadcn 的测试工程）
- Test: `tests/AtomUI.DesignLanguages.Shadcn.Tests/`（由 `tests/AtomUI.Themes.Tests/Shadcn/` 迁来）

**Interfaces:**
- Produces（Shadcn）: `ShadcnLanguage : DesignLanguage<ShadcnToken>`，`Id = "Shadcn"`；风格 `ShadcnStyles.Vega / Nova / Maia / Lyra / Mira / Luma / Sera / Rhea`，默认 `Nova`；密度 `Default`、`Compact`；形态 `Desktop`；可调种子 `ColorPrimary` 与语言种子 `ShadcnToken.BaseColor`。`ShadcnToken` 的属性来自现有内部记录的一对一公开：`ShadcnSurface` 的 18 个角色色、`ShadcnStyle` 的全部几何字段、`ShadcnOverlay` 的全部字段、`ShadcnPrimary` 的 `Primary` 与 `Foreground`。入口 `UseShadcn(Action<ShadcnLanguageBuilder>?)`。
- Produces（PrimeNG）: `PrimeNGLanguage : DesignLanguage<PrimeNGToken>`，`Id = "PrimeNG"`；风格 `Aura`（默认）、`Lara`、`Nora`；`PrimeNGToken` 来自 `PrimeNGPreset`、`PrimeNGAppearanceRules`、`PrimeNGPalette`、`PrimeNGNoir` 的一对一公开；可调种子 `ColorPrimary`。

`DeriveGlobal` 的写法（保证与基线取值相同的关键）：现有实现是「先跑 Ant 的 Map 计算器，再覆盖；Ant 的 Alias 公式之后再抢写一遍」。新实现不得调用 Ant 语言包，做法是：

- [ ] 把 Ant 的 Map 与 Alias 推导代码**复制**一份进本语言包作为起点（复制时把它对种子的读取改成读本语言适配后的局部值，即现有代码临时改写种子的那四个值）；然后把本语言对 Map 的覆盖直接写在对应的赋值处；本语言对 Alias 的写入放在 Alias 公式里**首次读取该 Token 之前**，使依赖它的 Alias 值随之变化——这与现有的「标记显式后重跑默认公式」得到相同结果。
- [ ] 紧凑密度：现有紧凑变体是在本语言的 Map 之后接 Ant 的紧凑算法。把紧凑的尺寸改写同样复制进本语言包，按 `Variant.Density` 执行。
- [ ] 复制完成后，这些代码归本语言所有，此后与 Ant Design 各自演进；通用的色彩数学继续调用 `AtomUI.Core` 的 `Theme/Derivation`。
- [ ] 取值比对（只比全局 Token 行）：本语言的全部变体 × 种子矩阵，与基线逐项相等。`ColorNeutral` 一行写进 `tools/performances/design_language_token_renames.txt`，对应语言 Token 的 `BaseColor`。

### Task M4-2 / M4-4：取值包（Shadcn / PrimeNG）

**Files（以 Shadcn 为例，PrimeNG 同构）:**
- Create: 与 M3-3 同构的四个取值包工程：`src/DesignLanguages/Shadcn/AtomUI.Controls.Themes.Shadcn/`、`src/DesignLanguages/Shadcn/AtomUI.Desktop.Controls.Themes.Shadcn/`、`src/DesignLanguages/Shadcn/AtomUI.Desktop.Controls.ColorPicker.Themes.Shadcn/`、`src/DesignLanguages/Shadcn/AtomUI.Desktop.Controls.DataGrid.Themes.Shadcn/`，引用关系与 M3-3 的表相同（把 `AntDesign` 换成 `Shadcn`）；`AtomUI.Desktop.Controls.Themes.Shadcn` 带 `README.md`
- Create: 每个取值包内 `<控件目录>/Shadcn<Xxx>Derivation.cs`，每个 Token 类一个
- Source: `src/AtomUI.Themes.Shadcn/Algorithms/Controls/*.cs`（`EvaluateGlobal` 与 `EvaluateOwn`）、M1-2 清点表与 M3-3 的 Ant Design 推导（公式的起点）
- Test: `tests/AtomUI.Themes.Shadcn.Tests/`（覆盖四个取值包）

**Interfaces:**
- Produces: `ShadcnLanguageBuilder` 与 `PrimeNGLanguageBuilder` 上各有 `AddSharedControls()`、`AddDesktopControls()`、`AddDesktopColorPicker()`、`AddDesktopDataGrid()`，名字与 M3-3 相同。

每个 Token 类的推导按固定办法得到，使结果与「Ant 公式之后再由语言阶段覆盖」相同：

- [ ] **该 Token 类在现有语言包里没有阶段**（Shadcn 约 110 个、PrimeNG 约 70 个）：复制 Ant Design 的推导，泛型参数换成本语言的语言 Token 类型，方法体不变。
- [ ] **有阶段**：复制 Ant Design 的推导作为起点；把现有 `EvaluateOwn` 的每一条赋值并入——若被赋值的属性在 Ant 公式里后面还被读取（原来带 `IsExplicit` 守卫的那些），就在原赋值处直接换成本语言的值；否则追加在方法末尾。现有 `EvaluateGlobal` 的每一处改写，按 M1-2 清点表换写为方法开头对 `view.Global` 的覆盖；原先读改写值的地方（清点表的「EvaluateOwn 依赖」）改读 `view.Global`。其中读内部表的地方改读 `context.Language`。
- [ ] 分批与 M3-3 相同。每批跑取值比对：本语言的全部变体 × 种子矩阵，该批控件的行逐项相等。
- [ ] 覆盖校验与完整性分析器无报错。
- [ ] 把 `README.md` 里的映射表、不支持清单从旧包迁来，并为每个控件写上目标等级（方案 11.1）。

### Task M4-5：恢复与换写

**Files:**
- Modify: `AtomUI.slnx`、`scripts/NuGetPackageProjects.ps1`、`scripts/verification/package-layout-allowlist.json`
- Modify: `controlgallery/AtomUIGallery/App.axaml.cs`（`UseShadcn`、`UsePrimeNG`，各自 `AddDesktopControls().AddDesktopColorPicker().AddDesktopDataGrid()`）
- Delete: `src/AtomUI.Themes.PrimeNG/`、`tests/AtomUI.Themes.Tests/`（内容已分别迁入各语言的测试工程）
- Modify: `tools/performances/AtomUI.GalleryPerformance/DesignLanguageVariants.cs`、`AtomUI.ControlBaseline/Program.cs`（PrimeNG 与 Shadcn 的 44 个变体改用新的选择模型，参数不变）

- [ ] 44 个旧主题 XML 不再需要：风格、明暗、密度由选择给出。唯一写在 XML 里而不在算法里的取值是 Lara 两个紧凑 profile 的那一项 Select 覆盖；把它并入 PrimeNG 的 Select 推导，条件为「风格是 Lara 且密度是 Compact」。
- [ ] M3 门槛时跳过的测试全部恢复。

**M4 门槛：** 全部 48 个变体：G-取值 全等、G-像素 为 0（例外清单按 Global Constraints 逐条批准，Ant Design 的 4 个变体不得有例外）、G-性能 不退化、G-回归 通过。停下评审并分组提交。

---

## M5 主仓收尾

### Task M5-1：读集与结构共享、性能验收

**Files:**
- Modify: `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs`、`ControlCompilationCache.cs`、`ThemeCacheKey.cs`
- Test: `tests/AtomUI.Core.Tests/Theme/Performance/`、`ControlCompilationCacheTests.cs`

- [ ] 控件求值结果连同读集一起缓存；子求值单元相对父单元的变更集与读集不相交时复用父结果（引用相等）。
- [ ] 按方案 12.5 增加五个场景的测量，与 M0 的性能基线对照。读集带来的收益抵不过 getter 判断的开销时，按方案第 18 章待定点 7 退回「全局一变、带控件 Token 的控件全部重算」，并把测量数据写进进度文档。
- [ ] 启动：只启用 Ant Design 与启用三种语言只用一种，差值不超过现有启动门槛。超出时把语言包与取值包的程序集加载推迟到首次求值。

### Task M5-2：Gallery 的语言与变体选择

**Files:**
- Modify: `controlgallery/AtomUIGallery/Workspace/Views/GalleryThemeMenu.cs`、`src/AtomUI.Toolkits.GalleryBase/Shell/GalleryWorkspaceViewModel.cs`、相关本地化资源

- [ ] 语言、风格、明暗、密度、品牌色、Shadcn 的 Base Color 各一个菜单，全部由 `IThemeManager.Languages` 生成；明暗开关对所有语言都走 `WithAppearance`。这一步会使「非 Ant 语言下点全局暗色」从混链变成该语言自己的暗色，属于有意修正，不在 48 个变体的像素基线范围内。

### Task M5-3：对等性样例与打包

**Files:**
- Create: `tests/fixtures/Sample.DesignLanguage/`（只引用公开 API 的样例语言包）、`tests/fixtures/Sample.Controls/`（控件与 Token 声明）、`tests/fixtures/Sample.Controls.Themes.AntDesign/`、`tests/fixtures/Sample.Controls.Themes.Sample/`（样例控件在样例语言下的取值）
- Modify: `scripts/NuGetPackageProjects.ps1`、`scripts/verification/*`、`build/PackageValidation.props`

- [ ] 四个样例工程随解决方案构建，并有一个测试在无头应用里启用样例语言与样例控件、断言控件拿到样例语言给的值；另一个测试只启用 Ant Design，断言同一个控件拿到 Ant Design 取值包给的值。
- [ ] `scripts/BuildNuGetPackages.ps1` 产出全部新包；干净的 NuGet 缓存下，一个只引用 `AtomUI.Desktop.Controls` 与 `AtomUI.Desktop.Controls.Themes.AntDesign` 的应用可以构建并启动。

### Task M5-4：文档

**Files:**
- Rewrite: `docs/architecture/systems/theming/runtime.md` 第 2、4、6、7 节及其引用；`docs/architecture/systems/theming/compiled-theme-packages/*.md`
- Rewrite: `docs/guides/theming/customization.md`、`writing-theme-algorithms.md`（改名 `writing-design-languages.md`）、`preset-theme-packages.md`（改名 `design-language-packages.md`）、`third-party-control-packages.md`
- Rewrite: `docs/modules/themes/overview.md`
- Create: `docs/releases/<版本>-api-changes.md` 与 `.zh-CN.md`（由 `unreleased-theme-algorithms-api-changes*` 合并而来，逐项列出本次移除与替代）
- Move: 方案文档从 `docs/superpowers/specs/` 提炼为 `docs/architecture/systems/theming/design-languages.md`

**M5 门槛：** 四项门槛在最终的树上完整重跑一遍；文档里的每个代码示例由 `ThemeDocumentationExampleTests` 一类的测试编译并运行。停下评审并分组提交。主仓重构到此完成。

### Task M5-6：验收后删除指纹比对（项目负责人要求，2026-11-22）

**Files:**
- Modify: `tools/performances/AtomUI.GalleryPerformance/Program.cs`（`ApplyVariant`）

- [ ] M5 门槛验收通过后执行，不得提前。删除 `ApplyVariant` 中「用 `CreateRequest` 再应用一次并比对指纹」的部分，保留菜单命令切换；
  方法注释同步改写。此时 Gallery 菜单与各工具已用同一个选择接口，两条路径合为一条，比对没有对象。
- [ ] 构建 `AtomUI.GalleryPerformance`，以一个变体跑一次 `--audit --audit-showcases button --snapshot-dir <dir>`，期望退出码 0。
- [ ] 全仓搜索 `Fingerprint` 在 `tools/performances` 下不再出现于变体切换代码中。单独提交。

---

## M6 移动端（AtomUI.Mobile 仓库，另立计划）

前提：M5 的包以预发布版本发到本地源。

- 现状：65 个 Token 类为 `internal`，仍是旧契约；几何值写死在各控件的 `CalculateTokenValues` 里。
- 工作：65 个 Token 类改为 `public partial` 并清为纯声明，留在 `AtomUI.Mobile.Controls` 内；建立 `AtomUI.Mobile.Controls.Themes.AntDesign`（引用 `AtomUI.Mobile.Controls` 与 `AtomUI.Controls.Themes.AntDesign`），公式按 M3-3 的规则换写；Ant Design 语言包声明 `Mobile` 形态；Mobile Core 的界面根给出 `DesignIdiom.Mobile`。
- 门槛分两步：先以「Ant Design 语言包在 `Mobile` 形态下给出与 `Desktop` 相同的全局取值」完成等价迁移，移动端渲染与迁移前逐像素一致；再把写死的几何值与全局取值改为依据 Ant Design Mobile 官方，这一步的差异逐条列出并经批准。

## M7 商业产品（各产品仓库，各立计划，先 MachGrid）

- 现状（MachGrid）：Token 类为 `internal`、仍是旧契约；表头、行悬停、分组行共用同一个全局 Token，选中行只取了底色没取前景；另有自建的 `ThemePackages/BrandWarm` 机制。
- 工作：Token 类改为 `public partial` 并清为纯声明，留在产品的控件包内；按方案 4.2 补齐并存状态各自的控件 Token 与成对的前景；建立 `<产品>.Themes.AntDesign`（公式按 M3-3 的规则换写）；BrandWarm 改为具名主题或派生语言；再按产品规划建立其他语言的取值包。
- 门槛：Ant Design 下与迁移前逐像素一致；其他语言按各取值包 README 声明的等级验收。

---

## Self-Review

- **方案覆盖：** 第 3 章包结构 → M3-2、M3-3、M4；第 4 章词汇 → M1-1、M2-1、M3-1；第 5 章语言 → M2-2、M3-2、M4-1、M4-3；第 6 章推导与管线 → M2-2、M2-3；第 7 章主题定义与选择 → M2-4、M2-5；第 8 章注册与校验 → M2-6；第 9 章形态 → M2-2（类型）、M6（落地）；第 12 章性能 → M0-4、M5-1；第 13 章扩展点 → M5-3；第 14 章工具链 → M2-6（取值溯源与设计时支持未排入，见下）；第 16 章迁移 → 全部里程碑。
- **未排入的两项：** 方案 14 章的「取值溯源」与「设计时预览支持」不影响任何门槛，放在 M5 之后单独安排，不在本方案的完成定义之内。
- **类型一致性：** `DesignLanguage<TLanguageToken>`、`DerivationContext<TLanguageToken>`、`IControlDerivation<TLanguageToken, TToken>`、`ControlView<TToken>`、`DesignVariant`、`ThemeSelection`、`DesignLanguageProfile.Key` 在各工作包中的写法一致；`AddSharedControls()`、`AddDesktopControls()`、`AddDesktopColorPicker()`、`AddDesktopDataGrid()` 在三种语言的构建器上同名。
- **Review Focus：** 五条分别落在 M2-4（3）、M2-5（1、5）、M2-6（2、4）。
