using System.Diagnostics;
using System.Reflection;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class ButtonFamilyEffectTests
{
    /// <summary>
    /// Flyout gap Tokens move a popup that opens outside the owner. The effect is asserted on the popup itself: with
    /// the overlay popup layer, the vertical distance between the anchor and the opened flyout grows by exactly the
    /// change of the Token-derived gap.
    /// </summary>
    private static void AssertFlyoutGapFollowsToken(
        Func<Control> createOwner,
        Func<Control, Control> anchorOf,
        Action<Control, bool> setOpen,
        ControlTokenIdentity identity,
        string token,
        Func<Control, double> readGap)
    {
        var owner    = createOwner();
        var provider = new ThemeConfigProvider { Config = BaselineConfig(null), Child = owner };
        var overlay  = new ScopeAwareOverlayLayerPanel { Width = 600, Height = 400 };
        overlay.Children.Add(provider);
        var layers = new VisualLayerManager { Child = overlay };
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .SetValue(layers, true);
        var window = new AvaloniaWindow { Width = 600, Height = 400, Content = layers };
        try
        {
            window.Show();
            Settle(window);
            var gapBefore    = readGap(owner);
            var offsetBefore = OpenAndMeasure(window, owner, anchorOf, setOpen);

            var accessor = ControlTokenAccessor.Capture(owner);
            var current  = token == "UniformlyMarginXXS"
                ? accessor.GetEffectiveGlobal<double>(SharedTokenKind.UniformlyMarginXXS)
                : gapBefore;
            var perturbed = current * 2;
            provider.Config = BaselineConfig((identity, token, perturbed.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            Settle(window);
            var gapAfter = readGap(owner);
            gapAfter.ShouldBeGreaterThan(gapBefore, $"Token '{token}' of '{identity}' did not reach the flyout gap.");

            var offsetAfter = OpenAndMeasure(window, owner, anchorOf, setOpen);
            (offsetAfter - offsetBefore).ShouldBe(gapAfter - gapBefore, 0.5,
                $"Token '{token}' of '{identity}' changed the gap from {gapBefore} to {gapAfter} but the opened flyout " +
                $"moved from {offsetBefore} to {offsetAfter}.");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static double OpenAndMeasure(
        AvaloniaWindow window,
        Control owner,
        Func<Control, Control> anchorOf,
        Action<Control, bool> setOpen)
    {
        setOpen(owner, false);
        Settle(window);
        setOpen(owner, true);
        Settle(window);
        var presenter = window.GetVisualDescendants()
                              .OfType<MenuFlyoutPresenter>()
                              .FirstOrDefault(static p => p.IsEffectivelyVisible && p.Bounds.Height > 0)
                              .ShouldNotBeNull("The flyout did not open.");
        var anchor = anchorOf(owner);
        var origin = presenter.TranslatePoint(new Point(), anchor).ShouldNotBeNull();
        return origin.Y - anchor.Bounds.Height;
    }

    private static void Settle(AvaloniaWindow window)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(300))
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Thread.Sleep(5);
        }
    }

    private static ThemeConfig BaselineConfig((ControlTokenIdentity Identity, string Token, string Value)? entry)
    {
        var builder = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        if (entry is { } value)
        {
            builder.WithControl(value.Identity, new ControlThemeConfigBuilder().WithToken(value.Token, value.Value).Build());
        }

        return builder.Build();
    }

    private static void AssertDropdownFlyoutGap(string token)
    {
        AssertFlyoutGapFollowsToken(
            static () => new DropdownButton { Content = "Dropdown", DropdownFlyout = CreateMenuFlyout() },
            static owner => owner,
            static (owner, open) => ((DropdownButton)owner).IsPopupPinnedOpen = open,
            DesignTokens.DropdownButtonTokens.Identity,
            token,
            static owner => ((DropdownButton)owner).MarginToAnchor);
    }

    private static void AssertSplitButtonFlyoutGap(string token)
    {
        AssertFlyoutGapFollowsToken(
            static () => new SplitButton { Content = "Split", Flyout = CreateMenuFlyout() },
            static owner => owner,
            static (owner, open) => ((SplitButton)owner).IsPopupPinnedOpen = open,
            DesignTokens.SplitButtonTokens.Identity,
            token,
            static owner => ((SplitButton)owner).GutterToFlyout);
    }
}
