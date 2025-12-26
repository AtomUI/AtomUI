// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Invalid, StdPseudoClass.Selected, StdPseudoClass.Editing, StdPseudoClass.Current)]
public class DataGridRowHeader : ContentControl
{
    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> SeparatorBrushProperty =
        AvaloniaProperty.Register<DataGridRowHeader, IBrush?>(nameof(SeparatorBrush));
    
    public static readonly StyledProperty<bool> IsSeparatorsVisibleProperty =
        AvaloniaProperty.Register<DataGridRowHeader, bool>(
            nameof(IsSeparatorsVisible));

    public IBrush? SeparatorBrush
    {
        get => GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value); 
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the row header separator lines are visible.
    /// </summary>
    public bool IsSeparatorsVisible
    {
        get => GetValue(IsSeparatorsVisibleProperty);
        set => SetValue(IsSeparatorsVisibleProperty, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGridRowHeader>();
    
    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    private Control? _rootElement;
    internal Control? Owner { get; set; }
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
            if (OwningRowGroupHeader != null && OwningRowGroupHeader.RowGroupInfo != null)
            {
                return OwningRowGroupHeader.RowGroupInfo.Slot;
            }
            return -1;
        }
    }
    
    #endregion
    
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridRowHeader" /> class. 
    /// </summary>
    public DataGridRowHeader()
    {
        AddHandler(PointerPressedEvent, HandlePointerPressed, handledEventsToo: true);
    }

    static DataGridRowHeader()
    {
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<DataGridRowHeader>(IsOffscreenBehavior.FromClip);
    }
    
    /// <summary>
    /// Builds the visual tree for the row header when a new template is applied. 
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _rootElement         = e.NameScope.Find<Control>(DataGridRowHeaderThemeConstants.RootLayoutPart);
        _horizontalSeparator = e.NameScope.Find<Rectangle>(DataGridRowHeaderThemeConstants.HorizontalSeparatorPart);
        if (_rootElement != null)
        {
            UpdatePseudoClasses();
        }

        Debug.Assert(OwningGrid != null);
        if (_horizontalSeparator != null)
        {
            _horizontalSeparator.Height = OwningGrid.BorderThickness.Left;
        }

        ConfigureSeparatorVisible();
    }

    private void ConfigureSeparatorVisible()
    {
        Debug.Assert(OwningGrid != null);
        bool newVisibility = OwningGrid.AreHorizontalGridLinesVisible;

        if (newVisibility != IsSeparatorsVisible)
        {
            IsSeparatorsVisible = newVisibility;
        }
    }

    /// <summary>
    /// Measures the children of a <see cref="T:AtomUI.Desktop.Controls.DataGridRowHeader" /> to prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)" /> pass.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
    /// </param>
    /// <returns>
    /// The size that the <see cref="T:AtomUI.Desktop.Controls.DataGridRowHeader" /> determines it needs during layout, based on its calculations of child object allocated sizes.
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
            else if (OwningRowGroupHeader != null && OwningGrid != null && OwningRowGroupHeader.RowGroupInfo != null)
            {
                PseudoClasses.Set(StdPseudoClass.Current, OwningRowGroupHeader.RowGroupInfo.Slot == OwningGrid.CurrentSlot);
            }
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        if (OwningRow != null)
        {
            OwningRow.IsMouseOver = true;
        }

        base.OnPointerEntered(e);
    }
    protected override void OnPointerExited(PointerEventArgs e)
    {
        if (OwningRow != null)
        {
            OwningRow.IsMouseOver = false;
        }

        base.OnPointerExited(e);
    }
    
    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (OwningGrid == null)
        {
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (!e.Handled)
                //if (!e.Handled && OwningGrid.IsTabStop)
            {
                OwningGrid.Focus();
            }
            if (OwningRow != null)
            {
                Debug.Assert(sender is DataGridRowHeader);
                Debug.Assert(sender == this);
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
                Debug.Assert(sender is DataGridRowHeader);
                Debug.Assert(sender == this);
                e.Handled = OwningGrid.UpdateStateOnMouseRightButtonDown(e, -1, Slot, false);
            }
        }
    } 
}
