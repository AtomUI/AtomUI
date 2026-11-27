namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes a group item that was added to or removed from the children of a column group item, for the
/// <see cref="IDataGridColumnGroupItem.GroupChanged" /> event.
/// </summary>
public class DataGridColumnGroupChangedArgs : EventArgs
{
    /// <summary>
    /// The group item that was added or removed. It is a child of the group item that raised the event.
    /// </summary>
    public IDataGridColumnGroupItem GroupItem { get; }
    /// <summary>
    /// Whether <see cref="GroupItem" /> was added to or removed from the group item that raised the event.
    /// </summary>
    public NotifyColumnGroupChangedType ChangedType { get; }

    /// <summary>
    /// Creates the arguments for the change of one child group item.
    /// </summary>
    /// <param name="item">
    /// The group item that was added or removed.
    /// </param>
    /// <param name="changedType">
    /// Whether the item was added or removed.
    /// </param>
    public DataGridColumnGroupChangedArgs(IDataGridColumnGroupItem item, NotifyColumnGroupChangedType changedType)
    {
        GroupItem   = item;   
        ChangedType = changedType;
    }
}

/// <summary>
/// The kind of change of the children of a column group item.
/// </summary>
public enum NotifyColumnGroupChangedType
{
    /// <summary>
    /// A group item was added to the children of the group item that raised the event.
    /// </summary>
    Add,
    /// <summary>
    /// A group item was removed from the children of the group item that raised the event.
    /// </summary>
    Remove,
}