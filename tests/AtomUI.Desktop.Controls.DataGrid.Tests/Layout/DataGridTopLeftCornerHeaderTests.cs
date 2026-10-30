using AtomUI.Controls.Primitives;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Layout;

public class DataGridTopLeftCornerHeaderTests
{
    static DataGridTopLeftCornerHeaderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Top_Left_Corner_Does_Not_Render_Column_Header_Vertical_Separator()
    {
        var grid = CreateWideGrid();
        var window = ShowGrid(grid);

        try
        {
            var topLeftCorner = grid.GetVisualDescendants()
                                    .OfType<global::AtomUI.Desktop.Controls.DataGridTopLeftColumnHeader>()
                                    .Single();

            topLeftCorner.IsVisible.ShouldBeTrue();
            topLeftCorner.Bounds.Width.ShouldBe(grid.RowHeaderWidth, 1);

            topLeftCorner.GetVisualDescendants()
                         .OfType<PixelAlignedBorder>()
                         .Where(border => border.Name is "VerticalSeparator" or "PART_VerticalSeparator")
                         .ShouldBeEmpty();

            grid.GetVisualDescendants()
                .OfType<PixelAlignedBorder>()
                .Where(border => border.Name == "PART_VerticalSeparator")
                .Any(border => border.IsVisible)
                .ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateWideGrid()
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns           = false,
            GridLinesVisibility           = DataGridGridLinesVisibility.All,
            HeadersVisibility             = DataGridHeadersVisibility.All,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            LeftFrozenColumnCount         = 3,
            RowHeaderWidth                = 32,
            ItemsSource                        = new TestDataGridSource<GridRow>(
                Enumerable.Range(0, 30).Select(index => new GridRow(index)).ToArray()),
            Width                         = 520,
            Height                        = 260
        };

        for (var i = 0; i < 8; i++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header  = $"Column {i}",
                Binding = new Binding(nameof(GridRow.Index)),
                Width   = new DataGridLength(140)
            });
        }

        return grid;
    }

    private static Window ShowGrid(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var window = new Window
        {
            Width   = 560,
            Height  = 320,
            Content = grid
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed record GridRow(int Index);
}
