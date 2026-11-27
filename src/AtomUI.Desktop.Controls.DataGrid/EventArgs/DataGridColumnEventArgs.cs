// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries a column for the column events of the grid: a changed display index, the end of a column
/// reorder, and a sorting or filtering request. A handler that sorts or filters the data itself sets
/// <see cref="HandledEventArgs.Handled" /> to stop the grid from applying its own.
/// </summary>
public class DataGridColumnEventArgs : HandledEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridColumnEventArgs" /> class.
    /// </summary>
    /// <param name="column">
    /// The column that the event occurs for.
    /// </param>
    public DataGridColumnEventArgs(DataGridColumn column)
    {
        Column = column ?? throw new ArgumentNullException(nameof(column));
    }

    /// <summary>
    /// Gets the column that the event occurs for.
    /// </summary>
    public DataGridColumn Column
    {
        get;
        private set;
    }
}