using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowFullScreenTitleBarTests
{
    static WindowFullScreenTitleBarTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static AtomUIWindow ShowWindow(Action<AtomUIWindow>? configure = null)
    {
        var window = new AtomUIWindow { Width = 600, Height = 400, Title = "Full screen" };
        configure?.Invoke(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return window;
    }

    private static FullScreenTitleBarHost Host(AtomUIWindow window)
    {
        return window.GetVisualDescendants().OfType<FullScreenTitleBarHost>().Single();
    }

    private static void Settle(AtomUIWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public void Default_Title_Bar_Instance_Moves_Into_The_Full_Screen_Slot_And_Back()
    {
        var rightAddOn = new Button { Content = "Session" };
        var window = ShowWindow(w => w.RightAddOn = rightAddOn);
        try
        {
            var titleBar = window.TitleBar.ShouldNotBeNull();
            window.NormalSlotTitleBar.ShouldBe(titleBar);
            window.EffectiveFullScreenTitleBarMode.ShouldBe(FullScreenTitleBarMode.Hidden);

            window.WindowState = WindowState.FullScreen;
            Settle(window);

            window.EffectiveFullScreenTitleBarMode.ShouldBe(FullScreenTitleBarMode.AutoHide);
            window.TitleBar.ShouldBeSameAs(titleBar);
            window.NormalSlotTitleBar.ShouldBeNull();
            window.FullScreenSlotContent.ShouldBeSameAs(titleBar);
            titleBar.GetVisualAncestors().ShouldContain(Host(window));
            titleBar.HostWindow.ShouldBe(window);
            titleBar.RightAddOn.ShouldBeSameAs(rightAddOn);

            window.WindowState = WindowState.Normal;
            Settle(window);

            window.NormalSlotTitleBar.ShouldBe(titleBar);
            window.FullScreenSlotContent.ShouldBeNull();
            titleBar.GetVisualAncestors().ShouldNotContain(Host(window));
            titleBar.HostWindow.ShouldBe(window);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Custom_Content_Replaces_Default_And_Hidden_Title_Bar_Resolves_To_Hidden()
    {
        var custom = new Border { Height = 32 };
        var window = ShowWindow(w => w.FullScreenTitleBar = custom);
        try
        {
            window.WindowState = WindowState.FullScreen;
            Settle(window);
            window.FullScreenSlotContent.ShouldBeSameAs(custom);
            window.NormalSlotTitleBar.ShouldBe(window.TitleBar);

            window.FullScreenTitleBar = null;
            window.IsTitleBarVisible  = false;
            Settle(window);
            window.EffectiveFullScreenTitleBarMode.ShouldBe(FullScreenTitleBarMode.Hidden);
            window.FullScreenSlotContent.ShouldBeNull();
            window.IsFullScreenTitleBarOpen.ShouldBeFalse();

            window.FullScreenTitleBarTemplate = new FuncDataTemplate<object?>((_, _) => new Border { Height = 20 });
            Settle(window);
            window.EffectiveFullScreenTitleBarMode.ShouldBe(FullScreenTitleBarMode.AutoHide);
            window.FullScreenSlotContentTemplate.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Open_State_Is_Coerced_By_Mode_And_Docked_Reserves_The_Surface_Height()
    {
        var window = ShowWindow(w =>
        {
            w.FullScreenTitleBarMode = FullScreenTitleBarMode.Pinned;
            w.Content                = new Border();
        });
        try
        {
            window.IsFullScreenTitleBarOpen.ShouldBeFalse();

            window.WindowState = WindowState.FullScreen;
            Settle(window);
            window.IsFullScreenTitleBarOpen.ShouldBeTrue();
            window.FullScreenContentInset.ShouldBe(default);
            window.IsFullScreenTitleBarOpen = false;
            window.IsFullScreenTitleBarOpen.ShouldBeTrue();

            window.FullScreenTitleBarMode = FullScreenTitleBarMode.Docked;
            Settle(window);
            var surfaceHeight = Host(window).SurfaceHeight;
            surfaceHeight.ShouldBeGreaterThan(0);
            window.FullScreenContentInset.Top.ShouldBe(surfaceHeight);
            var content = (Control)window.Content!;
            content.TranslatePoint(new Point(0, 0), window)!.Value.Y.ShouldBe(surfaceHeight, 0.5);

            window.FullScreenTitleBarMode = FullScreenTitleBarMode.Hidden;
            Settle(window);
            window.IsFullScreenTitleBarOpen.ShouldBeFalse();
            window.FullScreenContentInset.ShouldBe(default);
            window.NormalSlotTitleBar.ShouldBe(window.TitleBar);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void AutoHide_Opens_And_Closes_On_Explicit_Requests()
    {
        var window = ShowWindow(w => w.FullScreenTitleBarMouseLeaveDelay = 0);
        try
        {
            window.Activate();
            window.WindowState = WindowState.FullScreen;
            Settle(window);
            window.IsFullScreenTitleBarOpen.ShouldBeFalse();
            Host(window).IsSurfaceVisible.ShouldBeFalse();

            window.IsFullScreenTitleBarOpen = true;
            Settle(window);
            window.IsFullScreenTitleBarOpen.ShouldBeTrue();
            Host(window).IsSurfaceVisible.ShouldBeTrue();

            window.IsFullScreenTitleBarOpen = false;
            Settle(window);
            window.IsFullScreenTitleBarOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Owned_Window_Rejects_Full_Screen_Where_The_Platform_Cannot_Honour_It()
    {
        var owner = ShowWindow();
        var owned = new AtomUIWindow { Width = 300, Height = 200, Title = "Owned" };
        try
        {
            owned.Show(owner);
            Dispatcher.UIThread.RunJobs();
            // AppKit never takes an owned window into native full screen; other platforms accept the request.
            var supported = !OperatingSystem.IsMacOS();

            owned.IsFullScreenSupported.ShouldBe(supported);
            owned.CaptionButtonCommand.CanExecute(CaptionButtonAction.ToggleFullScreen).ShouldBe(supported);

            owned.WindowState = WindowState.FullScreen;
            Settle(owned);

            owned.WindowState.ShouldBe(supported ? WindowState.FullScreen : WindowState.Normal);
            owned.EffectiveFullScreenTitleBarMode.ShouldBe(
                supported ? FullScreenTitleBarMode.AutoHide : FullScreenTitleBarMode.Hidden);
            owned.NormalSlotTitleBar.ShouldBe(supported ? null : owned.TitleBar);
        }
        finally
        {
            owned.Close();
            owner.Close();
        }
    }

    [Fact]
    public void Toggle_Full_Screen_Restores_The_Previous_State()
    {
        var window = ShowWindow();
        try
        {
            window.ToggleFullScreen();
            window.WindowState.ShouldBe(WindowState.FullScreen);
            window.ToggleFullScreen();
            window.WindowState.ShouldBe(WindowState.Normal);
        }
        finally
        {
            window.Close();
        }
    }
}
