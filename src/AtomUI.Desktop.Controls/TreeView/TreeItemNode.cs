using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Node data for a <see cref="TreeView" /> without change notification. The <see cref="TreeViewItem" /> that
/// presents the node reads the values of the node once when it is prepared for the node, so a change made
/// afterwards is not noticed until the container is prepared again; it writes its own changes of
/// <see cref="IsChecked" />, <see cref="IsSelected" /> and <see cref="IsExpanded" /> back into the node.
/// <see cref="BindableTreeItemNode" /> synchronizes in both directions instead. As a record, a node equals
/// another only when all its fields are equal, which includes the mutable state such as <c>IsChecked</c>.
/// </summary>
public record TreeItemNode : ITreeItemNode, ISelectTagTextProvider
{
    /// <summary>
    /// The parent of this node, or <see langword="null" /> while the node is not in the children of another node.
    /// It only changes through <see cref="UpdateParentNode" />, which this node calls on the nodes that are added
    /// to or removed from <see cref="Children" />.
    /// </summary>
    public ITreeNode<ITreeItemNode>? ParentNode { get; private set; }
    /// <summary>
    /// The key that identifies this node in the node paths that the <see cref="TreeView" /> traverses to locate a
    /// node and builds to replay state. When it is <see langword="null" />, the segment of this node in a built
    /// path is taken from <see cref="Value" /> instead.
    /// </summary>
    public EntityKey? ItemKey { get; init; }
    /// <summary>
    /// The content that the node row presents. The default data template of the <see cref="TreeView" /> shows it
    /// as text, and <see cref="ISelectTagTextProvider.TagText" /> returns its string form.
    /// </summary>
    public object? Header { get; init; }
    /// <summary>
    /// The icon shown in the node row. It is visible only while <see cref="TreeView.IsShowIcon" /> is
    /// <see langword="true" /> and this value is not <see langword="null" />.
    /// </summary>
    public PathIcon? Icon { get; init; }
    /// <summary>
    /// Whether the node accepts interaction; the initial value is <see langword="true" />. The presenting
    /// <see cref="TreeViewItem" /> copies the value into its own <c>IsEnabled</c>, and the <see cref="TreeView" />
    /// skips a disabled node during keyboard navigation.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    /// <summary>
    /// The check state of the node: <see langword="true" /> for checked and <see langword="false" /> for
    /// unchecked; the initial value is <see langword="false" />. <see cref="TreeView.CheckedItems" /> owns the
    /// checked state and writes the state of the node into this property.
    /// </summary>
    public bool? IsChecked { get; set; } = false;
    /// <summary>
    /// Whether the node is selected. The <see cref="TreeView" /> writes its selection into this property, and the
    /// <see cref="TreeViewItem" /> that presents the node keeps it in step with its own selection.
    /// </summary>
    public bool IsSelected { get; set; }
    /// <summary>
    /// Whether the node is expanded and shows its children. The <see cref="TreeViewItem" /> that presents the node
    /// writes its own expansion state into this property.
    /// </summary>
    public bool IsExpanded { get; set; }
    /// <summary>
    /// Whether the node shows its check indicator, the check box or the radio button of the node; the initial
    /// value is <see langword="true" />. The <see cref="TreeView" /> treats a node without the indicator as not
    /// checkable.
    /// </summary>
    public bool IsIndicatorEnabled { get; set; } = true;
    /// <summary>
    /// The group of the radio button that serves as check indicator of the node. Nodes that share a group name
    /// accept only one checked node among them; <see langword="null" /> or an empty name puts the node in no
    /// group.
    /// </summary>
    public string? GroupName { get; init; }
    /// <summary>
    /// Whether the node is a leaf, a node without children. A leaf node shows the leaf icon instead of the
    /// switcher that expands a node.
    /// </summary>
    public bool IsLeaf { get; init; }
    /// <summary>
    /// A value that the application stores with the node. It also serves as the segment of this node in a built
    /// node path when <see cref="ItemKey" /> is <see langword="null" />.
    /// </summary>
    public object? Value { get; set; }
    string? ISelectTagTextProvider.TagText => Header?.ToString();
    
    private readonly AvaloniaList<ITreeItemNode> _children = [];
    
    /// <summary>
    /// The child nodes. A list assigned to the property is copied into the list of the node, so later changes to
    /// the assigned list are not seen. A node added to <see cref="Children" /> gets this node as parent; a node
    /// removed from it loses its parent even when another node has become its parent in the meantime.
    /// </summary>
    [Content]
    public IList<ITreeItemNode> Children
    {
        get => _children;
        init => _children.AddRange(value);
    }

    IEnumerable<ITreeItemNode> ITreeNode<ITreeItemNode>.Children => Children;

    public TreeItemNode()
    {
        _children.CollectionChanged += HandleCollectionChanged;
    }
    
    /// <summary>
    /// Makes <paramref name="parentNode" /> the <see cref="ParentNode" /> of this node. The <see cref="TreeView" />
    /// calls it when it moves a node, and another node calls it when this node is added to or removed from its
    /// children.
    /// </summary>
    /// <param name="parentNode">
    /// The new parent node, or <see langword="null" /> when the node no longer has a parent.
    /// </param>
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
                for (var i = 0; i < e.NewItems.Count; i++)
                {
                    if (e.NewItems[i] is ITreeItemNode treeNode)
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
                for (var i = 0; i < e.OldItems.Count; i++)
                {
                    if (e.OldItems[i] is ITreeItemNode treeNode)
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
