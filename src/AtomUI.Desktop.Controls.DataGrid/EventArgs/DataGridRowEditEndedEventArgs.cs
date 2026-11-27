// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes a row edit that has ended, for the <see cref="DataGrid.RowEditEnded" /> event, which the grid
/// raises after the row edit has been committed or canceled.
/// </summary>
public class DataGridRowEditEndedEventArgs : EventArgs
{
    /// <summary>
    /// Instantiates a new instance of this class.
    /// </summary>
    /// <param name="row">
    /// The row container of the cell container that has just exited edit mode.
    /// </param>
    /// <param name="editAction">
    /// The editing action that has been taken.
    /// </param>
    public DataGridRowEditEndedEventArgs(DataGridRow row, DataGridEditAction editAction)
    {
        Row        = row;
        EditAction = editAction;
    }

    /// <summary>
    /// The editing action that has been taken.
    /// </summary>
    public DataGridEditAction EditAction
    {
        get;
        private set;
    }

    /// <summary>
    /// The row container of the cell container that has just exited edit mode.
    /// </summary>
    public DataGridRow Row
    {
        get;
        private set;
    }
}