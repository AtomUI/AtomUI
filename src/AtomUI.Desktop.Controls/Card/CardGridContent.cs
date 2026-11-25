using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// A Token-owner transparent container: the grid content has no Own Tokens and no Token reads of its own, so its grid items
// (themselves transparent) keep the Card as owner and Controls[Card] reaches their reads and transitions.
[TokenOwnerTransparent]
public class CardGridContent : ItemsControl
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

    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<SizeType> SizeTypeProperty = 
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<CardGridContent>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CardGridContent>();

    internal static readonly DirectProperty<CardGridContent, Geometry?> SeparatorGeometryProperty =
        AvaloniaProperty.RegisterDirect<CardGridContent, Geometry?>(nameof(SeparatorGeometry), content => content.SeparatorGeometry);

    private Geometry? _separatorGeometry;

    internal Geometry? SeparatorGeometry
    {
        get => _separatorGeometry;
        set => SetAndRaise(SeparatorGeometryProperty, ref _separatorGeometry, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
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
                gridItem[!CardGridItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }
            gridItem[!SizeTypeProperty]        = this[!SizeTypeProperty];
            gridItem[!IsEnabledProperty]       = this[!IsEnabledProperty];
            gridItem[!IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
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
            gridItem.SetGridHoverClip(null);
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
        SeparatorGeometry = null;
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        if (_itemsPresenter != null)
        {
            _itemsPresenter.ApplyTemplate();
            SyncGridPanelProperties();
        }
    }

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

    private void DisposeCardGridItemState(CardGridItem gridItem)
    {
        if (_cardGridItemDisposables.Remove(gridItem, out var disposables))
        {
            disposables.Dispose();
        }
    }
}
