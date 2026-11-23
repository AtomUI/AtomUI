using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaSplitViewDisplayMode = Avalonia.Controls.SplitViewDisplayMode;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Splitter (the identity-less handle and drag bar read the Splitter's Tokens) and SplitView. Splitter Tokens that size hit
/// areas rather than visuals (the drag trigger area, the handle) and the integer popup z-index assert the laid-out bounds or
/// the property value; SplitView's easing asserts the pane transition it builds.
/// </summary>
public partial class LayoutFamilyEffectTests
{
    // ---- Splitter -------------------------------------------------------------------------------------------------------

    /// <summary>Two filled panels side by side; the first is collapsible with always visible collapse buttons.</summary>
    internal static Splitter CreateSplitter()
    {
        var first  = new AvaloniaBorder { Background = Brushes.LightSteelBlue, Height = 160 };
        var second = new AvaloniaBorder { Background = Brushes.LightSteelBlue, Height = 160 };
        Splitter.SetCollapsible(first, SplitterPanelCollapsible.Parse("Always"));
        Splitter.SetCollapsible(second, SplitterPanelCollapsible.Parse("Always"));
        var splitter = new Splitter { Width = 360, Height = 160, Orientation = Orientation.Vertical };
        splitter.Children.Add(first);
        splitter.Children.Add(second);
        return splitter;
    }

    private static SplitterHandle HandleOf(Control splitter) => Descendant<SplitterHandle>(splitter);

    private static IconButton CollapseButtonOf(Control splitter) =>
        HandleOf(splitter).GetVisualDescendants().OfType<IconButton>()
                          .First(static button => button.IsVisible && button.Bounds.Width > 0);

    /// <summary>Tokens drawn by the handle (line, grip, collapse buttons): compare the handle, not the whole splitter.</summary>
    private static readonly HashSet<string> s_handleTokens = new(StringComparer.Ordinal)
    {
        "HandleIconSize", "HandleIconColor", "HandleIconHoverColor", "HandleIconPressedColor", "ColorText", "ColorFill",
        "SplitBarCollapseOffset", "SplitBarCollapseOffsetNegative", "SplitBarCollapseCrossOffset", "SplitBarDraggableSize",
        "BorderRadiusXS", "FontSizeSM", "ControlItemBgHover", "ControlItemBgActive", "ControlItemBgActiveHover",
        "HandleLineColor", "HandleLineHoverColor", "HandleLineDragColor",
        "HandleGripColor", "HandleGripHoverColor", "HandleGripDragColor"
    };

    public static IEnumerable<object[]> SplitterRows() => MapRows("Splitter");

    [Theory]
    [MemberData(nameof(SplitterRows))]
    public void Splitter_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        switch (token)
        {
            // Hit areas: the drag trigger area is the drag bar's width, the handle size the handle's width.
            case "SplitTriggerSize":
                AssertSplitterExtent(token, "14", static splitter => Descendant<SplitterDragBar>(splitter).Bounds.Width, 14);
                return;
            case "SplitBarHandleSize":
                AssertSplitterExtent(token, "44", static splitter => HandleOf(splitter).Bounds.Width, 44);
                return;
            // The collapse buttons sit above the panels at the popup base z-index.
            case "ZIndexPopupBase":
                AssertSplitterExtent(token, "4000", static splitter => CollapseButtonOf(splitter).ZIndex, 4000);
                return;
        }

        Action<Control>? arrange = token switch
        {
            "HandleLineHoverColor" or "ControlItemBgActive" => static splitter => Pseudo(HandleOf(splitter), ":pointerover"),
            "HandleLineDragColor" or "ControlItemBgActiveHover" => static splitter => Pseudo(HandleOf(splitter), ":dragging"),
            "HandleGripHoverColor" => static splitter => Pseudo(Descendant<SplitterDragBar>(splitter), ":pointerover"),
            "HandleGripDragColor" => static splitter => Pseudo(Descendant<SplitterDragBar>(splitter), ":dragging"),
            "HandleIconHoverColor" => static splitter => Pseudo(CollapseButtonOf(splitter), ":pointerover"),
            "HandleIconPressedColor" => static splitter => Pseudo(CollapseButtonOf(splitter), ":pressed"),
            _ => null
        };
        Run(CreateSplitter, arrange, SplitterTokens.Identity, token,
            new EffectOptions
            {
                Perturb = token == "BorderRadiusXS" ? static _ => new CornerRadius(6) : null,
                // The radius rounds the small collapse buttons: compare one of them.
                Region  = token == "BorderRadiusXS"
                    ? static splitter => CollapseButtonOf(splitter)
                    : s_handleTokens.Contains(token) ? static splitter => HandleOf(splitter) : null
            });
    }

    private static void AssertSplitterExtent(string token, string value, Func<Control, double> read, double expected)
    {
        var splitter = CreateSplitter();
        WithProvider(splitter, provider =>
        {
            Dispatcher.UIThread.RunJobs();
            read(splitter).ShouldNotBe(expected, $"Pick an override of {token} that differs from its default.");
            provider.Config = WithControlToken(SplitterTokens.Identity, token, value);
            WaitFor(() => Math.Abs(read(splitter) - expected) < 0.01);
            TopLevel.GetTopLevel(splitter)!.UpdateLayout();
            read(splitter).ShouldBe(expected, 0.01, $"Controls[Splitter].{token} = {value} did not reach the splitter.");
        });
    }

    [Fact]
    public void Splitter_Bar_Size_Change_Remeasures_The_Handle_Spacing()
    {
        // Borrowed fix b1a6fc37a: SplitterPanel read SplitBarSize once per measure through a resource lookup and was never
        // invalidated, so a runtime theme or Controls[Splitter] change did not move the panels until another layout pass.
        var splitter = new Splitter { Orientation = Orientation.Vertical };
        splitter.Children.Add(new AvaloniaBorder { Width = 120, Height = 80 });
        splitter.Children.Add(new AvaloniaBorder { Width = 160, Height = 80 });
        var host = new Canvas { Children = { splitter } };
        WithProvider(host, provider =>
        {
            provider.Config = WithControlToken(SplitterTokens.Identity, "SplitBarSize", "4");
            Dispatcher.UIThread.RunJobs();
            var window = TopLevel.GetTopLevel(splitter)!;
            window.UpdateLayout();
            var panel   = Descendant<SplitterPanel>(splitter);
            var initial = panel.DesiredSize.Width;

            provider.Config = WithControlToken(SplitterTokens.Identity, "SplitBarSize", "20");
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            panel.DesiredSize.Width.ShouldBe(initial + 16d, 0.01, "The handle spacing did not follow SplitBarSize.");
        });
    }

    // ---- SplitView ------------------------------------------------------------------------------------------------------

    internal static SplitView CreateSplitView(
        AvaloniaSplitViewDisplayMode mode = AvaloniaSplitViewDisplayMode.Inline,
        bool open = true,
        bool lightDismiss = false)
    {
        return new SplitView
        {
            Width                      = 520,
            Height                     = 240,
            DisplayMode                = mode,
            IsPaneOpen                 = open,
            UseLightDismissOverlayMode = lightDismiss,
            Pane                       = new AvaloniaTextBlock { Text = "Pane" },
            // A filled content area: the pane length moves its start edge.
            Content                    = new AvaloniaBorder { Background = Brushes.LightSteelBlue }
        };
    }

    public static IEnumerable<object[]> SplitViewRows() => MapRows("SplitView");

    [Theory]
    [MemberData(nameof(SplitViewRows))]
    public void SplitView_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        switch (token)
        {
            case "PaneMotionEasing":
                AssertSplitViewEasingFollowsToken();
                return;
            case "CompactPaneThemeLength":
                Run(static () => CreateSplitView(AvaloniaSplitViewDisplayMode.CompactInline, open: false), null,
                    SplitViewTokens.Identity, token);
                return;
            case "ColorBgMask":
                Run(static () => CreateSplitView(AvaloniaSplitViewDisplayMode.Overlay, lightDismiss: true), null,
                    SplitViewTokens.Identity, token);
                return;
            // The pane divider is a LineWidth strip: compare the strip.
            case "ColorBorder":
                Run(static () => CreateSplitView(), null, SplitViewTokens.Identity, token,
                    new EffectOptions { Region = static splitView => Descendant<Control>(splitView, "HCPaneBorder") });
                return;
            default:
                Run(static () => CreateSplitView(), null, SplitViewTokens.Identity, token);
                return;
        }
    }

    private static void AssertSplitViewEasingFollowsToken()
    {
        // The easing only shapes the pane animation: the pane transition the SplitView builds carries the Token's easing.
        var splitView = CreateSplitView(open: false);
        WithProvider(splitView, provider =>
        {
            const string value = "0.5,0,0.5,1";
            provider.Config = WithControlToken(SplitViewTokens.Identity, "PaneMotionEasing", value);
            WaitFor(() => ThemeTokenValueFormatter.Format(splitView.PaneMotionEasing) ==
                          ThemeTokenValueFormatter.Format(Easing.Parse(value)));
            ThemeTokenValueFormatter.Format(splitView.PaneMotionEasing)
                                    .ShouldBe(ThemeTokenValueFormatter.Format(Easing.Parse(value)));
            splitView.IsPaneOpen = true;
            Dispatcher.UIThread.RunJobs();
            var transition = splitView.PaneOpenTransitions.ShouldNotBeNull().OfType<DoubleTransition>().Single();
            ThemeTokenValueFormatter.Format(transition.Easing).ShouldBe(ThemeTokenValueFormatter.Format(Easing.Parse(value)));
        });
    }

    [Fact]
    public void SplitView_Pane_Motion_Follows_EnableMotion_And_Its_Durations()
    {
        // SplitView now reads EnableMotion like every other motion-aware control; its pane transitions take the open / close
        // durations of Controls[SplitView].
        var splitView = CreateSplitView(open: false);
        WithProvider(splitView, provider =>
        {
            splitView.IsMotionEnabled.ShouldBeTrue("SplitView pane motion follows EnableMotion (on by default).");
            provider.Config = WithControlToken(SplitViewTokens.Identity, "PaneOpenMotionDuration", "00:00:00.4000000");
            WaitFor(() => splitView.PaneOpenMotionDuration == TimeSpan.FromMilliseconds(400));
            splitView.IsPaneOpen = true;
            Dispatcher.UIThread.RunJobs();
            splitView.PaneOpenTransitions.ShouldNotBeNull().OfType<DoubleTransition>().Single().Duration
                     .ShouldBe(TimeSpan.FromMilliseconds(400));

            provider.Config = WithControlToken(SplitViewTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !splitView.IsMotionEnabled);
            splitView.IsMotionEnabled.ShouldBeFalse("Controls[SplitView].EnableMotion did not reach the SplitView.");
            splitView.PaneOpenTransitions.ShouldBeNull();
        });
    }
}
