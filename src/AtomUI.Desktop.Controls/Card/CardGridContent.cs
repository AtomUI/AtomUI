using AtomUI.Theme.Resources;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia.Data;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public class CardGridContent : ItemsControl, IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<ColumnDefinitions> ColumnDefinitionsProperty =
        AvaloniaProperty.Register<CardGridContent, ColumnDefinitions>(nameof(ColumnDefinitions));
    
    public static readonly StyledProperty<RowDefinitions> RowDefinitionsProperty =
        AvaloniaProperty.Register<CardGridContent, RowDefinitions>(nameof(RowDefinitions));
    
    public static readonly StyledProperty<bool> IsHoverableProperty = 
        AvaloniaProperty.Register<CardGridContent, bool>(nameof (IsHoverable), true);

    public ColumnDefinitions ColumnDefinitions
    {
        get => GetValue(ColumnDefinitionsProperty);
        set => SetValue(ColumnDefinitionsProperty, value);
    }

    public RowDefinitions RowDefinitions
    {
        get => GetValue(RowDefinitionsProperty);
        set => SetValue(RowDefinitionsProperty, value);
    }
    
    public bool IsHoverable
    {
        get => GetValue(IsHoverableProperty);
        set => SetValue(IsHoverableProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<SizeType> SizeTypeProperty = 
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<CardGridContent>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CardGridContent>();
    
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    private ItemsPresenter? _itemsPresenter;
    private readonly Dictionary<CardGridItem, CompositeDisposable> _cardGridItemDisposables = new();

    public CardGridContent()
    {
        LogicalChildren.CollectionChanged += HandleLogicalChildrenChanged;
        SetCurrentValue(ColumnDefinitionsProperty, new ColumnDefinitions());
        SetCurrentValue(RowDefinitionsProperty, new RowDefinitions());
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CardGridItem();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CardGridItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is CardGridItem gridItem)
        {
            DisposeCardGridItemState(gridItem);
            var disposables = new CompositeDisposable(8);
            _cardGridItemDisposables[gridItem] = disposables;
            if (item != null && item is not Visual)
            {
                if (!gridItem.IsSet(CardGridItem.ContentProperty))
                {
                    gridItem.SetCurrentValue(CardGridItem.ContentProperty, item);
                }
            }

            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, gridItem, CardGridItem.ContentTemplateProperty, priority: BindingPriority.Template));
            }
            // Runtime supplied/generated containers can outlive their owner membership.
            // Their state defaults must work before templates; native item values win.
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, gridItem, CardGridItem.SizeTypeProperty, priority: BindingPriority.Template));
            disposables.Add(BindUtils.RelayBind(this, IsEnabledProperty, gridItem, CardGridItem.IsEnabledProperty, priority: BindingPriority.Template));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, gridItem, CardGridItem.IsMotionEnabledProperty, priority: BindingPriority.Template));
            disposables.Add(BindUtils.RelayBind(this, IsHoverableProperty, gridItem, CardGridItem.IsHoverableProperty, priority: BindingPriority.Template));
            PrepareCardGridItem(gridItem, item, index, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CardGridItem.");
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is CardGridItem gridItem)
        {
            DisposeCardGridItemState(gridItem);
        }

        base.ClearContainerForItemOverride(container);
    }
    
    protected virtual void PrepareCardGridItem(CardGridItem cardGridItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColumnDefinitionsProperty ||
            change.Property == RowDefinitionsProperty)
        {
            SyncGridPanelProperties();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        if (_itemsPresenter != null)
        {
            _itemsPresenter.ApplyTemplate();
            SyncGridPanelProperties();
        }
    }

    #region 实现 IThemeInheritanceSource 接口

    // ControlTheme cannot read the protected inheritance parent; it owns the
    // optional typography while this bridge exposes the actual native source.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    // Grid definitions are CLR collections, not Avalonia properties. AXAML cannot
    // install reactive bindings on them (AVLN3000); keep the typed projection.
    private void SyncGridPanelProperties()
    {
        if (_itemsPresenter?.Panel != null)
        {
            var panel = _itemsPresenter?.Panel;
            if (panel is Grid gridPanel)
            {
                gridPanel.ColumnDefinitions = ColumnDefinitions;
                gridPanel.RowDefinitions = RowDefinitions;
            }
        }
    }

    private void HandleLogicalChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var child in e.OldItems)
            {
                if (child is CardGridItem item)
                {
                    DisposeCardGridItemState(item);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var item in _cardGridItemDisposables.Keys.ToArray())
            {
                if (!LogicalChildren.Contains(item))
                {
                    DisposeCardGridItemState(item);
                }
            }
        }
    }

    private void DisposeCardGridItemState(CardGridItem gridItem)
    {
        if (_cardGridItemDisposables.Remove(gridItem, out var disposables))
        {
            disposables.Dispose();
        }
    }
}
