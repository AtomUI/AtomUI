using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowTitleBarLogoVisibilityTests
{
    static WindowTitleBarLogoVisibilityTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Title_Bar_Logo_Visibility_Mode_Defaults_To_Auto()
    {
        var enumSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarLogoVisibility.cs"));
        var titleBarSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBar.cs"));

        enumSource.ShouldContain("Auto");
        enumSource.ShouldContain("Always");
        enumSource.ShouldContain("Never");
        titleBarSource.ShouldContain("WindowTitleBarLogoVisibility.Auto");
    }

    [Fact]
    public void Title_Bar_Title_Alignment_Exposes_The_Approved_Public_Values()
    {
        var enumSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarTitleAlignment.cs"));

        enumSource.ShouldContain("public enum WindowTitleBarTitleAlignment");
        enumSource.ShouldContain("Auto");
        enumSource.ShouldContain("Left");
        enumSource.ShouldContain("Center");
        enumSource.ShouldContain("WindowCenter");
        enumSource.ShouldContain("Right");
    }

    [Fact]
    public void Title_Bar_Title_Alignment_Defaults_To_Auto()
    {
        var property = typeof(AtomUI.Desktop.Controls.WindowTitleBar)
                       .GetProperty("TitleAlignment", BindingFlags.Instance | BindingFlags.Public);
        var propertyField = typeof(AtomUI.Desktop.Controls.WindowTitleBar)
                            .GetField("TitleAlignmentProperty", BindingFlags.Static | BindingFlags.Public);

        property.ShouldNotBeNull();
        propertyField.ShouldNotBeNull();
        property.PropertyType.ShouldBe(typeof(AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment));

        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();
        property.GetValue(titleBar).ShouldBe(AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment.Auto);
    }

    [Fact]
    public void Window_Adds_Owner_And_Projects_Title_Alignment_To_The_Title_Bar()
    {
        var property = typeof(AtomUI.Desktop.Controls.Window)
                       .GetProperty("TitleAlignment", BindingFlags.Instance | BindingFlags.Public);
        var propertyField = typeof(AtomUI.Desktop.Controls.Window)
                            .GetField("TitleAlignmentProperty", BindingFlags.Static | BindingFlags.Public);
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));

        property.ShouldNotBeNull();
        propertyField.ShouldNotBeNull();
        property.PropertyType.ShouldBe(typeof(AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment));
        source.ShouldContain("WindowTitleBar.TitleAlignmentProperty.AddOwner<Window>()");
        source.ShouldContain(
            "titleBar.Bind(WindowTitleBar.TitleAlignmentProperty, this.GetObservable(TitleAlignmentProperty))");
    }

    [Fact]
    public void Window_Projects_Chrome_Layout_Inputs_To_The_Title_Bar()
    {
        var windowInsetsProperty = typeof(AtomUI.Desktop.Controls.Window)
                                   .GetField(
                                       "NativeChromeInsetsProperty",
                                       BindingFlags.Static | BindingFlags.NonPublic);
        var titleBarInsetsProperty = typeof(AtomUI.Desktop.Controls.WindowTitleBar)
                                     .GetField(
                                         "NativeChromeInsetsProperty",
                                         BindingFlags.Static | BindingFlags.NonPublic);
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));

        windowInsetsProperty.ShouldNotBeNull();
        titleBarInsetsProperty.ShouldNotBeNull();
        source.ShouldContain("internal IDisposable CreateTitleBarHostProjection(WindowTitleBar titleBar)");
        source.ShouldContain("lease.Add(titleBar.Bind(");
        source.ShouldContain("WindowTitleBar.NativeChromeInsetsProperty");
        source.ShouldContain("this.GetObservable(NativeChromeInsetsProperty)");
        source.ShouldContain("WindowTitleBar.IsCsdEnabledProperty");
        source.ShouldContain("this.GetObservable(IsCsdEnabledProperty)");
        source.ShouldContain("WindowTitleBar.HostWindowStateProperty");
        source.ShouldContain("this.GetObservable(WindowStateProperty)");
    }

    [Fact]
    public void MacOS_Uses_The_Existing_Button_Frame_Metric_As_A_Layout_Input_Not_Host_Padding()
    {
        var windowSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var windowTheme = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml"));
        var macButtonsSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/MacStandardWindowButtons.cs"));

        windowSource.ShouldContain("GetRecommendedTitleBarContentLeftMargin(effectSpacing)");
        windowSource.ShouldContain("NativeChromeInsets = new Thickness(titleBarOffset, 0, 0, 0)");
        windowSource.ShouldContain("NativeChromeInsets = default;");
        windowSource.ShouldNotContain("TitleBarOffsetMargin");
        windowTheme.ShouldNotContain("TitleBarOffsetMargin");
        macButtonsSource.ShouldNotContain("NativeChromeInsets");
    }

    [Fact]
    public void Title_Bar_Logo_Auto_Mode_Uses_Title_Content_Platform_And_Fullscreen_State()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBar.cs"));
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar
        {
            Logo = new object()
        };
        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.Windows);

        source.ShouldContain("UpdateEffectiveLogoVisible()");
        source.ShouldContain("WindowTitleBarLogoVisibility.Always => hasLogo");
        source.ShouldContain("WindowTitleBarLogoVisibility.Never => false");
        source.ShouldContain("_ => hasLogo && ShouldShowLogoInAutoMode()");
        source.ShouldContain("HostWindowState != WindowState.FullScreen && isLogoVisibleByMode");
        source.ShouldContain("HostWindowState != WindowState.FullScreen");
        source.ShouldContain("OsType == OsType.macOS");
        source.ShouldContain("string text => !string.IsNullOrWhiteSpace(text)");

        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();
        titleBar.HostWindowState = WindowState.FullScreen;
        GetIsEffectiveLogoVisible(titleBar).ShouldBeFalse();
        titleBar.HostWindowState = WindowState.Normal;
        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();
    }

    [Fact]
    public void Title_Bar_Logo_Auto_Mode_Hides_Titleless_Logo_On_MacOS()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();

        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.macOS);
        titleBar.Logo = new object();

        GetIsEffectiveLogoVisible(titleBar).ShouldBeFalse();
    }

    [Fact]
    public void Title_Bar_Logo_Auto_Mode_Keeps_Titleless_Logo_On_Non_MacOS_When_Not_Fullscreen()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();

        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.Windows);
        titleBar.Logo = new object();

        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();
    }

    [Fact]
    public void Title_Bar_Logo_Always_Mode_Keeps_Titleless_Logo_On_MacOS()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();

        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.macOS);
        titleBar.LogoVisibility = AtomUI.Desktop.Controls.WindowTitleBarLogoVisibility.Always;
        titleBar.Logo           = new object();

        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();
    }

    [Fact]
    public void Title_Bar_Logo_Auto_Mode_Recomputes_When_OsType_Changes()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();

        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.Windows);
        titleBar.Logo = new object();

        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();

        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.macOS);

        GetIsEffectiveLogoVisible(titleBar).ShouldBeFalse();
    }

    [Fact]
    public void Window_Propagates_Logo_Visibility_To_Title_Bar_Without_Clearing_Window_Icon()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));

        source.ShouldContain("WindowTitleBar.LogoVisibilityProperty.AddOwner<Window>()");
        source.ShouldContain("public WindowTitleBarLogoVisibility LogoVisibility");
        source.ShouldContain(
            "titleBar.Bind(WindowTitleBar.LogoVisibilityProperty, this.GetObservable(LogoVisibilityProperty))");
        source.ShouldContain("IsEffectiveFullscreenLogoVisibleProperty");
        source.ShouldContain("EffectiveFullscreenLogoProperty");
        source.ShouldContain("EffectiveFullscreenLogoTemplateProperty");
        source.ShouldContain("UpdateEffectiveFullscreenLogoVisible()");
        source.ShouldContain("WindowState == WindowState.FullScreen && isLogoVisibleByMode");
        // Effective Logo 模式：回退解析只进 EffectiveLogo/EffectiveLogoTemplate 渲染层，
        // 框架不再向 Logo/LogoTemplate/Icon 写入任何默认值
        source.ShouldContain("UpdateEffectiveLogo()");
        source.ShouldContain("ResolveEffectiveLogo");
        source.ShouldContain("EffectiveLogoProperty");
        source.ShouldNotContain("TryApplyWindowIconLogo");
        source.ShouldNotContain("ApplyDefaultLogoIfNeeded");
        source.ShouldNotContain("SetCurrentValue(LogoProperty");
        source.ShouldNotContain("SetCurrentValue(LogoTemplateProperty");
    }

    [Fact]
    public void Title_Bar_Templates_Bind_Logo_Visibility_To_Effective_State()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarTheme.axaml"));

        Regex.Matches(source, "IsVisible=\"\\{TemplateBinding IsEffectiveLogoVisible\\}\"")
             .Count
             .ShouldBe(3);
        Regex.Matches(source, "Content=\"\\{TemplateBinding EffectiveLogoPresenterContent\\}\"")
             .Count
             .ShouldBe(3);
        Regex.Matches(source, "ContentTemplate=\"\\{TemplateBinding EffectiveLogoPresenterContentTemplate\\}\"")
             .Count
             .ShouldBe(3);
    }

    [Fact]
    public void Fullscreen_Title_Bar_Templates_Hide_Logo_With_Effective_Fullscreen_State()
    {
        var fullscreenPopoverSource =
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Themes/FullscreenPopoverLayerTheme.axaml"));
        var drawnDecorationsSource =
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml"));

        fullscreenPopoverSource.ShouldContain(
            "IsVisible=\"{Binding $parent[atom:Window].IsEffectiveFullscreenLogoVisible}\"");
        drawnDecorationsSource.ShouldContain(
            "IsVisible=\"{Binding $parent[atom:Window].IsEffectiveFullscreenLogoVisible}\"");

        foreach (var source in new[] { fullscreenPopoverSource, drawnDecorationsSource })
        {
            source.ShouldContain("Content=\"{Binding $parent[atom:Window].EffectiveFullscreenLogo}\"");
            source.ShouldContain(
                "ContentTemplate=\"{Binding $parent[atom:Window].EffectiveFullscreenLogoTemplate}\"");
            source.ShouldNotContain("Content=\"{Binding $parent[atom:Window].EffectiveLogo}\"");
            source.ShouldNotContain(
                "ContentTemplate=\"{Binding $parent[atom:Window].EffectiveLogoTemplate}\"");
        }
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }

    private static bool GetIsEffectiveLogoVisible(AtomUI.Desktop.Controls.WindowTitleBar titleBar)
    {
        var property = typeof(AtomUI.Desktop.Controls.WindowTitleBar)
                       .GetProperty("IsEffectiveLogoVisible", BindingFlags.Instance | BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        return (bool)property.GetValue(titleBar)!;
    }
}
