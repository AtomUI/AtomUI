# 制作 AtomUI 字体包

字体包是一个普通的 .NET 类库，它把若干字体文件编译进程序集，并向 Avalonia 的 `FontManager` 注册一个逻辑名
字，让应用可以用 `fonts:<Key>#<FamilyName>` 引用它，而不依赖用户机器上装了什么字体。仓库里有两个现成的例
子：`AtomUI.Fonts.AlibabaSans`（6 个字重）和 `AtomUI.Fonts.AlibabaPuHuiTi`（1 个字重）。本文按这两个包的实际
结构，从零做出一个可用的字体包。

读完你会得到一个能被 `builder.UseXxxFont()` 注册、并真正改变界面文本外观的包。字体子系统的架构约束与 Token
派生规则不在本文范围内，以 [字体子系统架构](../../architecture/systems/typography/overview.md) 为准。

## 1. 注册和生效是两件事

把字体文件编译进程序集并调用 `AddFontCollection`，只是让 Avalonia **知道**有这么一个字族存在。它不会改变任何
一个控件的外观。界面上的文本用什么字体，由主题编译出的 `FontFamily` Token 决定，而那是另一条独立的链路。

```text
注册链路   Assets/*.ttf  ->  EmbeddedFontCollection  ->  FontManager   （与主题无关）
生效链路   WithDefaultFontFamily  ->  FontFamily Token  ->  ControlTheme  （与文件无关）

                        两者只在 Avalonia 解析 FontFamily 字符串的那一刻相遇
```

所以一个新字体包需要两步才能看到效果：注册集合，然后把默认 `FontFamily` Token 指向它。只做第一步，界面不会
有任何变化；只做第二步，字符串解析不到对应字族，会静默回退到链尾的 `$Default`。

`AtomUI.Fonts.AlibabaSans` 是个例外，它不需要第二步。因为 `DesignToken` 的默认 `FontFamily` 已经把它写在链首：

```csharp
FontFamily = FontFamily.Parse(
    "fonts:AlibabaSans#Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, "
  + "Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, $Default");
```

这是一条**前向引用**：`AtomUI.Core` 里的默认 Token 提到了一个它并不引用的程序集。如果应用没有调用
`UseAlibabaSansFont()`，这段字符串里的第一项解析不到，Avalonia 就顺着链往后找。你自己的包不在这条默认链里，
因此必须显式覆盖。

## 2. 建立项目

字体包放在 `src/` 下，命名沿用 `AtomUI.Fonts.<FontName>`。项目文件很短，两个 `ItemGroup` 就是全部内容：

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFrameworks>$(AtomUITargetFrameworks)</TargetFrameworks>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <AvaloniaResource Include="Assets/*" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="../AtomUI.Core/AtomUI.Core.csproj" />
    </ItemGroup>

</Project>
```

三处值得解释。

`$(AtomUITargetFrameworks)` 来自 `build/ProjectDefaults.props`，Debug 下是 `net10.0`，Release 下是
`net10.0`。直接使用这个变量，与产品统一的 .NET 10 目标保持一致。

`AvaloniaResource` 而不是 `EmbeddedResource`。这是 `avares://` 协议能找到文件的前提，用错了编译能过，运行时
解析资源 URI 会失败。

对 `AtomUI.Core` 的引用是为了拿到 `IAtomUIBuilder`，也就是第 4 节那个 `UseXxxFont` 扩展的目标类型。如果你只
打算提供 `AppBuilder` 形式的注册入口，这个引用可以去掉，只留 Avalonia 本体。

建好项目后记得加进 `AtomUI.slnx`，两个现成的字体包都在第 13、14 行附近。

## 3. 放入字体文件并确认字族名

字体文件放 `Assets/`，`.ttf` 和 `.otf` 都可以，上面的通配符会全部收进去。

同一字族的多个字重放在同一个包里，不需要任何额外声明。`AtomUI.Fonts.AlibabaSans` 的 `Assets/` 下有 6 个文件：

```text
AlibabaSans-Light.ttf     AlibabaSans-Regular.ttf   AlibabaSans-Medium.ttf
AlibabaSans-Bold.ttf      AlibabaSans-Heavy.ttf     AlibabaSans-Black.ttf
```

它们共享同一个字族名 `Alibaba Sans`。Avalonia 按字族名把这些面归组，运行时再根据 `FontWeight` 选具体那一
面，所以你不必也无法给每个字重单独起名。

接下来这一步最容易出错：**你需要字体的内部字族名，不是文件名。** `AlibabaPuHuiTi-3-55-Regular.otf` 的字族名
是 `Alibaba PuHuiTi 3.0`，两者没有任何字面关系。写错了不会报错，只会静默回退，表现为「字体包装了但没生效」。

在 macOS 或 Linux 上可以直接查：

```bash
fc-scan --format "%{family}\n" src/AtomUI.Fonts.AlibabaPuHuiTi/Assets/AlibabaPuHuiTi-3-55-Regular.otf
```

输出是一行逗号分隔的候选，不是多行：

```text
Alibaba PuHuiTi 3.0,阿里巴巴普惠体 3.0,Alibaba PuHuiTi 3.0 55 Regular,阿里巴巴普惠体 3.0 55 Regular
```

**取第一个。** 后面几项分别是本地化名称和带子族的全名，写进代码都不对。这里第一项 `Alibaba PuHuiTi 3.0` 正是
`AlibabaPuHuiTiFontConstants.FamilyName` 的值。

同一规则也解释了字重是怎么归组的。扫描 `AlibabaSans` 的 6 个文件：

```text
AlibabaSans-Regular.ttf   Alibaba Sans
AlibabaSans-Bold.ttf      Alibaba Sans
AlibabaSans-Light.ttf     Alibaba Sans,Alibaba Sans Light
AlibabaSans-Medium.ttf    Alibaba Sans,Alibaba Sans Medium
AlibabaSans-Heavy.ttf     Alibaba Sans,Alibaba Sans Heavy
AlibabaSans-Black.ttf     Alibaba Sans,Alibaba Sans Black
```

每一面的第一项都是 `Alibaba Sans`，这就是它们能被当成一个字族的原因。第二项是那一面自己的名字，你不需要用到。

macOS 也可以用「字体册」打开文件看标题，Windows 上双击字体文件看预览窗口顶部。

## 4. 编写 FontCollection

一个类，继承 `EmbeddedFontCollection`，把两个 URI 传给基类：

```csharp
using Avalonia.Media.Fonts;

namespace AtomUI.Fonts.MyFont;

public class MyFontFontCollection : EmbeddedFontCollection
{
    public MyFontFontCollection()
        : base(new Uri("fonts:MyFont", UriKind.Absolute),
               new Uri("avares://AtomUI.Fonts.MyFont/Assets", UriKind.Absolute))
    {
    }
}
```

第一个是**逻辑 URI**，你自己定的键，应用将来用 `fonts:MyFont#...` 引用它。它不需要和程序集名或字族名有关
系，但一旦发布就不能改，改了等于破坏所有下游引用。

第二个是**物理 URI**，必须精确匹配 `avares://<程序集名>/<资源目录>`。程序集名默认等于项目名，如果你在 csproj
里改过 `AssemblyName`，这里要跟着改，否则运行时找不到资源。

## 5. 编写注册扩展

有两个注册入口，服务的是两类宿主。

面向使用 `UseAtomUI` 的应用，扩展 `IAtomUIBuilder`：

```csharp
using AtomUI;
using Avalonia.Media;

namespace AtomUI.Fonts.MyFont;

public static class MyFontThemeManagerBuilderExtensions
{
    public static IAtomUIBuilder UseMyFontFont(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        FontManager.Current.AddFontCollection(new MyFontFontCollection());
        return builder;
    }
}
```

面向不走 AtomUI 主题系统、只用 Avalonia 的宿主，扩展 `AppBuilder`：

```csharp
using Avalonia;

namespace AtomUI.Fonts.MyFont;

public static class AppBuilderExtension
{
    public static AppBuilder WithMyFontFont(this AppBuilder appBuilder)
    {
        return appBuilder.ConfigureFonts(fontManager =>
        {
            fontManager.AddFontCollection(new MyFontFontCollection());
        });
    }
}
```

两者都实现，成本很低，也和现有两个包保持一致。仓库里实际被调用的是 `UseXxxFont`，`WithXxxFont` 目前没有调用
点，属于为独立 Avalonia 宿主预留的入口。

注意两者的时机差别。`UseXxxFont` 立即执行 `AddFontCollection`；`WithXxxFont` 把动作推迟到 Avalonia 初始化字
体子系统时。这也意味着 `UseXxxFont` 内部会触碰 `FontManager.Current`，从而触发字体管理器初始化，所以它应当
在 builder 配置阶段调用，不要更早。

### 命名空间要写上

`AlibabaSans` 的 `ThemeManagerBuilderExtensions.cs` 没有声明命名空间，扩展方法落在全局命名空间里。后果可以
在 Gallery 中直接观察到：Desktop Gallery 调用 `builder.UseAlibabaSansFont()` 却没有 `using
AtomUI.Fonts.AlibabaSans;`，而 Browser Gallery 为了 `UseAlibabaPuHuiTiFont()` 必须写上
`using AtomUI.Fonts.AlibabaPuHuiTi;`。

新包请按 `AlibabaPuHuiTi` 的写法带上命名空间。全局命名空间省掉一行 `using`，代价是无条件污染所有下游的名称
解析，并且让 IDE 无法通过引用关系提示这个方法从哪来。`AlibabaSans` 是历史遗留，不是范例。

### 可选：导出字族名常量

如果你的字族名较长或含版本号，导出一组常量能省掉下游拼字符串出错的机会：

```csharp
namespace AtomUI.Fonts.MyFont;

public static class MyFontFontConstants
{
    public const string FamilyName = "My Font 1.0";
    public const string FontFamily = "fonts:MyFont#My Font 1.0";
}
```

`AlibabaPuHuiTi` 有这组常量，`AlibabaSans` 没有。Browser Gallery 拼默认字体链时用的就是
`AlibabaPuHuiTiFontConstants.FontFamily`，而 `AlibabaSans` 那一段只能手写字面量。字族名越不直观，越值得导出。

### AssemblyInfo

若希望包内类型能在 AXAML 里用 `https://atomui.net` 命名空间引用，加上：

```csharp
using Avalonia.Metadata;

[assembly: XmlnsDefinition("https://atomui.net", "AtomUI.Fonts.MyFont")]
```

两个现成包都有这一行。纯字体包通常不需要在 AXAML 里实例化任何类型，但保持一致没有坏处。

## 6. 让字体真正生效

回到第 1 节的结论：注册完还得改 Token。在 `UseAtomUI` 里调用 `WithDefaultFontFamily`：

```csharp
.UseAtomUI(builder =>
{
    builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
    builder.UseMyFontFont();
    builder.WithDefaultFontFamily(FontFamily.Parse(
        $"{MyFontFontConstants.FontFamily}, Segoe UI, PingFang SC, $Default"));
    builder.UseDesktopControls();
})
```

Browser Gallery 是完整的现成例子，它注册两个包再拼一条链，中西文分工：

```csharp
builder.UseAlibabaSansFont();
builder.UseAlibabaPuHuiTiFont();
builder.WithDefaultFontFamily(FontFamily.Parse(
    $"fonts:AlibabaSans#Alibaba Sans, {AlibabaPuHuiTiFontConstants.FontFamily}, $Default"));
```

拼链时始终把 `$Default` 放在最后。它是 Avalonia 的平台默认字体，缺了它，一旦前面所有字族都解析失败，文本会
落到一个不受控的兜底字体上。

`WithDefaultFontFamily` 只是在 builder 上记下一个值，真正注入发生在 `ThemeManager.AddDefaultFont`。那里有一
条容易被忽略的优先级规则：

```csharp
if (FontFamily is null ||
    config?.Tokens.ContainsKey("FontFamily") == true)
{
    return config;
}
```

**主题配置里显式写了 `FontFamily` Token，`WithDefaultFontFamily` 就完全不生效。** 这是有意的设计，主题作者
的显式声明优先于宿主的默认值。但调试时很容易困惑：代码明明调了 `WithDefaultFontFamily`，界面却没变。先去看
主题 JSON 有没有写 `FontFamily`。

顺带一提，字符串格式本身错误会抛异常，但字符串里某个字族不存在不会。**格式错会炸，字族缺失只会静默回退。**

## 7. 验证

先确认能编译，资源被正确嵌入：

```bash
dotnet build src/AtomUI.Fonts.MyFont/AtomUI.Fonts.MyFont.csproj -c Debug
```

然后接到 Gallery 或你自己的应用里跑起来，肉眼确认文本变了样。字体这件事没有比看一眼更可靠的验证方式。

如果没生效，按这个顺序排查，它对应第 1 节那两条链路：

1. 字族名对不对。用 `fc-scan` 再核一遍，这是最高频的原因。
2. 物理 URI 里的程序集名对不对，尤其是改过 `AssemblyName` 的项目。
3. `UseXxxFont()` 真的被调用了吗。它在全局命名空间时不需要 `using`，容易误以为没接上。
4. 主题配置里有没有 `FontFamily` Token 把你的默认值挡掉了。
5. `AvaloniaResource` 有没有写成 `EmbeddedResource`。

想在代码里确认注册是否成功，可以在注册后查询字族列表：

```csharp
var installed = FontManager.Current.SystemFonts
    .Select(family => family.Name)
    .ToList();
```

`EmbeddedFontCollection` 注册的字族会出现在里面。查不到就说明问题在注册链路，不在 Token。

## 8. 分发与许可

字体包和语言包不同，仓库里没有为它们准备 `.nuspec`。`AtomUI.I18n.PtBR` 那种手写 nuspec 的做法不适用于字体
包，直接 `dotnet pack` 即可，`AvaloniaResource` 会随程序集一起进包。

包内引用通过 `ProjectReference`。注意 `AtomUI.Controls` 引用了 `AtomUI.Fonts.AlibabaSans`，但**没有**调用
`AddFontCollection` —— 引用只保证资源随构建产出，注册仍然是应用的责任。你的包如果被别的库引用，同样不要在库
里替应用做注册决定。

最后一件容易忽略的事：**把字体文件编译进程序集就是在再分发这个字体。** 两个现成包都没有附带许可文件，这是
现状而非榜样。做新包时先确认字体的授权允许嵌入再分发，然后把许可文本一并放进项目。商用字体、以及不少免费商
用字体，在这一点上的条款差别很大。

## 9. 检查清单

新字体包交付前逐项核对：

- [ ] 项目名为 `AtomUI.Fonts.<FontName>`，已加入 `AtomUI.slnx`
- [ ] `TargetFrameworks` 用 `$(AtomUITargetFrameworks)`，没写死 TFM
- [ ] 字体文件在 `Assets/`，通过 `AvaloniaResource` 引入
- [ ] 字族名用 `fc-scan` 或字体册确认过，不是从文件名猜的
- [ ] `EmbeddedFontCollection` 的物理 URI 与实际程序集名一致
- [ ] `UseXxxFont`（`IAtomUIBuilder`）与 `WithXxxFont`（`AppBuilder`）都已提供
- [ ] 注册扩展**声明了命名空间**
- [ ] 字族名不直观时导出了 `FontConstants`
- [ ] 在真实应用里跑起来，肉眼确认生效
- [ ] 字体许可允许嵌入再分发，许可文本已随包提供

## 相关文档

- [字体子系统架构](../../architecture/systems/typography/overview.md) —— 两条链路的完整架构约束
- [字体包与资源注册](../../architecture/systems/typography/font-packages.md) —— `FontCollection` 模型与资产清单
- [字族解析与回退](../../architecture/systems/typography/font-family-resolution.md) —— 回退链、覆盖路径与注入时机
- [主题定制指南](../theming/customization.md) —— Token 覆盖与主题配置
