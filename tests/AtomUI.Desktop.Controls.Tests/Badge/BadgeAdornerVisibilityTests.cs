using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Badge;

public class BadgeAdornerVisibilityTests
{
    static BadgeAdornerVisibilityTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void CountBadge_Layer_Adorner_Hides_When_Ancestor_Becomes_Invisible()
    {
        var badge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count            = 5,
            IsMotionEnabled  = false,
            DecoratedTarget  = new Border
            {
                Width  = 48,
                Height = 48
            }
        };
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInAdornerHost(host, width: 240, height: 160);

        var adorner = FindNativeAdorner(badge).ShouldNotBeNull();
        adorner.IsVisible.ShouldBeTrue();

        host.IsVisible = false;
        Dispatcher.UIThread.RunJobs();

        badge.IsEffectivelyVisible.ShouldBeFalse();
        adorner.IsVisible.ShouldBeFalse(
            "an ancestor IsVisible=false subtree keeps the badge attached, so the layer adorner must be hidden explicitly.");

        host.IsVisible = true;
        Dispatcher.UIThread.RunJobs();

        badge.IsEffectivelyVisible.ShouldBeTrue();
        adorner.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void DotBadge_Layer_Adorner_Hides_When_Ancestor_Becomes_Invisible()
    {
        var badge = new AtomUI.Desktop.Controls.DotBadge
        {
            Status          = AtomUI.Controls.Commons.DotBadgeStatus.Success,
            IsMotionEnabled = false,
            DecoratedTarget = new Border
            {
                Width  = 48,
                Height = 48
            }
        };
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInAdornerHost(host, width: 240, height: 160);

        var adorner = FindNativeAdorner(badge).ShouldNotBeNull();
        adorner.IsVisible.ShouldBeTrue();

        host.IsVisible = false;
        Dispatcher.UIThread.RunJobs();

        badge.IsEffectivelyVisible.ShouldBeFalse();
        adorner.IsVisible.ShouldBeFalse(
            "an ancestor IsVisible=false subtree keeps the badge attached, so the layer adorner must be hidden explicitly.");

        host.IsVisible = true;
        Dispatcher.UIThread.RunJobs();

        badge.IsEffectivelyVisible.ShouldBeTrue();
        adorner.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void CountBadge_Layer_Adorner_Stays_Hidden_While_Attached_Within_Invisible_Ancestor()
    {
        var badge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count            = 5,
            IsMotionEnabled  = false,
            DecoratedTarget  = new Border
            {
                Width  = 48,
                Height = 48
            }
        };
        var host = new DecoratedPanel
        {
            IsVisible = false,
            Child     = badge
        };
        using var context = ShowInAdornerHost(host, width: 240, height: 160);

        var adorner = FindNativeAdorner(badge).ShouldNotBeNull();
        badge.IsEffectivelyVisible.ShouldBeFalse();
        adorner.IsVisible.ShouldBeFalse(
            "a badge materialized inside a hidden subtree must not paint its indicator through the adorner layer.");

        host.IsVisible = true;
        Dispatcher.UIThread.RunJobs();

        adorner.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void CountBadge_Layer_Adorner_Hides_When_Badge_Itself_Becomes_Invisible()
    {
        var badge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count            = 5,
            IsMotionEnabled  = false,
            DecoratedTarget  = new Border
            {
                Width  = 48,
                Height = 48
            }
        };
        using var context = ShowInAdornerHost(badge, width: 240, height: 160);

        var adorner = FindNativeAdorner(badge).ShouldNotBeNull();
        adorner.IsVisible.ShouldBeTrue();

        badge.IsVisible = false;
        Dispatcher.UIThread.RunJobs();

        badge.IsEffectivelyVisible.ShouldBeFalse();
        adorner.IsVisible.ShouldBeFalse(
            "hiding the badge control itself must hide its layer indicator too.");

        badge.IsVisible = true;
        Dispatcher.UIThread.RunJobs();

        adorner.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void DotBadge_Detach_Clears_The_Adorned_Element_Before_Removing_The_Layer_Visual()
    {
        var badge = new AtomUI.Desktop.Controls.DotBadge
        {
            Status          = AtomUI.Controls.Commons.DotBadgeStatus.Success,
            IsMotionEnabled = false,
            DecoratedTarget = new Border { Width = 48, Height = 48 }
        };
        AssertAdornedElementClearedBeforeRemoval(badge);
    }

    [Fact]
    public void CountBadge_Detach_Clears_The_Adorned_Element_Before_Removing_The_Layer_Visual()
    {
        var badge = new AtomUI.Desktop.Controls.CountBadge
        {
            Count           = 5,
            IsMotionEnabled = false,
            DecoratedTarget = new Border { Width = 48, Height = 48 }
        };
        AssertAdornedElementClearedBeforeRemoval(badge);
    }

    /// <summary>
    /// Removing a layer adorner while it still names its adorned element lets the layer keep the old adorned visual
    /// reference in the removed composition visual; the association must be cleared while the layer can still update it.
    /// </summary>
    private static void AssertAdornedElementClearedBeforeRemoval(Control badge)
    {
        var host = new StackPanel { Children = { badge } };
        using var context = ShowInAdornerHost(host, width: 240, height: 160);

        var adorner      = FindNativeAdorner(badge).ShouldNotBeNull();
        var adornerLayer = AdornerLayer.GetAdornerLayer(badge).ShouldNotBeNull();
        Visual? adornedAtRemoval = badge;
        adornerLayer.Children.CollectionChanged += (_, args) =>
        {
            if (args.OldItems?.Contains(adorner) == true)
            {
                adornedAtRemoval = AdornerLayer.GetAdornedElement(adorner);
            }
        };

        host.Children.Remove(badge);
        Dispatcher.UIThread.RunJobs();

        adornerLayer.Children.ShouldNotContain(adorner);
        adornedAtRemoval.ShouldBeNull(
            "the adorned element must be cleared while the layer can still update the composition visual.");
    }

    private static Control? FindNativeAdorner(Control badge)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(badge);
        adornerLayer.ShouldNotBeNull();

        return adornerLayer.Children.SingleOrDefault(child =>
            ReferenceEquals(AdornerLayer.GetAdornedElement(child), badge));
    }

    private static WindowContext ShowInAdornerHost(Control control, double width = 120, double height = 100)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = control
        };
        var window = new AvaloniaWindow
        {
            Width   = width,
            Height  = height,
            Content = visualLayerManager
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window);
    }

    private sealed class DecoratedPanel : Decorator
    {
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        public WindowContext(AvaloniaWindow window)
        {
            _window = window;
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
