using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomButton = AtomUI.Desktop.Controls.Button;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class ButtonFocusVisualRenderingTests
{

    [Fact]
    public async Task Keyboard_Focused_Button_Should_Paint_Focus_Ring_And_Hide_When_Unfocused()
    {
        var button = new AtomButton
        {
            Content          = "Go",
            Width            = 96,
            Height           = 36,
            IsMotionEnabled  = false,
            Background       = Brushes.White,
            BorderBrush      = Brushes.LightGray
        };
        var focusSink = new TextBox { Width = 1, Height = 1, Opacity = 0 };

        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 120,
            Content = new Border
            {
                Background = Brushes.White,
                Padding    = new Thickness(40),
                Child      = new StackPanel
                {
                    Children = { button, focusSink }
                }
            }
        };

        try
        {
            window.Show();
            Refresh(window);

            using var unfocused = Capture(window);
            unfocused.ShouldNotBeNull();

            button.Focus(NavigationMethod.Tab).ShouldBeTrue();
            Refresh(window);

            using var focused = Capture(window);

            var focusedDiffs = CountDifferentPixels(unfocused, focused);
            focusedDiffs.ShouldBeGreaterThan(0, "键盘聚焦后按钮应绘制焦点环（#487 回归：TAB 选中后无任何视觉反馈）");
            focusedDiffs.ShouldBeLessThan((int)(unfocused.Width * unfocused.Height * 0.25),
                "焦点环应只影响按钮边缘区域，而非大面积重绘");

            // 焦点移走后焦点环应消失，画面回到未聚焦形态
            focusSink.Focus().ShouldBeTrue();
            button.IsFocused.ShouldBeFalse();
            Refresh(window);
            using var refocused = Capture(window);
            var unfocusedDiffs = CountDifferentPixels(unfocused, refocused);
            unfocusedDiffs.ShouldBeLessThan((int)(unfocused.Width * unfocused.Height * 0.005),
                "失去键盘焦点后焦点环应隐藏");
        }
        finally
        {
            window.Close();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        }
    }

    private static int CountDifferentPixels(SKBitmap left, SKBitmap right)
    {
        left.Width.ShouldBe(right.Width);
        left.Height.ShouldBe(right.Height);

        var count = 0;
        for (var y = 0; y < left.Height; y++)
        {
            for (var x = 0; x < left.Width; x++)
            {
                if (left.GetPixel(x, y) != right.GetPixel(x, y))
                {
                    count++;
                }
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
