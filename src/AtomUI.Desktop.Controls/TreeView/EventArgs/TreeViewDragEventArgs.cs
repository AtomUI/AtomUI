namespace AtomUI.Desktop.Controls;

public class TreeViewDragStartedEventArgs : EventArgs
{
    public TreeItem Item { get; }

    public TreeViewDragStartedEventArgs(TreeItem item)
    {
        Item = item;
    }
}

public class TreeViewDragCompletedEventArgs : EventArgs
{
    public TreeItem Item { get; }

    public TreeViewDragCompletedEventArgs(TreeItem item)
    {
        Item = item;
    }
}

public class TreeViewDragEnterEventArgs : EventArgs
{
    public TreeItem DraggedItem { get; }
    public TreeItem DragOverItem { get; }

    public TreeViewDragEnterEventArgs(TreeItem draggedItem, TreeItem dragOverItem)
    {
        DraggedItem  = draggedItem;
        DragOverItem = dragOverItem;
    }
}

public class TreeViewDragLeaveEventArgs : EventArgs
{
    public TreeItem DraggedItem { get; }
    public TreeItem DragOverItem { get; }

    public TreeViewDragLeaveEventArgs(TreeItem draggedItem, TreeItem dragOverItem)
    {
        DraggedItem  = draggedItem;
        DragOverItem = dragOverItem;
    }
}

public class TreeViewDragOverEventArgs : EventArgs
{
    public TreeItem DraggedItem { get; }
    public TreeItem DragOverItem { get; }

    public TreeViewDragOverEventArgs(TreeItem draggedItem, TreeItem dragOverItem)
    {
        DraggedItem  = draggedItem;
        DragOverItem = dragOverItem;
    }
}

public class TreeViewDroppedEventArgs : EventArgs
{
    public TreeItem DraggedItem { get; }
    public TreeItem? DroppedItem { get; }
    
    public int DropIndex { get; }

    public TreeViewDroppedEventArgs(TreeItem draggedItem, TreeItem? droppedItem, int dropIndex)
    {
        DraggedItem = draggedItem;
        DroppedItem = droppedItem;
        DropIndex   = dropIndex;
    }
}