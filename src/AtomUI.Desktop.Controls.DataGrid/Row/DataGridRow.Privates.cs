// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Utilities;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

public partial class DataGridRow
{
    private const byte DefaultMinHeight = 0;
    internal const int MaximumHeight = 65536;
    internal const double MinimumHeight = 0;
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGridRow>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGridRow>();
    
    internal static readonly StyledProperty<bool> IsDraggingProperty =
        AvaloniaProperty.Register<DataGridRow, bool>(nameof(IsDragging));
    
    internal static readonly DirectProperty<DataGridRow, bool> IsEditingModeProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, bool>(
            nameof(IsEditingMode),
            o => o.IsEditingMode);
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal bool IsDragging
    {
        get => GetValue(IsDraggingProperty);
        set => SetValue(IsDraggingProperty, value);
    }
    
    internal bool IsEditingMode
    {
        get => _isEditingMode;
        set => SetAndRaise(IsEditingModeProperty, ref _isEditingMode, value);
    }
    private bool _isEditingMode = false;
    
    internal int Slot { get; set; }
    
    internal DataGridCellCollection Cells { get; private set; }
    
    internal DataGrid? OwningGrid { get; set; }
    
    // Returns the actual template that should be sued for Details: either explicity set on this row
    // or inherited from the DataGrid
    private IDataTemplate? ActualDetailsTemplate
    {
        get
        {
            Debug.Assert(OwningGrid != null);
            return DetailsTemplate ?? OwningGrid.RowDetailsTemplate;
        }
    }
    
    // Returns the actual template that should be sued for header content: either explicity set on this row
    // or inherited from the DataGrid
    private IDataTemplate? ActualHeaderContentTemplate
    {
        get
        {
            Debug.Assert(OwningGrid != null);
            return HeaderContentTemplate ?? OwningGrid.RowHeaderContentTemplate;
        }
    }
    
    internal double ActualBottomGridLineHeight
    {
        get
        {
            if (_bottomGridLine != null && OwningGrid != null && OwningGrid.AreRowBottomGridLinesRequired)
            {
                // Unfortunately, _bottomGridLine has no size yet so we can't get its actualheight
                // TODO 需要审查这里不是常量可以吗？是不是会导致高度计算错误
                return OwningGrid.BorderThickness.Left;
            }
            return 0;
        }
    }
    
    internal DataGridCell FillerCell
    {
        get
        {
            Debug.Assert(OwningGrid != null);
            if (_fillerCell == null)
            {
                _fillerCell = new DataGridCell
                {
                    IsVisible = false,
                    OwningRow = this
                };
                BindUtils.RelayBind(OwningGrid, DataGrid.SizeTypeProperty, _fillerCell, DataGridCell.SizeTypeProperty);
                if (_cellsElement != null)
                {
                    _cellsElement.Children.Add(_fillerCell);
                }
            }
            return _fillerCell;
        }
    }
    
    internal bool HasBottomGridLine => _bottomGridLine != null;
    
    internal bool HasHeaderCell => _headerElement != null;

    internal DataGridRowHeader? HeaderCell => _headerElement;
    
    internal bool IsEditing => OwningGrid != null && OwningGrid.EditingRow == this;
    
    /// <summary>
    /// Layout when template is applied
    /// </summary>
    internal bool IsLayoutDelayed { get; private set; }
    
    internal bool IsMouseOver
    {
        get => OwningGrid != null && OwningGrid.MouseOverRowIndex == Index;
        set
        {
            if (OwningGrid != null && value != IsMouseOver)
            {
                if (value)
                {
                    OwningGrid.MouseOverRowIndex = Index;
                }
                else
                {
                    OwningGrid.MouseOverRowIndex = null;
                }
            }
        }
    }
    
    internal bool IsRecycled { get; private set; }

    internal bool IsRecyclable
    {
        get
        {
            if (OwningGrid != null)
            {
                return OwningGrid.IsRowRecyclable(this);
            }
            return true;
        }
    }

    internal int? MouseOverColumnIndex
    {
        get => _mouseOverColumnIndex;
        set
        {
            Debug.Assert(OwningGrid != null);
            if (_mouseOverColumnIndex != value)
            {
                DataGridCell? oldMouseOverCell = null;
                if (_mouseOverColumnIndex != null && OwningGrid.IsSlotVisible(Slot))
                {
                    if (_mouseOverColumnIndex > -1)
                    {
                        oldMouseOverCell = Cells[_mouseOverColumnIndex.Value];
                    }
                }
                _mouseOverColumnIndex = value;
                if (oldMouseOverCell != null && IsVisible)
                {
                    oldMouseOverCell.UpdatePseudoClasses();
                }
                if (_mouseOverColumnIndex != null && OwningGrid != null && OwningGrid.IsSlotVisible(Slot))
                {
                    if (_mouseOverColumnIndex > -1)
                    {
                        Cells[_mouseOverColumnIndex.Value].UpdatePseudoClasses();
                    }
                }
            }
        }
    }

    internal Panel? RootElement { get; private set; }
    
    // Height that the row will eventually end up at after a possible details animation has completed
    internal double TargetHeight
    {
        get
        {
            if (!double.IsNaN(Height))
            {
                return Height;
            }
            if (_detailsElement != null && _appliedDetailsVisibility == true && _appliedDetailsTemplate != null)
            {
                Debug.Assert(!double.IsNaN(_detailsElement.ContentHeight));
                Debug.Assert(!double.IsNaN(_detailsDesiredHeight));
                return DesiredSize.Height + _detailsDesiredHeight - _detailsElement.ContentHeight;
            }
            return DesiredSize.Height;
        }
    }

    private bool ActualDetailsVisibility
    {
        get
        {
            if (OwningGrid == null)
            {
                throw DataGridError.DataGrid.NoOwningGrid(GetType());
            }
            if (Index == -1)
            {
                throw DataGridError.DataGridRow.InvalidRowIndexCannotCompleteOperation();
            }
            return OwningGrid.GetRowDetailsVisibility(Index);
        }
    }
    
    #endregion
    
    private DataGridCellsPresenter? _cellsElement;
    private DataGridCell? _fillerCell;
    private DataGridRowHeader? _headerElement;
    private double _lastHorizontalOffset;
    private int? _mouseOverColumnIndex;
 
    private Rectangle? _bottomGridLine;
    private bool _areHandlersSuspended;
    
    // In the case where Details scales vertically when it's arranged at a different width, we
    // get the wrong height measurement so we need to check it again after arrange
    private bool _checkDetailsContentHeight;

    // Optimal height of the details based on the Element created by the DataTemplate
    private double _detailsDesiredHeight;

    private bool _detailsLoaded;
    private bool _headerContentLoaded;
    private bool _detailsVisibilityNotificationPending;
    private Control? _detailsContent;
    private Control? _headerContent;
    private IDisposable? _headerContentSizeSubscription;
    private IDisposable? _detailsContentSizeSubscription;
    private DataGridDetailsPresenter? _detailsElement;
    
    // Locally cache whether or not details are visible so we don't run redundant storyboards
    // The Details Template that is actually applied to the Row
    private IDataTemplate? _appliedDetailsTemplate;
    
    private bool? _appliedDetailsVisibility;
    private IDataTemplate? _appliedHeaderContentTemplate;
    
    private void HandleCellAdded(object? sender, DataGridCellEventArgs e)
    {
        _cellsElement?.Children.Add(e.Cell);
    }

    private void HandleCellRemoved(object? sender, DataGridCellEventArgs e)
    {
        _cellsElement?.Children.Remove(e.Cell);
    }
    
    private void HandleHeaderChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_headerElement != null)
        {
            _headerElement.Content = change.NewValue;
        }
    }

    private void HandleDetailsTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldValue = (IDataTemplate?)change.OldValue;
        var newValue = (IDataTemplate?)change.NewValue;

        if (!_areHandlersSuspended && OwningGrid != null)
        {
            IDataTemplate? ActualDetailsTemplate(IDataTemplate? template) => (template ?? OwningGrid.RowDetailsTemplate);

            // We don't always want to apply the new Template because they might have set the same one
            // we inherited from the DataGrid
            if (ActualDetailsTemplate(newValue) != ActualDetailsTemplate(oldValue))
            {
                ApplyDetailsTemplate(initializeDetailsPreferredHeight: false);
            }
        }
    }

    internal void ApplyDetailsTemplate(bool initializeDetailsPreferredHeight)
    {
        Debug.Assert(OwningGrid != null);
        if (_detailsElement != null && IsDetailsVisible)
        {
            IDataTemplate? oldDetailsTemplate = _appliedDetailsTemplate;
            if (ActualDetailsTemplate != null && ActualDetailsTemplate != _appliedDetailsTemplate)
            {
                if (_detailsContent != null)
                {
                    _detailsContentSizeSubscription?.Dispose();
                    _detailsContentSizeSubscription = null;
                    if (_detailsLoaded)
                    {
                        OwningGrid.NotifyUnloadingRowDetails(this, _detailsContent);
                        _detailsLoaded = false;
                    }
                }
                _detailsElement.Children.Clear();

                _detailsContent         = ActualDetailsTemplate.Build(DataContext);
                _appliedDetailsTemplate = ActualDetailsTemplate;

                if (_detailsContent != null)
                {
                    if (_detailsContent is Layoutable layoutableContent)
                    {
                        layoutableContent.LayoutUpdated += HandleLayoutUpdated;

                        _detailsContentSizeSubscription = new CompositeDisposable(2)
                        {
                            Disposable.Create(() => layoutableContent.LayoutUpdated -= HandleLayoutUpdated),
                            _detailsContent.GetObservable(MarginProperty).Subscribe(NotifyMarginChanged)
                        };
                    }
                    else
                    {
                        _detailsContentSizeSubscription =
                            _detailsContent.GetObservable(MarginProperty)
                                           .Subscribe(NotifyMarginChanged);

                    }

                    _detailsElement.Children.Add(_detailsContent);
                }
            }

            if (_detailsContent != null && !_detailsLoaded)
            {
                _detailsLoaded              = true;
                _detailsContent.DataContext = DataContext;
                OwningGrid.NotifyLoadingRowDetails(this, _detailsContent);
            }
            if (initializeDetailsPreferredHeight && double.IsNaN(_detailsDesiredHeight) &&
                _appliedDetailsTemplate != null && _detailsElement.Children.Count > 0)
            {
                EnsureDetailsDesiredHeight();
            }
            else if (oldDetailsTemplate == null)
            {
                _detailsDesiredHeight = double.NaN;
                EnsureDetailsDesiredHeight();
                _detailsElement.ContentHeight = _detailsDesiredHeight;
            }
        }
    }
    
    private void HandleHeaderContentTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldValue = (IDataTemplate?)change.OldValue;
        var newValue = (IDataTemplate?)change.NewValue;

        if (!_areHandlersSuspended && OwningGrid != null)
        {
            IDataTemplate? ActualHeaderContentTemplate(IDataTemplate? template) => (template ?? OwningGrid.RowHeaderContentTemplate);

            // We don't always want to apply the new Template because they might have set the same one
            // we inherited from the DataGrid
            if (ActualHeaderContentTemplate(newValue) != ActualHeaderContentTemplate(oldValue))
            {
                ApplyHeaderContentTemplate();
            }
        }
    }
    
    internal void ApplyHeaderContentTemplate()
    {
        Debug.Assert(OwningGrid != null);
        if (HeaderCell != null && OwningGrid.IsRowHeadersVisible)
        {
            if (ActualHeaderContentTemplate != null && ActualHeaderContentTemplate != _appliedHeaderContentTemplate)
            {
                if (_headerContent != null)
                {
                    _headerContentSizeSubscription?.Dispose();
                    _headerContentSizeSubscription = null;
                    if (_detailsLoaded)
                    {
                        OwningGrid.NotifyUnloadingRowDetails(this, _headerContent);
                        _detailsLoaded = false;
                    }
                }

                HeaderCell.Content = null;

                _headerContent                = ActualHeaderContentTemplate.Build(DataContext);
                _appliedHeaderContentTemplate = ActualHeaderContentTemplate;

                if (_headerContent != null)
                {
                    Header = _headerContent;
                }
            }

            if (_headerContent != null && !_headerContentLoaded)
            {
                _headerContentLoaded       = true;
                _headerContent.DataContext = DataContext;
            }
        }
    }
     
    private void HandleIsDetailsVisibleChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            if (OwningGrid == null)
            {
                throw DataGridError.DataGrid.NoOwningGrid(GetType());
            }
            if (Index == -1)
            {
                throw DataGridError.DataGridRow.InvalidRowIndexCannotCompleteOperation();
            }

            var newValue = (bool)(change.NewValue ?? false);
            OwningGrid.NotifyRowDetailsVisibilityPropertyChanged(Index, newValue);
            SetDetailsVisibilityInternal(newValue, raiseNotification: true, animate: true);
        }
    }
    
    private void HandlePointerPressed(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (OwningGrid != null)
        {
            OwningGrid.IsDoubleClickRecordsClickOnCall(this);
        }
    }
    
    internal void ApplyCellsState()
    {
        foreach (DataGridCell dataGridCell in Cells)
        {
            dataGridCell.UpdatePseudoClasses();
        }
    }

    internal void ApplyHeaderStatus()
    {
        Debug.Assert(OwningGrid != null);
        if (_headerElement != null && OwningGrid.IsRowHeadersVisible)
        {
            _headerElement.UpdatePseudoClasses();
        }
    }

    internal void ApplyState()
    {
        if (RootElement != null && OwningGrid != null && IsVisible)
        {
            var isSelected = Slot != -1 && OwningGrid.GetRowSelection(Slot);
            IsSelected = isSelected;
            PseudoClasses.Set(StdPseudoClass.Editing, IsEditing);
            PseudoClasses.Set(StdPseudoClass.Invalid, !IsValid);
            IsEditingMode = IsEditing;
            ApplyHeaderStatus();
        }
    }

    //TODO Animation
    internal void DetachFromDataGrid(bool recycle)
    {
        UnloadDetailsTemplate(recycle);

        if (recycle)
        {
            IsRecycled = true;

            if (_cellsElement != null)
            {
                _cellsElement.Recycle();
            }

            _checkDetailsContentHeight = false;

            // Clear out the old Details cache so it won't be reused for other data
            //_detailsDesiredHeight = double.NaN;
            if (_detailsElement != null)
            {
                _detailsElement.ClearValue(DataGridDetailsPresenter.ContentHeightProperty);
            }
        }

        Slot = -1;
    }

    internal void InvalidateCellsIndex()
    {
        _cellsElement?.InvalidateChildIndex();
    }

    internal void EnsureFillerVisibility()
    {
        _cellsElement?.EnsureFillerVisibility();
    }

    internal void EnsureGridLines()
    {
        if (OwningGrid != null)
        {
            if (_bottomGridLine != null)
            {
                // It looks like setting Visibility sometimes has side effects so make sure the value is actually
                // different before setting it
                bool newVisibility = OwningGrid.AreHorizontalGridLinesVisible;

                if (newVisibility != _bottomGridLine.IsVisible)
                {
                    _bottomGridLine.IsVisible = newVisibility;
                }
                _bottomGridLine.Height = OwningGrid.BorderThickness.Left;
            }

            foreach (DataGridCell cell in Cells)
            {
                cell.EnsureGridLine(OwningGrid.ColumnsInternal.LastVisibleColumn);
            }
        }
    }

    internal void EnsureHeaderStyleAndVisibility(Style? previousStyle)
    {
        if (_headerElement != null && OwningGrid != null)
        {
            _headerElement.IsVisible = OwningGrid.IsRowHeadersVisible;
        }
    }

    internal void EnsureHeaderVisibility()
    {
        if (_headerElement != null && OwningGrid != null)
        {
            _headerElement.IsVisible = OwningGrid.IsRowHeadersVisible;
        }
    }

    internal void InvalidateHorizontalArrange()
    {
        if (_cellsElement != null)
        {
            _cellsElement.InvalidateArrange();
        }
        if (_detailsElement != null)
        {
            _detailsElement.InvalidateArrange();
        }
    }

    internal void InvalidateDesiredHeight()
    {
        _cellsElement?.InvalidateDesiredHeight();
    }

    internal void ResetGridLine()
    {
        _bottomGridLine = null;
    }

    private void UnloadDetailsTemplate(bool recycle)
    {
        Debug.Assert(OwningGrid != null);
        if (_detailsElement != null)
        {
            if (_detailsContent != null)
            {
                if (_detailsLoaded)
                {
                    OwningGrid.NotifyUnloadingRowDetails(this, _detailsContent);
                }
                _detailsContent.DataContext = null;
                if (!recycle)
                {
                    _detailsContentSizeSubscription?.Dispose();
                    _detailsContentSizeSubscription = null;
                    _detailsContent                 = null;
                }
            }

            if (!recycle)
            {
                _detailsElement.Children.Clear();
            }
            _detailsElement.ContentHeight = 0;
        }
        if (!recycle)
        {
            _appliedDetailsTemplate = null;
            SetValueNoCallback(DetailsTemplateProperty, null);
        }

        _detailsLoaded            = false;
        _appliedDetailsVisibility = null;
        SetValueNoCallback(IsDetailsVisibleProperty, false);
    }

    //TODO Animation
    internal void EnsureDetailsContentHeight()
    {
        if ((_detailsElement != null)
            && (_detailsContent != null)
            && (double.IsNaN(_detailsContent.Height))
            && (IsDetailsVisible)
            && (!double.IsNaN(_detailsDesiredHeight))
            && !MathUtilities.AreClose(_detailsContent.Bounds.Inflate(_detailsContent.Margin).Height, _detailsDesiredHeight)
            && Slot != -1)
        {
            _detailsDesiredHeight = _detailsContent.Bounds.Inflate(_detailsContent.Margin).Height;

            if (true)
            {
                _detailsElement.ContentHeight = _detailsDesiredHeight;
            }
        }
    }
    
    // Makes sure the _detailsDesiredHeight is initialized.  We need to measure it to know what
    // height we want to animate to.  Subsequently, we just update that height in response to SizeChanged
    private void EnsureDetailsDesiredHeight()
    {
        Debug.Assert(_detailsElement != null && OwningGrid != null);

        if (_detailsContent != null)
        {
            Debug.Assert(_detailsElement.Children.Contains(_detailsContent));

            _detailsContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _detailsDesiredHeight = _detailsContent.DesiredSize.Height;
        }
        else
        {
            _detailsDesiredHeight = 0;
        }
    }

    //TODO Cleanup
    double? _previousDetailsHeight = null;

    //TODO Animation
    private void NotifyHeightChanged(double newValue)
    {
        if (_previousDetailsHeight.HasValue)
        {
            var oldValue = _previousDetailsHeight.Value;
            _previousDetailsHeight = newValue;
            if (!MathUtils.AreClose(newValue, oldValue) && !MathUtils.AreClose(newValue, _detailsDesiredHeight))
            {
                if (IsDetailsVisible && _appliedDetailsTemplate != null)
                {
                    Debug.Assert(_detailsElement != null);
                    // Update the new desired height for RowDetails
                    _detailsDesiredHeight = newValue;

                    _detailsElement.ContentHeight = newValue;

                    // Calling this when details are not visible invalidates during layout when we have no work
                    // to do.  In certain scenarios, this could cause a layout cycle
                    NotifyRowDetailsChanged();
                }
            }
        }
        else
        {
            _previousDetailsHeight = newValue;
        }
    }
    
    private void NotifyRowDetailsChanged()
    {
        OwningGrid?.NotifyRowDetailsChanged();
    }
    
    private void NotifySizeChanged(Rect newValue)
    {
        NotifyHeightChanged(newValue.Height);
    }
    
    private void NotifyMarginChanged(Thickness newValue)
    {
        if (_detailsContent != null)
        {
            NotifySizeChanged(_detailsContent.Bounds.Inflate(newValue));
        }
    }
    
    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        if (_detailsContent != null)
        {
            var margin = _detailsContent.Margin;
            var height = _detailsContent.DesiredSize.Height + margin.Top + margin.Bottom;

            NotifyHeightChanged(height);
        }
    }

    //TODO Animation
    // Sets IsDetailsVisible on the row and animates if necessary
    internal void SetDetailsVisibilityInternal(bool isVisible, bool raiseNotification, bool animate)
    {
        Debug.Assert(OwningGrid != null);
        Debug.Assert(Index != -1);

        if (_appliedDetailsVisibility != isVisible)
        {
            if (_detailsElement == null)
            {
                if (raiseNotification)
                {
                    _detailsVisibilityNotificationPending = true;
                }
                return;
            }

            _appliedDetailsVisibility = isVisible;
            SetValueNoCallback(IsDetailsVisibleProperty, isVisible);

            // Applies a new DetailsTemplate only if it has changed either here or at the DataGrid level
            ApplyDetailsTemplate(initializeDetailsPreferredHeight: true);

            // no template to show
            if (_appliedDetailsTemplate == null)
            {
                if (_detailsElement.ContentHeight > 0)
                {
                    _detailsElement.ContentHeight = 0;
                }
                return;
            }

            if (IsDetailsVisible)
            {
                // Set the details height directly
                _detailsElement.ContentHeight = _detailsDesiredHeight;
                _checkDetailsContentHeight    = true;
            }
            else
            {
                _detailsElement.ContentHeight = 0;
            }

            NotifyRowDetailsChanged();

            if (raiseNotification)
            {
                Debug.Assert(_detailsContent != null);
                OwningGrid.OnRowDetailsVisibilityChanged(new DataGridRowDetailsEventArgs(this, _detailsContent));
            }
        }
    }
    
    private void SetValueNoCallback<T>(AvaloniaProperty<T> property, T value, BindingPriority priority = BindingPriority.LocalValue)
    {
        _areHandlersSuspended = true;
        try
        {
            SetValue(property, value, priority);
        }
        finally
        {
            _areHandlersSuspended = false;
        }
    }
}
