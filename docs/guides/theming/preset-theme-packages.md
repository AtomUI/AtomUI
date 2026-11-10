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
选择精确 ID 得到相应外观与密度；基础 ID 直接叠加算法仍保留其显式 Map/Alias 覆盖，不会自动派发 profile。
预设包不自动启用控件包；它的 `<Control>` 覆盖要求对应控件包已经 `UseXxxControls()`。

## 2. 首批包与每个包的边界

| 包 | 主题 Id | 模拟的要点 | 不支持 |
| --- | --- | --- | --- |
| `AtomUI.Themes.PrimeNG` | `PrimeNGAura`、`PrimeNGLara`、`PrimeNGNora` | emerald 主色、slate / zinc 表面、圆角 6 / 0 / 2 三种风格、字段高度 40、按钮字重 500 / 600 / 700、焦点环三种形态 | ripple、浮动标签、以透明度表达禁用、Filled 字段形态、rem 缩放 |
| `AtomUI.Themes.Shadcn` | `ShadcnZinc`、`ShadcnSlate`、`ShadcnStone` | 近黑主色、细边框、无 wave、弱阴影、`BorderRadius 6`、`ControlHeight 36` | 焦点环形态、Command 式弹层 |
| `AtomUI.Themes.Bootstrap` | `Bootstrap` | `#0d6efd` 主色、`FontSize 16`、`ControlHeight 38`、`BorderRadius 6` | 渐变背景、内阴影 |
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
+-- AcmeThemes.cs                UseAcmeThemes() 调用生成的 Register(builder)
\-- README.md
```

建议流程：

1. 从原设计语言的公开 Token 源码或文档抄出值表，按 Seed → Map → Alias 的层次映射到 AtomUI 全局 Token；原语言
   直接给值、不派生的地方，直接写 Alias 层的值。
2. 先只写 `<Tokens>`，在 Gallery 的"主题预设"页里看整体效果；再用 `<Control>` 覆盖修正单个控件。
3. 暗色先交给 Dark 算法；对照后差异不可接受再加一份 `Appearance="Dark"` 的独立定义。
4. 做不到的效果写进 README 的"不支持"，不要去改控件。
5. 如果发现某个 Token 设置了却没有效果，那是主题系统的缺陷，请按 Token 名提 Issue，不要在预设里绕路。

完整 XML 协议见[主题定义 XML v1 规范](../../reference/theming/theme-definition-xml-v1.md)。
