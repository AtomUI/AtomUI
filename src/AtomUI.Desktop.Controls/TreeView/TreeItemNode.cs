using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia.Collections;
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
    
    private readonly AvaloniaList<ITreeItemNode> _children = [];
    
    [Content]
    public IList<ITreeItemNode> Children
    {
        get => _children;
        init => _children.AddRange(value);
    }

    public TreeItemNode()
    {
        _children.CollectionChanged += HandleCollectionChanged;
    }
    
    public void UpdateParentNode(ITreeItemNode? parentNode)
    {
        ParentNode = parentNode;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems != null)
            {
                foreach (var child in e.NewItems)
                {
                    if (child is ITreeItemNode treeNode)
                    {
                        treeNode.UpdateParentNode(this);
                    }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var child in e.OldItems)
                {
                    if (child is ITreeItemNode treeNode)
                    {
                        treeNode.UpdateParentNode(null);
                    }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var child in Children)
            {
                child.UpdateParentNode(this);
            }
        }
    }
}