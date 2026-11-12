using AtomUI.Controls.Commons;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Controls.Tests.Badge;

public class BadgeExtensionTests
{
    static BadgeExtensionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestCountBadgeAdorner : AbstractCountBadgeAdorner
    {
        public TestCountBadgeAdorner()
        {
            BadgeShadowColor = Brushes.White;
            BadgeShadowSize  = 1;
            Template = new FuncControlTemplate<TestCountBadgeAdorner>((owner, scope) =>
            {
                var presenter = new ContentPresenter { Name = "PART_ContentPresenter" };
                presenter[!ContentPresenter.ContentProperty] = owner[!PresentedContentProperty];
                scope.Register(presenter.Name, presenter);
                var surface = new Border { Name = "PART_IndicatorSurface", Child = presenter };
                scope.Register(surface.Name, surface);
                return surface;
            });
        }
    }

    private sealed class TestCountBadge : AbstractCountBadge
    {
        internal AbstractCountBadgeAdorner? Adorner => _badgeAdorner;

        private protected override AbstractCountBadgeAdorner CreateBadgeAdorner()
        {
            if (_badgeAdorner is null)
            {
                _badgeAdorner = new TestCountBadgeAdorner();
                SetupTokenBindings();
                NotifyDecoratedTargetChanged();
                if (BadgeColor is not null)
                {
                    ConfigureDotColor(BadgeColor);
                }
            }

            return _badgeAdorner;
        }
    }

    private sealed class TestDotBadgeAdorner : AbstractDotBadgeAdorner
    {
    }

    private sealed class TestDotBadge : AbstractDotBadge
    {
        internal AbstractDotBadgeAdorner? Adorner => _dotBadgeAdorner;

        private protected override AbstractDotBadgeAdorner CreateDotBadgeAdorner()
        {
            if (_dotBadgeAdorner is null)
            {
                _dotBadgeAdorner = new TestDotBadgeAdorner();
                SetupTokenBindings();
                NotifyDecoratedTargetChanged();
                if (DotColor is not null)
                {
                    ConfigureDotColor(DotColor);
                }
            }

            return _dotBadgeAdorner;
        }
    }

    private sealed class TestWindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        internal TestWindowContext(Control content, double width = 240, double height = 240)
        {
            _window = new AvaloniaWindow
            {
                Width   = width,
                Height  = height,
                Content = new VisualLayerManager
                {
                    EnableAdornerLayer = true,
                    Child              = content
                }
            };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        internal AvaloniaWindow Window => _window;

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Color GlobalColor(Control owner, SharedTokenKind kind)
    {
        var snapshot = ThemeScope.ResolveContext(owner).ShouldNotBeNull().Snapshot;
        return new ThemeTokenResolver().GetGlobal<Color>(snapshot, (int)kind);
    }

    private static Color BadgeIndicatorColor(TestCountBadge badge)
    {
        var brush = badge.Adorner.ShouldNotBeNull().BadgeColor.ShouldBeAssignableTo<ISolidColorBrush>();
        return brush.ShouldNotBeNull().Color;
    }

    private static async Task ApplyAppearanceAsync(ThemeAppearance appearance)
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();
        var algorithms = appearance == ThemeAppearance.Dark
            ? new[] { ThemeAlgorithm.Default, ThemeAlgorithm.Dark }
            : new[] { ThemeAlgorithm.Default };
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID,
                new ThemeConfigBuilder().WithAlgorithms(algorithms).Build(),
                ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        Dispatcher.UIThread.RunJobs();
    }

    private static ScrollViewer CreateScrollViewer(Control content)
    {
        // The bare ScrollViewer has no template in the test app; supply the minimal presenter template.
        return new ScrollViewer
        {
            Width                     = 200,
            Height                    = 120,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content                   = content,
            Template = new FuncControlTemplate<ScrollViewer>((owner, scope) =>
            {
                var presenter = new ScrollContentPresenter
                {
                    Name                = "PART_ContentPresenter",
                    CanVerticallyScroll = true
                };
                presenter[!ContentPresenter.ContentProperty] = owner[!ContentControl.ContentProperty];
                presenter.Bind(ScrollContentPresenter.OffsetProperty,
                    new Binding(nameof(ScrollViewer.Offset)) { Source = owner, Mode = BindingMode.TwoWay });
                scope.Register(presenter.Name, presenter);
                return presenter;
            })
        };
    }

    private static void Pump(Control root)
    {
        Dispatcher.UIThread.RunJobs();
        root.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public void Content_replaces_count_and_is_always_visible()
    {
        var badge = new TestCountBadge { IsMotionEnabled = false };
        using var context = new TestWindowContext(badge);

        // Zero count with IsZeroVisible=false hides the badge.
        badge.BadgeIsVisible.ShouldBeFalse();

        badge.Content = "NEW";
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeTrue();
        var adorner = badge.Adorner.ShouldNotBeNull();
        adorner.PresentedContent.ShouldBe("NEW");
        adorner.GetVisualDescendants().OfType<ContentPresenter>()
               .Single(p => p.Name == "PART_ContentPresenter").Content.ShouldBe("NEW");

        // Content wins over a non-zero count.
        badge.Count = 7;
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeTrue();
        adorner.PresentedContent.ShouldBe("NEW");

        // Clearing Content falls back to the existing count logic.
        badge.Content = null;
        Pump(badge);
        adorner.PresentedContent.ShouldBe("7");
        badge.BadgeIsVisible.ShouldBeTrue();

        badge.Count = 0;
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeFalse();
    }

    [Fact]
    public void Is_bordered_defaults_true()
    {
        new TestCountBadge().IsBordered.ShouldBeTrue();

        var badge = new TestCountBadge { Count = 3, IsMotionEnabled = false };
        using var context = new TestWindowContext(badge);
        var adorner = badge.Adorner.ShouldNotBeNull();
        adorner.IsBordered.ShouldBeTrue();
        adorner.IndicatorShadows.Count.ShouldBe(1);

        badge.IsBordered = false;
        Pump(badge);
        adorner.IsBordered.ShouldBeFalse();
        adorner.IndicatorShadows.Count.ShouldBe(0);

        badge.IsBordered = true;
        Pump(badge);
        adorner.IndicatorShadows.Count.ShouldBe(1);
    }

    [Fact]
    public void Status_names_resolve_from_theme()
    {
        var badge = new TestCountBadge { Count = 1, IsMotionEnabled = false };
        using var context = new TestWindowContext(badge);

        var expected = new Dictionary<string, SharedTokenKind>
        {
            ["primary"] = SharedTokenKind.ColorPrimary,
            ["success"] = SharedTokenKind.ColorSuccess,
            ["warning"] = SharedTokenKind.ColorWarning,
            ["error"]   = SharedTokenKind.ColorError
        };
        foreach (var (name, kind) in expected)
        {
            badge.BadgeColor = name;
            Pump(badge);
            BadgeIndicatorColor(badge).ShouldBe(GlobalColor(badge, kind), name);
        }

        // Names are matched case-insensitively and trimmed.
        badge.BadgeColor = " Success ";
        Pump(badge);
        BadgeIndicatorColor(badge).ShouldBe(GlobalColor(badge, SharedTokenKind.ColorSuccess));

        // Hex values keep working.
        badge.BadgeColor = "#112233";
        Pump(badge);
        BadgeIndicatorColor(badge).ShouldBe(Color.Parse("#112233"));
    }

    [Fact]
    public void Danger_equals_error()
    {
        var badge = new TestCountBadge { Count = 1, IsMotionEnabled = false };
        using var context = new TestWindowContext(badge);

        badge.BadgeColor = "danger";
        Pump(badge);
        var danger = BadgeIndicatorColor(badge);
        badge.BadgeColor = "error";
        Pump(badge);
        var error = BadgeIndicatorColor(badge);

        danger.ShouldBe(error);
        danger.ShouldBe(GlobalColor(badge, SharedTokenKind.ColorError));
    }

    [Fact]
    public async Task Color_updates_after_theme_variant_change()
    {
        var badge = new TestCountBadge { Count = 1, IsMotionEnabled = false };
        using var context = new TestWindowContext(badge);
        await ApplyAppearanceAsync(ThemeAppearance.Light);

        try
        {
            // One status name and one preset name: both must follow the theme.
            foreach (var name in new[] { "success", "red" })
            {
                badge.BadgeColor = name;
                Pump(badge);
                var light = BadgeIndicatorColor(badge);

                await ApplyAppearanceAsync(ThemeAppearance.Dark);
                var dark = BadgeIndicatorColor(badge);
                dark.ShouldNotBe(light, $"'{name}' should be re-resolved for the dark theme.");

                await ApplyAppearanceAsync(ThemeAppearance.Light);
                BadgeIndicatorColor(badge).ShouldBe(light, $"'{name}' should return to the light value.");
            }
        }
        finally
        {
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }

    [Fact]
    public async Task Color_is_current_after_theme_switch_while_detached()
    {
        await ApplyAppearanceAsync(ThemeAppearance.Light);
        var countBadge = new TestCountBadge
        {
            Count           = 1,
            IsMotionEnabled = false,
            BadgeColor      = "success",
            DecoratedTarget = new Border { Width = 40, Height = 40 }
        };
        var dotBadge = new TestDotBadge
        {
            IsMotionEnabled = false,
            DotColor        = "success",
            DecoratedTarget = new Border { Width = 40, Height = 40 }
        };
        var host = new StackPanel { Children = { countBadge, dotBadge } };
        using var context = new TestWindowContext(host);
        Pump(host);

        Color CountColor() => countBadge.Adorner.ShouldNotBeNull().BadgeColor
                                        .ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
        Color DotColor() => dotBadge.Adorner.ShouldNotBeNull().BadgeDotColor
                                    .ShouldBeAssignableTo<ISolidColorBrush>()!.Color;

        try
        {
            var lightCount = CountColor();
            var lightDot = DotColor();
            lightCount.ShouldBe(GlobalColor(countBadge, SharedTokenKind.ColorSuccess));

            // Detach, switch the theme variant while no subscription exists, then re-attach.
            host.Children.Clear();
            Pump(host);
            await ApplyAppearanceAsync(ThemeAppearance.Dark);
            host.Children.Add(countBadge);
            host.Children.Add(dotBadge);
            Pump(host);

            var darkSuccess = GlobalColor(countBadge, SharedTokenKind.ColorSuccess);
            darkSuccess.ShouldNotBe(lightCount);
            CountColor().ShouldBe(darkSuccess);
            DotColor().ShouldBe(darkSuccess);
            DotColor().ShouldNotBe(lightDot);
        }
        finally
        {
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }

    [Fact]
    public void Inline_mode_does_not_track_ancestor_visibility()
    {
        var badge = new TestCountBadge { Count = 3, IsMotionEnabled = false };
        var host = new StackPanel { Children = { badge } };
        using var context = new TestWindowContext(host);

        badge.TrackedAncestorCount.ShouldBe(0);

        badge.DecoratedTarget = new Border { Width = 40, Height = 40 };
        Pump(host);
        badge.TrackedAncestorCount.ShouldBeGreaterThan(0);

        badge.DecoratedTarget = null;
        Pump(host);
        badge.TrackedAncestorCount.ShouldBe(0);
    }

    [Fact]
    public void Visibility_tracking_stops_at_adorner_layer_host()
    {
        var badge = new TestCountBadge
        {
            Count           = 3,
            IsMotionEnabled = false,
            DecoratedTarget = new Border { Width = 40, Height = 40 }
        };
        var inner = new StackPanel { Children = { badge } };
        var scroller = CreateScrollViewer(inner);
        var outer = new Border { Child = scroller };
        var host = new StackPanel { Children = { outer } };
        using var context = new TestWindowContext(host);
        Pump(host);

        // Tracked: the badge itself plus every ancestor below the visual parent of the adorner layer.
        var layerHost = AdornerLayer.GetAdornerLayer(badge).ShouldNotBeNull().GetVisualParent();
        layerHost.ShouldBeOfType<VisualLayerManager>();
        var expected = 1;
        foreach (var ancestor in badge.GetVisualAncestors())
        {
            if (ReferenceEquals(ancestor, layerHost))
            {
                break;
            }

            expected++;
        }

        badge.TrackedAncestorCount.ShouldBe(expected);
        var fullChain = 1 + badge.GetVisualAncestors().Count();
        badge.TrackedAncestorCount.ShouldBeLessThan(fullChain);
        TestContext.Current.SendDiagnosticMessage($"tracked={badge.TrackedAncestorCount} fullChain={fullChain}");

        // The scroll viewer is inside the observed range and still hides the adorner.
        var adorner = badge.Adorner.ShouldNotBeNull();
        adorner.IsVisible.ShouldBeTrue();
        scroller.IsVisible = false;
        Pump(host);
        adorner.IsVisible.ShouldBeFalse();
        scroller.IsVisible = true;
        Pump(host);
        adorner.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Hidden_ancestor_above_scroll_viewer_hides_adorner()
    {
        var badge = new TestCountBadge
        {
            Count           = 3,
            IsMotionEnabled = false,
            DecoratedTarget = new Border { Width = 40, Height = 40 }
        };
        // The panel stands for a page: it sits above the scroll container (a templated ScrollViewer whose
        // content host is a ScrollContentPresenter) and hides the whole subtree.
        var presenter = CreateScrollViewer(badge);
        var page = new Panel { Children = { presenter } };
        using var context = new TestWindowContext(page);
        Pump(page);

        var adorner = badge.Adorner.ShouldNotBeNull();
        adorner.IsVisible.ShouldBeTrue();

        page.IsVisible = false;
        Pump(page);
        badge.IsEffectivelyVisible.ShouldBeFalse();
        adorner.IsVisible.ShouldBeFalse("a hidden page above the scroll container must hide the layer adorner.");

        page.IsVisible = true;
        Pump(page);
        adorner.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Empty_string_content_hides_badge()
    {
        var badge = new TestCountBadge { Count = 5, IsMotionEnabled = false };
        using var context = new TestWindowContext(badge);
        badge.BadgeIsVisible.ShouldBeTrue();

        badge.Content = "";
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeFalse();

        // Any other non-null content is shown, even with a zero count.
        badge.Count = 0;
        badge.Content = " ";
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeTrue();

        badge.Content = 0;
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeTrue();

        // Null returns to the count logic.
        badge.Content = null;
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeFalse();
        badge.Count = 2;
        Pump(badge);
        badge.BadgeIsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Dot_is_bordered_default_and_toggle()
    {
        // The Desktop dot has no ring, so the default keeps its look.
        new TestDotBadge().IsBordered.ShouldBeFalse();

        var badge = new TestDotBadge
        {
            IsMotionEnabled = false,
            DecoratedTarget = new Border { Width = 40, Height = 40 }
        };
        using var context = new TestWindowContext(badge);
        var adorner = badge.Adorner.ShouldNotBeNull();
        adorner.IsBordered.ShouldBeFalse();

        badge.IsBordered = true;
        Pump(badge);
        adorner.IsBordered.ShouldBeTrue();

        badge.IsBordered = false;
        Pump(badge);
        adorner.IsBordered.ShouldBeFalse();
    }

    [Fact]
    public void Badge_in_scroll_viewer_follows_target()
    {
        var target = new Border { Width = 40, Height = 40, Margin = new Thickness(0, 100, 0, 0) };
        var badge = new TestCountBadge
        {
            Count           = 3,
            IsMotionEnabled = false,
            DecoratedTarget = target
        };
        var content = new StackPanel
        {
            Children =
            {
                badge,
                new Border { Height = 1000 }
            }
        };
        var scroller = CreateScrollViewer(content);
        scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        using var context = new TestWindowContext(scroller, 300, 300);
        Pump(scroller);

        var adorner = badge.Adorner.ShouldNotBeNull();
        var indicator = adorner.GetVisualDescendants().OfType<Border>()
                               .Single(b => b.Name == "PART_IndicatorSurface");
        var window = context.Window;

        double IndicatorTop() => indicator.TranslatePoint(new Point(0, 0), window)!.Value.Y;
        double TargetTop() => target.TranslatePoint(new Point(0, 0), window)!.Value.Y;

        var targetBefore = TargetTop();
        var indicatorBefore = IndicatorTop();

        var presenter = scroller.GetVisualDescendants().OfType<ScrollContentPresenter>().Single();
        presenter.Offset = new Vector(0, 30);
        Pump(scroller);

        var targetDelta = TargetTop() - targetBefore;
        var indicatorDelta = IndicatorTop() - indicatorBefore;
        targetDelta.ShouldBe(-30, 0.5);
        indicatorDelta.ShouldBe(targetDelta, 0.5, "the layer adorner must move with the scrolled target.");
    }
}
