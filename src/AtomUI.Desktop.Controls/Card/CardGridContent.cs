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
/// <summary>
/// Grid content of a <see cref="Card" />: every item is shown in a <see cref="CardGridItem" /> on the grid
/// defined by <see cref="ColumnDefinitions" /> and <see cref="RowDefinitions" />, with separator lines
/// between the cells. Set it as the <see cref="ContentControl.Content" /> of a card to use the grid layout.
/// </summary>
[TokenOwnerTransparent]
public class CardGridContent : ItemsControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ColumnDefinitions" /> property.
    /// </summary>
    public static readonly StyledProperty<ColumnDefinitions> ColumnDefinitionsProperty =
        AvaloniaProperty.Register<CardGridContent, ColumnDefinitions>(nameof(ColumnDefinitions));
    
    /// <summary>
    /// Identifies the <see cref="RowDefinitions" /> property.
    /// </summary>
    public static readonly StyledProperty<RowDefinitions> RowDefinitionsProperty =
        AvaloniaProperty.Register<CardGridContent, RowDefinitions>(nameof(RowDefinitions));
    
    /// <summary>
    /// Identifies the <see cref="IsHoverable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHoverableProperty = 
        AvaloniaProperty.Register<CardGridContent, bool>(nameof (IsHoverable), true);

    /// <summary>
    /// Column definitions of the grid that lays out the items, for example <c>*, *, *, *</c>. The value is copied
    /// into the items panel of the template when the template is applied and whenever it changes. The initial
    /// value is an empty collection.
    /// </summary>
    public ColumnDefinitions ColumnDefinitions
    {
        get => GetValue(ColumnDefinitionsProperty);
        set => SetValue(ColumnDefinitionsProperty, value);
    }

    /// <summary>
    /// Row definitions of the grid that lays out the items, for example <c>Auto, Auto</c>. The value is copied
    /// into the items panel of the template when the template is applied and whenever it changes. The initial
    /// value is an empty collection.
    /// </summary>
    public RowDefinitions RowDefinitions
    {
        get => GetValue(RowDefinitionsProperty);
        set => SetValue(RowDefinitionsProperty, value);
    }
    
    /// <summary>
    /// Whether the grid content takes part in hovering; on by default. The value is not relayed to the items and
    /// no theme reads it: the hover effect of a cell follows <see cref="CardGridItem.IsHoverable" />, which is on
    /// by default as well.
    /// </summary>
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
    
    /// <summary>
    /// Size step of the grid items: the item theme sets the cell padding for each step. The value is forwarded to
    /// every <see cref="CardGridItem" />; when the grid content is the content of a <see cref="Card" />, the card
    /// relays its own <see cref="Card.SizeType" /> here, so set it on the card. Unset, the value is <c>Middle</c>.
    /// </summary>
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

    /// <summary>
    /// Creates a new <see cref="CardGridItem" /> as the container of an item.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CardGridItem();
    }
    
    /// <summary>
    /// Returns whether an item needs a container: an item that already is a <see cref="CardGridItem" /> is used as
    /// it is, any other item gets one.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CardGridItem>(item, out recycleKey);
    }

    /// <summary>
    /// Prepares a <see cref="CardGridItem" /> for an item: drops the state bound for an earlier use of the
    /// container, shows a non-visual item as its content unless the content was already set, forwards
    /// <c>ItemTemplate</c> when set, and binds size type, enabled state and motion switch from this control to the
    /// item. A container of another type throws <see cref="ArgumentOutOfRangeException" />.
    /// </summary>
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

    /// <summary>
    /// Releases the per-item state of a <see cref="CardGridItem" /> before the base member clears it: disposes the
    /// subscriptions added for the item and removes the hover clip.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is CardGridItem gridItem)
        {
            DisposeCardGridItemState(gridItem);
            gridItem.SetGridHoverClip(null);
        }

        base.ClearContainerForItemOverride(container);
    }
    
    /// <summary>
    /// Called from <see cref="PrepareContainerForItemOverride" /> once the content, template and relay bindings of
    /// a <see cref="CardGridItem" /> are set; override it to bind further per-item state. Add every subscription to
    /// <c>compositeDisposable</c>, which is disposed when the container is prepared again or cleared. The base
    /// member does nothing and need not be called.
    /// </summary>
    protected virtual void PrepareCardGridItem(CardGridItem cardGridItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    /// <summary>
    /// Copies <see cref="ColumnDefinitions" /> or <see cref="RowDefinitions" /> into the items panel after one of
    /// them changed. Calls the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColumnDefinitionsProperty ||
            change.Property == RowDefinitionsProperty)
        {
            SyncGridPanelProperties();
        }
    }

    /// <summary>
    /// Binds the applied template: clears the separator geometry, stores the presenter named
    /// <c>PART_ItemsPresenter</c>, applies its template and copies the column and row definitions into its panel.
    /// Overrides must call the base member first.
    /// </summary>
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
