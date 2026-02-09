// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Selected, StdPseudoClass.Editing, StdPseudoClass.Invalid, DataGridPseudoClass.RowHover)]
public partial class DataGridRow : TemplatedControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the Header dependency property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<DataGridRow, object?>(nameof(Header));
    
    public static readonly DirectProperty<DataGridRow, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, bool>(
            nameof(IsSelected),
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);
    
    public static readonly DirectProperty<DataGridRow, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, bool>(
            nameof(IsValid),
            o => o.IsValid);

    public static readonly StyledProperty<IDataTemplate?> DetailsTemplateProperty =
        AvaloniaProperty.Register<DataGridRow, IDataTemplate?>(nameof(DetailsTemplate));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderContentTemplateProperty =
        AvaloniaProperty.Register<DataGridRow, IDataTemplate?>(nameof(HeaderContentTemplate));
    
    public static readonly StyledProperty<bool> IsDetailsVisibleProperty =
        AvaloniaProperty.Register<DataGridRow, bool>(nameof(IsDetailsVisible));
    
    public static readonly DirectProperty<DataGridRow, int> IndexProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, int>(nameof(Index), 
            o => o.Index, 
            (o, v) => o.Index = v);
    
    public static readonly DirectProperty<DataGridRow, int> LogicIndexProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, int>(nameof(LogicIndex), 
            o => o.LogicIndex,
            (o, v) => o.LogicIndex = v);
    
    /// <summary>
    /// Gets or sets the row header.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    private bool _isSelected;
    
    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }
    
    /// <summary>
    /// Gets a value that indicates whether the data in a row is valid.
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        internal set => SetAndRaise(IsValidProperty, ref _isValid, value);
    }
    private bool _isValid = true;
    
    /// <summary>
    /// Gets or sets the template that is used to display the details section of the row.
    /// </summary>
    public IDataTemplate? DetailsTemplate
    {
        get => GetValue(DetailsTemplateProperty);
        set => SetValue(DetailsTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that is used to display the header content of the row.
    /// </summary>
    public IDataTemplate? HeaderContentTemplate
    {
        get => GetValue(HeaderContentTemplateProperty);
        set => SetValue(HeaderContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value that indicates when the details section of the row is displayed.
    /// </summary>
    public bool IsDetailsVisible
    {
        get => GetValue(IsDetailsVisibleProperty);
        set => SetValue(IsDetailsVisibleProperty, value);
    }
    
    /// <summary>
    /// Index of the row
    /// </summary>
    public int Index
    {
        get => _index;
        internal set => SetAndRaise(IndexProperty, ref _index, value);
    }
    private int _index;
    
    public int LogicIndex
    {
        get => _logicIndex;
        internal set => SetAndRaise(LogicIndexProperty, ref _logicIndex, value);
    }
    private int _logicIndex;
    
    #endregion

    static DataGridRow()
    {
        HeaderProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleHeaderChanged(e));
        DetailsTemplateProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleDetailsTemplateChanged(e));
        HeaderContentTemplateProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleHeaderContentTemplateChanged(e));
        IsDetailsVisibleProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleIsDetailsVisibleChanged(e));
        PointerPressedEvent.AddClassHandler<DataGridRow>((x, e) => x.HandlePointerPressed(e), handledEventsToo: true);
        IsTabStopProperty.OverrideDefaultValue<DataGridRow>(false);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<DataGridRow>(IsOffscreenBehavior.FromClip);
    }
    
    public DataGridRow()
    {
        MinHeight                 =  DefaultMinHeight;
        Index                     =  -1;
        IsValid                   =  true;
        Slot                      =  -1;
        _mouseOverColumnIndex     =  null;
        _detailsDesiredHeight     =  double.NaN;
        _detailsLoaded            =  false;
        _appliedDetailsVisibility =  false;
        Cells                     =  new DataGridCellCollection(this);
        Cells.CellAdded           += HandleCellAdded;
        Cells.CellRemoved         += HandleCellRemoved;
    }
    
    /// <summary>
    /// Returns the row which contains the given element
    /// </summary>
    /// <param name="element">element contained in a row</param>
    /// <returns>Row that contains the element, or null if not found
    /// </returns>
    public static DataGridRow? GetRowContainingElement(Control element)
    {
        // Walk up the tree to find the DataGridRow that contains the element
        Visual?      parent = element;
        DataGridRow? row    = parent as DataGridRow;
        while ((parent != null) && (row == null))
        {
            parent = parent.GetVisualParent();
            row    = parent as DataGridRow;
        }
        return row;
    }
    
    /// <summary>
    /// Arranges the content of the <see cref="T:AtomUI.Desktop.Controls.DataGridRow" />.
    /// </summary>
    /// <returns>
    /// The actual size used by the <see cref="T:AtomUI.Desktop.Controls.DataGridRow" />.
    /// </returns>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (OwningGrid == null)
        {
            return base.ArrangeOverride(finalSize);
        }
        
        // If the DataGrid was scrolled horizontally after our last Arrange, we need to make sure
        // the Cells and Details are Arranged again
        if (!MathUtils.AreClose(_lastHorizontalOffset, OwningGrid.HorizontalOffset))
        {
            _lastHorizontalOffset = OwningGrid.HorizontalOffset;
            InvalidateHorizontalArrange();
        }

        Size size = base.ArrangeOverride(finalSize);

        if (_checkDetailsContentHeight)
        {
            _checkDetailsContentHeight = false;
            EnsureDetailsContentHeight();
        }
        
        if (_bottomGridLine != null)
        {
            RectangleGeometry gridlineClipGeometry = new RectangleGeometry();
            gridlineClipGeometry.Rect = new Rect(0, 0, Math.Max(0, DesiredSize.Width - OwningGrid.HorizontalOffset), _bottomGridLine.DesiredSize.Height);
            _bottomGridLine.Clip = gridlineClipGeometry;
        }

        return size;
    }
    
    /// <summary>
    /// Measures the children of a <see cref="T:AtomUI.Desktop.Controls.DataGridRow" /> to
    /// prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)" /> pass.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
    /// </param>
    /// <returns>
    /// The size that the <see cref="T:AtomUI.Desktop.Controls.DataGridRow" /> determines it needs during layout, based on its calculations of child object allocated sizes.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OwningGrid == null)
        {
            return base.MeasureOverride(availableSize);
        }

        //Allow the DataGrid specific components to adjust themselves based on new values
        _headerElement?.InvalidateMeasure();
        _cellsElement?.InvalidateMeasure();
        _detailsElement?.InvalidateMeasure();

        Size desiredSize = base.MeasureOverride(availableSize);
        return desiredSize.WithWidth(Math.Max(desiredSize.Width, OwningGrid.CellsWidth));
    }
    
    /// <summary>
    /// Builds the visual tree for the column header when a new template is applied.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        RootElement = e.NameScope.Find<Panel>(DataGridRowThemeConstants.RootLayoutPart);
        if (RootElement != null)
        {
            ApplyState();
        }

        bool updateVerticalScrollBar = false;
        if (_cellsElement != null)
        {
            // If we're applying a new template, we  want to remove the cells from the previous _cellsElement
            _cellsElement.Children.Clear();
            updateVerticalScrollBar = true;
        }

        _cellsElement = e.NameScope.Find<DataGridCellsPresenter>(DataGridRowThemeConstants.CellsPresenterPart);
        if (_cellsElement != null)
        {
            _cellsElement.OwningRow = this;
            if (Parent is DataGridRowsPresenter dataGridRowsPresenter)
            {
                _cellsElement.OwningRowsPresenter = dataGridRowsPresenter;
            }
            // Cells that were already added before the Template was applied need to
            // be added to the Canvas
            if (Cells.Count > 0)
            {
                foreach (DataGridCell cell in Cells)
                {
                    _cellsElement.Children.Add(cell);
                }
            }
        }

        _detailsElement = e.NameScope.Find<DataGridDetailsPresenter>(DataGridRowThemeConstants.DetailsPresenterPart);
        if (_detailsElement != null && OwningGrid != null)
        {
            _detailsElement.OwningRow = this;
            if (ActualDetailsVisibility && ActualDetailsTemplate != null && _appliedDetailsTemplate == null)
            {
                // Apply the DetailsTemplate now that the row template is applied.
                SetDetailsVisibilityInternal(ActualDetailsVisibility, raiseNotification: _detailsVisibilityNotificationPending, animate: false);
                _detailsVisibilityNotificationPending = false;
            }
        }

        _bottomGridLine = e.NameScope.Find<Rectangle>(DataGridRowThemeConstants.BottomGridLinePart);
        EnsureGridLines();

        _headerElement = e.NameScope.Find<DataGridRowHeader>(DataGridRowThemeConstants.RowHeaderPart);
        if (_headerElement != null)
        {
            _headerElement.Owner = this;
            if (Header != null)
            {
                _headerElement.Content = Header;
            }
            EnsureHeaderStyleAndVisibility(null);
        }

        //The height of this row might have changed after applying a new style, so fix the vertical scroll bar
        if (OwningGrid != null && updateVerticalScrollBar)
        {
            OwningGrid.UpdateVerticalScrollBar();
        }
        
        if (OwningGrid != null && OwningGrid.IsRowHeadersVisible)
        {
            ApplyHeaderContentTemplate();
        }
        
    }
    
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        IsMouseOver = true;
    }
    
    protected override void OnPointerExited(PointerEventArgs e)
    {
        IsMouseOver = false;
        base.OnPointerExited(e);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty)
        {
            var owner = OwningGrid;
            if (owner != null && IsRecycled)
            {
                var columns = owner.ColumnsItemsInternal;
                var nc      = columns.Count;
                for (int ci = 0; ci < nc; ci++)
                {
                    if (columns[ci] is DataGridTemplateColumn column)
                    {
                        var content = (Control?)Cells[column.Index].Content;
                        if (content != null)
                        {
                            column.RefreshCellContent(content, nameof(DataGridTemplateColumn.CellTemplate));
                        }
                    }
                }
            }
        }
        else if (change.Property == IsSelectedProperty)
        {
            var value = change.GetNewValue<bool>();

            if (OwningGrid != null && Slot != -1)
            {
                OwningGrid.SetRowSelection(Slot, value, false);
            }

            PseudoClasses.Set(StdPseudoClass.Selected, value);
        }
        else if (change.Property == IndexProperty)
        {
            LogicIndex = Index + 1;
        }
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }

    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty),
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
}
