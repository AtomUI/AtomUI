using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A group in the column header of a <see cref="DataGrid" /> that nests columns and other groups in
/// <see cref="GroupChildren" /> and has a header of its own. It also acts as a resource host that resolves
/// resources through its owning grid.
/// </summary>
public class DataGridColumnGroupItem : AvaloniaObject,
                                       IDataGridColumnGroupItemInternal, 
                                       IDataGridColumnGroupChanged,
                                       IResourceHost,
                                       IThemeVariantHost
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridColumnGroupItem, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnGroupItem, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridColumnGroupItem, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnGroupItem, IDataTemplate?>(
            nameof(HeaderTemplate),
            o => o.HeaderTemplate,
            (o, v) => o.HeaderTemplate = v);
    
    /// <summary>
    /// Identifies the <see cref="HorizontalAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty =
        Layoutable.HorizontalAlignmentProperty.AddOwner<DataGridColumnGroupItem>();
    
    /// <summary>
    /// Identifies the <see cref="VerticalAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalAlignmentProperty =
        Layoutable.VerticalAlignmentProperty.AddOwner<DataGridColumnGroupItem>();

    /// <summary>
    /// Backing field of the <see cref="Header" /> property. Read and set the value through that property:
    /// assigning the field directly raises no change notification.
    /// </summary>
    public object? _header;
    /// <summary>
    /// The content shown in the header of this group. The header cell of the group binds to this property, so
    /// the header follows later changes.
    /// </summary>
    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    /// <summary>
    /// Backing field of the <see cref="HeaderTemplate" /> property. Read and set the value through that
    /// property: assigning the field directly raises no change notification.
    /// </summary>
    public IDataTemplate? _headerTemplate;
    /// <summary>
    /// Template that presents the <see cref="Header" /> of this group, or <see langword="null" /> to let the
    /// header use its default presentation. The header cell of the group binds to this property.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }
    
    /// <summary>
    /// Horizontal alignment of the content in the header cell of this group; the header cell binds to this
    /// property. The default is <c>Left</c>.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => GetValue(HorizontalAlignmentProperty);
        set => SetValue(HorizontalAlignmentProperty, value);
    }
    
    /// <summary>
    /// Vertical alignment of the content in the header cell of this group; the header cell binds to this
    /// property. The default is <c>Center</c>.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => GetValue(VerticalAlignmentProperty);
        set => SetValue(VerticalAlignmentProperty, value);
    }
    
    /// <summary>
    /// The group that contains this item, or <see langword="null" /> when no group does. The parent sets it when
    /// the item is added to the parent's <see cref="GroupChildren" /> and clears it when the item is removed.
    /// </summary>
    public IDataGridColumnGroupItem? GroupParent { get; set; }
    
    /// <summary>
    /// The columns and groups nested in this group. Adding or removing an item sets or clears the
    /// <see cref="GroupParent" /> of the item and raises <see cref="GroupChanged" /> after the collection changed.
    /// </summary>
    /// <remarks>
    /// This is the XAML content property of the group, so items written as direct content of the group are
    /// nested here.
    /// </remarks>
    [Content]
    public ObservableCollection<IDataGridColumnGroupItem> GroupChildren { get; }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised after an item was added to or removed from <see cref="GroupChildren" />, once the collection has
    /// changed. When an item is removed, its <see cref="GroupParent" /> is still set when the event is raised.
    /// The events of nested groups are raised again on this group with the same arguments, so they reach the
    /// root of the group tree.
    /// </summary>
    public event EventHandler<DataGridColumnGroupChangedArgs>? GroupChanged;
    /// <summary>
    /// Occurs when the pointer is pressed over the header of this group.
    /// </summary>
    public event EventHandler<PointerPressedEventArgs>? HeaderPointerPressed;
    /// <summary>
    /// Occurs when the pointer is released over the header of this group.
    /// </summary>
    public event EventHandler<PointerReleasedEventArgs>? HeaderPointerReleased;

    #endregion
    
    #region 内部属性定义
    
    DataGridHeaderViewItem? IDataGridColumnGroupItemInternal.GroupHeaderViewItem { get; set; }
    
    internal bool HasHeaderCell => _headerCell != null;

    internal DataGridColumnGroupHeader HeaderCell
    {
        get
        {
            _headerCell ??= CreateHeader();
            return _headerCell;
        }
    }
    
    /// <summary>
    /// The grid that hosts this group. Assigning a different grid moves the item's subscriptions to the resource
    /// and theme variant events of the new grid and moves the grid bindings of its header cell; assigning the
    /// grid it already has does nothing.
    /// </summary>
    protected internal DataGrid? OwningGrid
    {
        get => _owningGrid;
        internal set
        {
            if (ReferenceEquals(_owningGrid, value))
            {
                return;
            }

            UnregisterOwningGridResourceHost();
            _headerCell?.ReleaseOwningGridBinding();
            _owningGrid = value;
            RegisterOwningGridResourceHost(_owningGrid);
            if (_owningGrid is not null)
            {
                _headerCell?.BindOwningGridProperties(_owningGrid);
            }
        }
    }

    internal bool IsFrozen { get; set; } = false;
    #endregion
    
    private DataGridColumnGroupHeader? _headerCell;
    private DataGrid? _owningGrid;
    private DataGrid? _subscribedResourceHostGrid;
    // The children that are currently attached. A Reset carries no old items, so clearing their parent link
    // and unsubscribing nested groups have to rely on this list.
    private readonly List<IDataGridColumnGroupItem> _attachedChildren = [];

    static DataGridColumnGroupItem()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<DataGridColumnGroupItem>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<DataGridColumnGroupItem>(VerticalAlignment.Center);
    }

    public DataGridColumnGroupItem()
    {
        GroupChildren                   =  new ObservableCollection<IDataGridColumnGroupItem>();
        GroupChildren.CollectionChanged += HandleCollectionChanged;
    }

    #region 资源宿主定义

    /// <summary>
    /// Raised when the owning grid raised its own <c>ResourcesChanged</c> event, and once whenever
    /// <see cref="OwningGrid" /> is assigned a different grid.
    /// </summary>
    public event EventHandler<ResourcesChangedEventArgs>? ResourcesChanged;
    /// <summary>
    /// Raised when the owning grid raised its own <c>ActualThemeVariantChanged</c> event, and once whenever
    /// <see cref="OwningGrid" /> is assigned a different grid that is not <see langword="null" />.
    /// </summary>
    public event EventHandler? ActualThemeVariantChanged;

    /// <summary>
    /// Always <see langword="true" />. The item holds no resources of its own and resolves them through its
    /// owning grid and the application.
    /// </summary>
    public bool HasResources => true;

    /// <summary>
    /// The theme variant of the owning grid, or of the application when the group has no grid, or
    /// <see cref="ThemeVariant.Default" /> when there is no application either.
    /// </summary>
    public ThemeVariant ActualThemeVariant =>
        _owningGrid?.ActualThemeVariant ??
        Application.Current?.ActualThemeVariant ??
        ThemeVariant.Default;

    /// <summary>
    /// Looks a resource key up on the owning grid first and then on the application.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when either of them resolved the key; <paramref name="value" /> then holds the
    /// resource. <see langword="false" /> when neither did; <paramref name="value" /> is then <see langword="null" />.
    /// </returns>
    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (_owningGrid?.TryFindResource(key, theme, out value) == true)
        {
            return true;
        }

        if (Application.Current?.TryGetResource(key, theme, out value) == true)
        {
            return true;
        }

        value = null;
        return false;
    }

    void IResourceHost.NotifyHostedResourcesChanged(ResourcesChangedEventArgs e)
    {
        ResourcesChanged?.Invoke(this, e);
    }

    private void RegisterOwningGridResourceHost(DataGrid? owningGrid)
    {
        if (owningGrid is null)
        {
            RaiseResourcesChanged();
            return;
        }

        _subscribedResourceHostGrid = owningGrid;
        _subscribedResourceHostGrid.ResourcesChanged += HandleOwningGridResourcesChanged;
        _subscribedResourceHostGrid.ActualThemeVariantChanged += HandleOwningGridActualThemeVariantChanged;
        RaiseResourcesChanged();
        ActualThemeVariantChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UnregisterOwningGridResourceHost()
    {
        if (_subscribedResourceHostGrid is null)
        {
            return;
        }

        _subscribedResourceHostGrid.ResourcesChanged -= HandleOwningGridResourcesChanged;
        _subscribedResourceHostGrid.ActualThemeVariantChanged -= HandleOwningGridActualThemeVariantChanged;
        _subscribedResourceHostGrid = null;
    }

    private void HandleOwningGridResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        ResourcesChanged?.Invoke(this, e);
    }

    private void HandleOwningGridActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ActualThemeVariantChanged?.Invoke(this, e);
    }

    private void RaiseResourcesChanged()
    {
        ResourcesChanged?.Invoke(this, ResourcesChangedEventArgs.Create());
    }

    #endregion
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var child in _attachedChildren.ToArray())
            {
                DetachChild(child);
            }
            foreach (var child in GroupChildren)
            {
                AttachChild(child);
            }
            return;
        }

        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is IDataGridColumnGroupItem groupItem)
                {
                    DetachChild(groupItem);
                }
            }
        }

        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is IDataGridColumnGroupItem groupItem)
                {
                    AttachChild(groupItem);
                }
            }
        }
    }

    private void AttachChild(IDataGridColumnGroupItem child)
    {
        child.GroupParent = this;
        _attachedChildren.Add(child);
        if (child is DataGridColumnGroupItem subGroup)
        {
            subGroup.GroupChanged += HandleSubGroupChanged;
        }
        GroupChanged?.Invoke(this, new DataGridColumnGroupChangedArgs(child, NotifyColumnGroupChangedType.Add));
    }

    private void DetachChild(IDataGridColumnGroupItem child)
    {
        if (!_attachedChildren.Remove(child))
        {
            return;
        }
        if (child is DataGridColumnGroupItem subGroup)
        {
            subGroup.GroupChanged -= HandleSubGroupChanged;
        }
        GroupChanged?.Invoke(this, new DataGridColumnGroupChangedArgs(child, NotifyColumnGroupChangedType.Remove));
        child.GroupParent = null;
    }

    // Additions and removals inside a nested group are changes of this group tree as well. Forward them up to
    // the root, because the grid only subscribes to the root item.
    private void HandleSubGroupChanged(object? sender, DataGridColumnGroupChangedArgs args)
    {
        GroupChanged?.Invoke(this, args);
    }
    
    internal virtual DataGridColumnGroupHeader CreateHeader()
    {
        var result = new DataGridColumnGroupHeader();
        result.OwningGroupItem = this;
        Debug.Assert(OwningGrid != null);
        result[!DataGridColumnGroupHeader.HeaderProperty]                     = this[!HeaderProperty];
        result[!DataGridColumnGroupHeader.HeaderTemplateProperty]             = this[!HeaderTemplateProperty];
        result[!DataGridColumnGroupHeader.HorizontalContentAlignmentProperty] = this[!HorizontalAlignmentProperty];
        result[!DataGridColumnGroupHeader.VerticalContentAlignmentProperty]   = this[!VerticalAlignmentProperty];
        result.BindOwningGridProperties(OwningGrid);
        return result;
    }

    internal void NotifyHeaderPointerPressed(PointerPressedEventArgs e)
    {
        HeaderPointerPressed?.Invoke(this, e);
    }

    internal void NotifyHeaderPointerReleased(PointerReleasedEventArgs e)
    {
        HeaderPointerReleased?.Invoke(this, e);
    }
}
