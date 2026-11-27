# Shadcn 主题包对齐 shadcn/ui 现行标准 设计

日期：2026-11-15
状态：待审批
取值附录：[2026-11-15-shadcn-latest-alignment-data.md](2026-11-15-shadcn-latest-alignment-data.md)（脚本从官方数据生成，本文所有取值以附录为准）

## 1. 背景

### 1.1 用户反馈

在 Gallery 里切换 Shadcn Zinc / Slate / Stone 看不出差别，怀疑有 bug。

### 1.2 排查结论

- **切换本身没有问题。** `AtomUI.Themes.Tests` 中 Shadcn 的 16 个用例全部通过，三套主题的 `ColorPrimary` 等取值确实写入了。
- **看不出差别，是因为这三套都只是中性灰。** 三者主色都近黑：Zinc 和 Stone 的主色 RGB 单通道最多只差 4，边框、填充只差 3–12。官方 shadcn/ui 也是如此，Base Color 只决定灰阶的色调。
- **官方真正改变颜色的是另一个维度。** 是 Theme（17 种彩色主色），它和 Style（8 种控件几何）、Radius 一样，都是独立的维度，现有包里都没有。

### 1.3 之前为什么没对齐

1. **设计阶段没查官方源。** 2026-11-07 的方案（`2026-11-07-theme-customization-precision-design.md` 3.5 节）只写了一行"zinc / slate / stone 三色系，近黑主色"，凭的是旧版 new-york 时代的印象。PrimeNG 是逐项对照 `@primeuix` 源码做的；Shadcn 被归为"其余三个包各数天"，没有做源码调研。
2. **实现时主动冻结了版本。** 2026-11-10 的实现把取值钉在 Tailwind v3.4.17 色板上，README 明确写着"官方默认值会变，我们的取值固定不动"。
3. **规则出得晚，没回补。** "第三方主题以官网现行版本为准"这条规则，是后来做 PrimeNG 时才确立的，没有回头套到 Shadcn 上。
4. **测试抓不到偏差。** 测试只断言"XML 里的值等于我们自己的表"，抓不到和官方的偏差。

本方案按官网现行版本重做，并让测试以附录（官方数据）为基准。

## 2. 目标、非目标、反目标

**目标**

- 对齐 shadcn/ui 现行的四个维度：Style 8 种、Base Color 7 种、Theme（Neutral 加 17 种彩色）、Radius。取值来自官方 registry 与 `/create` 实测，记录在附录。
- 8 种 Style 都做，凡是 Token 能表达的都对齐；做不到的列入"不支持"清单，写进 README。
- **弹层的阴影与描边是重点验收项**（用户特别要求）：每种 Style、亮暗两种外观，菜单、Select、Popover、Tooltip、Dialog、Drawer 都要与官方一致。
- Gallery 里能分别选择 Style（设计语言）、Theme（品牌色）、Base Color（中性色），每种设计语言各自记住后两项的选择。

**非目标**

- 字体（Inter、Geist 等）与图标库（Lucide 等）：不属于主题 Token，本期不做。
- 官方的 Radius 档位选择（None / Small / Medium / Large）：本期只做各 Style 的默认圆角。Radius 选择可按 5.2 中性色轴同样的方式日后扩展。
- 官方的 Chart Color、Menu、Menu Accent 选项：本期取官方默认值（Menu Default / Solid，Menu Accent Subtle）。
- Command 面板、Sidebar 等 AtomUI 没有的组件。

**反目标（出现即失败）**

- 改变 Ant Design、PrimeNG、Bootstrap、Material 任何一个现有主题的渲染。
- 弹层阴影被弹层窗口裁切，或描边出现双线、横穿箭头底边。
- 出现"主题设置了，但没有效果"的 Token。
- 只靠"XML 等于我们自己的表"来测试：断言必须以附录数据为基准。

## 3. 官方现行结构（2026-11-15）

| 维度 | 作用 | 取值 |
|---|---|---|
| Style | 控件几何：高度、圆角、内边距、字号、焦点环宽度、弹层阴影级别 | Vega、**Nova**（默认）、Maia、Lyra、Mira、Luma、Sera、Rhea |
| Base Color | 中性色：background、foreground、card、popover、secondary、muted、accent、border、input、ring | **Neutral**（默认）、Stone、Zinc、Mauve、Olive、Mist、Taupe（Slate、Gray 已不在 `/create` 列表） |
| Theme | 只替换 `--primary` / `--primary-foreground`（以及 sidebar-primary，和 `--secondary`，见 6.3） | Neutral（= Base Color 自己的主色）+ Amber、Blue、Cyan、Emerald、Fuchsia、Green、Indigo、Lime、Orange、Pink、Purple、Red、Rose、Sky、Teal、Violet、Yellow |
| Radius | 基准 `--radius`，`sm` / `md` / `lg` / `xl` / `2xl` / `3xl` / `4xl` = 0.6 / 0.8 / 1 / 1.4 / 1.8 / 2.2 / 2.6 倍 | 默认 0.625rem = 10px |

**颜色格式。** 全部为 Tailwind v4 的 OKLCH。附录把它们换算成 hex，带透明度的写成 `#AARRGGBB`。

**颜色全部来自 Tailwind v4.3.5 色板**（npm 当前最新版，2026-11-15）。Mauve、Olive、Mist、Taupe 是 v4.3 新增的色阶；其余色阶在 4.1.11 到 4.3.5 之间没有变化，阴影定义也没有变化。shadcn 的颜色变量没有一个是自定义值，都是"某个色阶的某一档"。因此只要对齐 Tailwind 最新版色板，再加上 shadcn 的取档规则，颜色部分就完全确定：

- Base Color 的取档规则见附录 A 开头，已用脚本核对，七种全部命中；
- Theme 的取档规则见下一段。

**只对齐 Tailwind 还不够。** Style 的几何（高度、圆角倍数、内边距、字号）、每个组件用哪一级阴影、描边环和遮罩的浓度，都由 shadcn 组件源码决定，不在 Tailwind 里。这部分仍以 registry 为准，见附录 C。

**Theme 规则。** 已对 17 种逐项实测校验：

- 一般情况：浅色主色取 Tailwind v4 的 700 级，暗色取 800 级，前景取 50 级。
- Lime、Yellow 例外：浅色 400 级，暗色 500 级，前景 900 级。

## 4. 方案

### 4.1 三个维度落到 AtomUI 的方式（用户已选定）

| 方案 | 做法 | 取舍 |
|---|---|---|
| **A（选定）** | Style → 设计语言（8 个）；Theme → 品牌色（`BrandColor="Seed"`）；Base Color → 新增的中性色选择 | 设计语言菜单只有 8 项，色彩维度用色块选择。代价是 Core 要加一条与品牌色对称的中性色轴 |
| B | Style × Base Color 合并成设计语言（56 个） | 不改 Core，但菜单无法使用 |
| C | 只做 Nova 一种 Style | 工作量小，但不满足"8 种都尽可能对齐" |

### 4.2 中性色轴的实现方式（我的决定）

| 方案 | 做法 | 取舍 |
|---|---|---|
| **A（选定）** | 中性色作为**种子颜色**，写入一个**新增的可空 Seed Token `ColorNeutral`**，语义是"中性色阶的代表色"。Shadcn 取该 Base Color 浅色的 `--muted-foreground`，七种各不相同；Ant Design 等算法忽略它 | 与品牌色机制完全对称：都是"可选的颜色种子 + 设计语言声明是否支持"，沿用 `ThemeRequest` 的 Token 覆盖，不需要给算法传字符串参数；Gallery 色块天然适用，色块显示的就是种子本身 |
| B | 复用已有 Seed `ColorTextBase`（= `--foreground`） | **不可行**，有两个原因：Stone 与 Taupe 的 `--foreground` 完全相同（都是 `#0C0A09`，附录 A.1），无法区分；Ant 的暗色计算器直接读 `ColorTextBase` 作为文字基色（`DarkThemeVariantCalculator.cs:33`），浅色种子会让暗色派生出深色叠深色 |
| C | 中性色作为**名字**（如 `"Zinc"`），由 Core 传给算法 | 目前没有向算法传字符串参数的通道，需要新建；第三方设计语言也只能用名字表达，不能自定义颜色 |

## 5. 设计

### 5.1 设计语言与 profile

- **8 个设计语言：** `Shadcn.Vega`、`Shadcn.Nova`、`Shadcn.Maia`、`Shadcn.Lyra`、`Shadcn.Mira`、`Shadcn.Luma`、`Shadcn.Sera`、`Shadcn.Rhea`。
- **每个 4 个 profile**（沿用现有约定，共 32 个 XML）：
  - Id 形如 `ShadcnNova`、`ShadcnNova.Dark`、`ShadcnNova.Compact`、`ShadcnNova.DarkCompact`。
  - Name 形如 `Shadcn Nova`。
- **Compact 是 AtomUI 扩展**，官方没有对应：沿用 `AtomUI:Compact` 算法，在该 Style 的尺寸上再压缩一级。
- **所有 profile 声明** `BrandColor="Seed"`、`NeutralColor="Seed"`（新属性，见 5.2）。
- **XML 只写**算法链，以及默认种子 `ColorPrimary`、`ColorNeutral`（均取 Neutral 浅色值：`#171717`、`#737373`）。明暗 profile 写同一组种子，其余取值全部由算法给出。
  - 这符合 ATMTHM3101：Seed 语言不得在 XML 中显式写入品牌派生 Token。
- **设计语言顺序：** 按官方 `/create` 列表，Nova 是包内默认。整个应用的默认主题仍是 `DaybreakBlue`，这一点不变。
- **删除现有的 12 个 Zinc / Slate / Stone XML**：它们没有发布到 NuGet（README 中 "not published to NuGet"，Git 标签中也没有这个包），只需更新 Gallery 与文档引用，不需要迁移文档。仍要在未发布变更说明中记录 ID 变化。
  - **前提：** `scripts/NuGetPackageProjects.ps1:11` 已把 `AtomUI.Themes.Shadcn` 列为发布产品。如果本方案在下一次发布之后才合入，旧 ID 就已经发出去了，届时必须补迁移说明（旧 ID → `ShadcnVega` + Zinc / Stone 中性色；Slate 无对应，改用 Mist 或自定义中性色）。

### 5.2 Core：中性色轴（与品牌色对称）

参照 `2026-11-15-design-language-brand-color-design.md` 5.1–5.4 的品牌色实现，逐项对称添加。

| 位置 | 改动 |
|---|---|
| Seed Token | `DesignToken.Seed` 新增可空 `Color? ColorNeutral`（与 `ColorTextBase`、`ColorBgBase`、`ColorLink` 一样可空）。语义是"中性色阶的代表色"；缺省 null，Ant 默认、暗色、紧凑计算器都忽略它。资源键、生成器等随之产出，**Ant Design 的渲染不得改变** |
| XML 属性 | `NeutralColor="Seed\|Fixed"`，缺省 `Fixed`。`docs/reference/theming/schemas/atomui-theme-v1.xsd` 同步加入 |
| `ThemeDocumentReader` / 生成器 `CompiledThemePackageWriter` | 读写新属性与新 Seed。ABI 处理同品牌色：旧 ABI 的包照常加载，新属性取缺省值 `Fixed` |
| `ThemeInfo` | 新增 `NeutralColor`（类型 `ThemeNeutralColorSupport`，或复用 `ThemeBrandColorSupport` 并改名为通用的颜色种子支持枚举，**实现者定**，需保持公开 API 清晰）；另新增 `NeutralAccentColor`（定义自己的 `ColorNeutral` 种子，没有则为 null），与现有 `AccentColor`（定义自己的 `ColorPrimary` 种子）对称。Gallery 靠它标出当前选中项、实现"重置"（`GalleryThemeMenu.cs:168` 对品牌色就是这么做的） |
| `ThemeSelection` | 新增 `Color? NeutralColor` |
| `ThemeSelectionResult` | 新增 `Color? NeutralSeed` 与 `bool NeutralColorIgnored`，规则同品牌色：语言为 Seed 时传出种子，为 Fixed 时忽略并置标志 |
| 调用方 | 组 `ThemeRequest` 时把 `NeutralSeed` 写进 `ColorNeutral`（为 null 时不写） |
| 诊断 | 加两条，与品牌色已有规则对称：`NeutralColor="Seed"` 的定义不得显式写中性派生 Token（集合即 5.4 表中所有由 Base Color 决定的 AtomUI Token）；同一设计语言内各定义的 `NeutralColor` 必须一致。`NeutralColor="Seed"` 但既没有 `ColorNeutral` 种子、算法也不提供缺省时报警告，同 ATMTHM3102。码值在现有编号空间内落位 |

**语义。** 中性色种子 = 该 Base Color 浅色下的 `--muted-foreground`，即对应 Tailwind 中性色阶的 500 级。七个值互不相同（附录 A.1 `--muted-foreground` 一行），也正好是这组灰阶里最能看出色调的一档，可以直接拿来当色块颜色。

- 只有声明了 Seed 的设计语言才接受中性色种子。
- Ant Design 等现有语言不声明。即使有人写了 `ColorNeutral`，它们的算法也会忽略。

### 5.3 `AtomUI.Themes.Shadcn` 算法结构（参照 PrimeNG 包）

PrimeNG 包已有同类结构：`PrimeNGPresetAlgorithm`、`PrimeNGPalette`、`PrimeNGTailwindPalettes`、`PrimeNGNoir`、`Algorithms/Controls/*`。Shadcn 包按同样分层组织：

```text
AtomUI.Themes.Shadcn/
+-- Themes/Shadcn<Style>[.Dark|.Compact|.DarkCompact].theme.xml   32 个，只含算法链与默认种子
+-- Algorithms/
|   +-- ShadcnStyleAlgorithm.cs        共享实现：Map 阶段先跑 Ant 默认/暗色计算器，再写颜色与尺寸；Alias 阶段写焦点、阴影、遮罩等
|   +-- Shadcn<Style>Algorithms.cs     8 个 Style 的浅/暗算法注册（同 PrimeNGAuraAlgorithms 模式）
|   +-- ShadcnStyles.cs                8 个 Style 的几何表（5.5）
|   +-- ShadcnTailwindPalette.cs       Tailwind v4.3.5 全部色阶（7 个中性 + 17 个彩色 + red，各 50–950 共 11 档，hex）
|   +-- ShadcnBaseColors.cs            附录 A 开头的取档规则：由中性色阶生成 Base Color 的浅/暗变量（不存 7 张表）
|   +-- ShadcnThemeColor.cs            由 ColorPrimary 种子解析出浅/暗主色与前景（5.3.2）
|   +-- Controls/Shadcn<Control>Tokens.cs   控件阶段（5.4、5.6 中需要控件级取值的项）
```

#### 5.3.0 分层：种子只在 Map 阶段可见

以下几点已核对代码：

- 控件阶段只拿到计算后的 Token：`IControlTokenAlgorithm.EvaluateGlobal(context, controlGlobal)` 与 `EvaluateOwn(context, effectiveGlobal, token)`（`src/AtomUI.Core/Theme/Algorithms/IControlTokenAlgorithm.cs:46,54`）。
- `ThemeAlgorithmContext` 里只有外观和算法链（`ThemeAlgorithmContext.cs:26,31`）。
- PrimeNG 也只能从计算后的 `ColorPrimary` 反推 noir（`PrimeNGNoir.cs:144-155`）。

因此规定如下：

1. **Map 阶段一次性解析。**
   - 从 `effectiveSeed` 读出 `ColorNeutral` 与 `ColorPrimary`，解析出 Base Color 与 Theme（5.3.1、5.3.2）。
   - 把**所有**由 Base Color 或 Theme 决定的全局 Map Token 写完。
   - 原样把 `ColorNeutral` 写进 `nextMap`；为 null 时写入 Neutral 的种子 `#737373`。
2. **调用 Ant 计算器前先换种子。**
   - Map 阶段先跑 Ant 默认或暗色计算器，以便所有 Map Token 都有值。
   - 调用前，用一份**副本种子**把 `ColorTextBase` 设为当前外观下该 Base Color 的 `--foreground`、`ColorBgBase` 设为 `--background`（暗色即暗色值）。
   - 这样 Ant 派生出的中性 Token 至少明暗方向正确；随后再由 5.4 的表覆盖。
   - `DarkThemeVariantCalculator.cs:33` 直接读 `ColorTextBase`，不这样做，暗色会派生出深色叠深色。
3. **Alias 阶段与控件阶段**只读计算后的 `ColorNeutral` 与 `ColorPrimary`，重新查表得到 Base Color 与 Theme，用于焦点 ring、阴影环颜色、输入框 `--input` 等。
4. **测试要求：**
   - 对每个 Base Color、明暗两种外观、Theme Neutral 与一种彩色，断言 Alias 阶段与控件阶段解析出的 Base Color 与 Map 阶段一致；
   - 种子能经过整条算法链（含 `AtomUI:Compact` 与 `AtomUI:Dark` 追加的情形）原样到达后续阶段。

#### 5.3.1 Base Color 识别（`ColorNeutral` 种子）

- 种子的 RGB 等于 7 个中性色阶之一的 500 级：按附录 A 开头的取档规则，从该色阶生成浅/暗变量。生成结果必须逐项等于附录 A 的表，由测试保证。
- 未命中（用户自定义中性色）：把种子当作 500 级，生成 50–950 共 11 档的色阶，再按同一套取档规则取值。生成方式由实现者定，约束有三条：
  - 明度阶梯与 Neutral 表相同；
  - 色相取种子色相；
  - 色度按 Zinc 表各档的色度比例缩放，以种子的色度相对 Zinc `--muted-foreground` 色度（0.016）的倍数为准。

  同时要写单测，证明七个官方种子经过这条规则后不会被误判。
- 种子为 null（XML 缺省值也是 Neutral）：使用 Neutral。

#### 5.3.2 Theme 识别（`ColorPrimary` 种子）

- **种子等于当前 Base Color 的浅色 `--primary`**（或任一 Base Color 的 `--primary`）时，视为 Theme = Neutral：主色与前景取 Base Color 表，与官方 `/create` 选 Neutral 的效果一致。这一做法同 PrimeNG 的 noir 判定。
- **种子 RGB 等于某个 Tailwind v4 彩色色阶的 700 级**（Lime、Yellow 为 400 级）时，视为官方 Theme：
  - 浅色：主色取种子；
  - 暗色：主色取 800 级（Lime、Yellow 取 500 级）；
  - 前景：取 50 级（Lime、Yellow 取 900 级）。
- **其余种子**：浅色主色 = 种子；暗色主色与前景由实现者按以下约束派生：
  - 暗色主色在 OKLCH 中降低明度，降幅取 17 个官方 Theme 浅→暗明度差的平均值，色度按同样方式缩放；
  - 前景在"同色相、高明度、低色度"与"同色相、低明度"两者中，选对比度 ≥ 4.5 的那个，优先前者。
- **Gallery 色块**写入的是 Theme 的浅色主色（700 / 400 级），因此在明暗两种外观下都能命中官方值。

#### 5.3.3 品牌派生 Token

品牌色设计 5.1 规定的集合是：九个 `ColorPrimary*` 加 `ColorControlOutline`。shadcn 的取法：

| AtomUI Token | shadcn 来源 |
|---|---|
| `ColorPrimary` | `--primary` |
| `ColorPrimaryHover` | `hover:bg-primary/80`：主色 80% 不透明度合成到 `--background` 上 |
| `ColorPrimaryActive` | shadcn 无单独按下色（按下是位移）：取与 Hover 相同 |
| `ColorPrimaryBg` / `BgHover` | 主色 10% / 20% 合成到 background 上（shadcn 无对应变量，属于近似；用于 AtomUI 的浅色主色底） |
| `ColorPrimaryBorder` / `BorderHover` | 主色 30% / 50% 合成（近似，同上） |
| `ColorPrimaryText` / `TextHover` / `TextActive` | `--primary` / Hover / Active |
| `ColorTextLightSolid` | `--primary-foreground` |
| `ColorControlOutline` | `--ring` 加各 Style 的焦点环透明度（`ring-ring/50` 或 `/30`）。它不随品牌色变，这与官方一致：ring 取中性色 |

标注"近似"的四项，在官方组件中没有直接对应。实现者可以按 Gallery 实测调整比例，但必须写进 README 的映射表。

### 5.4 颜色 Token 映射（Base Color 派生）

| shadcn 变量 | AtomUI Token | 说明 |
|---|---|---|
| `--background` | `ColorBgLayout`、`ColorBgContainer`、`ColorBgBase` | 页面与控件底色。浅色都是白色。旧包把 Layout 设成 50 档，与官方不符 |
| `--card` | `Card` 控件级 `ColorBgContainer` | 暗色比 background 亮一档 |
| `--popover` | `ColorBgElevated` | 所有弹层表面 |
| `--foreground` | `ColorText`、`ColorTextHeading`、`ColorIcon`、`ColorIconHover` | `ColorTextBase` 只在调用 Ant 计算器时作为副本种子使用（5.3.0），不作为 Base Color 的标识 |
| `--foreground`（Tooltip 反色） | `ColorBgSpotlight`；Tooltip 文字色取 `--background` | 同时用于 5.6 的 Tooltip |
| `--primary` / `--foreground` | `ColorBgSolid`、`ColorBgSolidHover`、`ColorBgSolidActive` | Ant 的实色底（Solid 按钮等），取 foreground 与其 80% 合成 |
| `--muted-foreground` | `ColorTextSecondary`、`ColorTextDescription`、`ColorTextPlaceholder` | |
| `--muted-foreground` 50% | `ColorTextTertiary`、`ColorTextQuaternary`、`ColorTextDisabled` | shadcn 的禁用是整体半透明，这里用颜色近似（见 5.7） |
| `--muted` | `ColorFillTertiary`、`ColorFillContent`、`ColorBgContainerDisabled`、`ColorFillQuaternary` | |
| `--secondary` | `ColorFillSecondary` | Secondary 按钮底色 |
| `--accent` | `ControlItemBgHover`、`ControlItemBgActive`、`ColorFill` | 菜单与列表的悬停、选中 |
| `--accent-foreground` | `ControlItemColorActive` | |
| `--border` | `ColorBorder`、`ColorBorderSecondary`、`ColorSplit` | 暗色为 `#1AFFFFFF` |
| `--input` | 输入类控件的边框（控件级 `ColorBorder`） | 暗色为 `#26FFFFFF`，与 border 不同 |
| `--ring` | 输入类聚焦边框色、`ColorControlOutline` 底色 | Ant 的聚焦边框是主色，shadcn 是 ring：需要输入类控件级覆盖 |
| `--destructive` | `ColorError`（及其 Ant 派生） | 暗色值不同 |
| 无 | `ColorSuccess`、`ColorWarning`、`ColorInfo` | shadcn 不定义，沿用 AtomUI 算法（与旧包一致） |

**完整性要求。** 这张表列出了已知的项，但不是全部。实现者要枚举 `DesignToken` 中所有颜色类的 Map / Alias Token，例如 `ColorTextQuaternary`、`ColorFillContent`、`ColorBgSpotlight`、`ColorBgSolid`、`ColorIcon`、`ColorHighlight`、`ColorBgTextHover`、`ColorBgTextActive`、`ColorErrorBg` 系列。

- 每一项要么写进本表并由 Shadcn 算法覆盖，要么在 README 中说明"沿用 Ant 派生"及其原因。
- 写一条测试：每个 Base Color 在暗色下，所有中性颜色 Token 与 `ColorBgContainer` 的对比方向正确，不得出现深色叠深色。

**暗色下的透明色。** `--border` 与 `--input` 在暗色下是半透明白，AtomUI 颜色支持 alpha，直接使用即可。实现者要在 Gallery 实测：半透明边框叠在 `--card` 或 `--popover` 上时，效果与官方一致（官方也是半透明叠加）。

### 5.5 Style 几何

**换算约定。** radius = 10px，各档 sm / md / lg / xl / 2xl / 3xl / 4xl = 6 / 8 / 10 / 14 / 18 / 22 / 26。数值单位为 px，原始 class 见附录 C。

| Style | 控件高 默认 / 小 / 大 / 迷你 | 按钮 / 输入圆角 | 水平内边距 | 字号 / 字重 | 焦点环 | Card | Dialog |
|---|---|---|---|---|---|---|---|
| Vega | 36 / 32 / 40 / 24 | 8 / 8 | 10 | 14 / 500 | 3px, ring 50% | 圆角 14，内边距 24，`shadow-xs` + 环 10% | 圆角 14，内边距 24 |
| Nova | 32 / 28 / 36 / 24 | 10 / 10（小号 8） | 10 | 14（小号 12.8）/ 500 | 3px, 50% | 14，16，环 10% | 14，16 |
| Maia | 36 / 32 / 40 / 24 | 26（胶囊）/ 26 | 12（大 16） | 14 / 500 | 3px, 50% | 18，24，环 10% | 26，24 |
| Lyra | 32 / 28 / 36 / 24 | 0 / 0 | 10 | 12 / 500 | 1px, 50% | 0，16，环 10% | 0，16 |
| Mira | 28 / 24 / 32 / 20 | 8 / 8 | 8（大 10） | 12，行高 1.625 / 500 | 2px, 30% | 10，16，环 10% | 14，16 |
| Luma | 36 / 32 / 40 / 24 | 26 / 22 | 12（大 16） | 14 / 500 | 3px, 30% | 26，24，`shadow-md` + 环 5% | 26，24 |
| Sera | 40 / 36 / 44 / 28 | 0 / 0（输入为下划线） | 24（小 16，迷你 12，大 32） | 12 / 600，大写，字距 0.1em | 2px, 30% | 0，32，`shadow-sm` + 环 5% | 0，24 |
| Rhea | 32 / 28 / 36 / 24 | 18 / 18 | 12（大 16） | 14 / 500 | 3px, 30% | 24，20，`shadow-sm` + 环 5% | 24，24 |

**输入框底色**（控件级）：

| Style | 底色 |
|---|---|
| Vega / Nova / Lyra | 浅色透明，暗色 `--input` 30% |
| Maia | `--input` 30% |
| Mira | `--input` 20% |
| Luma / Rhea | `--input` 50%，边框透明 |

Checkbox、Switch、Tabs、Badge、Slider、Progress、Toggle、Table、Calendar、Textarea、Alert 的取值同样以附录 C 为准。实现者逐项映射到对应控件 Token；没有对应 Token 的项列入 5.7。

### 5.6 弹层（重点）

#### 5.6.1 官方取值

**阴影定义**（线上 CSS 原文，AtomUI 写法）：

| 级别 | 取值 |
|---|---|
| `shadow-xs` | `0 1 2 0 #0D000000` |
| `shadow-sm` | `0 1 3 0 #1A000000, 0 1 2 -1 #1A000000` |
| `shadow-md` | `0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000` |
| `shadow-lg` | `0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000` |
| `shadow-xl` | `0 20 25 -5 #1A000000, 0 8 10 -6 #1A000000` |
| `shadow-2xl` | `0 25 50 -12 #40000000` |

**描边环。**

- 官方写法是 `ring-1 ring-foreground/10`，即 `0 0 0 1 <foreground 的 10% 透明色>`，例如浅色 Neutral 为 `#1A0A0A0A`、暗色为 `#1AFAFAFA`。
- `/5` 档为 `#0D` 透明度。
- 环必须排在阴影**之前**。CSS 中环叠在阴影之上，AtomUI 的 `BoxShadows` 同样按"先写的在上"绘制，与现有 Shadcn / PrimeNG 预设的写法一致。

| Style | 菜单 / Select / ContextMenu | 子菜单 | Popover | Tooltip | Dialog | Drawer（Sheet） | 遮罩 |
|---|---|---|---|---|---|---|---|
| Vega | md + 环 10%，圆角 8，内边距 4 | lg | md + 环 10%，圆角 8，内边距 16 | 反色，无阴影无环，圆角 8 | **无阴影** + 环 10% | lg + 贴边一侧 1px 边框 | 黑 10% |
| Nova | md + 环 10%，圆角 10，内边距 4 | lg | md + 环 10%，圆角 10，内边距 10 | 同上，圆角 8 | **无阴影** + 环 10% | lg | 黑 10% |
| Maia | **2xl** + 环 5%（暗 10%），圆角 18 | 2xl（ContextMenu 为 lg） | 2xl + 环 5%，圆角 18，内边距 16 | 圆角 18 | 无阴影 + 环 5% | lg | 黑 80% |
| Lyra | md + 环 10%，圆角 0，无内边距 | lg | md + 环 10%，圆角 0 | 圆角 0，箭头直角 | 无阴影 + 环 10% | lg | 黑 10% |
| Mira | md + 环 10%，圆角 10 | md（ContextMenu 为 lg） | md + 环 10%，圆角 10 | 圆角 8 | 无阴影 + 环 10% | lg | 黑 80% |
| Luma | **lg** + 环 5%（暗 10%），圆角 22，内边距 6 | lg | lg + 环 5%，圆角 22 | 圆角 14 | **xl** + 环 5% | **xl** | 黑 30% |
| Sera | md + 环 10%，圆角 0，内边距 6 | md | md + 环 10%，圆角 0 | 圆角 0，箭头直角 | **md** + 环 10% | **md** | 黑 20% |
| Rhea | lg + 环 5%（暗 10%），圆角 18 | lg | lg + 环 5%，圆角 22 | 圆角 14 | **xl** + 环 5% | **xl** | 黑 30% |

- **Tooltip：**
  - 底色为 `--foreground`，文字为 `--background`，暗色下是白底深字；
  - 内边距 12×6，12px 字号；
  - 箭头是 10px 方块旋转 45°，圆角 2（Lyra / Sera 为 0），与框体同色。
- **Popover 官方没有箭头。** AtomUI 的箭头是控件属性 `IsArrowVisible`，主题无法关闭（见 5.7）。显示箭头时，用 `ArrowDecoratedBox` 的框体与箭头一体描边：
  - `ContentBorderColor` 为环色；
  - `ContentBorderThickness` 为 1；
  - 弹层不再画描边环（现有机制，见 `docs/controls/desktop/other/popup/token.md`）。
- **遮罩模糊**（`backdrop-blur-xs` / `sm`）做不到，只对齐 `ColorBgMask`。

#### 5.6.2 落到 AtomUI 的 Token

| 弹层 | AtomUI Token |
|---|---|
| 菜单、Select、Cascader、ComboBox、AutoComplete、Mentions（`PopupRoot` / overlay 路径） | `Controls[Popup]` 的 `PopupRootShadow` / `OverlayHostShadow` = 环 + 阴影 |
| 带箭头的 Flyout、Popover、PopupConfirm、Picker 面板、带箭头的下拉菜单 | `FlyoutHost` 的 `PopupRootShadow` / `OverlayHostShadow` = 仅阴影；`ArrowDecoratedBox` 的 `ContentBorderColor` / `ContentBorderThickness` = 环色 / 1 |
| Dialog | `Controls[Dialog]` 的 `BoxShadows` = 环（+ 阴影，按 Style） |
| Drawer | `DrawerToken` 的 `BoxShadowDrawerLeft/Right/Up/Down` = 阴影；贴边一侧 1px 边框，**实现者确认** Drawer 是否已有边框 Token，没有则列入 5.8 |
| Message、Notification | `Controls[Message]` / `Controls[Notification]` 的 `BoxShadows`：官方 Toast 由 sonner 绘制，registry 中无 class。**实现者在官网实测** Toast 的阴影与边框后再定，未实测前沿用 Popover 档 |
| 遮罩 | `ColorBgMask` |

AtomUI 弹层窗口的尺寸会随阴影变化，所以 `shadow-2xl` 不会被裁切（见 popup token 文档 3 节）。验收时仍需逐项确认。

#### 5.6.3 现有包中必须纠正的三处

1. Dialog 现在用的是 `shadow-lg`；Vega / Nova / Maia / Lyra / Mira 应为无阴影、只有环。
2. 带箭头的弹层现在没有任何边框（README 写着"保留纯阴影"），应改为一体描边。
3. 环色现在是实色的 `ColorBorder`，应改为 foreground 的 10% / 5%，暗色单独取值。

### 5.7 Token 做不到（写进 README 的"不支持"）

| 效果 | 受影响 Style | 近似处理 |
|---|---|---|
| 全大写与加宽字距 | Sera（按钮、菜单项、Tabs、Card / Dialog 标题） | 只对齐字号与字重 |
| 只有下划线的输入框 | Sera（Input、Select） | 保留四边细边框 |
| 焦点外环（边框外 1–3px 半透明环） | 全部 | 聚焦边框改用 ring 色；外环本身需要改模板（`InputActiveShadow` 存在但没有模板使用），本期不做 |
| 遮罩背景模糊 | 全部 Dialog | 只对齐遮罩颜色与浓度 |
| 按下下移 1px | 全部按钮 | 无 |
| 禁用 = 整体 50% 透明 | 全部 | 用禁用色近似；彩色主色下有偏差 |
| Popover 不显示箭头 | 全部 | 由应用设置 `IsArrowVisible="False"`；主题保证显示箭头时一体描边 |
| 子菜单阴影比主菜单高一级 | Vega、Nova、Lyra（Maia、Mira 仅 ContextMenu 子菜单） | 都用主菜单那一级（除非 5.8 的控件改动顺带解决） |
| Nova 等的 Card / Dialog 灰底底栏 | Nova、Lyra 等 | 不做 |
| 字体、图标库 | 全部 | 非目标 |

### 5.8 需要的控件改动（Ant Design 渲染必须不变）

以下三项实现前先实测确认。确认需要改动后，按"新增 Token、默认值等于现状"的方式做，与 `f619ba31d` 新增 `ContentBorderColor` 的做法一致。

1. **Tooltip 专属阴影。**
   - 现状：Tooltip 的弹层阴影可能与菜单共用 `Controls[Popup]`。
   - 需要：shadcn 的 Tooltip 无阴影，菜单有阴影。
   - 若实测确认共用，则给 Tooltip 新增阴影 Token（默认等于现状）。
2. **Drawer 贴边边框。** 若 Drawer 没有边框 Token，新增贴边一侧的边框色与粗细 Token（默认 0）。
3. **输入类控件的聚焦边框色、底色与边框色**（控件级）。逐个确认 LineEdit、TextArea、Select、Cascader、DatePicker 等是否有对应 Token，没有的列出并补齐（默认等于现状）。

不在本期范围：焦点外环、文字大小写与字距、单边边框。这三项都涉及模板或文本渲染，另立项。

### 5.9 Gallery

- **Theme 菜单：**
  - 设计语言组列出 8 个 Shadcn Style；
  - 品牌色组列出 Shadcn 的 Neutral 加 17 个 Theme 色块：Neutral 写入当前 Base Color 的 `--primary`，彩色写入 700 / 400 级；
  - 新增"中性色"组，列出 7 个 Base Color 色块：写入并显示各自浅色 `--muted-foreground`（即 `ColorNeutral` 种子）；
  - 当前选中项与"重置"按 `ThemeInfo.NeutralAccentColor` 判断，与品牌色按 `AccentColor` 判断的做法一致；
  - Fixed 时整组禁用并给出提示，同品牌色。
- **记忆：** 每种设计语言各自记住品牌色与中性色，仅在内存中，同品牌色设计。
- **品牌色块按设计语言提供。** Shadcn 的 Theme 色块（Neutral + 17）只在 Shadcn 设计语言下出现。Theme 菜单和 Theme Presets 页共用同一个色块来源（`GalleryBrandSwatches.For(language, …)`）。
  - Theme Presets 页现在对所有语言都用通用的 Tailwind 500 色板，PrimeNG 外加一个 noir（`ThemePresetsViewModel.cs:295-297`）。
  - Shadcn 的官方 Theme 取 700 / 400 级，500 色板一个都命中不了，全部会落到 5.3.2 的自定义派生。因此这一页对 Shadcn 必须改用 Shadcn 的色块。
- **Theme Presets 页：**
  - 按新的设计语言分组；
  - 设计语言声明 `NeutralColor="Seed"` 时，主色色块下面加一行中性色色块，规则同菜单；
  - 预览按所选的品牌色与中性色应用；
  - 导出 XML 时写出 `NeutralColor` 属性，以及所选的 `ColorPrimary`、`ColorNeutral` 种子（未选时写定义自己的种子）。
- **本地化：** 新增 8 个设计语言名、7 个中性色名、17 个 Theme 名，以及"中性色"组标题（中英文）。
- **测试：** 现有引用旧 ID 的 Gallery 测试改用新 ID：`ThemePresetsShowCaseTests`、`WorkspaceWindowLayoutTests`、`GalleryBaseFoundationTests`。

### 5.10 文档

- 重写 `src/AtomUI.Themes.Shadcn/README.md`：维度、映射表、不支持清单，并链接附录。
- 更新以下文档：
  - `docs/guides/theming/preset-theme-packages.md`
  - `docs/modules/themes/overview.md`
  - XML 参考（新属性与诊断）
  - GalleryBase 主题切换文档
  - 未发布 API 变更说明（中英文）：Core 新增中性色轴，Shadcn 主题 ID 变化
- 涉及控件改动时，按 `atomui-control-documentation` 同步更新控件的 `token.md` 与 `changelog.md`。

## 6. 细节与已知差异

### 6.1 颜色换算

OKLCH 按 CSS Color 4 换算到 sRGB，再四舍五入到 8 位。附录的 hex 是唯一来源，算法里的表从附录复制，测试也以附录为准。

### 6.2 透明色合成

`hover:bg-primary/80` 这类取值要合成到 `--background` 上，得到不透明色。原因是 AtomUI 的按钮背景直接绘制在父容器上；直接用半透明色，叠在非白底上时会偏离官方。

实现者在 Gallery 里对比两种做法，选与官网更接近的，并写进 README。

### 6.3 彩色 Theme 下的 `--secondary`

在 `/create` 实测（Base Neutral）时，选择任一彩色 Theme 后，`--secondary` 会变为 zinc 的 100 档（暗色为 zinc 的 800 档）。这是官方 Theme 定义的行为。

本期 Secondary 跟随 Base Color，不随 Theme 变化。差异极小（`#F5F5F5` 对 `#F4F4F5`），写进 README。

## 7. 验证

- **数据：**
  - 单测把 `ShadcnBaseColors`、`ShadcnTailwindPalette`、`ShadcnStyles` 与附录逐项比对，可从附录 Markdown 解析，或由生成脚本同时产出 C# 测试数据，**实现者定**；
  - 17 个 Theme 种子在亮暗下解析出的主色与前景，等于附录 B。
- **Core：** 中性色轴的 XML 读写、ABI 兼容、两条新诊断（各有通过与失败用例），以及解析器的 Seed / Fixed / 未知语言三种情形。
- **包：** 每个 profile 的声明（`DesignLanguage`、`BrandColor`、`NeutralColor`、`Density`）正确，目录诊断为空。
- **弹层（重点）：**
  - 每种 Style 亮暗各一次，断言以下 Token 等于 5.6 的表：`PopupRootShadow`、`OverlayHostShadow`、FlyoutHost 阴影、`ContentBorderColor` / `ContentBorderThickness`、Dialog `BoxShadows`、Drawer 阴影、`ColorBgMask`、Tooltip 底色与文字色。
  - 再按 `atomui-visual-acceptance` 做实机截图验收：宿主用 `atom:Window`；同一组件与官网 `/create` 对应 Style 截图，比较关键区域（环、阴影外沿、圆角、箭头）。容差按项目约定，通道 ≤ 8，只看边缘。
  - 截图要覆盖：带箭头与不带箭头的弹层、暗色半透明环、`shadow-2xl` 未被裁切。
- **回归：** Ant Design、PrimeNG、Bootstrap、Material 的现有测试全部通过。Gallery 主题切换、Theme Presets 测试通过。最后跑全量回归。

## 8. 分步

| 步 | 内容 | 完成标志 |
|---|---|---|
| 1 | Core 中性色轴（5.2）：`ColorNeutral` Seed、`ThemeInfo` 两个新属性、解析器、诊断、XSD、文档 | Core 测试全绿；旧包加载不受影响；Ant Design 渲染不变 |
| 2 | Shadcn 算法骨架、数据表（附录 → C#）、Nova 一种 Style 的 4 个 profile 跑通，含 Base Color 与 Theme 解析 | Nova 数据测试与弹层 Token 测试全绿 |
| 3 | 5.8 的三项控件确认与改动 | Ant 渲染不变（对应控件测试与截图） |
| 4 | 其余 7 种 Style | 各 Style 数据与弹层测试全绿 |
| 5 | Gallery（5.9） | Gallery 测试全绿 |
| 6 | 实机视觉验收（弹层优先），README 与文档 | 验收记录完成 |

## 9. 已决定与留给实现者

**用户决定**

- 对齐 shadcn/ui 现行标准。
- 8 种 Style 都尽可能对齐。
- 维度组织采用 4.1 方案 A。
- 弹层阴影与边框是重点。

**我的决定**

- 中性色以新增的可空 Seed Token `ColorNeutral` 表达，种子取 `--muted-foreground`（4.2 方案 A）。复用 `ColorTextBase` 被否决，因为 Stone 与 Taupe 撞色，且暗色计算器会冲突。
- `ThemeInfo` 新增 `NeutralAccentColor`，与 `AccentColor` 对称。
- 种子只在 Map 阶段可见：Map 阶段一次性解析，后续阶段从计算后的 `ColorNeutral` 反查；调用 Ant 计算器前换用按外观取值的副本种子（5.3.0）。
- Theme Presets 页按设计语言提供品牌色块，并为 Seed 语言加中性色行。
- Compact profile 沿用 AtomUI 约定。
- 字体、图标库、Radius 选择、Chart / Menu 选项为非目标。
- 删除旧的 Zinc / Slate / Stone 主题且不提供迁移文档（未发布）。
- `ColorPrimaryBg` 等四项取近似比例。
- Secondary 跟随 Base Color。

**留给实现者（须在 README 或 PR 说明中记录取舍）**

- `ThemeInfo` 上的枚举是新建还是复用。
- 自定义中性色与自定义主色的派生细则（受 5.3.1、5.3.2 的约束）。
- 透明色是合成还是直接使用（6.2）。
- Toast 的实测取值。
- 数据测试怎么从附录取数。
- 5.8 中各项是否确实需要改控件。

## 10. 前提

- 基于 `release/6.0`。工作区现有未提交改动（PrimeNG 相关）与本方案无关，先由其所属工作提交。
- 是否在独立 worktree 上实施，按 `docs/engineering/contributing/branch-and-worktree-guidelines.md` 在计划阶段向用户申请。
