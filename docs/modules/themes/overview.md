# AtomUI.Themes.* 预设主题包模块

`src/AtomUI.Themes.*` 是一组预设主题 NuGet 包，每个包用 Ant Design 的 Token 词汇模拟一种设计语言的大致观感。
它们走[编译主题包](../../architecture/systems/theming/compiled-theme-packages/overview.md#11-预设主题包)机制，
不包含控件代码或 ControlTheme，不引入设计语言抽象层。PrimeNG 与 Shadcn 包含各自的
[主题算法](../../guides/theming/writing-theme-algorithms.md)及其控件阶段（`Algorithms/`），这是包内唯一的 C# 运行时代码。

## 包清单

| 项目 | 主题 | 依赖 |
|---|---|---|
| `src/AtomUI.Themes.PrimeNG` | `PrimeNGAura`、`PrimeNGLara`、`PrimeNGNora`，各有四种 profile；算法 `PrimeNG:Aura`、`PrimeNG:Lara`、`PrimeNG:Nora` 及各自的 `.Dark` | Core + Desktop.Controls |
| `src/AtomUI.Themes.Shadcn` | `ShadcnVega` … `ShadcnRhea`（8 种 Style），各有四种 profile | Core + Desktop.Controls |

两包均已实现并在本地验证，尚未发布到 NuGet。四种 profile 为基础 ID 与 `.Dark`、`.Compact`、`.DarkCompact`；
注册不改变默认 DaybreakBlue，不自动启用 Desktop 控件。

Gallery 原有的 4 个色板主题（PolarGreen、SunsetOrange、GoldenPurple、Magenta）已删除，改为 `AntDesign` 设计语言
（Core 的 `DaybreakBlue`）在 Gallery Brand color 菜单中的命名品牌色块，品牌色写入请求的 `ColorPrimary`。两个包的
profile 都声明了 `DesignLanguage`、`Density` 和 `BrandColor`，见[主题定制指南](../../guides/theming/customization.md)
「设计语言与品牌色」。

## 项目结构

```text
AtomUI.Themes.PrimeNG/
+-- AtomUI.Themes.PrimeNG.csproj   <AtomUIThemeDefinition Include="Themes/*.theme.xml" />
+-- Algorithms/                    六个 [ThemeAlgorithm]、共用的规则表与色阶，Controls/ 下是控件阶段
+-- Themes/*.theme.xml             XML v1 主题定义：算法链和 Seed Token；Lara 紧凑 profile 另有一项 Select 覆盖
+-- PrimeNGThemes.cs               public static IThemeManagerBuilder UsePrimeNGThemes(this IThemeManagerBuilder)
\-- README.md                      规则表与来源、不支持清单、与原设计语言的已知差异
```

`UseXxxThemes()` 只调用生成的注册：`GeneratedThemePackageRegistration.Register(builder)`；声明了算法的包（PrimeNG）
先调用 `GeneratedThemeAlgorithmRegistration.Register(builder)`。

## 约束

- XML 只使用 Core 与已启用控件包登记的 Control 与 Token；预设包不自动启用控件包。
- 每个预设的值来自原设计语言的公开 Token 源码或文档，README 记录映射表（或规则表）的出处与版本。
- 选择实际静态 profile ID；直接给基础 ID 加算法不会自动选择 profile。PrimeNG 换成同族暗色算法与暗色 profile 等价，
  但后接 `AtomUI:Dark` 得到的是混合链。
- 算法只用 Core 的公开 API，第三方能写出同样的算法；算法须通过 `tests/AtomUI.Theme.ContractTests` 的契约校验。
- README 的"不支持"清单是验收项，必须与实际行为一致。
- 包的生效以主题系统的生效测试为前提：预设只是值表或主题算法，写进去的值没效果属于主题系统缺陷，不在预设包里打补丁。

## 相关文档

- [编译主题包架构](../../architecture/systems/theming/compiled-theme-packages/overview.md)
- [预设主题包指南](../../guides/theming/preset-theme-packages.md)：应用如何引用、切换和在包内新增预设。
- [编写主题算法](../../guides/theming/writing-theme-algorithms.md)：PrimeNG 算法的写法与契约。
- [Gallery 主题预设页](../../gallery/authoring/theme-presets-page.md)
- [主题定义 XML v1](../../reference/theming/theme-definition-xml-v1.md)
