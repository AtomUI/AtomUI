using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using System.Collections.Immutable;
using Avalonia.Data;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Query;

public class DataGridColumnQueryProjectionTests
{
    private static readonly DataGridFieldId Age = new("age");

    static DataGridColumnQueryProjectionTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Query_Projects_SortState_To_Column_Header_And_Realized_Cells()
    {
        var source = Source(DataGridSortDirections.All);
        var column = Column(DataGridSortDirections.All);
        var (window, grid) = Show(source, column);
        try
        {
            Complete(source, grid, 0, "initial");

            column.CanUserSort.ShouldBeNull();
            column.SortState.ShouldBe(DataGridColumnSortState.None);
            column.HeaderCell.CanUserSort.ShouldBeTrue();

            grid.SetSort(Age, DataGridSortDirection.Ascending);

            column.SortState.Direction.ShouldBe(DataGridSortDirection.Ascending);
            column.SortState.Priority.ShouldBe(0);
            column.HeaderCell.CurrentSortingState.ShouldBe(
                System.ComponentModel.ListSortDirection.Ascending);
            RealizedCells(grid, column).ShouldAllBe(cell => cell.IsSorting);
            source.Requests.Count.ShouldBe(2);

            grid.SetSort(Age, DataGridSortDirection.Ascending);
            source.Requests.Count.ShouldBe(2);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Effective_Capability_Requires_Grid_Column_Field_Schema_And_Direction_Intersection()
    {
        var source = Source(DataGridSortDirections.Ascending);
        var column = Column(DataGridSortDirections.Descending);
        var (window, grid) = Show(source, column);
        try
        {
            Complete(source, grid, 0, "initial");

            column.HeaderCell.CanUserSort.ShouldBeFalse();
            column.HeaderCell.ProcessSort(Avalonia.Input.KeyModifiers.None);
            grid.Query.ShouldBe(DataGridQuery.Empty);
            source.Requests.Count.ShouldBe(1);

            column.SupportedSortDirections = DataGridSortDirections.Ascending;
            column.HeaderCell.CanUserSort.ShouldBeTrue();
            column.HeaderCell.ProcessSort(Avalonia.Input.KeyModifiers.None);

            grid.Query.Sorts.ShouldBe([
                new DataGridSort(Age, DataGridSortDirection.Ascending)]);
            source.Requests.Count.ShouldBe(2);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Header_Gesture_Cycles_Query_And_Shift_Preserves_Priority()
    {
        var name = new DataGridFieldId("name");
        var source = new ControllableDataGridSource(new DataGridSourceSchema(
            typeof(Row),
            [
                new DataGridFieldSchema(Age, typeof(int), DataGridSortDirections.All, [], false),
                new DataGridFieldSchema(name, typeof(string), DataGridSortDirections.All, [], false)
            ],
            32,
            32))
        {
            HonorCancellation = true
        };
        var ageColumn = Column(DataGridSortDirections.All);
        var nameColumn = new DataGridTextColumn
        {
            Header = "Name",
            FieldId = name,
            Binding = new Binding(nameof(Row.Name))
        };
        var (window, grid) = Show(source, ageColumn, nameColumn);
        try
        {
            Complete(source, grid, 0, "initial");

            ageColumn.HeaderCell.ProcessSort(Avalonia.Input.KeyModifiers.None);
            grid.Query.Sorts.ShouldBe([
                new DataGridSort(Age, DataGridSortDirection.Ascending)]);

            nameColumn.HeaderCell.ProcessSort(Avalonia.Input.KeyModifiers.Shift);
            grid.Query.Sorts.ShouldBe([
                new DataGridSort(Age, DataGridSortDirection.Ascending),
                new DataGridSort(name, DataGridSortDirection.Ascending)]);
            ageColumn.SortState.Priority.ShouldBe(0);
            nameColumn.SortState.Priority.ShouldBe(1);

            ageColumn.HeaderCell.ProcessSort(Avalonia.Input.KeyModifiers.None);
            grid.Query.Sorts.ShouldBe([
                new DataGridSort(Age, DataGridSortDirection.Descending)]);
            ageColumn.SortState.Priority.ShouldBe(0);
            nameColumn.SortState.ShouldBe(DataGridColumnSortState.None);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Cells_Have_No_Long_Lived_Sort_Subscription()
    {
        var source = Source(DataGridSortDirections.All);
        var column = Column(DataGridSortDirections.All);
        var (window, grid) = Show(source, column);
        try
        {
            Complete(source, grid, 0, "initial");
            RealizedCells(grid, column).ShouldAllBe(cell => !cell.HasLongLivedSortSubscription);

            grid.SetSort(Age, DataGridSortDirection.Descending);

            RealizedCells(grid, column).ShouldAllBe(cell =>
                cell.IsSorting && !cell.HasLongLivedSortSubscription);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Filter_Confirm_And_Clear_Replace_Query_And_Checked_State_Is_Projected()
    {
        var region = new DataGridFieldId("region");
        var containsAny = new DataGridOperatorId("contains-any");
        var source = new ControllableDataGridSource(new DataGridSourceSchema(
            typeof(Row),
            [new DataGridFieldSchema(
                region,
                typeof(string),
                DataGridSortDirections.All,
                [new DataGridFilterOperatorSchema(
                    containsAny,
                    1,
                    16,
                    DataGridScalarKinds.String)],
                false)],
            32,
            32))
        {
            HonorCancellation = true
        };
        var column = new DataGridTextColumn
        {
            Header = "Region",
            FieldId = region,
            Filters = new[] { "London", "Paris" },
            Binding = new Binding(nameof(Row.Name))
        };
        var (window, grid) = Show(source, column);
        grid.CanUserFilterColumns = true;
        try
        {
            Complete(source, grid, 0, "initial");

            column.HeaderCell.ProcessFilter(["London", "Paris"]);

            grid.Query.Filters.ShouldBe([
                new DataGridFilter(
                    region,
                    containsAny,
                    ImmutableArray.Create(
                        DataGridScalar.FromString("London"),
                        DataGridScalar.FromString("Paris")))
            ]);
            column.IsFilterValueSelected("London").ShouldBeTrue();
            column.IsFilterValueSelected("Berlin").ShouldBeFalse();

            column.HeaderCell.ProcessClearFilter();
            grid.Query.Filters.ShouldBeEmpty();
            column.IsFilterValueSelected("London").ShouldBeFalse();
        }
        finally
        {
            Close(window);
        }
    }

    private static DataGridTextColumn Column(DataGridSortDirections directions) => new()
    {
        Header = "Age",
        FieldId = Age,
        SupportedSortDirections = directions,
        Binding = new Binding(nameof(Row.Age))
    };

    private static ControllableDataGridSource Source(DataGridSortDirections directions) => new(
        new DataGridSourceSchema(
            typeof(Row),
            [new DataGridFieldSchema(Age, typeof(int), directions, [], false)],
            32,
            32))
    {
        HonorCancellation = true
    };

    private static (Window Window, global::AtomUI.Desktop.Controls.DataGrid Grid) Show(
        ControllableDataGridSource source,
        params DataGridColumn[] columns)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            CanUserSortColumns = true,
            ItemsSource = source,
            Width = 320,
            Height = 180
        };
        foreach (var column in columns)
        {
            grid.Columns.Add(column);
        }
        var window = new Window { Width = 400, Height = 240, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        source.WaitForRequestCount(1);
        return (window, grid);
    }

    private static void Complete(
        ControllableDataGridSource source,
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int requestIndex,
        string snapshot)
    {
        var pending = source.RequestAt(requestIndex);
        var request = pending.Request;
        var count = Math.Min(request.Range.Count, 64 - request.Range.StartIndex);
        source.Complete(requestIndex, new DataGridRangeResult(
            request.Range.StartIndex,
            [.. Enumerable.Range(0, count).Select(offset =>
                DataGridSourceEntry.CreateData(
                    DataGridRowKey.FromInt64(request.Range.StartIndex + offset),
                    new Row(request.Range.StartIndex + offset, $"row-{request.Range.StartIndex + offset}"),
                    request.Range.StartIndex + offset,
                    request.Range.StartIndex + offset))],
            64,
            64,
            64,
            new DataGridSnapshotId(snapshot)));
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
    }

    private static DataGridCell[] RealizedCells(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        DataGridColumn column) =>
        [.. grid.DisplayData.GetScrollingRows()
            .Cast<DataGridRow>()
            .Where(row => column.Index < row.Cells.Count)
            .Select(row => row.Cells[column.Index])];

    private static void PumpUntil(Func<bool> condition)
    {
        var completed = SpinWait.SpinUntil(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return condition();
        }, TimeSpan.FromSeconds(5));
        completed.ShouldBeTrue();
    }

    private static void Close(Window window)
    {
        window.Close();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed record Row(int Age, string Name);
}
