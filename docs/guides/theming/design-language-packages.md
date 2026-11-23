# 设计语言包指南

AtomUI 的外观由**设计语言**决定。Ant Design、Shadcn、PrimeNG 是平级的设计语言，谁也不叠在谁上面：每种语言自己给出种子默认值、
全局 Token 与每个控件的取值；控件与模板只有一套，不知道当前是哪种语言。「换一种看起来像 shadcn 的外观」因此不是加载一份覆盖
Ant Design 的主题，而是启用另一种语言并选中它。

本文面向应用开发者与主题作者：语言一侧有哪些包、应用要引用哪些、怎样启用与选择，以及怎样在语言之上做具名主题。架构见
[设计语言架构](../../architecture/systems/theming/design-languages.md)；要自己写一种语言，见[编写设计语言](writing-design-languages.md)。

## 1. 包的角色

| 包 | 命名 | 内容 | 谁引用 |
| --- | --- | --- | --- |
| 语言包 | `AtomUI.DesignLanguages.<Lang>` | 语言本身：四个轴、种子默认值、全局推导、语言 Token、官方具名主题、`Use<Lang>()` 入口 | 取值包的作者、派生语言的作者。应用不直接引用，它随取值包传递进来 |
| 取值包 | `<控件包名>.Themes.<Lang>` | 控件推导、生成的静态注册入口，以及显式登记用的 `Add<控件包>()` | **应用** |
| 控件包 | `AtomUI.Desktop.Controls`、`AtomUI.Desktop.Controls.DataGrid`… | 控件、模板与控件 Token 的声明，不含任何取值 | 随取值包传递进来；应用照常调用 `Use<控件包>()` |

控件包与语言包只在取值包里相遇。应用为每个启用的「控件包 × 设计语言」选择一份取值包：

| 控件包 | Ant Design 取值包 |
| --- | --- |
| `AtomUI.Controls`（跨端共用控件） | `AtomUI.Controls.Themes.AntDesign`（随各端的取值包传递进来） |
| `AtomUI.Desktop.Controls` | `AtomUI.Desktop.Controls.Themes.AntDesign` |
| `AtomUI.Desktop.Controls.ColorPicker` | `AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign` |
| `AtomUI.Desktop.Controls.DataGrid` | `AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign` |

没有控件 Token 的控件包（例如 `AtomUI.Desktop.Controls.Extras`）不需要取值包。商业产品与第三方控件包遵守同样的结构，例如
`Acme.Controls` 配 `Acme.Controls.Themes.AntDesign`，见[第三方 AtomUI Control Package 指南](third-party-control-packages.md)。

## 2. 应用引用什么

应用开发者只需要记住一条：**用了哪个控件包，就装它的 `.Themes.<Lang>`**。

```xml
<PackageReference Include="AtomUI.Desktop.Controls.Themes.AntDesign" Version="..." />
<!-- only when the application uses DataGrid / ColorPicker -->
<PackageReference Include="AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign" Version="..." />
<PackageReference Include="AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign" Version="..." />
```

取值包引用它的控件包、对应的语言包和所依赖控件包的取值包，所以 `AtomUI.Desktop.Controls`、`AtomUI.DesignLanguages.AntDesign`、
`AtomUI.Controls.Themes.AntDesign` 都经传递依赖进来。同时支持多种语言时，为每种语言各装一组取值包。

## 3. 启用

应用分别启用控件包和设计语言，并指定初始语言。

```csharp
using AtomUI.DesignLanguages.AntDesign;

.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseDesktopDataGrid();

    builder.UseAntDesign(ant => ant.WithDefaultTheme(AntDesignThemes.DaybreakBlue));
    builder.WithInitialDesignLanguage<AntDesignLanguage>();
})
```

- `Use<控件包>()` 启用控件包，`Use<Lang>()` 登记语言及其具名主题。应用源码生成器把已引用的取值包挂入启动映像，启动时自动匹配。
  同一组合有多个候选时，通过 `UseValues(...)` 显式选择取值包。也可继续使用 `Add<控件包>()` 显式登记推导。
- 应用至少启用一种语言，否则启动失败：`No design language is enabled; enable one (for example builder.UseAntDesign(...)).`
- 每个已启用的语言都必须为每个已启用的控件包登记取值。缺了任何一个，启动时失败并写出应安装、启用的取值包，例如
  `'AtomUI:Tag' under 'Acme' (install and enable 'AtomUI.Desktop.Controls.Themes.Acme')`。
- 取值包是针对某个版本的控件包构建的。升级了控件包而取值包还是旧的，启动时失败，消息写出两个包名与两个 Token Schema 摘要；
  升级到同一版本即可。
- 只启用一种语言时可以省略 `WithInitialDesignLanguage`。多种语言需要明确初始语言，或用 `WithInitialTheme(ThemeSelection)` 恢复完整选择。
- 未实际应用的语言只参与登记和目录展示。初始化种子与推导等到该语言首次应用。

## 4. 选择语言、变体与具名主题

启用后，语言出现在 `IThemeManager.Languages`（`DesignLanguageInfo`：风格、密度、形态与可调种子），主题出现在 `IThemeManager.Themes`。
每个已启用的语言在目录里都有一个隐式条目 `$<LanguageId>`（`IsLanguageDefault = true`），代表它在所选变体下的默认外观，所以语言
本身不需要任何主题文件。省略具名主题时使用该语言的应用默认值，`WithoutTheme()` 显式选择隐式条目。用户选择由 `ApplyAsync` 应用：

```csharp
await themeManager.ApplyAsync(ThemeSelection
    .Of<AntDesignLanguage>()
    .WithTheme("DaybreakBlue")                 // optional named theme
    .WithAppearance(ThemeAppearance.Dark)
    .WithDensity(DesignDensity.Compact)
    .WithSeed("ColorPrimary", "#52C41A"));
```

- 明暗、密度、风格是参数，不需要为每个组合准备主题文件。明暗可以 `FollowSystemAppearance()`：主题管理器按平台当前的明暗解析，
  平台变化时重新应用。
- 选择的解析只有三条规则，没有回退搜索：(1) 语言已启用、所选主题属于该语言、风格与密度在语言声明的范围内，否则抛
  `ThemeSelectionException`；(2) 主题固定的轴取主题的值，选择里的值被忽略，记在 `ThemeSelectionResult.IgnoredAxes`；(3) 主题锁定的种子
  不被替换，记在 `IgnoredSeeds`。
- 当前所选的具名主题因目录刷新而消失时，回到同一语言的默认外观，变体不变，而不是换成另一个主题。
- 需要直接提交一份带完整运行时配置（含控件级取值）的请求时，用 `ApplyThemeAsync(ThemeRequest)`；Ant Design 的具名主题 id 是 `AntDesignThemes.DaybreakBlue`。

## 5. 第一方语言

| 语言 | 状态 | 风格 | 紧凑 | 形态 | 可调种子 | 具名主题 |
| --- | --- | --- | --- | --- | --- | --- |
| Ant Design（`AntDesign`） | 可用：语言包与 4 个取值包 | 单一（`AntDesignLanguage.DefaultStyle`，即 `Default`） | 官方 | Desktop、Mobile | `ColorPrimary` | Daybreak Blue（`DaybreakBlue`） |
| Shadcn（`Shadcn`） | 可用：语言包与 4 个取值包 | `Vega`、`Nova`（缺省）、`Maia`、`Lyra`、`Mira`、`Luma`、`Sera`、`Rhea` | AtomUI 扩展 | Desktop | `ColorPrimary`、语言种子 `BaseColor` | 无（语言缺省条目 `$Shadcn`） |
| PrimeNG（`PrimeNG`） | 可用：语言包与 4 个取值包 | `Aura`（缺省）、`Lara`、`Nora` | AtomUI 扩展 | Desktop | `ColorPrimary` | 无（语言缺省条目 `$PrimeNG`） |

Shadcn 与 PrimeNG 与 Ant Design 结构相同：`src/DesignLanguages/Shadcn/`、`src/DesignLanguages/PrimeNG/` 下的语言包与各控件包的取值包，
入口是 `UseShadcn(...)`、`UsePrimeNG(...)`；取值来源、规则表与不支持清单见各包 README。旧的 `AtomUI.Themes.Shadcn`、
`AtomUI.Themes.PrimeNG` 预设包建立在已移除的主题算法上，已删除。

每个语言包的 README 列出对标的官方版本、全局 Token 映射表、各轴中哪些是 AtomUI 扩展和不支持清单；每个取值包的 README 列出每个控件的
目标等级（A 对标 / B 协调）。一个效果既没列在「支持」里也没列在「不支持」里，就是文档缺漏，请提 Issue。

## 6. 具名主题

具名主题是语言之上的一份**配置**：属于哪种语言、固定哪些轴、覆盖哪些值。它不含规则：凡是「从 A 推出 B」的，都属于语言。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme xmlns="https://atomui.net/schemas/theme/v2"
       Id="AcmeBrand"
       Name="Acme Brand"
       Language="AntDesign">
  <Tokens>
    <Token Name="ColorPrimary" Value="#0F6CBD" Locked="true" />
    <Token Name="BorderRadius" Value="4" />
  </Tokens>
  <When Appearance="Dark">
    <Tokens>
      <Token Name="ColorBgContainer" Value="#1F1F1F" />
    </Tokens>
  </When>
</Theme>
```

- `Language` 必须写；根上写了 `Style`、`Appearance`、`Density` 即固定，不写即开放，由选择与作用域决定。固定只约束最终用户的选择，开发者写的
  `ThemeConfig` 与局部作用域不受约束。
- 一个文件覆盖所有变体：明暗、密度、形态下不同的值写进 `<When>` 条件段，按文档顺序覆盖基础段。
- `Locked="true"` 只能写在基础段的可调种子上，表示最终用户的品牌色选择不能替换它，界面也不再为它提供色板。
- 换品牌色时由语言推导出全部状态色，不需要手写悬停、按下、焦点环；显式写出的 Map / Alias 值在推导之前冻结，由它们算出的值同样跟着变。
- `<Control>` 修正单个控件：覆盖 Map、Alias 或控件 Token 只替换那一个值；覆盖种子会为该控件重新派生；要让某个控件固定暗色或紧凑，在
  `<Control>` 上写 `Appearance` / `Density`。

主题可以由语言包、取值包、产品包或应用携带。语言包只带官方有名字的主题（Ant Design：Daybreak Blue，由 `UseAntDesign()` 注册）；
任意来源之间重复的 `Theme.Id` 都是错误。具名主题之上，应用的 `ThemeConfig` 与局部 `ThemeConfigProvider` 照常叠加，优先级为
「语言推导 < 主题定义 < 作用域与运行时配置」，见[合并规则](../../architecture/systems/theming/runtime.md#43-合并规则)。

### 6.1 制作自己的主题包

先判断需要的是一份配置还是一套规则：

- **具名主题（只写配置）**：品牌色、圆角、字体、个别控件的取值。
- **派生语言或新语言（写规则）**：原设计体系有自己的色阶、几何表、明暗与密度规则，或品牌化需要不同的推导，见
  [编写设计语言](writing-design-languages.md)。

具名主题包的结构：

```text
Acme.Themes.Brand/
+-- Acme.Themes.Brand.csproj     references the language package; <AtomUIThemeDefinition Include="Themes/*.theme.xml" />
+-- Themes/AcmeBrand.theme.xml   theme definition (XML v2)
+-- AcmeThemes.cs                UseAcmeThemes() calls the generated GeneratedThemePackageRegistration.Register(builder.Theme)
\-- README.md
```

```csharp
public static class AcmeThemesBuilderExtensions
{
    public static IAtomUIBuilder UseAcmeThemes(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        AtomUI.Generated.AcmeThemesBrand.GeneratedThemePackageRegistration.Register(builder.Theme);
        return builder;
    }
}
```

生成器在构建期用与运行时相同的 Reader 与 XSD 校验主题，并对照被引用语言包与控件包的构建契约绑定 `Language`、Token 与 `<Control>`；
运行时不读 XML。主题包本身不启用语言或控件包：它引用的语言与控件必须由应用启用。细节见
[编译主题包架构](../../architecture/systems/theming/compiled-theme-packages/overview.md)与
[编译主题 ABI v4](../../reference/theming/compiled-theme-abi-v4.md)。

建议流程：

1. 先只写 `<Tokens>` 里的种子，在 Gallery 的「主题预设」页里看整体效果；种子推不出的个别值再写 Map / Alias 的显式值。
2. 用 `<Control>` 修正单个控件。
3. 不同变体下的值写进 `<When>`，不要为每个变体复制一份文件。
4. 做不到的效果写进 README 的「不支持」，不要去改控件。
5. 某个 Token 设置了却没有效果，是主题系统的缺陷，请按 Token 名提 Issue，不要在主题里绕路。

完整 XML 协议见[主题定义 XML v2 规范](../../reference/theming/theme-definition-xml-v2.md)；旧的 v1 主题文件用
`dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<directory>` 一次性换写。

## 相关文档

- [设计语言架构](../../architecture/systems/theming/design-languages.md)：四类包、依赖规则、单遍管线与启动校验。
- [编写设计语言](writing-design-languages.md)：语言包、取值包、派生语言与契约测试。
- [主题定制指南](customization.md)：Token、控件级配置、明暗与密度、局部主题。
- [第三方 AtomUI Control Package 指南](third-party-control-packages.md)：控件包与它的取值包。
- [主题定义 XML v2 规范](../../reference/theming/theme-definition-xml-v2.md)：主题文件格式与诊断。
