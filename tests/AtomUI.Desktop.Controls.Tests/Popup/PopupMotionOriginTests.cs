using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIPopup = AtomUI.Desktop.Controls.Popup;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Popups;

/// <summary>
/// The zoom origin of edge-aligned tooltips and popovers is inset from the aligned edge by the corner radius the popup
/// frame actually draws, so a Control-level radius override (Controls[ToolTip].ToolTipCornerRadius) moves the origin; the
/// scope-root BorderRadius Tokens are not consulted.
/// </summary>
public class PopupMotionOriginTests
{
    static PopupMotionOriginTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(20d, 22d)]
    [InlineData(4d, 12d)]
    public void Tooltip_Origin_Follows_The_Drawn_Corner_Radius(double cornerRadius, double expectedInset)
    {
        var host = new Border
        {
            Width               = 120,
            Height              = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center
        };
        ToolTip.SetTip(host, "A tooltip whose origin is checked");
        ToolTip.SetPlacement(host, PlacementMode.TopEdgeAlignedLeft);
        ToolTip.SetIsUseOverlayHost(host, true);
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithControl(ToolTipTokens.Identity, new ControlThemeConfigBuilder()
                                                          .WithToken("ToolTipCornerRadius", cornerRadius.ToString(System.Globalization.CultureInfo.InvariantCulture))
                                                          .Build())
                     .Build(),
            Child = host
        };
        var window = new AtomUIWindow { Width = 600, Height = 400, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ToolTip.SetIsOpen(host, true);
            Dispatcher.UIThread.RunJobs();

            var tip   = window.GetVisualDescendants().OfType<ToolTip>().Single();
            var popup = tip.Parent.ShouldBeOfType<AtomUIPopup>();
            tip.GetVisualDescendants().OfType<ArrowDecoratedBox>().Single().CornerRadius.TopLeft.ShouldBe(cornerRadius);

            var geometry = popup.GetMotionGeometry();
            geometry.HasAlignment.ShouldBeTrue();
            var motion = ((IPopupMotion)PopupMotionDefaults.TooltipOpen).Resolve(geometry, null);
            var origin = motion.Origin.ShouldNotBeNull();
            (origin.Point.X - geometry.ContentBounds.Left).ShouldBe(expectedInset, 0.01);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
