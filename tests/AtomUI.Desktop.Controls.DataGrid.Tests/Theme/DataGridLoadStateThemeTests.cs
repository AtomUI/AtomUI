using System.Collections.Immutable;
using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

public class DataGridLoadStateThemeTests
{
    private static readonly DataGridFieldId ValueField = new("value");

    static DataGridLoadStateThemeTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Load_State_And_Explicit_Operation_Drive_The_Existing_Spin()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid);
        try
        {
            var spin = grid.GetVisualDescendants().OfType<Spin>().Single();
            grid.LoadState.ShouldBe(DataGridLoadState.Loading);
            grid.EffectiveIsOperating.ShouldBeTrue();
            spin.IsSpinning.ShouldBeTrue();

            Complete(source, 0, "stable");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            grid.EffectiveIsOperating.ShouldBeFalse();
            spin.IsSpinning.ShouldBeFalse();

            grid.IsOperating = true;
            grid.EffectiveIsOperating.ShouldBeTrue();
            spin.IsSpinning.ShouldBeTrue();
            grid.IsOperating = false;
            grid.EffectiveIsOperating.ShouldBeFalse();
            spin.IsSpinning.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Sorting_Refresh_Keeps_Committed_Content_Fully_Visible()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid);
        try
        {
            Complete(source, 0, "stable");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            var spin = grid.GetVisualDescendants().OfType<Spin>().Single();
            var rowsBounds = grid.RowsPresenter!.Bounds;
            var rowKeys = grid.DisplayData.GetScrollingRows()
                .Cast<DataGridRow>()
                .Select(static row => row.RowKey)
                .ToArray();

            grid.SetSort(ValueField, DataGridSortDirection.Ascending);
            source.WaitForRequestCount(2);
            Dispatcher.UIThread.RunJobs();

            grid.LoadState.ShouldBe(DataGridLoadState.Refreshing);
            grid.EffectiveIsOperating.ShouldBeFalse();
            spin.IsSpinning.ShouldBeFalse();
            grid.RowsPresenter.Bounds.ShouldBe(rowsBounds);
            grid.DisplayData.GetScrollingRows().Cast<DataGridRow>()
                .Select(static row => row.RowKey).ShouldBe(rowKeys);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Refreshing_And_Rollback_Preserve_Committed_Geometry_And_Rows()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid);
        try
        {
            Complete(source, 0, "stable");
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            var spin = grid.GetVisualDescendants().OfType<Spin>().Single();
            var rowsBounds = grid.RowsPresenter!.Bounds;
            var headerBounds = grid.GetVisualDescendants()
                .OfType<DataGridColumnHeadersPresenter>()
                .Single()
                .Bounds;
            var scrollBounds = grid.VerticalScrollBar!.Bounds;
            var rowKeys = grid.DisplayData.GetScrollingRows()
                .Cast<DataGridRow>()
                .Select(static row => row.RowKey)
                .ToArray();

            source.RaiseInvalidated();
            source.WaitForRequestCount(2);
            Dispatcher.UIThread.RunJobs();

            grid.LoadState.ShouldBe(DataGridLoadState.Refreshing);
            grid.EffectiveIsOperating.ShouldBeFalse();
            spin.IsSpinning.ShouldBeFalse();
            grid.RowsPresenter.Bounds.ShouldBe(rowsBounds);
            grid.GetVisualDescendants().OfType<DataGridColumnHeadersPresenter>()
                .Single().Bounds.ShouldBe(headerBounds);
            grid.VerticalScrollBar.Bounds.ShouldBe(scrollBounds);
            grid.DisplayData.GetScrollingRows().Cast<DataGridRow>()
                .Select(static row => row.RowKey).ShouldBe(rowKeys);

            var error = new InvalidOperationException("refresh failed");
            source.Fail(1, error);
            PumpUntil(() => grid.LoadState == DataGridLoadState.Error);

            grid.LoadError.ShouldBeSameAs(error);
            grid.IsDataStale.ShouldBeTrue();
            grid.EffectiveIsOperating.ShouldBeFalse();
            spin.IsSpinning.ShouldBeFalse();
            grid.DisplayData.GetScrollingRows().Cast<DataGridRow>()
                .Select(static row => row.RowKey).ShouldBe(rowKeys);
            window.GetVisualDescendants().OfType<Popup>()
                .ShouldAllBe(static popup => !popup.IsOpen);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Initial_Error_Leaves_An_Empty_NonSpinning_Presentation()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid);
        try
        {
            source.Fail(0, new InvalidOperationException("initial failed"));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Error);

            grid.EffectiveIsOperating.ShouldBeFalse();
            grid.GetVisualDescendants().OfType<Spin>().Single().IsSpinning.ShouldBeFalse();
            grid.DisplayData.NumDisplayedScrollingElements.ShouldBe(0);
            grid.TotalEntryCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static ControllableDataGridSource Source() => new(new DataGridSourceSchema(
        typeof(Row),
        [new DataGridFieldSchema(
            ValueField,
            typeof(int),
            DataGridSortDirections.All,
            [],
            canGroup: false)],
        preferredRangeSize: 32,
        maximumRangeSize: 32));

    private static global::AtomUI.Desktop.Controls.DataGrid Grid(IDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source,
            RowHeight = 32,
            Width = 460,
            Height = 240
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId = ValueField,
            Header = "Value",
            Binding = new Binding(nameof(Row.Value))
        });
        return grid;
    }

    private static Window Show(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var window = new Window { Width = 540, Height = 320, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void Complete(
        ControllableDataGridSource source,
        int requestIndex,
        string snapshot)
    {
        var request = source.RequestAt(requestIndex).Request;
        var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(request.Range.Count);
        for (var offset = 0; offset < request.Range.Count; offset++)
        {
            var index = request.Range.StartIndex + offset;
            entries.Add(DataGridSourceEntry.CreateData(
                DataGridRowKey.FromInt64(index + 1L),
                new Row(index),
                index,
                index));
        }
        source.Complete(requestIndex, new DataGridRangeResult(
            request.Range.StartIndex,
            entries.MoveToImmutable(),
            100,
            100,
            100,
            new DataGridSnapshotId(snapshot)));
    }

    private static void PumpUntil(Func<bool> condition)
    {
        if (!SpinWait.SpinUntil(() =>
            {
                Dispatcher.UIThread.RunJobs();
                return condition();
            }, TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("The expected grid state was not reached.");
        }
    }

    private sealed record Row(int Value);
}
