using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public interface INavMenuNode : ITreeNode<INavMenuNode>
{
    IDataTemplate? HeaderTemplate { get; }
    void UpdateParentNode(INavMenuNode? parentNode) => throw new NotImplementedException();
}

public class NavMenuNode : AvaloniaObject, INavMenuNode
{
    public static readonly DirectProperty<NavMenuNode, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    public static readonly DirectProperty<NavMenuNode, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, IDataTemplate?>(
            nameof(HeaderTemplate),
            o => o.HeaderTemplate,
            (o, v) => o.HeaderTemplate = v);
    
    public static readonly DirectProperty<NavMenuNode, TreeNodeKey?> ItemKeyProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, TreeNodeKey?>(
            nameof(ItemKey),
            o => o.ItemKey,
            (o, v) => o.ItemKey = v);
    
    public static readonly DirectProperty<NavMenuNode, PathIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, PathIcon?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);
    
    public static readonly DirectProperty<NavMenuNode, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, bool>(
            nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);
        
    private object? _header;

    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    private IDataTemplate? _headerTemplate;

    public IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }
    
    private TreeNodeKey? _itemKey;

    public TreeNodeKey? ItemKey
    {
        get => _itemKey;
        set => SetAndRaise(ItemKeyProperty, ref _itemKey, value);
    }
    
    private PathIcon? _icon;

    public PathIcon? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }
    
    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }
    
    public ITreeNode<INavMenuNode>? ParentNode { get; private set; }
    
    private readonly AvaloniaList<INavMenuNode> _children = [];
    
    [Content]
    public IList<INavMenuNode> Children
    {
        get => _children;
        init => _children.AddRange(value);
    }
    
    public NavMenuNode()
    {
        _children.CollectionChanged += HandleCollectionChanged;
    }
    
    public void UpdateParentNode(INavMenuNode? parentNode)
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
                    if (child is INavMenuNode menuItemNode)
                    {
                        menuItemNode.UpdateParentNode(this);
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
                    if (child is INavMenuNode menuItemNode)
                    {
                        menuItemNode.UpdateParentNode(null);
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