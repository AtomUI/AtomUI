# Headless 测试接入

本文面向使用 AtomUI NuGet 包的应用开发者，说明如何创建可运行的 .NET 10 UI 测试项目。测试使用
`Avalonia.Headless`，不启动真实桌面窗口；AtomUI 仍通过正常的 `UseAtomUI` 和 Application 挂接注册主题与服务。

## 1. 选择版本与运行方式

下面是已验证的固定版本组合，不表示其他版本都兼容：

| AtomUI | Avalonia / Headless | xunit.v3 包版本 | 接入方式 | 命令模式 |
| --- | --- | --- | --- | --- |
| 6.2.8 | 12.1.4 | 3.2.2 | `Avalonia.Headless.XUnit` + `[AvaloniaFact]` | VSTest |
| 6.2.8 | 12.1.4 | 4.0.2 | `HeadlessUnitTestSession` + 普通 `[Fact]` | Microsoft.Testing.Platform（MTP） |

`xunit.v3` 是包名，表中的 3.x、4.x 是这个包的版本。`Avalonia.Headless.XUnit 12.1.4` 依赖的 xUnit 扩展 API
与 4.0.2 不兼容。虽然 NuGet 可以还原且编译可能成功，测试发现或执行仍会抛出 `MissingMethodException`。
该版本下选择 4.0.2 时，使用第二条路径，不引用 `Avalonia.Headless.XUnit`；上游进展见
[Avalonia #22072](https://github.com/AvaloniaUI/Avalonia/issues/22072)。更新适配器后应重新验证版本组合。

示例按独立测试项目组织：新建 `HeadlessTests` 目录，先复制第 2 节的两个公共文件，再选择第 3 或第 4 节的项目和
测试文件。不要把两套 assembly attribute 混入同一项目。命令均在该测试项目目录执行；示例不依赖仓库的
`Directory.Build.props`、项目引用或内部测试 helper。

## 2. 两条路径共用的应用与断言

### TestAppBuilder.cs

只选择 Headless 后端，不调用桌面平台探测或 `StartWithClassicDesktopLifetime`。测试应用不需要 `App.axaml`。
显式使用亮色初始主题，使测试不受操作系统外观设置影响。

```csharp
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Headless;

namespace HeadlessTests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .UseAtomUI(builder =>
            {
                builder.UseDesktopControls();
                builder.WithInitialTheme(
                    IThemeManager.DEFAULT_THEME_ID,
                    new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default).Build());
            });
}

public sealed partial class TestApplication : Application
{
}
```

Application 使用顶层、非泛型 `partial` 类，生成器负责在 `RegisterServices()` 中挂接 AtomUI。
测试中不要再手动调用一次 `SetupWithoutStarting()`：后续的适配器或 session 负责初始化。

### ButtonAssertions.cs

这段断言验证模板实际产生了文字节点、控件具有布局尺寸、异步继续执行时仍能访问 UI 线程，以及 AtomUI 的主题
提交确实改变窗口背景。它不固定视觉后代总数，也不把设置 Avalonia `RequestedThemeVariant` 当作 AtomUI 主题切换。

```csharp
using AtomUI;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomWindow = AtomUI.Desktop.Controls.Window;

namespace HeadlessTests;

internal static class ButtonAssertions
{
    public static async Task VerifyAsync()
    {
        Assert.True(Dispatcher.UIThread.CheckAccess());
        var application = Assert.IsType<TestApplication>(Application.Current);
        var manager = Assert.IsAssignableFrom<IThemeManager>(application.GetThemeManager());
        Assert.Equal(ThemeAppearance.Light, manager.CurrentTheme!.Appearance);

        var button = new AtomButton { Content = "Hello" };
        var window = new AtomWindow { Width = 320, Height = 200, Content = button };
        try
        {
            window.Show();
            window.UpdateLayout();
            Assert.NotNull(button.Template);
            Assert.Contains(button.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Hello");
            Assert.True(button.Bounds.Width > 0 && button.Bounds.Height > 0);
            var lightBackground = Assert.IsAssignableFrom<ISolidColorBrush>(window.Background).Color;

            await Task.Yield();
            Assert.True(Dispatcher.UIThread.CheckAccess());
            var result = await manager.ApplyThemeAsync(
                new ThemeRequest(
                    IThemeManager.DEFAULT_THEME_ID,
                    new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build(),
                    ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);

            Assert.Equal(ThemeTransitionStatus.Committed, result.Status);
            Assert.Equal(ThemeAppearance.Dark, manager.CurrentTheme!.Appearance);
            Assert.Equal(ThemeVariant.Dark, application.ActualThemeVariant);
            Assert.NotEqual(lightBackground, Assert.IsAssignableFrom<ISolidColorBrush>(window.Background).Color);
        }
        finally
        {
            window.Close();
        }
    }
}
```

窗口在 `finally` 中关闭。测试创建的订阅、流、计时器等可释放资源也应由测试清理；不要依赖 Application 隔离代替
这些资源自身的释放。主题 API 的完整契约见[主题定制指南](../theming/customization.md#10-在运行时切换主题)。

## 3. xunit.v3 3.2.2：使用 Avalonia 适配器

### HeadlessTests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AtomUI.Desktop.Controls" Version="6.2.8" />
    <PackageReference Include="Avalonia.Headless" Version="12.1.4" />
    <PackageReference Include="Avalonia.Headless.XUnit" Version="12.1.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="xunit.v3" Version="3.2.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

### AssemblyInfo.cs

三个 Avalonia attribute 分别选择 AppBuilder、测试框架和隔离级别。示例显式使用 `PerTest`，每个用例重新创建
Application 与 Dispatcher；UI 测试程序集关闭并行执行。

```csharp
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(HeadlessTests.TestAppBuilder))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

### ButtonTests.cs

两个测试都要求初始状态为亮色，随后切换到暗色，因此连续执行也能检查前一个测试的主题是否污染下一个测试。
两个测试均复用第 2 节的断言。

```csharp
using Avalonia.Headless.XUnit;

namespace HeadlessTests;

public sealed class ButtonTests
{
    [AvaloniaFact]
    public Task Button_template_and_theme_work() => ButtonAssertions.VerifyAsync();

    [AvaloniaFact]
    public Task Next_test_starts_with_a_light_theme() => ButtonAssertions.VerifyAsync();
}
```

运行这两个测试：

```bash
dotnet test HeadlessTests.csproj --filter FullyQualifiedName~HeadlessTests.ButtonTests
```

本节使用默认的 VSTest 命令模式。若现有解决方案的 `global.json` 已选择 MTP，应按该解决方案的运行配置接入，
不要直接混用两种命令的参数。UI 测试使用 `[AvaloniaFact]` / `[AvaloniaTheory]`，普通纯逻辑测试可以使用 `[Fact]`。

## 4. xunit.v3 4.0.2：直接使用 Headless session

`HeadlessUnitTestSession` 属于 `Avalonia.Headless`，不依赖 xUnit 适配器。它负责建立应用、调度测试和维护 UI
执行上下文。普通 `[Fact]` 本身不会初始化 Avalonia，因此控件创建、交互、断言和关闭都必须放在 `Dispatch` 内。

### HeadlessTests.csproj

4.0.2 默认使用 MTP 2；本节仅保留 MTP 所需依赖，不需要 `Microsoft.NET.Test.Sdk` 或 VSTest adapter。

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
    <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AtomUI.Desktop.Controls" Version="6.2.8" />
    <PackageReference Include="Avalonia.Headless" Version="12.1.4" />
    <PackageReference Include="xunit.v3" Version="4.0.2" />
  </ItemGroup>
</Project>
```

### global.json

放在测试项目目录，或合并到管理该项目的现有 `global.json`，保留原有 `sdk` 等配置。此设置选择 .NET 10 的 MTP
命令模式；它会影响该目录及子目录中的 `dotnet test`，应与现有解决方案的其他测试项目保持一致。

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

### AssemblyInfo.cs

4.0.2 的并行控制使用新属性；不要复制 3.2.2 示例中的 `CollectionBehavior.DisableTestParallelization`。

```csharp
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
```

### ButtonTests.cs

`StartNew` 默认使用 `PerTest`。每次 `Dispatch` 都建立独立的 Application 与 Dispatcher；第二个测试在同一 session
中连续调度两次，验证 Application 身份与应用资源没有复用。`Dispatch` 的异步泛型重载需要返回值，因此第一个测试
在断言后返回 `true`。`await using` 在完成或异常后释放 session，结束它的执行队列。

```csharp
using Avalonia.Headless;
using Xunit;

namespace HeadlessTests;

public sealed class ButtonTests
{
    [Fact]
    public async Task Button_template_and_theme_work()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        await session.Dispatch(async () =>
        {
            await ButtonAssertions.VerifyAsync();
            return true;
        }, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Each_dispatch_has_its_own_application()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
        Avalonia.Application? previous = null;
        await session.Dispatch(() =>
        {
            previous = Avalonia.Application.Current!;
            previous.Resources["TestMarker"] = "previous application";
        }, TestContext.Current.CancellationToken);
        await session.Dispatch(() =>
        {
            var current = Avalonia.Application.Current!;
            Assert.NotSame(previous, current);
            Assert.False(current.Resources.ContainsKey("TestMarker"));
        }, TestContext.Current.CancellationToken);
    }
}
```

MTP 模式运行这两个测试：

```bash
dotnet test --project HeadlessTests.csproj -- --filter-class HeadlessTests.ButtonTests
```

这里使用 MTP 的 `--project`、`--filter-class`，不要套用 VSTest 的位置参数、`--filter` 或 `--logger`。
如只需直接运行项目，也可使用 `dotnet run --project HeadlessTests.csproj`。

不需要自行创建 STA 线程。若另有 Windows 专用宿主，`SetApartmentState(STA)` 必须按平台处理；该调用在现代
.NET 的 macOS/Linux 上会抛出 `PlatformNotSupportedException`。仅创建线程并调用 `SetupWithoutStarting()`，
也不能替代这里的异步调度与隔离机制。

## 5. Application 挂接与常见错误

优先保留本指南中的 `partial TestApplication`。已有手写 `RegisterServices()` 时，挂接写法为：

```csharp
public sealed partial class TestApplication : Application
{
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }
}
```

此片段替换第 2 节的 Application 声明，并沿用其中的 `using` 与命名空间。保留 `partial` 时，生成器仍会生成应用
启动镜像接口，但不会重复生成已手写的 override。确实不能使用 `partial` 时，还要处理应用启动镜像选择，见
[启动指南的手写挂接](../startup/overview.md#6-手写挂接)。

| 错误或现象 | 检查与处理 |
| --- | --- |
| `MissingMethodException`，涉及 `TestIntrospectionHelper`、`IXunitTestAssembly` 等 xUnit API | 检查实际解析的 xUnit 与 Avalonia 适配器版本；本指南的 4.0.2 路径不引用适配器，也不使用其 attribute。切换路径后清理旧 `bin` / `obj` 并重新还原。 |
| `Testing with VSTest target is no longer supported` | 4.0.2 默认的 MTP 2 在 .NET 10 下需要第 4 节的 MTP 配置及命令；单独更换 Headless 宿主不能解决运行器配置问题。 |
| `CollectionBehavior.DisableTestParallelization` 报 `CS0619` | 4.0.2 改用第 4 节的 `Parallelization(Mode = ParallelMode.None)`。 |
| `ATOMUILOC008` | 参与本地化生成的 Application 不满足宿主条件，例如非 partial、嵌套、泛型或有多个具体 Application。优先使用单一顶层、非泛型 partial 应用；不要只按项目是 Exe 还是类库判断。 |
| `ATOMUIGEN039` | 具体 Application 手写了 `RegisterServices()` 却没有调用 `ApplicationStartup.Attach(this)`；在 `base.RegisterServices()` 后调用。 |
| `ATOMUIREG015` | 应用无法自动获得启动镜像接口；优先改为合适的 partial 宿主，手写宿主按启动指南显式配置 `UseStartupImage`。 |
| UI 线程访问错误或异步测试挂起 | 3.2.2 使用 Avalonia 测试 attribute；4.0.2 在 `Dispatch` 内执行 UI 操作并 await 返回的任务。不要用 `.Wait()` / `.Result` 阻塞 UI 线程。 |
| 模板为 null 或没有实际视觉节点 | 确认调用 `UseDesktopControls()`、Application 已挂接，且控件已加入显示的窗口并完成布局；只调用控件构造函数不等于模板已物化。 |

完整的启动挂接、主题和本地化条件以[启动指南](../startup/overview.md)和
[本地化公共契约](../../reference/localization/public-api.md)为准。

## 6. 验证边界与参考

上述示例在 macOS arm64、.NET SDK 10.0.300 下按 NuGet 包消费验证。示例验证 UI 线程、模板、布局、主题提交和
隔离，不验证真实像素、原生窗口装饰、系统剪贴板或平台输入法。需要这些行为时，使用对应平台的真实桌面或渲染
验证，并在目标平台运行测试；不要把 Headless 通过当作跨平台视觉验收或 NativeAOT 发布证据。

- [Avalonia Headless / xUnit 文档](https://docs.avaloniaui.net/docs/testing/headless-xunit)
- [xUnit 的 MTP 接入说明](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [.NET 的 dotnet test 运行模式](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [xUnit 4.0 的破坏性变更](https://xunit.net/releases/v3/4.0.0)
- [Thread.SetApartmentState 平台支持](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.setapartmentstate?view=net-10.0)
