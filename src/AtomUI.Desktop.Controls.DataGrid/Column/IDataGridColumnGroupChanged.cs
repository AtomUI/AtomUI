namespace AtomUI.Desktop.Controls;

/// <summary>
/// The contract of a column group item that reports when an item was added to or removed from the group
/// tree at or below it.
/// </summary>
public interface IDataGridColumnGroupChanged
{
    /// <summary>
    /// Raised after a child item was added to or removed from this group. On removal the child's
    /// <c>GroupParent</c> is still set when the event is raised. A group forwards the events of its nested
    /// groups upward, so the <see cref="DataGrid" /> only needs to subscribe to the root item.
    /// </summary>
    event EventHandler<DataGridColumnGroupChangedArgs> GroupChanged;
}