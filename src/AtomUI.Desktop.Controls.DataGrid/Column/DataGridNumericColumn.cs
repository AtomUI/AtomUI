using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A text column that edits a cell with a <see cref="NumericUpDown" /> bound to the value of the cell.
/// </summary>
/// <remarks>
/// <see cref="NumberFormat" />, <see cref="FormatString" />, <see cref="Maximum" /> and
/// <see cref="Minimum" /> are added from the properties of the editor and are bound to it while a cell is
/// edited. An unset value starts with the default of the editor, and a change also reaches editors that are
/// already open. The read-only text of a cell that is not being edited does not use them.
/// </remarks>
public class DataGridNumericColumn : DataGridAbstractTextColumn
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="NumberFormat" /> property.
    /// </summary>
    public static readonly StyledProperty<NumberFormatInfo?> NumberFormatProperty = 
        NumericUpDown.NumberFormatProperty.AddOwner<DataGridNumericColumn>();
    
    /// <summary>
    /// Identifies the <see cref="FormatString" /> property.
    /// </summary>
    public static readonly StyledProperty<string> FormatStringProperty = 
        NumericUpDown.FormatStringProperty.AddOwner<DataGridNumericColumn>();
    
    /// <summary>
    /// Identifies the <see cref="Maximum" /> property.
    /// </summary>
    public static readonly StyledProperty<Decimal> MaximumProperty =
        NumericUpDown.MaximumProperty.AddOwner<DataGridNumericColumn>();
    
    /// <summary>
    /// Identifies the <see cref="Minimum" /> property.
    /// </summary>
    public static readonly StyledProperty<Decimal> MinimumProperty =
        NumericUpDown.MinimumProperty.AddOwner<DataGridNumericColumn>();
    
    /// <summary>
    /// Number format that the <see cref="NumericUpDown" /> editing a cell of this column applies to its value.
    /// It does not format the read-only text of a cell that is not being edited.
    /// </summary>
    public NumberFormatInfo? NumberFormat
    {
        get => GetValue(NumberFormatProperty);
        set => SetValue(NumberFormatProperty, value);
    }
    
    /// <summary>
    /// Format string that the <see cref="NumericUpDown" /> editing a cell of this column applies to its value.
    /// It does not format the read-only text of a cell that is not being edited.
    /// </summary>
    public string FormatString
    {
        get => GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    /// <summary>
    /// Largest value that the <see cref="NumericUpDown" /> editing a cell of this column allows. It does not
    /// constrain the read-only text of a cell that is not being edited.
    /// </summary>
    public Decimal Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Smallest value that the <see cref="NumericUpDown" /> editing a cell of this column allows. It does not
    /// constrain the read-only text of a cell that is not being edited.
    /// </summary>
    public Decimal Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    
    #endregion
    
    public DataGridNumericColumn()
    {
        BindingTarget = NumericUpDown.ValueProperty;
    }

    /// <summary>
    /// Restores the text that the <see cref="NumericUpDown" /> editing the cell showed before the edit, or an
    /// empty string when the unedited value is not a string.
    /// </summary>
    protected override void CancelCellEdit(Control editingElement, object uneditedValue)
    {
        if (editingElement is NumericUpDown numericUpDown)
        {
            string? uneditedString = uneditedValue as string;
            numericUpDown.Text = uneditedString ?? string.Empty;
        }
    }

    /// <summary>
    /// Creates the <see cref="NumericUpDown" /> that edits a cell, applies the font and foreground of the column
    /// to it, and binds <see cref="NumberFormat" />, <see cref="FormatString" />, <see cref="Maximum" /> and
    /// <see cref="Minimum" /> of the column to it.
    /// </summary>
    protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
    {
        var numericUpDown = new NumericUpDown
        {                
            Name  = "CellNumberUpDown",
        };
        numericUpDown[!NumericUpDown.NumberFormatProperty] = this[!NumberFormatProperty];
        numericUpDown[!NumericUpDown.FormatStringProperty] = this[!FormatStringProperty];
        numericUpDown[!NumericUpDown.MaximumProperty]      = this[!MaximumProperty];
        numericUpDown[!NumericUpDown.MinimumProperty]      = this[!MinimumProperty];
      
        SyncProperties(numericUpDown);
        return numericUpDown;
    }

    /// <summary>
    /// Returns the text that the <see cref="NumericUpDown" /> editing the cell shows when editing begins, so that
    /// cancelling the edit can restore it.
    /// </summary>
    /// <returns>
    /// The text of the editor, or an empty string when the editing element is not a <see cref="NumericUpDown" />.
    /// </returns>
    protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        if (editingElement is NumericUpDown numericUpDown)
        {
            string uneditedText = numericUpDown.Text ?? string.Empty;
            return uneditedText;
        }
        return string.Empty;
    }
}