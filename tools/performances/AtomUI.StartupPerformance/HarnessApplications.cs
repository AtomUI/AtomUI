using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;

namespace AtomUI.StartupPerformance;

internal enum HarnessScenario
{
    Plain,
    AtomPlain,
    AtomWindow,
    AtomSplash
}

internal sealed class HarnessOptions
{
    internal static HarnessOptions Current { get; set; } = new();

    internal HarnessScenario Scenario { get; private set; } = HarnessScenario.AtomPlain;
    internal string? MultiCoreJitDirectory { get; private set; }
    internal bool PlatformDefaultsOff { get; private set; }

    internal static bool TryParse(string[] args, out HarnessOptions options, out string? error)
    {
        options = new HarnessOptions();
        error = null;
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (argument == "--scenario" && index + 1 < args.Length)
            {
                var value = args[++index];
                if (value == "plain")
                {
                    options.Scenario = HarnessScenario.Plain;
                }
                else if (value == "atom-plain")
                {
                    options.Scenario = HarnessScenario.AtomPlain;
                }
                else if (value == "atom-window")
                {
                    options.Scenario = HarnessScenario.AtomWindow;
                }
                else if (value == "atom-splash")
                {
                    options.Scenario = HarnessScenario.AtomSplash;
                }
                else
                {
                    error = $"unknown scenario '{value}'";
                    return false;
                }
            }
            else if (argument == "--multicorejit" && index + 1 < args.Length)
            {
                options.MultiCoreJitDirectory = Path.GetFullPath(args[++index]);
            }
            else if (argument == "--platform-defaults-off")
            {
                // 仅用于 plain 场景的"发行版默认配置参考行"：不套 AtomUI 的平台/渲染默认。
                options.PlatformDefaultsOff = true;
            }
            else
            {
                error = $"unknown argument '{argument}'";
                return false;
            }
        }
        return true;
    }
}

internal sealed class PlainHarnessApplication : Application
{
    public override void Initialize()
    {
        StartupTimeline.Mark("Mark.InitializeEntered");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        StartupTimeline.Mark("Mark.FrameworkCompleted");
        HarnessWindows.ShowPlainWindow(this);
        base.OnFrameworkInitializationCompleted();
    }
}

// 本工程引用生成器：应用类声明为 partial，由生成器补上 RegisterServices 挂接。
internal sealed partial class AtomUIHarnessApplication : Application
{
    /// <summary>宿主在 AppBuilder 链上登记 AtomUI 配置，并测量配置回调的同步耗时。</summary>
    internal static void Configure(IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var start = StartupTimeline.Now();
        builder.WithApplicationId("AtomUI.StartupPerformance");
        builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
        builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS]);
        builder.UseAlibabaSansFont();
        builder.UseDesktopControls();
        if (HarnessOptions.Current.Scenario == HarnessScenario.AtomSplash)
        {
            // Always：测量需要在 JIT 与 NativeAOT 下都显示 Splash。
            AtomUI.Controls.SplashBuilderExtensions.UseSplash(builder, SplashMode.Always);
        }
        StartupTimeline.Duration("UseAtomUI.SyncTotal", start, StartupTimeline.Now());
    }

    public override void Initialize()
    {
        StartupTimeline.Mark("Mark.InitializeEntered");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        StartupTimeline.Mark("Mark.FrameworkCompleted");
        if (HarnessOptions.Current.Scenario is HarnessScenario.AtomWindow or HarnessScenario.AtomSplash)
        {
            HarnessWindows.ShowAtomUIWindow(this);
        }
        else
        {
            HarnessWindows.ShowPlainWindow(this);
        }
        base.OnFrameworkInitializationCompleted();
    }
}

internal static class HarnessWindows
{
    // plain 与 atom-plain 使用同一个代码模板，避免依赖任何主题包提供 Avalonia Window 的模板。
    internal static void ShowPlainWindow(Application application)
    {
        var window = new Avalonia.Controls.Window
        {
            Width   = 800,
            Height  = 600,
            Content = new RenderSentinel(),
            Template = new FuncControlTemplate<Avalonia.Controls.Window>((owner, scope) => new ContentPresenter
            {
                Name                               = "PART_ContentPresenter",
                [!ContentPresenter.ContentProperty] = owner[!ContentControl.ContentProperty]
            }.RegisterInNameScope(scope))
        };
        Attach(application, window);
    }

    internal static void ShowAtomUIWindow(Application application)
    {
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width   = 800,
            Height  = 600,
            Content = new RenderSentinel()
        };
        Attach(application, window);
    }

    private static void Attach(Application application, Avalonia.Controls.Window window)
    {
        window.Opened += static (_, _) => StartupTimeline.Mark("Mark.WindowOpened");
        StartupTimeline.Mark("Mark.WindowConstructed");
        if (application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = window;
        }
    }
}
