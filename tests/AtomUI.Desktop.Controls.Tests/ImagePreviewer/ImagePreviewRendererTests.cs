using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewRendererTests
{
    public ImagePreviewRendererTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Stretch_Clr_Wrapper_Uses_Renderer_StretchProperty()
    {
        var renderer = new ImagePreviewRenderer();

        renderer.Stretch = Stretch.Fill;

        renderer.GetValue(ImagePreviewRenderer.StretchProperty).ShouldBe(Stretch.Fill);
    }

    [Fact]
    public void CornerRadius_Is_AddOwner_Of_Border_CornerRadiusProperty()
    {
        var renderer = new ImagePreviewRenderer();

        renderer.CornerRadius = new CornerRadius(4);

        renderer.GetValue(ImagePreviewRenderer.CornerRadiusProperty).ShouldBe(new CornerRadius(4));
        // AddOwner 注册同一属性实例：经 Border 注册表读取到同值，Semantic Style Setter 可用任一视图设置。
        renderer.GetValue(Border.CornerRadiusProperty).ShouldBe(new CornerRadius(4));
    }

    [Fact]
    public void CornerRadius_Applies_A_Rounded_Clip_Geometry_To_The_Child_Image()
    {
        var renderer = new ImagePreviewRenderer
        {
            Width        = 200,
            Height       = 120,
            Source       = new WriteableBitmap(new PixelSize(200, 120), new Vector(96, 96)),
            Stretch      = Stretch.Fill,
            CornerRadius = new CornerRadius(24)
        };

        ShowInWindow(renderer, () =>
        {
            var image = renderer.GetVisualChildren().OfType<Image>().Single();

            // 圆角裁剪直接落在子 Image 的 Clip 上——这是结构性修复的契约：
            // 渲染管线在遍历每个 Visual 时应用 Clip 几何，Image 只渲染一次且带裁剪。
            var clip = image.Clip;
            clip.ShouldNotBeNull();
            clip.ShouldBeOfType<StreamGeometry>();

            // 几何覆盖子 Image 的完整边界（圆角裁剪不会缩小可见范围）。
            AssertRectClose(clip!.Bounds, new Rect(image.Bounds.Size));

            // 几何形状判别：外角点（含圆角区内侧的 (2,2)，距圆心 22√2≈31 > 24）必须在几何外，
            // 直边内侧点必须在几何内。探针点经过挑选——headless 桩实现以连续三点三角形（"耳"）
            // 并集做点包含，多边形中心区域是其盲区；这些探针均落在"耳"覆盖区或明确外部，
            // 在 headless 桩与真实渲染后端下结论一致，可作为圆角形状（而非矩形）的依据。
            clip.FillContains(new Point(0.5, 0.5)).ShouldBeFalse();
            clip.FillContains(new Point(2, 2)).ShouldBeFalse();
            clip.FillContains(new Point(199.5, 0.5)).ShouldBeFalse();
            clip.FillContains(new Point(100, 1)).ShouldBeTrue();
            clip.FillContains(new Point(1, 60)).ShouldBeTrue();
        });
    }

    [Fact]
    public void Zero_CornerRadius_Clears_The_Child_Image_Clip()
    {
        var renderer = new ImagePreviewRenderer
        {
            Width        = 200,
            Height       = 120,
            Source       = new WriteableBitmap(new PixelSize(200, 120), new Vector(96, 96)),
            Stretch      = Stretch.Fill,
            CornerRadius = new CornerRadius(24)
        };

        ShowInWindow(renderer, () =>
        {
            var image = renderer.GetVisualChildren().OfType<Image>().Single();
            image.Clip.ShouldNotBeNull();

            renderer.CornerRadius = new CornerRadius(0);

            image.Clip.ShouldBeNull();
        });
    }

    [Fact]
    public void CornerRadius_Rebuilds_The_Clip_When_The_Child_Image_Relayouts()
    {
        var renderer = new ImagePreviewRenderer
        {
            Width        = 200,
            Height       = 120,
            Source       = new WriteableBitmap(new PixelSize(200, 120), new Vector(96, 96)),
            Stretch      = Stretch.Fill,
            CornerRadius = new CornerRadius(16)
        };

        ShowInWindow(renderer, () =>
        {
            var image = renderer.GetVisualChildren().OfType<Image>().Single();
            AssertRectClose(image.Clip!.Bounds, new Rect(new Size(200, 120)));

            renderer.Width  = 320;
            renderer.Height = 180;
            Dispatcher.UIThread.RunJobs();

            // 裁剪几何必须跟随子 Image 的最新边界，否则缩放后圆角会错位。
            AssertRectClose(image.Clip!.Bounds, new Rect(new Size(320, 180)));
        });
    }

    private static void AssertRectClose(Rect actual, Rect expected)
    {
        const double tolerance = 0.01;
        Math.Abs(actual.X - expected.X).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Y - expected.Y).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Width - expected.Width).ShouldBeLessThan(tolerance);
        Math.Abs(actual.Height - expected.Height).ShouldBeLessThan(tolerance);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 400,
            Height  = 300,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
