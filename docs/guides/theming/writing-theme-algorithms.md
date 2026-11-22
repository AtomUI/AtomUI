# 编写主题算法

主题算法把少量种子（Seed）推导成一整套 Token。AtomUI 内置的 `AtomUI:Default`、`AtomUI:Dark`、`AtomUI:Compact`
是 Ant Design 的三条规则；主题包和应用可以注册自己的算法，用另一种设计语言的规则在 AtomUI 的 Ant Design Token
体系里计算 Token。内置三个算法与第三方算法走同一套契约，没有特殊通道。

本文以 `AtomUI.Themes.PrimeNG`（`src/AtomUI.Themes.PrimeNG/Algorithms`）为实例，说明如何声明、注册、测试和发布一个
算法。运行时细节以[主题系统架构](../../architecture/systems/theming/runtime.md#7-编译器)为准，XML 格式以
[主题定义 XML v1 规范](../../reference/theming/theme-definition-xml-v1.md#5-algorithms-与-algorithm)为准。

## 1. 什么时候写算法

主题 XML 的显式值只能写常量。常量不会跟着种子走：把 `ColorPrimary` 换成蓝色后，写死的悬停色、焦点环仍是原来的
颜色；明暗、紧凑也要各写一份。只要一个设计语言里有「从 A 推出 B」的规则，就应该写进算法：

| 要表达的东西 | 放在哪里 |
| --- | --- |
| 颜色、尺寸的推导规则（色阶取级、圆角阶梯、焦点环跟随主色） | 算法 |
| 品牌种子（主色、错误色、圆角、控件高度） | 主题 XML 的 `<Tokens>` 或 `ThemeConfig` |
| 规则推不出的个别常量 | 主题 XML 的显式值 |

PrimeNG 的十二份主题 XML 改造后只剩算法链和 Seed Token（主色、错误色、圆角、控件高度），
只有 Lara 的两个紧凑 profile 另保留一项规则推不出的 Select 覆盖：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme xmlns="https://atomui.net/schemas/theme/v1" Id="PrimeNGAura.Dark" Name="PrimeNG Aura Dark" Appearance="Dark" IsDefault="false">
  <Algorithms>
    <Algorithm Catalog="PrimeNG" Id="Aura.Dark" />
  </Algorithms>
  <Tokens>
    <Token Name="ColorPrimary" Value="#020617" />
    <Token Name="ColorError" Value="#EF4444" />
    <Token Name="BorderRadius" Value="6" />
    <Token Name="ControlHeight" Value="40" />
  </Tokens>
</Theme>
```

## 2. 三个阶段与优先级

一个算法由最多三个阶段组成：

| 阶段 | 接口 | 是否必须 | 输入 | 可以写什么 | 没写的值 |
| --- | --- | --- | --- | --- | --- |
| Map | `IThemeAlgorithm.Evaluate(effectiveSeed, previousMap, nextMap)` | 必须 | 有效 Seed、链上前一个算法的 Map | `nextMap` 上的 Seed 与 Map 阶段 Token | 没有兜底：单独使用（`previousMap` 为 `null`）时必须写满全部 Map Token |
| Alias | `IThemeAliasAlgorithm.EvaluateAlias(in context, token)`，与 Map 阶段是同一个类 | 可选 | 已写入 Map 显式值、已按 Ant 公式算过 Alias 的全局 Token | Alias 阶段的全局 Token | Ant 的 `CalculateAliasTokenValues` |
| 控件 · 全局 | `IControlTokenAlgorithm<TToken>.EvaluateGlobal(in context, controlGlobal)`，每个控件一个类 | 可选 | 该控件自己的一份有效全局 Token | 任意全局 Token，只对该控件生效 | 作用域的全局值 |
| 控件 · Own | `IControlTokenAlgorithm<TToken>.EvaluateOwn(in context, effectiveGlobal, token)` | 可选 | 该控件最终的有效全局 Token（只读） | 该控件的 Own Token | 控件的 `CalculateTokenValues` |

优先级从低到高固定为：

**Ant 默认公式 < 算法 < 主题的显式值（XML 或 `ThemeConfig`）< 更内层作用域的显式值。**

算法写入的值和显式值一样参与派生：例如 Alias 阶段把 `ControlOutlineWidth` 写成 0，Ant 公式算出的
`InputActiveShadow` 就跟着变成 0 扩散。算法不需要、也不应该自己去重算这些派生值。用户在主题里显式写了的字段，
算法的写入不生效。

`ControlAlgorithmMode.Global` 与 `Custom` 两种控件模式有一个例外，见 [7.3](#73-控件的-global-与-custom-模式)。

## 3. 声明与注册

### 3.1 算法类

```csharp
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace Acme.Themes;

[ThemeAlgorithm("Acme", "Brand", 1, ThemeAppearanceEffect.Light)]
public sealed class AcmeBrandAlgorithm : IThemeAliasAlgorithm
{
    public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
    {
        // The Ant Design light algorithm gives every Map token a value; a new instance per call keeps the stage pure.
        new DefaultThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);

        // Acme's own rule: hover and active states are 10% and 20% darker than the primary seed.
        nextMap.ColorPrimaryHover  = Darken(effectiveSeed.ColorPrimary, 0.1);
        nextMap.ColorPrimaryActive = Darken(effectiveSeed.ColorPrimary, 0.2);
    }

    public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token)
    {
        token.ColorFocusBorder           = token.ColorPrimary;
        token.FocusVisualBorderThickness = new Thickness(token.LineWidth);
        token.ControlOutlineWidth        = 0;
    }

    private static Color Darken(Color color, double amount) =>
        Color.FromArgb(
            color.A,
            (byte)Math.Round(color.R * (1 - amount)),
            (byte)Math.Round(color.G * (1 - amount)),
            (byte)Math.Round(color.B * (1 - amount)));
}
```

`[ThemeAlgorithm(catalog, id, revision, appearanceEffect)]` 的四个参数：

- `catalog`、`id`：算法身份 `Catalog:Id`。两者都用 `[A-Za-z_][A-Za-z0-9_.-]{0,127}` 词法，区分大小写。用包自己的
  Catalog（`Acme`、`PrimeNG`），不要借用 `AtomUI`。同一预设的明暗变体是两个身份，例如 `PrimeNG:Aura` 与
  `PrimeNG:Aura.Dark`。
- `revision`：大于 0。算法的输出发生任何变化时都要加一，编译缓存和注册表修订号据此失效。
- `appearanceEffect`：`Light`、`Dark` 或 `Preserve`。编译器从 Light 开始按链的顺序折叠，最后一个非 `Preserve` 的
  效果决定主题外观；主题 XML 的 `Appearance` 必须与折叠结果一致。

生成器只收集满足下列条件的类：非抽象、非泛型、实现 `IThemeAlgorithm`、有公开无参构造函数，`catalog`/`id`
合法且 `revision > 0`，`appearanceEffect` 是三个取值之一。不满足的声明报构建错误 `ATOMUITOKEN002`，位置在
`[ThemeAlgorithm]` 特性上，消息之一：

- `'{type}' must be a non-abstract, non-generic class that implements IThemeAlgorithm.`
- `'{type}' declares a theme algorithm with an invalid catalog, id or revision.`
- `'{type}' declares a theme algorithm with an invalid appearance effect.`
- `'{type}' must have a public parameterless constructor.`

该类不会被注册，同一程序集里其他合法的算法照常生成。

PrimeNG 的六个算法共用一个实现，各自只是带不同规则表的子类（`PrimeNGAuraAlgorithms.cs` 摘录）：

```csharp
[ThemeAlgorithm("PrimeNG", "Aura", 1, ThemeAppearanceEffect.Light)]
public sealed class PrimeNGAuraAlgorithm : PrimeNGPresetAlgorithm
{
    public PrimeNGAuraAlgorithm()
        : base(PrimeNGAuraPreset.Table, dark: false)
    {
    }
}

[ThemeAlgorithm("PrimeNG", "Aura.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class PrimeNGAuraDarkAlgorithm : PrimeNGPresetAlgorithm
{
    public PrimeNGAuraDarkAlgorithm()
        : base(PrimeNGAuraPreset.Table, dark: true)
    {
    }
}
```

### 3.2 生成的注册与身份类

声明了算法或控件阶段的包，生成器产出两样东西：

- `AtomUI.Generated.<程序集标识>.GeneratedThemeAlgorithmRegistration.Register(IThemeManagerBuilder builder)`：调用
  `AddThemeAlgorithms` 登记本包的算法，调用 `AddControlTokenAlgorithms` 登记本包的控件阶段。程序集标识由程序集名
  去掉非字母数字字符、各段首字母大写得到，例如 `Acme.Themes` → `AtomUI.Generated.AcmeThemes`，
  `AtomUI.Themes.PrimeNG` → `AtomUI.Generated.AtomUIThemesPrimeNG`。
- 每个 Catalog 一个公开静态类 `<Catalog>Algorithms`，位于项目的 `RootNamespace`（缺省为程序集名），每个算法一个
  `ThemeAlgorithm` 属性，名字是 `Id` 去掉 `.` 与 `-`：`PrimeNGAlgorithms.Aura`、`PrimeNGAlgorithms.AuraDark`。

包作者再写一个公开入口，内容只是调用生成的注册。PrimeNG 的入口（`PrimeNGThemes.cs`）同时注册算法和包内的主题：

```csharp
public static IThemeManagerBuilder UsePrimeNGThemes(this IThemeManagerBuilder builder)
{
    ArgumentNullException.ThrowIfNull(builder);
    Generated.AtomUIThemesPrimeNG.GeneratedThemeAlgorithmRegistration.Register(builder);
    Generated.AtomUIThemesPrimeNG.GeneratedThemePackageRegistration.Register(builder);
    return builder;
}
```

`GeneratedThemePackageRegistration` 只在包里有 `<AtomUIThemeDefinition>` 主题 XML 时生成。只提供算法、不带主题
文件的包只调用第一行。

`IThemeManagerBuilder.AddThemeAlgorithms(IEnumerable<ThemeAlgorithmDescriptor>)` 是公开的，也可以手工构造
`ThemeAlgorithmDescriptor(algorithm, revision, appearanceEffect, factory)` 注册；但手工注册没有构建契约，编译主题
XML 在构建期看不到这个算法（见 3.4），一般不这样做。

### 3.3 应用如何引用

应用先在 `UseAtomUI` 里调用包的入口，然后在任何能写算法链的地方引用身份：

```xml
<Algorithms>
  <Algorithm Catalog="Acme" Id="Brand" />
  <Algorithm Id="Compact" />
</Algorithms>
```

```csharp
var config = new ThemeConfigBuilder()
    .WithAlgorithms(AcmeAlgorithms.Brand, ThemeAlgorithm.Compact)
    .Build();
```

- XML 的 `Catalog` 省略时为 `AtomUI`，所以 `<Algorithm Id="Compact" />` 仍是内置算法，现有主题文件不用改。
- C# 里也可以直接写 `new ThemeAlgorithm("Acme", "Brand")`；身份是 `(Catalog, Id)` 的序数比较。
- AXAML 里经 `ThemeAlgorithmTypeConverter` 写 `Default`、`Dark`、`Compact` 或 `Acme:Brand`。
- 同一条链里不能出现两次同一身份。

### 3.4 两条错误边界

| 情况 | 什么时候报 | 报什么 |
| --- | --- | --- |
| `Catalog` 或 `Id` 不是合法标识符 | 读取 XML 时 | `ATMTHM1002`，带行列 |
| 合法但当前没有注册（主题 XML） | 绑定时（动态 XML、用户目录主题） | `ATMTHM2001`：`Theme algorithm 'Acme:Brand' is not registered.` |
| 合法但当前没有注册（`ThemeConfig`） | 规范化配置时 | `ATMTHM4004`：`Algorithm 'Acme:Brand' is not registered.` |
| 编译进包的主题 XML 引用了构建期看不到的算法 | 构建期 | `ATOMUITOKEN002`：`Unknown algorithm 'Acme:Brand'.`，带行列，该主题不生成程序 |
| 两个包注册了同一身份 | 构建注册表时 | `ThemeSchemaException`：`Duplicate algorithm identity 'Acme:Brand'.` |
| 编译进包的主题引用的算法在运行时没有注册（应用没调用算法包的入口） | 绑定编译主题时 | `ThemeSchemaException`：`Compiled theme algorithm 'Acme:Brand' is not registered.` |

构建期能看到的算法是本程序集声明的算法，加上直接引用的程序集通过生成的构建契约（`ThemeBuildAlgorithmAttribute`）
携带的算法。

## 4. 各阶段能写什么

### 4.1 Map 阶段

`Evaluate(effectiveSeed, previousMap, nextMap)` 往 `nextMap` 写 Seed 与 Map 阶段的全局 Token。编译器为链上每个算法
新建一份 `nextMap`，内容复制自 `previousMap`（第一个算法复制自 `effectiveSeed`）；链上的算法都拿到同一个
`effectiveSeed`。

- 单独使用（`previousMap` 为 `null`）的算法必须写满 Map 阶段的全部 Token，没有兜底。最简单的做法是先跑一遍对应的
  Ant 算法，再改写自己的规则。PrimeNG 正是这样做的（`PrimeNGPresetAlgorithm.Evaluate` 摘录）：

  ```csharp
  if (_dark)
  {
      new DarkThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);
  }
  else
  {
      new DefaultThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);
  }

  WriteColors(effectiveSeed, nextMap, _dark ? _preset.Dark : _preset.Light);
  if (!_dark || previousMap is null)
  {
      WriteSizes(effectiveSeed, nextMap, _preset.Sizes);
  }
  ```

  内置计算器带有每次调用的内部状态，每次调用都新建一个，不要放进静态字段。
- 接在别的算法后面时，`previousMap` 是前一个算法的结果。PrimeNG 的暗色算法接在别的算法之后只重算颜色，尺寸沿用
  前一个 Map，与 `AtomUI:Dark` 一致；PrimeNG 的浅色算法不看 `previousMap`，会丢弃前一个算法的整个 Map。两种做法
  都合法，在包的 README 里写明是哪一种。
- 在 Map 阶段写 Alias Token 没有用：之后的 Ant Alias 公式会把它覆盖。

### 4.2 Alias 阶段

在同一个类上再实现 `IThemeAliasAlgorithm`。编译器用跑过 Map 阶段的同一个实例调用 `EvaluateAlias`，这时：

- Map 的显式值已经写入，Ant 的 `CalculateAliasTokenValues` 已经跑过，所以可以读任何全局 Token；
- 只能写 Alias 阶段的全局 Token，写了别的阶段的 Token 会报 `StageBoundary`（第 9 节）；
- 写入的值带动依赖它的 Ant 默认值，编译器会重跑默认公式。

PrimeNG 的 Alias 阶段（摘录）：

```csharp
public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token)
{
    ArgumentNullException.ThrowIfNull(token);
    var rules   = _preset.For(context.Appearance);
    var surface = rules.Surface.AsSpan();
    var alias   = _preset.Alias;

    token.ColorFocusBorder           = rules.FocusBorder(token);
    token.FocusVisualBorderThickness = new Thickness(token.LineWidth * alias.FocusVisualBorderLineWidths);
    token.ControlOutlineWidth        = token.LineWidth * alias.ControlOutlineLineWidths;
    // ...
    token.PaddingContentHorizontal = token.SizeSM;
}
```

规则按 `context.Appearance` 取，而不是按实例是明是暗：混合链 `[PrimeNG:Aura, AtomUI:Dark]` 会在暗色外观下运行浅色
算法的 Alias 阶段。宽度、内边距从 `LineWidth`、`SizeSM` 推出，后接 `AtomUI:Compact` 时跟着缩小。

### 4.3 控件阶段

控件阶段是每个控件一个类，实现 `IControlTokenAlgorithm<TToken>`，用一个或多个 `[ControlTokenAlgorithm]` 关联到
算法。同一个类可以服务多个算法，明暗变体通常共用：

```csharp
using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace Acme.Themes;

[ControlTokenAlgorithm("Acme", "Brand", 1)]
internal sealed class AcmeToolTipTokens : IControlTokenAlgorithm<ToolTipToken>
{
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ToolTipToken token)
    {
        token.ToolTipCornerRadius = effectiveGlobal.BorderRadius;
        token.ContentPadding      = new Thickness(effectiveGlobal.UniformlyPaddingSM, effectiveGlobal.UniformlyPaddingXS);
    }
}
```

- `EvaluateGlobal` 收到的是该控件自己的一份有效全局 Token，可以写任意全局 Token，只对这个控件生效。写入直接生效，
  不触发 Map 和 Alias 重算（语义同 `ControlAlgorithmMode.Disabled`）。在使用作用域全局 Token 的模式（未指定与
  `Disabled`）下，主题显式设置的 Map/Alias 值胜过这里的写入：阶段写过、而主题又显式设置了的 Map/Alias Token 会被
  写回主题显式值；Seed 是已被 Map 阶段消费的输入，不写回。`Global` 与 `Custom` 模式不写回（见 7.3 节的既有例外）。
  之后再写一次用户对该控件的全局覆盖，它胜过前两者。PrimeNG 的 Card 在这里写卡片作用域的 `ColorBgContainer`
  和 `BorderRadiusLG`。
- `EvaluateOwn` 写该控件的 Own Token，走与 Alias 阶段相同的三步流程，默认计算是该控件的 `CalculateTokenValues`。
  `effectiveGlobal` 只读，写它会报 `StageBoundary`。
- 两个方法都可能写空，PrimeNG 的大多数控件阶段 `EvaluateGlobal` 就是空的。
- 一个类可以只在某种外观下写值。PrimeNG 的 Steps 阶段只关联 `PrimeNG:Nora.Dark`，并在浅色外观下什么都不写
  （`PrimeNGStepsTokens.cs` 摘录）：

  ```csharp
  [ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
  internal sealed class PrimeNGStepsTokens : IControlTokenAlgorithm<StepsToken>
  {
      public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
      {
      }

      public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StepsToken token)
      {
          if (context.Appearance != ThemeAppearance.Dark)
          {
              return;
          }
          token.FinishIconColor = effectiveGlobal.ColorTextLightSolid;
      }
  }
  ```

控件阶段类的要求（不满足时生成器报 `ATOMUITOKEN002`：前四条报在类名上，特性参数无效报在该特性上）：

- 非抽象、非泛型、在本程序集内可访问的类（可以是 `internal`）；
- 恰好实现一次 `IControlTokenAlgorithm<TToken>`；
- 有公开无参构造函数；注册表为每个声明保留一个实例，编译器可能在一次编译里多次调用它；
- `TToken` 是绑定了控件的终端 Own Token 类；`[ControlTokenAlgorithm]` 的 `catalog`、`id` 合法，`revision > 0`。

**没有 Own Token 类的控件没有控件阶段。** `TToken` 必须是控件的 Own Token 类，没有 Own Token 类的控件无法声明控件
阶段；编译器对这类控件也不查控件阶段，所以连 `EvaluateGlobal` 都不会运行。要改这类控件的全局 Token，只能在主题的
`<Control>` 覆盖里写常量。

其他规则：

- 控件阶段的类可以和算法不在同一个程序集：控件包可以为自己的控件提供某个已有算法下的取值。
- 同一个（算法，控件）只能有一个实现，重复时注册表报
  `Duplicate control algorithm 'Acme:Brand' for control 'AtomUI:ToolTip'.`
- 算法或控件没有注册的实现会被接受并保留，但不会被调用。所以身份写错（例如 `"Acme", "Brnad"`）时**没有任何诊断**，
  只是这个阶段不生效；用第 6 节的契约测试或生效测试兜住。
- 控件阶段使用哪条链：`ControlAlgorithmMode.Custom` 用控件自己声明的链，其余三种模式用所在作用域的链。链上关联了
  该控件的每个算法都会运行，后者覆盖前者。

控件 Token 类是公开的 partial 类（`public sealed partial class ToolTipToken`），属性可以直接赋值。抽象中间层
（例如 `AbstractButtonGeometryToken`）也是公开的，可以被外部继承，它们的 `protected` 辅助方法属于公开 API。
`CalculateTokenValues` 是 `protected`，算法调用不到它，也不需要调用。

## 5. 共同约束

所有阶段都必须遵守前三条，第四条是写法上的取舍：

1. **纯函数、可确定。** 相同输入产生相同输出；不读时钟、环境、随机数或静态可变状态。编译缓存依赖这一点，编译器也会
   对同一阶段调用多次。每次运行要写同一组 Token。
2. **不改输入。** `effectiveSeed`、`previousMap`、`EvaluateOwn` 的 `effectiveGlobal` 只读；`context.Algorithms` 也当
   作只读，不要把它强转回数组修改。
3. **不需要重算派生值。** 改了某个字段后，依赖它的 Ant 默认值由编译器重新计算。改 `ControlOutlineWidth` 不需要
   自己去算 `InputActiveShadow`。
4. **尺寸尽量从 Map 的尺寸 Token 推出。** 推出的值会跟着尺寸种子（`ControlHeight`、`BorderRadius` 等）和后接的
   `AtomUI:Compact` 变化；写常量也允许，只是它不跟着变。PrimeNG 包对少数从改造前主题继承下来的固定尺寸用了常量
   （复选框 20/14、Lara 24/14，卡片与对话框内边距 20，提示内边距 12,8），并在 README 里写明；Select 的内边距、
   卡片与对话框的圆角等则从尺寸 Token 推出。确实需要按链区分时可以检查 `context.Algorithms`。

另外，**不支持用一个字段自己的当前值算它的新值**：

```csharp
// Wrong: ControlOutlineWidth's default is guarded by IsExplicit, so every rerun reads the previous write.
token.ControlOutlineWidth = token.ControlOutlineWidth + 1;

// Right: derive it from another token.
token.ControlOutlineWidth = token.LineWidth * 3;
```

Alias 与 Own 阶段会运行不止一次（第 8 节）。带 `IsExplicit` 守卫的字段第二次读到的是算法上一次写的值，结果逐轮累加，
超过 3 轮仍不稳定时编译失败（`Unstable`）。没有守卫的字段即使碰巧稳定，也不在支持范围内。Token 的 getter 是纯字段
读取，编译器没法在读取时替算法兜住这种写法。

## 6. 测试

### 6.1 契约测试

`tests/AtomUI.Theme.ContractTests` 是一个测试支持类库，`ThemeAlgorithmContract.Verify` 对一个已注册的算法跑完规格
4.6 的全部检查，返回违反的条目，空列表表示通过。PrimeNG 的测试（`tests/AtomUI.Themes.Tests/PrimeNGAlgorithmContractTests.cs`
摘录）：

```csharp
[Theory]
[InlineData("Aura")]
[InlineData("Aura.Dark")]
public void Preset_Algorithm_Satisfies_The_Contract(string id)
{
    var violations = ThemeAlgorithmContract.Verify(
        ThemeAlgorithmContractInput.Capture(builder => builder.UsePrimeNGThemes()),
        new ThemeAlgorithm("PrimeNG", id),
        standalone: true);

    violations.ShouldBeEmpty();
}
```

- `ThemeAlgorithmContractInput.Capture(register)` 在一个新的 builder 上运行注册代码，收集 Core 的内置算法、注册的
  算法、经 `IThemeManagerBuilder.AddControlPackage` 加入的控件和控件阶段；不需要 Avalonia 应用。经应用 builder
  启用的控件包不经过这里，用 `input with { Controls = ... }` 补上，否则控件阶段的检查会跳过这些控件。
- `standalone: true` 用于开始一条链的算法（要检查 Map 写满）；`false` 用于只能跟在别的算法后面的算法，例如
  `AtomUI:Dark`、`AtomUI:Compact`。能单独使用的算法必须用 `true` 校验，否则 Map 写满检查会被跳过。
- 检查项以常量给出：`Identity`、`MapComplete`、`Deterministic`、`InputsUnchanged`、`SeedResponsive`、
  `StatesDistinct`、`Readable`（正文对比度 ≥ 4.5，实色上文字 ≥ 3）、`AppearanceConsistent`、
  `ComposableWithCompact`、`HybridChainReadable`、`Stable`、`StageBoundary`。

`Readable` 只用默认种子（主色 `#1677FF`）编译，只检查正文 `ColorText` 对 `ColorBgContainer`、`ColorTextLightSolid`
对 `ColorPrimary` 这两对。包还要用自己主题的种子测可读性，并覆盖主题实际画出来的颜色对：链接、第三级与禁用文字、
选中底色上的选中文字等。PrimeNG 包在自己的迁移中有四处可读性回退是契约检查没有发现的（例如浅色链接色
`ColorLink` 在白底上只有 2.54），后来由 `PrimeNGThemePackageTests` 里按 profile 的对比度用例兜住。

这个库目前只在 AtomUI 仓库内可用：它不打包，靠 `AtomUI.Core` 对它开放的内部成员工作，仓库外的项目引用不到。
发布为 NuGet 包不在本次范围内。仓库外的算法作者暂时只能用自己的测试覆盖同样的检查。

### 6.2 生效测试

契约测试只证明规则自洽，不证明控件真的画出了这个值。对每个改了控件观感的规则，按
[主题生效测试规范](../../engineering/development/theme-effect-tests.md)补生效测试。控件阶段的身份写错没有诊断，
生效测试是唯一能发现它的地方。

## 7. 链的组合

### 7.1 每个算法的可选阶段都会运行

链上每个算法的 Alias 和控件阶段都会运行，不管后面还有没有别的算法；后运行的覆盖先运行的。阶段实现通过
`context.Appearance` 得知整条链的最终外观，通过 `context.Algorithms` 得知链上有哪些算法。

| 链 | 结果 |
| --- | --- |
| `[PrimeNG:Aura]`、`[PrimeNG:Aura.Dark]` | PrimeNG Aura 的浅色、暗色 |
| 以上任一 + `AtomUI:Compact` | 对应的紧凑版。Map 的尺寸由 Compact 缩小，Alias 和控件阶段从尺寸 Token 推出的值跟着变；控件阶段写成常量的尺寸（PrimeNG 的复选框、卡片与对话框内边距、提示内边距）不变 |
| `[PrimeNG:Aura, AtomUI:Dark]`（混合链） | 合法，但**不等于** PrimeNG 的暗色：Map 的颜色由 Ant 的 Dark 规则重算，PrimeNG 的 Alias 和控件阶段在暗色外观下运行。契约只保证结果可读 |

想要 PrimeNG 的暗色，就选 `PrimeNG:Aura.Dark`（或具名 profile `PrimeNGAura.Dark`），不要在浅色链后面接
`AtomUI:Dark`。AtomUI 不做「暗色对应算法」元数据，系统不会自动把 `[PrimeNG:Aura]` 换成 `[PrimeNG:Aura.Dark]`。
Gallery 的全局明暗开关是把 `AtomUI:Dark` 追加到当前链后面，对 PrimeNG 主题得到的就是混合链。

### 7.2 设计算法时要处理两种外观

同一个 Alias 或控件阶段的实现会在混合链里遇到与自己变体相反的外观。按 `context.Appearance` 取规则（第 4.2 节），
或者像 Steps 阶段那样在不适用的外观下什么都不写。

### 7.3 控件的 Global 与 Custom 模式

控件使用 `ControlAlgorithmMode.Global` 或 `Custom` 时，编译器以根主题**编译后**的全局值为种子，为该控件重跑一遍
算法链和 Alias 计算（含算法的 Alias 阶段），然后只写回该控件自己的覆盖。这带来两个后果：

- 主题在根上显式设置的 Map 与 Alias 值不参与这次重新推导，在这个控件的作用域里会被默认公式或算法的 Alias 阶段取代。
  这是本方案之前就有的行为，「算法 < 主题显式值」在这两种模式下只对该控件自己的覆盖成立。
- 链是在已经推导过的种子上重跑的。对自己的输出不幂等的算法会让颜色再偏一档：PrimeNG 的算法（例如 `ColorError`
  已是 400 级，不再是 Tailwind 色板的 500 级，于是走公式生成）和 Ant 的暗色算法都属于这种情况。

目前没有内置主题使用这两种控件模式。

## 8. 编译器如何运行 Alias 与 Own 阶段

Alias 阶段与控件 Own 阶段用同一个三步流程：

1. 标记并写入用户显式值，运行默认计算，再写一次显式值。链上没有算法实现该阶段时到此结束；内置三个算法没有可选
   阶段，所以内置主题走的就是这一步，不打开记账、不多跑公式。
2. 打开写入记账，按链的顺序运行各算法的该阶段，然后关闭记账。Token 的 setter 记下算法写过的属性名，编译器把它们解析
   成槽位并检查是否属于本阶段。什么都没写时结束。
3. 把算法写过的槽位和用户显式槽位一起标记为显式，写一次显式值，重跑默认计算（守卫跳过显式槽位，派生字段读到算法和
   用户的值），再运行一次算法的该阶段，最后写一次显式值。

第 3 步之后比较两次运行写入的槽位和值，不同就再来一轮；第 3 轮仍不同时编译失败。正常的算法一轮就稳定。

## 9. 违反契约时会看到什么

违反可选阶段契约，或算法在任一阶段自己抛异常时，编译器抛 `ThemeAlgorithmContractException`（`InvalidOperationException`
的子类），带 `Algorithm`（`ThemeAlgorithm`）、`TokenName`（可能为 `null`）和 `Violation`
（`ThemeAlgorithmContractViolationKind`）。消息里的 `{algorithm}` 是 `Catalog:Id`，`{stage}` 是 `Map`、`Alias` 或
`Control`（`Map` 只出现在 `Failed` 里）：

| `Violation` | 消息 | 含义与修正 |
| --- | --- | --- |
| `StageBoundary` | `Algorithm '{algorithm}' wrote token '{token}' outside the {stage} stage.` | Alias 阶段写了非 Alias Token；或 `EvaluateOwn` 写了不属于该控件的 Token，或写了只读的 `effectiveGlobal`。把写入挪到正确的阶段 |
| `Unstable` | `Algorithm '{algorithm}' did not stabilize after 3 rounds; unstable token '{token}'.` | 写入 3 轮仍不稳定，通常是用字段自己的当前值算新值（第 5 节），或者每次运行写的 Token 不同 |
| `Failed` | `Algorithm '{algorithm}' failed in the {stage} stage.` | 算法自己抛了异常，原始异常在 `InnerException` 里；`TokenName` 为 `null`。Map 阶段（`Evaluate`，含工厂创建实例）抛异常时 `{stage}` 是 `Map`，内置算法也一样；`EvaluateGlobal` 抛异常时 `{stage}` 也是 `Control` |

在哪里出现：

- **Map 与 Alias 阶段**属于全局编译，违规让这次主题编译整体失败（控件 `Global`/`Custom` 模式为该控件重跑的 Map 与
  Alias 例外，它们随该控件惰性编译，见下一条）。`ApplyThemeAsync` 返回 `Failed` 状态，异常在结果的
  `Exception` 里；初始主题编译失败时 `ThemeManager` 抛 `ThemeLoadException`，契约异常在它的 `InnerException` 链上。
  旧主题保持完整可用。
- **控件阶段**在该控件**第一次被读取**时才编译（控件快照是惰性编译的），违规在那时抛出，之后每次读取都再抛一次。
  它不是编译期诊断，也不让 `ApplyThemeAsync` 失败；包作者通常在控件第一次查资源时才看到它。用契约测试提前发现。

## 10. 发布

### 10.1 包里放什么

```text
Acme.Themes/
+-- Acme.Themes.csproj           引用 AtomUI.Core；有控件阶段时再引用对应的控件包
+-- Algorithms/AcmeBrandAlgorithm.cs
+-- Algorithms/Controls/*.cs     控件阶段，可选
+-- Themes/*.theme.xml           可选：只写算法链和种子
+-- AcmeThemes.cs                UseAcmeThemes()：调用生成的 Register(builder)
\-- README.md                    规则表、来源与版本、与原设计语言的已知差异
```

README 要逐条写清规则的来源（原设计语言的哪个 Token、哪个版本），哪些是自己的提议；PrimeNG 的 README 是可参照的样子。

### 10.2 运行时追加包

`application.GetAtomUI().LoadPackages(...)` 在应用启动后追加包时，规则是追加不得改变已经提交的编译结果：

- 追加的包可以带来新算法，也可以带来控件阶段。
- 控件阶段只有在「它的算法或它的控件是这次追加新注册的」时才被接受。为已经存在的算法和已经存在的控件补一个控件阶段
  会改变已编译的结果，整次追加被拒绝：
  `Package append cannot add a control algorithm for the already registered algorithm '{algorithm}' and control '{control}'.`
- 算法和控件在任何地方都没有注册的控件阶段（休眠实现）与启动时一样被接受并保留。它不会被调用；以后某次追加注册了
  缺的那一边时，那一边对那次追加是新注册，同样不改变已提交的结果。代价同样是身份写错时没有诊断。
- 追加带来新算法后，此前因缺这个算法而加载失败的用户目录主题要等主题目录重新加载才恢复。

### 10.3 IDE 里的假错误

控件 Token 类实现 `IControlToken` 的那一半只在完整构建时生成，IDE 设计期看不到。如果手写
`ControlTokenAlgorithmDescriptor.Create<TToken, TAlgorithm>`，并且 `TToken` 在同一个解决方案里，IDE 可能显示一个
假的 CS0315 错误，构建本身是成功的。正常使用由生成的注册类产出这行代码，不需要手写。

## 11. 性能

Token 对象只在主题编译期间存在；渲染和资源查找读的是编译后冻结的表，不经过 Token 属性，所以算法和记账不影响渲染期。

为了知道算法写了哪些字段，全局 Token 和控件 Token 的属性改成了 partial 属性，生成的 setter 在写入后检查「是否在
记账」。不记账时只多一次空引用判断，每个 Token 实例多 8 字节，不分配内存。这个判断有可测量的代价，在真实仓库里测得
（2026-11-13，默认 Ant 主题，与改造前的树对比）：

- 冷编译：按门槛统计量（25 分位数）慢 2.3%–2.7%，约 0.018 毫秒；按中位数慢 3.4%–3.7%。
- 明暗切换、编译分配（每个 Token 实例 +8 字节）、程序集与 NativeAOT 体积、编译结果都在门槛内。
- 启动耗时按字面规则没有通过：越界大多在更快的一侧；变慢一侧有两项，普通窗口的预编译分段高出基线最大值 0.005 毫秒，
  带启动屏的提交分段高出基线最大值 0.17 毫秒（差值 0.49 毫秒，小于基线自身的波动）。
- 剩余差距主要来自编译器内部对全局 Token 的逐属性复制：一次冷编译约 31,600 次属性写入，其中 94% 是这类复制，
  每次都经过带判断的 setter。

这没有达到规格 4.11 定的门槛（默认主题冷编译差异不超过 2% 等），由项目负责人决定按这个结果通过，是对门槛的一次
明确放宽。把全局 Token 的复制改成整体克隆可以让那 94% 的写入不再经过 setter，但没有实测过，是否单独做未定。

内置三个算法没有可选阶段，记账从不打开，也不多跑默认公式。额外的公式重算只发生在使用了带 Alias 或控件阶段的算法的
主题上，开销只由这些主题承担。

这一点有实测支撑（2026-11-14，与接受记账代价时的树 `3a87bba25` 对比）：内置三条链的冷编译、主题切换都在波动范围内，
每次冷编译的分配逐字节不变，编译结果不变；不过当天冷编译的分辨力只有 8%–9%，这批数据分辨不出 2% 的差异。启动方面，
没有注册控件阶段时控件阶段代码不会被执行（发布前修复了空表也触发 JIT 的问题），启动期间 JIT 的方法集合与基线相同，
耗时在约 1 毫秒的测量分辨力内不变。

你的主题用了阶段会付出多少：以 PrimeNG 为例，同一进程内 `PrimeNGAura` 冷编译约 0.88 毫秒，内置默认主题约 0.74 毫秒
（+18%），分配 1,249,256 字节对 1,138,032 字节（+9.8%）；`PrimeNGAura.Dark` 约 0.89 毫秒（+20%），1,254,568 字节
（+10.2%）。只注册算法和控件阶段而不使用时，内置主题的分配仍正好是 1,138,032 字节。

## 相关文档

- [主题定制指南](customization.md)：Token 层次、暗色与紧凑、Control 算法模式。
- [预设主题包指南](preset-theme-packages.md)：引用和制作预设主题包。
- [主题系统架构](../../architecture/systems/theming/runtime.md)：编译顺序、注册表、缓存。
- [主题定义 XML v1 规范](../../reference/theming/theme-definition-xml-v1.md)：`Algorithm` 元素与 `Catalog` 属性。
- [Control Token 设计规范](../../engineering/development/control-token-guidelines.md)：partial 属性与 `protected override`。
- [编译器诊断规范](../../engineering/development/compiler-diagnostics-guidelines.md)：`ATOMUITOKEN002`、`ATOMUIGEN049`。
- `src/AtomUI.Themes.PrimeNG/README.md`：PrimeNG 的规则表与来源。
