using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaCanvas = Avalonia.Controls.Canvas;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class DialogOverlayShadowRenderingTests
{
    // 浮层 MessageBox 打开后，surface 正下方不允许出现不透明黑色条带：
    // 唯一合法的外溢绘制是 token 三级盒阴影（alpha 5%~12%，最深层向下延伸约 45px）。
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Overlay_MessageBox_Casts_Only_A_Soft_Shadow_Below_The_Surface(bool motionEnabled)
    {
        var (window, messageBox) = await ShowConfirmMessageBoxAsync(motionEnabled);
        try
        {
            var surface = FindVisualByTypeName(window, "DialogSurface");
            var surfaceBounds = SurfaceBoundsInWindow(window, surface);
            using var actual = Capture(window);

            var shadowHost = surface.GetVisualDescendants()
                                    .First(visual => visual.GetType().Name == "ShadowsAwareContainer");
            SKBitmap shadowOff;
            try
            {
                shadowHost.SetValue(Border.BoxShadowProperty, default(BoxShadows));
                Refresh(window);
                shadowOff = Capture(window);
            }
            finally
            {
                shadowHost.ClearValue(Border.BoxShadowProperty);
            }
            using var baselinePixels = shadowOff;

            var blackBand = CountBlackPixelsInBand(actual, surfaceBounds, out var bandRect);
            var blackBandWithoutShadow = CountBlackPixelsInBand(shadowOff, surfaceBounds, out _);
            blackBand.ShouldBe(0,
                $"opaque black pixels under the dialog surface (motion={motionEnabled}); " +
                $"band={bandRect}, surface={surfaceBounds}, blackWithShadow={blackBand}, blackWithoutShadow={blackBandWithoutShadow}");

            // 阴影必须真实存在且保持柔和：清除阴影后条带区域应有变化，但任何像素都不得暗到接近纯黑。
            var scale = window.RenderScaling;
            var changed = 0;
            for (var y = (int)Math.Ceiling(surfaceBounds.Bottom * scale) + 1; y < Math.Min(actual.Height, (surfaceBounds.Bottom + 60) * scale); y++)
            for (var x = Math.Max(0, (int)(surfaceBounds.Left * scale) - 20); x < Math.Min(actual.Width, (surfaceBounds.Right + 20) * scale); x++)
            {
                var a = actual.GetPixel(x, y);
                var b = shadowOff.GetPixel(x, y);
                if (Math.Abs(a.Red - b.Red) >= 3 || Math.Abs(a.Green - b.Green) >= 3 || Math.Abs(a.Blue - b.Blue) >= 3)
                {
                    changed++;
                }
            }
            changed.ShouldBeGreaterThan(100,
                $"the dialog shadow must remain visible below the surface (motion={motionEnabled})");
        }
        finally
        {
            messageBox.IsOpen = false;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestScreen : ReactiveUI.IScreen
    {
        public ReactiveUI.RoutingState Router { get; } = new();
    }

    // 模态浮层必须用遮罩整体调暗页面（截图中页面未变暗，遮罩渲染一并锁定）。
    [Fact]
    public async Task Overlay_Modal_MessageBox_Dims_The_Page_With_A_Mask()
    {
        var (window, messageBox) = await ShowConfirmMessageBoxAsync(motionEnabled: false, open: false);
        try
        {
            using var beforeOpen = Capture(window);
            var opened = new TaskCompletionSource();
            messageBox.Opened += (_, _) => opened.TrySetResult();
            messageBox.IsOpen = true;
            await opened.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            await Task.Delay(300, TestContext.Current.CancellationToken);
            Refresh(window);
            using var afterOpen = Capture(window);

            var sampleX = afterOpen.Width - 20;
            var sampleY = afterOpen.Height - 20;
            var before = beforeOpen.GetPixel(sampleX, sampleY);
            var after  = afterOpen.GetPixel(sampleX, sampleY);
            after.Red.ShouldBeLessThan((byte)(before.Red - 30),
                $"the modal mask must darken the page: before={before}, after={after}");
        }
        finally
        {
            messageBox.IsOpen = false;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static async Task<(AvaloniaWindow Window, MessageBox MessageBox)> ShowConfirmMessageBoxAsync(bool motionEnabled, bool open = true)
    {
        var button = new AvaloniaButton
        {
            Content = "Confirm",
            Width   = 80,
            Height  = 32
        };
        AvaloniaCanvas.SetLeft(button, 40);
        AvaloniaCanvas.SetTop(button, 40);
        var messageBox = new MessageBox
        {
            Title           = "Do you want to delete these items?",
            Style           = MessageBoxStyle.Confirm,
            IsMotionEnabled = motionEnabled,
            PlacementTarget = button,
            Content         = new AvaloniaTextBlock { Text = "Some descriptions" }
        };
        var window = new Window
        {
            Width      = 800,
            Height     = 600,
            Background = Brushes.White,
            Content    = new AvaloniaCanvas { Children = { button, messageBox } }
        };
        window.Show();
        window.SetRenderScaling(1);
        Refresh(window);

        if (open)
        {
            var opened = new TaskCompletionSource();
            messageBox.Opened += (_, _) => opened.TrySetResult();
            messageBox.IsOpen = true;
            await opened.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            await Task.Delay(motionEnabled ? 1000 : 300, TestContext.Current.CancellationToken);
            Refresh(window);
        }
        return (window, messageBox);
    }

    private static Control FindVisualByTypeName(Visual root, string typeName)
    {
        return root.GetVisualDescendants()
                   .First(visual => visual.GetType().Name == typeName)
                   .ShouldBeAssignableTo<Control>();
    }

    private static Rect SurfaceBoundsInWindow(AvaloniaWindow window, Control surface)
    {
        var transform = surface.TransformToVisual(window).ShouldNotBeNull();
        return new Rect(surface.Bounds.Size).TransformToAABB(transform);
    }

    private static int CountBlackPixelsInBand(SKBitmap frame, Rect surfaceBounds, out Rect bandRect)
    {
        var scale  = 1d;
        var left   = Math.Max(0, (int)Math.Floor(surfaceBounds.Left * scale) - 20);
        var right  = Math.Min(frame.Width, (int)Math.Ceiling((surfaceBounds.Right + 20) * scale));
        var top    = Math.Min(frame.Height, (int)Math.Ceiling(surfaceBounds.Bottom * scale) + 1);
        var bottom = Math.Min(frame.Height, (int)Math.Ceiling((surfaceBounds.Bottom + 60) * scale));
        bandRect = new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        var count = 0;
        for (var y = top; y < bottom; y++)
        for (var x = left; x < right; x++)
        {
            var pixel = frame.GetPixel(x, y);
            if (pixel is { Red: < 64, Green: < 64, Blue: < 64 })
            {
                count++;
            }
        }
        return count;
    }

    private static SKBitmap Capture(AvaloniaWindow window)
    {
        using var frame = window.CaptureRenderedFrame().ShouldNotBeNull();
        using var stream = new MemoryStream();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }

    private static void Refresh(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}
