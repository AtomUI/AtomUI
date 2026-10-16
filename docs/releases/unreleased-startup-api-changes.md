# Pending startup API migration

This document records the approved, unreleased startup changes in the working tree, relative to the latest release,
6.2.5. It does not assign a release version. The eventual release must call out these source and binary compatibility
breaks in the Changelog.

Chinese version: [启动 API 迁移](unreleased-startup-api-changes.zh-CN.md).

See the [startup architecture](../architecture/systems/startup/overview.md) for the design and the
[startup guide](../guides/startup/overview.md) for examples.

## Breaking changes

### The entry point moves from `Application.UseAtomUI` to `AppBuilder.UseAtomUI`

`Application.UseAtomUI(Action<IAtomUIBuilder>?)` is removed. Declare the configuration on the `AppBuilder` chain:

```csharp
// 6.2.5: App.axaml.cs
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
// Now: Program.cs
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
              .UseAtomUIPlatformDetect()
              .WithAtomUIDefaultOptions()
              .UseAtomUI(builder =>
              {
                  builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
                  builder.UseDesktopControls();
              });

// Now: App.axaml.cs
public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
```

AtomUI is committed before `Initialize()`, so `App.axaml` can reference AtomUI resources directly. `GetThemeManager()`,
`GetLocalizer()` and `GetLanguageManager()` are unchanged.

### The application class must be hooked

Declare the application class `partial`; the AtomUI generator emits a `RegisterServices` override that calls
`AtomUIStartup.Attach(this)`. When the class cannot be partial, write the override yourself:

```csharp
public override void RegisterServices()
{
    base.RegisterServices();
    AtomUIStartup.Attach(this);
}
```

A handwritten `RegisterServices` override that does not call `Attach` reports the new compile error `ATOMUIGEN039`. A
class that is neither hooked nor partial fails after `Initialize()` with an `InvalidOperationException` that explains the
fix. Do not create controls or access themes in the application constructor.

### The configuration callback runs before platform initialization

The configuration callback now runs on the main thread before Avalonia's platform initialization. It must not access
`FontManager`, `Application.Current` or `TopLevel`, and must not create bitmaps or controls. `UseAlibabaSansFont()` and
`UseAlibabaPuHuiTiFont()` now register their font collections when AtomUI is committed. Move code that needs a
committed AtomUI into `builder.AddInitializer(themeManager => …)`.

### Splash moves to `AtomUI.Controls`; the Extras Splash is removed

`Splash`, `SplashWindow`, `SplashStatus`, `SplashOptions`, `SplashService`, `ISplashService`, `SplashPseudoClass`, the
static API, `SplashToken` and the Splash Semantic Part styles are removed from `AtomUI.Desktop.Controls.Extras`. The
startup Splash is now managed by the framework:

```csharp
using AtomUI.Controls;

builder.UseSplash(SplashMode.Auto, splash => splash
    .WithLogo(new Uri("avares://MyApp/Assets/logo.png"))
    .WithTitle("My App"));
```

- The Splash appears before AtomUI is committed and fades out automatically after the main window's first rendered
  batch. Manual closing and the minimum display duration are gone.
- Application-owned loading during startup (previously done while updating a manually shown Splash) moves to
  `builder.UseMainWindow(async context => …)`, which reports progress through `context.Progress`.
- Customize the look with `WithControlTheme<TTheme>()`, using only Splash controls, basic Avalonia elements and global
  tokens.
- The new `AtomUI.Controls.Splash` is a `TemplatedControl`, not a `ContentControl`; in-page previews must merge
  `SplashDefaultTheme`.

`AtomUI.Desktop.Controls.Extras` currently has no controls. `UseDesktopExtras()` remains and registers an empty package.

### macOS now prefers Metal

On macOS, `WithAtomUIDefaultOptions()` changes the rendering order from OpenGL, Metal, Software to Metal, OpenGL,
Software. OpenGL was preferred to avoid Metal content jitter during live resize, which Avalonia 12.1.0 fixed. To keep
OpenGL, override after `WithAtomUIDefaultOptions()`:

```csharp
.WithAtomUIDefaultOptions()
.With(new AvaloniaNativePlatformOptions
{
    RenderingMode = [AvaloniaNativeRenderingMode.OpenGl, AvaloniaNativeRenderingMode.Metal, AvaloniaNativeRenderingMode.Software]
})
```

### `IThemeManagerBuilder` gains a member

`IThemeManagerBuilder` adds `AddCompiledThemePackage(CompiledThemePackage)`. Code that implements the interface must add
it.

### Theme source identities change (visible in diagnostics)

- The built-in theme registration identifier changes from `AtomUI.Core.BuiltInThemes` to `AtomUI.Core`.
- Built-in theme source identities change from URIs such as `avares://AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml`
  to logical paths such as `AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml`.

Update code that matches theme diagnostics by these strings.

## New APIs

- `AppBuilderExtensions.UseAtomUI(this AppBuilder, Action<IAtomUIBuilder>)` and `AtomUIStartup.Attach(Application)`.
- `Application.GetStartup()` and `IAtomUIStartup`: `State` (`Preparing`, `Committed`, `Presenting`, `Ready`, `Failed`,
  `Canceled`), `Completion` and `ProgressChanged`.
- `UseMainWindow(Func<AtomUIMainWindowContext, Task<Window>>)`, `AtomUIMainWindowContext` and `AtomUIStartupProgress`.
- `SplashMode`; in `AtomUI.Controls`: `UseSplash`, `SplashOptionsBuilder`, `Splash`, `SplashWindow`, `SplashProgressBar`,
  `SplashStatus` and `SplashDefaultTheme`.
- The `AtomUI-Startup` EventSource (disabled by default).
- Compiled theme package infrastructure: `CompiledThemePackage`, `CompiledThemeProgram`, `CompiledThemeBindingBuilder`,
  `CompiledThemeValueWriter`, `ThemeBuildOwnerAttribute` and `ThemeBuildTokenAttribute` in
  `AtomUI.Theme.Definitions.Compiled`, plus the generated `AtomUI.Generated.AtomUICore.GeneratedThemePackageRegistration`
  in `AtomUI.Core`. These types serve generated code. Do not call `GeneratedThemePackageRegistration.Register` yourself:
  Core registers its own package automatically, and a second call registers it twice.

## Startup package manifest version 2

Rebuild control packages and applications with the matching Generator/Core. Startup package manifests and application startup
images now use version 2, carrying exact owner/key defining assemblies, semantic property/target contracts and global resource-key
schema evidence. Version 1 is rejected with a rebuild message. Conditional registration, compiled-theme and language ABIs keep their
independent versions.

`Application.GetAtomUI()` returns the application's `IAtomUI` runtime. After initial commit, call
`LoadPackages(Action<IAtomUIBuilder>)` on the UI thread to append packages atomically. Initial application settings cannot be changed
through this entry. See [package authoring](../guides/startup/package-authoring.md) for rollback and lifecycle rules.
