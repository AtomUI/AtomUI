// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.PreparingCellForEdit" /> event, raised when a cell of a
/// template column enters edit mode.
/// </summary>
public class DataGridPreparingCellForEditEventArgs: EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridPreparingCellForEditEventArgs" /> class.
    /// </summary>
    /// <param name="column">
    /// The column that contains the cell to be edited.
    /// </param>
    /// <param name="row">
    /// The row that contains the cell to be edited.
    /// </param>
    /// <param name="editingEventArgs">
    /// Information about the user gesture that caused the cell to enter edit mode.
    /// </param>
    /// <param name="editingElement">
    /// The element that the column displays for a cell in editing mode.
    /// </param>
    public DataGridPreparingCellForEditEventArgs(DataGridColumn column,
                                                 DataGridRow row,
                                                 RoutedEventArgs editingEventArgs,
                                                 Control editingElement)
    {
        Column           = column;
        Row              = row;
        EditingEventArgs = editingEventArgs;
        EditingElement   = editingElement;
    }

    /// <summary>
    /// Gets the column that contains the cell to be edited.
    /// </summary>
    public DataGridColumn Column
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the element that the column displays for a cell in editing mode.
    /// </summary>
    public Control EditingElement
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets information about the user gesture that caused the cell to enter edit mode.
    /// </summary>
    public RoutedEventArgs EditingEventArgs
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the row that contains the cell to be edited.
    /// </summary>
    public DataGridRow Row
    {
        get;
        private set;
    }

}