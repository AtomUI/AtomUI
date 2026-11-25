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
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class ButtonSpinnerHandleClipRenderingTests
{
    // The floating handle slides in from its edge (HandleOffset) while it fades in. Mid-slide it must be clipped to the
    // input frame: an unclipped handle draws its divider and arrows past the control's border.
    [Theory]
    [InlineData(Location.Right)]
    [InlineData(Location.Left)]
    public void Sliding_Handle_Is_Clipped_To_The_Input_Frame(Location location)
    {
        // NumericUpDown always places its handle on the right; a ButtonSpinner covers the left handle.
        Control numeric = location == Location.Right
            ? new NumericUpDown { Value = 3, IsMotionEnabled = false }
            : new ButtonSpinner
            {
                ButtonSpinnerLocation    = ButtonSpinnerLocation.Left,
                IsButtonSpinnerFloatable = true,
                IsMotionEnabled          = false,
                Content                  = "3"
            };
        numeric.Width               = 200;
        numeric.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        numeric.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Top;
        numeric.Margin              = new Thickness(60, 40);
        var window = new AvaloniaWindow
        {
            Width      = 360,
            Height     = 140,
            Background = Brushes.White,
            Content    = numeric
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var box = numeric.GetVisualDescendants().OfType<ButtonSpinnerDecoratedBox>().Single();
            box.IsHandleFloatable.ShouldBeTrue();
            box.ButtonSpinnerLocation.ShouldBe(location == Location.Right ? ButtonSpinnerLocation.Right : ButtonSpinnerLocation.Left);
            var handleWidth = box.SpinnerHandleWidth;
            handleWidth.ShouldBeGreaterThan(0);

            // Freeze the handle half-way through its slide, fully opaque, so whatever overhangs is visible.
            box.HandleOpacity = 1;
            box.HandleOffset  = (location == Location.Right ? 1 : -1) * handleWidth / 2;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            using var frame = Capture(window);
            var bounds = new Rect(numeric.Bounds.Size).TransformToAABB(numeric.TransformToVisual(window)!.Value);
            // Columns just outside the control on the handle's side, excluding the focus/shadow band of a few pixels.
            var outsideFrom = location == Location.Right ? (int)Math.Ceiling(bounds.Right) + 4 : (int)Math.Floor(bounds.Left) - 4 - (int)handleWidth;
            var outsideTo   = outsideFrom + (int)handleWidth;
            var inked       = 0;
            for (var x = Math.Max(0, outsideFrom); x < Math.Min(frame.Width, outsideTo); x++)
            {
                for (var y = (int)bounds.Top; y < (int)bounds.Bottom; y++)
                {
                    var pixel = frame.GetPixel(x, y);
                    if (pixel.Red < 245 || pixel.Green < 245 || pixel.Blue < 245)
                    {
                        inked++;
                    }
                }
            }
            inked.ShouldBe(0, $"handle pixels drawn outside the {location} edge of the control");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static SKBitmap Capture(AvaloniaWindow window)
    {
        using var frame = window.CaptureRenderedFrame().ShouldNotBeNull();
        using var stream = new MemoryStream();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }
}
