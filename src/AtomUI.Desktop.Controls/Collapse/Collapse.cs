using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Selects which part of a panel header toggles the panel: the whole header or only the expand icon.
/// </summary>
public enum CollapseTriggerType
{
    /// <summary>
    /// A click anywhere on the header toggles the panel, and keyboard focus lands on the header.
    /// </summary>
    Header,
    /// <summary>
    /// Only the expand icon toggles the panel: a click on the rest of the header does nothing, and keyboard focus
    /// lands on the expand icon.
    /// </summary>
    Icon
}

/// <summary>
/// Selects on which side of the title the expand icon of a panel is placed.
/// </summary>
public enum CollapseExpandIconPosition
{
    /// <summary>
    /// Before the title, at the start of the header.
    /// </summary>
    Start,
    /// <summary>
    /// After the title and the add-on content, at the end of the header.
    /// </summary>
    End
}

/// <summary>
/// A list of collapsible panels in the Ant Design style. Each item is shown by a <see cref="CollapseItem" />
/// that has a header, an expand icon and a content area, and is expanded while it is selected.
/// </summary>
/// <remarks>
/// An item that is itself a <see cref="CollapseItem" /> is used as its own panel; any other item gets a new one.
/// The bottom edge of the inherited <c>BorderThickness</c> gives the width of the separator lines between
/// adjacent panels and between the header and the content of a panel. The inherited <c>CornerRadius</c> is split
/// over the panels: the first panel takes the top corners and the last panel the bottom corners.
/// </remarks>
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter))]
public partial class Collapse : AbstractCollapse, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Collapse>();

    /// <summary>
    /// Identifies the <see cref="IsGhostStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGhostStyleProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsGhostStyle));

    /// <summary>
    /// Identifies the <see cref="IsBorderless" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsBorderless));

    /// <summary>
    /// Identifies the <see cref="IsAccordion" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsAccordionProperty =
        AbstractCollapse.IsAccordionProperty.AddOwner<Collapse>();

    /// <summary>
    /// Identifies the <see cref="TriggerType" /> property.
    /// </summary>
    public static readonly StyledProperty<CollapseTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<Collapse, CollapseTriggerType>(nameof(TriggerType));

    /// <summary>
    /// Identifies the <see cref="ExpandIconPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<CollapseExpandIconPosition> ExpandIconPositionProperty =
        AvaloniaProperty.Register<Collapse, CollapseExpandIconPosition>(nameof(ExpandIconPosition));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Collapse>();
    
    /// <summary>
    /// Identifies the <see cref="ItemHeaderPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> ItemHeaderPaddingProperty =
        AvaloniaProperty.Register<Collapse, Thickness>(nameof(ItemHeaderPadding));

    /// <summary>
    /// Identifies the <see cref="ItemContentPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> ItemContentPaddingProperty =
        AvaloniaProperty.Register<Collapse, Thickness>(nameof(ItemContentPadding));

    /// <summary>
    /// Size preset of the panels. The value is set on the control and forwarded to every
    /// <see cref="CollapseItem" />, where it selects the tokens for the header font size and line height, the
    /// expand icon margin and the default paddings of the header and the content.
    /// <see cref="CustomizableSizeType.Custom" /> is treated like <see cref="CustomizableSizeType.Middle" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Whether the panels are drawn in the ghost style: the header and the content area get no fill, and the
    /// separator lines between adjacent panels and between the header and the content of a panel are omitted.
    /// </summary>
    /// <remarks>
    /// The ghost style takes precedence over <see cref="IsBorderless" />.
    /// </remarks>
    public bool IsGhostStyle
    {
        get => GetValue(IsGhostStyleProperty);
        set => SetValue(IsGhostStyleProperty, value);
    }

    /// <summary>
    /// Whether the panels are drawn without a border: the content area is painted with the header background
    /// instead of the content background, and no separator line is drawn between the header and the content of a
    /// panel. The separator line between adjacent panels stays.
    /// </summary>
    /// <remarks>
    /// Has no visible effect while <see cref="IsGhostStyle" /> is <see langword="true" />.
    /// </remarks>
    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }

    /// <summary>
    /// Whether the panels work as an accordion, so that at most one panel is expanded at a time. The default is
    /// <see langword="false" />, which allows several panels to be expanded at once.
    /// </summary>
    /// <remarks>
    /// Changing the value switches the control between single and multiple selection. In accordion mode a
    /// selection of several panels is reduced to the first one when containers are prepared and when the control
    /// is attached to the visual tree. This is the same value as <see cref="AbstractCollapse.IsAccordion" />.
    /// </remarks>
    public new bool IsAccordion
    {
        get => GetValue(IsAccordionProperty);
        set => SetValue(IsAccordionProperty, value);
    }

    /// <summary>
    /// Selects which part of a panel header toggles the panel. The value is set on the control and forwarded to
    /// every <see cref="CollapseItem" />. The default is <see cref="CollapseTriggerType.Header" />.
    /// </summary>
    public CollapseTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    /// <summary>
    /// Selects on which side of the title the expand icon of a panel is placed. The value is set on the control and
    /// forwarded to every <see cref="CollapseItem" />. The default is
    /// <see cref="CollapseExpandIconPosition.Start" />.
    /// </summary>
    public CollapseExpandIconPosition ExpandIconPosition
    {
        get => GetValue(ExpandIconPositionProperty);
        set => SetValue(ExpandIconPositionProperty, value);
    }

    /// <summary>
    /// Whether the expand and collapse motion of the panel content areas is enabled. The value is set on the
    /// control and forwarded to every <see cref="CollapseItem" />, which also passes it to its expand icon.
    /// </summary>
    /// <remarks>
    /// The default theme binds this property to the <c>EnableMotion</c> design token, so clearing a local value
    /// returns the panels to that token.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Padding between the border and the content of every panel header, which holds the expand icon, the title
    /// and the add-on content. The value is set on the control and forwarded to every <see cref="CollapseItem" />.
    /// </summary>
    /// <remarks>
    /// While this property is not set, a panel uses the header padding of its size preset. A padding set directly
    /// on a <see cref="CollapseItem" /> takes precedence over this value.
    /// </remarks>
    public Thickness ItemHeaderPadding
    {
        get => GetValue(ItemHeaderPaddingProperty);
        set => SetValue(ItemHeaderPaddingProperty, value);
    }

    /// <summary>
    /// Padding between the border and the content of every panel body. The value is set on the control and
    /// forwarded to every <see cref="CollapseItem" />.
    /// </summary>
    /// <remarks>
    /// While this property is not set, a panel uses the content padding of its size preset. A padding set directly
    /// on a <see cref="CollapseItem" /> takes precedence over this value.
    /// </remarks>
    public Thickness ItemContentPadding
    {
        get => GetValue(ItemContentPaddingProperty);
        set => SetValue(ItemContentPaddingProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Collapse, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Collapse, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    #endregion

    private const string ScopePanelClass = "semantic-scope-panel";
    private const string ScopeItemClass = "semantic-scope-item";

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() =>
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            panel.Classes.Add(ScopePanelClass);
            return panel;
        });

    static Collapse()
    {
        ItemsPanelProperty.OverrideDefaultValue<Collapse>(DefaultPanel);
        AffectsRender<Collapse>(SelectionModeProperty);
    }

    public Collapse()
    {
        Items.CollectionChanged += HandleItemsCollectionChanged;
    }
    
    /// <summary>
    /// Creates a <see cref="CollapseItem" /> for an item that is not one itself. The panel is tagged as a scope
    /// item so that the semantic part styles reach its header, expand icon, title and content area.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var collapseItem = new CollapseItem();
        collapseItem.Classes.Add(ScopeItemClass);
        return collapseItem;
    }

    /// <summary>
    /// Decides whether an item needs a panel: an item that is itself a <see cref="CollapseItem" /> is used as its
    /// own panel, every other item needs one.
    /// </summary>
    /// <param name="item">
    /// The item to check.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <param name="recycleKey">
    /// <see langword="null" /> when the item is its own container; otherwise the default recycle key of the items
    /// control.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item needs a container; otherwise <see langword="false" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CollapseItem>(item, out recycleKey);
    }

    /// <summary>
    /// Prepares a panel for its item. After the base member has filled in the data of the item, the panel is
    /// tagged as a scope item, bound to the appearance values of this control, passed to
    /// <see cref="PrepareCollapseItem" />, and given its paddings, separator lines and corners.
    /// </summary>
    /// <param name="container">
    /// The panel that displays the item. It must be a <see cref="CollapseItem" />.
    /// </param>
    /// <param name="item">
    /// The item to display.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not a <see cref="CollapseItem" />.
    /// </exception>
    /// <remarks>
    /// An item template set on this control is bound to the panel as its content template.
    /// </remarks>
    protected sealed override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is CollapseItem collapseItem)
        {
            base.PrepareContainerForItemOverride(container, item, index);
            collapseItem.Classes.Add(ScopeItemClass);
            if (ItemTemplate != null)
            {
                collapseItem[!CollapseItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }
            
            collapseItem[!CollapseItem.SizeTypeProperty]           = this[!SizeTypeProperty];
            collapseItem[!CollapseItem.BorderThicknessProperty]    = this[!EffectiveBorderThicknessProperty];
            collapseItem[!CollapseItem.IsGhostStyleProperty]       = this[!IsGhostStyleProperty];
            collapseItem[!CollapseItem.IsBorderlessProperty]       = this[!IsBorderlessProperty];
            collapseItem[!CollapseItem.TriggerTypeProperty]        = this[!TriggerTypeProperty];
            collapseItem[!CollapseItem.ExpandIconPositionProperty] = this[!ExpandIconPositionProperty];
            collapseItem[!CollapseItem.IsMotionEnabledProperty]    = this[!IsMotionEnabledProperty];
            PrepareCollapseItem(collapseItem, item, index);
            ConfigureItemPaddings(collapseItem);
            ConfigureItemBorders(collapseItem, index);
            ConfigureItemCorners(collapseItem, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CollapseItem.");
        }
    }

    /// <inheritdoc />
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index) =>
        base.ContainerForItemPreparedOverride(container, item, index);

    /// <summary>
    /// Lets a derived class adjust a panel while it is prepared, after the appearance values of this control have
    /// been bound to the panel and before its paddings, separator lines and corners are configured.
    /// </summary>
    /// <param name="collapseItem">
    /// The panel being prepared.
    /// </param>
    /// <param name="item">
    /// The item to display.
    /// </param>
    /// <param name="index">
    /// The index of the item.
    /// </param>
    /// <remarks>
    /// The default implementation does nothing, so an override need not call the base member.
    /// </remarks>
    protected virtual void PrepareCollapseItem(CollapseItem collapseItem, object? item, int index)
    {
    }

    /// <summary>
    /// Updates the separator line below a panel and its corners for the index the panel moved to, so that only
    /// the last panel goes without a separator line and the first and the last panel keep their corners. A
    /// container that is not a <see cref="CollapseItem" /> is ignored.
    /// </summary>
    /// <param name="container">
    /// The container that moved.
    /// </param>
    /// <param name="oldIndex">
    /// The index the container had before.
    /// </param>
    /// <param name="newIndex">
    /// The index the container has now.
    /// </param>
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        if (container is CollapseItem collapseItem)
        {
            ConfigureItemBorders(collapseItem, newIndex);
            ConfigureItemCorners(collapseItem, newIndex);
        }
    }

    /// <summary>
    /// Calls the base member, then hands the panel that received focus over to
    /// <see cref="UpdateSelectionFromEvent" /> when focus arrived by directional navigation, and marks the event
    /// as handled when that call returns <see langword="true" />.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.NavigationMethod == NavigationMethod.Directional)
        {
            var containerFromEventSource = GetContainerFromEventSource(e.Source);
            if (containerFromEventSource is CollapseItem collapseItem)
            {
                e.Handled = UpdateSelectionFromEvent(collapseItem, e);
            }
        }
    }

    /// <summary>
    /// Calls the base member, then routes a press of the mouse left button on the header of a panel to
    /// <see cref="UpdateSelectionFromEvent" />, which toggles the panel, and marks the event as handled when that
    /// call returns <see langword="true" />. A press on the header is ignored while
    /// <see cref="TriggerType" /> is <see cref="CollapseTriggerType.Icon" />.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Pointer.Type == PointerType.Mouse)
        {
            var containerFromEventSource = GetContainerFromEventSource(e.Source);
            if (containerFromEventSource is CollapseItem collapseItem)
            {
                if (collapseItem.IsPointInHeaderBounds(e.GetPosition(collapseItem)))
                {
                    e.Handled = UpdateSelectionFromEvent(collapseItem, e);
                }
            }
        }
    }

    /// <summary>
    /// Routes the release of the left button of a touch or pen pointer to
    /// <see cref="UpdateSelectionFromEvent" />, which toggles the panel, when the release happens on the header of
    /// a panel, and marks the event as handled when that call returns <see langword="true" />. Mouse releases are
    /// ignored and the base member is not called.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left && e.Pointer.Type != PointerType.Mouse)
        {
            var container = GetContainerFromEventSource(e.Source);
            if (container != null
                && container.ContainsSelfOrDescendantAt(e.GetPosition(container)))
            {
                var containerFromEventSource = GetContainerFromEventSource(e.Source);
                if (containerFromEventSource is CollapseItem collapseItem)
                {
                    if (collapseItem.IsPointInHeaderBounds(e.GetPosition(collapseItem)))
                    {
                        e.Handled = UpdateSelectionFromEvent(collapseItem, e);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calls the base member, then applies the changed value to every panel: <c>BorderThickness</c>,
    /// <see cref="IsBorderless" /> and <see cref="IsGhostStyle" /> to the separator lines and the effective
    /// border thickness, <c>CornerRadius</c> to the corners, and <see cref="ItemHeaderPadding" /> or
    /// <see cref="ItemContentPadding" /> to the paddings.
    /// </summary>
    /// <param name="change">
    /// The property change.
    /// </param>
    /// <remarks>
    /// A derived class must call the base member, otherwise the selection mode no longer follows
    /// <see cref="IsAccordion" />.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BorderThicknessProperty ||
            change.Property == IsBorderlessProperty ||
            change.Property == IsGhostStyleProperty)
        {
            SetupEffectiveBorderThickness();
            ConfigureItemsBorders();
        }

        if (change.Property == CornerRadiusProperty)
        {
            ConfigureItemsCorners();
        }

        if (change.Property == ItemHeaderPaddingProperty ||
            change.Property == ItemContentPaddingProperty)
        {
            ConfigureItemsPaddings();
        }
    }

    /// <summary>
    /// Calls the base member, then recalculates the effective border thickness from <c>BorderThickness</c>,
    /// <see cref="IsBorderless" /> and <see cref="IsGhostStyle" />.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    /// <remarks>
    /// A derived class must call the base member, otherwise accordion mode is not enforced when the control is
    /// attached.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupEffectiveBorderThickness();
    }

    /// <summary>
    /// Updates the selection for an event that targets a panel and reports whether the event is handled. Pointer,
    /// key and focus events, and events that are already handled, are left to the base member alone.
    /// </summary>
    /// <param name="container">
    /// The panel that the event targets.
    /// </param>
    /// <param name="eventArgs">
    /// The arguments of the event. It is marked as handled when the item of the panel was selected.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the base member handled the event or when the item of the panel was selected;
    /// otherwise <see langword="false" />.
    /// </returns>
    /// <remarks>
    /// An event of any other kind, such as the click of the expand icon, selects the item of the panel. A panel
    /// that has no item in this control is ignored.
    /// </remarks>
    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        if (base.UpdateSelectionFromEvent(container, eventArgs))
        {
            return true;
        }

        if (eventArgs.Handled ||
            eventArgs is PointerEventArgs ||
            eventArgs is KeyEventArgs ||
            eventArgs is FocusChangedEventArgs)
        {
            return false;
        }

        var index = IndexFromContainer(container);
        if (index < 0)
        {
            return false;
        }

        UpdateSelection(index, select: true);
        eventArgs.Handled = true;
        return true;
    }

    private void ConfigureItemsBorders()
    {
        for (var i = 0; i < ItemCount; ++i)
        {
            if (GetCollapseItemAt(i) is { } collapseItem)
            {
                ConfigureItemBorders(collapseItem, i);
            }
        }
    }

    private void ConfigureItemsCorners()
    {
        for (var i = 0; i < ItemCount; ++i)
        {
            if (GetCollapseItemAt(i) is { } collapseItem)
            {
                ConfigureItemCorners(collapseItem, i);
            }
        }
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureItemsBorders();
        ConfigureItemsCorners();
    }

    private void ConfigureItemBorders(CollapseItem collapseItem, int index)
    {
        var line                = BorderThickness.Bottom;
        var isLast              = index == ItemCount - 1;
        var hasItemSeparator    = !IsGhostStyle && !isLast;
        var hasContentSeparator = !IsGhostStyle && !IsBorderless;

        collapseItem.ItemBorderThickness = hasItemSeparator
            ? new Thickness(0, 0, 0, line)
            : default;
        collapseItem.ContentBorderThickness = hasContentSeparator
            ? new Thickness(0, line, 0, 0)
            : default;
    }

    // The first header carries the top corners. The last item supplies the bottom
    // corners to its content and to the header when the content is hidden.
    private void ConfigureItemCorners(CollapseItem collapseItem, int index)
    {
        var radius  = CornerRadius;
        var isFirst = index == 0;
        var isLast  = index == ItemCount - 1;

        collapseItem.HeaderCornerRadius = new CornerRadius(
            isFirst ? radius.TopLeft : 0,
            isFirst ? radius.TopRight : 0,
            isLast ? radius.BottomRight : 0,
            isLast ? radius.BottomLeft : 0);
        collapseItem.ContentCornerRadius = isLast
            ? new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft)
            : default;
    }

    private CollapseItem? GetCollapseItemAt(int index)
    {
        return ContainerFromIndex(index) as CollapseItem;
    }

    private void ConfigureItemsPaddings()
    {
        if (Items.Count > 0)
        {
            for (var i = 0; i < ItemCount; i++)
            {
                if (GetCollapseItemAt(i) is { } collapseItem)
                {
                    ConfigureItemPaddings(collapseItem);
                }
            }
        }
    }

    private void ConfigureItemPaddings(CollapseItem collapseItem)
    {
        collapseItem.OwnerHeaderPadding = IsSet(ItemHeaderPaddingProperty)
            ? ItemHeaderPadding
            : null;
        collapseItem.OwnerContentPadding = IsSet(ItemContentPaddingProperty)
            ? ItemContentPadding
            : null;
    }

    private void SetupEffectiveBorderThickness()
    {
        if (IsBorderless || IsGhostStyle)
        {
            EffectiveBorderThickness = default;
        }
        else
        {
            EffectiveBorderThickness = BorderThickness;
        }
    }
}
