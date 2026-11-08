using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Popup and ToolTip. Popup is a Token-owner transparent container: its own Tokens (frame radius, shadows, gap) are read by
/// the hosts that open it, so the Popup rows open a ComboBox drop-down (an <c>atom:Popup</c>) and compare its frame; motion
/// Tokens are read back. ToolTip rows open a real tooltip on a host through the overlay popup layer and compare the tooltip.
/// </summary>
public partial class FeedbackFamilyEffectTests
{
    // ---- Popup --------------------------------------------------------------------------------

    /// <summary>
    /// PopupRootShadow is drawn only by a window-hosted popup root, which the headless platform cannot create; its row is
    /// replaced by <see cref="Popup_Root_Shadow_Is_The_Frame_Shadow_Of_A_Window_Hosted_Popup"/>.
    /// </summary>
    public static IEnumerable<object[]> PopupRows() =>
        MapRows("Popup").Where(static row => (string)row[0] != "PopupRootShadow");

    [Theory]
    [MemberData(nameof(PopupRows))]
    public void Popup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The gap moves the whole drop-down: compare the host area. Frame Tokens compare the drop-down frame.
        Func<Control, Control> region = token is "MarginToAnchor" or "UniformlyMarginXXS"
            ? static target => (Control)TopLevel.GetTopLevel(target)!.GetVisualDescendants().OfType<Panel>()
                                                .First(static panel => panel.Width > 0)
            : static target => PopupFrameOf(target);
        Run(static () =>
            {
                var comboBox = new ComboBox { Width = 200, IsPopupPinnedOpen = true };
                for (var index = 0; index < 3; index++)
                {
                    comboBox.Items.Add(new ComboBoxItem { Content = $"Item {index}" });
                }
                return OverlayLayerHost(comboBox);
            },
            null, PopupTokens.Identity, token,
            new EffectOptions
            {
                Target = static root =>
                {
                    WaitForVisual(() => PopupFrameOrNull(root), "The ComboBox drop-down");
                    return root.GetVisualDescendants().OfType<Popup>().First();
                },
                Region = region,
                // A radius change only repaints the four corners of the frame: perturb it to a clearly rounder frame.
                Perturb = token is "PopupCornerRadius" or "BorderRadiusLG"
                    ? static _ => new CornerRadius(32)
                    : null
            });
    }

    [Fact]
    public void Popup_Root_Shadow_Is_The_Frame_Shadow_Of_A_Window_Hosted_Popup()
    {
        // A popup that is not on the overlay layer opens in its own popup root, whose ShadowsAwareContainer draws the
        // popup's FrameShadow; the headless platform has no popup windows, so the frame shadow the root would draw is
        // asserted instead of its pixels.
        var popup = new Popup { ShouldUseOverlayLayer = false, Child = new AvaloniaBorder { Width = 80, Height = 40 } };
        WithProvider(new Panel { Children = { popup } }, provider =>
        {
            var shadow = BoxShadows.Parse("0 12 32 4 #66000000");
            popup.FrameShadow.ShouldBe(popup.PopupRootShadow);
            popup.FrameShadow.ShouldNotBe(shadow);
            provider.Config = WithControlToken(PopupTokens.Identity, "PopupRootShadow", shadow.ToString());
            WaitFor(() => popup.PopupRootShadow == shadow);
            popup.PopupRootShadow.ShouldBe(shadow);
            popup.FrameShadow.ShouldBe(shadow);
        });
    }

    [Fact]
    public void Popup_Motion_Follows_Its_Own_Motion_Tokens()
    {
        // The Popup's open / close motion duration and switch are Popup globals (Controls[Popup]).
        var popup = new Popup { Child = new AvaloniaBorder { Width = 80, Height = 40 } };
        WithProvider(new Panel { Children = { popup } }, provider =>
        {
            popup.IsMotionEnabled.ShouldBeTrue();
            var duration = TimeSpan.FromMilliseconds(777);
            popup.MotionDuration.ShouldNotBe(duration);
            provider.Config = WithControlToken(PopupTokens.Identity, "MotionDurationMid", duration.ToString("c"));
            WaitFor(() => popup.MotionDuration == duration);
            popup.MotionDuration.ShouldBe(duration);

            provider.Config = WithControlToken(PopupTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !popup.IsMotionEnabled);
            popup.IsMotionEnabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void ToolTip_Motion_Follows_Its_Motion_Tokens()
    {
        // The tooltip's zoom duration is its MotionDuration Own Token (default MotionDurationFast); the opened popup plays it.
        var root = CreateToolTipHost("Prompt text");
        WithProvider(root, provider =>
        {
            var tip   = OpenedToolTip(root);
            var popup = tip.Parent.ShouldBeOfType<Popup>();
            tip.MotionDuration.ShouldBe(ControlTokenAccessor.Capture(tip).GetEffectiveGlobal<TimeSpan>(SharedTokenKind.MotionDurationFast));
            var duration = TimeSpan.FromMilliseconds(555);
            provider.Config = WithControlToken(ToolTipTokens.Identity, "MotionDuration", duration.ToString("c"));
            WaitFor(() => popup.MotionDuration == duration);
            tip.MotionDuration.ShouldBe(duration);
            popup.MotionDuration.ShouldBe(duration);

            provider.Config = WithControlToken(ToolTipTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !popup.IsMotionEnabled);
            popup.IsMotionEnabled.ShouldBeFalse();
        });
    }

    private static AvaloniaBorder? PopupFrameOrNull(Control anyInWindow) =>
        TopLevel.GetTopLevel(anyInWindow)?.GetVisualDescendants().OfType<AvaloniaBorder>()
                .FirstOrDefault(static border => border.Name == "PopupFrame" && border.IsEffectivelyVisible &&
                                                 border.Bounds.Height > 0);

    private static Control PopupFrameOf(Control anyInWindow) =>
        PopupFrameOrNull(anyInWindow) ?? throw new InvalidOperationException("The drop-down frame is not open.");

    private const string PopupThemes = "src/AtomUI.Desktop.Controls/Popup/Themes/";

    /// <summary>
    /// Text defaults of the popup hosts, read with OwnerTokenResource: the hosts inherit the owner of the control that
    /// hosts the Popup (Popup is Token-owner transparent). <see cref="Popup_Content_Text_Follows_The_Host_Owner"/> renders
    /// them (the overlay host; the window-hosted popup root reads the same Tokens and cannot open headless).
    /// </summary>
    internal static readonly OwnerReadList s_overlayPopupHost =
        new(PopupThemes + "OverlayPopupHostTheme.axaml", ["ColorText", "FontFamily", "FontSize"]);

    internal static readonly OwnerReadList s_popupRoot =
        new(PopupThemes + "PopupRootTheme.axaml", ["ColorText", "FontFamily", "FontSize"]);

    [Theory]
    [InlineData("ColorText", "#ff00b96b")]
    [InlineData("FontSize", "22")]
    public void Popup_Content_Text_Follows_The_Host_Owner(string token, string value)
    {
        // A plain text popup opened from an Alert's extra action: the popup host's text defaults are the Alert's
        // effective globals, so Controls[Alert] changes the popup text and Controls[Popup] does not.
        var popup = new Popup
        {
            ShouldUseOverlayLayer = true,
            Child                 = new Avalonia.Controls.TextBlock { Text = "Popup content text", Margin = new Thickness(8) }
        };
        var anchor = new AvaloniaBorder { Width = 24, Height = 16, Child = popup };
        popup.PlacementTarget = anchor;
        var alert = new Alert { Message = "Host", ExtraAction = anchor, Width = 240 };
        var root  = OverlayLayerHost(alert, 480, 320);
        WithIsolationWindow(root, (provider, capture) =>
        {
            popup.IsOpen = true;
            var text = WaitForVisual(() => popup.Child as Control, "The popup content");
            var host = root.GetVisualDescendants().OfType<OverlayPopupHost>().Single();
            ThemeScope.GetTokenOwner(host).ShouldBe(AlertTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (PopupTokens.Identity, token, value));
            using var byPopup = capture();
            ChangedRatioOf(before, byPopup, text).ShouldBe(0d, $"Controls[Popup].{token} reached the popup content text.");

            provider.Config = IsolationConfig(control: (AlertTokens.Identity, token, value));
            using var byAlert = capture();
            ChangedRatioOf(before, byAlert, text).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                $"Controls[Alert].{token} did not reach the content of the Popup it hosts.");
        });
    }

    // ---- ToolTip ------------------------------------------------------------------------------

    public static IEnumerable<object[]> ToolTipRows() => MapRows("ToolTip");

    [Theory]
    [MemberData(nameof(ToolTipRows))]
    public void ToolTip_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The max width only shows on text long enough to wrap.
        var content = token == "ToolTipMaxWidth"
            ? string.Join(' ', Enumerable.Repeat("A tooltip long enough to wrap", 8))
            : "Prompt text";
        Run(() => CreateToolTipHost(content), null, ToolTipTokens.Identity, token,
            new EffectOptions { Target = static root => OpenedToolTip(root) });
    }

    /// <summary>A host showing an open tooltip (placement Top) through the overlay popup layer.</summary>
    internal static Control CreateToolTipHost(object content, Action<ToolTip>? init = null, Control? host = null,
                                              Control? sibling = null)
    {
        host ??= new AvaloniaBorder
        {
            Width      = 96,
            Height     = 32,
            Background = Brushes.LightGray
        };
        host.Margin = new Thickness(160, 200, 0, 0);
        var tip = new ToolTip { Content = content };
        init?.Invoke(tip);
        ToolTip.SetTip(host, tip);
        ToolTip.SetIsUseOverlayHost(host, true);
        ToolTip.SetIsOpen(host, true);
        var root = OverlayLayerHost(host, 560, 400);
        if (sibling is not null)
        {
            // A second member shares the layer panel, above the tooltip's anchor.
            sibling.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            sibling.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Top;
            ((Panel)((Avalonia.Controls.Primitives.VisualLayerManager)root).Child!).Children.Add(sibling);
        }
        return root;
    }

    internal static ToolTip OpenedToolTip(Control root) =>
        WaitForVisual(() => root.GetVisualDescendants().OfType<ToolTip>()
                                .FirstOrDefault(static tip => tip.Classes.Contains(ToolTipPseudoClass.Open)),
            "The tooltip");
}
