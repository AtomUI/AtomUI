# AtomUI.Themes.* 预设主题包模块

`src/AtomUI.Themes.*` 是一组只包含主题定义的 NuGet 包，每个包用 Ant Design 的 Token 词汇模拟一种设计语言的大致观感。
它们走[编译主题包](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-预设主题包)机制，
不包含控件代码、ControlTheme 或运行时逻辑，不引入设计语言抽象层。

## 包清单

| 项目 | 主题 | 依赖 |
|---|---|---|
| `src/AtomUI.Themes.PrimeNG` | `PrimeNGAura`（默认）、`PrimeNGLara`、`PrimeNGNora`；按需 `*Dark` 独立定义 | `AtomUI.Core` |
| `src/AtomUI.Themes.Shadcn` | `ShadcnZinc`、`ShadcnSlate`、`ShadcnStone` | `AtomUI.Core` |
| `src/AtomUI.Themes.Bootstrap` | `Bootstrap` | `AtomUI.Core` |
| `src/AtomUI.Themes.Material` | `Material` | `AtomUI.Core` |

Gallery 现有的 4 个色板主题（PolarGreen、SunsetOrange、GoldenPurple、Magenta）保留在 Gallery 项目；是否迁成
`AtomUI.Themes.AntColors` 包另行决定。

## 项目结构

```text
AtomUI.Themes.PrimeNG/
+-- AtomUI.Themes.PrimeNG.csproj   <AtomUIThemeDefinition Include="Themes/*.theme.xml" />
+-- Themes/*.theme.xml             XML v1 主题定义
+-- PrimeNGThemes.cs               public static IThemeManagerBuilder UsePrimeNGThemes(this IThemeManagerBuilder)
\-- README.md                      模拟范围、不支持清单、与原设计语言的已知差异
```

`UseXxxThemes()` 只调用生成的 `GeneratedThemePackageRegistration.Register(builder)`，不做其他事。

## 约束

- XML 只使用 Core 与已启用控件包登记的 Control 与 Token；预设包不自动启用控件包。
- 每个预设的值来自原设计语言的公开 Token 源码或文档，README 记录映射表的出处与版本。
- 暗色首版由 Dark 算法从 Seed 派生；对照不可接受时增加 `Appearance="Dark"` 独立定义。
- README 的"不支持"清单是验收项，必须与实际行为一致。
- 包的生效以主题系统的生效测试为前提：预设只是值表，值表写了但没效果属于主题系统缺陷，不在预设包里打补丁。

## 相关文档

- [编译主题包架构](../../architecture/systems/theming/compiled-theme-packages/overview.md)
- [预设主题包指南](../../guides/theming/preset-theme-packages.md)：应用如何引用、切换和在包内新增预设。
- [Gallery 主题预设页](../../gallery/authoring/theme-presets-page.md)
- [主题定义 XML v1](../../reference/theming/theme-definition-xml-v1.md)
