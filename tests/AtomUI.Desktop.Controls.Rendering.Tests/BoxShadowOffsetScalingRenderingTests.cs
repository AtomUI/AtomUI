using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

// Avalonia 的 Skia 后端把阴影偏移按物理像素施加；AtomUI 边框经 BoxShadowRenderHelper 绘制，偏移须随渲染缩放。
public class BoxShadowOffsetScalingRenderingTests
{
    public static TheoryData<double> Scalings => new() { 1d, 1.5d, 2d };

    [Theory]
    [MemberData(nameof(Scalings))]
    public void Outer_Shadow_Offset_Follows_Render_Scaling(double scaling)
    {
        var shadow = new BoxShadow { OffsetX = 4, Color = Colors.Black };
        using var frame = Render(scaling, shadow, out var bounds);
        var row = Px(bounds.Y + bounds.Height / 2, scaling);

        // 盒子右侧外 4 DIP 为实心阴影，其外不再有阴影
        Darkness(frame, Px(bounds.Right + 0.5, scaling), row).ShouldBeGreaterThan(200);
        Darkness(frame, Px(bounds.Right + 3.5, scaling), row).ShouldBeGreaterThan(200);
        Darkness(frame, Px(bounds.Right + 4.5, scaling), row).ShouldBe(0);
        // 盒子内部不绘制外阴影
        Darkness(frame, Px(bounds.Right - 1, scaling), row).ShouldBe(0);
    }

    [Theory]
    [MemberData(nameof(Scalings))]
    public void Inset_Shadow_Offset_Follows_Render_Scaling(double scaling)
    {
        var shadow = new BoxShadow { OffsetX = 4, IsInset = true, Color = Colors.Black };
        using var frame = Render(scaling, shadow, out var bounds);
        var row = Px(bounds.Y + bounds.Height / 2, scaling);

        // 盒子左侧内 4 DIP 为实心阴影，其余部分与盒子外不受影响
        Darkness(frame, Px(bounds.X + 0.5, scaling), row).ShouldBeGreaterThan(200);
        Darkness(frame, Px(bounds.X + 3.5, scaling), row).ShouldBeGreaterThan(200);
        Darkness(frame, Px(bounds.X + 4.5, scaling), row).ShouldBe(0);
        Darkness(frame, Px(bounds.X - 1, scaling), row).ShouldBe(0);
        Darkness(frame, Px(bounds.Right - 1, scaling), row).ShouldBe(0);
    }

    [Theory]
    [MemberData(nameof(Scalings))]
    public void Inset_Shadow_With_Negative_Spread_Keeps_Its_Profile(double scaling)
    {
        // AntD 冻结列阴影 inset 10px 0 8px -8px：分界处约暗 11 级，6 DIP 内衰减完
        var shadow = new BoxShadow { OffsetX = 10, Blur = 8, Spread = -8, IsInset = true, Color = Color.FromArgb(15, 5, 5, 5) };
        using var frame = Render(scaling, shadow, out var bounds);
        var row = Px(bounds.Y + bounds.Height / 2, scaling);

        Darkness(frame, Px(bounds.X + 0.5, scaling), row).ShouldBeInRange(8, 13);
        Darkness(frame, Px(bounds.X + 2.5, scaling), row).ShouldBeInRange(4, 8);
        Darkness(frame, Px(bounds.X + 8, scaling), row).ShouldBe(0);
    }

    private static SKBitmap Render(double scaling, BoxShadow shadow, out Rect bounds)
    {
        var border = new DashedBorder
        {
            Width               = 30,
            Height              = 40,
            Margin              = new Thickness(20),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment   = VerticalAlignment.Top,
            Background          = Brushes.Transparent,
            BoxShadow           = new BoxShadows(shadow)
        };
        var window = new AvaloniaWindow
        {
            Width   = 120,
            Height  = 100,
            Content = new Border { Background = Brushes.White, Child = border }
        };
        window.Show();
        window.SetRenderScaling(scaling);
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            bounds = new Rect(border.Bounds.Size).TransformToAABB(border.TransformToVisual(window).ShouldNotBeNull());
            using var captured = window.CaptureRenderedFrame().ShouldNotBeNull();
            using var stream = new MemoryStream();
            captured.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            return SKBitmap.Decode(stream);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static int Px(double dip, double scaling) => (int)Math.Floor(dip * scaling);

    private static int Darkness(SKBitmap frame, int x, int y) => 255 - frame.GetPixel(x, y).Red;
}
