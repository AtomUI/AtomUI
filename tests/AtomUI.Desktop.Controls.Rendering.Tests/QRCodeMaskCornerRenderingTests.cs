using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

/// <summary>
/// The QRCode mask cover is an opaque rounded-surface child painted over the frame. Without a corner clip it fills
/// the frame's square box, so a masked (Loading/Expired/Scanned) QR loses the rounded card entirely: the mask squares
/// off the corners and paints over the border. The frame must clip its content to the rounded inner edge.
/// </summary>
public class QRCodeMaskCornerRenderingTests
{
    [Theory]
    [InlineData(QRCodeStatus.Loading)]
    [InlineData(QRCodeStatus.Expired)]
    [InlineData(QRCodeStatus.Scanned)]
    public void Masked_QR_Keeps_The_Rounded_Frame_Corners(QRCodeStatus status)
    {
        // An explicit radius keeps the assertion independent of the active theme's radius token.
        const double radius = 26;
        var qr = new QRCode
        {
            Value        = "https://atomui.net",
            Size         = 160,
            Status       = status,
            CornerRadius = new CornerRadius(radius),
            Margin       = new Thickness(24),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Top
        };
        var page = Color.FromRgb(0, 160, 0);
        var window = new AvaloniaWindow
        {
            Width      = 280,
            Height     = 280,
            Background = new SolidColorBrush(page),
            Content    = new Border { Background = new SolidColorBrush(page), Child = qr }
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var frame  = qr.GetVisualDescendants().OfType<DashedBorder>().First(b => b.Name == "Frame");
            var origin = frame.TranslatePoint(new Point(0, 0), window).ShouldNotBeNull();
            using var bitmap = FrameCapture.Capture(window);

            for (var corner = 0; corner < 4; corner++)
            {
                var left = corner is 0 or 2;
                var top  = corner < 2;
                var cx   = left ? origin.X + radius : origin.X + frame.Bounds.Width - radius;
                var cy   = top ? origin.Y + radius : origin.Y + frame.Bounds.Height - radius;
                var voidPixels = 0;
                var maskPixels = 0;
                for (var dy = 0; dy < radius; dy++)
                {
                    for (var dx = 0; dx < radius; dx++)
                    {
                        var x = left ? origin.X + dx : origin.X + frame.Bounds.Width - 1 - dx;
                        var y = top ? origin.Y + dy : origin.Y + frame.Bounds.Height - 1 - dy;
                        if (Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) <= radius)
                        {
                            continue;
                        }

                        voidPixels++;
                        // The mask is white at 244/255 alpha, so any page colour lightens to near white under it.
                        var pixel = bitmap.GetPixel((int)x, (int)y);
                        if (pixel.Red > 235 && pixel.Green > 235 && pixel.Blue > 235)
                        {
                            maskPixels++;
                        }
                    }
                }

                // Only the antialiasing rim may differ; a solid fill means the mask squared off the corner.
                maskPixels.ShouldBeLessThan(voidPixels / 4,
                    $"corner{corner} of the {status} mask: {maskPixels}/{voidPixels} corner-void pixels are mask " +
                    "white, so the mask cover painted outside the rounded frame");
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
