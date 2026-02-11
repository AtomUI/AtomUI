using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public record TreeItemNode : ITreeItemNode, ISelectTagTextProvider
{
    public ITreeNode<ITreeItemNode>? ParentNode { get; private set; }
    public TreeNodeKey? ItemKey { get; init; }
    public object? Header { get; init; }
    public PathIcon? Icon { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool? IsChecked { get; set; } = false;
    public bool IsSelected { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsIndicatorEnabled { get; set; } = true;
    public string? GroupName { get; init; }
    public bool IsLeaf { get; init; }
    public object? Value { get; set; }
    string? ISelectTagTextProvider.TagText => Header?.ToString();

    private IList<ITreeItemNode> _children = [];
    
    [Content]
    public IList<ITreeItemNode> Children
    {
        get => _children;
        init
        {
            _children = value;
            foreach (var child in _children)
            {
                if (child is TreeItemNode item)
                {
                    item.ParentNode = this;
                }
            }
        }
    }
    
    public void UpdateParentNode(ITreeItemNode? parentNode)
    {
        ParentNode = parentNode;
    }
}