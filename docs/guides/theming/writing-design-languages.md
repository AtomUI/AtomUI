# 编写设计语言

AtomUI 里每一个取值——种子的默认值、全局 Token、每个控件的控件 Token、控件看到的全局 Token 覆盖——都由**设计语言**给出。
核心与控件包只声明词汇（有哪些 Token、各自表达什么），不含任何取值规则。Ant Design 自己也只是一个用公开 API 写成的语言，
第三方语言与它走同一条路径，没有特殊通道。

一种语言由两类包组成：

| 包 | 命名 | 内容 | 引用 |
| --- | --- | --- | --- |
| 语言包 | `<前缀>.DesignLanguages.<Lang>` | 语言定义、语言 Token、种子默认值与全局推导、具名主题、`Use<Lang>()` 入口 | `AtomUI.Core` |
| 取值包 | `<控件包名>.Themes.<Lang>` | 该控件包的每个控件 Token 类在该语言下的控件取值规则、`Add<控件包>()` 入口 | 对应控件包、对应语言包、所依赖控件包在同一语言下的取值包 |

控件包与语言包之间没有任何引用，它们只在取值包里相遇。本文以 Ant Design 为实例：语言包
`src/DesignLanguages/AntDesign/AtomUI.DesignLanguages.AntDesign`，取值包 `AtomUI.Controls.Themes.AntDesign`、
`AtomUI.Desktop.Controls.Themes.AntDesign`、`AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign`、
`AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign`。设计依据见
[设计语言架构方案](../../developments/specs/2026-11-22-design-language-architecture-design.md)第 5、6、8、13、14 章；运行时细节以
[主题系统架构](../../architecture/systems/theming/runtime.md)为准，主题文件格式以
[主题定义 XML v2 规范](../../reference/theming/theme-definition-xml-v2.md)为准。

## 1. 什么时候写一种语言

| 想做的事 | 做法 |
| --- | --- |
| 换品牌色、圆角、字体 | 不需要写语言：用具名主题（XML v2）或 `ThemeConfig` 覆盖种子 |
| 在某种语言上做品牌化，且需要不同的推导规则 | 派生语言（第 7 节），只重写不同的部分 |
| 对标另一套设计体系（自己的色阶、几何表、明暗与密度规则） | 新的语言包，加上为要支持的控件包各写一个取值包 |
| 为别人的控件包追加一种已有语言 | 只写取值包：引用它的控件包与那个语言包 |
| 新的控件包，不打算逐语言适配 | 通用取值规则（第 4.4 节），在任何语言下都能给出取值 |

主题 XML 的显式值只能写常量，常量不跟着种子走；凡是「从 A 推出 B」的规则，都属于语言。

## 2. 契约

```csharp
public abstract class DesignLanguage<TLanguageToken> : DesignLanguage
    where TLanguageToken : LanguageDesignToken, new()
{
    public abstract string Id { get; }
    public abstract DesignLanguageAxes Axes { get; }

    public abstract void DefineSeeds(in DesignVariant variant, DesignToken seed, TLanguageToken languageSeed);
    public abstract void DeriveLanguage(in DesignVariant variant, DesignToken seed, TLanguageToken language);
    public abstract void DeriveGlobal(in TokenEvaluationContext<TLanguageToken> context, DesignToken token);
}

public interface IControlTokenRule<TLanguageToken, TToken>
    where TLanguageToken : LanguageDesignToken
    where TToken : AbstractControlDesignToken
{
    void Apply(in TokenEvaluationContext<TLanguageToken> context, ControlTokenTarget<TToken> target);
}
```

这些类型都在 `AtomUI.Theme.Languages`。一次求值是单遍的，每一步只执行一次，没有重跑，没有收敛判定：

```text
1  确定语言与变体    风格、明暗、密度来自主题、选择与作用域；形态来自界面根
2  种子              DefineSeeds  <- 主题种子 <- 作用域种子
3  语言 Token        DeriveLanguage
4  全局 Token        DeriveGlobal（Map 与 Alias）
5  最终规范化        核心执行的、与语言无关的规则（例如关闭动效时标准时长归零）
6  控件视图          每个控件的 IControlTokenRule.Apply；按需惰性求值
7  发布快照
```

**显式值先冻结。** 进入第 3、4、6 步之前，管线把用户在对应词汇上显式给出的值（主题、`ThemeConfig`、作用域）写入 Token 对象并
冻结这些槽位：推导对冻结槽位的写入被忽略，读取得到显式值。所以推导就是普通的顺序赋值，不需要也不能判断「用户是否设置过」；
用户改了 A，后面由 A 算出的 B 自动跟随，不论 A 与 B 是同一类 Token 还是跨类（语言 Token → 全局 Token → 控件 Token）。
优先级只有一条：**语言推导 < 主题显式值 < 更内层作用域的显式值**。

**纯函数。** 所有方法都是输入的纯函数：相同输入给出相同输出，不读时钟、环境或可变静态状态，不修改不归自己写的对象。推导可能
在后台线程上被并发调用。复用通用的色彩数学时调用核心的中性工具（`AtomUI.Theme.Derivation` 的 `PaletteGenerator`、`ColorMap`、
`AtomUI.Media.ColorUtils` 等），不得调用另一种语言。

## 3. 语言包

### 3.1 语言 Token

语言 Token 是语言自己的词汇：全局 Token 里没有位置的角色、几何与色阶（例如 Shadcn 的 `--input`、`--ring`）。它与全局 Token、
控件 Token 同等地位：`partial` 属性、生成的 Schema、冻结语义、可以被主题的 `<LanguageTokens>` 和 `ThemeConfig.LanguageTokens`
显式覆盖。

```csharp
// Acme.DesignLanguages.Acme
[LanguageDesignToken]
public partial class AcmeToken : LanguageDesignToken
{
    /// <summary>
    /// Base colour family the neutral roles are taken from.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color BaseColor { get; set; }

    /// <summary>
    /// Border of form fields.
    /// </summary>
    public partial Color Input { get; set; }

    /// <summary>
    /// Focus ring colour; pairs with the field background.
    /// </summary>
    public partial Color Ring { get; set; }
}
```

- 类必须是顶层、非泛型、派生自 `LanguageDesignToken`、标 `[LanguageDesignToken]`，并与语言类在同一个程序集里。
- 标 `[DesignTokenKind(DesignTokenKind.Seed)]` 的属性是**语言种子**，在 `DefineSeeds` 里给默认值；其余是派生的语言 Token，
  在 `DeriveLanguage` 里写全。
- 语言 Token 不得与全局 Token 同名（注册表构建时报错）。
- **读取规则：** 一个值在全局 Token 里有位置时，推导从全局 Token 读；只有全局词汇里没有的概念才从语言 Token 读。这样用户对全局
  Token 的覆盖对所有语言的控件一致生效。控件模板不能引用语言 Token——控件包看不见语言包。
- 语言 Token 是语言的公开 API：新增属性不算破坏性变更，删除或改变语义才算。

Ant Design 的全局词汇本来就是 Ant Design 的形状，所以 `AntDesignToken` 没有属性；类仍然存在，让控件取值规则绑定到 Ant Design，
也让品牌语言可以派生自它：

```csharp
[LanguageDesignToken]
public partial class AntDesignToken : LanguageDesignToken
{
}
```

### 3.2 语言类与四个轴

```csharp
[DesignLanguage]
public sealed class AntDesignLanguage : DesignLanguage<AntDesignToken>
{
    public static DesignStyle DefaultStyle { get; } = new("Default");

    private static readonly DesignLanguageAxes s_axes = new(
        [DefaultStyle],
        DefaultStyle,
        [DesignDensity.Default, DesignDensity.Compact],
        [DesignIdiom.Desktop, DesignIdiom.Mobile],
        [GlobalTokenSchema.Get(nameof(DesignToken.ColorPrimary))]);

    public override string Id => "AntDesign";

    public override DesignLanguageAxes Axes => s_axes;

    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, AntDesignToken languageSeed)
    {
    }

    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, AntDesignToken language)
    {
    }

    public override void DeriveGlobal(in TokenEvaluationContext<AntDesignToken> context, DesignToken token)
    {
        MapTokenRules.Apply(context.Variant, token);
        AliasTokenRules.Apply(token);
    }
}
```

- 语言类必须是非抽象、非泛型、带公开无参构造的类，派生自 `DesignLanguage<TLanguageToken>` 并标 `[DesignLanguage]`。
- `Id` 必须**返回一个常量字符串**（构建契约从常量读取它，见第 6 节），并符合主题 Identifier 词法 `[A-Za-z_][A-Za-z0-9_.-]{0,127}`。
  用官方名的帕斯卡写法：`AntDesign`、`Shadcn`、`PrimeNG`。
- `DesignLanguageAxes(Styles, DefaultStyle, Densities, Idioms, AdjustableSeeds)` 声明四个轴里语言提供的取值：

| 轴 | 类型 | 说明 |
| --- | --- | --- |
| 风格 | `DesignStyle` | 语言内部的外观族（Shadcn 的 Nova、PrimeNG 的 Aura）；Ant Design 只有一个 `Default`。`DefaultStyle` 必须在列表里 |
| 明暗 | `ThemeAppearance` | 固定为 `Light` / `Dark`，不需要声明；两种都必须支持 |
| 密度 | `DesignDensity` | 核心预定义 `Default`、`Compact`，语言可以声明更多档（`new DesignDensity("Comfortable")`） |
| 形态 | `DesignIdiom` | 核心预定义 `Desktop`、`Mobile`，可以扩展；由界面根决定，不由主题决定 |

  风格、密度、形态是开放的标识符集合，新的语言、新的设备形态不需要改核心。请求了语言未声明的取值时，在配置提交或主题应用时
  失败，不做静默替换。语言独有的其他选择（Shadcn 的 Base Color、对比度等级）不新增轴，用语言种子表达。
- `AdjustableSeeds`：哪些种子适合交给最终用户更换（Ant Design：`ColorPrimary`）。它们必须是种子；界面据此决定提供哪些色板，
  主题只能对它们写 `Locked="true"`。全局种子用 `GlobalTokenSchema.Get(name)` 取得 descriptor，语言种子的 descriptor 在语言包内部
  由生成的 `GeneratedLanguageTokenSchema` 提供。
- `SeedDerivations`（`init`，可选）：每个可调种子派生出哪些 Token。主题没有锁定该种子却显式写了这些 Token 时，Binder 给出
  `ATMTHM3006` 警告。

### 3.3 三个方法各写什么

三套内置语言包的求值组织与命名见[语言包 Token 求值设计](../../architecture/systems/theming/language-token-evaluation.md)。

| 方法 | 输入 | 必须写全 |
| --- | --- | --- |
| `DefineSeeds` | 变体 | 该变体下的全局种子默认值与语言种子默认值。全局种子的声明默认值就是 antd 的种子，所以 Ant Design 什么也不写 |
| `DeriveLanguage` | 变体、种子 | 每一个非种子的语言 Token |
| `DeriveGlobal` | `context.Variant`、`context.Language`，`token` 里已有种子 | 每一个 Map 与 Alias Token |

- `DefaultThemeSeeds` 可声明隐式主题的色样与可调种子文本，供目录和菜单在首次应用语言前展示。
  未声明时目录省略这些展示值。实际取值始终由 `DefineSeeds` 按变体给出。
- 全局求值通过一次 `DeriveGlobal` 调用完成。显式 Map / Alias 值在调用前冻结，入口先应用 Map 规则再应用 Alias 规则，
  Alias 读取已生效的 Map 值。
  Seed / Map / Alias 作为词汇分层说明 Token 的角色。
- 明暗怎么算、每个密度档怎么算，是语言自己的规则。Ant Design 的 `MapTokenRules` 按 antd 的组合方式先跑默认算法，紧凑密度
  再叠加紧凑算法，暗色再叠加暗色算法；这些计算器是语言包的内部实现，不对外公开。官方没有定义的档位作为 AtomUI 扩展实现，并在
  README 标明。
- 语言在三个方法里都可以按 `variant.Idiom` 给不同的值：同一份种子在两种形态下得到两套语言 Token 与全局 Token。
- **成对角色**（背景与其上的前景，例如 `ColorPrimary` 与 `ColorTextLightSolid`、`ColorBgContainer` 与 `ColorText`）必须成对给值，并保证
  可读的对比度；契约测试会检查（第 8 节）。

### 3.4 入口与注册

生成器为语言包产出 `AtomUI.Generated.<程序集标识>.DesignLanguageRegistration.Register(IThemeManagerBuilder)`（程序集名去掉
非字母数字字符，例如 `AtomUI.Generated.AtomUIDesignLanguagesAntDesign`），把每个 `[DesignLanguage]` 类连同其语言 Token 的 Schema
登记为一个 `DesignLanguageDescriptor`。生成入口通过静态调用注册，支持裁剪与 AOT。
旧版生成类型 `GeneratedDesignLanguageRegistration` 转发到同一入口，兼容既有源码和二进制调用。

语言包提供一个 `Use<Lang>()` 入口，定义在语言包里，签名固定为接收一个配置回调：

```csharp
using System.Runtime.CompilerServices;
using AtomUI.Generated.AtomUIDesignLanguagesAntDesign;
using AtomUI.Theme.Languages;

public static class AntDesignBuilderExtensions
{
    public static IAtomUIBuilder UseAntDesign(this IAtomUIBuilder builder,
        Action<DesignLanguageBuilder<AntDesignLanguage>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        DesignLanguageRegistration.Register(builder.Theme);
        // The compiled theme package (Daybreak Blue) registers once per theme manager builder.
        if (s_registeredThemes.TryAdd(builder.Theme, s_marker))
        {
            ThemePackageRegistration.Register(builder.Theme);
        }
        configure?.Invoke(new DesignLanguageBuilder<AntDesignLanguage>(builder));
        return builder;
    }

    private static readonly ConditionalWeakTable<object, object> s_registeredThemes = new();
    private static readonly object s_marker = new();
}
```

`DesignLanguageBuilder<TLanguage>` 只是把应用 builder（`Root`）与主题 builder（`Theme`）交给取值包的扩展方法。

### 3.5 具名主题

语言包可以带官方有名字的主题（Ant Design：Daybreak Blue），以编译主题包的形式发布：

```xml
<ItemGroup>
  <AtomUIThemeDefinition Include="Themes/*.theme.xml" />
</ItemGroup>
```

主题文件是 [v2 格式](../../reference/theming/theme-definition-xml-v2.md)，只描述配置：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme xmlns="https://atomui.net/schemas/theme/v2"
       Id="DaybreakBlue"
       Name="Daybreak Blue"
       Language="AntDesign">
  <Tokens>
    <Token Name="ColorPrimary" Value="#1677FF" />
    <Token Name="ColorLink" Value="#1677FF" />
  </Tokens>
</Theme>
```

语言本身不需要带任何主题文件：每个已启用的语言在主题目录里自动有一个隐式条目 `$<LanguageId>`，代表该语言在所选变体下的默认外观。

## 4. 取值包

### 4.1 控件取值规则

```csharp
// AtomUI.Desktop.Controls.Themes.AntDesign/Tooltip/ToolTipTokenRule.cs
[ControlTokenRule]
internal sealed class ToolTipTokenRule : IControlTokenRule<AntDesignToken, ToolTipToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ToolTipToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        token.ToolTipMaxWidth     = 250;
        token.ToolTipColor        = global.ColorTextLightSolid;
        token.ToolTipBackground   = global.ColorBgSpotlight;
        token.ToolTipCornerRadius = new CornerRadius(4);
        token.ContentPadding      = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingSM / 2 + 2);
        token.MotionDuration      = global.MotionDurationFast;
        token.ToolTipShadows      = global.BoxShadowsSecondary;
        token.MarginToAnchor       = 4;
        token.ArrowSize            = global.SizePopupArrow;
        token.ArrowTipRadius       = global.BorderRadiusXS.TopLeft;
        token.ArrowBaseRadius      = global.BorderRadiusOuter.TopLeft;
        token.ArrowStrokeThickness = 1;
    }
}
```

- **身份是类型。** 推导类通过泛型参数绑定到语言 Token 类型与控件 Token 类型，写错在编译期报错。风格、明暗、密度、形态都是
  `context.Variant` 里的参数：一个控件 Token 类、一种语言，只有一个推导类。
- 规则类必须是非抽象、非泛型、带公开无参构造的类，标 `[ControlTokenRule]`，恰好实现一次
  `IControlTokenRule<TLanguageToken, TToken>`，`TToken` 必须是绑定到某个控件的终端控件 Token 类。
- **写入目标。** `target.Own` 是控件 Token，**必须写全**，包括行为开关与从抽象层继承的属性。`target.Global` 是该控件看到的
  全局 Token：读时得到该控件的值，写时只对这个控件生效；只写与全局不同的项，而且**只能写 Map 与 Alias**——种子不能在控件取值规则里
  覆盖（写种子会抛出），改种子要重新派生，违反单遍。控件要读一个语言可以按控件改变的值时，模板读的是表达这个角色的 Map / Alias
  Token（例如 Shadcn 在输入框视图里把 `InputActiveBorderColor` 设为 `--ring`、把 `BorderRadiusMD` 设为输入框圆角）或控件 Token。
- **读取顺序：** `target.Global.X` 若本方法已经写过就返回写入的值，否则返回共享的全局值；`target.Own.X` 只能读本方法已经写过的。
  另可读 `context.Language`（语言 Token）与 `context.Variant`。
- **显式值优先：** 主题与作用域对该控件写过的值在推导之前冻结，推导对它们的写入被忽略。
- 推导只使用 Token 类及其属性的类型，不得调用控件的行为代码，也不得在取值包里放控件或 ControlTheme。
- **背景与前景成对。** 取了背景（`ColorPrimary`、`ControlItemBgActive` 族、`ColorBgSpotlight`…）就必须取成对的前景
  （`ColorTextLightSolid`、`ControlItemColorActive`…）。

### 4.2 共享逻辑

同一语言内多个控件共用的取值通过 `TokenCalculator` 计算。Ant Design 的普通按钮规则在 `Apply` 中调用共享计算。

```csharp
[ControlTokenRule]
internal sealed class ButtonTokenRule : IControlTokenRule<AntDesignToken, ButtonToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StandardButtonTokenCalculator.Calculate(in context, global, token);
        token.GroupBorderColor = global.ColorPrimaryHover;
        token.ContentFontSize  = global.FontSize;
        // ...
    }
}
```

跨端共用的抽象 Token 层位于 `AtomUI.Controls`。`AtomUI.Controls.Themes.<Lang>` 提供公开的 `TokenCalculator.Calculate` 方法，供各端取值包调用。

### 4.3 查找规则

查找只有一条规则：沿当前语言的语言 Token 类型的继承链，由最具体到最一般，取第一个为该控件 Token 登记的推导。

```text
BrandToken : AntDesignToken : LanguageDesignToken

IControlTokenRule<BrandToken, TagToken>             派生语言自己的推导
IControlTokenRule<AntDesignToken, TagToken>         基语言的推导
IControlTokenRule<LanguageDesignToken, TagToken>    不绑定语言的通用取值规则
```

### 4.4 通用取值规则

绑定到根类型 `LanguageDesignToken` 的推导是通用取值规则：它只能读全局 Token，在任何语言下都能给出取值，外观协调但不对标。它供不打算
逐语言适配的第三方控件使用；第一方控件包不得用它代替语言专属推导。

### 4.5 入口：只登记取值

生成器为取值包产出 `AtomUI.Generated.<程序集标识>.GeneratedControlTokenRuleRegistration.Register(IThemeManagerBuilder)`，把每个
`[ControlTokenRule]` 类登记为一个 `ControlTokenRuleDescriptor`（静态、无捕获的委托，不使用反射）。取值包提供
`Add<控件包>()`，它是语言构建器上的扩展方法，**只登记取值**，并带上所依赖控件包的取值：

```csharp
using AtomUI.Generated.AtomUIDesktopControlsThemesAntDesign;

public static class AntDesignDesktopControlsExtensions
{
    public static DesignLanguageBuilder<AntDesignLanguage> AddDesktopControls(
        this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddCommonControls();   // AtomUI.Controls.Themes.AntDesign
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
```

`AddDesktopDataGrid()` 同样先调用 `AddDesktopControls()` 再登记 DataGrid 的推导。控件包本身由应用启用（`UseDesktopControls()`、
`UseDesktopDataGrid()`），`Add…()` 不调用 `Use…()`：安装包不等于启用，应用必须显式调用入口。


### 4.6 应用如何启用

```csharp
.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseDesktopDataGrid();

    builder.UseAntDesign(language => language.AddDesktopControls().AddDesktopDataGrid());

    builder.WithInitialTheme(ThemeSelection.Of<AntDesignLanguage>());
})
```

应用至少启用一种语言；同时启用多种语言时，每种语言都要登记它所覆盖的控件包。应用开发者的心智模型只有一条：**用了哪个控件包，
就装它的 `.Themes.<Lang>`**；语言包与共用控件的取值包经传递依赖带入。

## 5. 覆盖规则与启动校验

配置提交时（启动，或运行时追加包的提交处）执行以下校验，任一不满足即失败并指出缺少什么：

1. **覆盖**。每个已启用的语言 × 每个已启用控件包里的每个控件 Token 类，都能按 4.3 节找到取值规则。缺失时报
   `Controls without a token rule in an enabled design language: 'AtomUI:Tag' under 'Acme' (install and enable 'AtomUI.Desktop.Controls.Themes.Acme'), …`。
2. **唯一**。同一语言 Token 类型与同一控件只接受一条规则，重复时报 `Duplicate control token rule for '…' and control '…'.`。
   同一程序集里两个规则类绑定同一对类型时，生成器在构建期就报错。
3. **Token Schema 摘要：** 取值包在构建期记录它所覆盖的控件包 Token Schema 摘要；启动时与实际加载的控件包不一致（例如应用升级了
   控件包，而第三方取值包还是旧的）即失败，消息指出取值包与控件包，不会带着缺值运行。
4. **初始主题：** 初始选择的语言已启用，其风格、明暗、密度在该语言声明的范围内。

运行时追加包（插件）时执行同样的校验；新推导若会改变已提交语言下已提交控件的取值，追加被拒绝。没有运行时才暴露的「某控件在
某语言下没有取值」。

构建期由生成器报告的错误（都在出错的类型上，代码为 `ATOMUITOKEN002`）：

| 情况 | 消息要点 |
| --- | --- |
| 语言类不是非抽象、非泛型、带公开无参构造、派生自 `DesignLanguage<T>` 的类 | `must be a non-abstract, non-generic class derived from DesignLanguage<TLanguageToken>…` |
| 语言 Token 类未在同一程序集用 `[LanguageDesignToken]` 声明 | `The language token type '…' of '…' must be declared with [LanguageDesignToken] in the same assembly.` |
| 语言 Token 类不是顶层、非泛型、派生自 `LanguageDesignToken` | `must be a top-level, non-generic class derived from LanguageDesignToken.` |
| `Id` 不返回常量字符串 | `must return a constant string from its Id property.` |
| 推导类形状不对，或实现了零个 / 多个 `IControlTokenRule<,>` | `must be a non-abstract, non-generic class with a public parameterless constructor…`、`must implement IControlTokenRule<TLanguageToken, TToken> exactly once.` |
| `TToken` 不是绑定到控件的控件 Token | `derives '…', which is not a control token bound to a control.` |
| 两个规则类绑定同一对类型 | `'…' and '…' both register a token rule for '…' under '…'.` |

控件包新增或修改了控件 Token 后，它的每个取值包都要补上取值：契约测试的完整性检查（第 8 节）会指出漏写的 Token，启动时的摘要
校验会拒绝没有重新构建的旧取值包。

## 6. 构建契约

语言包的生成器在 `GeneratedThemeBuildContracts.g.cs` 里为每个语言写一条 `ThemeBuildLanguageAttribute`（身份与语言 Token 类），
并为语言 Token 写 `ThemeBuildTokenAttribute`。引用了语言包的程序集（应用、产品包、主题包）编译 `AtomUIThemeDefinition` 主题时，
生成器据此确认 `Language` 由某个被引用的语言包声明，并绑定 `<LanguageTokens>`。只引用语言包、不启用它，主题照样能编译，但启动时
会因语言未启用而失败。细节见 [编译主题 ABI v4](../../reference/theming/compiled-theme-abi-v4.md)。

## 7. 派生语言

一个语言可以派生自另一个语言，继承它的种子默认值、语言 Token、全局推导与全部控件取值规则，只重写不同的部分，用于在某种语言之上做
品牌化：

```csharp
[LanguageDesignToken]
public partial class BrandToken : AntDesignToken
{
    /// <summary>
    /// Brand accent used for highlighted surfaces.
    /// </summary>
    public partial Color Accent { get; set; }
}
```

语言类本身是 `DesignLanguage<BrandToken>`，种子、语言 Token 与全局推导委托给基语言的一个实例，再改写不同的部分。
`TokenEvaluationContext` 的构造函数是公开的，把同一个语言 Token 对象当作基语言的 Token 传入即可：

```csharp
[DesignLanguage]
public sealed class BrandLanguage : DesignLanguage<BrandToken>
{
    private static readonly AntDesignLanguage s_base = new();

    public override string Id => "Brand";

    public override DesignLanguageAxes Axes { get; } = new(
        [AntDesignLanguage.DefaultStyle], AntDesignLanguage.DefaultStyle,
        [DesignDensity.Default, DesignDensity.Compact], [DesignIdiom.Desktop],
        [GlobalTokenSchema.Get(nameof(DesignToken.ColorPrimary))]);

    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, BrandToken languageSeed)
    {
        s_base.DefineSeeds(variant, seed, languageSeed);
        seed.ColorPrimary = Color.Parse("#7C3AED");
    }

    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, BrandToken language)
    {
        s_base.DeriveLanguage(variant, seed, language);
        language.Accent = seed.ColorPrimary;
    }

    public override void DeriveGlobal(in TokenEvaluationContext<BrandToken> context, DesignToken token) =>
        s_base.DeriveGlobal(new TokenEvaluationContext<AntDesignToken>(context.Variant, context.Language), token);
}
```

- 派生语言的语言 Token 类必须派生自基语言的语言 Token 类；只有单继承。
- 控件取值规则按 4.3 的规则查找：只为不同的控件写 `IControlTokenRule<BrandToken, …>`，其余自动落到 `IControlTokenRule<AntDesignToken, …>`。
  基语言的取值包仍要登记，但不必启用基语言本身：派生语言的入口用基语言的 `DesignLanguageBuilder` 调用其 `Add*()`，例如
  `configure(new DesignLanguageBuilder<AntDesignLanguage>(language.Root))`。派生语言的取值包只写它改动的控件，构建期覆盖检查
  （`ATOMUIGEN052`）对派生语言不要求写全，缺的推导由启动时的覆盖校验沿继承链查找。
- 完整可运行的样例在 `tests/fixtures/DesignLanguageParity/`（样例语言、样例控件包与两个取值包），由
  `tests/AtomUI.DesignLanguages.Parity.Tests` 验证。
- 派生语言有自己的 `Id`，在主题目录里有自己的隐式条目。
- 三种第一方语言之间不得建立派生关系。

只换颜色与尺寸时不需要派生语言，具名主题就够了。

## 8. 契约测试

`tests/AtomUI.Theme.ContractTests`（测试支持类库，`DesignLanguageContract.cs`）提供可复用的契约校验：测试工程引用它，对自己的语言包
与取值包跑一遍。它在语言声明的每个变体（每个风格 × Light/Dark × 每个密度 × 每个形态）下执行：

| 检查 | 含义 |
| --- | --- |
| `Identity` | 语言恰好注册一次，`Id` 合法，每个轴至少声明一个取值，默认风格在声明的风格里 |
| `LanguageComplete` | `DeriveLanguage` 写了每一个非种子的语言 Token |
| `GlobalComplete` | `DeriveGlobal` 写了每一个 Map 与 Alias Token |
| `ControlComplete` | 每个控件取值规则写了其控件的每一个控件 Token |
| `Pure` | `DeriveGlobal` 与控件取值规则不修改语言 Token；在中间编译其他变体不改变某个变体的结果 |
| `Deterministic` | 同一变体编译两次，全局、语言、控件 Token 的值相同 |
| `Readable` | 正文（`ColorText` 在 `ColorBgContainer` 上）与主色上的文字（`ColorTextLightSolid` 在 `ColorPrimary` 上）达到对比度门槛 |

完整性的检查方法是用两组不同的样本值预填 Token 再运行推导：结果随样本变化，说明那个 Token 没有被写。

```csharp
[AvaloniaFact]
public void AntDesign_And_Its_Desktop_Value_Packages_Satisfy_The_Contract()
{
    var input = DesignLanguageContractInput.Capture(static builder =>
    {
        builder.UseDesktopControls();
        builder.UseDesktopColorPicker();
        builder.UseDesktopDataGrid();
        builder.UseAntDesign(static language => language.AddDesktopColorPicker().AddDesktopDataGrid());
    });

    DesignLanguageContract.Verify(input, "AntDesign").ShouldBeEmpty();
}
```

`Capture` 在一个新的应用 builder 上运行注册回调并构建注册表，不需要 Avalonia 应用；注册不完整（第 5 节第 1 条）时直接抛出。
`Verify` 返回 `DesignLanguageContractViolation(Check, Variant, TokenName, Message)` 列表，空列表表示满足契约。Ant Design 有 1 个风格 ×
2 种明暗 × 2 个密度 × 2 个形态，共 8 个变体。

契约测试证明取值写全、可复现、可读，不证明取值**对**：取值包还要按声明的等级完成视觉验收（以 `atom:Window` 为宿主，对全部 showcase
与全部控件做脚本验收），每个 Token 的生效由[主题生效测试](../../engineering/development/theme-effect-tests.md)证明。

## 9. 发布

- 语言包 README 写明：对标的官方版本、全局 Token 映射表、各轴中哪些是 AtomUI 扩展、不支持清单。
- 取值包 README 为每个控件声明目标等级：**A 对标**（官方有对应组件，按视觉验收口径对官方实测）或 **B 协调**（官方无对应组件，用
  语言 Token 组合取值，关键状态人工评审）。
- 取值包与控件包同仓、同版本发布；取值包对语言包声明最低版本。
- 依赖方向：控件包不得引用任何语言包或取值包；语言包只引用核心（派生语言另引用基语言）；取值包不得引用其他语言的语言包与取值包。
- 语言包与取值包只使用公开 API，不需要也不应该有对核心或控件包的 `InternalsVisibleTo`。
- 取值的出处：每个取值应指到该设计语言官方源码或文档的一处；官方未定义的标为 AtomUI 扩展。

## 相关文档

- [主题定制指南](customization.md)：在已有语言之上定制主题、控件级配置和局部主题。
- [设计语言包指南](design-language-packages.md)：选择语言、引用取值包与制作具名主题包。
- [第三方 AtomUI Control Package 指南](third-party-control-packages.md)：控件包的 Token 声明与它的取值包。
- [主题定义 XML v2 规范](../../reference/theming/theme-definition-xml-v2.md)：主题文件格式与诊断。
- [主题系统架构](../../architecture/systems/theming/runtime.md)：单遍管线、冻结语义、求值单元与发布层。
