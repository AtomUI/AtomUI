# AtomUI 多语言公共 API

本文定义应用、类库、ViewModel、服务和 XAML 使用 AtomUI 本地化基础设施时可依赖的公共契约。
Catalog 和 XLIFF 的源文件规则见 [catalog-and-xliff.md](catalog-and-xliff.md)，运行时语义见
[本地化运行时架构](../../architecture/systems/localization/runtime.md)。

## 根 Builder

`AppBuilder.UseAtomUI(...)` 的配置参数是根级 `IAtomUIBuilder`。
根 Builder 组合主题、本地化、控件包和运行时初始化，不把本地化重新塞进主题 Builder。

```text
IAtomUIBuilder
├── IThemeBuilder
├── ILocalizationBuilder
└── Application initializers
```

常用能力继续通过根 Builder 扩展方法暴露，避免调用方依赖内部子 Builder：

```csharp
.UseAtomUI(builder =>
{
    builder.UseLanguages(
        defaultLanguage: LanguageTags.ZhCN,
        supportedLanguages:
        [
            LanguageTags.EnUS,
            LanguageTags.ZhCN,
            LanguageTags.ZhTW
        ]);

    builder.UseDesktopControls();
})
```

命名必须保持以下层次：

- `UseAtomUI()` 是进入 AtomUI 框架的根入口。
- Builder 内使用 `UseLanguages()`、`UseDesktopControls()` 等领域名称，不重复 `AtomUI` 前缀。
- `defaultLanguage` 表示启动后首次提交的应用语言，不改变每个 Catalog 固定的 `en-US` 源语言。
- `defaultLanguage` 必须属于 `supportedLanguages`；Builder 对输入集合做防御性复制，并按首次出现顺序去重。
- 未调用 `UseLanguages()` 时采用 `en-US` 作为默认语言和唯一支持语言，保证最小应用可启动。

`UseLanguages()` 不接收包名、程序集、Descriptor 或语言包列表。应用 Catalog、组件 Catalog 和静态语言包
通过生成链注册，Builder 配置顺序不得影响最终结果。

`ILocalizationBuilder.AddModuleData(string moduleId, string data)` 是生成代码登记版本化模块数据的入口；每个模块只登记一次，
重复模块或无效数据会失败。`LanguageModuleManifestAttribute(int version, string moduleId, string data)` 仅携带构建期
链接元数据，允许运行时保持声明式登记。手工 `AddCatalog`、`AddTranslationBundle`、`AddLanguageDefinition` 与
`ConfigureLanguages` 契约继续有效。协议见[生成与构建](../../architecture/systems/localization/generation-and-build.md#模块数据传输协议-v1)。

## LanguageTag

语言身份使用开放且不可变的 BCP 47 值类型：

```csharp
public readonly record struct LanguageTag
{
    public string Value { get; }

    public static LanguageTag Parse(string value);
    public static bool TryParse(string? value, out LanguageTag language);
    public static LanguageTag FromCultureInfo(CultureInfo culture);
}
```

`LanguageTag` 构造后必须处于规范形式：language subtag 小写、script 首字母大写、region 大写，其他 subtag 按
BCP 47 规范化；已知 grandfathered/alias 映射使用仓库固定的 IANA/CLDR 数据。相等性和哈希只比较规范化后的
完整标签，使用 ordinal 语义。`ToString()` 返回规范标签，`default(LanguageTag)` 是无效值，任何公共入口都必须
拒绝它，不能把空值解释为系统语言或 `en-US`。

`Parse` 和 `TryParse` 用于配置、命令行、持久化和网络数据等字符串边界。普通程序代码应使用生成式常量：

```csharp
LanguageTags.EnUS
LanguageTags.EnGB
LanguageTags.ZhCN
LanguageTags.ZhHans
LanguageTags.ZhTW
LanguageTags.ZhHant
LanguageTags.JaJP
LanguageTags.ArSA
```

`LanguageTags` 根据仓库固定版本的 CLDR/IANA 数据生成常用语言、脚本和区域标签。BCP 47 可以组合脚本、
区域、variant、extension 和 private-use subtag，因此不得用 enum 声称穷举所有合法值。第三方私有标签应在
自己的静态类中封装：

```csharp
public static class AcmeLanguageTags
{
    public static LanguageTag EnAcme { get; } = LanguageTag.Parse("en-x-acme");
}
```

## LanguageDefinition

`LanguageTag` 只表达身份。语言选择器显示名称、格式化 Culture 和文字方向属于注册元数据：

```csharp
public sealed record LanguageDefinition(
    LanguageTag Tag,
    CultureInfo FormattingCulture,
    string NativeName,
    LanguageTextDirection TextDirection);
```

标准标签优先使用生成的 CLDR/.NET 元数据。无法映射到 .NET `CultureInfo` 的私有标签必须由应用显式提供
`LanguageDefinition`；系统不得静默改用 `InvariantCulture`。

```csharp
var privateLanguage = LanguageTag.Parse("en-x-acme");
builder.Localization.AddLanguageDefinition(new LanguageDefinition(
    privateLanguage,
    CultureInfo.GetCultureInfo("en-US"),
    "Acme English",
    LanguageTextDirection.LeftToRight));
builder.UseLanguages(privateLanguage, [privateLanguage]);
```

同一标签重复注册显式 `LanguageDefinition` 会在启动构建时失败；这与 `supportedLanguages` 中允许重复并去重的
集合语义不同。

## ILanguageManager

`ILanguageManager` 是全局应用语言状态的唯一所有者：

```csharp
public interface ILanguageManager
{
    LanguageState Current { get; }
    IReadOnlyList<LanguageDefinition> SupportedLanguages { get; }
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    LanguageChangeResult ChangeLanguage(LanguageTag language);
}
```

静态语言包已经在应用构建时编译完成，因此全局切换是同步 Snapshot 提交，不设计伪异步 API。
`ChangeLanguage()` 必须在 UI 线程调用；切换到当前语言返回 `NoOp`，不重复发布资源通知或事件。
`LanguageChangeResult` 只有 `Committed` 和 `NoOp` 两种成功状态，并携带旧/新 `LanguageState`。未支持标签抛出
`LanguageNotSupportedException`，错误线程或已释放 Manager 抛出 `InvalidOperationException`；静态 Snapshot 不存在
表示启动构建不变量被破坏，不能包装成普通 `Failed` 结果继续运行。

`ILanguageManager` 和 `ILocalizer` 保持 DI 中立。AtomUI 将实例注册到自身运行时服务位置供控件使用，应用可以把
同一实例注册进 Microsoft.Extensions.DependencyInjection、Splat 或自己的容器，不创建第二个 Manager。

## ILocalizer

ViewModel、服务和普通 C# 代码通过 `ILocalizer` 读取当前 Snapshot：

```csharp
public interface ILocalizer
{
    string Get<TResourceKind>(TResourceKind key)
        where TResourceKind : struct, Enum;

    string Format<TResourceKind>(TResourceKind key, params object?[] arguments)
        where TResourceKind : struct, Enum;
}
```

```csharp
var title = localizer.Get(LoginLangResourceKind.Title);
var message = localizer.Format(OrderLangResourceKind.ItemCount, count);
```

`Get` 和 `Format` 根据 enum CLR 类型定位 Catalog，通过 Generator 输出的 enum member switch 定位资源 slot。enum
成员名是构建期 Catalog Key，但运行时不得调用 `ToString()` 或 `Enum.GetName()` 构造资源路径。格式化使用
`LanguageState.FormattingCulture`，参数签名在构建期跨语言校验。

XAML 动态资源自动响应语言变化。ViewModel 如果要缓存已经本地化的字符串，必须订阅 `LanguageChanged` 并重新
计算；更推荐保存稳定资源 enum 或业务状态，在属性 getter/投影阶段调用 `ILocalizer`。

## LanguageCatalogUnitDescriptor

生成的 Catalog registration 使用字符串 Key 描述资源项，公共 descriptor 不再同时暴露数字 `Id` 和字符串
`Name`：

```csharp
public sealed record LanguageCatalogUnitDescriptor
{
    public LanguageCatalogUnitDescriptor(string key, bool isFormatted = false);

    public string Key { get; }

    public bool IsFormatted { get; }
}
```

`Key` 必须与 Catalog enum 成员名及 XLIFF `unit id` 完全一致，使用 ordinal、区分大小写的比较语义。构造函数拒绝
空值和空白，Catalog descriptor 拒绝重复 Key；Key 与 enum symbol 的一致性由 Generator 和 Build Tasks 校验。
`LanguageCatalogUnitDescriptor.Id`、`Name` 及接受数字 ID 的构造函数不保留兼容层，避免重新形成两套身份来源。

## XAML namespace 与 Markup Extension

应用为 Catalog 所在 C# namespace 声明普通 AXAML 别名：

```xml
<UserControl
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:login="using:MyApplication.Features.Login.Localization"
    xmlns:atom="using:AtomUI.Controls">

    <TextBlock Text="{login:LoginLangResource Title}" />
    <TextBlock Text="{atom:DatePickerLangResource Today}" />
</UserControl>
```

`login` 只是当前 AXAML 文件定义的 XML namespace 别名，不是 AtomUI 固定关键字。应优先使用 `login`、
`orders`、`settings` 等功能域名称，而不是含义模糊的全局 `i18n` 别名。

Generator 为 `LoginLangResourceKind` 生成 `LoginLangResourceExtension`。Avalonia 允许在 Markup Extension 使用处
省略 `Extension` 后缀，因此 `{login:LoginLangResource Title}` 在 XAML 编译期解析为
`LoginLangResourceKind.Title`，不是运行时字符串查找。

## 公共 API 中禁止的形态

```csharp
builder.UseAtomUILanguages(...);
builder.UseAtomUIZhHans();
builder.UseAtomUIJaJP();
builder.UseLanguages(packages: ...);
localizer.Get("Login.Title");
```

```xml
<TextBlock Text="{atomui:Loc DatePicker.Today}" />
```

这些形态分别造成重复命名、API 数量随语言增长、语言包运行时泄漏、弱类型字符串键或破坏既有 XAML 契约。
