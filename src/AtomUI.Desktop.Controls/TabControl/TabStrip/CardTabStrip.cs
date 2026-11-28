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
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A tab strip whose tabs are rendered as cards. Every tab takes its corner radius and border thickness
/// from the strip, and only the corners on the <see cref="BaseTabStrip.TabStripPlacement" /> side of a tab
/// are rounded. An optional add button raises <see cref="AddTabRequest" />.
/// </summary>
public partial class CardTabStrip : BaseTabStrip
{
    #region 公共属性实现

    /// <summary>
    /// Identifies the <see cref="IsShowAddTabButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowAddTabButtonProperty =
        AvaloniaProperty.Register<CardTabStrip, bool>(nameof(IsShowAddTabButton));

    /// <summary>
    /// Identifies the <see cref="AddTabRequest" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> AddTabRequestEvent =
        RoutedEvent.Register<CardTabStrip, RoutedEventArgs>(
            nameof(AddTabRequest),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Controls whether the add button after the last tab is visible. Clicking the button raises
    /// <see cref="AddTabRequest" />.
    /// </summary>
    public bool IsShowAddTabButton
    {
        get => GetValue(IsShowAddTabButtonProperty);
        set => SetValue(IsShowAddTabButtonProperty, value);
    }

    /// <summary>
    /// Occurs when the add button is clicked. The event bubbles up the element tree. The strip adds no tab
    /// itself, so a handler has to create the tab.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? AddTabRequest
    {
        add => AddHandler(AddTabRequestEvent, value);
        remove => RemoveHandler(AddTabRequestEvent, value);
    }

    /// <summary>
    /// Identifies the <see cref="CardBorderThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> CardBorderThicknessProperty =
        AvaloniaProperty.Register<CardTabStrip, Thickness>(nameof(CardBorderThickness));

    /// <summary>
    /// Identifies the <see cref="EffectiveCardBorderRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> EffectiveCardBorderRadiusProperty =
        AvaloniaProperty.Register<CardTabStrip, CornerRadius>(nameof(EffectiveCardBorderRadius));

    /// <summary>
    /// Corner radius applied to card tabs and the add-tab button for the active tab strip placement.
    /// </summary>
    public CornerRadius EffectiveCardBorderRadius
    {
        get => GetValue(EffectiveCardBorderRadiusProperty);
        set => SetValue(EffectiveCardBorderRadiusProperty, value);
    }

    /// <summary>
    /// Border thickness applied to card tabs and the add-tab button in the card tab strip template.
    /// </summary>
    public Thickness CardBorderThickness
    {
        get => GetValue(CardBorderThicknessProperty);
        set => SetValue(CardBorderThicknessProperty, value);
    }

    #endregion

    #region 内部属性实现

    internal static readonly StyledProperty<bool> UseCardRadiusTokenResourcesProperty =
        AvaloniaProperty.Register<CardTabStrip, bool>(nameof(UseCardRadiusTokenResources));
    
    internal static readonly StyledProperty<double> CardSizeProperty =
        AvaloniaProperty.Register<CardTabStrip, double>(nameof(CardSize));

    internal bool UseCardRadiusTokenResources
    {
        get => GetValue(UseCardRadiusTokenResourcesProperty);
        set => SetValue(UseCardRadiusTokenResourcesProperty, value);
    }
    
    private static readonly object[] s_cardRadiusKeys =
    [
        ControlTokenResourceKey.Global(CardTabStripTokens.Identity, SharedTokenKind.BorderRadiusSM),
        ControlTokenResourceKey.Global(CardTabStripTokens.Identity, SharedTokenKind.BorderRadiusMD),
        ControlTokenResourceKey.Global(CardTabStripTokens.Identity, SharedTokenKind.BorderRadiusLG)
    ];

    private OwnerTokenSet? _radiusTokens;
    private CornerRadius CardBorderRadius { get; set; }

    internal double CardSize
    {
        get => GetValue(CardSizeProperty);
        set => SetValue(CardSizeProperty, value);
    }

    #endregion

    private IconButton? _addTabButton;
    private ItemsPresenter? _itemsPresenter;

    /// <summary>
    /// Creates a card-shaped <see cref="TabStripItem" /> and adds <c>CardTabStripSemanticParts.ItemClass</c> to
    /// its classes. The item, its index and the recycle key do not affect the container.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var tabStripItem = new TabStripItem
        {
            Shape = TabSharp.Card
        };
        tabStripItem.Classes.Add(CardTabStripSemanticParts.ItemClass);
        return tabStripItem;
    }

    internal override void ReleaseTabStripItemOwnerBindings(TabStripItem tabStripItem)
    {
        base.ReleaseTabStripItemOwnerBindings(tabStripItem);
        // Card-only indexer bindings created in PrepareContainerForItemOverride.
        BindingOperations.GetBindingExpressionBase(tabStripItem, CornerRadiusProperty)?.Dispose();
        BindingOperations.GetBindingExpressionBase(tabStripItem, BorderThicknessProperty)?.Dispose();
    }

    /// <summary>
    /// Calls the base implementation, then keeps a <see cref="TabStripItem" /> container card-shaped and binds
    /// its <c>CornerRadius</c> and <c>BorderThickness</c> to <see cref="EffectiveCardBorderRadius" /> and
    /// <see cref="CardBorderThickness" />. <see cref="ReleaseTabStripItemOwnerBindings" /> releases these
    /// bindings again.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabStripItem tabStripItem)
        {
            tabStripItem.Shape = TabSharp.Card;
            tabStripItem.Classes.Add(CardTabStripSemanticParts.ItemClass);
            tabStripItem[!CornerRadiusProperty] = this[!EffectiveCardBorderRadiusProperty];
            tabStripItem[!BorderThicknessProperty] = this[!CardBorderThicknessProperty];
        }
    }

    /// <summary>
    /// Locates the <c>PART_AddTabButton</c> and <c>PART_ItemsPresenter</c> template parts and moves the click
    /// handler that raises <see cref="AddTabRequest" /> to the button of the new template.
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
    /// Keeps the tab cards in sync with property changes: the radius token subscription follows
    /// <see cref="UseCardRadiusTokenResources" />, a <see cref="BaseTabStrip.SizeType" /> change refreshes the
    /// radius read from the tokens, and a change of <see cref="BaseTabStrip.TabStripPlacement" /> or
    /// <see cref="CardSize" /> updates the effective corner radius and the size of the add button. Call the
    /// base method as well.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseCardRadiusTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateCardRadiusTokenSubscription();
        }
        else if (change.Property == SizeTypeProperty && _radiusTokens is not null)
        {
            RefreshRadius();
        }
        else if (change.Property == TabStripPlacementProperty || change.Property == CardSizeProperty)
        {
            HandleTabStripPlacementChanged();
        }
    }

    /// <summary>
    /// Calls the base method, then synchronizes the radius token subscription with
    /// <see cref="UseCardRadiusTokenResources" /> and refreshes the card radius.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateCardRadiusTokenSubscription();
    }

    /// <summary>
    /// Disposes the radius token subscription before calling the base method. The subscription is created
    /// again on the next attach while <see cref="UseCardRadiusTokenResources" /> is <see langword="true" />.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _radiusTokens?.Dispose();
        _radiusTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateCardRadiusTokenSubscription()
    {
        if (UseCardRadiusTokenResources && _radiusTokens is null)
        {
            _radiusTokens = new OwnerTokenSet(this, s_cardRadiusKeys.Length, RefreshRadius);
            for (var index = 0; index < s_cardRadiusKeys.Length; ++index)
            {
                _radiusTokens.SetResourceKey(index, s_cardRadiusKeys[index]);
            }
        }
        else if (!UseCardRadiusTokenResources && _radiusTokens is not null)
        {
            _radiusTokens.Dispose();
            _radiusTokens = null;
        }
        RefreshRadius();
    }

    private void RefreshRadius()
    {
        var radius = TabCardRadiusResources.Read(_radiusTokens, 0, SizeType, CardBorderRadius);
        if (CardBorderRadius != radius)
        {
            CardBorderRadius = radius;
            HandleTabStripPlacementChanged();
        }
    }

    private void HandleAddButtonClicked(object? sender, RoutedEventArgs args)
    {
        RaiseEvent(new RoutedEventArgs(AddTabRequestEvent));
    }

    /// <summary>
    /// Calls the base method, then updates the placement-dependent values: the effective corner radius of the
    /// tabs and the size of the add button.
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
            EffectiveCardBorderRadius = new CornerRadius(CardBorderRadius.TopLeft, CardBorderRadius.TopRight,
                bottomLeft: 0, bottomRight: 0);
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
