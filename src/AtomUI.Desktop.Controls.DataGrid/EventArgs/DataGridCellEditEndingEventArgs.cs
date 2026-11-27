// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.CellEditEnding" /> event, raised before the edit of a cell
/// ends with the given action. Cancelling the event aborts the commit or cancel of the cell edit.
/// </summary>
public class DataGridCellEditEndingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Instantiates a new instance of this class.
    /// </summary>
    /// <param name="column">
    /// The column of the cell that is about to exit edit mode.
    /// </param>
    /// <param name="row">
    /// The row container of the cell container that is about to exit edit mode.
    /// </param>
    /// <param name="editingElement">
    /// The editing element within the cell.
    /// </param>
    /// <param name="editAction">
    /// The editing action that will be taken.
    /// </param>
    public DataGridCellEditEndingEventArgs(DataGridColumn column,
                                           DataGridRow row,
                                           Control editingElement,
                                           DataGridEditAction editAction)
    {
        Column         = column;
        Row            = row;
        EditingElement = editingElement;
        EditAction     = editAction;
    }

    /// <summary>
    /// The column of the cell that is about to exit edit mode.
    /// </summary>
    public DataGridColumn Column
    {
        get;
        private set;
    }

    /// <summary>
    /// The edit action to take when leaving edit mode.
    /// </summary>
    public DataGridEditAction EditAction
    {
        get;
        private set;
    }

    /// <summary>
    /// The editing element within the cell. 
    /// </summary>
    public Control EditingElement
    {
        get;
        private set;
    }

    /// <summary>
    /// The row container of the cell container that is about to exit edit mode.
    /// </summary>
    public DataGridRow Row
    {
        get;
        private set;
    }

}