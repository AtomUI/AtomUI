using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Layout;

public class DataGridFrozenColumnLayoutTests
{
    static DataGridFrozenColumnLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Issue459_Dynamic_RightFrozenColumnCount_Pins_Rightmost_Column()
    {
        var grid = CreateWideGrid();
        var window = new Window
        {
            Width   = 460,
            Height  = 320,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var rightmostColumn = grid.Columns[^1];
            rightmostColumn.IsRightFrozen.ShouldBeFalse();

            // Issue #459 is triggered when the property changes after the first layout.
            grid.RightFrozenColumnCount = 1;
            Dispatcher.UIThread.RunJobs();

            rightmostColumn.IsRightFrozen.ShouldBeTrue();
            grid.HorizontalScrollBar.ShouldNotBeNull();
            grid.HorizontalScrollBar.IsVisible.ShouldBeTrue();
            grid.HorizontalMaximizeOffset.ShouldBeGreaterThan(0);

            var headersPresenter = grid.GetVisualDescendants()
                                       .OfType<DataGridColumnHeadersPresenter>()
                                       .Single();
            var row = grid.GetVisualDescendants()
                          .OfType<DataGridRow>()
                          .First(dataGridRow => dataGridRow.Index == 0);
            var scrollingColumn = grid.Columns[1];
            var scrollingHeader = scrollingColumn.HeaderCell;
            var scrollingCell   = row.Cells[scrollingColumn.Index];
            var rightmostHeader = rightmostColumn.HeaderCell;
            var rightmostCell   = row.Cells[rightmostColumn.Index];
            var scrollingHeaderLeftBeforeScroll  = scrollingHeader.Bounds.X;
            var scrollingCellLeftBeforeScroll    = scrollingCell.Bounds.X;
            var rightmostHeaderRightBeforeScroll = rightmostHeader.Bounds.Right;
            var rightmostCellRightBeforeScroll   = rightmostCell.Bounds.Right;

            var targetOffset = grid.HorizontalMaximizeOffset / 2;
            targetOffset.ShouldBeGreaterThan(0);
            grid.UpdateHorizontalOffset(targetOffset).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            grid.HorizontalOffset.ShouldBe(targetOffset, 0.01);
            grid.HorizontalScrollBar.Value.ShouldBe(targetOffset, 0.01);
            scrollingHeader.Bounds.X.ShouldBeLessThan(scrollingHeaderLeftBeforeScroll);
            scrollingCell.Bounds.X.ShouldBeLessThan(scrollingCellLeftBeforeScroll);
            rightmostHeader.IsFrozen.ShouldBeTrue();
            rightmostCell.IsFrozen.ShouldBeTrue();
            rightmostHeader.Bounds.Right.ShouldBe(headersPresenter.Bounds.Width, 1);
            rightmostCell.Bounds.Right.ShouldBe(rightmostHeader.Bounds.Right, 1);
            rightmostHeader.Bounds.Right.ShouldBe(rightmostHeaderRightBeforeScroll, 1);
            rightmostCell.Bounds.Right.ShouldBe(rightmostCellRightBeforeScroll, 1);
            rightmostHeader.IsShowFrozenShadow.ShouldBeTrue();
            rightmostCell.IsShowFrozenShadow.ShouldBeTrue();
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
            HeadersVisibility             = DataGridHeadersVisibility.Column,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            ItemsSource = new TestDataGridSource<GridRow>(new[]
            {
                new GridRow("1001", "Northwind", "Shanghai", "Ready")
            }),
            Width  = 420,
            Height = 260
        };

        AddTextColumn(grid, "Id", nameof(GridRow.Id));
        AddTextColumn(grid, "Customer", nameof(GridRow.Customer));
        AddTextColumn(grid, "Region", nameof(GridRow.Region));
        AddTextColumn(grid, "Order state", nameof(GridRow.State));

        return grid;
    }

    private static void AddTextColumn(global::AtomUI.Desktop.Controls.DataGrid grid,
                                      string header,
                                      string bindingPath)
    {
        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = header,
            Binding = new Binding(bindingPath),
            Width   = new DataGridLength(210)
        });
    }

    private sealed record GridRow(string Id, string Customer, string Region, string State);
}
