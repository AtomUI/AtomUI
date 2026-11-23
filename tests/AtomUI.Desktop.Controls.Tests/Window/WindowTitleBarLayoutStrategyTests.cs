using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowTitleBarLayoutStrategyTests
{
    [Theory]
    [InlineData(OsType.macOS, Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter)]
    [InlineData(OsType.Windows, Desktop.Controls.WindowTitleBarTitleAlignment.Left)]
    [InlineData(OsType.Linux, Desktop.Controls.WindowTitleBarTitleAlignment.Left)]
    [InlineData(OsType.Unknown, Desktop.Controls.WindowTitleBarTitleAlignment.Left)]
    [InlineData(OsType.Android, Desktop.Controls.WindowTitleBarTitleAlignment.Left)]
    public void Platform_Strategy_Maps_Auto_To_The_Approved_Default(
        OsType osType,
        Desktop.Controls.WindowTitleBarTitleAlignment expected)
    {
        Desktop.Controls.WindowTitleBarLayoutStrategies.Get(osType)
               .AutoAlignment
               .ShouldBe(expected);
    }

    [Theory]
    [InlineData(OsType.macOS)]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.Linux)]
    public void Known_Platforms_Preserve_Current_Reported_Overlap_Regardless_Of_Csd_Name(OsType osType)
    {
        var resolved = Desktop.Controls.WindowTitleBarLayoutStrategies.Get(osType)
                              .ResolveNativeChromeInsets(
                                  500,
                                  new Thickness(72, 4, 31, 5),
                                  isCsdEnabled: false);

        resolved.ShouldBe(new Thickness(72, 0, 31, 0));
    }

    [Fact]
    public void Known_Platform_Metrics_Are_Normalized_And_Clamped_To_The_Frame()
    {
        var resolved = Desktop.Controls.WindowTitleBarLayoutStrategies.Get(OsType.macOS)
                              .ResolveNativeChromeInsets(
                                  100,
                                  new Thickness(double.NaN, 3, 180, 4),
                                  isCsdEnabled: true);

        resolved.ShouldBe(new Thickness(0, 0, 100, 0));
    }

    [Fact]
    public void Unknown_Platform_Does_Not_Invent_An_Inset_From_Untrusted_Metrics()
    {
        var resolved = Desktop.Controls.WindowTitleBarLayoutStrategies.Get(OsType.Unknown)
                              .ResolveNativeChromeInsets(
                                  500,
                                  new Thickness(72, 0, 31, 0),
                                  isCsdEnabled: true);

        resolved.ShouldBe(default);
    }
}
