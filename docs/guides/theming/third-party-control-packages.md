# 第三方 AtomUI Control Package 指南

> 本文描述本地源码已实现、经过独立包作者与冷消费验证的接入方式；不表示旧发布版本已提供新 helper。
> 产品状态以 [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md#1-状态与事实边界)为准。

第三方控件包只需正常提供 Control、可选 Token 声明、编译型 Theme 和一个公开包入口；有控件 Token 时，再为支持的每种设计语言
提供一个取值包（`Acme.Controls.Themes.<Lang>`），给出这些 Token 在该语言下的取值。
应用通过 `UseAcmeControls()` 启用包，按平常方式在 C# 或 AXAML 中使用控件；裁剪发布自动保留需要的注册内容，
包括 Browser。作者不维护 AOT 控件名单、类型映射或发布模式分支。

## 1. 项目与包身份

目标产品框架为 `net10.0`；Browser 应用使用对应 Browser TFM。最小包项目形态如下，产品版本由普通包版本管理提供：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <PackageId>Acme.Controls</PackageId>
    <AssemblyName>Acme.Controls</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AtomUI.Desktop.Controls" />
  </ItemGroup>
</Project>
```

未使用 Central Package Management 时，为 PackageReference 填写采用本接入方式的实际产品版本。
正常产品包自动携带普通生成器、资源构建工具和发布后端，包作者无需另外引用或配置这些工具。

包资源身份默认由普通 PackageId/AssemblyName 推导。保持其稳定，并让 provider 的 Id 与该身份一致。
包身份负责资源与生命周期，不要求作者给每个控件、目录或主题另写注册身份。

推荐组织为：

```text
Acme.Controls/
├── Acme.Controls.csproj
├── Rating/
│   ├── Rating.cs
│   ├── RatingToken.cs              可选
│   └── Themes/RatingTheme.axaml
└── ThemeManagerBuilderExtensions.cs

Acme.Controls.Themes.AntDesign/        one value package per supported language
├── Acme.Controls.Themes.AntDesign.csproj
├── Rating/RatingTokenRule.cs
└── AntDesignAcmeControlsExtensions.cs  AddAcmeControls()
```

取值包引用控件包与语言包（以及控件包所依赖控件包在同一语言下的取值包），不引用其他语言的包：

```xml
<ItemGroup>
  <PackageReference Include="Acme.Controls" />
  <PackageReference Include="AtomUI.DesignLanguages.AntDesign" />
  <!-- Acme.Controls builds on AtomUI.Desktop.Controls, so take its Ant Design values too -->
  <PackageReference Include="AtomUI.Desktop.Controls.Themes.AntDesign" />
</ItemGroup>
```

目录用于组织源码，不决定控件保留范围。无需聚合所有主题的 AXAML 清单或逐控件注册文件。

## 2. Control、Token 与 Theme

本节与下一节以仓库里可运行的对等性样例为例：`tests/fixtures/DesignLanguageParity/` 只用公开 API 写成，由
`tests/AtomUI.DesignLanguages.Parity.Tests` 在 Headless 下验证渲染结果与契约。

| 样例工程 | 角色 | 引用 |
| --- | --- | --- |
| [`Sample.Controls`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls/) | 控件包：`SampleBadge`、`SampleTag`、它们的 Token 声明与 ControlTheme | `AtomUI.Core` |
| [`Sample.Controls.Themes.AntDesign`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls.Themes.AntDesign/) | 取值包：两个控件在 Ant Design 下的取值 | 控件包、`AtomUI.DesignLanguages.AntDesign` |
| [`Sample.DesignLanguage`](../../../tests/fixtures/DesignLanguageParity/Sample.DesignLanguage/) | 语言包：派生自 Ant Design 的品牌语言 `Sample` | `AtomUI.Core`、`AtomUI.DesignLanguages.AntDesign` |
| [`Sample.Controls.Themes.Sample`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls.Themes.Sample/) | 取值包：控件在 `Sample` 语言下的取值，只覆盖 Badge | 控件包、样例语言包、样例的 Ant Design 取值包 |

只写控件包、只支持 Ant Design 的第三方只需要前两个工程；后两个说明怎样为一个派生语言追加取值。

Control 使用正常 Avalonia 类型（[`SampleControls.cs`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls/SampleControls.cs)）：

```csharp
public class SampleBadge : TemplatedControl;

public class SampleTag : TemplatedControl;
```

只有存在无法由 Global Token 表达的专属设计值时，才添加控件 Token。Token 类只有声明，不含任何取值
（[`SampleTokens.cs`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls/SampleTokens.cs)）：

```csharp
[ControlDesignToken]
public sealed partial class SampleBadgeToken : AbstractControlDesignToken
{
    /// <summary>
    /// Surface color of the badge.
    /// </summary>
    public partial Color BadgeBackground { get; set; }

    /// <summary>
    /// Edge length of the badge.
    /// </summary>
    public partial double BadgeSize { get; set; }
}
```

- Token 类必须是 `public partial` 类，Token 属性必须是 partial 属性，实现部分由生成器产出（否则报 `ATOMUIGEN049`）。
- Token 类不得包含计算方法、默认值初始化器或对全局 Token 的引用：它在每种设计语言下等于多少，由该语言的**取值包**给出。
- 一个值如果只是「这个控件上的某个全局值」，不要声明新的控件 Token：模板直接读该控件的 Effective Global Token，语言或主题需要时在
  控件视图里覆盖那个全局 Token。
- 语义说明是语言作者的唯一依据：写清画在哪里、表达什么状态、与哪个 Token 成对。
- 控件包不引用任何语言包或取值包；违反时生成器报 `ATOMUIGEN053`。生成器为控件包产出 `[assembly: TokenSchemaDigest]`，记录它的控件
  Token Schema 摘要。

主题使用编译型 AXAML，模板经生成的强类型 `XxxTokenResource` 读 Token
（[`SampleTagTheme.axaml`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls/Themes/SampleTagTheme.axaml)）：

```xml
<ControlTheme x:Key="{x:Type sample:SampleTag}" TargetType="{x:Type sample:SampleTag}">
    <Setter Property="Template">
        <ControlTemplate TargetType="{x:Type sample:SampleTag}">
            <Border Name="PART_Surface"
                    Background="{sample:SampleTagTokenResource TagBackground}" />
        </ControlTemplate>
    </Setter>
</ControlTheme>
```

生成器从真实契约生成 Token/语义 descriptor、主题资产和逐控件注册片段。internal presenter 可以只拥有主题资源，
不必为了注册而公开类型或伪造 Token。使用生成的专用 Semantic Part Style 定制部件，保持原有声明式样式规则。

上游抽象 Token 与下游 `sealed` 终端 Token 都应显式标记 `[ControlDesignToken]`；抽象层不产生运行时 identity。
跨程序集继承与属性冲突规则见
[Control Design Token 继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)。

## 3. 取值包

控件包与语言包互不引用，它们只在**取值包**里相遇。取值包命名为 `<控件包名>.Themes.<Lang>`，引用控件包与语言包，为控件包里的每个
控件 Token 类写一个控件取值规则（[`AntDesignSampleTokenRules.cs`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls.Themes.AntDesign/AntDesignSampleTokenRules.cs)）：

```csharp
using AtomUI.Generated.SampleControlsThemesAntDesign;

[ControlTokenRule]
internal sealed class AntDesignSampleBadgeTokenRule : IControlTokenRule<AntDesignToken, SampleBadgeToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SampleBadgeToken> target)
    {
        target.Own.BadgeBackground = target.Global.ColorPrimary;
        target.Own.BadgeSize       = target.Global.ControlHeightSM;
    }
}

public static class AntDesignSampleControlsExtensions
{
    public static DesignLanguageBuilder<AntDesignLanguage> AddSampleControls(this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
```

- `target.Own` 必须写全（`ATOMUIGEN050`），不得先读后写（`ATOMUIGEN051`）；`target.Global` 是该控件看到的全局 Token，写入只对该控件生效，
  且只能写 Map 与 Alias：写种子在运行时抛出。主题与作用域给该控件的显式值在推导之前冻结，推导只做顺序赋值，不判断「用户是否设置过」。
- 取值包漏掉控件包里某个控件 Token 类的推导时，构建报 `ATOMUIGEN052`。取值包不得声明控件或 ControlTheme（`ATOMUIGEN053`）。
- 生成器产出 `GeneratedControlTokenRuleRegistration`（命名空间 `AtomUI.Generated.<程序集名去掉非字母数字字符>`）与
  `[assembly: CoversTokenSchema]`，记录构建时控件包的 Token Schema 摘要。
- `Add<控件包>()` 是 `DesignLanguageBuilder<TLanguage>` 上的扩展方法，**只登记取值**，不启用控件包；控件包依赖别的控件包时，在这里先调用
  依赖包的 `Add…()`。
- 取值包与控件包同仓、同版本发布，对语言包声明最低版本。

推导的完整规则见[编写设计语言](writing-design-languages.md#4-取值包)。

**按投入选择形态。**

| 形态 | 做法 | 结果 |
| --- | --- | --- |
| 完整 | 控件包 + 每种支持的语言一个取值包（样例的做法） | 与第一方同等；别人可以引用你的控件包为它追加语言 |
| 单程序集 | 控件、Token 与所支持语言的推导放在同一个程序集，引用这些语言包 | 最省事；控件程序集绑定了它支持的语言 |
| 通用 | 只提供绑定到根类型的推导 `IControlTokenRule<LanguageDesignToken, TToken>`，不引用任何语言包 | 在任何语言下都能给出取值（只能读全局 Token），外观协调但不对标 |

**为派生语言追加取值。** 派生语言的取值包只为改动的控件写推导，其余沿语言 Token 的继承链落到基语言的推导上
（[`SampleSampleTokenRules.cs`](../../../tests/fixtures/DesignLanguageParity/Sample.Controls.Themes.Sample/SampleSampleTokenRules.cs)）：

```csharp
using AtomUI.Generated.SampleControlsThemesSample;

[ControlTokenRule]
internal sealed class SampleSampleBadgeTokenRule : IControlTokenRule<SampleToken, SampleBadgeToken>
{
    public void Apply(in TokenEvaluationContext<SampleToken> context, ControlTokenTarget<SampleBadgeToken> target)
    {
        target.Own.BadgeBackground = context.Language.Accent;   // a language token of the derived language
        target.Own.BadgeSize       = target.Global.ControlHeight;
    }
}

public static class SampleSampleControlsExtensions
{
    public static DesignLanguageBuilder<SampleLanguage> AddSampleControls(this DesignLanguageBuilder<SampleLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        // Registers the base language's values the sample language inherits (here: SampleTag).
        language.WithAntDesignValues(static ant => ant.AddSampleControls());
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
```

`WithAntDesignValues` 定义在样例语言包里，用 `new DesignLanguageBuilder<AntDesignLanguage>(language.Root)` 调用 Ant Design 取值包的
`Add…()`：基语言的取值被登记，但 Ant Design 本身不必启用。构建期的 `ATOMUIGEN052` 不要求派生语言的取值包写全，缺的推导由启动时的
覆盖校验沿继承链确认。

## 4. 普通包入口

包通过生成的不可变 `Definition` 声明显式入口，无需创建每包 Provider：

```csharp
public static class SampleControlsBuilderExtensions
{
    public static IAtomUIBuilder UseSampleControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return AtomUI.Generated.SampleControls.PackageMetadata.Definition.Register(builder);
    }
}
```

生成命名空间由程序集名确定，生成文件不需要提交到 Git。依赖使用 `ControlPackageOptions.DependsOn`，应用级初始化
使用 `OnCommit`，首次消费初始化使用 `OnFirstUse`；服务与订阅通过 `context.Own(...)` 交给应用管理。生成式本地化语言模块
随包声明注册，配置阶段不访问平台对象。完整示例与版本规则见[控件包启动接入](../startup/package-authoring.md)。

同一包入口也能传给 `application.GetAtomUI().LoadPackages(builder => builder.UseAcmeControls())`，由 UI 线程原子追加。
直接重复注册和循环依赖会失败；依赖与显式 `Ensure` 幂等，不重复运行已启用包的配置或服务。追加的取值包执行与启动相同的覆盖校验；
新推导若会改变已提交语言下已提交控件的取值，整次追加被拒绝。

## 5. 应用启用

应用安装取值包（它把控件包与语言包带进来），启用控件包，再在每种已启用的语言下登记它的取值
（[`SampleParityTests.cs`](../../../tests/AtomUI.DesignLanguages.Parity.Tests/SampleParityTests.cs) 的两个应用 builder）：

```csharp
// Ant Design only
.UseAtomUI(builder => builder
    .UseSampleControls()
    .UseAntDesign(ant => ant.AddSampleControls()));

// the derived Sample language; Ant Design itself is not enabled
.UseAtomUI(builder => builder
    .UseSampleControls()
    .UseSample(sample => sample.AddSampleControls()));
```

真实的桌面应用同时启用 AtomUI 的桌面控件：

```csharp
.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseAcmeControls();
    builder.UseAntDesign(language => language.AddDesktopControls().AddAcmeControls());
})
```

然后正常使用 `<acme:Rating />`、`new Rating()`、生成的 Token 资源键或专用 Semantic Part Style。
编译型 AXAML 内的内部控件引用同样参与自动保留，无需向应用暴露内部注册细节。

启动时（配置提交处）按以下规则校验，任一不满足即失败，不会带着缺值运行：

- 每个已启用的语言都为每个已启用控件包里的每个控件 Token 类找得到推导；缺了时消息写出应安装、启用的取值包，例如
  `'Acme:Rating' under 'Shadcn' (install and enable 'Acme.Controls.Themes.Shadcn')`。
- 取值包记录的 Token Schema 摘要与实际加载的控件包一致；控件包新增或修改了 Token 而取值包没有重新构建时，消息写出两个包名与两个摘要。

仅引用 NuGet 或程序集不会启用可选包。应用需要调用其入口，框架不会根据元数据扫描结果自动运行 provider、语言或 initializer。
直接/传递 ProjectReference、普通预编译 adapter DLL 与 NuGet 使用同一入口；adapter 不维护控件使用清单。

普通非裁剪运行使用完整注册内容。Desktop NativeAOT、trimmed CoreCLR、Browser trimmed interpreter 与 Browser AOT
使用自动选择后的内容；发布失败不会静默退回整包注册。工具链检查和 Browser 后端由产品构建资产自动接入。

## 6. 资源应表达实际依赖

主题依赖的 converter、命名主题或辅助资源应在本地、词法父作用域、显式 ResourceInclude/MergedDictionaries，
或正常公共包资源中提供。不要依赖另一个未使用控件的私有字典偶然被完整注册。

StaticResource 的包内来源必须明确；不同局部字典可以有同名 key，它们不会被全局合并。
没有上述可见来源的 raw DynamicResource 可以继续作为宿主输入，在普通主题 API/文档中说明由宿主提供即可。
其他未 include 的私有字典含同名 key，不会自动成为依赖，也不能单独使合法宿主输入失败。

一个字典导出多个控件主题时，该字典保持完整并只挂载一次。显式嵌套字典的声明顺序保留；框架保证完整与裁剪模式中
共同保留资产的相对优先级一致。资源构造循环需要修正，类型链接闭包不能消除运行时资源递归。

平台声明按控件与资源的实际职责分别放置，接入方式见第 6.1 节；跨平台控件无需额外配置。
强类型 Token identity 带有实际 owner；从字符串构造的 identity 仍只是查找数据，不承诺自动保留控件。

动态 URI、运行时 AXAML、运行时类型名与发布后插件遵守 .NET 的裁剪/AOT 限制。
需要动态构造时优先使用正常的已知类型静态工厂，并按官方类型保留规则表达边界。

### 6.1 平台声明

控件本身不支持某个平台时，在控件声明上使用标准 .NET attribute。生成器已支持读取此类类型域，普通主题通过
TargetType 自动取得相应限制，不需要重复列出主题路径：

```csharp
using System.Runtime.Versioning;
using Avalonia.Controls;

[UnsupportedOSPlatform("browser")]
public class NativeWindowHost : Control
{
}
```

若只有本包提供的某套主题不支持 Browser，保留控件本身的平台能力，把 attribute 放在正常主题资源类上。
已有强类型 ControlTheme 直接复用其类；没有资源类的叶子字典（例如 `Themes/NativeOverlayTheme.axaml`）
可声明自己的 `x:Class`。聚合用的 `*Themes.axaml` 不作为独立注册资产：

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    x:Class="Acme.Controls.Themes.NativeOverlayTheme"
                    x:ClassModifier="internal">
    <!-- 保留原有主题、资源键和字典内容 -->
</ResourceDictionary>
```

```csharp
using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Acme.Controls.Themes;

[UnsupportedOSPlatform("browser")]
internal partial class NativeOverlayTheme : ResourceDictionary
{
    public NativeOverlayTheme() => AvaloniaXamlLoader.Load(this);
}
```

这是实际可加载的资源类，不是独立的 AOT 标记。外部库类型的本包主题也采用此方式，无需外部库改源码。
没有资源 CLR 类型时，资源自身的平台上界来自所属程序集；不能因为未写 `x:Class` 而忽略程序集限制。
声明不会按目录传播，资源移动也不会自动改变支持的平台。

使用配套生成器普通重建并按[迁移说明](../../releases/6.2.2-api-changes.zh-CN.md#资源平台声明收敛)删除旧元数据。
应用仍只调用普通包入口，包作者无需维护平台主题路径清单；普通跨平台主题也无需额外资源类。

## 7. 发布前验证

包作者验证的是产品行为：

- 普通构建与真实裁剪发布都能启动、创建控件模板、解析 Token 并应用专用 Semantic Part Style。
- 内部 presenter、命名主题、多目标资源和显式 include 正常工作，主题切换不改变注册 schema。
- 包入口的配置、语言与 initializer 顺序符合约定；未启用的可选包没有初始化副作用。
- 干净 NuGet cache 的消费应用只引用产品包即可构建，不需要手工添加构建工具或配置类型映射。
- 声明支持 Browser 时，分别运行 trimmed interpreter 与 AOT 发布产物，证明未用控件/资源被裁剪。
- 构建工具不进入运行时依赖、应用输出和发布目录。
- 每个取值包用契约测试库 `tests/AtomUI.Theme.ContractTests` 跑一遍：`DesignLanguageContractInput.Capture` 执行注册回调，
  `DesignLanguageContract.Verify(input, "<LanguageId>")` 在语言声明的每个变体下检查推导写全、纯、确定与可读，空列表即通过
  （样例：`SampleParityTests.The_Sample_Language_Passes_The_Design_Language_Contract`）。契约测试不证明取值**对**，取值还要按取值包 README
  声明的等级（A 对标 / B 协调）做视觉验收。
- 控件代码的主题消费检查（`ATOMUIGEN043`、`044`、`047`）只在控件包内运行：声明了 `[ControlDesignToken]` Token 类型的包自动启用，其余第三方控件包在项目文件里设置 `<AtomUIThemeConsumptionAnalysis>true</AtomUIThemeConsumptionAnalysis>` 显式启用；应用项目不会收到这些诊断。

升级到此生成 ABI 的控件包需要使用对应 SDK 重新构建；普通消费 adapter 不需要新增注册分析产物。
更完整的类型与资源规则见 [控件注册契约](../../architecture/foundations/control-registration-contracts.md)，
工具职责见 [控件注册生成](../../modules/generator/control-registration.md)。

## 显式主题资源依赖

跨资产引用字符串命名主题或非默认 type-key 主题时，在消费字典的 `ResourceDictionary.MergedDictionaries` 中声明普通
`ResourceInclude`。只存在于同包其他字典中的同名 `StaticResource` 不证明依赖，普通编译报告带位置的
`ATOMUIREG004`。显式 include 保留通常的本地资源优先级与 merged dictionary 声明顺序，并为官方链接器提供真实工厂引用。
不需要作者列出 AOT 控件或手写 TypeMap。

真实 `{x:Type T}` 若恰好引用相同 T 的已注册默认主题，并且提供者可用域覆盖消费域，编译出的类型引用本身就是
现有 TypeMap 的保留条件。这种受验证的默认 type-key 引用继续按环境作用域查找，保留应用级默认主题替换。

主题资产导出多个平台能力不同的目标时，应拆分资源；产品确实只提供共同平台上的主题时，按第 6.1 节方式
在资源类上明确声明限制。生成器不得静默求交集丢失主题；资产引用的 Token owner 必须覆盖其有效域。
域计算与 `ATOMUIREG006` 边界见[平台分支](../../architecture/foundations/aot-typemap-registration.md#7-平台分支)，
产品验证与发布状态以本文开头的状态链接为准。
