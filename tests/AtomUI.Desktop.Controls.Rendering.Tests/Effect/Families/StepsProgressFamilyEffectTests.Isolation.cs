using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.DesignTokens;
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
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the steps / progress family (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override
/// reaches X only (each progress control computes its own Own Tokens from its own effective globals), the root Token
/// reaches every member, and the registered status icon presenter in a progress template keeps its own identity while the
/// host's override reaches its brush.
/// </summary>
public partial class StepsProgressFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    [Theory]
    [InlineData("ColorInfo")]
    [InlineData("DefaultColor")]
    public void Controls_X_Progress_Colour_Changes_Only_X(string token)
    {
        var line      = new ProgressBar { Width = 240, Value = 50 };
        var steps     = new StepsProgressBar { Steps = 5, Value = 60 };
        var circle    = new CircleProgress { Width = 80, Value = 50 };
        var dashboard = new DashboardProgress { Width = 80, Value = 50 };
        Control[] all = [line, steps, circle, dashboard];
        var cases = new (ControlTokenIdentity Identity, Control Target)[]
        {
            (ProgressBarTokens.Identity, line), (StepsProgressBarTokens.Identity, steps),
            (CircleProgressTokens.Identity, circle), (DashboardProgressTokens.Identity, dashboard)
        };

        WithControls(all, (provider, capture) =>
        {
            foreach (var (identity, target) in cases)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, token));
                using var after = capture();

                foreach (var control in all)
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
    public void Root_ColorInfo_Changes_Every_Progress_Control()
    {
        var line      = new ProgressBar { Width = 240, Value = 50 };
        var steps     = new StepsProgressBar { Steps = 5, Value = 60 };
        var circle    = new CircleProgress { Width = 80, Value = 50 };
        var dashboard = new DashboardProgress { Width = 80, Value = 50 };
        Control[] all = [line, steps, circle, dashboard];
        WithControls(all, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = IsolationConfig(root: "ColorInfo");
            using var after = capture();

            foreach (var control in all)
            {
                ChangedRatioOf(before, after, control).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                    $"Root ColorInfo did not reach {control.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void Status_Icon_Follows_Controls_ProgressBar_Not_Controls_IconPresenter()
    {
        // The status icon presenter is a registered control with its own identity; its brush is resolved on the progress
        // control (an internal brush the theme fills from the host's Tokens) and bound down.
        var line = new ProgressBar { Width = 240, Value = 100 };
        WithControls([line], (provider, capture) =>
        {
            var icon = line.GetVisualDescendants().OfType<IconPresenter>()
                           .First(static p => p.Name == "PART_SuccessCompletedIconPresenter");
            icon.IsEffectivelyVisible.ShouldBeTrue();
            ThemeScope.GetTokenOwner(icon).ShouldBe(IconPresenterTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (IconPresenterTokens.Identity, "ColorSuccess"));
            using var byPart = capture();
            ChangedRatioOf(before, byPart, line).ShouldBe(0d, "Controls[IconPresenter].ColorSuccess reached the progress icon.");

            provider.Config = IsolationConfig(control: (ProgressBarTokens.Identity, "ColorSuccess"));
            using var byHost = capture();
            ChangedRatioOf(before, byHost, icon).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[ProgressBar].ColorSuccess did not reach the status icon.");
        });
    }

    [Fact]
    public void Controls_ProgressBar_MotionDurationVerySlow_Reaches_Its_Value_Transition_Only()
    {
        // The shared base theme declares the Value transition with a Token duration; it resolves per receiving control.
        var line     = new ProgressBar { Width = 240, Value = 50 };
        var circle   = new CircleProgress { Width = 80, Value = 50 };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().Build(),
            Child  = new StackPanel { Spacing = 24, Children = { line, circle } }
        };
        var window = new AvaloniaWindow { Width = 400, Height = 300, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var lineBaseline   = ValueDuration(line);
            var circleBaseline = ValueDuration(circle);
            lineBaseline.ShouldBeGreaterThan(TimeSpan.Zero);

            provider.Config = new ThemeConfigBuilder()
                              .WithControl(ProgressBarTokens.Identity, new ControlThemeConfigBuilder()
                                                                       .WithToken(nameof(SharedTokenKind.MotionDurationVerySlow),
                                                                           "00:00:01.2500000").Build())
                              .Build();
            var stopwatch = Stopwatch.StartNew();
            while (ValueDuration(line) == lineBaseline && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(5);
            }

            ValueDuration(line).ShouldBe(TimeSpan.FromMilliseconds(1250));
            ValueDuration(circle).ShouldBe(circleBaseline, "Controls[ProgressBar] reached the CircleProgress.");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        static TimeSpan ValueDuration(Animatable control) =>
            control.Transitions.ShouldNotBeNull().OfType<TransitionBase>()
                   .First(static transition => transition.Property == RangeBase.ValueProperty).Duration;
    }

    // ---- helpers ------------------------------------------------------------------------------

    private static ThemeConfig IsolationConfig((ControlTokenIdentity Identity, string Token)? control = null, string? root = null)
    {
        var builder = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        if (root is not null)
        {
            builder.WithToken(root, IsolationColor);
        }
        if (control is { } entry)
        {
            builder.WithControl(entry.Identity, new ControlThemeConfigBuilder().WithToken(entry.Token, IsolationColor).Build());
        }

        return builder.Build();
    }

    private static void WithControls(Control[] controls, Action<ThemeConfigProvider, Func<SKBitmap>> body)
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
        var provider = new ThemeConfigProvider { Config = IsolationConfig(), Child = panel };
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
