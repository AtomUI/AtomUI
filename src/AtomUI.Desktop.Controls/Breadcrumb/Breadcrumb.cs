using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class Breadcrumb : ItemsControl, IControlSharedTokenResourcesHost, IMotionAwareControl
{
    public const string DefaultSeparator = "/";
    
    #region 公共属性定义
    public static readonly StyledProperty<object?> SeparatorProperty =
        AvaloniaProperty.Register<Breadcrumb, object?>(
            nameof(Separator)
        );
    
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty =
        AvaloniaProperty.Register<Breadcrumb, IDataTemplate?>(nameof (SeparatorTemplate));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Breadcrumb>();

    [DependsOn(nameof(SeparatorTemplate))]
    public object? Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }
    
    public IDataTemplate? SeparatorTemplate
    {
        get => GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<BreadcrumbNavigateEventArgs>? NavigateRequest;

    #endregion

    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => BreadcrumbToken.ID;

    #endregion
    
    private Dictionary<BreadcrumbItem, CompositeDisposable> _itemBindingDisposables = new();

    public Breadcrumb()
    {
        LogicalChildren.CollectionChanged += HandleItemsCollectionChanged;
        this.RegisterResources();
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (LogicalChildren.Count > 0)
        {
            for (int i = 0; i < LogicalChildren.Count; i++)
            {
                var item = LogicalChildren[i];
                if (item is BreadcrumbItem breadcrumbItem)
                {
                    breadcrumbItem.IsLast = (i == LogicalChildren.Count - 1);
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is BreadcrumbItem breadcrumbItem)
                    {
                        if (_itemBindingDisposables.TryGetValue(breadcrumbItem, out var disposable))
                        {
                            disposable.Dispose();
                        }
                        _itemBindingDisposables.Remove(breadcrumbItem);
                    }
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new BreadcrumbItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<BreadcrumbItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is BreadcrumbItem breadcrumbItem)
        {
            var disposables = new CompositeDisposable(1);

            if (item != null && item is not Visual)
            {
                if (!breadcrumbItem.IsSet(BreadcrumbItem.ContentProperty))
                {
                    breadcrumbItem.SetCurrentValue(BreadcrumbItem.ContentProperty, item);
                }
                
                if (item is IBreadcrumbItemData breadcrumbItemData)
                {
                    if (!breadcrumbItem.IsSet(BreadcrumbItem.IconProperty))
                    {
                        breadcrumbItem.SetCurrentValue(BreadcrumbItem.IconProperty, breadcrumbItemData.Icon);
                    }
                
                    if (!breadcrumbItem.IsSet(BreadcrumbItem.SeparatorProperty) && breadcrumbItemData.Separator != null)
                    {
                        breadcrumbItem.SetCurrentValue(BreadcrumbItem.SeparatorProperty, breadcrumbItemData.Separator);
                    }
                
                    if (!breadcrumbItem.IsSet(BreadcrumbItem.SeparatorTemplateProperty) && breadcrumbItemData.SeparatorTemplate != null)
                    {
                        breadcrumbItem.SetCurrentValue(BreadcrumbItem.SeparatorTemplateProperty, breadcrumbItemData.SeparatorTemplate);
                    }
                
                    if (!breadcrumbItem.IsSet(BreadcrumbItem.NavigateContextProperty) && breadcrumbItemData.NavigateContext != null)
                    {
                        breadcrumbItem.SetCurrentValue(BreadcrumbItem.NavigateContextProperty, breadcrumbItemData.NavigateContext);
                    }
                           
                    if (!breadcrumbItem.IsSet(BreadcrumbItem.NavigateUriProperty) && breadcrumbItemData.NavigateUri != null)
                    {
                        breadcrumbItem.SetCurrentValue(BreadcrumbItem.NavigateUriProperty, breadcrumbItemData.NavigateUri);
                    }
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, breadcrumbItem, BreadcrumbItem.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, breadcrumbItem, BreadcrumbItem.IsMotionEnabledProperty));
            
            PrepareBreadcrumbItem(breadcrumbItem, item, index, disposables);
            
            if (_itemBindingDisposables.TryGetValue(breadcrumbItem, out var disposable))
            {
                disposable.Dispose();
                _itemBindingDisposables.Remove(breadcrumbItem);
            }
            _itemBindingDisposables.Add(breadcrumbItem, disposables);
            ConfigureItemSeparator(breadcrumbItem);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type BreadcrumbItem.");
        }
    }
    
    protected virtual void PrepareBreadcrumbItem(BreadcrumbItem breadcrumbItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    private void ConfigureItemSeparator(BreadcrumbItem breadcrumbItem)
    {
        if (!breadcrumbItem.IsSet(SeparatorProperty))
        {
            breadcrumbItem.SetCurrentValue(SeparatorProperty, Separator);
        }

        if (!breadcrumbItem.IsSet(SeparatorTemplateProperty))
        {
            breadcrumbItem.SetCurrentValue(SeparatorTemplateProperty, SeparatorTemplate);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (Separator == null)
        {
            SetCurrentValue(SeparatorProperty, DefaultSeparator);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SeparatorProperty ||
            change.Property == SeparatorTemplateProperty)
        {
            if (Items.Count > 0)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    var item = Items[i];
                    if (item is BreadcrumbItem breadcrumbItem)
                    {
                        ConfigureItemSeparator(breadcrumbItem);
                    }
                }
            }
        }
    }

    internal void NotifyNavigateRequest(BreadcrumbItem breadcrumbItem)
    {
        NavigateRequest?.Invoke(this, new BreadcrumbNavigateEventArgs(breadcrumbItem));
    }
}