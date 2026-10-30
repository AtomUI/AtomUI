using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public partial class CardTabControl : BaseTabControl
{
    #region 公共属性实现

    public static readonly StyledProperty<bool> IsShowAddTabButtonProperty =
        AvaloniaProperty.Register<CardTabControl, bool>(nameof(IsShowAddTabButton));

    public static readonly RoutedEvent<RoutedEventArgs> AddTabRequestEvent =
        RoutedEvent.Register<CardTabControl, RoutedEventArgs>(
            nameof(AddTabRequest),
            RoutingStrategies.Bubble);

    public bool IsShowAddTabButton
    {
        get => GetValue(IsShowAddTabButtonProperty);
        set => SetValue(IsShowAddTabButtonProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? AddTabRequest
    {
        add => AddHandler(AddTabRequestEvent, value);
        remove => RemoveHandler(AddTabRequestEvent, value);
    }

    #endregion

    #region 内部属性实现

    internal static readonly StyledProperty<CornerRadius> CardBorderRadiusProperty =
        AvaloniaProperty.Register<CardTabControl, CornerRadius>(nameof(CardBorderRadius));
    
    internal static readonly StyledProperty<CornerRadius> EffectiveCardBorderRadiusProperty =
        AvaloniaProperty.Register<CardTabControl, CornerRadius>(nameof(EffectiveCardBorderRadius));

    internal static readonly StyledProperty<Thickness> CardBorderThicknessProperty =
        AvaloniaProperty.Register<CardTabControl, Thickness>(nameof(CardBorderThickness));

    internal static readonly StyledProperty<double> CardSizeProperty =
        AvaloniaProperty.Register<CardTabControl, double>(nameof(CardSize));

    internal CornerRadius CardBorderRadius
    {
        get => GetValue(CardBorderRadiusProperty);
        set => SetValue(CardBorderRadiusProperty, value);
    }
    
    internal CornerRadius EffectiveCardBorderRadius
    {
        get => GetValue(EffectiveCardBorderRadiusProperty);
        set => SetValue(EffectiveCardBorderRadiusProperty, value);
    }

    internal Thickness CardBorderThickness
    {
        get => GetValue(CardBorderThicknessProperty);
        set => SetValue(CardBorderThicknessProperty, value);
    }
    
    internal double CardSize
    {
        get => GetValue(CardSizeProperty);
        set => SetValue(CardSizeProperty, value);
    }

    #endregion

    private IconButton? _addTabButton;
    private ItemsPresenter? _itemsPresenter;
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var tabItem = new TabItem
        {
            Shape = TabSharp.Card
        };
        tabItem.Classes.Add(CardTabControlSemanticParts.ItemClass);
        return tabItem;
    }

    internal override void ReleaseTabItemOwnerBindings(TabItem tabItem)
    {
        base.ReleaseTabItemOwnerBindings(tabItem);
        // Card-only indexer bindings created in PrepareContainerForItemOverride.
        BindingOperations.GetBindingExpressionBase(tabItem, CornerRadiusProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(tabItem, BorderThicknessProperty)?.Dispose();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabItem tabItem)
        {
            tabItem.Shape = TabSharp.Card;
            tabItem.Classes.Add(CardTabControlSemanticParts.ItemClass);
            tabItem[!CornerRadiusProperty] = this[!EffectiveCardBorderRadiusProperty];
            tabItem[!BorderThicknessProperty] = this[!CardBorderThicknessProperty];
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_addTabButton is not null)
        {
            _addTabButton.Click -= HandleAddButtonClicked;
        }

        base.OnApplyTemplate(e);
        _addTabButton   = e.NameScope.Find<IconButton>("PART_AddTabButton");
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        if (_addTabButton is not null)
        {
            _addTabButton.Click += HandleAddButtonClicked;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TabStripPlacementProperty ||
            change.Property == CardSizeProperty)
        {
            HandleTabStripPlacementChanged();
        }
    }

    private void HandleAddButtonClicked(object? sender, RoutedEventArgs args)
    {
        RaiseEvent(new RoutedEventArgs(AddTabRequestEvent));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        HandleTabStripPlacementChanged();
        return size;
    }

    private void HandleTabStripPlacementChanged()
    {
        if (TabStripPlacement == Dock.Top)
        {
            EffectiveCardBorderRadius = new CornerRadius(CardBorderRadius.TopLeft,
                CardBorderRadius.TopRight, bottomLeft: 0, bottomRight: 0);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width  = CardSize;
                _addTabButton.Height = _itemsPresenter.DesiredSize.Height;
            }
        }
        else if (TabStripPlacement == Dock.Bottom)
        {
            EffectiveCardBorderRadius = new CornerRadius(0, 0, bottomLeft: CardBorderRadius.BottomLeft,
                bottomRight: CardBorderRadius.BottomRight);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width  = CardSize;
                _addTabButton.Height = _itemsPresenter.DesiredSize.Height;
            }
        }
        else if (TabStripPlacement == Dock.Left)
        {
            EffectiveCardBorderRadius = new CornerRadius(CardBorderRadius.TopLeft, 0,
                bottomLeft: CardBorderRadius.BottomLeft, bottomRight: 0);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width               = CardSize;
                _addTabButton.Height              = CardSize;
                _addTabButton.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }
        else
        {
            EffectiveCardBorderRadius = new CornerRadius(0, CardBorderRadius.TopRight, bottomLeft: 0,
                bottomRight: CardBorderRadius.BottomRight);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width               = CardSize;
                _addTabButton.Height              = CardSize;
                _addTabButton.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }
    }
}
