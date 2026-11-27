// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Presents the details section inside a row template. The owning row sets <see cref="ContentHeight" />;
/// the section is laid out next to the row group indentation and, unless the grid freezes row details, it
/// is clipped where it would cover the elements to its left while the grid scrolls horizontally.
/// </summary>
public sealed class DataGridDetailsPresenter : Panel
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ContentHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ContentHeightProperty =
        AvaloniaProperty.Register<DataGridDetailsPresenter, double>(nameof(ContentHeight));
    
    /// <summary>
    /// Gets or sets the height of the details section. The owning row sets it from the measured details content;
    /// it affects measure, and <see cref="double.NaN" /> is treated as 0.
    /// </summary>
    public double ContentHeight
    {
        get => GetValue(ContentHeightProperty);
        set => SetValue(ContentHeightProperty, value);
    }

    #endregion

    internal DataGridRow? OwningRow { get; set; }
    
    private DataGrid? OwningGrid => OwningRow?.OwningGrid;
    private RectangleGeometry? _clipGeometry;

    static DataGridDetailsPresenter()
    {
        AffectsMeasure<DataGridDetailsPresenter>(ContentHeightProperty);
    }


    /// <summary>
    /// Arranges the content of the <see cref="T:AtomUI.Desktop.Controls.DataGridDetailsPresenter" />.
    /// </summary>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
    /// <returns>
    /// The actual size used by the <see cref="T:AtomUI.Desktop.Controls.DataGridDetailsPresenter" />.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (OwningGrid == null)
        {
            return base.ArrangeOverride(finalSize);
        }
        Debug.Assert(OwningGrid.ColumnsInternal.RowGroupSpacerColumn != null);
        double rowGroupSpacerWidth = OwningGrid.ColumnsInternal.RowGroupSpacerColumn.Width.Value;
        double leftEdge            = rowGroupSpacerWidth;
        double xClip               = OwningGrid.IsRowGroupHeadersFrozen ? rowGroupSpacerWidth : 0;
        double width;
        if (OwningGrid.IsRowDetailsFrozen)
        {
            leftEdge += OwningGrid.HorizontalOffset;
            width    =  OwningGrid.CellsWidth;
        }
        else
        {
            xClip += OwningGrid.HorizontalOffset;
            width =  Math.Max(OwningGrid.CellsWidth, OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth);
        }
        // Details should not extend through the indented area
        width -= rowGroupSpacerWidth;
        double height = Math.Max(0, double.IsNaN(ContentHeight) ? 0 : ContentHeight);
        
        foreach (Control child in Children)
        {
            child.Arrange(new Rect(leftEdge, 0, width, height));
        }
        
        if (OwningGrid.IsRowDetailsFrozen)
        {
            // Frozen Details should not be clipped, similar to frozen cells
            ClearClipGeometry();
        }
        else
        {
            // Clip so Details doesn't obstruct elements to the left (the RowHeader by default) as we scroll to the right
            UpdateClipGeometry(new Rect(xClip, 0, Math.Max(0, width - xClip + rowGroupSpacerWidth), height));
        }

        return finalSize;
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

    private void ClearClipGeometry()
    {
        if (Clip is not null)
        {
            Clip = null;
        }
    }

    /// <summary>
    /// Measures the children of a <see cref="T:AtomUI.Desktop.Controls.DataGridDetailsPresenter" /> to
    /// prepare for arranging them during the <see cref="ArrangeOverride" /> pass.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
    /// </param>
    /// <returns>
    /// The size that the <see cref="T:AtomUI.Desktop.Controls.DataGridDetailsPresenter" /> determines it needs during layout, based on its calculations of child object allocated sizes.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OwningGrid == null || Children.Count == 0)
        {
            return default;
        }
        Debug.Assert(OwningGrid.ColumnsInternal.RowGroupSpacerColumn != null);

        double desiredWidth = OwningGrid.IsRowDetailsFrozen ?
            OwningGrid.CellsWidth :
            Math.Max(OwningGrid.CellsWidth, OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth);
        
        desiredWidth -= OwningGrid.ColumnsInternal.RowGroupSpacerColumn.Width.Value;
        
        double desiredHeight = Math.Max(0, double.IsNaN(ContentHeight) ? 0 : ContentHeight);
        foreach (Control child in Children)
        {
            child.Measure(new Size(desiredWidth, desiredHeight));
        }
        
        return new Size(desiredWidth, desiredHeight);
    }
}
