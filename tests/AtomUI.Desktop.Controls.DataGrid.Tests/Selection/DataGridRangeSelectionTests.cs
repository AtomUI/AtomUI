using System.Collections.Immutable;
using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Selection;

public class DataGridRangeSelectionTests
{
    private static readonly DataGridFieldId ValueField = new("value");

    static DataGridRangeSelectionTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void SelectionChanged_Reports_Immutable_Old_And_New_Expressions()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        DataGridSelectionChangedEventArgs? observed = null;
        grid.SelectionChanged += (_, args) => observed = args;
        try
        {
            Complete(source, grid, 0, "s1");

            grid.SetRowSelection(0, isSelected: true, setAnchorSlot: true);

            observed.ShouldNotBeNull();
            observed.OldSelection.ShouldBeSameAs(DataGridSelectionState.Empty);
            observed.NewSelection.ExplicitKeys.ShouldBe([Key(1)]);
            observed.NewSelection.IndexIntervals.ShouldBeEmpty();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Range_Gestures_Use_Keys_Intervals_And_One_AllMatching_Descriptor()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, 0, "s1");
            var initialFetchCount = source.Requests.Count;

            grid.SetRowSelection(0, isSelected: true, setAnchorSlot: true);
            grid.Selection.ExplicitKeys.ShouldBe([Key(1)]);
            grid.CurrentRowKey.ShouldBeNull();
            DisplayedRow(grid, 0).IsSelected.ShouldBeTrue();

            grid.SetRowsSelection(2, 8);
            grid.Selection.IndexIntervals.Length.ShouldBe(1);
            grid.Selection.IndexIntervals[0].StartIndex.ShouldBe(2);
            grid.Selection.IndexIntervals[0].EndIndexExclusive.ShouldBe(9);

            grid.SelectAll();
            grid.Selection.AllMatchingQuery.ShouldNotBeNull();
            grid.Selection.IndexIntervals.ShouldBeEmpty();
            source.Requests.Count.ShouldBe(initialFetchCount);

            grid.SetRowSelection(0, isSelected: false, setAnchorSlot: false);
            grid.Selection.ExcludedKeys.ShouldBe([Key(1)]);
            DisplayedRow(grid, 0).IsSelected.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Recycled_Rows_Reproject_Selection_By_Key_And_Do_Not_Use_Old_Slots()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, 0, "s1");
            grid.SetRowSelection(2, isSelected: true, setAnchorSlot: true);

            grid.RequestRangeViewport(64, 8, 1);
            Dispatcher.UIThread.RunJobs();
            source.WaitForRequestCount(2);
            Complete(source, grid, 1, "s1");
            DisplayedRows(grid).ShouldAllBe(static row => !row.IsSelected);

            grid.RequestRangeViewport(0, 8, -1);
            Dispatcher.UIThread.RunJobs();
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready &&
                            grid.DisplayData.FirstScrollingSlot <= 2 &&
                            grid.DisplayData.LastScrollingSlot >= 2);
            Dispatcher.UIThread.RunJobs();
            DisplayedRow(grid, 2).IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Selection_Transitions_Only_After_Successful_Presentation_Commit()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, 0, "s1");
            grid.SetRowsSelection(2, 8);
            var oldSelection = grid.Selection;

            grid.SetSort(ValueField, DataGridSortDirection.Ascending);
            source.WaitForRequestCount(2);
            grid.Selection.ShouldBeSameAs(oldSelection);

            Complete(source, grid, 1, "s2");
            grid.Selection.ExplicitKeys.ShouldBe(oldSelection.ExplicitKeys);
            grid.Selection.IndexIntervals.ShouldBeEmpty();

            var appliedSelection = grid.Selection;
            grid.SetSort(ValueField, DataGridSortDirection.Descending);
            source.WaitForRequestCount(3);
            source.Fail(2, new InvalidOperationException("failed"));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Error);
            grid.Selection.ShouldBeSameAs(appliedSelection);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Source_Replacement_Clears_Selection_And_Current_Key()
    {
        var source = Source();
        var replacement = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, 0, "s1");
            grid.SetRowSelection(2, isSelected: true, setAnchorSlot: true);
            grid.CurrentRowKey = Key(3);

            grid.ItemsSource = replacement;

            grid.Selection.ShouldBeSameAs(DataGridSelectionState.Empty);
            grid.CurrentRowKey.ShouldBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Current_Key_And_Home_End_Navigation_Follow_Global_Range_Identity()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, 0, "s1");
            grid.UpdateSelectionAndCurrency(
                    0,
                    0,
                    DataGridSelectionAction.SelectCurrent,
                    scrollIntoView: false)
                .ShouldBeTrue();
            grid.CurrentRowKey.ShouldBe(Key(1));

            var command = new KeyEventArgs
            {
                KeyModifiers = KeyModifiers.Control | KeyModifiers.Meta
            };
            grid.ProcessEndKey(command).ShouldBeTrue();
            CompleteRequestContaining(source, grid, 127, "s1");
            grid.CurrentRowKey.ShouldBe(
                Key(128),
                GetRangeNavigationState(grid, source));
            grid.CurrentSlot.ShouldBe(127);

            grid.ProcessHomeKey(command).ShouldBeTrue();
            PumpUntil(() => grid.CurrentRowKey == Key(1));
            grid.CurrentSlot.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Page_Navigation_Changes_Current_Key_Without_Materializing_Selection_Items()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, 0, "s1");
            grid.UpdateSelectionAndCurrency(
                0, 0, DataGridSelectionAction.SelectCurrent, scrollIntoView: false);
            var noModifiers = new KeyEventArgs();

            grid.ProcessNextKey(noModifiers).ShouldBeTrue();
            grid.CurrentRowKey.ShouldNotBeNull();
            grid.CurrentRowKey.ShouldNotBe(Key(1));
            grid.Selection.ExplicitKeys.ShouldBe([Key(1)]);

            grid.ProcessPriorKey(noModifiers).ShouldBeTrue();
            grid.CurrentRowKey.ShouldBe(Key(1));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public async Task Key_Lookup_Scrolls_To_Uncached_Row_Without_Changing_Current_Or_Selection()
    {
        var inner = Source();
        var source = new LookupSource(inner, Key(65), new DataGridKeyLookupResult(64, 64));
        var grid = Grid(source);
        var window = Show(grid, inner);
        try
        {
            Complete(inner, grid, 0, "s1");
            var scroll = grid.ScrollIntoViewAsync(
                Key(65),
                cancellationToken: TestContext.Current.CancellationToken).AsTask();
            PumpUntil(() => scroll.IsCompleted);
            (await scroll).ShouldBeTrue();
            source.LookupCount.ShouldBe(1);

            CompleteRequestContaining(inner, grid, 64, "s1");
            grid.DisplayData.FirstScrollingSlot.ShouldBe(64);
            grid.CurrentRowKey.ShouldBeNull();
            grid.Selection.ShouldBeSameAs(DataGridSelectionState.Empty);
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
            ImmutableArray<DataGridFilterOperatorSchema>.Empty,
            canGroup: true)],
        preferredRangeSize: 32,
        maximumRangeSize: 32))
    {
        HonorCancellation = true
    };

    private static global::AtomUI.Desktop.Controls.DataGrid Grid(IDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source,
            SelectionMode = DataGridSelectionMode.Extended,
            Width = 360,
            Height = 220
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId = ValueField,
            Header = "Value",
            Binding = new Binding(nameof(Row.Value))
        });
        return grid;
    }

    private static Window Show(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        ControllableDataGridSource source)
    {
        var window = new Window { Width = 420, Height = 280, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        source.WaitForRequestCount(1);
        return window;
    }

    private static void Complete(
        ControllableDataGridSource source,
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int requestIndex,
        string snapshot)
    {
        var request = source.RequestAt(requestIndex).Request;
        source.Complete(requestIndex, ResultFor(request, snapshot));
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
        Dispatcher.UIThread.RunJobs();
    }

    private static void CompleteRequestContaining(
        ControllableDataGridSource source,
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int displayIndex,
        string snapshot)
    {
        ControllableDataGridSource.PendingRequest? pending = null;
        var requestIndex = -1;
        PumpUntil(() =>
        {
            var requests = source.Requests;
            for (var index = 1; index < requests.Count; index++)
            {
                var candidate = requests[index];
                if (displayIndex >= candidate.Request.Range.StartIndex &&
                    displayIndex < candidate.Request.Range.EndExclusive)
                {
                    pending = candidate;
                    requestIndex = index;
                    return true;
                }
            }
            return false;
        });
        source.Complete(requestIndex, ResultFor(pending!.Request, snapshot));
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready &&
                        grid.DisplayData.FirstScrollingSlot <= displayIndex &&
                        grid.DisplayData.LastScrollingSlot >= displayIndex);
        Dispatcher.UIThread.RunJobs();
    }

    private static DataGridRangeResult ResultFor(
        DataGridFetchRequest request,
        string snapshot)
    {
        const int total = 128;
        var count = Math.Min(request.Range.Count, total - request.Range.StartIndex);
        var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
        for (var offset = 0; offset < count; offset++)
        {
            var index = request.Range.StartIndex + offset;
            entries.Add(DataGridSourceEntry.CreateData(
                Key(index + 1L),
                new Row(index),
                index,
                index));
        }
        return new DataGridRangeResult(
            request.Range.StartIndex,
            entries.MoveToImmutable(),
            total,
            total,
            total,
            new DataGridSnapshotId(snapshot));
    }

    private static List<DataGridRow> DisplayedRows(
        global::AtomUI.Desktop.Controls.DataGrid grid) =>
        grid.DisplayData.GetScrollingRows()
            .Cast<DataGridRow>()
            .OrderBy(static row => row.Slot)
            .ToList();

    private static DataGridRow DisplayedRow(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int slot) => DisplayedRows(grid).Single(row => row.Slot == slot);

    private static DataGridRowKey Key(long value) => DataGridRowKey.FromInt64(value);

    private static string GetRangeNavigationState(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        ControllableDataGridSource source)
    {
        var pendingField = typeof(global::AtomUI.Desktop.Controls.DataGrid).GetField(
            "_pendingRangeNavigation",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return $"currentSlot={grid.CurrentSlot} first={grid.DisplayData.FirstScrollingSlot} " +
               $"last={grid.DisplayData.LastScrollingSlot} pending={pendingField?.GetValue(grid)} " +
               $"requests={string.Join(';', source.Requests.Select(static request => request.Request.Range))}";
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

    private sealed class LookupSource : IDataGridKeyLookupSource
    {
        private readonly ControllableDataGridSource _inner;
        private readonly DataGridRowKey _key;
        private readonly DataGridKeyLookupResult _result;

        public LookupSource(
            ControllableDataGridSource inner,
            DataGridRowKey key,
            DataGridKeyLookupResult result)
        {
            _inner = inner;
            _key = key;
            _result = result;
        }

        public DataGridSourceSchema Schema => _inner.Schema;

        public int LookupCount { get; private set; }

        public event EventHandler? Invalidated
        {
            add => _inner.Invalidated += value;
            remove => _inner.Invalidated -= value;
        }

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken) =>
            _inner.FetchAsync(request, cancellationToken);

        public ValueTask<DataGridKeyLookupResult?> LookupAsync(
            DataGridKeyLookupRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LookupCount++;
            return ValueTask.FromResult<DataGridKeyLookupResult?>(
                request.RowKey == _key ? _result : null);
        }
    }
}
