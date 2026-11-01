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

public class DataGridColumnGroupItem : AvaloniaObject,
                                       IDataGridColumnGroupItemInternal, 
                                       IDataGridColumnGroupChanged,
                                       IResourceHost,
                                       IThemeVariantHost
{
    #region 公共属性定义

    public static readonly DirectProperty<DataGridColumnGroupItem, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnGroupItem, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    public static readonly DirectProperty<DataGridColumnGroupItem, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnGroupItem, IDataTemplate?>(
            nameof(HeaderTemplate),
            o => o.HeaderTemplate,
            (o, v) => o.HeaderTemplate = v);
    
    public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty =
        Layoutable.HorizontalAlignmentProperty.AddOwner<DataGridColumnGroupItem>();
    
    public static readonly StyledProperty<VerticalAlignment> VerticalAlignmentProperty =
        Layoutable.VerticalAlignmentProperty.AddOwner<DataGridColumnGroupItem>();

    public object? _header;
    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    public IDataTemplate? _headerTemplate;
    public IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }
    
    public HorizontalAlignment HorizontalAlignment
    {
        get => GetValue(HorizontalAlignmentProperty);
        set => SetValue(HorizontalAlignmentProperty, value);
    }
    
    public VerticalAlignment VerticalAlignment
    {
        get => GetValue(VerticalAlignmentProperty);
        set => SetValue(VerticalAlignmentProperty, value);
    }
    
    public IDataGridColumnGroupItem? GroupParent { get; set; }
    
    [Content]
    public ObservableCollection<IDataGridColumnGroupItem> GroupChildren { get; }
    #endregion

    #region 公共事件定义

    public event EventHandler<DataGridColumnGroupChangedArgs>? GroupChanged;
    public event EventHandler<PointerPressedEventArgs>? HeaderPointerPressed;
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

    public event EventHandler<ResourcesChangedEventArgs>? ResourcesChanged;
    public event EventHandler? ActualThemeVariantChanged;

    public bool HasResources => true;

    public ThemeVariant ActualThemeVariant =>
        _owningGrid?.ActualThemeVariant ??
        Application.Current?.ActualThemeVariant ??
        ThemeVariant.Default;

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
        if (e.OldItems != null)
        {
            var oldItems = e.OldItems;
            for (var i = 0; i < oldItems.Count; i++)
            {
                var item = oldItems[i];
                if (item is IDataGridColumnGroupItem groupItem)
                {
                    HandleColumnGroupChanged(groupItem, NotifyColumnGroupChangedType.Remove);
                    groupItem.GroupParent = null;
                }
            }
        }

        if (e.NewItems != null)
        {
            var newItems = e.NewItems;
            for (var i = 0; i < newItems.Count; i++)
            {
                var item = newItems[i];
                if (item is IDataGridColumnGroupItem groupItem)
                {
                    groupItem.GroupParent = this;
                    HandleColumnGroupChanged(groupItem, NotifyColumnGroupChangedType.Add);
                }
            }
        }
    }

    private void HandleColumnGroupChanged(IDataGridColumnGroupItem groupItem, NotifyColumnGroupChangedType changedType)
    {
        if (groupItem is DataGridColumn)
        {
            Debug.Assert(groupItem.GroupParent != null);
            var current = groupItem;
            while (current.GroupParent != null)
            {
                current = current.GroupParent;
            }
            GroupChanged?.Invoke(this, new DataGridColumnGroupChangedArgs(groupItem, changedType));
        }
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
