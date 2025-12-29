using AtomUI.Controls;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public record TreeViewItemData : ITreeViewItemData
{
    public ITreeNode<ITreeViewItemData>? ParentNode { get; private set; }
    public TreeNodeKey? ItemKey { get; init; }
    public object? Header { get; init; }
    public PathIcon? Icon { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool? IsChecked { get; init; } = false;
    public bool IsSelected { get; init; }
    public bool IsExpanded { get; init; }
    public bool IsIndicatorEnabled { get; init; } = true;
    
    public string? GroupName { get; init; }
    public bool IsLeaf { get; init; } = false;

    private IList<ITreeViewItemData> _children = [];
    public IList<ITreeViewItemData> Children
    {
        get => _children;
        init
        {
            _children = value;
            foreach (var child in _children)
            {
                if (child is TreeViewItemData item)
                {
                    item.ParentNode = this;
                }
            }
        }
    }
    
    public void UpdateParentNode(ITreeViewItemData? parentNode)
    {
        ParentNode = parentNode;
    }
}