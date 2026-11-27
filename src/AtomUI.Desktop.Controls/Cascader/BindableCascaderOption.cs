using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An <see cref="ICascaderOption" /> that keeps its state in styled properties, so that application code and
/// data templates can bind to an option and are notified when the view changes it.
/// </summary>
[GenerateScopedResourceHost]
public partial class BindableCascaderOption : AvaloniaObject, ICascaderOption, ISelectTagTextProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ItemKey" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, EntityKey?> ItemKeyProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, EntityKey?>(
            nameof(ItemKey),
            o => o.ItemKey,
            (o, v) => o.ItemKey = v);

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, PathIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, PathIcon?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);

    /// <summary>
    /// Identifies the <see cref="IsEnabled" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, bool>(
            nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);

    /// <summary>
    /// Identifies the <see cref="IsChecked" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, bool?> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, bool?>(
            nameof(IsChecked),
            o => o.IsChecked,
            (o, v) => o.IsChecked = v);

    /// <summary>
    /// Identifies the <see cref="IsExpanded" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, bool>(
            nameof(IsExpanded),
            o => o.IsExpanded,
            (o, v) => o.IsExpanded = v);

    /// <summary>
    /// Identifies the <see cref="IsCheckBoxEnabled" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, bool> IsCheckBoxEnabledProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, bool>(
            nameof(IsCheckBoxEnabled),
            o => o.IsCheckBoxEnabled,
            (o, v) => o.IsCheckBoxEnabled = v);

    /// <summary>
    /// Identifies the <see cref="IsLeaf" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, bool>(
            nameof(IsLeaf),
            o => o.IsLeaf,
            (o, v) => o.IsLeaf = v);

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, object?>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);

    /// <summary>
    /// Identifies the <see cref="Children" /> property.
    /// </summary>
    public static readonly DirectProperty<BindableCascaderOption, IList<ICascaderOption>> ChildrenProperty =
        AvaloniaProperty.RegisterDirect<BindableCascaderOption, IList<ICascaderOption>>(
            nameof(Children),
            o => o.Children,
            (o, v) => o.Children = v);

    private EntityKey? _itemKey;

    /// <summary>
    /// Key of this option. The view uses the text of the key as the segment of the selected path and to match a
    /// segment back to the option; when the key is <see langword="null" />, <see cref="Value" /> stands in for it.
    /// </summary>
    public EntityKey? ItemKey
    {
        get => _itemKey;
        set => SetAndRaise(ItemKeyProperty, ref _itemKey, value);
    }

    private object? _header;

    /// <summary>
    /// Label of this option. The item in the dropdown shows it as content. The tags of a multiple selection and
    /// the segments of the selected path show <c>ToString()</c> of it, or an empty string when it is
    /// <see langword="null" />.
    /// </summary>
    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }

    private PathIcon? _icon;

    /// <summary>
    /// Icon shown in front of the label in the dropdown item. The icon area of the item stays hidden while the
    /// value is <see langword="null" />.
    /// </summary>
    public PathIcon? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }

    private bool _isEnabled = true;

    /// <summary>
    /// Whether the option accepts selection. When <see langword="false" />, the check box of its item is disabled,
    /// the option is left out when the check state of its parent is derived, and a path that passes through the
    /// option is disabled in the filtered list.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }

    private bool? _isChecked = false;

    /// <summary>
    /// Check state of the option: <see langword="true" /> is checked, <see langword="false" /> is unchecked and
    /// <see langword="null" /> is indeterminate. The view writes this property when the user changes the selection,
    /// and it assigns <see langword="null" /> to a parent whose children are only partly checked.
    /// </summary>
    public bool? IsChecked
    {
        get => _isChecked;
        set => SetAndRaise(IsCheckedProperty, ref _isChecked, value);
    }

    private bool _isExpanded;

    /// <summary>
    /// Whether the children of the option are shown in the column next to the one that holds the option. The view
    /// writes this property as the user opens and closes levels, so the value follows the levels that are open.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    private bool _isCheckBoxEnabled = true;

    /// <summary>
    /// Whether the check box of the item accepts user input. When <see langword="false" />, the check box is
    /// disabled and the option is left out when the check state of its parent is derived.
    /// </summary>
    public bool IsCheckBoxEnabled
    {
        get => _isCheckBoxEnabled;
        set => SetAndRaise(IsCheckBoxEnabledProperty, ref _isCheckBoxEnabled, value);
    }

    private bool _isLeaf;

    /// <summary>
    /// Whether the option is the end of its branch. A leaf is selected by a click and is never expanded. Without
    /// a data loader the view treats an option as a leaf exactly when it has no children. When children are
    /// loaded on demand, an option without children is a leaf only if this value is <see langword="true" /> or
    /// its children were already loaded; any other option without children makes the view request its children
    /// once it is expanded.
    /// </summary>
    public bool IsLeaf
    {
        get => _isLeaf;
        set => SetAndRaise(IsLeafProperty, ref _isLeaf, value);
    }

    private object? _value;

    /// <summary>
    /// Value of this option. While <see cref="ItemKey" /> is <see langword="null" />, the view uses the text of the
    /// value as the segment of the selected path and to match a segment back to the option.
    /// </summary>
    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    private IList<ICascaderOption> _children = new AvaloniaList<ICascaderOption>();
    private readonly List<ICascaderOption> _attachedChildren = [];

    /// <summary>
    /// The options one level below this option. It is the XAML content property, so child options can be written
    /// as nested elements. Setting <see langword="null" /> stores an empty list. While a list is assigned here,
    /// items added to or removed from it have <see cref="ParentNode" /> updated as long as the list implements
    /// <see cref="INotifyCollectionChanged" />, and when children are loaded on demand the view appends the loaded
    /// options to this list.
    /// </summary>
    [Content]
    public IList<ICascaderOption> Children
    {
        get => _children;
        set => SetChildren(value);
    }

    #endregion

    /// <summary>
    /// The option that lists this option in its <see cref="Children" />, or <see langword="null" /> while the option
    /// is not attached to a parent. Only <see cref="UpdateParentNode" /> changes it.
    /// </summary>
    public ITreeNode<ICascaderOption>? ParentNode { get; private set; }

    string? ISelectTagTextProvider.TagText => Header?.ToString();

    IEnumerable<ICascaderOption> ITreeNode<ICascaderOption>.Children => Children;

    public BindableCascaderOption()
    {
        AttachChildren(Children);
    }

    /// <summary>
    /// Sets <see cref="ParentNode" /> to the given parent, or clears it when <see langword="null" /> is passed.
    /// The owner of a children list calls it for each item that is added to or removed from that list; application
    /// code does not need to call it.
    /// </summary>
    public void UpdateParentNode(ICascaderOption? parentNode)
    {
        ParentNode = parentNode;
    }

    private void SetChildren(IList<ICascaderOption>? children)
    {
        children ??= new AvaloniaList<ICascaderOption>();
        if (ReferenceEquals(_children, children))
        {
            return;
        }

        DetachChildren(_children);
        SetAndRaise(ChildrenProperty, ref _children, children);
        AttachChildren(_children);
    }

    private void AttachChildren(IList<ICascaderOption> children)
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

    private void DetachChildren(IList<ICascaderOption> children)
    {
        if (children is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged -= HandleChildrenCollectionChanged;
        }

        DetachAttachedChildren();
    }

    private void AttachChild(ICascaderOption child)
    {
        child.UpdateParentNode(this);
        if (!_attachedChildren.Contains(child))
        {
            _attachedChildren.Add(child);
        }
    }

    private void DetachChild(ICascaderOption child)
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
                if (e.OldItems[i] is ICascaderOption option)
                {
                    DetachChild(option);
                }
            }
        }

        if (e.NewItems != null)
        {
            for (var i = 0; i < e.NewItems.Count; i++)
            {
                if (e.NewItems[i] is ICascaderOption option)
                {
                    AttachChild(option);
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
