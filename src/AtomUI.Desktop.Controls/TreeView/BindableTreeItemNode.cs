using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Node data for a <see cref="TreeView" /> that keeps its state in Avalonia properties, so that every change
/// raises a change notification. The <see cref="TreeViewItem" /> that presents the node reads the values of the
/// node into its own properties and follows later changes of the node, and it writes its own changes of
/// <see cref="IsChecked" />, <see cref="IsSelected" /> and <see cref="IsExpanded" /> back into the node.
/// <see cref="TreeItemNode" /> is the counterpart without change notification.
/// </summary>
[GenerateScopedResourceHost]
public partial class BindableTreeItemNode : AvaloniaObject, ITreeItemNode, ISelectTagTextProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ItemKey" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, EntityKey?> ItemKeyProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, EntityKey?>(
            nameof(ItemKey),
            o => o.ItemKey,
            (o, v) => o.ItemKey = v);

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, PathIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, PathIcon?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);

    /// <summary>
    /// Identifies the <see cref="IsEnabled" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, bool>(
            nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);

    /// <summary>
    /// Identifies the <see cref="IsChecked" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, bool?> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, bool?>(
            nameof(IsChecked),
            o => o.IsChecked,
            (o, v) => o.IsChecked = v);

    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, bool>(
            nameof(IsSelected),
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);

    /// <summary>
    /// Identifies the <see cref="IsExpanded" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, bool>(
            nameof(IsExpanded),
            o => o.IsExpanded,
            (o, v) => o.IsExpanded = v);

    /// <summary>
    /// Identifies the <see cref="IsIndicatorEnabled" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, bool> IsIndicatorEnabledProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, bool>(
            nameof(IsIndicatorEnabled),
            o => o.IsIndicatorEnabled,
            (o, v) => o.IsIndicatorEnabled = v);

    /// <summary>
    /// Identifies the <see cref="GroupName" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, string?> GroupNameProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, string?>(
            nameof(GroupName),
            o => o.GroupName,
            (o, v) => o.GroupName = v);

    /// <summary>
    /// Identifies the <see cref="IsLeaf" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, bool>(
            nameof(IsLeaf),
            o => o.IsLeaf,
            (o, v) => o.IsLeaf = v);

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, object?>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);

    /// <summary>
    /// Identifies the <see cref="Children" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableTreeItemNode, IList<ITreeItemNode>> ChildrenProperty =
        AvaloniaProperty.RegisterDirect<BindableTreeItemNode, IList<ITreeItemNode>>(
            nameof(Children),
            o => o.Children,
            (o, v) => o.Children = v);

    private EntityKey? _itemKey;

    /// <summary>
    /// The key that identifies this node in the node paths that the <see cref="TreeView" /> traverses to locate a
    /// node and builds to replay state. When it is <see langword="null" />, the segment of this node in a built
    /// path is taken from <see cref="Value" /> instead.
    /// </summary>
    public EntityKey? ItemKey
    {
        get => _itemKey;
        set => SetAndRaise(ItemKeyProperty, ref _itemKey, value);
    }

    private object? _header;

    /// <summary>
    /// The content that the node row presents. The default data template of the <see cref="TreeView" /> shows it
    /// as text, and <see cref="ISelectTagTextProvider.TagText" /> returns its string form.
    /// </summary>
    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }

    private PathIcon? _icon;

    /// <summary>
    /// The icon shown in the node row. It is visible only while <see cref="TreeView.IsShowIcon" /> is
    /// <see langword="true" /> and this value is not <see langword="null" />.
    /// </summary>
    public PathIcon? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }

    private bool _isEnabled = true;

    /// <summary>
    /// Whether the node accepts interaction; the initial value is <see langword="true" />. The presenting
    /// <see cref="TreeViewItem" /> copies the value into its own <c>IsEnabled</c>, and the <see cref="TreeView" />
    /// skips a disabled node during keyboard navigation.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }

    private bool? _isChecked = false;

    /// <summary>
    /// The check state of the node: <see langword="true" /> for checked and <see langword="false" /> for
    /// unchecked; the initial value is <see langword="false" />. <see cref="TreeView.CheckedItems" /> owns the
    /// checked state and writes the state of the node into this property.
    /// </summary>
    public bool? IsChecked
    {
        get => _isChecked;
        set => SetAndRaise(IsCheckedProperty, ref _isChecked, value);
    }

    private bool _isSelected;

    /// <summary>
    /// Whether the node is selected. The <see cref="TreeView" /> writes its selection into this property, and a
    /// change of this property is copied into the <see cref="TreeViewItem" /> that presents the node.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }

    private bool _isExpanded;

    /// <summary>
    /// Whether the node is expanded and shows its children. A change of this property is copied into the
    /// <see cref="TreeViewItem" /> that presents the node.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    private bool _isIndicatorEnabled = true;

    /// <summary>
    /// Whether the node shows its check indicator, the check box or the radio button of the node; the initial
    /// value is <see langword="true" />. The <see cref="TreeView" /> treats a node without the indicator as not
    /// checkable.
    /// </summary>
    public bool IsIndicatorEnabled
    {
        get => _isIndicatorEnabled;
        set => SetAndRaise(IsIndicatorEnabledProperty, ref _isIndicatorEnabled, value);
    }

    private string? _groupName;

    /// <summary>
    /// The group of the radio button that serves as check indicator of the node. Nodes that share a group name
    /// accept only one checked node among them; <see langword="null" /> or an empty name puts the node in no
    /// group.
    /// </summary>
    public string? GroupName
    {
        get => _groupName;
        set => SetAndRaise(GroupNameProperty, ref _groupName, value);
    }

    private bool _isLeaf;

    /// <summary>
    /// Whether the node is a leaf, a node without children. A leaf node shows the leaf icon instead of the
    /// switcher that expands a node.
    /// </summary>
    public bool IsLeaf
    {
        get => _isLeaf;
        set => SetAndRaise(IsLeafProperty, ref _isLeaf, value);
    }

    private object? _value;

    /// <summary>
    /// A value that the application stores with the node. It also serves as the segment of this node in a built
    /// node path when <see cref="ItemKey" /> is <see langword="null" />.
    /// </summary>
    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    private IList<ITreeItemNode> _children = new AvaloniaList<ITreeItemNode>();
    private readonly List<ITreeItemNode> _attachedChildren = [];

    /// <summary>
    /// The child nodes. Assigning a list makes this node the parent of its nodes and removes this node as the
    /// parent of the nodes of the previous list; <see langword="null" /> is replaced by an empty list, and
    /// assigning the list that is already in use changes nothing. A list that implements
    /// <see cref="INotifyCollectionChanged" /> keeps the parents of its nodes up to date: a node added to the
    /// list gets this node as parent, and a node removed from the list loses this node as parent.
    /// </summary>
    [Content]
    public IList<ITreeItemNode> Children
    {
        get => _children;
        set => SetChildren(value);
    }

    #endregion

    /// <summary>
    /// The parent of this node, or <see langword="null" /> while the node is not in the children of another node.
    /// It only changes through <see cref="UpdateParentNode" />, which this node calls on the nodes that are added
    /// to or removed from <see cref="Children" />.
    /// </summary>
    public ITreeNode<ITreeItemNode>? ParentNode { get; private set; }

    string? ISelectTagTextProvider.TagText => Header?.ToString();

    IEnumerable<ITreeItemNode> ITreeNode<ITreeItemNode>.Children => Children;

    public BindableTreeItemNode()
    {
        AttachChildren(Children);
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

    private void SetChildren(IList<ITreeItemNode>? children)
    {
        children ??= new AvaloniaList<ITreeItemNode>();
        if (ReferenceEquals(_children, children))
        {
            return;
        }

        DetachChildren(_children);
        SetAndRaise(ChildrenProperty, ref _children, children);
        AttachChildren(_children);
    }

    private void AttachChildren(IList<ITreeItemNode> children)
    {
        if (children is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged += HandleChildrenCollectionChanged;
        }

        for (var i = 0; i < children.Count; i++)
        {
            AttachChild(children[i]);
        }
    }

    private void DetachChildren(IList<ITreeItemNode> children)
    {
        if (children is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged -= HandleChildrenCollectionChanged;
        }

        DetachAttachedChildren();
    }

    private void AttachChild(ITreeItemNode child)
    {
        child.UpdateParentNode(this);
        if (!_attachedChildren.Contains(child))
        {
            _attachedChildren.Add(child);
        }
    }

    private void DetachChild(ITreeItemNode child)
    {
        if (ReferenceEquals(child.ParentNode, this))
        {
            child.UpdateParentNode(null);
        }

        _attachedChildren.Remove(child);
    }

    private void DetachAttachedChildren()
    {
        for (var i = 0; i < _attachedChildren.Count; i++)
        {
            if (ReferenceEquals(_attachedChildren[i].ParentNode, this))
            {
                _attachedChildren[i].UpdateParentNode(null);
            }
        }

        _attachedChildren.Clear();
    }

    private void HandleChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            for (var i = 0; i < e.OldItems.Count; i++)
            {
                if (e.OldItems[i] is ITreeItemNode treeNode)
                {
                    DetachChild(treeNode);
                }
            }
        }

        if (e.NewItems != null)
        {
            for (var i = 0; i < e.NewItems.Count; i++)
            {
                if (e.NewItems[i] is ITreeItemNode treeNode)
                {
                    AttachChild(treeNode);
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            DetachAttachedChildren();
            for (var i = 0; i < Children.Count; i++)
            {
                AttachChild(Children[i]);
            }
        }
    }
}
