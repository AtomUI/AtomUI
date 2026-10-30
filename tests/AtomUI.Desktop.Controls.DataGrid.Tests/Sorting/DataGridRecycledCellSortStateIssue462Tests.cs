using Avalonia;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Sorting;

// https://github.com/AtomUI/AtomUI/issues/462 回归测试
// 连续切换排序字段时，提交新查询会复用回收池中的 DataGridRow。回收行已有单元格必须从
// DataGridColumn.SortState 重新投影 IsSorting，不能携带此前实现周期的历史排序状态。
public class DataGridRecycledCellSortStateIssue462Tests
{
    private static readonly DataGridFieldId FirstField = new("first");
    private static readonly DataGridFieldId SecondField = new("second");
    private static readonly DataGridFieldId ThirdField = new("third");
    private static readonly DataGridFieldId FourthField = new("fourth");

    static DataGridRecycledCellSortStateIssue462Tests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void RepeatedSorts_ShouldProjectOnlyFinalSortStateToRecycledCells()
    {
        var rows = new[]
        {
            new SortRow(1, 40, 2, 3, 4),
            new SortRow(2, 10, 4, 1, 3),
            new SortRow(3, 30, 1, 4, 2),
            new SortRow(4, 20, 3, 2, 1)
        };
        var descriptor = DataGridLocalSourceDescriptor.For<SortRow>(
                static row => DataGridRowKey.FromInt64(row.Id))
            .Field(FirstField, static row => row.First)
            .Field(SecondField, static row => row.Second)
            .Field(ThirdField, static row => row.Third)
            .Field(FourthField, static row => row.Fourth);
        using var source = DataGridLocalSource.Create(rows, descriptor);
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            CanUserSortColumns = true,
            SelectionMode = DataGridSelectionMode.Extended,
            ItemsSource = source,
            Width = 640,
            Height = 240
        };
        AddColumn(grid, "First", FirstField, nameof(SortRow.First));
        AddColumn(grid, "Second", SecondField, nameof(SortRow.Second));
        AddColumn(grid, "Third", ThirdField, nameof(SortRow.Third));
        AddColumn(grid, "Fourth", FourthField, nameof(SortRow.Fourth));

        var selectedKey = DataGridRowKey.FromInt64(3);
        grid.Selection = new DataGridSelectionState([selectedKey], null, [], []);

        var window = new Window
        {
            Width = 680,
            Height = 280,
            Content = grid
        };
        try
        {
            window.Show();
            PumpUntil(() =>
                grid.LoadState == DataGridLoadState.Ready &&
                DisplayedIds(grid).SequenceEqual([1L, 2L, 3L, 4L]));

            ApplySortAndWait(grid, FirstField, [2, 4, 3, 1]);
            ApplySortAndWait(grid, SecondField, [3, 1, 4, 2]);
            ApplySortAndWait(grid, ThirdField, [2, 4, 1, 3]);
            ApplySortAndWait(grid, FourthField, [4, 3, 2, 1]);

            grid.Query.Sorts.ShouldBe([
                new DataGridSort(FourthField, DataGridSortDirection.Ascending)]);

            var realizedRows = DisplayedRows(grid);
            realizedRows.Length.ShouldBe(4);
            foreach (var row in realizedRows)
            {
                Enumerable.Range(0, 4)
                   .Select(index => row.Cells[index].IsSorting)
                   .ShouldBe([false, false, false, true],
                       $"row {((SortRow)row.DataContext!).Id} should project only the final sort column");
            }

            grid.Selection.ExplicitKeys.ShouldBe([selectedKey]);
            realizedRows.Single(row => ((SortRow)row.DataContext!).Id == 3)
                        .IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void AddColumn(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        string header,
        DataGridFieldId field,
        string property)
    {
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = header,
            FieldId = field,
            Binding = new Binding(property),
            CanUserSort = true,
            Width = new DataGridLength(140)
        });
    }

    private static void ApplySortAndWait(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        DataGridFieldId field,
        long[] expectedIds)
    {
        grid.SetSort(field, DataGridSortDirection.Ascending);
        PumpUntil(() =>
            grid.LoadState == DataGridLoadState.Ready &&
            grid.AppliedQuery == grid.Query &&
            DisplayedIds(grid).SequenceEqual(expectedIds));
    }

    private static long[] DisplayedIds(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        DisplayedRows(grid)
            .Select(row => ((SortRow)row.DataContext!).Id)
            .ToArray();

    private static DataGridRow[] DisplayedRows(global::AtomUI.Desktop.Controls.DataGrid grid) =>
        grid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .Select(row => new { Row = row, Position = row.TranslatePoint(default, grid) })
            .Where(item => item.Position is not null && item.Row.DataContext is SortRow)
            .OrderBy(item => item.Position!.Value.Y)
            .Select(item => item.Row)
            .ToArray();

    private static void PumpUntil(Func<bool> condition)
    {
        if (!SpinWait.SpinUntil(() =>
            {
                Dispatcher.UIThread.RunJobs();
                return condition();
            }, TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("The expected sorted presentation was not reached.");
        }
    }

    private sealed record SortRow(long Id, int First, int Second, int Third, int Fourth);
}
