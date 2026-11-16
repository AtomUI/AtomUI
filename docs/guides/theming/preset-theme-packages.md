# 预设主题包指南

AtomUI 自始至终是 Ant Design 设计语言。预设主题包用 Ant Design 的 Token 词汇模拟其他设计语言的大致观感，让应用
一行代码换一种"看起来像 PrimeNG / shadcn / Bootstrap / Material"的外观。它们不复刻细节，也不改变任何控件。
机制见[编译主题包架构](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-预设主题包)。

当前四个预设包已实现并在本地验证，尚未发布到 NuGet。以下包引用示例适用于发布后，当前可引用本地项目。

## 1. 引用并启用

```xml
<PackageReference Include="AtomUI.Themes.PrimeNG" Version="..." />
```

```csharp
.UseAtomUI(builder =>
{
    builder.Theme.UsePrimeNGThemes();
    builder.UseDesktopControls();
})
```

`UsePrimeNGThemes()` 把包内的主题注册进同一个 ThemeCatalog。之后它们出现在 `IThemeManager.AvailableThemes` 里，
和 DaybreakBlue、用户目录主题一样切换：

```csharp
await themeManager.ApplyThemeAsync(new ThemeRequest("PrimeNGAura", null, ThemeTransitionReason.UserRequest));
```

每个基础 ID 都有 `.Dark`、`.Compact`、`.DarkCompact` 静态 profile，例如 `PrimeNGAura.DarkCompact`。
选择精确 ID 得到相应外观与密度，系统不会自动派发 profile。

PrimeNG 包的值由它注册的六个主题算法计算（`PrimeNG:Aura`、`PrimeNG:Aura.Dark`、`PrimeNG:Lara`、`PrimeNG:Lara.Dark`、
`PrimeNG:Nora`、`PrimeNG:Nora.Dark`），主题 XML 只有算法链和 Seed Token（Lara 的两个紧凑 profile 另保留一项 Select 覆盖）。因此对 PrimeNG：

- 把链换成同族的暗色算法（`[PrimeNG:Aura.Dark]`）与选择 `.Dark` profile 结果相同；基础链后接 `AtomUI:Compact` 与
  `.Compact` profile 相同（Lara 的紧凑 profile 另有一项 README 列出的不等价值）。
- 在基础链后面接 `AtomUI:Dark`（Gallery 全局明暗开关就是这样做的）得到的是混合链：颜色由 Ant 的 Dark 规则重算，
  PrimeNG 的 Alias 与控件规则在暗色外观下运行。结果可读，但不是 PrimeNG 的暗色。
- 换主色种子后，悬停、按下、焦点环和选中底色都跟着变。

Bootstrap、Material 仍是「Ant 算法 + 显式值」：基础 ID 叠加算法会保留其显式 Map/Alias 覆盖，所以给基础
主题换上 Dark 算法不等于选中具名的 Dark profile。Shadcn 已改为与 PrimeNG 相同的算法包：8 种 Style（设计语言）
× 7 种 Base Color（`NeutralColor="Seed"` 的 `ColorNeutral` 种子）× Neutral + 17 种 Theme（`ColorPrimary` 种子）。

预设包不自动启用控件包。`<Control>` 覆盖和 PrimeNG 的控件阶段都要求对应控件包已经 `UseXxxControls()`；控件没有
注册时，它的控件阶段不会被调用。

## 2. 首批包与每个包的边界

| 包 | 主题 Id | 模拟的要点 | 不支持 |
| --- | --- | --- | --- |
| `AtomUI.Themes.PrimeNG` | `PrimeNGAura`、`PrimeNGLara`、`PrimeNGNora` | 由 PrimeNG 算法计算：PrimeNG 色阶取级、noir 主色（slate-950 种子，与 primeng.dev 默认一致；可换任意种子色）、slate / zinc 表面、圆角 6 / 6 / 2、字段高度 40 / 44 / 40、按钮字重 500 / 600 / 700、焦点环三种形态 | ripple、浮动标签、以透明度表达禁用、Filled 字段形态、rem 缩放 |
| `AtomUI.Themes.Shadcn` | `ShadcnVega`、`ShadcnNova`、`ShadcnMaia`、`ShadcnLyra`、`ShadcnMira`、`ShadcnLuma`、`ShadcnSera`、`ShadcnRhea` | 对齐 shadcn/ui 现行标准：Tailwind v4.3.5 色板 + 取档规则、8 种 Style 几何与弹层阴影档、7 种 Base Color、Neutral + 17 种 Theme | 大写/字距（Sera）、焦点外环、遮罩模糊、按下位移、Popover 圆角（随全局 BorderRadius） |
| `AtomUI.Themes.Bootstrap` | `Bootstrap` | Bootstrap 6.0：`#0087fe` 主色、`FontSize 16`、`ControlHeight 38`、`BorderRadius 8` | 渐变背景、内阴影 |
| `AtomUI.Themes.Material` | `Material` | 紫色主色、`BorderRadius 4`、`ControlHeight 40`、三级 BoxShadow 近似 elevation | ripple、浮动标签、全大写 |

每个包的 README 列出完整映射表、出处版本、不支持清单和已知差异。一个效果没列在"模拟"里也没列在"不支持"里，就是
文档缺漏，请提 Issue。

## 3. 在预设之上继续定制

预设只是一个主题定义，应用的 `ThemeConfig` 照常叠加在它之上：

```csharp
var overrides = new ThemeConfigBuilder().WithToken("ColorPrimary", "#0ea5e9").Build();
await themeManager.ApplyThemeAsync(new ThemeRequest("PrimeNGAura", overrides, ThemeTransitionReason.UserRequest));
```

局部 `ThemeConfigProvider` 同样有效。预设里的 `<Control>` 覆盖与应用的 `Controls` 配置按
[合并规则](../../architecture/systems/theming/runtime.md#43-合并规则)合并。

## 4. 制作自己的预设包

第三方预设包与 AtomUI 的首批包走完全相同的路径：

```text
Acme.Themes.Brand/
+-- Acme.Themes.Brand.csproj     引用 AtomUI.Core；<AtomUIThemeDefinition Include="Themes/*.theme.xml" />
+-- Themes/AcmeBrand.theme.xml   XML v1 主题定义
+-- Algorithms/                  可选：本设计语言的主题算法
+-- AcmeThemes.cs                UseAcmeThemes() 调用生成的 Register(builder)
\-- README.md
```

有两种做法：

- **只写值表。** 用 Ant 的算法加 XML 显式值。简单，但写死的值不随种子变化，明暗、紧凑要各维护一份定义。
- **写算法。** 把原设计语言「从种子推出其他值」的规则写成主题算法（PrimeNG 包的做法），XML 基本只剩算法链和种子。
  换主色时状态色跟着变，暗色可以由同族的暗色算法得到。见[编写主题算法](writing-theme-algorithms.md)。

建议流程：

1. 从原设计语言的公开 Token 源码或文档抄出值表，按 Seed → Map → Alias 的层次映射到 AtomUI 全局 Token；原语言
   直接给值、不派生的地方，直接写 Alias 层的值。值表里能用规则表达的部分（色阶取级、跟随主色的焦点环、从尺寸 Token
   推出的内边距）写进算法。
2. 先只写 `<Tokens>`，在 Gallery 的"主题预设"页里看整体效果；再用 `<Control>` 覆盖或算法的控件阶段修正单个控件。
3. 暗色：只写值表时先交给 Dark 算法，对照后差异不可接受再加一份 `Appearance="Dark"` 的独立定义；写算法时提供一个
   同族的暗色算法，不要依赖在浅色链后接 `AtomUI:Dark`。
4. 做不到的效果写进 README 的"不支持"，不要去改控件。
5. 如果发现某个 Token 设置了却没有效果，那是主题系统的缺陷，请按 Token 名提 Issue，不要在预设里绕路。

完整 XML 协议见[主题定义 XML v1 规范](../../reference/theming/theme-definition-xml-v1.md)。
