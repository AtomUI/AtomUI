using AtomUI.Media;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Media;

/// <summary>
/// CSS border-radius semantics: a corner radius never exceeds half the shorter side of its rectangle, so an
/// oversized radius stays a symmetric capsule instead of Avalonia's asymmetric overshoot.
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
}
