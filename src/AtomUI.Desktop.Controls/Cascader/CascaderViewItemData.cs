using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public interface ICascaderViewItemData : ITreeNode<ICascaderViewItemData>
{
    new object? Header { get; set; }
    bool? IsChecked { get; set; }
    bool IsExpanded { get; set; }
    bool IsIndicatorEnabled { get; set; }
    bool IsLeaf { get; }
    object? Value { get; set; }

    void UpdateParentNode(ICascaderViewItemData? parentNode)
    {
        throw new NotImplementedException();
    }
}

public class CascaderViewItemData : AvaloniaObject, ICascaderViewItemData
{
    public static readonly DirectProperty<CascaderViewItemData, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, object?>(nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    public static readonly DirectProperty<CascaderViewItemData, PathIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, PathIcon?>(nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);
    
    public static readonly DirectProperty<CascaderViewItemData, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, bool>(nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);
    
    public static readonly DirectProperty<CascaderViewItemData, bool?> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, bool?>(nameof(IsChecked),
            o => o.IsChecked,
            (o, v) => o.IsChecked = v);
    
    public static readonly DirectProperty<CascaderViewItemData, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, bool>(nameof(IsExpanded),
            o => o.IsExpanded,
            (o, v) => o.IsExpanded = v);
    
    public static readonly DirectProperty<CascaderViewItemData, bool> IsIndicatorEnabledProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, bool>(nameof(IsIndicatorEnabled),
            o => o.IsIndicatorEnabled,
            (o, v) => o.IsIndicatorEnabled = v);
    
    public ITreeNode<ICascaderViewItemData>? ParentNode { get; private set; }
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
    
    public bool IsLeaf { get; init; } = false;
    
    public object? Value { get; set; }

    private IList<ICascaderViewItemData> _children = [];
    public IList<ICascaderViewItemData> Children
    {
        get => _children;
        init
        {
            _children = value;
            foreach (var child in _children)
            {
                if (child is CascaderViewItemData item)
                {
                    item.ParentNode = this;
                }
            }
        }
    }
    
    public void UpdateParentNode(ICascaderViewItemData? parentNode)
    {
        ParentNode = parentNode;
    }
}