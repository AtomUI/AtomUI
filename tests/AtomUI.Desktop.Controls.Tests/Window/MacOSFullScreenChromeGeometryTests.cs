using AtomUI.Native;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

/// <summary>
/// Guards the rules that turn the observed macOS title strip into full-screen chrome metrics. The strip itself can
/// only be observed on a real macOS session, so the two ways AppKit hides it (sliding it above the window, or fading
/// it out in place on a display with a camera housing) are pinned here as inputs.
/// </summary>
public class MacOSFullScreenChromeGeometryTests
{
    private const double StripHeight = 44;
    private const double MenuBarHeight = 24;
    private const double ButtonsTrailingEdge = 76;

    private static TitleStripState Strip(double topOffset, double alpha, bool isFused)
    {
        return new TitleStripState(topOffset, StripHeight, alpha, isFused, ButtonsTrailingEdge);
    }

    [Fact]
    public void Window_Without_A_Strip_Reports_No_Chrome()
    {
        var metrics = MacOSFullScreenChromeGeometry.Compute(0, null);

        metrics.ShouldBe(new FullScreenNativeChromeMetrics(0, default, 0, false));
    }

    [Fact]
    public void Faded_Out_Strip_At_The_Client_Top_Is_Not_Presented()
    {
        // Camera-housing display at rest: the strip sits at the client top with alpha 0.
        var fused   = MacOSFullScreenChromeGeometry.Compute(0, Strip(0, 0, true));
        var stacked = MacOSFullScreenChromeGeometry.Compute(0, Strip(0, 0, false));

        fused.ShouldBe(new FullScreenNativeChromeMetrics(0, default, 0, false));
        // A strip that is not shown must not push the managed surface down.
        stacked.ShouldBe(new FullScreenNativeChromeMetrics(0, default, 0, false));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    public void Fading_In_Strip_At_The_Client_Top_Is_Presented_Without_Extent(double alpha)
    {
        // Camera-housing display: the menu bar stays above the client area, so nothing covers the surface.
        var metrics = MacOSFullScreenChromeGeometry.Compute(0, Strip(0, alpha, true));

        metrics.IsChromePresented.ShouldBeTrue();
        metrics.Extent.ShouldBe(0);
        metrics.ControlInset.ShouldBe(new Thickness(ButtonsTrailingEdge, 0, 0, 0));
    }

    [Fact]
    public void Strip_Slid_Above_The_Window_Is_Not_Presented()
    {
        var metrics = MacOSFullScreenChromeGeometry.Compute(0, Strip(-(StripHeight + MenuBarHeight), 1, true));

        metrics.ShouldBe(new FullScreenNativeChromeMetrics(0, default, 0, false));
    }

    [Fact]
    public void Fused_Strip_Under_The_Menu_Bar_Reports_Only_The_Menu_Bar_As_Extent()
    {
        var metrics = MacOSFullScreenChromeGeometry.Compute(0, Strip(MenuBarHeight, 1, true));

        metrics.IsChromePresented.ShouldBeTrue();
        metrics.Extent.ShouldBe(MenuBarHeight);
        metrics.ControlInset.ShouldBe(new Thickness(ButtonsTrailingEdge, 0, 0, 0));
    }

    [Fact]
    public void Strip_Entering_The_Client_Area_Is_Presented_Before_The_Menu_Bar_Covers_It()
    {
        // Half of the strip has slid in; the buttons are already visible, so the surface must be their background.
        var metrics = MacOSFullScreenChromeGeometry.Compute(0, Strip(-StripHeight / 2, 1, true));

        metrics.IsChromePresented.ShouldBeTrue();
        metrics.Extent.ShouldBe(0);
    }

    [Fact]
    public void Stacked_Strip_Counts_Into_The_Extent_And_Reserves_No_Control_Inset()
    {
        var metrics = MacOSFullScreenChromeGeometry.Compute(0, Strip(MenuBarHeight, 1, false));

        metrics.IsChromePresented.ShouldBeTrue();
        metrics.Extent.ShouldBe(MenuBarHeight + StripHeight);
        metrics.ControlInset.ShouldBe(default);
    }

    [Fact]
    public void Persistent_Inset_Is_Passed_Through()
    {
        var metrics = MacOSFullScreenChromeGeometry.Compute(MenuBarHeight, Strip(MenuBarHeight, 1, true));

        metrics.PersistentInset.ShouldBe(MenuBarHeight);
    }
}
