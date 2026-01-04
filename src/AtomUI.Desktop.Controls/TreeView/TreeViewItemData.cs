using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class TreeViewItemData : AvaloniaObject, ITreeViewItemData
{
    public static readonly DirectProperty<TreeViewItemData, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemData, object?>(nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    public static readonly DirectProperty<TreeViewItemData, PathIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemData, PathIcon?>(nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);
    
    public static readonly DirectProperty<TreeViewItemData, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemData, bool>(nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);
    
    public static readonly DirectProperty<TreeViewItemData, bool?> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemData, bool?>(nameof(IsChecked),
            o => o.IsChecked,
            (o, v) => o.IsChecked = v);
    
    public static readonly DirectProperty<TreeViewItemData, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemData, bool>(nameof(IsSelected),
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);
    
    public static readonly DirectProperty<TreeViewItemData, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemData, bool>(nameof(IsExpanded),
            o => o.IsExpanded,
            (o, v) => o.IsExpanded = v);
    
    public static readonly DirectProperty<TreeViewItemData, bool> IsIndicatorEnabledProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItemData, bool>(nameof(IsIndicatorEnabled),
            o => o.IsIndicatorEnabled,
            (o, v) => o.IsIndicatorEnabled = v);
    
    public ITreeNode<ITreeViewItemData>? ParentNode { get; private set; }
    public TreeNodeKey? ItemKey { get; init; }
    
    private object? _header;

    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
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
    
    private bool? _isChecked = false;

    public bool? IsChecked
    {
        get => _isChecked;
        set => SetAndRaise(IsCheckedProperty, ref _isChecked, value);
    }
    
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }
    
    private bool _isExpanded;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }
    
    private bool _isIndicatorEnabled = true;

    public bool IsIndicatorEnabled
    {
        get => _isIndicatorEnabled;
        set => SetAndRaise(IsIndicatorEnabledProperty, ref _isIndicatorEnabled, value);
    }
    
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