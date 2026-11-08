using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

/// <summary>
/// A border drawn through the geometry path (background sizing other than CenterBorder, or non-uniform corners) still
/// paints its box shadow; input frames use that path, and their focus shadows were invisible before.
/// </summary>
public class DashedBorderShadowRenderingTests
{
    [Theory]
    [InlineData(BackgroundSizing.InnerBorderEdge, 6, 6)]
    [InlineData(BackgroundSizing.OuterBorderEdge, 6, 6)]
    [InlineData(BackgroundSizing.CenterBorder, 6, 0)]
    public void Box_Shadow_Paints_Outside_The_Border_In_Every_Rendering_Path(
        BackgroundSizing sizing,
        double leftRadius,
        double rightRadius)
    {
        var border = new PixelAlignedBorder
        {
            Width               = 120,
            Height              = 32,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Top,
            BorderThickness  = new Thickness(1),
            CornerRadius     = new CornerRadius(leftRadius, rightRadius, rightRadius, leftRadius),
            BackgroundSizing = sizing,
            Background       = Brushes.White,
            BorderBrush      = Brushes.Gray,
            BoxShadow        = BoxShadows.Parse("0 0 0 4 #ff1677ff")
        };
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 120,
            Content = new Border { Background = Brushes.White, Padding = new Thickness(40), Child = border }
        };

        try
        {
            window.Show();
            FrameCapture.Refresh(window);
            using var frame = FrameCapture.Capture(window);

            // Two pixels left of the border, vertically centred: inside the 4px spread ring, outside the border.
            var pixel = frame.GetPixel(40 - 2, 40 + 16);
            pixel.ShouldNotBe(SKColors.White, $"The {sizing} border did not paint its box shadow.");
        }
        finally
        {
            window.Close();
        }
    }
}
