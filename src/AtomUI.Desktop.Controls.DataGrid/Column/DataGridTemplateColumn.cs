// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A column that builds its cells from templates set on the column instead of a data binding.
/// <see cref="CellTemplate" /> supplies the display content of the cells and
/// <see cref="CellEditingTemplate" /> the content shown while a cell is edited.
/// </summary>
public class DataGridTemplateColumn : DataGridColumn
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="CellTemplate" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridTemplateColumn, IDataTemplate?> CellTemplateProperty =
        AvaloniaProperty.RegisterDirect<DataGridTemplateColumn, IDataTemplate?>(
            nameof(CellTemplate),
            o => o.CellTemplate,
            (o, v) => o.CellTemplate = v);
    
    /// <summary>
    /// Defines the <see cref="CellEditingTemplate" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridTemplateColumn, IDataTemplate?> CellEditingTemplateProperty =
        AvaloniaProperty.RegisterDirect<DataGridTemplateColumn, IDataTemplate?>(
            nameof(CellEditingTemplate),
            o => o.CellEditingTemplate,
            (o, v) => o.CellEditingTemplate = v);

    /// <summary>
    /// Template that builds the display content of the cells in this column. It receives the data item of the
    /// row the cell belongs to.
    /// </summary>
    /// <remarks>
    /// This is the XAML content property of the column, so a template written as direct content of the column
    /// lands here. Without a template, building a cell throws an exception outside the designer; in the designer
    /// the cell stays empty instead.
    /// </remarks>
    [Content]
    public IDataTemplate? CellTemplate
    {
        get => _cellTemplate;
        set => SetAndRaise(CellTemplateProperty, ref _cellTemplate, value);
    }
    private IDataTemplate? _cellTemplate;
    
    /// <summary>
    /// Template that builds the editing content of the cells in this column. It receives the data item of the
    /// row the cell belongs to.
    /// </summary>
    /// <remarks>
    /// While this property is <see langword="null" />, <see cref="IsReadOnly" /> reports <see langword="true" />,
    /// and an editing element is built from <see cref="CellTemplate" /> instead.
    /// </remarks>
    public IDataTemplate? CellEditingTemplate
    {
        get => _cellEditingCellTemplate;
        set => SetAndRaise(CellEditingTemplateProperty, ref _cellEditingCellTemplate, value);
    }
    
    private IDataTemplate? _cellEditingCellTemplate;
    
    /// <summary>
    /// Always <see langword="true" /> while <see cref="CellEditingTemplate" /> is <see langword="null" />;
    /// otherwise the value of the base member. A value that is set is stored either way and applies once an
    /// editing template is set.
    /// </summary>
    public override bool IsReadOnly
    {
        get
        {
            if (CellEditingTemplate is null)
            {
                return true;
            }

            return base.IsReadOnly;
        }
        set => base.IsReadOnly = value;
    }

    #endregion
    
    private bool _forceGenerateCellFromTemplate;
    
    /// <summary>
    /// Called by the grid when a cell of the column exits edit mode. It makes the next
    /// <see cref="GenerateElement" /> call build a new display element instead of reusing the current content of
    /// the cell, then calls the base member.
    /// </summary>
    protected override void EndCellEdit(Control editingElement)
    {
        // The next call to GenerateElement should not reuse the current content, as the cell needs to exit edit mode.
        _forceGenerateCellFromTemplate = true;
        base.EndCellEdit(editingElement);
    }

    /// <summary>
    /// Builds the display content of a cell with <see cref="CellTemplate" /> and the data item of the row the
    /// cell belongs to. A recycling template also receives the current content of the cell, except right after
    /// the cell left edit mode.
    /// </summary>
    /// <returns>
    /// The built element, or <see langword="null" /> in the designer when <see cref="CellTemplate" /> is
    /// <see langword="null" />. Outside the designer a missing template throws.
    /// </returns>
    protected override Control? GenerateElement(DataGridCell cell, object dataItem)
    {
        if (CellTemplate != null)
        {
            if (_forceGenerateCellFromTemplate)
            {
                _forceGenerateCellFromTemplate = false;
                return CellTemplate.Build(dataItem);
            }
            return (CellTemplate is IRecyclingDataTemplate recyclingDataTemplate)
                ? recyclingDataTemplate.Build(dataItem, cell.Content as Control)
                : CellTemplate.Build(dataItem);
        }
        if (Design.IsDesignMode)
        {
            return null;
        }
        throw DataGridError.DataGridTemplateColumn.MissingTemplateForType(typeof(DataGridTemplateColumn));
    }

    /// <summary>
    /// Builds the editing content of a cell with <see cref="CellEditingTemplate" />, or with
    /// <see cref="CellTemplate" /> when no editing template is set, and the data item of the row the cell
    /// belongs to. The column has no binding, so <paramref name="binding" /> is always <see langword="null" />.
    /// </summary>
    /// <returns>
    /// The built element, or <see langword="null" /> in the designer when no template is set. Outside the
    /// designer a missing template throws.
    /// </returns>
    protected override Control? GenerateEditingElement(DataGridCell cell, object dataItem, out BindingExpressionBase? binding)
    {
        binding = null;
        if(CellEditingTemplate != null)
        {
            return CellEditingTemplate.Build(dataItem);
        }
        if (CellTemplate != null)
        {
            return CellTemplate.Build(dataItem);
        }
        if (Design.IsDesignMode)
        {
            return null;
        }
        throw DataGridError.DataGridTemplateColumn.MissingTemplateForType(typeof(DataGridTemplateColumn));
    }

    /// <summary>
    /// Returns no unedited value when a cell of the column enters edit mode: the column is not bound to a cell
    /// value, so there is nothing to capture for a later cancel.
    /// </summary>
    /// <returns>
    /// Always <see langword="null" />.
    /// </returns>
    protected override object? PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        return null;
    }

    /// <summary>
    /// When <paramref name="propertyName" /> names <see cref="CellTemplate" /> and the element is the content of
    /// a cell, replaces the content of the cell with an element rebuilt by <see cref="GenerateElement" />. Calls
    /// the base member in every case.
    /// </summary>
    protected internal override void RefreshCellContent(Control element, string propertyName)
    {
        if (element.Parent is DataGridCell cell)
        {
            if(propertyName == nameof(CellTemplate))
            {
                Debug.Assert(cell.DataContext != null);
                cell.Content = GenerateElement(cell, cell.DataContext);
            }
        }
        base.RefreshCellContent(element, propertyName);
    }
}
