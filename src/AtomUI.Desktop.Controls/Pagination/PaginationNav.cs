using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class PaginationNav : SelectingItemsControl, ICustomizableSizeTypeAware
{
    #region 公共属性定义
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<PaginationNav>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<PaginationNav>();

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<PageNavRequestArgs>? PageNavigateRequest;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<PaginationNav, double>(nameof(ItemSpacing));
    
    internal double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    // The pager's item inputs, template-bound from the pager and forwarded to each item.
    internal static readonly StyledProperty<double> ItemSizeProperty =
        AbstractPagination.ItemSizeProperty.AddOwner<PaginationNav>();

    internal static readonly StyledProperty<IBrush?> ItemActiveColorDisabledProperty =
        AbstractPagination.ItemActiveColorDisabledProperty.AddOwner<PaginationNav>();

    internal static readonly StyledProperty<Thickness> ItemPaddingInlineProperty =
        AbstractPagination.ItemPaddingInlineProperty.AddOwner<PaginationNav>();

    internal static readonly StyledProperty<IBrush?> ItemColorProperty =
        AbstractPagination.ItemColorProperty.AddOwner<PaginationNav>();

    internal static readonly StyledProperty<IBrush?> ItemHoverColorProperty =
        AbstractPagination.ItemHoverColorProperty.AddOwner<PaginationNav>();

    internal static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        AbstractPagination.ItemHoverBgProperty.AddOwner<PaginationNav>();

    internal static readonly StyledProperty<CornerRadius> ItemBorderRadiusProperty =
        AbstractPagination.ItemBorderRadiusProperty.AddOwner<PaginationNav>();

    internal double ItemSize
    {
        get => GetValue(ItemSizeProperty);
        set => SetValue(ItemSizeProperty, value);
    }

    internal IBrush? ItemActiveColorDisabled
    {
        get => GetValue(ItemActiveColorDisabledProperty);
        set => SetValue(ItemActiveColorDisabledProperty, value);
    }

    internal Thickness ItemPaddingInline
    {
        get => GetValue(ItemPaddingInlineProperty);
        set => SetValue(ItemPaddingInlineProperty, value);
    }

    internal IBrush? ItemColor
    {
        get => GetValue(ItemColorProperty);
        set => SetValue(ItemColorProperty, value);
    }

    internal IBrush? ItemHoverColor
    {
        get => GetValue(ItemHoverColorProperty);
        set => SetValue(ItemHoverColorProperty, value);
    }

    internal IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }

    internal CornerRadius ItemBorderRadius
    {
        get => GetValue(ItemBorderRadiusProperty);
        set => SetValue(ItemBorderRadiusProperty, value);
    }

    #endregion
    
    static PaginationNav()
    {
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<PaginationNav>(false);
        AffectsMeasure<PaginationNav>(SizeTypeProperty);
    }

    public PaginationNav()
    {
        SelectionMode = SelectionMode.Single;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // The fixed navigation slots are logical children, so creating them in the constructor styled and initialized
        // all of them even when the nav is never measured (hidden, empty or single-page pagination). They are added
        // when the template applies instead. The items panel is only built when ItemsPresenter is measured, after
        // this call, so every container is still prepared with the final ItemCount, exactly as before.
        if (ItemCount == 0)
        {
            for (var i = 0; i < Pagination.MaxNavItemCount; i++)
            {
                Items.Add(new PaginationNavItem());
            }
        }

        base.OnApplyTemplate(e);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new PaginationNavItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<PaginationNavItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is PaginationNavItem navItem)
        {
            navItem[!PaginationNavItem.SizeTypeProperty]        = this[!SizeTypeProperty];
            navItem[!PaginationNavItem.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            navItem[!PaginationNavItem.ItemSizeProperty]        = this[!ItemSizeProperty];
            navItem[!PaginationNavItem.ItemActiveColorDisabledProperty] = this[!ItemActiveColorDisabledProperty];
            navItem[!PaginationNavItem.ItemPaddingInlineProperty]       = this[!ItemPaddingInlineProperty];
            navItem[!PaginationNavItem.ItemColorProperty]               = this[!ItemColorProperty];
            navItem[!PaginationNavItem.ItemHoverColorProperty]          = this[!ItemHoverColorProperty];
            navItem[!PaginationNavItem.ItemHoverBgProperty]             = this[!ItemHoverBgProperty];
            navItem[!PaginationNavItem.ItemBorderRadiusProperty]        = this[!ItemBorderRadiusProperty];
            navItem.Click -= HandleNavItemClicked;
            navItem.Click += HandleNavItemClicked;
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is PaginationNavItem navItem)
        {
            navItem.Click -= HandleNavItemClicked;
        }

        base.ClearContainerForItemOverride(container);
    }

    private void HandleNavItemClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is PaginationNavItem navItem)
        {
            PageNavigateRequest?.Invoke(this, new PageNavRequestArgs(navItem, IndexFromContainer(navItem), navItem.PageNumber));
        }
    }
}
