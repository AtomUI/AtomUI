namespace AtomUI.Desktop.Controls;

public class TreeViewDragStartedEventArgs : EventArgs
{
    public TreeViewItem Item { get; }

    public TreeViewDragStartedEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}

public class TreeViewDragCompletedEventArgs : EventArgs
{
    public TreeViewItem Item { get; }

    public TreeViewDragCompletedEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}

public class TreeViewDragEnterEventArgs : EventArgs
{
    public TreeViewItem DraggedItem { get; }
    public TreeViewItem DragOverItem { get; }

    public TreeViewDragEnterEventArgs(TreeViewItem draggedItem, TreeViewItem dragOverItem)
    {
        DraggedItem  = draggedItem;
        DragOverItem = dragOverItem;
    }
}

public class TreeViewDragLeaveEventArgs : EventArgs
{
    public TreeViewItem DraggedItem { get; }
    public TreeViewItem DragOverItem { get; }

    public TreeViewDragLeaveEventArgs(TreeViewItem draggedItem, TreeViewItem dragOverItem)
    {
        DraggedItem  = draggedItem;
        DragOverItem = dragOverItem;
    }
}

public class TreeViewDragOverEventArgs : EventArgs
{
    public TreeViewItem DraggedItem { get; }
    public TreeViewItem DragOverItem { get; }

    public TreeViewDragOverEventArgs(TreeViewItem draggedItem, TreeViewItem dragOverItem)
    {
        DraggedItem  = draggedItem;
        DragOverItem = dragOverItem;
    }
}

public class TreeViewDroppedEventArgs : EventArgs
{
    public TreeViewItem DraggedItem { get; }
    public TreeViewItem? DroppedItem { get; }
    
    public int DropIndex { get; }

    public TreeViewDroppedEventArgs(TreeViewItem draggedItem, TreeViewItem? droppedItem, int dropIndex)
    {
        DraggedItem = draggedItem;
        DroppedItem = droppedItem;
        DropIndex   = dropIndex;
    }
}