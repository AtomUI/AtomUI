using System.Reflection;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

using AtomUIWindow = AtomUI.Desktop.Controls.Window;

public class WindowCaptionButtonConfigurationTests
{
    static WindowCaptionButtonConfigurationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Window_Exposes_Independent_Caption_Button_Visibility_Defaults()
    {
        var window = new AtomUIWindow();

        window.IsMinimizeCaptionButtonVisible.ShouldBeTrue();
        window.IsMaximizeCaptionButtonVisible.ShouldBeTrue();
        window.IsCloseCaptionButtonVisible.ShouldBeTrue();
        window.IsFullScreenCaptionButtonVisible.ShouldBeFalse();
        window.IsPinCaptionButtonVisible.ShouldBeFalse();

        AssertPublicStyledProperty(nameof(AtomUIWindow.IsMinimizeCaptionButtonVisible));
        AssertPublicStyledProperty(nameof(AtomUIWindow.IsMaximizeCaptionButtonVisible));
        AssertPublicStyledProperty(nameof(AtomUIWindow.IsCloseCaptionButtonVisible));
        AssertPublicStyledProperty(nameof(AtomUIWindow.IsFullScreenCaptionButtonVisible));
        AssertPublicStyledProperty(nameof(AtomUIWindow.IsPinCaptionButtonVisible));
    }

    [Fact]
    public void Caption_Button_Visibility_Does_Not_Change_Window_Capability()
    {
        var window = new AtomUIWindow
        {
            CanMinimize = true,
            CanMaximize = true
        };

        window.IsMinimizeCaptionButtonVisible = false;
        window.IsMaximizeCaptionButtonVisible = false;

        window.CanMinimize.ShouldBeTrue();
        window.CanMaximize.ShouldBeTrue();

        window.CanMinimize = false;
        window.CanMaximize = false;
        window.IsMinimizeCaptionButtonVisible = true;
        window.IsMaximizeCaptionButtonVisible = true;

        window.CanMinimize.ShouldBeFalse();
        window.CanMaximize.ShouldBeFalse();
    }

    [Fact]
    public void Window_Host_Projection_Projects_Caption_Inputs_Command_And_Host_Association_To_Title_Bar()
    {
        var window = new AtomUIWindow
        {
            IsMinimizeCaptionButtonVisible = false,
            IsMaximizeCaptionButtonVisible = false,
            IsCloseCaptionButtonVisible = false,
            IsFullScreenCaptionButtonVisible = true,
            IsPinCaptionButtonVisible = true,
            CanMinimize = false,
            CanMaximize = false,
            Topmost = true,
            WindowState = WindowState.Maximized
        };
        var titleBar = new WindowTitleBar();

        titleBar.AttachHost(window);

        titleBar.HostWindow.ShouldBeSameAs(window);
        titleBar.IsMinimizeCaptionButtonVisible.ShouldBeFalse();
        titleBar.IsMaximizeCaptionButtonVisible.ShouldBeFalse();
        titleBar.IsCloseCaptionButtonVisible.ShouldBeFalse();
        titleBar.IsFullScreenCaptionButtonVisible.ShouldBeTrue();
        titleBar.IsPinCaptionButtonVisible.ShouldBeTrue();
        titleBar.CanMinimize.ShouldBeFalse();
        titleBar.CanMaximize.ShouldBeFalse();
        titleBar.IsWindowTopmost.ShouldBeTrue();
        titleBar.HostWindowState.ShouldBe(WindowState.Maximized);
        titleBar.CaptionButtonCommand.ShouldBeSameAs(window.CaptionButtonCommand);

        window.IsMinimizeCaptionButtonVisible = true;
        window.CanMinimize = true;
        window.Topmost = false;
        window.WindowState = WindowState.Normal;

        titleBar.IsMinimizeCaptionButtonVisible.ShouldBeTrue();
        titleBar.CanMinimize.ShouldBeTrue();
        titleBar.IsWindowTopmost.ShouldBeFalse();
        titleBar.HostWindowState.ShouldBe(WindowState.Normal);

        titleBar.DetachHost(window);
        titleBar.HostWindow.ShouldBeNull();
        titleBar.CaptionButtonCommand.ShouldBeNull();
    }

    [Fact]
    public void Content_Title_Bar_Discovers_Host_And_Executes_Caption_Command()
    {
        var titleBar = new WindowTitleBar
        {
            Title = "Content title"
        };
        var window = new AtomUIWindow
        {
            IsTitleBarVisible = false,
            Title = "Host title",
            Content = titleBar
        };

        try
        {
            window.Show();

            titleBar.HostWindow.ShouldBeSameAs(window);
            titleBar.Title.ShouldBe("Content title");
            var command = titleBar.CaptionButtonCommand.ShouldNotBeNull();
            command.ShouldBeSameAs(window.CaptionButtonCommand);

            command.Execute(CaptionButtonAction.ToggleMaximize);

            window.WindowState.ShouldBe(WindowState.Maximized);

            command.Execute(CaptionButtonAction.ToggleMaximize);
            window.WindowState.ShouldBe(WindowState.Normal);

            command.Execute(CaptionButtonAction.Close);
            window.IsVisible.ShouldBeFalse();
            titleBar.HostWindow.ShouldBeNull();
            titleBar.CaptionButtonCommand.ShouldBeNull();
        }
        finally
        {
            if (window.IsVisible)
            {
                window.Close();
            }
        }

        titleBar.HostWindow.ShouldBeNull();
        titleBar.CaptionButtonCommand.ShouldBeNull();
    }

    [Fact]
    public void Content_Title_Bar_Double_Click_Request_Toggles_Only_The_Current_Host_Window()
    {
        var titleBar = new WindowTitleBar();
        var content = new StackPanel
        {
            Children =
            {
                titleBar
            }
        };
        var window = new AtomUIWindow
        {
            IsTitleBarVisible = false,
            Content = content
        };

        try
        {
            window.Show();

            RaiseTitleBarDoubleClick(titleBar);
            window.WindowState.ShouldBe(WindowState.Maximized);

            RaiseTitleBarDoubleClick(titleBar);
            window.WindowState.ShouldBe(WindowState.Normal);

            content.Children.Remove(titleBar);
            RaiseTitleBarDoubleClick(titleBar);
            window.WindowState.ShouldBe(WindowState.Normal);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Multiple_Content_Title_Bars_Own_Independent_Host_Projection_Leases()
    {
        var firstTitleBar  = new WindowTitleBar();
        var secondTitleBar = new WindowTitleBar();
        var content = new StackPanel
        {
            Children =
            {
                firstTitleBar,
                secondTitleBar
            }
        };
        var window = new AtomUIWindow
        {
            IsTitleBarVisible = false,
            IsMinimizeCaptionButtonVisible = false,
            Content = content
        };

        try
        {
            window.Show();

            firstTitleBar.HostWindow.ShouldBeSameAs(window);
            secondTitleBar.HostWindow.ShouldBeSameAs(window);
            firstTitleBar.IsMinimizeCaptionButtonVisible.ShouldBeFalse();
            secondTitleBar.IsMinimizeCaptionButtonVisible.ShouldBeFalse();

            content.Children.Remove(firstTitleBar);

            firstTitleBar.HostWindow.ShouldBeNull();
            firstTitleBar.CaptionButtonCommand.ShouldBeNull();
            secondTitleBar.HostWindow.ShouldBeSameAs(window);
            secondTitleBar.CaptionButtonCommand.ShouldBeSameAs(window.CaptionButtonCommand);

            window.IsMinimizeCaptionButtonVisible = true;

            firstTitleBar.IsMinimizeCaptionButtonVisible.ShouldBeTrue();
            secondTitleBar.IsMinimizeCaptionButtonVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }

        secondTitleBar.HostWindow.ShouldBeNull();
        secondTitleBar.CaptionButtonCommand.ShouldBeNull();
    }

    [Fact]
    public void Content_Title_Bar_Releases_Old_Host_Before_Attaching_To_New_Host()
    {
        var titleBar = new WindowTitleBar();
        var firstContent  = new StackPanel();
        var secondContent = new StackPanel();
        var firstWindow = new AtomUIWindow
        {
            IsTitleBarVisible = false,
            CanMinimize = false,
            Content = firstContent
        };
        var secondWindow = new AtomUIWindow
        {
            IsTitleBarVisible = false,
            CanMinimize = true,
            Content = secondContent
        };

        firstContent.Children.Add(titleBar);

        try
        {
            firstWindow.Show();
            secondWindow.Show();

            titleBar.HostWindow.ShouldBeSameAs(firstWindow);
            titleBar.CanMinimize.ShouldBeFalse();

            firstContent.Children.Remove(titleBar);
            secondContent.Children.Add(titleBar);

            titleBar.HostWindow.ShouldBeSameAs(secondWindow);
            titleBar.CaptionButtonCommand.ShouldBeSameAs(secondWindow.CaptionButtonCommand);
            titleBar.CanMinimize.ShouldBeTrue();

            firstWindow.CanMinimize = true;
            firstWindow.CanMinimize = false;

            titleBar.CanMinimize.ShouldBeTrue();

            firstWindow.Close();

            titleBar.HostWindow.ShouldBeSameAs(secondWindow);
            titleBar.CaptionButtonCommand.ShouldBeSameAs(secondWindow.CaptionButtonCommand);
        }
        finally
        {
            if (firstWindow.IsVisible)
            {
                firstWindow.Close();
            }
            if (secondWindow.IsVisible)
            {
                secondWindow.Close();
            }
        }

        titleBar.HostWindow.ShouldBeNull();
        titleBar.CaptionButtonCommand.ShouldBeNull();
    }

    [Fact]
    public void Default_Title_Bar_Host_Projection_Does_Not_Depend_On_Configure_Override_Calling_Base()
    {
        var window = new ConfigureOverrideWindow();

        try
        {
            window.Show();

            window.ConfigureTitleBarCalled.ShouldBeTrue();
            var titleBar = window.TitleBar.ShouldNotBeNull();
            titleBar.HostWindow.ShouldBeSameAs(window);
            titleBar.CaptionButtonCommand.ShouldBeSameAs(window.CaptionButtonCommand);

            RaiseTitleBarDoubleClick(titleBar);
            window.WindowState.ShouldBe(WindowState.Maximized);

            RaiseTitleBarDoubleClick(titleBar);
            window.WindowState.ShouldBe(WindowState.Normal);
        }
        finally
        {
            window.Close();
        }

        window.TitleBar.ShouldNotBeNull().HostWindow.ShouldBeNull();
    }

    [Theory]
    [InlineData(WindowState.Normal, true, true, true, true, true)]
    [InlineData(WindowState.Minimized, true, true, true, true, true)]
    [InlineData(WindowState.Maximized, true, true, false, true, true)]
    [InlineData(WindowState.FullScreen, false, false, true, true, true)]
    public void Caption_Button_Group_Derives_Effective_Visibility_From_Window_State(
        WindowState state,
        bool minimizeVisible,
        bool maximizeVisible,
        bool fullScreenVisible,
        bool pinVisible,
        bool closeVisible)
    {
        var group = CreateFullyEnabledGroup();

        group.HostWindowState = state;

        group.IsMinimizeButtonEffectivelyVisible.ShouldBe(minimizeVisible);
        group.IsMaximizeButtonEffectivelyVisible.ShouldBe(maximizeVisible);
        group.IsFullScreenButtonEffectivelyVisible.ShouldBe(fullScreenVisible);
        group.IsPinButtonEffectivelyVisible.ShouldBe(pinVisible);
        group.IsCloseButtonEffectivelyVisible.ShouldBe(closeVisible);
        group.IsWindowMaximized.ShouldBe(state == WindowState.Maximized);
        group.IsWindowFullScreen.ShouldBe(state == WindowState.FullScreen);
    }

    [Fact]
    public void Caption_Button_Group_Combines_Requested_Visibility_With_Capability()
    {
        var group = CreateFullyEnabledGroup();

        group.CanMinimize = false;
        group.CanMaximize = false;
        group.IsPinCaptionButtonSupported = false;

        group.IsMinimizeButtonEffectivelyVisible.ShouldBeFalse();
        group.IsMaximizeButtonEffectivelyVisible.ShouldBeFalse();
        group.IsPinButtonEffectivelyVisible.ShouldBeFalse();
        group.IsFullScreenButtonEffectivelyVisible.ShouldBeTrue();
        group.IsCloseButtonEffectivelyVisible.ShouldBeTrue();

        group.CanMinimize = true;
        group.CanMaximize = true;
        group.IsPinCaptionButtonSupported = true;
        group.IsMinimizeCaptionButtonVisible = false;
        group.IsMaximizeCaptionButtonVisible = false;
        group.IsFullScreenCaptionButtonVisible = false;
        group.IsPinCaptionButtonVisible = false;
        group.IsCloseCaptionButtonVisible = false;

        group.IsMinimizeButtonEffectivelyVisible.ShouldBeFalse();
        group.IsMaximizeButtonEffectivelyVisible.ShouldBeFalse();
        group.IsFullScreenButtonEffectivelyVisible.ShouldBeFalse();
        group.IsPinButtonEffectivelyVisible.ShouldBeFalse();
        group.IsCloseButtonEffectivelyVisible.ShouldBeFalse();
    }

    [Fact]
    public void Window_Caption_Command_Owns_State_Transitions_And_Fullscreen_Restoration()
    {
        var window = new AtomUIWindow
        {
            CanMinimize = false,
            CanMaximize = false,
            WindowState = WindowState.Normal
        };
        var command = window.CaptionButtonCommand;

        command.CanExecute(CaptionButtonAction.Minimize).ShouldBeFalse();
        command.CanExecute(CaptionButtonAction.ToggleMaximize).ShouldBeFalse();

        window.CanMinimize = true;
        window.CanMaximize = true;

        command.CanExecute(CaptionButtonAction.Minimize).ShouldBeTrue();
        command.Execute(CaptionButtonAction.Minimize);
        window.WindowState.ShouldBe(WindowState.Minimized);

        window.WindowState = WindowState.Normal;
        command.Execute(CaptionButtonAction.ToggleMaximize);
        window.WindowState.ShouldBe(WindowState.Maximized);
        command.Execute(CaptionButtonAction.ToggleMaximize);
        window.WindowState.ShouldBe(WindowState.Normal);

        window.WindowState = WindowState.Maximized;
        window.WindowState = WindowState.FullScreen;
        command.Execute(CaptionButtonAction.ToggleFullScreen);
        window.WindowState.ShouldBe(WindowState.Maximized);

        window.WindowState = WindowState.FullScreen;
        command.CanExecute(CaptionButtonAction.Minimize).ShouldBeFalse();
        command.CanExecute(CaptionButtonAction.ToggleMaximize).ShouldBeFalse();
    }

    [Fact]
    public void Caption_Button_Themes_Use_Declarative_Command_Parameters_Without_Group_Host_Wiring()
    {
        var groupSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/CaptionButtonGroup.cs"));
        var titleBarSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBar.cs"));
        var groupTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml"));
        var popoverSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Utils/FullscreenPopoverLayer.cs"));
        var popoverTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/FullscreenPopoverLayerTheme.axaml"));
        var drawnDecorationsTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml"));
        XNamespace atom = "https://atomui.net";

        groupSource.ShouldNotContain("Window? HostWindow");
        groupSource.ShouldNotContain("HostWindow { get;");
        groupSource.ShouldNotContain("void Attach(");
        groupSource.ShouldNotContain("void Detach(");
        groupSource.ShouldNotContain(".Click +=");
        groupSource.ShouldNotContain("Find<CaptionButton>");
        titleBarSource.ShouldContain("FindLogicalAncestorOfType<Window>()");

        AssertCommand(groupTheme, atom, "PART_MinimizeButton", CaptionButtonAction.Minimize);
        AssertCommand(groupTheme, atom, "PART_MaximizeButton", CaptionButtonAction.ToggleMaximize);
        AssertCommand(groupTheme, atom, "PART_FullScreenButton", CaptionButtonAction.ToggleFullScreen);
        AssertCommand(groupTheme, atom, "PART_PinButton", CaptionButtonAction.TogglePin);
        AssertCommand(groupTheme, atom, "PART_CloseButton", CaptionButtonAction.Close);

        popoverSource.ShouldNotContain(".Click +=");
        AssertCommand(popoverTheme, atom, "PART_PopoverFullScreenButton", CaptionButtonAction.ToggleFullScreen);
        AssertCommand(popoverTheme, atom, "PART_PopoverCloseButton", CaptionButtonAction.Close);
        AssertVisibilityBinding(
            popoverTheme,
            atom,
            "PART_PopoverFullScreenButton",
            "IsFullScreenCaptionButtonVisible");
        AssertVisibilityBinding(
            popoverTheme,
            atom,
            "PART_PopoverCloseButton",
            "IsCloseCaptionButtonVisible");
        AssertVisibilityBinding(
            drawnDecorationsTheme,
            atom,
            "PART_FullScreenButton",
            "IsFullScreenCaptionButtonVisible");
        AssertVisibilityBinding(
            drawnDecorationsTheme,
            atom,
            "PART_CloseButton",
            "IsCloseCaptionButtonVisible");
    }

    private static CaptionButtonGroup CreateFullyEnabledGroup()
    {
        return new CaptionButtonGroup
        {
            IsMinimizeCaptionButtonVisible = true,
            IsMaximizeCaptionButtonVisible = true,
            IsCloseCaptionButtonVisible = true,
            IsFullScreenCaptionButtonVisible = true,
            IsPinCaptionButtonVisible = true,
            CanMinimize = true,
            CanMaximize = true,
            IsPinCaptionButtonSupported = true
        };
    }

    private static void AssertPublicStyledProperty(string propertyName)
    {
        typeof(AtomUIWindow)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            .ShouldNotBeNull()
            .PropertyType.ShouldBe(typeof(bool));
        typeof(AtomUIWindow)
            .GetField($"{propertyName}Property", BindingFlags.Static | BindingFlags.Public)
            .ShouldNotBeNull()
            .GetValue(null)
            .ShouldBeAssignableTo<StyledProperty<bool>>();
    }

    private static void AssertCommand(
        XDocument document,
        XNamespace atom,
        string partName,
        CaptionButtonAction action)
    {
        var buttons = document.Descendants()
                              .Where(element =>
                                  element.Name.Namespace == atom &&
                                  element.Name.LocalName is "CaptionButton" or "WindowsCaptionButton" &&
                                  (string?)element.Attribute("Name") == partName)
                              .ToList();

        buttons.ShouldNotBeEmpty();
        buttons.ShouldAllBe(button =>
            (string?)button.Attribute("Command") == "{TemplateBinding CaptionButtonCommand}" ||
            (string?)button.Attribute("Command") ==
            "{Binding $parent[atom:Window].CaptionButtonCommand}");
        buttons.ShouldAllBe(button =>
            (string?)button.Attribute("CommandParameter") ==
            $"{{x:Static atom:CaptionButtonAction.{action}}}");
    }

    private static void AssertVisibilityBinding(
        XDocument document,
        XNamespace atom,
        string partName,
        string propertyName)
    {
        document.Descendants(atom + "CaptionButton")
                .Single(element => (string?)element.Attribute("Name") == partName)
                .Attribute("IsVisible")
                .ShouldNotBeNull()
                .Value.ShouldBe($"{{Binding $parent[atom:Window].{propertyName}}}");
    }

    private sealed class ConfigureOverrideWindow : AtomUIWindow
    {
        internal bool ConfigureTitleBarCalled { get; private set; }

        protected override void NotifyConfigureTitleBar(WindowTitleBar titleBar)
        {
            ConfigureTitleBarCalled = true;
        }
    }

    private static void RaiseTitleBarDoubleClick(WindowTitleBar titleBar)
    {
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        titleBar.RaiseEvent(new PointerPressedEventArgs(
            titleBar,
            pointer,
            titleBar,
            default,
            0,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None,
            2));
        titleBar.RaiseEvent(new PointerReleasedEventArgs(
            titleBar,
            pointer,
            titleBar,
            default,
            1,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
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
}
