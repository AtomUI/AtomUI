# AtomUI 与其他控件框架的主题集成

本文面向在同一个 Avalonia 应用中使用 AtomUI、Avalonia 原生控件和其他控件库的开发者，说明主题注册、资源覆盖、
明暗切换及集成验证。AtomUI 的 Token 和模板定制见[主题定制指南](customization.md)，
AtomUI 生态控件包的生成式注册见[第三方 Control Package 指南](third-party-control-packages.md)。

## 1. 选择集成范围

| 应用组成 | 接入方式 | 兼容性边界 |
| --- | --- | --- |
| AtomUI 控件 | 使用 `UseAtomUI` 和实际需要的控件包入口 | 不因集成文档而额外引入 FluentTheme |
| AtomUI 与 Avalonia 原生控件 | 按需引入 FluentTheme 等基础主题 | 双方可能导出相同的 Avalonia 类型资源键，必须确定优先级 |
| AtomUI 与独立第三方控件 | 启用该控件所需的主题及资源 | 类型不同不代表资源、全局 Selector、字体和弹层完全隔离 |
| AtomUI 与另一套完整控件框架 | 按页面和控件明确主题归属，先验证最小组合 | 不承诺任意两套全局主题直接叠加即可兼容 |

主题共存不代表设计语言自动统一。主题色、圆角、尺寸、字体和动画参数分别由各框架拥有；颜色映射使用各框架的公开
配置入口，不复制或改写 AtomUI 的内部 Provider、Token 字典或模板节点。

## 2. 区分三种不同的优先级

### 2.1 控件与资源键的身份

`Avalonia.Controls.Button` 与 AtomUI 的 `Button` 是不同的 CLR 类型，隐式 ControlTheme 键也不同。XAML 前缀只帮助
读者辨认类型，不建立资源隔离边界。

公共基础类型仍可能冲突。例如 AtomUI 和 Fluent 都提供 `WindowDrawnDecorations` 类型键，AtomUI Window 会消费该键。
`ScrollViewer`、`ScrollBar`、Popup 宿主等基础资源也应纳入集成检查，不能只检查业务页面的 Button。

### 2.2 资源查找顺序

资源按真实宿主和作用域查找；不能把“最后加载的框架优先”当成所有场景的规则。在应用级查询中：

1. `Application.Resources` 先于 `Application.Styles` 中的资源。
2. 同一个字典的直接条目先于其合并字典；合并字典之间，同键通常由后加入者优先提供。
3. Styles 容器自身的 Resources 先于其子 Style 的资源；同一 Styles 集合的子项按倒序查询。
4. 控件和页面的较近作用域可能先于 Application 命中。弹层和独立窗口还必须检查真实资源宿主。

这是资源键的解析顺序，不是 Setter 的属性优先级。Local Value、Binding、Style、ControlTheme 和伪类之间的关系
仍遵循 Avalonia 属性系统；调整主题注册顺序不能保证任意 Setter 都覆盖成功。

### 2.3 AtomUI 内部主题顺序

AtomUI 的 Token provider、静态主题资产、控件包与局部 ThemeContext 有自己的稳定顺序。应用通过公开入口配置，
不移动内部字典来改变次序。规则见[惰性物化与资源查询](../../architecture/systems/theming/lazy-materialization.md)。

## 3. 启动时序与适用范围

AtomUI 在 `Application.RegisterServices()` 阶段提交内核，早于 `Application.Initialize()` 和 `App.axaml` 加载。
因此 AXAML 可以使用 AtomUI 资源；Prism 等在 Initialize 中构造 Shell 的宿主也保留这个时序。
完整启动接入见[启动指南](../startup/overview.md)。

6.2.7 的[应用资源宿主实现](../../architecture/systems/theming/application-resource-host.md)将框架资源挂到根
`Application.Styles.Resources`，所以可以正常在 App.axaml 中声明 FluentTheme 与 Application.Resources。
开发者不需要在代码中重排主题、迁移语言字典或在 Loaded 后补资源。

**版本适用说明：**本页适用于包含宿主修复的 6.2.7。6.2.6 以及不含该改动的早期 6.2.7 候选产物仍有
[#506](https://github.com/AtomUI/AtomUI/issues/506) 与 [#507](https://github.com/AtomUI/AtomUI/issues/507) 的限制：
旧产物需要在 Initialize 中先把 FluentTheme 插入 Styles 起始位置，并把应用资源合并到已有 Resources，避免替换旧根字典。
升级到包含宿主修复的产物后，删除这段兼容代码，采用下面的普通 AXAML 写法。实际验证范围见
[资源宿主验证](../../architecture/systems/theming/application-resource-host-verification.md)。

## 4. FluentTheme 接入示例

项目需引用 `AtomUI.Desktop.Controls` 与 `Avalonia.Themes.Fluent`。Fluent 包应与应用解析到的 Avalonia 版本一致，
AtomUI 包应使用同一发布版本。`MyApp` 是本例的程序集名；替换命名空间时同时替换 `avares` URI 中的程序集名。
仓库源码消费和 NuGet 消费的生成器配置差异见[控件包启动接入](../startup/package-authoring.md)。

### 4.1 注册 AtomUI

```csharp
// Program.cs
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Theme;
using Avalonia;

namespace MyApp;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseAtomUIPlatformDetect()
            .WithAtomUIDefaultOptions()
            .UseAtomUI(builder =>
            {
                builder.UseDesktopControls();
                builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS, LanguageTags.ZhCN]);
                builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
            });
}
```

### 4.2 在 AXAML 中声明应用资源与基础主题

```csharp
// App.axaml.cs
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace MyApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
```

生成器负责 partial App 的启动挂接，应用不再调用一次 UseAtomUI 或重建 ThemeManager。
应用资源先于下面 Style 的 StaticResource 消费声明；FluentTheme 只声明一次。

```xml
<!-- App.axaml -->
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">
    <Application.Resources>
        <ResourceDictionary>
            <x:Double x:Key="MyApp.NoteFontSize">14</x:Double>
            <ResourceDictionary.ThemeDictionaries>
                <ResourceDictionary x:Key="Light">
                    <SolidColorBrush x:Key="MyApp.TextBrush">#202020</SolidColorBrush>
                </ResourceDictionary>
                <ResourceDictionary x:Key="Dark">
                    <SolidColorBrush x:Key="MyApp.TextBrush">#F0F0F0</SolidColorBrush>
                </ResourceDictionary>
            </ResourceDictionary.ThemeDictionaries>
        </ResourceDictionary>
    </Application.Resources>
    <Application.Styles>
        <FluentTheme />
        <Style Selector="TextBlock.integration-note">
            <Setter Property="FontSize" Value="{StaticResource MyApp.NoteFontSize}" />
            <Setter Property="Foreground" Value="{DynamicResource MyApp.TextBrush}" />
        </Style>
    </Application.Styles>
</Application>
```

AXAML 文件必须由 Avalonia 构建任务作为资源编译。较大的应用资源可以用 ResourceInclude 放进上述根字典的
MergedDictionaries；不需要借此保留旧的 Application.Resources 实例。

### 4.3 明确每个控件的归属

```xml
<!-- MainWindow.axaml -->
<atom:Window xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:atom="https://atomui.net"
             x:Class="MyApp.MainWindow"
             Width="640" Height="400" Title="Theme Integration">
    <StackPanel Margin="24" Spacing="16">
        <TextBlock Classes="integration-note" Text="AtomUI 与 Avalonia 控件共存" />
        <Button Content="Avalonia Button" />
        <atom:Button Content="AtomUI Button" />
    </StackPanel>
</atom:Window>
```

```csharp
// MainWindow.axaml.cs
using Avalonia.Markup.Xaml;

namespace MyApp;

public partial class MainWindow : AtomUI.Desktop.Controls.Window
{
    public MainWindow() => AvaloniaXamlLoader.Load(this);
}
```

Fluent 为 Avalonia Button 提供主题，AtomUI 为自己的 Button 和 Window 提供主题。窗口装饰仍涉及共享基础类型键，
所以即使页面按钮外观正常，也要单独验收标题栏、最大化、全屏、Popup 和滚动区域。

## 5. 应用覆盖与局部定制

应用自有资源采用带产品前缀的键，避免无意占用其他框架的字符串键。全局公共资源放进应用资源文件，页面独有资源
放在页面或控件的 Resources；Style.Resources 用于该 Style 的资源需求，不能一概视作控件子树的公共资源容器。

需要覆盖一个框架已有的键时，先确认键的类型、值类型、Light/Dark 分支以及消费位置，再有意定义同键资源。
不要凭控件类的短名称判断两个 key 相同；也不要用改名内部资源键来绕过冲突。

**覆盖迁移：**框架资源现在高于 Application.Styles 子项的资源。原来在独立 `Style.Resources` 中用同键
覆盖 AtomUI 全局默认值的应用，应把这些条目移入 `Application.Resources`；普通 Style Setter 和局部资源仍按各自
作用域与属性优先级工作。例如全局替换窗口装饰时，使用 `Application.Resources` 中的
`<ControlTheme x:Key="{x:Type WindowDrawnDecorations}" ...>`，并提供完整有效的窗口装饰主题。
不要为了覆盖资源而调整内部框架前缀，也不要整体替换 `Application.Styles.Resources`；后者是当前框架宿主。
这是可观察的资源优先级变化，已纳入 [6.2.7 迁移说明](../../releases/6.2.7-api-changes.zh-CN.md)。

资源更新有不同语义：

- 会随明暗、语言或运行时配置更新的值使用 DynamicResource 或框架公开的资源扩展。
- 稳定常量、转换器和明确的静态依赖可以使用 StaticResource，且必须在解析时已可用。
- AtomUI Token 使用公开 ThemeConfig、Token resource extension 和 ThemeConfigProvider；第三方 Token 不会自动映射。
- AtomUI 内部稳定区域的样式定制使用生成的专用 Semantic Part Style，见
  [Semantic Part 系统](../../architecture/systems/theming/semantic-parts.md)。

## 6. 统一明暗主题的控制入口

AtomUI 成功提交主题时，会根据最终 Appearance 更新 `Application.RequestedThemeVariant`。遵循 Avalonia
ThemeVariant 的 Fluent 控件和应用 ThemeDictionaries 因此可以一起切换。只设置 RequestedThemeVariant 不会反向
生成 AtomUI Token snapshot；混合应用应让 AtomUI 主题入口负责全局明暗请求。

```csharp
// AppTheme.cs
using AtomUI;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;

namespace MyApp;

public static class AppTheme
{
    public static Task<ThemeTransitionResult> SetDarkModeAsync(bool dark)
    {
        var manager = Application.Current?.GetThemeManager()
            ?? throw new InvalidOperationException("Application is not initialized.");
        var config = dark
            ? new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build()
            : new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default).Build();
        return manager.ApplyThemeAsync(new ThemeRequest(
            IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest));
    }
}
```

本例基于内置默认主题。已有自定义主题的应用应在自己的不可变配置构建流程中保留主题 Id、Token 和 Control 配置，
不要直接用示例配置替换整套产品配置。调用从 UI 线程发起；调用者必须处理返回状态和诊断：Committed/NoOp 表示
已提交或无需更新，Superseded 表示被更新请求取代，Failed 表示失败。PublishDiagnostics 也需检查，不能把通知异常
视为成功传播到所有消费者。

跟随系统使用 `WithFollowSystemThemes(light, dark)` 注册两个有效 ThemeRequest，由 AtomUI 的现有流程接收系统变化。
第三方框架若有独立 ThemeManager，由应用在其公开 API 边界做明确适配；不建立两个管理器互相写回的循环。
局部页面的 ThemeConfigProvider、独立 Window 和 Popup 需分别验证，不能从全局窗口的切换结果推断全部作用域已同步。

## 7. 本地化与资源生命周期

主题切换不等于语言切换。语言通过 UseLanguages 和应用的 LanguageManager 管理；不能因为 GetLocalizer 仍有返回值，
就认定 AXAML 语言资源仍在资源链中。验收必须同时检查强类型资源查询、DynamicResource 更新和 RTL 布局。

应用只能管理自己加入的资源、Style 和订阅。不要 Clear 全局 Styles/MergedDictionaries，不复制语言 Provider，
也不要缓存旧的 Application.Resources 后再覆盖新字典。插件卸载或页面退出时，释放本模块拥有的订阅和资源宿主关系；
不拆卸其他框架仍在使用的资源。主题管理器由应用作用域持有，页面不得为一次换色重建管理器。

## 8. 性能、裁剪和 NativeAOT

只引入实际需要的完整基础主题，每个实例挂载一次。运行时切换通过主题请求和资源通知完成，不反复移除、重插全局
Style，也不依靠 Dispatcher 延时重新排序。加载多套完整主题的构造、绑定和资源成本需要计入应用启动预算。

保留 AtomUI 生成式注册、按键索引和惰性资产加载。未知资源键存在合法回退，不保证所有 miss 都是零物化；也不能
为了性能跳过第三方资源。新增框架前后，应记录首帧、完整 Shell、分配和资产物化数量。测量口径见
[启动验证](../../architecture/systems/startup/verification.md)，资源成本边界见
[惰性物化与资源查询](../../architecture/systems/theming/lazy-materialization.md)。

使用正常编译的 AXAML、静态 URI 和显式控件包入口。不为集成引入程序集扫描、反射构造或运行时 XAML 字符串加载。
JIT 接入成功不代表 NativeAOT 已验证；第三方库及最终应用都需要真实发布验证。本文示例不构成对任意第三方库、
平台或发布模式的兼容性认证。

## 9. 排查与交付检查

| 现象 | 优先检查 |
| --- | --- |
| 标题栏变成另一套外观 | WindowDrawnDecorations 实际命中的 provider、主题重复注册及加载顺序 |
| 语言服务存在但文本为空 | 是否使用包含宿主修复的版本、根 Styles.Resources 是否被替换、语言 key 是否仍能查询 |
| 明暗只改变一部分控件 | AtomUI 请求是否提交、Requested/ActualThemeVariant、局部覆盖和第三方独立管理器 |
| StaticResource 解析失败 | 字典是否先于消费端加载、URI 程序集名、资源是否被编译 |
| 自定义 Style 无效 | 实际 CLR 类型、资源作用域、属性优先级及公开 Semantic Part Style 是否命中 |
| 启动变慢或首次打开卡顿 | 重复全局主题、未知键回退、提前资产物化和首次模板创建成本 |
| JIT 正常而 AOT 失败 | 第三方 trim/AOT 支持、动态加载路径及发布后的真实资源保留 |

交付检查包括：首次显示、Light/Dark 往返、语言及 RTL、应用显式覆盖、AtomUI/原生/第三方控件、标题栏、Popup、
滚动与独立窗口。比较更改前后的资源归属和性能；关闭页面后确认其订阅与宿主可释放。
涉及平台窗口的结果按实际运行的平台记录，Headless 不代替原生标题栏画面与交互验收。

当前加载顺序和根字典替换问题可分别查阅 [#506](https://github.com/AtomUI/AtomUI/issues/506) 与
[#507](https://github.com/AtomUI/AtomUI/issues/507)。这些记录用于核对版本适用性，不代表修复已经发布。

## 10. 继续阅读

- [主题定制指南](customization.md)：AtomUI 自身的主题与控件定制。
- [WindowDrawnDecorations](../windowing/window-drawn-decorations.md)：窗口装饰契约。
- [本地化运行时](../../architecture/systems/localization/runtime.md)：语言状态与资源发布。
- [Avalonia Themes](https://docs.avaloniaui.net/docs/styling/themes)：Fluent 等基础主题的官方说明。
- [Avalonia Resources](https://docs.avaloniaui.net/docs/app-development/resource-dictionary)：资源作用域与字典。
- [Avalonia Theme Variants](https://docs.avaloniaui.net/docs/styling/theme-variants)：Requested/ActualThemeVariant。
- [Avalonia Control Themes](https://docs.avaloniaui.net/docs/styling/control-themes)：ControlTheme 与 Selector 的区别。
