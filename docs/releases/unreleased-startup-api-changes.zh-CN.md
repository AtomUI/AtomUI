# 启动 API 迁移（待发布）

本文记录工作区中已批准、尚未发布的启动相关变更，相对于最近的正式版本 6.2.5。本文不指定发布版本号；正式发布时须在
Changelog 中标明这些源代码与二进制兼容性破坏。

英文版本见 [Pending startup API migration](unreleased-startup-api-changes.md)。

设计见[启动系统架构](../architecture/systems/startup/overview.md)，接入示例见[启动指南](../guides/startup/overview.md)。

## 破坏性变更

### 入口从 `Application.UseAtomUI` 移到 `AppBuilder.UseAtomUI`

`Application.UseAtomUI(Action<IAtomUIBuilder>?)` 已删除。配置改在 `AppBuilder` 链上声明：

```csharp
// 6.2.5：App.axaml.cs
public override void Initialize()
{
    AvaloniaXamlLoader.Load(this);
    this.UseAtomUI(builder =>
    {
        builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
        builder.UseDesktopControls();
    });
}
```

```csharp
// 现在：Program.cs
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
              .UseAtomUIPlatformDetect()
              .WithAtomUIDefaultOptions()
              .UseAtomUI(builder =>
              {
                  builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
                  builder.UseDesktopControls();
              });

// 现在：App.axaml.cs
public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
```

AtomUI 内核在 `Initialize()` 之前提交，`App.axaml` 可以直接引用 AtomUI 资源。`GetThemeManager()`、`GetLocalizer()`、
`GetLanguageManager()` 保持不变。

### 应用类必须挂接

应用类声明为 `partial` 即可，AtomUI 生成器生成 `RegisterServices` 重写并调用 `AtomUIStartup.Attach(this)`。不能声明为
partial 时手写：

```csharp
public override void RegisterServices()
{
    base.RegisterServices();
    AtomUIStartup.Attach(this);
}
```

手写了 `RegisterServices` 却没有调用 `Attach` 时报新的编译错误 `ATOMUIGEN039`；既没有挂接也不是 partial 时，启动在
`Initialize()` 之后抛出带修复说明的 `InvalidOperationException`。不要在应用构造函数中创建控件或访问主题。

### 配置回调在平台初始化之前执行

配置回调现在于主线程、Avalonia 平台初始化之前执行，回调中不能访问 `FontManager`、`Application.Current`、`TopLevel`，
也不能创建位图或控件。`UseAlibabaSansFont()`、`UseAlibabaPuHuiTiFont()` 的字体集合改为在内核提交时注册。需要在
内核就绪后执行的代码放进 `builder.AddInitializer(themeManager => …)`。

### Splash 迁入 `AtomUI.Controls`，Extras 中的 Splash 删除

`AtomUI.Desktop.Controls.Extras` 中的 `Splash`、`SplashWindow`、`SplashStatus`、`SplashOptions`、`SplashService`、
`ISplashService`、`SplashPseudoClass`、静态 API、`SplashToken` 与 Splash 的 Semantic Part 样式全部删除。启动 Splash 改由
框架管理：

```csharp
using AtomUI.Controls;

builder.UseSplash(SplashMode.Auto, splash => splash
    .WithLogo(new Uri("avares://MyApp/Assets/logo.png"))
    .WithTitle("My App"));
```

- Splash 在内核提交之前显示，主窗体首个渲染批次完成后自动淡出，不再需要手动关闭，也不再有最短显示时长。
- 启动期间的应用自有加载（原先在 Splash 显示期间手动更新进度的流程）改用 `builder.UseMainWindow(async context => …)`，
  经 `context.Progress` 汇报进度。
- 外观定制改用 `WithControlTheme<TTheme>()`：只能使用 Splash 控件、Avalonia 基础元素与全局 Token。
- 新的 `AtomUI.Controls.Splash` 是 `TemplatedControl`，不再是 `ContentControl`；页面内预览需要合并 `SplashDefaultTheme`。

`AtomUI.Desktop.Controls.Extras` 暂无控件，`UseDesktopExtras()` 保留并只登记一个空包。

### macOS 默认渲染改为 Metal 优先

`WithAtomUIDefaultOptions()` 在 macOS 上的渲染顺序由 OpenGL、Metal、Software 改为 Metal、OpenGL、Software。原先选 OpenGL
是为了规避 Metal 在 live resize 时的内容抖动，该问题已由 Avalonia 12.1.0 修复。需要保留 OpenGL 时，在
`WithAtomUIDefaultOptions()` 之后覆盖：

```csharp
.WithAtomUIDefaultOptions()
.With(new AvaloniaNativePlatformOptions
{
    RenderingMode = [AvaloniaNativeRenderingMode.OpenGl, AvaloniaNativeRenderingMode.Metal, AvaloniaNativeRenderingMode.Software]
})
```

### `IThemeManagerBuilder` 新增成员

`IThemeManagerBuilder` 新增 `AddCompiledThemePackage(CompiledThemePackage)`。自行实现该接口的代码需要补齐该成员。

### 主题来源标识变化（诊断中可见）

- 内置主题的注册标识由 `AtomUI.Core.BuiltInThemes` 改为 `AtomUI.Core`。
- 内置主题的来源标识由 `avares://AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml` 形式的 URI 改为
  `AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml` 形式的逻辑路径。

按这些字符串匹配主题诊断的代码需要同步修改。

## 新增 API

- `AppBuilderExtensions.UseAtomUI(this AppBuilder, Action<IAtomUIBuilder>)`、`AtomUIStartup.Attach(Application)`。
- `Application.GetStartup()` 与 `IAtomUIStartup`：`State`（`Preparing`、`Committed`、`Presenting`、`Ready`、`Failed`、
  `Canceled`）、`Completion`、`ProgressChanged`。
- `UseMainWindow(Func<AtomUIMainWindowContext, Task<Window>>)`、`AtomUIMainWindowContext`、`AtomUIStartupProgress`。
- `SplashMode`；`AtomUI.Controls` 中的 `UseSplash`、`SplashOptionsBuilder`、`Splash`、`SplashWindow`、`SplashProgressBar`、
  `SplashStatus`、`SplashDefaultTheme`。
- `AtomUI-Startup` EventSource（默认关闭）。
- 编译主题包基础设施：`AtomUI.Theme.Definitions.Compiled` 命名空间中的 `CompiledThemePackage`、`CompiledThemeProgram`、
  `CompiledThemeBindingBuilder`、`CompiledThemeValueWriter`、`ThemeBuildOwnerAttribute`、`ThemeBuildTokenAttribute`，以及
  `AtomUI.Core` 生成的 `AtomUI.Generated.AtomUICore.GeneratedThemePackageRegistration`。这些类型供生成代码使用；不要
  自行调用 `GeneratedThemePackageRegistration.Register`，Core 已自动注册自身的主题包，重复调用会重复注册。

## 启动包清单版本 2

控件包与应用须使用匹配的 Generator/Core 重新构建。启动包清单和应用镜像现在使用版本 2，包含 owner/key 的精确
定义程序集、语义属性/目标和全局资源键 schema 证据。版本 1 会报告重建错误；条件注册、编译主题和语言 ABI 各自独立。

`Application.GetAtomUI()` 返回应用的 `IAtomUI` 运行时。初始提交后可在 UI 线程调用
`LoadPackages(Action<IAtomUIBuilder>)` 原子追加包；该入口不能修改初始应用设置。
回滚与生命周期规则见[控件包启动接入](../guides/startup/package-authoring.md)。
