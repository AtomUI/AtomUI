// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A column that displays the bound value as text in each cell and edits it with a <see cref="LineEdit" />.
/// </summary>
public class DataGridTextColumn : DataGridAbstractTextColumn
{
    public DataGridTextColumn()
    {
        BindingTarget = LineEdit.TextProperty;
    }
    
    /// <summary>
    /// Causes the column cell being edited to revert to the specified value.
    /// </summary>
    /// <param name="editingElement">
    /// The element that the column displays for a cell in editing mode.
    /// </param>
    /// <param name="uneditedValue">
    /// The previous, unedited value in the cell being edited.
    /// </param>
    protected override void CancelCellEdit(Control editingElement, object uneditedValue)
    {
        if (editingElement is LineEdit lineEdit)
        {
            string? uneditedString = uneditedValue as string;
            lineEdit.Text = uneditedString ?? string.Empty;
        }
    }
    
    /// <summary>
    /// Gets a <see cref="T:AtomUI.Desktop.Controls.TextBox" /> control that is bound to the column's <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </summary>
    /// <param name="cell">
    /// The cell that will contain the generated element.
    /// </param>
    /// <param name="dataItem">
    /// The data item represented by the row that contains the intended cell.
    /// </param>
    /// <returns>
    /// A new <see cref="T:AtomUI.Desktop.Controls.TextBox" /> control that is bound to the column's
    /// <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </returns>
    protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
    {
        var lineEdit = new LineEdit
        {                
            Name = "CellTextBox"
        };
        SyncProperties(lineEdit);
        return lineEdit;
    }

    /// <summary>
    /// Called when the cell in the column enters editing mode.
    /// </summary>
    /// <param name="editingElement">
    /// The element that the column displays for a cell in editing mode.
    /// </param>
    /// <param name="editingEventArgs">
    /// Information about the user gesture that is causing a cell to enter editing mode.
    /// </param>
    /// <returns>
    /// The unedited value.
    /// </returns>
    protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        if (editingElement is LineEdit lineEdit)
        {
            string uneditedText = lineEdit.Text ?? string.Empty;
            int    len          = uneditedText.Length;
            if (editingEventArgs is KeyEventArgs keyEventArgs && keyEventArgs.Key == Key.F2)
            {
                // Put caret at the end of the text
                lineEdit.SelectionStart = len;
                lineEdit.SelectionEnd   = len;
            }
            else
            {
                // Select all text
                lineEdit.SelectionStart = 0;
                lineEdit.SelectionEnd   = len;
                lineEdit.CaretIndex     = len;
            }

            return uneditedText;
        }
        return string.Empty;
    }
}