// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.BeginningEdit" /> event, raised before a cell or row enters
/// edit mode. Cancelling the event keeps the cell from entering edit mode.
/// </summary>
public class DataGridBeginningEditEventArgs: CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="T:AtomUI.Desktop.Controls.DataGridBeginningEditEventArgs" /> class.
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
    public DataGridBeginningEditEventArgs(DataGridColumn column,
                                          DataGridRow row,
                                          RoutedEventArgs editingEventArgs)
    {
        Column           = column;
        Row              = row;
        EditingEventArgs = editingEventArgs;
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