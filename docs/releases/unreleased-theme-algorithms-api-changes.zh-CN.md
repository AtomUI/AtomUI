# 设计语言迁移指南（未发布）

[English](unreleased-theme-algorithms-api-changes.md)

本指南说明从 6.2.15 升级的改动。主题系统改为以**设计语言**组织：语言包（例如 `AtomUI.DesignLanguages.AntDesign`）
负责种子与全局推导，控件包只声明自己的 Token，每个控件包在每种语言下有一个取值包（例如
`AtomUI.Desktop.Controls.Themes.AntDesign`）写出控件取值。主题按语言、风格、明暗与密度选择，不再按算法链。发布版本
尚未确定，由发布负责人并入 `docs/releases/<版本>-api-changes.md`。细节见[编写设计语言](../guides/theming/writing-design-languages.md)、
[主题定义 XML v2](../reference/theming/theme-definition-xml-v2.md) 与[编译主题包生成 ABI v4](../reference/theming/compiled-theme-abi-v4.md)。

除[新增 Token 与修正](#新增-token-与修正)列出的有意修正外，Ant Design 的渲染与 6.2.15 完全一致。

## 速查

| 范围 | 6.2.15 | 现在 / 迁移 |
| --- | --- | --- |
| 启动 | `UseDesktopControls()`（以及 ColorPicker、DataGrid）自带 Ant Design 主题 | 显式启用语言：`UseAntDesign(language => language.AddDesktopControls())`；一种语言都没启用时启动抛出 "No design language is enabled; enable one (for example builder.UseAntDesign(...))." |
| 包 | Ant Design 公式在 `AtomUI.Core` 与各控件 Token 类里 | 新包 `AtomUI.DesignLanguages.AntDesign`、`AtomUI.Controls.Themes.AntDesign`、`AtomUI.Desktop.Controls.Themes.AntDesign`、`AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign`、`AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign` |
| `ThemeAlgorithm` 与算法链 | 枚举 `Default` / `Dark` / `Compact`、`ThemeConfigBuilder.WithAlgorithms`、主题 XML 的 `<Algorithms>` | 删除。明暗（`ThemeAppearance`）、密度（`DesignDensity`）、风格（`DesignStyle`）、形态（`DesignIdiom`）是相互独立的参数 |
| 计算器 | `DefaultThemeVariantCalculator`、`DarkThemeVariantCalculator`、`CompactThemeVariantCalculator`、`AbstractThemeVariantCalculator`、`IThemeVariantCalculator`、`IThemeVariantCalculatorFactory`、`IThemeAlgorithm`、`ThemeAlgorithmAttribute`、`ThemeAlgorithmDescriptor`、`ThemeAppearanceEffect`、`ThemeAlgorithmTypeConverter` | 删除；Ant Design 的公式在 Ant Design 语言包里 |
| 色彩工具 | `AtomUI.Theme.Algorithms`（`PaletteGenerator`、`PresetPalettes`、`PresetPrimaryColor`、`PresetColorType`、`PaletteInfo`、`PaletteGenerateOption`、`ColorMap`） | 同名类型移到 `AtomUI.Theme.Derivation`，改 `using` 即可；同一命名空间新增公开的 `CalculatorUtils`、`FontSizeInfo`、`RadiusInfo` |
| `IThemeManager` | `AvailableThemes` | `Themes`（另列出每种已启用语言的隐式默认条目 `$<语言>`）；新增 `Languages`、`CurrentSelection`、`ApplyAsync(ThemeSelection)` |
| `ThemeCatalogReloadResult` | `AvailableThemes` | `Themes` |
| `ThemeInfo` | `(Id, Name, Appearance, IsDefault)` 及 `AccentColor` | `(Id, Name, Language)`，`init` 属性 `Style`、`Appearance`（可空）、`Density`、`LockedSeeds`、`SeedValues`（主题自己给出的可调种子值）、`AccentColor`、`IsLanguageDefault`；`IsDefault` 删除 |
| `ThemeConfig` / `ThemeConfigBuilder` | `Algorithms`、`WithAlgorithms(...)` | `Language`、`Style`、`Appearance`、`Density`、`Idiom`、`LanguageTokens`；`WithLanguage`、`WithStyle`、`WithAppearance`、`WithDensity`、`WithIdiom`、`WithLanguageToken` |
| `ControlAlgorithmMode` | 控件配置的四种控件算法模式 | 删除；控件级取值只替换该值，控件级种子或轴使该控件成为独立的求值单元 |
| 主题定义 XML | v1 | 只支持 v2；用 `dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<目录> [-p:AtomUIThemeDefinitionOutput=<目录>]` 一次性换写 |
| 编译主题包 | ABI 1 | ABI 4；用当前生成器重新构建 |
| 控件 Token 类 | 多数为 `internal`，取值在 `CalculateTokenValues` 里计算 | `public partial`，只有声明；取值来自所用语言的取值包 |
| `SharedTokenResourceValue` | 公开 | 删除 |

## 启用设计语言

```csharp
// 以前（6.2.15）
builder.UseAtomUI(root => root
    .UseDesktopControls()
    .UseDesktopColorPicker()
    .UseDesktopDataGrid());

// 现在
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls.Themes.AntDesign;   // AddDesktopControls, AddDesktopColorPicker, AddDesktopDataGrid

builder.UseAtomUI(root => root
    .UseDesktopControls()
    .UseDesktopColorPicker()
    .UseDesktopDataGrid()
    .UseAntDesign(language => language
        .AddDesktopControls()
        .AddDesktopColorPicker()
        .AddDesktopDataGrid()));
```

- `UseDesktopControls()`、`UseDesktopColorPicker()`、`UseDesktopDataGrid()` 照旧启用控件包。
- `UseAntDesign(...)` 启用语言及其官方主题（Daybreak Blue）。`Add*()` 只登记取值包，不启用控件包。
- 每个已启用的控件包在每种已启用的语言下都要有取值包；缺了的在启动时报错，消息写出缺少的取值包名。

## 选择主题

```csharp
// 以前
await themeManager.ApplyThemeAsync(new ThemeRequest("DaybreakBlue", config, ThemeTransitionReason.UserRequest));

// 现在：按语言与变体
await themeManager.ApplyAsync(
    ThemeSelection.Of("AntDesign")
        .WithAppearance(ThemeAppearance.Dark)
        .WithDensity(DesignDensity.Compact)
        .WithSeed("ColorPrimary", "#1677FF"));
```

- `ThemeSelection` 携带 `Language`、`Style`、可选的具名主题 `ThemeId`、`Appearance`（或 `FollowSystemAppearance()`）、
  `Density`、全局种子 `Seeds` 与语言种子 `LanguageSeeds`。`ThemeSelection.Of<TLanguage>()` 用语言类型代替 Id。
- `IThemeManager.Languages` 以 `DesignLanguageInfo`（`Id`、`Styles`、`DefaultStyle`、`Densities`、`Idioms`、
  `AdjustableSeeds`）列出每种已启用的语言；语言、风格、密度与品牌色菜单由它生成。
- 每种已启用的语言在主题目录里有隐式条目 `$<语言>`（`ThemeInfo.LanguageDefaultId`，标记 `IsLanguageDefault`），
  没有主题文件也能选中该语言。
- 未知的语言、主题或轴取值抛出 `ThemeSelectionException`。
- `ApplyThemeAsync(ThemeRequest)` 提交一份带完整运行时配置（含控件级取值）的主题请求。`IThemeManager.DEFAULT_THEME_ID` 已删除：
  核心不再写死任何语言的主题 id，改用 Ant Design 语言包的 `AntDesignThemes.DaybreakBlue`。
- `IThemeManagerBuilder.WithInitialTheme(ThemeSelection)` 设定启动时的选择。

## 主题配置

`ThemeConfig.Algorithms` 与 `ThemeConfigBuilder.WithAlgorithms` 已删除，直接写变体：

```csharp
// 以前
new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark, ThemeAlgorithm.Compact).Build();

// 现在
new ThemeConfigBuilder()
    .WithAppearance(ThemeAppearance.Dark)
    .WithDensity(DesignDensity.Compact)
    .Build();
```

每个轴各自从外层作用域继承，内层不再整体替换算法链。显式取值在任何推导运行之前就固定下来：主题或配置给出的值不会被
覆盖，由它派生的值跟着它变。

## 主题定义 XML v2

主题文件改用 v2 格式。同一语言、同一风格的四个文件（浅色、深色、紧凑、深色紧凑）合为一个文件，差异写在 `<When>` 段里。
`<Algorithms>`、`IsDefault`、`DesignLanguage`、`BrandColor`、`NeutralColor` 已删除：`<Theme>` 上的 `Language` 与 `Style`
给出分组，Token 上的 `Locked` 说明哪些值用户不能替换。v1 文件不再读取；用户主题目录里的 v1 文件报 `ATMTHM1004` 并
给出换写命令，换写之前该目录不生效。一次性换写：

```bash
dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<目录>
```

命令缺省就地改写，并删除被合并的变体文件；另可用 `-p:AtomUIThemeDefinitionOutput=<目录>` 输出到别处。见
[主题定义 XML v2](../reference/theming/theme-definition-xml-v2.md) 第 14 节。

## 编译主题包

生成器产出编译主题 ABI 4，记录包所需的语言（清单作用域 `language`）。按 ABI 1 构建的包不再加载，需重新构建。

## 控件 Token 类与第三方控件包

`AtomUI.Controls`、`AtomUI.Desktop.Controls`、ColorPicker、DataGrid 的全部控件 Token 类为 `public partial`，只含声明。
`CalculateTokenValues` 与在构造函数里赋值的写法已删除；Ant Design 的取值原样移入各 Ant Design 取值包的
`[ControlDerivation]` 类。

第三方控件包声明自己的 Token，并为支持的每种语言发布一个取值包：

```csharp
// 控件包：只有声明
[ControlDesignToken]
public sealed partial class RatingToken : AbstractControlDesignToken
{
    public partial double StarSize { get; set; }
}

// 取值包（例如 Acme.Controls.Themes.AntDesign）
[ControlDerivation]
internal sealed class AntDesignRatingDerivation : IControlDerivation<AntDesignToken, RatingToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<RatingToken> view)
    {
        view.Token.StarSize = view.Global.ControlHeight * 0.625;
    }
}
```

- Token 类或 Token 属性不是 `partial` 时报 `ATOMUIGEN049`。
- 推导必须写全所属类的每个 Token（`ATOMUIGEN050`），不得先读后写（`ATOMUIGEN051`）。对 `view.Global` 的写入是控件
  作用域内对 Map 与 Alias Token 的覆盖；写种子会抛出。
- 生成器为控件包产出 `[assembly: TokenSchemaDigest]`，为取值包产出 `[assembly: CoversTokenSchema]`。取值包若针对另一
  版本的控件 Token 构建，启动时被拒绝，消息写出两个包名与两个摘要。
- 见[编写设计语言](../guides/theming/writing-design-languages.md)第 4 节与[第三方控件包](../guides/theming/third-party-control-packages.md)。

## 编写设计语言

语言是一个派生自 `DesignLanguage<TLanguageToken>`、标注 `[DesignLanguage]` 的类：声明 `Axes`（`DesignLanguageAxes`：
风格、明暗、密度、形态、可调种子），实现 `DefineSeeds`、`DeriveLanguage` 与 `DeriveGlobal`。语言 Token 派生自
`LanguageDesignToken` 并标注 `[LanguageDesignToken]`。取值包实现 `IControlDerivation<TLanguageToken, TToken>`；查找沿语言
Token 的继承链进行，所以派生语言继承其基础语言的控件取值，只覆盖不同之处。契约测试库 `AtomUI.Theme.ContractTests`
（`DesignLanguageContract.Verify`）逐变体检查身份、完整、纯净、确定与可读。

新增公开类型：`DesignLanguage`、`DesignLanguage<TLanguageToken>`、`DesignLanguageAttribute`、`DesignLanguageAxes`、
`DesignLanguageBuilder<TLanguage>`、`DesignStyle`、`DesignDensity`、`DesignIdiom`、`DesignVariant`、
`DerivationContext<TLanguageToken>`、`ControlView<TToken>`、`IControlDerivation<TLanguageToken, TToken>`、
`ControlDerivationAttribute`、`LanguageDesignToken`、`LanguageDesignTokenAttribute`、`ReservedTokenAttribute`、
`IControlToken`、`DesignLanguageDescriptor`、`ControlDerivationDescriptor`、`GlobalTokenSchema`、
`TokenSchemaDigestAttribute`、`CoversTokenSchemaAttribute`、`ThemeBuildLanguageAttribute`、`ThemeDerivationException`、
`DesignLanguageInfo`、`ThemeSelection`、`ThemeSelectionResult`、`ThemeSelectionResolver`、`ThemeSelectionException`。
`IThemeManagerBuilder` 新增 `AddDesignLanguage`、`AddControlDerivations`（由生成的注册代码调用，只有自行实现该接口的代码
需要补上）与 `WithInitialTheme`。

## Shadcn 与 PrimeNG

`AtomUI.Themes.Shadcn`、`AtomUI.Themes.PrimeNG`（从未发布到 NuGet）由设计语言 `Shadcn`、`PrimeNG` 取代，结构与 Ant Design 相同：

| 语言 | 包 | 风格 | 可调种子 | 入口 |
| --- | --- | --- | --- | --- |
| `Shadcn` | `AtomUI.DesignLanguages.Shadcn`、`AtomUI.Controls.Themes.Shadcn`、`AtomUI.Desktop.Controls.Themes.Shadcn`、`AtomUI.Desktop.Controls.ColorPicker.Themes.Shadcn`、`AtomUI.Desktop.Controls.DataGrid.Themes.Shadcn` | `Vega`、`Nova`（缺省）、`Maia`、`Lyra`、`Mira`、`Luma`、`Sera`、`Rhea` | `ColorPrimary`、语言种子 `BaseColor` | `UseShadcn(language => language.AddDesktopControls())` |
| `PrimeNG` | `AtomUI.DesignLanguages.PrimeNG`、`AtomUI.Controls.Themes.PrimeNG`、`AtomUI.Desktop.Controls.Themes.PrimeNG`、`AtomUI.Desktop.Controls.ColorPicker.Themes.PrimeNG`、`AtomUI.Desktop.Controls.DataGrid.Themes.PrimeNG` | `Aura`（缺省）、`Lara`、`Nora` | `ColorPrimary` | `UsePrimeNG(language => language.AddDesktopControls())` |

两种语言不带具名主题：原来的 profile id（`ShadcnNova`、`ShadcnNova.Dark`、`PrimeNGAura.DarkCompact` 等）已删除，改用
`ThemeSelection.Of("Shadcn", new DesignStyle("Nova")).WithAppearance(ThemeAppearance.Dark)` 等选择；语言自己的条目是
`$Shadcn` / `$PrimeNG`。每个变体与种子用例编译出的
Token 值与原 profile 相同。各包 README 列出规则、来源与不支持清单。

## 新增 Token 与修正

新增的 Token；除特别说明外，缺省值保持 6.2.15 的 Ant Design 渲染：

| Token | 含义 |
| --- | --- |
| `DesignToken.BorderRadiusMD`（Map） | 中号圆角（默认尺寸），与种子 `BorderRadius` 同值；控件读它而不是种子 |
| `DesignToken.InputActiveBorderColor`（Alias） | 输入框聚焦态边框色，缺省 `ColorPrimary` |
| `AbstractButtonVariantToken.LinkColor`、`DangerAccentColor` | 链接按钮静止态颜色（缺省 `ColorLink`）与危险按钮各变体的基色（缺省 `ColorError`） |
| `AlertToken.SuccessIconColor`、`InfoIconColor`、`WarningIconColor`、`ErrorIconColor`；`MessageCardToken.LoadingIconColor` | 状态图标色（缺省为各状态种子；信息与加载图标为 `ColorPrimary`） |
| `TagToken.SuccessColor`、`InfoColor`、`WarningColor`、`ErrorColor` | 状态标签的主色（缺省为各状态种子） |
| `ToolTipToken.ToolTipFontSize`、`ToolTipMinHeight` | 提示框字号与最小高度（缺省 `FontSize`、`ControlHeight`） |
| `DesignToken.ControlItemColorActive`（Alias） | `ControlItemBgActive` 上的文字颜色，缺省为 `ColorText`。Select、Cascader、ComboBox、ListBox、ListView、TransferListView、TreeView、TransferTreeView、TreeSelect 的选中文字 Token 与新增的 `DataGridToken.RowSelectedColor` 缺省取它 |
| `AbstractCalendarToken.ItemActiveColor`、`AbstractDateViewerToken.CellActiveColor` | 选中格子上的主色文字，缺省 `ColorPrimary` |
| `ArrowDecoratedBoxToken.ContentBorderColor`、`ContentBorderThickness`、`ContentMinHeight` | 箭头框体的边框与最小高度；边框不为 0 时绘制框体与箭头的一条轮廓，不再隐藏箭头 |
| `ArrowDecoratedBoxToken.ArrowTipRadius`、`ArrowBaseRadius`（`ToolTipToken` 上也有） | 圆角箭头几何 |
| `ProgressBarToken.InnerLabelColor` | 内嵌百分比标签的深色候选，缺省 `ColorTextLabel` |
| `ToolTipToken.ToolTipShadows`、`DrawerToken.EdgeBorderColor`、`EdgeBorderThickness` | 阴影与边缘边框 |
| `AbstractButtonGeometryToken.RecipeVariants`、`OutlineOnlyDefaultShadow`、`DisabledByOpacity`、`DisabledOpacity`、`PressMoves`、`UnderlineLinkOnHover`、`UnderlineLinkAtRest`；`SplitButtonToken.InsetSeparator`；`LineEditToken` / `TextAreaToken` / `SelectToken` 的 `TransparentOutlineBorder`、`BottomOutlineBorder`；`SelectToken.MutedSelectIndicator`、`OptionBackgroundInset`、`OptionBorderRadius`；`FloatButtonToken.HasOutline`、`OutlineOnlyShadow`、`CapsuleDescription`、`UppercaseDescription`、`DescriptionWeight`、`OutlineDescriptionLineHeight`；`OtpLineEditToken.JoinCells`；`AbstractTimePickerToken.CenterItemContent`、`ItemBackgroundInset` | 由语言设置的行为开关；缺省值即 6.2.15 的行为 |

Ant Design 中有意的视觉修正：

- `ArrowIndicator` 按 antd 的圆角箭头构造（等腰直角三角形，尖端为半径 `ArrowTipRadius` 的圆弧，底边两侧为半径
  `ArrowBaseRadius` 的内凹圆弧）；`ArrowDecoratedBoxToken.ArrowSize` 缺省由 `SizePopupArrow / 1.3` 改为
  `SizePopupArrow`。箭头形状略有变化。
- ToolTip 主题把箭头框体的 `BorderThickness` 设为 0；PopupConfirm 与锚点的间距改读 `FlyoutHostToken.MarginToAnchor`
  （缺省值相同）。
- 显式给出的全局 Alias 值现在带动由它派生的 Alias Token。6.2.15 中显式 Alias 值在 Alias 公式之后才写入，例如显式的
  `ControlOutlineWidth` 传不到 `InputActiveShadow`。没有设置 `ControlOutlineWidth`、`ColorControlOutline`、
  `ColorErrorOutline`、`ColorWarningOutline`、`LineWidthFocus`、`ControlPaddingHorizontal`、`ControlPaddingHorizontalSM`、
  `UniformlyPaddingSM` 的主题编译结果不变。控件级的 Alias 值只替换该值，不重新推导该控件的其他 Token。

## 诊断

| Id | 变化 |
| --- | --- |
| `ATOMUIGEN049` | 新增（Error）：Token 类或 Token 属性不是 `partial` |
| `ATOMUIGEN050` | 新增（Error）：控件推导没有写全 Token |
| `ATOMUIGEN051` | 新增（Error）：控件推导先读后写 |
| `ATOMUIGEN052` | 新增（Error）：取值包缺少其覆盖的某个控件 Token 类的推导 |
| `ATOMUIGEN053` | 新增（Error）：违反包依赖方向（语言包引用控件包、控件包引用语言一侧，或取值包声明控件） |
| `ATOMUIGEN024` | 随 `CalculateTokenValues` 删除 |
| `ATMTHM1004` | v1 主题文件；消息给出换写命令 |

主题 XML v2 的 Reader 与 Binder 诊断见[主题定义 XML v2](../reference/theming/theme-definition-xml-v2.md)第 10 节。

## 性能

2026-11-22 对照不含此改动的同一棵树测量（缺省 Ant Design 主题，Release，30 个样本，两轮交替）：一次冷编译的分配由 1,202,392
字节降到 712,792 字节（-41%），中位耗时由 2.44 ms 降到 1.92 ms。同一作用域的控件共用推导起点的全局视图（推导改写了全局 Token
的控件除外）；只覆盖 Map 或 Alias Token 的局部作用域，对推导没有读到这些 Token 的控件直接复用父作用域的结果。编译结果逐值相同；
明暗切换在本机分辨力内无变化。
