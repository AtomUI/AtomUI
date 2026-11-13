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

public class CardGridItemShadowRenderingTests
{
    // 网格线由 lineWidth 偏移的阴影绘制，高 DPI 下线宽须与卡片外边框一致（均为 1 DIP）
    [Theory]
    [InlineData(1d)]
    [InlineData(1.5d)]
    [InlineData(2d)]
    public void Grid_Lines_Keep_The_Card_Border_Width(double scaling)
    {
        var grid = new CardGridContent
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions    = new RowDefinitions("Auto,Auto")
        };
        for (var r = 0; r < 2; r++)
        for (var c = 0; c < 2; c++)
        {
            grid.Items.Add(new CardGridItem { Row = r, Column = c, Content = string.Empty, IsHoverable = false, Height = 60 });
        }
        var card = new Card { Header = "Card", Width = 300, Content = grid };
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 300,
            Content = new Border { Background = Brushes.White, Padding = new Thickness(20), Child = card }
        };
        window.Show();
        window.SetRenderScaling(scaling);
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            using var frame = Capture(window);

            var item = (Control)grid.Items[0]!;
            var center = item.TranslatePoint(new Point(item.Bounds.Width / 2, item.Bounds.Height / 2), window)!.Value;
            var row = (int)(center.Y * scaling);
            var cardBounds = new Rect(card.Bounds.Size).TransformToAABB(card.TransformToVisual(window)!.Value);
            var runs = DarkRuns(frame, row, (int)Math.Floor(cardBounds.X * scaling), (int)Math.Ceiling(cardBounds.Right * scaling));

            // 最后一段为卡片右边框；列间网格线位于首列右边缘处（分数缩放下左边框与首列网格线可能被抗锯齿拆开，不参与比较）
            var columnEdge = item.TranslatePoint(new Point(item.Bounds.Width, 0), window)!.Value.X * scaling;
            var gridLine = runs.Single(run => Math.Abs(run.Start - columnEdge) <= 2);
            gridLine.Length.ShouldBe(runs[^1].Length, $"grid line runs {string.Join(", ", runs)} at {scaling}x");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static List<(int Start, int Length)> DarkRuns(SKBitmap frame, int y, int from, int to)
    {
        var runs = new List<(int Start, int Length)>();
        var start = -1;
        for (var x = from; x <= to; x++)
        {
            var dark = x < to && frame.GetPixel(x, y).Red < 250;
            if (dark && start < 0)
            {
                start = x;
            }
            else if (!dark && start >= 0)
            {
                runs.Add((start, x - start));
                start = -1;
            }
        }
        return runs;
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
