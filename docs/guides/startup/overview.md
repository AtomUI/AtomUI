# 启动指南

本文说明如何在应用中接入 AtomUI 的启动流程：入口、应用类挂接、Splash、异步加载主窗体与 Prism。设计与契约见
[启动系统架构](../../architecture/systems/startup/overview.md)。控件包入口、二进制包链接和语言证明见
[控件包启动接入](package-authoring.md)。

使用 Avalonia.Headless 创建测试应用时，参见 [Headless 测试接入](../testing/overview.md)，其中提供 xUnit 两套版本的
完整项目、Application 挂接、UI 线程调度和隔离示例。

## 1. 最小用法

在 `AppBuilder` 链上调用 `UseAtomUI`，配置回调里只做声明：

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
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UseAtomUIPlatformDetect()
                  .WithAtomUIDefaultOptions()
                  .UseAtomUI(builder =>
                  {
                      builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS, LanguageTags.ZhCN]);
                      builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
                      builder.UseDesktopControls();
                  });
}
```

应用类声明为 `partial`（带 `App.axaml` 的应用类本来就是 partial），AtomUI 生成器会为它补上 `RegisterServices` 挂接：

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

AtomUI 内核在 `Initialize()` 之前提交完毕，`App.axaml` 与主窗体可以直接使用 AtomUI 主题与本地化。

同时使用 FluentTheme 等外部主题，或需要合并应用资源时，先阅读
[与其他框架的主题集成](../theming/framework-theme-integration.md)，确认资源优先级、宿主修复的版本范围与覆盖迁移方式。

## 2. 启用 Splash

不调用 `UseSplash` 就不显示 Splash。默认模式 `Auto`：JIT 运行时显示，NativeAOT 不显示。

```csharp
using AtomUI.Controls;

.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseSplash(SplashMode.Auto, splash => splash
        .WithLogo(new Uri("avares://MyApp/Assets/logo.png"))
        .WithTitle("My App")
        .WithMessage("Loading…"));
})
```

- Logo 必须是应用程序集的 `AvaloniaResource`，或文件 URI；图片在显示 Splash 时才加载，尺寸要小。
- `SplashMode.Always` 在 NativeAOT 下也显示；`SplashMode.Off` 等同于不调用。
- Splash 只在经典桌面生命周期下显示；headless、浏览器与设计器不显示。

## 3. 自定义 Splash 主题

`WithControlTheme<TTheme>()` 指定一个 x:Class 资源字典。局部修改时先合并默认主题，再用 `BasedOn` 派生，只写需要改的
样式。下面把卡片加宽并换成浮层背景色：

```xml
<!-- Assets/MySplashTheme.axaml -->
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:atom="https://atomui.net"
                    x:Class="MyApp.MySplashTheme">
    <ResourceDictionary.MergedDictionaries>
        <atom:SplashDefaultTheme/>
    </ResourceDictionary.MergedDictionaries>

    <ControlTheme x:Key="{x:Type atom:Splash}"
                  TargetType="atom:Splash"
                  BasedOn="{StaticResource {x:Type atom:Splash}}">
        <Style Selector="^ /template/ Border#Card">
            <Setter Property="Width" Value="520"/>
            <Setter Property="Background" Value="{atom:SharedTokenResource ColorBgElevated}"/>
        </Style>
    </ControlTheme>
</ResourceDictionary>
```

```csharp
// Assets/MySplashTheme.axaml.cs
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyApp;

public partial class MySplashTheme : ResourceDictionary
{
    public MySplashTheme() => AvaloniaXamlLoader.Load(this);
}
```

```csharp
builder.UseSplash(SplashMode.Auto, splash => splash
    .WithTitle("My App")
    .WithControlTheme<MySplashTheme>());
```

默认模板的部件：`Border#Card`、`Image#BackgroundImage`、`StackPanel#Content`、`Image#Logo`、`TextBlock#Title`、
`TextBlock#Message`、`SplashProgressBar#Progress`、`Path#PART_CloseButton`；伪类 `:error`、`:opaque`。也可以不合并默认主题，
直接定义完整的 `Splash` ControlTheme 与模板。

Splash 在内核提交之前显示，自定义主题必须遵守：

- 只用 Splash 控件、Avalonia 基础元素（`Border`、`Panel`、`Image`、`TextBlock`、`Path` 等）和
  `{atom:SharedTokenResource …}` 全局 Token；
- 不放 AtomUI 控件（此时还没有控件主题），不引用控件 Token；
- 不依赖自定义字体；
- 模板与图片直接计入 Splash 首帧，越简单越好。

## 4. 异步加载主窗体并汇报进度

主窗体需要先加载数据时，用 `UseMainWindow` 代替在 `OnFrameworkInitializationCompleted` 中创建主窗体。工厂在主循环开始后
于 UI 线程执行，可以 await；返回的窗口由 AtomUI 设为主窗体并显示：

```csharp
.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseSplash();
    builder.UseMainWindow(async context =>
    {
        context.Progress.Report(new StartupProgress("Loading projects…", 0.3));
        var projects = await ProjectStore.LoadAsync(context.CancellationToken);
        context.Progress.Report(new StartupProgress("Preparing workspace…", 0.8));
        return new MainWindow(projects);
    });
})
```

- 汇报的 `Fraction`（0–1）使 Splash 切换为确定进度；`Message` 为 null 时不改消息。
- 用户在交接前关闭 Splash 时 `context.CancellationToken` 取消，应用随即退出。
- 工厂返回后显示主窗体，等待其首个渲染批次完成，再立即隐藏并关闭 Splash，避免 Splash 消失后主窗体长时间未出现。
- 使用 `UseMainWindow` 时不要再自行给 `desktop.MainWindow` 赋值；它只适用于经典桌面生命周期。

## 5. Prism

Prism 的应用类同样声明为 `partial`。Prism 在 `base.Initialize()` 中创建 Shell 并在 `OnInitialized()` 中显示它，此时内核
已经提交，Shell 可以直接使用 AtomUI 控件；Prism 随后把 Shell 设为 `desktop.MainWindow`，Splash 在 Shell 首个渲染批次完成后
立即隐藏并关闭。

```csharp
using Avalonia;
using Avalonia.Markup.Xaml;
using Prism.DryIoc;
using Prism.Ioc;

namespace MyApp;

public partial class App : PrismApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize();
    }

    protected override AvaloniaObject CreateShell() => Container.Resolve<ShellWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
```

## 6. 手写挂接

应用类不能声明为 `partial`，或者需要自己重写 `RegisterServices` 时，在 `base.RegisterServices()` 之后调用
`ApplicationStartup.Attach(this)`：

```csharp
public sealed class App : Application
{
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }
}
```

非 partial 应用还需要在 `UseAtomUI` 配置中调用
`builder.UseStartupImage(AtomUI.Generated.<AssemblyOwner>.GeneratedApplicationStartupImage.Instance)`，
以使用本应用生成的镜像；`ATOMUIREG015` 会提示这一步。partial 应用即使手写挂接，仍会自动获得镜像接口。

手写了 `RegisterServices` 却没有调用 `Attach` 时，编译报 `ATOMUIGEN039`；既没有挂接也不是 partial 时，启动在
`Initialize()` 之后抛出带修复说明的异常。

源码调试时，修改启动挂接 API 后，应一并重建目标配置使用的 Core、Generator 与应用宿主。
应用已正确声明为 partial 却仍提示未调用 Attach 时，检查实际使用的生成器是否与 Core 匹配；
旧生成器可能因查找旧入口类型而跳过挂接生成。重建 Release 不会更新 Debug 目录中的生成器。

## 7. 等待启动结果

`Application.GetStartup()` 返回唯一的启动过程，可用于在启动完成后再做的工作：

```csharp
var startup = Application.Current!.GetStartup();
await startup.Completion; // Ready 时完成；失败带原始异常；取消为已取消
```

## 8. 性能规则

- `UseAtomUI` 只登记回调；普通宿主在平台服务就绪后、Application 构造前执行配置。配置可以间接构造
  `AvaloniaObject`，不需要为保护 Dispatcher 时钟改写普通数据仓库。此时不要依赖 `Application.Current`、应用主题资源
  或已打开的 `TopLevel`；主窗体与耗时加载仍使用下面的正常生命周期入口。设计器与 Browser/WASI 的差异见
  [启动生命周期](../../architecture/systems/startup/lifecycle.md#9-宿主差异)。
- 不要在应用构造函数中创建控件或访问主题：构造函数早于 `RegisterServices`，此时内核尚未提交。
- Splash 内容只用基础元素，Logo 与背景图要小。
- 主窗体构造函数中的工作直接推迟 Splash 交接；耗时加载放进 `UseMainWindow` 的异步工厂并汇报进度。
