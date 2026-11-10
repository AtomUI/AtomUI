using System.Diagnostics;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
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
/// Scope isolation of the Button family (theme-effect-tests.md, family gate): a <c>Controls[Button]</c> override changes
/// Buttons only, the root Token changes every consumer, and a Button nested in another control's template keeps its
/// own identity.
/// </summary>
public partial class ButtonFamilyEffectTests
{
    private const string OverrideColor = "#ff00b96b";

    [Fact]
    public void Controls_Button_ColorPrimary_Changes_The_Button_But_Not_A_Tag_In_The_Same_Window()
    {
        var button = new AtomButton { Content = "Button", ButtonType = ButtonType.Primary };
        var tag    = new CheckableTag { Content = "Tag", IsChecked = true };
        WithPair(button, tag, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = Config(control: (ButtonTokens.Identity, "ColorPrimary"));
            using var after = capture();

            ChangedRatio(before, after, button).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio);
            ChangedRatio(before, after, tag).ShouldBe(0d);
        });
    }

    [Fact]
    public void Root_ColorPrimary_Changes_Both_The_Button_And_The_Tag()
    {
        var button = new AtomButton { Content = "Button", ButtonType = ButtonType.Primary };
        var tag    = new CheckableTag { Content = "Tag", IsChecked = true };
        WithPair(button, tag, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = Config(root: "ColorPrimary");
            using var after = capture();

            ChangedRatio(before, after, button).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio);
            ChangedRatio(before, after, tag).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio);
        });
    }

    [Fact]
    public void Button_Nested_In_SearchEdit_Ignores_LineEdit_And_SearchEdit_Overrides_And_Follows_Button()
    {
        var search = new SearchEdit { SearchButtonStyle = SearchButtonType.Primary, Width = 240 };
        WithPair(search, new Border(), (provider, capture) =>
        {
            var searchButton = search.GetVisualDescendants().OfType<AtomButton>().First();
            ThemeScope.GetTokenOwner(searchButton).ShouldBe(ButtonTokens.Identity);
            var baseline     = BrushColor(searchButton.Background);
            baseline.ShouldNotBe(Color.Parse(OverrideColor));

            provider.Config = Config(control: (LineEditTokens.Identity, "ColorPrimary"));
            capture().Dispose();
            BrushColor(searchButton.Background).ShouldBe(baseline);

            provider.Config = Config(control: (SearchEditTokens.Identity, "ColorPrimary"));
            capture().Dispose();
            BrushColor(searchButton.Background).ShouldBe(baseline);

            provider.Config = Config(control: (ButtonTokens.Identity, "ColorPrimary"));
            capture().Dispose();
            BrushColor(searchButton.Background).ShouldBe(Color.Parse(OverrideColor));
        });
    }

    private static ThemeConfig Config(
        (Theme.Schema.ControlTokenIdentity Identity, string Token)? control = null,
        string? root = null)
    {
        var builder = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        if (root is not null)
        {
            builder.WithToken(root, OverrideColor);
        }
        if (control is { } entry)
        {
            builder.WithControl(entry.Identity, new ControlThemeConfigBuilder().WithToken(entry.Token, OverrideColor).Build());
        }

        return builder.Build();
    }

    private static void WithPair(Control first, Control second, Action<ThemeConfigProvider, Func<SKBitmap>> body)
    {
        var provider = new ThemeConfigProvider
        {
            Config = Config(),
            Child = new StackPanel
            {
                Orientation         = Orientation.Horizontal,
                Spacing             = 24,
                Margin              = new Thickness(24),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Top,
                Children            = { first, second }
            }
        };
        var window = new AvaloniaWindow { Width = 600, Height = 300, Content = provider };
        try
        {
            window.Show();
            Settle(window);
            body(provider, () =>
            {
                Settle(window);
                return FrameCapture.Capture(window);
            });
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

    private static double ChangedRatio(SKBitmap before, SKBitmap after, Control control)
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

    private static Color? BrushColor(IBrush? brush) => (brush as ISolidColorBrush)?.Color;
}
