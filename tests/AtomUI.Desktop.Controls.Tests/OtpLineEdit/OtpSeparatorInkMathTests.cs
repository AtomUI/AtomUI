using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.OtpLineEdit;

public class OtpSeparatorInkMathTests
{
    // 以 Segoe UI 14px 的典型行盒为参照：baseline≈11.826、lineHeight≈15.258。
    private const double Baseline   = 11.826171875;
    private const double LineHeight = 15.2578125;
    private const double Advance    = 14;
    private const double DesignEm   = 2048;

    [Fact]
    public void Asterisk_Like_Ink_Is_Shifted_Right_And_Down_To_The_Box_Center()
    {
        // 星号：墨迹偏行盒上半区（YBearing 大）、墨迹偏 advance 左侧（XBearing 小）。
        var metrics = new GlyphMetrics
        {
            XBearing = 205,
            YBearing = 1024,
            Width    = 696,
            Height   = 577
        };

        var (dx, dy) = OtpSeparatorInkMath.ComputeInkCenterOffset(
            metrics, Advance, Baseline, LineHeight, 14, DesignEm);

        // 精确手算值（scale=14/2048）：墨迹中心 x=(205+696/2)*s≈3.780 → dx=7-3.780≈3.220；
        // 墨迹中心高于基线 (1024-577/2)*s≈5.028，行盒中心高于基线 ≈4.197 → dy≈0.831。
        dx.ShouldBe(3.2197, 0.02);
        dy.ShouldBe(0.8306, 0.02);
    }

    [Fact]
    public void Hyphen_Like_Ink_Is_Vertically_Centered_In_The_Line_Box()
    {
        // 连字符：墨迹水平基本居中（dx≈0.001）；墨迹中心高于基线 ≈3.104，
        // 低于行盒中心 ≈4.197 → 轻微上移 dy≈-1.094。
        var metrics = new GlyphMetrics
        {
            XBearing = 287,
            YBearing = 523,
            Width    = 1474,
            Height   = 138
        };

        var (dx, dy) = OtpSeparatorInkMath.ComputeInkCenterOffset(
            metrics, Advance, Baseline, LineHeight, 14, DesignEm);

        dx.ShouldBe(0, 0.05);
        dy.ShouldBe(-1.0938, 0.02);
    }

    [Fact]
    public void Offset_Scales_With_Font_Size()
    {
        var metrics = new GlyphMetrics
        {
            XBearing = 205,
            YBearing = 1024,
            Width    = 696,
            Height   = 577
        };

        var small = OtpSeparatorInkMath.ComputeInkCenterOffset(
            metrics, Advance / 2, Baseline / 2, LineHeight / 2, 7, DesignEm);
        var large = OtpSeparatorInkMath.ComputeInkCenterOffset(
            metrics, Advance, Baseline, LineHeight, 14, DesignEm);

        small.OffsetX.ShouldNotBe(0);
        small.OffsetY.ShouldNotBe(0);
        (large.OffsetX / 2).ShouldBe(small.OffsetX, 0.01);
        (large.OffsetY / 2).ShouldBe(small.OffsetY, 0.01);
    }

    [Fact]
    public void Degenerate_Ink_Produces_Zero_Offset()
    {
        var metrics = new GlyphMetrics
        {
            XBearing = 0,
            YBearing = 0,
            Width    = 0,
            Height   = 0
        };

        var (dx, dy) = OtpSeparatorInkMath.ComputeInkCenterOffset(
            metrics, Advance, Baseline, LineHeight, 14, DesignEm);

        dx.ShouldBe(0);
        dy.ShouldBe(0);
    }
}
