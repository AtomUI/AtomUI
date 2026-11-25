using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class IconBackgroundRenderingTests
{
    private const int IconSize = 40;
    private static readonly PixelPoint IconOrigin = new(30, 20);

    [Fact]
    public void Default_Icon_Draws_Glyph_Only_Inside_Its_Bounds()
    {
        using var bitmap = Render(background: null);

        CountPixels(bitmap, InsideIcon, IsDark).ShouldBeGreaterThan(20);
        CountPixels(bitmap, OutsideIcon, pixel => !IsWhite(pixel)).ShouldBe(0);
    }

    // Icon previously had a template Border bound to Background, painted above the glyph. The template was
    // removed for performance; this pins the unchanged visual output: an opaque Background covers the glyph.
    [Fact]
    public void Opaque_Background_Keeps_Covering_The_Glyph_Like_The_Former_Template_Border()
    {
        using var bitmap = Render(Brushes.Red);

        CountPixels(bitmap, InsideIcon, IsRed).ShouldBe(IconSize * IconSize);
        CountPixels(bitmap, InsideIcon, IsDark).ShouldBe(0);
    }

    private static SKBitmap Render(IBrush? background)
    {
        var icon = new SearchOutlined
        {
            Width       = IconSize,
            Height      = IconSize,
            StrokeBrush = Brushes.Black,
            FillBrush   = Brushes.Black
        };
        if (background is not null)
        {
            icon.Background = background;
        }

        var canvas = new Canvas();
        Canvas.SetLeft(icon, IconOrigin.X);
        Canvas.SetTop(icon, IconOrigin.Y);
        canvas.Children.Add(icon);

        var window = new AvaloniaWindow
        {
            Width      = 120,
            Height     = 100,
            Background = Brushes.White,
            Content    = canvas
        };

        window.Show();
        try
        {
            window.SetRenderScaling(1);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            using var renderedFrame = window.CaptureRenderedFrame().ShouldNotBeNull();
            using var stream        = new MemoryStream();
            renderedFrame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            return SKBitmap.Decode(stream);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static bool InsideIcon(int x, int y)
    {
        return x >= IconOrigin.X && x < IconOrigin.X + IconSize &&
               y >= IconOrigin.Y && y < IconOrigin.Y + IconSize;
    }

    private static bool OutsideIcon(int x, int y)
    {
        // A one-pixel margin keeps antialiased glyph edges out of the outside region.
        return x < IconOrigin.X - 1 || x > IconOrigin.X + IconSize ||
               y < IconOrigin.Y - 1 || y > IconOrigin.Y + IconSize;
    }

    private static int CountPixels(SKBitmap bitmap, Func<int, int, bool> region, Func<SKColor, bool> predicate)
    {
        var count = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (region(x, y) && predicate(bitmap.GetPixel(x, y)))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool IsDark(SKColor pixel) => pixel.Red < 64 && pixel.Green < 64 && pixel.Blue < 64;

    private static bool IsRed(SKColor pixel) => pixel.Red > 200 && pixel.Green < 64 && pixel.Blue < 64;

    private static bool IsWhite(SKColor pixel) => pixel.Red > 250 && pixel.Green > 250 && pixel.Blue > 250;
}
