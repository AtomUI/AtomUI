using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public interface ICascaderViewOption : ITreeNode<ICascaderViewOption>
{
    new object? Header { get; set; }
    bool? IsChecked { get; set; }
    bool IsCheckBoxEnabled { get; set; }
    bool IsExpanded { get; set; }
    bool IsLeaf { get; set; }
    object? Value { get; set; }
    
    void UpdateParentNode(ICascaderViewOption? parentNode)
    {
        throw new NotImplementedException();
    }
}

public record CascaderViewOption : ICascaderViewOption, ISelectTagTextProvider
{
    public ITreeNode<ICascaderViewOption>? ParentNode { get; private set; }
    
    public TreeNodeKey? ItemKey { get; init; }
    public object? Header { get; set; }
    public PathIcon? Icon { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool? IsChecked { get; set; } = false;
    public bool IsLeaf { get; set; } = false;
    public bool IsExpanded { get; set; }
    public bool IsCheckBoxEnabled { get; set; } = true;
    public object? Value { get; set; }
    
    string? ISelectTagTextProvider.TagText => Header?.ToString();
    private readonly AvaloniaList<ICascaderViewOption> _children = [];
    
    [Content]
    public IList<ICascaderViewOption> Children
    {
        get => _children;
        init => _children.AddRange(value);
    }
    
    public void UpdateParentNode(ICascaderViewOption? parentNode)
    {
        ParentNode = parentNode;
    }

    public CascaderViewOption()
    {
        _children.CollectionChanged += HandleCollectionChanged;
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems != null)
            {
                foreach (var child in e.NewItems)
                {
                    if (child is ICascaderViewOption option)
                    {
                        option.UpdateParentNode(this);
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
                    if (child is ICascaderViewOption option)
                    {
                        option.UpdateParentNode(null);
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