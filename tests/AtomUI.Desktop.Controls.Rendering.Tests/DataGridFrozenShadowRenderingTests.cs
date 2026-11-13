using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class DataGridFrozenShadowRenderingTests
{
    // 对齐 AntD：冻结列分界阴影绘制在冻结单元格外侧、紧贴分界线的滚动区一侧，而不是冻结单元格内部。
    [Theory]
    [InlineData(1d)]
    [InlineData(1.5d)]
    [InlineData(2d)]
    public void Frozen_Shadow_Is_Cast_Outside_The_Frozen_Boundary(double scaling)
    {
        var items = Enumerable.Range(0, 3).Select(_ => new ShadowRow()).ToList();
        var grid = new DataGrid
        {
            AutoGenerateColumns           = false,
            HeadersVisibility             = DataGridHeadersVisibility.Column,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            LeftFrozenColumnCount         = 2,
            RightFrozenColumnCount        = 2,
            ItemsSource                   = new ShadowSource(items),
            Width                         = 700,
            Height                        = 260
        };
        for (var i = 0; i < 10; i++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header  = string.Empty,
                Binding = new Binding(nameof(ShadowRow.Value)),
                Width   = new DataGridLength(150)
            });
        }
        var window = new AvaloniaWindow
        {
            Width   = 760,
            Height  = 320,
            Content = new Border { Padding = new Thickness(20), Background = Brushes.White, Child = grid }
        };
        window.Show();
        window.SetRenderScaling(scaling);
        try
        {
            Refresh(window);
            grid.ScrollIntoView(items[0], grid.Columns[5]);
            Refresh(window);

            var row = grid.GetVisualDescendants().OfType<DataGridRow>().First(r => r.Index == 0);
            var lastLeft = CellOf(grid.Columns[1], row);
            var firstRight = CellOf(grid.Columns[8], row);
            lastLeft.IsShowFrozenShadowForTest().ShouldBeTrue();
            firstRight.IsShowFrozenShadowForTest().ShouldBeTrue();

            using var frame = Capture(window);
            var leftBounds = Bounds(lastLeft, window);
            var rightBounds = Bounds(firstRight, window);
            AssertShadowOutside(frame, scaling, leftBounds, rightBounds, leftBounds.Y + leftBounds.Height / 2);

            // 表头与单元格共用同一分界线
            var headers = grid.GetVisualDescendants().OfType<Control>().Where(c => c.GetType().Name == "DataGridColumnHeader").ToList();
            // 滚动到冻结列下方的表头可能与冻结表头重叠，取绘制在最上层的那个
            var leftHeader = headers.Where(h => Math.Abs(Bounds(h, window).Right - leftBounds.Right) < 1)
                                    .OrderByDescending(h => h.ZIndex).First();
            var rightHeader = headers.Where(h => Math.Abs(Bounds(h, window).X - rightBounds.X) < 1)
                                     .OrderByDescending(h => h.ZIndex).First();
            var headerBounds = Bounds(leftHeader, window);
            AssertShadowOutside(frame, scaling, headerBounds, Bounds(rightHeader, window),
                headerBounds.Y + headerBounds.Height / 2);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void AssertShadowOutside(SKBitmap frame, double scaling, Rect leftBounds, Rect rightBounds, double y)
    {
        // 采样坐标以 DIP 给出，按渲染缩放换算为帧缓冲的物理像素
        int Px(double dip) => (int)Math.Floor(dip * scaling);
        var row = Px(y);
        var leftEdge = leftBounds.Right;
        var rightEdge = rightBounds.X;

        // 阴影落在分界线外侧的滚动区：紧贴分界线处明显比远离分界线处更暗（AntD 阴影在 1 DIP 处约暗 9 级）
        Shade(frame, Px(leftEdge + 1), Px(leftEdge + 20), row).ShouldBeGreaterThan(5,
            $"the left frozen shadow must visibly darken the scrolling area next to the boundary at {scaling}x");
        Shade(frame, Px(rightEdge - 1.5), Px(rightEdge - 20), row).ShouldBeGreaterThan(5,
            $"the right frozen shadow must visibly darken the scrolling area next to the boundary at {scaling}x");

        // 冻结单元格内部（紧贴分界线一侧）不应出现阴影
        Math.Abs(Shade(frame, Px(leftEdge - 3), Px(leftBounds.X + leftBounds.Width / 2), row)).ShouldBeLessThanOrEqualTo(1,
            $"the left frozen shadow must not be drawn inside the frozen cell at {scaling}x");
        Math.Abs(Shade(frame, Px(rightEdge + 3), Px(rightBounds.X + rightBounds.Width / 2), row)).ShouldBeLessThanOrEqualTo(1,
            $"the right frozen shadow must not be drawn inside the frozen cell at {scaling}x");
    }

    private static DataGridCell CellOf(DataGridColumn column, DataGridRow row) =>
        column.GetCellContent(row).ShouldNotBeNull().FindAncestorOfType<DataGridCell>().ShouldNotBeNull();

    // 采样点相对参考点的变暗程度
    private static int Shade(SKBitmap frame, int x, int referenceX, int y) =>
        frame.GetPixel(referenceX, y).Red - frame.GetPixel(x, y).Red;

    private static Rect Bounds(Visual visual, AvaloniaWindow window)
    {
        var transform = visual.TransformToVisual(window).ShouldNotBeNull();
        return new Rect(visual.Bounds.Size).TransformToAABB(transform);
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
        Dispatcher.UIThread.RunJobs();
    }

    public sealed class ShadowRow
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class ShadowSource(IReadOnlyList<ShadowRow> items) : IDataGridSource
    {
        private readonly DataGridSnapshotId _snapshot = new($"shadow-{Guid.NewGuid():N}");

        public DataGridSourceSchema Schema { get; } = new(typeof(ShadowRow),
            ImmutableArray<DataGridFieldSchema>.Empty, preferredRangeSize: 32, maximumRangeSize: 128);

        public event EventHandler? Invalidated
        {
            add { }
            remove { }
        }

        public ValueTask<DataGridRangeResult> FetchAsync(DataGridFetchRequest request, CancellationToken cancellationToken)
        {
            var start = Math.Min(request.Range.StartIndex, items.Count);
            var count = Math.Min(request.Range.Count, items.Count - start);
            var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
            for (var index = start; index < start + count; index++)
            {
                entries.Add(DataGridSourceEntry.CreateData(DataGridRowKey.FromInt64(index + 1L), items[index], index, index));
            }
            return ValueTask.FromResult(new DataGridRangeResult(start, entries.MoveToImmutable(),
                items.Count, items.Count, items.Count, _snapshot));
        }
    }
}

internal static class DataGridCellShadowExtensions
{
    // IsShowFrozenShadow 为 internal，渲染测试程序集不可见，按名称读取 DirectProperty。
    public static bool IsShowFrozenShadowForTest(this DataGridCell cell)
    {
        var property = AvaloniaPropertyRegistry.Instance.GetRegistered(cell)
                                               .Single(p => p.Name == "IsShowFrozenShadow");
        return (bool)cell.GetValue(property)!;
    }
}
