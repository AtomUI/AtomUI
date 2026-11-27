namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="TreeView.ItemDragStarted" /> event, raised once a pointer pressed on a node
/// moves past the drag threshold. The drag is already active and the node is already flagged as dragging.
/// </summary>
public class TreeViewDragStartedEventArgs : EventArgs
{
    /// <summary>
    /// The node that is being dragged.
    /// </summary>
    public TreeViewItem ViewItem { get; }

    /// <summary>
    /// Creates event data that reports <paramref name="viewItem" /> as the node whose drag has started.
    /// </summary>
    /// <param name="viewItem">
    /// The node that is being dragged.
    /// </param>
    public TreeViewDragStartedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}

/// <summary>
/// Data for the <see cref="TreeView.ItemDragCompleted" /> event, raised when the drag of a node ends,
/// whether or not the node was dropped. The tree and the node are no longer flagged as dragging, and a
/// drop that was performed has already been reported through <see cref="TreeView.ItemDropped" />.
/// </summary>
public class TreeViewDragCompletedEventArgs : EventArgs
{
    /// <summary>
    /// The node whose drag has ended.
    /// </summary>
    public TreeViewItem ViewItem { get; }

    /// <summary>
    /// Creates event data that reports <paramref name="viewItem" /> as the node whose drag has ended.
    /// </summary>
    /// <param name="viewItem">
    /// The node whose drag has ended.
    /// </param>
    public TreeViewDragCompletedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}

/// <summary>
/// Data for the <see cref="TreeView.ItemDragEnter" /> event, raised while a node is dragged and the node
/// under the pointer changes to another node of this tree. It follows <see cref="TreeView.ItemDragLeave" />
/// for the node the pointer left, when there was one. The new node is already flagged as drag-over.
/// </summary>
public class TreeViewDragEnterEventArgs : EventArgs
{
    /// <summary>
    /// The node that is being dragged.
    /// </summary>
    public TreeViewItem DraggedViewItem { get; }
    /// <summary>
    /// The node the pointer has just moved onto.
    /// </summary>
    public TreeViewItem DragOverViewItem { get; }

    /// <summary>
    /// Creates event data that reports the pointer entering <paramref name="dragOverViewItem" /> while
    /// <paramref name="draggedViewItem" /> is dragged.
    /// </summary>
    /// <param name="draggedViewItem">
    /// The node that is being dragged.
    /// </param>
    /// <param name="dragOverViewItem">
    /// The node the pointer has just moved onto.
    /// </param>
    public TreeViewDragEnterEventArgs(TreeViewItem draggedViewItem, TreeViewItem dragOverViewItem)
    {
        DraggedViewItem  = draggedViewItem;
        DragOverViewItem = dragOverViewItem;
    }
}

/// <summary>
/// Data for the <see cref="TreeView.ItemDragLeave" /> event, raised while a node is dragged and the pointer
/// leaves the node it was over. That node is no longer flagged as drag-over, and the node the pointer
/// moved to, if any, is reported afterwards through <see cref="TreeView.ItemDragEnter" />. Ending the drag
/// over a node does not raise this event.
/// </summary>
public class TreeViewDragLeaveEventArgs : EventArgs
{
    /// <summary>
    /// The node that is being dragged.
    /// </summary>
    public TreeViewItem DraggedViewItem { get; }
    /// <summary>
    /// The node the pointer has just left.
    /// </summary>
    public TreeViewItem DragOverViewItem { get; }

    /// <summary>
    /// Creates event data that reports the pointer leaving <paramref name="dragOverViewItem" /> while
    /// <paramref name="draggedViewItem" /> is dragged.
    /// </summary>
    /// <param name="draggedViewItem">
    /// The node that is being dragged.
    /// </param>
    /// <param name="dragOverViewItem">
    /// The node the pointer has just left.
    /// </param>
    public TreeViewDragLeaveEventArgs(TreeViewItem draggedViewItem, TreeViewItem dragOverViewItem)
    {
        DraggedViewItem  = draggedViewItem;
        DragOverViewItem = dragOverViewItem;
    }
}

/// <summary>
/// Data for the <see cref="TreeView.ItemDragOver" /> event, raised while a node is dragged and the pointer
/// moves within the node it is already over. The move that brings the pointer onto a node raises
/// <see cref="TreeView.ItemDragEnter" /> instead.
/// </summary>
public class TreeViewDragOverEventArgs : EventArgs
{
    /// <summary>
    /// The node that is being dragged.
    /// </summary>
    public TreeViewItem DraggedViewItem { get; }
    /// <summary>
    /// The node the pointer is still over.
    /// </summary>
    public TreeViewItem DragOverViewItem { get; }

    /// <summary>
    /// Creates event data that reports the pointer moving within <paramref name="dragOverViewItem" /> while
    /// <paramref name="draggedViewItem" /> is dragged.
    /// </summary>
    /// <param name="draggedViewItem">
    /// The node that is being dragged.
    /// </param>
    /// <param name="dragOverViewItem">
    /// The node the pointer is still over.
    /// </param>
    public TreeViewDragOverEventArgs(TreeViewItem draggedViewItem, TreeViewItem dragOverViewItem)
    {
        DraggedViewItem  = draggedViewItem;
        DragOverViewItem = dragOverViewItem;
    }
}

/// <summary>
/// Data for the <see cref="TreeView.ItemDropped" /> event, raised after the dragged node has been
/// inserted at its drop target. It is not raised when the target is the dragged node or one of its
/// descendants, nor when the tree is populated from an items source and the data did not accept the move.
/// </summary>
public class TreeViewDroppedEventArgs : EventArgs
{
    /// <summary>
    /// The node that was dragged.
    /// </summary>
    public TreeViewItem DraggedViewItem { get; }
    /// <summary>
    /// The node whose items received the dragged node, or <see langword="null" /> when the node was dropped
    /// at the root level of the tree.
    /// </summary>
    public TreeViewItem? DroppedItem { get; }
    
    /// <summary>
    /// The position of the drop within the items of <see cref="DroppedItem" />, or within the root items of
    /// the tree when it is <see langword="null" />. For a drop beside a node, it is that node's index when the
    /// drop is before that node and that index plus one when the drop is after it. A drop into a node uses
    /// its child count, which appends the dragged node. The index is counted before the dragged node leaves
    /// its old parent, so after a move within one parent the final index can be one less.
    /// </summary>
    public int DropIndex { get; }

    /// <summary>
    /// Creates event data that reports where <paramref name="draggedViewItem" /> was dropped.
    /// </summary>
    /// <param name="draggedViewItem">
    /// The node that was dragged.
    /// </param>
    /// <param name="droppedItem">
    /// The node whose items received the dragged node, or <see langword="null" /> for a drop at the root level
    /// of the tree.
    /// </param>
    /// <param name="dropIndex">
    /// The position of the drop within the target items, counted before the dragged node leaves its old
    /// parent.
    /// </param>
    public TreeViewDroppedEventArgs(TreeViewItem draggedViewItem, TreeViewItem? droppedItem, int dropIndex)
    {
        DraggedViewItem = draggedViewItem;
        DroppedItem = droppedItem;
        DropIndex   = dropIndex;
    }
}