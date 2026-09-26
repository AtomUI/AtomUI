using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowTitleBarTitleVisibilityTests
{
    static WindowTitleBarTitleVisibilityTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Title_Bar_Is_Title_Visible_Defaults_To_True()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();
        titleBar.IsTitleVisible.ShouldBeTrue();
    }

    [Fact]
    public void Title_Bar_Effective_Title_Visibility_Follows_Title_Presence()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();

        GetIsEffectiveTitleVisible(titleBar).ShouldBeFalse(); // Title == null

        titleBar.Title = "AtomUI";
        GetIsEffectiveTitleVisible(titleBar).ShouldBeTrue();

        titleBar.Title = null;
        GetIsEffectiveTitleVisible(titleBar).ShouldBeFalse();
    }

    [Fact]
    public void Title_Bar_Effective_Title_Visibility_Respects_Is_Title_Visible()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar { Title = "AtomUI" };

        titleBar.IsTitleVisible = false;
        GetIsEffectiveTitleVisible(titleBar).ShouldBeFalse();

        titleBar.IsTitleVisible = true;
        GetIsEffectiveTitleVisible(titleBar).ShouldBeTrue();
    }

    [Fact]
    public void Title_Bar_Effective_Title_Visibility_Keeps_Empty_String_Semantics()
    {
        // 现行为：Title 为空字符串（非 null）时 presenter 可见。IsTitleVisible=true 默认路径必须逐位保持。
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar { Title = string.Empty };
        GetIsEffectiveTitleVisible(titleBar).ShouldBeTrue();
    }

    [Fact]
    public void Title_Bar_Logo_Auto_Mode_Hides_Logo_When_Title_Hidden_On_MacOS()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();
        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.macOS);
        titleBar.Logo  = new object();
        titleBar.Title = "AtomUI";

        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();

        titleBar.IsTitleVisible = false;
        GetIsEffectiveLogoVisible(titleBar).ShouldBeFalse();
    }

    [Fact]
    public void Title_Bar_Logo_Auto_Mode_Keeps_Logo_When_Title_Hidden_On_Non_MacOS()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar();
        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.Windows);
        titleBar.Logo           = new object();
        titleBar.Title          = "AtomUI";
        titleBar.IsTitleVisible = false;

        // 无标题分支：非 macOS、非全屏时 Logo 仍显示
        GetIsEffectiveLogoVisible(titleBar).ShouldBeTrue();
    }

    [Fact]
    public void Hidden_Title_Presenter_Does_Not_Hide_Caption_Buttons()
    {
        var titleBar = new AtomUI.Desktop.Controls.WindowTitleBar
        {
            Width          = 360,
            Height         = 40,
            Title          = "AtomUI",
            IsTitleVisible = false
        };
        titleBar.SetValue(AtomUI.Desktop.Controls.WindowTitleBar.OsTypeProperty, OsType.Windows);
        Application.Current!.TryFindResource(typeof(AtomUI.Desktop.Controls.WindowTitleBar), out var resource)
                   .ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();

            // CaptionButtonFrame 等内嵌部件模板同样定义 PART_ContentPresenter，
            // 标题部件必须在 Title 角色子树内查找，不能假设整棵树名字唯一。
            var titleLayout = titleBar.GetVisualDescendants()
                                      .OfType<Desktop.Controls.WindowTitleBarLayoutPanel>()
                                      .Single()
                                      .Children
                                      .Single(child =>
                                          Desktop.Controls.WindowTitleBarLayoutPanel.GetRole(child) ==
                                          Desktop.Controls.WindowTitleBarLayoutRole.Title);
            var titlePresenter = titleLayout.GetVisualDescendants()
                                            .OfType<ContentPresenter>()
                                            .Single(presenter => presenter.Name == "PART_ContentPresenter");
            var captionButtons = titleBar.GetVisualDescendants()
                                         .OfType<Control>()
                                         .Single(control => control.Name == "PART_CaptionButtonGroup");

            titlePresenter.IsVisible.ShouldBeFalse();
            captionButtons.IsVisible.ShouldBeTrue();
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Title_Bar_Templates_Bind_Title_Visibility_To_Effective_State()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarTheme.axaml"));

        Regex.Matches(source, "IsVisible=\"\\{TemplateBinding IsEffectiveTitleVisible\\}\"")
             .Count
             .ShouldBe(3);
    }

    [Fact]
    public void Window_Adds_Owner_And_Projects_Is_Title_Visible_To_The_Title_Bar()
    {
        var property = typeof(AtomUI.Desktop.Controls.Window)
                       .GetProperty("IsTitleVisible", BindingFlags.Instance | BindingFlags.Public);
        var propertyField = typeof(AtomUI.Desktop.Controls.Window)
                            .GetField("IsTitleVisibleProperty", BindingFlags.Static | BindingFlags.Public);
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));

        property.ShouldNotBeNull();
        propertyField.ShouldNotBeNull();
        property.PropertyType.ShouldBe(typeof(bool));
        source.ShouldContain("WindowTitleBar.IsTitleVisibleProperty.AddOwner<Window>()");
        source.ShouldContain(
            "titleBar.Bind(WindowTitleBar.IsTitleVisibleProperty, this.GetObservable(IsTitleVisibleProperty))");
    }

    [Fact]
    public void Window_Hiding_Title_Text_Preserves_The_Os_Window_Title()
    {
        var window = new AtomUI.Desktop.Controls.Window
        {
            Title          = "AtomUI Demo",
            IsTitleVisible = false
        };

        window.Title.ShouldBe("AtomUI Demo");
    }

    [Fact]
    public void Fullscreen_Layers_Bind_Title_Text_To_Effective_Title_Visibility()
    {
        var themePaths = new[]
        {
            "src/AtomUI.Desktop.Controls/Window/Themes/FullscreenPopoverLayerTheme.axaml",
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml"
        };

        foreach (var themePath in themePaths)
        {
            var source = File.ReadAllText(GetRepoFile(themePath));
            source.ShouldContain("Text=\"{Binding $parent[atom:Window].Title}\"");
            source.ShouldContain(
                "IsVisible=\"{Binding $parent[atom:Window].IsEffectiveFullscreenTitleVisible}\"");
        }
    }

    [Fact]
    public void Window_Fullscreen_Logo_Auto_Mode_Follows_Hidden_Title()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));

        source.ShouldContain("UpdateEffectiveFullscreenTitleVisible()");
        source.ShouldContain(
            "_ => hasLogo && HasTitleContent(Title) && IsEffectiveFullscreenTitleVisible");
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

    private static bool GetIsEffectiveTitleVisible(AtomUI.Desktop.Controls.WindowTitleBar titleBar)
    {
        var property = typeof(AtomUI.Desktop.Controls.WindowTitleBar)
                       .GetProperty("IsEffectiveTitleVisible", BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        return (bool)property.GetValue(titleBar)!;
    }

    private static bool GetIsEffectiveLogoVisible(AtomUI.Desktop.Controls.WindowTitleBar titleBar)
    {
        var property = typeof(AtomUI.Desktop.Controls.WindowTitleBar)
                       .GetProperty("IsEffectiveLogoVisible", BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        return (bool)property.GetValue(titleBar)!;
    }
}
