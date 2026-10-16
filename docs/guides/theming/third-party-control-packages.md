# 第三方 AtomUI Control Package 指南

> 本文描述本地源码已实现、经过独立包作者与冷消费验证的接入方式；不表示旧发布版本已提供新 helper。
> 产品状态以 [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md#1-状态与事实边界)为准。

第三方控件包只需正常提供 Control、可选 Token、编译型 Theme 和一个公开包入口。
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
```

目录用于组织源码，不决定控件保留范围。无需聚合所有主题的 AXAML 清单或逐控件注册文件。

## 2. Control、Token 与 Theme

Control 使用正常 Avalonia 类型：

```csharp
using Avalonia.Controls.Primitives;

namespace Acme.Controls;

public class Rating : TemplatedControl
{
}
```

只有存在无法由 Global Token 表达的专属设计值时，才添加 Own Token：

```csharp
using AtomUI.Theme.DesignTokens;

namespace Acme.Controls;

[ControlDesignToken]
internal sealed class RatingToken : AbstractControlDesignToken
{
    public double StarSize { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        StarSize = EffectiveGlobalToken.ControlHeight;
    }
}
```

主题使用编译型 AXAML，明确导出 key 与 TargetType：

```xml
<ResourceDictionary
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:acme="using:Acme.Controls"
    x:ClassModifier="internal">
  <ControlTheme x:Key="{x:Type acme:Rating}"
                TargetType="acme:Rating">
    <!-- 正常编写模板、强类型 TokenResource 与资源引用。 -->
  </ControlTheme>
</ResourceDictionary>
```

生成器从真实契约生成 Token/语义 descriptor、主题资产和逐控件注册片段。internal presenter 可以只拥有主题资源，
不必为了注册而公开类型或伪造 Token。使用生成的专用 Semantic Part Style 定制部件，保持原有声明式样式规则。

上游抽象 Token 与下游 `sealed` 终端 Token 都应显式标记 `[ControlDesignToken]`；抽象层不产生运行时 identity。
跨程序集继承、属性冲突和默认计算调用规则见
[Control Design Token 继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)。

## 3. 普通包入口

包通过生成的不可变 `Definition` 声明显式入口，无需创建每包 Provider：

```csharp
using AtomUI;
using AtomUI.Generated.AcmeControls;

namespace Acme.Controls;

public static class ThemeManagerBuilderExtensions
{
    public static IAtomUIBuilder UseAcmeControls(this IAtomUIBuilder builder)
        => GeneratedControlPackageRegistration.Definition.Register(builder);
}
```

生成命名空间由程序集名确定，生成文件不需要提交到 Git。依赖使用 `ControlPackageOptions.DependsOn`，应用级初始化
使用 `OnCommit`，首次消费初始化使用 `OnFirstUse`；服务与订阅通过 `context.Own(...)` 交给应用管理。生成式语言模块
随包声明注册，配置阶段不访问平台对象。完整示例与版本规则见[控件包启动接入](../startup/package-authoring.md)。

同一包入口也能传给 `application.GetAtomUI().LoadPackages(builder => builder.UseAcmeControls())`，由 UI 线程原子追加。
直接重复注册和循环依赖会失败；依赖与显式 `Ensure` 幂等，不重复运行已启用包的配置或服务。

## 4. 应用启用

应用只安装控件包并调用普通入口：

```csharp
.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseAcmeControls();
})
```

然后正常使用 `<acme:Rating />`、`new Rating()`、生成的 Token 资源键或专用 Semantic Part Style。
编译型 AXAML 内的内部控件引用同样参与自动保留，无需向应用暴露内部注册细节。

仅引用 NuGet 或程序集不会启用可选包。应用需要调用其入口，框架不会根据元数据扫描结果自动运行 provider、语言或 initializer。
直接/传递 ProjectReference、普通预编译 adapter DLL 与 NuGet 使用同一入口；adapter 不维护控件使用清单。

普通非裁剪运行使用完整注册内容。Desktop NativeAOT、trimmed CoreCLR、Browser trimmed interpreter 与 Browser AOT
使用自动选择后的内容；发布失败不会静默退回整包注册。工具链检查和 Browser 后端由产品构建资产自动接入。

## 5. 资源应表达实际依赖

主题依赖的 converter、命名主题或辅助资源应在本地、词法父作用域、显式 ResourceInclude/MergedDictionaries，
或正常公共包资源中提供。不要依赖另一个未使用控件的私有字典偶然被完整注册。

StaticResource 的包内来源必须明确；不同局部字典可以有同名 key，它们不会被全局合并。
没有上述可见来源的 raw DynamicResource 可以继续作为宿主输入，在普通主题 API/文档中说明由宿主提供即可。
其他未 include 的私有字典含同名 key，不会自动成为依赖，也不能单独使合法宿主输入失败。

一个字典导出多个控件主题时，该字典保持完整并只挂载一次。显式嵌套字典的声明顺序保留；框架保证完整与裁剪模式中
共同保留资产的相对优先级一致。资源构造循环需要修正，类型链接闭包不能消除运行时资源递归。

平台声明按控件与资源的实际职责分别放置，接入方式见下一节；跨平台控件无需额外配置。
强类型 Token identity 带有实际 owner；从字符串构造的 identity 仍只是查找数据，不承诺自动保留控件。

动态 URI、运行时 AXAML、运行时类型名与发布后插件遵守 .NET 的裁剪/AOT 限制。
需要动态构造时优先使用正常的已知类型静态工厂，并按官方类型保留规则表达边界。

### 5.1 平台声明

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

## 6. 发布前验证

包作者验证的是产品行为：

- 普通构建与真实裁剪发布都能启动、创建控件模板、解析 Token 并应用专用 Semantic Part Style。
- 内部 presenter、命名主题、多目标资源和显式 include 正常工作，主题切换不改变注册 schema。
- 包入口的配置、语言与 initializer 顺序符合约定；未启用的可选包没有初始化副作用。
- 干净 NuGet cache 的消费应用只引用产品包即可构建，不需要手工添加构建工具或配置类型映射。
- 声明支持 Browser 时，分别运行 trimmed interpreter 与 AOT 发布产物，证明未用控件/资源被裁剪。
- 构建工具不进入运行时依赖、应用输出和发布目录。

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

主题资产导出多个平台能力不同的目标时，应拆分资源；产品确实只提供共同平台上的主题时，按第 5.1 节方式
在资源类上明确声明限制。生成器不得静默求交集丢失主题；资产引用的 Token owner 必须覆盖其有效域。
域计算与 `ATOMUIREG006` 边界见[平台分支](../../architecture/foundations/aot-typemap-registration.md#7-平台分支)，
产品验证与发布状态以本文开头的状态链接为准。
