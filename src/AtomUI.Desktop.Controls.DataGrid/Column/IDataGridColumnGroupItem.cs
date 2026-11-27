using System.Collections.ObjectModel;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The contract of a node of the column group tree of a <see cref="DataGrid" />: a group that nests
/// columns and other groups, or a column that a group contains.
/// </summary>
public interface IDataGridColumnGroupItem
{
    /// <summary>
    /// Content that the header of this item shows: the header of a column, or the header above the nested
    /// columns of a group.
    /// </summary>
    object? Header { get; set; }
    /// <summary>
    /// Template that presents the <see cref="Header" />, or <see langword="null" /> to let the header use its
    /// default presentation.
    /// </summary>
    IDataTemplate? HeaderTemplate { get; set; }
    /// <summary>
    /// The group that contains this item, or <see langword="null" /> when no group contains it: a root group,
    /// or a column that is used outside a group.
    /// </summary>
    IDataGridColumnGroupItem? GroupParent { get; set; }
    /// <summary>
    /// The columns and groups nested in this item. Adding an item to the collection nests it in the group
    /// and sets the <see cref="GroupParent" /> of the item. A column has no children, so its collection is
    /// empty.
    /// </summary>
    ObservableCollection<IDataGridColumnGroupItem> GroupChildren { get; }
    /// <summary>
    /// Raised after an item was added to or removed from the <see cref="GroupChildren" /> of a group. When an
    /// item is removed, its <see cref="GroupParent" /> is still set when the event is raised.
    /// A group forwards the events of its nested groups upward, so a subscriber of a root group receives every
    /// change below it.
    /// </summary>
    event EventHandler<DataGridColumnGroupChangedArgs>? GroupChanged;
}

internal interface IDataGridColumnGroupItemInternal : IDataGridColumnGroupItem
{
    DataGridHeaderViewItem? GroupHeaderViewItem { get; internal set; }
}