namespace AtomUI.Desktop.Controls;

public class TreeViewDragStartedEventArgs : EventArgs
{
    public TreeViewItem ViewItem { get; }

    public TreeViewDragStartedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}

public class TreeViewDragCompletedEventArgs : EventArgs
{
    public TreeViewItem ViewItem { get; }

    public TreeViewDragCompletedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}

public class TreeViewDragEnterEventArgs : EventArgs
{
    public TreeViewItem DraggedViewItem { get; }
    public TreeViewItem DragOverViewItem { get; }

    public TreeViewDragEnterEventArgs(TreeViewItem draggedViewItem, TreeViewItem dragOverViewItem)
    {
        DraggedViewItem  = draggedViewItem;
        DragOverViewItem = dragOverViewItem;
    }
}

public class TreeViewDragLeaveEventArgs : EventArgs
{
    public TreeViewItem DraggedViewItem { get; }
    public TreeViewItem DragOverViewItem { get; }

    public TreeViewDragLeaveEventArgs(TreeViewItem draggedViewItem, TreeViewItem dragOverViewItem)
    {
        DraggedViewItem  = draggedViewItem;
        DragOverViewItem = dragOverViewItem;
    }
}

public class TreeViewDragOverEventArgs : EventArgs
{
    public TreeViewItem DraggedViewItem { get; }
    public TreeViewItem DragOverViewItem { get; }

    public TreeViewDragOverEventArgs(TreeViewItem draggedViewItem, TreeViewItem dragOverViewItem)
    {
        DraggedViewItem  = draggedViewItem;
        DragOverViewItem = dragOverViewItem;
    }
}

public class TreeViewDroppedEventArgs : EventArgs
{
    public TreeViewItem DraggedViewItem { get; }
    public TreeViewItem? DroppedItem { get; }
    
    public int DropIndex { get; }

    public TreeViewDroppedEventArgs(TreeViewItem draggedViewItem, TreeViewItem? droppedItem, int dropIndex)
    {
        DraggedViewItem = draggedViewItem;
        DroppedItem = droppedItem;
        DropIndex   = dropIndex;
    }
}