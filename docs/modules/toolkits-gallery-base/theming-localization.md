# GalleryBase 主题与本地化设计

本文档细化 GalleryBase 如何注册 AtomUI 主题资源、Control Token、XAML namespace 和 Shell 级语言资源。GalleryBase 可以使用 AtomUI 作为 UI 实现，但主题和语言边界必须保持产品中立。

## 设计目标

- GalleryBase 拥有自己的 Token 和主题，不借用 `AtomUIGallery` 命名空间。
- 产品页面继续使用产品自己的语言资源，不进入 GalleryBase。
- Shell 文案由 GalleryBase 提供，产品品牌和页面文案由产品提供。
- Control、可选 Own Token 和主题资产遵循 AtomUI 约定，并由生成器建立 descriptor 与 manifest。
- 整个 GalleryBase 包只使用一次生成的注册入口，不逐 Control 或逐 Theme 手工注册。
- 支持 Browser 和 Desktop 共用同一套主题 Provider。

## XAML namespace

GalleryBase 使用独立 XAML namespace：

```csharp
[assembly: XmlnsPrefix("https://atomui.net/toolkits/gallery-base", "gallery")]
[assembly: XmlnsDefinition("https://atomui.net/toolkits/gallery-base", "AtomUI.Toolkits.GalleryBase")]
[assembly: XmlnsDefinition("https://atomui.net/toolkits/gallery-base", "AtomUI.Toolkits.GalleryBase.Controls")]
[assembly: XmlnsDefinition("https://atomui.net/toolkits/gallery-base", "AtomUI.Toolkits.GalleryBase.Controls.DesignTokens")]
[assembly: XmlnsDefinition("https://atomui.net/toolkits/gallery-base", "AtomUI.Toolkits.GalleryBase.Models")]
[assembly: XmlnsDefinition("https://atomui.net/toolkits/gallery-base", "AtomUI.Toolkits.GalleryBase.Shell")]
```

产品 XAML 使用：

```xml
xmlns:gallery="https://atomui.net/toolkits/gallery-base"
```

`AtomUIGallery.ShowCases.*` 不再放进 GalleryBase 的 XML namespace 定义。产品自己的 ViewModel namespace 继续使用 `using:` 或产品自有 XML namespace。

## 主题注册入口

```csharp
public static class ThemeManagerBuilderExtensions
{
    public static IAtomUIBuilder UseGalleryBase(
        this IAtomUIBuilder builder,
        Action<GalleryBaseOptions>? configure = null);
}
```

入口职责：

- 注册 GalleryBase 生成的 exact CLR type/identity Control descriptor、ControlTheme asset manifest 和主题 Provider。
- 登记 GalleryBase 控件的通用取值规则（生成的 `GeneratedControlTokenRuleRegistration`，见下文「Control Token」），并以
  `DependsOn` 启用 `UseDesktopControls()`。
- 通过 `GeneratedLanguageModuleRegistration` 把 GalleryBase Catalog 与内置 Translation Bundle 注册到
  `builder.Localization`。
- 构建并保存 `GalleryBaseConfiguration`。

入口不维护逐 Control Token 列表、主题 URI 列表或聚合 AXAML；这些内容由构建期生成结果提供。

推荐启动顺序：

```csharp
.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseDesktopColorPicker();
    builder.UseDesktopDataGrid();
    builder.UseAntDesign(language => language.AddDesktopColorPicker().AddDesktopDataGrid());
    builder.UseGalleryBase(...);
})
```

GalleryBase 依赖 AtomUI Desktop 控件作为 Shell 默认 UI，因此产品要启用 Desktop 控件包，并至少启用一种设计语言、登记 Desktop
控件在该语言下的取值包（`AddDesktopControls()` 等只登记取值，控件包仍由 `Use…()` 启用）。GalleryBase 自己的控件不需要
逐语言的取值包。

GalleryBase 本身不启用设计语言，也不注册产品主题定义。AtomUIGallery 在 `UseGalleryControls()` 中以 `DependsOn` 启用
`UseGalleryBase(...)` 与 `UseAntDesign(language => language.AddDesktopColorPicker().AddDesktopDataGrid())`；单独使用
`UseGalleryBase()` 的应用自行选择语言。`DaybreakBlue` 是 Ant Design 语言包 `AtomUI.DesignLanguages.AntDesign` 中的编译主题，由
`UseAntDesign(...)` 注册；目录里另有每个已启用语言的隐式条目 `$<LanguageId>`。Gallery 自身不包含任何主题 XML，也不生成
`ThemePackageRegistration`。
原先的 Polar Green、Sunset Orange、Golden Purple、Magenta 四个预设已删除，改为 Theme 菜单 Brand color 中 `AntDesign` 语言专属的命名色块
（`#52C41A`、`#FA8C16`、`#722ED1`、`#EB2F96`，另有 Daybreak Blue `#1677FF`），由 `GalleryBrandSwatches` 统一维护。
`PrimeNG.*` 语言使用自己的列表 `GalleryBrandSwatches.PrimeNG`：primeng.dev（primeng 22.1.5）配置器的主色，依站点顺序为
Noir `#020617`（站点默认，也是 PrimeNG 主题的 `AccentColor`）和 emerald、green、lime、orange、amber、yellow、teal、cyan、
sky、blue、indigo、violet、purple、fuchsia、pink、rose 十六个 Tailwind 500 色（站点没有 red）；其他把 `ColorPrimary` 声明为
可调种子的语言用 `Tailwind500`。

具名主题由构建期生成类型化主题包（[编译主题 ABI v4](../../reference/theming/compiled-theme-abi-v4.md)），生成内容只有强类型
注册与显式值，派生由运行时的设计语言推导完成。不扫描程序集或资源目录。Desktop Gallery 另外使用 `WithApplicationId("AtomUIGallery")` 和
`UseUserThemeDirectory()` 追加用户目录；Browser Gallery 使用随应用发布的编译包。Gallery 不提供独立的
reload command 或菜单，宿主需要刷新用户主题时直接调用 `IThemeManager.ReloadThemesAsync()`。Catalog 的冲突、
首次用户来源降级、整批原子刷新和删除当前用户主题后的默认回退语义以
[主题系统架构](../../architecture/systems/theming/runtime.md)为准。

## Shell 主题选择状态

`GalleryWorkspaceViewModel` 把 ThemeManager 已提交的 Catalog 和主题状态投影成 Shell 可消费的只读状态：

- `AvailableThemes`：`IThemeManager.Themes` 的防御性快照（含每个已启用语言的隐式条目）；`ThemeCatalogChanged` 后以事件参数的
  `Themes` 整体替换。
- `CurrentThemeId`：只由已提交的 `ThemeChanged`/Catalog 回退状态更新，不把菜单点击当成提交结果。
- `DesignLanguages`、`CurrentDesignLanguage`：设计语言键为语言 `Id`，语言声明了多个风格时加 `.Style`（`AntDesign`、`Shadcn.Nova`）。
  `DesignLanguages` 按目录顺序列出 `IThemeManager.Languages` 中已启用语言的键；`CurrentDesignLanguage` 是已提交主题的语言与风格
  （尚未同步主题状态时为 `null`）。
- `SwitchThemeCommand`：接收 Theme Id，切到该主题及其所属语言（风格取主题声明的风格，同语言时沿用当前风格），带上当前的
  Appearance、Compact、该语言记忆的品牌色与中性色和用户动过的 Motion / Wave Spirit。主题固定了明暗时 Appearance 跟随它；
  Follow System 下与系统当前明暗一致时保持 Follow System。
- `SwitchDesignLanguageCommand`：按设计语言键切换语言与风格，不带具名主题（即该语言的隐式条目），保留 Appearance、Compact
  与该语言记忆的品牌色、中性色。目录中不存在的键（例如菜单生成后目录已变化）被忽略，不会让命令出错。
- `CanChooseBrandColor`、`BrandColor`：当前语言把 `ColorPrimary` 声明为可调种子（`DesignLanguageInfo.AdjustableSeeds`）且当前主题
  没有锁定它（`ThemeInfo.LockedSeeds`）时可选；`BrandColor` 是该语言记忆的品牌色，`null` 表示使用主题自身色。
- `SetBrandColorCommand`：设置当前语言的品牌色；不可选时为 no-op，与主题 `AccentColor` 相同的颜色视为清除。品牌色按设计语言
  记忆在 ViewModel 内存中（`_brandByLanguage`），不持久化；切到别的语言再切回时恢复。Dark、Compact、Motion 变化沿用同一份记忆。
- `CanChooseNeutralColor`、`NeutralColor`、`SetNeutralColorCommand`：中性色（种子 `ColorNeutral`）的同一套规则，记忆在
  `_neutralByLanguage`，`null` 表示使用主题自身的中性色。
- `AppearanceMode` 与 `IsLightAppearanceMode` / `IsDarkAppearanceMode` / `IsSystemAppearanceMode`：投影 Light、Dark、Follow System 三态；`SetAppearanceModeCommand` 负责显式切换，`ToggleDarkModeCommand` 保留 bool 到 Light/Dark 的便捷映射。

每次切换都由 ViewModel 重建一份完整的 `ThemeSelection`，经 `IThemeManager.ApplyAsync(ThemeSelection)` 提交：语言与风格、
具名主题（隐式条目不写主题）、`WithAppearance(Light/Dark)`、Compact（仅当语言声明了 `DesignDensity.Compact` 时
`WithDensity(DesignDensity.Compact)`）、品牌色与中性色种子，以及用户动过的 Motion / Wave Spirit。品牌色不通过切换主题 Id
实现：AtomUIGallery 不提供 PolarGreen 等 Ant 色彩预设主题文件，而是把品牌色作为 `ColorPrimary` 种子写入选择
（`WithSeed`，仅当它可选且已选色时），由语言的全局推导派生 Hover/Active/Bg 等状态和焦点光晕 `ColorControlOutline`；
`ColorLink`、`ColorInfo` 等其他种子保持独立，不随品牌色变化。Motion 和 Wave Spirit 在用户动过开关后作为 `EnableMotion`、
`EnableWaveSpirit` 种子写入，之前以主题自己的声明为准；关闭 Motion 会同时关闭 Wave Spirit，启用 Wave Spirit 会同时启用
Motion。提交失败时命令抛出 `ThemeLoadException`，失败或被更新请求替代时重新投影已提交状态。

Follow System 通过 `IGallerySystemAppearanceSource` 更新 Dark 状态，并以 `ThemeTransitionReason.FollowSystem` 提交同一
完整请求。主题 Id、Appearance、Compact、Motion 和 Wave Spirit 保持正交；失败或被更新请求替代时，Shell 会重新
投影已提交状态，避免 Radio 菜单停留在未提交选项。

ViewModel 对 `ThemeChanged`、`ThemeCatalogChanged` 和语言事件都保存对称的 handler，并在 `Dispose()` 中解除这些
handler 及当前 Follow System appearance 订阅。
产品窗口可以基于 `AvailableThemes` 动态创建同组 Radio 菜单，但不应复制主题名称/Id，也不应自行解析 XML。
AtomUIGallery 的 Theme 菜单（`GalleryThemeMenu`）分为四个子菜单：Design language（每个设计语言键一个 Radio 项，同一语言的
多个风格折叠进以语言命名的子菜单）、Brand color 与 Neutral color（当前语言的色块与 Reset；种子不可调或被当前主题锁定时整组
禁用，并以悬停提示说明原因）和 Other themes（目录中的具名主题，隐式条目由 Design language 项提供；没有时隐藏）。主题声明了
`AccentColor` 时（设计语言项取该语言隐式条目的值），菜单项在文字右侧显示 `12 × 12`、`2px` 圆角的实心正方形色块，未声明时
不创建占位色块。Light、Dark、Follow System 位于 Appearance 子菜单，
Compact、Motion 和 Wave Spirit 保留为外层 Theme 菜单中的正交开关。

## 主题资产与生成注册

```text
<ControlFolder>/Themes/**/*.axaml
    -> 导出主题、RequiredTokenOwners 与逐控件注册片段
    -> package-level UseGalleryBase() registration
```

GalleryBase 当前主题资产包括：

- `ShowCaseItemTheme.axaml`
- `ShowCasePanelTheme.axaml`
- `GalleryShowCaseHeaderTheme.axaml`
- `GalleryStickyTabsHostTheme.axaml`
- `ColorItemControlTheme.axaml`
- `ColorListControlTheme.axaml`
- `IconGalleryTheme.axaml`
- `IconInfoItemTheme.axaml`
- `GalleryWindowTitleBarTheme.axaml`

构建集成按 `Themes/**/*.axaml` 约定收集这些资产。平台 Provider 只把生成 manifest 中适用的资产接入 Avalonia
Styles；开发者不维护 `GalleryControlThemesProvider.axaml` 聚合字典、Theme Module、glob Attribute 或手工
manifest。单个 Control 只有一个 ControlTheme 时只保留对应主题文件，不增加包装层。

`GalleryShellView` 当前主要通过 C# 组合 AtomUI 现有控件和 Shared Token 完成 sidebar、footer、分隔线与内容区背景；后续如果 sidebar/navigation 有独立主题状态，再补对应 GalleryBase 控件主题和 token。

## Control Token

GalleryBase Token 清单：

| Token | 职责 |
|---|---|
| `ShowCaseItemToken` | 卡片 padding、圆角、阴影、标题权重、placeholder |
| `ShowCasePanelToken` | item 宽度、列数、行列间距、内容 margin |
| `GalleryShowCaseHeaderToken` | Header margin、标题字号、Tag 间距、metadata 卡片 padding、label/value 宽度 |
| `GalleryStickyTabsHostToken` | sticky 背景、边线、padding |
| `GalleryWindowTitleBarToken` | 标题栏菜单字体和间距 |
| `GallerySidebarToken` | 侧边栏宽度、brand/footer padding、分隔线 |
| `GalleryNavigationToken` | 导航 margin、默认缩进、滚动条间距 |

Own Token 类只在 Control 存在独有设计值时创建，并由 AtomUI generator 按约定识别：

```csharp
[ControlDesignToken]
internal sealed partial class ShowCaseItemToken : AbstractControlDesignToken
{
    /// <summary>Padding inside a showcase card.</summary>
    public partial Thickness CardPadding { get; set; }
}
```

GalleryBase 的 Token 类保持 `internal`，但与 AtomUI 控件包一样必须是 `partial` 类、Token 属性必须是 partial 属性
（实现部分由生成器产出，否则报 `ATOMUIGEN049`），只有声明，不含计算方法。因为是 `internal`，其他程序集的取值包不能为
GalleryBase 控件写推导；GalleryBase 在自己的 `TokenRules/Controls/` 里为每个 Token 类提供一个绑定根类型 `LanguageDesignToken`
的**通用取值规则**，只读全局 Token，在任何已启用语言下都给出取值，由 `UseGalleryBase()` 登记：

```csharp
[ControlTokenRule]
internal sealed class GenericShowCaseItemTokenRule : IControlTokenRule<LanguageDesignToken, ShowCaseItemToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<ShowCaseItemToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.CardPadding = new Thickness(global.SizeUnit * 5);
        // ... every other ShowCaseItemToken property
    }
}
```

GalleryBase 自己声明控件 Token，同时带推导，因此不受「取值包不含控件」（`ATOMUIGEN053`）的约束。推导契约见
[编写设计语言](../../guides/theming/writing-design-languages.md#44-通用取值规则)。

`[ControlDesignToken]` 只标记 Own Token 类型，不携带 Control 类型、identity 或 ID。禁止泛型
`[ControlDesignToken<TControl>]` 和手写 ID。即使某个 GalleryBase Control 没有 Own Token，生成器仍为它生成独立
identity、零 Own Token descriptor 和 `XxxTokenResource`。

生成资源扩展后，主题使用：

```xml
Padding="{gallery:ShowCaseItemTokenResource CardPadding}"
```

`GalleryShowCaseHeaderToken` 只承载 Gallery 文档页头的结构性视觉值，例如 margin、间距、字号、metadata 卡片边框和默认宽度。metadata label/value 属于页头信息文本，默认通过同一字体 token 跟随全局 UI 字体；等宽字体只用于 `GalleryCodeViewer` 等代码展示控件。分类、状态、引入版本 Tag 的颜色仍通过 `GalleryShowCaseHeader` 属性传给 AtomUI `Tag`，不在 token 中写死具体业务状态。

## Global Token 使用规则

GalleryBase 可以使用以下 AtomUI Global Token：

- `ColorBgContainer`
- `ColorBgLayout`
- `ColorBorderSecondary`
- `ColorTextSecondary`
- `BoxShadowsTertiary`
- `SizeUnit`
- `BorderRadius`

每个 GalleryBase Control 天然可以覆盖完整 Global Token schema。ControlTheme 希望读取当前 Control 的 Effective
Global Token 时使用对应的 `XxxTokenResource`；只有要求该值永远跟随当前 ThemeContext 全局结果时才使用
`SharedTokenResource`。两者都不产生 Global Token 消费清单。不允许在 GalleryBase 主题中写入产品色值，例如固定
品牌蓝、AtomUI logo 色或产品状态色。
产品色应通过产品页面或产品品牌配置表达。

## 通用页头本地化

GalleryBase 只提供产品无关的 ShowCase Header metadata 资源：

```text
Localization/
  GalleryShowCaseHeaderLang/
    GalleryShowCaseHeaderLangResourceKind.cs
    en-US.xlf
    zh-CN.xlf
    zh-TW.xlf
```

资源范围：

| 资源 | 示例 |
|---|---|
| Namespace label | Namespace |
| Package label | Package |
| Base class label | Base class |

这些 label 属于通用文档页头结构，不应再由每个产品 ShowCase 重复定义。产品页面继续提供控件自己的 title、category、status、subtitle、description 和引入版本文案。

不属于 GalleryBase 的资源：

- `Button`
- `DataGrid`
- `Overview`
- `Community`
- `Install`
- `API`
- `Design Token` 页面内容
- 控件分类、稳定状态、Preview 状态和页面描述

这些由产品项目定义。设置菜单、主题、语言和搜索等 Shell 文案也归产品项目的 Workspace Catalog，GalleryBase
不定义产品导航或 Shell 语言资源。

## 产品导航文案

第一阶段产品导航 header 可以直接传字符串。后续支持可更新语言资源对象：

```csharp
public sealed class GalleryLanguageText : IGalleryLocalizedText
{
    public object ResourceKey { get; }
    public string Fallback { get; }
}
```

刷新策略：

- Shell 监听 `ILanguageManager.LanguageChanged`。
- Header 是 `IGalleryLocalizedText` 时通过 `ILocalizer` 重新解析。
- Header 是字符串时保持不变。

## 语言模块注册

GalleryBase 的 Catalog 和三种内置语言由生成器编译进 owning assembly。`UseGalleryBase` 只调用生成的模块注册入口，
不扫描程序集，也不维护 Provider 池：

```csharp
GeneratedLanguageModuleRegistration.Register(builder.Localization);
```

GalleryBase 自有 `Localization/**/*.xlf` 由公共本地化 targets 自动发现并投影为生成器输入。新增标准目录下的 Catalog 时，不在 GalleryBase 项目文件中重复声明 `AtomUILanguage Include`；项目级重复声明会干扰 Rider 的文件角色，使 XLIFF 未进入 IDE 生成器输入，而普通构建仍可能通过。

产品 Catalog 采用同一 `[LanguageCatalog]` + XLIFF 约定，并由产品自己的生成模块注册入口加入。宿主只需在
`UseAtomUI(...)` 配置阶段调用产品模块扩展；GalleryBase 不扫描产品程序集。

## Browser 主题限制

Browser 使用同一套主题 Provider，但要避免：

- 隐式启用用户主题目录；Browser 只能使用随应用发布的显式 Resolver。

- 依赖仅 Desktop 原生窗口 API 的主题资源。
- 在 theme 中引用 Desktop-only 控件模板。
- 因 Browser 裁剪导致 token 类型未被 DynamicDependency 保留。

如果某个 GalleryBase 控件在 Browser 需要不同模板，应在 Provider 内按平台提供安全模板，而不是让产品项目自己替换。

## 测试要求

- GalleryBase AssemblyInfo 不包含产品 ShowCase namespace。
- `UseGalleryBase` 注册 GalleryBase token 和 theme provider。
- `GalleryControlThemesProvider` 包含 `GalleryShowCaseHeaderTheme.axaml`。
- GalleryBase Language Module 不包含产品页面 Catalog。
- GalleryBase Language Module 包含 ShowCase Header metadata 的通用 label Catalog。
- 主题文件不包含 `AtomUIGallery/Assets` 或产品 URI。
- Browser 构建时能解析 GalleryBase theme provider。
- 语言切换后 Shell 菜单状态和文案刷新。

## 控件有效 Token 与参考展示边界

GalleryBase ControlTheme 使用所属控件的有效全局 Token，支持 Controls[控件] 与局部作用域。
普通标签字号、字重、padding、边框读取现有全局 Token；ShowCase/Inspector 的 viewport、参考 swatch 行、图标 tile 与工具栏点击区域保持固定布局。
`IconInfoItem.IconNameFontSize`（13px）、`ColorItemControl.HexFontSize`（10px）与 `HexOpacity`（0.8）是实际标签 Own Token，
保持原默认值而允许独立覆盖；它们不是色板内容数据，也不以新字体派生公式猜测已有设计关系。`ColorItemControl` 名称默认字号仍12px、字重600。
包级 WarningsAsErrors 对活跃主题消费诊断生效，参考尺寸豁免仅在对应布局节点说明。
