using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public interface ICascaderViewItemData : ITreeNode<ICascaderViewItemData>
{
    new object? Header { get; set; }
    bool? IsChecked { get; set; }
    bool IsCheckBoxEnabled { get; set; }
    bool IsExpanded { get; set; }
    bool IsLeaf { get; }
    object? Value { get; set; }

    void UpdateParentNode(ICascaderViewItemData? parentNode)
    {
        throw new NotImplementedException();
    }
}

public class CascaderViewItemData : AvaloniaObject, ICascaderViewItemData, ISelectTagTextProvider
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
    
    public static readonly DirectProperty<CascaderViewItemData, bool> IsCheckBoxEnabledProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, bool>(nameof(IsCheckBoxEnabled),
            o => o.IsCheckBoxEnabled,
            (o, v) => o.IsCheckBoxEnabled = v);
    
    public static readonly DirectProperty<CascaderViewItemData, bool?> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, bool?>(nameof(IsChecked),
            o => o.IsChecked,
            (o, v) => o.IsChecked = v);
    
    public static readonly DirectProperty<CascaderViewItemData, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, bool>(nameof(IsExpanded),
            o => o.IsExpanded,
            (o, v) => o.IsExpanded = v);
    
    public static readonly DirectProperty<CascaderViewItemData, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItemData, object?>(nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);
    
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
    
    private bool _isCheckBoxEnabled = true;

    public bool IsCheckBoxEnabled
    {
        get => _isCheckBoxEnabled;
        set => SetAndRaise(IsCheckBoxEnabledProperty, ref _isCheckBoxEnabled, value);
    }
    
    public bool IsLeaf { get; init; } = false;
    
    private object? _value = true;

    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }
    
    string? ISelectTagTextProvider.TagText => Header?.ToString();

    private AvaloniaList<ICascaderViewItemData> _children = [];
    public IList<ICascaderViewItemData> Children
    {
        get => _children;
        init => _children.AddRange(value);
    }
    
    public void UpdateParentNode(ICascaderViewItemData? parentNode)
    {
        ParentNode = parentNode;
    }

    public CascaderViewItemData()
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
                    if (child is ICascaderViewItemData cascaderViewItem)
                    {
                        cascaderViewItem.UpdateParentNode(this);
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
                    if (child is ICascaderViewItemData cascaderViewItem)
                    {
                        cascaderViewItem.UpdateParentNode(null);
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