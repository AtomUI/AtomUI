// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries a group header for the <see cref="DataGrid.LoadingRowGroup" /> and
/// <see cref="DataGrid.UnloadingRowGroup" /> events.
/// </summary>
public class DataGridRowGroupHeaderEventArgs : EventArgs
{
    /// <summary>
    /// Constructs a DataGridRowGroupHeaderEventArgs instance
    /// </summary>
    public DataGridRowGroupHeaderEventArgs(DataGridRowGroupHeader rowGroupHeader)
    {
        RowGroupHeader = rowGroupHeader;
    }

    /// <summary>
    /// DataGridRowGroupHeader associated with this instance
    /// </summary>
    public DataGridRowGroupHeader RowGroupHeader
    {
        get;
        private set;
    }
}