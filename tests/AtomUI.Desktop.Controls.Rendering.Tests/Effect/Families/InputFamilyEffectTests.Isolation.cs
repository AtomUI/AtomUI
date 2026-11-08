using System.Diagnostics;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the input family (theme-effect-tests.md, family gate). The input surface is drawn by shared,
/// identity-less primitives (InputControlFrame, AddOnDecoratedBox and their derived boxes) that read the hosting
/// control's effective globals, so a <c>Controls[X]</c> override must reach X's surface only; the root Token reaches
/// every surface; and registered controls composed into an input (SearchEdit's Button, AutoComplete's LineEdit,
/// NumericUpDown's spinner and editor) keep the identity the composition gives them.
/// </summary>
public partial class InputFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    [Fact]
    public void Controls_X_ColorBorder_Changes_Only_The_Surface_Of_X()
    {
        var textBox   = new TextBox { Width = 120 };
        var lineEdit  = new LineEdit { Width = 120 };
        var search    = new SearchEdit { Width = 160 };
        var textArea  = new TextArea { Width = 120 };
        var spinner   = new ButtonSpinner { Width = 120, Content = "1" };
        var numeric   = new NumericUpDown { Width = 120, Value = 1 };
        Control[] all = [textBox, lineEdit, search, textArea, spinner, numeric];
        var cases = new (ControlTokenIdentity Identity, Control Target)[]
        {
            (TextBoxTokens.Identity, textBox), (LineEditTokens.Identity, lineEdit), (SearchEditTokens.Identity, search),
            (TextAreaTokens.Identity, textArea), (ButtonSpinnerTokens.Identity, spinner),
            (NumericUpDownTokens.Identity, numeric)
        };

        WithControls(all, (provider, capture) =>
        {
            foreach (var (identity, target) in cases)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, "ColorBorder"));
                using var after = capture();

                foreach (var control in all)
                {
                    var ratio = ChangedRatioOf(before, after, control);
                    if (ReferenceEquals(control, target))
                    {
                        ratio.ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                            $"Controls[{identity}].ColorBorder did not reach {control.GetType().Name}.");
                    }
                    else
                    {
                        ratio.ShouldBe(0d, $"Controls[{identity}].ColorBorder leaked into {control.GetType().Name}.");
                    }
                }
            }
        });
    }

    [Fact]
    public void Root_ColorBorder_Changes_Every_Input_Surface()
    {
        Control[] all =
        [
            new TextBox { Width = 120 }, new LineEdit { Width = 120 }, new SearchEdit { Width = 160 },
            new TextArea { Width = 120 }, new ButtonSpinner { Width = 120, Content = "1" },
            new NumericUpDown { Width = 120, Value = 1 }
        ];
        WithControls(all, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = IsolationConfig(root: "ColorBorder");
            using var after = capture();
            foreach (var control in all)
            {
                ChangedRatioOf(before, after, control).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Root ColorBorder did not reach {control.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void NumericUpDown_Spinner_And_Editor_Follow_NumericUpDown_Not_ButtonSpinner_Or_TextBox()
    {
        // NumericUpDown's spinner theme is BasedOn the ButtonSpinner default theme and its editor theme on TextBox's:
        // neither may carry the ButtonSpinner / TextBox identity into the NumericUpDown.
        var numeric = new NumericUpDown { Width = 140, Value = 123 };
        WithControls([numeric], (provider, capture) =>
        {
            var editor = numeric.GetVisualDescendants().OfType<EmbeddedTextBox>().First();
            ThemeScope.GetTokenOwner(numeric.GetVisualDescendants().OfType<NumericUpDownSpinner>().First())
                      .ShouldBe(NumericUpDownTokens.Identity);
            ThemeScope.GetTokenOwner(editor).ShouldBe(NumericUpDownTokens.Identity);

            using var baseline = capture();
            foreach (var (identity, token) in new[]
                     {
                         (ButtonSpinnerTokens.Identity, "ColorBorder"), (TextBoxTokens.Identity, "ColorText"),
                         (TextBoxTokens.Identity, "FontSize")
                     })
            {
                provider.Config = IsolationConfig(control: (identity, token), value: token == "FontSize" ? "28" : null);
                using var after = capture();
                ChangedRatioOf(baseline, after, numeric).ShouldBe(0d, $"Controls[{identity}].{token} reached NumericUpDown.");
            }

            provider.Config = IsolationConfig(control: (NumericUpDownTokens.Identity, "ColorText"));
            using var own = capture();
            ChangedRatioOf(baseline, own, editor).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio);
        });
    }

    [Fact]
    public void SearchEdit_Button_Ignores_SearchEdit_And_LineEdit_Overrides_And_The_Frame_Ignores_Button()
    {
        var search = new SearchEdit { SearchButtonStyle = SearchButtonType.Primary, Width = 260 };
        WithControls([search], (provider, capture) =>
        {
            var button = search.GetVisualDescendants().OfType<AtomButton>().First();
            // The input part of the frame (it shares its right border column with the button, which is excluded).
            var frame  = search.GetVisualDescendants().OfType<AddOnDecoratedBoxContentFrame>().First();
            using var baseline = capture();

            foreach (var identity in new[] { SearchEditTokens.Identity, LineEditTokens.Identity })
            {
                foreach (var token in new[] { "ColorPrimary", "ColorText", "ColorTextLightSolid" })
                {
                    provider.Config = IsolationConfig(control: (identity, token));
                    using var after = capture();
                    ChangedRatioOf(baseline, after, button).ShouldBe(0d,
                        $"Controls[{identity}].{token} reached the nested search Button.");
                }
            }

            // The joined corner is part of the SearchEdit surface (guideline: SearchEdit owns the joined border and
            // radius), so the button's outer corner follows the SearchEdit radius.
            provider.Config = IsolationConfig(control: (SearchEditTokens.Identity, "BorderRadius"), value: "12");
            using var radius = capture();
            ChangedRatioOf(baseline, radius, button).ShouldBeGreaterThan(0d);

            provider.Config = IsolationConfig(control: (ButtonTokens.Identity, "ColorPrimary"));
            using var buttonOverride = capture();
            ChangedRatioOf(baseline, buttonOverride, button).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio);
            ChangedRatioOf(baseline, buttonOverride, frame, clipRightAt: button).ShouldBe(0d,
                "Controls[Button].ColorPrimary reached the SearchEdit frame.");

            provider.Config = IsolationConfig(control: (ButtonTokens.Identity, "ColorBorder"));
            using var buttonBorder = capture();
            ChangedRatioOf(baseline, buttonBorder, frame, clipRightAt: button).ShouldBe(0d,
                "Controls[Button].ColorBorder reached the SearchEdit frame.");
        });
    }

    [Fact]
    public void AutoComplete_Input_Is_A_Nested_LineEdit_With_Its_Own_Identity()
    {
        // AutoComplete composes a registered LineEdit (by StyleKey); the input surface follows Controls[LineEdit].
        var autoComplete = new AutoComplete { Width = 200 };
        WithControls([autoComplete], (provider, capture) =>
        {
            var input = autoComplete.GetVisualDescendants().OfType<LineEdit>().First();
            ThemeScope.GetTokenOwner(input).ShouldBe(LineEditTokens.Identity);
            using var baseline = capture();

            provider.Config = IsolationConfig(control: (AutoCompleteTokens.Identity, "ColorBorder"));
            using var own = capture();
            ChangedRatioOf(baseline, own, input).ShouldBe(0d);

            provider.Config = IsolationConfig(control: (LineEditTokens.Identity, "ColorBorder"));
            using var nested = capture();
            ChangedRatioOf(baseline, nested, input).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio);
        });
    }

    private static ThemeConfig IsolationConfig(
        (ControlTokenIdentity Identity, string Token)? control = null,
        string? root = null,
        string? value = null)
    {
        var builder = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        if (root is not null)
        {
            builder.WithToken(root, value ?? IsolationColor);
        }
        if (control is { } entry)
        {
            builder.WithControl(entry.Identity,
                new ControlThemeConfigBuilder().WithToken(entry.Token, value ?? IsolationColor).Build());
        }

        return builder.Build();
    }

    private static void WithControls(Control[] controls, Action<ThemeConfigProvider, Func<SKBitmap>> body)
    {
        var panel = new WrapPanel
        {
            Orientation         = Orientation.Horizontal,
            ItemSpacing         = 24,
            LineSpacing         = 24,
            Margin              = new Thickness(24),
            Width               = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment   = VerticalAlignment.Top
        };
        foreach (var control in controls)
        {
            control.VerticalAlignment = VerticalAlignment.Top;
            panel.Children.Add(control);
        }

        var provider = new ThemeConfigProvider { Config = IsolationConfig(), Child = panel };
        var window   = new AvaloniaWindow { Width = 800, Height = 400, Content = provider };
        try
        {
            window.Show();
            SettleWindow(window);
            body(provider, () =>
            {
                SettleWindow(window);
                return FrameCapture.Capture(window);
            });
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void SettleWindow(AvaloniaWindow window)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(200))
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Thread.Sleep(5);
        }
        FrameCapture.Refresh(window);
    }

    private static double ChangedRatioOf(SKBitmap before, SKBitmap after, Control control, Control? clipRightAt = null)
    {
        var window = (Visual)TopLevel.GetTopLevel(control)!;
        var origin = control.TranslatePoint(new Point(), window).ShouldNotBeNull();
        var right  = origin.X + control.Bounds.Width;
        if (clipRightAt?.TranslatePoint(new Point(), window) is { } clip)
        {
            right = Math.Min(right, clip.X);
        }
        var region = new SKRectI(
            (int)Math.Floor(origin.X),
            (int)Math.Floor(origin.Y),
            (int)Math.Floor(right),
            (int)Math.Ceiling(origin.Y + control.Bounds.Height));
        var changed = FrameCapture.CountDifferentPixels(before, after, region, TokenEffectHarness.DefaultChannelTolerance);
        return (double)changed / (region.Width * region.Height);
    }
}
