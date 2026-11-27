// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// Visual container follows the DataGrid owner, including TokenTransitions in its template.
/// <summary>
/// The row indicator cell at the start of a <see cref="DataGridRow" /> or of a group header row. It mirrors
/// the invalid, selected, editing and current state of the owning row, or the current state of the owning
/// group header, as pseudo-classes.
/// </summary>
[TokenOwnerTransparent]
[PseudoClasses(StdPseudoClass.Invalid, StdPseudoClass.Selected, StdPseudoClass.Editing, StdPseudoClass.Current)]
public class DataGridRowHeader : ContentControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SeparatorBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SeparatorBrushProperty =
        AvaloniaProperty.Register<DataGridRowHeader, IBrush?>(nameof(SeparatorBrush));
    
    /// <summary>
    /// Identifies the <see cref="IsSeparatorsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSeparatorsVisibleProperty =
        AvaloniaProperty.Register<DataGridRowHeader, bool>(
            nameof(IsSeparatorsVisible));

    /// <summary>
    /// Brush of the separator line under the header, the segment of the row's bottom grid line that runs under
    /// it. The line is drawn only while <see cref="IsSeparatorsVisible" /> is <see langword="true" />. When the
    /// value is cleared, the theme supplies the <c>TableBorderColor</c> token.
    /// </summary>
    public IBrush? SeparatorBrush
    {
        get => GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value); 
    }
    
    /// <summary>
    /// Controls whether the separator line under the header is drawn. The header assigns this value itself,
    /// from the owning grid's rule for the bottom grid line of its slot, whenever its owner changes, a template
    /// is applied or the grid lines are refreshed, so a value set elsewhere is replaced at that point.
    /// </summary>
    public bool IsSeparatorsVisible
    {
        get => GetValue(IsSeparatorsVisibleProperty);
        set => SetValue(IsSeparatorsVisibleProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGridRowHeader>();
    
    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    private Control? _rootElement;
    private Control? _owner;

    internal Control? Owner
    {
        get => _owner;
        set
        {
            if (_owner != value)
            {
                _owner = value;
                ConfigureOwnerDependentState();
            }
        }
    }

    private DataGridRow? OwningRow => Owner as DataGridRow;
    private DataGridRowGroupHeader? OwningRowGroupHeader => Owner as DataGridRowGroupHeader;
    private Rectangle? _horizontalSeparator;
    
    private DataGrid? OwningGrid
    {
        get
        {
            if (OwningRow != null)
            {
                return OwningRow.OwningGrid;
            }
            if (OwningRowGroupHeader != null)
            {
                return OwningRowGroupHeader.OwningGrid;
            }
            return null;
        }
    }
    
    private int Slot
    {
        get
        {
            if (OwningRow != null)
            {
                return OwningRow.Slot;
            }
            return OwningRowGroupHeader?.SourceSlot ?? -1;
        }
    }
    
    #endregion
    
    static DataGridRowHeader()
    {
        PointerPressedEvent.AddClassHandler<DataGridRowHeader>(
            (x, e) => x.HandlePointerPressed(e),
            handledEventsToo: true);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<DataGridRowHeader>(IsOffscreenBehavior.FromClip);
    }
    
    /// <summary>
    /// Resolves the root layout and the separator line from the new template, then applies the state that
    /// depends on the owner: the pseudo-classes and the thickness and visibility of the separator line.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _rootElement         = e.NameScope.Find<Control>(DataGridRowHeaderThemeConstants.RootLayoutPart);
        _horizontalSeparator = e.NameScope.Find<Rectangle>(DataGridRowHeaderThemeConstants.HorizontalSeparatorPart);
        ConfigureOwnerDependentState();
    }

    internal void EnsureGridLines()
    {
        ConfigureOwnerDependentState();
    }

    private void ConfigureOwnerDependentState()
    {
        if (_rootElement != null)
        {
            UpdatePseudoClasses();
        }

        var owningGrid = OwningGrid;
        if (owningGrid == null)
        {
            return;
        }

        if (_horizontalSeparator != null)
        {
            _horizontalSeparator.Height = BorderUtils.BuildRenderScaleAwareThickness(owningGrid, owningGrid.BorderThickness.Left);
        }

        ConfigureSeparatorVisible(owningGrid);
    }

    private void ConfigureSeparatorVisible(DataGrid owningGrid)
    {
        bool newVisibility = owningGrid.ShouldDisplayRowBottomGridLine(Slot);

        if (newVisibility != IsSeparatorsVisible)
        {
            IsSeparatorsVisible = newVisibility;
        }
    }

    /// <summary>
    /// Measures the header of a row with the row header width and the row height of the owning grid in place
    /// of the available size, where the grid fixes them, and returns a width of at least the current row header
    /// width of the grid.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that the header can give to its children. Its width is used only while the grid does
    /// not fix its row header width, and its height only while the grid does not fix its row height.
    /// </param>
    /// <returns>
    /// The measured size, with the width replaced by the actual row header width of the grid when that width
    /// is fixed or the measured header is narrower; the size measured by the base class when the header does
    /// not belong to a row of a grid.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OwningRow == null || OwningGrid == null)
        {
            return base.MeasureOverride(availableSize);
        }
        double measureHeight = double.IsNaN(OwningGrid.RowHeight) ? availableSize.Height : OwningGrid.RowHeight;
        double measureWidth = double.IsNaN(OwningGrid.RowHeaderWidth) ? availableSize.Width : OwningGrid.RowHeaderWidth;
        Size   measuredSize = base.MeasureOverride(new Size(measureWidth, measureHeight));

        // Auto grow the row header or force it to a fixed width based on the DataGrid's setting
        if (!double.IsNaN(OwningGrid.RowHeaderWidth) || measuredSize.Width < OwningGrid.ActualRowHeaderWidth)
        {
            return new Size(OwningGrid.ActualRowHeaderWidth, measuredSize.Height);
        }

        return measuredSize;
    }

    internal void UpdatePseudoClasses()
    {
        if (_rootElement != null && Owner != null && Owner.IsVisible)
        {
            if (OwningRow != null)
            {
                PseudoClasses.Set(StdPseudoClass.Invalid, !OwningRow.IsValid);
                PseudoClasses.Set(StdPseudoClass.Selected, OwningRow.IsSelected);
                PseudoClasses.Set(StdPseudoClass.Editing, OwningRow.IsEditing);

                if (OwningGrid != null)
                {
                    PseudoClasses.Set(StdPseudoClass.Current, OwningRow.Slot == OwningGrid.CurrentSlot);
                }
            }
            else if (OwningRowGroupHeader != null && OwningGrid != null)
            {
                PseudoClasses.Set(StdPseudoClass.Current, OwningRowGroupHeader.SourceSlot == OwningGrid.CurrentSlot);
            }
        }
    }

    /// <summary>
    /// Sets <c>IsMouseOver</c> of the owning row, so that the row shows its hover state while the pointer is
    /// over the header. Calls the base member afterwards.
    /// </summary>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        if (OwningRow != null)
        {
            OwningRow.IsMouseOver = true;
        }

        base.OnPointerEntered(e);
    }
    /// <summary>
    /// Clears <c>IsMouseOver</c> of the owning row when the pointer leaves the header. Calls the base member
    /// afterwards.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        if (OwningRow != null)
        {
            OwningRow.IsMouseOver = false;
        }

        base.OnPointerExited(e);
    }
    
    private void HandlePointerPressed(PointerPressedEventArgs e)
    {
        if (OwningGrid == null)
        {
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (!e.Handled)
            {
                OwningGrid.Focus();
            }
            if (OwningRow != null)
            {
                e.Handled = OwningGrid.UpdateStateOnMouseLeftButtonDown(e, -1, Slot, false);
            }
        }
        else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (!e.Handled)
            {
                OwningGrid.Focus();
            }
            if (OwningRow != null)
            {
                e.Handled = OwningGrid.UpdateStateOnMouseRightButtonDown(e, -1, Slot, false);
            }
        }
    } 
}
