using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.IconControls;

public class IconLoadingAnimationVisibilityTests
{
    static IconLoadingAnimationVisibilityTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Visible_Spin_Icon_Runs_After_Load()
    {
        var icon = CreateSpinIcon();
        var (outer, _) = CreateHost(icon);

        Show(outer, () => icon.IsLoadingAnimationRunning.ShouldBeTrue());
    }

    [Fact]
    public void Own_Visibility_Starts_And_Stops_Animation()
    {
        var icon = CreateSpinIcon();
        icon.IsVisible = false;
        var (outer, _) = CreateHost(icon);

        Show(outer, () =>
        {
            icon.IsLoadingAnimationRunning.ShouldBeFalse();

            icon.IsVisible = true;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeTrue();

            icon.IsVisible = false;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeFalse();
        });
    }

    [Fact]
    public void Ancestor_Visibility_Starts_And_Stops_Animation()
    {
        var icon = CreateSpinIcon();
        var (outer, inner) = CreateHost(icon);
        inner.IsVisible = false;

        Show(outer, () =>
        {
            icon.IsLoadingAnimationRunning.ShouldBeFalse();

            inner.IsVisible = true;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeTrue();

            outer.IsVisible = false;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeFalse();

            outer.IsVisible = true;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeTrue();
        });
    }

    [Fact]
    public void Showing_Icon_Under_Hidden_Ancestor_Waits_For_Ancestor()
    {
        var icon = CreateSpinIcon();
        icon.IsVisible = false;
        var (outer, inner) = CreateHost(icon);
        inner.IsVisible = false;

        Show(outer, () =>
        {
            icon.IsVisible = true;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeFalse();

            inner.IsVisible = true;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeTrue();
        });
    }

    [Fact]
    public void Changing_Loading_Animation_While_Loaded_Starts_And_Stops()
    {
        var icon = new LoadingOutlined
        {
            Width  = 24,
            Height = 24
        };
        var (outer, _) = CreateHost(icon);

        Show(outer, () =>
        {
            icon.IsLoadingAnimationRunning.ShouldBeFalse();

            icon.LoadingAnimation = IconAnimation.Spin;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeTrue();

            icon.LoadingAnimation = IconAnimation.Pulse;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeTrue();

            icon.LoadingAnimation = IconAnimation.None;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeFalse();
            icon.IsTrackingEffectiveVisibility.ShouldBeFalse();
        });
    }

    [Fact]
    public void Detach_Stops_And_Reattach_Restarts_Animation()
    {
        var icon = CreateSpinIcon();
        var (outer, inner) = CreateHost(icon);

        Show(outer, () =>
        {
            icon.IsLoadingAnimationRunning.ShouldBeTrue();

            inner.Child = null;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeFalse();
            icon.IsTrackingEffectiveVisibility.ShouldBeFalse();

            inner.Child = icon;
            Pump();
            icon.IsLoadingAnimationRunning.ShouldBeTrue();
        });
    }

    [Fact]
    public void Hidden_Spin_Icon_Does_Not_Track_Ancestors()
    {
        var icon = CreateSpinIcon();
        icon.IsVisible = false;
        var (outer, _) = CreateHost(icon);

        Show(outer, () =>
        {
            icon.IsTrackingEffectiveVisibility.ShouldBeFalse();

            icon.IsVisible = true;
            Pump();
            icon.IsTrackingEffectiveVisibility.ShouldBeTrue();

            icon.IsVisible = false;
            Pump();
            icon.IsTrackingEffectiveVisibility.ShouldBeFalse();
        });
    }

    [Fact]
    public void Static_Icon_Does_Not_Track_Ancestors()
    {
        var icon = new SearchOutlined
        {
            Width  = 24,
            Height = 24
        };
        var (outer, _) = CreateHost(icon);

        Show(outer, () =>
        {
            icon.IsTrackingEffectiveVisibility.ShouldBeFalse();
            icon.IsLoadingAnimationRunning.ShouldBeFalse();
        });
    }

    private static LoadingOutlined CreateSpinIcon()
    {
        return new LoadingOutlined
        {
            Width            = 24,
            Height           = 24,
            LoadingAnimation = IconAnimation.Spin
        };
    }

    private static (Border Outer, Border Inner) CreateHost(Control icon)
    {
        var inner = new Border { Child = icon };
        var outer = new Border { Child = new StackPanel { Children = { inner } } };
        return (outer, inner);
    }

    private static void Pump()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    private static void Show(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 120,
            Height  = 120,
            Content = content
        };

        try
        {
            window.Show();
            Pump();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
