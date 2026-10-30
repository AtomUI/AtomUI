using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public enum CollapseTriggerType
{
    Header,
    Icon
}

public enum CollapseExpandIconPosition
{
    Start,
    End
}

[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter))]
public partial class Collapse : SelectingItemsControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Collapse>();

    public static readonly StyledProperty<bool> IsGhostStyleProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsGhostStyle));

    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsBorderless));

    public static readonly StyledProperty<bool> IsAccordionProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(IsAccordion));

    public static readonly StyledProperty<CollapseTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<Collapse, CollapseTriggerType>(nameof(TriggerType));

    public static readonly StyledProperty<CollapseExpandIconPosition> ExpandIconPositionProperty =
        AvaloniaProperty.Register<Collapse, CollapseExpandIconPosition>(nameof(ExpandIconPosition));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Collapse>();
    
    public static readonly StyledProperty<Thickness> ItemHeaderPaddingProperty =
        AvaloniaProperty.Register<Collapse, Thickness>(nameof(ItemHeaderPadding));

    public static readonly StyledProperty<Thickness> ItemContentPaddingProperty =
        AvaloniaProperty.Register<Collapse, Thickness>(nameof(ItemContentPadding));

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsGhostStyle
    {
        get => GetValue(IsGhostStyleProperty);
        set => SetValue(IsGhostStyleProperty, value);
    }

    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }

    public bool IsAccordion
    {
        get => GetValue(IsAccordionProperty);
        set => SetValue(IsAccordionProperty, value);
    }

    public CollapseTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    public CollapseExpandIconPosition ExpandIconPosition
    {
        get => GetValue(ExpandIconPositionProperty);
        set => SetValue(ExpandIconPositionProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public Thickness ItemHeaderPadding
    {
        get => GetValue(ItemHeaderPaddingProperty);
        set => SetValue(ItemHeaderPaddingProperty, value);
    }

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
        SelectionModeProperty.OverrideDefaultValue<Collapse>(SelectionMode.Multiple | SelectionMode.Toggle);
        ItemsPanelProperty.OverrideDefaultValue<Collapse>(DefaultPanel);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<Collapse>(false);
        AffectsRender<Collapse>(SelectionModeProperty);
    }

    public Collapse()
    {
        SetupSelectionMode();
        Items.CollectionChanged += HandleItemsCollectionChanged;
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var collapseItem = new CollapseItem();
        collapseItem.Classes.Add(ScopeItemClass);
        return collapseItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CollapseItem>(item, out recycleKey);
    }

    protected sealed override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is CollapseItem collapseItem)
        {
            collapseItem.Classes.Add(ScopeItemClass);
            if (item != null && item is not Visual)
            {
                if (!collapseItem.IsSet(CollapseItem.ContentProperty))
                {
                    collapseItem.SetCurrentValue(CollapseItem.ContentProperty, item);
                }

                if (item is ICollapseItemData itemData)
                {
                    if (!collapseItem.IsSet(CollapseItem.HeaderProperty))
                    {
                        collapseItem.SetCurrentValue(CollapseItem.HeaderProperty, itemData.Header);
                    }
                    if (!collapseItem.IsSet(CollapseItem.IsSelectedProperty))
                    {
                        collapseItem.SetCurrentValue(CollapseItem.IsSelectedProperty, itemData.IsSelected);
                    }
                    if (!collapseItem.IsSet(CollapseItem.IsShowExpandIconProperty))
                    {
                        collapseItem.SetCurrentValue(CollapseItem.IsShowExpandIconProperty, itemData.IsShowExpandIcon);
                    }
                }
            }

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

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        var containerWasSelected = container.GetValue(CollapseItem.IsSelectedProperty);
        var previousSelectedIndex = IsAccordion ? Selection.SelectedIndex : -1;

        base.ContainerForItemPreparedOverride(container, item, index);

        if (IsAccordion &&
            containerWasSelected &&
            previousSelectedIndex >= 0 &&
            previousSelectedIndex < index &&
            Selection.SelectedIndex == index)
        {
            Selection.SelectedIndex = previousSelectedIndex;
        }

        NormalizeAccordionSelection();
    }

    protected virtual void PrepareCollapseItem(CollapseItem collapseItem, object? item, int index)
    {
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        if (container is CollapseItem collapseItem)
        {
            ConfigureItemBorders(collapseItem, newIndex);
            ConfigureItemCorners(collapseItem, newIndex);
        }
    }

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsAccordionProperty)
        {
            SetupSelectionMode();
        }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupEffectiveBorderThickness();
        NormalizeAccordionSelection();
    }

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

    // Mirrors antd's collapse style rules: the first item's header carries the top
    // corners, the last item's header and content carry the bottom corners, so their
    // backgrounds follow the container's rounded border instead of covering it.
    private void ConfigureItemCorners(CollapseItem collapseItem, int index)
    {
        var radius  = CornerRadius;
        var isFirst = index == 0;
        var isLast  = index == ItemCount - 1;

        collapseItem.HeaderCornerRadius = new CornerRadius(
            isFirst ? radius.TopLeft : 0,
            isFirst ? radius.TopRight : 0,
            isLast ? radius.BottomLeft : 0,
            isLast ? radius.BottomRight : 0);
        collapseItem.ContentCornerRadius = isLast
            ? new CornerRadius(0, 0, radius.BottomLeft, radius.BottomRight)
            : default;
    }

    private CollapseItem? GetCollapseItemAt(int index)
    {
        return ContainerFromIndex(index) as CollapseItem;
    }

    private void NormalizeAccordionSelection()
    {
        if (!IsAccordion || Selection.SelectedIndexes.Count <= 1)
        {
            return;
        }

        Selection.SelectedIndex = Selection.SelectedIndexes.Min();
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

    private void SetupSelectionMode()
    {
        SelectionMode = IsAccordion
            ? SelectionMode.Single | SelectionMode.Toggle
            : SelectionMode.Multiple | SelectionMode.Toggle;
        NormalizeAccordionSelection();
    }
}
