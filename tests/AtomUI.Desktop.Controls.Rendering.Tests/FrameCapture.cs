using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Shouldly;
using SkiaSharp;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

/// <summary>
/// Compositor frame capture helpers shared by rendering and Token effect tests.
/// </summary>
internal static class FrameCapture
{
    /// <summary>Captures the window's last rendered compositor frame as a decoded bitmap.</summary>
    public static SKBitmap Capture(AvaloniaWindow window)
    {
        using var frame  = window.CaptureRenderedFrame().ShouldNotBeNull();
        using var stream = new MemoryStream();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }

    /// <summary>Drains queued UI work and runs a layout pass so the next capture reflects current state.</summary>
    public static void Refresh(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    /// <summary>Counts pixels that differ in any channel by more than <paramref name="channelTolerance"/>.</summary>
    public static int CountDifferentPixels(SKBitmap left, SKBitmap right, int channelTolerance = 0)
    {
        left.Width.ShouldBe(right.Width);
        left.Height.ShouldBe(right.Height);
        return CountDifferentPixels(left, right, new SKRectI(0, 0, left.Width, left.Height), channelTolerance);
    }

    /// <summary>
    /// Counts pixels inside <paramref name="region"/> that differ in any channel by more than
    /// <paramref name="channelTolerance"/>.
    /// </summary>
    public static int CountDifferentPixels(SKBitmap left, SKBitmap right, SKRectI region, int channelTolerance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channelTolerance);
        left.Width.ShouldBe(right.Width);
        left.Height.ShouldBe(right.Height);
        region.Left.ShouldBeGreaterThanOrEqualTo(0);
        region.Top.ShouldBeGreaterThanOrEqualTo(0);
        region.Right.ShouldBeLessThanOrEqualTo(left.Width);
        region.Bottom.ShouldBeLessThanOrEqualTo(left.Height);

        var count = 0;
        for (var y = region.Top; y < region.Bottom; y++)
        {
            for (var x = region.Left; x < region.Right; x++)
            {
                if (Differs(left.GetPixel(x, y), right.GetPixel(x, y), channelTolerance))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool Differs(SKColor left, SKColor right, int channelTolerance)
    {
        return Math.Abs(left.Red - right.Red) > channelTolerance ||
               Math.Abs(left.Green - right.Green) > channelTolerance ||
               Math.Abs(left.Blue - right.Blue) > channelTolerance ||
               Math.Abs(left.Alpha - right.Alpha) > channelTolerance;
    }
}
