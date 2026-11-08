using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the tag / badge family (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override reaches
/// X only, the root Token reaches X, registered controls nested in a member (the Tag's close IconButton, the group's
/// CheckableTags) keep their own identity, and the badge indicators living in the adorner layer follow their badge.
/// </summary>
public partial class TagBadgeFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    /// <summary>Each member in a state showing its main colour Token.</summary>
    private static (ControlTokenIdentity Identity, string Token, Control Control)[] CreateIsolationMembers() =>
    [
        (TagTokens.Identity, "ColorBorder", new Tag { Text = "Outlined", Variant = TagVariant.Outlined }),
        (CheckableTagTokens.Identity, "ColorPrimary", new CheckableTag { Content = "Checked", IsChecked = true }),
        (CountBadgeTokens.Identity, "ColorError", new CountBadge { Count = 5, IsMotionEnabled = false }),
        (DotBadgeTokens.Identity, "ColorSuccess",
            new DotBadge { Status = DotBadgeStatus.Success, Text = "Success", IsMotionEnabled = false }),
        (RibbonBadgeTokens.Identity, "ColorPrimary", new RibbonBadge { Text = "Ribbon" })
    ];

    [Fact]
    public void Controls_X_Main_Colour_Changes_Only_X()
    {
        var members = CreateIsolationMembers();
        WithControls(members.Select(static m => m.Control).ToArray(), (provider, capture) =>
        {
            foreach (var (identity, token, target) in members)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, token, IsolationColor));
                using var after = capture();

                foreach (var (_, _, control) in members)
                {
                    var ratio = ChangedRatioOf(before, after, control);
                    if (ReferenceEquals(control, target))
                    {
                        ratio.ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                            $"Controls[{identity}].{token} did not reach {control.GetType().Name}.");
                    }
                    else
                    {
                        ratio.ShouldBe(0d, $"Controls[{identity}].{token} leaked into {control.GetType().Name}.");
                    }
                }
            }
        });
    }

    [Fact]
    public void Root_Main_Colours_Change_Every_Member()
    {
        var members = CreateIsolationMembers();
        WithControls(members.Select(static m => m.Control).ToArray(), (provider, capture) =>
        {
            foreach (var (_, token, target) in members)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(root: (token, IsolationColor));
                using var after = capture();
                ChangedRatioOf(before, after, target).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                    $"Root {token} did not reach {target.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void Tag_Close_Button_Keeps_Its_Identity_And_Follows_The_Tag_Close_Icon_Size()
    {
        // The close button is a registered IconButton: it keeps its own identity, and its icon size is read explicitly
        // from the Tag's TagCloseIconSize, so Controls[Tag] reaches it and the IconButton's own icon size does not.
        var tag = new Tag { Text = "Closable", IsClosable = true };
        WithControls([tag], (provider, capture) =>
        {
            var close = tag.GetVisualDescendants().OfType<IconButton>().Single(static b => b.Name == "PART_CloseButton");
            ThemeScope.GetTokenOwner(close).ShouldBe(IconButtonTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (IconButtonTokens.Identity, "IconSizeSM", "24"));
            using var byButton = capture();
            ChangedRatioOf(before, byButton, tag).ShouldBe(0d, "Controls[IconButton].IconSizeSM reached the Tag's close icon.");

            provider.Config = IsolationConfig(control: (TagTokens.Identity, "TagCloseIconSize", "24"));
            using var byTag = capture();
            ChangedRatioOf(before, byTag, close).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[Tag].TagCloseIconSize did not reach the close icon.");
        });
    }

    [Fact]
    public void Group_Tags_Keep_Their_Identity()
    {
        // The group's items are registered CheckableTags: Controls[CheckableTag] reaches them, Controls[CheckableTagGroup]
        // only reaches the group's spacing.
        var group = new CheckableTagGroup { Options = new[] { "Movies", "Books", "Music" } };
        WithControls([group], (provider, capture) =>
        {
            var item = group.GetVisualDescendants().OfType<CheckableTag>().First();
            ThemeScope.GetTokenOwner(item).ShouldBe(CheckableTagTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (CheckableTagGroupTokens.Identity, "ColorText", IsolationColor));
            using var byGroup = capture();
            ChangedRatioOf(before, byGroup, group).ShouldBe(0d, "Controls[CheckableTagGroup].ColorText reached the tags.");

            provider.Config = IsolationConfig(control: (CheckableTagTokens.Identity, "ColorText", IsolationColor));
            using var byTag = capture();
            ChangedRatioOf(before, byTag, item).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[CheckableTag].ColorText did not reach the group's tags.");
        });
    }

    [Fact]
    public void Layer_Indicators_Follow_Their_Badge()
    {
        // Decorated badges render their indicator in the adorner layer, outside the badge's visual subtree; the indicator
        // reads its badge's TokenResource through its logical parent (the badge).
        var count = new CountBadge { Count = 5, IsMotionEnabled = false, DecoratedTarget = new AvaloniaBorder { Width = 48, Height = 48 } };
        var dot   = new DotBadge { IsMotionEnabled = false, DecoratedTarget = new AvaloniaBorder { Width = 48, Height = 48 } };
        WithControls([count, dot], adornerLayer: true, body: (provider, _) =>
        {
            var countIndicator = LayerAdorner<AbstractCountBadgeAdorner>(count);
            var dotIndicator   = LayerAdorner<AbstractDotBadgeAdorner>(dot);
            var countDefault   = BrushColor(countIndicator.BadgeColor);
            var dotDefault     = BrushColor(dotIndicator.BadgeDotColor);

            provider.Config = IsolationConfig(control: (CountBadgeTokens.Identity, "IndicatorColor", IsolationColor));
            WaitFor(() => BrushColor(countIndicator.BadgeColor) == Color.Parse(IsolationColor));
            BrushColor(countIndicator.BadgeColor).ShouldBe(Color.Parse(IsolationColor));
            BrushColor(dotIndicator.BadgeDotColor).ShouldBe(dotDefault, "Controls[CountBadge] reached the DotBadge.");

            provider.Config = IsolationConfig(control: (DotBadgeTokens.Identity, "DotColor", IsolationColor));
            WaitFor(() => BrushColor(dotIndicator.BadgeDotColor) == Color.Parse(IsolationColor));
            BrushColor(dotIndicator.BadgeDotColor).ShouldBe(Color.Parse(IsolationColor));
            BrushColor(countIndicator.BadgeColor).ShouldBe(countDefault, "Controls[DotBadge] reached the CountBadge.");
        });
    }

    [Fact]
    public void Controls_CheckableTag_MotionDurationMid_Reaches_Its_Transitions_Only()
    {
        var checkable = new CheckableTag { Content = "Checkable" };
        var provider  = new ThemeConfigProvider { Config = new ThemeConfigBuilder().Build(), Child = checkable };
        var window    = new AvaloniaWindow { Width = 400, Height = 200, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var baseline = BackgroundDuration(checkable);
            baseline.ShouldBeGreaterThan(TimeSpan.Zero);

            provider.Config = MotionConfig(TagTokens.Identity);
            SettleFor(TimeSpan.FromMilliseconds(200));
            BackgroundDuration(checkable).ShouldBe(baseline, "Controls[Tag] reached the CheckableTag transition.");

            provider.Config = MotionConfig(CheckableTagTokens.Identity);
            WaitFor(() => BackgroundDuration(checkable) != baseline);
            BackgroundDuration(checkable).ShouldBe(TimeSpan.FromMilliseconds(500));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        static ThemeConfig MotionConfig(ControlTokenIdentity identity) =>
            new ThemeConfigBuilder()
                .WithControl(identity, new ControlThemeConfigBuilder()
                                       .WithToken(nameof(SharedTokenKind.MotionDurationMid), "00:00:00.5000000").Build())
                .Build();

        static TimeSpan BackgroundDuration(Animatable control) =>
            control.Transitions.ShouldNotBeNull().OfType<TransitionBase>()
                   .First(static transition => transition.Property == TemplatedControl.BackgroundProperty).Duration;
    }

    // ---- helpers ------------------------------------------------------------------------------

    private static TAdorner LayerAdorner<TAdorner>(Control badge)
        where TAdorner : Control
    {
        var layer = AdornerLayer.GetAdornerLayer(badge).ShouldNotBeNull();
        return layer.Children.OfType<TAdorner>().Single(adorner => ReferenceEquals(AdornerLayer.GetAdornedElement(adorner), badge));
    }

    private static Color BrushColor(IBrush? brush) => brush.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;

    private static ThemeConfig IsolationConfig(
        (ControlTokenIdentity Identity, string Token, string Value)? control = null,
        (string Token, string Value)? root = null)
    {
        var builder = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        if (root is { } rootEntry)
        {
            builder.WithToken(rootEntry.Token, rootEntry.Value);
        }
        if (control is { } entry)
        {
            builder.WithControl(entry.Identity, new ControlThemeConfigBuilder().WithToken(entry.Token, entry.Value).Build());
        }

        return builder.Build();
    }

    private static void WithControls(Control[] controls, Action<ThemeConfigProvider, Func<SKBitmap>> body) =>
        WithControls(controls, false, body);

    private static void WithControls(Control[] controls, bool adornerLayer, Action<ThemeConfigProvider, Func<SKBitmap>> body)
    {
        var panel = new StackPanel
        {
            Orientation         = Orientation.Vertical,
            Spacing             = 16,
            Margin              = new Thickness(24),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment   = VerticalAlignment.Top
        };
        foreach (var control in controls)
        {
            panel.Children.Add(control);
        }
        ThemeChangeFailedEventArgs? failure = null;
        var provider = new ThemeConfigProvider
        {
            Config = IsolationConfig(),
            Child  = adornerLayer ? new VisualLayerManager { EnableAdornerLayer = true, Child = panel } : panel
        };
        provider.ThemeChangeFailed += (_, args) => failure = args;
        var window = new AvaloniaWindow { Width = 800, Height = 600, Content = provider };
        try
        {
            window.Show();
            Settle(window);
            body(provider, () =>
            {
                Settle(window);
                return FrameCapture.Capture(window);
            });
            failure.ShouldBeNull(failure?.Exception?.Message);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        static void Settle(AvaloniaWindow window)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(200))
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Thread.Sleep(5);
            }
        }
    }

    private static void WaitFor(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(5);
        }
    }

    private static void SettleFor(TimeSpan duration)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < duration)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(5);
        }
    }

    private static double ChangedRatioOf(SKBitmap before, SKBitmap after, Control control)
    {
        var window = (Visual)TopLevel.GetTopLevel(control)!;
        var origin = control.TranslatePoint(new Point(), window).ShouldNotBeNull();
        var region = new SKRectI(
            (int)Math.Floor(origin.X),
            (int)Math.Floor(origin.Y),
            (int)Math.Ceiling(origin.X + control.Bounds.Width),
            (int)Math.Ceiling(origin.Y + control.Bounds.Height));
        var changed = FrameCapture.CountDifferentPixels(before, after, region, TokenEffectHarness.DefaultChannelTolerance);
        return (double)changed / (region.Width * region.Height);
    }
}
