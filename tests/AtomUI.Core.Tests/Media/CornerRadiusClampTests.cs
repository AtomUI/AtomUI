using AtomUI.Media;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Media;

/// <summary>
/// CSS border-radius semantics: a corner radius never exceeds half the shorter side of its rectangle, so an
/// oversized radius stays a symmetric capsule instead of Avalonia's asymmetric overshoot. Overlapping adjacent
/// radii shrink by one uniform factor (ScaleToSize), the rule Skia's rounded rect applies.
/// </summary>
public class CornerRadiusClampTests
{
    [Theory]
    [InlineData(26d, 36d, 10d)]
    [InlineData(26d, 24d, 17d)]
    [InlineData(18d, 60d, 28d)]
    public void Clamp_Never_Exceeds_Half_The_Shorter_Side(double radius, double width, double height)
    {
        var clamped = CornerRadiusUtils.ClampToSize(new CornerRadius(radius), width, height);
        var limit = Math.Min(width, height) / 2;
        Math.Max(Math.Max(clamped.TopLeft, clamped.TopRight), Math.Max(clamped.BottomLeft, clamped.BottomRight))
            .ShouldBeLessThanOrEqualTo(limit + 0.01);
    }

    [Fact]
    public void Clamp_Keeps_Small_Radii_And_Corners_Independent()
    {
        var source = new CornerRadius(1, 2, 3, 4);
        CornerRadiusUtils.ClampToSize(source, 100, 100).ShouldBe(source);
    }

    [Fact]
    public void Clamp_Caps_Each_Corner_Separately()
    {
        // 26 on a 36x10 rect: every corner caps at 5 (half of the 10 side), symmetric like the browser.
        CornerRadiusUtils.ClampToSize(new CornerRadius(26), 36, 10).ShouldBe(new CornerRadius(5));
        // A 17-high button with the full 26 handle radius: caps at 8.5, not the raw 26.
        CornerRadiusUtils.ClampToSize(new CornerRadius(26), 24, 17).ShouldBe(new CornerRadius(8.5));
    }

    [Fact]
    public void Scale_Capsule_Radius_Down_To_Half_The_Shorter_Side()
    {
        // Luma InputRadius 22 on a 32px-high middle input: the browser (and Skia) render a capsule
        // with radius 16, not a 22 radius arc.
        CornerRadiusUtils.ScaleToSize(new CornerRadius(22, 0, 0, 22), 300, 32)
            .ShouldBe(new CornerRadius(16, 0, 0, 16));
    }

    [Fact]
    public void Scale_Applies_The_Single_Min_Factor_To_Every_Corner()
    {
        // Adjacent left radii 30+10=40 exceed the 32px edge: f = 32/40 = 0.8 scales both corners.
        CornerRadiusUtils.ScaleToSize(new CornerRadius(30, 0, 0, 10), 100, 32)
            .ShouldBe(new CornerRadius(24, 0, 0, 8));
    }

    [Fact]
    public void Scale_Keeps_Adjacent_Radii_That_Exactly_Fill_The_Edge()
    {
        // 30+2 = 32 equals the edge: CSS keeps both radii, while a per-corner clamp would cut the 30.
        CornerRadiusUtils.ScaleToSize(new CornerRadius(30, 0, 0, 2), 100, 32)
            .ShouldBe(new CornerRadius(30, 0, 0, 2));
    }

    [Fact]
    public void Scale_Keeps_Small_Radii_And_Corners_Independent()
    {
        var source = new CornerRadius(1, 2, 3, 4);
        CornerRadiusUtils.ScaleToSize(source, 100, 100).ShouldBe(source);
    }

    [Fact]
    public void Scale_Returns_Zero_For_A_Degenerate_Size()
    {
        CornerRadiusUtils.ScaleToSize(new CornerRadius(5, 0, 0, 5), 0, 0).ShouldBe(default(CornerRadius));
    }
}
