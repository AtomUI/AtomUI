using System.Diagnostics;
using AtomUI.Controls;
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
using AtomCheckBox = AtomUI.Desktop.Controls.CheckBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the choice family (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override reaches X only,
/// the root Token reaches every member, the CheckBox / RadioButton items of a group keep their own identity, and the
/// identity-less indicator parts follow their host (their reads and Token-declared transitions resolve to the host).
/// </summary>
public partial class ChoiceFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    [Fact]
    public void Controls_X_ColorPrimary_Changes_Only_X()
    {
        var checkBox = new AtomCheckBox { Content = "CheckBox", IsChecked = true };
        var radio    = new RadioButton { Content = "Radio", IsChecked = true };
        var toggle   = new ToggleSwitch { IsChecked = true };
        Control[] all = [checkBox, radio, toggle];
        var cases = new (ControlTokenIdentity Identity, Control Target)[]
        {
            (CheckBoxTokens.Identity, checkBox), (RadioButtonTokens.Identity, radio), (ToggleSwitchTokens.Identity, toggle)
        };

        WithControls(all, (provider, capture) =>
        {
            foreach (var (identity, target) in cases)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, "ColorPrimary"));
                using var after = capture();

                foreach (var control in all)
                {
                    var ratio = ChangedRatioOf(before, after, control);
                    if (ReferenceEquals(control, target))
                    {
                        ratio.ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                            $"Controls[{identity}].ColorPrimary did not reach {control.GetType().Name}.");
                    }
                    else
                    {
                        ratio.ShouldBe(0d, $"Controls[{identity}].ColorPrimary leaked into {control.GetType().Name}.");
                    }
                }
            }
        });
    }

    [Fact]
    public void Root_ColorPrimary_Changes_Every_Member()
    {
        var checkBox = new AtomCheckBox { Content = "CheckBox", IsChecked = true };
        var radio    = new RadioButton { Content = "Radio", IsChecked = true };
        var toggle   = new ToggleSwitch { IsChecked = true };
        Control[] all = [checkBox, radio, toggle];
        WithControls(all, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = IsolationConfig(root: "ColorPrimary");
            using var after = capture();

            foreach (var control in all)
            {
                ChangedRatioOf(before, after, control).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                    $"Root ColorPrimary did not reach {control.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void CheckBox_Items_Of_A_Group_Keep_The_CheckBox_Identity()
    {
        var group = new CheckBoxGroup
        {
            ItemsSource = new[]
            {
                new CheckBoxOption { Content = "Apple", IsChecked = true },
                new CheckBoxOption { Content = "Pear", IsChecked = true }
            }
        };
        AssertItemsKeepTheirIdentity(group, CheckBoxGroupTokens.Identity, CheckBoxTokens.Identity);
    }

    [Fact]
    public void RadioButton_Items_Of_A_Group_Keep_The_RadioButton_Identity()
    {
        var group = new RadioButtonGroup
        {
            ItemsSource = new[]
            {
                new RadioButtonOption { Content = "Apple", IsChecked = true },
                new RadioButtonOption { Content = "Pear" }
            }
        };
        AssertItemsKeepTheirIdentity(group, RadioButtonGroupTokens.Identity, RadioButtonTokens.Identity);
    }

    [Fact]
    public void Slider_Thumb_Follows_Controls_Slider_Not_Controls_SliderThumb()
    {
        // SliderThumb is registered but Token-owner transparent: it inherits the Slider as owner, and its theme reads the
        // Slider's TokenResource, so the host's override reaches it and the thumb's own identity does not.
        var slider = new Slider { Width = 240, Value = 50 };
        WithControls([slider], (provider, capture) =>
        {
            var thumb = slider.GetVisualDescendants().OfType<SliderThumb>().First();
            ThemeScope.GetTokenOwner(thumb).ShouldBe(SliderTokens.Identity);
            ThemeScope.GetTokenOwner(slider.GetVisualDescendants().OfType<SliderTrack>().First())
                      .ShouldBe(SliderTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (SliderThumbTokens.Identity, "ColorBgContainer"));
            using var part = capture();
            ChangedRatioOf(before, part, thumb).ShouldBe(0d, "Controls[SliderThumb].ColorBgContainer reached the thumb.");

            provider.Config = IsolationConfig(control: (SliderTokens.Identity, "ColorBgContainer"));
            using var host = capture();
            ChangedRatioOf(before, host, thumb).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[Slider].ColorBgContainer did not reach the thumb.");
        });
    }

    [Fact]
    public void Controls_CheckBox_Overrides_Reach_The_Indicator_Metrics()
    {
        // The PrimeNG presets size the indicator through <Control Id="CheckBox">: box 20, radius 4, check mark 14.
        var checkBox = new AtomCheckBox { Content = "CheckBox", IsChecked = true };
        var radio    = new RadioButton { Content = "Radio", IsChecked = true };
        WithControls([checkBox, radio], (provider, capture) =>
        {
            var indicator = checkBox.GetVisualDescendants().OfType<CheckBoxIndicator>().First();
            var mark      = indicator.GetVisualDescendants().OfType<Icon>().First(static icon => icon.Name == "CheckedMark");
            var dot       = radio.GetVisualDescendants().OfType<Control>().First(static c => c.Name == "Indicator");
            var radioSize = dot.Bounds.Size;
            indicator.Bounds.Width.ShouldNotBe(20d);

            provider.Config = new ThemeConfigBuilder()
                              .WithToken(nameof(SharedTokenKind.EnableMotion), "false")
                              .WithControl(CheckBoxTokens.Identity, new ControlThemeConfigBuilder()
                                                                    .WithToken("CheckIndicatorSize", "20")
                                                                    .WithToken("CheckedMarkSize", "14")
                                                                    .WithToken(nameof(SharedTokenKind.BorderRadiusSM), "4")
                                                                    .Build())
                              .Build();
            capture().Dispose();

            indicator.Bounds.Size.ShouldBe(new Size(20, 20));
            indicator.CornerRadius.ShouldBe(new CornerRadius(4));
            mark.Bounds.Size.ShouldBe(new Size(14, 14));
            dot.Bounds.Size.ShouldBe(radioSize, "Controls[CheckBox] resized the RadioButton indicator.");
        });
    }

    [Fact]
    public void Controls_CheckBox_MotionDurationMid_Reaches_The_Indicator_Transitions_But_Not_The_RadioButton()
    {
        // The indicators are identity-less: their atom:TokenTransitions resolve through the inherited owner, the host.
        var checkBox = new AtomCheckBox { Content = "CheckBox" };
        var radio    = new RadioButton { Content = "Radio" };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().Build(),
            Child  = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 24, Children = { checkBox, radio } }
        };
        var window = new AvaloniaWindow { Width = 400, Height = 200, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var checkIndicator = checkBox.GetVisualDescendants().OfType<CheckBoxIndicator>().First();
            var radioIndicator = radio.GetVisualDescendants().OfType<TemplatedControl>().First(static c => c.Name == "Indicator");
            ThemeScope.GetTokenOwner(checkIndicator).ShouldBe(CheckBoxTokens.Identity);
            var baseline = BackgroundDuration(checkIndicator);
            baseline.ShouldBeGreaterThan(TimeSpan.Zero);
            var radioBaseline = BackgroundDuration(radioIndicator);

            provider.Config = new ThemeConfigBuilder()
                              .WithControl(CheckBoxTokens.Identity, new ControlThemeConfigBuilder()
                                                                    .WithToken(nameof(SharedTokenKind.MotionDurationMid),
                                                                        "00:00:00.5000000").Build())
                              .Build();
            var stopwatch = Stopwatch.StartNew();
            while (BackgroundDuration(checkIndicator) == baseline && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(5);
            }

            BackgroundDuration(checkIndicator).ShouldBe(TimeSpan.FromMilliseconds(500));
            BackgroundDuration(radioIndicator).ShouldBe(radioBaseline, "Controls[CheckBox] reached the RadioButton.");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        static TimeSpan BackgroundDuration(Animatable control) =>
            control.Transitions.ShouldNotBeNull().OfType<TransitionBase>()
                   .First(static transition => transition.Property == TemplatedControl.BackgroundProperty).Duration;
    }

    [Fact]
    public void Controls_Slider_MotionDurationMid_Reaches_The_Track_And_Thumb_Transitions()
    {
        // The track and thumb are Token-owner transparent, so their atom:TokenTransitions resolve to the Slider.
        var slider   = new Slider { Width = 240, Value = 50 };
        var provider = new ThemeConfigProvider { Config = new ThemeConfigBuilder().Build(), Child = slider };
        var window   = new AvaloniaWindow { Width = 400, Height = 200, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var track = slider.GetVisualDescendants().OfType<SliderTrack>().First();
            var thumb = slider.GetVisualDescendants().OfType<SliderThumb>().First();
            var trackBaseline = DurationOf(track, SliderTrack.TrackGrooveBrushProperty);
            var thumbBaseline = DurationOf(thumb, TemplatedControl.BorderBrushProperty);
            trackBaseline.ShouldBeGreaterThan(TimeSpan.Zero);
            thumbBaseline.ShouldBeGreaterThan(TimeSpan.Zero);

            foreach (var part in new[] { SliderTrackTokens.Identity, SliderThumbTokens.Identity })
            {
                provider.Config = MotionDurationConfig(part);
                SettleFor(TimeSpan.FromMilliseconds(200));
                DurationOf(track, SliderTrack.TrackGrooveBrushProperty).ShouldBe(trackBaseline, $"Controls[{part}] reached the track.");
                DurationOf(thumb, TemplatedControl.BorderBrushProperty).ShouldBe(thumbBaseline, $"Controls[{part}] reached the thumb.");
            }

            provider.Config = MotionDurationConfig(SliderTokens.Identity);
            var stopwatch = Stopwatch.StartNew();
            while ((DurationOf(track, SliderTrack.TrackGrooveBrushProperty) == trackBaseline ||
                    DurationOf(thumb, TemplatedControl.BorderBrushProperty) == thumbBaseline) &&
                   stopwatch.Elapsed < TimeSpan.FromSeconds(10))
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(5);
            }

            DurationOf(track, SliderTrack.TrackGrooveBrushProperty).ShouldBe(TimeSpan.FromMilliseconds(500));
            DurationOf(thumb, TemplatedControl.BorderBrushProperty).ShouldBe(TimeSpan.FromMilliseconds(500));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        static ThemeConfig MotionDurationConfig(ControlTokenIdentity identity) =>
            new ThemeConfigBuilder()
                .WithControl(identity, new ControlThemeConfigBuilder()
                                       .WithToken(nameof(SharedTokenKind.MotionDurationMid), "00:00:00.5000000").Build())
                .Build();

        static void SettleFor(TimeSpan duration)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < duration)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(5);
            }
        }

        static TimeSpan DurationOf(Animatable control, AvaloniaProperty property) =>
            control.Transitions.ShouldNotBeNull().OfType<TransitionBase>()
                   .First(transition => transition.Property == property).Duration;
    }

    [Fact]
    public void Disabled_ToggleSwitch_Is_Dimmed_Once_As_A_Whole()
    {
        // antd dims a disabled switch once, on the root (opacity: switchDisabledOpacity). The disabled switch must render
        // exactly like an enabled switch whose only change is that root opacity: a knob or groove dimmed on its own as
        // well would differ from it.
        var disabled  = new ToggleSwitch { IsChecked = true, IsEnabled = false };
        var reference = new ToggleSwitch { IsChecked = true };
        var page = new Border
        {
            Background = Avalonia.Media.Brushes.White,
            Padding    = new Thickness(24),
            Child      = new StackPanel { Spacing = 24, Children = { disabled, reference } }
        };
        WithControls([page], (_, capture) =>
        {
            var opacity = ControlTokenAccessor.Capture(disabled).GetOwn<double>(ToggleSwitchTokenKind.SwitchDisabledOpacity);
            opacity.ShouldBeLessThan(1d);
            disabled.Opacity.ShouldBe(opacity);
            disabled.GetVisualDescendants().OfType<SwitchKnob>().First().Opacity.ShouldBe(1d);
            reference.Opacity = opacity;

            using var frame  = capture();
            var       window = (Visual)TopLevel.GetTopLevel(disabled)!;
            var       a      = disabled.TranslatePoint(new Point(), window).ShouldNotBeNull();
            var       b      = reference.TranslatePoint(new Point(), window).ShouldNotBeNull();
            disabled.Bounds.Size.ShouldBe(reference.Bounds.Size);
            var differing = 0;
            for (var y = 0; y < (int)disabled.Bounds.Height; y++)
            {
                for (var x = 0; x < (int)disabled.Bounds.Width; x++)
                {
                    var left  = frame.GetPixel((int)a.X + x, (int)a.Y + y);
                    var right = frame.GetPixel((int)b.X + x, (int)b.Y + y);
                    if (Math.Abs(left.Red - right.Red) > TokenEffectHarness.DefaultChannelTolerance ||
                        Math.Abs(left.Green - right.Green) > TokenEffectHarness.DefaultChannelTolerance ||
                        Math.Abs(left.Blue - right.Blue) > TokenEffectHarness.DefaultChannelTolerance)
                    {
                        differing++;
                    }
                }
            }

            differing.ShouldBe(0, "The disabled switch is not the root-opacity result of the enabled switch.");
            // And the dimming is visible: the reference without root opacity differs.
            reference.Opacity = 1;
            using var undimmed = capture();
            var knob   = reference.GetVisualDescendants().OfType<SwitchKnob>().First();
            var centre = knob.TranslatePoint(new Point(4, knob.Bounds.Height / 2), window).ShouldNotBeNull();
            undimmed.GetPixel((int)centre.X - 8, (int)centre.Y)
                    .ShouldNotBe(frame.GetPixel((int)centre.X - 8, (int)centre.Y));
        });
    }

    // ---- helpers ------------------------------------------------------------------------------

    /// <summary>
    /// The group override does not reach its items (registered controls with their own identity); the item identity's
    /// override does.
    /// </summary>
    private static void AssertItemsKeepTheirIdentity(
        Control group,
        ControlTokenIdentity groupIdentity,
        ControlTokenIdentity itemIdentity)
    {
        WithControls([group], (provider, capture) =>
        {
            var item = group.GetVisualDescendants().OfType<ToggleButton>().First(static b => b.IsChecked == true);
            ThemeScope.GetTokenOwner(item).ShouldBe(itemIdentity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (groupIdentity, "ColorPrimary"));
            using var byGroup = capture();
            ChangedRatioOf(before, byGroup, item).ShouldBe(0d, $"Controls[{groupIdentity}].ColorPrimary reached its item.");

            provider.Config = IsolationConfig(control: (itemIdentity, "ColorPrimary"));
            using var byItem = capture();
            ChangedRatioOf(before, byItem, item).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                $"Controls[{itemIdentity}].ColorPrimary did not reach the item.");
        });
    }

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
        var window = new AvaloniaWindow { Width = 600, Height = 400, Content = provider };
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
