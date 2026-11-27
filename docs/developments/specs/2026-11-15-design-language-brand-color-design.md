# 设计语言与品牌色分离：设计方案

日期：2026-11-15 · 状态：已批准（2026-11-15） · 方案：B（主题声明支持能力）

## 1. 意图

参照 primeng.dev 的主题配置器（Primary / Surface / Theme 三栏），让**品牌色**成为独立于**设计语言**的选择：

- 用户先选设计语言（Ant Design、PrimeNG Aura/Lara/Nora、Bootstrap、Material、Shadcn），再选品牌色。
- **每种设计语言各自记住自己的品牌色**：Aura 选靛蓝、切到 Lara 选玫红、再切回 Aura，仍是靛蓝。
- **设计语言不支持时不能换品牌色**：该语言保持自己的固定品牌色，品牌色选择器禁用并说明原因。

是否支持由设计语言的作者**显式声明**，而不是从 XML 里推断（用户选定方案 B）。

## 2. 目标、非目标、反目标

**目标**

1. 主题定义声明自己属于哪个设计语言、是否接受品牌色、是哪个外观/密度变体。
2. `ThemeInfo` 公开这三项，Gallery 与应用读同一个答案。
3. 目录加载时拒绝自相矛盾的声明（声明可换却手写了主色派生状态）。
4. Core 提供一个纯函数：把（设计语言、品牌色、明暗、紧凑）解析成 profile ID 和 `ThemeConfig` 覆盖。
5. Gallery 的 Theme 菜单与 Theme Presets 页改用上述声明与解析，去掉现有的启发式（名称后缀、`FixesPrimaryStates`）。

**非目标（本期不做）**

- Surface（中性色阶）独立选择。Shadcn 的 Zinc/Slate/Stone 本质上是 surface，留待后续按同一模式扩展。
- 品牌色记忆的持久化（存盘）。Core 不承担应用状态；Gallery 本期只在内存中记住。
- 为 Shadcn、Material 编写真正的品牌色算法（它们本期声明为不支持）。
- 设计语言自带"推荐色板"的 XML 语法。色板仍是界面层的事。

**反目标（出现即失败）**

- 某个声明"支持"的设计语言，换品牌色后悬停/按下/选中等派生状态没有跟着变，或暗色下颜色错误。
- 不支持的设计语言里，品牌色选择被静默忽略而界面看起来像已生效。
- 引入设计语言抽象层或改控件主题（参见 `design-language-branch-direction`：AtomUI 只做 Ant 体系内的主题定制）。本方案只增加主题元数据与一个解析函数。

## 3. 现状

| 设计语言 | 品牌色现状 | 来源 |
|---|---|---|
| Ant Design（`DaybreakBlue`，Core 内置） | 一个 `ColorPrimary` 种子，Ant 算法派生全部状态 | `src/AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml` |
| Ant 色彩预设（PolarGreen、SunsetOrange 等） | 只是不同的 `ColorPrimary` 种子；**仅 Gallery 资产** | `controlgallery/AtomUIGallery/Assets/Themes/*.theme.xml` |
| PrimeNG Aura/Lara/Nora（各 4 个 profile） | 种子映射到 Tailwind 色阶（500 命中手调表，否则走 PrimeUIX palette 公式），暗色取 400 | `src/AtomUI.Themes.PrimeNG` |
| Bootstrap（4 个 profile） | 种子 + Ant 算法；明暗 profile 用同一种子 `#0087FE` | `src/AtomUI.Themes.Bootstrap` |
| Material（4 个 profile） | 种子 + Ant 算法；**暗色 profile 换了种子**（`#D0BCFF`，M3 tone 80） | `src/AtomUI.Themes.Material` |
| Shadcn Zinc/Slate/Stone（各 4 个 profile） | 十个 `ColorPrimary*` 状态全部手写（近黑） | `src/AtomUI.Themes.Shadcn` |

相关机制：

- 主题元数据有两条入口，都要改：运行时 XML 读取 `ThemeDocumentReader`（`src/AtomUI.Core/Theme/Definitions/ThemeDocumentReader.cs`，`ReadTheme` 读 `Id/Name/Appearance/IsDefault`）→ `CompiledThemeCatalog` 生成 `ThemeInfo`；构建期由生成器 `CompiledThemePackageWriter` 写出 `CompiledThemeProgram`（构造参数 `id, name, appearance, isDefault, accentColor`）。
- `ThemeInfo`（`src/AtomUI.Core/Theme/ThemeInfo.cs`）现在只有 `Id, Name, Appearance, IsDefault, AccentColor`。
- `ThemeRequest(ThemeId, Config, Reason)`；`Config.Algorithms` 一旦指定就**替换** profile 自己的算法链（`ThemeConfigMerger.Apply`）。
- 目录诊断码格式 `ATMTHMnnnn`（例：`ATMTHM3002` InvalidTokenValue）。
- Gallery 现状：`GalleryWorkspaceViewModel` 靠 `.Dark/.Compact/.DarkCompact` 后缀判断 profile 家族（本日修复）；Theme Presets 页 `ThemePresetsViewModel` 用 `FixesPrimaryStates`（profile 是否手写 `ColorPrimary*`）决定能否选色，提供 5 个 Tailwind 500 色块；Theme 菜单按 ID 前缀分组（另一会话正在改的 `WorkspaceWindow.axaml.cs`）。

## 4. 方案比较

| 方案 | 做法 | 代价 / 风险 | 结论 |
|---|---|---|---|
| A. 从 XML 推断（仅 Gallery） | 沿用 `FixesPrimaryStates`：手写了 `ColorPrimary*` 状态即视为不支持 | 无新 API；但支持与否取决于文件"恰好"写了哪些 Token，将来任何一个 profile 为别的原因钉一个状态就会悄悄失去选色；应用无法复用 | 否 |
| **B. 主题声明支持能力** | 主题定义显式声明设计语言、品牌色能力、变体；目录校验矛盾；Core 提供解析纯函数；品牌记忆由调用方持有 | 需改 Core 元数据两条入口、所有主题包 XML、Gallery | **选定（用户确认）** |
| C. Core 完整选择服务 | 在 B 基础上增加 `IThemeSelectionService`，持有当前选择、各语言记忆并持久化 | Core 承担持久化与应用状态，所有应用被迫接受其存储方式 | 否（B 可在需要时演进为 C） |

## 5. 设计

### 5.1 主题定义的新属性（XML 与生成器同步）

```xml
<Theme Id="PrimeNGAura.DarkCompact" Name="PrimeNG Aura DarkCompact"
       Appearance="Dark" Density="Compact"
       DesignLanguage="PrimeNG.Aura" BrandColor="Seed" IsDefault="false">
```

`docs/reference/theming/schemas/atomui-theme-v1.xsd` 是严格校验的 schema（无 `anyAttribute`），三个属性要同步加入。

| 属性 | 取值 | 缺省 | 含义 |
|---|---|---|---|
| `DesignLanguage` | 标识符（同 `Id` 的字符规则），如 `AntDesign`、`PrimeNG.Aura` | 无（`null`） | 品牌色记忆与变体解析的分组键。缺省时该主题自成一组、不参与分组 |
| `BrandColor` | `Seed` / `Fixed` | `Fixed` | `Seed`：品牌色作为 `ColorPrimary` 种子，派生状态由算法计算；`Fixed`：品牌色由主题固定，不接受替换 |
| `Density` | `Normal` / `Compact` | `Normal` | 变体的密度，与已有的 `Appearance` 一起标识变体 |

`BrandColor` 缺省为 `Fixed` 是**我的决定**：现有用户主题目录里的 XML 没有这个属性，保守地视为不可换，作者显式 opt-in。

**品牌色的范围（我的决定）**：品牌色 = `ColorPrimary` 种子，与 antd `ConfigProvider` 的 `colorPrimary` 同义。`ColorLink` 是独立种子，**不**随品牌色变化（antd 中链接色也独立于主色）；只有设计语言自己的算法把链接色派生自主色时（PrimeNG）才会跟随。品牌派生的 Token 集合是：九个 `ColorPrimary*` 状态（Hover/Active/Bg/BgHover/Border/BorderHover/Text/TextHover/TextActive）和焦点光晕 `ColorControlOutline`。

### 5.2 `ThemeInfo` 扩展

```csharp
public enum ThemeDensity : byte { Normal, Compact }
public enum ThemeBrandColorSupport : byte { Fixed, Seed }

public sealed record ThemeInfo(string Id, string Name, ThemeAppearance Appearance, bool IsDefault)
{
    // 现有：AccentColor（主题自己的 ColorPrimary 种子，即其默认品牌色）
    public string? DesignLanguage { get; init; }
    public ThemeDensity Density { get; init; }
    public ThemeBrandColorSupport BrandColor { get; init; }
}
```

以 `init` 属性追加，不改变现有构造函数签名（公开 API 只增不改）。`CompiledThemeProgram` 增加对应构造参数，生成器写出新的 ABI 版本 2。

**旧编译包继续可用（我的决定）**：ABI 1 的包照常加载，三项取缺省（`DesignLanguage = null`、`Density = Normal`、`BrandColor = Fixed`）。这样 6.2.x 线上第三方编译包不受影响，不需要迁移文档；`CompiledThemeProgram` 同时接受 1 与 2。

### 5.3 目录校验（新增诊断）

加载（运行时 XML）与构建（生成器）都检查，同一规则：

| 码（建议） | 条件 | 级别 |
|---|---|---|
| `ATMTHM3101` | `BrandColor="Seed"` 但显式写了任一品牌派生 Token（5.1 的集合），无论在全局 `<Tokens>` 还是 `<Controls>` 下的控件级覆盖 | Error |
| `ATMTHM3102` | `BrandColor="Seed"` 但没有 `ColorPrimary` 种子，且其算法链也不提供默认种子 | Warning（实现者若发现所有算法都有默认种子，可删去此项） |
| `ATMTHM3103` | 同一 `DesignLanguage` 内有两个定义的 (`Appearance`, `Density`) 相同 | 见下 |
| `ATMTHM3104` | 同一 `DesignLanguage` 内各定义的 `BrandColor` 不一致 | 见下 |

码值是建议，实现者按现有编号空间落位；规则本身不可省。

**3101/3102 是单个定义的规则**：运行时与生成器都检查，级别相同。

**3103/3104 是跨定义的规则（我的决定）**：
- 生成器只看得到一个包，只在**包内**检查，Error。
- 运行时目录在合并所有来源（包、用户目录、重载）后检查，级别为 **Warning**，不让一个零散的用户 XML 阻止应用启动。冲突的那个定义（按加载顺序在后者：包先于用户目录）**退出分组**：视为 `DesignLanguage = null`，仍可按 Id 使用。
- 其他来源可以为已有设计语言**补充变体**（例如用户目录给 `AntDesign` 加一个 Dark profile），只要 (`Appearance`, `Density`) 不重复且 `BrandColor` 一致。

### 5.4 解析：Core 纯函数

```csharp
public sealed record ThemeSelection(
    string DesignLanguage, ThemeAppearance Appearance, ThemeDensity Density, Color? BrandColor);

public sealed record ThemeSelectionResult(
    string ThemeId,                       // 选中的 profile
    IReadOnlyList<ThemeAlgorithm>? Algorithms, // null = 用 profile 自己的链
    Color? BrandSeed,                     // 要写入 ColorPrimary 的种子；Fixed 或未选色时为 null
    bool BrandColorIgnored);              // 选了色但该设计语言是 Fixed

public static class ThemeSelectionResolver
{
    public static ThemeSelectionResult Resolve(IReadOnlyList<ThemeInfo> catalog, ThemeSelection selection);
    public static IReadOnlyList<string> DesignLanguages(IReadOnlyList<ThemeInfo> catalog);
}
```

规则：

1. 在 `catalog` 中找 `DesignLanguage` 相同且 (`Appearance`, `Density`) 精确匹配的定义 → `ThemeId` 为它，`Algorithms = null`。
2. 没有精确匹配：取该语言的 `Light`+`Normal` 定义为基础，`Algorithms = 基础定义自身的链 + [Dark?] + [Compact?]`（即 Ant Design 现在的做法：只有一个基础 profile，明暗紧凑靠内置算法）。基础定义的链由调用方从 `ThemeState`/目录取得；若 Core 无法在纯函数里拿到链，实现者可让 `Resolve` 返回"在基础链上追加"的标记，由 `ThemeManager` 展开 —— **这一点留给实现者**，约束是结果必须等价于"基础 profile + 内置 Dark/Compact"。
   - **修订（2026-11-15，终审 I1）**：语言没有 `Light`+`Normal` 定义时不抛异常，改为回退到该语言最近的已声明定义：先取与所选 `Appearance` 相同的第一个定义，再取与所选 `Density` 相同的第一个，最后取该语言的第一个定义（均按目录顺序），在它的链上同样去掉再追加内置 Dark/Compact。这样一个只声明了暗色变体的用户目录主题不会让调用方崩溃。
3. 找不到该语言 → 抛 `ArgumentException`（这是唯一的异常情形）。
4. 品牌：语言 `BrandColor == Seed` 且 `selection.BrandColor` 非空 → `BrandSeed = selection.BrandColor`；`Fixed` 且非空 → `BrandSeed = null, BrandColorIgnored = true`。

调用方用结果组 `ThemeRequest`：`ThemeId`、`Algorithms`（为 null 时不指定，保留 profile 链）、`ColorPrimary = BrandSeed`（为 null 时不写），再叠加自己的 Token（如动效开关）。解析器不读写任何状态。

### 5.5 各包的声明（本期）

| 包 / 定义 | `DesignLanguage` | `BrandColor` | 理由 |
|---|---|---|---|
| Core `DaybreakBlue` | `AntDesign` | `Seed` | Ant 算法完整派生；它显式写的 `ColorLink #1677FF` 是独立种子，不在品牌范围内，保留 |
| PrimeNG ×12 | `PrimeNG.Aura` / `PrimeNG.Lara` / `PrimeNG.Nora` | `Seed` | 种子映射到 Tailwind 色阶，明暗同一种子 |
| Bootstrap ×4 | `Bootstrap` | `Fixed` | 四个 profile 都手写了焦点光晕 `ColorControlOutline`（亮 `#6FC8FF`、暗 `#0064BA`），换品牌色后焦点环会停留在蓝色。改为由算法派生（会改变 Bootstrap 默认外观）后再声明 `Seed` |
| Material ×4 | `Material` | `Fixed` | 暗色 profile 需 M3 tone 80 的另一个种子，单一品牌色无法正确映射；待有 Material 算法再改 |
| Shadcn ×12 | `Shadcn.Zinc` / `Shadcn.Slate` / `Shadcn.Stone` | `Fixed` | 十个主色状态手写 |

以上为**我的决定**。Shadcn 的三族先各自成语言；引入 surface 轴后再合并为 `Shadcn` + surface。

### 5.6 Gallery

**Ant 色彩预设（用户决定，2026-11-15）**：PolarGreen、SunsetOrange、GoldenPurple、Magenta 四个 Gallery 主题文件删除（`controlgallery/AtomUIGallery/Assets/Themes/*.theme.xml` 及 `AtomUIGallery.csproj` 中的登记），改为 `AntDesign` 的命名品牌色块：Daybreak Blue `#1677FF`、Polar Green、Sunset Orange、Golden Purple、Magenta（色值取自被删文件的 `ColorPrimary`）。按 5.1 的品牌范围，选这些色块时链接色不再跟随（今天选 Polar Green 链接也变绿），这是已接受的行为变化。Gallery 中引用这几个 Id 的代码与测试（如 `SwitchThemeCommand.Execute("PolarGreen")`）改为切换 `AntDesign` 并选对应色块。

**放在哪里（我的决定）**：品牌记忆与基于解析器的切换放在 `AtomUI.Toolkits.GalleryBase` 的 `GalleryWorkspaceViewModel`（它已负责主题切换，本日的 profile 切换修复也在这里），新增的命令只增不改；菜单 UI 仍在 AtomUIGallery 的 `WorkspaceWindow.axaml.cs`。基于 GalleryBase 的其他 Gallery（如 Charts）因此一并获得该能力。

**没有设计语言的主题（我的决定）**：用户目录主题等 `DesignLanguage = null` 的定义放在菜单的 "Other themes" 分组（正在进行的菜单改动已有此组）。选中它们时品牌色选择器禁用，Appearance / Compact 沿用现在的做法（追加内置 Dark/Compact 算法）。

**显示名（我的决定）**：设计语言的显示名由 Gallery 的本地化资源按 `DesignLanguage` 标识**精确查表**（en-US、zh-CN、zh-TW、pt-BR），查不到时退回该语言基础定义的 `Name`。这是按键查表，不是按 Id 前缀猜测。

**品牌色记忆**：`GalleryWorkspaceViewModel` 持有 `Dictionary<string, Color?> _brandByLanguage`（键为 `DesignLanguage`），只在内存中。切换语言时取回该语言的记忆；未选过则为 `null`（用主题自己的 `AccentColor`）。

**色块**：一个通用色板（Tailwind 500 的 17 色 + 设计语言自身默认色）。Ant Design 额外显示上述 5 个命名色。选中项与 `AccentColor` 相同时视为"未选"（`null`），与 Theme Presets 页现有语义一致。

**Theme 菜单**（示意，最终布局与正在进行的分组菜单改动合并）：

```
Theme ▸ Design language ▸ ○ Ant Design   ○ PrimeNG Aura   ○ PrimeNG Lara   ○ PrimeNG Nora
                          ○ Bootstrap    ○ Material       ○ Shadcn Zinc    ○ Shadcn Slate  ○ Shadcn Stone
        Brand color     ▸ ■ ■ ■ ■ ■ ■ …   ↺ Reset
                          （Fixed 时整组禁用，悬停提示："此设计语言使用固定品牌色"）
        Appearance      ▸ Light / Dark / System
        ☐ Compact Mode
```

现有按 ID 前缀分组的菜单改为按 `DesignLanguage` 分组；`GalleryWorkspaceViewModel` 中按后缀找 profile 的逻辑（`GetProfileBaseId`、`FindProfileVariant`）改用 `ThemeSelectionResolver`。

**Theme Presets 页**：`FixesPrimaryStates` 换成读 `ThemeInfo.BrandColor`；预览配置改用解析器。页面是局部预览（`ThemeConfigProvider`），不写根主题，这一点不变。

## 6. 约束

- 只增不改公开 API：`ThemeInfo` 新增 `init` 属性；新增类型；已有 `ThemeRequest`/`ThemeConfig` 语义不变。
- 编译主题包与运行时 XML 两条入口行为一致（同一规则、同一诊断）。
- 不改任何控件主题或 Token 定义；不引入设计语言抽象层。
- `AccentColor` 保持现有语义（主题自身的 `ColorPrimary` 种子），作为"未选色"时的显示值。
- 代码内 summary 用英文（`agent-docs-standard-location`）；中文文档按仓库现有约定。
- Gallery 视觉验收用 `atom:Window` 宿主。

## 7. 验证

- Core：`ThemeDocumentReader`/生成器读写新属性；四条诊断各有通过与失败用例；`ThemeSelectionResolver` 覆盖精确匹配、回退到基础+内置算法、Fixed 忽略品牌、未知语言。
- 主题包：每个包的测试断言其声明（`DesignLanguage`、`BrandColor`、`Density`）；目录诊断为空。
- 品牌色效果：对每个 `Seed` 语言，换一个非默认品牌色后，明暗两种外观的 `ColorPrimary`、Hover/Active/Bg 派生值与 `ColorControlOutline` 都随之改变（PrimeNG 暗色取 400 档）。
- Gallery：语言间切换记住各自品牌色；Fixed 语言禁用选择器且根主题的 `ColorPrimary` 不变；现有 Gallery 测试（主题切换、Theme Presets）通过。

## 8. 前提与基线

- 在当前未提交的修复（Shadcn 阴影、Gallery 主题切换、ToggleSwitch、ButtonSpinner、PrimeNG 尺寸）提交到 `release/6.0` 之后开始。
- 另一会话正在修改 Bootstrap 包（4 个 XML、README、测试）、Gallery 分组菜单（`WorkspaceWindow.axaml.cs` 及其本地化）、主题文档和未发布的变更说明。本方案触及同样的文件，实施计划以这些改动落地后的状态为基线。
- 是否在独立分支/worktree 上实施，按 `docs/engineering/contributing/branch-and-worktree-guidelines.md` 在计划阶段向用户申请。

## 9. 已决定事项

- 方案 B（主题声明支持能力）— 用户选定。
- 设计语言不支持时不能换品牌色 — 用户提出。
- Ant 色彩预设转为 `AntDesign` 品牌色块并删除四个主题文件 — 用户选定（见 5.6）。

## 10. 留给实现者

- 生成器诊断与运行时诊断的具体编号与消息文本；XSD 的具体写法。
- `ThemeSelectionResolver` 回退情形下如何取得基础定义的算法链（见 5.4 第 2 条）。
- ABI 1/2 并存的具体实现方式（两个构造函数或版本分派）。
- Theme 菜单的最终布局，与正在进行的按设计语言分组菜单改动合并。
