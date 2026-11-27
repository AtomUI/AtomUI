using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A tab control with card tabs. The tabs have rounded corners on the edge away from the content, an add-tab
/// button can be shown next to them, and each item is hosted in a card-shaped <see cref="TabItem" />.
/// </summary>
public partial class CardTabControl : BaseTabControl
{
    #region 公共属性实现

    /// <summary>
    /// Identifies the <see cref="IsShowAddTabButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowAddTabButtonProperty =
        AvaloniaProperty.Register<CardTabControl, bool>(nameof(IsShowAddTabButton));

    /// <summary>
    /// Identifies the <see cref="AddTabRequest" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> AddTabRequestEvent =
        RoutedEvent.Register<CardTabControl, RoutedEventArgs>(
            nameof(AddTabRequest),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Shows the add-tab button next to the tabs; the default is <see langword="false" />.
    /// Clicking the button raises <see cref="AddTabRequest" />; the control adds no tab itself.
    /// The button is the <c>add</c> semantic part and is styled with <c>CardTabControlAddStyle</c>.
    /// </summary>
    public bool IsShowAddTabButton
    {
        get => GetValue(IsShowAddTabButtonProperty);
        set => SetValue(IsShowAddTabButtonProperty, value);
    }

    /// <summary>
    /// Raised when the add-tab button is clicked. The event bubbles up and carries no data. The control does not
    /// add a tab itself; a handler adds it.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? AddTabRequest
    {
        add => AddHandler(AddTabRequestEvent, value);
        remove => RemoveHandler(AddTabRequestEvent, value);
    }

    #endregion

    #region 内部属性实现

    internal static readonly StyledProperty<CornerRadius> EffectiveCardBorderRadiusProperty =
        AvaloniaProperty.Register<CardTabControl, CornerRadius>(nameof(EffectiveCardBorderRadius));

    internal static readonly StyledProperty<Thickness> CardBorderThicknessProperty =
        AvaloniaProperty.Register<CardTabControl, Thickness>(nameof(CardBorderThickness));

    internal static readonly StyledProperty<double> CardSizeProperty =
        AvaloniaProperty.Register<CardTabControl, double>(nameof(CardSize));

    private static readonly object[] s_cardRadiusKeys =
    [
        ControlTokenResourceKey.Global(CardTabControlTokens.Identity, SharedTokenKind.BorderRadiusSM),
        ControlTokenResourceKey.Global(CardTabControlTokens.Identity, SharedTokenKind.BorderRadiusMD),
        ControlTokenResourceKey.Global(CardTabControlTokens.Identity, SharedTokenKind.BorderRadiusLG)
    ];

    /// <summary>
    /// The border-radius tokens of this control, one for each size type in the order small, middle and large.
    /// The theme of the control sets <c>UseCardRadiusTokenResources</c>, so the corner radius of its card tabs
    /// follows the token of the current <c>SizeType</c> through <c>CardRadius</c>.
    /// </summary>
    private protected override object[] CardRadiusKeys => s_cardRadiusKeys;

    /// <summary>
    /// Recalculates the corner radius of the card tabs and the size of the add-tab button from the new
    /// <c>CardRadius</c> and the tab strip placement, with the update that also runs when the placement changes.
    /// </summary>
    private protected override void OnCardRadiusChanged() => HandleTabStripPlacementChanged();

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
    
    /// <summary>
    /// Creates the container for an item that is not a <see cref="TabItem" /> already: a new card-shaped tab that
    /// carries the semantic item class.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then turns a <see cref="TabItem" /> container into a card tab: sets the card shape,
    /// adds the semantic item class, and binds the corner radius and border thickness of the tab to the effective
    /// card radius and border thickness of the control.
    /// <see cref="ReleaseTabItemOwnerBindings" /> disposes those two bindings.
    /// </summary>
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

    /// <summary>
    /// Applies the template and hooks up the add-tab button: the click handler of the previous button is detached,
    /// the base member is called, and a click on the new button raises <see cref="AddTabRequest" />.
    /// </summary>
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

    /// <summary>
    /// Calls the base member and then recalculates the card corner radius and the add-tab button size when
    /// <c>TabStripPlacement</c> or <see cref="CardSize" /> changes.
    /// </summary>
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

    /// <summary>
    /// Arranges the control and then applies the card corner radius and the add-tab button size of the current tab
    /// strip placement. Call the base member first.
    /// </summary>
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
            EffectiveCardBorderRadius = new CornerRadius(CardRadius.TopLeft,
                CardRadius.TopRight, bottomLeft: 0, bottomRight: 0);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width  = CardSize;
                _addTabButton.Height = _itemsPresenter.DesiredSize.Height;
            }
        }
        else if (TabStripPlacement == Dock.Bottom)
        {
            EffectiveCardBorderRadius = new CornerRadius(0, 0, bottomLeft: CardRadius.BottomLeft,
                bottomRight: CardRadius.BottomRight);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width  = CardSize;
                _addTabButton.Height = _itemsPresenter.DesiredSize.Height;
            }
        }
        else if (TabStripPlacement == Dock.Left)
        {
            EffectiveCardBorderRadius = new CornerRadius(CardRadius.TopLeft, 0,
                bottomLeft: CardRadius.BottomLeft, bottomRight: 0);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width               = CardSize;
                _addTabButton.Height              = CardSize;
                _addTabButton.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }
        else
        {
            EffectiveCardBorderRadius = new CornerRadius(0, CardRadius.TopRight, bottomLeft: 0,
                bottomRight: CardRadius.BottomRight);
            if (_addTabButton is not null && _itemsPresenter is not null)
            {
                _addTabButton.Width               = CardSize;
                _addTabButton.Height              = CardSize;
                _addTabButton.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }
    }
}
