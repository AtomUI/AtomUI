using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Item metrics and colours from the terminal's Own Tokens. The terminal theme (ListView, or TransferListView, whose
// setters replace the ListView ones) assigns them on the list root; the list forwards the item values to each
// container it prepares. They do not inherit through the list tree.
public partial class ListView
{
    #region 内部属性定义

    internal static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<ListView, Thickness>(nameof(ContentPadding));

    internal Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemColor));

    internal IBrush? ItemColor
    {
        get => GetValue(ItemColorProperty);
        set => SetValue(ItemColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemHoverColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemHoverColor));

    internal IBrush? ItemHoverColor
    {
        get => GetValue(ItemHoverColorProperty);
        set => SetValue(ItemHoverColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemSelectedColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemSelectedColor));

    internal IBrush? ItemSelectedColor
    {
        get => GetValue(ItemSelectedColorProperty);
        set => SetValue(ItemSelectedColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemDisabledColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemDisabledColor));

    internal IBrush? ItemDisabledColor
    {
        get => GetValue(ItemDisabledColorProperty);
        set => SetValue(ItemDisabledColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemBgColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemBgColor));

    internal IBrush? ItemBgColor
    {
        get => GetValue(ItemBgColorProperty);
        set => SetValue(ItemBgColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemHoverBgColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemHoverBgColor));

    internal IBrush? ItemHoverBgColor
    {
        get => GetValue(ItemHoverBgColorProperty);
        set => SetValue(ItemHoverBgColorProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> ItemSelectedBgColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemSelectedBgColor));

    internal IBrush? ItemSelectedBgColor
    {
        get => GetValue(ItemSelectedBgColorProperty);
        set => SetValue(ItemSelectedBgColorProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ItemPaddingProperty =
        AvaloniaProperty.Register<ListView, Thickness>(nameof(ItemPadding));

    internal Thickness ItemPadding
    {
        get => GetValue(ItemPaddingProperty);
        set => SetValue(ItemPaddingProperty, value);
    }

    internal static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<ListView, Thickness>(nameof(ItemMargin));

    internal Thickness ItemMargin
    {
        get => GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    internal static readonly StyledProperty<IBrush?> GroupHeaderColorProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(GroupHeaderColor));

    internal IBrush? GroupHeaderColor
    {
        get => GetValue(GroupHeaderColorProperty);
        set => SetValue(GroupHeaderColorProperty, value);
    }

    internal static readonly StyledProperty<Thickness> GroupHeaderPaddingProperty =
        AvaloniaProperty.Register<ListView, Thickness>(nameof(GroupHeaderPadding));

    internal Thickness GroupHeaderPadding
    {
        get => GetValue(GroupHeaderPaddingProperty);
        set => SetValue(GroupHeaderPaddingProperty, value);
    }

    internal static readonly StyledProperty<Thickness> SelectedIndicatorMarginProperty =
        AvaloniaProperty.Register<ListView, Thickness>(nameof(SelectedIndicatorMargin));

    internal Thickness SelectedIndicatorMargin
    {
        get => GetValue(SelectedIndicatorMarginProperty);
        set => SetValue(SelectedIndicatorMarginProperty, value);
    }

    #endregion

    private static void ForwardItemTheme(ListView list, ListViewItem item)
    {
        item[!ListViewItem.ItemColorProperty]               = list[!ItemColorProperty];
        item[!ListViewItem.ItemHoverColorProperty]          = list[!ItemHoverColorProperty];
        item[!ListViewItem.ItemSelectedColorProperty]       = list[!ItemSelectedColorProperty];
        item[!ListViewItem.ItemDisabledColorProperty]       = list[!ItemDisabledColorProperty];
        item[!ListViewItem.ItemBgColorProperty]             = list[!ItemBgColorProperty];
        item[!ListViewItem.ItemPaddingProperty]             = list[!ItemPaddingProperty];
        item[!ListViewItem.ItemMarginProperty]              = list[!ItemMarginProperty];
        item[!ListViewItem.GroupHeaderColorProperty]        = list[!GroupHeaderColorProperty];
        item[!ListViewItem.GroupHeaderPaddingProperty]      = list[!GroupHeaderPaddingProperty];
        item[!ListViewItem.SelectedIndicatorMarginProperty] = list[!SelectedIndicatorMarginProperty];
    }
}
