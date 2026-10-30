using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowTitleBarEffectiveLogoTests
{
    static WindowTitleBarEffectiveLogoTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Effective_Logo_Falls_Back_To_Window_Icon_When_Logo_Not_Set()
    {
        var icon = CreateIcon();
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 200,
            Height = 100,
            Icon  = icon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetEffectiveLogo(window).ShouldBe(icon);
            GetEffectiveLogoTemplate(window).ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Runtime_Logo_Control_Replaces_Fallback_Without_Template_Residue()
    {
        var icon = CreateIcon();
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 200,
            Height = 100,
            Icon  = icon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var runtimeLogo = new Border();
            window.Logo = runtimeLogo;
            Dispatcher.UIThread.RunJobs();

            GetEffectiveLogo(window).ShouldBe(runtimeLogo);
            GetEffectiveLogoTemplate(window).ShouldBeNull();

            window.Logo = null;
            Dispatcher.UIThread.RunJobs();
            GetEffectiveLogo(window).ShouldBe(icon);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Explicit_LogoTemplate_Suppresses_Icon_Fallback()
    {
        var icon = CreateIcon();
        var template = new FuncDataTemplate<object>((_, _) => new Border());
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width  = 200,
            Height = 100,
            Icon   = icon,
            LogoTemplate = template
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetEffectiveLogo(window).ShouldBeNull();
            GetEffectiveLogoTemplate(window).ShouldBe(template);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Never_Visibility_Hides_Icon_Fallback_Logo()
    {
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 200,
            Height = 100,
            Icon = CreateIcon(),
            LogoVisibility = WindowTitleBarLogoVisibility.Never
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();

            var titleBar = GetTitleBar(window);
            titleBar.ShouldNotBeNull();
            titleBar.ApplyTemplate();
            GetIsEffectiveLogoVisible(titleBar).ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(OsType.Linux)]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.macOS)]
    public void Standalone_Title_Bar_Presenter_Renders_Explicit_Logo(OsType osType)
    {
        const string logo = "standalone-logo";
        var titleBar = new WindowTitleBar
        {
            Width          = 320,
            Height         = 40,
            Logo           = logo,
            LogoVisibility = WindowTitleBarLogoVisibility.Always,
            Title          = "Standalone title bar"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, osType);
        Application.Current!.TryFindResource(typeof(WindowTitleBar), out var resource).ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new AvaloniaWindow
        {
            Width   = 320,
            Height  = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();

            var logoPresenter = titleBar.GetVisualDescendants()
                                        .OfType<ContentPresenter>()
                                        .Single(presenter => presenter.Name == "PART_Logo");
            logoPresenter.Content.ShouldBe(logo);
        }
        finally
        {
            host.Close();
        }
    }

    [Theory]
    [InlineData(OsType.Linux)]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.macOS)]
    public void Custom_Title_Bar_Logo_Takes_Precedence_Over_Host_Window_Fallback(OsType osType)
    {
        const string hostLogo = "host-logo";
        const string titleBarLogo = "title-bar-logo";
        var titleBar = new WindowTitleBar
        {
            Width          = 320,
            Height         = 40,
            Logo           = titleBarLogo,
            LogoVisibility = WindowTitleBarLogoVisibility.Always,
            Title          = "Custom title bar"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, osType);
        Application.Current!.TryFindResource(typeof(WindowTitleBar), out var resource).ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var window = new AtomUI.Desktop.Controls.Window
        {
            Width   = 400,
            Height  = 160,
            Logo    = hostLogo,
            Title   = "Host window",
            Content = titleBar
        };

        try
        {
            window.Show();
            titleBar.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var logoPresenter = titleBar.GetVisualDescendants()
                                        .OfType<ContentPresenter>()
                                        .Single(presenter => presenter.Name == "PART_Logo");
            logoPresenter.Content.ShouldBe(titleBarLogo);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Hosted_Title_Bar_Without_Explicit_Logo_Tracks_Host_Effective_Logo()
    {
        var titleBar = new WindowTitleBar
        {
            Width          = 320,
            Height         = 40,
            LogoVisibility = WindowTitleBarLogoVisibility.Always,
            Title          = "Hosted title bar"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Windows);
        Application.Current!.TryFindResource(typeof(WindowTitleBar), out var resource).ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var window = new AtomUI.Desktop.Controls.Window
        {
            Width   = 400,
            Height  = 160,
            Logo    = "host-logo-1",
            Title   = "Host window",
            Content = titleBar
        };

        try
        {
            window.Show();
            titleBar.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var logoPresenter = titleBar.GetVisualDescendants()
                                        .OfType<ContentPresenter>()
                                        .Single(presenter => presenter.Name == "PART_Logo");
            logoPresenter.Content.ShouldBe("host-logo-1");

            window.Logo = "host-logo-2";
            Dispatcher.UIThread.RunJobs();
            logoPresenter.Content.ShouldBe("host-logo-2");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Fullscreen_Logo_Visibility_Is_Inactive_Until_Window_Enters_Fullscreen()
    {
        var window = new AtomUI.Desktop.Controls.Window
        {
            Title          = "AtomUI",
            LogoVisibility = WindowTitleBarLogoVisibility.Always
        };

        GetIsEffectiveFullscreenLogoVisible(window).ShouldBeFalse();
        GetEffectiveFullscreenLogo(window).ShouldBeNull();
        GetEffectiveFullscreenLogoTemplate(window).ShouldBeNull();

        window.Icon = CreateIcon();
        GetIsEffectiveFullscreenLogoVisible(window).ShouldBeFalse();
        GetEffectiveFullscreenLogo(window).ShouldBeNull();
        GetEffectiveFullscreenLogoTemplate(window).ShouldBeNull();

        window.WindowState = WindowState.FullScreen;
        GetIsEffectiveFullscreenLogoVisible(window).ShouldBeTrue();
        GetEffectiveFullscreenLogo(window).ShouldBe(window.Icon);
        GetEffectiveFullscreenLogoTemplate(window).ShouldNotBeNull();

        window.WindowState = WindowState.Normal;
        GetIsEffectiveFullscreenLogoVisible(window).ShouldBeFalse();
        GetEffectiveFullscreenLogo(window).ShouldBeNull();
        GetEffectiveFullscreenLogoTemplate(window).ShouldBeNull();

        window.Icon = null;
        GetIsEffectiveFullscreenLogoVisible(window).ShouldBeFalse();
        GetEffectiveFullscreenLogo(window).ShouldBeNull();
        GetEffectiveFullscreenLogoTemplate(window).ShouldBeNull();
    }

    [Fact]
    public void Hosted_Title_Bar_Releases_Logo_Visibility_When_Host_Enters_Fullscreen()
    {
        var logo = new Border();
        var titleBar = new WindowTitleBar
        {
            Title          = "AtomUI",
            Logo           = logo,
            LogoVisibility = WindowTitleBarLogoVisibility.Always
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Windows);

        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();
        GetEffectiveLogoPresenterContent(titleBar).ShouldBe(logo);
        GetEffectiveLogoPresenterContentTemplate(titleBar).ShouldBeNull();

        titleBar.HostWindowState = WindowState.FullScreen;
        GetIsEffectiveLogoVisible(titleBar).ShouldBeFalse();
        GetEffectiveLogoPresenterContent(titleBar).ShouldBeNull();
        GetEffectiveLogoPresenterContentTemplate(titleBar).ShouldBeNull();

        titleBar.HostWindowState = WindowState.Normal;
        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();
        GetEffectiveLogoPresenterContent(titleBar).ShouldBe(logo);
        GetEffectiveLogoPresenterContentTemplate(titleBar).ShouldBeNull();
    }

    [Fact]
    public void Main_Window_Runtime_Logo_Changes_Update_Child_Fallback_Until_Child_Closes()
    {
        var application = Application.Current!;
        var lifetimeField = typeof(Application).GetField(
            "_applicationLifetime",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var previousLifetime = lifetimeField.GetValue(application);
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        var mainWindow = new AtomUI.Desktop.Controls.Window { Logo = "main-logo-1" };
        var childWindow = new AtomUI.Desktop.Controls.Window
        {
            Width  = 200,
            Height = 100
        };

        try
        {
            lifetime.MainWindow = mainWindow;
            lifetimeField.SetValue(application, lifetime);

            childWindow.Show();
            Dispatcher.UIThread.RunJobs();
            GetEffectiveLogo(childWindow).ShouldBe("main-logo-1");

            mainWindow.Logo = "main-logo-2";
            Dispatcher.UIThread.RunJobs();
            GetEffectiveLogo(childWindow).ShouldBe("main-logo-2");

            childWindow.Close();
            mainWindow.Logo = "main-logo-after-close";
            Dispatcher.UIThread.RunJobs();
            GetEffectiveLogo(childWindow).ShouldBe("main-logo-2");
        }
        finally
        {
            childWindow.Close();
            lifetime.MainWindow = null;
            lifetimeField.SetValue(application, previousLifetime);
        }
    }

    [Fact]
    public void Main_Window_Fallback_Does_Not_Subscribe_Before_Child_Is_Shown()
    {
        var application = Application.Current!;
        var lifetimeField = typeof(Application).GetField(
            "_applicationLifetime",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var previousLifetime = lifetimeField.GetValue(application);
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        var mainWindow = new AtomUI.Desktop.Controls.Window { Logo = "main-logo" };
        var childWindow = new AtomUI.Desktop.Controls.Window();

        try
        {
            lifetime.MainWindow = mainWindow;
            lifetimeField.SetValue(application, lifetime);

            childWindow.Icon = CreateIcon();
            childWindow.Icon = null;

            GetMainWindowLogoFallbackLease(childWindow).ShouldBeNull();
            GetEffectiveLogo(childWindow).ShouldBe("main-logo");
        }
        finally
        {
            lifetime.MainWindow = null;
            lifetimeField.SetValue(application, previousLifetime);
        }
    }

    [Fact]
    public void Logo_Fallback_Resolution_Matrix()
    {
        var resolver = GetFallbackResolver();
        resolver.ShouldNotBeNull();

        var hostIcon = CreateIcon();
        var mainWindowIcon = CreateIcon();
        var mainWindowLogo = new Border();

        var host = new AtomUI.Desktop.Controls.Window { Icon = hostIcon };
        var mainWindow = new AtomUI.Desktop.Controls.Window { Icon = mainWindowIcon, Logo = mainWindowLogo };

        // 1. 显式 Logo 优先，模板跟随显式设置
        var explicitTemplate = new FuncDataTemplate<object>((_, _) => new Border());
        host.LogoTemplate = explicitTemplate;
        host.Logo = new Border();
        Resolve(resolver, host, mainWindow).logo.ShouldNotBeNull();

        // 2. 显式 LogoTemplate 存在时不回退
        host.Logo = null;
        var (logo2, template2) = Resolve(resolver, host, mainWindow);
        logo2.ShouldBeNull();
        template2.ShouldBe(explicitTemplate);

        // 3. 自身 Icon 优先于主窗口
        host.LogoTemplate = null;
        var (logo3, template3) = Resolve(resolver, host, mainWindow);
        logo3.ShouldBe(hostIcon);
        template3.ShouldNotBeNull();

        // 4. 自身无 Icon → 继承主窗口显式 Logo
        host.Icon = null;
        var (logo4, _) = Resolve(resolver, host, mainWindow);
        logo4.ShouldBe(mainWindowLogo);

        // 5. 主窗口也只余 Icon → 继承主窗口 Icon
        mainWindow.Logo = null;
        var (logo5, template5) = Resolve(resolver, host, mainWindow);
        logo5.ShouldBe(mainWindowIcon);
        template5.ShouldNotBeNull();

        // 6. 全空 → 无 logo
        mainWindow.Icon = null;
        var (logo6, template6) = Resolve(resolver, host, null);
        logo6.ShouldBeNull();
        template6.ShouldBeNull();
    }

    private static WindowIcon CreateIcon()
    {
        var bitmap = new RenderTargetBitmap(new PixelSize(4, 4), new Vector(96, 96));
        return new WindowIcon(bitmap);
    }

    private static object? GetEffectiveLogo(AtomUI.Desktop.Controls.Window window)
    {
        return typeof(AtomUI.Desktop.Controls.Window)
               .GetProperty("EffectiveLogo", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(window);
    }

    private static object? GetEffectiveLogoTemplate(AtomUI.Desktop.Controls.Window window)
    {
        return typeof(AtomUI.Desktop.Controls.Window)
               .GetProperty("EffectiveLogoTemplate", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(window);
    }

    private static object? GetEffectiveFullscreenLogo(AtomUI.Desktop.Controls.Window window)
    {
        return typeof(AtomUI.Desktop.Controls.Window)
               .GetProperty("EffectiveFullscreenLogo", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(window);
    }

    private static object? GetEffectiveFullscreenLogoTemplate(AtomUI.Desktop.Controls.Window window)
    {
        return typeof(AtomUI.Desktop.Controls.Window)
               .GetProperty("EffectiveFullscreenLogoTemplate", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(window);
    }

    private static WindowTitleBar? GetTitleBar(AtomUI.Desktop.Controls.Window window)
    {
        return typeof(AtomUI.Desktop.Controls.Window)
               .GetProperty("TitleBar", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(window) as WindowTitleBar;
    }

    private static bool GetIsEffectiveLogoVisible(WindowTitleBar titleBar)
    {
        return (bool)typeof(WindowTitleBar)
               .GetProperty("IsEffectiveLogoVisible", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(titleBar)!;
    }

    private static object? GetEffectiveLogoPresenterContent(WindowTitleBar titleBar)
    {
        return typeof(WindowTitleBar)
               .GetProperty("EffectiveLogoPresenterContent", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(titleBar);
    }

    private static object? GetEffectiveLogoPresenterContentTemplate(WindowTitleBar titleBar)
    {
        return typeof(WindowTitleBar)
               .GetProperty("EffectiveLogoPresenterContentTemplate", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(titleBar);
    }

    private static bool GetIsEffectiveFullscreenLogoVisible(AtomUI.Desktop.Controls.Window window)
    {
        return (bool)typeof(AtomUI.Desktop.Controls.Window)
               .GetProperty("IsEffectiveFullscreenLogoVisible", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(window)!;
    }

    private static object? GetMainWindowLogoFallbackLease(AtomUI.Desktop.Controls.Window window)
    {
        return typeof(AtomUI.Desktop.Controls.Window)
               .GetField("_mainWindowLogoFallbackLease", BindingFlags.Instance | BindingFlags.NonPublic)!
               .GetValue(window);
    }

    private static MethodInfo? GetFallbackResolver()
    {
        return typeof(AtomUI.Desktop.Controls.Window).GetMethod(
            "ResolveEffectiveLogo",
            BindingFlags.Static | BindingFlags.NonPublic);
    }

    private static (object? logo, object? template) Resolve(
        MethodInfo resolver,
        AtomUI.Desktop.Controls.Window host,
        AtomUI.Desktop.Controls.Window? mainWindow)
    {
        var result = resolver!.Invoke(null, [host, mainWindow]);
        var tupleType = result!.GetType();
        return (tupleType.GetField("Logo")?.GetValue(result) ?? tupleType.GetField("Item1")!.GetValue(result),
                tupleType.GetField("Template")?.GetValue(result) ?? tupleType.GetField("Item2")!.GetValue(result));
    }
}
