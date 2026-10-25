// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Used within the template of a <see cref="T:AtomUI.Desktop.Controls.DataGrid" /> to specify the
/// location in the control's visual tree where the rows are to be added.
/// </summary>
public sealed class DataGridRowsPresenter : Panel, IChildIndexProvider
{
    #region 内部属性定义
    
    internal int? DraggedRowIndex { get; set; }
    
    internal Double DragRowOffset { get; set; }
    
    internal DataGrid? OwningGrid { get; set; }
    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;

    #endregion
    
    private DataGridRow? _dragIndicator;
    private RectangleGeometry? _clipGeometry;

    static DataGridRowsPresenter()
    {
        InputElement.ScrollGestureEvent.AddClassHandler<DataGridRowsPresenter>((x, e) => x.OnScrollGesture(e));
    }

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add => _childIndexChanged += value;
        remove => _childIndexChanged -= value;
    }

    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        if (child is not DataGridRow row)
        {
            throw new InvalidOperationException("Group headers are not data children.");
        }
        return row.Index;
    }

    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        if (OwningGrid is null)
        {
            count = 0;
            return false;
        }

        if (OwningGrid.IsRangePresentationActive)
        {
            count = OwningGrid.RangeWindowDataCount;
            return true;
        }

        return OwningGrid.RangeDataAccess.TryGetCount(false, true, out count);
    }

    internal void InvalidateChildIndex(DataGridRow row)
    {
        _childIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(row, row.Index));
    }

    /// <summary>
    /// Arranges the content of the <see cref="T:AtomUI.Desktop.Controls.DataGridRowsPresenter" />.
    /// </summary>
    /// <returns>
    /// The actual size used by the <see cref="T:AtomUI.Desktop.Controls.DataGridRowsPresenter" />.
    /// </returns>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (finalSize.Height == 0 || OwningGrid == null)
        {
            return base.ArrangeOverride(finalSize);
        }
        Debug.Assert(OwningGrid.ColumnsInternal.FillerColumn is not null);

        OwningGrid.HandleFillerColumnWidthNeeded(finalSize.Width);

        double rowDesiredWidth = OwningGrid.RowHeadersDesiredWidth + OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth + OwningGrid.ColumnsInternal.FillerColumn.FillerWidth;
        double topEdge = -OwningGrid.NegVerticalOffset;

        var displayData           = OwningGrid.DisplayData;
        int displayedElementCount = displayData.NumDisplayedScrollingElements;
        for (int i = 0; i < displayedElementCount; i++)
        {
            topEdge = ArrangeElement(
                displayData.GetScrollingElementAtDisplayIndex(i),
                rowDesiredWidth,
                topEdge);
        }

        if (_dragIndicator != null)
        {
            topEdge = ArrangeElement(_dragIndicator, rowDesiredWidth, topEdge);
        }

        double finalHeight = Math.Max(topEdge + OwningGrid.NegVerticalOffset, finalSize.Height);

        // Clip the RowsPresenter so rows cannot overlap other elements in certain styling scenarios
        UpdateClipGeometry(new Rect(0, 0, finalSize.Width, finalHeight));

        // Runtime row layout can outlive the renderer's pending scene notification.
        // Coalesce a refresh after this layout has reached the compositor.
        PointerOverRefresh.Request(this);

        return new Size(finalSize.Width, finalHeight);
    }

    private double ArrangeElement(Control element, double rowDesiredWidth, double topEdge)
    {
        Debug.Assert(OwningGrid != null);

        double elementHeight = OwningGrid.GetDisplayedElementHeight(element);
        if (element is DataGridRow row)
        {
            Debug.Assert(row.Index != -1); // A displayed row should always have its index

            // Visibility for all filler cells needs to be set in one place. Setting it individually in
            // each CellsPresenter causes an NxN layout cycle (see DevDiv Bugs 211557).
            row.EnsureFillerVisibility();
            if (_dragIndicator == row)
            {
                row.Arrange(new Rect(0, DragRowOffset, rowDesiredWidth, elementHeight));
            }
            else
            {
                row.Arrange(new Rect(0, topEdge, rowDesiredWidth, elementHeight));
            }
        }
        else if (element is DataGridRowGroupHeader groupHeader)
        {
            double leftEdge = OwningGrid.IsRowGroupHeadersFrozen ? 0 : -OwningGrid.HorizontalOffset;
            groupHeader.Arrange(new Rect(leftEdge, topEdge, rowDesiredWidth - leftEdge, elementHeight));
        }

        return topEdge + elementHeight;
    }

    private void UpdateClipGeometry(Rect clipRect)
    {
        if (_clipGeometry is null)
        {
            _clipGeometry = new RectangleGeometry
            {
                Rect = clipRect
            };
        }
        else if (!_clipGeometry.Rect.Equals(clipRect))
        {
            _clipGeometry.Rect = clipRect;
            InvalidateVisual();
        }

        if (!ReferenceEquals(Clip, _clipGeometry))
        {
            Clip = _clipGeometry;
        }
    }

    /// <summary>
    /// Measures the children of a <see cref="T:AtomUI.Desktop.Controls.DataGridRowsPresenter" /> to 
    /// prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)" /> pass.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
    /// </param>
    /// <returns>
    /// The size that the <see cref="T:AtomUI.Desktop.Controls.DataGridRowsPresenter" /> determines it needs during layout, based on its calculations of child object allocated sizes.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (double.IsInfinity(availableSize.Height))
        {
            if (TopLevel.GetTopLevel(this) is { } topLevel)
            {
                double maxHeight = topLevel.IsArrangeValid ?
                    topLevel.Bounds.Height :
                    LayoutHelper.ApplyLayoutConstraints(topLevel, availableSize).Height;

                availableSize = availableSize.WithHeight(maxHeight);
            }
        }

        if (availableSize.Height == 0 || OwningGrid == null)
        {
            return base.MeasureOverride(availableSize);
        }

        // If the Width of our RowsPresenter changed then we need to invalidate our rows
        bool invalidateRows = (!OwningGrid.RowsPresenterAvailableSize.HasValue || !MathUtils.AreClose(availableSize.Width, OwningGrid.RowsPresenterAvailableSize.Value.Width))
                              && !double.IsInfinity(availableSize.Width);

        // The DataGrid uses the RowsPresenter available size in order to autogrow
        // and calculate the scrollbars
        OwningGrid.RowsPresenterAvailableSize = availableSize;

        OwningGrid.OnRowsMeasure();

        double totalHeight     = -OwningGrid.NegVerticalOffset;
        double totalCellsWidth = OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth;

        double headerWidth = 0;
        
        var displayData           = OwningGrid.DisplayData;
        int displayedElementCount = displayData.NumDisplayedScrollingElements;
        for (int i = 0; i < displayedElementCount; i++)
        {
            MeasureElement(
                displayData.GetScrollingElementAtDisplayIndex(i),
                invalidateRows,
                ref headerWidth,
                ref totalHeight);
        }

        if (_dragIndicator != null)
        {
            MeasureElement(_dragIndicator, invalidateRows, ref headerWidth, ref totalHeight);
        }

        OwningGrid.RowHeadersDesiredWidth = headerWidth;
        // Could be positive infinity depending on the DataGrid's bounds
        OwningGrid.AvailableSlotElementRoom = availableSize.Height - totalHeight;

        totalHeight = Math.Max(0, totalHeight);

        return new Size(totalCellsWidth + headerWidth, totalHeight);
    }

    private void MeasureElement(Control element, bool invalidateRows, ref double headerWidth, ref double totalHeight)
    {
        Debug.Assert(OwningGrid != null);

        if (element is DataGridRow row && invalidateRows)
        {
            row.InvalidateMeasure();
        }

        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        if (element is DataGridRow { HeaderCell: not null } dataRow)
        {
            headerWidth = Math.Max(headerWidth, dataRow.HeaderCell.DesiredSize.Width);
        }
        else if (element is DataGridRowGroupHeader { HeaderCell: not null } groupHeader)
        {
            headerWidth = Math.Max(headerWidth, groupHeader.HeaderCell.DesiredSize.Width);
        }

        if (element != _dragIndicator)
        {
            totalHeight += OwningGrid.GetDisplayedElementHeight(element);
        }
    }

    private void OnScrollGesture(ScrollGestureEventArgs e)
    {
        e.Handled = e.Handled || (OwningGrid?.UpdateScroll(-e.Delta) ?? false);
    }
    
    internal void NotifyAboutToDragging(object? data, int sourceIndex)
    {
        Debug.Assert(OwningGrid != null);
        Debug.Assert(DraggedRowIndex == sourceIndex);
        _dragIndicator            = OwningGrid.GetGeneratedGhostRow(data);
        _dragIndicator.Index      = sourceIndex;
        _dragIndicator.IsDragging = true;
        LogicalChildren.Add(_dragIndicator);
        VisualChildren.Add(_dragIndicator);
    }

    internal void NotifyDropped()
    {
        if (_dragIndicator != null)
        {
            LogicalChildren.Remove(_dragIndicator);
            VisualChildren.Remove(_dragIndicator);
        }

        _dragIndicator = null;
    }
    
#if DEBUG
    internal void PrintChildren()
    {
        foreach (Control element in Children)
        {
            if (element is DataGridRow row)
            {
                Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Slot: {0} Row: {1} Visibility: {2} ", row.Slot, row.Index, row.IsVisible));
            }
            else if (element is DataGridRowGroupHeader groupHeader)
            {
                Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Slot: {0} GroupHeader: {1} Visibility: {2}", groupHeader.SourceSlot, groupHeader.SourceGroup?.Key, groupHeader.IsVisible));
            }
        }
    }
#endif
}
