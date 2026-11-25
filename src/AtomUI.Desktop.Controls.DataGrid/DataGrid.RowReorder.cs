using AtomUI.Controls;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    private enum RowReorderSessionState
    {
        Pressed,
        Dragging,
        Completed,
        Cancelled
    }

    private sealed class RowReorderSession
    {
        public RowReorderSession(
            IPointer pointer,
            DataGridRowReorderHandle handle,
            DataGridRow row,
            object? item,
            IDataGridMovableSource source,
            DataGridQuery query,
            DataGridSnapshotId snapshot,
            DataGridRowKey rowKey,
            CancellationToken mutationCancellationToken,
            DataGridRowsPresenter rowsPresenter,
            Point startPosition)
        {
            Pointer       = pointer;
            Handle        = handle;
            Row           = row;
            Item          = item;
            RangeSource   = source;
            RangeQuery    = query;
            RangeSnapshot = snapshot;
            RowKey        = rowKey;
            MutationCancellationToken = mutationCancellationToken;
            RowsPresenter = rowsPresenter;
            StartPosition = startPosition;
            SourceIndex   = row.Index;
            SourceSlot    = row.Slot;
            SourceBounds  = row.Bounds;
        }

        public IPointer Pointer { get; }
        public DataGridRowReorderHandle Handle { get; }
        public DataGridRow Row { get; }
        public object? Item { get; }
        public IDataGridMovableSource? RangeSource { get; }
        public DataGridQuery? RangeQuery { get; }
        public DataGridSnapshotId RangeSnapshot { get; }
        public DataGridRowKey RowKey { get; }
        public CancellationToken MutationCancellationToken { get; }
        public DataGridRowsPresenter RowsPresenter { get; }
        public Point StartPosition { get; }
        public int SourceIndex { get; }
        public int SourceSlot { get; }
        public Rect SourceBounds { get; }
        public Point LastPointerPosition { get; set; }
        public (int FirstSlot, double NegOffset) ResolvedViewport { get; set; }
        public DispatcherTimer? AutoScrollTimer { get; set; }
        public int? TargetIndex { get; set; }
        public DataGridRow? DropTargetRow { get; set; }
        public RowReorderSessionState State { get; set; } = RowReorderSessionState.Pressed;
    }

    // While the pointer rests beyond the top or bottom edge, the rows keep scrolling at a pace proportional to how
    // far beyond the edge it is, capped per tick.
    private static readonly TimeSpan RowReorderAutoScrollInterval = TimeSpan.FromMilliseconds(30);
    private const double RowReorderAutoScrollMaxStep = 20;

    private RowReorderSession? _rowReorderSession;
    private bool _rowReorderRefreshPosted;

    internal bool TryBeginRowReorder(
        DataGridRowReorderHandle handle,
        DataGridRow row,
        IPointer pointer,
        Point startPosition)
    {
        CancelRowReorder();

        if (!IsEnabled ||
            !CanUserReorderRows ||
            !ReferenceEquals(handle.OwningGrid, this) ||
            !ReferenceEquals(handle.OwningRow, row) ||
            !ReferenceEquals(row.OwningGrid, this) ||
            row.Index < 0 ||
            RowsPresenter is not { } rowsPresenter)
        {
            return false;
        }

        var item = row.DataContext;
        if (ItemsSource is not IDataGridMovableSource movableSource ||
            movableSource is IDataGridMoveAvailability { CanMove: false } ||
            !IsRangeMoveSemanticallyAllowed ||
            _rangeGeneration is not { } generation ||
            _rangePresentationIndex is not { } presentation ||
            !row.IsRangeBacked ||
            !TryGetCommittedRangeEntry(row.Slot, out var entry) ||
            entry.Kind != DataGridSourceEntryKind.Data ||
            entry.RowKey != row.RowKey)
        {
            return false;
        }
        var session = new RowReorderSession(
            pointer,
            handle,
            row,
            item,
            movableSource,
            presentation.Snapshot.Query,
            presentation.Snapshot.Snapshot,
            entry.RowKey,
            generation.CancellationToken,
            rowsPresenter,
            startPosition);
        _rowReorderSession = session;
        // The rows presenter owns the capture: the handle lives in a recyclable row, and scrolling that row
        // out of the viewport (wheel or edge auto-scroll) would otherwise detach the capture target mid-drag.
        pointer.Capture(rowsPresenter);
        if (!ReferenceEquals(pointer.Captured, rowsPresenter))
        {
            CancelRowReorder(session);
            return false;
        }

        return true;
    }

    internal void HandleRowReorderPointerMoved(
        IPointer pointer,
        Point pointerPosition,
        bool isLeftButtonPressed)
    {
        if (!TryGetRowReorderSession(pointer, out var session))
        {
            return;
        }
        if (!isLeftButtonPressed || !IsRowReorderSessionValid(session))
        {
            CancelRowReorder(session);
            return;
        }

        if (session.State == RowReorderSessionState.Pressed)
        {
            var distance = new Vector(
                pointerPosition.X - session.StartPosition.X,
                pointerPosition.Y - session.StartPosition.Y);
            if (distance.Length <= Constants.DragThreshold)
            {
                return;
            }
            // RowReordering reports the pressed row, so it must still be the realized source row.
            if (!IsRowReorderSourceRowRealized(session))
            {
                CancelRowReorder(session);
                return;
            }

            session.State = RowReorderSessionState.Dragging;
            var eventArgs = new DataGridRowReorderingEventArgs(session.Row);
            NotifyRowReordering(eventArgs);
            if (eventArgs.Cancel || !IsRowReorderSessionValid(session))
            {
                CancelRowReorder(session);
                return;
            }

            session.RowsPresenter.DraggedRowIndex = session.SourceIndex;
            session.RowsPresenter.NotifyAboutToDragging(session.Item, session.SourceIndex);
        }

        session.LastPointerPosition = pointerPosition;
        UpdateRowReorderDrag(session, pointerPosition, pointerMoved: true);
    }

    // Scrolling (wheel or edge auto-scroll) is applied asynchronously and moves rows under a stationary pointer.
    // Once the presenter has arranged a different viewport than the one the drop target was hit-tested against,
    // the target is re-resolved from the last pointer position; otherwise releasing without moving would drop
    // on a row that is no longer under the pointer.
    internal void NotifyRowsArrangedForRowReorder()
    {
        if (_rowReorderSession is not { State: RowReorderSessionState.Dragging } session ||
            session.ResolvedViewport == GetRowReorderViewport() ||
            _rowReorderRefreshPosted)
        {
            return;
        }

        _rowReorderRefreshPosted = true;
        Dispatcher.UIThread.Post(() =>
        {
            _rowReorderRefreshPosted = false;
            // A scroll applied after this was posted leaves rows not yet re-arranged; hit-testing them now would
            // record the new viewport against stale bounds. The pending arrange posts the refresh again.
            if (ReferenceEquals(_rowReorderSession, session) &&
                session.State == RowReorderSessionState.Dragging &&
                session.RowsPresenter.IsMeasureValid &&
                session.RowsPresenter.IsArrangeValid &&
                session.ResolvedViewport != GetRowReorderViewport())
            {
                UpdateRowReorderDrag(session, session.LastPointerPosition, pointerMoved: false);
            }
        }, DispatcherPriority.Background);
    }

    private (int FirstSlot, double NegOffset) GetRowReorderViewport() =>
        (DisplayData.FirstScrollingSlot, NegVerticalOffset);

    // Only a pointer move positions the ghost row and starts or stops edge auto-scroll; a refresh after scrolling
    // just re-resolves the drop target, leaving the ghost where the pointer put it.
    private void UpdateRowReorderDrag(RowReorderSession session, Point pointerPosition, bool pointerMoved)
    {
        if (session.State != RowReorderSessionState.Dragging ||
            session.RowsPresenter.DraggedRowIndex is null)
        {
            return;
        }

        var resolvedViewport = GetRowReorderViewport();
        var targetIndex = GetRowReorderTarget(pointerPosition);
        if (!ReferenceEquals(_rowReorderSession, session))
        {
            return;
        }
        if (!IsRowReorderSessionValid(session))
        {
            CancelRowReorder(session);
            return;
        }

        session.TargetIndex                    = targetIndex;
        session.ResolvedViewport               = resolvedViewport;
        UpdateRowReorderDropTarget(session);
        if (!pointerMoved)
        {
            return;
        }
        // The ghost follows the pointer but is kept inside the viewport, so the dragged row stays visible while the
        // pointer is beyond an edge and the rows auto-scroll underneath it.
        var dragRowOffset = session.SourceBounds.Y + pointerPosition.Y - session.StartPosition.Y;
        session.RowsPresenter.DragRowOffset = Math.Clamp(
            dragRowOffset,
            0,
            Math.Max(0, CellsEstimatedHeight - session.SourceBounds.Height));
        session.RowsPresenter.DraggedRowIndex = session.SourceIndex;
        session.RowsPresenter.InvalidateArrange();
        UpdateRowReorderAutoScroll(session);
    }

    private void UpdateRowReorderAutoScroll(RowReorderSession session)
    {
        if (GetRowReorderAutoScrollDelta(session.LastPointerPosition) == 0)
        {
            StopRowReorderAutoScroll(session);
            return;
        }
        if (session.AutoScrollTimer is not null)
        {
            return;
        }

        // Scroll right away on crossing the edge; the timer keeps scrolling while the pointer stays there.
        ScrollRowReorderStep(session);
        if (ReferenceEquals(_rowReorderSession, session))
        {
            session.AutoScrollTimer = new DispatcherTimer(
                RowReorderAutoScrollInterval,
                DispatcherPriority.Background,
                (_, _) => ScrollRowReorderStep(session));
        }
    }

    private void ScrollRowReorderStep(RowReorderSession session)
    {
        if (!ReferenceEquals(_rowReorderSession, session) ||
            session.State != RowReorderSessionState.Dragging)
        {
            StopRowReorderAutoScroll(session);
            return;
        }
        if (!IsRowReorderSessionValid(session))
        {
            CancelRowReorder(session);
            return;
        }

        var delta = GetRowReorderAutoScrollDelta(session.LastPointerPosition);
        if (delta == 0)
        {
            StopRowReorderAutoScroll(session);
            return;
        }
        // The scroll is applied asynchronously; NotifyRowsArrangedForRowReorder re-resolves the drop target after it.
        ScrollSlotsByHeight(delta);
    }

    private double GetRowReorderAutoScrollDelta(Point pointerPosition)
    {
        if (VerticalScrollBar is not { IsVisible: true } verticalScrollBar)
        {
            return 0;
        }

        var viewportHeight = CellsEstimatedHeight;
        if (pointerPosition.Y < 0 && verticalScrollBar.Value > 0)
        {
            return Math.Max(
                Math.Max(pointerPosition.Y, -RowReorderAutoScrollMaxStep),
                -verticalScrollBar.Value);
        }
        if (pointerPosition.Y > viewportHeight && verticalScrollBar.Value < verticalScrollBar.Maximum)
        {
            return Math.Min(
                Math.Min(pointerPosition.Y - viewportHeight, RowReorderAutoScrollMaxStep),
                verticalScrollBar.Maximum - verticalScrollBar.Value);
        }
        return 0;
    }

    private static void StopRowReorderAutoScroll(RowReorderSession session)
    {
        session.AutoScrollTimer?.Stop();
        session.AutoScrollTimer = null;
    }

    internal void HandleRowReorderPointerReleased(IPointer pointer)
    {
        if (!TryGetRowReorderSession(pointer, out var session))
        {
            return;
        }

        var targetIndex = session.TargetIndex;
        var shouldCommit = session.State == RowReorderSessionState.Dragging &&
                           targetIndex.HasValue &&
                           targetIndex.Value != session.SourceIndex &&
                           IsRowReorderSessionValid(session);
        var moved = false;
        DataGridMoveRequest? rangeRequest = null;

        _rowReorderSession = null;
        ReleaseRowReorderPointerCapture(session);
        try
        {
            if (shouldCommit)
            {
                moved = TryCreateRangeMoveRequest(
                    session,
                    targetIndex!.Value,
                    out var request);
                rangeRequest = moved ? request : null;
            }
            session.State = moved
                ? RowReorderSessionState.Completed
                : RowReorderSessionState.Cancelled;
        }
        catch
        {
            session.State = RowReorderSessionState.Cancelled;
            throw;
        }
        finally
        {
            CleanupRowReorderSession(session, releasePointerCapture: false);
        }

        if (moved && rangeRequest is { } pendingMove)
        {
            _ = CompleteRangeRowReorderAsync(
                new WeakReference<DataGrid>(this),
                new WeakReference<DataGridRow>(session.Row),
                _rangeMoveContext,
                session.RangeSource!,
                pendingMove,
                session.MutationCancellationToken);
        }
    }

    internal void HandleRowReorderPointerCaptureLost(IPointer pointer)
    {
        if (TryGetRowReorderSession(pointer, out var session))
        {
            CancelRowReorder(session, releasePointerCapture: false);
        }
    }

    internal void CancelRowReorder()
    {
        if (_rowReorderSession is { } session)
        {
            CancelRowReorder(session);
        }
    }

    private bool TryGetRowReorderSession(IPointer pointer, out RowReorderSession session)
    {
        session = _rowReorderSession!;
        return session != null &&
               ReferenceEquals(session.Pointer, pointer) &&
               session.State is RowReorderSessionState.Pressed or RowReorderSessionState.Dragging;
    }

    // The session is identified by the source row's data (key at its committed slot), not by the row control:
    // virtualization recycles the control once the row scrolls out of the viewport while the drag continues.
    private bool IsRowReorderSessionValid(RowReorderSession session)
    {
        return ReferenceEquals(_rowReorderSession, session) &&
               IsEnabled &&
               CanUserReorderRows &&
               ReferenceEquals(RowsPresenter, session.RowsPresenter) &&
               IsRangeMoveSemanticallyAllowed &&
               ReferenceEquals(ItemsSource, session.RangeSource) &&
               _rangePresentationIndex is { } presentation &&
               presentation.Snapshot.Query == session.RangeQuery &&
               presentation.Snapshot.Snapshot == session.RangeSnapshot &&
               TryGetCommittedRangeEntry(session.SourceSlot, out var entry) &&
               entry.Kind == DataGridSourceEntryKind.Data &&
               entry.RowKey == session.RowKey;
    }

    private bool IsRowReorderSourceRowRealized(RowReorderSession session)
    {
        return ReferenceEquals(session.Handle.OwningGrid, this) &&
               ReferenceEquals(session.Handle.OwningRow, session.Row) &&
               ReferenceEquals(session.Row.OwningGrid, this) &&
               session.Row.Slot == session.SourceSlot &&
               session.Row.RowKey == session.RowKey &&
               IsSlotVisible(session.SourceSlot) &&
               ReferenceEquals(DisplayData.GetDisplayedElement(session.SourceSlot), session.Row);
    }

    private bool TryCreateRangeMoveRequest(
        RowReorderSession session,
        int targetIndex,
        out DataGridMoveRequest request)
    {
        if (!TryGetCommittedRangeEntry(targetIndex, out var target) ||
            target.Kind != DataGridSourceEntryKind.Data ||
            target.RowKey == session.RowKey)
        {
            request = default;
            return false;
        }
        request = targetIndex < session.SourceIndex
            ? new DataGridMoveRequest(
                session.RangeQuery!,
                session.RangeSnapshot,
                session.RowKey,
                beforeKey: target.RowKey,
                afterKey: null)
            : new DataGridMoveRequest(
                session.RangeQuery!,
                session.RangeSnapshot,
                session.RowKey,
                beforeKey: null,
                afterKey: target.RowKey);
        return true;
    }

    private static async Task CompleteRangeRowReorderAsync(
        WeakReference<DataGrid> ownerReference,
        WeakReference<DataGridRow> rowReference,
        AsyncLocal<DataGridMoveRequest?> moveContext,
        IDataGridMovableSource source,
        DataGridMoveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await ExecuteRangeMoveAsync(moveContext, source, request, cancellationToken).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!ownerReference.TryGetTarget(out var owner) ||
                    !owner.CompleteRangeMove(source, request, result))
                {
                    return;
                }
                if (rowReference.TryGetTarget(out var row))
                {
                    owner.NotifyRowReordered(new DataGridRowEventArgs(row));
                }
            });
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ownerReference.TryGetTarget(out var owner) &&
                    ReferenceEquals(owner.ItemsSource, source))
                {
                    owner.SetRangeError(exception);
                }
            });
        }
    }

    private void CancelRowReorder(RowReorderSession session, bool releasePointerCapture = true)
    {
        if (session.State is RowReorderSessionState.Completed or RowReorderSessionState.Cancelled &&
            !ReferenceEquals(_rowReorderSession, session))
        {
            return;
        }

        session.State = RowReorderSessionState.Cancelled;
        if (ReferenceEquals(_rowReorderSession, session))
        {
            _rowReorderSession = null;
        }
        CleanupRowReorderSession(session, releasePointerCapture);
    }

    private static void CleanupRowReorderSession(RowReorderSession session, bool releasePointerCapture)
    {
        var hadDragVisual = session.RowsPresenter.DraggedRowIndex is not null;
        StopRowReorderAutoScroll(session);
        using var suppression = session.Handle.SuppressIndicatorTransitions();
        if (releasePointerCapture)
        {
            ReleaseRowReorderPointerCapture(session);
        }

        SetRowReorderDropTarget(session, null, DataGridRowDropPosition.None);
        session.RowsPresenter.NotifyDropped();
        session.RowsPresenter.DraggedRowIndex = null;
        session.RowsPresenter.DragRowOffset   = 0;
        if (hadDragVisual)
        {
            session.RowsPresenter.InvalidateArrange();
        }
    }

    private void UpdateRowReorderDropTarget(RowReorderSession session)
    {
        DataGridRow? targetRow = null;
        var position = DataGridRowDropPosition.None;
        if (session.TargetIndex is { } targetIndex && targetIndex != session.SourceIndex)
        {
            var displayedElementCount = DisplayData.NumDisplayedScrollingElements;
            for (var displayIndex = 0; displayIndex < displayedElementCount; displayIndex++)
            {
                if (DisplayData.GetScrollingElementAtDisplayIndex(displayIndex) is DataGridRow row &&
                    row.Index == targetIndex)
                {
                    targetRow = row;
                    // Matches TryCreateRangeMoveRequest: moving up inserts before the target, moving down after it.
                    position = targetIndex < session.SourceIndex
                        ? DataGridRowDropPosition.Before
                        : DataGridRowDropPosition.After;
                    break;
                }
            }
        }
        SetRowReorderDropTarget(session, targetRow, position);
    }

    private static void SetRowReorderDropTarget(
        RowReorderSession session,
        DataGridRow? targetRow,
        DataGridRowDropPosition position)
    {
        if (session.DropTargetRow is { } previous && !ReferenceEquals(previous, targetRow))
        {
            previous.DropPosition = DataGridRowDropPosition.None;
        }
        session.DropTargetRow = targetRow;
        if (targetRow is not null)
        {
            targetRow.DropPosition = position;
        }
    }

    private static void ReleaseRowReorderPointerCapture(RowReorderSession session)
    {
        if (ReferenceEquals(session.Pointer.Captured, session.RowsPresenter))
        {
            session.Pointer.Capture(null);
        }
    }

    private int? GetRowReorderTarget(Point pointerPosition)
    {
        if (RowsPresenter is not { } rowsPresenter)
        {
            return null;
        }

        var topEdge    = 0d;
        var bottomEdge = rowsPresenter.DesiredSize.Height;

        if (pointerPosition.Y < topEdge)
        {
            pointerPosition = pointerPosition.WithY(topEdge);
        }
        else if (pointerPosition.Y >= bottomEdge)
        {
            pointerPosition = pointerPosition.WithY(Math.Max(topEdge, bottomEdge - 1));
        }

        var displayedElementCount = DisplayData.NumDisplayedScrollingElements;
        for (var displayIndex = 0; displayIndex < displayedElementCount; displayIndex++)
        {
            if (DisplayData.GetScrollingElementAtDisplayIndex(displayIndex) is DataGridRow row)
            {
                var rowPosition = rowsPresenter.Translate(row, pointerPosition);
                if (rowPosition.Y >= 0 && rowPosition.Y <= row.Bounds.Height)
                {
                    return row.Index;
                }
            }
        }

        return null;
    }
}
