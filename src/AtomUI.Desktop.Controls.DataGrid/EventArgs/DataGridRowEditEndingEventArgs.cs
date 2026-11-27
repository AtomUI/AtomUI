// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.RowEditEnding" /> event, raised before the edit of a row
/// ends with the given action. Cancelling the event aborts the commit or cancel of the row edit.
/// </summary>
public class DataGridRowEditEndingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Instantiates a new instance of this class.
    /// </summary>
    /// <param name="row">
    /// The row container of the cell container that is about to exit edit mode.
    /// </param>
    /// <param name="editAction">
    /// The editing action that will be taken.
    /// </param>
    public DataGridRowEditEndingEventArgs(DataGridRow row, DataGridEditAction editAction)
    {
        Row        = row;
        EditAction = editAction;
    }

    /// <summary>
    /// The editing action that will be taken.
    /// </summary>
    public DataGridEditAction EditAction
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