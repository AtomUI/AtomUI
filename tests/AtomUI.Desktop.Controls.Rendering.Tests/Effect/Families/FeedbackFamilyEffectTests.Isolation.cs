using System.Diagnostics;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
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
/// Scope isolation of the feedback family (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override reaches X
/// only, the root Token reaches X, registered controls nested in a member keep their own identity, and the Token-owner
/// transparent Popup lets its host's owner reach the popup host's text defaults.
/// </summary>
public partial class FeedbackFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    [Fact]
    public void Controls_Alert_And_ToolTip_Main_Colours_Change_Only_Their_Member()
    {
        var alert = new Alert { Message = "Info", Type = AlertType.Info, Width = 240 };
        var root  = CreateToolTipHost("Prompt text", sibling: alert);
        WithIsolationWindow(root, (provider, capture) =>
        {
            var tip = OpenedToolTip(root);
            var members = new (ControlTokenIdentity Identity, string Token, Control Control)[]
            {
                (AlertTokens.Identity, "ColorInfoBg", alert),
                (ToolTipTokens.Identity, "ColorBgSpotlight", tip)
            };
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

                provider.Config = IsolationConfig();
                using var rootBefore = capture();
                provider.Config = IsolationConfig(root: (token, IsolationColor));
                using var rootAfter = capture();
                ChangedRatioOf(rootBefore, rootAfter, target).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                    $"Root {token} did not reach {target.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void Alert_Close_Button_Keeps_Its_Identity_And_Follows_The_Alert_Close_Icon_Size()
    {
        var alert = new Alert { Message = "Closable", IsClosable = true, Width = 240 };
        WithIsolationWindow(alert, (provider, capture) =>
        {
            var close = alert.GetVisualDescendants().OfType<IconButton>().Single(static b => b.Name == "PART_CloseBtn");
            ThemeScope.GetTokenOwner(close).ShouldBe(IconButtonTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (IconButtonTokens.Identity, "IconSizeSM", "24"));
            using var byButton = capture();
            ChangedRatioOf(before, byButton, alert).ShouldBe(0d, "Controls[IconButton].IconSizeSM reached the Alert's close icon.");

            provider.Config = IsolationConfig(control: (AlertTokens.Identity, "CloseIconSize", "24"));
            using var byAlert = capture();
            ChangedRatioOf(before, byAlert, close).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[Alert].CloseIconSize did not reach the close icon.");
        });
    }

    [Fact]
    public void ToolTip_Arrow_Box_Keeps_Its_Identity_And_Takes_The_ToolTip_Frame()
    {
        // The arrow decorated box is a registered part: its own arrow Tokens follow Controls[ArrowDecoratedBox], while the
        // frame values the ToolTip sets on it (background, radius, padding) follow Controls[ToolTip].
        var root = CreateToolTipHost("Prompt text");
        WithIsolationWindow(root, (provider, capture) =>
        {
            var tip = OpenedToolTip(root);
            var box = tip.GetVisualDescendants().OfType<ArrowDecoratedBox>().Single();
            ThemeScope.GetTokenOwner(box).ShouldBe(ArrowDecoratedBoxTokens.Identity);
            ThemeScope.GetTokenOwner(tip).ShouldBe(ToolTipTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (ArrowDecoratedBoxTokens.Identity, "ColorBgSpotlight", IsolationColor));
            using var byBox = capture();
            ChangedRatioOf(before, byBox, tip).ShouldBe(0d, "Controls[ArrowDecoratedBox].ColorBgSpotlight reached the tooltip fill.");

            provider.Config = IsolationConfig(control: (ToolTipTokens.Identity, "ToolTipBackground", IsolationColor));
            using var byTip = capture();
            ChangedRatioOf(before, byTip, box).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[ToolTip].ToolTipBackground did not reach the arrow decorated box.");
        });
    }

    [Fact]
    public void Popup_Host_Text_Defaults_Follow_The_Owner_Of_The_Popup()
    {
        // Popup is Token-owner transparent: the overlay popup host of a ComboBox drop-down inherits the ComboBox's owner, so
        // its text defaults follow Controls[ComboBox], not Controls[Popup].
        var comboBox = new ComboBox { Width = 200, IsPopupPinnedOpen = true };
        comboBox.Items.Add(new ComboBoxItem { Content = "Item" });
        var root = OverlayLayerHost(comboBox);
        WithIsolationWindow(root, (provider, _) =>
        {
            WaitForVisual(() => PopupFrameOrNull(root), "The ComboBox drop-down");
            var host = root.GetVisualDescendants().OfType<OverlayPopupHost>().Single();
            ThemeScope.GetTokenOwner(host).ShouldBe(ComboBoxTokens.Identity);
            var color = Color.Parse(IsolationColor);

            provider.Config = IsolationConfig(control: (PopupTokens.Identity, "ColorText", IsolationColor));
            SettleFor(TimeSpan.FromMilliseconds(200));
            ForegroundOf(host).ShouldNotBe(color, "Controls[Popup].ColorText reached the popup host.");

            provider.Config = IsolationConfig(control: (ComboBoxTokens.Identity, "ColorText", IsolationColor));
            WaitFor(() => ForegroundOf(host) == color);
            ForegroundOf(host).ShouldBe(color, "Controls[ComboBox].ColorText did not reach its popup host.");
        });

        static Color ForegroundOf(TemplatedControl control) =>
            control.Foreground.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
    }

    // ---- helpers ------------------------------------------------------------------------------

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

    private static void WithIsolationWindow(Control content, Action<ThemeConfigProvider, Func<SKBitmap>> body)
    {
        content.HorizontalAlignment = HorizontalAlignment.Left;
        content.VerticalAlignment   = VerticalAlignment.Top;
        ThemeChangeFailedEventArgs? failure = null;
        var provider = new ThemeConfigProvider
        {
            Config = IsolationConfig(),
            Child  = new AvaloniaBorder { Padding = new Thickness(24), Child = content }
        };
        provider.ThemeChangeFailed += (_, args) => failure = args;
        var window = new AvaloniaWindow { Width = 900, Height = 700, Content = provider };
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
