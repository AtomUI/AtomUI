using System.Collections.Immutable;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Interaction;

public class DataGridRangeMutationTests
{
    private static readonly DataGridFieldId ValueField = new("value");

    static DataGridRangeMutationTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Range_Row_Reorder_Commits_By_Stable_Keys_And_Cleans_Drag_State_First()
    {
        var source = new MovableSource(4);
        var (grid, window) = Realize(source);
        var reorderedCount = 0;
        var cleanupObserved = false;
        grid.RowReordered += (_, _) =>
        {
            reorderedCount++;
            cleanupObserved = grid.RowsPresenter!.DraggedRowIndex is null;
        };

        try
        {
            EnsureFourRows(grid);
            var handle = grid.GetVisualDescendants()
                             .OfType<DataGridRowReorderHandle>()
                             .Single(candidate => candidate.OwningRow!.Index == 0);
            var drag = BeginPointerDrag(handle, window);
            var target = MovePointerDragToRow(grid, handle, window, drag, 2);

            EndPointerDrag(handle, window, drag, target);
            PumpUntil(() => source.MoveCount == 1 && reorderedCount == 1);

            var request = source.LastMove.ShouldNotBeNull();
            request.RowKey.ShouldBe(Key(1));
            request.BeforeKey.ShouldBeNull();
            request.AfterKey.ShouldBe(Key(3));
            request.Query.ShouldBeSameAs(DataGridQuery.Empty);
            request.Snapshot.ShouldBe(new DataGridSnapshotId("s1"));
            source.Order.ShouldBe([2, 3, 1, 4]);
            drag.Pointer.Captured.ShouldBeNull();
            cleanupObserved.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Range_Row_Reorder_Does_Not_Start_When_Query_Changes_Order()
    {
        var source = new MovableSource(4);
        var query = new DataGridQuery(
            [new DataGridSort(ValueField, DataGridSortDirection.Ascending)],
            [],
            []);
        var (grid, window) = Realize(source, query);
        try
        {
            EnsureFourRows(grid);
            var handle = grid.GetVisualDescendants()
                             .OfType<DataGridRowReorderHandle>()
                             .First();
            var drag = BeginPointerDrag(handle, window);

            drag.Pointer.Captured.ShouldBeNull();
            source.MoveCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Source_Invalidation_Cancels_Range_Drag_And_Releases_Pointer_Capture()
    {
        var source = new MovableSource(4);
        var (grid, window) = Realize(source);
        try
        {
            EnsureFourRows(grid);
            var handle = grid.GetVisualDescendants()
                             .OfType<DataGridRowReorderHandle>()
                             .First();
            var drag = BeginPointerDrag(handle, window);
            MovePointerDragToRow(grid, handle, window, drag, 2);
            drag.Pointer.Captured.ShouldBeSameAs(handle);

            source.Invalidate();
            Dispatcher.UIThread.RunJobs();

            drag.Pointer.Captured.ShouldBeNull();
            grid.RowsPresenter!.DraggedRowIndex.ShouldBeNull();
            source.MoveCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Two_Range_Grids_Keep_Row_Drag_Sessions_Isolated()
    {
        var firstSource = new MovableSource(4);
        var secondSource = new MovableSource(4);
        var (firstGrid, firstWindow) = Realize(firstSource);
        var (secondGrid, secondWindow) = Realize(secondSource);
        try
        {
            EnsureFourRows(firstGrid);
            EnsureFourRows(secondGrid);
            var firstHandle = GetHandle(firstGrid);
            var secondHandle = GetHandle(secondGrid);
            var firstDrag = BeginPointerDrag(firstHandle, firstWindow);
            var firstTarget = MovePointerDragToRow(firstGrid, firstHandle, firstWindow, firstDrag, 2);
            var secondDrag = BeginPointerDrag(secondHandle, secondWindow);
            var secondTarget = MovePointerDragToRow(secondGrid, secondHandle, secondWindow, secondDrag, 1);

            EndPointerDrag(firstHandle, firstWindow, firstDrag, firstTarget);
            EndPointerDrag(secondHandle, secondWindow, secondDrag, secondTarget);
            PumpUntil(() => firstSource.MoveCount == 1 && secondSource.MoveCount == 1);

            firstSource.Order.ShouldBe([2, 3, 1, 4]);
            secondSource.Order.ShouldBe([2, 1, 3, 4]);
        }
        finally
        {
            firstWindow.Close();
            secondWindow.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Range_Row_Reorder_Starts_Only_After_Drag_Threshold_And_Once_Per_Pointer()
    {
        var source = new MovableSource(4);
        var (grid, window) = Realize(source);
        var reorderingCount = 0;
        grid.RowReordering += (_, _) => reorderingCount++;
        try
        {
            EnsureFourRows(grid);
            var handle = GetHandle(grid);
            var drag = BeginPointerDrag(handle, window);

            MovePointerDrag(handle, window, drag, drag.Start + new Vector(Constants.DragThreshold, 0), 1);
            reorderingCount.ShouldBe(0);

            MovePointerDragToRow(grid, handle, window, drag, 1);
            MovePointerDragToRow(grid, handle, window, drag, 2);
            reorderingCount.ShouldBe(1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Cancelling_Range_RowReordering_Cleans_Session_Without_Source_Call()
    {
        var source = new MovableSource(4);
        var (grid, window) = Realize(source);
        grid.RowReordering += (_, args) => args.Cancel = true;
        try
        {
            EnsureFourRows(grid);
            var handle = GetHandle(grid);
            var drag = BeginPointerDrag(handle, window);
            var target = MovePointerDragToRow(grid, handle, window, drag, 2);
            EndPointerDrag(handle, window, drag, target);

            source.MoveCount.ShouldBe(0);
            source.Order.ShouldBe([1, 2, 3, 4]);
            drag.Pointer.Captured.ShouldBeNull();
            grid.RowsPresenter!.DraggedRowIndex.ShouldBeNull();
            grid.RowsPresenter.DragRowOffset.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Releasing_Over_Source_Row_Does_Not_Commit_Range_Move()
    {
        var source = new MovableSource(4);
        var (grid, window) = Realize(source);
        var reorderedCount = 0;
        grid.RowReordered += (_, _) => reorderedCount++;
        try
        {
            EnsureFourRows(grid);
            var handle = GetHandle(grid);
            var drag = BeginPointerDrag(handle, window);
            var target = drag.Start + new Vector(Constants.DragThreshold + 1, 0);
            MovePointerDrag(handle, window, drag, target, 1);
            EndPointerDrag(handle, window, drag, target);

            source.MoveCount.ShouldBe(0);
            reorderedCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData(DragCancellationKind.PointerCapture)]
    [InlineData(DragCancellationKind.GridDisabled)]
    [InlineData(DragCancellationKind.ReorderDisabled)]
    [InlineData(DragCancellationKind.ColumnRemoved)]
    [InlineData(DragCancellationKind.Detached)]
    public void Range_Drag_Owner_Changes_Release_Capture_And_State(DragCancellationKind cancellation)
    {
        var source = new MovableSource(4);
        var (grid, window) = Realize(source);
        try
        {
            EnsureFourRows(grid);
            var handle = GetHandle(grid);
            var drag = BeginPointerDrag(handle, window);
            MovePointerDragToRow(grid, handle, window, drag, 2);

            switch (cancellation)
            {
                case DragCancellationKind.PointerCapture:
                    drag.Pointer.Capture(null);
                    break;
                case DragCancellationKind.GridDisabled:
                    grid.IsEnabled = false;
                    break;
                case DragCancellationKind.ReorderDisabled:
                    grid.CanUserReorderRows = false;
                    break;
                case DragCancellationKind.ColumnRemoved:
                    grid.Columns.RemoveAt(0);
                    break;
                case DragCancellationKind.Detached:
                    window.Content = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cancellation));
            }
            Dispatcher.UIThread.RunJobs();

            drag.Pointer.Captured.ShouldBeNull();
            grid.RowsPresenter!.DraggedRowIndex.ShouldBeNull();
            grid.RowsPresenter.DragRowOffset.ShouldBe(0);
            source.MoveCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Replacing_Range_Source_Cancels_Drag_Before_Source_Property_Changes()
    {
        var source = new MovableSource(4);
        var replacement = new MovableSource(2);
        var (grid, window) = Realize(source);
        PointerDrag? drag = null;
        bool? cleanAtChange = null;
        grid.PropertyChanged += (_, change) =>
        {
            if (change.Property == global::AtomUI.Desktop.Controls.DataGrid.ItemsSourceProperty)
            {
                cleanAtChange = drag?.Pointer.Captured is null &&
                                grid.RowsPresenter!.DraggedRowIndex is null &&
                                grid.RowsPresenter.DragRowOffset == 0;
            }
        };
        try
        {
            EnsureFourRows(grid);
            var handle = GetHandle(grid);
            drag = BeginPointerDrag(handle, window);
            MovePointerDragToRow(grid, handle, window, drag, 2);

            grid.ItemsSource = replacement;
            Dispatcher.UIThread.RunJobs();

            cleanAtChange.ShouldBe(true);
            source.MoveCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Source_Invalidation_Cancels_Awaiting_Range_Move_And_Suppresses_Completion_Event()
    {
        var source = new MovableSource(4) { DelayMoves = true };
        var (grid, window) = Realize(source);
        var reorderedCount = 0;
        grid.RowReordered += (_, _) => reorderedCount++;
        try
        {
            EnsureFourRows(grid);
            var handle = grid.GetVisualDescendants()
                             .OfType<DataGridRowReorderHandle>()
                             .Single(candidate => candidate.OwningRow!.Index == 0);
            var drag = BeginPointerDrag(handle, window);
            var target = MovePointerDragToRow(grid, handle, window, drag, 2);
            EndPointerDrag(handle, window, drag, target);
            PumpUntil(() => source.PendingMoveCount == 1);

            source.Invalidate();
            PumpUntil(() => source.CancelledMoveCount == 1);

            source.MoveCount.ShouldBe(0);
            reorderedCount.ShouldBe(0);
            grid.RowsPresenter!.DraggedRowIndex.ShouldBeNull();
        }
        finally
        {
            source.ReleaseMoves();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Mutation_Contracts_Reject_View_Indexes_And_Invalid_Identity()
    {
        var snapshot = new DataGridSnapshotId("s1");

        Should.Throw<ArgumentException>(() => new DataGridMoveRequest(
            DataGridQuery.Empty, snapshot, default, Key(2), null));
        Should.Throw<ArgumentException>(() => new DataGridMoveRequest(
            DataGridQuery.Empty, snapshot, Key(1), null, null));
        Should.Throw<ArgumentException>(() => new DataGridMoveRequest(
            DataGridQuery.Empty, snapshot, Key(1), Key(2), Key(3)));
        Should.Throw<ArgumentException>(() => new DataGridMoveRequest(
            DataGridQuery.Empty, snapshot, Key(1), Key(1), null));
    }

    [Fact]
    public void Range_BeginEdit_Does_Not_Fall_Through_To_Legacy_Editable_Object_Owner()
    {
        var source = new CapabilitySource(4);
        var (grid, window) = Realize(source);
        try
        {
            EnsureFourRows(grid);
            grid.SelectionMode = DataGridSelectionMode.Extended;
            grid.UpdateSelectionAndCurrency(
                    1,
                    0,
                    DataGridSelectionAction.SelectCurrent,
                    scrollIntoView: false)
                .ShouldBeTrue();

            grid.BeginEdit(new RoutedEventArgs()).ShouldBeFalse();
            source.Rows[0].BeginEditCount.ShouldBe(0);
            grid.EditingRow.ShouldBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public async Task Range_Mutation_APIs_Use_Applied_Query_Snapshot_Keys_And_Selection()
    {
        var source = new CapabilitySource(4);
        var (grid, window) = Realize(source);
        try
        {
            EnsureFourRows(grid);
            grid.SelectionMode = DataGridSelectionMode.Extended;
            grid.Selection = new DataGridSelectionState(
                [Key(1)],
                null,
                [],
                []);
            var values = ImmutableDictionary<DataGridFieldId, DataGridScalar>.Empty
                .Add(ValueField, DataGridScalar.FromInt64(42));

            (await grid.EditRowAsync(
                Key(1), values, TestContext.Current.CancellationToken)).ShouldBeTrue();
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            (await grid.DeleteRowAsync(
                Key(2), TestContext.Current.CancellationToken)).ShouldBeTrue();
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            (await grid.AddRowAsync(
                values, TestContext.Current.CancellationToken)).ShouldBeTrue();
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
            (await grid.ExecuteBulkSelectionAsync(
                "archive", TestContext.Current.CancellationToken)).ShouldBeTrue();

            source.LastEdit.ShouldNotBeNull().RowKey.ShouldBe(Key(1));
            source.LastEdit.Value.Query.ShouldBeSameAs(DataGridQuery.Empty);
            source.LastEdit.Value.Snapshot.ShouldBe(new DataGridSnapshotId("s1"));
            source.LastEdit.Value.Values[ValueField].ShouldBe(DataGridScalar.FromInt64(42));
            source.LastDelete.ShouldNotBeNull().RowKey.ShouldBe(Key(2));
            source.LastAdd.ShouldNotBeNull().Values[ValueField]
                  .ShouldBe(DataGridScalar.FromInt64(42));
            source.LastBulk.ShouldNotBeNull().Operation.ShouldBe("archive");
            source.LastBulk.Value.Selection.ExplicitKeys.ShouldBe([Key(1)]);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public async Task Invalidation_Cancels_Pending_Mutation_And_Prevents_Stale_Completion()
    {
        var source = new CapabilitySource(4) { DelayMutations = true };
        var (grid, window) = Realize(source);
        try
        {
            EnsureFourRows(grid);
            var pending = grid.DeleteRowAsync(
                Key(1), TestContext.Current.CancellationToken).AsTask();
            PumpUntil(() => source.PendingMutationCount == 1);

            source.Invalidate();
            PumpUntil(() => source.CancelledMutationCount == 1);

            (await pending).ShouldBeFalse();
            source.DeleteCount.ShouldBe(0);
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public async Task Unsupported_Or_Stale_Range_Mutations_Are_Disabled_Without_Source_Calls()
    {
        var readOnlySource = new MovableSource(4);
        var (readOnlyGrid, readOnlyWindow) = Realize(readOnlySource);
        try
        {
            EnsureFourRows(readOnlyGrid);
            (await readOnlyGrid.DeleteRowAsync(
                Key(1), TestContext.Current.CancellationToken)).ShouldBeFalse();
            (await readOnlyGrid.ExecuteBulkSelectionAsync(
                "archive", TestContext.Current.CancellationToken)).ShouldBeFalse();
        }
        finally
        {
            readOnlyWindow.Close();
            Dispatcher.UIThread.RunJobs();
        }

        var source = new CapabilitySource(4);
        var (grid, window) = Realize(source);
        try
        {
            EnsureFourRows(grid);
            source.DelayFetches = true;
            source.Invalidate();
            PumpUntil(() => grid.IsDataStale && grid.LoadState == DataGridLoadState.Refreshing);

            (await grid.DeleteRowAsync(
                Key(1), TestContext.Current.CancellationToken)).ShouldBeFalse();
            source.DeleteCount.ShouldBe(0);
            source.ReleaseFetches();
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static (
        global::AtomUI.Desktop.Controls.DataGrid Grid,
        Window Window) Realize(
        IDataGridSource source,
        DataGridQuery? query = null)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            CanUserReorderRows = true,
            ItemsSource = source,
            Query = query ?? DataGridQuery.Empty,
            Width = 400,
            Height = 260
        };
        grid.Columns.Add(new DataGridRowReorderColumn());
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId = ValueField,
            Header = "Value",
            Binding = new Binding(nameof(Row.Value))
        });
        var window = new Window { Width = 480, Height = 320, Content = grid };
        window.Show();
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
        return (grid, window);
    }

    private static void EnsureFourRows(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        grid.RequestRangeViewport(0, 4, 0);
        PumpUntil(() => grid.DisplayData.GetScrollingRows().Cast<DataGridRow>().Count() == 4);
    }

    private static DataGridRowReorderHandle GetHandle(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int rowIndex = 0) =>
        grid.GetVisualDescendants()
            .OfType<DataGridRowReorderHandle>()
            .Single(candidate => candidate.OwningRow!.Index == rowIndex);

    private static PointerDrag BeginPointerDrag(DataGridRowReorderHandle handle, Visual root)
    {
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
        var start = GetCenterInRoot(handle, root);
        handle.RaiseEvent(new PointerPressedEventArgs(
            handle,
            pointer,
            root,
            start,
            0,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
        return new PointerDrag(pointer, start);
    }

    private static Point MovePointerDragToRow(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        DataGridRowReorderHandle handle,
        Visual root,
        PointerDrag drag,
        int targetIndex)
    {
        var row = grid.GetVisualDescendants()
                      .OfType<DataGridRow>()
                      .Single(candidate => candidate.Index == targetIndex && !candidate.IsDragging);
        var target = GetCenterInRoot(row, root);
        handle.RaiseEvent(new PointerEventArgs(
            InputElement.PointerMovedEvent,
            handle,
            drag.Pointer,
            root,
            target,
            1,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.Other),
            KeyModifiers.None));
        return target;
    }

    private static void MovePointerDrag(
        DataGridRowReorderHandle handle,
        Visual root,
        PointerDrag drag,
        Point target,
        ulong timestamp) =>
        handle.RaiseEvent(new PointerEventArgs(
            InputElement.PointerMovedEvent,
            handle,
            drag.Pointer,
            root,
            target,
            timestamp,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.Other),
            KeyModifiers.None));

    private static void EndPointerDrag(
        DataGridRowReorderHandle handle,
        Visual root,
        PointerDrag drag,
        Point position) =>
        handle.RaiseEvent(new PointerReleasedEventArgs(
            handle,
            drag.Pointer,
            root,
            position,
            2,
            new PointerPointProperties(
                RawInputModifiers.None,
                PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));

    private static Point GetCenterInRoot(Control control, Visual root) =>
        control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            root).ShouldNotBeNull();

    private static DataGridRowKey Key(long value) => DataGridRowKey.FromInt64(value);

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

    private sealed record PointerDrag(Pointer Pointer, Point Start);

    public enum DragCancellationKind
    {
        PointerCapture,
        GridDisabled,
        ReorderDisabled,
        ColumnRemoved,
        Detached
    }

    private sealed class Row : System.ComponentModel.IEditableObject
    {
        public Row(int id, int value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }

        public int Value { get; }

        public int BeginEditCount { get; private set; }

        public void BeginEdit() => BeginEditCount++;

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
        }
    }

    private sealed class MovableSource : IDataGridMovableSource
    {
        private readonly List<Row> _rows;
        private readonly TaskCompletionSource _moveGate = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private int _snapshotVersion = 1;

        public MovableSource(int count)
        {
            _rows = Enumerable.Range(1, count)
                              .Select(value => new Row(value, value))
                              .ToList();
        }

        public DataGridSourceSchema Schema { get; } = new(
            typeof(Row),
            [new DataGridFieldSchema(
                ValueField,
                typeof(int),
                DataGridSortDirections.All,
                ImmutableArray<DataGridFilterOperatorSchema>.Empty,
                canGroup: false)],
            preferredRangeSize: 32,
            maximumRangeSize: 32);

        public int MoveCount { get; private set; }

        public bool DelayMoves { get; init; }

        public int PendingMoveCount { get; private set; }

        public int CancelledMoveCount { get; private set; }

        public DataGridMoveRequest? LastMove { get; private set; }

        public int[] Order => _rows.Select(static row => row.Id).ToArray();

        public event EventHandler? Invalidated;

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = Math.Min(request.Range.Count, _rows.Count - request.Range.StartIndex);
            var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
            for (var offset = 0; offset < count; offset++)
            {
                var index = request.Range.StartIndex + offset;
                var row = _rows[index];
                entries.Add(DataGridSourceEntry.CreateData(
                    Key(row.Id), row, index, index));
            }
            return ValueTask.FromResult(new DataGridRangeResult(
                request.Range.StartIndex,
                entries.MoveToImmutable(),
                _rows.Count,
                _rows.Count,
                _rows.Count,
                new DataGridSnapshotId($"s{_snapshotVersion}")));
        }

        public async ValueTask<DataGridMutationResult> MoveAsync(
            DataGridMoveRequest request,
            CancellationToken cancellationToken)
        {
            if (DelayMoves)
            {
                PendingMoveCount++;
                try
                {
                    await _moveGate.Task.WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    CancelledMoveCount++;
                    throw;
                }
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            MoveCount++;
            LastMove = request;
            var sourceIndex = _rows.FindIndex(row => Key(row.Id) == request.RowKey);
            var row = _rows[sourceIndex];
            _rows.RemoveAt(sourceIndex);
            var targetKey = request.BeforeKey ?? request.AfterKey!.Value;
            var targetIndex = _rows.FindIndex(item => Key(item.Id) == targetKey);
            if (request.AfterKey.HasValue)
            {
                targetIndex++;
            }
            _rows.Insert(targetIndex, row);
            _snapshotVersion++;
            return new DataGridMutationResult(new DataGridSnapshotId($"s{_snapshotVersion}"));
        }

        public void ReleaseMoves() => _moveGate.TrySetResult();

        public void Invalidate()
        {
            _snapshotVersion++;
            Invalidated?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class CapabilitySource : IDataGridEditableSource, IDataGridBulkSelectionSource
    {
        private readonly List<Row> _rows;
        private readonly TaskCompletionSource _fetchGate = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private int _snapshotVersion = 1;

        public CapabilitySource(int count)
        {
            _rows = Enumerable.Range(1, count)
                              .Select(value => new Row(value, value))
                              .ToList();
        }

        public DataGridSourceSchema Schema { get; } = new(
            typeof(Row),
            [new DataGridFieldSchema(
                ValueField,
                typeof(int),
                DataGridSortDirections.All,
                ImmutableArray<DataGridFilterOperatorSchema>.Empty,
                canGroup: false)],
            preferredRangeSize: 32,
            maximumRangeSize: 32);

        public bool DelayMutations { get; init; }

        public bool DelayFetches { get; set; }

        public int PendingMutationCount { get; private set; }

        public int CancelledMutationCount { get; private set; }

        public IReadOnlyList<Row> Rows => _rows;

        public int DeleteCount { get; private set; }

        public DataGridEditRequest? LastEdit { get; private set; }

        public DataGridRowMutationRequest? LastDelete { get; private set; }

        public DataGridAddRequest? LastAdd { get; private set; }

        public DataGridBulkSelectionRequest? LastBulk { get; private set; }

        public event EventHandler? Invalidated;

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return DelayFetches
                ? new ValueTask<DataGridRangeResult>(FetchAfterGateAsync(request, cancellationToken))
                : ValueTask.FromResult(CreateRangeResult(request));
        }

        public void ReleaseFetches() => _fetchGate.TrySetResult();

        private async Task<DataGridRangeResult> FetchAfterGateAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken)
        {
            await _fetchGate.Task.WaitAsync(cancellationToken);
            return CreateRangeResult(request);
        }

        private DataGridRangeResult CreateRangeResult(DataGridFetchRequest request)
        {
            var count = Math.Min(request.Range.Count, _rows.Count - request.Range.StartIndex);
            var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
            for (var offset = 0; offset < count; offset++)
            {
                var index = request.Range.StartIndex + offset;
                var row = _rows[index];
                entries.Add(DataGridSourceEntry.CreateData(Key(row.Id), row, index, index));
            }
            return new DataGridRangeResult(
                request.Range.StartIndex,
                entries.MoveToImmutable(),
                _rows.Count,
                _rows.Count,
                _rows.Count,
                new DataGridSnapshotId($"s{_snapshotVersion}"));
        }

        public async ValueTask<DataGridMutationResult> CommitAsync(
            DataGridEditRequest request,
            CancellationToken cancellationToken)
        {
            await WaitForMutationAsync(cancellationToken);
            LastEdit = request;
            return AdvanceSnapshot();
        }

        public ValueTask<DataGridMutationResult> CancelAsync(
            DataGridRowMutationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(new DataGridMutationResult(null));

        public async ValueTask<DataGridMutationResult> AddAsync(
            DataGridAddRequest request,
            CancellationToken cancellationToken)
        {
            await WaitForMutationAsync(cancellationToken);
            LastAdd = request;
            return AdvanceSnapshot();
        }

        public async ValueTask<DataGridMutationResult> DeleteAsync(
            DataGridRowMutationRequest request,
            CancellationToken cancellationToken)
        {
            await WaitForMutationAsync(cancellationToken);
            LastDelete = request;
            DeleteCount++;
            return AdvanceSnapshot();
        }

        public async ValueTask<DataGridMutationResult> ExecuteAsync(
            DataGridBulkSelectionRequest request,
            CancellationToken cancellationToken)
        {
            await WaitForMutationAsync(cancellationToken);
            LastBulk = request;
            return AdvanceSnapshot();
        }

        public void Invalidate()
        {
            _snapshotVersion++;
            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        private async Task WaitForMutationAsync(CancellationToken cancellationToken)
        {
            if (!DelayMutations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return;
            }
            PendingMutationCount++;
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                CancelledMutationCount++;
                throw;
            }
        }

        private DataGridMutationResult AdvanceSnapshot()
        {
            _snapshotVersion++;
            return new DataGridMutationResult(new DataGridSnapshotId($"s{_snapshotVersion}"));
        }
    }
}
